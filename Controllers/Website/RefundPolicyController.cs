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
    public class RefundPolicyController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: RefundPolicy
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Refund Policy Page
            ViewBag.PageTitle = "Refund Policy | EquiBillBook GST Billing Software | Money Back Guarantee";
            ViewBag.MetaDescription = "Read EquiBillBook's refund policy for GST billing software. Learn about our money-back guarantee, refund terms, cancellation policy, and customer satisfaction commitment. Clear refund terms for our billing software service.";
            ViewBag.MetaKeywords = "EquiBillBook refund policy, GST billing software refund, money back guarantee, software refund terms, billing software cancellation, accounting software refund policy, GST software money back, EquiBillBook refund terms";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Refund Policy - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Read EquiBillBook's comprehensive refund policy. Learn about our money-back guarantee, refund terms, and customer satisfaction commitment for our GST billing software.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook Refund Policy - Money Back Guarantee";
            ViewBag.TwitterDescription = "Read EquiBillBook's refund policy for GST billing software. Clear refund terms and money-back guarantee.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/refundpolicy";

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