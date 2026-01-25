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
    public class AdminOtherSoftwareImportController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminOtherSoftwareImport
        public async Task<ActionResult> Index(long? UserId, long? Under)
        {
            ClsUserVm obj = new ClsUserVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                obj.UserType = "user";
                //obj.Title = "Users";
                obj.UserId = Convert.ToInt64(UserId);
                obj.Under = Convert.ToInt64(Under);
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId,
                PageIndex = obj.PageIndex,
                Domain = obj.Domain,
                UserId = obj.UserId
            };
            var result = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.OtherSoftwareImports = oClsResponse.Data.OtherSoftwareImports;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = oClsResponse.Data.PendingCount;
            ViewBag.UploadedCount = oClsResponse.Data.UploadedCount;
            ViewBag.RejectedCount = oClsResponse.Data.RejectedCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            return View();
        }

        public async Task<ActionResult> OtherSoftwareImportFetch(ClsOtherSoftwareImportVm obj)
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
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            var result = await otherSoftwareImportController.AllOtherSoftwareImports(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.OtherSoftwareImports = oClsResponse.Data.OtherSoftwareImports;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PendingCount = oClsResponse.Data.PendingCount;
            ViewBag.UploadedCount = oClsResponse.Data.UploadedCount;
            ViewBag.RejectedCount = oClsResponse.Data.RejectedCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialOtherSoftwareImport");
        }

        public async Task<ActionResult> UpdateStatus(ClsOtherSoftwareImportVm obj)
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
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.OtherSoftwareImportController otherSoftwareImportController = new WebApi.OtherSoftwareImportController();
            var result = await otherSoftwareImportController.UpdateStatus(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

    }
}