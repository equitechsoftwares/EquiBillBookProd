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
    public class SubscriptionController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Subscription

        public async Task<ActionResult> Index()
        {
            ClsTransaction transactionObj = new ClsTransaction();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                transactionObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.CurrentPlan(transactionObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = Convert.ToInt64(arr[2]), CompanyId = transactionObj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.MyPlan = oClsResponse.Data.MyPlan;
            ViewBag.UpgradePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade plan").FirstOrDefault();
            //ViewBag.UpgradeAddonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade addons").FirstOrDefault();
            ViewBag.ActivatePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "activate plan").FirstOrDefault();

            ViewBag.IsPurchaseTaken = oClsResponse.Data.MyPlan.PlanAddons.Where(a => a.Type == "Purchase").Select(a => a.IsTaken).FirstOrDefault();
            ViewBag.IsDomainTaken = oClsResponse.Data.MyPlan.PlanAddons.Where(a => a.Type == "Domain").Select(a => a.IsTaken).FirstOrDefault();
            ViewBag.IsCatalogueTaken = oClsResponse.Data.MyPlan.PlanAddons.Where(a => a.Type == "Catalogue").Select(a => a.IsTaken).FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> UpgradePlan()
        {
            ClsPlanVm planObj = new ClsPlanVm();
            ClsTermLengthVm termLengthObj = new ClsTermLengthVm();
            ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm();
            ClsCountryVm countryObj = new ClsCountryVm();
            ClsOnlinePaymentSettingsVm onlinePaymentObj = new ClsOnlinePaymentSettingsVm();
            ClsUserVm userObj = new ClsUserVm();
            ClsCouponVm couponObj = new ClsCouponVm();
            //obj.Domain = "pos.neelkanthbakery.com";//Request.Url.Host.Replace("www.", "");
            string domain = Request.Url.Host.Replace("www.", "");
            termLengthObj.Domain = domain;
            planAddonsObj.Domain = domain;
            countryObj.Domain = domain;
            onlinePaymentObj.Domain = domain;
            userObj.Domain = domain;
            couponObj.Domain = domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                planObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                termLengthObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                planAddonsObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                onlinePaymentObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                userObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                userObj.AddedBy = Convert.ToInt64(arr[2]);
                couponObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                couponObj.AddedBy = Convert.ToInt64(arr[2]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            termLengthObj.TermLengthId = 4;
            planAddonsObj.TermLengthId = 4;

            PlanController planController = new PlanController();
            var planResult = await planController.ActivePlans(planObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(planResult);

            TermLengthController termLengthController = new TermLengthController();
            var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(termLengthResult);

            PlanAddonsController planAddonsController = new PlanAddonsController();
            var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            CountryController countryController = new CountryController();
            var countryResult = await countryController.MainCountry(countryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            OnlinePaymentSettingsController onlinePaymentController = new OnlinePaymentSettingsController();
            var onlinePaymentResult = await onlinePaymentController.ActiveOnlinePaymentSettingsAdmin(onlinePaymentObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentResult);

            UserController userController = new UserController();
            var userResult21 = await userController.FetchRootCompany(userObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(userResult21);

            var userResult22 = await userController.FetchWhitelabelByDomain(userObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(userResult22);

            CouponController couponController = new CouponController();
            var couponResult = await couponController.ActiveCoupons(couponObj);
            ClsResponse oClsResponse41 = await oCommonController.ExtractResponseFromActionResult(couponResult);

            ViewBag.Plans = oClsResponse.Data.Plans;
            ViewBag.TermLengths = oClsResponse5.Data.TermLengths;
            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            ViewBag.IsAutoFetch = true;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.Coupons = oClsResponse41.Data.Coupons;
            //if (oClsResponse21.Data.User.UserId == oClsResponse22.Data.User.UserId)
            //{
            //    ViewBag.ShowCoupon = true;
            //}
            //else
            //{
            //    ViewBag.ShowCoupon = false;
            //}

            //ViewBag.ShowCoupon = false;

            return View();
        }

        public async Task<ActionResult> MyTransactions(ClsUserVm obj)
        {
            ClsTransactionVm transactionObj = new ClsTransactionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                transactionObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                transactionObj.PageIndex = 1;
                transactionObj.Domain = Request.Url.Host.Replace("www.", "");
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
                //obj.PageSize = 10;
                //obj.Title = "Country";
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.AllTransactions(transactionObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);


            ViewBag.Transactions = oClsResponse.Data.Transactions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            //ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            //ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "my billings").FirstOrDefault();

            ViewBag.TotalPayableCost = oClsResponse.Data.Transactions.Sum(x => x.PayableCost);

            return View();
        }

        public async Task<ActionResult> MyTransactionsFetch(ClsUserVm obj)
        {
            ClsTransactionVm transactionObj = new ClsTransactionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                transactionObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                transactionObj.PageIndex = obj.PageIndex;
                transactionObj.PageSize = obj.PageSize;
                transactionObj.Domain = Request.Url.Host.Replace("www.", "");
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
                //obj.Title = "Country";
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.AllTransactions(transactionObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);


            ViewBag.Transactions = oClsResponse.Data.Transactions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            //ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            //ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "my billings").FirstOrDefault();

            ViewBag.TotalPayableCost = oClsResponse.Data.Transactions.Sum(x => x.PayableCost);

            return PartialView("PartialMyTransactions");
        }

        public async Task<ActionResult> InsertTransaction(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InsertTransaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UpgradeAddons()
        {
            ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm();
            ClsCountryVm countryObj = new ClsCountryVm();
            ClsOnlinePaymentSettingsVm onlinePaymentObj = new ClsOnlinePaymentSettingsVm();
            ClsUserVm userObj = new ClsUserVm();
            ClsCouponVm couponObj = new ClsCouponVm();
            string domain = Request.Url.Host.Replace("www.", "");
            planAddonsObj.Domain = domain;
            countryObj.Domain = domain;
            onlinePaymentObj.Domain = domain;
            userObj.Domain = domain;
            couponObj.Domain = domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                planAddonsObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                onlinePaymentObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                userObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                userObj.AddedBy = Convert.ToInt64(arr[2]);
                couponObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                couponObj.AddedBy = Convert.ToInt64(arr[2]);
                ////ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            PlanAddonsController planAddonsController = new PlanAddonsController();
            var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            CountryController countryController = new CountryController();
            var countryResult = await countryController.MainCountry(countryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            OnlinePaymentSettingsController onlinePaymentController = new OnlinePaymentSettingsController();
            var onlinePaymentResult = await onlinePaymentController.ActiveOnlinePaymentSettingsAdmin(onlinePaymentObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentResult);

            UserController userController = new UserController();
            var userResult21 = await userController.FetchRootCompany(userObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(userResult21);

            var userResult22 = await userController.FetchWhitelabelByDomain(userObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(userResult22);

            CouponController couponController = new CouponController();
            var couponResult = await couponController.ActiveCoupons(couponObj);
            ClsResponse oClsResponse41 = await oCommonController.ExtractResponseFromActionResult(couponResult);

            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.MonthsLeft = oClsResponse10.Data.MonthsLeft;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            ViewBag.Coupons = oClsResponse41.Data.Coupons;
            //if (oClsResponse21.Data.User.UserId == oClsResponse22.Data.User.UserId)
            //{
            //    ViewBag.ShowCoupon = true;
            //}
            //else
            //{
            //    ViewBag.ShowCoupon = false;
            //}

            //ViewBag.ShowCoupon = false;

            return View();
        }

        public async Task<ActionResult> FetchAdons(ClsPlanAddonsVm obj)
        {
            ClsCountryVm countryObj = new ClsCountryVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            countryObj.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                ////ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            PlanAddonsController planAddonsController = new PlanAddonsController();
            var planAddonsResult = await planAddonsController.ActivePlanAddons(obj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            CountryController countryController = new CountryController();
            var countryResult = await countryController.MainCountry(countryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(countryResult);

            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.MonthsLeft = oClsResponse10.Data.MonthsLeft;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            ViewBag.IsAutoFetch = obj.IsAutoFetch;
            return PartialView("PartialAddons");
        }

        public async Task<ActionResult> Invoice(string invoiceid)
        {
            ClsTransactionVm transactionObj = new ClsTransactionVm();
            ClsOnlinePaymentSettingsVm onlinePaymentObj = new ClsOnlinePaymentSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                transactionObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                transactionObj.TransactionNo = invoiceid;
                transactionObj.Domain = Request.Url.Host.Replace("www.", "");
                onlinePaymentObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                onlinePaymentObj.Domain = Request.Url.Host.Replace("www.", "");
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.Transaction(transactionObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = Convert.ToInt64(arr[2]), CompanyId = transactionObj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            OnlinePaymentSettingsController onlinePaymentController = new OnlinePaymentSettingsController();
            var onlinePaymentResult = await onlinePaymentController.ActiveOnlinePaymentSettingsAdmin(onlinePaymentObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentResult);

            ViewBag.Transaction = oClsResponse.Data.Transaction;
            ViewBag.UpgradePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade plan").FirstOrDefault();
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            return View();
        }

        public async Task<ActionResult> InvoiceFetch(ClsTransactionVm obj)
        {
            ClsOnlinePaymentSettingsVm onlinePaymentObj = new ClsOnlinePaymentSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
                onlinePaymentObj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                onlinePaymentObj.Domain = Request.Url.Host.Replace("www.", "");
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.Transaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = Convert.ToInt64(arr[2]), CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            OnlinePaymentSettingsController onlinePaymentController = new OnlinePaymentSettingsController();
            var onlinePaymentResult = await onlinePaymentController.ActiveOnlinePaymentSettingsAdmin(onlinePaymentObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(onlinePaymentResult);

            ViewBag.Transaction = oClsResponse.Data.Transaction;
            ViewBag.UpgradePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "upgrade plan").FirstOrDefault();
            ViewBag.OnlinePaymentSettings = oClsResponse20.Data.OnlinePaymentSettings;
            return PartialView("PartialInvoice");
        }

        public async Task<ActionResult> PaymentSuccess(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.PaymentSuccess(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> Success(string invoiceid)
        {
            ClsTransactionVm obj = new ClsTransactionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.TransactionNo = invoiceid;
            }

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.Transaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.Transaction = oClsResponse.Data.Transaction;

            //ViewBag.ActivatePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "activate plan").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> ActivatePlan(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.ActivatePlan(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> InsertAddonTransaction(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InsertAddonTransaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> InitPaymentGateway(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InitPaymentGateway(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> InitPlanPrice(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InitPlanPrice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> InitAddonPrice(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InitAddonPrice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> InsertTransaction_Paid(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InsertTransaction_Paid(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> InsertAddonTransaction_Paid(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.InsertAddonTransaction_Paid(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> TransactionDelete(ClsTransactionVm obj)
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

            TransactionController transactionController = new TransactionController();
            var transactionResult = await transactionController.TransactionDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ApplyCoupon(ClsCouponVm obj)
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

            CouponController couponController = new CouponController();
            var couponResult = await couponController.ApplyCoupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(couponResult);
            return Json(oClsResponse);
        }

    }
}