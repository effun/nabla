using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Nabla.Mis.Web.Mvc
{
    public class EnhancedController : Controller
    {
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonNetResult
            {
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                Data = data,
                ContentType = contentType
            };
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = Json(filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }
            else
                base.OnException(filterContext);
        }

        protected async Task<ActionResult> IfModelStateValidAsync(Func<Task<ActionResult>> validAction, Func<Task<ActionResult>> invalidAction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ActionResult result = await validAction();

                    return result;
                }
                catch (Exception ex)
                {
                    if (Request.IsLocal)
                        throw;
                    else
                        ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return await invalidAction();
        }

        protected Task<ActionResult> IfModelStateValidAsync<TModel>(Func<Task<ActionResult>> validAction, TModel model, string viewName)
        {
            return IfModelStateValidAsync(validAction, () => View(viewName, model));
        }

        protected Task<ActionResult> IfModelStateValidAsync(Func<Task<ActionResult>> validAction, Func<ActionResult> invalidAction)
        {
            return IfModelStateValidAsync(validAction, () => Task.FromResult(invalidAction()));
        }

        protected async Task<JsonResult> IfModelStateValidAjaxAsync(Func<Task<object>> func)
        {
            object data;

            if (ModelState.IsValid)
            {
                try
                {
                    data = await func();
                }
                catch (Exception ex)
                {
                    data = ex;
                }
            }
            else
                data = new AjaxError(ViewData);

            return Json(data);
        }

        protected ActionResult BadRequest()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
        }

        protected ActionResult RedirectToReturnUrl(string defaultUrl)
        {
            string returnUrl = ReturnUrl;

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            else
                return Redirect(defaultUrl);
        }

        protected string ReturnUrl
        {
            get
            {
                string returnUrl = Request.QueryString["returnUrl"];

                if (!string.IsNullOrEmpty(returnUrl))
                    return returnUrl;
                else
                    return null;
            }
        }

        protected ActionResult RedirectToReturnAction(string defaultAction)
        {
            return RedirectToReturnAction(defaultAction, null);
        }

        protected ActionResult RedirectToReturnAction(string defaultAction, string defaultController)
        {
            return RedirectToReturnAction(defaultAction, defaultController, null);
        }

        protected ActionResult RedirectToReturnAction(string defaultAction, object routeValues)
        {
            return RedirectToReturnAction(defaultAction, null, routeValues);
        }

        protected ActionResult RedirectToReturnAction(string defaultAction, string defaultController, object routeValues)
        {
            string url = Url.Action(defaultAction, defaultController, routeValues);

            return RedirectToReturnUrl(url);
        }

        
    }
}