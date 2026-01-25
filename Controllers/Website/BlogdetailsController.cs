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
    public class BlogdetailsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        
        // GET: Blogdetails
        public async Task<ActionResult> Index(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return RedirectToAction("Index", "Blog");
            }

            // Convert title slug back to title format for lookup, or use as slug
            string slug = title.Replace('-', ' ').ToLower();
            
            // Check domain and get company ID
            ClsDomainVm obj = new ClsDomainVm();
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            long companyId = 0;
            if (obj.Domain != "localhost" && obj.Domain != "equibillbook.com" && 
                obj.Domain != "equitechsoftwares.in" && obj.Domain != "demo1.equitechsoftwares.com")
            {
                if (oClsResponse1.WhitelabelType == 0)
                {
                    return Redirect("/errorpage/domain");
                }
                else if (oClsResponse1.WhitelabelType == 2 || oClsResponse1.WhitelabelType == 1)
                {
                    return Redirect("/login");
                }
                else
                {
                    // Get CompanyId from Data
                    if (oClsResponse1.Data != null)
                    {
                        try
                        {
                            companyId = oClsResponse1.Data.CompanyId;
                        }
                        catch
                        {
                            companyId = 0;
                        }
                    }
                }
            }

            // Fetch blog details
            var blogRequest = new ClsBlogVm
            {
                CompanyId = companyId,
                UniqueSlug = title, // Use the slug from URL
                Title = null,
                IpAddress = Request.UserHostAddress,
                Browser = Request.Browser.Browser,
                Platform = "Web"
            };

            // Initialize API controller
            WebApi.BlogController blogController = new WebApi.BlogController();

            // Call API method directly
            var blogResult = await blogController.WebBlog(blogRequest);
            ClsResponse blogResponse = await oCommonController.ExtractResponseFromActionResult(blogResult);

            if (blogResponse.Status == 0 || blogResponse.Data == null || blogResponse.Data.Blog == null)
            {
                return RedirectToAction("Index", "Blog");
            }

            dynamic blog = blogResponse.Data.Blog;
            ViewBag.Blog = blog;
            
            // Handle Blogs and RelatedBlogs with proper type casting
            if (blogResponse.Data.Blogs != null)
            {
                ViewBag.Blogs = blogResponse.Data.Blogs;
            }
            else
            {
                ViewBag.Blogs = new List<ClsBlogVm>();
            }
            
            if (blogResponse.Data.RelatedBlogs != null)
            {
                ViewBag.RelatedBlogs = blogResponse.Data.RelatedBlogs;
            }
            else
            {
                ViewBag.RelatedBlogs = new List<ClsBlogVm>();
            }

            // Parse tags
            if (blog != null && blog.Taglist != null && !string.IsNullOrWhiteSpace(blog.Taglist.ToString()))
            {
                string tagList = blog.Taglist.ToString();
                string[] tags = tagList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> tagListResult = new List<string>();
                foreach (string tag in tags)
                {
                    string trimmedTag = tag.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTag))
                    {
                        tagListResult.Add(trimmedTag);
                    }
                }
                ViewBag.Tag = tagListResult.ToArray();
            }
            else
            {
                ViewBag.Tag = new string[0];
            }

            // SEO Meta Tags
            string blogTitle = blog.Title?.ToString() ?? "Blog Post";
            string blogDescription = blog.MetaDescription?.ToString() ?? 
                                   blog.ShortDescription?.ToString() ?? 
                                   "Read our latest blog post about GST billing and business management.";
            string blogImage = blog.Image?.ToString() ?? 
                             (Request.Url.Scheme + "://" + Request.Url.Host + "/Content/web-assets/images/logo.png");
            string blogUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/blog/" + title;

            ViewBag.PageTitle = blog.MetaTitle?.ToString() ?? blogTitle;
            ViewBag.MetaDescription = blogDescription;
            ViewBag.MetaKeywords = blog.MetaKeywords?.ToString() ?? "GST billing, inventory management, accounting software";
            ViewBag.CanonicalUrl = blogUrl;
            ViewBag.OgTitle = blogTitle;
            ViewBag.OgDescription = blogDescription;
            ViewBag.OgImage = blogImage;
            ViewBag.OgUrl = blogUrl;
            ViewBag.TwitterTitle = blogTitle;
            ViewBag.TwitterDescription = blogDescription;
            ViewBag.TwitterImage = blogImage;

            // Article Schema Data for ViewBag
            ViewBag.ArticleSchemaTitle = blogTitle;
            ViewBag.ArticleSchemaDescription = blogDescription;
            ViewBag.ArticleSchemaImage = blogImage;
            ViewBag.ArticleSchemaPublishedDate = blog.PublishedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? blog.AddedOn?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            ViewBag.ArticleSchemaModifiedDate = blog.PublishedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? blog.AddedOn?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            ViewBag.ArticleSchemaAuthor = "EquiBillBook";
            ViewBag.ArticleSchemaUrl = blogUrl;

            return View();
        }
    }
}