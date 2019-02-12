using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nabla.Linq
{
    public class QueryArgs
    {
        [NonCriteria]
        public int PageIndex { get; set; }

        [NonCriteria]
        public int PageSize { get; set; }

        [NonCriteria]
        public Sort[] Sort { get; set; }

        public virtual IQueryable<T> Where<T>(IQueryable<T> query)
        {
            return WhereBuilder.Build(query, this);
        }

        public virtual IQueryable<T> OrderBy<T>(IQueryable<T> query)
        {
            return OrderByBuilder.Build(query, this);
        }

        internal protected virtual bool ResolveProperty(object value, ModelPropertyInfo argumentProperty, WhereBuilderContext context)
        {
            return WhereBuilder.ResolvePropertyQueryable(value, argumentProperty, context)
                || WhereBuilder.ResolvePropertyExpressionOrCriteria(value, argumentProperty, context);
        }

        internal protected virtual IQueryable DefaultSort(IQueryable query, bool enforceOrderable)
        {
            var q0 = query;

            if (_defaultSorts != null && _defaultSorts.TryGetValue(query.ElementType, out Delegate sorter))
            {
                query = (IQueryable)sorter.DynamicInvoke(query, this);
            }
            else
                OrderByBuilder.TryBuildDefault(ref query, this);

            if (enforceOrderable && q0 == query || !(query is IOrderedQueryable))
                query = OrderByBuilder.BuildByKeys(query);

            return query;
        }

        ConcurrentDictionary<Type, Delegate> _defaultSorts;

        public void SetDefaultSort<T>(Func<IQueryable<T>, QueryArgs, IQueryable<T>> sorter)
        {
            if (_defaultSorts == null)
            {
                _defaultSorts = new ConcurrentDictionary<Type, Delegate>();
            }

            _defaultSorts[typeof(T)] = sorter;
        }
    }
}
