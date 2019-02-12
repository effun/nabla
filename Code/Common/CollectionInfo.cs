using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Nabla
{
    public class CollectionInfo
    {
        internal CollectionInfo(Type objectType)
        {
            ObjectType = objectType;
        }

        public Type ObjectType { get; private set; }

        public bool SupportManipulation { get; internal set; }

        public Type ElementType { get; internal set; }

        public bool IsGeneric { get; internal set; }

        public bool IsArray { get; internal set; }

        public Type InterfaceType { get; internal set; }

        public bool IsCollection => InterfaceType != null;

        //private object InvokeMethod(object instance, string name, object[] parameters)
        //{
        //    if (instance == null)
        //        throw new ArgumentNullException(nameof(instance));

        //    var map = instance.GetType().GetInterfaceMap(InterfaceType);
        //    MethodInfo method = null;

        //    for (int i = 0; i < map.InterfaceMethods.Length; i++)
        //    {
        //        var im = map.InterfaceMethods[i];

        //        if (im.Name == name)
        //        {
        //            method = map.TargetMethods[i];
        //            break;
        //        }
        //    }

        //    if (method == null)
        //        throw new InvalidOperationException($"Method {name} not found on type {instance.GetType()}");

        //    return method.Invoke(instance, parameters);
        //}

        //public void InvokeClear(object instance)
        //{
        //    InvokeMethod(instance, "Clear", new object[0]);
        //}

        //public void InvokeAdd(object instance, object item)
        //{
        //    InvokeMethod(instance, "Add", new object[] { item });
        //}

        //public bool InvokeRemove(object instance, object item)
        //{
        //    return (bool)InvokeMethod(instance, "Remove", new object[] { item });
        //}

        public ICollectionWrapper CreateWrapper(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!instance.GetType().CanCastTo(ObjectType))
                throw new ArgumentException("Invalid instance type.");
            
            if (IsGeneric)
            {
                return (ICollectionWrapper)Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(ElementType), instance);
            }
            else
                return new CollectionWrapper((ICollection)instance);
        }

        public ICollectionWrapper CreateWrapper()
        {
            object instance;

            if (ObjectType.IsInterface)
            {
                instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(ElementType ?? typeof(object)));
            }
            else
            {
                try
                {
                    instance = Activator.CreateInstance(ObjectType);
                }
                catch(Exception ex)
                {
                    throw new InvalidOperationException("Cannot create instance for " + ObjectType, ex);
                }
            }

            return CreateWrapper(instance);
        }
    }
}
