using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    internal class CollectionWrapper : ICollectionWrapper
    {
        IList _list;
        ICollection _collection;

        public CollectionWrapper(ICollection instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            _list = instance as IList;
            _collection = instance;
            
        }

        public object RawCollection => _collection;

        public int Count => _collection.Count;

        public object SyncRoot => _collection.SyncRoot;

        public bool IsSynchronized => _collection.IsSynchronized;

        public bool IsReadOnly => _list?.IsReadOnly ?? ThrowNotSupported<bool>();

        public bool IsFixedSize => _list?.IsFixedSize ?? ThrowNotSupported<bool>();

        public object this[int index]
        {
            get
            {
                ThrowNotSupported();
                return _list[index];
            }
            set
            {
                ThrowNotSupported();
                _list[index] = value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            _collection.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        private void ThrowNotSupported()
        {
            if (_list == null)
                throw new InvalidOperationException("Current collection does not support manipulation.");
        }

        private T ThrowNotSupported<T>()
        {
            ThrowNotSupported();
            return default(T);
        }

        public int Add(object value)
        {
            ThrowNotSupported();

            return _list.Add(value);
        }

        public bool Contains(object value)
        {
            ThrowNotSupported();
            return _list.Contains(value);
        }

        public void Clear()
        {
            ThrowNotSupported();
            _list.Clear();
        }

        public int IndexOf(object value)
        {
            ThrowNotSupported();
            return _list.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ThrowNotSupported();
            _list.Insert(index, value);
        }

        public void Remove(object value)
        {
            ThrowNotSupported();
            _list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            ThrowNotSupported();
            _list.RemoveAt(index);
        }
    }

    internal class CollectionWrapper<T> : ICollectionWrapper
    {
        IList<T> _list;
        ICollection<T> _collection;
        ICollection _collection0;

        public CollectionWrapper(ICollection<T> instance)
        {
            _collection = instance ?? throw new ArgumentNullException(nameof(instance));
            _list = instance as IList<T>;
            _collection0 = instance as ICollection;

        }

        public object RawCollection => _collection;

        public int Count => _collection.Count;

        public object SyncRoot => _collection0?.SyncRoot;

        public bool IsSynchronized => _collection0?.IsSynchronized ?? false;

        public bool IsReadOnly => _collection.IsReadOnly;

        public bool IsFixedSize => (_collection as IList)?.IsFixedSize ?? false;

        public object this[int index]
        {
            get
            {
                ThrowNotSupported();
                return _list[index];
            }
            set
            {
                ThrowNotSupported();
                _list[index] = ValidateValueType(value);
            }
        }

        private T ValidateValueType(object value)
        {
            if (value == null || value is T)
                return (T)value;

            throw new InvalidOperationException("Value type not match, should be " + typeof(T));
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.GetType().GetElementType() != typeof(T))
                throw new ArgumentException("Invalid array element type.");

            _collection.CopyTo((T[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        private void ThrowNotSupported()
        {
            if (_list == null)
                throw new InvalidOperationException("Current collection does not support manipulation.");
        }

        private TR ThrowNotSupported<TR>()
        {
            ThrowNotSupported();
            return default(TR);
        }

        public int Add(object value)
        {
            _collection.Add(ValidateValueType(value));
            return _collection.Count;
        }

        public bool Contains(object value)
        {
            return _collection.Contains(ValidateValueType(value));
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public int IndexOf(object value)
        {
            ThrowNotSupported();
            return _list.IndexOf(ValidateValueType(value));
        }

        public void Insert(int index, object value)
        {
            ThrowNotSupported();
            _list.Insert(index, ValidateValueType(value));
        }

        public void Remove(object value)
        {
            _collection.Remove(ValidateValueType(value));
        }

        public void RemoveAt(int index)
        {
            ThrowNotSupported();
            _list.RemoveAt(index);
        }
    }

    public interface ICollectionWrapper : IList
    {
        object RawCollection { get; }
    }
}
