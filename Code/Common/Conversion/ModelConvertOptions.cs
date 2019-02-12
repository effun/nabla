using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla.Conversion
{
    public class ModelConvertOptions
    {
        private bool? _ignoreKeys, _ignoreReadOnly;

        private HashSet<TargetOptions> _targets;
        private HashSet<SourceOptions> _sources;

        private static readonly MethodInfo SourceMethod, TargetMethod;

        static ModelConvertOptions()
        {
            SourceMethod = typeof(ModelConvertOptions).GetMethod("Source", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            TargetMethod = typeof(ModelConvertOptions).GetMethod("Target", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }

        public ModelConvertOptions()
        {
            //_properties = new Dictionary<Type, Dictionary<string, PropertyConvertOptions>>();
            _targets = new HashSet<TargetOptions>();
            _sources = new HashSet<SourceOptions>();
        }

        internal Type MatchSourceType(Type type)
        {
            return MatchType(type, _sources.Select(o => o.ModelType));
        }

        internal Type MatchTargetType(Type type)
        {
            return MatchType(type, _targets.Select(o => o.ModelType));
        }

        private Type MatchType(Type type, IEnumerable<Type> types)
        {
            Type match = null;

            foreach (var modelType in types)
            {
                if (modelType == type)
                    return type;

                if (type.IsSubclassOf(modelType))
                    match = modelType;
            }

            return match ?? type;

        }

        private TargetOptions GetTarget(Type type)
        {
            return _targets.FirstOrDefault(o => o.ModelType == type);
        }

        private SourceOptions GetSource(Type type)
        {
            return _sources.FirstOrDefault(o => o.ModelType == type);
        }

        public TargetOptions<T> Target<T>()
        {
            var options = GetTarget(typeof(T));

            if (options == null)
            {
                options = new TargetOptions<T>(this);
                _targets.Add(options);
            }

            return (TargetOptions<T>)options;
        }

        public TargetOptions Target(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            return (TargetOptions)TargetMethod.MakeGenericMethod(modelType).Invoke(this, null);
        }

        public SourceOptions Source(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            return (SourceOptions)SourceMethod.MakeGenericMethod(modelType).Invoke(this, null);
        }

        public SourceOptions<T> Source<T>()
        {
            var options = GetSource(typeof(T));

            if (options == null)
            {
                options = new SourceOptions<T>(this);
                _sources.Add(options);
            }

            return (SourceOptions<T>)options;
        }


        public ModelConvertOptions IgnoreKeys()
        {
            _ignoreKeys = true;
            return this;
        }

        public ModelConvertOptions IgnoreReadOnly()
        {
            _ignoreReadOnly = true;
            return this;
        }

        internal bool ShouldIgnoreReadOnly(ModelPropertyInfo property)
        {
            var ignore = _ignoreReadOnly ?? GetTarget(property.Model.ModelType)?.ReadOnlyIgnored;

            return ignore.GetValueOrDefault();
        }

        private TargetPropertyOptions GetTargetProperty(ModelPropertyInfo property)
        {
            return GetTarget(property.Model.ModelType)?[property.Name];
        }

        private SourcePropertyOptions GetSourceProperty(ModelPropertyInfo property)
        {
            return GetSource(property.Model.ModelType)?[property.Name];
        }

        internal bool ShouldSetValue(ModelPropertyInfo property)
        {
            if (property.IsKey)
            {
                if (_ignoreKeys.HasValue && !_ignoreKeys.Value)
                    return false;
            }

            var options = GetTargetProperty(property);

            if (options != null)
                return !options.Ignore;

            return true;
        }

        internal object GetDefaultValue(PropertyConvertContext context)
        {
            return GetTargetProperty(context.TargetProperty)?.DefaultValue;
        }

        internal string MapProperty(ModelPropertyInfo property)
        {
            return GetTargetProperty(property)?.MapTo;
        }

        internal bool Convert(PropertyConvertContext context)
        {
            var source = GetSourceProperty(context.SourceProperty);
            var ok = false;

            if (source != null && source.Converter != null)
            {
                context.Value = source.Converter(context.Value, context);
                ok = true;
            }
            else
            {
                var options = GetTargetProperty(context.TargetProperty);

                if (options != null && options.Converter != null)
                {
                    context.Value = options.Converter(context.Value, context);
                    ok = true;
                }
            }

            return ok;
        }

        internal Func<object, PropertyConvertContext, bool> GetFilter(ModelPropertyInfo property)
        {
            return GetSourceProperty(property)?.Filter;
        }

        //public ModelConvertOptions Map<TSource, TTarget>(Expression<Func<TSource, object>> source, Expression<Func<TTarget, object>> target)
        //{
        //    return Map(source, target, null);
        //}

        //public ModelConvertOptions Map<TSource, TTarget>(Expression<Func<TSource, object>> source, Expression<Func<TTarget, object>> target, Func<TSource, object, object> converter)
        //{
        //    return this;
        //}

        //public ModelConvertOptions Include<TTarget>(Expression<Func<TTarget, object>> expression)
        //{
        //    return this;
        //}

        //public ModelConvertOptions Exclude<TTarget>(Expression<Func<TTarget, object>> expression)
        //{
        //    return this;
        //}

        //public ModelConvertOptions Property<T>(Expression<Func<T, object>> selector, PropertyConvertOptions options)
        //{
        //    if (selector == null)
        //    {
        //        throw new ArgumentNullException(nameof(selector));
        //    }

        //    if (options == null)
        //    {
        //        throw new ArgumentNullException(nameof(options));
        //    }

        //    PropertyInfo propertyInfo = ExtractProperty(selector);

        //    SetPropertyOptions(options, propertyInfo, typeof(T));

        //    return this;
        //}

        //public ModelConvertOptions Ignore<T>(Expression<Func<T, object>> selector)
        //{
        //    if (selector == null)
        //    {
        //        throw new ArgumentNullException(nameof(selector));
        //    }

        //    var info = ExtractProperty(selector);

        //    var options = GetPropertyOptions(typeof(T), info.Name);

        //    if (options == null)
        //    {
        //        options = new PropertyConvertOptions
        //        {
        //            Ignore = true
        //        };

        //        SetPropertyOptions(options, info, typeof(T));
        //    }
        //    else
        //        options.Ignore = true;

        //    return this;
        //}

        //private void SetPropertyOptions(PropertyConvertOptions options, PropertyInfo propertyInfo, Type modelType)
        //{
        //    if (!_properties.TryGetValue(modelType, out var dict))
        //    {
        //        dict = new Dictionary<string, PropertyConvertOptions>();
        //        _properties.Add(modelType, dict);
        //    }

        //    dict[propertyInfo.Name] = options;
        //}

        //internal PropertyConvertOptions GetPropertyOptions(ModelPropertyInfo property)
        //{

        //    Type modelType = property.Model.ModelType;
        //    string name = property.Name;

        //    return GetPropertyOptions(modelType, name);
        //}

        //private PropertyConvertOptions GetPropertyOptions(Type modelType, string name)
        //{
        //    if (_properties.TryGetValue(modelType, out var dict))
        //    {
        //        if (dict.TryGetValue(name, out var options))
        //            return options;
        //    }

        //    return null;
        //}

    }

}
