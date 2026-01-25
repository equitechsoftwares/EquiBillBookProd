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
    public class AccountsReportsController : Controller
    {
        // GET: AccountsReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> BankTransactions(ClsAccountVm obj)
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

            //obj.AccountId = Convert.ToInt64(accountId);
            ViewBag.AccountId = obj.AccountId;
            ViewBag.AccountName = obj.AccountName;
            ViewBag.Type = obj.Type;

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { AccountId = obj.AccountId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, Type = obj.Type };
            var result = await accountController.BankTransactions(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccountsDropdown", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.Page = obj.Page;
            return View();
        }
        public async Task<ActionResult> BankTransactionsFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { AccountId = obj.AccountId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, Type = obj.Type };
            var result = await accountController.BankTransactions(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.User = oClsResponse.Data.User;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payments").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialBankTransactions");
        }
        public async Task<ActionResult> AccountTransactions(ClsAccountVm obj)
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

            //obj.AccountId = Convert.ToInt64(accountId);
            ViewBag.AccountId = obj.AccountId;
            ViewBag.AccountName = obj.AccountName;
            ViewBag.Type = obj.Type;

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { AccountId = obj.AccountId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, Type = obj.Type, AccountSubTypeId = obj.AccountSubTypeId };
            var result = await accountController.AccountTransactions(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccountsDropdown", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.Page = obj.Page;
            ViewBag.SubPage = obj.SubPage;
            ViewBag.AccountSubTypeId = obj.AccountSubTypeId;
            return View();
        }
        public async Task<ActionResult> AccountTransactionsFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { AccountId = obj.AccountId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, Type = obj.Type, AccountSubTypeId = obj.AccountSubTypeId };
            var result = await accountController.AccountTransactions(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.User = oClsResponse.Data.User;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payments").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.Page = obj.Page;
            ViewBag.SubPage = obj.SubPage;
            ViewBag.AccountSubTypeId = obj.AccountSubTypeId;

            return PartialView("PartialAccountTransactions");
        }
        public async Task<ActionResult> AccountTypeSummary(ClsAccountVm obj)
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

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.AccountTypeSummary(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type summary").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> AccountTypeSummaryFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.AccountTypeSummary(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payments").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialAccountTypeSummary");
        }
        public async Task<ActionResult> GeneralLedger(ClsAccountVm obj)
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

                //if(id != null)
                //{
                //    obj.AccountSubTypeId = Convert.ToInt64(id);
                //}
            }

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, AccountSubTypeId = obj.AccountSubTypeId };
            var result = await accountController.GeneralLedger(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Accounts = oClsResponse.Data.Accounts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "general ledger").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            ViewBag.Page = obj.Page;
            ViewBag.AccountSubTypeId = obj.AccountSubTypeId;

            return View();
        }
        public async Task<ActionResult> GeneralLedgerFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate, AccountSubTypeId = obj.AccountSubTypeId };
            var result = await accountController.GeneralLedger(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Accounts = oClsResponse.Data.Accounts;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "general ledger").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            ViewBag.Page = obj.Page;
            ViewBag.AccountSubTypeId = obj.AccountSubTypeId;

            return PartialView("PartialGeneralLedger");
        }
        public async Task<ActionResult> DayBook()
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

            obj.FromDate = DateTime.Now.Date;
            obj.ToDate = DateTime.Now.Date;

            ViewBag.FromDate = obj.FromDate;
            ViewBag.Todate = obj.ToDate;

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.AccountTransactions(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var accountController2 = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result11 = await accountController2.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "day book").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> TrialBalance()
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

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.TrialBalance(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "trial balance").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> TrialBalanceFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.TrialBalance(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "trial balance").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialTrialBalance");
        }
        public async Task<ActionResult> SundryDebtors()
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

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.SundryDebtors(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Receivable").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SundryDebtorsFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.SundryDebtors(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Users = oClsResponse.Data.Users;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Receivable").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSundryDebtors");
        }
        public async Task<ActionResult> SundryDebtorDetails(ClsBankPaymentVm obj)
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

            var accountController = new WebApi.AccountController();
            var result = await accountController.SundryDebtorDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Receivable").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.CustomerName = obj.CustomerName;

            return View();
        }
        public async Task<ActionResult> SundryDebtorDetailsFetch(ClsBankPaymentVm obj)
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
            var accountController = new WebApi.AccountController();
            var result = await accountController.SundryDebtorDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Receivable").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();
            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSundryDebtorDetails");
        }
        public async Task<ActionResult> SundryCreditors()
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

            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.SundryCreditors(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Payable").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SundryCreditorsFetch(ClsAccountVm obj)
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
            var accountController = new WebApi.AccountController();
            var bankPaymentObj = new ClsBankPaymentVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await accountController.SundryCreditors(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Users = oClsResponse.Data.Users;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Payable").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSundryCreditors");
        }
        public async Task<ActionResult> SundryCreditorDetails(ClsBankPaymentVm obj)
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

            var accountController = new WebApi.AccountController();
            var result = await accountController.SundryCreditorDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Payable").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.SupplierName = obj.SupplierName;

            return View();
        }
        public async Task<ActionResult> SundryCreditorDetailsFetch(ClsBankPaymentVm obj)
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
            var accountController = new WebApi.AccountController();
            var result = await accountController.SundryCreditorDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "Accounts Payable").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();
            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSundryCreditorDetails");
        }
        public async Task<ActionResult> JournalReport()
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

            var taxController = new WebApi.TaxController();
            var salesObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await taxController.B2B(salesObj);
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
        public async Task<ActionResult> JournalReportFetch(ClsAccountVm obj)
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

            var taxController = new WebApi.TaxController();
            var salesObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await taxController.B2B(salesObj);
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