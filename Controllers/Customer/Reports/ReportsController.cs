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

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class ReportsController : Controller
    {
        // GET: Reports
        CommonController oCommonController = new CommonController();

        public async Task<ActionResult> Index()
        {
            ClsHeaderVm obj = new ClsHeaderVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.HeaderType = "reports";

            var headerController = new WebApi.HeaderController();
            var headerResult = await headerController.HeaderPermissions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(headerResult);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            var saleSettingsController = new WebApi.SaleSettingsController();
            var saleSettingsObj = new ClsSaleSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            var purchaseSettingsController = new WebApi.PurchaseSettingsController();
            var purchaseSettingsObj = new ClsPurchaseSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var purchaseSettingsResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingsResult);

            var businessSettingsController = new WebApi.BusinessSettingsController();
            var businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            //var res60 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "ExpenseSettings/ExpenseSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse60 = serializer.Deserialize<ClsResponse>(res60);

            ViewBag.Headers = oClsResponse.Data.Headers;
            ViewBag.ItemSetting = oClsResponse4.Data.ItemSetting;
            ViewBag.SaleSetting = oClsResponse5.Data.SaleSetting;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.ExpenseSetting = oClsResponse60.Data.ExpenseSetting;

            return View();
        }
    }

}