using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Website
{
    public class TermsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Terms
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Terms Page
            ViewBag.PageTitle = "Terms and Conditions | EquiBillBook GST Billing Software | Legal Terms";
            ViewBag.MetaDescription = "Read EquiBillBook's terms and conditions for GST billing software usage. Understand user rights, responsibilities, service terms, data protection, and legal agreements. Comprehensive legal terms for our billing software service.";
            ViewBag.MetaKeywords = "EquiBillBook terms and conditions, GST billing software terms, software terms of service, billing software legal terms, accounting software terms, software user agreement, GST software legal terms, EquiBillBook legal";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Terms and Conditions - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Read EquiBillBook's comprehensive terms and conditions. Understand user rights, service terms, data protection, and legal agreements for our GST billing software.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook Terms and Conditions";
            ViewBag.TwitterDescription = "Read EquiBillBook's terms and conditions for GST billing software. Comprehensive legal terms and user agreements.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/terms";

            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                return View();
            }
            else
            {
                if (oClsResponse1.WhitelabelType == 0)
                {
                    return Redirect("/errorpage/domain");
                }
                else if (oClsResponse1.WhitelabelType == 2 || oClsResponse1.WhitelabelType == 1)
                {
                    return Redirect("/login");
                }
            }
            return View();
        }
    }
}