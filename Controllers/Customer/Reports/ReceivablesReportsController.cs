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
    public class ReceivablesReportsController : Controller
    {
        // GET: ReceivablesReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> Customer(long? BranchId, string MobileNo)
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
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }

                obj.MobileNo = MobileNo;
            }
            obj.UserType = "customer";
            
            var userController = new WebApi.UserController();
            var result = await userController.CustomerReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController2 = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController2.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var userGroupController = new WebApi.UserGroupController();
            var userGroupObj = new ClsUserGroupVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result6 = await userGroupController.ActiveUserGroups(userGroupObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(result6);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.UserGroups = oClsResponse6.Data.UserGroups;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer report").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalAdvanceBalance = oClsResponse.Data.UserReport.Sum(x => x.AdvanceBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSales = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturn = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> CustomersFetch(ClsUserVm obj)
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
            obj.UserType = "customer";
            
            var userController = new WebApi.UserController();
            var result = await userController.CustomerReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalAdvanceBalance = oClsResponse.Data.UserReport.Sum(x => x.AdvanceBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSales = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturn = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialCustomers");
        }
        public async Task<ActionResult> CustomerLedger(ClsSalesVm obj)
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
            }
            
            var userController = new WebApi.UserController();
            var result = await userController.CustomerLedger(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.User = oClsResponse.Data.User;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer statement").FirstOrDefault();
            ViewBag.LedgerDiscountPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer discounts").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalDebit = oClsResponse.Data.Sales.Sum(x => x.Debit);
            ViewBag.TotalCredit = oClsResponse.Data.Sales.Sum(x => x.Credit);
            ViewBag.TotalBalance = oClsResponse.Data.Sales.Sum(x => x.Balance);

            ViewBag.BranchId = obj.BranchId;
            ViewBag.CustomerName = obj.CustomerName;

            return View();
        }
        public async Task<ActionResult> CustomerLedgerFetch(ClsSalesVm obj)
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
            var userController = new WebApi.UserController();
            var result = await userController.CustomerLedger(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.User = oClsResponse.Data.User;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer statement").FirstOrDefault();

            ViewBag.TotalDebit = oClsResponse.Data.Sales.Sum(x => x.Debit);
            ViewBag.TotalCredit = oClsResponse.Data.Sales.Sum(x => x.Credit);
            ViewBag.TotalBalance = oClsResponse.Data.Sales.Sum(x => x.Balance);

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialCustomerLedger");
        }
        public async Task<ActionResult> CustomerBalances()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> CustomerBalancesFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> CustomerBalanceSummary()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> CustomerBalanceSummaryFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> ReceivableSummary()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> ReceivableSummaryFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> ReceivableDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> ReceivableDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> SalesQuotationDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> SalesQuotationDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> SalesOrderDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> SalesOrderDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> SalesProformaDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> SalesProformaDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> DeliveryChallanDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> DeliveryChallanDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> SalesDetails()
        {
            ClsAccountVm obj = new ClsAccountVm();
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
            }
            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> SalesDetailsFetch(ClsAccountVm obj)
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
            var taxController = new WebApi.TaxController();
            ClsSalesVm taxObj = new ClsSalesVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await taxController.B2B(taxObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result39 = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(result39);

            var taxTypeController = new WebApi.TaxTypeController();
            var taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var result51 = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(result51);

            var businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            var businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result55 = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(result55);

            var taxSettingController = new WebApi.TaxSettingController();
            var taxSettingObj = new ClsTaxSettingVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result56 = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(result56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
    }
}