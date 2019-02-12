using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Nabla.Linq
{
    internal class ReplacementVisitor : ExpressionVisitor
    {
        Func<Expression, Expression> _evaluator;

        public ReplacementVisitor(Expression expression, Expression replaceWith)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (replaceWith == null)
            {
                throw new ArgumentNullException(nameof(replaceWith));
            }

            _evaluator = (current) =>
            {
                if (current == expression)
                {
                    return replaceWith;
                }

                return null;
            };

        }

        public ReplacementVisitor(Func<Expression, Expression> evaluator)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public override Expression Visit(Expression node)
        {
            var expr = _evaluator(node);

            if (expr == null || expr == node)
                return base.Visit(node);

            return expr;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var test = _evaluator(node.Expression);

            if (test != null && test != node.Expression)
            {
                return Expression.MakeMemberAccess(test, test.Type.GetPropertyOrField(node.Member.Name, throwError: true));
            }

            return base.VisitMember(node);
        }

        public static Expression Replace(Expression expression, Expression find, Expression replaceWith)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ReplacementVisitor util = new ReplacementVisitor(find, replaceWith);

            return util.Visit(expression);
        }

        public static Expression Replace(Expression expression, Func<Expression, Expression> evaluator)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ReplacementVisitor visitor = new ReplacementVisitor(evaluator);

            return visitor.Visit(expression);
        }
    }

}
