using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    public static class QueryableExtensions
    {
        public static bool TraceSQL { get; set; }

        public static IQueryable<T> Page<T>(this IQueryable<T> query, QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.PageSize > 0)
            {
                if (args.PageIndex > 0)
                    query = query.Skip(args.PageIndex * args.PageSize);

                query = query.Take(args.PageSize);
            }

            return query;
        }

        public static PagedResult<T> PagedResult<T>(this IQueryable<T> query, QueryArgs args)
        {
            IQueryable<T> query1;

            var state = WhereAndOrderBy(query, args);

            query1 = state.Final;

            WriteSQL(query1);

            return Linq.PagedResult<T>.Create(query1.ToArray(), args, state.Count, state.Plain);
        }

        private static QueryState<T> WhereAndOrderBy<T>(IQueryable<T> query, QueryArgs args)
        {
            QueryState<T> state = new QueryState<T>();

            state.Plain = query = query.Where(args);
            state.Count = query.Count();
            state.Final = query.OrderBy(args).Page(args);

            return state;
        }

        public static PagedResult<TSource, TResult> PagedResult<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args)
        {
            return PagedResult<TSource, TResult>(query, args, null);
        }

        public static PagedResult<TSource, TResult> PagedResult<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args, ModelConvertOptions options)
        {
            IQueryable<TResult> query1;

            var state = WhereAndOrderBy(query, args);
            query1 = Convert<TSource, TResult>(state.Final, options);

            WriteSQL(query1);

            return Linq.PagedResult<TSource, TResult>.Create(query1.ToArray(), args, state.Count, state.Plain);
        }

        public static PagedResult<TResult> PagedResultByResult<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args)
        {
            return PagedResultByResult<TSource, TResult>(query, args, null);
        }

        public static PagedResult<TResult> PagedResultByResult<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args, ModelConvertOptions options)
        {
            IQueryable<TResult> query1;

            query1 = Convert<TSource, TResult>(query, options);
            var state = WhereAndOrderBy(query1, args);

            query1 = state.Final;

            WriteSQL(query1);

            return Linq.PagedResult<TResult>.Create(query1.ToArray(), args, state.Count, state.Plain);

        }

        private static void WriteSQL(IQueryable query)
        {
            if (TraceSQL)
            {
                Trace.WriteLine(query.ToString());
            }
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            return args.Where(query);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            return args.OrderBy(query);
        }

        public static IQueryable<TResult> Convert<TSource, TResult>(this IQueryable<TSource> source)
        {
            return Convert<TSource, TResult>(source, null);
        }

        public static IQueryable<TResult> Convert<TSource, TResult>(this IQueryable<TSource> source, ModelConvertOptions options)
        {
            return SelectBuilder.Build<TSource, TResult>(source, options);
        }

        public static IQueryable<T> Apply<T>(this IQueryable<T> query, QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            return query.Where(args).OrderBy(args).Page(args);
        }

    }

}
