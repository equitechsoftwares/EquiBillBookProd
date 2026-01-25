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
    public class FeaturesController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Features
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Features Page
            ViewBag.PageTitle = "Complete GST Billing Software Features | 48+ Tools | EquiBillBook";
            ViewBag.MetaDescription = "Explore 48+ powerful GST billing software features: multi-location management, inventory tracking, POS billing, role management, barcode scanning.";
            ViewBag.MetaKeywords = "GST billing software features, inventory management features, POS billing system, multi-location accounting, role management software, barcode scanning software, tax reports GST, WhatsApp billing software, expense management software, stock transfer, accounting software features India, cloud accounting features, CA firm software features, wholesale billing features";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Complete GST Billing Software Features | 48+ Tools | EquiBillBook";
            ViewBag.OgDescription = "Discover 48+ powerful features: Multi-location management, advanced inventory tracking, POS billing, GST compliance, role management, and WhatsApp integration. Perfect for Indian businesses.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/features.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "48+ GST Billing Software Features | EquiBillBook";
            ViewBag.TwitterDescription = "48+ powerful features for Indian businesses: Multi-location management, inventory tracking, POS billing, GST compliance & more. See all features!";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/features.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/features";

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