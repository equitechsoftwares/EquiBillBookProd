using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class TableTypeController : Controller
    {
        CommonController oCommonController = new CommonController();

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ClsTableTypeVm obj = new ClsTableTypeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.GetTableTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.TableTypes = oClsResponse.Data.TableTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table types").FirstOrDefault();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = obj.PageIndex > 0 ? obj.PageIndex : 1;
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.GetTableTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.TableTypes = oClsResponse.Data.TableTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "table types").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> Add()
        {
            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            ClsTableTypeVm obj = new ClsTableTypeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TableTypeId = id;
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.TableType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);

            ViewBag.TableType = oClsResponse.Data.TableType;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InsertTableType(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.InsertTableType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateTableType(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.InsertTableType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> TableTypeDelete(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.TableTypeDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> TableTypeActiveInactive(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = obj.AddedBy > 0 ? obj.AddedBy : Convert.ToInt64(arr[2]);
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.TableTypeActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetTableTypes(ClsTableTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.TableTypeController tableTypeController = new WebApi.TableTypeController();
            var tableTypeResult = await tableTypeController.GetTableTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tableTypeResult);
            return Json(oClsResponse);
        }
    }
}
