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
    public class SalesSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: SalesSettings
        #region Customer Group
        public async Task<ActionResult> CustomerGroup(ClsUserGroupVm obj)
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
                //obj.Title = "Customer Group";
            }
            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var allUserGroupsResult = await userGroupController.AllUserGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUserGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.UserGroups = oClsResponse.Data.UserGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CustomerGroupFetch(ClsUserGroupVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Customer Group";
            }
            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var allUserGroupsResult = await userGroupController.AllUserGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUserGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.UserGroups = oClsResponse.Data.UserGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            return PartialView("PartialCustomerGroup");
        }
        public async Task<ActionResult> CustomerGroupEdit(long UserGroupId)
        {
            ClsUserGroupVm obj = new ClsUserGroupVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserGroupId = UserGroupId;
            }
            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroup userGroupObj = new ClsUserGroup { UserGroupId = obj.UserGroupId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.UserGroup(userGroupObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeSellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeSellingPriceGroupsResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.UserGroup = oClsResponse.Data.UserGroup;
            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CustomerGroupAdd()
        {
            ClsUserGroupVm obj = new ClsUserGroupVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeSellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeSellingPriceGroupsResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CustomerGroupInsert(ClsUserGroupVm obj)
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
            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var insertUserGroupResult = await userGroupController.InsertUserGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertUserGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CustomerGroupUpdate(ClsUserGroupVm obj)
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

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var updateUserGroupResult = await userGroupController.UpdateUserGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateUserGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CustomerGroupActiveInactive(ClsUserGroupVm obj)
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

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var userGroupActiveInactiveResult = await userGroupController.UserGroupActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userGroupActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CustomerGroupDelete(ClsUserGroupVm obj)
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

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            var userGroupDeleteResult = await userGroupController.UserGroupDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userGroupDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Sales Settings

        public async Task<ActionResult> SaleSettings()
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

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activePaymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(activePaymentTermsResult);

            ViewBag.SaleSetting = oClsResponse5.Data.SaleSetting;
            ViewBag.Taxs = oClsResponse9.Data.Taxs;
            ViewBag.AccountSubTypes = oClsResponse29.Data.AccountSubTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SaleSettingsUpdate(ClsSaleSettingsVm obj)
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
            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            var saleSettingsUpdateResult = await saleSettingsController.SaleSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleSettingsUpdateResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Pos Settings
        public async Task<ActionResult> PosSettings()
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

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(posSettingResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(itemSettingResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activePaymentTypesResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(activePaymentTypesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
            ClsAdditionalChargeVm additionalChargeObj = new ClsAdditionalChargeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAdditionalChargesResult = await additionalChargeController.ActiveAdditionalCharges(additionalChargeObj);
            ClsResponse oClsResponse75 = await oCommonController.ExtractResponseFromActionResult(activeAdditionalChargesResult);

            ViewBag.PosSetting = oClsResponse6.Data.PosSetting;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PaymentTypes = oClsResponse11.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> PosSettingsUpdate(ClsPosSettingsVm obj)
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
            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            var posSettingsUpdateResult = await posSettingsController.PosSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(posSettingsUpdateResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PosSetting()
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

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(posSettingResult);

            return Json(oClsResponse6);
        }
        #endregion

        #region Sales Credit Note Reason
        public async Task<ActionResult> SalesCreditNoteReason(ClsSalesCreditNoteReasonVm obj)
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
                //obj.Title = "SalesCreditNoteReason";
            }
            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var allSalesCreditNoteReasonsResult = await salesCreditNoteReasonController.AllSalesCreditNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesCreditNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesCreditNoteReasons = oClsResponse.Data.SalesCreditNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales credit note reasons").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SalesCreditNoteReasonFetch(ClsSalesCreditNoteReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "SalesCreditNoteReason";
            }

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var allSalesCreditNoteReasonsResult = await salesCreditNoteReasonController.AllSalesCreditNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesCreditNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesCreditNoteReasons = oClsResponse.Data.SalesCreditNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales credit note reasons").FirstOrDefault();

            return PartialView("PartialSalesCreditNoteReason");
        }
        public async Task<ActionResult> SalesCreditNoteReasonEdit(long SalesCreditNoteReasonId)
        {
            ClsSalesCreditNoteReasonVm obj = new ClsSalesCreditNoteReasonVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SalesCreditNoteReasonId = SalesCreditNoteReasonId;
            }
            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            ClsSalesCreditNoteReason salesCreditNoteReasonObj = new ClsSalesCreditNoteReason { SalesCreditNoteReasonId = obj.SalesCreditNoteReasonId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesCreditNoteReasonResult = await salesCreditNoteReasonController.SalesCreditNoteReason(salesCreditNoteReasonObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonResult);

            ViewBag.SalesCreditNoteReason = oClsResponse.Data.SalesCreditNoteReason;
            return View();
        }
        public ActionResult SalesCreditNoteReasonAdd()
        {
            return View();
        }
        public async Task<ActionResult> SalesCreditNoteReasonInsert(ClsSalesCreditNoteReasonVm obj)
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
            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var insertSalesCreditNoteReasonResult = await salesCreditNoteReasonController.InsertSalesCreditNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSalesCreditNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesCreditNoteReasonUpdate(ClsSalesCreditNoteReasonVm obj)
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

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var updateSalesCreditNoteReasonResult = await salesCreditNoteReasonController.UpdateSalesCreditNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSalesCreditNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesCreditNoteReasonActiveInactive(ClsSalesCreditNoteReasonVm obj)
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

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var salesCreditNoteReasonActiveInactiveResult = await salesCreditNoteReasonController.SalesCreditNoteReasonActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesCreditNoteReasonDelete(ClsSalesCreditNoteReasonVm obj)
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

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            var salesCreditNoteReasonDeleteResult = await salesCreditNoteReasonController.SalesCreditNoteReasonDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Sales Debit Note Reason
        public async Task<ActionResult> SalesDebitNoteReason(ClsSalesDebitNoteReasonVm obj)
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
                //obj.Title = "SalesDebitNoteReason";
            }
            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var allSalesDebitNoteReasonsResult = await salesDebitNoteReasonController.AllSalesDebitNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesDebitNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesDebitNoteReasons = oClsResponse.Data.SalesDebitNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales debit note reasons").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SalesDebitNoteReasonFetch(ClsSalesDebitNoteReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "SalesDebitNoteReason";
            }

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var allSalesDebitNoteReasonsResult = await salesDebitNoteReasonController.AllSalesDebitNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesDebitNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesDebitNoteReasons = oClsResponse.Data.SalesDebitNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales debit note reasons").FirstOrDefault();

            return PartialView("PartialSalesDebitNoteReason");
        }
        public async Task<ActionResult> SalesDebitNoteReasonEdit(long SalesDebitNoteReasonId)
        {
            ClsSalesDebitNoteReasonVm obj = new ClsSalesDebitNoteReasonVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SalesDebitNoteReasonId = SalesDebitNoteReasonId;
            }
            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            ClsSalesDebitNoteReason salesDebitNoteReasonObj = new ClsSalesDebitNoteReason { SalesDebitNoteReasonId = obj.SalesDebitNoteReasonId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesDebitNoteReasonResult = await salesDebitNoteReasonController.SalesDebitNoteReason(salesDebitNoteReasonObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonResult);

            ViewBag.SalesDebitNoteReason = oClsResponse.Data.SalesDebitNoteReason;
            return View();
        }
        public ActionResult SalesDebitNoteReasonAdd()
        {
            return View();
        }
        public async Task<ActionResult> SalesDebitNoteReasonInsert(ClsSalesDebitNoteReasonVm obj)
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
            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var insertSalesDebitNoteReasonResult = await salesDebitNoteReasonController.InsertSalesDebitNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSalesDebitNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesDebitNoteReasonUpdate(ClsSalesDebitNoteReasonVm obj)
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

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var updateSalesDebitNoteReasonResult = await salesDebitNoteReasonController.UpdateSalesDebitNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSalesDebitNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesDebitNoteReasonActiveInactive(ClsSalesDebitNoteReasonVm obj)
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

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var salesDebitNoteReasonActiveInactiveResult = await salesDebitNoteReasonController.SalesDebitNoteReasonActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesDebitNoteReasonDelete(ClsSalesDebitNoteReasonVm obj)
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

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            var salesDebitNoteReasonDeleteResult = await salesDebitNoteReasonController.SalesDebitNoteReasonDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonDeleteResult);
            return Json(oClsResponse);
        }
        #endregion
    }
}