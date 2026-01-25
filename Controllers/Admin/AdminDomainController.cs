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
    public class AdminDomainController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: AdminDomain
        public async Task<ActionResult> Index(long UserId)
        {
            ClsDomainVm obj = new ClsDomainVm();
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
                obj.Title = "Domains";
                obj.UserId = UserId;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var result = await domainController.AllDomainsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Domains = oClsResponse.Data.Domains;
            return View();
        }

        public async Task<ActionResult> DomainFetch(ClsDomainVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                //obj.Title = "State";
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var result = await domainController.AllDomainsAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Domains = oClsResponse.Data.Domains;
            return PartialView("PartialDomain");
        }

        public async Task<ActionResult> DomainInsert(ClsDomainVm obj)
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
                //obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var result = await domainController.InsertDomainAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> DomainDelete(ClsDomainVm obj)
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
                //obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var result = await domainController.DomainDeleteAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> VerifyDomainConnection(ClsDomainVm obj)
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
                //obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var result = await domainController.VerifyDomainConnectionAdmin(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse);
        }
    }
}