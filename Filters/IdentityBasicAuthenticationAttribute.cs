using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EquiBillBook.Filters
{
    public class IdentityBasicAuthenticationAttribute : AuthorizationFilterAttribute
    {        
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var data = new
            {
                Status = 2,
                Message = "Something went wrong. Please login again"
            };
            try
            {
                Contract.Assert(actionContext != null);

                if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                           || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                {
                    return;
                }

                if (actionContext.Request.Headers.Authorization == null)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "no headers", "application/json");
                }
                else
                {
                    string authenticationtocken = actionContext.Request.Headers.Authorization.Parameter;
                    string decodedauthenticationtocken = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationtocken));
                    string[] usernamepasswordArray = decodedauthenticationtocken.Split(':');
                    string[] usernameTypeArray = usernamepasswordArray[0].Split('_');

                    string userType = usernameTypeArray[0];
                    long username = Convert.ToInt64(usernameTypeArray[1]);
                    string password = usernamepasswordArray[1];

                    ConnectionContext oConnectionContext = new ConnectionContext();
                    if (oConnectionContext.DbClsLoginDetails.Where(a => a.UserType == userType && a.AddedBy == username && a.Token == password && a.IsLoggedOut == false).FirstOrDefault() == null)
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, data, "application/json");
                    }
                }
            }
            catch(Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, ex.ToString(), "application/json");
            }
        }
    }
}