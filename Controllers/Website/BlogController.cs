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
    public class BlogController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        
        // GET: Blog
        public async Task<ActionResult> Index(ClsDomainVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");

            // Initialize API controller
            DomainController domainController = new DomainController();

            // Call API method directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            long companyId = 0;
            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                // Fetch blogs for main domain
                companyId = 0;
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
                else
                {
                    // Fetch blogs for whitelabel domain
                    companyId = oClsResponse1.Data?.CompanyId ?? 0;
                }
            }

            // Load initial blog page and categories
            await LoadBlogs(companyId);
            await LoadBlogCategories(companyId);
            ViewBag.CompanyId = companyId;

            // SEO Meta Tags
            ViewBag.PageTitle = "Blog - EquiBillBook";
            ViewBag.MetaDescription = "Read our latest blog posts about GST billing, inventory management, accounting software, and business tips for small businesses in India.";
            ViewBag.MetaKeywords = "GST billing blog, inventory management tips, accounting software blog, small business tips, billing software India";
            ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/blog";
            ViewBag.OgTitle = "Blog - EquiBillBook";
            ViewBag.OgDescription = ViewBag.MetaDescription;
            ViewBag.OgImage = Request.Url.Scheme + "://" + Request.Url.Host + "/Content/web-assets/images/logo.png";
            ViewBag.TwitterTitle = ViewBag.OgTitle;
            ViewBag.TwitterDescription = ViewBag.MetaDescription;
            ViewBag.TwitterImage = ViewBag.OgImage;

            return View("~/Views/Website/Blog/Index.cshtml");
        }

        private async Task LoadBlogs(long companyId)
        {
            try
            {
                var blogRequest = new ClsBlogVm
                {
                    CompanyId = companyId,
                    PageIndex = 1,
                    PageSize = 9 // Initial page for infinite scroll
                };

                // Initialize API controller
                WebApi.BlogController blogController = new WebApi.BlogController();

                // Call API method directly
                var blogResult = await blogController.WebBlogs(blogRequest);
                ClsResponse blogResponse = await oCommonController.ExtractResponseFromActionResult(blogResult);

                if (blogResponse.Status == 1 && blogResponse.Data != null)
                {
                    ViewBag.Blogs = blogResponse.Data.Blogs;
                    ViewBag.BlogTotalCount = blogResponse.Data.TotalCount;
                }
                else
                {
                    ViewBag.Blogs = new List<object>();
                    ViewBag.BlogTotalCount = 0;
                }
            }
            catch (Exception)
            {
                ViewBag.Blogs = new List<object>();
            }
        }

        private async Task LoadBlogCategories(long companyId)
        {
            try
            {
                var categoryRequest = new ClsBlogVm
                {
                    CompanyId = companyId
                };

                // Initialize API controller
                WebApi.BlogController blogController = new WebApi.BlogController();

                // Call API method directly
                var categoryResult = await blogController.WebBlogCategories(categoryRequest);
                ClsResponse categoryResponse = await oCommonController.ExtractResponseFromActionResult(categoryResult);

                if (categoryResponse.Status == 1 && categoryResponse.Data != null)
                {
                    ViewBag.BlogCategories = categoryResponse.Data.BlogCategories;
                }
                else
                {
                    ViewBag.BlogCategories = new List<object>();
                }
            }
            catch (Exception)
            {
                ViewBag.BlogCategories = new List<object>();
            }
        }
    }
}