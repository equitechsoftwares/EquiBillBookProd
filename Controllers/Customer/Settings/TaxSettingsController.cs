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
    public class TaxSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: TaxSettings

        #region Tax Settings
        public async Task<ActionResult> TaxSettings(ClsTaxSettingVm obj)
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
                //obj.PageSize = 10;
                //obj.Title = "payment methods";
            }
            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var allTaxSettingsResult = await taxSettingController.AllTaxSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TaxSettings = oClsResponse.Data.TaxSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax settings").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TaxSettingFetch(ClsTaxSettingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "payment methods";
            }
            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var allTaxSettingsResult = await taxSettingController.AllTaxSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TaxSettings = oClsResponse.Data.TaxSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax settings").FirstOrDefault();

            return PartialView("PartialTaxSetting");
        }
        public async Task<ActionResult> TaxSettingEdit(long TaxSettingId)
        {
            ClsTaxSettingVm obj = new ClsTaxSettingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TaxSettingId = TaxSettingId;
            }
            obj.IsCompositionScheme = true;

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSetting taxSettingObj = new ClsTaxSetting { TaxSettingId = obj.TaxSettingId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.TaxSetting(taxSettingObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBusinessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(activeBusinessRegistrationNamesResult);

            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.TaxSetting = oClsResponse.Data.TaxSetting;
            return View();
        }
        public async Task<ActionResult> TaxSettingAdd()
        {
            ClsTaxSettingVm obj = new ClsTaxSettingVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.IsCompositionScheme = true;

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBusinessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(activeBusinessRegistrationNamesResult);

            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            return View();
        }
        public async Task<ActionResult> TaxSettingInsert(ClsTaxSettingVm obj)
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
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var insertTaxSettingResult = await taxSettingController.InsertTaxSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertTaxSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxSettingUpdate(ClsTaxSettingVm obj)
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

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var updateTaxSettingResult = await taxSettingController.UpdateTaxSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateTaxSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxSettingActiveInactive(ClsTaxSettingVm obj)
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

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var taxSettingActiveInactiveResult = await taxSettingController.TaxSettingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSettingActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxSettingDelete(ClsTaxSettingVm obj)
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

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var taxSettingDeleteResult = await taxSettingController.TaxSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSettingDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveTaxSettings(ClsTaxSettingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            var activeTaxSettingsResult = await taxSettingController.ActiveTaxSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeTaxSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.TaxSettingPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax Settings").FirstOrDefault();

            return PartialView("PartialTaxSettingsDropdown");
        }
        #endregion

        #region Tax Exemption
        public async Task<ActionResult> TaxExemptions(ClsTaxExemptionVm obj)
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
                //obj.PageSize = 10;
                //obj.Title = "payment methods";
            }
            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var allTaxExemptionsResult = await taxExemptionController.AllTaxExemptions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxExemptionsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TaxExemptions = oClsResponse.Data.TaxExemptions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TaxExemptionFetch(ClsTaxExemptionVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "payment methods";
            }
            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var allTaxExemptionsResult = await taxExemptionController.AllTaxExemptions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxExemptionsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TaxExemptions = oClsResponse.Data.TaxExemptions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            return PartialView("PartialTaxExemption");
        }
        public async Task<ActionResult> TaxExemptionEdit(long TaxExemptionId)
        {
            ClsTaxExemptionVm obj = new ClsTaxExemptionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TaxExemptionId = TaxExemptionId;
            }
            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemption taxExemptionObj = new ClsTaxExemption { TaxExemptionId = obj.TaxExemptionId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.TaxExemption(taxExemptionObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TaxExemption = oClsResponse.Data.TaxExemption;
            return View();
        }
        public ActionResult TaxExemptionAdd()
        {
            return View();
        }
        public async Task<ActionResult> TaxExemptionInsert(ClsTaxExemptionVm obj)
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
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var insertTaxExemptionResult = await taxExemptionController.InsertTaxExemption(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertTaxExemptionResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxExemptionUpdate(ClsTaxExemptionVm obj)
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

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var updateTaxExemptionResult = await taxExemptionController.UpdateTaxExemption(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateTaxExemptionResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxExemptionActiveInactive(ClsTaxExemptionVm obj)
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

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionActiveInactiveResult = await taxExemptionController.TaxExemptionActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxExemptionActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxExemptionDelete(ClsTaxExemptionVm obj)
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

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionDeleteResult = await taxExemptionController.TaxExemptionDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxExemptionDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveTaxExemptions(ClsTaxExemptionVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            var activeTaxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeTaxExemptionsResult);

            return Json(oClsResponse);
        }
        #endregion

        #region tax
        public async Task<ActionResult> Taxlist(ClsTaxVm obj)
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
                //obj.PageSize = 10;
                //obj.Title = "Tax List";
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            var allTaxsResult = await taxController.AllTaxs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TaxFetch(ClsTaxVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Tax List";
            }

            WebApi.TaxController taxController = new WebApi.TaxController();
            var allTaxsResult = await taxController.AllTaxs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();

            return PartialView("PartialTax");
        }
        public async Task<ActionResult> TaxEdit(long TaxId)
        {
            ClsTaxVm obj = new ClsTaxVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TaxId = TaxId;
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTax taxObj = new ClsTax { TaxId = obj.TaxId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.Tax(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            ViewBag.Tax = oClsResponse.Data.Tax;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            return View();
        }
        public async Task<ActionResult> TaxAdd()
        {
            ClsTaxVm obj = new ClsTaxVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            return View();
        }
        public async Task<ActionResult> TaxInsert(ClsTaxVm obj)
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
            WebApi.TaxController taxController = new WebApi.TaxController();
            var insertTaxResult = await taxController.InsertTax(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertTaxResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxUpdate(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var updateTaxResult = await taxController.UpdateTax(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateTaxResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxActiveInactive(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxActiveInactiveResult = await taxController.TaxActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxDelete(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxDeleteResult = await taxController.TaxDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

        #region tax group
        public async Task<ActionResult> TaxGroupList(ClsTaxVm obj)
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
                //obj.PageSize = 10;
                //obj.Title = "Tax Group";
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            var allTaxGroupsResult = await taxController.AllTaxGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TaxGroupFetch(ClsTaxVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Tax Group";
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            var allTaxGroupsResult = await taxController.AllTaxGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTaxGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            return PartialView("PartialTaxGroup");
        }
        public async Task<ActionResult> TaxGroupEdit(long TaxId)
        {
            ClsTaxVm obj = new ClsTaxVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TaxId = TaxId;
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTax taxObj = new ClsTax { TaxId = obj.TaxId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxGroupResult = await taxController.TaxGroup(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxGroupResult);

            var activeTaxsResult = await taxController.ActiveTaxs(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            ViewBag.Tax = oClsResponse.Data.Tax;

            ViewBag.Taxs = oClsResponse1.Data.Taxs;

            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxsForGroup = oClsResponse1.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            return View();
        }
        public async Task<ActionResult> TaxGroupAdd()
        {
            ClsTax obj = new ClsTax();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxVmObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxsResult = await taxController.ActiveTaxs(taxVmObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;

            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxsForGroup = oClsResponse.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            return View();
        }
        public async Task<ActionResult> TaxGroupInsert(ClsTaxVm obj)
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
            WebApi.TaxController taxController = new WebApi.TaxController();
            var insertTaxGroupResult = await taxController.InsertTaxGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertTaxGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxGroupUpdate(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var updateTaxGroupResult = await taxController.UpdateTaxGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateTaxGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxGroupActiveInactive(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxActiveInactiveResult = await taxController.TaxActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxGroupDelete(ClsTaxVm obj)
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

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxDeleteResult = await taxController.TaxDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> FetchTaxPercent(ClsTaxVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TaxController taxController = new WebApi.TaxController();
            var fetchTaxPercentResult = await taxController.FetchTaxPercent(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchTaxPercentResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveAllTaxs(ClsTaxVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TaxController taxController = new WebApi.TaxController();
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TaxBreakups(ClsSalesVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxBreakupsResult = await taxController.TaxBreakups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxBreakupsResult);
            return Json(oClsResponse);
        }

        #endregion

    }
}