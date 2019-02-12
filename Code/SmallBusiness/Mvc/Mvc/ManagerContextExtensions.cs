using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nabla.Mis.Web.Mvc
{
    public static class ManagerContextExtensions
    {
        public static T ManagerContext<T>(this ViewContext context)
            where T : ManagerContext
        {
            if (context.Controller is ManagerController<T> mc)
            {
                return mc.ManagerContext;
            }
            else
                return null;
        }

        public static T Manager<T>(this ViewContext context)
            where T: class, IManager
        {
            var mc = context.ManagerContext<ManagerContext>();

            return mc?.GetManager<T>();
        }
    }
}
