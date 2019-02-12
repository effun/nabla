using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    [TypeConverter(typeof(DateTimeRangeConverter))]
    public struct DateTimeRange : IRange, IEquatable<DateTimeRange>
    {
        DateTime? _lower, _upper;

        public DateTimeRange(DateTime? lower, DateTime? upper)
        {
            _lower = lower;
            _upper = upper;
        }

        public bool UpperSpecified => _upper.GetValueOrDefault() != default(DateTime);

        public bool LowerSpecified => _lower.GetValueOrDefault() != default(DateTime);

        public DateTime? Upper => _upper;

        public DateTime? Lower => _lower;

        IComparable IRange.GetLowerBound(RangePolicy policy)
        {
            if (_lower != null)
            {
                if (policy == RangePolicy.Fill)
                    return _lower.Value.Date;
                else
                    return _lower.Value;
            }
            else
                return null;
        }

        IComparable IRange.GetUpperBound(RangePolicy policy)
        {
            if (_upper != null)
            {
                if (policy == RangePolicy.Fill)
                {
                    return _upper.Value.Date.AddDays(1).AddSeconds(-1);
                }
                else
                    return _upper;
            }
            else
                return null;
        }

        public static readonly DateTimeRange Empty = new DateTimeRange(null, null);

        public bool IsEmpty => !UpperSpecified && !LowerSpecified;

        public override string ToString()
        {
            return ToString("d", DateTimeRangeConverter.DefaultSepartors[0]);
        }

        public string ToString(string dateFormat, char seperator)
        {
            StringBuilder text = new StringBuilder();

            if (LowerSpecified)
                text.Append(_lower.Value.ToString(dateFormat));
            text.Append(' ');
            text.Append(seperator);
            text.Append(' ');
            if (UpperSpecified)
                text.Append(_upper.Value.ToString(dateFormat));

            return text.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is DateTimeRange && Equals((DateTimeRange)obj);
        }

        public bool Equals(DateTimeRange other)
        {
            return EqualityComparer<DateTime?>.Default.Equals(_lower, other._lower) &&
                   EqualityComparer<DateTime?>.Default.Equals(_upper, other._upper);
        }

        public override int GetHashCode()
        {
            var hashCode = -2116856867;
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(_lower);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(_upper);
            return hashCode;
        }

        public static bool operator ==(DateTimeRange range1, DateTimeRange range2)
        {
            return range1.Equals(range2);
        }

        public static bool operator !=(DateTimeRange range1, DateTimeRange range2)
        {
            return !(range1 == range2);
        }
    }

    public class DateTimeRangeConverter : TypeConverter
    {
        public const string DefaultSepartors = "~,";

        private char[] _separtors;

        public DateTimeRangeConverter()
            : this(DefaultSepartors)
        {

        }

        public DateTimeRangeConverter(string separtor)
            : this(separtor.ToCharArray())
        {
        }

        public DateTimeRangeConverter(char[] seperators)
        {
            _separtors = seperators;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text)
            {
                int p = text.IndexOfAny(_separtors);
                string left, right;

                if (p >= 0)
                {
                    if (p > 0)
                        left = text.Substring(0, p).Trim();
                    else
                        left = string.Empty;

                    if (p < text.Length - 1)
                        right = text.Substring(p + 1).Trim();
                    else
                        right = string.Empty;

                    DateTime? low, upper;

                    if (left.Length > 0) low = DateTime.Parse(left);
                    else low = null;

                    if (right.Length > 0) upper = DateTime.Parse(right);
                    else upper = null;

                    return new DateTimeRange(low, upper);

                }
                else
                    left = right = text;

            }
            else if (value == null)
                return DateTimeRange.Empty;

            return base.ConvertFrom(context, culture, value);
        }
    }
}
