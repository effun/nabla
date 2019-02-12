using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla
{
    [DebuggerDisplay("Name={Name}, Type={Type.FullName}")]
    public class ModelPropertyInfo
    {
        PropertyInfo _property;
        Action<object, object> _setter;
        Func<object, object> _getter;
        ModelKeyType _keyType;
        int _order;
        ModelInfo _model;
        CollectionInfo _collectionInfo;
        ModelMapAttribute[] _maps;
        DisplayAttribute _display;

        internal ModelPropertyInfo(PropertyInfo info, ModelInfo model)
        {
            _property = info;
            _getter = CreateGetterAccessor(info);
            _setter = CreateSetterAccessor(info);
            _model = model;
            _collectionInfo = info.PropertyType.GetCollectionInfo();
            _maps = info.GetCustomAttributes<ModelMapAttribute>().ToArray();
            _display = info.GetCustomAttribute<DisplayAttribute>();

            if (_property.IsDefined(typeof(KeyAttribute)))
                _keyType = ModelKeyType.Explicit;

            var ca = _property.GetCustomAttribute<ColumnAttribute>();

            if (ca != null)
            {
                _order = ca.Order;
            }
        }

        public ModelInfo Model => _model;

        public string Name => _property.Name;

        public Type Type => _property.PropertyType;

        public PropertyInfo PropertyInfo => _property;

        public bool IsReadOnly => _setter == null;

        public int Order => _order;

        public bool IsCollection => _collectionInfo.InterfaceType != null;

        public CollectionInfo CollectionInfo => _collectionInfo;

        public DisplayAttribute Display => _display;

        public bool IsComplex => Type.IsComplex();

        /// <summary>
        /// Gets a value indicates weather a <see cref="KeyAttribute"/> has defined on this property.
        /// </summary>
        public bool IsKey => _keyType != ModelKeyType.None;

        public ModelKeyType KeyType { get => _keyType; internal set => _keyType = value; }

        internal string GetPropertyMap(Type sourceType)
        {
            return (_maps.FirstOrDefault(o => o.SourceType == sourceType) ??
                _maps.FirstOrDefault(o => sourceType.IsSubclassOf(o.SourceType)))?.PropertyName;
        }

        internal ModelMapAttribute[] Maps => _maps;

        public object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return _getter(instance);
        }

        public void SetValue(object instance, object value)
        {
            if (_setter == null)
                throw new InvalidOperationException($"The property {_property.Name} of {_property.DeclaringType} is read-only.");

            _setter(instance, value);
        }



        private static Action<object, object> CreateSetterAccessor(PropertyInfo property)
        {
            var method = property.GetSetMethod();
            
            if (method != null && method.IsPublic)
            {
                ParameterExpression value = Expression.Parameter(typeof(object), "value"),
                    instance = Expression.Parameter(typeof(object), "instance");

                return Expression.Lambda<Action<object, object>>(
                    Expression.Call(Expression.Convert(instance, property.DeclaringType), method,
                    Expression.Convert(value, property.PropertyType)), instance, value)
                    .Compile();

            }
            else
                return null;
        }


        private static Func<object, object> CreateGetterAccessor(PropertyInfo property)
        {
            var method = property.GetGetMethod();

            if (method != null)
            {
                ParameterExpression instance = Expression.Parameter(typeof(object), "instance");

                return Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Call(Expression.Convert(instance, property.DeclaringType), method), typeof(object)), instance).Compile();
            }
            else
                return null;
        }
        
        public TypeConverter GetConverter()
        {
            var attr = _property.GetCustomAttribute<TypeConverterAttribute>();

            if (attr != null)
            {
                return (TypeConverter)Activator.CreateInstance(Type.GetType(attr.ConverterTypeName, true));
            }

            return TypeDescriptor.GetConverter(_property.PropertyType);
        }

        public T GetCustomAttribute<T>(bool inherit = false)
            where T : Attribute
        {
            return _property.GetCustomAttribute<T>(inherit);
        }

        public IEnumerable<T> GetCustomAttributes<T>(bool inherit = false)
            where T : Attribute
        {
            return _property.GetCustomAttributes<T>(inherit);
        }

        public bool IsDefined(Type attributeType, bool inherit = false)
        {
            return _property.IsDefined(attributeType, inherit);
        }
    }


    public enum ModelKeyType
    {
        /// <summary>
        /// 不是主键
        /// </summary>
        None,
        /// <summary>
        /// 隐式声明的主键
        /// </summary>
        Implicit,
        /// <summary>
        /// 显式定义的主键
        /// </summary>
        Explicit
    }
}
