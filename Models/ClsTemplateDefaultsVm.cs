using System;
using System.Collections.Generic;

namespace EquiBillBook.Models
{
    /// <summary>
    /// ViewModel for template defaults loaded from master tables
    /// Used to return all default settings (colors, label colors) for a template
    /// </summary>
    public class ClsInvoiceTemplateDefaultsMasterVm
    {
        public Dictionary<string, string> Colors { get; set; }
        public Dictionary<string, Dictionary<string, ColorInfo>> ColorsByCategory { get; set; } // Colors grouped by category
        public Dictionary<string, int> CategorySortOrders { get; set; } // Category name to SortOrder mapping
        public Dictionary<string, Dictionary<string, string>> LabelColors { get; set; } // Label colors from labels
        public long InvoiceTemplateMasterId { get; set; }

        public ClsInvoiceTemplateDefaultsMasterVm()
        {
            Colors = new Dictionary<string, string>();
            ColorsByCategory = new Dictionary<string, Dictionary<string, ColorInfo>>();
            CategorySortOrders = new Dictionary<string, int>();
            LabelColors = new Dictionary<string, Dictionary<string, string>>();
        }
    }

    /// <summary>
    /// Color information including name and description
    /// </summary>
    public class ColorInfo
    {
        public string ColorKey { get; set; }
        public string ColorName { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }
}

