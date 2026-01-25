using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Admin
{
    public class AdminResetPasswordController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminResetPassword
        public async Task<ActionResult> Index(string id)
        {
            ClsForgotPasswordVm obj = new ClsForgotPasswordVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.Token = id;

            WebApi.UserController userController = new WebApi.UserController();
            var resetPasswordLinkValidationResult = await userController.ResetPasswordLinkValidation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(resetPasswordLinkValidationResult);

            if (oClsResponse.Status == 1)
            {
                return View();
            }
            else
            {
                return Redirect("/ErrorPage/LinkExpired");
            }
        }

        public async Task<ActionResult> ResetPassword(ClsForgotPasswordVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web";

            WebApi.UserController userController = new WebApi.UserController();
            var resetPasswordResult = await userController.ResetPassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(resetPasswordResult);

            return Json(oClsResponse);
        }
    }
}