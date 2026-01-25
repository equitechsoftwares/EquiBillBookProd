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
    public class PrivacyPolicyController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: PrivacyPolicy
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Privacy Policy Page
            ViewBag.PageTitle = "Privacy Policy | EquiBillBook GST Billing Software | Data Protection";
            ViewBag.MetaDescription = "Read EquiBillBook's privacy policy for GST billing software. Learn how we protect your data, handle personal information, ensure security, and maintain privacy compliance. Comprehensive data protection and privacy terms for our billing software service.";
            ViewBag.MetaKeywords = "EquiBillBook privacy policy, GST billing software privacy, data protection policy, software privacy terms, billing software data security, accounting software privacy, GST software data protection, EquiBillBook data privacy";
            
            // Open Graph Tags
            ViewBag.OgTitle = "Privacy Policy - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Read EquiBillBook's comprehensive privacy policy. Learn how we protect your data, ensure security, and maintain privacy compliance for our GST billing software.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook Privacy Policy - Data Protection";
            ViewBag.TwitterDescription = "Read EquiBillBook's privacy policy for GST billing software. Comprehensive data protection and privacy terms.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/privacypolicy";

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