using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class SupplierPaymentReportsController : Controller
    {
        // GET: SupplierPaymentReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> PaymentsPaid()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> PaymentsPaidFetch(ClsAccountVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> TimeToPay()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> TimeToPayFetch(ClsAccountVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> DebitNoteDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> DebitNoteDetailsFetch(ClsAccountVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> RefundHistory()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> RefundHistoryFetch(ClsAccountVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
    }
}