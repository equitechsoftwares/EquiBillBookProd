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
    public class AuthorizationPrivilegeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false) &&
                !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined
                 (typeof(AllowAnonymousAttribute), true))
            {
                var request = filterContext.HttpContext.Request;
                var consentCookie = request.Cookies["data"];

                if (consentCookie == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "login" }, { "action", "index" } });
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
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "login" }, { "action", "index" }, { "logout", true } });
                    }

                    if (oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == CompanyId &&
             a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault() == 0)
                    {
                        if (request.ContentLength != 0)
                        {
                            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                            //if ((request.Params["PageIndex"] == null || request.Params["PageIndex"] == "" || request.Params["PageIndex"] == "0") && !request.Url.ToString().ToLower().Contains("view"))
                            if ((request.Url.ToString().ToLower().Contains("insert") || request.Url.ToString().ToLower().Contains("update") ||
                                request.Url.ToString().ToLower().Contains("inactive") || request.Url.ToString().ToLower().Contains("delete") ||
                                request.Url.ToString().ToLower().Contains("changepassword") ||
                                request.Url.ToString().ToLower().Contains("profileupdate")
                                 || request.Url.ToString().ToLower().Contains("businesssettings") ||
                                 request.Url.ToString().ToLower().Contains("sendtestsms")
                                 || request.Url.ToString().ToLower().Contains("sendtestemail") ||
                                 request.Url.ToString().ToLower().Contains("sendtestwhatsapp") ||
                                 request.Url.ToString().ToLower().Contains("validateloginemail"))
                                 && (controllerName.ToLower() != "subscription" && controllerName.ToLower() != "feedback"
                                 && controllerName.ToLower() != "supportticket"))
                            {
                                bool IsTrial = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == CompanyId &&
             a.StartDate != null && a.Status == 2).Select(a => a.IsTrial).FirstOrDefault();

                                filterContext.Result = new ContentResult { Content = IsTrial.ToString() };
                            }
                        }
                    }

                }
                base.OnActionExecuting(filterContext);
            }

        }

    }
}