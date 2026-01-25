using WebApi = EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class PaymentLinkController : Controller
    {
        WebApi.CommonController oCommonController = new WebApi.CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: PaymentLink
        public async Task<ActionResult> Index(ClsPaymentLinkVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }
            ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            obj.UserType = "customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var paymentLinksResult = await paymentLinkController.AllPaymentLinks(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentLinksResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.PaymentLinks = oClsResponse.Data.PaymentLinks;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Users = oClsResponse3.Data.Users;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment link").FirstOrDefault();

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }
        public async Task<ActionResult> PaymentLinkFetch(ClsPaymentLinkVm obj)
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
                //obj.Title = "PaymentLink";
            }
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var paymentLinksResult = await paymentLinkController.AllPaymentLinks(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentLinksResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.PaymentLinks = oClsResponse.Data.PaymentLinks;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment link").FirstOrDefault();

            return PartialView("PartialPaymentLink");
        }
        public async Task<ActionResult> Edit(long PaymentLinkId)
        {
            ClsPaymentLinkVm obj = new ClsPaymentLinkVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PaymentLinkId = PaymentLinkId;
            }
            obj.UserType = "customer";
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            ClsPaymentLink paymentLinkObj = new ClsPaymentLink { PaymentLinkId = obj.PaymentLinkId, AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentLinkResult = await paymentLinkController.PaymentLink(paymentLinkObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentLinkResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ViewBag.PaymentLink = oClsResponse.Data.PaymentLink;
            ViewBag.Users = oClsResponse3.Data.Users;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

            return View();
        }
        public async Task<ActionResult> Add()
        {
            ClsPaymentLinkVm obj = new ClsPaymentLinkVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.UserType = "customer";
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countryResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            var allCurrencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(allCurrencyResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TaxExemptionType = obj.UserType };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stateResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.SellingPriceGroups = oClsResponse1.Data.SellingPriceGroups;
            ViewBag.Users = oClsResponse3.Data.Users;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.AccountSubTypes = oClsResponse12.Data.AccountSubTypes;
            ViewBag.Countrys = oClsResponse13.Data.Countrys;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.AllCurrencys = oClsResponse27.Data.Currencys;
            ViewBag.OnlinePaymentSettings = oClsResponse10.Data.OnlinePaymentSettings;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.Currency = oClsResponse26.Data.Currency;

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> PaymentLinkInsert(ClsPaymentLinkVm obj)
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

            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var result = await paymentLinkController.InsertPaymentLink(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentLinkUpdate(ClsPaymentLinkVm obj)
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

            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var result = await paymentLinkController.UpdatePaymentLink(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> PaymentLinkDelete(ClsPaymentLinkVm obj)
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

            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var result = await paymentLinkController.PaymentLinkDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [AllowAnonymous]
        public async Task<ActionResult> SecurePay(string ReferenceId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            obj.ReferenceId = ReferenceId;

            serializer.MaxJsonLength = 2147483644;
            
            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            ClsPaymentLinkVm paymentLinkObj = new ClsPaymentLinkVm { ReferenceId = obj.ReferenceId };
            var result = await paymentLinkController.SecurePayLink(paymentLinkObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Branch = oClsResponse.Data.Branch;
            ViewBag.PaymentLink = oClsResponse.Data.PaymentLink;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.OnlinePaymentSetting = oClsResponse.Data.OnlinePaymentSetting;
            return View();
            //if(oClsResponse.Data.PaymentLink.Status == "Generated")
            //{
            //    return View();
            //}
            //else if(oClsResponse.Data.PaymentLink.Status == "Paid")
            //{
            //    return Redirect("/paymentlink/paid");
            //}
            //else
            //{
            //    return Redirect("/payment/expired");
            //}
        }
        [AllowAnonymous]
        public async Task<ActionResult> InitPaymentGateway(ClsPaymentLinkVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            WebApi.PaymentLinkController paymentLinkController = new WebApi.PaymentLinkController();
            var result = await paymentLinkController.SecurePayLink(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        [AllowAnonymous]
        public async Task<ActionResult> InsertPaymentOnline(ClsCustomerPaymentVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.InsertCustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
    }
}