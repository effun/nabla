using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Web.Mvc.Html.Bootstrap
{
    public static class BootstrapHelper
    {
        public static string GetClassName(string component, BootstrapSize size, int span)
        {
            string name = component;

            if (size != BootstrapSize.Auto)
                name += "-" + GetSizeName(size);

            if (span >= 0)
                name += "-" + span;

            return name;
        }

        public static string GetSizeName(BootstrapSize size)
        {
            switch (size)
            {
                case BootstrapSize.Auto:
                    return string.Empty;
                case BootstrapSize.ExtraSmall:
                    return "xs";
                case BootstrapSize.Small:
                    return "sm";
                case BootstrapSize.Mediume:
                    return "md";
                case BootstrapSize.Large:
                    return "lg";
                case BootstrapSize.ExtraLarge:
                    return "xs";
                default:
                    throw new ArgumentException("Invalid size " + size);
            }
        }

        public static string GetColorName(BootstrapColor color)
        {
            return color.ToString().ToLower();
        }
    }
}
