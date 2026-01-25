using System;
using System.Web;

namespace EquiBillBook.Handlers
{
    public class SitemapHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            // Transfer request to the Sitemap controller route
            // This bypasses static file handling and routes through MVC
            context.Server.TransferRequest("/Sitemap/Index", true);
        }
    }
}

