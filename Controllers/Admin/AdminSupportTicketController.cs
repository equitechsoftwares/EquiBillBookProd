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
    public class AdminSupportTicketController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminSupportTicket
        public async Task<ActionResult> Index(long? Under)
        {
            ClsSupportTicketVm obj = new ClsSupportTicketVm();
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
                obj.Title = "Support Ticket";
            }
            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var supportTicketsResult = await supportTicketController.AllSupportTicketsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(supportTicketsResult);

            ViewBag.SupportTickets = oClsResponse.Data.SupportTickets;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return View();
        }

        public async Task<ActionResult> SupportTicketFetch(ClsSupportTicketVm obj)
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
                obj.Title = "Support Ticket";
            }
            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var supportTicketsResult = await supportTicketController.AllSupportTicketsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(supportTicketsResult);
            ViewBag.SupportTickets = oClsResponse.Data.SupportTickets;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return PartialView("PartialSupportTicket");
        }

        public async Task<ActionResult> SupportTicketDetails(long Under,long SupportTicketId)
        {
            ClsSupportTicketVm obj = new ClsSupportTicketVm();
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
                obj.Title = "Support Ticket";
                obj.Under = Convert.ToInt64(Under);
                obj.SupportTicketId = SupportTicketId;
            }
            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var supportTicketResult = await supportTicketController.SupportTicketAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(supportTicketResult);

            WebApi.DomainController domainController = new WebApi.DomainController();
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.SupportTicket = oClsResponse.Data.SupportTicket;
            ViewBag.UserType = oClsResponse1.UserType;
            return View();
        }

        public async Task<ActionResult> SupportTicketDetailsInsert(ClsSupportTicketDetailsVm obj)
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
                obj.Platform = "Web";
            }
            WebApi.SupportTicketDetailsController supportTicketDetailsController = new WebApi.SupportTicketDetailsController();
            var insertResult = await supportTicketDetailsController.InsertSupportTicketDetailsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CloseSupportTicket(ClsSupportTicketVm obj)
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
                obj.Platform = "Web";
            }
            WebApi.SupportTicketController supportTicketController = new WebApi.SupportTicketController();
            var closeResult = await supportTicketController.CloseSupportTicketAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(closeResult);
            return Json(oClsResponse);
        }

    }
}