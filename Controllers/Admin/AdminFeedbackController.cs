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
    public class AdminFeedbackController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminFeedback
        public async Task<ActionResult> Index(long? Under)
        {
            ClsFeedbackVm obj = new ClsFeedbackVm();
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
                obj.Under = Convert.ToInt64(Under);
                //obj.PageSize = 10;
                //obj.Title = "Feedback";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.FeedbackController feedbackController = new WebApi.FeedbackController();
            var result = await feedbackController.AllFeedbacksAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Feedbacks = oClsResponse.Data.Feedbacks;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return View();
        }

        public async Task<ActionResult> FeedbackFetch(ClsFeedbackVm obj)
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
                //obj.Title = "Support Ticket";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.FeedbackController feedbackController = new WebApi.FeedbackController();
            var result = await feedbackController.AllFeedbacksAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            ViewBag.Feedbacks = oClsResponse.Data.Feedbacks;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return PartialView("PartialFeedback");
        }

    }
}