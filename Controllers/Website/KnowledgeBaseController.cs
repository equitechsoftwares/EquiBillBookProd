using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Website
{
    public class KnowledgeBaseController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: KnowledgeBase
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                // Load all articles and categories
                ViewBag.Articles = await KnowledgeBaseArticles.GetAllArticlesAsync();
                ViewBag.Categories = await KnowledgeBaseArticles.GetCategoriesAsync();

                // SEO Meta Tags
                ViewBag.PageTitle = "Knowledge Base - Help Center | EquiBillBook";
                ViewBag.MetaDescription = "Comprehensive help center and knowledge base for EquiBillBook. Find guides, tutorials, and answers to common questions about billing, inventory, accounting, and more.";
                ViewBag.MetaKeywords = "EquiBillBook help, knowledge base, user guide, billing software help, accounting software tutorial, GST software guide";
                ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/knowledgebase";
                ViewBag.OgTitle = "Knowledge Base - Help Center | EquiBillBook";
                ViewBag.OgDescription = ViewBag.MetaDescription;
                ViewBag.OgImage = Request.Url.Scheme + "://" + Request.Url.Host + "/Content/web-assets/images/innerbanner.png";

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

        // GET: KnowledgeBase/Article/{slug}
        public async Task<ActionResult> Article(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index");
            }

            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                var article = await KnowledgeBaseArticles.GetArticleBySlugAsync(slug);
                
                if (article == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Article = article;
                ViewBag.RelatedArticles = await KnowledgeBaseArticles.GetRelatedArticlesAsync(article.Category, article.Slug);
                ViewBag.Categories = await KnowledgeBaseArticles.GetCategoriesAsync();

                // SEO Meta Tags
                ViewBag.PageTitle = article.Title + " - Knowledge Base | EquiBillBook";
                ViewBag.MetaDescription = article.Description;
                ViewBag.MetaKeywords = string.Join(", ", article.Tags);
                ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/knowledgebase/article/" + article.Slug;
                ViewBag.OgTitle = article.Title;
                ViewBag.OgDescription = article.Description;
                ViewBag.OgImage = Request.Url.Scheme + "://" + Request.Url.Host + "/Content/web-assets/images/innerbanner.png";
                ViewBag.TwitterTitle = article.Title;
                ViewBag.TwitterDescription = article.Description;
                ViewBag.TwitterImage = ViewBag.OgImage;

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

        // GET: KnowledgeBase/Category/{category}
        public async Task<ActionResult> Category(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return RedirectToAction("Index");
            }

            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                // Convert URL slug back to category name (e.g., "getting-started" -> "Getting Started")
                var allCategories = await KnowledgeBaseArticles.GetCategoriesAsync();
                var categoryName = allCategories.FirstOrDefault(c => 
                    c.ToLower().Replace(" ", "-") == category.ToLower().Replace(" ", "-")) ?? category;
                
                ViewBag.Category = categoryName;
                var categoryArticles = await KnowledgeBaseArticles.GetArticlesByCategoryAsync(categoryName);
                ViewBag.Articles = categoryArticles;
                ViewBag.Categories = allCategories;
                
                // SEO Meta Tags
                ViewBag.PageTitle = categoryName + " - Knowledge Base | EquiBillBook";
                ViewBag.MetaDescription = "Explore " + categoryName + " articles and guides in EquiBillBook. Find comprehensive tutorials, step-by-step instructions, and helpful resources for " + categoryName.ToLower() + " features.";
                ViewBag.MetaKeywords = categoryName + ", EquiBillBook " + categoryName + ", " + categoryName + " guide, " + categoryName + " tutorial, billing software help";
                ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/knowledgebase/category/" + category;
                ViewBag.OgTitle = categoryName + " - Knowledge Base | EquiBillBook";
                ViewBag.OgDescription = ViewBag.MetaDescription;
                ViewBag.OgImage = Request.Url.Scheme + "://" + Request.Url.Host + "/Content/web-assets/images/innerbanner.png";
                ViewBag.TwitterTitle = categoryName + " - Knowledge Base | EquiBillBook";
                ViewBag.TwitterDescription = ViewBag.MetaDescription;
                ViewBag.TwitterImage = ViewBag.OgImage;
                
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

    // Helper class to load knowledge base articles from JSON and HTML files
    public static class KnowledgeBaseArticles
    {
        private static List<KnowledgeBaseArticle> _articles = null;
        private static readonly SemaphoreSlim _lockObject = new SemaphoreSlim(1, 1);
        private static DateTime _lastLoadTime = DateTime.MinValue;

        private static string GetJsonFilePath()
        {
            return HostingEnvironment.MapPath("~/Content/KnowledgeBase/articles.json");
        }

        private static string GetArticlesFolderPath()
        {
            return HostingEnvironment.MapPath("~/Content/KnowledgeBase/articles");
        }

        private static async Task LoadArticlesAsync()
        {
            await _lockObject.WaitAsync();
            try
            {
                // Reload if file has changed (simple cache invalidation)
                var jsonFilePath = GetJsonFilePath();
                if (!File.Exists(jsonFilePath))
                {
                    _articles = new List<KnowledgeBaseArticle>();
                    return;
                }

                var fileInfo = new FileInfo(jsonFilePath);
                if (_articles != null && fileInfo.LastWriteTime <= _lastLoadTime)
                {
                    return; // Already loaded and file hasn't changed
                }

                _articles = new List<KnowledgeBaseArticle>();
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                try
                {
                    // Read JSON file asynchronously
                    string jsonContent;
                    using (var reader = new StreamReader(jsonFilePath))
                    {
                        jsonContent = await reader.ReadToEndAsync();
                    }
                    var articleMetadata = serializer.Deserialize<List<ArticleMetadata>>(jsonContent);

                    // Load HTML content for each article
                    var articlesFolder = GetArticlesFolderPath();
                    foreach (var metadata in articleMetadata)
                    {
                        var article = new KnowledgeBaseArticle
                        {
                            Slug = metadata.slug,
                            Title = metadata.title,
                            Category = metadata.category,
                            Description = metadata.description,
                            Tags = metadata.tags ?? new List<string>(),
                            LastUpdated = DateTime.TryParse(metadata.lastUpdated, out DateTime lastUpdated) 
                                ? lastUpdated 
                                : DateTime.Now
                        };

                        // Load HTML content from file asynchronously
                        string htmlFilePath = Path.Combine(articlesFolder, metadata.contentFile);
                        if (File.Exists(htmlFilePath))
                        {
                            using (var reader = new StreamReader(htmlFilePath))
                            {
                                article.Content = await reader.ReadToEndAsync();
                            }
                        }
                        else
                        {
                            article.Content = "<p>Content file not found.</p>";
                        }

                        _articles.Add(article);
                    }

                    _lastLoadTime = fileInfo.LastWriteTime;
                }
                catch (Exception ex)
                {
                    // Log error (you might want to use a proper logging framework)
                    System.Diagnostics.Debug.WriteLine("Error loading knowledge base articles: " + ex.Message);
                    _articles = new List<KnowledgeBaseArticle>();
                }
            }
            finally
            {
                _lockObject.Release();
            }
        }

        public static async Task<List<KnowledgeBaseArticle>> GetAllArticlesAsync()
        {
            await LoadArticlesAsync();
            return _articles.OrderBy(a => a.Category).ThenBy(a => a.Title).ToList();
        }

        public static async Task<KnowledgeBaseArticle> GetArticleBySlugAsync(string slug)
        {
            await LoadArticlesAsync();
            return _articles.FirstOrDefault(a => a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<List<KnowledgeBaseArticle>> GetArticlesByCategoryAsync(string category)
        {
            await LoadArticlesAsync();
            return _articles.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                          .OrderBy(a => a.Title).ToList();
        }

        public static async Task<List<string>> GetCategoriesAsync()
        {
            await LoadArticlesAsync();
            
            // Define the desired category order as per KnowledgeBase_Plan.md
            var categoryOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Getting Started", 1 },
                { "Sales", 2 },
                { "Purchase", 3 },
                { "Inventory", 4 },
                { "Accounts", 5 },
                { "Banking", 6 },
                { "Restaurant", 7 },
                { "Customers", 8 },
                { "Suppliers", 9 },
                { "Expense", 10 },
                { "Reports", 11 },
                { "Settings", 12 },
                { "Additional Features", 13 },
                { "Troubleshooting", 14 },
                { "Integrations", 15 }
            };
            
            var categories = _articles.Select(a => a.Category).Distinct().ToList();
            
            // Sort categories by the predefined order, then alphabetically for any categories not in the list
            return categories
                .OrderBy(c => categoryOrder.ContainsKey(c) ? categoryOrder[c] : 999)
                .ThenBy(c => c)
                .ToList();
        }

        public static async Task<List<KnowledgeBaseArticle>> GetRelatedArticlesAsync(string category, string excludeSlug)
        {
            await LoadArticlesAsync();
            return _articles.Where(a => a.Category.Equals(category, StringComparison.OrdinalIgnoreCase) 
                                    && !a.Slug.Equals(excludeSlug, StringComparison.OrdinalIgnoreCase))
                          .Take(5).ToList();
        }

        // Helper class for JSON deserialization
        private class ArticleMetadata
        {
            public string slug { get; set; }
            public string title { get; set; }
            public string category { get; set; }
            public string description { get; set; }
            public List<string> tags { get; set; }
            public string contentFile { get; set; }
            public string lastUpdated { get; set; }
        }
    }

    // Article model class
    public class KnowledgeBaseArticle
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Content { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
