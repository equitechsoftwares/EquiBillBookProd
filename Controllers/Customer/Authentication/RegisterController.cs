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
    public class RegisterController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Register
        public async Task<ActionResult> Index()
        {
            // Load SEO settings from database
            SeoHelper.LoadSeoSettings(this, "register");

            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            CountryController countryController = new CountryController();
            DomainController domainController = new DomainController();
            CompetitorController competitorController = new CompetitorController();

            ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm { Domain = obj.Domain };
            var countryResult = await countryController.ActiveCountrysMapped(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TimeZone/ActiveTimeZones", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Currency/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            ClsCountryVm competitorObj = new ClsCountryVm { Domain = obj.Domain };
            var competitorResult = await competitorController.ActiveCompetitors(competitorObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(competitorResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Competitors = oClsResponse3.Data.Competitors;
            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
            //ViewBag.TimeZones = oClsResponse1.Data.TimeZones;
            //ViewBag.Currencys = oClsResponse2.Data.Currencys;

            if (oClsResponse1.WhitelabelType == 0)
            {
                return Redirect("/errorpage/domain");
            }
            else if (oClsResponse1.WhitelabelType == 2)
            {
                return Redirect("/login");
            }
            return View();
        }

        public async Task<ActionResult> RegisterOtp(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.RegisterOtp(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Register(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.Register(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Data.User != null)
            {
                //assign token to cookies
                Response.Cookies["data"]["Token"] = oClsResponse.Data.User.Token;
                //Response.Cookies["data"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
                Response.Cookies["data"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
                Response.Cookies["data"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
                Response.Cookies["data"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
                Response.Cookies["data"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
                Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
                Response.Cookies["data"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
                Response.Cookies["data"]["IsDemo"] = "False";
                Response.Cookies["data"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
                Response.Cookies["data"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
                Response.Cookies["data"]["WhitelabelBusinessIcon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessIcon);

                //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = "0";
                Response.Cookies["BusinessSetting"]["DateFormat"] = "dd/MM/yyyy";
                Response.Cookies["BusinessSetting"]["TimeFormat"] = "hh:mm tt";
                Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = "1";

                Response.Cookies["SystemSetting"]["ShowHelpText"] = "True";
                Response.Cookies["SystemSetting"]["EnableDarkMode"] = "False";
                Response.Cookies["SystemSetting"]["FixedHeader"] = "False";
                Response.Cookies["SystemSetting"]["FixedFooter"] = "False";
                Response.Cookies["SystemSetting"]["EnableSound"] = "True";
                Response.Cookies["SystemSetting"]["CollapseSidebar"] = "True";

                foreach (var item in oClsResponse.Data.ShortCutKeySettings)
                {
                    Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
                }
                //Response.Cookies["ShortCutKeySetting"]["AddNewForm"] = "shift+n";
                //Response.Cookies["ShortCutKeySetting"]["SaveForm"] = "shift+s";
                //Response.Cookies["ShortCutKeySetting"]["SaveAddAnother"] = "shift+a";
                //Response.Cookies["ShortCutKeySetting"]["UpdateForm"] = "shift+u";
                //Response.Cookies["ShortCutKeySetting"]["UpdateAddAnother"] = "shift+g";
                //Response.Cookies["ShortCutKeySetting"]["GoBack"] = "shift+o";

                Response.Cookies["ItemSetting"]["ExpiryDateFormat"] = Convert.ToString("MM/yyyy");
                
                Response.Cookies["data"].Expires = DateTime.Today.AddDays(365);
                Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(365);
                Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(365);
                Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(365);
                Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(365);
            }
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Country(ClsCountryVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            CountryController countryController = new CountryController();
            ClsCountry countryObj = new ClsCountry { CountryId = obj.CountryId };
            var result = await countryController.Country(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            oClsResponse.Data.Currencys = oClsResponse.Data.Country.Currencys.Where(a => a.CountryCurrencyMapId != 0).ToList();

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ActiveStates(ClsCityVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { CountryId = obj.CountryId };
            var result = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.States = oClsResponse.Data.States;

            return PartialView("PartialStatesDropdown");
        }

        public async Task<ActionResult> Reseller()
        {
            // SEO Meta Tags for Reseller Registration Page
            ViewBag.PageTitle = "Become a Reseller | EquiBillBook GST Billing Software | Partner Program";
            ViewBag.MetaDescription = "Join EquiBillBook's reseller program and earn 25% commission on GST billing software sales. Start your own software business with our proven billing solution. White-label options available. Partner with India's leading GST software company.";
            ViewBag.MetaKeywords = "EquiBillBook reseller, GST billing software reseller, software reseller program, billing software partnership, software commission program, white-label billing software, GST software reseller, software business opportunity India";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Become a Reseller - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Join our reseller program and earn 25% commission on GST billing software sales. White-label options available. Start your software business today!";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook Reseller Program - Earn 25% Commission";
            ViewBag.TwitterDescription = "Join EquiBillBook's reseller program. Earn 25% commission on GST billing software sales. White-label options available.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/register/reseller";

            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            CountryController countryController = new CountryController();
            DomainController domainController = new DomainController();
            CompetitorController competitorController = new CompetitorController();

            ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm { Domain = obj.Domain };
            var countryResult = await countryController.ActiveCountrysMapped(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ClsCountryVm competitorObj = new ClsCountryVm { Domain = obj.Domain };
            var competitorResult = await competitorController.ActiveCompetitors(competitorObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(competitorResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Competitors = oClsResponse3.Data.Competitors;
            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

            if (oClsResponse1.WhitelabelType == 0)
            {
                return Redirect("/errorpage/domain");
            }
            else if (oClsResponse1.WhitelabelType == 2)
            {
                return Redirect("/login");
            }
            return View();
        }

        public async Task<ActionResult> Whitelabel()
        {
            // SEO Meta Tags for White-Label Registration Page
            ViewBag.PageTitle = "White-Label Partnership | EquiBillBook GST Billing Software | Custom Branding";
            ViewBag.MetaDescription = "Start your own branded GST billing software business with EquiBillBook's white-label partnership. Earn 50% commission with complete customization, custom domain, and your own branding. Perfect for software companies and entrepreneurs.";
            ViewBag.MetaKeywords = "white-label GST billing software, custom billing software, white-label partnership, branded billing software, custom software solution, GST software white-label, software customization, branded accounting software India";
            
            // Open Graph Tags
            ViewBag.OgTitle = "White-Label Partnership - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Start your branded GST billing software business. Earn 50% commission with complete customization, custom domain, and your own branding.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook White-Label Partnership - 50% Commission";
            ViewBag.TwitterDescription = "Start your branded GST billing software business. Complete customization, custom domain, and your own branding. Earn 50% commission!";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/register/whitelabel";

            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            CountryController countryController = new CountryController();
            DomainController domainController = new DomainController();
            CompetitorController competitorController = new CompetitorController();

            ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm { Domain = obj.Domain };
            var countryResult = await countryController.ActiveCountrysMapped(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ClsCountryVm competitorObj = new ClsCountryVm { Domain = obj.Domain };
            var competitorResult = await competitorController.ActiveCompetitors(competitorObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(competitorResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Competitors = oClsResponse3.Data.Competitors;
            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;

            if (oClsResponse1.WhitelabelType == 0)
            {
                return Redirect("/errorpage/domain");
            }
            else if (oClsResponse1.WhitelabelType == 2)
            {
                return Redirect("/login");
            }
            return View();
        }

        public async Task<ActionResult> ResellerRegisterOtp(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.ResellerRegisterOtp(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ResellerRegister(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.UserType = "Reseller";
            obj.CommissionPercent = 25;
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.ResellerRegister(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> WhitelabelRegister(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.UserType = "Whitelabel Reseller";
            obj.CommissionPercent = 50;
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.ResellerRegister(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CheckUserName(ClsUserVm obj)
        {
            obj.IpAddress = Request.UserHostAddress;
            obj.Browser = Request.Browser.Browser;
            obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            serializer.MaxJsonLength = 2147483644;

            UserController userController = new UserController();
            var result = await userController.CheckUserName(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

    }
}