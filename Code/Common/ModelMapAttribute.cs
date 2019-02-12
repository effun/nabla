using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ModelMapAttribute : Attribute
    {

        public ModelMapAttribute(Type soruceType, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            SourceType = soruceType ?? throw new ArgumentNullException(nameof(soruceType));
            PropertyName = propertyName;
        }

        public Type SourceType { get; private set; }

        public string PropertyName { get; private set; }
    }
}
