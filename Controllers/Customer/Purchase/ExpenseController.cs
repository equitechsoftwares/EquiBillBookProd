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
    public class ExpenseController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: Expense
        #region Expense
        public async Task<ActionResult> Index(ClsExpenseVm obj)
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
                //obj.Title = "Expense";

                //if (BranchId != null)
                //{
                //    obj.BranchId = Convert.ToInt64(BranchId);
                ViewBag.BranchId = obj.BranchId;
                //}
            }
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.AllExpenses(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ExpenseSettingsController expenseSettingsController = new ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { CompanyId = obj.CompanyId };
            var expenseSettingsResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingsResult);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");

            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            //ViewBag.ExpensePaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense payment").FirstOrDefault();

            

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalAmount = oClsResponse.Data.Expenses.Sum(x => x.GrandTotal);
            //ViewBag.TotalPaid = oClsResponse.Data.Expenses.Sum(x => x.Paid);
            //ViewBag.TotalDue = oClsResponse.Data.Expenses.Sum(x => x.Due);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.CustomerId = obj.CustomerId;
            ViewBag.SupplierId = obj.SupplierId;

            ViewBag.CustomerName = obj.CustomerName;
            ViewBag.SupplierName = obj.SupplierName;
            return View();
        }
        public async Task<ActionResult> ExpenseFetch(ClsExpenseVm obj)
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
                //obj.Title = "Expense";
            }
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.AllExpenses(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            //ViewBag.ExpensePaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense payment").FirstOrDefault();
            

            //ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ////ViewBag.IsAccounts = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "accounts").FirstOrDefault() == null ? false : true;

            ViewBag.TotalAmount = oClsResponse.Data.Expenses.Sum(x => x.GrandTotal);
            //ViewBag.TotalPaid = oClsResponse.Data.Expenses.Sum(x => x.Paid);
            //ViewBag.TotalDue = oClsResponse.Data.Expenses.Sum(x => x.Due);

            return PartialView("PartialExpense");
        }
        public async Task<ActionResult> Edit(long ExpenseId)
        {
            ClsExpenseVm obj = new ClsExpenseVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ExpenseId = ExpenseId;
            }
            obj.Type = "Chart Of Account";
            ClsExpense expenseObj = new ClsExpense { ExpenseId = obj.ExpenseId, CompanyId = obj.CompanyId };
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.Expense(expenseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse.Data.Expense.BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm planAddonsObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsList = oCommonController.PlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = new ClsResponse { Data = new ClsData { PlanAddons = planAddonsList } };

            ClsTaxVm taxForGroupObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxForGroupResult = await taxController.ActiveTaxs(taxForGroupObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxForGroupResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ViewBag.Expense = oClsResponse.Data.Expense;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.Branchs = oClsResponse2.Data.Branchs;
            //ViewBag.ExpenseSubCategories = oClsResponse.Data.ExpenseSubCategories;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.ExpenseCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense categories").FirstOrDefault();
            ViewBag.ExpenseSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense sub categories").FirstOrDefault();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> Add()
        {
            ClsExpenseVm obj = new ClsExpenseVm();
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

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm planAddonsObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsList = oCommonController.PlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = new ClsResponse { Data = new ClsData { PlanAddons = planAddonsList } };

            ClsTaxVm taxForGroupObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxForGroupResult = await taxController.ActiveTaxs(taxForGroupObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxForGroupResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            //ViewBag.ExpenseCategories = oClsResponse.Data.ExpenseCategories;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            //ViewBag.BankAccounts = oClsResponse11.Data.Accounts.Where(a => (a.Type == "Bank" || a.Type == "Credit Card"));
            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower()== "Accounts").FirstOrDefault() ;

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.ExpenseCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense categories").FirstOrDefault();
            ViewBag.ExpenseSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense sub categories").FirstOrDefault();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> ExpenseInsert(ClsExpenseVm obj)
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
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.InsertExpense(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpenseUpdate(ClsExpenseVm obj)
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

            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.UpdateExpense(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpenseActiveInactive(ClsExpenseVm obj)
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

            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.ExpenseActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpenseDelete(ClsExpenseVm obj)
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

            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.ExpenseDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpensePaymentDelete(ClsExpensePaymentVm obj)
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

            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.ExpensePaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PaymentInsert(ClsExpensePaymentVm obj)
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
            // Note: InsertPayment method may not exist in ExpensePaymentController API
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpensePayment/InsertPayment", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> Payments(ClsExpensePaymentVm obj)
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
                obj.UserType = "supplier";
            }
            // Note: Payments method may not exist in ExpensePaymentController API
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpensePayment/Payments", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            ViewBag.ExpensePayments = oClsResponse.Data.ExpensePayments;

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);
            ViewBag.ExpensePaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense payment").FirstOrDefault();

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);
            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;


            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;
            ViewBag.OpenPaymentModal = true;
            return PartialView("PartialPayments");
        }
        public async Task<ActionResult> PaymentView(ClsExpensePaymentVm obj)
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
                obj.UserType = "supplier";
            }
            // Note: Payment method may not exist in ExpensePaymentController API
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpensePayment/Payment", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            ViewBag.ExpensePayment = oClsResponse.Data.ExpensePayment;

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.OpenPaymentModal = true;
            return PartialView("PartialPaymentView");
        }

        public async Task<ActionResult> ExpenseImport()
        {
            ClsExpenseVm obj = new ClsExpenseVm();
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

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            return View();
        }
        public async Task<ActionResult> ImportExpense(ClsExpenseVm obj)
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
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.ImportExpense(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpenseView(long ExpenseId)
        {
            ClsExpenseVm obj = new ClsExpenseVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ExpenseId = ExpenseId;
            }
            obj.Type = "Chart Of Account";
            ClsExpense expenseObj = new ClsExpense { ExpenseId = obj.ExpenseId, CompanyId = obj.CompanyId };
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.Expense(expenseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse.Data.Expense.BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm planAddonsObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsList = oCommonController.PlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = new ClsResponse { Data = new ClsData { PlanAddons = planAddonsList } };

            ClsTaxVm taxForGroupObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxForGroupResult = await taxController.ActiveTaxs(taxForGroupObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxForGroupResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var expenseJournalResult = await expenseController.ExpenseJournal(obj);
            ClsResponse oClsResponse40 = await oCommonController.ExtractResponseFromActionResult(expenseJournalResult);

            ExpenseSettingsController expenseSettingsController = new ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { CompanyId = obj.CompanyId };
            var expenseSettingsResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingsResult);

            VehicleController vehicleController = new VehicleController();
            ClsVehicleVm vehicleObj = new ClsVehicleVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var vehicleResult = await vehicleController.ActiveVehicles(vehicleObj);
            ClsResponse oClsResponse61 = await oCommonController.ExtractResponseFromActionResult(vehicleResult);

            ViewBag.Expense = oClsResponse.Data.Expense;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.Branchs = oClsResponse2.Data.Branchs;
            //ViewBag.ExpenseSubCategories = oClsResponse.Data.ExpenseSubCategories;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.BankPayments = oClsResponse40.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse40.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse40.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            ViewBag.Vehicles = oClsResponse61.Data.Vehicles;

            ViewBag.ExpenseCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense categories").FirstOrDefault();
            ViewBag.ExpenseSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense sub categories").FirstOrDefault();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.VehiclePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "vehicle").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialExpenseView");
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
            obj.Type = "Expense";
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId, PageSize = obj.PageSize, Type = obj.Type };
            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport_Expense");
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
            obj.Type = "Expense";
            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.InsertOtherSoftwareImport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
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

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ExpenseCountByBatch(ClsItemVm obj)
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

            ClsExpenseVm expenseObj = new ClsExpenseVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type };
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.ExpenseCountByBatch(expenseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> MileageEdit(long ExpenseId)
        {
            ClsExpenseVm obj = new ClsExpenseVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ExpenseId = ExpenseId;
            }
            obj.Type = "Chart Of Account";
            ClsExpense expenseObj = new ClsExpense { ExpenseId = obj.ExpenseId, CompanyId = obj.CompanyId };
            WebApi.ExpenseController expenseController = new WebApi.ExpenseController();
            var expenseResult = await expenseController.Expense(expenseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(expenseResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = oClsResponse.Data.Expense.BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm planAddonsObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsList = oCommonController.PlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = new ClsResponse { Data = new ClsData { PlanAddons = planAddonsList } };

            ClsTaxVm taxForGroupObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxForGroupResult = await taxController.ActiveTaxs(taxForGroupObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxForGroupResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            StateController stateController = new StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ExpenseSettingsController expenseSettingsController = new ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { CompanyId = obj.CompanyId };
            var expenseSettingsResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingsResult);

            VehicleController vehicleController = new VehicleController();
            ClsVehicleVm vehicleObj = new ClsVehicleVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var vehicleResult = await vehicleController.ActiveVehicles(vehicleObj);
            ClsResponse oClsResponse61 = await oCommonController.ExtractResponseFromActionResult(vehicleResult);

            ViewBag.Expense = oClsResponse.Data.Expense;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.Branchs = oClsResponse2.Data.Branchs;
            //ViewBag.ExpenseSubCategories = oClsResponse.Data.ExpenseSubCategories;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            ViewBag.Vehicles = oClsResponse61.Data.Vehicles;

            ViewBag.ExpenseCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense categories").FirstOrDefault();
            ViewBag.ExpenseSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense sub categories").FirstOrDefault();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.VehiclePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "vehicle").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> MileageAdd()
        {
            ClsExpenseVm obj = new ClsExpenseVm();
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

            obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            UserController userController = new UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            PaymentTypeController paymentTypeController = new PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            AccountTypeController accountTypeController = new AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ClsMenuVm planAddonsObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsList = oCommonController.PlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = new ClsResponse { Data = new ClsData { PlanAddons = planAddonsList } };

            ClsTaxVm taxForGroupObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxForGroupResult = await taxController.ActiveTaxs(taxForGroupObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxForGroupResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ExpenseSettingsController expenseSettingsController = new ExpenseSettingsController();
            ClsExpenseSettingsVm expenseSettingsObj = new ClsExpenseSettingsVm { CompanyId = obj.CompanyId };
            var expenseSettingsResult = await expenseSettingsController.ExpenseSetting(expenseSettingsObj);
            ClsResponse oClsResponse60 = await oCommonController.ExtractResponseFromActionResult(expenseSettingsResult);

            VehicleController vehicleController = new VehicleController();
            ClsVehicleVm vehicleObj = new ClsVehicleVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var vehicleResult = await vehicleController.ActiveVehicles(vehicleObj);
            ClsResponse oClsResponse61 = await oCommonController.ExtractResponseFromActionResult(vehicleResult);

            //ViewBag.ExpenseCategories = oClsResponse.Data.ExpenseCategories;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Users = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "user");
            ViewBag.Customers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "customer");
            ViewBag.Suppliers = oClsResponse3.Data.Users.Where(a => a.UserType.ToLower() == "supplier");
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            //ViewBag.BankAccounts = oClsResponse11.Data.Accounts.Where(a => (a.Type == "Bank" || a.Type == "Credit Card"));
            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower()== "Accounts").FirstOrDefault() ;

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;
            ViewBag.Vehicles = oClsResponse61.Data.Vehicles;
            //ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            //ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.ExpenseCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense categories").FirstOrDefault();
            ViewBag.ExpenseSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense sub categories").FirstOrDefault();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.VehiclePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "vehicle").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }

        #endregion
    }
}