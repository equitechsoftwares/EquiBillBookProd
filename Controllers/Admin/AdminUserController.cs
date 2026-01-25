using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminUserController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: AdminUser
        public async Task<ActionResult> Index(long? UserId, long? Under)
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                obj.UserType = "user";
                //obj.Title = "Users";
                obj.UserId = Convert.ToInt64(UserId);
                obj.Under = Convert.ToInt64(Under);
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllCompanies(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var result1 = await userController.FetchRootCompany(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.RootUserId = oClsResponse1.Data.User.UserId;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }

        public async Task<ActionResult> UserFetch(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = "user";
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllCompanies(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var result1 = await userController.FetchRootCompany(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.RootUserId = oClsResponse1.Data.User.UserId;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialUser");
        }

        public async Task<ActionResult> WhitelabelResellers()
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllWhitelabelResellers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }

        public async Task<ActionResult> WhitelabelResellersFetch(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllWhitelabelResellers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialWhitelabelResellers");
        }

        public async Task<ActionResult> Resellers(long? Under)
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
                //obj.UserId = Convert.ToInt64(UserId);
                obj.Under = Convert.ToInt64(Under);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllResellers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }

        public async Task<ActionResult> ResellersFetch(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.AllResellers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialResellers");
        }

        public async Task<ActionResult> MyTransactions(long? UserId, long? Under)
        {
            ClsTransactionVm obj = new ClsTransactionVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                if (UserId != null)
                {
                    obj.UserId = Convert.ToInt64(UserId);
                    obj.Under = Convert.ToInt64(Under);
                }
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.AllTransactionsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Transactions = oClsResponse.Data.Transactions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;
            return View();
        }

        public async Task<ActionResult> MyTransactionsFetch(ClsTransactionVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                //obj.Title = "Country";
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.AllTransactionsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Transactions = oClsResponse.Data.Transactions;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return PartialView("PartialMyTransactions");
        }

        public async Task<ActionResult> invoice(string invoiceid)
        {
            ClsTransactionVm obj = new ClsTransactionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["adata"]["CurrencySymbol"];
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.TransactionNo = invoiceid;
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.Transaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ClsDomainVm domainObj = new ClsDomainVm();
            domainObj.Domain = obj.Domain;
            domainObj.AddedBy = obj.AddedBy;
            domainObj.CompanyId = obj.CompanyId;
            WebApi.DomainController domainController = new WebApi.DomainController();
            var result1 = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Transaction = oClsResponse.Data.Transaction;
            ViewBag.UserType = oClsResponse1.UserType;

            return View();
        }

        public async Task<ActionResult> PaymentSuccess(ClsTransactionVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.PaymentSuccessAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> Success(string invoiceid)
        {
            ClsTransactionVm obj = new ClsTransactionVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["adata"]["CurrencySymbol"];
                obj.TransactionNo = invoiceid;
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.Transaction(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.Transaction = oClsResponse.Data.Transaction;

            //ViewBag.ActivatePlanPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "activate plan").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> SalesSummary(long? UserId, long? Under)
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            ViewBag.Domain = obj.Domain;
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.UserType = "user";
                //obj.Title = "Users";
                if (UserId == null)
                {
                    obj.UserId = obj.AddedBy;
                }
                else
                {
                    obj.UserId = Convert.ToInt64(UserId);
                }

                obj.Under = Convert.ToInt64(Under);
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.TransactionSummaryAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            WebApi.UserController userController = new WebApi.UserController();
            var result1 = await userController.FetchRootCompany(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            ViewBag.Transactions = oClsResponse.Data.Transactions;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.RootUserId = oClsResponse1.Data.User.UserId;


            if (ViewBag.AddedBy == ViewBag.RootUserId)
            {
                ViewBag.canPay = true;
            }
            else if (arr[0].ToLower() == "reseller")
            {
                ViewBag.canPay = false;
            }
            else if (arr[0].ToLower() == "whitelabel reseller")
            {
                if (UserId == null)
                {
                    ViewBag.canPay = false;
                }
                else
                {
                    ViewBag.canPay = true;
                }
            }

            ViewBag.UserType = arr[0].ToLower();

            return View();
        }

        public async Task<ActionResult> SalesByMonth(int Month, int Year, long UserId, long? Under)
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.UserType = "user";
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
                obj.Month = Month;
                obj.Year = Year;
                obj.UserId = UserId;
                obj.Under = Convert.ToInt64(Under);
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.TransactionByMonthAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Transactions = oClsResponse.Data.Transactions;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }

        public async Task<ActionResult> UserActiveInactive(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserActiveInactiveAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ResellerRenew(ClsResellerPaymentVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);

            }
            WebApi.ResellerPaymentController resellerPaymentController = new WebApi.ResellerPaymentController();
            var result = await resellerPaymentController.ResellerRenew(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ResellerPaymentInsert(ClsResellerPaymentVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);

            }
            WebApi.ResellerPaymentController resellerPaymentController = new WebApi.ResellerPaymentController();
            var result = await resellerPaymentController.ResellerPaymentInsert(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ResellerCommissionPercentUpdate(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);

            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.ResellerCommissionPercentUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CheckMaxSpecialDiscount(ClsTransactionVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.CheckMaxSpecialDiscount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UpdateSpecialDiscount(ClsTransactionVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.TransactionController transactionController = new WebApi.TransactionController();
            var result = await transactionController.UpdateSpecialDiscount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> UserLogin(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);

                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.UserController userController = new WebApi.UserController();
            var result = await userController.UserLoginFromAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            if (oClsResponse.Data.User != null)
            {
                int days = 0;
                //if (obj.IsRememberMe == true)
                //{
                    days = 365;
                //}
                //else
                //{
                //    days = 1;
                //}

                Response.Cookies["data"]["Token"] = oClsResponse.Data.User.Token;
                //Response.Cookies["data"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
                Response.Cookies["data"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
                Response.Cookies["data"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
                Response.Cookies["data"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
                Response.Cookies["data"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
                //if (obj.Browser.ToLower() == "safari")
                //{
                //    Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencyCode + " ");
                //}
                //else
                //{
                Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
                //}
                Response.Cookies["data"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
                Response.Cookies["data"]["IsDemo"] = "False";
                Response.Cookies["data"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
                Response.Cookies["data"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
                Response.Cookies["data"]["WhitelabelBusinessIcon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessIcon);
                Response.Cookies["data"]["WhitelabelFavicon"] = Convert.ToString(oClsResponse.Data.User.WhitelabelFavicon);
                Response.Cookies["data"].Expires = DateTime.Today.AddDays(days);


                //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DefaultProfitPercent);
                Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DateFormat);
                Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.TimeFormat);
                Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CurrencySymbolPlacement);
                Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["SystemSetting"]["ShowHelpText"] = Convert.ToString(oClsResponse.Data.BusinessSetting.ShowHelpText);
                Response.Cookies["SystemSetting"]["EnableDarkMode"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableDarkMode);
                Response.Cookies["SystemSetting"]["FixedHeader"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedHeader);
                Response.Cookies["SystemSetting"]["FixedFooter"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedFooter);
                if (obj.Browser.ToLower() == "safari")
                {
                    Response.Cookies["SystemSetting"]["EnableSound"] = "False";
                }
                else
                {
                    Response.Cookies["SystemSetting"]["EnableSound"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableSound);
                }
                Response.Cookies["SystemSetting"]["CollapseSidebar"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CollapseSidebar);
                Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(days);

                foreach (var item in oClsResponse.Data.ShortCutKeySettings)
                {
                    Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
                }
                //Response.Cookies["ShortCutKeySetting"]["AddNewForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.AddNewForm);
                //Response.Cookies["ShortCutKeySetting"]["SaveForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveForm);
                //Response.Cookies["ShortCutKeySetting"]["SaveAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveAddAnother);
                //Response.Cookies["ShortCutKeySetting"]["UpdateForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateForm);
                //Response.Cookies["ShortCutKeySetting"]["UpdateAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateAddAnother);
                //Response.Cookies["ShortCutKeySetting"]["GoBack"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.GoBack);
                Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(days);

                Response.Cookies["ItemSetting"]["ExpiryDateFormat"] = Convert.ToString(oClsResponse.Data.ItemSetting.ExpiryDateFormat);
                Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(days);
            }
            return Redirect("/dashboard?type=login");
        }
    }
}