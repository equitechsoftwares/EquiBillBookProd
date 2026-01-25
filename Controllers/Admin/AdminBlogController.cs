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
    public class AdminBlogController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index(ClsBlogVm obj)
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

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.AllBlogs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Blogs = oClsResponse.Data.Blogs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return View();
        }

        public async Task<ActionResult> BlogFetch(ClsBlogVm obj)
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
            
            // Ensure PageIndex is at least 1
            if (obj.PageIndex <= 0)
            {
                obj.PageIndex = 1;
            }
            
            // Ensure PageSize has a valid default
            if (obj.PageSize <= 0)
            {
                obj.PageSize = 10;
            }
            
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.AllBlogs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Blogs = oClsResponse.Data.Blogs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PublishedCount = oClsResponse.Data.PublishedCount;
            ViewBag.UnpublishedCount = oClsResponse.Data.UnpublishedCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialAdminBlog");
        }

        public ActionResult Add()
        {
            return View();
        }

        public async Task<ActionResult> Edit(long BlogId)
        {
            ClsBlogVm obj = new ClsBlogVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            obj.BlogId = BlogId;
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.Blog(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            ViewBag.Blog = oClsResponse.Data.Blog;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Insert(ClsBlogVm obj)
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
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.InsertBlog(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Update(ClsBlogVm obj)
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
                if (obj.AddedBy == 0)
                {
                    obj.AddedBy = Convert.ToInt64(arr[2]);
                }
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.UpdateBlog(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(ClsBlogVm obj)
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
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.DeleteBlog(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> GetBlog(ClsBlogVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.Blog(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ActiveInactive(ClsBlogVm obj)
        {
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogController blogController = new WebApi.BlogController();
            var result = await blogController.BlogActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ActiveCategories(ClsBlogCategoryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.BlogCategoryController blogCategoryController = new WebApi.BlogCategoryController();
            var result = await blogCategoryController.ActiveBlogCategories(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}

