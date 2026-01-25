using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Website
{
    public class HomeController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Home
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Home Page
            ViewBag.PageTitle = "India's #1 GST Billing Software | Free 30-Day Trial | EquiBillBook";
            ViewBag.MetaDescription = "India's leading GST billing software for small businesses. Create invoices, manage inventory, handle GST compliance. Free 30-day trial!";
            ViewBag.MetaKeywords = "GST billing software India, cloud accounting software India, POS billing software, multi-location accounting, CA firm software, best billing software 2024, invoice software India, inventory management software, GST compliance software, small business accounting, free GST billing software trial, EquiBillBook, GST invoice generator, business management software India, affordable accounting software India";
            
            // Open Graph Tags
            ViewBag.OgTitle = "India's #1 GST Billing Software | Free Trial | EquiBillBook";
            ViewBag.OgDescription = "Transform your business with India's most trusted GST billing software. Create invoices, manage inventory, ensure GST compliance. Join 2000+ businesses. Start free trial!";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/banner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "India's #1 GST Billing Software | EquiBillBook";
            ViewBag.TwitterDescription = "Complete GST billing & inventory solution for Indian businesses. Free 30-day trial, no credit card required. Join 2000+ businesses today!";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/banner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/";

            obj.IpAddress = Request.UserHostAddress;
            obj.Domain = Request.Url.Host.Replace("www.", "");

            //obj.Under = oCommonController.Under;

            //var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WebsiteViewer/InsertWebsiteViewer", "", "", "");            
            //ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                return View();
            }
            else
            {
                if (oClsResponse1.WhitelabelType == 0)
                {
                    return Redirect("/errorpage/domain");
                }
                else if (oClsResponse1.WhitelabelType == 2 || oClsResponse1.WhitelabelType == 1)
                {
                    return Redirect("/login");
                }
            }


            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        //[HttpGet]
        //public async Task<ActionResult> Demo(ClsUserVm obj)
        //{
        //    obj.IpAddress = Request.UserHostAddress;
        //    obj.Browser = Request.Browser.Browser;
        //    obj.Platform = "Web";
        //    obj.Domain = Request.Url.Host.Replace("www.", "");
        //    obj.UserType = "user";
        //    obj.EmailId = "demo@equibillbook.com";
        //    obj.Password = "1234";
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/Login", "", "", "");

        //    serializer.MaxJsonLength = 2147483644;
        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

        //    if (oClsResponse.Data.User != null)
        //    {

        //        int days = 0;
        //        if (obj.IsRememberMe == true)
        //        {
        //            days = 365;
        //        }
        //        else
        //        {
        //            days = 1;
        //        }

        //        Response.Cookies["data"]["Token"] = oClsResponse.Data.User.Token;
        //        //Response.Cookies["data"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
        //        Response.Cookies["data"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
        //        Response.Cookies["data"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
        //        Response.Cookies["data"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
        //        Response.Cookies["data"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
        //        //if (obj.Browser.ToLower() == "safari")
        //        //{
        //        //    Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencyCode + " ");
        //        //}
        //        //else
        //        //{
        //        Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
        //        //}
        //        Response.Cookies["data"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
        //        Response.Cookies["data"]["IsDemo"] = "False";
        //        Response.Cookies["data"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
        //        Response.Cookies["data"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
        //        Response.Cookies["data"].Expires = DateTime.Today.AddDays(days);


        //        //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DefaultProfitPercent);
        //        Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DateFormat);
        //        Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.TimeFormat);
        //        Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CurrencySymbolPlacement);
        //        Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(days);

        //        Response.Cookies["SystemSetting"]["ShowHelpText"] = Convert.ToString(oClsResponse.Data.BusinessSetting.ShowHelpText);
        //        Response.Cookies["SystemSetting"]["EnableDarkMode"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableDarkMode);
        //        Response.Cookies["SystemSetting"]["FixedHeader"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedHeader);
        //        Response.Cookies["SystemSetting"]["FixedFooter"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedFooter);
        //        if (obj.Browser.ToLower() == "safari")
        //        {
        //            Response.Cookies["SystemSetting"]["EnableSound"] = "False";
        //        }
        //        else
        //        {
        //            Response.Cookies["SystemSetting"]["EnableSound"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableSound);
        //        }
        //        Response.Cookies["SystemSetting"]["CollapseSidebar"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CollapseSidebar);
        //        Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(days);

        //        foreach (var item in oClsResponse.Data.ShortCutKeySettings)
        //        {
        //            Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
        //        }
        //        //Response.Cookies["ShortCutKeySetting"]["AddNewForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.AddNewForm);
        //        //Response.Cookies["ShortCutKeySetting"]["SaveForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveForm);
        //        //Response.Cookies["ShortCutKeySetting"]["SaveAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveAddAnother);
        //        //Response.Cookies["ShortCutKeySetting"]["UpdateForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateForm);
        //        //Response.Cookies["ShortCutKeySetting"]["UpdateAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateAddAnother);
        //        //Response.Cookies["ShortCutKeySetting"]["GoBack"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.GoBack);
        //        Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(days);

        //        Response.Cookies["ItemSetting"]["ExpiryDateFormat"] = Convert.ToString(oClsResponse.Data.ItemSetting.ExpiryDateFormat);
        //        Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(days);

        //    }
        //    return Redirect("/dashboard?type=login");
        //}
    }
}