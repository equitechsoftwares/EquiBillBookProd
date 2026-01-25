using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        // GET: AdminSettings
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> profileupdate()
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            // Note: User/User endpoint - checking if there's a direct method or if we need to keep PostMethod
            // For now, using PostMethod as the exact method name in UserController is unclear
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/User", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            ClsDomainVm domainObj = new ClsDomainVm();
            domainObj.Domain = obj.Domain;
            domainObj.AddedBy = obj.AddedBy;
            domainObj.CompanyId = obj.CompanyId;
            WebApi.DomainController domainController = new WebApi.DomainController();
            var result1 = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.UserType = oClsResponse1.UserType;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ProfileUpdate(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.ProfileUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public ActionResult changepassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.LoginDetailsId = Convert.ToInt64(Request.Cookies["adata"]["LoginDetailsId"]);
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.ChangePassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ChangeLoginEmail()
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            // Note: User/User endpoint - keeping PostMethod as exact method name is unclear
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/User", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            ViewBag.User = oClsResponse.Data.User;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ValidateLoginEmail(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.ValidateLoginEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateLoginEmail(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = arr[0];
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UpdateLoginEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> businesssettings()
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            obj.PaymentType = "all";
            
            // Convert ClsUserVm to ClsBusinessSettingsVm for BusinessSetting call
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm();
            businessSettingsObj.CompanyId = obj.CompanyId;
            businessSettingsObj.AddedBy = obj.AddedBy;
            businessSettingsObj.Domain = obj.Domain;

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var result = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ClsSmsSettingsVm smsObj = new ClsSmsSettingsVm();
            smsObj.CompanyId = obj.CompanyId;
            smsObj.AddedBy = obj.AddedBy;
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result13 = await smsSettingsController.AllSmsSettings(smsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            ClsEmailSettingsVm emailObj = new ClsEmailSettingsVm();
            emailObj.CompanyId = obj.CompanyId;
            emailObj.AddedBy = obj.AddedBy;
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result14 = await emailSettingsController.AllEmailSettings(emailObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            ClsWhatsappSettingsVm whatsappObj = new ClsWhatsappSettingsVm();
            whatsappObj.CompanyId = obj.CompanyId;
            whatsappObj.AddedBy = obj.AddedBy;
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result15 = await whatsappSettingsController.AllWhatsappSettings(whatsappObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(result15);

            // Note: TimeZone controller - keeping PostMethod as exact method name needs verification
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res16 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TimeZone/ActiveTimeZones", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse16 = serializer.Deserialize<ClsResponse>(res16);

            // Note: UserCurrencyMap controller - keeping PostMethod as exact method name needs verification
            var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ClsOnlinePaymentSettingsVm onlinePaymentObj = new ClsOnlinePaymentSettingsVm();
            onlinePaymentObj.CompanyId = obj.CompanyId;
            onlinePaymentObj.AddedBy = obj.AddedBy;
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result18 = await onlinePaymentSettingsController.AllOnlinePaymentSettings(onlinePaymentObj);
            ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(result18);

            // Note: UserCurrencyMap/MainCurrency - keeping PostMethod as exact method name needs verification
            var res26 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/MainCurrency", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse26 = serializer.Deserialize<ClsResponse>(res26);

            // Note: Currency controller - keeping PostMethod as exact method name needs verification
            var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Currency/ActiveCurrencys", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm();
            countryObj.CompanyId = obj.CompanyId;
            countryObj.AddedBy = obj.AddedBy;
            WebApi.CountryController countryController = new WebApi.CountryController();
            var result20 = await countryController.AllCountrysMapped(countryObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            ClsBranchVm branchObj = new ClsBranchVm();
            branchObj.CompanyId = obj.CompanyId;
            branchObj.AddedBy = obj.AddedBy;
            branchObj.Domain= obj.Domain;
            WebApi.BranchController branchController = new WebApi.BranchController();
            var result21 = await branchController.BranchAdmin(branchObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            //var res58 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PwaSettings/PwaSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse58 = serializer.Deserialize<ClsResponse>(res58);

            ClsPwaSettingsVm pwaObj = new ClsPwaSettingsVm();
            pwaObj.CompanyId = obj.CompanyId;
            pwaObj.AddedBy = obj.AddedBy;
            WebApi.PwaSettingsController pwaSettingsController = new WebApi.PwaSettingsController();
            var result59 = await pwaSettingsController.AllPwaSettings(pwaObj);
            ClsResponse oClsResponse59 = await oCommonController.ExtractResponseFromActionResult(result59);

            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.SmsSettings = oClsResponse13.Data.SmsSettings;
            ViewBag.EmailSettings = oClsResponse14.Data.EmailSettings;
            ViewBag.WhatsappSettings = oClsResponse15.Data.WhatsappSettings;
            ViewBag.TimeZones = oClsResponse16.Data.TimeZones;
            ViewBag.OnlinePaymentSettings = oClsResponse18.Data.OnlinePaymentSettings;
            ViewBag.Country = oClsResponse.Data.Country;
            ViewBag.Currencys = oClsResponse17.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Countrys = oClsResponse20.Data.Countrys;
            ViewBag.Branch = oClsResponse21.Data.Branch;
            ViewBag.States = oClsResponse21.Data.States;
            ViewBag.Citys = oClsResponse21.Data.Citys;
            //ViewBag.PwaSetting = oClsResponse58.Data.PwaSetting;
            ViewBag.PwaSettings = oClsResponse59.Data.PwaSettings;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> businesssettings(ClsBusinessSettingsVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var result = await businessSettingsController.BusinessSettingsUpdateAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = Convert.ToString(obj.DefaultProfitPercent);
            Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(obj.DateFormat);
            Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(obj.TimeFormat);
            Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(obj.CurrencySymbolPlacement);
            Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(365);

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> SystemUpdate(ClsBusinessSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var result = await businessSettingsController.SystemUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            Response.Cookies["aSystemSetting"]["ShowHelpText"] = Convert.ToString(obj.ShowHelpText);
            Response.Cookies["aSystemSetting"]["EnableDarkMode"] = Convert.ToString(obj.EnableDarkMode);
            Response.Cookies["aSystemSetting"]["FixedHeader"] = Convert.ToString(obj.FixedHeader);
            Response.Cookies["aSystemSetting"]["FixedFooter"] = Convert.ToString(obj.FixedFooter);
            Response.Cookies["aSystemSetting"]["EnableSound"] = Convert.ToString(obj.EnableSound);
            Response.Cookies["aSystemSetting"].Expires = DateTime.Today.AddDays(365);

            return Json(oClsResponse);
        }

        //[HttpPost]
        //public async Task<ActionResult> PwaSettingsUpdate(ClsPwaSettingsVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["adata"] != null)
        //    {
        //        arr[0] = Request.Cookies["adata"]["UserType"];
        //        arr[1] = Request.Cookies["adata"]["Token"];
        //        arr[2] = Request.Cookies["adata"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
        //        obj.IpAddress = Request.UserHostAddress;
        //        obj.Browser = Request.Browser.Browser;
        //        obj.Platform = "Web";
        //        obj.Domain = Request.Url.Host.Replace("www.", "");
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PwaSettings/PwaSettingsUpdate", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
        //    return Json(oClsResponse);
        //}

        [HttpPost]
        public async Task<ActionResult> PwaSettingsFetch(ClsPwaSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.PwaSettingsController pwaSettingsController = new WebApi.PwaSettingsController();
            var result = await pwaSettingsController.AllPwaSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.PwaSettings = oClsResponse.Data.PwaSettings;

            return PartialView("PartialPwaSettings");
        }
        [HttpPost]
        public async Task<ActionResult> PwaSetting(ClsPwaSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.PwaSettingsController pwaSettingsController = new WebApi.PwaSettingsController();
            var result = await pwaSettingsController.PwaSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ViewBag.PwaSetting = oClsResponse.Data.PwaSetting;
            //ViewBag.Currencys = oClsResponse17.Data.Currencys;
            return PartialView("PartialPwaSettingsEdit");
        }
        [HttpPost]
        public async Task<ActionResult> UpdatePwaSettings(ClsPwaSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.PwaSettingsController pwaSettingsController = new WebApi.PwaSettingsController();
            var result = await pwaSettingsController.PwaSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> EmailSettingsFetch(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result = await emailSettingsController.AllEmailSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.EmailSettings = oClsResponse.Data.EmailSettings;

            return PartialView("PartialEmailSettings");
        }
        [HttpPost]
        public async Task<ActionResult> EmailSetting(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result = await emailSettingsController.EmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ViewBag.EmailSetting = oClsResponse.Data.EmailSetting;
            //ViewBag.Currencys = oClsResponse17.Data.Currencys;
            return PartialView("PartialEmailSettingsEdit");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateEmailSettings(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result = await emailSettingsController.UpdateEmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> InsertEmailSettings(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result = await emailSettingsController.InsertEmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> EmailSettingsDelete(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var result = await emailSettingsController.EmailSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> SmsSettingsFetch(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result = await smsSettingsController.AllSmsSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.SmsSettings = oClsResponse.Data.SmsSettings;

            return PartialView("PartialSmsSettings");
        }
        [HttpPost]
        public async Task<ActionResult> SmsSetting(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result = await smsSettingsController.SmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ViewBag.SmsSetting = oClsResponse.Data.SmsSetting;
            //ViewBag.Currencys = oClsResponse17.Data.Currencys;
            return PartialView("PartialSmsSettingsEdit");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateSmsSettings(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result = await smsSettingsController.UpdateSmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> InsertSmsSettings(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result = await smsSettingsController.InsertSmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SmsSettingsDelete(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var result = await smsSettingsController.SmsSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> WhatsappSettingsFetch(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result = await whatsappSettingsController.AllWhatsappSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.WhatsappSettings = oClsResponse.Data.WhatsappSettings;

            return PartialView("PartialWhatsappSettings");
        }
        [HttpPost]
        public async Task<ActionResult> WhatsappSetting(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result = await whatsappSettingsController.WhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ViewBag.WhatsappSetting = oClsResponse.Data.WhatsappSetting;
            //ViewBag.Currencys = oClsResponse17.Data.Currencys;
            return PartialView("PartialWhatsappSettingsEdit");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateWhatsappSettings(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result = await whatsappSettingsController.UpdateWhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> InsertWhatsappSettings(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result = await whatsappSettingsController.InsertWhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WhatsappSettingsDelete(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var result = await whatsappSettingsController.WhatsappSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> EmailModulesUpdate(ClsNotificationModulesVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            // Note: NotificationModulesSettingsUpdate method not found in NotificationModulesController
            // Keeping PostMethod as exact method name needs verification
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "NotificationModules/NotificationModulesSettingsUpdate", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> sendTestEmail(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var result = await notificationModulesController.SendTestEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        //[HttpPost]
        //public async Task<ActionResult> SmsSettingsUpdate(ClsSmsSettingsVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["adata"] != null)
        //    {
        //        arr[0] = Request.Cookies["adata"]["UserType"];
        //        arr[1] = Request.Cookies["adata"]["Token"];
        //        arr[2] = Request.Cookies["adata"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
        //        obj.IpAddress = Request.UserHostAddress;
        //        obj.Browser = Request.Browser.Browser;
        //        obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/SmsSettingsUpdate", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
        //    return Json(oClsResponse);
        //}
        [HttpPost]
        public async Task<ActionResult> sendTestSms(ClsSmsSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var result = await notificationModulesController.SendTestSms(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        //[HttpPost]
        //public async Task<ActionResult> WhatsappSettingsUpdate(ClsWhatsappSettingsVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["adata"] != null)
        //    {
        //        arr[0] = Request.Cookies["adata"]["UserType"];
        //        arr[1] = Request.Cookies["adata"]["Token"];
        //        arr[2] = Request.Cookies["adata"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
        //        obj.IpAddress = Request.UserHostAddress;
        //        obj.Browser = Request.Browser.Browser;
        //        obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/WhatsappSettingsUpdate", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
        //    return Json(oClsResponse);
        //}
        [HttpPost]
        public async Task<ActionResult> sendTestWhatsapp(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var result = await notificationModulesController.SendTestWhatsapp(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> OnlinePaymentSettingsFetch(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result = await onlinePaymentSettingsController.AllOnlinePaymentSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.OnlinePaymentSettings = oClsResponse.Data.OnlinePaymentSettings;

            return PartialView("PartialOnlinePaymentSettings");
        }
        [HttpPost]
        public async Task<ActionResult> OnlinePaymentSetting(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result = await onlinePaymentSettingsController.OnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            // Note: UserCurrencyMap/ActiveCurrencys - keeping PostMethod as exact method name needs verification
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserCurrencyMap/ActiveCurrencys", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ViewBag.OnlinePaymentSetting = oClsResponse.Data.OnlinePaymentSetting;
            ViewBag.Currencys = oClsResponse17.Data.Currencys;
            return PartialView("PartialOnlinePaymentSettingsEdit");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateOnlinePaymentSettings(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result = await onlinePaymentSettingsController.UpdateOnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> InsertOnlinePaymentSettings(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result = await onlinePaymentSettingsController.InsertOnlinePaymentSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OnlinePaymentSettingsDelete(ClsOnlinePaymentSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var result = await onlinePaymentSettingsController.OnlinePaymentSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> TimeZone(ClsTimeZoneVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TimeZoneController timeZoneController = new WebApi.TimeZoneController();
            var result = await timeZoneController.TimeZone(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> CheckTime(ClsBusinessSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var result = await businessSettingsController.CheckTime(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCountryMapped(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.CountryController countryController = new WebApi.CountryController();
            var result = await countryController.UpdateCountryMapped(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateBranch(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            var result = await branchController.UpdateBranchAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveStates(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            ClsStateVm stateObj = new ClsStateVm();
            stateObj.CompanyId = obj.CompanyId;
            stateObj.AddedBy = obj.AddedBy;
            stateObj.CountryId = obj.CountryId;
            WebApi.StateController stateController = new WebApi.StateController();
            var result = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.States = oClsResponse.Data.States;

            return PartialView("PartialStatesDropdown");
        }
        public async Task<ActionResult> ActiveCitys(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.CityController cityController = new WebApi.CityController();
            var result = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Citys = oClsResponse.Data.Citys;

            return PartialView("PartialCitysDropdown");
        }
        public async Task<ActionResult> stateInsert(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.StateController stateController = new WebApi.StateController();
            var result = await stateController.InsertState(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> cityInsert(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.CityController cityController = new WebApi.CityController();
            var result = await cityController.InsertCity(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        #region Currency User Mapping
        public async Task<ActionResult> CurrencyMappingList(ClsUserCurrencyMapVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            ClsCurrencyVm currencyObj = new ClsCurrencyVm();
            currencyObj.CompanyId = obj.CompanyId;
            currencyObj.AddedBy = obj.AddedBy;
            currencyObj.PageIndex = obj.PageIndex;
            currencyObj.PageSize = obj.PageSize;
            
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.AllCurrencys(currencyObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            
            // Note: Menu/ControlsPermission - keeping PostMethod as exact method name needs verification
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var result2 = await currencyController.AllCurrencys(currencyObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(result2);

            ViewBag.Currencys = oClsResponse.Data.Currencys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.CurrencysDropdown = oClsResponse2.Data.Currencys;
            return View();
        }

        public async Task<ActionResult> CurrencyMappingFetch(ClsCurrencyVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.AllCurrencys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            
            // Note: Menu/ControlsPermission - keeping PostMethod as exact method name needs verification
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.Currencys = oClsResponse.Data.Currencys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            return PartialView("PartialCurrencyMapping");
        }
        public async Task<ActionResult> CurrencyMappingEdit(long UserCurrencyMapId)
        {
            ClsUserCurrencyMapVm obj = new ClsUserCurrencyMapVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.UserCurrencyMapId = UserCurrencyMapId;
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.Currency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Currency/ActiveCurrencys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            ViewBag.Currency = oClsResponse.Data.Currency;
            //ViewBag.Currencys = oClsResponse2.Data.Currencys;
            return View();
        }
        public async Task<ActionResult> CurrencyMappingAdd()
        {
            ClsUserCurrencyMapVm obj = new ClsUserCurrencyMapVm();

            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            ClsCurrencyVm currencyObj = new ClsCurrencyVm();
            currencyObj.CompanyId = obj.CompanyId;
            currencyObj.AddedBy = obj.AddedBy;
            
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var result2 = await currencyController.AllCurrencys(currencyObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(result2);

            ViewBag.Currency = oClsResponse.Data.Currency;
            ViewBag.Currencys = oClsResponse2.Data.Currencys;
            return View();
        }
        public async Task<ActionResult> CurrencyMappingInsert(ClsUserCurrencyMapVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.InsertCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingUpdate(ClsUserCurrencyMapVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.UpdateCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingActiveInactive(ClsUserCurrencyMapVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.CurrencyActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingDelete(ClsUserCurrencyMapVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var result = await userCurrencyMapController.CurrencyDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        #endregion

        #region Reseller Payment Method
        public async Task<ActionResult> ResellerPaymentMethod(long? UserId)
        {
            ClsResellerPaymentMethodVm obj = new ClsResellerPaymentMethodVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                if(UserId == null)
                {
                    obj.UserId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                    ViewBag.IsShow = true;
                }
                else
                {
                    obj.UserId = Convert.ToInt64(UserId);
                    ViewBag.IsShow = false;
                }
            }
            WebApi.ResellerPaymentMethodController resellerPaymentMethodController = new WebApi.ResellerPaymentMethodController();
            var result = await resellerPaymentMethodController.ResellerPaymentMethod(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.ResellerPaymentMethod = oClsResponse.Data.ResellerPaymentMethod;

            return View();
        }

        public async Task<ActionResult> ResellerPaymentMethodUpdate(ClsResellerPaymentMethodVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.ResellerPaymentMethodController resellerPaymentMethodController = new WebApi.ResellerPaymentMethodController();
            var result = await resellerPaymentMethodController.UpdateResellerPaymentMethod(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        #endregion

        #region SEO Settings Management

        // GET: AdminSettings/SeoSettings
        public async Task<ActionResult> SeoSettings()
        {
            ClsPageSeoSettingsVm obj = new ClsPageSeoSettingsVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            WebApi.PageSeoSettingsController pageSeoSettingsController = new WebApi.PageSeoSettingsController();
            var result = await pageSeoSettingsController.AllPageSeoSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.SeoSettings = oClsResponse.Data.PageSeoSettingsList;

            // Predefined page identifiers
            var pageIdentifiers = new List<string>
            {
                "login", "register", "forgotpassword"
            };

            ViewBag.PageIdentifiers = pageIdentifiers;
            
            return View();
        }

        // GET: AdminSettings/SeoSettingsAdd
        public ActionResult SeoSettingsAdd()
        {
            // Predefined page identifiers
            var pageIdentifiers = new List<string>
            {
                "login", "register", "forgotpassword"
            };
            
            ViewBag.PageIdentifiers = pageIdentifiers;
            
            return View();
        }

        // GET: AdminSettings/SeoSettingsEdit/{id}
        public async Task<ActionResult> SeoSettingsEdit(long id)
        {
            ClsPageSeoSettingsVm obj = new ClsPageSeoSettingsVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageSeoSettingsId = id;
            }
            WebApi.PageSeoSettingsController pageSeoSettingsController = new WebApi.PageSeoSettingsController();
            var result = await pageSeoSettingsController.PageSeoSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Data == null || oClsResponse.Data.PageSeoSettings == null)
            {
                return HttpNotFound();
            }

            // Convert to ClsPageSeoSettings for view
            // JavaScriptSerializer deserializes anonymous types into dynamic objects
            ClsPageSeoSettingsVm apiData = oClsResponse.Data.PageSeoSettings;
            
            ClsPageSeoSettings seoSettings = new ClsPageSeoSettings
            {
                PageSeoSettingsId = apiData.PageSeoSettingsId,
                PageIdentifier = apiData.PageIdentifier ?? "",
                PageTitle = apiData.PageTitle ?? "",
                MetaDescription = apiData.MetaDescription ?? "",
                MetaKeywords = apiData.MetaKeywords ?? "",
                OgTitle = apiData.OgTitle ?? "",
                OgDescription = apiData.OgDescription ?? "",
                OgImage = apiData.OgImage ?? "",
                OgUrl = apiData.OgUrl ?? "",
                TwitterTitle = apiData.TwitterTitle ?? "",
                TwitterDescription = apiData.TwitterDescription ?? "",
                TwitterImage = apiData.TwitterImage ?? "",
                CanonicalUrl = apiData.CanonicalUrl ?? "",
                IsActive = apiData.IsActive
            };

            // Predefined page identifiers
            var pageIdentifiers = new List<string>
            {
                "login", "register", "forgotpassword"
            };
            
            ViewBag.PageIdentifiers = pageIdentifiers;
            ViewBag.SeoSettings = seoSettings;
            
            return View();
        }

        // POST: AdminSettings/SeoSettingsSave
        [HttpPost]
        public async Task<ActionResult> SeoSettingsSave(ClsPageSeoSettingsVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
            }
            WebApi.PageSeoSettingsController pageSeoSettingsController = new WebApi.PageSeoSettingsController();
            System.Web.Http.IHttpActionResult result;
            if (obj.PageSeoSettingsId > 0)
            {
                result = await pageSeoSettingsController.UpdatePageSeoSettings(obj);
            }
            else
            {
                result = await pageSeoSettingsController.InsertPageSeoSettings(obj);
            }
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            
            return Json(oClsResponse);
        }

        // POST: AdminSettings/SeoSettingsDelete
        [HttpPost]
        public async Task<ActionResult> SeoSettingsDelete(ClsPageSeoSettingsVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
            }
            WebApi.PageSeoSettingsController pageSeoSettingsController = new WebApi.PageSeoSettingsController();
            var result = await pageSeoSettingsController.PageSeoSettingsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            
            return Json(oClsResponse);
        }

        #endregion

    }
}