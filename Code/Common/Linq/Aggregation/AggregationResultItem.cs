using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nabla.Linq
{
    [DebuggerDisplay("{Label ?? \"No Label\"}, {Level}, Depth={Depth}")]
    public class AggregationResultItem<T>
    {

        public AggregationResultItem(AggregationResult<T> result, T model, AggregationLevel level, Func<T, string> label, IEnumerable<AggregationResultItem<T>> subItems)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            Model = model;
            Level = level;
            Label = label?.Invoke(Model);

            if (subItems != null)
            {
                List<AggregationResultItem<T>> list = new List<AggregationResultItem<T>>();

                foreach (var subitem in subItems)
                {
                    if (subitem.Parent != null)
                        throw new InvalidOperationException("Sub item belongs to another item.");

                    subitem.Parent = this;
                    Depth = subitem.IncreaseDepth();
                    list.Add(subitem);
                }

                SubItems = list;
            }
            else
                SubItems = new List<AggregationResultItem<T>>(5);

        }

        public int Depth { get; private set; }

        public string Label { get; set; }

        public IList<AggregationResultItem<T>> SubItems { get; }

        public T Model { get; }

        public AggregationResult<T> Result { get; }

        public AggregationLevel Level { get; }

        public AggregationResultItem<T> Parent { get; private set; }

        private int IncreaseDepth()
        {
            int current = Depth++;

            foreach (var item in SubItems)
                item.IncreaseDepth();

            return current;
        }

    }
}
