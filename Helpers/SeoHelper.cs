using EquiBillBook.Models;
using System.Linq;
using System.Web.Mvc;

namespace EquiBillBook.Helpers
{
    public static class SeoHelper
    {
        public static void LoadSeoSettings(Controller controller, string pageIdentifier)
        {
            using (var _db = new ConnectionContext())
            {
                // Get domain from request (same pattern as login page)
                string domain = controller.Request.Url != null 
                    ? controller.Request.Url.Host.Replace("www.", "") 
                    : string.Empty;

                // Get CompanyId from domain (same pattern as DomainController.DomainCheckForRedirection)
                long companyId = 0;
                if (!string.IsNullOrEmpty(domain))
                {
                    companyId = _db.DbClsDomain
                        .Where(a => a.Domain == domain && a.IsDeleted == false && a.IsActive == true)
                        .Select(a => a.CompanyId)
                        .FirstOrDefault();
                }

                // Load SEO settings filtered by PageIdentifier and CompanyId
                var seoSettings = _db.DbClsPageSeoSettings
                    .FirstOrDefault(s => s.PageIdentifier.ToLower() == pageIdentifier.ToLower() 
                        && s.CompanyId == companyId
                        && s.IsActive 
                        && !s.IsDeleted);

                if (seoSettings != null)
                {
                    controller.ViewBag.PageTitle = seoSettings.PageTitle;
                    controller.ViewBag.MetaDescription = seoSettings.MetaDescription;
                    controller.ViewBag.MetaKeywords = seoSettings.MetaKeywords;
                    controller.ViewBag.OgTitle = seoSettings.OgTitle;
                    controller.ViewBag.OgDescription = seoSettings.OgDescription;
                    controller.ViewBag.OgImage = seoSettings.OgImage;
                    controller.ViewBag.OgUrl = seoSettings.OgUrl;
                    controller.ViewBag.TwitterTitle = seoSettings.TwitterTitle;
                    controller.ViewBag.TwitterDescription = seoSettings.TwitterDescription;
                    controller.ViewBag.TwitterImage = seoSettings.TwitterImage;
                    controller.ViewBag.CanonicalUrl = seoSettings.CanonicalUrl;
                }
            }
        }
    }
}

