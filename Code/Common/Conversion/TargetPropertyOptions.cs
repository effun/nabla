using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Conversion
{
    public class TargetOptions : BaseOptions<TargetPropertyOptions>
    {
        public TargetOptions(ModelConvertOptions model, Type modelType)
            : base(model, modelType)
        {
        }

        protected override TargetPropertyOptions CreateProperty(string name)
        {
            return new TargetPropertyOptions(name);
        }

        public TargetOptions IgnoreKeys()
        {
            _ignoreKeys = true;
            return this;
        }

        public TargetOptions IgnoreReadOnly()
        {
            _ignoreReadOnly = true;
            return this;
        }

        private bool _ignoreKeys, _ignoreReadOnly;

        internal bool KeysIgnored => _ignoreKeys;

        internal bool ReadOnlyIgnored => _ignoreReadOnly;

        public SourceOptions<T> Source<T>()
        {
            return Options.Source<T>();
        }
    }

    public class TargetOptions<T> : TargetOptions

    {
        public TargetOptions(ModelConvertOptions model)
            : base(model, typeof(T))
        {
        }

        public TargetOptions<T> Ignore<TResult>(Expression<Func<T, TResult>> expression)
        {
            Property(expression).Ignore = true;

            return this;
        }

        public TargetOptions<T> Convert<TResult>(Expression<Func<T, TResult>> expression, Func<object, PropertyConvertContext, TResult> action)
        {
            Property(expression).Converter = (value, context) => action(value, context);
            return this;
        }

        public TargetOptions<T> DefaultValue<TResult>(Expression<Func<T, TResult>> expression, TResult defalutValue)
        {
            Property(expression).DefaultValue = defalutValue;
            return this;
        }

        public TargetOptions<T> MapTo<TResult>(Expression<Func<T, TResult>> expression, string sourcePropertyName)
        {
            Property(expression).MapTo = sourcePropertyName;
            return this;
        }

        public new TargetOptions<T> IgnoreKeys()
        {
            return (TargetOptions<T>)base.IgnoreKeys();
        }

        public new TargetOptions<T> IgnoreReadOnly()
        {
            return (TargetOptions<T>)base.IgnoreReadOnly();
        }

        public new T Value { get => (T)base.Value; internal set => base.Value = value; }

        public T ConvertBy<TSource>(TSource source)
        {
            return Source<TSource>().ConvertTo<T>(source);
        }

        public void PopulateBy<TSource>(TSource source)
        {
            Source<TSource>().PopulateTo(Value, source);
        }

    }

    public class TargetPropertyOptions : PropertyOptions
    {
        public TargetPropertyOptions(string propertyName)
            : base(propertyName)
        {

        }

        /// <summary>
        /// 指示是否忽略此属性，定义在目标属性上。
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 为目标属性定义一个转换器，用于将源模型的值转换为目标模型的值。
        /// </summary>
        public Func<object, PropertyConvertContext, object> Converter { get; set; }

        /// <summary>
        /// 属性的默认值，定义在目标属性上。
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 指定将目标属性影射到源模型的哪个属性上，定义在目标属性上。
        /// </summary>
        public string MapTo { get; set; }
    }


    public class PropertyOptions
    {
        public PropertyOptions(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }

    public abstract class BaseOptions<T>
        where T : PropertyOptions
    {
        Dictionary<string, T> _options;

        public BaseOptions(ModelConvertOptions model, Type modelType)
        {
            Options = model;
            ModelType = modelType;
        }

        public ModelConvertOptions Options { get; }

        public Type ModelType { get; }

        public T this[string propertyName]
        {
            get
            {
                if (_options == null || !_options.TryGetValue(propertyName, out T po))
                    return null;

                return po;
            }
            //protected set
            //{
            //    if (_options == null)
            //        _options = new Dictionary<string, T>();

            //    _options[propertyName] = value;
            //}
        }

        protected abstract T CreateProperty(string name);

        public T Property(string name)
        {
            if (_options == null)
                _options = new Dictionary<string, T>();

            if (!_options.TryGetValue(name, out T value))
            {
                value = CreateProperty(name);
                _options.Add(name, value);
            }

            return value;
        }

        protected T Property(PropertyInfo property)
        {
            return Property(property.Name);
        }

        protected T Property(LambdaExpression lambda)
        {
            return Property(ExpressionHelper.ExtractProperty(lambda));
        }


        public object Value { get; internal set; }

    }

}
