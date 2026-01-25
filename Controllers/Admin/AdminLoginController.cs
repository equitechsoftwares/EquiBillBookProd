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
    public class AdminLoginController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminLogin
        public async Task<ActionResult> Index(bool? logout)
        {
            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);
            ViewBag.WhitelabelType = oClsResponse1.WhitelabelType;
            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

            if (logout == null)
            {
                if (Request.Cookies["adata"] != null)
                {
                    return Redirect("/admindashboard");
                }
                else
                {
                    if (oClsResponse1.WhitelabelType == 0)
                    {
                        return Redirect("/errorpage/domain");
                    }

                    return View();
                }
            }
            else
            {
                Response.Cookies["adata"].Expires = DateTime.Today.AddDays(-1);
                return View();
            }
        }

        public async Task<ActionResult> Login(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.UserType = "reseller";
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.Login(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Data.User != null)
            {

                int days = 0;
                if (obj.IsRememberMe == true)
                {
                    days = 365;
                }
                else
                {
                    days = 1;
                }

                Response.Cookies["adata"]["Token"] = oClsResponse.Data.User.Token;
                //Response.Cookies["adata"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
                Response.Cookies["adata"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
                Response.Cookies["adata"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
                Response.Cookies["adata"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
                Response.Cookies["adata"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
                //if (obj.Browser.ToLower() == "safari")
                //{
                //    Response.Cookies["adata"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencyCode + " ");
                //}
                //else
                //{
                    Response.Cookies["adata"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
                //}
                Response.Cookies["adata"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
                Response.Cookies["adata"]["IsDemo"] = "False";
                Response.Cookies["adata"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
                Response.Cookies["adata"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
                Response.Cookies["adata"]["WhitelabelBusinessIcon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessIcon);
                Response.Cookies["adata"]["WhitelabelFavicon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelFavicon);
                Response.Cookies["adata"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["aBusinessSetting"]["DateFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DateFormat);
                Response.Cookies["aBusinessSetting"]["TimeFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.TimeFormat);
                Response.Cookies["aBusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CurrencySymbolPlacement);
                Response.Cookies["aBusinessSetting"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["aSystemSetting"]["ShowHelpText"] = Convert.ToString(oClsResponse.Data.BusinessSetting.ShowHelpText);
                Response.Cookies["aSystemSetting"]["EnableDarkMode"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableDarkMode);
                Response.Cookies["aSystemSetting"]["FixedHeader"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedHeader);
                Response.Cookies["aSystemSetting"]["FixedFooter"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedFooter);
                if (obj.Browser.ToLower() == "safari")
                {
                    Response.Cookies["aSystemSetting"]["EnableSound"] = "False";
                }
                else
                {
                    Response.Cookies["aSystemSetting"]["EnableSound"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableSound);
                }
                Response.Cookies["aSystemSetting"].Expires = DateTime.Today.AddDays(days);
            }
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Logout(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["LoginDetailsId"];
                arr[1] = Request.Cookies["adata"]["Id"];
                obj.LoginDetailsId = Convert.ToInt64(arr[0]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.AddedBy = Convert.ToInt64(arr[1]);
            }

            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.Logout(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            Response.Cookies["adata"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["aBusinessSetting"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["aSystemSetting"].Expires = DateTime.Today.AddDays(-1);
            //Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(-1);
            //return Redirect("/login");
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}