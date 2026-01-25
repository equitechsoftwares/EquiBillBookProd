using EquiBillBook.Controllers.WebApi;
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
    public class ResetPasswordController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: ResetPassword
        public async Task<ActionResult> Index(string id)
        {
            // SEO Meta Tags for Reset Password Page
            ViewBag.PageTitle = "Reset Password | EquiBillBook GST Billing Software | Create New Password";
            ViewBag.MetaDescription = "Create a new password for your EquiBillBook GST billing software account. Secure password reset process with token validation. Complete your account recovery and regain access to your business software.";
            ViewBag.MetaKeywords = "reset password EquiBillBook, create new password, password reset token, account recovery, GST software password change, billing software password reset, secure password reset, EquiBillBook new password";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Reset Password - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Create a new password for your EquiBillBook account. Secure token-based password reset process for your GST billing software.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "Reset Password - EquiBillBook";
            ViewBag.TwitterDescription = "Create a new password for your EquiBillBook GST billing software account. Secure password reset process.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/resetpassword";

            ClsForgotPasswordVm obj = new ClsForgotPasswordVm();
            obj.Token = id;
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            DomainController domainController = new DomainController();

            var resetPasswordResult = await userController.ResetPasswordLinkValidation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(resetPasswordResult);

            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

            if (oClsResponse.Status==1)
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
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web";
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            DomainController domainController = new DomainController();

            var resetPasswordResult = await userController.ResetPassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(resetPasswordResult);

            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

            return Json(oClsResponse);
        }

    }
}