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
    public class OtherSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: OtherSettings
        #region payment term
        public async Task<ActionResult> PaymentTermlist(ClsPaymentTermVm obj)
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
            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var allPaymentTermsResult = await paymentTermController.AllPaymentTerms(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPaymentTermsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PaymentTerms = oClsResponse.Data.PaymentTerms;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> PaymentTermFetch(ClsPaymentTermVm obj)
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
            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var allPaymentTermsResult = await paymentTermController.AllPaymentTerms(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPaymentTermsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PaymentTerms = oClsResponse.Data.PaymentTerms;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            return PartialView("PartialPaymentTerm");
        }
        public async Task<ActionResult> PaymentTermEdit(long PaymentTermId)
        {
            ClsPaymentTermVm obj = new ClsPaymentTermVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PaymentTermId = PaymentTermId;
            }
            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTerm paymentTermObj = new ClsPaymentTerm { PaymentTermId = obj.PaymentTermId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.PaymentTerm(paymentTermObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            ViewBag.PaymentTerm = oClsResponse.Data.PaymentTerm;
            return View();
        }
        public ActionResult PaymentTermAdd()
        {
            return View();
        }
        public async Task<ActionResult> PaymentTermInsert(ClsPaymentTermVm obj)
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
            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var insertPaymentTermResult = await paymentTermController.InsertPaymentTerm(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertPaymentTermResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTermUpdate(ClsPaymentTermVm obj)
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

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var updatePaymentTermResult = await paymentTermController.UpdatePaymentTerm(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updatePaymentTermResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTermActiveInactive(ClsPaymentTermVm obj)
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

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var paymentTermActiveInactiveResult = await paymentTermController.PaymentTermActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTermActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTermDelete(ClsPaymentTermVm obj)
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

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var paymentTermDeleteResult = await paymentTermController.PaymentTermDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTermDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CalculateDueDate(ClsPaymentTermVm obj)
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

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            var calculateDueDateResult = await paymentTermController.CalculateDueDate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(calculateDueDateResult);

            return Json(oClsResponse);
        }

        #endregion

        #region Hsn/ Sac
        public async Task<ActionResult> ItemCode(ClsItemCodeVm obj)
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
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var allItemCodesResult = await itemCodeController.AllItemCodes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allItemCodesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> ItemCodeFetch(ClsItemCodeVm obj)
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
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var allItemCodesResult = await itemCodeController.AllItemCodes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allItemCodesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            return PartialView("PartialItemCode");
        }
        public async Task<ActionResult> ItemCodeEdit(long ItemCodeId)
        {
            ClsItemCodeVm obj = new ClsItemCodeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemCodeId = ItemCodeId;
                obj.TaxExemptionType = "Item";
            }
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCode itemCodeObj = new ClsItemCode { ItemCodeId = obj.ItemCodeId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ItemCode(itemCodeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(activeTaxExemptionsResult);

            ViewBag.ItemCode = oClsResponse.Data.ItemCode;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> ItemCodeAdd()
        {
            ClsItemCodeVm obj = new ClsItemCodeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TaxExemptionType = "Item";
            }
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCode itemCodeObj = new ClsItemCode { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ItemCode(itemCodeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(activeTaxExemptionsResult);

            ViewBag.ItemCode = oClsResponse.Data.ItemCode;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> ItemCodeInsert(ClsItemCodeVm obj)
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
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var insertItemCodeResult = await itemCodeController.InsertItemCode(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertItemCodeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemCodeUpdate(ClsItemCodeVm obj)
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

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var updateItemCodeResult = await itemCodeController.UpdateItemCode(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateItemCodeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemCodeActiveInactive(ClsItemCodeVm obj)
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

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var itemCodeActiveInactiveResult = await itemCodeController.ItemCodeActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemCodeDelete(ClsItemCodeVm obj)
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

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var itemCodeDeleteResult = await itemCodeController.ItemCodeDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveItemCodes(ClsItemCodeVm obj)
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

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            var activeItemCodesResult = await itemCodeController.ActiveItemCodes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeItemCodesResult);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            //ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();

            //return PartialView("PartialItemCodesDropdown");

            return Json(oClsResponse);
        }
        public async Task<ActionResult> fetchItemCodeTax(long ItemCodeId)
        {
            ClsItemCodeVm obj = new ClsItemCodeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemCodeId = ItemCodeId;
            }
            WebApi.ItemCodeMasterController itemCodeMasterController = new WebApi.ItemCodeMasterController();
            ClsItemCodeMasterVm itemCodeMasterObj = new ClsItemCodeMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Search = obj.Search };
            var itemCodeAutocompleteResult = await itemCodeMasterController.ItemCodeAutocomplete(itemCodeMasterObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeAutocompleteResult);

            return Json(oClsResponse);
        }        
        public async Task<ActionResult> ItemCodeAutocomplete(ClsItemCodeVm obj)
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
            WebApi.ItemCodeMasterController itemCodeMasterController = new WebApi.ItemCodeMasterController();
            ClsItemCodeMasterVm itemCodeMasterObj = new ClsItemCodeMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Search = obj.Search };
            var itemCodeAutocompleteResult = await itemCodeMasterController.ItemCodeAutocomplete(itemCodeMasterObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeAutocompleteResult);

            return Json(oClsResponse);
        }
        #endregion

        #region payment type
        public async Task<ActionResult> PaymentTypelist(ClsPaymentTypeVm obj)
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
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var allPaymentTypesResult = await paymentTypeController.AllPaymentTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPaymentTypesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PaymentTypes = oClsResponse.Data.PaymentTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> PaymentTypeFetch(ClsPaymentTypeVm obj)
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
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var allPaymentTypesResult = await paymentTypeController.AllPaymentTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPaymentTypesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PaymentTypes = oClsResponse.Data.PaymentTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            return PartialView("PartialPaymentType");
        }
        public async Task<ActionResult> PaymentTypeEdit(long PaymentTypeId)
        {
            ClsPaymentTypeVm obj = new ClsPaymentTypeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PaymentTypeId = PaymentTypeId;
            }
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentType paymentTypeObj = new ClsPaymentType { PaymentTypeId = obj.PaymentTypeId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.PaymentType(paymentTypeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ViewBag.PaymentType = oClsResponse.Data.PaymentType;
            return View();
        }
        public ActionResult PaymentTypeAdd()
        {
            return View();
        }
        public async Task<ActionResult> PaymentTypeInsert(ClsPaymentTypeVm obj)
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
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var insertPaymentTypeResult = await paymentTypeController.InsertPaymentType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertPaymentTypeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTypeUpdate(ClsPaymentTypeVm obj)
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

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var updatePaymentTypeResult = await paymentTypeController.UpdatePaymentType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updatePaymentTypeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTypeActiveInactive(ClsPaymentTypeVm obj)
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

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeActiveInactiveResult = await paymentTypeController.PaymentTypeActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTypeActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentTypeDelete(ClsPaymentTypeVm obj)
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

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeDeleteResult = await paymentTypeController.PaymentTypeDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentTypeDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> FetchAccountMapped(ClsBranchPaymentTypeMapVm obj)
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

            WebApi.BranchPaymentTypeMapController branchPaymentTypeMapController = new WebApi.BranchPaymentTypeMapController();
            var fetchAccountMappedResult = await branchPaymentTypeMapController.FetchAccountMapped(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchAccountMappedResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Additional Charge
        public async Task<ActionResult> AdditionalChargeList(ClsAdditionalChargeVm obj)
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
            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var allAdditionalChargesResult = await additionalChargeController.AllAdditionalCharges(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allAdditionalChargesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.AdditionalCharges = oClsResponse.Data.AdditionalCharges;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "additional charges").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> AdditionalChargeFetch(ClsAdditionalChargeVm obj)
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
            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var allAdditionalChargesResult = await additionalChargeController.AllAdditionalCharges(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allAdditionalChargesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.AdditionalCharges = oClsResponse.Data.AdditionalCharges;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "additional charges").FirstOrDefault();

            return PartialView("PartialAdditionalCharge");
        }
        public async Task<ActionResult> AdditionalChargeEdit(long AdditionalChargeId)
        {
            ClsAdditionalChargeVm obj = new ClsAdditionalChargeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.AdditionalChargeId = AdditionalChargeId;
            }
            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            ClsAdditionalCharge additionalChargeObj = new ClsAdditionalCharge { AdditionalChargeId = obj.AdditionalChargeId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var additionalChargeResult = await additionalChargeController.AdditionalCharge(additionalChargeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(additionalChargeResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeAccountsDropdownResult2 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult2);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeItemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(activeItemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(activeTaxExemptionsResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountTypesDropdownResult = await accountTypeController.ActiveAccountTypesDropdown(accountTypeObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(activeAccountTypesDropdownResult);

            ViewBag.AdditionalCharge = oClsResponse.Data.AdditionalCharge;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.AccountSubTypes = oClsResponse29.Data.AccountSubTypes;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.AccountPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "chart of accounts").FirstOrDefault();
            ViewBag.AccountTypes = oClsResponse54.Data.AccountTypes;

            return View();
        }
        public async Task<ActionResult> AdditionalChargeAdd()
        {
            ClsAdditionalChargeVm obj = new ClsAdditionalChargeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCode itemCodeObj = new ClsItemCode { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ItemCode(itemCodeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeAccountsDropdownResult2 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult2);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            ClsItemCodeVm itemCodeVmObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeItemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeVmObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(activeItemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(activeTaxExemptionsResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountTypesDropdownResult = await accountTypeController.ActiveAccountTypesDropdown(accountTypeObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(activeAccountTypesDropdownResult);

            ViewBag.ItemCode = oClsResponse.Data.ItemCode;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.AccountSubTypes = oClsResponse29.Data.AccountSubTypes;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.AccountPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "chart of accounts").FirstOrDefault();
            ViewBag.AccountTypes = oClsResponse54.Data.AccountTypes;

            return View();
        }
        public async Task<ActionResult> AdditionalChargeInsert(ClsAdditionalChargeVm obj)
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
            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var insertAdditionalChargeResult = await additionalChargeController.InsertAdditionalCharge(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertAdditionalChargeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AdditionalChargeUpdate(ClsAdditionalChargeVm obj)
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

            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var updateAdditionalChargeResult = await additionalChargeController.UpdateAdditionalCharge(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateAdditionalChargeResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AdditionalChargeActiveInactive(ClsAdditionalChargeVm obj)
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

            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var additionalChargeActiveInactiveResult = await additionalChargeController.AdditionalChargeActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(additionalChargeActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AdditionalChargeDelete(ClsAdditionalChargeVm obj)
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

            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var additionalChargeDeleteResult = await additionalChargeController.AdditionalChargeDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(additionalChargeDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveAdditionalCharges(ClsAdditionalChargeVm obj)
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

            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            var activeAdditionalChargesResult = await additionalChargeController.ActiveAdditionalCharges(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeAdditionalChargesResult);

            return Json(oClsResponse);
        }

        #endregion

        #region Prefix
        public async Task<ActionResult> PrefixList(ClsPrefixVm obj)
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
            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var allPrefixsResult = await prefixController.AllPrefixs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPrefixsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Prefixs = oClsResponse.Data.Prefixs;
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
        public async Task<ActionResult> PrefixFetch(ClsPrefixVm obj)
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
            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var allPrefixsResult = await prefixController.AllPrefixs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPrefixsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Prefixs = oClsResponse.Data.Prefixs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            return PartialView("PartialPrefix");
        }
        public async Task<ActionResult> PrefixEdit(long PrefixId)
        {
            ClsPrefixVm obj = new ClsPrefixVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PrefixId = PrefixId;
            }
            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            ClsPrefix prefixObj = new ClsPrefix { PrefixId = obj.PrefixId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var prefixResult = await prefixController.Prefix(prefixObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(prefixResult);

            ClsPrefixMasterVm prefixMasterObj = new ClsPrefixMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allPrefixMastersResult = await prefixController.AllPrefixMasters(prefixMasterObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(allPrefixMastersResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

            ViewBag.Prefix = oClsResponse.Data.Prefix;
            ViewBag.PrefixMasters = oClsResponse1.Data.PrefixMasters;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            return View();
        }
        public async Task<ActionResult> PrefixAdd()
        {
            ClsPrefixVm obj = new ClsPrefixVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            ClsPrefixMasterVm prefixMasterObj = new ClsPrefixMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allPrefixMastersResult = await prefixController.AllPrefixMasters(prefixMasterObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPrefixMastersResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PrefixMasters = oClsResponse.Data.PrefixMasters;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            return View();
        }
        public async Task<ActionResult> PrefixInsert(ClsPrefixVm obj)
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
            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var insertPrefixResult = await prefixController.InsertPrefix(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertPrefixResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PrefixUpdate(ClsPrefixVm obj)
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

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var updatePrefixResult = await prefixController.UpdatePrefix(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updatePrefixResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PrefixActiveInactive(ClsPrefixVm obj)
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

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var prefixActiveInactiveResult = await prefixController.PrefixActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(prefixActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PrefixDelete(ClsPrefixVm obj)
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

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            var prefixDeleteResult = await prefixController.PrefixDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(prefixDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Currency User Mapping
        public async Task<ActionResult> CurrencyMappingList(ClsUserCurrencyMapVm obj)
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
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize };
            var allCurrencysResult = await userCurrencyMapController.AllCurrencys(currencyMapObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCurrencysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

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
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, Search = obj.Search };
            var allCurrencysResult = await userCurrencyMapController.AllCurrencys(currencyMapObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCurrencysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

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
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserCurrencyMapId = UserCurrencyMapId;
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyResult = await userCurrencyMapController.Currency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(currencyResult);

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
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj1 = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await currencyController.ActiveCurrencys(currencyObj1);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            ViewBag.Currency = oClsResponse.Data.Currency;
            ViewBag.Currencys = oClsResponse2.Data.Currencys;
            return View();
        }
        public async Task<ActionResult> CurrencyMappingInsert(ClsUserCurrencyMapVm obj)
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
            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var insertCurrencyResult = await userCurrencyMapController.InsertCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertCurrencyResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingUpdate(ClsUserCurrencyMapVm obj)
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

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var updateCurrencyResult = await userCurrencyMapController.UpdateCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateCurrencyResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingActiveInactive(ClsUserCurrencyMapVm obj)
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

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyActiveInactiveResult = await userCurrencyMapController.CurrencyActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(currencyActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CurrencyMappingDelete(ClsUserCurrencyMapVm obj)
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

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyDeleteResult = await userCurrencyMapController.CurrencyDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(currencyDeleteResult);
            return Json(oClsResponse);
        }


        #endregion

        public async Task<ActionResult> CreditLimit()
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
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> CreditLimitUpdate(ClsBusinessSettingsVm obj)
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
            var creditLimitUpdateResult = await businessSettingsController.CreditLimitUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(creditLimitUpdateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ShortCutKeySettings()
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

            WebApi.ShortCutKeySettingController shortCutKeySettingController = new WebApi.ShortCutKeySettingController();
            ClsShortCutKeySettingVm shortCutKeyObj = new ClsShortCutKeySettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeShortCutKeySettingsResult = await shortCutKeySettingController.ActiveShortCutKeySettings(shortCutKeyObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(activeShortCutKeySettingsResult);
            return Json(oClsResponse10);
        }


        public async Task<ActionResult> ShortCutKeys()
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

            WebApi.ShortCutKeySettingController shortCutKeySettingController = new WebApi.ShortCutKeySettingController();
            ClsShortCutKeySettingVm shortCutKeyObj = new ClsShortCutKeySettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var shortCutKeySettingsResult = await shortCutKeySettingController.ShortCutKeySettings(shortCutKeyObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(shortCutKeySettingsResult);

            ViewBag.ShortCutKeySettings = oClsResponse10.Data.ShortCutKeySettings;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ShortCutKeySettingUpdate(ClsBusinessSettingsVm obj)
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
            WebApi.ShortCutKeySettingController shortCutKeySettingController = new WebApi.ShortCutKeySettingController();
            var shortCutKeySettingUpdateResult = await shortCutKeySettingController.ShortCutKeySettingUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(shortCutKeySettingUpdateResult);

            ClsShortCutKeySettingVm activeShortCutKeyObj = new ClsShortCutKeySettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeShortCutKeySettingsResult = await shortCutKeySettingController.ActiveShortCutKeySettings(activeShortCutKeyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(activeShortCutKeySettingsResult);

            foreach (var item in oClsResponse26.Data.ShortCutKeySettings)
            {
                Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
            }

            //Response.Cookies["ShortCutKeySetting"]["AddNewForm"] = Convert.ToString(obj.AddNewForm);
            //Response.Cookies["ShortCutKeySetting"]["SaveForm"] = Convert.ToString(obj.SaveForm);
            //Response.Cookies["ShortCutKeySetting"]["SaveAddAnother"] = Convert.ToString(obj.SaveAddAnother);
            //Response.Cookies["ShortCutKeySetting"]["UpdateForm"] = Convert.ToString(obj.UpdateForm);
            //Response.Cookies["ShortCutKeySetting"]["UpdateAddAnother"] = Convert.ToString(obj.UpdateAddAnother);
            //Response.Cookies["ShortCutKeySetting"]["GoBack"] = Convert.ToString(obj.GoBack);
            //Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(365);

            return Json(oClsResponse);
        }

    }
}