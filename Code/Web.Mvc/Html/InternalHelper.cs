using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nabla.Web.Mvc.Html
{
    internal static class InternalHelper
    {
        internal static IDictionary<string, object> AddClassName(object htmlAttributes, params string[] names)
        {
            var values = ToValues(htmlAttributes);

            AddClassName(values, names);

            return values;
        }

        public static void AddClassName(IDictionary<string, object> values, params string[] names)
        {
            string name;

            if (values.TryGetValue("class", out object value))
                name = value?.ToString() ?? string.Empty;
            else
                name = string.Empty;

            if (name.Length > 0)
                name = name + " " + string.Join(" ", names);
            else
                name = string.Join(" ", names);

            values["class"] = name;
        }

        public static IDictionary<string, object> ToValues(object htmlAttributes)
        {
            var values = htmlAttributes as IDictionary<string, object>;

            if (values == null)
            {
                if (htmlAttributes != null)
                    values = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                else
                    values = new RouteValueDictionary();
            }

            return values;
        }
    }
}
