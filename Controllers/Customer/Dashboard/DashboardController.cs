using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class DashboardController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Dashboard

        public async Task<ActionResult> Index(ClsUserVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.ExpiryDate = DateTime.Now.AddDays(-1);

            // Initialize API controllers
            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            MenuController menuController = new MenuController();
            BranchController branchController = new BranchController();
            UserController userController = new UserController();

            // Call API methods directly
            var dashboardResult = await dashboardController.Dashboard(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(dashboardResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            //var res26 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ShortCutKeySetting/ActiveShortCutKeySettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse26 = serializer.Deserialize<ClsResponse>(res26);

            var rootCompanyResult = await userController.FetchRootCompany(obj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(rootCompanyResult);

            var whitelabelResult = await userController.FetchWhitelabelByDomain(obj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(whitelabelResult);

            ViewBag.Dashboard = oClsResponse.Data.Dashboard;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.Transaction = oClsResponse.Data.Transaction;

            ViewBag.UpgradePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade plan").FirstOrDefault();

            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PosPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "pos").FirstOrDefault();
            ViewBag.SupplierPaymentReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.CustomerPaymentReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.IncomePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "income").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.StockExpiryReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock expiry report").FirstOrDefault();
            ViewBag.StockAlertReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock alert report").FirstOrDefault();

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();

            ViewBag.ExpiredItemsReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expired items report").FirstOrDefault();
            ViewBag.StockAlertReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock alert report").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            //ViewBag.ShortCutKeySettings = oClsResponse26.Data.ShortCutKeySettings;

            if (oClsResponse21.Data.User.UserId == oClsResponse22.Data.User.UserId)
            {
                ViewBag.ShowDemoAccount = true;
            }
            else
            {
                ViewBag.ShowDemoAccount = false;
            }

            ViewBag.BranchId = obj.BranchId;
            ViewBag.Timeout = 0;
            return View();
        }

        public async Task<ActionResult> DashboardFetch(ClsUserVm obj)
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
            obj.ExpiryDate = DateTime.Now.AddDays(-1);

            // Initialize API controllers
            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            MenuController menuController = new MenuController();
            BranchController branchController = new BranchController();
            ShortCutKeySettingController shortCutKeySettingController = new ShortCutKeySettingController();

            // Call API methods directly
            var dashboardResult = await dashboardController.Dashboard(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(dashboardResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsShortCutKeySettingVm shortCutObj = new ClsShortCutKeySettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var shortCutResult = await shortCutKeySettingController.ActiveShortCutKeySettings(shortCutObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(shortCutResult);

            ViewBag.Dashboard = oClsResponse.Data.Dashboard;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.Transaction = oClsResponse.Data.Transaction;

            ViewBag.UpgradePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade plan").FirstOrDefault();

            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PosPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "pos").FirstOrDefault();
            ViewBag.SupplierPaymentReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.CustomerPaymentReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.IncomePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "income").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.StockExpiryReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock expiry report").FirstOrDefault();
            ViewBag.StockAlertReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock alert report").FirstOrDefault();

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();

            ViewBag.ExpiredItemsReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expired items report").FirstOrDefault();
            ViewBag.StockAlertReportPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock alert report").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.ShortCutKeySettings = oClsResponse26.Data.ShortCutKeySettings;

            ViewBag.BranchId = obj.BranchId;
            ViewBag.Timeout = 500;
            return PartialView("PartialDashboard");
        }

        public async Task<ActionResult> FetchTopCustomersGraph(ClsUserVm obj)
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

            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            var result = await dashboardController.TopCustomersGraph(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> FetchTopItemsGraph(ClsUserVm obj)
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

            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            var result = await dashboardController.TopItemsGraph(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesMonthWiseGraph(ClsUserVm obj)
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

            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            var result = await dashboardController.SalesMonthWiseGraph(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseMonthWiseGraph(ClsUserVm obj)
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

            WebApi.DashboardController dashboardController = new WebApi.DashboardController();
            var result = await dashboardController.PurchaseMonthWiseGraph(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<PartialViewResult> MenuPermissions()
        {
            ClsMenuVm obj = new ClsMenuVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            // Initialize API controllers
            MenuController menuController = new MenuController();
            ItemSettingsController itemSettingsController = new ItemSettingsController();
            SaleSettingsController saleSettingsController = new SaleSettingsController();
            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            PurchaseSettingsController purchaseSettingsController = new PurchaseSettingsController();
            ExpenseSettingsController expenseSettingsController = new ExpenseSettingsController();

            // Call API methods directly
            var menuResult = await menuController.MenuPermissions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId };
            var purchaseSettingsResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingsResult);

            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { CompanyId = obj.CompanyId };
            var expenseSettingsResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(expenseSettingsResult);

            ViewBag.Menus = oClsResponse.Data.Menus;
            ViewBag.User = oClsResponse.Data.User;
            ViewBag.ItemSetting = oClsResponse1.Data.ItemSetting;
            ViewBag.SaleSetting = oClsResponse2.Data.SaleSetting;

            ViewBag.WhitelabelType = oClsResponse.WhitelabelType;
            ViewBag.BusinessSetting = oClsResponse3.Data.BusinessSetting;
            ViewBag.PurchaseSetting = oClsResponse4.Data.PurchaseSetting;
            ViewBag.ExpenseSetting = oClsResponse5.Data.ExpenseSetting;

            return PartialView("~/Views/Shared/PartialMenus.cshtml");
        }

        public async Task<PartialViewResult> Footer()
        {
            ClsDomainVm obj = new ClsDomainVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            DomainController domainController = new DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(domainResult);
            ViewBag.WhitelabelType = oClsResponse.WhitelabelType;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;

            return PartialView("~/Views/Shared/PartialFooter.cshtml");
        }

    }
}