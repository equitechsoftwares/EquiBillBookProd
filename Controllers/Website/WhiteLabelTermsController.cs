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
    public class WhiteLabelTermsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: WhiteLabelTerms
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            // SEO Meta Tags for White Label Terms Page
            ViewBag.PageTitle = "White-Label Terms & Conditions | EquiBillBook GST Billing Software | Custom Branding Terms";
            ViewBag.MetaDescription = "Read EquiBillBook's white-label terms and conditions for GST billing software customization. Learn about custom branding rights, white-label partnership terms, brand customization rules, and legal agreements. Comprehensive white-label agreement for our billing software program.";
            ViewBag.MetaKeywords = "EquiBillBook white-label terms, GST billing software white-label agreement, white-label partnership terms, custom branding terms, software white-label conditions, billing software white-label legal, accounting software white-label terms, GST software white-label partnership, EquiBillBook white-label legal";
            
            // Open Graph Tags
            ViewBag.OgTitle = "White-Label Terms & Conditions - EquiBillBook GST Billing Software";
            ViewBag.OgDescription = "Read EquiBillBook's comprehensive white-label terms and conditions. Learn about custom branding rights, white-label partnership terms, and legal agreements for our GST billing software white-label program.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "EquiBillBook White-Label Terms - Custom Branding Agreement";
            ViewBag.TwitterDescription = "Read EquiBillBook's white-label terms and conditions for GST billing software customization. Custom branding rights and legal terms.";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/whitelabelterms";

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