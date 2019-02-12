using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Mis
{
    //public class UserManager<TDbContext, TUserStore, TKey, TRole, TLogin, TClaim>
    //    : UserManager<IdentityUser<TKey, TLogin, TRole, TClaim>, TKey>, IManager<TDbContext>
    //    where TKey : IEquatable<TKey>
    //    where TDbContext : DbContext
    //    where TUserStore : UserStore<IdentityUser<TKey, TLogin, TRole, TClaim>, IdentityRole<TKey, TRole>, TKey, TLogin, TRole, TClaim>
    //    where TClaim : IdentityUserClaim<TKey>, new()
    //    where TRole : IdentityUserRole<TKey>, new()
    //    where TLogin : IdentityUserLogin<TKey>, new()
    //{

    //    ManagerContext<TDbContext> _context;

    //    public UserManager(ManagerContext<TDbContext> context, Func<TDbContext, TUserStore> createStore)
    //        : base(createStore(context.Database))
    //    {
    //        _context = context ?? throw new ArgumentNullException(nameof(context));
    //    }

    //    public UserManager(ManagerContext<TDbContext> context)
    //        : this(context, DefaultCreateStore)
    //    {

    //    }

    //    ManagerContext<TDbContext> IManager<TDbContext>.Context => _context;

    //    private static TUserStore DefaultCreateStore(TDbContext db)
    //    {
    //        return (TUserStore)Activator.CreateInstance(typeof(TUserStore), new object[] { db });
    //    }
    //}


}
