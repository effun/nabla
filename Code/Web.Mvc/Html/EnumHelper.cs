using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using mvc = System.Web.Mvc.Html;

namespace Nabla.Web.Mvc.Html
{
    public static class EnumHelper
    {
        public static IEnumerable<SelectListItem> GetSelectList<T>(IEnumerable<T> values)
            where T: struct
        {
            if (!mvc.EnumHelper.IsValidForEnumHelper(typeof(T)))
                throw new ArgumentException("Invalid enum type " + typeof(T));

            return values.Cast<Enum>()
                .Select(o => new SelectListItem {
                    Text = o.DisplayName(),
                    Value = Convert.ChangeType(o, Enum.GetUnderlyingType(typeof(T))).ToString()
                });
        }
    }
}
