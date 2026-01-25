using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Website
{
    public class DownloadController : Controller
    {
        // GET: Download
        public ActionResult Index()
        {
            // SEO Meta Tags for Download Page
            ViewBag.PageTitle = "GST Billing App Download | Android, iOS & Desktop | EquiBillBook";
            ViewBag.MetaDescription = "Download EquiBillBook GST billing software app for Android, iPhone, iPad, and desktop. PWA works offline, auto-updates. Free download!";
            ViewBag.MetaKeywords = "download EquiBillBook, GST billing app download, billing software app, Android billing app, iPhone billing app, desktop billing software, PWA download, offline billing app, free GST software download, mobile billing app, cloud accounting app, CA firm software download";
            
            // Open Graph Tags
            ViewBag.OgTitle = "GST Billing App Download | Android, iOS & Desktop | EquiBillBook";
            ViewBag.OgDescription = "Download EquiBillBook GST billing app for Android, iPhone, iPad, and desktop. Works offline, auto-updates, native app experience. Free download!";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Twitter Tags  
            ViewBag.TwitterTitle = "GST Billing App Download | All Devices | EquiBillBook";
            ViewBag.TwitterDescription = "Free GST billing app for Android, iPhone, iPad & desktop. Works offline, auto-updates, native app experience. Download now!";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";
            
            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/download";

            return View();
        }
    }
}