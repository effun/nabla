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
    public class ManagerBase<TContext, TEntity, TKey> : ManagerBase<TContext>
        where TContext : ManagerContext
        where TEntity : class
        where TKey : IEquatable<TKey>
    {
        public ManagerBase(TContext server) : base(server)
        {
        }

        public async Task<TEntity> FindAsync(TKey key, bool throwError = false)
        {
            var entity = await Table.FindAsync(GetKeyValues(key));

            if (entity == null && throwError)
                throw new ArgumentException("The specified entity not found.");

            return entity;
        }

        protected virtual IQueryable<TEntity> DefaultSort(IQueryable<TEntity> query, QueryArgs args)
        {
            return query;
        }

        protected virtual IQueryable<TEntity> QueryFilter(IQueryable<TEntity> query, QueryArgs args)
        {
            return query;
        }

        public Task<PagedResult<TEntity>> QueryAsync(QueryArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Sort.IsNullOrEmpty()) args.SetDefaultSort<TEntity>(DefaultSort);

            return QueryFilter(Table, args).PagedResultAsync(args);
        }

        public Task<PagedResult<TEntity, TResult>> QueryAsync<TResult>(QueryArgs args, ModelConvertOptions options = null)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Sort.IsNullOrEmpty()) args.SetDefaultSort<TEntity>(DefaultSort);

            return QueryFilter(Table, args).PagedResultAsync<TEntity, TResult>(args, options);
        }

        public Task<PagedResult<TResult>> QueryByResultAsync<TResult>(QueryArgs args, ModelConvertOptions options = null)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Sort.IsNullOrEmpty()) args.SetDefaultSort<TEntity>(DefaultSort);

            return QueryFilter(Table, args).PagedResultByResultAsync<TEntity, TResult>(args, options);
        }

        public async Task<TEntity[]> AllAsync()
        {
            QueryArgs args = new QueryArgs();

            var result = await QueryAsync(args);

            return result.Items;
        }

        protected DbSet<TEntity> Table => Database.Set<TEntity>();

        protected static object[] GetKeyValues(TKey key)
        {
            if (key is IKey i)
            {
                return i.Values;
            }
            else
                return new object[] { key };
        }

    }

    public class ManagerBase<TContext, TEntity, TModel, TKey> : ManagerBase<TContext, TEntity, TKey>
        where TContext : ManagerContext
        where TEntity : class, new()
        where TModel : class
        where TKey : IEquatable<TKey>
    {
        public ManagerBase(TContext server)
            : base(server)
        {

        }

        protected virtual Task UpdateEntityAsync(TEntity entity, TModel model, EntityAction action)
        {
            CreateConvertOptions(action).Source<TModel>().PopulateTo(entity, model);

            return Task.CompletedTask;
        }


        protected virtual ModelConvertOptions CreateConvertOptions(EntityAction action)
        {
            var options = new ModelConvertOptions();

            if (action == EntityAction.ToModel)
            {
                options.Target<TModel>().Source<TEntity>();
            }
            else
                options.Target<TEntity>().Source<TModel>();

            options.IgnoreReadOnly();

            if (action == EntityAction.Modify)
                options.Target<TEntity>().IgnoreKeys();

            return options;
        }

        public virtual Task<TEntity> CreateAsync(TModel model)
        {
            return CreateAsync(model, true);
        }

        protected virtual async Task<TEntity> CreateAsync(TModel model, bool saveChanges)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entity = new TEntity();

            await UpdateEntityAsync(entity, model, EntityAction.Create);

            var set = Table;

            set.Add(entity);

            if (saveChanges)
                await Database.SaveChangesAsync();

            return entity;

        }

        public async Task<TEntity> UpdateAsync(TModel model, TKey key)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entity = await FindAsync(key);

            if (entity != null)
            {
                await UpdateEntityAsync(entity, model, EntityAction.Modify);

                await Database.SaveChangesAsync();

                return entity;
            }
            else
                return null;
        }

        protected virtual Task OnDeleteEntity(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(TKey key)
        {
            var set = Table;
            var entity = await FindAsync(key);

            if (entity == null)
                return false;

            await OnDeleteEntity(entity);

            set.Remove(entity);

            await Database.SaveChangesAsync();

            return true;
        }


        public virtual TModel ToModel(TEntity entity)
        {
            return CreateConvertOptions(EntityAction.ToModel).Source<TEntity>().ConvertTo<TModel>(entity);
        }

        public async Task<TModel> FindModelAsync(TKey key)
        {
            var entity = await FindAsync(key);

            if (entity != null)
                return ToModel(entity);
            else
                return null;
        }

    }

}
