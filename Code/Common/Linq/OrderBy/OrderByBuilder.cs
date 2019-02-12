using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    internal static class OrderByBuilder
    {
        public static IQueryable<T> Build<T>(IQueryable<T> query, QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var sort = args.Sort;

            if (sort == null || sort.Length == 0)
                return (IQueryable<T>)args.DefaultSort(query, true);
            else
                return (IQueryable<T>)Build(query, sort);
        }

        internal static IQueryable BuildByKeys(IQueryable query)
        {
            ModelInfo info = ModelInfo.GetModelInfo(query.ElementType);

            var keys = info.Keys;

            if (keys.Count == 0)
                throw new InvalidOperationException("There is no key property defined in " + info.ModelType);

            return Build(query, keys.Select(o => new Sort(o.Name)).ToArray());
        }

        internal static bool TryBuildDefault(ref IQueryable query, QueryArgs args)
        {
            Type queryType = typeof(IQueryable<>).MakeGenericType(query.ElementType);
            MethodInfo method = args.GetType().GetMethod("DefaultSort",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                new Type[] { queryType }, null);

            if (method != null && queryType.IsAssignableFrom(method.ReturnType))
            {
                object value = method.Invoke(args, new object[] { query });

                if (value != null)
                    query = (IQueryable)value;

                return true;
            }
            else
                return false;
        }

        private static IQueryable Build(IQueryable query, Sort[] sorts)
        {
            ModelInfo model = ModelInfo.GetModelInfo(query.ElementType);

            query = Build(query, sorts.Where(o => o.SortType != SortType.None).OrderBy(o => o.SortOrder).Select(o => ParseSort(o, model)));

            return query;

        }

        internal static IQueryable Build(IQueryable query, IEnumerable<Tuple<Sort, LambdaExpression>> sorts)
        {
            bool then = false;

            foreach (var state in sorts)
            {
                var lambda = state.Item2;
                string name = "Queryable";

                if (!then)
                {
                    name += "OrderBy";
                    then = true;
                }
                else
                    name += "ThenBy";

                if (state.Item1.SortType == SortType.Descending)
                    name += "Descending";

                query = (IQueryable)TypicalLinqMethods.GetMethod(name)
                    .MakeGenericMethod(query.ElementType, lambda.ReturnType)
                    .Invoke(null, new object[] { query, lambda });
            }

            return query;
        }

        private static Tuple<Sort, LambdaExpression> ParseSort(Sort sort, ModelInfo model)
        {
            List<ModelPropertyInfo> chain = new List<ModelPropertyInfo>(3);

            ModelConvert.FillPropertyChain(model, sort.PropertyName, chain);
            ParameterExpression parameter = Expression.Parameter(model.ModelType, "obj");
            MemberExpression member = null;

            foreach (var mp in chain)
            {
                member = Expression.MakeMemberAccess((Expression)member ?? parameter, mp.PropertyInfo);
            }

            return Tuple.Create(sort, Expression.Lambda(typeof(Func<,>).MakeGenericType(model.ModelType, member.Type), member, parameter));
        }
    }

}
