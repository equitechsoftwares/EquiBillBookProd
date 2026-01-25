using EquiBillBook.Models;
using EquiBillBook.Controllers.Website;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using System.Data.Entity;

namespace EquiBillBook.Controllers.Website
{
    public class SitemapController : Controller
    {
        // GET: Sitemap
        public async Task<ActionResult> Index()
        {
            // Set content type to XML
            Response.ContentType = "application/xml";
            Response.ContentEncoding = Encoding.UTF8;

            string baseUrl = Request.Url != null 
                ? Request.Url.Scheme + "://" + Request.Url.Authority 
                : "https://equibillbook.com";

            // Create XML document
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            // Create root element
            XmlElement urlset = xmlDoc.CreateElement("urlset");
            urlset.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            xmlDoc.AppendChild(urlset);

            // Add static pages
            AddUrl(xmlDoc, urlset, baseUrl, "", "weekly", "1.0");
            AddUrl(xmlDoc, urlset, baseUrl, "/home", "weekly", "1.0");
            AddUrl(xmlDoc, urlset, baseUrl, "/aboutus", "monthly", "0.8");
            AddUrl(xmlDoc, urlset, baseUrl, "/features", "monthly", "0.9");
            AddUrl(xmlDoc, urlset, baseUrl, "/pricing", "weekly", "0.9");
            AddUrl(xmlDoc, urlset, baseUrl, "/contact", "monthly", "0.7");
            AddUrl(xmlDoc, urlset, baseUrl, "/download", "monthly", "0.8");
            AddUrl(xmlDoc, urlset, baseUrl, "/faq", "monthly", "0.6");

            // Add blog index page
            AddUrl(xmlDoc, urlset, baseUrl, "/blog", "weekly", "0.8");

            // Add knowledge base pages
            AddUrl(xmlDoc, urlset, baseUrl, "/knowledgebase", "weekly", "0.9");
            
            // Add all knowledge base articles dynamically
            try
            {
                var allArticles = await KnowledgeBaseArticles.GetAllArticlesAsync();
                foreach (var article in allArticles)
                {
                    string lastMod = article.LastUpdated.ToString("yyyy-MM-dd");
                    AddUrl(xmlDoc, urlset, baseUrl, "/knowledgebase/article/" + article.Slug, "monthly", "0.8", lastMod);
                }
                
                // Add category pages
                var categories = await KnowledgeBaseArticles.GetCategoriesAsync();
                foreach (var category in categories)
                {
                    string categorySlug = category.ToLower().Replace(" ", "-");
                    AddUrl(xmlDoc, urlset, baseUrl, "/knowledgebase/category/" + categorySlug, "weekly", "0.7");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break sitemap generation
                System.Diagnostics.Debug.WriteLine("Error adding knowledge base articles to sitemap: " + ex.Message);
            }

            // Add all published blog posts dynamically
            using (var db = new ConnectionContext())
            {
                var publishedBlogs = await (from a in db.DbClsBlog
                                     where a.IsDeleted == false &&
                                           a.IsActive == true &&
                                           a.CompanyId == 0 && // Only main domain blogs
                                           a.UniqueSlug != null &&
                                           a.UniqueSlug != ""
                                     select new
                                     {
                                         a.UniqueSlug,
                                         a.PublishedDate,
                                         a.ModifiedOn,
                                         a.AddedOn
                                     })
                                     .OrderByDescending(a => a.PublishedDate ?? a.AddedOn)
                                     .ToListAsync();

                // Filter empty strings in memory
                var filteredBlogs = publishedBlogs.Where(a => !string.IsNullOrWhiteSpace(a.UniqueSlug)).ToList();

                foreach (var blog in filteredBlogs)
                {
                    DateTime lastModified = blog.ModifiedOn ?? blog.PublishedDate ?? blog.AddedOn;
                    string lastMod = lastModified.ToString("yyyy-MM-dd");
                    AddUrl(xmlDoc, urlset, baseUrl, "/blog/" + blog.UniqueSlug, "weekly", "0.7", lastMod);
                }
            }

            // Add authentication pages
            AddUrl(xmlDoc, urlset, baseUrl, "/login", "monthly", "0.8");
            AddUrl(xmlDoc, urlset, baseUrl, "/register", "monthly", "0.8");
            AddUrl(xmlDoc, urlset, baseUrl, "/forgotpassword", "monthly", "0.5");
            AddUrl(xmlDoc, urlset, baseUrl, "/resetpassword", "monthly", "0.5");

            // Add reseller pages
            AddUrl(xmlDoc, urlset, baseUrl, "/register/reseller", "monthly", "0.6");
            AddUrl(xmlDoc, urlset, baseUrl, "/register/whitelabel", "monthly", "0.6");

            // Add legal pages
            AddUrl(xmlDoc, urlset, baseUrl, "/terms", "monthly", "0.4");
            AddUrl(xmlDoc, urlset, baseUrl, "/privacypolicy", "monthly", "0.4");
            AddUrl(xmlDoc, urlset, baseUrl, "/refundpolicy", "monthly", "0.4");
            AddUrl(xmlDoc, urlset, baseUrl, "/resellerterms", "monthly", "0.5");
            AddUrl(xmlDoc, urlset, baseUrl, "/whitelabelterms", "monthly", "0.5");

            return Content(xmlDoc.OuterXml, "application/xml", Encoding.UTF8);
        }

        private void AddUrl(XmlDocument doc, XmlElement parent, string baseUrl, string path, string changefreq, string priority, string lastmod = null)
        {
            XmlElement url = doc.CreateElement("url");
            parent.AppendChild(url);

            XmlElement loc = doc.CreateElement("loc");
            loc.InnerText = baseUrl + path;
            url.AppendChild(loc);

            if (!string.IsNullOrWhiteSpace(lastmod))
            {
                XmlElement lastmodElement = doc.CreateElement("lastmod");
                lastmodElement.InnerText = lastmod;
                url.AppendChild(lastmodElement);
            }

            XmlElement changefreqElement = doc.CreateElement("changefreq");
            changefreqElement.InnerText = changefreq;
            url.AppendChild(changefreqElement);

            XmlElement priorityElement = doc.CreateElement("priority");
            priorityElement.InnerText = priority;
            url.AppendChild(priorityElement);
        }
    }
}

