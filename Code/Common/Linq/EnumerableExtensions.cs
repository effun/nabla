using Nabla.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> coll, Action<T> action)
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T item in coll)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> coll, Action<T, int> action)
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int index = 0;

            foreach (T item in coll)
            {
                action(item, index++);
            }
        }

    }
}

namespace Nabla.Linq
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TTarget> Convert<TTarget>(this IEnumerable coll)
        {
            ModelConvertOptions options = new ModelConvertOptions();

            return Convert<TTarget>(coll, options);
        }

        public static IEnumerable<TTarget> Convert<TTarget>(this IEnumerable coll, ModelConvertOptions options)
        {
            Type sourceType = coll.GetType();

            if (sourceType.IsGenericType)
            {
                sourceType = sourceType.GetGenericArguments()[0];

                if (options == null)
                    options = new ModelConvertOptions();

                options.Source(sourceType);
            }

            foreach (object obj in coll)
                yield return ModelConvert.Convert<TTarget>(obj, options);
        }


    }
}
