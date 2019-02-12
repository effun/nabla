using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nabla.Web.Mvc.Html.Bootstrap.V4
{
    public static class BootstrapProgressBarExtensions
    {
        private const string DefaultLabelFormatString = "{0:P0}";

        public static MvcHtmlString ProgressBar(this HtmlHelper helper, double value, string labelFormatString = null, BootstrapColor color = BootstrapColor.None)
        {
            return helper.ProgressBar(value, new ProgressBarOptions { ShowLabel = !string.IsNullOrEmpty(labelFormatString), LabelFormatString = labelFormatString, Color = color });
        }

        public static MvcHtmlString ProgressBar(this HtmlHelper helper, double value, ProgressBarOptions options)
        {
            TagBuilder progress = new TagBuilder("div");
            TagBuilder bar = new TagBuilder("div");

            if (options == null)
                options = new ProgressBarOptions();

            bar.AddCssClass("progress-bar");
            if (options.Color != BootstrapColor.None)
                bar.AddCssClass("bg-" + BootstrapHelper.GetColorName(options.Color));

            if (options.Striped || options.Animated)
                bar.AddCssClass("progress-bar-striped");
            if (options.Animated)
                bar.AddCssClass("progress-bar-animated");

            bar.Attributes.Add("role", "progressbar");
            bar.Attributes.Add("aria-valuemin", "0");
            bar.Attributes.Add("aria-valuemax", "100");
            bar.Attributes.Add("aria-valuenow", value.ToString("p1"));

            bar.AddInlineStyle("width", value.ToString("p1"));

            if (options.ShowLabel)
            {
                string label = string.Format(options.LabelFormatString ?? DefaultLabelFormatString, value);

                bar.SetInnerText(label);
                bar.AddInlineStyle("min-width", (label.Length + 1) + "em");
            }

            progress.AddCssClass("progress");

            progress.InnerHtml = bar.ToString();

            return new MvcHtmlString(progress.ToString());

        }

        public static MvcHtmlString ProgressBar(this HtmlHelper helper, int value, string labelFormatString = null, BootstrapColor color = BootstrapColor.None)
        {
            return helper.ProgressBar(value / 100.0, labelFormatString, color);
        }

        public static MvcHtmlString ProgressBar(this HtmlHelper helper, int value, bool showLabel, BootstrapColor color = BootstrapColor.None)
        {
            return helper.ProgressBar(value, showLabel ? DefaultLabelFormatString : null, color);
        }

        public static MvcHtmlString ProgressBar(this HtmlHelper helper, double value, bool showLabel, BootstrapColor color = BootstrapColor.None)
        {
            return helper.ProgressBar(value, showLabel ? DefaultLabelFormatString : null, color);
        }

        public static MvcHtmlString ProgressBarFor<T>(this HtmlHelper<T> helper, Expression<Func<T, int>> expression, string labelFormatString = null, BootstrapColor color = BootstrapColor.None)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            return helper.ProgressBar((int)metadata.Model, labelFormatString, color);
        }

        public static MvcHtmlString ProgressBarFor<T>(this HtmlHelper<T> helper, Expression<Func<T, double>> expression, string labelFormatString = null, BootstrapColor color = BootstrapColor.None)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            return helper.ProgressBar((double)metadata.Model, labelFormatString, color);
        }

        public static MvcHtmlString ProgressBarFor<T>(this HtmlHelper<T> helper, Expression<Func<T, int>> expression, bool showLabel, BootstrapColor color = BootstrapColor.None)
        {
            return helper.ProgressBarFor(expression, showLabel ? DefaultLabelFormatString : null, color);
        }

        public static MvcHtmlString ProgressBarFor<T>(this HtmlHelper<T> helper, Expression<Func<T, double>> expression, bool showLabel, BootstrapColor color = BootstrapColor.None)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            return helper.ProgressBarFor(expression, showLabel ? DefaultLabelFormatString : null, color);
        }

    }
}
