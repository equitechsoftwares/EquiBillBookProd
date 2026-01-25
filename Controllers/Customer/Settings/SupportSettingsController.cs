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

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class SupportSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: SupportSettings
        public ActionResult Feedback()
        {
            return View();
        }

        public ActionResult FeedbackAdd()
        {
            return View();
        }
        public async Task<ActionResult> FeedbackInsert(ClsFeedbackVm obj)
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

            WebApi.FeedbackController feedbackController = new WebApi.FeedbackController();
            var insertFeedbackResult = await feedbackController.InsertFeedback(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertFeedbackResult);
            return Json(oClsResponse);
        }

        #region Support Ticket
        public async Task<ActionResult> SupportTicket(ClsSupportTicketVm obj)
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
                obj.Title = "Support Ticket";
            }

            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var allSupportTicketsResult = await supportTicketController.AllSupportTickets(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSupportTicketsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.SupportTickets = oClsResponse.Data.SupportTickets;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "support ticket").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> SupportTicketFetch(ClsSupportTicketVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Title = "Support Ticket";
            }
            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var allSupportTicketsResult = await supportTicketController.AllSupportTickets(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSupportTicketsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SupportTickets = oClsResponse.Data.SupportTickets;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "support ticket").FirstOrDefault();

            return PartialView("PartialSupportTicket");
        }

        public ActionResult SupportTicketAdd()
        {
            return View();
        }

        public async Task<ActionResult> SupportTicketInsert(ClsSupportTicketVm obj)
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

            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var insertSupportTicketResult = await supportTicketController.InsertSupportTicket(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSupportTicketResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> SupportTicketDetails(long SupportTicketId)
        {
            ClsSupportTicketVm obj = new ClsSupportTicketVm();
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
                obj.Title = "Support Ticket";
                obj.SupportTicketId = SupportTicketId;
            }

            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            ClsSupportTicket supportTicketObj = new ClsSupportTicket
            {
                SupportTicketId = obj.SupportTicketId,
                CompanyId = obj.CompanyId
            };
            var supportTicketResult = await supportTicketController.SupportTicket(supportTicketObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(supportTicketResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm
            {
                AddedBy = obj.AddedBy,
                CompanyId = obj.CompanyId
            };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SupportTicket = oClsResponse.Data.SupportTicket;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "support ticket").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> SupportTicketDetailsInsert(ClsSupportTicketDetailsVm obj)
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

            WebApi.SupportTicketDetailsController supportTicketDetailsController = new WebApi.SupportTicketDetailsController();
            var insertSupportTicketDetailsResult = await supportTicketDetailsController.InsertSupportTicketDetails(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSupportTicketDetailsResult);
            return Json(oClsResponse);
        }
        #endregion
    }
}