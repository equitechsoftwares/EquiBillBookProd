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

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminMenuController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: AdminMenu
        public async Task<ActionResult> Index()  
        {
            ClsMenuVm obj = new ClsMenuVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }

            serializer.MaxJsonLength = 2147483644;

            WebApi.MenuController menuController = new WebApi.MenuController();
            var allMenusResult = await menuController.AllMenus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allMenusResult);

            var activeParentMenusResult = await menuController.ActiveParentMenus(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeParentMenusResult);

            WebApi.HeaderController headerController = new WebApi.HeaderController();
            ClsHeaderVm headerObj = new ClsHeaderVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeHeadersResult = await headerController.ActiveHeaders(headerObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeHeadersResult);

            ViewBag.Menus = oClsResponse.Data.Menus;
            ViewBag.ParentMenus = oClsResponse1.Data.Menus;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Headers = oClsResponse2.Data.Headers;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            return View();
        }
        public async Task<ActionResult> MenuFetch(ClsMenuVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.MenuController menuController = new WebApi.MenuController();
            var allMenusResult = await menuController.AllMenus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allMenusResult);

            var activeParentMenusResult = await menuController.ActiveParentMenus(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeParentMenusResult);

            WebApi.HeaderController headerController = new WebApi.HeaderController();
            ClsHeaderVm headerObj = new ClsHeaderVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeHeadersResult = await headerController.ActiveHeaders(headerObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeHeadersResult);

            ViewBag.Menus = oClsResponse.Data.Menus;
            ViewBag.ParentMenus = oClsResponse1.Data.Menus;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Headers = oClsResponse2.Data.Headers;

            return PartialView("PartialMenu");
        }
        public async Task<ActionResult> UpdateMenu(ClsMenuVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.MenuController menuController = new WebApi.MenuController();
            var result = await menuController.UpdateMenu(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ActiveParentMenus(ClsMenuVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.MenuController menuController = new WebApi.MenuController();
            var result = await menuController.ActiveParentMenus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
    }
}