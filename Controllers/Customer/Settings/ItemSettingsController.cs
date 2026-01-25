using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class ItemSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: ItemSettings
        public async Task<ActionResult> ItemSettings()
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
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            obj.PaymentType = "all";
            obj.UnitType = "all";

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var itemSettingResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(itemSettingResult);

            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeUnitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(activeUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.ItemSetting = oClsResponse4.Data.ItemSetting;
            ViewBag.Units = oClsResponse8.Data.Units;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();

            return View();
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ItemSettingsUpdate(ClsItemSettingsVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.ItemSettingsController itemSettingsController = new WebApi.ItemSettingsController();
            var itemSettingsUpdateResult = await itemSettingsController.ItemSettingsUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemSettingsUpdateResult);

            Response.Cookies["ItemSetting"]["DateFormat"] = obj.ExpiryDateFormat;
            Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(365);
            return Json(oClsResponse);
        }

        #region category

        public async Task<ActionResult> Category(ClsCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Categories";
            }
            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var allCategoriesResult = await categoryController.AllCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CategoryFetch(ClsCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Categories";
            }
            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var allCategoriesResult = await categoryController.AllCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();

            return PartialView("PartialCategory");
        }
        public async Task<ActionResult> CategoryEdit(long CategoryId)
        {
            ClsCategoryVm obj = new ClsCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CategoryId = CategoryId;
            }
            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            ClsCategory categoryObj = new ClsCategory { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CategoryId = obj.CategoryId };
            var categoryResult = await categoryController.Category(categoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            ViewBag.Category = oClsResponse.Data.Category;
            return View();
        }
        public ActionResult CategoryAdd()
        {
            return View();
        }
        public async Task<ActionResult> CategoryInsert(ClsCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var insertCategoryResult = await categoryController.InsertCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CategoryUpdate(ClsCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var updateCategoryResult = await categoryController.UpdateCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CategoryActiveInactive(ClsCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var categoryActiveInactiveResult = await categoryController.CategoryActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(categoryActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CategoryDelete(ClsCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            var categoryDeleteResult = await categoryController.CategoryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(categoryDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region SubCategory

        public async Task<ActionResult> SubCategory(ClsSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Sub Categories";
            }
            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var allSubCategoriesResult = await subCategoryController.AllSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SubCategoryFetch(ClsSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Sub Categories";
            }
            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var allSubCategoriesResult = await subCategoryController.AllSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();

            return PartialView("PartialSubCategory");
        }
        public async Task<ActionResult> SubCategoryEdit(long SubCategoryId)
        {
            ClsSubCategoryVm obj = new ClsSubCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SubCategoryId = SubCategoryId;
            }
            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            ClsSubCategory subCategoryObj = new ClsSubCategory { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SubCategoryId = obj.SubCategoryId };
            var subCategoryResult = await subCategoryController.SubCategory(subCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subCategoryResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubCategory = oClsResponse.Data.SubCategory;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> SubCategoryAdd()
        {
            ClsSubCategoryVm obj = new ClsSubCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCategorysResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCategorysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> SubCategoryInsert(ClsSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var insertSubCategoryResult = await subCategoryController.InsertSubCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSubCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubCategoryUpdate(ClsSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var updateSubCategoryResult = await subCategoryController.UpdateSubCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSubCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubCategoryActiveInactive(ClsSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var subCategoryActiveInactiveResult = await subCategoryController.SubCategoryActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subCategoryActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubCategoryDelete(ClsSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            var subCategoryDeleteResult = await subCategoryController.SubCategoryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subCategoryDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region SubSubCategory

        public async Task<ActionResult> SubSubCategory(ClsSubSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Sub Sub Categories";
            }
            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var allSubSubCategoriesResult = await subSubCategoryController.AllSubSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSubSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SubSubCategoryFetch(ClsSubSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Sub Sub Categories";
            }
            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var allSubSubCategoriesResult = await subSubCategoryController.AllSubSubCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSubSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            return PartialView("PartialSubSubCategory");
        }
        public async Task<ActionResult> SubSubCategoryEdit(long SubSubCategoryId)
        {
            ClsSubSubCategoryVm obj = new ClsSubSubCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SubSubCategoryId = SubSubCategoryId;
            }
            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            ClsSubSubCategory subSubCategoryObj = new ClsSubSubCategory { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SubSubCategoryId = obj.SubSubCategoryId };
            var subSubCategoryResult = await subSubCategoryController.SubSubCategory(subSubCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subSubCategoryResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubSubCategory = oClsResponse.Data.SubSubCategory;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;

            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SubSubCategoryAdd()
        {
            ClsSubCategoryVm obj = new ClsSubCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.CategoryController categoryController = new WebApi.CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCategorysResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCategorysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Categories = oClsResponse.Data.Categories;

            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SubSubCategoryInsert(ClsSubSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var insertSubSubCategoryResult = await subSubCategoryController.InsertSubSubCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSubSubCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubSubCategoryUpdate(ClsSubSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var updateSubSubCategoryResult = await subSubCategoryController.UpdateSubSubCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSubSubCategoryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubSubCategoryActiveInactive(ClsSubSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var subSubCategoryActiveInactiveResult = await subSubCategoryController.SubSubCategoryActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subSubCategoryActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SubSubCategoryDelete(ClsSubSubCategoryVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            var subSubCategoryDeleteResult = await subSubCategoryController.SubSubCategoryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(subSubCategoryDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveSubCategories(ClsSubSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            ClsSubCategoryVm subCategoryObj = new ClsSubCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CategoryId = obj.CategoryId };
            var activeSubCategoriesResult = await subCategoryController.ActiveSubCategories(subCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubCategories = oClsResponse.Data.SubCategories;

            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();

            return PartialView("PartialSubCategoryDropdown");
        }
        public async Task<ActionResult> ActiveSubSubCategories(ClsSubSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SubSubCategoryController subSubCategoryController = new WebApi.SubSubCategoryController();
            ClsSubSubCategoryVm subSubCategoryObj = new ClsSubSubCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SubCategoryId = obj.SubCategoryId };
            var activeSubSubCategoriesResult = await subSubCategoryController.ActiveSubSubCategories(subSubCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSubSubCategoriesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;

            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();

            return PartialView("PartialSubSubCategoryDropdown");
        }
        public async Task<ActionResult> ActiveModalSubCategories(ClsSubSubCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SubCategoryController subCategoryController = new WebApi.SubCategoryController();
            ClsSubCategoryVm subCategoryObj = new ClsSubCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CategoryId = obj.CategoryId };
            var activeSubCategoriesResult = await subCategoryController.ActiveSubCategories(subCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSubCategoriesResult);
            return Json(oClsResponse);
        }

        #endregion

        #region units
        public async Task<ActionResult> Unit(ClsUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Units";
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            var allUnitsResult = await unitController.AllUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> UnitFetch(ClsUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Units";
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            var allUnitsResult = await unitController.AllUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();

            return PartialView("PartialUnit");
        }
        public async Task<ActionResult> UnitEdit(long UnitId)
        {
            ClsUnitVm obj = new ClsUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UnitId = UnitId;
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnit unitObj = new ClsUnit { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UnitId = obj.UnitId };
            var unitResult = await unitController.Unit(unitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(unitResult);

            ViewBag.Unit = oClsResponse.Data.Unit;
            return View();
        }
        public ActionResult UnitAdd()
        {
            return View();
        }
        public async Task<ActionResult> UnitInsert(ClsUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UnitController unitController = new WebApi.UnitController();
            var insertUnitResult = await unitController.InsertUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UnitUpdate(ClsUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UnitController unitController = new WebApi.UnitController();
            var updateUnitResult = await unitController.UpdateUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UnitActiveInactive(ClsUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UnitController unitController = new WebApi.UnitController();
            var unitActiveInactiveResult = await unitController.UnitActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(unitActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UnitDelete(ClsUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.UnitController unitController = new WebApi.UnitController();
            var unitDeleteResult = await unitController.UnitDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(unitDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Secondary Units

        public async Task<ActionResult> SecondaryUnit(ClsSecondaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Secondary Units";
            }
            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var allSecondaryUnitsResult = await secondaryUnitController.AllSecondaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSecondaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SecondaryUnitFetch(ClsSecondaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Secondary Units";
            }
            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var allSecondaryUnitsResult = await secondaryUnitController.AllSecondaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSecondaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            return PartialView("PartialSecondaryUnit");
        }
        public async Task<ActionResult> SecondaryUnitEdit(long SecondaryUnitId)
        {
            ClsSecondaryUnitVm obj = new ClsSecondaryUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SecondaryUnitId = SecondaryUnitId;
            }
            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            ClsSecondaryUnit secondaryUnitObj = new ClsSecondaryUnit { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SecondaryUnitId = obj.SecondaryUnitId };
            var secondaryUnitResult = await secondaryUnitController.SecondaryUnit(secondaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(secondaryUnitResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SecondaryUnit = oClsResponse.Data.SecondaryUnit;
            ViewBag.Units = oClsResponse.Data.Units;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SecondaryUnitAdd()
        {
            ClsSecondaryUnitVm obj = new ClsSecondaryUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeUnitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SecondaryUnitInsert(ClsSecondaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var insertSecondaryUnitResult = await secondaryUnitController.InsertSecondaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSecondaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SecondaryUnitUpdate(ClsSecondaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var updateSecondaryUnitResult = await secondaryUnitController.UpdateSecondaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSecondaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SecondaryUnitActiveInactive(ClsSecondaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var secondaryUnitActiveInactiveResult = await secondaryUnitController.SecondaryUnitActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(secondaryUnitActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SecondaryUnitDelete(ClsSecondaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            var secondaryUnitDeleteResult = await secondaryUnitController.SecondaryUnitDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(secondaryUnitDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Tertiary Units

        public async Task<ActionResult> TertiaryUnit(ClsTertiaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Tertiary Units";
            }
            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var allTertiaryUnitsResult = await tertiaryUnitController.AllTertiaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTertiaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TertiaryUnitFetch(ClsTertiaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Tertiary Units";
            }
            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var allTertiaryUnitsResult = await tertiaryUnitController.AllTertiaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allTertiaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();

            return PartialView("PartialTertiaryUnit");
        }
        public async Task<ActionResult> TertiaryUnitEdit(long TertiaryUnitId)
        {
            ClsTertiaryUnitVm obj = new ClsTertiaryUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.TertiaryUnitId = TertiaryUnitId;
            }
            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            ClsTertiaryUnit tertiaryUnitObj = new ClsTertiaryUnit { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TertiaryUnitId = obj.TertiaryUnitId };
            var tertiaryUnitResult = await tertiaryUnitController.TertiaryUnit(tertiaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tertiaryUnitResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.TertiaryUnit = oClsResponse.Data.TertiaryUnit;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TertiaryUnitAdd()
        {
            ClsTertiaryUnitVm obj = new ClsTertiaryUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeUnitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> TertiaryUnitInsert(ClsTertiaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var insertTertiaryUnitResult = await tertiaryUnitController.InsertTertiaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertTertiaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TertiaryUnitUpdate(ClsTertiaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var updateTertiaryUnitResult = await tertiaryUnitController.UpdateTertiaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateTertiaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TertiaryUnitActiveInactive(ClsTertiaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var tertiaryUnitActiveInactiveResult = await tertiaryUnitController.TertiaryUnitActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tertiaryUnitActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> TertiaryUnitDelete(ClsTertiaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            var tertiaryUnitDeleteResult = await tertiaryUnitController.TertiaryUnitDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(tertiaryUnitDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveSecondaryUnits(ClsTertiaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SecondaryUnitController secondaryUnitController = new WebApi.SecondaryUnitController();
            ClsSecondaryUnitVm secondaryUnitObj = new ClsSecondaryUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UnitId = obj.UnitId };
            var activeSecondaryUnitsResult = await secondaryUnitController.ActiveSecondaryUnits(secondaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeSecondaryUnitsResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Quaternary Units

        public async Task<ActionResult> QuaternaryUnit(ClsQuaternaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Quaternary Units";
            }
            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var allQuaternaryUnitsResult = await quaternaryUnitController.AllQuaternaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allQuaternaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> QuaternaryUnitFetch(ClsQuaternaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Quaternary Units";
            }
            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var allQuaternaryUnitsResult = await quaternaryUnitController.AllQuaternaryUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allQuaternaryUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();

            return PartialView("PartialQuaternaryUnit");
        }
        public async Task<ActionResult> QuaternaryUnitEdit(long QuaternaryUnitId)
        {
            ClsQuaternaryUnitVm obj = new ClsQuaternaryUnitVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.QuaternaryUnitId = QuaternaryUnitId;
            }
            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            ClsQuaternaryUnit quaternaryUnitObj = new ClsQuaternaryUnit { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, QuaternaryUnitId = obj.QuaternaryUnitId };
            var quaternaryUnitResult = await quaternaryUnitController.QuaternaryUnit(quaternaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(quaternaryUnitResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.QuaternaryUnit = oClsResponse.Data.QuaternaryUnit;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> QuaternaryUnitAdd()
        {
            ClsSubCategoryVm obj = new ClsSubCategoryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.UnitController unitController = new WebApi.UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeUnitsResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeUnitsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Units = oClsResponse.Data.Units;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> QuaternaryUnitInsert(ClsQuaternaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var insertQuaternaryUnitResult = await quaternaryUnitController.InsertQuaternaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertQuaternaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> QuaternaryUnitUpdate(ClsQuaternaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var updateQuaternaryUnitResult = await quaternaryUnitController.UpdateQuaternaryUnit(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateQuaternaryUnitResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> QuaternaryUnitActiveInactive(ClsQuaternaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var quaternaryUnitActiveInactiveResult = await quaternaryUnitController.QuaternaryUnitActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(quaternaryUnitActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> QuaternaryUnitDelete(ClsQuaternaryUnitVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            var quaternaryUnitDeleteResult = await quaternaryUnitController.QuaternaryUnitDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(quaternaryUnitDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveTertiaryUnits(ClsQuaternaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.TertiaryUnitController tertiaryUnitController = new WebApi.TertiaryUnitController();
            ClsTertiaryUnitVm tertiaryUnitObj = new ClsTertiaryUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SecondaryUnitId = obj.SecondaryUnitId };
            var activeTertiaryunitsResult = await tertiaryUnitController.ActiveTertiaryUnits(tertiaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeTertiaryunitsResult);
            return Json(oClsResponse);

        }
        public async Task<ActionResult> ActiveQuaternaryUnits(ClsQuaternaryUnitVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.QuaternaryUnitController quaternaryUnitController = new WebApi.QuaternaryUnitController();
            ClsQuaternaryUnitVm quaternaryUnitObj = new ClsQuaternaryUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, TertiaryUnitId = obj.TertiaryUnitId };
            var activeQuaternaryunitsResult = await quaternaryUnitController.ActiveQuaternaryUnits(quaternaryUnitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeQuaternaryunitsResult);
            return Json(oClsResponse);
        }

        #endregion

        #region variation
        public async Task<ActionResult> Variation(ClsVariationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Variation";
            }
            WebApi.VariationController variationController = new WebApi.VariationController();
            var allVariationsResult = await variationController.AllVariations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allVariationsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> VariationFetch(ClsVariationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Variation";
            }
            WebApi.VariationController variationController = new WebApi.VariationController();
            var allVariationsResult = await variationController.AllVariations(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allVariationsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();

            return PartialView("PartialVariation");
        }
        public ActionResult VariationAdd()
        {
            return View();
        }
        public async Task<ActionResult> VariationInsert(ClsVariationVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            var insertVariationResult = await variationController.InsertVariation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertVariationResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VariationEdit(long VariationId)
        {
            ClsVariation obj = new ClsVariation();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.VariationId = VariationId;
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            var variationResult = await variationController.Variation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(variationResult);

            ViewBag.Variation = oClsResponse.Data.Variation;
            return View();
        }
        public async Task<ActionResult> VariationUpdate(ClsVariationVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            var updateVariationResult = await variationController.Updatevariation(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateVariationResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VariationActiveInactive(ClsVariationVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            var variationActiveInactiveResult = await variationController.variationActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(variationActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VariationDelete(ClsVariationVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            var variationDeleteResult = await variationController.variationDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(variationDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> VariationDetailsDelete(ClsVariationDetailsVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.VariationDetailsController variationDetailsController = new WebApi.VariationDetailsController();
            var variationDetailsDeleteResult = await variationDetailsController.VariationDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(variationDetailsDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveVariations(ClsVariationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.VariationController variationController = new WebApi.VariationController();
            ClsVariation variationObj = new ClsVariation { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activevariationsResult = await variationController.Activevariations(variationObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activevariationsResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveVariationDetails(ClsVariationVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.VariationController variationController = new WebApi.VariationController();
            ClsVariation variationObj = new ClsVariation { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeVariationDetailsResult = await variationController.ActiveVariationDetails(variationObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeVariationDetailsResult);
            return Json(oClsResponse);
        }
        #endregion

        #region warranties
        public async Task<ActionResult> Warranties(ClsWarrantyVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var allWarrantysResult = await warrantyController.AllWarrantys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allWarrantysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Warrantys = oClsResponse.Data.Warrantys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> WarrantiesFetch(ClsWarrantyVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var allWarrantysResult = await warrantyController.AllWarrantys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allWarrantysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Warrantys = oClsResponse.Data.Warrantys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();

            return PartialView("PartialWarranties");
        }
        public async Task<ActionResult> WarrantiesEdit(long WarrantyId)
        {
            ClsWarrantyVm obj = new ClsWarrantyVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.WarrantyId = WarrantyId;
            }
            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            ClsWarranty warrantyObj = new ClsWarranty { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, WarrantyId = obj.WarrantyId };
            var warrantyResult = await warrantyController.Warranty(warrantyObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            ViewBag.Warranty = oClsResponse.Data.Warranty;
            return View();
        }
        public ActionResult WarrantiesAdd()
        {
            return View();
        }
        public async Task<ActionResult> WarrantiesInsert(ClsWarrantyVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var insertWarrantyResult = await warrantyController.InsertWarranty(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertWarrantyResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WarrantiesUpdate(ClsWarrantyVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var updateWarrantyResult = await warrantyController.UpdateWarranty(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateWarrantyResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WarrantiesActiveInactive(ClsWarrantyVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var warrantyActiveInactiveResult = await warrantyController.WarrantyActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(warrantyActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WarrantiesDelete(ClsWarrantyVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var warrantyDeleteResult = await warrantyController.WarrantyDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(warrantyDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ActiveWarrantys(ClsWarrantyVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.WarrantyController warrantyController = new WebApi.WarrantyController();
            var activeWarrantysResult = await warrantyController.ActiveWarrantys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeWarrantysResult);
            return Json(oClsResponse);
        }

        #endregion

        #region brand
        public async Task<ActionResult> Brand(ClsBrandVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Brand";
            }
            WebApi.BrandController brandController = new WebApi.BrandController();
            var allBrandsResult = await brandController.AllBrands(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allBrandsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> BrandFetch(ClsBrandVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Brand";
            }
            WebApi.BrandController brandController = new WebApi.BrandController();
            var allBrandsResult = await brandController.AllBrands(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allBrandsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();

            return PartialView("PartialBrand");
        }
        public async Task<ActionResult> BrandEdit(long BrandId)
        {
            ClsBrandVm obj = new ClsBrandVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BrandId = BrandId;
            }
            WebApi.BrandController brandController = new WebApi.BrandController();
            ClsBrand brandObj = new ClsBrand { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BrandId = obj.BrandId };
            var brandResult = await brandController.Brand(brandObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(brandResult);

            ViewBag.Brand = oClsResponse.Data.Brand;
            return View();
        }
        public ActionResult BrandAdd()
        {
            return View();
        }
        public async Task<ActionResult> BrandInsert(ClsBrandVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BrandController brandController = new WebApi.BrandController();
            var insertBrandResult = await brandController.InsertBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertBrandResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BrandUpdate(ClsBrandVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BrandController brandController = new WebApi.BrandController();
            var updateBrandResult = await brandController.UpdateBrand(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateBrandResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BrandActiveInactive(ClsBrandVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BrandController brandController = new WebApi.BrandController();
            var brandActiveInactiveResult = await brandController.BrandActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(brandActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BrandDelete(ClsBrandVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BrandController brandController = new WebApi.BrandController();
            var brandDeleteResult = await brandController.BrandDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(brandDeleteResult);
            return Json(oClsResponse);
        }        
        #endregion

        #region Salt
        public async Task<ActionResult> Salt(ClsSaltVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Salt";
            }
            WebApi.SaltController saltController = new WebApi.SaltController();
            var allSaltsResult = await saltController.AllSalts(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSaltsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Salts = oClsResponse.Data.Salts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SaltFetch(ClsSaltVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Salt";
            }
            WebApi.SaltController saltController = new WebApi.SaltController();
            var allSaltsResult = await saltController.AllSalts(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSaltsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Salts = oClsResponse.Data.Salts;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            return PartialView("PartialSalt");
        }
        public async Task<ActionResult> SaltEdit(long SaltId)
        {
            ClsSaltVm obj = new ClsSaltVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.SaltId = SaltId;
            }
            WebApi.SaltController saltController = new WebApi.SaltController();
            ClsSalt saltObj = new ClsSalt { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SaltId = obj.SaltId };
            var saltResult = await saltController.Salt(saltObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saltResult);

            ViewBag.Salt = oClsResponse.Data.Salt;
            return View();
        }
        public ActionResult SaltAdd()
        {
            return View();
        }
        public async Task<ActionResult> SaltInsert(ClsSaltVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SaltController saltController = new WebApi.SaltController();
            var insertSaltResult = await saltController.InsertSalt(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSaltResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SaltUpdate(ClsSaltVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SaltController saltController = new WebApi.SaltController();
            var updateSaltResult = await saltController.UpdateSalt(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSaltResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SaltActiveInactive(ClsSaltVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SaltController saltController = new WebApi.SaltController();
            var saltActiveInactiveResult = await saltController.SaltActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saltActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SaltDelete(ClsSaltVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SaltController saltController = new WebApi.SaltController();
            var saltDeleteResult = await saltController.SaltDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saltDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SaltView(ClsSaltVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.PageSize = 10000000;

            WebApi.SaltController saltController = new WebApi.SaltController();
            ClsSalt saltObj = new ClsSalt { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SaltId = obj.SaltId };
            var saltResult = await saltController.Salt(saltObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saltResult);

            WebApi.ItemController itemController = new WebApi.ItemController();
            ClsItemDetailsVm itemDetailsObj = new ClsItemDetailsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SaltId = obj.SaltId, PageSize = obj.PageSize };
            var currentStockResult = await itemController.CurrentStock(itemDetailsObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(currentStockResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Salt = oClsResponse.Data.Salt;
            ViewBag.ItemDetails = oClsResponse1.Data.ItemDetails;

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();

            return PartialView("PartialSaltView");
        }
        public async Task<ActionResult> saltAutocomplete(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.SaltController saltController = new WebApi.SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, Search = obj.ItemName };
            var saltAutocompleteResult = await saltController.SaltAutocomplete(saltObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saltAutocompleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SearchItemsBySalt(ClsSaltVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Brand";
            }
            obj.PageSize = 10000000;

            WebApi.ItemController itemController = new WebApi.ItemController();
            ClsItemDetailsVm itemDetailsObj = new ClsItemDetailsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SaltId = obj.SaltId, PageSize = obj.PageSize };
            var currentStockResult = await itemController.CurrentStock(itemDetailsObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(currentStockResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.ItemDetails = oClsResponse.Data.ItemDetails;

            ViewBag.ItemsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            return PartialView("PartialSaltItems");
        }
        #endregion

        #region Selling Price Group

        public async Task<ActionResult> SellingPriceGroup(ClsSellingPriceGroupVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "SellingPrice Group";
            }
            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var allSellingPriceGroupsResult = await sellingPriceGroupController.AllSellingPriceGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSellingPriceGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SellingPriceGroups = oClsResponse.Data.SellingPriceGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SellingPriceGroupFetch(ClsSellingPriceGroupVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "SellingPrice Group";
            }
            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var allSellingPriceGroupsResult = await sellingPriceGroupController.AllSellingPriceGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSellingPriceGroupsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SellingPriceGroups = oClsResponse.Data.SellingPriceGroups;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            return PartialView("PartialSellingPriceGroup");
        }
        public async Task<ActionResult> SellingPriceGroupEdit(ClsSellingPriceGroupVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            ClsSellingPriceGroup sellingPriceGroupObj = new ClsSellingPriceGroup { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, SellingPriceGroupId = obj.SellingPriceGroupId };
            var sellingPriceGroupResult = await sellingPriceGroupController.SellingPriceGroup(sellingPriceGroupObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.SellingPriceGroup = oClsResponse.Data.SellingPriceGroup;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            return View();
        }
        public async Task<ActionResult> SellingPricegroupAdd()
        {
            ClsUserGroupVm obj = new ClsUserGroupVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            return View();
        }
        public async Task<ActionResult> SellingPricegroupInsert(ClsSellingPriceGroupVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var insertSellingPriceGroupResult = await sellingPriceGroupController.InsertSellingPriceGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSellingPriceGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SellingPricegroupUpdate(ClsSellingPriceGroupVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var updateSellingPriceGroupResult = await sellingPriceGroupController.UpdateSellingPriceGroup(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSellingPriceGroupResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SellingPricegroupActiveInactive(ClsSellingPriceGroupVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var sellingPriceGroupActiveInactiveResult = await sellingPriceGroupController.SellingPriceGroupActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SellingPricegroupdelete(ClsSellingPriceGroupVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.SellingPriceGroupController sellingPriceGroupController = new WebApi.SellingPriceGroupController();
            var sellingPriceGroupDeleteResult = await sellingPriceGroupController.SellingPriceGroupDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Stock Adjustment Reason
        public async Task<ActionResult> StockAdjustmentReason(ClsStockAdjustmentReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "StockAdjustmentReason";
            }
            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var allStockAdjustmentReasonsResult = await stockAdjustmentReasonController.AllStockAdjustmentReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStockAdjustmentReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.StockAdjustmentReasons = oClsResponse.Data.StockAdjustmentReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjustment reasons").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> StockAdjustmentReasonFetch(ClsStockAdjustmentReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "StockAdjustmentReason";
            }
            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var allStockAdjustmentReasonsResult = await stockAdjustmentReasonController.AllStockAdjustmentReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStockAdjustmentReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.StockAdjustmentReasons = oClsResponse.Data.StockAdjustmentReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjustment reasons").FirstOrDefault();

            return PartialView("PartialStockAdjustmentReason");
        }
        public async Task<ActionResult> StockAdjustmentReasonEdit(long StockAdjustmentReasonId)
        {
            ClsStockAdjustmentReasonVm obj = new ClsStockAdjustmentReasonVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.StockAdjustmentReasonId = StockAdjustmentReasonId;
            }
            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            ClsStockAdjustmentReason stockAdjustmentReasonObj = new ClsStockAdjustmentReason { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, StockAdjustmentReasonId = obj.StockAdjustmentReasonId };
            var stockAdjustmentReasonResult = await stockAdjustmentReasonController.StockAdjustmentReason(stockAdjustmentReasonObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonResult);

            ViewBag.StockAdjustmentReason = oClsResponse.Data.StockAdjustmentReason;
            return View();
        }
        public ActionResult StockAdjustmentReasonAdd()
        {
            return View();
        }
        public async Task<ActionResult> StockAdjustmentReasonInsert(ClsStockAdjustmentReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var insertStockAdjustmentReasonResult = await stockAdjustmentReasonController.InsertStockAdjustmentReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertStockAdjustmentReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockAdjustmentReasonUpdate(ClsStockAdjustmentReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var updateStockAdjustmentReasonResult = await stockAdjustmentReasonController.UpdateStockAdjustmentReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateStockAdjustmentReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockAdjustmentReasonActiveInactive(ClsStockAdjustmentReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var stockAdjustmentReasonActiveInactiveResult = await stockAdjustmentReasonController.StockAdjustmentReasonActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockAdjustmentReasonDelete(ClsStockAdjustmentReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockAdjustmentReasonController stockAdjustmentReasonController = new WebApi.StockAdjustmentReasonController();
            var stockAdjustmentReasonDeleteResult = await stockAdjustmentReasonController.StockAdjustmentReasonDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockAdjustmentReasonDeleteResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Stock Transfer Reason
        public async Task<ActionResult> StockTransferReason(ClsStockTransferReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "StockTransferReason";
            }
            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var allStockTransferReasonsResult = await stockTransferReasonController.AllStockTransferReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStockTransferReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.StockTransferReasons = oClsResponse.Data.StockTransferReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer reasons").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> StockTransferReasonFetch(ClsStockTransferReasonVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "StockTransferReason";
            }
            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var allStockTransferReasonsResult = await stockTransferReasonController.AllStockTransferReasons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStockTransferReasonsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.StockTransferReasons = oClsResponse.Data.StockTransferReasons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock transfer reasons").FirstOrDefault();

            return PartialView("PartialStockTransferReason");
        }
        public async Task<ActionResult> StockTransferReasonEdit(long StockTransferReasonId)
        {
            ClsStockTransferReasonVm obj = new ClsStockTransferReasonVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.StockTransferReasonId = StockTransferReasonId;
            }
            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            ClsStockTransferReason stockTransferReasonObj = new ClsStockTransferReason { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, StockTransferReasonId = obj.StockTransferReasonId };
            var stockTransferReasonResult = await stockTransferReasonController.StockTransferReason(stockTransferReasonObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonResult);

            ViewBag.StockTransferReason = oClsResponse.Data.StockTransferReason;
            return View();
        }
        public ActionResult StockTransferReasonAdd()
        {
            return View();
        }
        public async Task<ActionResult> StockTransferReasonInsert(ClsStockTransferReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var insertStockTransferReasonResult = await stockTransferReasonController.InsertStockTransferReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertStockTransferReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockTransferReasonUpdate(ClsStockTransferReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var updateStockTransferReasonResult = await stockTransferReasonController.UpdateStockTransferReason(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateStockTransferReasonResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockTransferReasonActiveInactive(ClsStockTransferReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var stockTransferReasonActiveInactiveResult = await stockTransferReasonController.StockTransferReasonActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StockTransferReasonDelete(ClsStockTransferReasonVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StockTransferReasonController stockTransferReasonController = new WebApi.StockTransferReasonController();
            var stockTransferReasonDeleteResult = await stockTransferReasonController.StockTransferReasonDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stockTransferReasonDeleteResult);
            return Json(oClsResponse);
        }
        #endregion
    }
}