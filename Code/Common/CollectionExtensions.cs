using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty(this ICollection collection, bool checkElements = false)
        {
            if (collection == null || collection.Count == 0)
                return true;

            if (checkElements)
            {
                foreach (var element in collection)
                {
                    if (element != null)
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
