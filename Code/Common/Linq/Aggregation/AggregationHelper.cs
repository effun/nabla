using Nabla.Linq.Aggregations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Nabla.Linq
{
    internal class AggregationHelper
    {
        internal static GroupingKey[] ExtractGroupingKeys(LambdaExpression lambda, Type resultType)
        {
            MemberInfo member = ExpressionHelper.ExtractMember(lambda, false);

            if (member != null)
            {
                return new[] { new GroupingKey(lambda, null) };
            }

            if (lambda.Body is NewExpression init)
            {
                int count = init.Arguments.Count;

                List<GroupingKey> list = new List<GroupingKey>(count);


                for (int i = 0; i < count; i++)
                {
                    var arg = init.Arguments[i];
                    member = init.Members[i];

                    member = resultType.GetPropertyOrField(member.Name, throwError: true);

                    var p = Expression.Parameter(resultType);

                    list.Add(new GroupingKey(
                        Expression.Lambda(arg, lambda.Parameters[0]),
                        Expression.Lambda(Expression.MakeMemberAccess(p, member), p)));

                }

                return list.ToArray();
            }

            throw new InvalidOperationException("Not supported expression: " + lambda);

        }

        internal static Expression ExtractExpression(LambdaExpression lambda, ParameterExpression param)
        {
            if (lambda.Parameters.Count > 0)
                return ReplacementVisitor.Replace(lambda.Body, lambda.Parameters[0], param);
            else
                return lambda.Body;
        }

    }
}
