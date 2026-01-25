using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminBlogCategoryController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.AllBlogCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.BlogCategories = oClsResponse.Data.BlogCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return View();
        }

        public async Task<ActionResult> BlogCategoryFetch(ClsBlogCategoryVm obj)
        {
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

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.AllBlogCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.BlogCategories = oClsResponse.Data.BlogCategories;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialAdminBlogCategory");
        }

        public ActionResult Add()
        {
            return View();
        }

        public async Task<ActionResult> Edit(long BlogCategoryId)
        {
            ClsBlogCategoryVm obj = new ClsBlogCategoryVm();
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

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            ClsBlogCategory blogCategoryObj = new ClsBlogCategory { BlogCategoryId = BlogCategoryId, CompanyId = obj.CompanyId };
            var result = await blogCategoryController.BlogCategory(blogCategoryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            ViewBag.BlogCategory = oClsResponse.Data.BlogCategory;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Insert(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            obj.IsActive = true;
            obj.IsDeleted = false;
            obj.Browser = Request.UserAgent;
            obj.Platform = Request.Browser.Platform;
            obj.IpAddress = Request.UserHostAddress;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.InsertBlogCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Update(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.ModifiedBy = Convert.ToInt64(arr[2]);
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            obj.IsDeleted = false;
            obj.Browser = Request.UserAgent;
            obj.Platform = Request.Browser.Platform;
            obj.IpAddress = Request.UserHostAddress;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.UpdateBlogCategory(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            obj.Browser = Request.UserAgent;
            obj.Platform = Request.Browser.Platform;
            obj.IpAddress = Request.UserHostAddress;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.BlogCategoryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ActiveInactive(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            obj.Browser = Request.UserAgent;
            obj.Platform = Request.Browser.Platform;
            obj.IpAddress = Request.UserHostAddress;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.BlogCategoryActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}

