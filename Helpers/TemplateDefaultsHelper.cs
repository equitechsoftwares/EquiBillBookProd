using System;
using System.Collections.Generic;
using System.Linq;
using EquiBillBook.Models;
using System.Data.Entity;

namespace EquiBillBook.Helpers
{
    public static class TemplateDefaultsHelper
    {
        /// <summary>
        /// Loads all template defaults from master tables for a specific predefined template
        /// Only loads defaults for the specified InvoiceTemplateMasterId (no fallback)
        /// </summary>
        public static ClsInvoiceTemplateDefaultsMasterVm LoadTemplateDefaults(DbContext dbContext, long invoiceTemplateMasterId)
        {
            var defaults = new ClsInvoiceTemplateDefaultsMasterVm
            {
                InvoiceTemplateMasterId = invoiceTemplateMasterId
            };

            try
            {
                // Load Colors - only for specified InvoiceTemplateMasterId
                var colors = dbContext.Set<ClsInvoiceTemplateColorMaster>()
                    .Where(c => c.IsActive && c.InvoiceTemplateMasterId == invoiceTemplateMasterId)
                    .ToList();
                
                // Load categories separately
                var colorIds = colors.Select(c => c.InvoiceTemplateColorCategoryMasterId).Where(id => id.HasValue).Distinct().ToList();
                var categories = colorIds.Any() 
                    ? dbContext.Set<ClsInvoiceTemplateColorCategoryMaster>()
                        .Where(cat => colorIds.Contains(cat.InvoiceTemplateColorCategoryMasterId)
                            && cat.InvoiceTemplateMasterId == invoiceTemplateMasterId)
                        .ToDictionary(cat => cat.InvoiceTemplateColorCategoryMasterId, cat => cat)
                    : new Dictionary<long, ClsInvoiceTemplateColorCategoryMaster>();

                foreach (var color in colors)
                {
                    // Maintain backward compatibility with flat Colors dictionary
                    defaults.Colors[color.ColorKey] = color.DefaultValue;

                    // Group colors by category - use normalized category or fallback to old Category field or "General"
                    var category = "General";
                    int categorySortOrder = int.MaxValue; // Default for uncategorized/General
                    if (color.InvoiceTemplateColorCategoryMasterId.HasValue && categories.ContainsKey(color.InvoiceTemplateColorCategoryMasterId.Value))
                    {
                        var cat = categories[color.InvoiceTemplateColorCategoryMasterId.Value];
                        if (!string.IsNullOrEmpty(cat.CategoryName))
                        {
                            category = cat.CategoryName;
                            categorySortOrder = cat.SortOrder;
                        }
                    }
                    else if (!string.IsNullOrEmpty(color.Category))
                    {
                        category = color.Category; // Fallback to old Category field for backward compatibility
                    }

                    if (!defaults.ColorsByCategory.ContainsKey(category))
                    {
                        defaults.ColorsByCategory[category] = new Dictionary<string, ColorInfo>();
                        defaults.CategorySortOrders[category] = categorySortOrder;
                    }

                    defaults.ColorsByCategory[category][color.ColorKey] = new ColorInfo
                    {
                        ColorKey = color.ColorKey,
                        ColorName = color.ColorName,
                        DefaultValue = color.DefaultValue,
                        Description = color.Description,
                        SortOrder = color.SortOrder
                    };
                }

                // Sort colors within each category by SortOrder
                foreach (var categoryKey in defaults.ColorsByCategory.Keys.ToList())
                {
                    var sortedColors = defaults.ColorsByCategory[categoryKey]
                        .OrderBy(kvp => kvp.Value.SortOrder)
                        .ThenBy(kvp => kvp.Key)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    defaults.ColorsByCategory[categoryKey] = sortedColors;
                }

                // Load Label Colors - from tblInvoiceTemplateLabelsMaster joined with label categories (to get CategoryKey)
                var labelColors = (from l in dbContext.Set<ClsInvoiceTemplateLabelMaster>()
                                   join c in dbContext.Set<ClsInvoiceTemplateLabelCategoryMaster>()
                                       on l.InvoiceTemplateLabelCategoryMasterId equals c.InvoiceTemplateLabelCategoryMasterId
                                   where l.IsActive
                                         && c.IsActive
                                         && l.InvoiceTemplateMasterId == invoiceTemplateMasterId
                                         && !string.IsNullOrEmpty(l.LabelColor)
                                   orderby c.CategoryKey, l.SortOrder
                                   select new
                                   {
                                       c.CategoryKey,
                                       l.LabelKey,
                                       l.LabelColor
                                   }).ToList();

                foreach (var label in labelColors)
                {
                    if (!defaults.LabelColors.ContainsKey(label.CategoryKey))
                    {
                        defaults.LabelColors[label.CategoryKey] = new Dictionary<string, string>();
                    }
                    defaults.LabelColors[label.CategoryKey][label.LabelKey] = label.LabelColor;
                }
            }
            catch (Exception ex)
            {
                // Log error and return empty defaults (no hardcoded fallbacks)
                System.Diagnostics.Debug.WriteLine($"Error loading template defaults: {ex.Message}");
                // Return empty defaults - no fallback values
            }

            return defaults;
        }

        /// <summary>
        /// Gets default color value by key for a specific predefined template
        /// Only returns value for the specified InvoiceTemplateMasterId (no fallback)
        /// </summary>
        public static string GetDefaultColor(DbContext dbContext, string colorKey, long invoiceTemplateMasterId, string fallback = "#3b82f6")
        {
            try
            {
                var color = dbContext.Set<ClsInvoiceTemplateColorMaster>()
                    .FirstOrDefault(c => c.ColorKey == colorKey && c.IsActive && c.InvoiceTemplateMasterId == invoiceTemplateMasterId);

                return color?.DefaultValue ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Gets default label color for a specific predefined template
        /// Only returns value for the specified InvoiceTemplateMasterId (no fallback)
        /// Label colors are stored in tblInvoiceTemplateLabelsMaster
        /// </summary>
        public static string GetDefaultLabelColor(DbContext dbContext, string categoryKey, string labelKey, long invoiceTemplateMasterId, string fallback = "#1f2937")
        {
            try
            {
                var label = (from l in dbContext.Set<ClsInvoiceTemplateLabelMaster>()
                             join c in dbContext.Set<ClsInvoiceTemplateLabelCategoryMaster>()
                                 on l.InvoiceTemplateLabelCategoryMasterId equals c.InvoiceTemplateLabelCategoryMasterId
                             where l.IsActive
                                   && c.IsActive
                                   && l.InvoiceTemplateMasterId == invoiceTemplateMasterId
                                   && c.CategoryKey == categoryKey
                                   && l.LabelKey == labelKey
                                   && !string.IsNullOrEmpty(l.LabelColor)
                             select l.LabelColor).FirstOrDefault();

                return string.IsNullOrEmpty(label) ? fallback : label;
            }
            catch
            {
                return fallback;
            }
        }

    }
}

