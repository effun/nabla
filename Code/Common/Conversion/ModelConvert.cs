using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nabla.Conversion
{
    public static class ModelConvert
    {

        public static SourceOptions<T> Source<T>(T value)
        {
            ModelConvertOptions options = new ModelConvertOptions();
            
            var source = options.Source<T>();

            source.Value = value;

            return source;
        }

        public static TargetOptions<T> Target<T>()
        {
            ModelConvertOptions options = new ModelConvertOptions();

            return options.Target<T>();
        }

        public static TargetOptions<T> Target<T>(T value)
        {
            var options = Target<T>();

            options.Value = value;

            return options;
        }

        //public static object Convert(object source, Type type)
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException(nameof(source));
        //    }

        //    if (type == null)
        //    {
        //        throw new ArgumentNullException(nameof(type));
        //    }

        //    return Convert(source, type, null);
        //}

        public static object Convert(object source, Type type, ModelConvertOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (options == null)
            {
                options = new ModelConvertOptions();
            }

            return Convert(source, options.MatchSourceType(source.GetType()), type, options);
        }

        private static object Convert(object source, Type sourceType ,Type targetType, ModelConvertOptions options)
        {
            ModelInfo info0 = ModelInfo.GetModelInfo(sourceType)
                , info1 = ModelInfo.GetModelInfo(targetType);

            object target = info1.CreateInstance();

            WalkConvertiblProperties(target, info1, source, info0, options, SetPropertyValue, null);

            return target;

        }

        internal static TTarget Convert<TTarget>(object source, ModelConvertOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return (TTarget)Convert(source, typeof(TTarget), options ?? Target<TTarget>().Options);
        }

        //public static void Populate<TTarget, TSource>(TTarget target, TSource source)
        //{
        //    var options = new ModelConvertOptions();

        //    options
        //        .Source<TSource>()
        //        .Target<TTarget>();

        //    Populate(target, source, options);
        //}

        internal static void Populate<TTarget, TSource>(TTarget target, TSource source, ModelConvertOptions options)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Populate(target, typeof(TTarget), source, typeof(TSource), options);
        }

        //public static void Populate(object target, object source, Action<ModelConvertOptions> initOptions)
        //{
        //    ModelConvertOptions options;

        //    if (initOptions != null)
        //    {
        //        options = new ModelConvertOptions();
        //        initOptions(options);
        //    }
        //    else
        //        options = null;

        //    Populate(target, source, options);
        //}

        //public static void Populate(object target, object source)
        //{
        //    if (target == null)
        //    {
        //        throw new ArgumentNullException(nameof(target));
        //    }

        //    if (source == null)
        //    {
        //        throw new ArgumentNullException(nameof(source));
        //    }

        //    Populate(target, source, (ModelConvertOptions)null);
        //}

        //public static void Populate(object target, object source, ModelConvertOptions options)
        //{
        //    if (target == null)
        //    {
        //        throw new ArgumentNullException(nameof(target));
        //    }

        //    if (source == null)
        //    {
        //        throw new ArgumentNullException(nameof(source));
        //    }

        //    Populate(target, target.GetType(), source, source.GetType(), options);
        //}

        public static void Populate(object target, Type targetType, object source, Type sourceType, ModelConvertOptions options)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (options == null)
            {
                options = new ModelConvertOptions();
            }

            WalkConvertiblProperties(
                target, ModelInfo.GetModelInfo(options.MatchTargetType(targetType)),
                source, ModelInfo.GetModelInfo(options.MatchSourceType(sourceType)),
                options, SetPropertyValue, null);
        }

        internal static void WalkConvertiblProperties(object target, ModelInfo targetInfo, object source, ModelInfo sourceInfo,
            ModelConvertOptions options, Action<ModelPropertyInfo, PropertyConvertContext, object> callback, object state)
        {
            foreach (var mp1 in targetInfo.Properties)
            {
                var mp0 = MapProperty(mp1, sourceInfo, options);

                if (mp0.Length > 0 && options.ShouldSetValue(mp1))
                {
                    PropertyConvertContext context = new PropertyConvertContext(source, mp0, target, mp1, GetValue(mp0, source), options);

                    callback(mp1, context, state);
                }
            }
        }

        private static void SetPropertyValue(ModelPropertyInfo mp1, PropertyConvertContext context, object state)
        {
            if (!mp1.IsCollection /*|| mp1.Type.IsAssignableFrom(value.GetType()) && !mp1.IsReadOnly*/ )
                SetValueScalar(context);
            else
                SetValueCollection(context);
        }

        private static object GetValue(ModelPropertyInfo[] chain, object instance)
        {
            object value = null;

            if (instance != null)
            {
                foreach (var p in chain)
                {
                    value = p.GetValue(instance);

                    if (value != null)
                    {
                        instance = value;
                    }
                    else
                        break;
                }
            }

            return value;
        }

        internal static void FillPropertyChain(ModelInfo sourceModel, string name, List<ModelPropertyInfo> chain)
        {
            ModelInfo m0 = sourceModel;

            string[] path = name.Split(Type.Delimiter);
            int count = path.Length;

            for (int i = 0; i < count; i++)
            {
                string n = path[i];
                var p0 = m0.GetProperty(n);

                if (p0 == null)
                    throw new InvalidOperationException("Property " + n + " not found on " + m0.ModelType);

                chain.Add(p0);

                if (i < count - 1)
                    m0 = ModelInfo.GetModelInfo(p0.Type);
            }

        }

        private static ModelPropertyInfo[] MapProperty(ModelPropertyInfo targetProperty, ModelInfo sourceModel, ModelConvertOptions options)
        {
            List<ModelPropertyInfo> chain = new List<ModelPropertyInfo>(3);
            string name = options.MapProperty(targetProperty) ?? targetProperty.GetPropertyMap(sourceModel.ModelType);

            if (name != null)
            {
                FillPropertyChain(sourceModel, name, chain);
            }
            else
            {
                var p0 = sourceModel.GetProperty(name = targetProperty.Name);

                if (p0 == null)
                {
                    Stack<ModelPropertyInfo> stack = null;

                    if (FindNestedProperty(name, sourceModel, ref stack))
                        chain.AddRange(stack);
                }
                else
                    chain.Add(p0);
            }

            return chain.ToArray();
        }

        private static bool FindNestedProperty(string name, ModelInfo sourceModel, ref Stack<ModelPropertyInfo> chain)
        {
            foreach (var p in sourceModel.Properties)
            {
                string n = p.Name;

                if (name.StartsWith(n))
                {
                    if (name != n)
                    {
                        if (ModelInfo.IsModelTypeSupported(p.Type))
                        {
                            var m = ModelInfo.GetModelInfo(p.Type);
                            var n1 = name.Substring(n.Length);

                            if (FindNestedProperty(n1, m, ref chain))
                            {
                                chain.Push(p);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (chain == null)
                            chain = new Stack<ModelPropertyInfo>(5);

                        chain.Push(p);

                        return true;
                    }
                }
            }

            return false;
        }

        private static void SetValueScalar(PropertyConvertContext context)
        {
            if (CheckReadOnly(context))
                return;

            ConvertPropertyValue(context);

            object value = context.Value;

            if (value == null)
            {
                Type type = context.TargetProperty.Type;

                if (!type.CanBeNull())
                {
                    value = context.Options.GetDefaultValue(context) ?? type.DefaltValue();
                    //throw new InvalidOperationException($"Try to set null value to a property which cannot be null. Property = {mp1.PropertyInfo.DeclaringType}.{mp1.Name}");
                }
            }

            context.TargetProperty.SetValue(context.Target, value);
        }

        private static bool CheckReadOnly(PropertyConvertContext context)
        {
            if (context.TargetProperty.IsReadOnly)
            {
                if (!context.Options.ShouldIgnoreReadOnly(context.TargetProperty))
                    throw new InvalidOperationException($"The property {context.TargetModel.ModelType}.{context.TargetProperty.Name} is read-only.");
                else
                    return true;
            }

            return false;
        }

        private static void SetValueCollection(PropertyConvertContext context)
        {
            var collInfo = context.TargetProperty.CollectionInfo;
            ArrayList coll0;

            bool man = collInfo.SupportManipulation;

            if (!context.SourceProperty.IsCollection)
                throw new InvalidOperationException($"The source property {context.SourceModel.ModelType}.{context.SourceProperty.Name} is not a collection type.");

            if (context.Value != null)
            {
                var wrapper = context.SourceProperty.CollectionInfo.CreateWrapper(context.Value);
                var filter = context.Options.GetFilter(context.SourceProperty);

                coll0 = new ArrayList(wrapper.Count);

                foreach (object v in wrapper)
                {
                    if (filter?.Invoke(v, context) ?? true)
                    {
                        var tv = ConvertPropertyValue(v, context.SourceProperty.CollectionInfo.ElementType ?? typeof(object), collInfo.ElementType ?? typeof(object), null, context);
                        coll0.Add(tv);
                    }
                }
            }
            else
            {
                SetValueScalar(context);
                return;
            }

            if (collInfo.IsArray)
            {
                man = false;
                context.Value = coll0.ToArray(collInfo.ElementType ?? typeof(object));

                SetValueScalar(context);
            }
            else if (man)
            {
                var tv = context.TargetProperty.GetValue(context.Target);
                ICollectionWrapper coll1;

                if (tv != null)
                {
                    coll1 = collInfo.CreateWrapper(tv);
                    coll1.Clear();
                }
                else
                    coll1 = collInfo.CreateWrapper();

                foreach (object v in coll0)
                    coll1.Add(v);

                if (tv == null)
                {
                    context.Value = coll1.RawCollection;
                    SetValueScalar(context);
                }
            }
            else
                throw new NotSupportedException("Collection type conversion for " + context.TargetProperty.Type + " is not supported yet.");

            //if (collInfo.SupportManipulation)
            //{
            //    object tv = context.TargetProperty.GetValue(context.TargetProperty);

            //    if (tv != null)
            //    {
            //        var coll1 = collInfo.CreateWrapper(tv);

            //        coll1.Clear();

            //        if (coll0 != null)
            //        {
            //            foreach (object v in coll0)
            //                coll1.Add(v);
            //        }

            //        return;
            //    }

            //}

            //if (CheckReadOnly(context))
            //    return;

            //if (coll0 == null)
            //{
            //    SetValueScalar(context);
            //    return;
            //}

            //object value;

            //// 如果是数组，创建一个新的数组并赋值。
            //if (collInfo.IsArray)
            //{
            //    Array array = Array.CreateInstance(collInfo.ElementType, coll0.Count);
            //    int index = 0;
            //    foreach (var v in coll0)
            //        array.SetValue(v, index);

            //    value = array;

            //    return;
            //}

            //if (collInfo.ObjectType.IsInterface)
            //{
            //    // 如果是接口，创建一个List`1或ArrayList
            //    IList list;


            //}
            //else
            //{
            //    // 直接创建集合的实例，并进行逐一添加
            //    value = ModelInfo.GetModelInfo(collInfo.ObjectType).CreateInstance();
            //}
        }

        private static void ConvertPropertyValue(PropertyConvertContext context)
        {
            if (!context.Options.Convert(context))
                context.Value = ConvertPropertyValue(context.Value, context.SourceProperty.Type, context.TargetProperty.Type, context.TargetProperty, context);
        }

        private static object ConvertPropertyValue(object source, Type sourceType, Type targetType, ModelPropertyInfo property, PropertyConvertContext context)
        {
            if (source == null || targetType.IsAssignableFrom(sourceType))
                return source;

            TypeConverter converter;

            if (property == null)
                converter = TypeDescriptor.GetConverter(targetType);
            else
                converter = property.GetConverter();

            if (converter != null && converter.CanConvertTo(targetType))
            {
                return converter.ConvertTo(source, targetType);
            }

            Type type;
            if ((type = Nullable.GetUnderlyingType(targetType)) != null)
            {
                if (type == source.GetType())
                    return source;
            }

            try
            {
                return System.Convert.ChangeType(source, targetType);
            }
            catch
            {

            }

            if (sourceType.IsClass && targetType.IsClass)
            {
                return Convert(source, sourceType, targetType, context.Options);
            }

            throw new InvalidOperationException($"Cannot convert type from {source.GetType()} to {targetType}.");
        }
    }

    public class PropertyConvertContext
    {
        internal PropertyConvertContext(object source, ModelPropertyInfo[] sourcePropertyChain, object target, ModelPropertyInfo targetProperty, object value, ModelConvertOptions options)
        {
            _sourceProperties = sourcePropertyChain;
            TargetProperty = targetProperty;
            Value = value;
            Source = source;
            Target = target;
            Options = options;
        }

        private ModelPropertyInfo[] _sourceProperties;

        public object Source { get; private set; }

        public object Target { get; private set; }

        public ModelPropertyInfo SourceProperty => _sourceProperties[0];

        public ModelPropertyInfo[] SourcePropertyChain => _sourceProperties;

        public ModelPropertyInfo TargetProperty { get; private set; }

        public ModelInfo SourceModel => SourceProperty.Model;

        public ModelInfo TargetModel => TargetProperty.Model;

        public object Value { get; set; }

        public ModelConvertOptions Options { get; private set; }

    }


}
