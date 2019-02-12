using Nabla.Linq.Aggregations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla.Linq
{

    public class AggregationResult<T>
    {
        private AggregationResult()
        {
            _items = new List<AggregationResultItem<T>>();
            _generations = new List<AggregationResultItem<T>[]>(3);
        }

        List<AggregationResultItem<T>> _items;
        List<AggregationResultItem<T>[]> _generations;

        public ICollection<AggregationResultItem<T>> Items => _generations[_generations.Count - 1];

        public ICollection<AggregationResultItem<T>> GetGeneration(int number)
        {
            if (number < 0 || number >= _generations.Count)
                throw new ArgumentOutOfRangeException();

            return new ReadOnlyCollection<AggregationResultItem<T>>(_generations[number]);
        }

        public int GenerationCount => _generations.Count;

        public AggregationResult<T> Lift(string label)
        {
            Lift(null, AggregationLevel.Total, (o) => label);

            return this;
        }

        public AggregationResult<T> Lift<TKey>(Expression<Func<T, TKey>> keySelector, Func<T, string> label)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var keys = AggregationHelper.ExtractGroupingKeys(keySelector, typeof(T));

            Lift(keys, AggregationLevel.Subtotal, label);

            return this;
        }

        private void Lift(GroupingKey[] keys, AggregationLevel level, Func<T, string> label)
        {
            if (keys != null)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    LambdaExpression source = key.SourceSelector, result = key.ResultSelector;

                    if (source != null && LiftModelToViewItem(ref source) ||
                        result != null && LiftModelToViewItem(ref result))
                    {
                        keys[i] = new GroupingKey(source, result);
                    }
                }
            }
            else
                keys = new GroupingKey[] { GroupingKey.NoGroupingKey };

            LambdaExpression groupBy = AggregationHelper<AggregationResultItem<T>>.MakeGroupByExpression(keys);

            var temp = TypicalLinqMethods.EnumerableGroupBy
                .MakeGenericMethod(typeof(AggregationResultItem<T>), groupBy.ReturnType)
                .Invoke(null, new object[] { Items, groupBy.Compile() });

            var selectItem = MakeSelectExpression(groupBy, keys, level, label);

            MethodInfo method = TypicalLinqMethods.EnumerableSelect
                .MakeGenericMethod(selectItem.Parameters[0].Type, typeof(AggregationResultItem<T>));

            var coll = (IEnumerable<AggregationResultItem<T>>)method.Invoke(null, new object[] { temp, selectItem.Compile() });

            _items.Clear();
            _items.AddRange(coll);

            AppendGeneration();
        }

        private void AppendGeneration()
        {
            _generations.Add(_items.ToArray());
        }

        static readonly ConstructorInfo ViewItemConstructor = typeof(AggregationResultItem<T>).GetConstructors()[0];

        private LambdaExpression MakeSelectExpression(LambdaExpression groupBy, GroupingKey[] keys, AggregationLevel level, Func<T, string> label)
        {
            var groupingType = typeof(IGrouping<,>).MakeGenericType(groupBy.ReturnType, typeof(AggregationResultItem<T>));
            ParameterExpression param1 = Expression.Parameter(groupingType, "grouping");
            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (var mapping in keys.Cast<Mapping>().Union(_functions))
            {
                if (mapping != GroupingKey.NoGroupingKey)
                {
                    var member = ExpressionHelper.ExtractMember(mapping.ResultSelector ?? mapping.SourceSelector);
                    var expr = mapping.GetSourceMapping(param1);
                    var type = member.GetPropertyOrFieldType();

                    if (expr.Type != type)
                        expr = Expression.Convert(expr, type);

                    var binding = Expression.Bind(member, expr);

                    bindings.Add(binding);
                }
            }

            var initModel = Expression.MemberInit(Expression.New(typeof(T)), bindings);

            var body = Expression.New(ViewItemConstructor,
                Expression.Constant(this),
                initModel,
                Expression.Constant(level),
                Expression.Constant(label, typeof(Func<T, string>)),
                param1);

            return Expression.Lambda(body, param1);

        }

        private static FunctionMapping FromSourceMapping(FunctionMapping mapping)
        {
            var selector = mapping.ResultSelector;
            var func = mapping.Function;

            Debug.Assert(selector != null);

            LiftModelToViewItem(ref selector);

            if (func is Count)
            {
                func = new Sum();
            }

            return new FunctionMapping(selector, selector, func);
        }

        private static bool LiftModelToViewItem(ref LambdaExpression lambda)
        {
            if (lambda.Parameters[0].Type == typeof(T))
            {
                ParameterExpression paramItem = Expression.Parameter(typeof(AggregationResultItem<T>), "item"), paramOrigin = lambda.Parameters[0];
                MemberExpression model = Expression.Property(paramItem, "Model");

                var body = ReplacementVisitor.Replace(lambda.Body, (current) =>
                {
                    if (current is MemberExpression member)
                    {
                        if (member.Member.DeclaringType == typeof(T))
                        {
                            return Expression.MakeMemberAccess(model, member.Member);
                        }
                    }

                    return null;
                });

                lambda = Expression.Lambda(body, paramItem);
                return true;
            }
            else
                return false;
        }

        public ICollection<AggregationResultItem<T>> Flatten(FlattenMode mode = FlattenMode.SubLevelFirst)
        {
            List<AggregationResultItem<T>> list = new List<AggregationResultItem<T>>();

            if (mode == FlattenMode.SubLevelFirst || mode == FlattenMode.TopLevelFirst)
            {
                Flatten(Items, list, mode);
            }
            else
            {
                IEnumerable<AggregationResultItem<T>[]> generations = _generations;

                if (mode == FlattenMode.LaterGenerationFirst)
                    generations = generations.Reverse();

                foreach (var gen in generations)
                    list.AddRange(gen);
            }

            return list.AsReadOnly();
        }

        private static void Flatten(IEnumerable<AggregationResultItem<T>> items, List<AggregationResultItem<T>> list, FlattenMode mode)
        {
            foreach (var item in items)
            {
                if (mode == FlattenMode.TopLevelFirst)
                    list.Add(item);

                Flatten(item.SubItems, list, mode);

                if (mode == FlattenMode.SubLevelFirst)
                    list.Add(item);
            }
        }

        private FunctionMapping[] _functions;

        private static AggregationResult<T> Create(IEnumerable<T> models, Func<T, string> label, IEnumerable<FunctionMapping> functions)
        {
            AggregationResult<T> view = new AggregationResult<T>
            {
                _functions = functions.Select(o => FromSourceMapping(o)).ToArray()
            };

            foreach (var model in models)
            {
                var item = new AggregationResultItem<T>(view, model, AggregationLevel.Details, label, null);

                view._items.Add(item);
            }

            view.AppendGeneration();

            return view;
        }

        public static AggregationResult<T> Create<TSource>(IQueryable<TSource> query, AggregationHelper<TSource, T> helper, Func<T, string> label)
        {
            return Create(helper.GetQuery(query).AsEnumerable(), helper, label);

        }

        public static AggregationResult<T> Create<TSource>(IEnumerable<T> query, AggregationHelper<TSource, T> helper, Func<T, string> label)
        {
            return Create(query, label, helper.Functions);
        }
    }
}
