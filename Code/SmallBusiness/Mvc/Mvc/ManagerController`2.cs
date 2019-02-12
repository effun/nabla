using Nabla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Nabla.Mis.Web.Mvc
{
    public class ManagerController<TManagerContext, TManager> : ManagerController<TManagerContext>
        where TManager : ManagerBase<TManagerContext>
        where TManagerContext : ManagerContext

    {
        public ManagerController()
        {

        }


        public ManagerController(TManagerContext context)
            : base(context)
        {

        }

        public TManager Manager => ManagerContext.GetManager<TManager>();

    }



 
}