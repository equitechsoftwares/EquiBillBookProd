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
    public class PurchaseQuotationController : Controller
    {
        // GET: PurchaseQuotation
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public async Task<ActionResult> Index(long? BranchId, int? Date, int? Month, int? Year, long? SupplierId, string SupplierName)
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
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "PurchaseQuotation";

                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }

                if (SupplierId != null)
                {
                    obj.SupplierId = Convert.ToInt64(SupplierId);
                }

                if (Date != null && Month != null && Year != null)
                {
                    System.DateTime newdate = new System.DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), Convert.ToInt32(Date));

                    obj.FromDate = newdate;
                    obj.ToDate = newdate;

                    ViewBag.FromDate = obj.FromDate;
                    ViewBag.Todate = obj.ToDate;
                }

                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.UserType = "supplier";

            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.AllPurchaseQuotations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var userController = new UserController();
            var result1 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new BranchController();
            var result25 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var accountTypeController = new AccountTypeController();
            var result28 = await accountTypeController.ActiveAccountTypes(new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            ViewBag.PurchaseQuotations = oClsResponse.Data.PurchaseQuotations;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse1.Data.Users;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.PurchaseQuotationSetting = oClsResponse14.Data.PurchaseQuotationSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase quotation").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();
            

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalItems = oClsResponse.Data.PurchaseQuotations.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseQuotations.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.PurchaseQuotations.Sum(x => x.GrandTotal);
            //ViewBag.TotalPaid = oClsResponse.Data.PurchaseQuotations.Sum(x => x.Paid);
            //ViewBag.TotalDue = oClsResponse.Data.PurchaseQuotations.Sum(x => x.Due);
            //ViewBag.TotalPurchaseQuotationReturnDue = oClsResponse.Data.PurchaseQuotations.Sum(x => x.PurchaseQuotationReturnDue);
            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.SupplierId = SupplierId;
            ViewBag.SupplierName = SupplierName;
            return View();
        }
        public async Task<ActionResult> PurchaseQuotationFetch(ClsPurchaseQuotationVm obj)
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
                //obj.Title = "PurchaseQuotation";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.AllPurchaseQuotations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);


            ViewBag.PurchaseQuotations = oClsResponse.Data.PurchaseQuotations;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase quotation").FirstOrDefault();
            ViewBag.PurchaseStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase status update").FirstOrDefault();

            ViewBag.TotalItems = oClsResponse.Data.PurchaseQuotations.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseQuotations.Sum(x => x.FreeQuantity);
            ViewBag.TotalGrandTotal = oClsResponse.Data.PurchaseQuotations.Sum(x => x.GrandTotal);
            //ViewBag.TotalPaid = oClsResponse.Data.PurchaseQuotations.Sum(x => x.Paid);
            //ViewBag.TotalDue = oClsResponse.Data.PurchaseQuotations.Sum(x => x.Due);
            //ViewBag.TotalPurchaseQuotationReturnDue = oClsResponse.Data.PurchaseQuotations.Sum(x => x.PurchaseQuotationReturnDue);

            return PartialView("PartialPurchaseQuotation");
        }
        public async Task<ActionResult> Edit(long PurchaseQuotationId)
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
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";
                obj.PurchaseQuotationId = PurchaseQuotationId;
            }
            obj.CountryId = 2;

            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.PurchaseQuotation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.PurchaseQuotation.BranchId;
            obj.IsAdvance = true;

            var userController = new UserController();
            var result6 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result12 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new CountryController();
            var result13 = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new ItemSettingsController();
            var result8 = await itemSettingsController.ItemSetting(new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new PurchaseSettingsController();
            var result14 = await purchaseSettingsController.PurchaseSetting(new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            var onlinePaymentSettingsController = new OnlinePaymentSettingsController();
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var unitController = new UnitController();
            var result24 = await unitController.ActiveUnits(new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new BrandController();
            var result25 = await brandController.ActiveBrands(new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new CategoryController();
            var result26 = await categoryController.ActiveCategorys(new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WarrantyController();
            var result27 = await warrantyController.ActiveWarrantys(new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var currencyController = new CurrencyController();
            var result29 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var result30 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new ItemCodeController();
            var result52 = await itemCodeController.ActiveItemCodes(new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new SaltController();
            var result63 = await saltController.ActiveSalts(new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var result66 = await branchController.MainBranch(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.PurchaseQuotation = oClsResponse.Data.PurchaseQuotation;
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

            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var itemSettingsController = new ItemSettingsController();
            var result8 = await itemSettingsController.ItemSetting(new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var unitController = new UnitController();
            var result24 = await unitController.ActiveUnits(new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new BrandController();
            var result25 = await brandController.ActiveBrands(new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new CategoryController();
            var result26 = await categoryController.ActiveCategorys(new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WarrantyController();
            var result27 = await warrantyController.ActiveWarrantys(new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var currencyController = new CurrencyController();
            var result29 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result30 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new ItemCodeController();
            var result52 = await itemCodeController.ActiveItemCodes(new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var saltController = new SaltController();
            var result63 = await saltController.ActiveSalts(new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
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
            ClsPurchaseQuotationVm obj = new ClsPurchaseQuotationVm();
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

            var branchController = new BranchController();
            var result = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;

            var paymentTypeController = new PaymentTypeController();
            var result5 = await paymentTypeController.ActivePaymentTypes(new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance });
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            var userController = new UserController();
            var result6 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var itemSettingsController = new ItemSettingsController();
            var result8 = await itemSettingsController.ItemSetting(new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result12 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new CountryController();
            var result13 = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var purchaseSettingsController = new PurchaseSettingsController();
            var result14 = await purchaseSettingsController.PurchaseSetting(new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            var onlinePaymentSettingsController = new OnlinePaymentSettingsController();
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var accountController = new AccountController();
            var result22 = await accountController.ActiveAccounts(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(result22);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var unitController = new UnitController();
            var result24 = await unitController.ActiveUnits(new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new BrandController();
            var result25 = await brandController.ActiveBrands(new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new CategoryController();
            var result26 = await categoryController.ActiveCategorys(new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WarrantyController();
            var result27 = await warrantyController.ActiveWarrantys(new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var accountTypeController = new AccountTypeController();
            var result28 = await accountTypeController.ActiveAccountTypes(new ClsAccountTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(result28);

            var currencyController = new CurrencyController();
            var result29 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var result30 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new ItemCodeController();
            var result52 = await itemCodeController.ActiveItemCodes(new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new SaltController();
            var result63 = await saltController.ActiveSalts(new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var result66 = await branchController.MainBranch(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
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
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
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
        public async Task<ActionResult> PurchaseQuotationCreate(long Id, string type)
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
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";

                obj.SalesOrderId = Id;
                obj.PurchaseQuotationId = Id;
            }

            ClsResponse oClsResponse;
            if (type.ToLower() == "sales order")
            {
                var salesOrderController = new WebApi.SalesOrderController();
                var salesOrderObj = new ClsSalesOrderVm { SalesOrderId = Id, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
                var result = await salesOrderController.SalesOrder(salesOrderObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.SalesOrder.BranchId;
                ViewBag.PurchaseQuotation = oClsResponse.Data.SalesOrder;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else
            {
                var purchaseQuotationController = new WebApi.PurchaseQuotationController();
                var result = await purchaseQuotationController.PurchaseQuotation(obj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

                obj.BranchId = oClsResponse.Data.PurchaseQuotation.BranchId;
                ViewBag.PurchaseQuotation = oClsResponse.Data.PurchaseQuotation;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            
            obj.IsAdvance = true;

            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            var userController = new UserController();
            var result6 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result12 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new CountryController();
            var result13 = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new ItemSettingsController();
            var result8 = await itemSettingsController.ItemSetting(new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new PurchaseSettingsController();
            var result14 = await purchaseSettingsController.PurchaseSetting(new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            var onlinePaymentSettingsController = new OnlinePaymentSettingsController();
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var unitController = new UnitController();
            var result24 = await unitController.ActiveUnits(new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new BrandController();
            var result25 = await brandController.ActiveBrands(new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new CategoryController();
            var result26 = await categoryController.ActiveCategorys(new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WarrantyController();
            var result27 = await warrantyController.ActiveWarrantys(new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var currencyController = new CurrencyController();
            var result29 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var result30 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new ItemCodeController();
            var result52 = await itemCodeController.ActiveItemCodes(new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new SaltController();
            var result63 = await saltController.ActiveSalts(new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var result66 = await branchController.MainBranch(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
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
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.SourcesOfSupply = oClsResponse54.Data.States;
            ViewBag.DestinationsOfSupply = oClsResponse54.Data.States;
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
        public async Task<ActionResult> PurchaseQuotationsInsert(ClsPurchaseQuotationVm obj)
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
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.InsertPurchaseQuotation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PurchaseQuotationUpdate(ClsPurchaseQuotationVm obj)
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
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.UpdatePurchaseQuotation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseQuotationDelete(ClsPurchaseQuotationVm obj)
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
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.PurchaseQuotationDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PurchaseQuotationDetailsDelete(ClsPurchaseQuotationDetailsVm obj)
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
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.PurchaseQuotationDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Invoice(string InvoiceId)
        {
            ClsPurchaseQuotationVm obj = new ClsPurchaseQuotationVm();
            obj.InvoiceId = InvoiceId;
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.PurchaseQuotation = oClsResponse.Data.PurchaseQuotation;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalUnitPrice = oClsResponse.Data.PurchaseQuotation.PurchaseQuotationDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseQuotation.PurchaseQuotationDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.PurchaseQuotation.PurchaseQuotationDetails.Select(a => a.AmountIncTax).DefaultIfEmpty().Sum();
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.ItemSetting = oClsResponse.Data.ItemSetting;
            return View();
        }
        public async Task<ActionResult> PurchaseQuotationView(long PurchaseQuotationId)
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
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.UserType = "supplier";
                obj.PurchaseQuotationId = PurchaseQuotationId;
            }
            obj.CountryId = 2;

            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.PurchaseQuotation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var taxController = new TaxController();
            var result4 = await taxController.ActiveAllTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(result4);

            obj.BranchId = oClsResponse.Data.PurchaseQuotation.BranchId;
            obj.IsAdvance = true;

            var userController = new UserController();
            var result6 = await userController.AllActiveUsers(new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var menuController = new MenuController();
            var result35 = await menuController.ControlsPermission(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userCurrencyMapController = new UserCurrencyMapController();
            var result12 = await userCurrencyMapController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var countryController = new CountryController();
            var result13 = await countryController.ActiveCountrys(new ClsCountryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(result13);

            var itemSettingsController = new ItemSettingsController();
            var result8 = await itemSettingsController.ItemSetting(new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            var purchaseSettingsController = new PurchaseSettingsController();
            var result14 = await purchaseSettingsController.PurchaseSetting(new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            var onlinePaymentSettingsController = new OnlinePaymentSettingsController();
            var result20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(new ClsOnlinePaymentSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(result20);

            var unitController = new UnitController();
            var result24 = await unitController.ActiveUnits(new ClsUnitVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(result24);

            var brandController = new BrandController();
            var result25 = await brandController.ActiveBrands(new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var categoryController = new CategoryController();
            var result26 = await categoryController.ActiveCategorys(new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(result26);

            var warrantyController = new WarrantyController();
            var result27 = await warrantyController.ActiveWarrantys(new ClsWarrantyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(result27);

            var currencyController = new CurrencyController();
            var result29 = await currencyController.ActiveCurrencys(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(result29);

            var result30 = await userCurrencyMapController.MainCurrency(new ClsCurrencyVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(result30);

            var result36 = await menuController.PlanAddons(new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(result36);

            var accountController = new AccountController();
            var result37 = await accountController.ActiveAccountsDropdown(new ClsAccount { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(result37);

            var result38 = await taxController.ActiveTaxs(new ClsTaxVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(result38);

            var businessSettingsController = new BusinessSettingsController();
            var result39 = await businessSettingsController.BusinessSetting(new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var paymentTermController = new PaymentTermController();
            var result42 = await paymentTermController.ActivePaymentTerms(new ClsPaymentTermVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(result42);

            var taxTypeController = new TaxTypeController();
            var result51 = await taxTypeController.ActiveTaxTypes(new ClsTaxTypeVm { CompanyId = obj.CompanyId });
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var itemCodeController = new ItemCodeController();
            var result52 = await itemCodeController.ActiveItemCodes(new ClsItemCodeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(result52);

            var stateController = new StateController();
            var result54 = await stateController.ActiveStates(new ClsStateVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(result54);

            var taxExemptionController = new TaxExemptionController();
            var result53 = await taxExemptionController.ActiveTaxExemptions(new ClsTaxExemptionVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(result53);

            var businessRegistrationNameController = new BusinessRegistrationNameController();
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var saltController = new SaltController();
            var result63 = await saltController.ActiveSalts(new ClsSaltVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(result63);

            var result66 = await branchController.MainBranch(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(result66);

            ViewBag.PurchaseQuotation = oClsResponse.Data.PurchaseQuotation;
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

            return PartialView("PartialPurchaseQuotationView");
        }
        public async Task<ActionResult> PurchaseQuotationImport()
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
            }
            var branchController = new BranchController();
            var result1 = await branchController.ActiveBranchs(new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy });
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportPurchaseQuotation(ClsPurchaseQuotationVm obj)
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
            // Note: ImportPurchaseQuotation API method may not exist in WebApi.PurchaseQuotationController
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PurchaseQuotation/ImportPurchaseQuotation", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UpdatePurchaseQuotationStatus(ClsPurchaseQuotationVm obj)
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
            var purchaseQuotationController = new WebApi.PurchaseQuotationController();
            var result = await purchaseQuotationController.UpdatePurchaseQuotationStatus(obj);
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
    }
}