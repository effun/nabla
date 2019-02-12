using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    public abstract class Criteria
    {

        public static Criteria Create(object value)
        {
            return Create(value, null);
        }

        public static Criteria Create(object value, ModelPropertyInfo property)
        {
            if (TryCreate(value, property, out Criteria criteria))
                return criteria;

            throw new NotSupportedException("Unable to convert " + value.GetType() + " to criteria implicitly.");

        }

        public static bool TryCreate(object value, out Criteria criteria)
        {
            return TryCreate(value, null, out criteria);
        }

        public static bool TryCreate(object value, ModelPropertyInfo property, out Criteria criteria)
        {

            if (value == null)
            {
                criteria = null;
                return true;
            }

            Type type = value.GetType();

            if (value is Criteria)
                criteria = (Criteria)value;
            else if (value is string)
                criteria = String((string)value, property);
            else if (value == DBNull.Value)
                criteria = IsNull();
            else if (value is IRange range)
            {
                RangePolicyAttribute attr = property?.GetCustomAttribute<RangePolicyAttribute>();

                criteria = Between(range, attr?.Policy ?? default(RangePolicy));
            }
            else if (type.IsArray)
                criteria = Contain((Array)value);
            else if (type.IsGenericType)
            {
                var info = TypeHelpers.GetCollectionInfo(type);

                if (info.IsCollection)
                    criteria = Contain((ICollection)value);
                else
                {
                    Type type1 = Nullable.GetUnderlyingType(type);

                    if (type1 != null)
                        type = type1;

                    criteria = null;
                }
            }
            else
                criteria = null;

            if (criteria == null && value is IComparable comparable)
                criteria = Equal(comparable);

            return criteria != null;
        }

        public static StringCriteria String(string value)
        {
            StringOperator so;


            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length > 1)
                {
                    char ind1 = value[0], ind2 = value[value.Length - 1];

                    if (ind1 == '=')
                    {
                        value = value.Substring(1);
                        so = StringOperator.Equal;
                    }
                    else if (ind1 == '*' || ind2 == '*')
                    {
                        so = StringOperator.StartsWith;

                        if (ind1 == '*')
                        {
                            value = value.Substring(1);
                            so = StringOperator.EndsWith;
                        }

                        if (ind2 == '*')
                        {
                            value = value.Substring(0, value.Length - 1);
                            if (so == StringOperator.EndsWith)
                                so = StringOperator.Contains;
                        }
                    }
                    else
                        so = StringOperator.Contains;
                }
                else
                    so = StringOperator.Contains;
            }
            else
            {
                value = null;
                so = StringOperator.Equal;
            }

            return new StringCriteria(value, so);
        }

        private static StringCriteria String(string value, ModelPropertyInfo property)
        {
            var attr = property?.GetCustomAttribute<StringCriteriaAttribute>();

            if (attr == null)
                return String(value);

            return String(value, attr.Operator, attr.IgnoreEmpty);

        }

        public static StringCriteria String(string value, StringOperator @operator, bool ignoreEmpty = true)
        {
            if (ignoreEmpty && string.IsNullOrEmpty(value))
                value = null;

            return new StringCriteria(value, @operator);
        }

        public static ContainCriteria Contain(ICollection coll)
        {
            Array values;

            if (coll != null)
            {
                if (coll == null)
                    throw new ArgumentNullException(nameof(coll));

                Type type = coll.GetType();

                if (!type.IsArray)
                {
                    if (!type.IsGenericType)
                        throw new ArgumentException("A generic collection is required");

                    type = type.GetGenericArguments()[0];

                    values = Array.CreateInstance(type, coll.Count);

                    coll.CopyTo(values, 0);

                }
                else
                    values = (Array)coll;
            }
            else
                values = null;

            return new ContainCriteria(values);
        }

        public static CompareCriteria Compare(IComparable value, ComparisonOperator @operator)
        {
            return new CompareCriteria(value, @operator);
        }

        public static CompareCriteria Equal(IComparable value)
        {
            return Compare(value, ComparisonOperator.Equal);
        }

        public static CompareCriteria LessThan(IComparable value)
        {
            return Compare(value, ComparisonOperator.LessThan);
        }

        public static CompareCriteria LessThanOrEqual(IComparable value)
        {
            return Compare(value, ComparisonOperator.LessThanOrEqual);
        }

        public static CompareCriteria GreaterThan(IComparable value)
        {
            return Compare(value, ComparisonOperator.GreaterThan);
        }

        public static CompareCriteria GreaterThanOrEqual(IComparable value)
        {
            return Compare(value, ComparisonOperator.GreaterThanOrEqual);
        }

        public static RangeCriteria Between<T>(T? lower, T? upper, RangePolicy policy = RangePolicy.Fill)
            where T: struct, IComparable
        {
            if (lower != null && upper != null)
            {
                if (lower.Value.CompareTo(upper.Value) > 0)
                {
                    T? temp = upper;

                    upper = lower;
                    lower = temp;
                }
            }

            return new RangeCriteria(new ValueRange<T>(lower, upper), policy);
        }

        public static RangeCriteria Between(IRange range, RangePolicy policy)
        {
            return new RangeCriteria(range, policy);
        }

        public static NullTestCriteria IsNull(bool isNull = true)
        {
            return new NullTestCriteria(isNull);
        }

        internal protected abstract Expression CreateExpression(MemberExpression member, WhereBuilderContext context);

    }

    public enum ComparisonOperator
    {
        Equal,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
    }

    public class CompareCriteria : Criteria
    {
        internal CompareCriteria(IComparable value, ComparisonOperator opt)
        {
            Value = value;
            Operator = opt;
        }

        public IComparable Value { get; private set; }
        
        public ComparisonOperator Operator { get; private set; }

        protected internal override Expression CreateExpression(MemberExpression member, WhereBuilderContext context)
        {
            if (Value == null)
                return null;

            var value = Expression.Constant(Value, member.Type);

            switch (Operator)
            {
                case ComparisonOperator.Equal:
                    return Expression.Equal(member, value);
                case ComparisonOperator.LessThan:
                    return Expression.LessThan(member, value);
                case ComparisonOperator.GreaterThan:
                    return Expression.GreaterThan(member, value);
                case ComparisonOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, value);
                case ComparisonOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, value);
                default:
                    throw new NotSupportedException("Comparision operator " + Operator + " is invalid.");
            }
        }

    }

    public enum StringOperator
    {
        Equal,
        Contains,
        StartsWith,
        EndsWith,
    }

    public class StringCriteria : Criteria
    {
        internal StringCriteria(string value, StringOperator @operator)
        {
            Value = value;
            Operator = @operator;
        }

        public string Value { get; private set; }

        public StringOperator Operator { get; private set; }

        protected internal override Expression CreateExpression(MemberExpression member, WhereBuilderContext context)
        {
            if (Value == null)
                return null;

            var value = Expression.Constant(Value, member.Type);

            if (Operator != StringOperator.Equal)
            {
                return Expression.Call(member, TypicalLinqMethods.GetMethod("String" + Operator), value);
            }
            else
                return Expression.Equal(member, value, true, null);

        }
    }

    public class RangeCriteria : Criteria
    {
        CompareCriteria _lower, _upper;

        internal RangeCriteria(IRange range, RangePolicy policy)
        {
            if (range.LowerSpecified)
                _lower = new CompareCriteria(range.GetLowerBound(policy), ComparisonOperator.GreaterThanOrEqual);
            if (range.UpperSpecified)
                _upper = new CompareCriteria(range.GetUpperBound(policy), ComparisonOperator.LessThanOrEqual);
        }

        public IComparable UpperValue
        {
            get => _upper.Value;
        }

        public IComparable LowerValue
        {
            get => _lower.Value;
        }

        protected internal override Expression CreateExpression(MemberExpression member, WhereBuilderContext context)
        {
            Expression lower, upper;

            lower = _lower?.CreateExpression(member, context);
            upper = _upper?.CreateExpression(member, context);

            if (upper != null)
            {
                if (lower != null)
                    lower = Expression.And(lower, upper);
                else
                    lower = upper;
            }

            return lower;
        }

    }

    public class ContainCriteria : Criteria
    {
        Array _values;
        Type _elementType;

        internal ContainCriteria(Array array)
        {
            if (array != null)
            {
                _values = array;
                _elementType = array.GetType().GetElementType();
            }
        }

        public ICollection Values
        {
            get => _values;
        }

        protected internal override Expression CreateExpression(MemberExpression member, WhereBuilderContext context)
        {
            if (_values != null)
            {
                var array = Expression.Constant(_values, typeof(IEnumerable<>).MakeGenericType(_elementType));
                return Expression.Call(null, TypicalLinqMethods.EnumerableContains.MakeGenericMethod(_elementType), array, member);
            }
            else
                return null;
        }
    }

    public class NullTestCriteria : Criteria
    {
        internal NullTestCriteria(bool isNull )
        {
            ShouldBeNull = isNull;
        }

        public bool ShouldBeNull { get; private set; }

        protected internal override Expression CreateExpression(MemberExpression member, WhereBuilderContext context)
        {
            Expression expr;

            if (Nullable.GetUnderlyingType(member.Type) == null)
            {
                Expression value = Expression.Constant(null);

                if (ShouldBeNull)
                    expr = Expression.Equal(member, value, true, null);
                else
                    expr = Expression.NotEqual(member, value, true, null);
            }
            else
            {
                expr = Expression.PropertyOrField(member, "HasValue");

                if (ShouldBeNull)
                    expr = Expression.Not(expr);
            }

            return expr;
        }
    }

}
