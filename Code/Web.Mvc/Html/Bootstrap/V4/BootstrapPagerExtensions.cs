using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nabla.Web.Mvc.Html.Bootstrap.V4
{
    public static class BootstrapPagerExtensions
    {
        static PagerSettings _defaultSettings = new PagerSettings();

        public static PagerSettings DefaultSettings
        {
            get => _defaultSettings;
            set
            {
                _defaultSettings = value ?? throw new ArgumentNullException("value");
            }
        }

        private static StringBuilder PagerLink(this StringBuilder html, int index, bool active, bool disabled, string href, string label = null, string tip = null)
        {
            string tag;

            tag = !disabled && !active ? "a" : "span";

            html.Append("<li class=\"page-item");

            if (disabled || active)
            {
                html.Append(" ")
                    .Append(active ? "active" : "disabled");
            }

            html.Append("\"><").Append(tag).Append(" class=\"page-link\"");

            if (tag == "a")
            {
                html.Append(" href=\"")
                    .AppendFormat(href, index)
                    .Append("\"");
            }

            if (tip != null)
                html.Append(" aria-label=\"")
                    .Append("\" title=\"")
                    .Append(tip)
                    .Append("\"");

            html.Append("><span>")
                .Append(label ?? (index + 1).ToString())
                .Append("</span></")
                .Append(tag)
                .AppendLine("></li>");

            return html;
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int pageIndex, int pageCount)
        {
            return Pager(helper, pageIndex, pageCount, _defaultSettings);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int pageIndex, int pageCount, string href = null, BootstrapSize size = BootstrapSize.Auto)
        {
            var settings = _defaultSettings.Clone();
            settings.Size = size;
            settings.PageLinkHref = href ?? _defaultSettings.PageLinkHref;

            return Pager(helper, pageIndex, pageCount, settings);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int pageIndex, int pageCount, PagerSettings settings)
        {
            StringBuilder html = new StringBuilder();
            int w0, w1, max = settings?.MaxButtons ?? _defaultSettings.MaxButtons;
            string href = settings?.PageLinkHref ?? _defaultSettings.PageLinkHref;
            BootstrapSize size = settings?.Size ?? _defaultSettings.Size;

            if (pageCount > 0)
            {
                if (pageCount > max)
                {
                    w0 = Math.Max(pageIndex - max / 2, 0);
                    w1 = w0 + max - 1;
                }
                else
                {
                    w0 = 0;
                    w1 = pageCount - 1;
                }

                html.Append("<nav ><ul class=\"pagination");

                if (size != BootstrapSize.Auto)
                {
                    html.Append(" pagination-")
                        .Append(BootstrapHelper.GetSizeName(size));
                }

                html.Append("\">");

                html.PagerLink(0, false, pageIndex < 1, href, "&laquo;", settings?.PreviousPageText ?? _defaultSettings.PreviousPageText);

                if (w0 > 0 && pageIndex > 0)
                    html.PagerLink(0, false, false, href);

                for (int i = w0; i <= w1; i++)
                {
                    html.PagerLink(i, i == pageIndex, false, href);
                }

                if (w1 < pageCount - 1 && pageIndex < pageCount - 1)
                    html.PagerLink(pageCount - 1, false, false, href);

                html.PagerLink(pageIndex + 1, false, pageIndex > pageCount, href, "&raquo;", settings?.NextPageText ?? _defaultSettings.NextPageText)
                    .Append("</ul></nav>");
            }

            return new MvcHtmlString(html.ToString());

        }

    }

    
}
