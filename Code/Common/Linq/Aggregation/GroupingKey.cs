using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Nabla.Linq.Aggregations
{
    internal class GroupingKey : Mapping
    {
        public GroupingKey(LambdaExpression expression, LambdaExpression resultSelector)
            : base(expression, resultSelector)
        {
        }

        public string PropertyName { get; set; }

        public override Expression GetSourceMapping(ParameterExpression param)
        {
            var expr = Expression.Property(param, "Key");

            return Expression.Property(expr, PropertyName);
        }

        static GroupingKey _nogrouping;

        public static GroupingKey NoGroupingKey
        {
            get
            {
                if (_nogrouping == null)
                {
                    _nogrouping = new GroupingKey(Expression.Lambda(Expression.Constant(1)), null);
                }

                return _nogrouping;
            }
        }
    }

    internal class GroupingKey<T1> : IEquatable<GroupingKey<T1>>
    {
        public GroupingKey()
        {
            
        }

        public T1 Item1 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1>);
        }

        public bool Equals(GroupingKey<T1> other)
        {
            return other != null &&
                   EqualityComparer<T1>.Default.Equals(Item1, other.Item1);
        }

        public override int GetHashCode()
        {
            return 592959197 + EqualityComparer<T1>.Default.GetHashCode(Item1);
        }
    }

    internal class GroupingKey<T1, T2> : GroupingKey<T1>, IEquatable<GroupingKey<T1, T2>>
    {
        public GroupingKey()
        {
        }

        public T2 Item2 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2>);
        }

        public bool Equals(GroupingKey<T1, T2> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override int GetHashCode()
        {
            var hashCode = -1623862820;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3> : GroupingKey<T1, T2>, IEquatable<GroupingKey<T1, T2, T3>>
    {
        public T3 Item3 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3>);
        }

        public bool Equals(GroupingKey<T1, T2, T3> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T3>.Default.Equals(Item3, other.Item3);
        }

        public override int GetHashCode()
        {
            var hashCode = -1607085201;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4> : GroupingKey<T1, T2, T3>, IEquatable<GroupingKey<T1, T2, T3, T4>>
    {
        public T4 Item4 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T4>.Default.Equals(Item4, other.Item4);
        }

        public override int GetHashCode()
        {
            var hashCode = -1657418058;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(Item4);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4, T5> : GroupingKey<T1, T2, T3, T4>, IEquatable<GroupingKey<T1, T2, T3, T4, T5>>
    {
        public T5 Item5 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4, T5>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4, T5> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T5>.Default.Equals(Item5, other.Item5);
        }

        public override int GetHashCode()
        {
            var hashCode = -1640640439;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T5>.Default.GetHashCode(Item5);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4, T5, T6> : GroupingKey<T1, T2, T3, T4, T5>, IEquatable<GroupingKey<T1, T2, T3, T4, T5, T6>>
    {
        public T6 Item6 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4, T5, T6>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4, T5, T6> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T6>.Default.Equals(Item6, other.Item6);
        }

        public override int GetHashCode()
        {
            var hashCode = -1690973296;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T6>.Default.GetHashCode(Item6);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4, T5, T6, T7> : GroupingKey<T1, T2, T3, T4, T5, T6>, IEquatable<GroupingKey<T1, T2, T3, T4, T5, T6, T7>>
    {
        public T7 Item7 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4, T5, T6, T7>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4, T5, T6, T7> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T7>.Default.Equals(Item7, other.Item7);
        }

        public override int GetHashCode()
        {
            var hashCode = -1674195677;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T7>.Default.GetHashCode(Item7);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8> : GroupingKey<T1, T2, T3, T4, T5, T6, T7>, IEquatable<GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        public T8 Item8 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T8>.Default.Equals(Item8, other.Item8);
        }

        public override int GetHashCode()
        {
            var hashCode = -1456086630;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T8>.Default.GetHashCode(Item8);
            return hashCode;
        }
    }

    internal class GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8, T9> : GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8>, IEquatable<GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        public T9 Item9 { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
        }

        public bool Equals(GroupingKey<T1, T2, T3, T4, T5, T6, T7, T8, T9> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<T9>.Default.Equals(Item9, other.Item9);
        }

        public override int GetHashCode()
        {
            var hashCode = -1439309011;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T9>.Default.GetHashCode(Item9);
            return hashCode;
        }
    }
}
