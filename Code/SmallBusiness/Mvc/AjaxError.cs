using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nabla.Mis.Web
{
    public class AjaxError
    {
        public AjaxError(IEnumerable<string> errors)
        {
            Errors = errors.ToArray();
        }

        public AjaxError(Exception error, bool details)
        {
            Errors = new string[] { error.Message };

            if (details)
            {
                StackTrace = error.StackTrace;
                ExceptionType = error.GetType().FullName;
            }
        }

        public AjaxError(ViewDataDictionary viewData)
        {
            List<string> errors = new List<string>();

            foreach (var pair in  viewData.ModelState)
            {
                var state = pair.Value;

                if (state != null)
                {
                    errors.AddRange(state.Errors.Where(o => !string.IsNullOrEmpty(o.ErrorMessage)).Select(o => o.ErrorMessage));
                }
            }

            Errors = errors.ToArray();
        }

        public string[] Errors { get; set; }

        public string StackTrace { get; set; }

        public string ExceptionType { get; set; }
    }
}