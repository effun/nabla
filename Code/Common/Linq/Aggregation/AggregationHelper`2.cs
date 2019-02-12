using Nabla.Linq.Aggregations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    public class AggregationHelper<TSource, TResult>
    {
        HashSet<Mapping> _mappings;
        IQueryable<TSource> _source;
        List<Tuple<Sort, LambdaExpression>> _sorts;

        public AggregationHelper()
        {
            _mappings = new HashSet<Mapping>();
        }

        public AggregationHelper(IQueryable<TSource> query)
            : this()
        {
            _source = query ?? throw new ArgumentNullException(nameof(query));
        }

        public AggregationHelper<TSource, TResult> GroupBy<TValue>(Expression<Func<TSource, TValue>> keySelector)
        {
            foreach (var key in AggregationHelper.ExtractGroupingKeys(keySelector, typeof(TResult)))
            {
                _mappings.Add(key);
            }

            return this;
        }

        public AggregationHelper<TSource, TResult> GroupBy<TValue>(Expression<Func<TSource, TValue>> keySelector, Expression<Func<TResult, TValue>> resultSelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            _mappings.Add(new GroupingKey(keySelector, resultSelector));
            return this;
        }

        public AggregationHelper<TSource, TResult> Sum<TValue>(Expression<Func<TSource, TValue>> sourceSelector)
        {
            return Sum(sourceSelector, null);
        }

        public AggregationHelper<TSource, TResult> Sum<TValue>(Expression<Func<TSource, TValue>> sourceSelector, Expression<Func<TResult, TValue>> resultSelector)
        {
            if (sourceSelector == null)
            {
                throw new ArgumentNullException(nameof(sourceSelector));
            }

            _mappings.Add(new FunctionMapping(sourceSelector, resultSelector, new Sum()));
            return this;
        }

        public AggregationHelper<TSource, TResult> Max<TValue>(Expression<Func<TSource, TValue>> sourceSelector)
        {
            return Max(sourceSelector, null);
        }

        public AggregationHelper<TSource, TResult> Max<TValue>(Expression<Func<TSource, TValue>> sourceSelector, Expression<Func<TResult, TValue>> resultSelector)
        {
            if (sourceSelector == null)
            {
                throw new ArgumentNullException(nameof(sourceSelector));
            }

            _mappings.Add(new FunctionMapping(sourceSelector, resultSelector, new Max()));
            return this;
        }

        public AggregationHelper<TSource, TResult> Min<TValue>(Expression<Func<TSource, TValue>> sourceSelector)
        {
            return Min(sourceSelector, null);
        }

        public AggregationHelper<TSource, TResult> Min<TValue>(Expression<Func<TSource, TValue>> sourceSelector, Expression<Func<TResult, TValue>> resultSelector)
        {
            if (sourceSelector == null)
            {
                throw new ArgumentNullException(nameof(sourceSelector));
            }

            _mappings.Add(new FunctionMapping(sourceSelector, resultSelector, new Min()));
            return this;
        }

        public AggregationHelper<TSource, TResult> Average<TValue>(Expression<Func<TSource, TValue>> sourceSelector)
        {
            return Average(sourceSelector, null);
        }

        public AggregationHelper<TSource, TResult> Average<TValue>(Expression<Func<TSource, TValue>> sourceSelector, Expression<Func<TResult, TValue>> resultSelector)
        {
            if (sourceSelector == null)
            {
                throw new ArgumentNullException(nameof(sourceSelector));
            }

            _mappings.Add(new FunctionMapping(sourceSelector, resultSelector, new Average()));
            return this;
        }

        public AggregationHelper<TSource, TResult> Count(Expression<Func<TResult, int>> resultSelector)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }

            _mappings.Add(new FunctionMapping(Mapping.NoSource, resultSelector, new Count()));
            return this;
        }

        public AggregationHelper<TSource, TResult> OrderBy<TValue>(Expression<Func<TResult, TValue>> keySelector)
        {
            AddSorts(keySelector, SortType.Ascending);
            return this;
        }

        public AggregationHelper<TSource, TResult> OrderByDescending<TValue>(Expression<Func<TResult, TValue>> keySelector)
        {
            AddSorts(keySelector, SortType.Descending);
            return this;
        }

        private void AddSorts(LambdaExpression selector, SortType type)
        {
            if (_sorts == null)
                _sorts = new List<Tuple<Sort, LambdaExpression>>(5);

            foreach (var expr in ExpressionHelper.ExtractExpressions(selector))
            {
                _sorts.Add(Tuple.Create(new Sort(null, type), Expression.Lambda(expr, selector.Parameters[0])));
            }
        }

        private IEnumerable<GroupingKey> GroupingKeys => _mappings.OfType<GroupingKey>();

        internal IEnumerable<FunctionMapping> Functions => _mappings.OfType<FunctionMapping>();



        //private IQueryable BuildGroupBy(IQueryable<TSource> query)
        //{
        //    MakeGroupByExpression(GroupingKeys.ToArray(), out Type keyType, out var expr);

        //    IQueryable result = (IQueryable)TypicalLinqMethods.QueryableGroupBy
        //        .MakeGenericMethod(typeof(TSource), keyType)
        //        .Invoke(null, new object[] { query, expr });

        //    return result;
        //}

        internal static LambdaExpression MakeGroupByExpression(GroupingKey[] keys)
        {
            var parameter = Expression.Parameter(typeof(TSource));
            int count = keys.Length;
            Type keyType;

            if (count == 0)
            {
                keys = new GroupingKey[] { GroupingKey.NoGroupingKey };
                count = 1;
            }

            keyType = Type.GetType(typeof(GroupingKey).FullName + "`" + count, false);
            if (keyType == null)
                throw new InvalidOperationException("Too many grouping keys, at most 9 keys are allowed.");

            keyType = keyType.MakeGenericType(keys.Select(o => o.SourceSelector.ReturnType).ToArray());

            MemberBinding[] bindings = new MemberBinding[count];

            for (int i = 0; i < count; i++)
            {
                var key = keys[i];

                key.PropertyName = "Item" + (i + 1);

                bindings[i] = Expression.Bind(keyType.GetProperty(key.PropertyName), AggregationHelper.ExtractExpression(key.SourceSelector, parameter));
            }

            return Expression.Lambda(Expression.MemberInit(Expression.New(keyType), bindings), parameter);
        }


        internal static LambdaExpression  MakeSelectExpression(LambdaExpression groupBy, IEnumerable<Mapping> mappings)
        {
            ParameterExpression param1 = Expression.Parameter(typeof(TResult));
            var param = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(groupBy.ReturnType, typeof(TSource)));
            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (var mapping in mappings)
            {
                MemberInfo member;

                if (mapping.ResultSelector == null)
                {
                    if (mapping.SourceSelector == Mapping.NoSource)
                        throw new InvalidOperationException("No source selector specified.");

                    if (typeof(TSource) != typeof(TResult))
                    {
                        member = ExpressionHelper.ExtractMember(mapping.SourceSelector);

                        member = typeof(TResult).GetPropertyOrField(member.Name, throwError: true);

                        mapping.ResultSelector = Expression.Lambda(Expression.MakeMemberAccess(param1, member), param1);
                    }
                    else
                    {
                        member = ExpressionHelper.ExtractMember(mapping.SourceSelector);

                        mapping.ResultSelector = mapping.SourceSelector;
                    }
                }
                else
                    member = ExpressionHelper.ExtractMember(mapping.ResultSelector);

                Expression source = mapping.GetSourceMapping(param);// ExtractExpression(mapping.SourceSelector, param);

                bindings.Add(Expression.Bind(member, source));
            }

            return Expression.Lambda(Expression.MemberInit(Expression.New(typeof(TResult)), bindings), param);
        }

        public IQueryable<TResult> GetQuery(IQueryable<TSource> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var groupBy = MakeGroupByExpression(GroupingKeys.ToArray());

            IQueryable query1 = (IQueryable)TypicalLinqMethods.QueryableGroupBy
                .MakeGenericMethod(typeof(TSource), groupBy.ReturnType)
                .Invoke(null, new object[] { query, groupBy });

            var selectExpr = MakeSelectExpression(groupBy, _mappings);

            MethodInfo selectMethod = TypicalLinqMethods.QueryableSelect.MakeGenericMethod(query1.ElementType, typeof(TResult));

            query1 = (IQueryable)selectMethod.Invoke(null, new object[] { query1, selectExpr });

            if (!_sorts.IsNullOrEmpty())
                query1 = OrderByBuilder.Build(query1, _sorts);

            return (IQueryable<TResult>)query1;
        }

        private IQueryable<TSource> SourceQuery => _source ?? throw new InvalidOperationException("The source query not taken, specify it in the constructor.");

        public IQueryable<TResult> GetQuery()
        {
            return GetQuery(SourceQuery);
        }

        public TResult[] ToArray(IQueryable<TSource> query)
        {
            return GetQuery(query).ToArray();
        }

        public TResult[] ToArray()
        {
            return ToArray(SourceQuery);
        }

        public AggregationResult<TResult> ToResult(IQueryable<TSource> query, Func<TResult, string> label)
        {
            return AggregationResult<TResult>.Create(query, this, label);
        }

        public AggregationResult<TResult> ToResult(Func<TResult, string> label)
        {
            return ToResult(SourceQuery, label);
        }

    }

    public class AggregationHelper<T> : AggregationHelper<T, T>
    {
        public AggregationHelper()
        {

        }

        public AggregationHelper(IQueryable<T> source)
            : base(source)
        {

        }
    }
}
