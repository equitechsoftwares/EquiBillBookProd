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
    public class ResellerTermsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: ResellerTerms
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Reseller Terms Page
            ViewBag.PageTitle = "Reseller Terms & Conditions | EquiBillBook GST Billing Software | Partnership Terms";
            ViewBag.MetaDescription = "Read EquiBillBook's reseller terms and conditions for GST billing software partnership. Learn about commission structure, partnership requirements, reseller obligations, and legal terms. Comprehensive reseller agreement for our billing software program.";
            ViewBag.MetaKeywords = "EquiBillBook reseller terms, GST billing software reseller agreement, reseller partnership terms, software reseller conditions, billing software reseller legal, accounting software reseller terms, GST software partnership, EquiBillBook reseller legal";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Reseller Terms & Conditions - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Read EquiBillBook's comprehensive reseller terms and conditions. Learn about commission structure, partnership requirements, and legal terms for our GST billing software reseller program.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook Reseller Terms - Partnership Agreement";
            ViewBag.TwitterDescription = "Read EquiBillBook's reseller terms and conditions for GST billing software partnership. Commission structure and legal terms.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/resellerterms";

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