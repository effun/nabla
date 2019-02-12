using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla.Linq.Aggregations
{
    internal class Sum : EnumerableAggregation
    {

    }

    internal class Average : EnumerableAggregation
    {

    }

    internal class Count : EnumerableAggregation
    {
        internal override Expression CreateExpression(ParameterExpression parameter, LambdaExpression selector)
        {
            var method = TypicalLinqMethods.EnumerableCount;

            method = method.MakeGenericMethod(parameter.Type.GenericTypeArguments[1]);

            return Expression.Call(null, method, parameter);

        }
    }

    internal class Min : EnumerableAggregation
    {

    }

    internal class Max : EnumerableAggregation
    {

    }

    internal class EnumerableAggregation : AggregationFunction
    {
        protected virtual string MethodName => GetType().Name;

        protected virtual MethodInfo GetMethod(LambdaExpression selector)
        {
            return TypicalLinqMethods.EnumerableAggregation(MethodName, selector.ReturnType);
        }

        internal override Expression CreateExpression(ParameterExpression parameter, LambdaExpression selector)
        {
            var method = GetMethod(selector);

            if (method == null)
                throw new InvalidOperationException("No suitable aggregation method found for " + GetType().Name + " of type " + selector.ReturnType);

            method = method.MakeGenericMethod(parameter.Type.GenericTypeArguments[1]);

            return Expression.Call(null, method, parameter, selector);
        }
    }
}
