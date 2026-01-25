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
    public class PurchaseSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: PurchaseSettings
        #region Vehicle
        public async Task<ActionResult> Vehiclelist(ClsVehicleVm obj)
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
            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var allVehiclesResult = await vehicleController.AllVehicles(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allVehiclesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Vehicles = oClsResponse.Data.Vehicles;
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
        public async Task<ActionResult> VehicleFetch(ClsVehicleVm obj)
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
            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var allVehiclesResult = await vehicleController.AllVehicles(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allVehiclesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Vehicles = oClsResponse.Data.Vehicles;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            return PartialView("PartialVehicle");
        }
        public async Task<ActionResult> VehicleEdit(long VehicleId)
        {
            ClsVehicleVm obj = new ClsVehicleVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.VehicleId = VehicleId;
            }
            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            ClsVehicle vehicleObj = new ClsVehicle { VehicleId = obj.VehicleId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var vehicleResult = await vehicleController.Vehicle(vehicleObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(vehicleResult);

            WebApi.ExpenseSettingsController expenseSettingsController = new WebApi.ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var expenseSettingResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingResult);

            ViewBag.Vehicle = oClsResponse.Data.Vehicle;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            return View();
        }
        public async Task<ActionResult> VehicleAdd()
        {
            ClsVehicleVm obj = new ClsVehicleVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.ExpenseSettingsController expenseSettingsController = new WebApi.ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var expenseSettingResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingResult);

            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            return View();
        }
        public async Task<ActionResult> VehicleInsert(ClsVehicleVm obj)
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
            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var insertVehicleResult = await vehicleController.InsertVehicle(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertVehicleResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VehicleUpdate(ClsVehicleVm obj)
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

            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var updateVehicleResult = await vehicleController.UpdateVehicle(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateVehicleResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VehicleActiveInactive(ClsVehicleVm obj)
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

            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var vehicleActiveInactiveResult = await vehicleController.VehicleActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(vehicleActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VehicleDelete(ClsVehicleVm obj)
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

            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var vehicleDeleteResult = await vehicleController.VehicleDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(vehicleDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> MileageUpdate(ClsVehicleVm obj)
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

            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            var mileageUpdateResult = await vehicleController.MileageUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(mileageUpdateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> MileageRateFetch(ClsVehicleVm obj)
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

            WebApi.VehicleController vehicleController = new WebApi.VehicleController();
            ClsVehicle vehicleObj = new ClsVehicle { VehicleId = obj.VehicleId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var vehicleResult = await vehicleController.Vehicle(vehicleObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(vehicleResult);
            return Json(oClsResponse);
        }

        #endregion

        public async Task<ActionResult> PurchaseSettings()
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

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

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

            ViewBag.PurchaseSetting = oClsResponse7.Data.PurchaseSetting;
            ViewBag.AccountSubTypes = oClsResponse29.Data.AccountSubTypes;
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> PurchaseSettingsUpdate(ClsPurchaseSettingsVm obj)
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
            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsUpdateResult = await purchaseSettingsController.PurchaseSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(purchaseSettingsUpdateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ExpenseSettings()
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

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeUnitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(activeUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            WebApi.ExpenseSettingsController expenseSettingsController = new WebApi.ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var expenseSettingResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingResult);

            ViewBag.Units = oClsResponse8.Data.Units;
            ViewBag.AccountSubTypes = oClsResponse29.Data.AccountSubTypes;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> UpdateExpenseSettings(ClsExpenseSettingsVm obj)
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
            WebApi.ExpenseSettingsController expenseSettingsController = new WebApi.ExpenseSettingsController();
            var expenseSettingsUpdateResult = await expenseSettingsController.ExpenseSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseSettingsUpdateResult);
            return Json(oClsResponse);
        }

        #region Purchase Debit Note Reason
        public async Task<ActionResult> PurchaseDebitNoteReason(ClsPurchaseDebitNoteReasonVm obj)
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
                //obj.Title = "PurchaseDebitNoteReason";
            }

            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var allPurchaseDebitNoteReasonsResult = await purchaseDebitNoteReasonController.AllPurchaseDebitNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPurchaseDebitNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PurchaseDebitNoteReasons = oClsResponse.Data.PurchaseDebitNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase debit note reasons").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonFetch(ClsPurchaseDebitNoteReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "PurchaseDebitNoteReason";
            }
            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var allPurchaseDebitNoteReasonsResult = await purchaseDebitNoteReasonController.AllPurchaseDebitNoteReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allPurchaseDebitNoteReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.PurchaseDebitNoteReasons = oClsResponse.Data.PurchaseDebitNoteReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase debit note reasons").FirstOrDefault();

            return PartialView("PartialPurchaseDebitNoteReason");
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonEdit(long PurchaseDebitNoteReasonId)
        {
            ClsPurchaseDebitNoteReasonVm obj = new ClsPurchaseDebitNoteReasonVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PurchaseDebitNoteReasonId = PurchaseDebitNoteReasonId;
            }
            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            ClsPurchaseDebitNoteReason purchaseDebitNoteReasonObj = new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseDebitNoteReasonResult = await purchaseDebitNoteReasonController.PurchaseDebitNoteReason(purchaseDebitNoteReasonObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(purchaseDebitNoteReasonResult);

            ViewBag.PurchaseDebitNoteReason = oClsResponse.Data.PurchaseDebitNoteReason;
            return View();
        }
        public ActionResult PurchaseDebitNoteReasonAdd()
        {
            return View();
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonInsert(ClsPurchaseDebitNoteReasonVm obj)
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
            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var insertPurchaseDebitNoteReasonResult = await purchaseDebitNoteReasonController.InsertPurchaseDebitNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertPurchaseDebitNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonUpdate(ClsPurchaseDebitNoteReasonVm obj)
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

            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var updatePurchaseDebitNoteReasonResult = await purchaseDebitNoteReasonController.UpdatePurchaseDebitNoteReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updatePurchaseDebitNoteReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonActiveInactive(ClsPurchaseDebitNoteReasonVm obj)
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

            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonActiveInactiveResult = await purchaseDebitNoteReasonController.PurchaseDebitNoteReasonActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(purchaseDebitNoteReasonActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseDebitNoteReasonDelete(ClsPurchaseDebitNoteReasonVm obj)
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

            WebApi.PurchaseDebitNoteReasonController purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonDeleteResult = await purchaseDebitNoteReasonController.PurchaseDebitNoteReasonDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(purchaseDebitNoteReasonDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

    }
}