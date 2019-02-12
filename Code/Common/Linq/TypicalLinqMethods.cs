using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Nabla.Linq
{
    internal static class TypicalLinqMethods
    {
        static Dictionary<string, MethodInfo> _methods;

        private static void EnsureInitialized()
        {
            if (_methods == null)
            {
                var methods = new Dictionary<string, MethodInfo>();

                typeof(Enumerable).FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public,
                    (info, c) =>
                    {
                        MethodInfo method = (MethodInfo)info;
                        string name = info.Name;

                        if (name == "Contains")
                        {
                            if (method.GetParameters().Length == 2)
                            {
                                methods.Add("Enumerable" + name, method);
                                return true;
                            }
                        }
                        else if (name == "ToArray")
                        {
                            methods.Add(name, (MethodInfo)info);
                        }
                        if (name == "Select")
                        {
                            if (method.GetParameters()[1].ParameterType.GenericTypeArguments.Length == 2)
                                methods.Add("Enumerable" + name, method);
                        }
                        else if (name == "GroupBy")
                        {
                            if (method.GetParameters().Length == 2)
                            {
                                methods.Add("Enumerable" + name, method);
                            }
                        }
                        else if (name == "Average" || name == "Sum" || name == "Max" || name == "Min")
                        {
                            ParameterInfo[] parameters = method.GetParameters();

                            if (parameters.Length == 2)
                            {
                                var p = parameters[1];
                                var pt = p.ParameterType;

                                if (pt.IsGenericType)
                                {
                                    var gats = pt.GenericTypeArguments;

                                    if (gats.Length == 2 && !gats[1].IsGenericParameter)
                                        methods.Add(MakeAggregationName(false, name, gats[1]), method);
                                }
                            }
                        }
                        else if (name == "Count")
                        {
                            if (method.GetParameters().Length == 1)
                                methods.Add(MakeAggregationName(false, name, typeof(int)), method);
                        }
                        return false;
                    }
                , null);

                typeof(Queryable).FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Public,
                    (info, c) =>
                    {
                        string name = info.Name;
                        MethodInfo m = (MethodInfo)info;

                        if (name == "OrderBy" || name == "OrderByDescending" || name == "ThenBy" || name == "ThenByDescending")
                        {
                            if (m.GetParameters().Length == 2)
                            {
                                methods.Add("Queryable" + name, m);
                                return true;
                            }
                        }
                        if (name == "Select")
                        {
                            if (m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                                methods.Add("Queryable" + name, m);
                        }
                        else if (name == "Count")
                        {
                            if (m.GetParameters().Length == 1)
                                methods.Add("Queryable" + name, m);
                        }
                        else if (name == "GroupBy")
                        {
                            if (m.GetParameters().Length == 2)
                            {
                                methods.Add("Queryable" + name, m);
                            }
                        }
                        else if (name == "Average" || name == "Sum" || name == "Max" || name == "Min")
                        {
                            ParameterInfo[] parameters = m.GetParameters();

                            if (parameters.Length == 2)
                            {
                                var p = parameters[1];
                                var pt = p.ParameterType;

                                if (pt.IsSubclassOf(typeof(LambdaExpression)))
                                {
                                    var gats = pt.GenericTypeArguments[0].GenericTypeArguments;

                                    if (gats.Length == 2 && !gats[1].IsGenericParameter)
                                        methods.Add(MakeAggregationName(true, name, gats[1]), m);
                                }
                            }
                        }
                        else if (name == "Skip" || name == "Take")
                            methods.Add("Queryable" + name, (MethodInfo)info);

                        return false;
                    }
                , null);


                Type type = typeof(string);
                methods["StringStartsWith"] = type.GetMethod("StartsWith", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { type }, null);
                methods["StringEndsWidth"] = type.GetMethod("EndsWith", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { type }, null);
                methods["StringContains"] = type.GetMethod("Contains", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { type }, null);

                _methods = methods;
            }
        }

        private static string MakeAggregationName(bool queryable, string methodName, Type type)
        {
            string typeName, category;

            Type ut = Nullable.GetUnderlyingType(type);

            if (ut == null) typeName = type.Name;
            else typeName = ut.Name + "?";

            category = queryable ? "Queryable" : "Enumerable";

            return $"{category}_{methodName}_{typeName}";
        }

        public static MethodInfo EnumerableAggregation(string name, Type type)
        {
            return GetMethod(MakeAggregationName(false, name, type));
        }

        public static MethodInfo QueryableAggregation(string name, Type type)
        {
            return GetMethod(MakeAggregationName(true, name, type));
        }

        public static MethodInfo GetMethod(string name)
        {
            EnsureInitialized();

            if (_methods.TryGetValue(name, out MethodInfo method))
                return method;

            throw new ArgumentException($"Method {name} not found.");
        }

        public static MethodInfo QueryableOrderBy => GetMethod("QueryableOrderBy");

        public static MethodInfo QueryableOrderByDescending => GetMethod("QueryableOrderByDescending");

        public static MethodInfo QueryableThenBy => GetMethod("QueryableThenBy");

        public static MethodInfo QueryableThenByDescending => GetMethod("QueryableThenByDescending");

        public static MethodInfo QueryableGroupBy => GetMethod("QueryableGroupBy");

        public static MethodInfo EnumerableContains => GetMethod("EnumerableContains");

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> object of <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>.
        /// </summary>
        public static MethodInfo EnumerableCount => EnumerableAggregation("Count", typeof(int));

        public static MethodInfo EnumerableSelect => GetMethod("EnumerableSelect");

        public static MethodInfo EnumerableGroupBy => GetMethod("EnumerableGroupBy");

        public static MethodInfo StringStartsWith => GetMethod("StringStartsWith");

        public static MethodInfo StringEndsWith => GetMethod("StringEndsWith");

        public static MethodInfo StringContains => GetMethod("StringContains");

        public static MethodInfo QueryableSelect => GetMethod("QueryableSelect");

        public static MethodInfo QueryableSkip => GetMethod("QueryableSkip");

        public static MethodInfo QueryableTake => GetMethod("QueryableTake");

        public static MethodInfo ToArray => GetMethod("ToArray");

        public static MethodInfo QueryableCount => GetMethod("QueryableCount");
    }
}
