using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq.Entity
{
    public static class AsyncAggregationHelperExtensions
    {
        public static Task<TResult[]> ToArrayAsync<TSource, TResult>(this AggregationHelper<TSource, TResult> helper)
        {
            return helper.GetQuery().ToArrayAsync();
        }

        public static Task<TResult[]> ToArrayAsync<TSource, TResult>(this AggregationHelper<TSource, TResult> helper, IQueryable<TSource> query)
        {
            return helper.GetQuery(query).ToArrayAsync();
        }

        public static async Task<AggregationResult<TResult>> ToResultAsync<TSource, TResult>(this AggregationHelper<TSource, TResult> helper, IQueryable<TSource> query, Func<TResult, string> label)
        {
            var array = await helper.GetQuery(query).ToArrayAsync();

            return AggregationResult<TResult>.Create(array, helper, label);
        }

        public static async Task<AggregationResult<TResult>> ToResultAsync<TSource, TResult>(this AggregationHelper<TSource, TResult> helper, Func<TResult, string> label)
        {
            var array = await helper.GetQuery().ToArrayAsync();

            return AggregationResult<TResult>.Create(array, helper, label);
        }

        public static async Task<AggregationResult<T>> ToResultAsync<T>(this AggregationHelper<T> helper, IQueryable<T> query, Func<T, string> label)
        {
            var array = await helper.GetQuery(query).ToArrayAsync();

            return AggregationResult<T>.Create(array, helper, label);
        }

        public static async Task<AggregationResult<T>> ToResultAsync<T>(this AggregationHelper<T> helper, Func<T, string> label)
        {
            var array = await helper.GetQuery().ToArrayAsync();

            return AggregationResult<T>.Create(array, helper, label);
        }
    }
}
