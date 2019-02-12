using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Nabla.Mis
{

    public class ManagerBase<TContext> : IManager
        where TContext : ManagerContext
    {
        TContext _server;

        public ManagerBase(TContext server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public TContext Context => _server;

        ManagerContext IManager.Context => Context;

        public DbContext Database => _server.Database;

        protected Task<int> SaveChangesAsync()
        {
            return Database.SaveChangesAsync();
        }

        public string GetCurrentUserId(bool throwError = true)
        {
            var id = Context.GetCurrentUserId();

            if (id == null && throwError)
                throw new InvalidOperationException("未能找到当前用户信息，可能是用户没有登录。");

            return id;
        }

        protected int UseTransaction()
        {
            return Context.UseTransaction();
        }

        protected void CommitTransaction()
        {
            Context.CommitTransaction();
        }

        protected void CancelTransaction()
        {
            Context.CancelTransaction();
        }
    }
}
