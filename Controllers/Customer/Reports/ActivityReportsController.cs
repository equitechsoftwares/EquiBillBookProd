using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class ActivityReportsController : Controller
    {
        // GET: ActivityReports
        CommonController oCommonController = new CommonController();
        public async Task<ActionResult> ActivityLog()
        {
            ClsActivityLogVm obj = new ClsActivityLogVm();
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
                //obj.Title = "Activity Log Report";
                obj.UserType = "user";
            }

            obj.ReportType = "Activity Log";

            var activityLogController = new WebApi.ActivityLogController();
            var result = await activityLogController.AllActivityLogs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            //var res1 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Role/AllRoles", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse1 = serializer.Deserialize<ClsResponse>(res1);

            var userController = new WebApi.UserController();
            var userObj = new ClsUserVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result9 = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(result9);

            var menuController = new WebApi.MenuController();
            var menuObj = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result35 = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(result35);

            //var res25 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Branch/ActiveBranchs", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse25 = serializer.Deserialize<ClsResponse>(res25);

            var menuController2 = new WebApi.MenuController();
            var menuObj2 = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result40 = await menuController2.MenuPermissions(menuObj2);
            ClsResponse oClsResponse40 = await oCommonController.ExtractResponseFromActionResult(result40);

            var saleSettingsController = new WebApi.SaleSettingsController();
            var saleSettingsObj = new ClsSaleSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result12 = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(result12);

            var itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var result14 = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(result14);

            ViewBag.ActivityLogs = oClsResponse.Data.ActivityLogs;
            ViewBag.Menus = oClsResponse40.Data.Menus;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Users = oClsResponse9.Data.Users;
            //ViewBag.Roles = oClsResponse1.Data.Roles;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "activity log report").FirstOrDefault();

            //ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> ActivityLogFetch(ClsActivityLogVm obj)
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
                obj.Title = "Customers";
            }

            obj.ReportType = "Activity Log";

            var activityLogController = new WebApi.ActivityLogController();
            var result = await activityLogController.AllActivityLogs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);


            ViewBag.ActivityLogs = oClsResponse.Data.ActivityLogs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialActivityLog");
        }
    }
}