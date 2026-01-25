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
    public class PurchaseReportsController : Controller
    {
        // GET: PurchaseReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> ItemPurchase()
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
            }
            obj.UserType = "supplier";

            var purchaseController = new WebApi.PurchaseController();
            ClsPurchaseVm purchaseObj = new ClsPurchaseVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await purchaseController.ItemPurchaseReport(purchaseObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result3 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Brand/ActiveBrands", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res2 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Category/ActiveCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse2 = serializer.Deserialize<ClsResponse>(res2);

            //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var itemController = new WebApi.ItemController();
            var itemObj = new ClsItemVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await itemController.ActiveItems(itemObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse3.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;
            //ViewBag.Brands = oClsResponse1.Data.Brands;
            //ViewBag.Categories = oClsResponse2.Data.Categories;

            ViewBag.Items = oClsResponse5.Data.Items;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.totalpaid = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.Paid);
            //ViewBag.grandtotal = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.GrandTotal);
            //ViewBag.totaldue = oClsResponse.Data.PurchaseDetails == null ? 0 : oClsResponse.Data.PurchaseDetails.Sum(x => x.Due);

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item purchase report").FirstOrDefault();
            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalAdjustedQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.AdjustedQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ItemPurchaseFetch(ClsPurchaseVm obj)
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
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.ItemPurchaseReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            //ViewBag.totalpaid = oClsResponse.Data.Purchases == null ? 0 : oClsResponse.Data.Purchases.Sum(x => x.Paid);
            //ViewBag.grandtotal = oClsResponse.Data.Purchases == null ? 0 : oClsResponse.Data.Purchases.Sum(x => x.GrandTotal);
            //ViewBag.totaldue = oClsResponse.Data.Purchases == null ? 0 : oClsResponse.Data.Purchases.Sum(x => x.Due);

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalAdjustedQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.AdjustedQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialItemPurchase");
        }
        public async Task<ActionResult> PurchaseByItem()
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

            var itemController = new WebApi.ItemController();
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await itemController.PurchaseByItemReport(itemObj);
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

            //var res3 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Category/ActiveCategorys", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse3 = serializer.Deserialize<ClsResponse>(res3);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            var itemController2 = new WebApi.ItemController();
            var itemObj2 = new ClsItemVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result5 = await itemController2.ActiveItems(itemObj2);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(result5);

            //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

            ViewBag.Type = "Detailed";
            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.Users = oClsResponse1.Data.Users;
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> PurchaseByItemFetch(ClsPurchaseVm obj)
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
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await itemController.PurchaseByItemReport(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseByItem");
        }
        public async Task<ActionResult> PurchaseDetailsByItem(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                ItemName = obj.ItemName
            };
            var result = await itemController.PurchaseDetailsByItem(itemObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.ItemName = obj.ItemName;
            return View();
        }
        public async Task<ActionResult> PurchaseDetailsByItemFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                ItemName = obj.ItemName
            };
            var result = await itemController.PurchaseDetailsByItem(itemObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseDetailsByItem");
        }
        public async Task<ActionResult> PurchaseReturnDetailsByItem(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                ItemName = obj.ItemName
            };
            var result = await itemController.PurchaseReturnDetailsByItem(itemObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.ItemName = obj.ItemName;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnDetailsByItemFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm itemObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                ItemName = obj.ItemName
            };
            var result = await itemController.PurchaseReturnDetailsByItem(itemObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseReturnDetailsByItem");
        }
        public async Task<ActionResult> PurchaseByCategory()
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

            var categoryController = new WebApi.CategoryController();
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await categoryController.PurchaseByCategoryReport(categoryObj);
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

            var categoryController2 = new WebApi.CategoryController();
            var categoryObj2 = new ClsCategoryVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result3 = await categoryController2.ActiveCategorys(categoryObj2);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(result3);

            var branchController = new WebApi.BranchController();
            var branchObj = new ClsBranchVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result25 = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(result25);

            //var res5 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Item/ActiveItems", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse5 = serializer.Deserialize<ClsResponse>(res5);

            //var res8 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ItemSettings/ItemSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse8 = serializer.Deserialize<ClsResponse>(res8);

            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> PurchaseByCategoryFetch(ClsPurchaseVm obj)
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
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await categoryController.PurchaseByCategoryReport(categoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseByCategory");
        }
        public async Task<ActionResult> PurchaseDetailsByCategory(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                CategoryName = obj.CategoryName
            };
            var result = await categoryController.PurchaseDetailsByCategory(categoryObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by category").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.CategoryName = obj.CategoryName;
            return View();
        }
        public async Task<ActionResult> PurchaseDetailsByCategoryFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                CategoryName = obj.CategoryName
            };
            var result = await categoryController.PurchaseDetailsByCategory(categoryObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by category").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseDetailsByCategory");
        }
        public async Task<ActionResult> PurchaseReturnDetailsByCategory(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                CategoryName = obj.CategoryName
            };
            var result = await categoryController.PurchaseReturnDetailsByCategory(categoryObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by category").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.CategoryName = obj.CategoryName;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnDetailsByCategoryFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm categoryObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                CategoryName = obj.CategoryName
            };
            var result = await categoryController.PurchaseReturnDetailsByCategory(categoryObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by category").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseReturnDetailsByCategory");
        }
        public async Task<ActionResult> PurchaseByBrand()
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

            var brandController = new WebApi.BrandController();
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await brandController.PurchaseByBrandReport(brandObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "User/AllActiveUsers", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            //var res6 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "UserGroup/ActiveUserGroups", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse6 = serializer.Deserialize<ClsResponse>(res6);

            var brandController2 = new WebApi.BrandController();
            var brandObj2 = new ClsBrandVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result2 = await brandController2.ActiveBrands(brandObj2);
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
            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by item").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> PurchaseByBrandFetch(ClsPurchaseVm obj)
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
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate
            };
            var result = await brandController.PurchaseByBrandReport(brandObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //ViewBag.Type = obj.Type;
            ViewBag.PurchaseDetails = oClsResponse.Data.PurchaseDetails;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TotalQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.Quantity);
            ViewBag.TotalFreeQuantity = oClsResponse.Data.PurchaseDetails.Sum(x => x.FreeQuantity);
            ViewBag.TotalUnitCost = oClsResponse.Data.PurchaseDetails.Sum(x => x.UnitCost);
            ViewBag.TotalDiscount = oClsResponse.Data.PurchaseDetails.Sum(x => x.Discount);
            ViewBag.TotalTaxPercent = oClsResponse.Data.PurchaseDetails.Sum(x => x.TaxPercent);
            //ViewBag.TotalPriceIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.PriceIncTax);
            ViewBag.TotalAmountIncTax = oClsResponse.Data.PurchaseDetails.Sum(x => x.AmountIncTax);

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.CustomersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.StockTransferPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();
            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseByBrand");
        }
        public async Task<ActionResult> PurchaseDetailsByBrand(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BrandName = obj.BrandName
            };
            var result = await brandController.PurchaseDetailsByBrand(brandObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by brand").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.BrandName = obj.BrandName;
            return View();
        }
        public async Task<ActionResult> PurchaseDetailsByBrandFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BrandName = obj.BrandName
            };
            var result = await brandController.PurchaseDetailsByBrand(brandObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by brand").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseDetailsByBrand");
        }
        public async Task<ActionResult> PurchaseReturnDetailsByBrand(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BrandName = obj.BrandName
            };
            var result = await brandController.PurchaseReturnDetailsByBrand(brandObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by brand").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            ViewBag.BrandName = obj.BrandName;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnDetailsByBrandFetch(ClsPurchaseDetailsVm obj)
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
            ClsSalesDetailsVm brandObj = new ClsSalesDetailsVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                BrandName = obj.BrandName
            };
            var result = await brandController.PurchaseReturnDetailsByBrand(brandObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by brand").FirstOrDefault();
            //ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalTransactionAmount = oClsResponse.Data.BankPayments.Sum(x => x.TransactionAmount);

            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            //ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseReturnDetailsByBrand");
        }
        public async Task<ActionResult> PurchaseBySupplier(ClsUserVm obj)
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
                obj.UserType = "supplier";
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
            var result = await userController.PurchaseBySupplierReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            var userController2 = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result1 = await userController2.AllActiveUsers(userObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            //ViewBag.TotalOpeningBalance = oClsResponse.Data.UserReport.Sum(x => x.OpeningBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSalesExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> PurchaseBySupplierFetch(ClsPurchaseVm obj)
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
            ClsUserVm userObj = new ClsUserVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                PageIndex = obj.PageIndex,
                PageSize = obj.PageSize,
                BranchId = obj.BranchId,
                FromDate = obj.FromDate,
                ToDate = obj.ToDate,
                UserType = "supplier"
            };
            var result = await userController.PurchaseBySupplierReport(userObj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            //ViewBag.TotalOpeningBalance = oClsResponse.Data.UserReport.Sum(x => x.OpeningBalance);
            ViewBag.TotalSalesInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesInvoices);
            ViewBag.TotalSalesExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSales);
            ViewBag.TotalPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesPaid);
            ViewBag.TotalDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesDue);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseBySupplier");
        }
        public async Task<ActionResult> PurchaseDetailsBySupplier(ClsPurchaseVm obj)
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
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.AllPurchases(obj);
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

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by supplier").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesExcTax = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Purchases.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Purchases.Sum(x => x.Due);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            ViewBag.SupplierName = obj.SupplierName;
            ViewBag.Page = obj.Page;
            return View();
        }
        public async Task<ActionResult> PurchaseDetailsBySupplierFetch(ClsPurchaseVm obj)
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
            var purchaseController = new WebApi.PurchaseController();
            var result = await purchaseController.AllPurchases(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.Purchases = oClsResponse.Data.Purchases;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesExcTax = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.Purchases.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.Purchases.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.Purchases.Sum(x => x.Due);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseDetailsBySupplier");
        }        
        public async Task<ActionResult> PurchaseReturnBySupplier(ClsUserVm obj)
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
                obj.UserType = "supplier";
            }
            var userController = new WebApi.UserController();
            var result = await userController.PurchaseReturnBySupplierReport(obj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase return by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturnExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn - x.TotalTaxAmount);
            ViewBag.TotalSalesReturnIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.UserReport.Sum(x => x.TotalAmountRemaining);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> PurchaseReturnBySupplierFetch(ClsUserVm obj)
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
            var result = await userController.PurchaseReturnBySupplierReport(obj);
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

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase return by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesReturnInvoices = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnInvoices);
            ViewBag.TotalSalesReturnExcTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn - x.TotalTaxAmount);
            ViewBag.TotalSalesReturnIncTax = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturn);
            ViewBag.TotalSalesReturnPaid = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnPaid);
            ViewBag.TotalSalesReturnDue = oClsResponse.Data.UserReport.Sum(x => x.TotalSalesReturnDue);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.UserReport.Sum(x => x.TotalAmountRemaining);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseReturnBySupplier");
        }
        public async Task<ActionResult> PurchaseReturnDetailsBySupplier(ClsPurchaseVm obj)
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
            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.AllPurchaseReturns(obj);
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

            ViewBag.PurchaseReturns = oClsResponse.Data.PurchaseReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse1.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase return by supplier").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.TotalSalesExcTax = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.PurchaseReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.PurchaseReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.PurchaseReturns.Sum(x => x.AmountRemaining);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;
            ViewBag.SupplierName = obj.SupplierName;
            ViewBag.Page = obj.Page;
            return View();
        }
        public async Task<ActionResult> PurchaseReturnDetailsBySupplierFetch(ClsPurchaseVm obj)
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
            var purchaseReturnController = new WebApi.PurchaseReturnController();
            var result = await purchaseReturnController.AllPurchaseReturns(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            ViewBag.PurchaseReturns = oClsResponse.Data.PurchaseReturns;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase return by supplier").FirstOrDefault();
            ViewBag.CustomerGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer group").FirstOrDefault();

            ViewBag.TotalSalesExcTax = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal - x.TotalTaxAmount);
            ViewBag.TotalSalesIncTax = oClsResponse.Data.PurchaseReturns.Sum(x => x.GrandTotal);
            ViewBag.TotalPaid = oClsResponse.Data.PurchaseReturns.Sum(x => x.Paid);
            ViewBag.TotalDue = oClsResponse.Data.PurchaseReturns.Sum(x => x.Due);
            ViewBag.TotalAmountRemaining = oClsResponse.Data.PurchaseReturns.Sum(x => x.AmountRemaining);

            ViewBag.SuppliersPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "suppliers").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialPurchaseReturnDetailsBySupplier");
        }
        public async Task<ActionResult> PurchaseByBusinessLocation()
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
        public async Task<ActionResult> PurchaseByBusinessLocationFetch(ClsAccountVm obj)
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
        public async Task<ActionResult> PurchaseRegister()
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
        public async Task<ActionResult> PurchaseRegisterFetch(ClsAccountVm obj)
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