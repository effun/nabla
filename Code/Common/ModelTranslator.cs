using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Nabla
{
    public class ModelTranslator<TSource, TTarget>
            where TSource : class
            where TTarget : class
    {

        public ModelTranslator()
        {

        }

        public ModelTranslator(bool ignoreKeys)
        {
            IgnoreKeys = ignoreKeys;
        }

        PropertyDescriptorCollection _sourceProperties, _targetProperties;

        private void EnsureInitialized()
        {
            if (_sourceProperties == null)
            {
                _sourceProperties = TypeDescriptor.GetProperties(typeof(TSource));
                _targetProperties = TypeDescriptor.GetProperties(typeof(TTarget));
            }
        }


        public TTarget Translate(TSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            TTarget target = Activator.CreateInstance<TTarget>();

            Translate(source, target);

            return target;
        }

        public void Translate(TSource source, TTarget target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            EnsureInitialized();

            foreach (PropertyDescriptor p0 in _sourceProperties)
            {
                string name = p0.Name;

                PropertyDescriptor p1 = _targetProperties[name];

                if (p1 != null && (!IgnoreKeys || !IsKey(p1)))
                {
                    Type t0, t1;

                    t0 = p0.PropertyType;
                    t1 = p1.PropertyType;

                    if (t1.IsAssignableFrom(t0))
                        p1.SetValue(target, p0.GetValue(source));
                    else
                        throw new InvalidCastException($"Cannot cast {t0} to {t1} for {name} of {typeof(TTarget)}.");
                }

            }
        }

        private bool IsKey(PropertyDescriptor property)
        {
            if (property.Attributes[typeof(KeyAttribute)] != null)
                return true;

            if (property.Name.ToUpper() == "ID")
                return true;

            return false;
        }

        public bool IgnoreKeys { get; set; } = false;
    }
}