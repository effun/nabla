using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    static class ExpressionHelper
    {
        public static PropertyInfo ExtractProperty(LambdaExpression lambda, bool throwError = true)
        {
            MemberInfo member = ExtractMember(lambda, throwError);

            if (member != null)
            {
                PropertyInfo property = member as PropertyInfo;

                if (property == null && throwError)
                    throw new ArgumentException($"{member.DeclaringType}.{member.Name} is not a property.");

                return property;
            }
            else
                return null;
        }

        public static MemberInfo ExtractMember(LambdaExpression lambda, bool throwError = true)
        {
            MemberExpression member;
            Expression expr = lambda.Body;

            while (expr != null && !(expr is MemberExpression))
            {
                if (expr is UnaryExpression unary)
                    expr = unary.Operand;
                else
                    expr = null;
            }

            member = expr as MemberExpression;

            if (member == null && throwError)
                throw new ArgumentException(lambda.Body + " is not a member access expression.");

            return member?.Member;
        }

        public static ICollection<Expression> ExtractExpressions(LambdaExpression lambda)
        {
            var body = lambda.Body;

            if (body is NewExpression newexpr)
            {
                return newexpr.Arguments;
            }
            else if (body is NewArrayExpression listexpr)
            {
                return listexpr.Expressions;
                //return listexpr.Initializers.SelectMany(o => o.Arguments).ToArray();
            }
            else if (body is MemberInitExpression miexpr)
            {
                return miexpr.NewExpression.Arguments;
            }
            else
                return new Expression[] { body };
        }
    }
}
