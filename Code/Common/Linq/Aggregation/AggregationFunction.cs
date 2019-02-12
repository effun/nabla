using System.Linq.Expressions;

namespace Nabla.Linq.Aggregations
{
    internal abstract class AggregationFunction
    {
        internal abstract Expression CreateExpression(ParameterExpression parameter, LambdaExpression selector);
    }

    
}
