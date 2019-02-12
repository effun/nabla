using System;
using System.Linq.Expressions;

namespace Nabla.Linq.Aggregations
{
    internal class FunctionMapping : Mapping
    {
        public FunctionMapping(LambdaExpression source, LambdaExpression result, AggregationFunction function)
            : base(source, result)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public AggregationFunction Function { get; }

        public override Expression GetSourceMapping(ParameterExpression param)
        {
            return Function.CreateExpression(param, SourceSelector);
        }
    }

}
