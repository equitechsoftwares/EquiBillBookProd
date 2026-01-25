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
    public class BankingController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: Banking
        #region Accounts
        public async Task<ActionResult> Index(ClsAccountVm obj)
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
            obj.Type = "Bank & Credit Card";

            AccountController accountController = new AccountController();
            BranchController branchController = new BranchController();
            MenuController menuController = new MenuController();
            PaymentTypeController paymentTypeController = new PaymentTypeController();
            AccountTypeController accountTypeController = new AccountTypeController();

            var accountResult = await accountController.AllAccounts(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ViewBag.Accounts = oClsResponse.Data.Accounts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            //ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "banks").FirstOrDefault();
            ViewBag.AccountsTransactionsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> AccountFetch(ClsAccountVm obj)
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
            //obj.Type = "Bank & Credit Card";
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

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "banks").FirstOrDefault();
            ViewBag.AccountsTransactionsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account transactions").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            

            return PartialView("PartialAccount");
        }
        public async Task<ActionResult> AccountAdd()
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

            MenuController menuController = new MenuController();
            UserCurrencyMapController userCurrencyMapController = new UserCurrencyMapController();
            CurrencyController currencyController = new CurrencyController();

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsCurrencyVm userCurrencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            //ViewBag.AccountTypes = oClsResponse.Data.AccountTypes;

            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> AccountInsert(ClsAccountVm obj)
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
            var result = await accountController.InsertAccount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AccountEdit(long AccountId)
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
            MenuController menuController = new MenuController();
            UserCurrencyMapController userCurrencyMapController = new UserCurrencyMapController();
            CurrencyController currencyController = new CurrencyController();

            var accountResult = await accountController.Account(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsCurrencyVm userCurrencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            ViewBag.Account = oClsResponse.Data.Account;
            ViewBag.AccountSubTypes = oClsResponse.Data.AccountSubTypes;
            //ViewBag.AccountTypes = oClsResponse1.Data.AccountTypes;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> AccountUpdate(ClsAccountVm obj)
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
            var result = await accountController.UpdateAccount(obj);
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
        public async Task<ActionResult> AccountDetailsDelete(ClsAccountDetailsVm obj)
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
            string _json = serializer.Serialize(obj);
            // NOTE: AccountDetailsDelete endpoint may not exist as a direct API method - keeping as PostMethod call
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountDetails/AccountDetailsDelete", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveAccounts(ClsAccountVm obj)
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
            var result = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

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

        public async Task<ActionResult> ActiveOtherAccounts(ClsAccountVm obj)
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
            var result = await accountController.ActiveOtherAccounts(accountObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
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
            MenuController menuController = new MenuController();
            UserCurrencyMapController userCurrencyMapController = new UserCurrencyMapController();
            CurrencyController currencyController = new CurrencyController();

            var accountResult = await accountController.Account(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(accountResult);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsCurrencyVm userCurrencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            ViewBag.Account = oClsResponse.Data.Account;
            ViewBag.AccountSubTypes = oClsResponse.Data.AccountSubTypes;
            //ViewBag.AccountTypes = oClsResponse1.Data.AccountTypes;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            return PartialView("PartialAccountView");
        }

        #endregion

        #region Contra
        public async Task<ActionResult> Contra(ClsContraVm obj)
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
            //obj.Type = "Chart Of Account";

            ContraController contraController = new ContraController();
            BranchController branchController = new BranchController();
            MenuController menuController = new MenuController();
            AccountController accountController = new AccountController();

            var contraResult = await contraController.AllContras(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(contraResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            ViewBag.Contras = oClsResponse.Data.Contras;
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

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            //ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> ContraFetch(ClsContraVm obj)
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

            ContraController contraController = new ContraController();
            MenuController menuController = new MenuController();

            var contraResult = await contraController.AllContras(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(contraResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.Contras = oClsResponse.Data.Contras;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            return PartialView("PartialContra");
        }
        public async Task<ActionResult> ContraAdd()
        {
            ClsContraVm obj = new ClsContraVm();
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
            PaymentTypeController paymentTypeController = new PaymentTypeController();

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            return View();
        }
        public async Task<ActionResult> ContraInsert(ClsContraVm obj)
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

            ContraController contraController = new ContraController();
            var result = await contraController.InsertContra(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ContraEdit(long ContraId)
        {
            ClsContraVm obj = new ClsContraVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ContraId = ContraId;
            }
            serializer.MaxJsonLength = 2147483644;

            ContraController contraController = new ContraController();
            AccountController accountController = new AccountController();
            BranchController branchController = new BranchController();
            PaymentTypeController paymentTypeController = new PaymentTypeController();
            MenuController menuController = new MenuController();

            var contraResult = await contraController.Contra(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(contraResult);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Contra = oClsResponse.Data.Contra;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            
            return View();
        }
        public async Task<ActionResult> ContraUpdate(ClsContraVm obj)
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

            ContraController contraController = new ContraController();
            var result = await contraController.UpdateContra(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ContraDelete(long ContraId)
        {
            ClsContraVm obj = new ClsContraVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ContraId = ContraId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            ContraController contraController = new ContraController();
            var result = await contraController.ContraDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> ContraView(long ContraId)
        {
            ClsContraVm obj = new ClsContraVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ContraId = ContraId;
            }
            serializer.MaxJsonLength = 2147483644;

            ContraController contraController = new ContraController();
            AccountController accountController = new AccountController();
            BranchController branchController = new BranchController();
            PaymentTypeController paymentTypeController = new PaymentTypeController();
            MenuController menuController = new MenuController();

            var contraResult = await contraController.Contra(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(contraResult);

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var contraJournalResult = await contraController.ContraJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(contraJournalResult);

            ViewBag.Contra = oClsResponse.Data.Contra;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.BankPayments = oClsResponse36.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse36.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse36.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            return PartialView("PartialContraView");
        }

        public async Task<ActionResult> ContraImport()
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
            obj.Type = "Contra";
            serializer.MaxJsonLength = 2147483644;

            AccountController accountController = new AccountController();
            BranchController branchController = new BranchController();
            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            PaymentTypeController paymentTypeController = new PaymentTypeController();

            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.PaymentTypes = oClsResponse6.Data.PaymentTypes;
            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            // Calculate column visibility flags
            ViewBag.ShowBranchColumn = ViewBag.Branchs != null && ViewBag.Branchs.Count > 1;

            return View();
        }

        public async Task<ActionResult> ImportContra(ClsContraVm obj)
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

            ContraController contraController = new ContraController();
            var result = await contraController.ImportContra(obj);
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
            obj.Type = "Contra";
            serializer.MaxJsonLength = 2147483644;

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type, PageSize = obj.PageSize };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport_Contra");
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
            obj.Type = "Contra";
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
        public async Task<ActionResult> ContraCountByBatch(ClsItemVm obj)
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

            ContraController contraController = new ContraController();
            ClsContraVm contraObj = new ClsContraVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BatchNo = obj.BatchNo };
            var result = await contraController.ContraCountByBatch(contraObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        
        public async Task<ActionResult> DownloadContraSample()
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
            obj.Type = "Contra";
            serializer.MaxJsonLength = 2147483644;

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;

            string csvContent = GenerateContraCsvContent(showBranchColumn, oClsResponse1.Data.Branchs);

            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(csvBytes, "text/csv", "Contra_Sample.csv");
        }

        private string GenerateContraCsvContent(bool showBranchColumn, dynamic branches)
        {
            StringBuilder csv = new StringBuilder();

            // headers
            List<string> headers = new List<string>();
            if (showBranchColumn)
            {
                headers.Add("BranchName");
            }
            headers.AddRange(new string[] { "ReferenceNo", "Type", "FromAccountName", "ToAccountName", "Amount", "PaymentDate", "PaymentType", "Notes" });
            csv.AppendLine(string.Join(",", headers));

            //// sample rows
            //List<string> sampleRow1 = new List<string>();
            //if (showBranchColumn)
            //{
            //    sampleRow1.Add(branches != null && branches.Count > 0 ? branches[0].Branch : "");
            //}
            //sampleRow1.AddRange(new string[] { "REF001", "Fund Transfer", "Cash", "Bank Account", "1000", "2020-04-21", "", "Sample contra" });

            //List<string> sampleRow2 = new List<string>();
            //if (showBranchColumn)
            //{
            //    sampleRow2.Add("");
            //}
            //sampleRow2.AddRange(new string[] { "", "Deposit", "Cash", "Bank Account", "500", "2020-04-22", "Cheque", "Deposit example" });

            //csv.AppendLine(string.Join(",", sampleRow1));
            //csv.AppendLine(string.Join(",", sampleRow2));

            return csv.ToString();
        }

        public async Task<ActionResult> DownloadContraSampleXlsx()
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
            obj.Type = "Contra";
            serializer.MaxJsonLength = 2147483644;

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Contra Import Sample");

                int col = 1;
                if (showBranchColumn)
                {
                    worksheet.Cell(1, col).Value = "BranchName";
                    worksheet.Cell(1, col).Style.Font.Bold = false;
                    col++;
                }

                string[] headers = { "ReferenceNo", "Type", "FromAccountName", "ToAccountName", "Amount", "PaymentDate", "PaymentType", "Notes" };
                foreach (string header in headers)
                {
                    worksheet.Cell(1, col).Value = header;
                    worksheet.Cell(1, col).Style.Font.Bold = false;
                    col++;
                }

                //int row = 2;
                //col = 1;
                //if (showBranchColumn)
                //{
                //    worksheet.Cell(row, col).Value = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 0 ? oClsResponse1.Data.Branchs[0].Branch : "";
                //    col++;
                //}
                //string[] sampleRow1 = { "REF001", "Fund Transfer", "Cash", "Bank Account", "1000", "2020-04-21", "", "Sample contra" };
                //foreach (string value in sampleRow1)
                //{
                //    worksheet.Cell(row, col).Value = value;
                //    col++;
                //}

                //row++;
                //col = 1;
                //if (showBranchColumn)
                //{
                //    worksheet.Cell(row, col).Value = "";
                //    col++;
                //}
                //string[] sampleRow2 = { "", "Deposit", "Cash", "Bank Account", "500", "2020-04-22", "Cheque", "Deposit example" };
                //foreach (string value in sampleRow2)
                //{
                //    worksheet.Cell(row, col).Value = value;
                //    col++;
                //}

                // Format Date column to yyyy-mm-dd format
                int dateColumnIndex = showBranchColumn ? 7 : 6; // PaymentDate column index
                var dateColumn = worksheet.Column(dateColumnIndex);
                dateColumn.Style.NumberFormat.Format = "yyyy-mm-dd";

                worksheet.ColumnsUsed().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Contra_Sample.xlsx");
                }
            }
        }
        #endregion

    }
}