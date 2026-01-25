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
    public class LoginController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Login
        public async Task<ActionResult> Index(bool? logout)
        {
            // Load SEO settings from database
            SeoHelper.LoadSeoSettings(this, "login");

            if (Request.Cookies["data"] != null)
            {
                if(logout == null)
                {
                    return Redirect("/dashboard");
                }
                else
                {
                    ClsDomainVm obj = new ClsDomainVm();
                    obj.Domain = Request.Url.Host.Replace("www.", "");
                    ViewBag.Domain = obj.Domain;
                    serializer.MaxJsonLength = 2147483644;

                    DomainController domainController = new DomainController();
                    var domainResult = await domainController.DomainCheckForRedirection(obj);
                    ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);
                    ViewBag.WhitelabelType = oClsResponse1.WhitelabelType;

                    ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
                    Response.Cookies["data"].Expires = DateTime.Today.AddDays(-1);
                    return View();
                }
            }
            else
            {
                ClsDomainVm obj = new ClsDomainVm();
                obj.Domain = Request.Url.Host.Replace("www.", "");
                ViewBag.Domain = obj.Domain;
                serializer.MaxJsonLength = 2147483644;

                DomainController domainController = new DomainController();
                var domainResult = await domainController.DomainCheckForRedirection(obj);
                ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);
                ViewBag.WhitelabelType = oClsResponse1.WhitelabelType;

                ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

                if (logout == null)
                {
                    if (oClsResponse1.WhitelabelType == 0)
                    {
                        return Redirect("/errorpage/domain");
                    }

                    return View();
                }
                else
                {
                    Response.Cookies["data"].Expires = DateTime.Today.AddDays(-1);
                    return View();
                }
            }
        }

        public async Task<ActionResult> Login(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web";
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.UserType = "user";
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
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

                Response.Cookies["data"]["Token"] = oClsResponse.Data.User.Token;
                //Response.Cookies["data"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
                Response.Cookies["data"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
                Response.Cookies["data"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
                Response.Cookies["data"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
                Response.Cookies["data"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
                //if (obj.Browser.ToLower() == "safari")
                //{
                //    Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencyCode + " ");
                //}
                //else
                //{
                Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
                //}
                Response.Cookies["data"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
                Response.Cookies["data"]["IsDemo"] = "False";
                Response.Cookies["data"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
                Response.Cookies["data"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
                Response.Cookies["data"]["WhitelabelBusinessIcon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessIcon);
                Response.Cookies["data"]["WhitelabelFavicon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelFavicon);
                Response.Cookies["data"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DateFormat);
                Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.TimeFormat);
                Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CurrencySymbolPlacement);                
                Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["SystemSetting"]["ShowHelpText"] = Convert.ToString(oClsResponse.Data.BusinessSetting.ShowHelpText);
                Response.Cookies["SystemSetting"]["EnableDarkMode"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableDarkMode);
                Response.Cookies["SystemSetting"]["FixedHeader"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedHeader);
                Response.Cookies["SystemSetting"]["FixedFooter"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedFooter);
                if (obj.Browser.ToLower() == "safari")
                {
                    Response.Cookies["SystemSetting"]["EnableSound"] = "False";
                }
                else
                {
                    Response.Cookies["SystemSetting"]["EnableSound"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableSound);
                }
                Response.Cookies["SystemSetting"]["CollapseSidebar"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CollapseSidebar);
                Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(days);

                foreach (var item in oClsResponse.Data.ShortCutKeySettings)
                {
                    Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
                }
                Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["ItemSetting"]["ExpiryDateFormat"] = Convert.ToString(oClsResponse.Data.ItemSetting.ExpiryDateFormat);
                Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(days);

            }
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Logout(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["LoginDetailsId"];
                arr[1] = Request.Cookies["data"]["Id"];
                obj.LoginDetailsId = Convert.ToInt64(arr[0]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.AddedBy = Convert.ToInt64(arr[1]);
            }

            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.Logout(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            Response.Cookies["data"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(-1);
            Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(-1);
            //return Redirect("/login");
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

    }
}