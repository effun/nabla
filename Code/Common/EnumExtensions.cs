using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace System
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Enum value, bool shortName = false, string seperator = ",")
        {
            Type type = value.GetType();

            if (Enum.IsDefined(type, value))
                return GetText(type, value, shortName);

            if (type.IsDefined(typeof(FlagsAttribute), false))
            {
                Array array = Enum.GetValues(type);
                List<string> list = new List<string>(array.Length);
                object zero = Enum.ToObject(type, 0);
                foreach (Enum v in array)
                {
                    if (!zero.Equals(v) && value.HasFlag(v))
                        list.Add(GetText(type, v, shortName));
                }

                if (list.Count > 0)
                    return string.Join(seperator, list);
            }

            return value.ToString();

        }

        private static string GetText(Type type, object value, bool shortName)
        {
            string name = Enum.GetName(type, value);

            FieldInfo field = type.GetField(name);
            bool found = false;

            DisplayAttribute da1 = field.GetCustomAttribute<DisplayAttribute>();

            if (da1 != null)
            {
                string n = shortName ? da1.GetShortName() : da1.GetName();

                if (string.IsNullOrEmpty(n))
                {
                    n = da1.GetName();
                    found = !string.IsNullOrEmpty(n);
                }
                else
                    found = true;

                if (found) name = n;
            }

            if (!found)
            {
                DescriptionAttribute da = field.GetCustomAttribute<DescriptionAttribute>();

                if (da != null)
                    name = da.Description;

            }

            return name;
        }

    }
}