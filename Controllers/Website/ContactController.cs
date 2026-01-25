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
    public class ContactController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: Contact
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for Contact Page
            ViewBag.PageTitle = "GST Billing Software Support | Kolkata, India | EquiBillBook";
            ViewBag.MetaDescription = "Get in touch with EquiBillBook GST billing software team. Contact +91 80132 00659, info@equibillbook.com. Located in Kolkata, India.";
            ViewBag.MetaKeywords = "contact EquiBillBook, GST billing software support, billing software help, EquiBillBook contact number, accounting software support India, GST software customer service, business software support Kolkata, EquiBillBook phone number, cloud accounting support, CA firm software support";
            
            // Open Graph Tags
            ViewBag.OgTitle = "GST Billing Software Support | Kolkata, India | EquiBillBook";
            ViewBag.OgDescription = "Get expert support for your GST billing software. Call +91 80132 00659, email info@equibillbook.com, or use WhatsApp. Located in Kolkata, India.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "GST Software Support | Kolkata, India | EquiBillBook";
            ViewBag.TwitterDescription = "Need help with GST billing software? Call +91 80132 00659, WhatsApp support available. Expert assistance for Indian businesses.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/contact";

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

        public async Task<ActionResult> RequestQuote(ClsEnquiryVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            EnquiryController enquiryController = new EnquiryController();

            // Call API method directly
            var enquiryResult = await enquiryController.InsertEnquiry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(enquiryResult);

            return Json(oClsResponse);
        }

    }
}