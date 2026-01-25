using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace EquiBillBook.Filters
{
    public class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        CommonController oCommonController = new CommonController();
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;

            ClsExceptionLogger oClsExceptionLogger = new ClsExceptionLogger()
            {
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace,
                Uri = actionExecutedContext.Request?.RequestUri?.AbsoluteUri,
                RequestJson = JsonConvert.SerializeObject(actionExecutedContext.ActionContext.ActionArguments.ToList()),
                AddedOn = DateTime.Now,
                InnerException = exception.InnerException?.Message
            };

            ConnectionContext oConnectionContext = new ConnectionContext();
            oConnectionContext.DbClsExceptionLogger.Add(oClsExceptionLogger);
            oConnectionContext.SaveChanges();

            var data = new
            {
                Status = 0,
                Message = "Something went wrong. Please try again later.", //exception.Message,
                InnerException = exception.InnerException?.Message
            };
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, data, "application/json");
        }
    }
}