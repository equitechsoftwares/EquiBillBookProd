using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class SystemSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: SystemSettings
        public async Task<ActionResult> SystemSettings()
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            obj.PaymentType = "all";
            obj.UnitType = "all";

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SystemUpdate(ClsBusinessSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            var systemUpdateResult = await businessSettingsController.BusinessSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(systemUpdateResult);

            Response.Cookies["SystemSetting"]["ShowHelpText"] = Convert.ToString(obj.ShowHelpText);
            Response.Cookies["SystemSetting"]["EnableDarkMode"] = Convert.ToString(obj.EnableDarkMode);
            Response.Cookies["SystemSetting"]["FixedHeader"] = Convert.ToString(obj.FixedHeader);
            Response.Cookies["SystemSetting"]["FixedFooter"] = Convert.ToString(obj.FixedFooter);
            Response.Cookies["SystemSetting"]["EnableSound"] = Convert.ToString(obj.EnableSound);
            Response.Cookies["SystemSetting"]["CollapseSidebar"] = Convert.ToString(obj.CollapseSidebar);
            Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(365);

            return Json(oClsResponse);
        }

        #region Domains
        public async Task<ActionResult> Domains(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
            }

            WebApi.DomainController domainController = new WebApi.DomainController();
            var allDomainsResult = await domainController.AllDomains(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allDomainsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Domains= oClsResponse.Data.Domains;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "domains").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> DomainFetch(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Categories";
            }
            WebApi.DomainController domainController = new WebApi.DomainController();
            var allDomainsResult = await domainController.AllDomains(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allDomainsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Domains = oClsResponse.Data.Domains;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "domains").FirstOrDefault();

            return PartialView("PartialDomains");
        }
       
        public ActionResult DomainAdd()
        {
            return View();
        }
        public async Task<ActionResult> DomainInsert(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; 
                //obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.DomainController domainController = new WebApi.DomainController();
            var insertDomainResult = await domainController.InsertDomain(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertDomainResult);
            return Json(oClsResponse);
        }
        
        public async Task<ActionResult> DomainDelete(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.DomainController domainController = new WebApi.DomainController();
            var domainDeleteResult = await domainController.DomainDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(domainDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> VerifyDomainConnection(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                //obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.DomainController domainController = new WebApi.DomainController();
            var verifyDomainConnectionResult = await domainController.VerifyDomainConnection(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(verifyDomainConnectionResult);
            return Json(oClsResponse);
        }
        #endregion
    }
}