using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    public static class TypeHelpers
    {
        public static IEnumerable<Type> FindTypes(Type baseType, TypeDiscoveryFilters filters)
        {
            return FindTypes(baseType, baseType.Assembly, filters);
        }

        public static IEnumerable<Type> FindTypes(Type baseType, Assembly assembly, TypeDiscoveryFilters filters)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            foreach (Type type in assembly.GetTypes())
            {
                if ((filters & TypeDiscoveryFilters.NonPublic) == TypeDiscoveryFilters.NonPublic && type.IsPublic)
                    continue;

                if ((filters & TypeDiscoveryFilters.Abstract) == TypeDiscoveryFilters.Abstract && type.IsAbstract)
                    continue;

                if ((filters & TypeDiscoveryFilters.BaseType) == TypeDiscoveryFilters.BaseType && type != baseType)
                    continue;

                if ((filters & TypeDiscoveryFilters.DefaultConstructor) == TypeDiscoveryFilters.DefaultConstructor)
                {
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;
                }

                if (type.IsSubclassOf(baseType))
                    yield return type;
            }

        }

        public static bool CanBeNull(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsClass || type.IsInterface)
                return true;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return true;
            }

            return false;
        }

        public static bool IsComplex(this Type type)
        {
            return !TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        public static CollectionInfo GetCollectionInfo(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            CollectionInfo info = new CollectionInfo(type);

            if (type.IsArray)
            {
                info.ElementType = type.GetElementType();
                info.IsArray = true;
                info.InterfaceType = typeof(ICollection);
            }
            else
            {
                int level = 0;

                if (type.IsInterface)
                {
                    FillCollectionInfo(type, info, ref level);
                }

                if (level == 0)
                {
                    foreach (var it in type.GetInterfaces())
                    {
                        FillCollectionInfo(it, info, ref level);

                        if (level == 4)
                            break;
                    }
                }
            }

            return info;

        }

        private static void FillCollectionInfo(Type it, CollectionInfo info, ref int level)
        {
            /*
             * level:
             *  0 = not a collection
             *  1 = collection
             *  2 = list
             *  3 = generic collection, does not support collection manipulation.
             *  4 = generic list
             */

            if (it.IsGenericType)
            {
                Type def = it.GetGenericTypeDefinition();

                if (def == typeof(IList<>))
                {
                    info.SupportManipulation = true;
                    info.ElementType = it.GetGenericArguments()[0];
                    info.InterfaceType = it;
                    info.IsGeneric = true;
                    level = 4;
                }
                else if (def == typeof(ICollection<>) && level < 3)
                {
                    info.SupportManipulation = true;
                    info.ElementType = it.GetGenericArguments()[0];
                    info.IsGeneric = true;
                    info.InterfaceType = it;
                    level = 3;
                }

            }
            else
            {
                if (it == typeof(IList) && level < 2)
                {
                    info.SupportManipulation = true;
                    info.InterfaceType = it;
                    level = 2;
                }
                else if (it == typeof(ICollection) && level < 1)
                {
                    info.InterfaceType = it;
                    level = 1;
                }
            }

        }

        public static bool CanCastTo(this Type type, Type target)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (target.IsAssignableFrom(type))
                return true;

            if (target.IsInterface)
            {
                return type.GetInterface(target.FullName) == target;
            }

            return false;
        }

        public static object DefaltValue(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        }

        public static MemberInfo GetPropertyOrField(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance, bool throwError = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var m1 = type.GetMember(name, bindingFlags)
                .FirstOrDefault(o => o.MemberType == MemberTypes.Property || o.MemberType == MemberTypes.Field);

            if (m1 == null && throwError)
                throw new InvalidOperationException("Property or field " + name + " not defined on " + type);

            return m1;
        }

        public static Type GetPropertyOrFieldType(this MemberInfo member)
        {
            if (member is PropertyInfo pi)
                return pi.PropertyType;

            if (member is FieldInfo fi)
                return fi.FieldType;

            throw new ArgumentException($"Member {member.Name} of type {member.DeclaringType} is not a property or field");
        }
    }

    [Flags]
    public enum TypeDiscoveryFilters
    {
        Default = 0,
        NonPublic = 1,
        Abstract = 2,
        BaseType = 4,
        DefaultConstructor = 8
    }
}
