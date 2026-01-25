using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    public class CustomersController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Customers
        #region customer
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
                obj.PageIndex = 1;
                obj.UserType = "customer";
                    ViewBag.BranchId = obj.BranchId;
            }
            serializer.MaxJsonLength = 2147483644;

            // Direct API method calls
            WebApi.UserController userController = new WebApi.UserController();
            var userResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ViewBag.Users = oClsResponse.Data.Users;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CustomerStatementPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer statement").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.QuotationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales quotation").FirstOrDefault();
            ViewBag.OrderPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales order").FirstOrDefault();
            ViewBag.ProformaPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales proforma").FirstOrDefault();
            ViewBag.DeliveryChallanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "delivery challan").FirstOrDefault();
            ViewBag.CustomerPaymentsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payments").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalCreditLimit = oClsResponse.Data.Users.Sum(x => x.CreditLimit);
            ViewBag.TotalOpeningBalance = oClsResponse.Data.Users.Sum(x => x.OpeningBalance);
            ViewBag.TotalAdvanceBalance = oClsResponse.Data.Users.Sum(x => x.AdvanceBalance);
            ViewBag.TotalSales = oClsResponse.Data.Users.Sum(x => x.TotalSales);
            ViewBag.TotalSalesDue = oClsResponse.Data.Users.Sum(x => x.TotalSalesDue);
            ViewBag.TotalSalesReturn = oClsResponse.Data.Users.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.Users.Sum(x => x.TotalSalesReturnDue);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            return View();
        }
        public async Task<ActionResult> UserFetch(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            // Direct API method calls
            WebApi.UserController userController = new WebApi.UserController();
            var userResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CustomerStatementPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer statement").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.QuotationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales quotation").FirstOrDefault();
            ViewBag.OrderPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales order").FirstOrDefault();
            ViewBag.ProformaPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales proforma").FirstOrDefault();
            ViewBag.DeliveryChallanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "delivery challan").FirstOrDefault();
            ViewBag.CustomerPaymentsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payments").FirstOrDefault();

            ViewBag.TotalCreditLimit = oClsResponse.Data.Users.Sum(x => x.CreditLimit);
            ViewBag.TotalOpeningBalance = oClsResponse.Data.Users.Sum(x => x.OpeningBalance);
            ViewBag.TotalAdvanceBalance = oClsResponse.Data.Users.Sum(x => x.AdvanceBalance);
            ViewBag.TotalSales = oClsResponse.Data.Users.Sum(x => x.TotalSales);
            ViewBag.TotalSalesDue = oClsResponse.Data.Users.Sum(x => x.TotalSalesDue);
            ViewBag.TotalSalesReturn = oClsResponse.Data.Users.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.Users.Sum(x => x.TotalSalesReturnDue);

            return PartialView("PartialCustomer");
        }
        public async Task<ActionResult> Edit(long UserId)
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
                obj.UserId = UserId;
            }
            obj.TaxExemptionType = "customer";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            // Direct API method calls
            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyVmObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyVmObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyVmObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.TaxExemptionType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.UserGroups = oClsResponse.Data.UserGroups;
            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse4.Data.Currencys;
            ViewBag.Taxs = oClsResponse7.Data.Taxs;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.CurrencyPermission= oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            return View();
        }
        public async Task<ActionResult> Add()
        {
            ClsBranchVm obj = new ClsBranchVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.TaxExemptionType = "customer";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            // Direct API method calls
            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            var branchResult = await branchController.ActiveBranchs(obj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyVmObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyVmObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyVmObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.TaxExemptionType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(obj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            //ViewBag.AltStates = oClsResponse.Data.States;
            ViewBag.UserGroups = oClsResponse1.Data.UserGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.SellingPriceGroups = oClsResponse4.Data.SellingPriceGroups;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.Taxs = oClsResponse7.Data.Taxs;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> UserInsert(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.InsertUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UserUpdate(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UpdateUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UserActiveInactive(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> Userdelete(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveStates(ClsStateVm obj)
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

            WebApi.StateController stateController = new WebApi.StateController();
            var stateResult = await stateController.ActiveStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();

            if (obj.Type == "" || obj.Type == null)
            {
                ViewBag.States = oClsResponse.Data.States;
                return PartialView("PartialStatesDropdown");
            }
            else
            {
                ViewBag.AltStates = oClsResponse.Data.States;
                return PartialView("PartialAltStatesDropdown");
            }
        }
        public async Task<ActionResult> ActiveCitys(ClsCityVm obj)
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

            WebApi.CityController cityController = new WebApi.CityController();
            var cityResult = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(cityResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            if (obj.Type == "" || obj.Type == null)
            {
                ViewBag.Citys = oClsResponse.Data.Citys;
                return PartialView("PartialCitysDropdown");
            }
            else
            {
                ViewBag.AltCitys = oClsResponse.Data.Citys;
                return PartialView("PartialAltCitysDropdown");
            }
        }
        public async Task<ActionResult> UserAutocomplete(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserAutocomplete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> ExistingCustomer(string MobileNo)
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
                obj.MobileNo = MobileNo;
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var userResult = await userController.UserByMobileNo(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyVmObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyVmObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.UserGroups = oClsResponse.Data.UserGroups;
            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse4.Data.Currencys;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            return View();
        }
        
        [HttpPost]
        public async Task<ActionResult> GetCustomerByMobile(string MobileNo)
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
                obj.MobileNo = MobileNo;
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserByMobileNo(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
        
        public async Task<ActionResult> AddExisting(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AddExisting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CustomerView(long UserId)
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
                obj.UserId = UserId;
            }
            //obj.Type = "customer";
            obj.TaxExemptionType = "customer";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyVmObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(currencyVmObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyVmObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.TaxExemptionType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.UserGroups = oClsResponse.Data.UserGroups;
            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse4.Data.Currencys;
            ViewBag.Taxs = oClsResponse7.Data.Taxs;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            return PartialView("PartialCustomerView");
        }
        public async Task<ActionResult> CustomerImport()
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
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Customer";
            obj.ShowAllStates = true;
            obj.ShowAllCities = true;
            //obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = "customer" };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, ShowAllStates = obj.ShowAllStates };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.CityController cityController = new WebApi.CityController();
            ClsCityVm cityObj = new ClsCityVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, ShowAllCities = obj.ShowAllCities };
            var cityResult = await cityController.ActiveCitys(cityObj);
            ClsResponse oClsResponse76 = await oCommonController.ExtractResponseFromActionResult(cityResult);

            //ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Currencys = oClsResponse4.Data.Currencys;
            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;
            ViewBag.Taxs = oClsResponse7.Data.Taxs;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.UserGroups = oClsResponse2.Data.UserGroups;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.States = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Citys = oClsResponse76.Data.Citys;

            return View();
        }
        public async Task<ActionResult> ImportCustomer(ClsUserVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.ImportCustomer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> DownloadCustomerSample()
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
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            // Build headers dynamically similar to the instruction table
            bool isIndia = oClsResponse39.Data.BusinessSetting.CountryId == 2;
            
            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            bool enableCustomerGroup = oClsResponse12.Data.SaleSetting.EnableCustomerGroup == true;

            List<string> headers = new List<string>();
            headers.Add("Name");
            headers.Add("MobileNo");
            headers.Add("EmailId");
            headers.Add("AltMobileNo");
            headers.Add("BusinessName");
            headers.Add("DOB");
            headers.Add("JoiningDate");
            if (enableCustomerGroup)
            {
                headers.Add("UserGroup");
            }
            headers.Add("CreditLimit");
            headers.Add("OpeningBalance");
            headers.Add("PaymentTerm");
            if (isIndia)
            {
                headers.Add("GstTreatment");
                headers.Add("BusinessLegalName");
                headers.Add("BusinessTradeName");
                headers.Add("PanNo");
                headers.Add("PlaceOfSupply");
                headers.Add("TaxPreference");
            }
            headers.Add("BillingLandmark");
            headers.Add("BillingCountryName");
            headers.Add("BillingStateName");
            headers.Add("BillingCityName");
            headers.Add("BillingZipcode");
            headers.Add("BillingAddress");
            headers.Add("IsShippingAddressDifferent");
            headers.Add("ShippingCustomerName");
            headers.Add("ShippingMobileNo");
            headers.Add("ShippingEmailId");
            headers.Add("ShippingAltMobileNo");
            headers.Add("ShippingLandmark");
            headers.Add("ShippingCountryName");
            headers.Add("ShippingStateName");
            headers.Add("ShippingCityName");
            headers.Add("ShippingZipcode");
            headers.Add("ShippingAddress");

            StringBuilder csv = new StringBuilder();
            csv.AppendLine(string.Join(",", headers));
           
            byte[] csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(csvBytes, "text/csv", "Customer_Sample.csv");
        }

        public async Task<ActionResult> DownloadCustomerSampleXlsx()
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
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            bool isIndia = oClsResponse39.Data.BusinessSetting.CountryId == 2;
            bool enableCustomerGroup = oClsResponse12.Data.SaleSetting.EnableCustomerGroup == true;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Customer Import Sample");

                int col = 1;
                Action<string> addHeader = (h) => { worksheet.Cell(1, col).Value = h; worksheet.Cell(1, col).Style.Font.Bold = false; col++; };

                addHeader("Name");
                addHeader("MobileNo");
                addHeader("EmailId");
                addHeader("AltMobileNo");
                addHeader("BusinessName");
                addHeader("DOB");
                addHeader("JoiningDate");
                if (enableCustomerGroup)
                {
                    addHeader("UserGroup");
                }
                addHeader("CreditLimit");
                addHeader("OpeningBalance");
                addHeader("PaymentTerm");
                if (isIndia)
                {
                    addHeader("GstTreatment");
                    addHeader("BusinessLegalName");
                    addHeader("BusinessTradeName");
                    addHeader("PanNo");
                    addHeader("PlaceOfSupply");
                    addHeader("TaxPreference");
                }
                addHeader("BillingLandmark");
                addHeader("BillingCountryName");
                addHeader("BillingStateName");
                addHeader("BillingCityName");
                addHeader("BillingZipcode");
                addHeader("BillingAddress");
                addHeader("IsShippingAddressDifferent");
                addHeader("ShippingCustomerName");
                addHeader("ShippingMobileNo");
                addHeader("ShippingEmailId");
                addHeader("ShippingAltMobileNo");
                addHeader("ShippingLandmark");
                addHeader("ShippingCountryName");
                addHeader("ShippingStateName");
                addHeader("ShippingCityName");
                addHeader("ShippingZipcode");
                addHeader("ShippingAddress");               

                // Format date columns
                // Find columns for DOB and JoiningDate (fixed positions regardless of India block)
                int dobCol = 6; // Name, Mobile, Email, AltMobile, BusinessName, then DOB
                int joiningDateCol = 7;
                worksheet.Column(dobCol).Style.NumberFormat.Format = "yyyy-mm-dd";
                worksheet.Column(joiningDateCol).Style.NumberFormat.Format = "yyyy-mm-dd";

                worksheet.ColumnsUsed().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customer_Sample.xlsx");
                }
            }
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
            obj.Type = "Customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Type = obj.Type, PageSize = obj.PageSize };
            var result = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport");
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
            obj.Type = "Customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            var result = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UserCountByBatch(ClsUserVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserCountByBatch(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> FetchUserCurrency(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.FetchUserCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost]
        public async Task<ActionResult> GetCustomerRewardPoints(ClsCustomerRewardPointsVm obj)
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

            WebApi.RewardPointsController rewardPointsController = new WebApi.RewardPointsController();
            var result = await rewardPointsController.GetCustomerRewardPoints(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            
            string res = serializer.Serialize(new { Status = oClsResponse.Status, Message = oClsResponse.Message, Data = oClsResponse.Data });
            
            // Preserve the dynamic data from the API response
            // The API returns Data as an anonymous object with AvailablePoints, which gets lost during deserialization
            // So we need to manually extract it from the raw JSON response
            try
            {
                Dictionary<string, object> apiResponse = serializer.Deserialize<Dictionary<string, object>>(res);
                if (apiResponse != null && apiResponse.ContainsKey("Data") && apiResponse["Data"] != null && oClsResponse.Data != null)
                {
                    // Set the dynamic data so it's available in JavaScript
                    oClsResponse.Data.CustomerRewardPointsData = apiResponse["Data"];
                }
            }
            catch
            {
                // If deserialization fails, continue with the original response
            }
            
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> CalculatePointsEarned(ClsRewardPointSettingsVm obj)
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

            WebApi.RewardPointsController rewardPointsController = new WebApi.RewardPointsController();
            var result = await rewardPointsController.CalculatePointsEarned(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UnusedAdvanceBalance(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;

            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();

            return PartialView("PartialUnusedAdvanceBalance");
        }
        public async Task<ActionResult> UnpaidSalesInvoices(ClsCustomerPaymentVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSales salesObj = new ClsSales { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CustomerId = obj.CustomerId };
            var salesResult = await salesController.UnpaidSalesInvoices(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesResult);

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.CustomerPayment(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.CustomerPayment = oClsResponse1.Data.CustomerPayment;
            return PartialView("PartialUnpaidInvoices");
        }

        public async Task<ActionResult> CreditLimitUpdate(ClsUserVm obj)
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

            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UpdateCreditLimit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        #endregion        

        #region Customer Payments

        public async Task<ActionResult> Payment(ClsCustomerPaymentVm obj)
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
                obj.UserType = "customer";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;
            obj.PaymentType = "all";
            //obj.Type = "Chart Of Account";

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.AllCustomerPayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccountsDropdown", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Users = oClsResponse1.Data.Users;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            //ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            //ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            //ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            //ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.CustomerId = obj.CustomerId;
            ViewBag.CustomerName = obj.CustomerName;
            return View();
        }
        public async Task<ActionResult> PaymentFetch(ClsCustomerPaymentVm obj)
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
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.AllCustomerPayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            return PartialView("PartialPayment");
        }
        public async Task<ActionResult> PaymentAdd()
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> Dues(ClsCustomerPaymentVm obj)
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
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSales salesObj = new ClsSales { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CustomerId = obj.CustomerId };
            var salesResult = await salesController.UnpaidSalesInvoices(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesResult);
            ViewBag.Sales = oClsResponse.Data.Sales;

            return PartialView("PartialDues");
        }
        public async Task<ActionResult> AdvancePaymentAdd()
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            //var res38 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Tax/ActiveTaxs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse38 = serializer.Deserialize<ClsResponse>(res38);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> PaymentInsert(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.InsertCustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentDelete(long CustomerPaymentId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.CustomerPaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentCancel(long CustomerPaymentId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.CustomerPaymentCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentView(long CustomerPaymentId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CustomerPaymentId = CustomerPaymentId;
            }
            obj.Type = "customer payment";
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.CustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            var customerPaymentJournalResult = await customerPaymentController.CustomerPaymentJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(customerPaymentJournalResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            ViewBag.CustomerPayment = oClsResponse.Data.CustomerPayment;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.TotalRefund = oClsResponse.Data.CustomerPayment.CustomerPaymentIds.Where(a => a.Type == "Customer Refund").Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();

            ViewBag.Sales = oClsResponse36.Data.Sales;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            // Load booking data if this is a booking payment
            ViewBag.Booking = null;
            if (oClsResponse.Data.CustomerPayment != null && oClsResponse.Data.CustomerPayment.BookingId > 0)
            {
                ClsTableBookingVm bookingObj = new ClsTableBookingVm();
                bookingObj.BookingId = oClsResponse.Data.CustomerPayment.BookingId;
                bookingObj.CompanyId = obj.CompanyId;
                bookingObj.AddedBy = obj.AddedBy;
                
                WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
                var bookingResult = await tableBookingController.TableBooking(bookingObj);
                ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(bookingResult);
                if (oClsResponseBooking.Data != null && oClsResponseBooking.Data.Booking != null)
                {
                    ViewBag.Booking = oClsResponseBooking.Data.Booking;
                }
            }

            return PartialView("PartialPaymentView");
        }
        public async Task<ActionResult> DueSummary(ClsCustomerPaymentVm obj)
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
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var dueSummaryResult = await customerPaymentController.DueSummary(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(dueSummaryResult);
            ViewBag.UserPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.User = oClsResponse.Data.User;

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == obj.Type.ToLower()).FirstOrDefault();

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);
            
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            //obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Title = obj.Title.ToLower();

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.OpenPaymentModal = true;
            ViewBag.Type = obj.Type.ToLower();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return PartialView("PartialPaymentAdd");
        }
        public async Task<ActionResult> Refunds(ClsCustomerPaymentVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;
            obj.PaymentType = "all";
            //obj.Type = "Chart Of Account";

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.CustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            var customerRefundsResult = await customerPaymentController.CustomerRefunds(obj);
            ClsResponse oClsResponse62 = await oCommonController.ExtractResponseFromActionResult(customerRefundsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            ViewBag.CustomerPayment = oClsResponse.Data.CustomerPayment;
            ViewBag.CustomerPayments = oClsResponse62.Data.CustomerPayments;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return PartialView("PartialRefunds");
        }
        public async Task<ActionResult> RefundInsert(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.InsertCustomerRefund(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> RefundDelete(long CustomerPaymentId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.RefundDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> RefundCancel(long CustomerPaymentId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.RefundCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentDue(ClsCustomerPaymentVm obj)
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
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var dueSummaryResult = await customerPaymentController.DueSummary(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(dueSummaryResult);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> ApplyCreditsToInvoices(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.ApplyCreditsToInvoices(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        #endregion
    }
}