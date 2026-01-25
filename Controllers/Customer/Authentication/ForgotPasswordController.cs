using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Helpers;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{    
    public class ForgotPasswordController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: ForgotPassword
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // Load SEO settings from database
            SeoHelper.LoadSeoSettings(this, "forgotpassword");

            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            DomainController domainController = new DomainController();
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
            obj.UserType = "user";
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            ClsForgotPasswordVm forgotPasswordObj = new ClsForgotPasswordVm { Domain = obj.Domain, EmailId = obj.EmailId, UserType = obj.UserType };
            var result = await userController.ForgotPassword(forgotPasswordObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

    }
}