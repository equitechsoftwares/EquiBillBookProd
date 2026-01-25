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
    public class FaqController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: Faq
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for FAQ Page
            ViewBag.PageTitle = "GST Billing Software FAQ | Help Center | EquiBillBook";
            ViewBag.MetaDescription = "Find answers to common questions about EquiBillBook GST billing software. Learn about pricing, free trial, data security, account setup & more.";
            ViewBag.MetaKeywords = "EquiBillBook FAQ, GST billing software questions, billing software help, accounting software FAQ, free trial questions, data security FAQ, cloud billing software help, EquiBillBook support, CA firm software help, POS billing help, multi-location accounting help";
            
            // Open Graph Tags
            ViewBag.OgTitle = "GST Billing Software FAQ | Help Center | EquiBillBook";
            ViewBag.OgDescription = "Get instant answers to common EquiBillBook questions. Learn about pricing, free trial, security, setup, and features. Comprehensive FAQ for GST billing software users.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "GST Software FAQ | Help Center | EquiBillBook";
            ViewBag.TwitterDescription = "Find answers to EquiBillBook questions: pricing, free trial, security, setup, features. Complete FAQ for GST billing software users.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/faq";

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