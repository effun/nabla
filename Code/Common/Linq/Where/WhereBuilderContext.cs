using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    public class WhereBuilderContext
    {
        internal WhereBuilderContext(IQueryable source, QueryArgs arguments)
        {
            _currentQuery = _source = source;
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        private readonly IQueryable _source;
        private IQueryable _currentQuery;
        private QueryArgs _arguments;
        
        public QueryArgs Arguments
        {
            get => _arguments;
        }

        public ModelInfo ElementModel => ModelInfo.GetModelInfo(ElementType);

        public Type ElementType
        {
            get => _source.ElementType;
        }

        public IQueryable SourceQuery => _source;

        internal IQueryable CurrentQuery { get => _currentQuery; set => _currentQuery = value; }

        ParameterExpression _parameter;

        public ParameterExpression Parameter
        {
            get
            {
                if (_parameter == null)
                {
                    _parameter = Expression.Parameter(ElementType, "obj");
                }

                return _parameter;
            }
        }

        Expression _expression;
        List<ModelPropertyInfo> _criteriaProperties;

        internal Expression Expression => _expression;

        internal void Append(Expression expr)
        {
            if (_expression == null)
                _expression = expr;
            else
                _expression = Expression.AndAlso(_expression, expr);
        }

        internal ICollection<ModelPropertyInfo> CriteriaProperties
        {
            get
            {
                if (_criteriaProperties == null)
                    _criteriaProperties = new List<ModelPropertyInfo>(20);

                return _criteriaProperties;
            }
        }

    }


}
