using EquiBillBook.Controllers.Website;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{    
    public class KnowledgeBaseController : ApiController
    {
        dynamic data = null;

        // Public endpoint - Get paginated knowledge base articles
        [HttpPost]
        public async Task<IHttpActionResult> GetArticles(ClsKnowledgeBaseArticleVm obj)
        {
            try
            {
                // Handle null object
                if (obj == null)
                {
                    obj = new ClsKnowledgeBaseArticleVm();
                }

                var allArticles = await KnowledgeBaseArticles.GetAllArticlesAsync();
                
                // Apply category filter if provided
                if (!string.IsNullOrWhiteSpace(obj.Category))
                {
                    allArticles = allArticles.Where(a => 
                        a.Category != null && a.Category.Equals(obj.Category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(obj.Search))
                {
                    string searchLower = obj.Search.ToLower();
                    allArticles = allArticles.Where(a =>
                        (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                        (a.Description != null && a.Description.ToLower().Contains(searchLower)) ||
                        (a.Category != null && a.Category.ToLower().Contains(searchLower)) ||
                        (a.Tags != null && a.Tags.Any(t => t != null && t.ToLower().Contains(searchLower)))
                    ).ToList();
                }

                var totalCount = allArticles.Count;

                // Apply pagination
                int pageSize = obj.PageSize > 0 ? obj.PageSize : 12; // Default 12 articles per page
                int pageIndex = obj.PageIndex > 0 ? obj.PageIndex : 1;
                int skip = pageSize * (pageIndex - 1);

                var paginatedArticles = allArticles
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Slug,
                        a.Title,
                        a.Category,
                        a.Description,
                        a.Tags,
                        LastUpdated = a.LastUpdated.ToString("MMM dd, yyyy")
                    })
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Articles = paginatedArticles,
                        TotalCount = totalCount,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        HasMore = (skip + pageSize) < totalCount
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error loading articles: " + ex.Message,
                    Data = new
                    {
                        Articles = new List<object>(),
                        TotalCount = 0,
                        PageIndex = 1,
                        PageSize = 12,
                        HasMore = false
                    }
                };

                return await Task.FromResult(Ok(data));
            }
        }
    }

    // View model for knowledge base article requests
    public class ClsKnowledgeBaseArticleVm
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Category { get; set; }
        public string Search { get; set; }
    }
}
