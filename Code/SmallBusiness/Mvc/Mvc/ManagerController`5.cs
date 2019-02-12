using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nabla.Mis.Web.Mvc
{
    public class ManagerController<TManagerContext, TManager, TEntity, TModel, TKey> : ManagerController<TManagerContext, TManager>
        where TManagerContext : ManagerContext
        where TManager : ManagerBase<TManagerContext, TEntity, TModel, TKey>
        where TModel : class
        where TEntity : class, new()
        where TKey : IEquatable<TKey>
    {

        public ManagerController()
        {

        }


        public ManagerController(TManagerContext context)
            : base(context)
        {

        }

        protected virtual Task InitializeDetails(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public async Task<ActionResult> Details(TKey id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            TEntity entity = await Manager.FindAsync(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            await InitializeDetails(entity);

            return View(entity);
        }

        protected virtual string CreateViewName => "Edit";

        protected virtual Task<bool> InitializeEdit(TModel model, TEntity entity, EntityAction action)
        {
            ViewBag.IsCreate = action == EntityAction.Create;

            return Task.FromResult(true);
        }

        public async Task<ActionResult> Create()
        {
            bool ok = await InitializeEdit(null, null, EntityAction.Create);

            if (ok)
            {
                return View(CreateViewName);
            }
            else
                return BadRequest();
        }

        protected virtual TKey GetEntityKey(TEntity entity)
        {
            return (TKey)ModelInfo.GetModelInfo(typeof(TEntity)).GetKeyValue(entity)[0];
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TModel model)
        {
            return await IfModelStateValidAsync(async () =>
            {
                var entity = await Manager.CreateAsync(model);
                return EditSuccessAction(GetEntityKey(entity), model, entity, EntityAction.Create);
            }, () => EditFailAction(model, EntityAction.Create));
        }

        // GET: Projects/Edit/5
        public async Task<ActionResult> Edit(TKey id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var entity = await Manager.FindAsync(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = Manager.ToModel(entity);

            if (!await InitializeEdit(model, entity, EntityAction.Modify))
                return BadRequest();

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TModel model, TKey id)
        {
            return await IfModelStateValidAsync(async () =>
            {
                var entity = await Manager.UpdateAsync(model, id);
                return EditSuccessAction(id, model, entity, EntityAction.Modify);
            }, () => EditFailAction(model, EntityAction.Modify));
        }

        protected virtual ActionResult EditFailAction(TModel model, EntityAction action)
        {
            string name;

            if (action == EntityAction.Create)
            {
                name = CreateViewName;
            }
            else
                name = "Edit";

            ViewBag.IsCreate = action == EntityAction.Create;

            return View(name, model);
        }

        protected virtual ActionResult EditSuccessAction(TKey id, TModel model, TEntity entity, EntityAction action)
        {
            return RedirectToReturnAction("Details", null, new { id });
        }

        // GET: Projects/Delete/5
        public async Task<ActionResult> Delete(TKey id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            TEntity entity = await Manager.FindAsync(id);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(TKey id)
        {

            await Manager.DeleteAsync(id);

            return RedirectToAction("Index");
        }

    }

    public class ManagerController<TManagerContext, TManager, TEntity, TModel> : ManagerController<TManagerContext, TManager, TEntity, TModel, int>
        where TManager : ManagerBase<TManagerContext, TEntity, TModel, int>
        where TModel : class
        where TEntity : class, new()
        where TManagerContext : ManagerContext
    {
        public ManagerController()
        {

        }


        public ManagerController(TManagerContext context)
            : base(context)
        {

        }

    }


}
