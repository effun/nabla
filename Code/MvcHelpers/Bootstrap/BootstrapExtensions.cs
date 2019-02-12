using System;

namespace System.Web.Mvc
{
    public static class BootstrapExtensions
    {
        public static MvcHtmlString ActionButton(this HtmlHelper helper, string text, string icon = null, BootstrapColorTheme theme = BootstrapColorTheme.Default)
        {
            TagBuilder tb = new TagBuilder("a");
            tb.AddClassName("btn");
            tb.AddClassName(theme.ToClassName("btn"));

            
        }

        private static string ToClassName(this BootstrapColorTheme theme, BootstrapComponent component)
        {
            
        }
    }
}