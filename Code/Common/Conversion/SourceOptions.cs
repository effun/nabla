using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Conversion
{
    public class SourceOptions<T> : SourceOptions
    {
        public SourceOptions(ModelConvertOptions model)
            : base(model, typeof(T))
        {

        }

        public SourceOptions<T> Filter<TResult>(Expression<Func<T, TResult>> expression, Func<object, PropertyConvertContext, bool> filter)
        {
            Property(expression).Filter = filter;

            return this;
        }

        public SourceOptions<T> Converter<TResult>(Expression<Func<T, TResult>> expression, Func<TResult, PropertyConvertContext, object> converter)
        {
            Property(expression).Converter = (value, context) => converter((TResult)value, context);

            return this;
        }

        public new T Value { get => (T)base.Value; internal set => base.Value = value; }

        public TTarget ConvertTo<TTarget>()
        {
            return ConvertTo<TTarget>(Value);
        }

        public TTarget ConvertTo<TTarget>(T source)
        {
            return ModelConvert.Convert<TTarget>(source, Options.Target<TTarget>().Options);
        }

        public void PopulateTo<TTarget>(TTarget target)
        {
            PopulateTo(target, Value);
        }

        public void PopulateTo<TTarget>(TTarget target, T source)
        {
            Target<TTarget>();

            ModelConvert.Populate(target, source, Options);

        }
    }

    public class SourceOptions : BaseOptions<SourcePropertyOptions>
    {
        public SourceOptions(ModelConvertOptions model, Type modelType)
            : base(model, modelType)
        {
        }

        protected override SourcePropertyOptions CreateProperty(string name)
        {
            return new SourcePropertyOptions(name);
        }

        public TargetOptions<T> Target<T>()
        {
            return Options.Target<T>();
        }

        public object ConvertTo(Type type)
        {
            return ModelConvert.Convert(Value, type, Options);
        }

        public object ConvertTo(object source, Type type)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Type st = source.GetType();

            if (st != ModelType && !st.IsSubclassOf(ModelType))
                throw new ArgumentException("Source type mismatch.");

            return ModelConvert.Convert(source, type, Options);
        }
    }

    public class SourcePropertyOptions : PropertyOptions
    {
        public SourcePropertyOptions(string propertyName)
            : base(propertyName)
        {
        }

        /// <summary>
        /// 为集合定义一个过滤器，该方法应定义在源模型的属性上，第一传入参数为源集合的项。
        /// </summary>
        public Func<object, PropertyConvertContext, bool> Filter { get; set; }

        public Func<object, PropertyConvertContext, object> Converter { get; set; }
    }
}
