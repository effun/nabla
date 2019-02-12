using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla
{
    [DebuggerDisplay("ModelType={ModelType.FullName}")]
    public class ModelInfo
    {
        Type _modelType;
        Dictionary<string, ModelPropertyInfo> _properties;
        Func<object> _ctor;
        ModelPropertyInfo[] _key;

        private ModelInfo(Type type)
        {
            _modelType = type;
            _ctor = CreateConstructorAccessor(type);
            _properties = new Dictionary<string, ModelPropertyInfo>();

            List<ModelPropertyInfo> list = new List<ModelPropertyInfo>(3);

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                ModelPropertyInfo mpi = new ModelPropertyInfo(property, this);

                if (_properties.ContainsKey(property.Name))
                {
                    if (property.DeclaringType != type)
                        continue;

                    _properties[property.Name] = mpi;
                }
                else
                    _properties.Add(property.Name, mpi);

                if (mpi.IsKey) list.Add(mpi);
            }

            if (list.Count == 0)
            {
                ModelPropertyInfo key;

                if ((key = FindProperty("Id")) != null)
                    list.Add(key);
                else if ((key = FindProperty(type.Name + "Id")) != null)
                    list.Add(key);
                else if ((key = FindProperty("Key")) != null)
                    list.Add(key);
                else if ((key = FindProperty("RowId")) != null)
                    list.Add(key);
                else if ((key = FindProperty("RowKey")) != null)
                    list.Add(key);

                if (key != null)
                    key.KeyType = ModelKeyType.Implicit;
            }

            _key = list.OrderBy(o => o.Order).ToArray();
        }

        private ModelPropertyInfo FindProperty(string name)
        {
            name = name.ToUpper();
            return _properties.Values.FirstOrDefault(o => o.PropertyInfo.Name.ToUpper() == name);
        }

        public Type ModelType => _modelType;

        public IEnumerable<ModelPropertyInfo> Properties => _properties.Values;

        public ModelPropertyInfo GetProperty(string name)
        {
            if (_properties.TryGetValue(name, out ModelPropertyInfo info))
                return info;
            else
                return null;
        }

        public bool ContainsProperty(string name)
        {
            if (!string.IsNullOrEmpty(name))
                return _properties.ContainsKey(name);
            else
                return false;
        }

        private static Func<object> CreateConstructorAccessor(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);

            if (ctor != null)
                return Expression.Lambda<Func<object>>(Expression.Convert(Expression.New(ctor), typeof(object))).Compile();
            else
                return null;
        }

        public object CreateInstance()
        {
            if (_ctor == null)
                throw new InvalidOperationException($"Default constructor not found of type {_modelType}");

            return _ctor();
        }

        public object[] GetKeyValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_key.Length == 0)
                throw new InvalidOperationException($"Model {_modelType} does not contain key property.");

            return _key.Select(o => o.GetValue(instance)).ToArray();
        }

        public ICollection<ModelPropertyInfo> Keys
        {
            get => Array.AsReadOnly(_key);
        }

        public object[] GetKeyValue(object instance, Type referenceType)
        {
            return GetKeyValue(instance, GetModelInfo(referenceType));
        }

        public object[] GetKeyValue(object instance, ModelInfo referenceModel)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (referenceModel == null)
            {
                throw new ArgumentNullException(nameof(referenceModel));
            }

            if (referenceModel._key.Length == 0)
                throw new InvalidOperationException($"The reference model {referenceModel._modelType} does not contain key property.");

            int count = referenceModel._key.Length;
            object[] value = new object[count];

            for (int i = 0; i< count; i++)
            {
                var mp = GetProperty(referenceModel._key[i].Name);

                if (mp == null)
                    throw new InvalidOperationException($"Key property {mp.Name} defined in {referenceModel._modelType} does not define in {_modelType}");

                value[i] = mp.GetValue(instance);
            }

            return value;
        }

        private static ConcurrentDictionary<Type, ModelInfo> _models;

        public static ModelInfo GetModelInfo(Type type)
        {
            if (!IsModelTypeSupported(type))
                throw new NotSupportedException("Getting model information is not supported on " + type.FullName);

            if (_models == null)
                _models = new ConcurrentDictionary<Type, ModelInfo>();

            if (!_models.TryGetValue(type, out ModelInfo info))
            {
                info = new ModelInfo(type);
                _models.TryAdd(type, info);
            }

            return info;
        }

        internal static bool IsModelTypeSupported(Type type)
        {
            TypeCode code = Type.GetTypeCode(type);

            if (code != TypeCode.Object)
                return false;

            return true;
        }
    }

}
