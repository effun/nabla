using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    public class Sort
    {
        public Sort(string propertyName)
            : this(propertyName, SortType.Ascending, 0)
        {

        }

        public Sort(string propertyName, SortType sortType)
            : this(propertyName, sortType, 0)
        {

        }

        public Sort(string propertyName, SortType sortType, int sortOrder)
        {
            PropertyName = propertyName;
            SortType = sortType;
            SortOrder = sortOrder;
        }

        public string PropertyName { get; set; }

        public int SortOrder { get; set; }

        public SortType SortType { get; set; }
    }

    public enum SortType
    {
        None,
        Ascending,
        Descending
    }
}
