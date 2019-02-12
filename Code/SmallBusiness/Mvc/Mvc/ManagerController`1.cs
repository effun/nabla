using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nabla.Mis.Web.Mvc
{
    public class ManagerController<TManagerContext> : EnhancedController
         where TManagerContext : ManagerContext
    {
        TManagerContext _managerContext;

        public ManagerController()
        {

        }

        public ManagerController(TManagerContext context)
        {
            _managerContext = context;
        }

        public static Func<HttpRequestBase, TManagerContext> GetManagerContextFunc { get; set; }

        protected virtual TManagerContext CreateManagerContext()
        {
            return (TManagerContext)ModelInfo.GetModelInfo(typeof(TManagerContext)).CreateInstance();
        }

        private TManagerContext GetManagerContext()
        {
            var context = GetManagerContextFunc?.Invoke(Request);

            return context ?? CreateManagerContext();
        }

        public TManagerContext ManagerContext
        {
            get
            {
                if (_managerContext == null)
                    _managerContext = CreateManagerContext();

                return _managerContext;
            }
        }

        protected T GetManager<T>()
            where T : IManager
        {
            return ManagerContext.GetManager<T>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _managerContext?.Dispose();

            base.Dispose(disposing);
        }

    }
}
