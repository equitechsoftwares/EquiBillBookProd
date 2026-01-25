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
    public class AdminDashboardController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: AdminDashboard
        public ActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> MenuPermissions()
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
                obj.Domain = Request.Url.Host.Replace("www.", "");

                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(domainResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var userResult = await userController.FetchRootCompany(userObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.WhitelabelType = oClsResponse.WhitelabelType;
            ViewBag.UserType = oClsResponse.UserType;
            ViewBag.Domain = obj.Domain;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;

            ViewBag.RootUserId = oClsResponse1.Data.User.UserId;

            return PartialView("~/Views/Shared/PartialAdminMenus.cshtml");
        }

        public async Task<PartialViewResult> Footer()
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
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.DomainController domainController = new WebApi.DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.WhitelabelType = oClsResponse.WhitelabelType;
            ViewBag.BusinessSetting = oClsResponse.Data.BusinessSetting;

            return PartialView("~/Views/Shared/PartialAdminFooter.cshtml");
        }

    }
}