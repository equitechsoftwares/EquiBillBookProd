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
    public class PurchaseController : Controller
    {
        // GET: Purchase

        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index(ClsPurchaseVm obj)
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
                //obj.Title = "Purchase";

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

                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.UserType = "supplier";
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.AllPurchases(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ////obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            //obj.IsAdvance = true;
            //var paymentTypeController = new PaymentTypeController();
            //var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            //ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var accountController = new AccountController();
            //var accountObj = new ClsAccountVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result11 = await accountController.ActiveAccounts(accountObj);
            //ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            //var purchaseSettingsController = new PurchaseSettingsController();
            //var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            //ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var smsSettingsController = new SmsSettingsController();
            //var smsSettingsObj = new ClsSmsSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result17 = await smsSettingsController.ActiveSmsSettings(smsSettingsObj);
            //ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(result17);

            //var emailSettingsController = new EmailSettingsController();
            //var emailSettingsObj = new ClsEmailSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result18 = await emailSettingsController.ActiveEmailSettings(emailSettingsObj);
            //ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(result18);

            //var whatsappSettingsController = new WhatsappSettingsController();
            //var whatsappSettingsObj = new ClsWhatsappSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result19 = await whatsappSettingsController.ActiveWhatsappSettings(whatsappSettingsObj);
            //ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(result19);

            //var planAddonsController = new PlanAddonsController();
            //var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result21 = await planAddonsController.PlanAddons(planAddonsObj);
            //ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var accountTypeController = new WebApi.AccountTypeController();
            var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse1.Data.Users;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();
            ViewBag.BillOfEntryPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "bill of entry").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.OpenPaymentModal = false;

            //ViewBag.TotalPaidQuantity = oClsResponse.Data.Purchases.Sum(x => x.PaidQuantity);
            ViewBag.TotalItems = oClsResponse.Data.Purchases.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Purchases.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Purchases.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Purchases.Sum(x => x.Due);
            ViewBag.TotalPurchaseReturnDue = oClsResponse.Data.Purchases.Sum(x => x.PurchaseReturnDue);
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.SupplierId = obj.SupplierId;
            ViewBag.SupplierName = obj.SupplierName;
            return View();
        }
        public async Task<ActionResult> PurchaseFetch(ClsPurchaseVm obj)
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
                //obj.Title = "Purchase";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.AllPurchases(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();
            ViewBag.BillOfEntryPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "bill of entry").FirstOrDefault();

            //ViewBag.TotalPaidQuantity = oClsResponse.Data.Purchases.Sum(x => x.PaidQuantity);
            ViewBag.TotalItems = oClsResponse.Data.Purchases.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Purchases.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Purchases.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Purchases.Sum(x => x.Due);
            ViewBag.TotalPurchaseReturnDue = oClsResponse.Data.Purchases.Sum(x => x.PurchaseReturnDue);

            return PartialView("PartialPurchase");
        }
        public async Task<ActionResult> Edit(long PurchaseId)
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                obj.PurchaseId = PurchaseId;
            }
            obj.CountryId = 2;
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.Purchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Purchase.BranchId;
            obj.IsAdvance = true;
            //var paymentTypeController = new PaymentTypeController();
            //var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            //var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            //ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyObjForActive = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(currencyObjForActive);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var smsSettingsController = new SmsSettingsController();
            //var smsSettingsObj = new ClsSmsSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result17 = await smsSettingsController.ActiveSmsSettings(smsSettingsObj);
            //ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(result17);

            //var emailSettingsController = new EmailSettingsController();
            //var emailSettingsObj = new ClsEmailSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result18 = await emailSettingsController.ActiveEmailSettings(emailSettingsObj);
            //ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(result18);

            //var whatsappSettingsController = new WhatsappSettingsController();
            //var whatsappSettingsObj = new ClsWhatsappSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result19 = await whatsappSettingsController.ActiveWhatsappSettings(whatsappSettingsObj);
            //ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(result19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            //var planAddonsController = new PlanAddonsController();
            //var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result21 = await planAddonsController.PlanAddons(planAddonsObj);
            //ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            //var accountTypeController = new AccountTypeController();
            //var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            //ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var currencyObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(currencyObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }

        public async Task<ActionResult> LoadModals()
        {
            ClsPurchaseQuotationVm obj = new ClsPurchaseQuotationVm();
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
            obj.CountryId = 2;
            // Essential API calls for modals
            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyObjForMain = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController.MainCurrency(currencyObjForMain);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            // Set ViewBag data for modals
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            // Set permissions for modals
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            // Set addon flags
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("Components/_Modals");
        }
        public async Task<ActionResult> Add()
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                ViewBag.Status = "Draft";
            }
            obj.CountryId = 2;
            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyObjForActive = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(currencyObjForActive);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var smsSettingsController = new SmsSettingsController();
            //var smsSettingsObj = new ClsSmsSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result17 = await smsSettingsController.ActiveSmsSettings(smsSettingsObj);
            //ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(result17);

            //var emailSettingsController = new EmailSettingsController();
            //var emailSettingsObj = new ClsEmailSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result18 = await emailSettingsController.ActiveEmailSettings(emailSettingsObj);
            //ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(result18);

            //var whatsappSettingsController = new WhatsappSettingsController();
            //var whatsappSettingsObj = new ClsWhatsappSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result19 = await whatsappSettingsController.ActiveWhatsappSettings(whatsappSettingsObj);
            //ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(result19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result22 = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(result22);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var accountTypeController = new WebApi.AccountTypeController();
            var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var currencyObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(currencyObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController2 = new WebApi.AccountController();
            var accountObj2 = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController2.ActiveAccountsDropdown(accountObj2);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            
            ViewBag.Accounts = oClsResponse22.Data.Accounts;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            //ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> PurchaseCreate(long Id, string type)
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                obj.SalesOrderId = Id;
                obj.PurchaseQuotationId = Id;
                obj.PurchaseOrderId = Id;
                obj.PurchaseId = Id;
            }
            ClsResponse oClsResponse;
            if (type.ToLower() == "sales order")
            {
                var salesOrderController = new WebApi.SalesOrderController();
                var salesOrderObj = new ClsSalesOrderVm { SalesOrderId = Id, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
                var result = await salesOrderController.SalesOrder(salesOrderObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.SalesOrder.BranchId;
                ViewBag.Purchase = oClsResponse.Data.SalesOrder;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else if (type.ToLower() == "purchase quotation")
            {
                var purchaseQuotationController = new WebApi.PurchaseQuotationController();
                var purchaseQuotationObj = new ClsPurchaseQuotationVm { PurchaseQuotationId = Id, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
                var result = await purchaseQuotationController.PurchaseQuotation(purchaseQuotationObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.PurchaseQuotation.BranchId;
                ViewBag.Purchase = oClsResponse.Data.PurchaseQuotation;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else if (type.ToLower() == "purchase order")
            {
                var purchaseOrderController = new WebApi.PurchaseOrderController();
                var purchaseOrderObj = new ClsPurchaseOrderVm { PurchaseOrderId = Id, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
                var result = await purchaseOrderController.PurchaseOrder(purchaseOrderObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.PurchaseOrder.BranchId;
                ViewBag.Purchase = oClsResponse.Data.PurchaseOrder;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else
            {
                var purchaseController = new WebApi.PurchaseController();
                var purchaseObj = new ClsPurchaseVm { PurchaseId = Id, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
                var result = await purchaseController.Purchase(purchaseObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.Purchase.BranchId;
                ViewBag.Purchase = oClsResponse.Data.Purchase;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.IsAdvance = true;
            //var paymentTypeController = new PaymentTypeController();
            //var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            //var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            //ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var currencyObjForActive = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(currencyObjForActive);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var smsSettingsController = new SmsSettingsController();
            //var smsSettingsObj = new ClsSmsSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result17 = await smsSettingsController.ActiveSmsSettings(smsSettingsObj);
            //ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(result17);

            //var emailSettingsController = new EmailSettingsController();
            //var emailSettingsObj = new ClsEmailSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result18 = await emailSettingsController.ActiveEmailSettings(emailSettingsObj);
            //ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(result18);

            //var whatsappSettingsController = new WhatsappSettingsController();
            //var whatsappSettingsObj = new ClsWhatsappSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result19 = await whatsappSettingsController.ActiveWhatsappSettings(whatsappSettingsObj);
            //ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(result19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            //var planAddonsController = new PlanAddonsController();
            //var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result21 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            //ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            //var accountTypeController = new AccountTypeController();
            //var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            //var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            //ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var currencyObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(currencyObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> PurchasesInsert(ClsPurchaseVm obj)
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
            var result = await purchaseController.InsertPurchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseUpdate(ClsPurchaseVm obj)
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
            var result = await purchaseController.UpdatePurchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseDelete(ClsPurchaseVm obj)
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
            var result = await purchaseController.PurchaseDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseCancel(ClsPurchaseVm obj)
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
            var result = await purchaseController.PurchaseCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseDetailsDelete(ClsPurchaseDetailsVm obj)
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
            var result = await purchaseController.PurchaseDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchasePaymentInsert(ClsSupplierPaymentVm obj)
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
            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.InsertPurchasePayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseReturnPaymentInsert(ClsSupplierPaymentVm obj)
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
            // Note: InsertPurchaseReturnPayment API method not found in SupplierRefundController or SupplierPaymentController
            // Leaving PostMethod call as-is until the API method is identified
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SupplierRefund/InsertPurchaseReturnPayment", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchasePayments(ClsSupplierPaymentVm obj)
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

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.PurchasePayments(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var purchaseController = new WebApi.PurchaseController();
            var purchaseObj = new ClsPurchaseVm { PurchaseId = obj.PurchaseId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await purchaseController.Purchase(purchaseObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
            ViewBag.AdvanceBalance = oClsResponse.Data.User.AdvanceBalance;
            ViewBag.Due = oClsResponse.Data.User.Due;
            ViewBag.SupplierId = oClsResponse.Data.User.UserId;

            ViewBag.Purchase = oClsResponse1.Data.Purchase;

            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result11 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == obj.Type.ToLower()).FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == obj.Type.ToLower()).FirstOrDefault();

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result21 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);
            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();

            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.OpenPaymentModal = true;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            ViewBag.Type = obj.Type.ToLower();

            return PartialView("PartialPurchasePayments");
        }
        
        public async Task<ActionResult> PurchasePaymentView(ClsSupplierPaymentVm obj)
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

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.SupplierPayment = oClsResponse.Data.SupplierPayment;

            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result11 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result21 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(result21);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var supplierPaymentController2 = new WebApi.SupplierPaymentController();
            var result36 = await supplierPaymentController2.SupplierPaymentJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.OpenPaymentModal = true;

            ViewBag.BankPayments = oClsResponse36.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse36.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse36.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.Type = obj.Type.ToLower();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return PartialView("PartialPurchasePaymentView");
        }
        public async Task<ActionResult> PurchasePaymentDelete(long SupplierPaymentId)
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
                obj.UserType = "supplier";
                obj.SupplierPaymentId = SupplierPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.PurchasePaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchasePaymentCancel(long SupplierPaymentId)
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
                obj.UserType = "supplier";
                obj.SupplierPaymentId = SupplierPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.PurchasePaymentCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Invoice(string InvoiceId)
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
            //string[] arr = { "", "", "" };
            //if (Request.Cookies["data"] != null)
            //{
            //    arr[0] = Request.Cookies["data"]["UserType"];
            //    arr[1] = Request.Cookies["data"]["Token"];
            //    arr[2] = Request.Cookies["data"]["Id"];
            //    obj.AddedBy = Convert.ToInt64(arr[2]);
            //    obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            //    //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            //    obj.UserType = "customer";
            //    
            //}

            //obj.CompanyId = Id;
            obj.InvoiceId = InvoiceId;

            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalUnitPrice = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.AmountIncTax).DefaultIfEmpty().Sum();
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.ItemSetting = oClsResponse.Data.ItemSetting;
            return View();
        }
        public async Task<ActionResult> PurchaseView(long PurchaseId)
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                obj.PurchaseId = PurchaseId;
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.CountryId = 2;

            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.Purchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var supplierPaymentObj = new ClsSupplierPaymentVm { PurchaseId = obj.PurchaseId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result2 = await supplierPaymentController.PurchasePayments(supplierPaymentObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(result2);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Purchase.BranchId;
            obj.IsAdvance = true;
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(userCurrencyMapObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            var purchaseController2 = new WebApi.PurchaseController();
            var result40 = await purchaseController2.PurchaseJournal(obj);
            ClsResponse oClsResponse40 = await oCommonController.ExtractResponseFromActionResult(result40);

            obj.BillOfEntryId = oClsResponse.Data.Purchase.BillOfEntryId;
            var billOfEntryController = new WebApi.BillOfEntryController();
            var billOfEntryObj = new ClsBillOfEntryVm { BillOfEntryId = obj.BillOfEntryId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result74 = await billOfEntryController.BillOfEntry(billOfEntryObj);
            ClsResponse oClsResponse74 = await oCommonController.ExtractResponseFromActionResult(result74);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.SupplierPayments = oClsResponse2.Data.SupplierPayments.Where(a => a.PaymentType != "Advance").ToList();
            ViewBag.CreditsApplied = oClsResponse2.Data.SupplierPayments.Where(a =>a.ParentId != 0 && a.PaymentType == "Advance").ToList();
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.BillOfEntry = oClsResponse74.Data.BillOfEntry;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.BankPayments = oClsResponse40.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse40.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse40.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();
            ViewBag.BillOfEntryPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "bill of entry").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialPurchaseView");
        }
        public async Task<ActionResult> PurchaseImport()
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportPurchase(ClsPurchaseVm obj)
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
            var result = await purchaseController.ImportPurchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UpdatePurchaseStatus(ClsPurchaseVm obj)
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
            var result = await purchaseController.UpdatePurchaseStatus(obj);
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

            var stateController = new WebApi.StateController();
            var result = await stateController.ActiveStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
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

            var cityController = new WebApi.CityController();
            var result = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
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
        public async Task<ActionResult> CreditNoteAdd(ClsPurchaseVm obj)
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
            obj.CountryId = 2;
            obj.PurchaseType = "Credit Note";

            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.Purchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Purchase.BranchId;
            obj.SupplierId = oClsResponse.Data.Purchase.SupplierId;
            obj.IsAdvance = true;
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(userCurrencyMapObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            var purchaseController2 = new WebApi.PurchaseController();
            var purchaseInvoicesObj = new ClsPurchaseVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result67 = await purchaseController2.PurchaseInvoices(purchaseInvoicesObj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.PurchaseInvoices = oClsResponse67.Data.Purchases;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> CreditNoteCreate()
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                ViewBag.Status = "Draft";
            }
            obj.CountryId = 2;
            obj.PurchaseType = "Credit Note";

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result22 = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(result22);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var accountTypeController = new WebApi.AccountTypeController();
            var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(userCurrencyMapObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController2 = new WebApi.AccountController();
            var accountObj2 = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController2.ActiveAccountsDropdown(accountObj2);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            
            ViewBag.Accounts = oClsResponse22.Data.Accounts;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            //ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> CreditNoteEdit(ClsPurchaseVm obj)
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
            obj.CountryId = 2;
            obj.PurchaseType = "Credit Note";

            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.Purchase(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Purchase.BranchId;
            obj.SupplierId = oClsResponse.Data.Purchase.SupplierId;
            obj.IsAdvance = true;
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            //var res28 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AccountType/ActiveAccountTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse28 = serializer.Deserialize<ClsResponse>(res28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(userCurrencyMapObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            var purchaseController2 = new WebApi.PurchaseController();
            var purchaseInvoicesObj = new ClsPurchaseVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result67 = await purchaseController2.PurchaseInvoices(purchaseInvoicesObj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            //ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.PurchaseInvoices = oClsResponse67.Data.Purchases;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }

        #region return
        public async Task<ActionResult> PurchaseReturn(ClsPurchaseVm obj)
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
                //obj.Title = "Purchase Return";

                ViewBag.BranchId = obj.BranchId;

                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.UserType = "supplier";

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.AllPurchaseReturns(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ////obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccounts", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            //var res14 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PurchaseSettings/PurchaseSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse14 = serializer.Deserialize<ClsResponse>(res14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var accountTypeController = new WebApi.AccountTypeController();
            var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            ViewBag.PurchaseReturns = oClsResponse.Data.PurchaseReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();
            

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalPaidQuantity = oClsResponse.Data.PurchaseReturns.Sum(x => x.PaidQuantity);
            ViewBag.TotalItems = oClsResponse.Data.PurchaseReturns.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseReturns.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.PurchaseReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.PurchaseReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.PurchaseReturns.Sum(x => x.AmountRemaining);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.SupplierId = obj.SupplierId;
            ViewBag.SupplierName = obj.SupplierName;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnFetch(ClsPurchaseVm obj)
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
                //obj.Title = "Purchase Return";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.AllPurchaseReturns(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21= serializer.Deserialize<ClsResponse>(res21);


            ViewBag.PurchaseReturns = oClsResponse.Data.PurchaseReturns;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();


            //ViewBag.TotalPaidQuantity = oClsResponse.Data.PurchaseReturns.Sum(x => x.PaidQuantity);
            ViewBag.TotalItems = oClsResponse.Data.PurchaseReturns.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseReturns.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.PurchaseReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.PurchaseReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.PurchaseReturns.Sum(x => x.AmountRemaining);

            return PartialView("PartialPurchaseReturn");
        }
        public async Task<ActionResult> PurchaseReturnAdd(long PurchaseId)
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                obj.PurchaseId = PurchaseId;
            }

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var purchaseReturnObj = new ClsPurchaseReturnVm { PurchaseId = obj.PurchaseId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await purchaseReturnController.PurchaseReturn(purchaseReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result15 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(result15);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res20 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "OnlinePaymentSettings/ActiveOnlinePaymentSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse20 = serializer.Deserialize<ClsResponse>(res20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonObj = new ClsPurchaseDebitNoteReasonVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result73 = await purchaseDebitNoteReasonController.ActivePurchaseDebitNoteReasons(purchaseDebitNoteReasonObj);
            ClsResponse oClsResponse73 = await oCommonController.ExtractResponseFromActionResult(result73);

            obj.BranchId = oClsResponse.Data.PurchaseReturn.BranchId;
            obj.SupplierId = oClsResponse.Data.PurchaseReturn.SupplierId;

            var purchaseController = new WebApi.PurchaseController();
            var purchaseInvoicesObj = new ClsPurchaseVm { BranchId = obj.BranchId, SupplierId = obj.SupplierId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result67 = await purchaseController.PurchaseInvoices(purchaseInvoicesObj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.PurchaseReturn = oClsResponse.Data.PurchaseReturn;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.PurchaseSetting = oClsResponse15.Data.PurchaseSetting;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            //ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.PurchaseDebitNoteReasons = oClsResponse73.Data.PurchaseDebitNoteReasons;
            ViewBag.PurchaseInvoices = oClsResponse67.Data.Purchases;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.PurchaseDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase debit note reasons").FirstOrDefault();

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> PurchaseReturnInsert(ClsPurchaseReturnVm obj)
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
            //string url = "";
            //if (obj.PurchaseReturnId == 0)
            //{
            //    url = "PurchaseReturn/InsertPurchaseReturn";
            //}
            //else
            //{
            //    url = "PurchaseReturn/UpdatePurchaseReturn";
            //}
            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.InsertPurchaseReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseReturnUpdate(ClsPurchaseReturnVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.UpdatePurchaseReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseReturnDelete(ClsPurchaseReturnVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.PurchaseReturnDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseReturnCancel(ClsPurchaseReturnVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.PurchaseReturnCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseReturnDetailsDelete(ClsPurchaseReturnDetailsVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.PurchaseReturnDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseReturnCreate()
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
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
                ViewBag.Status = "Draft";
            }
            obj.CountryId = 2;

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;
            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var userCurrencyMapController = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new WebApi.CountryController();
            var countryObj = new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result13 = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            var onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            var onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result22 = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(result22);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var unitController = new WebApi.UnitController();
            var unitObj = new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result24 = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result26 = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WebApi.WarrantyController();
            var warrantyObj = new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result27 = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var accountTypeController = new WebApi.AccountTypeController();
            var accountTypeObj = new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result28 = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new WebApi.CurrencyController();
            var currencyObj = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result29 = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController2 = new WebApi.UserCurrencyMapController();
            var userCurrencyMapObj2 = new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result30 = await userCurrencyMapController2.MainCurrency(userCurrencyMapObj2);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController2 = new WebApi.AccountController();
            var accountObj2 = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController2.ActiveAccountsDropdown(accountObj2);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new WebApi.ItemCodeController();
            var itemCodeObj = new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result52 = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new WebApi.SaltController();
            var saltObj = new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result63 = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var branchController2 = new WebApi.BranchController();
            var branchObj2 = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result66 = await branchController2.MainBranch(branchObj2);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            var purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonObj = new ClsPurchaseDebitNoteReasonVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result73 = await purchaseDebitNoteReasonController.ActivePurchaseDebitNoteReasons(purchaseDebitNoteReasonObj);
            ClsResponse oClsResponse73 = await oCommonController.ExtractResponseFromActionResult(result73);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            ViewBag.Currencys = oClsResponse12.Data.Currencys;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.Accounts = oClsResponse22.Data.Accounts;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            //ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();

            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.Brands = oClsResponse25.Data.Brands;
            ViewBag.Categories = oClsResponse26.Data.Categories;
            ViewBag.Warrantys = oClsResponse27.Data.Warrantys;
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse29.Data.Currencys;
            ViewBag.Currency = oClsResponse30.Data.Currency;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.Salts = oClsResponse63.Data.Salts;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.PurchaseDebitNoteReasons = oClsResponse73.Data.PurchaseDebitNoteReasons;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

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

            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            ViewBag.AccountTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account type").FirstOrDefault();
            ViewBag.AccountSubTypePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "account sub type").FirstOrDefault();
            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();
            ViewBag.PurchaseDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase debit note reasons").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> PurchaseReturnEdit(ClsPurchaseReturnVm obj)
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

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.PurchaseReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result15 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(result15);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res20 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "OnlinePaymentSettings/ActiveOnlinePaymentSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse20 = serializer.Deserialize<ClsResponse>(res20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonObj = new ClsPurchaseDebitNoteReasonVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result73 = await purchaseDebitNoteReasonController.ActivePurchaseDebitNoteReasons(purchaseDebitNoteReasonObj);
            ClsResponse oClsResponse73 = await oCommonController.ExtractResponseFromActionResult(result73);

            obj.BranchId = oClsResponse.Data.PurchaseReturn.BranchId;
            obj.SupplierId = oClsResponse.Data.PurchaseReturn.SupplierId;

            var purchaseController = new WebApi.PurchaseController();
            var purchaseInvoicesObj = new ClsPurchaseVm { BranchId = obj.BranchId, SupplierId = obj.SupplierId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result67 = await purchaseController.PurchaseInvoices(purchaseInvoicesObj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.PurchaseReturn = oClsResponse.Data.PurchaseReturn;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.PurchaseSetting = oClsResponse15.Data.PurchaseSetting;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            //ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.PurchaseDebitNoteReasons = oClsResponse73.Data.PurchaseDebitNoteReasons;
            ViewBag.PurchaseInvoices = oClsResponse67.Data.Purchases;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.PurchaseDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase debit note reasons").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> PurchaseInvoices(ClsPurchaseVm obj)
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
            var result67 = await purchaseController.PurchaseInvoices(obj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            return Json(oClsResponse67);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PurchaseReturnInvoice(string InvoiceId)
        {
            ClsPurchaseReturnVm obj = new ClsPurchaseReturnVm();
            //string[] arr = { "", "", "" };
            //if (Request.Cookies["data"] != null)
            //{
            //    arr[0] = Request.Cookies["data"]["UserType"];
            //    arr[1] = Request.Cookies["data"]["Token"];
            //    arr[2] = Request.Cookies["data"]["Id"];
            //    obj.AddedBy = Convert.ToInt64(arr[2]);
            //    obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            //    //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            //    obj.UserType = "customer";
            //    
            //}

            //obj.CompanyId = Id;
            obj.InvoiceId = InvoiceId;

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Purchase = oClsResponse.Data.Purchase;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalUnitPrice = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.Purchase.PurchaseDetails.Select(a => a.AmountIncTax).DefaultIfEmpty().Sum();
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnView(ClsPurchaseReturnVm obj)
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

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var supplierPaymentObj = new ClsSupplierPaymentVm { PurchaseReturnId = obj.PurchaseReturnId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result2 = await supplierPaymentController.SupplierPayment(supplierPaymentObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(result2);

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.PurchaseReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result15 = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(result15);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res20 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "OnlinePaymentSettings/ActiveOnlinePaymentSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse20 = serializer.Deserialize<ClsResponse>(res20);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var taxController2 = new WebApi.TaxController();
            var taxObj2 = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result38 = await taxController2.ActiveTaxs(taxObj2);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var purchaseReturnController2 = new WebApi.PurchaseReturnController();
            var purchaseReturnJournalObj = new ClsPurchaseReturnVm { PurchaseReturnId = obj.PurchaseReturnId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result40 = await purchaseReturnController2.PurchaseReturnJournal(purchaseReturnJournalObj);
            ClsResponse oClsResponse40 = await oCommonController.ExtractResponseFromActionResult(result40);

            var paymentTermController = new WebApi.PaymentTermController();
            var paymentTermObj = new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result42 = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var taxExemptionController = new WebApi.TaxExemptionController();
            var taxExemptionObj = new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result53 = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new WebApi.StateController();
            var stateObj = new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var purchaseDebitNoteReasonController = new WebApi.PurchaseDebitNoteReasonController();
            var purchaseDebitNoteReasonObj = new ClsPurchaseDebitNoteReasonVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result73 = await purchaseDebitNoteReasonController.ActivePurchaseDebitNoteReasons(purchaseDebitNoteReasonObj);
            ClsResponse oClsResponse73 = await oCommonController.ExtractResponseFromActionResult(result73);

            obj.BranchId = oClsResponse.Data.PurchaseReturn.BranchId;
            obj.SupplierId = oClsResponse.Data.PurchaseReturn.SupplierId;

            var purchaseController = new WebApi.PurchaseController();
            var purchaseInvoicesObj = new ClsPurchaseVm { BranchId = obj.BranchId, SupplierId = obj.SupplierId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result67 = await purchaseController.PurchaseInvoices(purchaseInvoicesObj);
            ClsResponse oClsResponse67 = await oCommonController.ExtractResponseFromActionResult(result67);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.PurchaseReturn = oClsResponse.Data.PurchaseReturn;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.PurchaseSetting = oClsResponse15.Data.PurchaseSetting;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            ViewBag.BankPayments = oClsResponse40.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse40.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse40.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.SupplierPayment = oClsResponse2.Data.SupplierPayment;
            if(oClsResponse2.Data.SupplierPayment != null)
            {
                ViewBag.TotalRefund = oClsResponse2.Data.SupplierPayment.SupplierPaymentIds.Where(a => a.Type == "Supplier Refund").Select(a => a.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
                ViewBag.TotalRefund = 0;
            }

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            //ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
            ViewBag.PurchaseDebitNoteReasons = oClsResponse73.Data.PurchaseDebitNoteReasons;
            ViewBag.PurchaseInvoices = oClsResponse67.Data.Purchases;
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialPurchaseReturnView");
        }
        public async Task<ActionResult> PurchaseReturnImport()
        {
            ClsPurchaseReturnVm obj = new ClsPurchaseReturnVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportPurchaseReturn(ClsPurchaseReturnVm obj)
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
            // Note: ImportPurchaseReturn API method not found in PurchaseReturnController
            // Leaving PostMethod call as-is until the API method is identified
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PurchaseReturn/ImportPurchaseReturn", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseDetails(ClsPurchaseVm obj)
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
            var result = await purchaseController.PurchaseDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res4 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Tax/ActiveAllTaxs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse4 = serializer.Deserialize<ClsResponse>(res4);

            ////var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            //var res6 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse6 = serializer.Deserialize<ClsResponse>(res6);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            //var res15 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PurchaseSettings/PurchaseSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse15 = serializer.Deserialize<ClsResponse>(res15);

            ////var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            ////var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            ////var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            ////var res20 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "OnlinePaymentSettings/ActiveOnlinePaymentSettings", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse20 = serializer.Deserialize<ClsResponse>(res20);

            ////var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            ////ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            //var res36 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse36 = serializer.Deserialize<ClsResponse>(res36);

            //var res37 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccountsDropdown", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse37 = serializer.Deserialize<ClsResponse>(res37);

            //var res38 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Tax/ActiveTaxs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse38 = serializer.Deserialize<ClsResponse>(res38);

            //var res39 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessSettings/BusinessSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse39 = serializer.Deserialize<ClsResponse>(res39);

            //var res42 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentTerm/ActivePaymentTerms", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse42 = serializer.Deserialize<ClsResponse>(res42);

            //var res51 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxType/ActiveTaxTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse51 = serializer.Deserialize<ClsResponse>(res51);

            //var res53 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxExemption/ActiveTaxExemptions", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse53 = serializer.Deserialize<ClsResponse>(res53);

            //var res54 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "State/ActiveStates", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse54 = serializer.Deserialize<ClsResponse>(res54);

            //ViewBag.Branchs = oClsResponse1.Data.Branchs;
            //ViewBag.PurchaseReturn = oClsResponse.Data.PurchaseReturn;
            //ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            //ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ////ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            //ViewBag.Users = oClsResponse6.Data.Users;
            //ViewBag.PurchaseSetting = oClsResponse15.Data.PurchaseSetting;

            //ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            //ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;

            ////ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            ////ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            ////ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ////ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;

            //ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            //ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            //ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            //ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            //ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            //ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            //ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            //ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            //ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            //ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;

            //ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            //ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            //return PartialView("PartialPurchaseDetails");
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BillOfEntryAdd(ClsBillOfEntryVm obj)
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

            var billOfEntryController = new WebApi.BillOfEntryController();
            var result = await billOfEntryController.BillOfEntry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new WebApi.TaxController();
            var taxObj = new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result4 = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var planAddonsController = new WebApi.PlanAddonsController();
            var planAddonsObj = new ClsPlanAddonsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result36 = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new WebApi.AccountController();
            var accountObj = new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result37 = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            ViewBag.BillOfEntry = oClsResponse.Data.BillOfEntry;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

            

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> BillOfEntryInsert(ClsBillOfEntryVm obj)
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

            var billOfEntryController = new WebApi.BillOfEntryController();
            var result = await billOfEntryController.InsertBillOfEntry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BillOfEntryUpdate(ClsBillOfEntryVm obj)
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

            var billOfEntryController = new WebApi.BillOfEntryController();
            var result = await billOfEntryController.UpdateBillOfEntry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BillOfEntryDelete(ClsBillOfEntryVm obj)
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

            var billOfEntryController = new WebApi.BillOfEntryController();
            var result = await billOfEntryController.BillOfEntryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        #endregion

        [AllowAnonymous]
        public async Task<ActionResult> PaymentReceipt(string ReferenceId)
        {
            ClsSupplierPaymentVm obj = new ClsSupplierPaymentVm();
            obj.ReferenceId = ReferenceId;

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.SupplierPaymentReceipt(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Branch = oClsResponse.Data.Branch;
            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            return View();
        }

        public async Task<ActionResult> PurchaseInvoicesByReference(ClsPurchaseVm obj)
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

            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.PurchaseInvoicesByReference(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            ViewBag.OpenPurchaseInvoicesModal = true;

            return PartialView("PartialPurchaseInvoices");
        }

        public async Task<ActionResult> UpdatePurchaseReturnStatus(ClsPurchaseReturnVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.UpdatePurchaseReturnStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> AvailableCredits(ClsSupplierPaymentVm obj)
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

            var supplierPaymentController = new WebApi.SupplierPaymentController();
            var result = await supplierPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var purchaseController = new WebApi.PurchaseController();
            var purchaseObj = new ClsPurchaseVm { PurchaseId = obj.PurchaseId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await purchaseController.Purchase(purchaseObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.SupplierPayments = oClsResponse.Data.SupplierPayments;
            ViewBag.Purchase = oClsResponse1.Data.Purchase;

            ViewBag.InvoiceNo = obj.InvoiceNo;
            ViewBag.GrandTotal = obj.GrandTotal;
            ViewBag.Due = obj.Due;

            return PartialView("PartialAvailableCredits");
        }

        public async Task<ActionResult> PurchaseReturnSearchItems(ClsPurchaseVm obj)
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

            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var purchaseReturnObj = new ClsPurchaseReturnVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, Search = obj.Search };
            var result = await purchaseReturnController.PurchaseReturnSearchItems(purchaseReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
    }
}