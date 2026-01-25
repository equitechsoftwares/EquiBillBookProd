using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class EmployeeReportsController : Controller
    {
        // GET: EmployeeReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> Register()
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
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Item Purchase Report";
                obj.IsAdvance = true;
            }
            obj.UserType = "user";
            var cashRegisterController = new WebApi.CashRegisterController();
            var cashRegisterObj = new ClsCashRegisterVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, BranchId = obj.BranchId, FromDate = obj.FromDate, ToDate = obj.ToDate };
            var result = await cashRegisterController.CashRegisterReport(cashRegisterObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId };
            var result11 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId, UserType = obj.UserType };
            var result1 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.CashRegisters = oClsResponse.Data.CashRegisters;
            ViewBag.FooterTotal = oClsResponse.Data.PaymentTypes;
            ViewBag.PaymentTypes = oClsResponse11.Data.PaymentTypes;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.totalpaid = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.Paid);
            //ViewBag.grandtotal = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.GrandTotal);
            //ViewBag.totaldue = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.Due);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "register report").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalOpeningBalance = oClsResponse.Data.CashRegisters.Sum(x => x.OpeningBalance);
            ViewBag.TotalSales = oClsResponse.Data.CashRegisters.Sum(x => x.TotalSales);
            ViewBag.TotalPayment = oClsResponse.Data.CashRegisters.Sum(x => x.TotalPayment);
            ViewBag.TotalCreditSales = oClsResponse.Data.CashRegisters.Sum(x => x.TotalCreditSales);
            ViewBag.TotalChangeReturn = oClsResponse.Data.CashRegisters.Sum(x => x.ChangeReturn);
            ViewBag.TotalClosingBalance = oClsResponse.Data.CashRegisters.Sum(x => x.ClosingBalance);

            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.BranchPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "branch").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> RegisterReportFetch(ClsCashRegisterVm obj)
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
            var cashRegisterController = new WebApi.CashRegisterController();
            var result = await cashRegisterController.CashRegisterReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var paymentTypeController = new WebApi.PaymentTypeController();
            var paymentTypeObj = new ClsPaymentTypeVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId };
            var result11 = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(result11);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.CashRegisters = oClsResponse.Data.CashRegisters;
            ViewBag.PaymentTypes = oClsResponse11.Data.PaymentTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.FooterTotal = oClsResponse.Data.PaymentTypes;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalCashInHand = oClsResponse.Data.CashRegisters.Sum(x => x.CashInHand);
            ViewBag.TotalChangeReturn = oClsResponse.Data.CashRegisters.Sum(x => x.ChangeReturn);

            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();
            ViewBag.BranchPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "branch").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialRegister");
        }
    }
}