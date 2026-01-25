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
    public class SalesReportsController : Controller
    {
        // GET: SalesReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> ItemSales()
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
                //obj.Title = "Item Sales Report";
            }

            var itemController = new WebApi.ItemController();
            var salesDetailsObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId
            };
            var result = await itemController.ItemSalesReport(salesDetailsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            //var res6 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserGroup/ActiveUserGroups", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse6 = serializer.Deserialize<ClsResponse>(res6);

            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Brand/ActiveBrands", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            //var res3 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Category/ActiveCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse3 = serializer.Deserialize<ClsResponse>(res3);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var activeItemController = new WebApi.ItemController();
            var activeItemObj = new ClsItemVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await activeItemController.ActiveItems(activeItemObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

            ViewBag.Type = "Detailed";
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            //ViewBag.UserGroups = oClsResponse6.Data.UserGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.Brands = oClsResponse2.Data.Brands;
            //ViewBag.Categories = oClsResponse3.Data.Categories;

            ViewBag.Items = oClsResponse5.Data.Items;
            //ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item sales report").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalPriceExcTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceExcTax);
            ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ItemSalesFetch(ClsSalesDetailsVm obj)
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

            System.Web.Http.IHttpActionResult result;
            if (obj.Type == "Detailed")
            {
                var itemController = new WebApi.ItemController();
                result = await itemController.ItemSalesReport(obj);
            }
            else if (obj.Type == "DetailedWithPurchase")
            {
                var itemController = new WebApi.ItemController();
                result = await itemController.ItemsReport(obj);
            }
            else if (obj.Type == "Item")
            {
                var itemController = new WebApi.ItemController();
                result = await itemController.SalesByItemReport(obj);
            }
            else if (obj.Type == "Category")
            {
                var categoryController = new WebApi.CategoryController();
                result = await categoryController.SalesByCategoryReport(obj);
            }
            else if (obj.Type == "Brand")
            {
                var brandController = new WebApi.BrandController();
                result = await brandController.SalesByBrandReport(obj);
            }
            else
            {
                var itemController = new WebApi.ItemController();
                result = await itemController.ItemSalesReport(obj);
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Type = obj.Type;
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalPriceExcTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceExcTax);
            ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialItemSales");
        }
        public async Task<ActionResult> SellingPriceGroup(long? BranchId)
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
            }

            var sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var result = await sellingPriceGroupController.SellingPriceGroupReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.totalpaid = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.grandtotal = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.totaldue = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group report").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSales = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalSalesReturn = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SellingPriceGroupFetch(ClsUserVm obj)
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
            var sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var result = await sellingPriceGroupController.SellingPriceGroupReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.totalpaid = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.grandtotal = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.totaldue = oClsResponse.Data.UserReport == null ? 0 : oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.TotalSales = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalSalesReturn = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSellingPriceGroup");
        }
        public async Task<ActionResult> SalesByItem(long? BranchId, string SkuCode)
        {
            ClsSalesDetailsVm obj = new ClsSalesDetailsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }

                obj.SKU = SkuCode;
            }

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesByItemReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var activeItemController = new WebApi.ItemController();
            var activeItemObj = new ClsItemVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await activeItemController.ActiveItems(activeItemObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            ViewBag.Type = "Detailed";
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.Items = oClsResponse5.Data.Items;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SalesByItemFetch(ClsSalesDetailsVm obj)
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

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesByItemReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Type = obj.Type;
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            //ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            //ViewBag.TotalUnitCost = oClsResponse.Data.SalesDetails.Sum(x => x.UnitCost);
            //ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            //ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            //ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesByItem");
        }
        public async Task<ActionResult> SalesDetailsByItem(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesDetailsByItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.ItemName = obj.ItemName;
            return View();
        }
        public async Task<ActionResult> SalesDetailsByItemFetch(ClsSalesDetailsVm obj)
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

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesDetailsByItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesDetailsByItem");
        }
        public async Task<ActionResult> SalesByCategory()
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
                //obj.Title = "Item Sales Report";
            }

            var salesDetailsObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId
            };

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesByCategoryReport(salesDetailsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res6 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserGroup/ActiveUserGroups", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse6 = serializer.Deserialize<ClsResponse>(res6);

            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Brand/ActiveBrands", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            var activeCategoryController = new WebApi.CategoryController();
            var categoryObj = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result3 = await activeCategoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Item/ActiveItems", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

            ViewBag.Type = "Detailed";
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            //ViewBag.UserGroups = oClsResponse6.Data.UserGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.Brands = oClsResponse2.Data.Brands;
            ViewBag.Categories = oClsResponse3.Data.Categories;

            //ViewBag.Items = oClsResponse5.Data.Items;
            //ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            //ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            //ViewBag.TotalUnitCost = oClsResponse.Data.SalesDetails.Sum(x => x.UnitCost);
            //ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            //ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            //ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SalesByCategoryFetch(ClsSalesDetailsVm obj)
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

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesByCategoryReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Type = obj.Type;
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            //ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            //ViewBag.TotalUnitCost = oClsResponse.Data.SalesDetails.Sum(x => x.UnitCost);
            //ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            //ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            //ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesByCategory");
        }
        public async Task<ActionResult> SalesDetailsByCategory(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by category").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.CategoryName = obj.CategoryName;
            return View();
        }
        public async Task<ActionResult> SalesDetailsByCategoryFetch(ClsSalesDetailsVm obj)
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

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by category").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesDetailsByCategory");
        }
        public async Task<ActionResult> SalesByBrand()
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
                //obj.Title = "Item Sales Report";
            }

            var salesDetailsObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId
            };

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesByBrandReport(salesDetailsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res6 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserGroup/ActiveUserGroups", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse6 = serializer.Deserialize<ClsResponse>(res6);

            var activeBrandController = new WebApi.BrandController();
            var brandObj = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result2 = await activeBrandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(result2);

            //var res3 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Category/ActiveCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse3 = serializer.Deserialize<ClsResponse>(res3);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Item/ActiveItems", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            ViewBag.Type = "Detailed";
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            //ViewBag.UserGroups = oClsResponse6.Data.UserGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.Brands = oClsResponse2.Data.Brands;
            //ViewBag.Categories = oClsResponse3.Data.Categories;

            //ViewBag.Items = oClsResponse5.Data.Items;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            //ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            //ViewBag.TotalUnitCost = oClsResponse.Data.SalesDetails.Sum(x => x.UnitCost);
            //ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            //ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            //ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SalesByBrandFetch(ClsSalesDetailsVm obj)
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

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesByBrandReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Type = obj.Type;
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.TotalQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.Quantity);
            //ViewBag.TotalFreeQuantity = oClsResponse.Data.SalesDetails.Sum(x => x.FreeQuantity);
            //ViewBag.TotalUnitCost = oClsResponse.Data.SalesDetails.Sum(x => x.UnitCost);
            //ViewBag.TotalDiscount = oClsResponse.Data.SalesDetails.Sum(x => x.Discount);
            //ViewBag.TotalTaxPercent = oClsResponse.Data.SalesDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.PriceIncTax);
            //ViewBag.TotalAmountIncTax = oClsResponse.Data.SalesDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesByBrand");
        }
        public async Task<ActionResult> SalesDetailsByBrand(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesDetailsByBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by brand").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.BrandName = obj.BrandName;
            return View();
        }
        public async Task<ActionResult> SalesDetailsByBrandFetch(ClsSalesDetailsVm obj)
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

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesDetailsByBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by brand").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesDetailsByBrand");
        }
        public async Task<ActionResult> SalesByCustomer(ClsUserVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            if (obj.Day != 0 && obj.Month != 0 && obj.Year != 0)
            {
                System.DateTime newdate = new System.DateTime(obj.Year, obj.Month, obj.Day);

                obj.FromDate = newdate;
                obj.ToDate = newdate;

                ViewBag.FromDate = obj.FromDate;
                ViewBag.Todate = obj.ToDate;
            }

            var userController = new WebApi.UserController();
            var result = await userController.SalesByCustomerReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var activeUserController = new WebApi.UserController();
            var activeUserObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await activeUserController.AllActiveUsers(activeUserObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalOpeningBalance = oClsResponse.Data.UserReport.Sum(x => x.OpeningBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSalesExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SalesByCustomerFetch(ClsSalesVm obj)
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
            var userVm = new ClsUserVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BranchId = obj.BranchId,
                Search = obj.Search
            };
            var result = await userController.SalesByCustomerReport(userVm);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            //ViewBag.TotalOpeningBalance = oClsResponse.Data.UserReport.Sum(x => x.OpeningBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSalesExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesByCustomer");
        }
        public async Task<ActionResult> SalesReturnByCustomer(ClsUserVm obj)
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
                //obj.Title = "Item Sales Report";
            }
            var userController = new WebApi.UserController();
            var result = await userController.SalesReturnByCustomerReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var activeUserController = new WebApi.UserController();
            var activeUserObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await activeUserController.AllActiveUsers(activeUserObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales return by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturnExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn - x.TotalTaxAmount);
            ViewBag.TotalSalesReturnIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.UserReport.Sum(x => x.TotalAmountRemaining);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> SalesReturnByCustomerFetch(ClsSalesVm obj)
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
            var userVm = new ClsUserVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BranchId = obj.BranchId,
                Search = obj.Search
            };
            var result = await userController.SalesReturnByCustomerReport(userVm);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.UserReport = oClsResponse.Data.UserReport;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales return by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturnExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn - x.TotalTaxAmount);
            ViewBag.TotalSalesReturnIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.UserReport.Sum(x => x.TotalAmountRemaining);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesReturnByCustomer");
        }
        public async Task<ActionResult> SalesDetailsByCustomer(ClsSalesVm obj)
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
                //obj.Title = "Item Sales Report";
            }
            var salesController = new WebApi.SalesController();
            var result = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var activeUserController = new WebApi.UserController();
            var activeUserObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await activeUserController.AllActiveUsers(activeUserObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.Sales = oClsResponse.Data.Sales;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by customer").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesExcTax = oClsResponse.Data.Sales.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.CustomerName = obj.CustomerName;
            ViewBag.Page = obj.Page;
            return View();
        }
        public async Task<ActionResult> SalesDetailsByCustomerFetch(ClsSalesVm obj)
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
            var salesController = new WebApi.SalesController();
            var result = await salesController.AllSales(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Sales = oClsResponse.Data.Sales;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesExcTax = oClsResponse.Data.Sales.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.Sales.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Sales.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Sales.Sum(x => x.Due);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesDetailsByCustomer");
        }
        public async Task<ActionResult> WarrantyExpiryReport()
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
                //obj.Title = "Item Sales Report";
            }
            obj.UserType = "customer";
            var salesDetailsObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };

            var salesController = new WebApi.SalesController();
            var result = await salesController.WarrantyExpiryReport(salesDetailsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var userController = new WebApi.UserController();
            var activeUserObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController.AllActiveUsers(activeUserObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result8 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(result8);

            ViewBag.Type = "Detailed";
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.Items = oClsResponse5.Data.Items;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> WarrantyExpiryReportFetch(ClsSalesDetailsVm obj)
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

            var salesController = new WebApi.SalesController();
            var result = await salesController.WarrantyExpiryReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Type = obj.Type;
            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialWarrantyExpiryReport");
        }
        public async Task<ActionResult> SalesReturnDetailsByItem(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesReturnDetailsByItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.ItemName = obj.ItemName;
            return View();
        }
        public async Task<ActionResult> SalesReturnDetailsByItemFetch(ClsSalesDetailsVm obj)
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

            var itemController = new WebApi.ItemController();
            var result = await itemController.SalesReturnDetailsByItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by item").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesReturnDetailsByItem");
        }
        public async Task<ActionResult> SalesReturnDetailsByCategory(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesReturnDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by category").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.CategoryName = obj.CategoryName;
            return View();
        }
        public async Task<ActionResult> SalesReturnDetailsByCategoryFetch(ClsSalesDetailsVm obj)
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

            var categoryController = new WebApi.CategoryController();
            var result = await categoryController.SalesReturnDetailsByCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by category").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesReturnDetailsByCategory");
        }
        public async Task<ActionResult> SalesReturnDetailsByBrand(ClsSalesDetailsVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesReturnDetailsByBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by brand").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.BrandName = obj.BrandName;
            return View();
        }
        public async Task<ActionResult> SalesReturnDetailsByBrandFetch(ClsSalesDetailsVm obj)
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

            var brandController = new WebApi.BrandController();
            var result = await brandController.SalesReturnDetailsByBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales by brand").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesReturnDetailsByBrand");
        }
        public async Task<ActionResult> SalesReturnDetailsByCustomer(ClsSalesVm obj)
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
                //obj.Title = "Item Sales Report";
            }

            var salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.AllSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(result1);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            ViewBag.SalesReturns = oClsResponse.Data.SalesReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales return by customer").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesExcTax = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.SalesReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.SalesReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.SalesReturns.Sum(x => x.AmountRemaining);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.CustomerName = obj.CustomerName;

            ViewBag.Page = obj.Page;
            return View();
        }
        public async Task<ActionResult> SalesReturnDetailsByCustomerFetch(ClsSalesVm obj)
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

            var salesReturnController = new WebApi.SalesReturnController();
            var result = await salesReturnController.AllSalesReturn(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.SalesReturns = oClsResponse.Data.SalesReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales return by customer").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesExcTax = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.SalesReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.SalesReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.SalesReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.SalesReturns.Sum(x => x.AmountRemaining);

            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialSalesReturnDetailsByCustomer");
        }
        public async Task<ActionResult> SalesByBusinessLocation()
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> SalesByBusinessLocationFetch(ClsAccountVm obj)
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> SalesByPaymentModes()
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> SalesByPaymentModesFetch(ClsAccountVm obj)
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> CustomerGroupReport()
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> CustomerGroupReportFetch(ClsAccountVm obj)
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> SalesRegister()
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> SalesRegisterFetch(ClsAccountVm obj)
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> FreeSupplies()
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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
        public async Task<ActionResult> FreeSuppliesFetch(ClsAccountVm obj)
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

            var taxObj = new ClsSalesVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, TaxSettingId = obj.TaxSettingId };
            var taxController = new WebApi.TaxController();
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