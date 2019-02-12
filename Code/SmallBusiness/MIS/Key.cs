using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Mis
{
    public interface IKey : IEquatable<IKey>
    {
        object[] Values { get; }
    }

    public struct Key<T1, T2> : IKey, IEquatable<Key<T1, T2>>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
    {
        private T1 _value1;
        private T2 _value2;

        public Key(T1 v1, T2 v2)
        {
            _value1 = v1;
            _value2 = v2;
        }

        public object[] Values => new object[] { _value1, _value2 };

        bool IEquatable<IKey>.Equals(IKey other)
        {
            return Equals(other);
        }

        public override bool Equals(object other)
        {
            if (other is Key<T1, T2> o)
            {
                return Equals(o);
            }
            else
                return false;
        }

        public bool Equals(Key<T1, T2> other)
        {
            return other._value1.Equals(_value1) && other._value2.Equals(_value2);
        }

        public override int GetHashCode()
        {
            var hashCode = 1738453169;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(_value1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(_value2);
            return hashCode;
        }

        public static bool operator ==(Key<T1, T2> key1, Key<T1, T2> key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(Key<T1, T2> key1, Key<T1, T2> key2)
        {
            return !(key1 == key2);
        }
    }

    public struct Key<T1, T2, T3> : IKey, IEquatable<Key<T1, T2, T3>>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
        where T3 : IEquatable<T3>
    {
        private T1 _value1;
        private T2 _value2;
        private T3 _value3;

        public Key(T1 v1, T2 v2, T3 v3)
        {
            _value1 = v1;
            _value2 = v2;
            _value3 = v3;
        }

        public object[] Values => new object[] { _value1, _value2, _value3 };

        public override bool Equals(object other)
        {
            if (other is Key<T1, T2, T3> o)
            {
                return Equals(o);
            }
            else
                return false;
        }

        bool IEquatable<IKey>.Equals(IKey other)
        {
            return Equals(other);
        }

        public bool Equals(Key<T1, T2, T3> other)
        {
            return other._value1.Equals(_value1) && other._value2.Equals(_value2) && other._value3.Equals(_value3);
        }

        public override int GetHashCode()
        {
            var hashCode = 1011907661;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(_value1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(_value2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(_value3);
            return hashCode;
        }

        public static bool operator ==(Key<T1, T2, T3> key1, Key<T1, T2, T3> key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(Key<T1, T2, T3> key1, Key<T1, T2, T3> key2)
        {
            return !(key1 == key2);
        }
    }

    public struct Key<T1, T2, T3, T4> : IKey, IEquatable<Key<T1, T2, T3, T4>>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
        where T3 : IEquatable<T3>
        where T4 : IEquatable<T4>
    {
        private T1 _value1;
        private T2 _value2;
        private T3 _value3;
        private T4 _value4;

        public Key(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            _value1 = v1;
            _value2 = v2;
            _value3 = v3;
            _value4 = v4;
        }

        public object[] Values => new object[] { _value1, _value2, _value3, _value4 };

        public override bool Equals(object other)
        {
            if (other is Key<T1, T2, T3, T4> o)
            {
                return Equals(o);
            }
            else
                return false;
        }

        bool IEquatable<IKey>.Equals(IKey other)
        {
            return Equals(other);
        }

        public bool Equals(Key<T1, T2, T3, T4> other)
        {
            return other._value1.Equals(_value1) && other._value2.Equals(_value2) && other._value3.Equals(_value3) && other._value4.Equals(_value4);
        }

        public override int GetHashCode()
        {
            var hashCode = 2066801334;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(_value1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(_value2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(_value3);
            hashCode = hashCode * -1521134295 + EqualityComparer<T4>.Default.GetHashCode(_value4);
            return hashCode;
        }

        public static bool operator ==(Key<T1, T2, T3, T4> key1, Key<T1, T2, T3, T4> key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(Key<T1, T2, T3, T4> key1, Key<T1, T2, T3, T4> key2)
        {
            return !(key1 == key2);
        }
    }

}
