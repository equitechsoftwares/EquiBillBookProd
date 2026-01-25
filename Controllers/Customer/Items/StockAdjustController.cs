using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
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
    [AuthorizationPrivilegeFilter]
    public class StockAdjustController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: stockadjust
        public async Task<ActionResult> index(long? BranchId)
        {
            ClsStockAdjustmentVm obj = new ClsStockAdjustmentVm();
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
                //obj.Title = "Stock Adjust";
            }
            if (BranchId != null)
            {
                obj.BranchId = Convert.ToInt64(BranchId);
                ViewBag.BranchId = obj.BranchId;
            }
            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.AllStockAdjustments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.StockAdjustments = oClsResponse.Data.StockAdjustments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalQuantity = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalQuantity);
            ViewBag.TotalItems = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalAmount);
            ViewBag.TotalAmountRecovered = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalAmountRecovered);

            return View();
        }
        public async Task<ActionResult> StockAdjustmentFetch(ClsStockAdjustmentVm obj)
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
                //obj.Title = "Stock Adjust";
            }
            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.AllStockAdjustments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);


            ViewBag.StockAdjustments = oClsResponse.Data.StockAdjustments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            //ViewBag.TotalQuantity = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalQuantity);
            ViewBag.TotalItems = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalAmount);
            ViewBag.TotalAmountRecovered = oClsResponse.Data.StockAdjustments.Sum(x => x.TotalAmountRecovered);

            return PartialView("PartialStockAdjustment");
        }
        public async Task<ActionResult> Edit(long StockAdjustmentId)
        {
            ClsStockAdjustmentVm obj = new ClsStockAdjustmentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";
                obj.StockAdjustmentId = StockAdjustmentId;
            }
            ClsStockAdjustment stockAdjustmentObj = new ClsStockAdjustment { StockAdjustmentId = obj.StockAdjustmentId, CompanyId = obj.CompanyId };
            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.StockAdjustment(stockAdjustmentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            StockAdjustmentReasonController stockAdjustmentReasonController = new StockAdjustmentReasonController();
            ClsStockAdjustmentReasonVm stockAdjustmentReasonObj = new ClsStockAdjustmentReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockAdjustmentReasonResult = await stockAdjustmentReasonController.ActiveStockAdjustmentReasons(stockAdjustmentReasonObj);
            ClsResponse oClsResponse69 = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonResult);

            ViewBag.StockAdjustment = oClsResponse.Data.StockAdjustment;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockAdjustmentReasons = oClsResponse69.Data.StockAdjustmentReasons;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.StockAdjustmentReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjustment reasons").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> Add()
        {
            ClsStockAdjustmentVm obj = new ClsStockAdjustmentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";
            }
            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            StockAdjustmentReasonController stockAdjustmentReasonController = new StockAdjustmentReasonController();
            ClsStockAdjustmentReasonVm stockAdjustmentReasonObj = new ClsStockAdjustmentReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockAdjustmentReasonResult = await stockAdjustmentReasonController.ActiveStockAdjustmentReasons(stockAdjustmentReasonObj);
            ClsResponse oClsResponse69 = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockAdjustmentReasons = oClsResponse69.Data.StockAdjustmentReasons;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.StockAdjustmentReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjustment reasons").FirstOrDefault();

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> StockAdjustmentInsert(ClsStockAdjustmentVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.InsertStockAdjustment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockAdjustmentUpdate(ClsStockAdjustmentVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.UpdateStockAdjustment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> StockAdjustmentdelete(ClsStockAdjustmentVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.StockAdjustmentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> StockAdjustmentDetailsDelete(ClsStockAdjustmentDetailsVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.StockAdjustmentDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> StockAdjustmentView(long StockAdjustmentId)
        {
            ClsStockAdjustmentVm obj = new ClsStockAdjustmentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";
                obj.StockAdjustmentId = StockAdjustmentId;
            }
            ClsStockAdjustment stockAdjustmentObj = new ClsStockAdjustment { StockAdjustmentId = obj.StockAdjustmentId, CompanyId = obj.CompanyId };
            StockAdjustmentController stockAdjustmentController = new StockAdjustmentController();
            var stockAdjustmentResult = await stockAdjustmentController.StockAdjustment(stockAdjustmentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var stockAdjustmentJournalResult = await stockAdjustmentController.StockAdjustmentJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentJournalResult);

            StockAdjustmentReasonController stockAdjustmentReasonController = new StockAdjustmentReasonController();
            ClsStockAdjustmentReasonVm stockAdjustmentReasonObj = new ClsStockAdjustmentReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockAdjustmentReasonResult = await stockAdjustmentReasonController.ActiveStockAdjustmentReasons(stockAdjustmentReasonObj);
            ClsResponse oClsResponse69 = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonResult);

            ViewBag.StockAdjustment = oClsResponse.Data.StockAdjustment;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockAdjustmentReasons = oClsResponse69.Data.StockAdjustmentReasons;

            ViewBag.BankPayments = oClsResponse36.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse36.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse36.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return PartialView("PartialStockAdjustmentView");
        }
        public async Task<ActionResult> StockAdjustmentImport()
        {
            ClsStockAdjustmentVm obj = new ClsStockAdjustmentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportStockAdjustment(ClsStockAdjustmentVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            // Note: ImportStockAdjustment method may not exist in StockAdjustmentController API
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "StockAdjustment/ImportStockAdjustment", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
    }
}