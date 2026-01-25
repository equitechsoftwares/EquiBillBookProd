using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EquiBillBook
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Allow routes to handle requests even if they match static files (needed for sitemap.xml)
            routes.RouteExistingFiles = true;
            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Sitemap Route - must come before default route to handle /sitemap.xml
            routes.MapRoute(
                name: "Sitemap",
                url: "sitemap.xml",
                defaults: new { controller = "Sitemap", action = "Index" }
            );

            // Public Catalogue Route - must come before default route
            routes.MapRoute(
                name: "PublicCatalogue",
                url: "c/{slug}",
                defaults: new { controller = "PublicCatalogue", action = "View" }
            );

            // Public Booking Route - must come before default route
            routes.MapRoute(
                name: "PublicBooking",
                url: "book/{slug}",
                defaults: new { controller = "PublicBooking", action = "BookTable" }
            );

            // Public Table Booking Route - must come before default route
            routes.MapRoute(
                name: "PublicTableBooking",
                url: "booktable/{slug}",
                defaults: new { controller = "PublicBooking", action = "BookTableBySlug" }
            );

            // Blog Index Route - explicit route for /blog to go to BlogController.Index
            routes.MapRoute(
                name: "BlogIndex",
                url: "blog",
                defaults: new { controller = "Blog", action = "Index" }
            );

            // Blog Post Route - must come after BlogIndex but before default route
            // This route handles /blog/{slug} URLs and routes them to BlogdetailsController
            routes.MapRoute(
                name: "BlogPost",
                url: "blog/{title}",
                defaults: new { controller = "Blogdetails", action = "Index" },
                constraints: new { title = @".+" } // Ensure title is not empty
            );

            // Knowledge Base Routes
            routes.MapRoute(
                name: "KnowledgeBaseIndex",
                url: "knowledgebase",
                defaults: new { controller = "KnowledgeBase", action = "Index" }
            );

            routes.MapRoute(
                name: "KnowledgeBaseArticle",
                url: "knowledgebase/article/{slug}",
                defaults: new { controller = "KnowledgeBase", action = "Article" },
                constraints: new { slug = @".+" }
            );

            routes.MapRoute(
                name: "KnowledgeBaseCategory",
                url: "knowledgebase/category/{category}",
                defaults: new { controller = "KnowledgeBase", action = "Category" },
                constraints: new { category = @".+" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
