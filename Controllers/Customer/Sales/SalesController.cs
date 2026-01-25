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
using WebApi = EquiBillBook.Controllers.WebApi;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class SalesController : Controller
    {
        WebApi.CommonController oCommonController = new WebApi.CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Sales

        public async Task<ActionResult> Index(ClsSalesVm obj)
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
                obj.SalesType = "Sales";
                //obj.Status = "Due";
                //ViewBag.Status = "Due";
                //obj.Title = "Sales";

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
            
            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesResult = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesResult);

            ////obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            //obj.IsAdvance = true;
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var userResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccounts", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerDebitNotePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.ShippingBillPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "shipping bill").FirstOrDefault();

            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalItems = oClsResponse.Data.Sales.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Sales.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.Sales.Sum(x => x.TotalDiscount);
            ViewBag.TotalAmount = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);
            ViewBag.TotalReturnDue = oClsResponse.Data.Sales.Sum(x => x.ReturnDue);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;

            ViewBag.CustomerName = obj.CustomerName;

            return View();
        }

        public async Task<ActionResult> SalesFetch(ClsSalesVm obj)
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
                obj.SalesType = "Sales";
                //obj.Status = "Due";
                //obj.Title = "Sales";
                ViewBag.Status = obj.Status;
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;
            
            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesResult = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerDebitNotePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.ShippingBillPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "shipping bill").FirstOrDefault();
            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.TotalItems = oClsResponse.Data.Sales.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Sales.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.Sales.Sum(x => x.TotalDiscount);
            ViewBag.TotalAmount = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);
            ViewBag.TotalReturnDue = oClsResponse.Data.Sales.Sum(x => x.ReturnDue);

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;
            return PartialView("PartialSales");
        }

        public async Task<ActionResult> Edit(long SalesId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.SalesId = SalesId;
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Sale.BranchId;
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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

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
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointResult);

            //var res75 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "AdditionalCharge/ActiveAdditionalCharges", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse75 = serializer.Deserialize<ClsResponse>(res75);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.Sale.Status;
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
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
            ViewBag.Units = oClsResponse24.Data.Units;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;
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

        public async Task<ActionResult> Details(long id)
        {
            ClsSalesVm obj = new ClsSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
                obj.SalesId = id;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            if (oClsResponse.Status == 1 && oClsResponse.Data?.Sale != null && !string.IsNullOrEmpty(oClsResponse.Data.Sale.InvoiceId))
            {
                // Redirect to Invoice view with InvoiceId
                return RedirectToAction("Invoice", new { InvoiceId = oClsResponse.Data.Sale.InvoiceId });
            }
            else
            {
                // If invoice not found, redirect to Sales Index
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Invoice(string InvoiceId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var invoiceResult = await salesController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(invoiceResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalUnitPrice = oClsResponse.Data.Sale.SalesDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.Sale.SalesDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.Sale.SalesDetails.Select(a => a.AmountIncTax).DefaultIfEmpty().Sum();
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.OnlinePaymentSetting = oClsResponse.Data.OnlinePaymentSetting;
            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> InitPaymentGateway(ClsSalesVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var invoiceResult = await salesController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(invoiceResult);

            return Json(oClsResponse);
        }
        [AllowAnonymous]
        public async Task<ActionResult> InsertPaymentOnline(ClsCustomerPaymentVm obj)
        {
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var paymentResult = await customerPaymentController.InsertSalesPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> Add()
        {
            ClsSalesVm obj = new ClsSalesVm();
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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(unitResult);

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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointResult);

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
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            //ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

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
            ClsSalesVm obj = new ClsSalesVm();
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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.BrandController brandController = new WebApi.BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var brandResult = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse77 = await oCommonController.ExtractResponseFromActionResult(brandResult);

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse78 = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypeResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypeResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(unitResult);

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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

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
        public async Task<ActionResult> SalesCreate(long Id, string type)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.SalesQuotationId = Id;
                obj.SalesOrderId = Id;
                obj.DeliveryChallanId = Id;
                obj.SalesProformaId = Id;
                obj.SalesId = Id;
            }
            serializer.MaxJsonLength = 2147483644;
            ClsResponse oClsResponse = null;

            if (type.ToLower() == "sales quotation")
            {
                WebApi.SalesQuotationController salesQuotationController = new WebApi.SalesQuotationController();
                ClsSalesQuotationVm quotationObj = new ClsSalesQuotationVm 
                { 
                    AddedBy = obj.AddedBy, 
                    CompanyId = obj.CompanyId, 
                    UserType = obj.UserType, 
                    SalesQuotationId = obj.SalesQuotationId 
                };
                var salesQuotationResult = await salesQuotationController.SalesQuotation(quotationObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesQuotationResult);

                obj.BranchId = oClsResponse.Data.SalesQuotation.BranchId;
                ViewBag.Sale = oClsResponse.Data.SalesQuotation;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else if (type.ToLower() == "sales order")
            {
                WebApi.SalesOrderController salesOrderController = new WebApi.SalesOrderController();
                ClsSalesOrderVm orderObj = new ClsSalesOrderVm 
                { 
                    AddedBy = obj.AddedBy, 
                    CompanyId = obj.CompanyId, 
                    UserType = obj.UserType, 
                    SalesOrderId = obj.SalesOrderId 
                };
                var salesOrderResult = await salesOrderController.SalesOrder(orderObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesOrderResult);

                obj.BranchId = oClsResponse.Data.SalesOrder.BranchId;
                ViewBag.Sale = oClsResponse.Data.SalesOrder;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else if (type.ToLower() == "sales proforma")
            {
                WebApi.SalesProformaController salesProformaController = new WebApi.SalesProformaController();
                ClsSalesProformaVm proformaObj = new ClsSalesProformaVm 
                { 
                    AddedBy = obj.AddedBy, 
                    CompanyId = obj.CompanyId, 
                    UserType = obj.UserType, 
                    SalesProformaId = obj.SalesProformaId 
                };
                var salesProformaResult = await salesProformaController.SalesProforma(proformaObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesProformaResult);

                obj.BranchId = oClsResponse.Data.SalesProforma.BranchId;
                ViewBag.Sale = oClsResponse.Data.SalesProforma;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else if (type.ToLower() == "delivery challan")
            {
                WebApi.DeliveryChallanController deliveryChallanController = new WebApi.DeliveryChallanController();
                ClsDeliveryChallanVm deliveryChallanObj = new ClsDeliveryChallanVm 
                { 
                    AddedBy = obj.AddedBy, 
                    CompanyId = obj.CompanyId, 
                    UserType = obj.UserType, 
                    DeliveryChallanId = obj.DeliveryChallanId 
                };
                var deliveryChallanResult = await deliveryChallanController.DeliveryChallan(deliveryChallanObj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(deliveryChallanResult);

                obj.BranchId = oClsResponse.Data.DeliveryChallan.BranchId;
                ViewBag.Sale = oClsResponse.Data.DeliveryChallan;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }
            else
            {
                WebApi.SalesController salesController = new WebApi.SalesController();
                var saleResult = await salesController.Sale(obj);
                oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

                obj.BranchId = oClsResponse.Data.Sale.BranchId;
                ViewBag.Sale = oClsResponse.Data.Sale;
                ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            }

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

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
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> RecurringSalesCreate(ClsSalesVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            ClsRecurringSalesVm recurringSalesObj = new ClsRecurringSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, RecurringSalesId = obj.RecurringSalesId };
            var recurringSaleResult = await recurringSalesController.RecurringSale(recurringSalesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSaleResult);

            obj.BranchId = oClsResponse.Data.RecurringSale.BranchId;
            ViewBag.Sale = oClsResponse.Data.RecurringSale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

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
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> RecurringBillOfSupplyCreate(ClsSalesVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.RecurringSalesController recurringSalesController = new WebApi.RecurringSalesController();
            ClsRecurringSalesVm recurringSalesObj = new ClsRecurringSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, RecurringSalesId = obj.RecurringSalesId };
            var recurringSaleResult = await recurringSalesController.RecurringSale(recurringSalesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(recurringSaleResult);

            obj.BranchId = oClsResponse.Data.RecurringSale.BranchId;
            ViewBag.Sale = oClsResponse.Data.RecurringSale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

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
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
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
            ClsCurrencyVm userCurrencyMapObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userCurrencyMapResult = await userCurrencyMapController.ActiveCurrencys(userCurrencyMapObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(userCurrencyMapResult);

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

            var onlinePaymentSettingsResult20 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult20);

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
            var accountResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currencyResult = await currencyController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencyResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(userCurrencyMapObj);
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
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            var stateResult54 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(stateResult54);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            var mainBranchResult = await branchController.MainBranch(branchObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> SalesInsert(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var insertResult = await salesController.InsertSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesUpdate(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var updateResult = await salesController.UpdateSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesDelete(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var deleteResult = await salesController.SalesDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(deleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesCancel(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var cancelResult = await salesController.SalesCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(cancelResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesDetailsDelete(ClsSalesDetailsVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var deleteResult = await salesController.SalesDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(deleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesPaymentInsert(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var paymentResult = await customerPaymentController.InsertSalesPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesReturnPaymentInsert(ClsCustomerPaymentVm obj)
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

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var refundResult = await customerPaymentController.InsertCustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(refundResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesPayments(ClsCustomerPaymentVm obj)
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
            ClsCustomerPaymentVm customerPaymentObj = new ClsCustomerPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesId = obj.SalesId };
            var paymentsResult = await customerPaymentController.SalesPayments(customerPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(paymentsResult);

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesId = obj.SalesId, UserType = obj.UserType };
            var saleResult = await salesController.Sale(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(saleResult);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.AdvanceBalance = oClsResponse.Data.User.AdvanceBalance;
            ViewBag.Due = oClsResponse.Data.User.Due;
            ViewBag.CustomerId = oClsResponse.Data.User.UserId;

            ViewBag.Sale = oClsResponse1.Data.Sale;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = salesObj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales payment").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.SaleSetting = oClsResponse14.Data.SaleSetting;

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.OpenPaymentModal = true;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            ViewBag.Type = "sales payment";
            return PartialView("PartialSalesPayments");
        }
        public async Task<ActionResult> SalesPaymentView(ClsCustomerPaymentVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentResult = await customerPaymentController.CustomerPayment(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            ViewBag.CustomerPayment = oClsResponse.Data.CustomerPayment;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            var journalResult = await customerPaymentController.CustomerPaymentJournal(obj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(journalResult);



            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();

            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.OpenPaymentModal = true;

            ViewBag.BankPayments = oClsResponse36.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse36.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse36.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();

            ViewBag.Type = obj.Type.ToLower();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            return PartialView("PartialSalesPaymentView");
        }
        public async Task<ActionResult> SalesPaymentDelete(long CustomerPaymentId)
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
                obj.UserType = "supplier";
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.SalesPaymentDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesPaymentCancel(long CustomerPaymentId)
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
                obj.UserType = "supplier";
                obj.CustomerPaymentId = CustomerPaymentId;
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var result = await customerPaymentController.SalesPaymentCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesView(long SalesId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.SalesId = SalesId;
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            ClsCustomerPaymentVm customerPaymentObj = new ClsCustomerPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesId = obj.SalesId };
            var paymentsResult = await customerPaymentController.SalesPayments(customerPaymentObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(paymentsResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Sale.BranchId;

            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            var salesJournalResult = await salesController.SalesJournal(obj);
            ClsResponse oClsResponse64 = await oCommonController.ExtractResponseFromActionResult(salesJournalResult);

            var salesInvoicesResult = await salesController.SalesInvoices(obj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            ClsSalesDebitNoteReasonVm salesDebitNoteReasonObj = new ClsSalesDebitNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesDebitNoteReasonsResult = await salesDebitNoteReasonController.ActiveSalesDebitNoteReasons(salesDebitNoteReasonObj);
            ClsResponse oClsResponse71 = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonsResult);

            obj.ShippingBillId = oClsResponse.Data.Sale.ShippingBillId;
            WebApi.ShippingBillController shippingBillController = new WebApi.ShippingBillController();
            ClsShippingBillVm shippingBillObj = new ClsShippingBillVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, ShippingBillId = obj.ShippingBillId };
            var shippingBillResult = await shippingBillController.ShippingBill(shippingBillObj);
            ClsResponse oClsResponse74 = await oCommonController.ExtractResponseFromActionResult(shippingBillResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
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

            ViewBag.Status = oClsResponse.Data.Sale.Status;
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
            ViewBag.ShippingBill = oClsResponse74.Data.ShippingBill;
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
            ViewBag.BankPayments = oClsResponse64.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse64.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse64.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.SalesDebitNoteReasons = oClsResponse71.Data.SalesDebitNoteReasons;
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

            return PartialView("PartialSalesView");
        }
        public async Task<ActionResult> SalesImport()
        {
            ClsSalesVm obj = new ClsSalesVm();
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

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportSales(ClsSalesVm obj)
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
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Sales/ImportSales", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
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
            var result = await stateController.ActiveStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
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
            var result = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
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

        public async Task<ActionResult> SearchLot(ClsAvailableLots obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var result = await itemController.SearchLot(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UpdateSalesStatus(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.UpdateSalesStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ActiveSellingPriceGroups(ClsSalesVm obj)
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

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var result = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesInvoices(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.SalesInvoices(obj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse68);
        }

        public async Task<ActionResult> SalesDetails(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.SalesDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res4 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Tax/ActiveAllTaxs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse4 = serializer.Deserialize<ClsResponse>(res4);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SaleSettings/SaleSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            //var res14 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse14 = serializer.Deserialize<ClsResponse>(res14);

            //var res36 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse36 = serializer.Deserialize<ClsResponse>(res36);

            //var res38 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Tax/ActiveTaxs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse38 = serializer.Deserialize<ClsResponse>(res38);

            //var res39 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessSettings/BusinessSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse39 = serializer.Deserialize<ClsResponse>(res39);

            //var res51 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxType/ActiveTaxTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse51 = serializer.Deserialize<ClsResponse>(res51);

            //var res52 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemCode/ActiveItemCodes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse52 = serializer.Deserialize<ClsResponse>(res52);

            //var res53 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxExemption/ActiveTaxExemptions", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse53 = serializer.Deserialize<ClsResponse>(res53);

            //ViewBag.SalesReturn = oClsResponse.Data.SalesReturn;
            //ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            //ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            //ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            //ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            //ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;

            //ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            //ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();

            //return PartialView("PartialSalesDetails");

            return Json(oClsResponse);
        }

        public async Task<ActionResult> DebitNoteAdd(ClsSalesVm obj)
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
                obj.UserType = "customer";
            }
            obj.Type = "item";
            obj.SalesType = "Debit Note";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Sale.BranchId;
            obj.CustomerId = oClsResponse.Data.Sale.CustomerId;
            obj.IsAdvance = true;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            var salesInvoicesResult = await salesController.SalesInvoices(obj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            ClsSalesDebitNoteReasonVm salesDebitNoteReasonObj = new ClsSalesDebitNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesDebitNoteReasonsResult = await salesDebitNoteReasonController.ActiveSalesDebitNoteReasons(salesDebitNoteReasonObj);
            ClsResponse oClsResponse71 = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonsResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.Sale.Status;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.SalesDebitNoteReasons = oClsResponse71.Data.SalesDebitNoteReasons;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.SalesDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales debit note reasons").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> DebitNoteCreate(ClsSalesVm obj)
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
                obj.UserType = "customer";
                ViewBag.Status = "Due";
            }
            obj.SalesType = "Debit Note";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            var accountTypesResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypesResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            ClsSalesDebitNoteReasonVm salesDebitNoteReasonObj = new ClsSalesDebitNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesDebitNoteReasonsResult = await salesDebitNoteReasonController.ActiveSalesDebitNoteReasons(salesDebitNoteReasonObj);
            ClsResponse oClsResponse71 = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonsResult);

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

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesDebitNoteReasons = oClsResponse71.Data.SalesDebitNoteReasons;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.SalesDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales debit note reasons").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> DebitNoteEdit(ClsSalesVm obj)
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
                obj.UserType = "customer";
            }
            obj.Type = "item";
            obj.SalesType = "Debit Note";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;            
            obj.BranchId = oClsResponse.Data.Sale.BranchId;
            obj.CustomerId = oClsResponse.Data.Sale.CustomerId;
            obj.IsAdvance = true;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            var salesInvoicesResult = await salesController.SalesInvoices(obj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            WebApi.SalesDebitNoteReasonController salesDebitNoteReasonController = new WebApi.SalesDebitNoteReasonController();
            ClsSalesDebitNoteReasonVm salesDebitNoteReasonObj = new ClsSalesDebitNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesDebitNoteReasonsResult = await salesDebitNoteReasonController.ActiveSalesDebitNoteReasons(salesDebitNoteReasonObj);
            ClsResponse oClsResponse71 = await oCommonController.ExtractResponseFromActionResult(salesDebitNoteReasonsResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.Sale.Status;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.SalesDebitNoteReasons = oClsResponse71.Data.SalesDebitNoteReasons;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.SalesDebitNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales debit note reasons").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> BillOfSupplyAdd(ClsSalesVm obj)
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
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            obj.IsAdvance = true;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            var accountTypesResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypesResult);

            WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

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

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
            ViewBag.Currency = oClsResponse26.Data.Currency;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> BillOfSupplyEdit(ClsSalesVm obj)
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
                obj.UserType = "customer";
            }
            obj.Type = "item";
            obj.CountryId = 2;
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var saleResult = await salesController.Sale(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            //obj.BranchId = oClsResponse1.Data.Branchs[0].BranchId;
            obj.BranchId = oClsResponse.Data.Sale.BranchId;
            obj.IsAdvance = true;

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
            ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

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
            var currenciesResult = await userCurrencyMapController.ActiveCurrencys(currencyObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(currenciesResult);

            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var countriesResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(countriesResult);

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

            var onlinePaymentSettingsResult2 = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult2);

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
            ClsCurrencyVm currencyMainObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currenciesMainResult = await currencyController.ActiveCurrencys(currencyMainObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currenciesMainResult);

            var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            var statesResult2 = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult2);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.UserGroups = oClsResponse7.Data.UserGroups;
            ViewBag.States = oClsResponse8.Data.States;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            ViewBag.Status = oClsResponse.Data.Sale.Status;
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
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }

        #region return
        public async Task<ActionResult> SalesReturn(ClsSalesVm obj)
        {
            //ClsUserVm obj = new ClsUserVm();
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
                //obj.Title = "Sales Return";
                //if (BranchId != null)
                //{
                //    obj.BranchId = Convert.ToInt64(BranchId);
                ViewBag.BranchId = obj.BranchId;
                //}
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            obj.UserType = "customer";
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var allSalesReturnResult = await salesReturnController.AllSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesReturnResult);

            ////obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            //_json = serializer.Serialize(obj);
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccounts", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            //var res20 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse20 = serializer.Deserialize<ClsResponse>(res20);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypesResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypesResult);

            ViewBag.SalesReturns = oClsResponse.Data.SalesReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalItems = oClsResponse.Data.SalesReturns.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesReturns.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.SalesReturns.Sum(x => x.TotalDiscount);
            ViewBag.TotalGrandTotal = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.SalesReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.SalesReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.SalesReturns.Sum(x => x.AmountRemaining);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;

            ViewBag.CustomerId = obj.CustomerId;
            ViewBag.CustomerName = obj.CustomerName;
            return View();
        }
        public async Task<ActionResult> SalesReturnFetch(ClsSalesVm obj)
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
                //obj.Title = "Sales Return";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var allSalesReturnResult = await salesReturnController.AllSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesReturnResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            ViewBag.SalesReturns = oClsResponse.Data.SalesReturns;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.TotalItems = oClsResponse.Data.SalesReturns.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesReturns.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.SalesReturns.Sum(x => x.TotalDiscount);
            ViewBag.TotalGrandTotal = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.SalesReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.SalesReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.SalesReturns.Sum(x => x.AmountRemaining);

            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;

            return PartialView("PartialSalesReturn");
        }
        public async Task<ActionResult> SalesReturnAdd(long SalesId)
        {
            ClsSalesVm obj = new ClsSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.SalesId = SalesId;
                obj.UserType = "customer";
                obj.SalesType = "Credit Note Add";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            ClsSalesReturn salesReturnObj = new ClsSalesReturn { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesId = obj.SalesId };
            var saleReturnResult = await salesReturnController.SaleReturn(salesReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleReturnResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            ClsSalesCreditNoteReasonVm salesCreditNoteReasonObj = new ClsSalesCreditNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesCreditNoteReasonsResult = await salesCreditNoteReasonController.ActiveSalesCreditNoteReasons(salesCreditNoteReasonObj);
            ClsResponse oClsResponse72 = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonsResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            obj.BranchId = oClsResponse.Data.SalesReturn.BranchId;
            obj.CustomerId = oClsResponse.Data.SalesReturn.CustomerId;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesInvoicesResult = await salesController.SalesInvoices(obj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            ViewBag.SalesReturn = oClsResponse.Data.SalesReturn;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesCreditNoteReasons = oClsResponse72.Data.SalesCreditNoteReasons;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.SalesCreditNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales credit note reasons").FirstOrDefault();
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> SalesreturnInsert(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.InsertSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> SalesreturnUpdate(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.UpdateSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesReturndelete(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.SalesReturnDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesReturnCancel(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.SalesReturnCancel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SalesReturnDetailsDelete(ClsSalesReturnDetailsVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.SalesReturnDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [AllowAnonymous]
        public async Task<ActionResult> SalesReturnInvoice(string InvoiceId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            ClsSalesVm salesReturnObj = new ClsSalesVm { InvoiceId = InvoiceId };
            var invoiceResult = await salesReturnController.Invoice(salesReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(invoiceResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.TotalUnitPrice = oClsResponse.Data.Sale.SalesDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.Sale.SalesDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.Sale.SalesDetails.Select(a => a.AmountIncTax).DefaultIfEmpty().Sum();
            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            return View();
        }
        public async Task<ActionResult> SalesReturnView(ClsSalesReturnVm obj)
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
                obj.UserType = "customer";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            ClsSalesReturn salesReturnObj = new ClsSalesReturn { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesReturnId = obj.SalesReturnId };
            var saleReturnResult = await salesReturnController.SaleReturn(salesReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleReturnResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            ClsCustomerPaymentVm customerPaymentObj = new ClsCustomerPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesReturnId = obj.SalesReturnId };
            var customerPaymentResult = await customerPaymentController.CustomerPayment(customerPaymentObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(customerPaymentResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = salesReturnObj.BranchId, IsAdvance = true };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            ClsSalesReturnVm salesReturnJournalObj = new ClsSalesReturnVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesReturnId = obj.SalesReturnId };
            var salesReturnJournalResult = await salesReturnController.SalesReturnJournal(salesReturnJournalObj);
            ClsResponse oClsResponse65 = await oCommonController.ExtractResponseFromActionResult(salesReturnJournalResult);

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            ClsSalesCreditNoteReasonVm salesCreditNoteReasonObj = new ClsSalesCreditNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesCreditNoteReasonsResult = await salesCreditNoteReasonController.ActiveSalesCreditNoteReasons(salesCreditNoteReasonObj);
            ClsResponse oClsResponse72 = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonsResult);

            ViewBag.SalesReturn = oClsResponse.Data.SalesReturn;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.BankPayments = oClsResponse65.Data.BankPayments;
            ViewBag.TotalDebit = oClsResponse65.Data.BankPayments.Select(a => a.Debit).DefaultIfEmpty().Sum();
            ViewBag.TotalCredit = oClsResponse65.Data.BankPayments.Select(a => a.Credit).DefaultIfEmpty().Sum();
            ViewBag.SalesCreditNoteReasons = oClsResponse72.Data.SalesCreditNoteReasons;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            ViewBag.CustomerPayment = oClsResponse2.Data.CustomerPayment;
            if (oClsResponse2.Data.CustomerPayment != null)
            {
                ViewBag.TotalRefund = oClsResponse2.Data.CustomerPayment.CustomerPaymentIds.Where(a => a.Type == "Customer Refund").Select(a => a.Amount).DefaultIfEmpty().Sum();
            }
            else
            {
                ViewBag.TotalRefund = 0;
            }


            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();

            return PartialView("PartialSalesReturnView");
        }
        public async Task<ActionResult> SalesReturnImport()
        {
            ClsSalesReturnVm obj = new ClsSalesReturnVm();
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

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportSalesReturn(ClsSalesReturnVm obj)
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
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SalesReturn/ImportSalesReturn", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SalesReturnCreate()
        {
            ClsSalesVm obj = new ClsSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
                obj.SalesType = "Credit Note Add";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            ClsSalesCreditNoteReasonVm salesCreditNoteReasonObj = new ClsSalesCreditNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesCreditNoteReasonsResult = await salesCreditNoteReasonController.ActiveSalesCreditNoteReasons(salesCreditNoteReasonObj);
            ClsResponse oClsResponse72 = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonsResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesCreditNoteReasons = oClsResponse72.Data.SalesCreditNoteReasons;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesCreditNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales credit note reasons").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            return View();
        }
        public async Task<ActionResult> SalesReturnEdit(ClsSalesReturnVm obj)
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
                obj.UserType = "customer";
                obj.SalesType = "Credit Note Edit";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            ClsSalesReturn salesReturnObj = new ClsSalesReturn { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SalesReturnId = obj.SalesReturnId };
            var saleReturnResult = await salesReturnController.SaleReturn(salesReturnObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saleReturnResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = salesReturnObj.BranchId, IsAdvance = true };
            var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

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

            WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
            ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeTaxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
            ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
            ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypesResult);

            WebApi.ItemCodeController itemCodeController = new WebApi.ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemCodesResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodesResult);

            WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var statesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

            WebApi.BranchController branchMainController = new WebApi.BranchController();
            ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
            ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

            WebApi.SalesCreditNoteReasonController salesCreditNoteReasonController = new WebApi.SalesCreditNoteReasonController();
            ClsSalesCreditNoteReasonVm salesCreditNoteReasonObj = new ClsSalesCreditNoteReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var salesCreditNoteReasonsResult = await salesCreditNoteReasonController.ActiveSalesCreditNoteReasons(salesCreditNoteReasonObj);
            ClsResponse oClsResponse72 = await oCommonController.ExtractResponseFromActionResult(salesCreditNoteReasonsResult);

            ClsSalesVm salesInvoicesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = oClsResponse.Data.SalesReturn.BranchId, CustomerId = oClsResponse.Data.SalesReturn.CustomerId };

            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesInvoicesResult = await salesController.SalesInvoices(salesInvoicesObj);
            ClsResponse oClsResponse68 = await oCommonController.ExtractResponseFromActionResult(salesInvoicesResult);

            WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
            ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
            ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

            ViewBag.SalesReturn = oClsResponse.Data.SalesReturn;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;
            ViewBag.Users = oClsResponse6.Data.Users;
            ViewBag.SaleSetting = oClsResponse11.Data.SaleSetting;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
            ViewBag.MainBranch = oClsResponse66.Data.Branch;
            ViewBag.SalesCreditNoteReasons = oClsResponse72.Data.SalesCreditNoteReasons;
            ViewBag.SalesInvoices = oClsResponse68.Data.Sales;
            ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.SalesCreditNoteReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales credit note reasons").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> WriteOffUpdate(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.UpdateWriteOff(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CancelWriteOff(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.CancelWriteOff(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ShippingBillAdd(ClsShippingBillVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.ShippingBillController shippingBillController = new WebApi.ShippingBillController();
            var shippingBillResult = await shippingBillController.ShippingBill(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(shippingBillResult);

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

            ViewBag.ShippingBill = oClsResponse.Data.ShippingBill;
            ViewBag.TaxBreakups = oClsResponse.Data.Taxs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;

            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;



            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> ShippingBillInsert(ClsShippingBillVm obj)
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

            WebApi.ShippingBillController shippingBillController = new WebApi.ShippingBillController();
            var result = await shippingBillController.InsertShippingBill(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ShippingBillUpdate(ClsShippingBillVm obj)
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

            WebApi.ShippingBillController shippingBillController = new WebApi.ShippingBillController();
            var result = await shippingBillController.UpdateShippingBill(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ShippingBillDelete(ClsShippingBillVm obj)
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

            WebApi.ShippingBillController shippingBillController = new WebApi.ShippingBillController();
            var result = await shippingBillController.ShippingBillDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }
        #endregion

        #region Pos
        public async Task<ActionResult> Pos(ClsSalesVm obj)
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
                //obj.Status = "Due";
                //ViewBag.Status = "Due";
                obj.SalesType = "pos";

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
            serializer.MaxJsonLength = 2147483644;
            obj.UserType = "customer";

            WebApi.SalesController salesController = new WebApi.SalesController();
            var allSalesResult = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesResult);

            ////obj.BranchId = oClsResponse.Data.Branchs[0].BranchId;
            //obj.IsAdvance = true;
            //_json = serializer.Serialize(obj);
            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "PaymentType/ActivePaymentTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var usersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(usersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res11 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Account/ActiveAccounts", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse11 = serializer.Deserialize<ClsResponse>(res11);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            //var res17 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/ActiveSmsSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse17 = serializer.Deserialize<ClsResponse>(res17);

            //var res18 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/ActiveEmailSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse18 = serializer.Deserialize<ClsResponse>(res18);

            //var res19 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/ActiveWhatsappSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse19 = serializer.Deserialize<ClsResponse>(res19);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            WebApi.AccountTypeController accountTypeController = new WebApi.AccountTypeController();
            ClsAccountTypeVm accountTypeObj = new ClsAccountTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var accountTypesResult = await accountTypeController.ActiveAccountTypes(accountTypeObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(accountTypesResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            //ViewBag.PaymentTypes = oClsResponse5.Data.PaymentTypes;

            //ViewBag.Accounts = oClsResponse11.Data.Accounts;

            //ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            //ViewBag.SmsSettings = oClsResponse17.Data.SmsSettings;
            //ViewBag.EmailSettings = oClsResponse18.Data.EmailSettings;
            //ViewBag.WhatsappSettings = oClsResponse19.Data.WhatsappSettings;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerDebitNotePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer debit note").FirstOrDefault();


            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.OpenPaymentModal = false;

            ViewBag.TotalItems = oClsResponse.Data.Sales.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Sales.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.Sales.Sum(x => x.TotalDiscount);
            ViewBag.TotalAmount = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);
            ViewBag.TotalReturnDue = oClsResponse.Data.Sales.Sum(x => x.ReturnDue);

            ViewBag.AccountTypes = oClsResponse28.Data.AccountTypes;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;

            return View();
        }
        public async Task<ActionResult> PosFetch(ClsSalesVm obj)
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
                //obj.SalesType = "Sales";
                //obj.Status = "Due";
                //obj.Title = "Sales";
                ViewBag.Status = obj.Status;
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var allSalesResult = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSalesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            //var res21 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/PlanAddons", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse21 = serializer.Deserialize<ClsResponse>(res21);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerDebitNotePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer debit note").FirstOrDefault();


            ViewBag.SalesStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales status update").FirstOrDefault();

            ViewBag.TotalItems = oClsResponse.Data.Sales.Sum(x => x.TotalItems);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.Sales.Sum(x => x.FreeQuantity);
            ViewBag.TotalDiscount = oClsResponse.Data.Sales.Sum(x => x.TotalDiscount);
            ViewBag.TotalAmount = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);
            ViewBag.TotalReturnDue = oClsResponse.Data.Sales.Sum(x => x.ReturnDue);

            ViewBag.PosSetting = oClsResponse13.Data.PosSetting;

            return PartialView("PartialSales");
        }

        public async Task<ActionResult> PosEdit(long SalesId, long? BranchId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.SalesId = SalesId;
                obj.PageIndex = 1;
                obj.PageSize = 100000000;
                ViewBag.BranchId = BranchId;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            ClsCashRegister cashRegisterObj = new ClsCashRegister { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var cashRegisterStatusResult = await cashRegisterController.CashRegisterStatusCheck(cashRegisterObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(cashRegisterStatusResult);

            if (oClsResponse12.Data.CashRegister.Status == 2)
            {
                return Redirect("/sales/cashregister?SalesId=" + SalesId);
            }
            else
            {
                if (BranchId == null)
                {
                    ViewBag.BranchId = oClsResponse12.Data.CashRegister.BranchId;
                }
                else
                {
                    ViewBag.BranchId = BranchId;
                }

                ViewBag.StateId = oClsResponse12.Data.CashRegister.StateId;

                obj.BranchId = ViewBag.BranchId;
                obj.IsAdvance = true;

                WebApi.SalesController salesController = new WebApi.SalesController();
                var saleResult = await salesController.Sale(obj);
                ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(saleResult);

                WebApi.ItemController itemController = new WebApi.ItemController();
                ClsItemBranchMapVm itemPosObj = new ClsItemBranchMapVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
                var itemsPosResult = await itemController.ItemsPos(itemPosObj);
                ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(itemsPosResult);

                WebApi.UserController userController = new WebApi.UserController();
                ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var usersResult = await userController.AllActiveUsers(userObj);
                ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(usersResult);

                WebApi.TaxController taxController = new WebApi.TaxController();
                ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
                ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

                WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
                ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
                ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

                WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
                ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
                var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
                ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

                var holdListResult = await salesController.HoldList(obj);
                ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(holdListResult);

                WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
                ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
                ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

                WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
                ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
                ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

                WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
                ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
                ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

                WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
                ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
                ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

                WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
                ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var warrantysResult = await warrantyController.ActiveWarrantys(warrantyObj);
                ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(warrantysResult);

                WebApi.MenuController menuController = new WebApi.MenuController();
                ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var permissionResult = await menuController.ControlsPermission(menuObj);
                ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

                WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
                ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
                ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

                var planAddonsResult = await menuController.PlanAddons(menuObj);
                ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

                WebApi.AccountController accountController = new WebApi.AccountController();
                ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
                ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

                WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
                ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
                ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

                WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
                ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
                ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

                WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
                ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
                ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

                WebApi.StateController stateController = new WebApi.StateController();
                ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
                var statesResult = await stateController.ActiveStates(stateObj);
                ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

                WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
                ClsAdditionalChargeVm additionalChargeObj = new ClsAdditionalChargeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var additionalChargesResult = await additionalChargeController.ActiveAdditionalCharges(additionalChargeObj);
                ClsResponse oClsResponse75 = await oCommonController.ExtractResponseFromActionResult(additionalChargesResult);

                WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
                ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
                ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

                ViewBag.Sale = oClsResponse11.Data.Sale;
                ViewBag.TaxBreakups = oClsResponse11.Data.Taxs;
                ViewBag.ItemDetails = oClsResponse3.Data.ItemDetails;
                ViewBag.Users = oClsResponse4.Data.Users;
                ViewBag.Taxs = oClsResponse5.Data.Taxs;
                ViewBag.HoldList = oClsResponse8.Data.Sales;
                ViewBag.PaymentTypes = oClsResponse7.Data.PaymentTypes;
                ViewBag.HoldTotalCount = oClsResponse8.Data.TotalCount;
                ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
                ViewBag.Name = oClsResponse12.Data.CashRegister.Name;
                ViewBag.Branch = oClsResponse12.Data.CashRegister.Branch;
                ViewBag.OpenTime = oClsResponse12.Data.CashRegister.OpenTime;
                ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
                ViewBag.PosSetting = oClsResponse16.Data.PosSetting;
                ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
                ViewBag.Warrantys = oClsResponse21.Data.Warrantys;
                ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
                ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
                ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
                ViewBag.CashRegisterPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash register").FirstOrDefault();
                ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
                ViewBag.Currency = oClsResponse26.Data.Currency;
                ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
                ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
                ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
                ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
                ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
                ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
                ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
                ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
                ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
                ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
                ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

                ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

                return View();
            }
        }

        public async Task<ActionResult> PosAdd(long? BranchId)
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.PageIndex = 1;
                obj.PageSize = 100000000;

            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            ClsCashRegister cashRegisterObj = new ClsCashRegister { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var cashRegisterStatusResult = await cashRegisterController.CashRegisterStatusCheck(cashRegisterObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(cashRegisterStatusResult);

            if (oClsResponse12.Data.CashRegister.Status == 2)
            {
                return Redirect("/sales/cashregister");
            }
            else
            {
                if (BranchId == null)
                {
                    ViewBag.BranchId = oClsResponse12.Data.CashRegister.BranchId;
                }
                else
                {
                    ViewBag.BranchId = BranchId;
                }

                ViewBag.StateId = oClsResponse12.Data.CashRegister.StateId;

                obj.BranchId = ViewBag.BranchId;
                obj.IsAdvance = true;

                WebApi.ItemController itemController = new WebApi.ItemController();
                ClsItemBranchMapVm itemPosObj = new ClsItemBranchMapVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
                var itemsPosResult = await itemController.ItemsPos(itemPosObj);
                ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(itemsPosResult);

                WebApi.UserController userController = new WebApi.UserController();
                ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var usersResult = await userController.AllActiveUsers(userObj);
                ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(usersResult);

                WebApi.TaxController taxController = new WebApi.TaxController();
                ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
                ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

                WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
                ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
                ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

                WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
                ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
                var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
                ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

                WebApi.SalesController salesController = new WebApi.SalesController();
                var holdListResult = await salesController.HoldList(obj);
                ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(holdListResult);

                WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
                ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var sellingPriceGroupsResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
                ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupsResult);

                WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
                ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
                ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

                WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
                ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
                ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

                WebApi.OnlinePaymentSettingsController onlinePaymentSettingsController = new WebApi.OnlinePaymentSettingsController();
                ClsOnlinePaymentSettingsVm onlinePaymentSettingsObj = new ClsOnlinePaymentSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var onlinePaymentSettingsResult = await onlinePaymentSettingsController.ActiveOnlinePaymentSettings(onlinePaymentSettingsObj);
                ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentSettingsResult);

                WebApi.MenuController menuController = new WebApi.MenuController();
                ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var permissionResult = await menuController.ControlsPermission(menuObj);
                ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

                WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
                ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var mainCurrencyResult = await userCurrencyMapController.MainCurrency(currencyObj);
                ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

                var planAddonsResult = await menuController.PlanAddons(menuObj);
                ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

                WebApi.AccountController accountController = new WebApi.AccountController();
                ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
                ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

                WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
                ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
                ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

                WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
                ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
                ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

                WebApi.StateController stateController = new WebApi.StateController();
                ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var statesResult = await stateController.ActiveStates(stateObj);
                ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

                WebApi.AdditionalChargeController additionalChargeController = new WebApi.AdditionalChargeController();
                ClsAdditionalChargeVm additionalChargeObj = new ClsAdditionalChargeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var additionalChargesResult = await additionalChargeController.ActiveAdditionalCharges(additionalChargeObj);
                ClsResponse oClsResponse75 = await oCommonController.ExtractResponseFromActionResult(additionalChargesResult);

                WebApi.RewardPointSettingsController rewardPointSettingsController = new WebApi.RewardPointSettingsController();
                ClsRewardPointSettingsVm rewardPointSettingsObj = new ClsRewardPointSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var rewardPointSettingResult = await rewardPointSettingsController.RewardPointSetting(rewardPointSettingsObj);
                ClsResponse oClsResponseRewardPoint = await oCommonController.ExtractResponseFromActionResult(rewardPointSettingResult);

                ViewBag.ItemDetails = oClsResponse3.Data.ItemDetails;
                ViewBag.Users = oClsResponse4.Data.Users;
                ViewBag.HoldList = oClsResponse8.Data.Sales;
                ViewBag.PaymentTypes = oClsResponse7.Data.PaymentTypes;
                ViewBag.HoldTotalCount = oClsResponse8.Data.TotalCount;
                ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
                ViewBag.Name = oClsResponse12.Data.CashRegister.Name;
                ViewBag.Branch = oClsResponse12.Data.CashRegister.Branch;
                ViewBag.OpenTime = oClsResponse12.Data.CashRegister.OpenTime;
                ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
                ViewBag.PosSetting = oClsResponse16.Data.PosSetting;
                ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
                ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
                ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
                ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
                ViewBag.CashRegisterPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash register").FirstOrDefault();
                ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
                ViewBag.Currency = oClsResponse26.Data.Currency;
                ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
                ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;

                ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
                ViewBag.AdditionalCharges = oClsResponse75.Data.AdditionalCharges;
                ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
                ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
                ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
                ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
                ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
                ViewBag.RewardPointSetting = oClsResponseRewardPoint.Data.RewardPointSetting;

                ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
                return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadPosModals()
        {
            ClsSalesVm obj = new ClsSalesVm();
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
                obj.PageIndex = 1;
                obj.PageSize = 100000000;
                obj.IsAdvance = true;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            ClsCashRegister cashRegisterObj = new ClsCashRegister { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var cashRegisterStatusResult = await cashRegisterController.CashRegisterStatusCheck(cashRegisterObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(cashRegisterStatusResult);

            if (oClsResponse12.Data.CashRegister.Status == 2)
            {
                return Redirect("/sales/cashregister");
            }
            else
            {

                ViewBag.BranchId = oClsResponse12.Data.CashRegister.BranchId;

                ViewBag.StateId = oClsResponse12.Data.CashRegister.StateId;

                obj.BranchId = ViewBag.BranchId;

                WebApi.ItemController itemController = new WebApi.ItemController();
                ClsItemBranchMapVm itemPosObj = new ClsItemBranchMapVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
                var itemsPosResult = await itemController.ItemsPos(itemPosObj);
                ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(itemsPosResult);

                WebApi.UserController userController = new WebApi.UserController();
                ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var usersResult = await userController.AllActiveUsers(userObj);
                ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(usersResult);

                WebApi.TaxController taxController = new WebApi.TaxController();
                ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var allTaxResult = await taxController.ActiveAllTaxs(taxObj);
                ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(allTaxResult);

                WebApi.UserGroupController userGroupController = new WebApi.UserGroupController();
                ClsUserGroupVm userGroupObj = new ClsUserGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var userGroupsResult = await userGroupController.ActiveUserGroups(userGroupObj);
                ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(userGroupsResult);

                WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
                ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId, IsAdvance = obj.IsAdvance };
                var paymentTypeResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
                ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(paymentTypeResult);

                //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Sales/HoldList", arr[0], arr[1], arr[2]);
                //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

                WebApi.UserCurrencyMapController userCurrencyMapController = new WebApi.UserCurrencyMapController();
                ClsCurrencyVm activeCurrencysObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var activeCurrencysResult = await userCurrencyMapController.ActiveCurrencys(activeCurrencysObj);
                ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(activeCurrencysResult);

                WebApi.CountryController countryController = new WebApi.CountryController();
                ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var countrysResult = await countryController.ActiveCountrys(countryObj);
                ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(countrysResult);

                WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
                ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var warrantysResult = await warrantyController.ActiveWarrantys(warrantyObj);
                ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(warrantysResult);

                WebApi.AccountController accountController = new WebApi.AccountController();
                ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var accountsResult = await accountController.ActiveAccounts(accountObj);
                ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(accountsResult);

                WebApi.MenuController menuController = new WebApi.MenuController();
                ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var permissionResult = await menuController.ControlsPermission(menuObj);
                ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

                WebApi.CurrencyController currencyController = new WebApi.CurrencyController();
                ClsCurrencyVm currencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var currencysResult = await currencyController.ActiveCurrencys(currencyObj);
                ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(currencysResult);

                ClsCurrencyVm mainCurrencyObj = new ClsCurrencyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var mainCurrencyResult = await userCurrencyMapController.MainCurrency(mainCurrencyObj);
                ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(mainCurrencyResult);

                var planAddonsResult = await menuController.PlanAddons(menuObj);
                ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

                ClsAccount accountDropdownObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var accountDropdownResult = await accountController.ActiveAccountsDropdown(accountDropdownObj);
                ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(accountDropdownResult);

                WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
                ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
                ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

                WebApi.PaymentTermController paymentTermController = new WebApi.PaymentTermController();
                ClsPaymentTermVm paymentTermObj = new ClsPaymentTermVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var paymentTermsResult = await paymentTermController.ActivePaymentTerms(paymentTermObj);
                ClsResponse oClsResponse42 = await oCommonController.ExtractResponseFromActionResult(paymentTermsResult);

                WebApi.TaxExemptionController taxExemptionController = new WebApi.TaxExemptionController();
                ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var taxExemptionsResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
                ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionsResult);

                WebApi.StateController stateController = new WebApi.StateController();
                ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var statesResult = await stateController.ActiveStates(stateObj);
                ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(statesResult);

                WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
                ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var businessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
                ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNamesResult);

                WebApi.BranchController branchMainController = new WebApi.BranchController();
                ClsBranchVm branchMainObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var mainBranchResult = await branchMainController.MainBranch(branchMainObj);
                ClsResponse oClsResponse66 = await oCommonController.ExtractResponseFromActionResult(mainBranchResult);

                ViewBag.ItemDetails = oClsResponse3.Data.ItemDetails;
                ViewBag.Users = oClsResponse4.Data.Users;
                ViewBag.Taxs = oClsResponse5.Data.Taxs;
                ViewBag.UserGroups = oClsResponse6.Data.UserGroups;
                ViewBag.PaymentTypes = oClsResponse7.Data.PaymentTypes;
                //ViewBag.HoldList = oClsResponse8.Data.Sales;
                //ViewBag.HoldTotalCount = oClsResponse8.Data.TotalCount;
                ViewBag.Currencys = oClsResponse13.Data.Currencys;
                ViewBag.Countrys = oClsResponse14.Data.Countrys;
                ViewBag.Warrantys = oClsResponse21.Data.Warrantys;
                ViewBag.Accounts = oClsResponse22.Data.Accounts;
                ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;

                ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
                ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
                ViewBag.CurrencyPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "currency").FirstOrDefault();
                ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
                ViewBag.CashRegisterPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash register").FirstOrDefault();
                ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
                ViewBag.AllCurrencys = oClsResponse25.Data.Currencys;
                ViewBag.Currency = oClsResponse26.Data.Currency;
                ViewBag.PaymentTerms = oClsResponse42.Data.PaymentTerms;
                ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
                ViewBag.PlacesOfSupply = oClsResponse54.Data.States;
                ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
                ViewBag.MainBranch = oClsResponse66.Data.Branch;

                ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
                ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
                ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
                ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
                ViewBag.PaymentTermPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment terms").FirstOrDefault();

                ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
                return PartialView("Components/_posModals");
            }
        }

        public async Task<ActionResult> ItemsPos(ClsItemBranchMapVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemsPosResult = await itemController.ItemsPos(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemsPosResult);

            WebApi.PosSettingsController posSettingsController = new WebApi.PosSettingsController();
            ClsPosSettingsVm posSettingsObj = new ClsPosSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var posSettingsResult = await posSettingsController.PosSetting(posSettingsObj);
            ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(posSettingsResult);

            ViewBag.ItemDetails = oClsResponse.Data.ItemDetails;
            ViewBag.PosSetting = oClsResponse16.Data.PosSetting;

            return PartialView("PartialItemsPos");
        }

        public async Task<ActionResult> BrandsPos(ClsBrandVm obj)
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

            WebApi.BrandController brandController = new WebApi.BrandController();
            var activeBrandsResult = await brandController.ActiveBrands(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeBrandsResult);
            ViewBag.Brands = oClsResponse.Data.Brands;
            return PartialView("PartialBrandsPos");
        }

        public async Task<ActionResult> CategoriesPos(ClsCategoryVm obj)
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

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var activeCategorysResult = await categoryController.ActiveCategorys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCategorysResult);
            ViewBag.Categories = oClsResponse.Data.Categories;
            return PartialView("PartialCategoriesPos");
        }

        public async Task<ActionResult> SubCategoriesPos(ClsSubCategoryVm obj)
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

            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var activeSubCategoriesResult = await subCategoryController.ActiveSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSubCategoriesResult);
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            return PartialView("PartialSubCategoriesPos");
        }

        public async Task<ActionResult> SubSubCategoriesPos(ClsSubSubCategoryVm obj)
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

            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var activeSubSubCategoriesResult = await subSubCategoryController.ActiveSubSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSubSubCategoriesResult);
            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            return PartialView("PartialSubSubCategoriesPos");
        }

        public async Task<ActionResult> PosHoldFetch(ClsSalesVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageSize = 10000000;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            var holdListResult = await salesController.HoldList(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(holdListResult);


            ViewBag.HoldList = oClsResponse.Data.Sales;
            ViewBag.HoldTotalCount = oClsResponse.Data.TotalCount;

            return PartialView("PartialPosHoldList");
        }

        [AllowAnonymous]
        public async Task<ActionResult> Receipt(string InvoiceId)
        {
            ClsSales obj = new ClsSales();
            //string[] arr = { "", "", "" };
            //if (Request.Cookies["data"] != null)
            //{
            //    arr[0] = Request.Cookies["data"]["UserType"];
            //    arr[1] = Request.Cookies["data"]["Token"];
            //    arr[2] = Request.Cookies["data"]["Id"];
            //    obj.AddedBy = Convert.ToInt64(arr[2]);
            //    obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            //    //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            //    obj.InvoiceNo = InvoiceNo;
            //}

            //obj.CompanyId = Id;
            obj.InvoiceId = InvoiceId;

            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSalesVm salesInvoiceObj = new ClsSalesVm { InvoiceId = InvoiceId };
            var invoiceResult = await salesController.Invoice(salesInvoiceObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(invoiceResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            return View();
        }
        [AllowAnonymous]
        public async Task<ActionResult> SalesReturnReceipt(string InvoiceId)
        {
            ClsSales obj = new ClsSales();
            //string[] arr = { "", "", "" };
            //if (Request.Cookies["data"] != null)
            //{
            //    arr[0] = Request.Cookies["data"]["UserType"];
            //    arr[1] = Request.Cookies["data"]["Token"];
            //    arr[2] = Request.Cookies["data"]["Id"];
            //    obj.AddedBy = Convert.ToInt64(arr[2]);
            //    obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            //    //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            //    obj.InvoiceNo = InvoiceNo;
            //}

            //obj.CompanyId = Id;
            obj.InvoiceId = InvoiceId;

            serializer.MaxJsonLength = 2147483644;

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            ClsSalesVm salesReturnInvoiceObj = new ClsSalesVm { InvoiceId = InvoiceId };
            var invoiceResult = await salesReturnController.Invoice(salesReturnInvoiceObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(invoiceResult);

            ViewBag.Sale = oClsResponse.Data.Sale;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            return View();
        }

        public async Task<ActionResult> SalesByUser(ClsSalesVm obj)
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
                obj.SalesType = "sales";
                //obj.Status = "Draft";
                //obj.Title = "Draft";
            }
            serializer.MaxJsonLength = 2147483644;

            //obj.Status = "Due";
            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesByUserResult = await salesController.SalesByUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesByUserResult);

            //obj.Status = "Draft";
            //_json = serializer.Serialize(obj);
            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Sales/SalesByUser", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //obj.Status = "Quotation";
            //_json = serializer.Serialize(obj);
            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Sales/SalesByUser", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            //obj.Status = "Proforma";
            //_json = serializer.Serialize(obj);
            //var res3 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Sales/SalesByUser", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse3 = serializer.Deserialize<ClsResponse>(res3);


            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);


            ViewBag.Finals = oClsResponse.Data.Sales.Where(a => a.Status != "Hold");
            //ViewBag.Drafts = oClsResponse1.Data.Sales;
            //ViewBag.Quotations = oClsResponse2.Data.Sales;
            //ViewBag.Proformas = oClsResponse3.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            return PartialView("PartialRecentTransactions");
        }

        public async Task<ActionResult> FetchCashRegister(ClsSalesVm obj)
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
                obj.Title = "Draft";
            }
            serializer.MaxJsonLength = 2147483644;

            obj.Status = "Due";
            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            ClsCashRegister cashRegisterObj = new ClsCashRegister { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var currentRegisterResult = await cashRegisterController.CurrentRegister(cashRegisterObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(currentRegisterResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var permissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(permissionResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ViewBag.CashRegister = oClsResponse.Data.CashRegister;
            ViewBag.TotalSales = oClsResponse.Data.TotalSales;
            ViewBag.TotalPayment = oClsResponse.Data.TotalPayment;
            ViewBag.TotalCreditSales = oClsResponse.Data.TotalCreditSales;
            ViewBag.TotalChangeReturn = oClsResponse.Data.TotalChangeReturn;
            ViewBag.TotalRefund = oClsResponse.Data.TotalRefund;
            ViewBag.PaymentTypes = oClsResponse.Data.PaymentTypes;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;
            ViewBag.CashRegisterPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash register").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;


            return PartialView("PartialCashRegister");
        }

        public async Task<ActionResult> CloseCashRegister(ClsCashRegisterVm obj)
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

            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            var result = await cashRegisterController.CloseCashRegister(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> PaymentTypes(ClsPaymentTypeVm obj)
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

            obj.IsAdvance = false;
            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            var activePaymentTypesResult = await paymentTypeController.ActivePaymentTypes(obj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(activePaymentTypesResult);

            return Json(oClsResponse5);
        }

        #endregion

        #region Cash Register
        public async Task<ActionResult> CashRegister()
        {
            ClsSalesVm obj = new ClsSalesVm();
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.CashRegisterPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "cash register").FirstOrDefault();

            return View();

        }

        public async Task<ActionResult> CashRegisterInsert(ClsCashRegisterVm obj)
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

            WebApi.CashRegisterController cashRegisterController = new WebApi.CashRegisterController();
            var result = await cashRegisterController.InsertCashRegister(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
        #endregion

        public async Task<ActionResult> ValidateStock(ClsSalesVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var result = await salesController.ValidateStock(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PaymentReceipt(string ReferenceId)
        {
            ClsCustomerPaymentVm obj = new ClsCustomerPaymentVm();
            obj.ReferenceId = ReferenceId;

            serializer.MaxJsonLength = 2147483644;

            WebApi.CustomerPaymentController customerPaymentController = new WebApi.CustomerPaymentController();
            var customerPaymentReceiptResult = await customerPaymentController.CustomerPaymentReceipt(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(customerPaymentReceiptResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Branch = oClsResponse.Data.Branch;
            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;

            // Load booking data if this is a booking payment
            ViewBag.Booking = null;
            if (oClsResponse.Data.BookingId > 0 && oClsResponse.Data.CompanyId > 0)
            {
                try
                {
                    ClsTableBookingVm bookingObj = new ClsTableBookingVm();
                    bookingObj.BookingId = oClsResponse.Data.BookingId;
                    bookingObj.CompanyId = oClsResponse.Data.CompanyId;
                    
                    WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
                    var tableBookingResult = await tableBookingController.TableBooking(bookingObj);
                    ClsResponse oClsResponseBooking = await oCommonController.ExtractResponseFromActionResult(tableBookingResult);
                        if (oClsResponseBooking != null && oClsResponseBooking.Data != null && oClsResponseBooking.Data.Booking != null)
                        {
                            ViewBag.Booking = oClsResponseBooking.Data.Booking;
                    }
                }
                catch
                {
                    // If booking data cannot be loaded, continue without it
                    ViewBag.Booking = null;
                }
            }

            return View();
        }

        public async Task<ActionResult> SalesInvoicesByReference(ClsPurchaseVm obj)
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

            WebApi.SalesController salesController = new WebApi.SalesController();
            var salesInvoicesByRefResult = await salesController.SalesInvoicesByReference(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(salesInvoicesByRefResult);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.OpenSalesInvoicesModal = true;

            return PartialView("PartialSalesInvoices");
        }

        public async Task<ActionResult> UpdateSalesReturnStatus(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.UpdateSalesReturnStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> AvailableCredits(ClsCustomerPaymentVm obj)
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
            var unusedAdvanceBalanceResult = await customerPaymentController.UnusedAdvanceBalance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(unusedAdvanceBalanceResult);

            WebApi.SalesController salesController = new WebApi.SalesController();
            ClsSalesVm saleObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, InvoiceNo = obj.InvoiceNo };
            var saleResult = await salesController.Sale(saleObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(saleResult);

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.Sale = oClsResponse1.Data.Sale;

            ViewBag.InvoiceNo = obj.InvoiceNo;
            ViewBag.GrandTotal = obj.GrandTotal;
            ViewBag.Due = obj.Due;

            return PartialView("PartialAvailableCredits");
        }

        public async Task<ActionResult> SalesReturnSearchItems(ClsSalesReturnVm obj)
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

            WebApi.SalesReturnController salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.SalesReturnSearchItems(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> GetTablesForPos(ClsRestaurantTableVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.RestaurantTableController restaurantTableController = new WebApi.RestaurantTableController();
            var result = await restaurantTableController.GetTables(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> GetBookingsForPos(ClsTableBookingVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.TableBookingController tableBookingController = new WebApi.TableBookingController();
            var result = await tableBookingController.GetBookings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetStandaloneKotsForPos(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.KotController kotController = new WebApi.KotController();
            var result = await kotController.GetStandaloneKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}