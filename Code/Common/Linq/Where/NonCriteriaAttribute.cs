using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NonCriteriaAttribute : Attribute
    {
    }
}
