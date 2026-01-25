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
    public class StationTypeController : Controller
    {
        CommonController oCommonController = new CommonController();

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ClsStationTypeVm obj = new ClsStationTypeVm();
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.GetStationTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.StationTypes = oClsResponse.Data.StationTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "station types").FirstOrDefault();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ClsStationTypeVm obj)
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.GetStationTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.StationTypes = oClsResponse.Data.StationTypes;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "station types").FirstOrDefault();

            return View();
        }

        public async Task<ActionResult> Add()
        {
            return View();
        }

        public async Task<ActionResult> Edit(long id)
        {
            ClsStationTypeVm obj = new ClsStationTypeVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.StationTypeId = id;
            }

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.StationType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);

            ViewBag.StationType = oClsResponse.Data.StationType;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InsertStationType(ClsStationTypeVm obj)
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.InsertStationType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateStationType(ClsStationTypeVm obj)
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.InsertStationType(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> StationTypeDelete(ClsStationTypeVm obj)
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.StationTypeDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> StationTypeActiveInactive(ClsStationTypeVm obj)
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

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.StationTypeActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> GetStationTypes(ClsStationTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.GetStationTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<JsonResult> ActiveStationTypes(ClsStationTypeVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = obj.CompanyId > 0 ? obj.CompanyId : Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.StationTypeController stationTypeController = new WebApi.StationTypeController();
            var stationTypeResult = await stationTypeController.ActiveStationTypes(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stationTypeResult);
            return Json(oClsResponse);
        }
    }
}
