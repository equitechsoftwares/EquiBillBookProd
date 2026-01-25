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
    public class SupplierController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Supplier
        public async Task<ActionResult> Index(long? BranchId)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                obj.UserType = "supplier";
                //obj.Title = "Suppliers";
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }
                ViewBag.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
            var result = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var accountTypeController = new AccountTypeController();
            var result28 = await accountTypeController.ActiveAccountTypes(new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.SupplierStatementPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier statement").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.PurchaseQuotationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase quotation").FirstOrDefault();
            ViewBag.PurchaseOrderPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase order").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            //ViewBag.SupplierPurchasesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier purchases").FirstOrDefault();
            //ViewBag.SupplierPaymentsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payments").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalCreditLimit = oClsResponse.Data.Users.Sum(x => x.CreditLimit);
            ViewBag.TotalOpeningBalance = oClsResponse.Data.Users.Sum(x => x.OpeningBalance);
            ViewBag.TotalAdvanceBalance = oClsResponse.Data.Users.Sum(x => x.AdvanceBalance);
            ViewBag.TotalPurchase = oClsResponse.Data.Users.Sum(x => x.TotalPurchase);
            ViewBag.TotalPurchaseDue = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseDue);
            ViewBag.TotalPurchaseReturn = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseReturn);
            ViewBag.TotalPurchaseReturnDue = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseReturnDue);

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
                obj.UserType = "supplier";
                //obj.Title = "Suppliers";
                ViewBag.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
            var result = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);
            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.SupplierStatementPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier statement").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.PurchaseQuotationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase quotation").FirstOrDefault();
            ViewBag.PurchaseOrderPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase order").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();

            ViewBag.TotalCreditLimit = oClsResponse.Data.Users.Sum(x => x.CreditLimit);
            ViewBag.TotalOpeningBalance = oClsResponse.Data.Users.Sum(x => x.OpeningBalance);
            ViewBag.TotalAdvanceBalance = oClsResponse.Data.Users.Sum(x => x.AdvanceBalance);
            ViewBag.TotalPurchase = oClsResponse.Data.Users.Sum(x => x.TotalPurchase);
            ViewBag.TotalPurchaseDue = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseDue);
            ViewBag.TotalPurchaseReturn = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseReturn);
            ViewBag.TotalPurchaseReturnDue = oClsResponse.Data.Users.Sum(x => x.TotalPurchaseReturnDue);

            return PartialView("PartialSupplier");
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
            obj.CountryId = 2;
            var userController = new UserController();
            var userObj = new ClsUser { UserId = obj.UserId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var branchController = new BranchController();
            var result3 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result5 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var businessSettingsController = new BusinessSettingsController();
            var result6 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var currencyController = new CurrencyController();
            var result25 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result26 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
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
            obj.CountryId = 2;
            var countryController = new CountryController();
            var result = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new BranchController();
            var result3 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result5 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var businessSettingsController = new BusinessSettingsController();
            var result6 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var currencyController = new CurrencyController();
            var result25 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result26 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var result66 = await branchController.MainBranch(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            //ViewBag.AltCountrys = oClsResponse.Data.Countrys;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var userController = new UserController();
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
            var stateController = new StateController();
            var result = await stateController.ActiveStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

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
            var cityController = new CityController();
            var result = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

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
            var userController = new UserController();
            var result = await userController.UserAutocomplete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ExistingSupplier(string MobileNo)
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
                obj.UserType = "supplier";
            }
            var userController = new UserController();
            var result = await userController.UserByMobileNo(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var branchController = new BranchController();
            var result3 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result5 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            return View();
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
            var userController = new UserController();
            var result = await userController.AddExisting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SupplierView(long UserId)
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
            obj.CountryId = 2;
            var userController = new UserController();
            var userObj = new ClsUser { UserId = obj.UserId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var branchController = new BranchController();
            var result3 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result5 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var businessSettingsController = new BusinessSettingsController();
            var result6 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var currencyController = new CurrencyController();
            var result25 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result26 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            return PartialView("PartialSupplierView");
        }

        public async Task<ActionResult> SupplierImport()
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
            obj.Type = "Supplier";
            obj.ShowAllStates = true;
            obj.ShowAllCities = true;
            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var countryController = new CountryController();
            var result = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var currencyController = new CurrencyController();
            var result5 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var otherSoftwareImportController = new OtherSoftwareImportController();
            var result6 = await otherSoftwareImportController.AllOtherSoftwareImports(new ClsOtherSoftwareImportVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var businessSettingsController = new BusinessSettingsController();
            var result7 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(result7);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var cityController = new CityController();
            var result76 = await cityController.ActiveCitys(new ClsCityVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse76 = await oCommonController.ExtractResponseFromActionResult(result76);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            //ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Currencys = oClsResponse5.Data.Currencys;
            ViewBag.OtherSoftwareImports = oClsResponse6.Data.OtherSoftwareImports;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.BusinessSetting = oClsResponse7.Data.BusinessSetting;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.States = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Citys = oClsResponse76.Data.Citys;

            return View();
        }
        public async Task<ActionResult> ImportSupplier(ClsUserVm obj)
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
                obj.UserType = "supplier";
            }
            var userController = new UserController();
            var result = await userController.ImportSupplier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> DownloadSupplierSample()
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
            obj.Type = "Supplier";
            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            bool isIndia = oClsResponse39.Data.BusinessSetting.CountryId == 2;

            System.Text.StringBuilder csv = new System.Text.StringBuilder();

            var headers = new System.Collections.Generic.List<string>();
            headers.Add("Name");
            headers.Add("MobileNo");
            headers.Add("EmailId");
            headers.Add("AltMobileNo");
            headers.Add("BusinessName");
            headers.Add("DOB");
            headers.Add("JoiningDate");
            headers.Add("CreditLimit");
            headers.Add("OpeningBalance");
            headers.Add("PaymentTerm");
            if (isIndia)
            {
                headers.Add("GstTreatment");
                headers.Add("GSTIN");
                headers.Add("PanNo");
                headers.Add("SourceOfSupply");
                headers.Add("BillingLandmark");
                headers.Add("BillingCountryName");
                headers.Add("BillingStateName");
                headers.Add("BillingCityName");
                headers.Add("BillingZipcode");
                headers.Add("BillingAddress");
                headers.Add("IsShippingAddressDifferent");
                headers.Add("ShippingName");
                headers.Add("ShippingMobileNo");
                headers.Add("ShippingEmailId");
                headers.Add("ShippingAltMobileNo");
                headers.Add("ShippingLandmark");
                headers.Add("ShippingCountryName");
                headers.Add("ShippingStateName");
                headers.Add("ShippingCityName");
                headers.Add("ShippingZipcode");
                headers.Add("ShippingAddress");
            }
            else
            {
                headers.Add("IsBusinessRegistered");
                headers.Add("BusinessRegistrationName");
                headers.Add("CountryName");
                headers.Add("Address");
                headers.Add("StateName");
                headers.Add("CityName");
                headers.Add("Zipcode");
                headers.Add("AltCountryName");
                headers.Add("AltAddress");
                headers.Add("AltStateName");
                headers.Add("AltCityName");
                headers.Add("AltZipcode");
            }

            csv.AppendLine(string.Join(",", headers));            

            byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(csvBytes, "text/csv", "Supplier_Sample.csv");
        }

        public async Task<ActionResult> DownloadSupplierSampleXlsx()
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
            obj.Type = "Supplier";
            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            bool isIndia = oClsResponse39.Data.BusinessSetting.CountryId == 2;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Supplier Import Sample");

                int col = 1;
                System.Action<string> addHeader = (h) => { worksheet.Cell(1, col).Value = h; worksheet.Cell(1, col).Style.Font.Bold = false; col++; };

                addHeader("Name");
                addHeader("MobileNo");
                addHeader("EmailId");
                addHeader("AltMobileNo");
                addHeader("BusinessName");
                addHeader("DOB");
                addHeader("JoiningDate");
                addHeader("CreditLimit");
                addHeader("OpeningBalance");
                addHeader("PaymentTerm");
                if (isIndia)
                {
                    addHeader("GstTreatment");
                    addHeader("GSTIN");
                    addHeader("PanNo");
                    addHeader("SourceOfSupply");
                    addHeader("BillingLandmark");
                    addHeader("BillingCountryName");
                    addHeader("BillingStateName");
                    addHeader("BillingCityName");
                    addHeader("BillingZipcode");
                    addHeader("BillingAddress");
                    addHeader("IsShippingAddressDifferent");
                    addHeader("ShippingName");
                    addHeader("ShippingMobileNo");
                    addHeader("ShippingEmailId");
                    addHeader("ShippingAltMobileNo");
                    addHeader("ShippingLandmark");
                    addHeader("ShippingCountryName");
                    addHeader("ShippingStateName");
                    addHeader("ShippingCityName");
                    addHeader("ShippingZipcode");
                    addHeader("ShippingAddress");
                }
                else
                {
                    addHeader("IsBusinessRegistered");
                    addHeader("BusinessRegistrationName");
                    addHeader("CountryName");
                    addHeader("Address");
                    addHeader("StateName");
                    addHeader("CityName");
                    addHeader("Zipcode");
                    addHeader("AltCountryName");
                    addHeader("AltAddress");
                    addHeader("AltStateName");
                    addHeader("AltCityName");
                    addHeader("AltZipcode");
                }

                int dobCol = 6;
                int joiningDateCol = 7;
                worksheet.Column(dobCol).Style.NumberFormat.Format = "yyyy-mm-dd";
                worksheet.Column(joiningDateCol).Style.NumberFormat.Format = "yyyy-mm-dd";
                worksheet.ColumnsUsed().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Supplier_Sample.xlsx");
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
            obj.Type = "Supplier";
            var otherSoftwareImportController = new OtherSoftwareImportController();
            var result5 = await otherSoftwareImportController.AllOtherSoftwareImports(new ClsOtherSoftwareImportVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageSize = obj.PageSize });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

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
            obj.Type = "Supplier";
            var otherSoftwareImportController = new OtherSoftwareImportController();
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
            var otherSoftwareImportController = new OtherSoftwareImportController();
            var result = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UnusedAdvanceBalance(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;

            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();

            return PartialView("PartialUnusedAdvanceBalance");
        }

        public async Task<ActionResult> UnpaidPurchaseInvoices(ClsSupplierPaymentVm obj)
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
            var purchaseController = new WebApi.PurchaseController();
            var purchaseObj = new ClsPurchase { SupplierId = obj.SupplierId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await purchaseController.UnpaidPurchaseInvoices(purchaseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var supplierPaymentController = new SupplierPaymentController();
            var result1 = await supplierPaymentController.SupplierPayment(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            ViewBag.SupplierPayment = oClsResponse1.Data.SupplierPayment;
            return PartialView("PartialUnpaidInvoices");
        }

        #region Supplier Payments
        public async Task<ActionResult> Payment(ClsSupplierPaymentVm obj)
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
                obj.UserType = "supplier";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.PaymentType = "all";
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.AllSupplierPayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccountsDropdown", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
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

            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            //ViewBag.AccountBookPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account book").FirstOrDefault();
            //ViewBag.FundTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "fund transfer").FirstOrDefault();
            //ViewBag.DepositPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "deposit").FirstOrDefault();
            

            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            //ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            //ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            //ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            ViewBag.SupplierId = obj.SupplierId;
            ViewBag.SupplierName = obj.SupplierName;
            return View();
        }
        public async Task<ActionResult> PaymentFetch(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.AllSupplierPayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
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
                obj.UserType = "supplier";
            }
            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result21 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

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
        public async Task<ActionResult> Dues(ClsSupplierPaymentVm obj)
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
            var purchaseController = new WebApi.PurchaseController();
            var purchaseObj = new ClsPurchase { SupplierId = obj.SupplierId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await purchaseController.UnpaidPurchaseInvoices(purchaseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            ViewBag.Purchases = oClsResponse.Data.Purchases;

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
                obj.UserType = "supplier";
            }
            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result21 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> PaymentInsert(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.InsertSupplierPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentDelete(long SupplierPaymentId)
        {
            ClsSupplierPaymentVm obj = new ClsSupplierPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SupplierPaymentId = SupplierPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentCancel(long SupplierPaymentId)
        {
            ClsSupplierPaymentVm obj = new ClsSupplierPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SupplierPaymentId = SupplierPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPaymentCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentView(long SupplierPaymentId)
        {
            ClsSupplierPaymentVm obj = new ClsSupplierPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SupplierPaymentId = SupplierPaymentId;
            }
            obj.Type = "supplier payment";
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var result36 = await supplierPaymentController.SupplierPaymentJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result21 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            ViewBag.SupplierPayment = oClsResponse.Data.SupplierPayment;

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.TotalRefund = oClsResponse.Data.SupplierPayment.SupplierPaymentIds.Where(a => a.Type == "Supplier Refund").Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();

            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();

            ViewBag.Purchases = oClsResponse36.Data.Purchases;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return PartialView("PartialPaymentView");
        }
        public async Task<ActionResult> DueSummary(ClsSupplierPaymentVm obj)
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
            ClsResponse oClsResponse;
            if (obj.Type.ToLower() == "supplier payment")
            {
                var supplierPaymentController = new SupplierPaymentController();
                var result = await supplierPaymentController.DueSummary(obj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            }
            else if (obj.Type.ToLower() == "supplier refund")
            {
                // Note: SupplierRefundController may not exist, using SupplierPaymentController for refunds
                var supplierPaymentController = new SupplierPaymentController();
                var result = await supplierPaymentController.DueSummary(obj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            }
            else
            {
                oClsResponse = new ClsResponse();
            }
            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
            ViewBag.User = oClsResponse.Data.User;

            var menuController = new MenuController();
            var result21 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == obj.Type.ToLower()).FirstOrDefault();

            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Title = obj.Title.ToLower();

            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            ViewBag.OpenPaymentModal = true;
            ViewBag.Type = obj.Type.ToLower();

            return PartialView("PartialPaymentAdd");
        }
        public async Task<ActionResult> Refunds(ClsSupplierPaymentVm obj)
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
                obj.UserType = "Supplier";
            }
            obj.PaymentType = "all";
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var result62 = await supplierPaymentController.SupplierRefunds(obj);
            ClsResponse oClsResponse62 = await oCommonController.ExtractResponseFromActionResult(result62);

            var accountController = new AccountController();
            var result11 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var result21 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oclsresponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            ViewBag.SupplierPayment = oClsResponse.Data.SupplierPayment;
            ViewBag.SupplierPayments = oClsResponse62.Data.SupplierPayments;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.Users = oClsResponse1.Data.Users;

            ViewBag.PaymentTypes = oclsresponse5.Data.PaymentTypes;

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return PartialView("PartialRefunds");
        }
        public async Task<ActionResult> RefundInsert(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.InsertSupplierRefund(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> RefundDelete(long SupplierPaymentId)
        {
            ClsSupplierPaymentVm obj = new ClsSupplierPaymentVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SupplierPaymentId = SupplierPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.RefundDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentDue(ClsSupplierPaymentVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "supplier";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.DueSummary(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ApplyCreditsToInvoices(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new SupplierPaymentController();
            var result = await supplierPaymentController.ApplyCreditsToInvoices(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        #endregion
    }
}