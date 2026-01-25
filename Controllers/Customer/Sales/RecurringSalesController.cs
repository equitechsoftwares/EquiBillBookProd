using WebApi = EquiBillBook.Controllers.WebApi;
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
    public class RecurringSalesController : Controller
    {
        WebApi.CommonController oCommonController = new WebApi.CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: RecurringSales
        public async Task<ActionResult> Index(ClsRecurringSalesVm obj)
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

                if (obj.BranchId != 0)
                {
                    ViewBag.BranchId = obj.BranchId;
                }

                if (obj.Day != 0 && obj.Month != 0 && obj.Year != 0)
                {
                    System.DateTime newdate = new System.DateTime(obj.Year, obj.Month, obj.Day);

                    obj.FromDate = newdate;
                    obj.ToDate = newdate;

                    ViewBag.FromDate = obj.FromDate;
                    ViewBag.Todate = obj.ToDate;
                }

                if (obj.CustomerId != 0)
                {
                    ViewBag.CustomerId = obj.CustomerId;
                }
                else
                {
                    ViewBag.CustomerId = 0;
                }
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;
            obj.UserType = "customer";
            
            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var recurringSalesResult = await recurringSalesController.AllRecurringSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSalesResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.RecurringSales = oClsResponse.Data.RecurringSales;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "recurring sales").FirstOrDefault();
            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalItems = oClsResponse.Data.RecurringSales.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.RecurringSales.Sum(x => x.GrandTotal);
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;

            ViewBag.CustomerName = obj.CustomerName;

            return View();
        }

        public async Task<ActionResult> RecurringSalesFetch(ClsRecurringSalesVm obj)
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
            
            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var recurringSalesResult = await recurringSalesController.AllRecurringSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSalesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            ViewBag.RecurringSales = oClsResponse.Data.RecurringSales;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "recurring sales").FirstOrDefault();

            ViewBag.TotalItems = oClsResponse.Data.RecurringSales.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.RecurringSales.Sum(x => x.GrandTotal);

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            return PartialView("PartialRecurringSales");
        }

        public async Task<ActionResult> Edit(long RecurringSalesId)
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
                obj.RecurringSalesId = RecurringSalesId;
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var recurringSaleResult = await recurringSalesController.RecurringSale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSaleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.RecurringSale.BranchId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsResponse oClsResponse20 = oClsResponse10;

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var warrantyResult = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.RecurringSale = oClsResponse.Data.RecurringSale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.RecurringSale.Status;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;

            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.Warrantys = oClsResponse24.Data.Warrantys;
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }

        public async Task<ActionResult> Add()
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "customer";
                ViewBag.Status = "Due";
            }
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsResponse oClsResponse20 = oClsResponse10;

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(unitsResult);

            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            //var res75 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AdditionalCharge/ActiveAdditionalCharges", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse75 = serializer.Deserialize<ClsResponse>(res75);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> BillOfSupplyEdit(long RecurringSalesId)
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
                obj.RecurringSalesId = RecurringSalesId;
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var recurringSaleResult = await recurringSalesController.RecurringSale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSaleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.RecurringSale.BranchId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsResponse oClsResponse20 = oClsResponse10;

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var warrantyResult = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.RecurringSale = oClsResponse.Data.RecurringSale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.RecurringSale.Status;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;

            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.Warrantys = oClsResponse24.Data.Warrantys;
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }

        public async Task<ActionResult> BillOfSupplyAdd()
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "customer";
                ViewBag.Status = "Due";
            }
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsResponse oClsResponse20 = oClsResponse10;

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(unitsResult);

            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            //var res75 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AdditionalCharge/ActiveAdditionalCharges", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse75 = serializer.Deserialize<ClsResponse>(res75);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> LoadModals()
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "customer";
                ViewBag.Status = "Due";
            }
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            ClsResponse oClsResponse20 = oClsResponse10;

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            WebApi.BrandController brandController = new WebApi.BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var brandsResult = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse77 = await oCommonController.ExtractResponseFromActionResult(brandsResult);

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoriesResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse78 = await oCommonController.ExtractResponseFromActionResult(categoriesResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(unitsResult);

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var warrantyResult = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.SaltController saltController = new WebApi.SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saltResult = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(saltResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            //var res75 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AdditionalCharge/ActiveAdditionalCharges", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse75 = serializer.Deserialize<ClsResponse>(res75);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Brands = oClsResponse77.Data.Brands;
            ViewBag.Categories = oClsResponse78.Data.Categories;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();
            ViewBag.OpeningStockPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "add/edit opening stock").FirstOrDefault();
            return PartialView("Components/_Modals");
        }

        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> RecurringSalesInsert(ClsRecurringSalesVm obj)
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

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var result = await recurringSalesController.InsertRecurringSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> RecurringSalesUpdate(ClsRecurringSalesVm obj)
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

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var result = await recurringSalesController.UpdateRecurringSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> RecurringSalesDelete(ClsRecurringSalesVm obj)
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

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var result = await recurringSalesController.RecurringSalesDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> RecurringSalesDetailsDelete(ClsRecurringSalesDetailsVm obj)
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

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var result = await recurringSalesController.RecurringSalesDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> RecurringSalesStop(ClsRecurringSalesVm obj)
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

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var result = await recurringSalesController.RecurringSalesStop(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> RecurringSalesView(long RecurringSalesId)
        {
            ClsRecurringSalesVm obj = new ClsRecurringSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "customer";
                obj.RecurringSalesId = RecurringSalesId;
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            var recurringSaleResult = await recurringSalesController.RecurringSale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSaleResult);

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSalesVm salesObj79 = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var salesAllResult = await salesController.AllSales(salesObj79);
            ClsResponse oClsResponse79 = await oCommonController.ExtractResponseFromActionResult(salesAllResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            ClsCustomerPaymentVm customerPaymentObj = new ClsCustomerPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesId = 0 };
            var paymentsResult = await customerPaymentController.SalesPayments(customerPaymentObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(paymentsResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.RecurringSale.BranchId;

            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            ClsResponse oClsResponse20 = oClsResponse10;

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var warrantyResult = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            ClsResponse oClsResponse54 = oClsResponse8;

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            ClsSalesVm salesObj68 = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var salesInvoicesResult = await salesController.SalesInvoices(salesObj68);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            ViewBag.RecurringSale = oClsResponse.Data.RecurringSale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.CustomerPayments = oClsResponse2.Data.CustomerPayments.Where(a => a.PaymentType != "Advance").ToList();
            ViewBag.CreditsApplied = oClsResponse2.Data.CustomerPayments.Where(a => a.ParentId != 0 && a.PaymentType == "Advance").ToList();
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.RecurringSale.Status;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.Warrantys = oClsResponse24.Data.Warrantys;
            ViewBag.Accounts = oClsResponse22.Data.Accounts;

            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.ChildSalesInvoices = oClsResponse79.Data.Sales;
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ShippingBillPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "shipping bill").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialRecurringSalesView");
        }
    }
}