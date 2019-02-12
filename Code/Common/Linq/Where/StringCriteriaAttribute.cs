using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StringCriteriaAttribute : Attribute
    {
        public StringCriteriaAttribute(StringOperator @operator, bool ignoreEmpty = true)
        {
            Operator = @operator;
            IgnoreEmpty = ignoreEmpty;
        }

        public StringOperator Operator { get; private set; }

        public bool IgnoreEmpty { get; private set; }
    }
}
