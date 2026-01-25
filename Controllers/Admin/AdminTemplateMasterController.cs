using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminTemplateMasterController : Controller
    {
        private readonly ConnectionContext _db = new ConnectionContext();

        // GET: /AdminTemplateMaster
        public async Task<ActionResult> Index()
        {
            // Load predefined templates for the Template Masters page (both active and inactive, but not deleted)
            var templates = await _db.DbClsInvoiceTemplatesMaster
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.InvoiceType)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();

            // Get all distinct invoice types from templates
            var distinctInvoiceTypes = templates
                .Select(t => t.InvoiceType)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Get all available invoice types using helper (for admin, get all types regardless of settings)
            var allInvoiceTypes = Helpers.InvoiceTypeHelper.GetAllInvoiceTypes();
            
            ViewBag.DistinctInvoiceTypes = distinctInvoiceTypes;
            ViewBag.AllInvoiceTypes = allInvoiceTypes;

            return View(templates);
        }

        // ========== INVOICE TEMPLATES ==========

        // GET: /AdminTemplateMaster/InvoiceTemplates
        public async Task<ActionResult> InvoiceTemplates()
        {
            var templates = await _db.DbClsInvoiceTemplates
                .OrderBy(t => t.InvoiceType)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();

            return View(templates);
        }

        // GET: /AdminTemplateMaster/EditInvoiceTemplate/5
        public async Task<ActionResult> EditInvoiceTemplate(long id)
        {
            var template = await _db.DbClsInvoiceTemplates
                .FirstOrDefaultAsync(t => t.InvoiceTemplateId == id);

            if (template == null)
            {
                return HttpNotFound();
            }

            return View(template);
        }

        // POST: /AdminTemplateMaster/EditInvoiceTemplate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditInvoiceTemplate(ClsInvoiceTemplate model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplates
                .FirstOrDefaultAsync(t => t.InvoiceTemplateId == model.InvoiceTemplateId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.InvoiceType = model.InvoiceType;
            existing.TemplateName = model.TemplateName;
            existing.TemplateKey = model.TemplateKey;
            existing.Description = model.Description;
            existing.IsDefault = model.IsDefault;
            existing.IsActive = model.IsActive;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Invoice template updated successfully.";
            return RedirectToAction("InvoiceTemplates");
        }

        // GET: /AdminTemplateMaster/CreateInvoiceTemplate
        public ActionResult CreateInvoiceTemplate()
        {
            var model = new ClsInvoiceTemplate
            {
                IsActive = true,
                AddedOn = DateTime.UtcNow
            };
            return View(model);
        }

        // POST: /AdminTemplateMaster/CreateInvoiceTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateInvoiceTemplate(ClsInvoiceTemplate model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplates.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Invoice template created successfully.";
            return RedirectToAction("InvoiceTemplates");
        }

        // POST: /AdminTemplateMaster/DeleteInvoiceTemplate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteInvoiceTemplate(long id)
        {
            var existing = await _db.DbClsInvoiceTemplates
                .FirstOrDefaultAsync(t => t.InvoiceTemplateId == id);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Invoice template deleted (soft delete) successfully.";
            return RedirectToAction("InvoiceTemplates");
        }

        // ========== PRE-DEFINED TEMPLATES ==========

        public async Task<ActionResult> EditPreDefinedTemplate(long id)
        {
            var template = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == id && !t.IsDeleted);

            if (template == null)
            {
                return HttpNotFound();
            }

            // Get all available invoice types for dropdown
            var allInvoiceTypes = Helpers.InvoiceTypeHelper.GetAllInvoiceTypes();
            ViewBag.InvoiceTypes = new SelectList(
                allInvoiceTypes.Select(t => new { Value = t.InvoiceTypeKey, Text = t.DisplayName }),
                "Value",
                "Text",
                template.InvoiceType
            );

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPreDefinedTemplate(ClsInvoiceTemplatesMaster model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate ViewBag for dropdown on validation error
                var allInvoiceTypes = Helpers.InvoiceTypeHelper.GetAllInvoiceTypes();
                ViewBag.InvoiceTypes = new SelectList(
                    allInvoiceTypes.Select(t => new { Value = t.InvoiceTypeKey, Text = t.DisplayName }),
                    "Value",
                    "Text",
                    model.InvoiceType
                );
                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.InvoiceType = model.InvoiceType;
            existing.TemplateKey = model.TemplateKey;
            existing.TemplateName = model.TemplateName;
            existing.Description = model.Description;
            existing.PreviewColor = model.PreviewColor;
            existing.Icon = model.Icon;
            existing.PreviewImageUrl = model.PreviewImageUrl;
            existing.TemplateHtmlPath = model.TemplateHtmlPath;
            existing.TemplateConfig = model.TemplateConfig;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.RequiredAddons = model.RequiredAddons;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pre-defined template updated successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult CreatePreDefinedTemplate()
        {
            var model = new ClsInvoiceTemplatesMaster
            {
                IsActive = true,
                IsDeleted = false,
                AddedOn = DateTime.UtcNow
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePreDefinedTemplate(ClsInvoiceTemplatesMaster model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplatesMaster.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pre-defined template created successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePreDefinedTemplate(long id)
        {
            var existing = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == id && !t.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pre-defined template deleted successfully.";
            return RedirectToAction("Index");
        }

        // ========== DEFAULT COLORS (PER TEMPLATE) ==========

        public async Task<ActionResult> Colors(long invoiceTemplateMasterId)
        {
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";
            ViewBag.IsPreDefined = true;

            var colors = await _db.DbClsInvoiceTemplateColorMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.ColorKey)
                .ToListAsync();

            // Load categories separately and create a lookup dictionary
            var categoryIds = colors.Where(c => c.InvoiceTemplateColorCategoryMasterId.HasValue)
                .Select(c => c.InvoiceTemplateColorCategoryMasterId.Value)
                .Distinct()
                .ToList();

            var categories = categoryIds.Any()
                ? await _db.DbClsInvoiceTemplateColorCategoryMasters
                    .Where(cat => categoryIds.Contains(cat.InvoiceTemplateColorCategoryMasterId) 
                        && cat.InvoiceTemplateMasterId == invoiceTemplateMasterId)
                    .ToDictionaryAsync(cat => cat.InvoiceTemplateColorCategoryMasterId, cat => cat)
                : new Dictionary<long, ClsInvoiceTemplateColorCategoryMaster>();

            ViewBag.Categories = categories;

            return View(colors);
        }

        public async Task<ActionResult> CreateColor(long invoiceTemplateMasterId)
        {
            var model = new ClsInvoiceTemplateColorMaster
            {
                InvoiceTemplateMasterId = invoiceTemplateMasterId,
                IsActive = true,
                AddedOn = DateTime.UtcNow
            };

            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";

            // Load categories for dropdown
            var categories = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .Where(c => c.IsActive && !c.IsDeleted && c.InvoiceTemplateMasterId == invoiceTemplateMasterId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "InvoiceTemplateColorCategoryMasterId", "CategoryName");

            // Load existing colors grouped by category for reference
            var existingColors = await _db.DbClsInvoiceTemplateColorMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.ColorKey)
                .ToListAsync();
            
            // Load categories for existing colors
            var existingColorCategoryIds = existingColors.Where(c => c.InvoiceTemplateColorCategoryMasterId.HasValue)
                .Select(c => c.InvoiceTemplateColorCategoryMasterId.Value)
                .Distinct()
                .ToList();
            
            var existingColorsCategories = existingColorCategoryIds.Any()
                ? await _db.DbClsInvoiceTemplateColorCategoryMasters
                    .Where(cat => existingColorCategoryIds.Contains(cat.InvoiceTemplateColorCategoryMasterId)
                        && cat.InvoiceTemplateMasterId == invoiceTemplateMasterId)
                    .ToDictionaryAsync(cat => cat.InvoiceTemplateColorCategoryMasterId, cat => cat)
                : new Dictionary<long, ClsInvoiceTemplateColorCategoryMaster>();
            
            ViewBag.ExistingColors = existingColors;
            ViewBag.ExistingColorsCategories = existingColorsCategories;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateColor(ClsInvoiceTemplateColorMaster model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ViewBag data for dropdown and existing colors
                var categories = await _db.DbClsInvoiceTemplateColorCategoryMasters
                    .Where(c => c.IsActive && !c.IsDeleted && c.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();
                ViewBag.Categories = new SelectList(categories, "InvoiceTemplateColorCategoryMasterId", "CategoryName");

                var existingColors = await _db.DbClsInvoiceTemplateColorMasters
                    .Where(c => c.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.ColorKey)
                    .ToListAsync();
                
                var existingColorCategoryIds = existingColors.Where(c => c.InvoiceTemplateColorCategoryMasterId.HasValue)
                    .Select(c => c.InvoiceTemplateColorCategoryMasterId.Value)
                    .Distinct()
                    .ToList();
                
                var existingColorsCategories = existingColorCategoryIds.Any()
                    ? await _db.DbClsInvoiceTemplateColorCategoryMasters
                        .Where(cat => existingColorCategoryIds.Contains(cat.InvoiceTemplateColorCategoryMasterId)
                            && cat.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId)
                        .ToDictionaryAsync(cat => cat.InvoiceTemplateColorCategoryMasterId, cat => cat)
                    : new Dictionary<long, ClsInvoiceTemplateColorCategoryMaster>();
                
                ViewBag.ExistingColors = existingColors;
                ViewBag.ExistingColorsCategories = existingColorsCategories;

                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
                var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);
                ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{model.InvoiceTemplateMasterId}";

                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplateColorMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color default created successfully.";
            return RedirectToAction("Colors", new { invoiceTemplateMasterId = model.InvoiceTemplateMasterId });
        }

        public async Task<ActionResult> EditColor(long id)
        {
            var color = await _db.DbClsInvoiceTemplateColorMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorMasterId == id);

            if (color == null)
            {
                return HttpNotFound();
            }

            // Load categories for dropdown
            var categories = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .Where(c => c.IsActive && !c.IsDeleted && c.InvoiceTemplateMasterId == color.InvoiceTemplateMasterId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "InvoiceTemplateColorCategoryMasterId", "CategoryName", color.InvoiceTemplateColorCategoryMasterId);

            ViewBag.InvoiceTemplateMasterId = color.InvoiceTemplateMasterId;

            return View(color);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditColor(ClsInvoiceTemplateColorMaster model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ViewBag data for dropdown
                var categories = await _db.DbClsInvoiceTemplateColorCategoryMasters
                    .Where(c => c.IsActive && !c.IsDeleted && c.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();
                ViewBag.Categories = new SelectList(categories, "InvoiceTemplateColorCategoryMasterId", "CategoryName", model.InvoiceTemplateColorCategoryMasterId);

                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;

                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplateColorMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorMasterId == model.InvoiceTemplateColorMasterId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.ColorKey = model.ColorKey;
            existing.ColorName = model.ColorName;
            existing.DefaultValue = model.DefaultValue;
            existing.Description = model.Description;
            existing.InvoiceTemplateColorCategoryMasterId = model.InvoiceTemplateColorCategoryMasterId;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color default updated successfully.";
            return RedirectToAction("Colors", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteColor(long id)
        {
            var existing = await _db.DbClsInvoiceTemplateColorMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorMasterId == id && !c.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color default deleted successfully.";
            return RedirectToAction("Colors", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        // ========== COLOR CATEGORIES (PER TEMPLATE) ==========

        public async Task<ActionResult> Categories(long invoiceTemplateMasterId)
        {
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";
            ViewBag.IsPreDefined = true;

            var categories = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        public async Task<ActionResult> CreateCategory(long invoiceTemplateMasterId)
        {
            var model = new ClsInvoiceTemplateColorCategoryMaster
            {
                InvoiceTemplateMasterId = invoiceTemplateMasterId,
                IsActive = true,
                AddedOn = DateTime.UtcNow
            };

            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategory(ClsInvoiceTemplateColorCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
                var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);
                ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{model.InvoiceTemplateMasterId}";

                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplateColorCategoryMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color category created successfully.";
            return RedirectToAction("Categories", new { invoiceTemplateMasterId = model.InvoiceTemplateMasterId });
        }

        public async Task<ActionResult> EditCategory(long id)
        {
            var category = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorCategoryMasterId == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.InvoiceTemplateMasterId = category.InvoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == category.InvoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{category.InvoiceTemplateMasterId}";

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategory(ClsInvoiceTemplateColorCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
                var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);
                ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{model.InvoiceTemplateMasterId}";

                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorCategoryMasterId == model.InvoiceTemplateColorCategoryMasterId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
            existing.CategoryKey = model.CategoryKey;
            existing.CategoryName = model.CategoryName;
            existing.Description = model.Description;
            existing.Icon = model.Icon;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color category updated successfully.";
            return RedirectToAction("Categories", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCategory(long id)
        {
            var existing = await _db.DbClsInvoiceTemplateColorCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateColorCategoryMasterId == id && !c.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            // Check if any colors are using this category
            var colorsUsingCategory = await _db.DbClsInvoiceTemplateColorMasters
                .AnyAsync(c => c.InvoiceTemplateColorCategoryMasterId == id && !c.IsDeleted);

            if (colorsUsingCategory)
            {
                TempData["ErrorMessage"] = "Cannot delete category. There are colors assigned to this category.";
                return RedirectToAction("Categories", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Color category deleted successfully.";
            return RedirectToAction("Categories", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> CategoryActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplateColorCategoryMasters
                    .FirstOrDefaultAsync(c => c.InvoiceTemplateColorCategoryMasterId == id);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Category not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Category {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }


        // ========== LABEL CATEGORIES (PER TEMPLATE) ==========

        public async Task<ActionResult> LabelCategories(long invoiceTemplateMasterId)
        {
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";
            ViewBag.IsPreDefined = true;

            var categories = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        public async Task<ActionResult> CreateLabelCategory(long invoiceTemplateMasterId)
        {
            var model = new ClsInvoiceTemplateLabelCategoryMaster
            {
                InvoiceTemplateMasterId = invoiceTemplateMasterId,
                IsActive = true,
                AddedOn = DateTime.UtcNow
            };
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateLabelCategory(ClsInvoiceTemplateLabelCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
                var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);
                ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{model.InvoiceTemplateMasterId}";
                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplateLabelCategoryMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label category created successfully.";
            return RedirectToAction("LabelCategories", new { invoiceTemplateMasterId = model.InvoiceTemplateMasterId });
        }

        public async Task<ActionResult> EditLabelCategory(long id)
        {
            var category = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateLabelCategoryMasterId == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.InvoiceTemplateMasterId = category.InvoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == category.InvoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{category.InvoiceTemplateMasterId}";

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLabelCategory(ClsInvoiceTemplateLabelCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;
                var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && !t.IsDeleted);
                ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{model.InvoiceTemplateMasterId}";
                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateLabelCategoryMasterId == model.InvoiceTemplateLabelCategoryMasterId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.CategoryKey = model.CategoryKey;
            existing.CategoryName = model.CategoryName;
            existing.Description = model.Description;
            existing.Icon = model.Icon;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label category updated successfully.";
            return RedirectToAction("LabelCategories", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteLabelCategory(long id)
        {
            var existing = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .FirstOrDefaultAsync(c => c.InvoiceTemplateLabelCategoryMasterId == id && !c.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label category deleted successfully.";
            return RedirectToAction("LabelCategories", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        // ========== LABELS (PER TEMPLATE) ==========

        public async Task<ActionResult> Labels(long invoiceTemplateMasterId)
        {
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var preDefinedTemplate = await _db.DbClsInvoiceTemplatesMaster
                .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == invoiceTemplateMasterId && !t.IsDeleted);
            ViewBag.TemplateName = preDefinedTemplate != null ? preDefinedTemplate.TemplateName : $"Pre-defined Template #{invoiceTemplateMasterId}";
            ViewBag.IsPreDefined = true;

            // Label categories for dropdowns
            var categories = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = categories;

            // Load labels with category information
            var labelsQuery = from l in _db.DbClsInvoiceTemplateLabelMasters
                              join c in _db.DbClsInvoiceTemplateLabelCategoryMasters
                                  on l.InvoiceTemplateLabelCategoryMasterId equals c.InvoiceTemplateLabelCategoryMasterId
                              where l.InvoiceTemplateMasterId == invoiceTemplateMasterId
                                    && !l.IsDeleted
                                    && !c.IsDeleted
                              orderby c.CategoryKey, l.SortOrder, l.LabelKey
                              select new ClsInvoiceTemplateLabelMasterVm
                              {
                                  InvoiceTemplateLabelMasterId = l.InvoiceTemplateLabelMasterId,
                                  InvoiceTemplateMasterId = l.InvoiceTemplateMasterId,
                                  InvoiceTemplateLabelCategoryMasterId = l.InvoiceTemplateLabelCategoryMasterId,
                                  CategoryKey = c.CategoryKey,
                                  CategoryName = c.CategoryName,
                                  LabelKey = l.LabelKey,
                                  LabelText = l.LabelText,
                                  LabelColor = l.LabelColor,
                                  IsVisibleByDefault = l.IsVisibleByDefault,
                                  SortOrder = l.SortOrder,
                                  IsActive = l.IsActive
                              };

            var labels = await labelsQuery.ToListAsync();

            return View(labels);
        }

        public async Task<ActionResult> CreateLabel(long invoiceTemplateMasterId)
        {
            ViewBag.InvoiceTemplateMasterId = invoiceTemplateMasterId;
            var categories = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .Where(c => c.InvoiceTemplateMasterId == invoiceTemplateMasterId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = new System.Web.Mvc.SelectList(categories, "InvoiceTemplateLabelCategoryMasterId", "CategoryName");

            var model = new ClsInvoiceTemplateLabelMaster
            {
                InvoiceTemplateMasterId = invoiceTemplateMasterId,
                IsActive = true,
                IsVisibleByDefault = true,
                AddedOn = DateTime.UtcNow
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateLabel(ClsInvoiceTemplateLabelMaster model)
        {
            if (!ModelState.IsValid)
            {
                var categoriesReload = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                    .Where(c => c.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();
                ViewBag.Categories = new System.Web.Mvc.SelectList(categoriesReload, "InvoiceTemplateLabelCategoryMasterId", "CategoryName");
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;

                return View(model);
            }

            model.AddedOn = DateTime.UtcNow;
            _db.DbClsInvoiceTemplateLabelMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label created successfully.";
            return RedirectToAction("Labels", new { invoiceTemplateMasterId = model.InvoiceTemplateMasterId });
        }

        public async Task<ActionResult> EditLabel(long id)
        {
            var label = await _db.DbClsInvoiceTemplateLabelMasters
                .FirstOrDefaultAsync(l => l.InvoiceTemplateLabelMasterId == id);

            if (label == null)
            {
                return HttpNotFound();
            }

            ViewBag.InvoiceTemplateMasterId = label.InvoiceTemplateMasterId;
            var categories = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                .Where(c => c.InvoiceTemplateMasterId == label.InvoiceTemplateMasterId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = new System.Web.Mvc.SelectList(categories, "InvoiceTemplateLabelCategoryMasterId", "CategoryName", label.InvoiceTemplateLabelCategoryMasterId);

            return View(label);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLabel(ClsInvoiceTemplateLabelMaster model)
        {
            if (!ModelState.IsValid)
            {
                var categoriesReload = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                    .Where(c => c.InvoiceTemplateMasterId == model.InvoiceTemplateMasterId && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();
                ViewBag.Categories = new System.Web.Mvc.SelectList(categoriesReload, "InvoiceTemplateLabelCategoryMasterId", "CategoryName", model.InvoiceTemplateLabelCategoryMasterId);
                ViewBag.InvoiceTemplateMasterId = model.InvoiceTemplateMasterId;

                return View(model);
            }

            var existing = await _db.DbClsInvoiceTemplateLabelMasters
                .FirstOrDefaultAsync(l => l.InvoiceTemplateLabelMasterId == model.InvoiceTemplateLabelMasterId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.InvoiceTemplateLabelCategoryMasterId = model.InvoiceTemplateLabelCategoryMasterId;
            existing.LabelKey = model.LabelKey;
            existing.LabelText = model.LabelText;
            existing.LabelColor = model.LabelColor;
            existing.IsVisibleByDefault = model.IsVisibleByDefault;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label updated successfully.";
            return RedirectToAction("Labels", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteLabel(long id)
        {
            var existing = await _db.DbClsInvoiceTemplateLabelMasters
                .FirstOrDefaultAsync(l => l.InvoiceTemplateLabelMasterId == id && !l.IsDeleted);

            if (existing == null)
            {
                return HttpNotFound();
            }

            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Label deleted successfully.";
            return RedirectToAction("Labels", new { invoiceTemplateMasterId = existing.InvoiceTemplateMasterId });
        }

        // ========== ACTIVE/INACTIVE ENDPOINTS ==========

        [HttpPost]
        public async Task<ActionResult> PreDefinedTemplateActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplatesMaster
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateMasterId == id && !t.IsDeleted);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Template not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Template {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> InvoiceTemplateActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplates
                    .FirstOrDefaultAsync(t => t.InvoiceTemplateId == id);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Template not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Template {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ColorActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplateColorMasters
                    .FirstOrDefaultAsync(c => c.InvoiceTemplateColorMasterId == id);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Color not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Color {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> LabelCategoryActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplateLabelCategoryMasters
                    .FirstOrDefaultAsync(c => c.InvoiceTemplateLabelCategoryMasterId == id);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Label category not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Label category {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> LabelActiveInactive(long id, bool isActive)
        {
            try
            {
                var existing = await _db.DbClsInvoiceTemplateLabelMasters
                    .FirstOrDefaultAsync(l => l.InvoiceTemplateLabelMasterId == id);

                if (existing == null)
                {
                    return Json(new ClsResponse
                    {
                        Status = 0,
                        Message = "Label not found."
                    });
                }

                existing.IsActive = isActive;
                existing.ModifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new ClsResponse
                {
                    Status = 1,
                    Message = $"Label {(isActive ? "activated" : "deactivated")} successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new ClsResponse
                {
                    Status = 0,
                    Message = $"Error: {ex.Message}"
                });
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


