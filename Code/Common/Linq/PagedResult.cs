using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Nabla.Linq
{
    public class PagedResult
    {
        QueryArgs _arguments;

        public PagedResult(Array items, QueryArgs args)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            _arguments = args ?? throw new ArgumentNullException(nameof(args));

            PageIndex = args.PageIndex;
            PageSize = args.PageSize;
            Total = items.Length;
        }

        public PagedResult(Array items, QueryArgs args, int total)
            : this(items, args)
        {
            Total = total;
        }

        public int PageCount
        {
            get
            {
                int ps = PageSize, tt = Total;

                if (ps > 0)
                {
                    return (int)Math.Ceiling(tt / (double)ps);
                }
                else
                    return Math.Sign(tt);
            }
        }

        public int PageSize { get;  }

        public int PageIndex { get; }

        public int Total { get;  }

        public QueryArgs Arguments => _arguments;

        public Array Items { get; }
    }

    public class PagedResult<T> : PagedResult
    {
        public PagedResult(T[] items, QueryArgs args)
            : base(items, args)
        {
        }

        public PagedResult(T[] items, QueryArgs args, int total)
            : base(items, args, total)
        {
        }

        public new T[] Items => (T[])base.Items;

        protected IQueryable<T> PlainQuery { get; private set; }

        public AggregationHelper<T> Aggregate()
        {
            return new AggregationHelper<T>(PlainQuery);
        }

        public AggregationHelper<T, TAggregation> Aggregate<TAggregation>()
        {
            return new AggregationHelper<T, TAggregation>(PlainQuery);
        }

        public static PagedResult<T> Create(T[] items, QueryArgs args, int total, IQueryable<T> plain)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            PagedResult<T> result;

            if (args.PageSize > 0)
                result = new PagedResult<T>(items, args, total);
            else
                result = new PagedResult<T>(items, args);

            result.PlainQuery = plain;

            return result;

        }
    }

    public class PagedResult<TSource, TResult> : PagedResult
    {
        public PagedResult(TResult[] items, QueryArgs args)
            : base(items, args)
        {
        }

        public PagedResult(TResult[] items, QueryArgs args, int total)
            : base(items, args, total)
        {
        }

        public new TResult[] Items => (TResult[])base.Items;

        protected IQueryable<TSource> PlainQuery { get ; private set ; }

        public AggregationHelper<TSource> AggregateSource()
        {
            return new AggregationHelper<TSource>(PlainQuery);
        }

        public AggregationHelper<TSource, TResult> Aggregate()
        {
            return new AggregationHelper<TSource, TResult>(PlainQuery);
        }

        public AggregationHelper<TSource, TAggregation> Aggregate<TAggregation>()
        {
            return new AggregationHelper<TSource, TAggregation>(PlainQuery);
        }

        public static PagedResult<TSource, TResult> Create(TResult[] items, QueryArgs args, int total, IQueryable<TSource> plain)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            PagedResult<TSource, TResult> result;

            if (args.PageSize > 0)
                result = new PagedResult<TSource, TResult>(items, args, total);
            else
                result = new PagedResult<TSource, TResult>(items, args);

            result.PlainQuery = plain;

            return result;

        }

    }
}