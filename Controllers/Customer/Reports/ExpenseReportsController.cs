using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class ExpenseReportsController : Controller
    {
        // GET: ExpenseReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> ExpenseDetails(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.GrandTotal).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseDetailsFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);


            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.Subtotal).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.GrandTotal).DefaultIfEmpty().Sum();


            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseDetails");
        }
        public async Task<ActionResult> ExpenseSummaryByCategory(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();


            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseSummaryByCategoryFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseSummaryByExpenseAccount");
        }
        public async Task<ActionResult> ExpenseDetailsByCategory(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.AccountName = obj.AccountName;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseDetailsByCategoryFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseDetailsByCategory");
        }
        public async Task<ActionResult> ExpenseSummaryByCustomer(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByCustomer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseSummaryByCustomerFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByCustomer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseSummaryByCustomer");
        }
        public async Task<ActionResult> ExpenseDetailsByCustomer(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByCustomer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.CustomerName = obj.CustomerName;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseDetailsByCustomerFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByCustomer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);


            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseDetailsByCustomer");
        }
        public async Task<ActionResult> ExpenseSummaryBySupplier(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryBySupplier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseSummaryBySupplierFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryBySupplier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseSummaryBySupplier");
        }
        public async Task<ActionResult> ExpenseDetailsBySupplier(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsBySupplier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.SupplierName = obj.SupplierName;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseDetailsBySupplierFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsBySupplier(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);


            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseDetailsBySupplier");
        }
        public async Task<ActionResult> ExpenseSummaryByUser(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();
            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseSummaryByUserFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseSummaryByUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.UsersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.TotalExpenseCount = oClsResponse.Data.Expenses.Where(a => a.AmountExcTax != 0).Select(a => a.AmountExcTax).Count();
            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseSummaryByUser");
        }
        public async Task<ActionResult> ExpenseDetailsByUser(ClsExpenseVm obj)
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
                ViewBag.BranchId = obj.BranchId;
            }

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseCategory/ActiveExpenseCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Expenses = oClsResponse.Data.Expenses;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.ExpenseCategories = oClsResponse1.Data.ExpenseCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "expense details").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.UserName = obj.UserName;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ExpenseDetailsByUserFetch(ClsExpenseVm obj)
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

            var expenseController = new WebApi.ExpenseController();
            var result = await expenseController.ExpenseDetailsByUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);


            ViewBag.Expenses = oClsResponse.Data.Expenses;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.TotalAmount = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalAmountExcTax = oClsResponse.Data.Expenses.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum();
            ViewBag.TotalAmountIncTax = oClsResponse.Data.Expenses.Select(a => a.Amount).DefaultIfEmpty().Sum();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialExpenseDetailsByUser");
        }

    }
}