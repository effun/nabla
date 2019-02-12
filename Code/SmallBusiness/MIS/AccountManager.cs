using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Nabla.Conversion;
using Nabla.Linq;
using Nabla.Linq.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Mis
{
    public abstract class AccountManager<TContext, TUser, TRole, TKey, TUserModel> : IManager
        where TContext : ManagerContext
        where TUser : IdentityUser<TKey, IdentityUserLogin<TKey>, IdentityUserRole<TKey>, IdentityUserClaim<TKey>>, new()
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey, IdentityUserRole<TKey>>, new()
        where TUserModel : class, IUserModel
    {
        TContext _context;

        public AccountManager(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TContext Context => _context;

        ManagerContext IManager.Context => Context;

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            if (!result.Succeeded)
            {
                message += Environment.NewLine + string.Join(Environment.NewLine, result.Errors);

                throw new InvalidOperationException(message);
            }
        }

        #region users

        protected abstract UserStore<TUser, TRole, TKey, IdentityUserLogin<TKey>, IdentityUserRole<TKey>, IdentityUserClaim<TKey>> CreateUserStore(DbContext db);

        private UserManager<TUser, TKey> _userManager;

        public UserManager<TUser, TKey> UserManager
        {
            get
            {
                if (_userManager == null)
                    _userManager = CreateUserManager(Context.Database);

                return _userManager;
            }
        }

        protected virtual UserManager<TUser, TKey> CreateUserManager(DbContext database)
        {
            return new UserManager<TUser, TKey>(CreateUserStore(database));
        }


        public Task<TUser> FindUserByIdAsync(TKey id, bool throwError = false)
        {
            var user = UserManager.FindByIdAsync(id);

            if (user == null && throwError)
                throw new ArgumentException("User " + id + " not found.");

            return user;
        }

        public Task<TUser> FindUserByNameAsync(string name)
        {
            return UserManager.FindByNameAsync(name);
        }

        protected virtual IQueryable<TUser> UserDefaultSort(IQueryable<TUser> query, QueryArgs args)
        {
            return query.OrderBy(o => o.UserName);
        }

        /// <summary>
        /// Retrieve all users.
        /// </summary>
        /// <returns></returns>
        public async Task<TUser[]> AllUsersAsync()
        {
            UserQueryArgs args = new UserQueryArgs();

            return (await QueryUsersAsync(args)).Items;
        }

        public Task<PagedResult<TUser>> QueryUsersAsync(UserQueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            args.SetDefaultSort<TUser>(UserDefaultSort);

            return UserManager.Users.PagedResultAsync(args);
        }

        protected virtual async Task UpdateUserAsync(TUser entity, TUserModel model, EntityAction action)
        {
            ModelConvert.Target(entity)
                .Ignore(o => o.PasswordHash)
                .Ignore(o => o.Roles)
                .Ignore(o => o.Id)
                .PopulateBy(model);

            if (model.Roles != null)
            {
                entity.Roles.Clear();

                var rm = RoleManager;

                foreach (var name in model.Roles)
                {
                    var role = await rm.FindByNameAsync(name);

                    entity.Roles.Add(new IdentityUserRole<TKey> { RoleId = role.Id });
                }
            }
        }

        public async Task<TUser> CreateUserAsync(TUserModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            TUser entity = new TUser();

            await UpdateUserAsync(entity, model, EntityAction.Create);

            var result = await UserManager.CreateAsync(entity, model.Password);

            ThrowIdentityErrors("Failed to create user.", result);

            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<TUser> UpdateUserAsync(TUserModel model, TKey userId)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entity = await FindUserByIdAsync(userId, true);

            await UpdateUserAsync(entity, model, EntityAction.Modify);

            Context.UseTransaction();

            var mgr = UserManager;

            var result = await mgr.UpdateAsync(entity);

            ThrowIdentityErrors("Failed to update user.", result);

            if (!string.IsNullOrEmpty(model.Password))
            {
                mgr.UserTokenProvider = CreateTokenProvider();
                var token = await mgr.GeneratePasswordResetTokenAsync(userId);
                result = await mgr.ResetPasswordAsync(userId, token, model.Password);

                ThrowIdentityErrors("Failed to change password.", result);
            }

            Context.CommitTransaction();

            return entity;
        }

        protected virtual IUserTokenProvider<TUser, TKey> CreateTokenProvider()
        {
            return new DummyTokenProvider();
        }

        #endregion

        #region roles

        private RoleManager<TRole, TKey> _roleManager;

        public RoleManager<TRole, TKey> RoleManager
        {
            get
            {
                if (_roleManager == null)
                    _roleManager = CreateRoleManager(Context.Database);

                return _roleManager;
            }
        }

        protected virtual RoleManager<TRole, TKey> CreateRoleManager(DbContext db)
        {
            return new RoleManager<TRole, TKey>(CreateRoleStore(db));
        }

        protected abstract RoleStore<TRole, TKey, IdentityUserRole<TKey>> CreateRoleStore(DbContext db);

        protected virtual IQueryable<TRole> RoleDefaultSort(IQueryable<TRole> query)
        {
            return query.OrderBy(o => o.Name);
        }

        public Task<TRole[]> AllRolesAsync()
        {
            return RoleDefaultSort(RoleManager.Roles).ToArrayAsync();
        }

        #endregion

        #region nested types

        class DummyTokenProvider : IUserTokenProvider<TUser, TKey>
        {
            public Task<string> GenerateAsync(string purpose, UserManager<TUser, TKey> manager, TUser user)
            {
                return Task.FromResult(Guid.NewGuid().ToString("N"));
            }

            public Task<bool> IsValidProviderForUserAsync(UserManager<TUser, TKey> manager, TUser user)
            {
                return Task.FromResult(user.GetType() == typeof(TUser));
            }

            public Task NotifyAsync(string token, UserManager<TUser, TKey> manager, TUser user)
            {
                return Task.CompletedTask;
            }

            public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser, TKey> manager, TUser user)
            {
                return Task.FromResult(true);
            }
        }

        #endregion
    }


}
