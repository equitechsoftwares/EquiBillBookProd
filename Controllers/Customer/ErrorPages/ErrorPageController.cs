using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{
    public class ErrorPageController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: ErrorPage
        public async Task<ActionResult> Index()
        {
            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            DomainController domainController = new DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
            return View();
        }

        public async Task<ActionResult> LinkExpired()
        {
            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            DomainController domainController = new DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
            return View();
        }

        public async Task<ActionResult> Domain()
        {
            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            DomainController domainController = new DomainController();
            var domainResult = await domainController.DomainCheckForRedirection(obj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ViewBag.BusinessSetting = oClsResponse1.Data.BusinessSetting;
            return View();
        }
    }
}