using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc
{
    public static class TagBuilderExtensions
    {
        public static TagBuilder AddInlineStyle(this TagBuilder tag, string name, string value)
        {
            tag.Attributes.TryGetValue("style", out string style);

            if (!string.IsNullOrEmpty(style))
                style += "; ";

            style += name + ": " + value;

            tag.Attributes["style"] = style;

            return tag;
        }
    }
}
