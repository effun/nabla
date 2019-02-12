using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    public struct ValueRange<T> : IRange
        where T : struct, IComparable
    {
        T? _upper, _lower;

        public ValueRange(T? lower, T? upper)
        {
            _upper = upper;
            _lower = lower;
        }

        public ValueRange(T value)
        {
            _upper = value;
            _lower = value;
        }

        public T Upper => _upper ?? default(T);

        IComparable IRange.GetUpperBound(RangePolicy policy) => _upper;

        public T Lower => _lower ?? default(T);

        IComparable IRange.GetLowerBound(RangePolicy policy) => _lower;

        bool IRange.UpperSpecified => _upper.HasValue;

        bool IRange.LowerSpecified => _lower.HasValue;
    }

    public interface IRange
    {
        bool UpperSpecified { get; }

        bool LowerSpecified { get; }

        IComparable GetUpperBound(RangePolicy policy);

        IComparable GetLowerBound(RangePolicy policy);
        
    }

    public enum RangePolicy
    {
        Fill,
        Edge
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RangePolicyAttribute : Attribute
    {
        public RangePolicyAttribute(RangePolicy policy)
        {
            Policy = policy;
        }

        public RangePolicy Policy { get; }
    }
}
