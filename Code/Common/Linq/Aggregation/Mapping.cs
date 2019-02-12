using System;
using System.Linq.Expressions;

namespace Nabla.Linq
{
    abstract class Mapping
    {
        public Mapping(LambdaExpression source, LambdaExpression result)
        {
            SourceSelector = source ?? throw new ArgumentNullException(nameof(source));
            ResultSelector = result;
        }

        public LambdaExpression SourceSelector { get; }

        public LambdaExpression ResultSelector { get; set; }

        public abstract Expression GetSourceMapping(ParameterExpression param);


        public static readonly LambdaExpression NoSource = Expression.Lambda(Expression.Empty());
    }

}
