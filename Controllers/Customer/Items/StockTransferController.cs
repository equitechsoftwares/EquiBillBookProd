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
    public class StockTransferController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: stocktransfer
        public async Task<ActionResult> index(ClsStockTransferVm obj)
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
                //obj.Title = "Stock Transfer";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }

            if (obj.BranchId != 0)
            {
                ViewBag.BranchId = obj.BranchId;
            }

            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.AllStockTransfers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.StockTransfers = oClsResponse.Data.StockTransfers;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.StockTransferStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer status update").FirstOrDefault();

            //ViewBag.TotalQuantity = oClsResponse.Data.StockTransfers.Sum(x => x.TotalQuantity);
            ViewBag.TotalItems = oClsResponse.Data.StockTransfers.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.StockTransfers.Sum(x => x.TotalAmount);
            //ViewBag.TotalShippingCharge = oClsResponse.Data.StockTransfers.Sum(x => x.ShippingCharge);

            return View();
        }
        public async Task<ActionResult> StockTransferFetch(ClsStockTransferVm obj)
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
                //obj.Title = "Stock Transfer";
                ViewBag.InvoiceUrl = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "");
            }
            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.AllStockTransfers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);


            ViewBag.StockTransfers = oClsResponse.Data.StockTransfers;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer").FirstOrDefault();

            ViewBag.StockTransferStatusUpdate = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer status update").FirstOrDefault();

            //ViewBag.TotalQuantity = oClsResponse.Data.StockTransfers.Sum(x => x.TotalQuantity);
            ViewBag.TotalItems = oClsResponse.Data.StockTransfers.Sum(x => x.TotalItems);
            ViewBag.TotalAmount = oClsResponse.Data.StockTransfers.Sum(x => x.TotalAmount);
            //ViewBag.TotalShippingCharge = oClsResponse.Data.StockTransfers.Sum(x => x.ShippingCharge);

            return PartialView("PartialStockTransfer");
        }
        public async Task<ActionResult> Edit(long StockTransferId)
        {
            ClsStockTransferVm obj = new ClsStockTransferVm();
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
                obj.StockTransferId = StockTransferId;
            }
            ClsStockTransfer stockTransferObj = new ClsStockTransfer { StockTransferId = obj.StockTransferId, CompanyId = obj.CompanyId };
            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.StockTransfer(stockTransferObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            StockTransferReasonController stockTransferReasonController = new StockTransferReasonController();
            ClsStockTransferReasonVm stockTransferReasonObj = new ClsStockTransferReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockTransferReasonResult = await stockTransferReasonController.ActiveStockTransferReasons(stockTransferReasonObj);
            ClsResponse oClsResponse70 = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonResult);

            ViewBag.StockTransfer = oClsResponse.Data.StockTransfer;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockTransferReasons = oClsResponse70.Data.StockTransferReasons;

            ViewBag.StockTransferReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer reasons").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> Add()
        {
            ClsStockTransferVm obj = new ClsStockTransferVm();
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
            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            StockTransferReasonController stockTransferReasonController = new StockTransferReasonController();
            ClsStockTransferReasonVm stockTransferReasonObj = new ClsStockTransferReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockTransferReasonResult = await stockTransferReasonController.ActiveStockTransferReasons(stockTransferReasonObj);
            ClsResponse oClsResponse70 = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonResult);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockTransferReasons = oClsResponse70.Data.StockTransferReasons;

            ViewBag.StockTransferReasonsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer reasons").FirstOrDefault();

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> StockTransferInsert(ClsStockTransferVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.InsertStockTransfer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockTransferUpdate(ClsStockTransferVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.UpdateStockTransfer(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> StockTransferdelete(ClsStockTransferVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.StockTransferDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> StockTransferDetailsDelete(ClsStockTransferDetailsVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.StockTransferDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockTransferView(long StockTransferId)
        {
            ClsStockTransferVm obj = new ClsStockTransferVm();
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
                obj.StockTransferId = StockTransferId;
            }
            ClsStockTransfer stockTransferObj = new ClsStockTransfer { StockTransferId = obj.StockTransferId, CompanyId = obj.CompanyId };
            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.StockTransfer(stockTransferObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            StockTransferReasonController stockTransferReasonController = new StockTransferReasonController();
            ClsStockTransferReasonVm stockTransferReasonObj = new ClsStockTransferReasonVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var stockTransferReasonResult = await stockTransferReasonController.ActiveStockTransferReasons(stockTransferReasonObj);
            ClsResponse oClsResponse70 = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonResult);

            ViewBag.StockTransfer = oClsResponse.Data.StockTransfer;
            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.ItemSetting = oClsResponse15.Data.ItemSetting;
            ViewBag.StockTransferReasons = oClsResponse70.Data.StockTransferReasons;

            return PartialView("PartialStockTransferView");
        }
        public async Task<ActionResult> StockTransferImport()
        {
            ClsStockTransferVm obj = new ClsStockTransferVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> ImportStockTransfer(ClsStockTransferVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            // Note: ImportStockTransfer method may not exist in StockTransferController API
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "StockTransfer/ImportStockTransfer", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            return Json(oClsResponse);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Invoice(string InvoiceId)
        {
            ClsStockTransferVm obj = new ClsStockTransferVm();
            //obj.CompanyId = Id;
            obj.InvoiceId = InvoiceId;

            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.Invoice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);

            ViewBag.StockTransfer = oClsResponse.Data.StockTransfer;
            ViewBag.TotalUnitPrice = oClsResponse.Data.StockTransfer.StockTransferDetails.Select(a => a.UnitCost).DefaultIfEmpty().Sum();
            ViewBag.TotalQuantity = oClsResponse.Data.StockTransfer.StockTransferDetails.Select(a => a.Quantity).DefaultIfEmpty().Sum();
            ViewBag.TotalAmount = oClsResponse.Data.StockTransfer.StockTransferDetails.Select(a => a.Amount).DefaultIfEmpty().Sum();
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;
            ViewBag.OnlinePaymentSetting = oClsResponse.Data.OnlinePaymentSetting;
            return View();
        }

        public async Task<ActionResult> UpdateStockTransferStatus(ClsStockTransferVm obj)
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
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.StockTransferController stockTransferController = new WebApi.StockTransferController();
            var stockTransferResult = await stockTransferController.UpdateStockTransferStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferResult);
            return Json(oClsResponse);
        }

    }
}