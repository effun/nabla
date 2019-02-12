using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    internal static class WhereBuilder
    {
        public static IQueryable<T> Build<T>(IQueryable<T> query, QueryArgs args)
        {
            WhereBuilderContext context = new WhereBuilderContext(query, args);

            Build(context);

            query = (IQueryable<T>)context.CurrentQuery;

            var current = context.Expression;

            if (current != null)
                return query.Where(Expression.Lambda<Func<T, bool>>(current, context.Parameter));
            else
                return query;
        }

        private static void Build(WhereBuilderContext context)
        {
            InitializeContext(context);

            foreach (var mp in context.CriteriaProperties)
            {
                object value = mp.GetValue(context.Arguments);

                if (!context.Arguments.ResolveProperty(value, mp, context) && value != null)
                {
                    ResolvePropertyDefault(value, mp, context);
                }
            }
        }

        private static void ResolvePropertyDefault(object value, ModelPropertyInfo mp, WhereBuilderContext context)
        {
            Criteria criteria = value as Criteria;

            if (criteria == null)
                criteria = Criteria.Create(value, mp);

            var members = MapProperty(mp, context);

            if (members != null)
            {
                Expression expr = null;

                foreach (var mb in members)
                {
                    var expr1 = criteria.CreateExpression(mb, context);

                    if (expr1 != null)
                    {
                        if (expr == null)
                            expr = expr1;
                        else
                            expr = Expression.OrElse(expr, expr1);
                    }
                }

                if (expr != null)
                    context.Append(expr);
            }
        }

        private static IEnumerable<MemberExpression> MapProperty(ModelPropertyInfo argumentProperty, WhereBuilderContext context)
        {
            var info = context.ElementModel;

            List<string> names = new List<string>(5);
            List<ModelPropertyInfo> chain = new List<ModelPropertyInfo>(5);

            names.AddRange(argumentProperty.Maps.Where(o => o.SourceType == info.ModelType).Select(o => o.PropertyName));
            if (names.Count == 0)
                names.Add(argumentProperty.Name);

            foreach (string name in names.Distinct())
            {
                chain.Clear();

                ModelConvert.FillPropertyChain(info, name, chain);

                Debug.Assert(chain.Count > 0);

                MemberExpression member = null;

                foreach (var mp in chain)
                {
                    member = Expression.MakeMemberAccess((Expression)member ?? context.Parameter, mp.PropertyInfo);
                }

                yield return member;
            }

        }


        internal static bool ResolvePropertyQueryable(object value, ModelPropertyInfo argumentProperty, WhereBuilderContext context)
        {
            Type argType = context.Arguments.GetType();
            Type[] parameterTypes = new Type[] { typeof(IQueryable<>).MakeGenericType(context.ElementType), argumentProperty.Type };

            var method = argType.GetMethod("Resolve" + argumentProperty.Name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);

            if (method == null && value != null && Nullable.GetUnderlyingType(argumentProperty.Type) == value.GetType())
            {
                parameterTypes[1] = value.GetType();
                method = argType.GetMethod("Resolve" + argumentProperty.Name,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
            }

            if (method != null && method.ReturnType == parameterTypes[0])
            {
                var query = method.Invoke(context.Arguments, new object[] { context.CurrentQuery, value });

                if (query != null)
                    context.CurrentQuery = (IQueryable)query;

                return true;
            }
            else
                return false;
        }

        internal static bool ResolvePropertyExpressionOrCriteria(object value, ModelPropertyInfo argumentProperty, WhereBuilderContext context)
        {
            var method = context.Arguments.GetType().GetMethod("Resolve" + argumentProperty.Name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { argumentProperty.Type }, null);

            if (method != null)
            {
                Type retType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(context.ElementType, typeof(bool)));

                if (method.ReturnType == retType)
                {
                    LambdaExpression lambda = (LambdaExpression)method.Invoke(context.Arguments, new object[] { value });

                    if (lambda != null)
                    {
                        //ExpressionReplacementUtil util = new ExpressionReplacementUtil(lambda.Parameters[0], context.Parameter);

                        //context.Append(util.Visit(lambda.Body));

                        context.Append(ReplacementVisitor.Replace(lambda.Body, lambda.Parameters[0], context.Parameter));
                    }

                    return true;
                }
                else if ((typeof(Criteria)).IsAssignableFrom(method.ReturnType))
                {
                    Criteria criteria = (Criteria)method.Invoke(context.Arguments, new object[] { value });

                    if (criteria != null)
                        ResolvePropertyDefault(criteria, argumentProperty, context);

                    return true;
                }
            }

            return false;
        }

        private static void InitializeContext(WhereBuilderContext context)
        {
            Type eltType = context.ElementType;
            ModelInfo info = ModelInfo.GetModelInfo(context.Arguments.GetType());

            foreach (var mp in info.Properties.Where(o => !o.IsDefined(typeof(NonCriteriaAttribute))))
            {
                context.CriteriaProperties.Add(mp);
            }

        }


    }

}
