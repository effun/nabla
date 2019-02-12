using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Mis
{
    public abstract class ManagerContext : IDisposable
    {
        DbContext _database;

        public ManagerContext()
        {
            
        }

        public ManagerContext(DbContext database)
        {
            _database = database;
        }

        public IIdentity CurrentUser
        {
            get
            {
                var user = System.Threading.Thread.CurrentPrincipal;

                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                    return user.Identity;
                else
                    return null;
            }
        }

        public string GetCurrentUserId()
        {
            return CurrentUser?.GetUserId();
        }

        protected abstract DbContext CreateDatabase();

        public DbContext Database
        {
            get
            {
                if (_database == null)
                    _database = CreateDatabase();

                return _database;
            }
        }

        public virtual void Dispose()
        {
            _database?.Dispose();

            if (_managers != null)
            {
                foreach (var manager in _managers)
                {
                    (manager as IDisposable)?.Dispose();
                }
            }
        }

        #region managers
        HashSet<IManager> _managers;

        private void AttachManager(IManager manager)
        {
            if (_managers == null)
                _managers = new HashSet<IManager>();
            else if (_managers.Any(o => o.GetType() == manager.GetType()))
                throw new InvalidOperationException($"已存在类型为{manager.GetType()}的管理器。");

            _managers.Add(manager);
        }

        private void DeattachManager(IManager manager)
        {
            _managers?.Remove(manager);
        }

        public T GetManager<T>()
            where T : IManager
        {
            return (T)GetManager(typeof(T));
        }

        private IManager GetManager(Type type)
        {
            IManager manager = _managers?.FirstOrDefault(o => o.GetType() == type);

            if (manager == null)
            {
                manager = (IManager)Activator.CreateInstance(type, this);

                AttachManager(manager);
            }

            return manager;
        }

        protected virtual IEnumerable<Assembly> GetManagerAssemblies()
        {
            yield return GetType().Assembly;
        }

        public async Task ForEachManagerAsync<T>(Func<T, Task<bool>> action)
        {
            Assembly assembly = Database.GetType().Assembly;
            await ForEachManagerAsync(action, GetManagerAssemblies());
        }

        public async Task ForEachManagerAsync<T>(Func<T, Task<bool>> action, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsAbstract && type.GetInterface("IManager") == typeof(IManager))
                    {
                        if (typeof(T).IsAssignableFrom(type))
                        {
                            var mgr = GetManager(type);

                            if (await action((T)mgr))
                                break;
                        }
                    }
                }
            }
        }

        public Task ForEachManagerAsync(Func<IManager, Task<bool>> action)
        {
            return ForEachManagerAsync<IManager>(action);
        }



        #endregion

        #region transactions

        private int _transCounter;

        internal bool CommitTransaction()
        {
            if (_transCounter > 0)
            {
                if (--_transCounter == 0)
                {
                    var trans = Database.Database.CurrentTransaction;

                    if (trans == null)
                        throw new InvalidOperationException("There's no transaction associated with current database.");

                    trans.Commit();

                    return true;
                }
            }

            return false;
        }

        internal int UseTransaction()
        {
            if (Database.Database.CurrentTransaction == null)
            {
                Debug.Assert(_transCounter == 0);
                Database.Database.BeginTransaction();
            }

            return ++_transCounter;
        }

        internal void CancelTransaction()
        {
            if (_transCounter > 1)
                throw new InvalidOperationException("Multi-level of trancation detected, cancelling transaction is not allowed.");

            var trans = Database.Database.CurrentTransaction;

            if (trans != null)
                trans.Rollback();

            _transCounter = 0;
        }

        internal Task<int> SaveChangesAsync()
        {
            return Database.SaveChangesAsync();
        }

        #endregion

    }
}
