using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq.Entity
{
    public static class AsyncQueryableExtensions
    {

        private static async Task<QueryState<T>> WhereAndOrderBy<T>(IQueryable<T> query, QueryArgs args)
        {
            IQueryable<T> plain;

            plain = query = query.Where(args);

            int count = await query.CountAsync();

            query = query.OrderBy(args).Page(args);

            return new QueryState<T> { Count = count, Final = query, Plain = plain };
        }

        public static async Task<PagedResult<T>> PagedResultAsync<T>(this IQueryable<T> query, QueryArgs args)
        {
            var state = await WhereAndOrderBy(query, args);

            WriteSQL(state.Final);

            return PagedResult<T>.Create(await state.Final.ToArrayAsync(), args, state.Count, state.Plain);
        }

        public static Task<PagedResult<TSource, TResult>> PagedResultAsync<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args)
        {
            return PagedResultAsync<TSource, TResult>(query, args, null);
        }

        public static async Task<PagedResult<TSource, TResult>> PagedResultAsync<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args, ModelConvertOptions options)
        {
            IQueryable<TResult> query1;
            var state = await WhereAndOrderBy(query, args);
            query1 = state.Final.Convert<TSource, TResult>(options);

            WriteSQL(query1);

            return PagedResult<TSource, TResult>.Create(await query1.ToArrayAsync(), args, state.Count, state.Plain);

        }

        public static Task<PagedResult<TResult>> PagedResultByResultAsync<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args)
        {
            return PagedResultByResultAsync<TSource, TResult>(query, args, null);
        }

        public static async Task<PagedResult<TResult>> PagedResultByResultAsync<TSource, TResult>(this IQueryable<TSource> query, QueryArgs args, ModelConvertOptions options)
        {
            IQueryable<TResult> query1;

            query1 = query.Convert<TSource, TResult>(options);
            var state = await WhereAndOrderBy(query1, args);
            query1 = state.Final;

            WriteSQL(query1);

            return PagedResult<TResult>.Create(await query1.ToArrayAsync(), args, state.Count, state.Plain);
        }

        public static async Task<T[]> ToArrayAsync<T>(this IQueryable<T> query, QueryArgs args)
        {
            var result = await query.PagedResultAsync(args);

            return result.Items;
        }

        private static void WriteSQL(IQueryable query)
        {
            if (QueryableExtensions.TraceSQL)
            {
                Trace.WriteLine(query.ToString());
            }
        }

    }
}
