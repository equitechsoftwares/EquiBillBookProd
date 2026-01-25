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
    public class AboutUsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: AboutUs
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for About Us Page
            ViewBag.PageTitle = "Leading GST Billing Software Company | Our Story | EquiBillBook";
            ViewBag.MetaDescription = "Learn about EquiBillBook, India's trusted GST billing software company. Founded by Equitech Softwares, serving 2000+ businesses with cloud accounting.";
            ViewBag.MetaKeywords = "about EquiBillBook, GST billing software company, Equitech Softwares, cloud accounting company India, billing software developers, business management software company, Indian software company, accounting software provider, CA firm software company, POS billing software company";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Leading GST Billing Software Company | Our Story | EquiBillBook";
            ViewBag.OgDescription = "Discover EquiBillBook's journey in revolutionizing business management for Indian entrepreneurs. Trusted by 2000+ businesses with advanced GST billing and accounting solutions.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/features.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "GST Billing Software Leaders | EquiBillBook";
            ViewBag.TwitterDescription = "Learn about India's most trusted GST billing software company. Serving 2000+ businesses with innovative cloud-based accounting solutions.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/features.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/aboutus";

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