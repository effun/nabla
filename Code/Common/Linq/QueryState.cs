using System.Linq;

namespace Nabla.Linq
{
    class QueryState<T>
    {
        public IQueryable<T> Final { get; set; }

        public IQueryable<T> Plain { get; set; }

        public int Count { get; set; }
    }

}
