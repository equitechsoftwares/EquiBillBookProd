using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EquiBillBook.Filters
{
    public class AdminAuthorizationPrivilegeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false) &&
                !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined
                 (typeof(AllowAnonymousAttribute), true))
            {
                var request = filterContext.HttpContext.Request;
                var consentCookie = request.Cookies["adata"];

                if (consentCookie == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "adminlogin" }, { "action", "index" } });
                }
                else
                {
                    string userType = consentCookie["UserType"];
                    string token = consentCookie["Token"];
                    long id = Convert.ToInt64(consentCookie["Id"]);
                    long CompanyId = Convert.ToInt64(consentCookie["CompanyId"]);

                    ConnectionContext oConnectionContext = new ConnectionContext();
                    if (oConnectionContext.DbClsLoginDetails.Where(a => a.UserType == userType && a.AddedBy == id && a.Token == token && a.IsLoggedOut == false).FirstOrDefault() == null)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "adminlogin" }, { "action", "index" }, { "logout", true } });
                    }
                }
                base.OnActionExecuting(filterContext);
            }

        }

    }
}