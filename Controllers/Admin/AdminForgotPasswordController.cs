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
    public class AdminForgotPasswordController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminForgotPassword
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);
            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
            if (oClsResponse1.WhitelabelType == 0)
            {
                return Redirect("/errorpage/domain");
            }

            return View();
        }

        public async Task<ActionResult> ForgotPassword(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.UserType = "reseller";
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            ClsForgotPasswordVm forgotPasswordObj = new ClsForgotPasswordVm 
            { 
                EmailId = obj.EmailId, 
                Domain = obj.Domain, 
                UserType = obj.UserType 
            };
            var result = await userController.ForgotPassword(forgotPasswordObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}