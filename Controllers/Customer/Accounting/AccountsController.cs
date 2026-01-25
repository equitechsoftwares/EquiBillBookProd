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
using System.Text;
using System.IO;
using ClosedXML.Excel;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class AccountsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Accounts

        public async Task<ActionResult> ActiveAccountSubTypes(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountSubTypeController accountSubTypeController = new AccountSubTypeController();
            ClsAccountSubTypeVm accountSubTypeObj = new ClsAccountSubTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var result = await accountSubTypeController.ActiveAccountSubTypes(accountSubTypeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //ViewBag.AccountSubTypes = oClsResponse.Data.AccountSubTypes;

            //return PartialView("PartialAccountSubTypeDropdown");
            return Json(oClsResponse);
        }
        //public async Task<ActionResult> AccountBook(long AccountId)
        //{
        //    ClsBankPaymentVm obj = new ClsBankPaymentVm();
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["data"] != null)
        //    {
        //        arr[0] = Request.Cookies["data"]["UserType"];
        //        arr[1] = Request.Cookies["data"]["Token"];
        //        arr[2] = Request.Cookies["data"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
        //        obj.PageIndex = 1;
        //        //obj.PageSize = 10;
        //        //obj.Title = "Accounts";
        //        obj.AccountId = AccountId;
        //        obj.Type = "account book";
        //        //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/CashFlowReport", arr[0], arr[1], arr[2]);
        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

        //    var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/Account", arr[0], arr[1], arr[2]);
        //    ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

        //    var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
        //    ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

        //    ViewBag.BankPayments = oClsResponse.Data.BankPayments;
        //    ViewBag.TotalCount = oClsResponse.Data.TotalCount;
        //    ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
        //    ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

        //    ViewBag.Account = oClsResponse1.Data.Account;

        //    ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
        //    ViewBag.CurrentPageIndex = obj.PageIndex;
        //    ViewBag.PageSize = oClsResponse.Data.PageSize;
        //    ViewBag.PageIndex = obj.PageIndex;
        //    ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();

        //    ViewBag.FromDate = oClsResponse.Data.FromDate;
        //    ViewBag.ToDate = oClsResponse.Data.ToDate;

        //    ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Sum(x => x.Credit);
        //    ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Sum(x => x.Debit);
        //    return View();
        //}

        //public async Task<ActionResult> AccountBookFetch(ClsCustomerPaymentVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["data"] != null)
        //    {
        //        arr[0] = Request.Cookies["data"]["UserType"];
        //        arr[1] = Request.Cookies["data"]["Token"];
        //        arr[2] = Request.Cookies["data"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
        //        obj.PageIndex = 1;
        //        //obj.PageSize = 10;
        //        obj.Title = "Accounts";
        //        obj.Type = "account book";
        //        //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/CashFlowReport", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

        //    var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
        //    ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

        //    ViewBag.BankPayments = oClsResponse.Data.BankPayments;
        //    ViewBag.TotalCount = oClsResponse.Data.TotalCount;
        //    ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
        //    ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

        //    ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
        //    ViewBag.CurrentPageIndex = obj.PageIndex;
        //    ViewBag.PageSize = oClsResponse.Data.PageSize;
        //    ViewBag.PageIndex = obj.PageIndex;

        //    ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Sum(x => x.Credit);
        //    ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Sum(x => x.Debit);

        //    return PartialView("PartialAccountBook");
        //}

        //public async Task<ActionResult> ActiveOtherAccounts(ClsAccountVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["data"] != null)
        //    {
        //        arr[0] = Request.Cookies["data"]["UserType"];
        //        arr[1] = Request.Cookies["data"]["Token"];
        //        arr[2] = Request.Cookies["data"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveOtherAccounts", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
        //    return Json(oClsResponse);
        //}

        //public async Task<ActionResult> paymentInsert(ClsBankPaymentVm obj)
        //{
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["data"] != null)
        //    {
        //        arr[0] = Request.Cookies["data"]["UserType"];
        //        arr[1] = Request.Cookies["data"]["Token"];
        //        arr[2] = Request.Cookies["data"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
        //        obj.IpAddress = Request.UserHostAddress;
        //        obj.Browser = Request.Browser.Browser;
        //        obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);
        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BankPayment/InsertPayment", arr[0], arr[1], arr[2]);

        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
        //    return Json(oClsResponse);
        //}

        //public async Task<ActionResult> paymentDelete(long BankPaymentId)
        //{
        //    ClsBankPaymentVm obj = new ClsBankPaymentVm();
        //    string[] arr = { "", "", "" };
        //    if (Request.Cookies["data"] != null)
        //    {
        //        arr[0] = Request.Cookies["data"]["UserType"];
        //        arr[1] = Request.Cookies["data"]["Token"];
        //        arr[2] = Request.Cookies["data"]["Id"];
        //        obj.AddedBy = Convert.ToInt64(arr[2]);
        //        obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
        //        obj.BankPaymentId = BankPaymentId;
        //        obj.IpAddress = Request.UserHostAddress;
        //        obj.Browser = Request.Browser.Browser;
        //        obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
        //    }
        //    serializer.MaxJsonLength = 2147483644;
        //    string _json = serializer.Serialize(obj);

        //    var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BankPayment/PaymentDelete", arr[0], arr[1], arr[2]);
        //    ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

        //    return Json(oClsResponse);
        //}

        //#endregion

        #region Chart of Accounts
        public async Task<ActionResult> ChartOfAccount(ClsAccountVm obj)
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
                //obj.Title = "Accounts";
            }
            serializer.MaxJsonLength = 2147483644;
            obj.PaymentType = "all";
            obj.Type = "Chart Of Account";

            // Initialize API controllers
            AccountController accountController = new AccountController();
            MenuController menuController = new MenuController();
            PaymentTypeController paymentTypeController = new PaymentTypeController();
            AccountTypeController accountTypeController = new AccountTypeController();

            // Call API methods directly
            var accountResult = await accountController.AllAccounts(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ViewBag.Accounts = oClsResponse.Data.Accounts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;


            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            //ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "chart of accounts").FirstOrDefault();
            ViewBag.AccountsTransactionsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> ChartOfAccountFetch(ClsAccountVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Accounts";
            }
            obj.Type = "Chart Of Account";
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            MenuController menuController = new MenuController();

            var accountResult = await accountController.AllAccounts(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.Accounts = oClsResponse.Data.Accounts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "chart of accounts").FirstOrDefault();
            ViewBag.AccountsTransactionsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            return PartialView("PartialChartOfAccount");
        }
        public async Task<ActionResult> ChartOfAccountAdd()
        {
            ClsAccount obj = new ClsAccount();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            AccountTypeController accountTypeController = new AccountTypeController();
            MenuController menuController = new MenuController();

            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypesDropdown(accountTypeObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> ChartOfAccountInsert(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            var result = await accountController.InsertChartOfAccount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ChartOfAccountEdit(long AccountId)
        {
            ClsAccount obj = new ClsAccount();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.AccountId = AccountId;
            }
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            AccountTypeController accountTypeController = new AccountTypeController();
            MenuController menuController = new MenuController();

            var accountResult = await accountController.Account(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypesDropdown(accountTypeObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Account = oClsResponse.Data.Account;
            ViewBag.AccountSubTypes = oClsResponse.Data.AccountSubTypes;
            ViewBag.AccountTypes = oClsResponse1.Data.AccountTypes;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> ChartOfAccountUpdate(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            var result = await accountController.UpdateChartOfAccount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AccountActiveInactive(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            var result = await accountController.AccountActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AccountDelete(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            var result = await accountController.AccountDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveAccountsDropdown(ClsAccountVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var result = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse11, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AccountView(long AccountId)
        {
            ClsAccount obj = new ClsAccount();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.AccountId = AccountId;
            }
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            AccountTypeController accountTypeController = new AccountTypeController();
            MenuController menuController = new MenuController();

            var accountResult = await accountController.Account(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypesDropdown(accountTypeObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Account = oClsResponse.Data.Account;
            ViewBag.AccountSubTypes = oClsResponse.Data.AccountSubTypes;
            ViewBag.AccountTypes = oClsResponse1.Data.AccountTypes;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            return PartialView("PartialAccountView");
        }
        #endregion

        #region Journal
        public async Task<ActionResult> Journal(ClsJournalVm obj)
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
                //obj.Title = "Accounts";
            }
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            MenuController menuController = new MenuController();
            AccountController accountController = new AccountController();
            BranchController branchController = new BranchController();

            var journalResult = await journalController.AllJournals(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(journalResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            ViewBag.Journals = oClsResponse.Data.Journals;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            //ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            //ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> JournalFetch(ClsJournalVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Accounts";
            }
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            MenuController menuController = new MenuController();

            var journalResult = await journalController.AllJournals(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(journalResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.Journals = oClsResponse.Data.Journals;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            return PartialView("PartialJournal");
        }
        public async Task<ActionResult> JournalAdd()
        {
            ClsJournalVm obj = new ClsJournalVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            MenuController menuController = new MenuController();
            BranchController branchController = new BranchController();
            UserController userController = new UserController();

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ExpenseFor = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() != "user");

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            return View();
        }
        public async Task<ActionResult> JournalInsert(ClsJournalVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            var result = await journalController.InsertJournal(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> JournalEdit(long JournalId)
        {
            ClsJournalVm obj = new ClsJournalVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.JournalId = JournalId;
            }
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            AccountController accountController = new AccountController();
            MenuController menuController = new MenuController();
            BranchController branchController = new BranchController();
            UserController userController = new UserController();

            ClsJournal journalObj = new ClsJournal { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, JournalId = obj.JournalId };
            var journalResult = await journalController.Journal(journalObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(journalResult);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.Journal = oClsResponse.Data.Journal;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ExpenseFor = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() != "user");

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.TotalDebit = oClsResponse.Data.Journal.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.Journal.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();
            return View();
        }
        public async Task<ActionResult> JournalUpdate(ClsJournalVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            var result = await journalController.UpdateJournal(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> JournalDelete(long JournalId)
        {
            ClsJournalVm obj = new ClsJournalVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.JournalId = JournalId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            var result = await journalController.JournalDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> JournalPaymentDelete(ClsJournalPaymentVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            var result = await journalController.JournalPaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }        
        public async Task<ActionResult> JournalView(long JournalId)
        {
            ClsJournalVm obj = new ClsJournalVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.JournalId = JournalId;
            }
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            AccountController accountController = new AccountController();
            MenuController menuController = new MenuController();
            BranchController branchController = new BranchController();
            UserController userController = new UserController();

            ClsJournal journalObj = new ClsJournal { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, JournalId = obj.JournalId };
            var journalResult = await journalController.Journal(journalObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(journalResult);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.Journal = oClsResponse.Data.Journal;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ExpenseFor = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() != "user");

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.TotalDebit = oClsResponse.Data.Journal.JournalPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse.Data.Journal.JournalPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            return PartialView("PartialJournalView");
        }
        public async Task<ActionResult> JournalImport()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Journal";
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var result = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ExpenseFor = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() != "user");
            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            // Calculate column visibility flags
            ViewBag.ShowBranchColumn = ViewBag.Branchs != null && ViewBag.Branchs.Count > 1;

            return View();
        }

        public async Task<ActionResult> DownloadJournalSample()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Journal";
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;

            // Generate CSV content
            string csvContent = GenerateJournalCsvContent(showBranchColumn, oClsResponse1.Data.Branchs);

            // Return CSV file
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(csvBytes, "text/csv", "Journal_Sample.csv");
        }

        private string GenerateJournalCsvContent(bool showBranchColumn, dynamic branches)
        {
            StringBuilder csv = new StringBuilder();

            // Add headers
            List<string> headers = new List<string>();
            if (showBranchColumn)
            {
                headers.Add("BranchName");
            }
            headers.AddRange(new string[] { "Date", "ReferenceNo", "Notes", "Account", "Description", "Contact", "Debit", "Credit", "GroupName" });

            csv.AppendLine(string.Join(",", headers));

            //// Add sample data rows
            //List<string> sampleRow1 = new List<string>();
            //if (showBranchColumn)
            //{
            //    sampleRow1.Add(branches != null && branches.Count > 0 ? branches[0].Branch : "");
            //}
            //sampleRow1.AddRange(new string[] { "2020-04-21", "REF001", "Sample journal entry", "Cash", "Sample description", "9000000000 | Customer", "1000", "", "Sample Group" });

            //List<string> sampleRow2 = new List<string>();
            //if (showBranchColumn)
            //{
            //    sampleRow2.Add("");
            //}
            //sampleRow2.AddRange(new string[] { "2020-04-21", "", "Another sample entry", "Bank Account", "Another description", "", "", "1000", "Sample Group" });

            //csv.AppendLine(string.Join(",", sampleRow1));
            //csv.AppendLine(string.Join(",", sampleRow2));

            return csv.ToString();
        }

        public async Task<ActionResult> DownloadJournalSampleXlsx()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Journal";
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;

            // Generate XLSX content
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Journal Import Sample");

                // Add headers
                int col = 1;
                if (showBranchColumn)
                {
                    worksheet.Cell(1, col).Value = "BranchName";
                    worksheet.Cell(1, col).Style.Font.Bold = false;
                    col++;
                }
                
                string[] headers = { "Date", "ReferenceNo", "Notes", "Account", "Description", "Contact", "Debit", "Credit", "GroupName" };
                foreach (string header in headers)
                {
                    worksheet.Cell(1, col).Value = header;
                    worksheet.Cell(1, col).Style.Font.Bold = false;
                    col++;
                }

                //// Add sample data rows
                //int row = 2;
                
                //// Sample row 1
                //col = 1;
                //if (showBranchColumn)
                //{
                //    worksheet.Cell(row, col).Value = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 0 ? oClsResponse1.Data.Branchs[0].Branch : "";
                //    col++;
                //}
                //string[] sampleRow1 = { "2020-04-21", "REF001", "Sample journal entry", "Cash", "Sample description", "9000000000 | Customer", "1000", "", "Sample Group" };
                //foreach (string value in sampleRow1)
                //{
                //    worksheet.Cell(row, col).Value = value;
                //    col++;
                //}

                //// Sample row 2
                //row++;
                //col = 1;
                //if (showBranchColumn)
                //{
                //    worksheet.Cell(row, col).Value = "";
                //    col++;
                //}
                //string[] sampleRow2 = { "2020-04-21", "", "Another sample entry", "Bank Account", "Another description", "", "", "1000", "Sample Group" };
                //foreach (string value in sampleRow2)
                //{
                //    worksheet.Cell(row, col).Value = value;
                //    col++;
                //}

                // Format Date column to yyyy-mm-dd format
                int dateColumnIndex = showBranchColumn ? 2 : 1; // Date column index
                var dateColumn = worksheet.Column(dateColumnIndex);
                dateColumn.Style.NumberFormat.Format = "yyyy-mm-dd";

                // Auto-fit columns
                worksheet.ColumnsUsed().AdjustToContents();

                // Return XLSX file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Journal_Sample.xlsx");
                }
            }
        }

        public async Task<ActionResult> ImportJournal(ClsJournalVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            var result = await journalController.ImportJournal(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportFetch(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }

            obj.PageSize = 10000000;
            obj.Type = "Journal";
            serializer.MaxJsonLength = 2147483644;

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type, PageSize = obj.PageSize };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport_Journal");
        }
        public async Task<ActionResult> OtherSoftwareImportInsert(ClsOtherSoftwareImportVm obj)
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
            obj.Type = "Journal";
            serializer.MaxJsonLength = 2147483644;

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var result = await otherSoftwareImportController.InsertOtherSoftwareImport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportDelete(ClsOtherSoftwareImportVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var result = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> JournalCountByBatch(ClsItemVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            JournalController journalController = new JournalController();
            ClsJournalVm journalObj = new ClsJournalVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BatchNo = obj.BatchNo };
            var result = await journalController.JournalCountByBatch(journalObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        #endregion

        #region Payment Account
        public async Task<ActionResult> PaymentAccount(ClsAccountVm obj)
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
                //obj.Title = "Payment Account Report";
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            // NOTE: PaymentAccountReport is commented out in AccountController - needs to be uncommented before direct API call can be used
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/PaymentAccountReport", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            MenuController menuController = new MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment account report").FirstOrDefault();

            ViewBag.TotalAmount = oClsResponse.Data.BankPayments.Sum(x => x.Amount);

            return View();
        }

        public async Task<ActionResult> PaymentAccountFetch(ClsAccountVm obj)
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
                obj.Title = "Payment Account Report";
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            // NOTE: PaymentAccountReport is commented out in AccountController - needs to be uncommented before direct API call can be used
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/PaymentAccountReport", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            MenuController menuController = new MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment account report").FirstOrDefault();

            ViewBag.TotalAmount = oClsResponse.Data.BankPayments.Sum(x => x.Amount);

            return PartialView("PartialPaymentAccountReport");
        }

        public async Task<ActionResult> LinkAccount(ClsCustomerPaymentVm obj)
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
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            // NOTE: LinkPaymentWithAccount is commented out in AccountController - needs to be uncommented before direct API call can be used
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/LinkPaymentWithAccount", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }

        #endregion

        #region Accounts Overview
        public async Task<ActionResult> AccountsOverview(ClsCustomerPaymentVm obj)
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
                //obj.Title = "Cash Flow";
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            // NOTE: CashFlowReport is commented out in AccountController - needs to be uncommented before direct API call can be used
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/CashFlowReport", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            MenuController menuController = new MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash flow").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalCredit = oClsResponse.Data.BankPayments.Sum(x => x.Credit);
            ViewBag.TotalDebit = oClsResponse.Data.BankPayments.Sum(x => x.Debit);

            return View();
        }

        #endregion
    }
}