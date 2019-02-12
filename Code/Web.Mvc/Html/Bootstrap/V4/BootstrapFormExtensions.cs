using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Nabla.Web.Mvc.Html.Bootstrap.V4
{
    public static class BootstrapFormExtensions
    {
        private const string FormControl = "form-control";

        public static MvcHtmlString HLabelFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, BootstrapSize breakpoint = BootstrapSize.Mediume, int columns = 3, string labelText = null, object htmlAttributes = null)
        {
            var values = InternalHelper.ToValues(htmlAttributes);

            if (columns >= 0)
            {
                InternalHelper.AddClassName(values, BootstrapHelper.GetClassName("col", breakpoint, columns));
            }

            return helper.LabelFor(expression, labelText, values);
        }

        private static IDictionary<string, object> InitializeInputControl(this IDictionary<string, object> htmlAttributes, BootstrapSize size, bool @readonly, bool disabled)
        {
            if (size != BootstrapSize.Auto)
                InternalHelper.AddClassName(htmlAttributes, "form-control-" + BootstrapHelper.GetSizeName(size));

            if (@readonly)
                htmlAttributes.Add("readonly", "readonly");

            if (disabled)
                htmlAttributes.Add("disabled", "disabled");

            return htmlAttributes;
        }

        public static MvcHtmlString FormTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string format = null, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            IDictionary<string, object> attrs = InternalHelper.AddClassName(htmlAttributes, FormControl);

            return helper.TextBoxFor(expression, format, attrs.InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormPasswordFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, string>> expression, BootstrapSize size = BootstrapSize.Auto, object htmlAttributes = null)
        {
            var attrs = InternalHelper.AddClassName(htmlAttributes, FormControl);

            return helper.PasswordFor(expression, attrs.InitializeInputControl(size, false, false));
        }

        public static MvcHtmlString FormNumberBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool hideZero = true, int decimals = 0, bool @readonly = false, bool disabled = false, BootstrapSize size = BootstrapSize.Auto, double? min = null, double? max = null, object htmlAttributes = null)
        {
            var values = InternalHelper.AddClassName(htmlAttributes, FormControl);
            string format = null;

            if (hideZero)
            {
                format = "{0:#";

                if (decimals > 0)
                    format += "." + new string('#', decimals);

                format += "}";
            }

            values["type"] = "number";
            if (min != null) values["min"] = min;
            if (max != null) values["max"] = max;

            return helper.TextBoxFor(expression, format, values.InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormDateBoxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, DateTime>> expr, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            var values = InternalHelper.AddClassName(htmlAttributes, FormControl);
            values["type"] = "date";

            return helper.TextBoxFor(expr, Constants.DateBoxValueFormatString, values.InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormDateBoxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, DateTime?>> expr, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            var values = InternalHelper.AddClassName(htmlAttributes, FormControl);
            values["type"] = "date";

            return helper.TextBoxFor(expr, Constants.DateBoxValueFormatString, values.InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormTextAreaFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, int rows = 0, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            var values = InternalHelper.AddClassName(htmlAttributes, FormControl);

            if (rows > 0) values["rows"] = rows;

            return helper.TextAreaFor(expression, values.InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormCheckBoxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression,
            string labelText = null, bool radio = false, bool inline = false, bool disabled = false, object htmlAttributes = null)
        {
            TagBuilder div = new TagBuilder("div");
            IDictionary<string, object> attrsInput = new RouteValueDictionary(), attrsLabel = InternalHelper.AddClassName(htmlAttributes, "form-check-label");

            div.AddCssClass("form-check");
            if (disabled)
                div.AddCssClass("disabled");
            if (inline)
                div.AddCssClass("form-check-inline");

            InternalHelper.AddClassName(attrsInput, "form-check-input");

            if (disabled)
            {
                attrsInput.Add("disabled", "disabled");
            }

            InternalHelper.AddClassName(attrsLabel, "form-check-label");

            div.InnerHtml = helper.CheckBoxFor(expression, attrsInput).ToHtmlString() + helper.LabelFor(expression, labelText, attrsLabel).ToHtmlString();

            return new MvcHtmlString(div.ToString());
        }

        public static MvcHtmlString FormDropDownListFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> items, string optionLabel = null, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            return helper.DropDownListFor(expression, items, optionLabel, InternalHelper.AddClassName(htmlAttributes, FormControl).InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString FormDropDownListFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool?>> expression, string trueText, string falseText, string nullText = null, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            SelectListItem[] items = new SelectListItem[]
            {
                new SelectListItem { Text = trueText , Value = "true"},
                new SelectListItem { Text = falseText, Value = "false"}
            };

            return FormDropDownListFor(helper, expression, items, nullText ?? string.Empty, size, @readonly, disabled, htmlAttributes);
        }

        public static MvcHtmlString FormDropDownListFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression, string trueText, string falseText, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            SelectListItem[] items = new SelectListItem[]
            {
                new SelectListItem { Text = trueText , Value = "true"},
                new SelectListItem { Text = falseText, Value = "false"}
            };

            return FormDropDownListFor(helper, expression, items, null, size, @readonly, disabled, htmlAttributes);
        }

        public static MvcHtmlString FormEnumDropDownListFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string optionLabel = null, BootstrapSize size = BootstrapSize.Auto, bool @readonly = false, bool disabled = false, object htmlAttributes = null)
        {
            return helper.EnumDropDownListFor(expression, optionLabel, InternalHelper.AddClassName(htmlAttributes, FormControl).InitializeInputControl(size, @readonly, disabled));
        }

        public static MvcHtmlString DangerValidationMessageFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string message = null, object htmlAttributes = null)
        {
            return helper.ValidationMessageFor(expression, message ?? string.Empty, InternalHelper.AddClassName(htmlAttributes, "text-danger"));
        }

        public static MvcHtmlString HFormGroupFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Func<Expression<Func<TModel, TValue>>, MvcHtmlString> input, string labelText = null, int labelColumns = 3, object htmlAttributes = null)
        {
            return HFormGroupFor(helper, expression, () => input(expression), labelText, labelColumns, htmlAttributes);
        }

        public static MvcHtmlString HFormGroupFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Func<MvcHtmlString> input, string labelText = null, int labelColumns = 3, object htmlAttributes = null)
        {
            TagBuilder group = new TagBuilder("div"), child = new TagBuilder("div");
            
            StringBuilder html = new StringBuilder();

            if (htmlAttributes != null)
                group.MergeAttributes(InternalHelper.ToValues(htmlAttributes));

            group.AddCssClass("form-group row");

            html.AppendLine(helper.HLabelFor(expression, labelText: labelText, columns: labelColumns).ToHtmlString());

            child.AddCssClass("col");

            child.InnerHtml = input().ToHtmlString() + Environment.NewLine + helper.DangerValidationMessageFor(expression).ToHtmlString();

            html.Append(child.ToString());

            group.InnerHtml = html.ToString();

            return new MvcHtmlString(group.ToString());
        }

        public static MvcHtmlString FormGroupFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Func<Expression<Func<TModel, TValue>>, MvcHtmlString> input, string labelText = null, object htmlAttributes = null)
        {
            return FormGroupFor(helper, expression, () => input(expression), labelText, htmlAttributes);
        }

        public static MvcHtmlString FormGroupFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Func<MvcHtmlString> input, string labelText = null, object htmlAttributes = null)
        {
            TagBuilder group = new TagBuilder("div");

            StringBuilder html = new StringBuilder();

            if (htmlAttributes != null)
                group.MergeAttributes(InternalHelper.ToValues(htmlAttributes));

            group.AddCssClass("form-group");
            

            html.AppendLine(helper.LabelFor(expression, labelText: labelText).ToHtmlString());

            html.AppendLine(input().ToHtmlString() + Environment.NewLine + helper.DangerValidationMessageFor(expression).ToHtmlString());

            group.InnerHtml = html.ToString();

            return new MvcHtmlString(group.ToString());
        }

    }
}
