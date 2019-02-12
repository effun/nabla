using Newtonsoft.Json;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Nabla.Mis.Web
{
    public class JsonNetResult : JsonResult
    {

        public JsonSerializerSettings SerializerSettings { get; set; }

        public JsonNetResult()
        {
            SerializerSettings = new JsonSerializerSettings();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType)
              ? ContentType
              : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            object data = Data;

            if (data is Exception error)
            {
                var err = new AjaxError(error, context.HttpContext.Request.IsLocal);
                data = err;
            }

            if (data is AjaxError)
                response.StatusCode = 500;

            using (JsonTextWriter writer = new JsonTextWriter(response.Output))
            {
                if (context.HttpContext.Request.IsLocal)
                    writer.Formatting = Formatting.Indented;

                JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, data);

                writer.Flush();
            }
        }


    }

}