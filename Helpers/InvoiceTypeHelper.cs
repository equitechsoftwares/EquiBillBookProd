using System;
using System.Collections.Generic;
using System.Linq;
using EquiBillBook.Models;

namespace EquiBillBook.Helpers
{
    public static class InvoiceTypeHelper
    {
        public static List<InvoiceTypeConfig> GetAvailableInvoiceTypes(long companyId, ClsSaleSettingsVm saleSettings, ClsPurchaseSettingsVm purchaseSettings)
        {
            var invoiceTypes = new List<InvoiceTypeConfig>();
            
            // Customer (Sales) invoice types - exact sequence from screenshot
            // 1. Sales Quotation
            if (saleSettings?.EnableSalesQuotation == true)
            {
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "SalesQuotation",
                    DisplayName = "Sales Quotation",
                    Icon = "fas fa-file-alt",
                    IsEnabled = true,
                    SortOrder = 1,
                    Category = "Customer"
                });
            }
            
            // 2. Sales Order
            if (saleSettings?.EnableSalesOrder == true)
            {
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "SalesOrder",
                    DisplayName = "Sales Order",
                    Icon = "fas fa-file-contract",
                    IsEnabled = true,
                    SortOrder = 2,
                    Category = "Customer"
                });
            }
            
            // 3. Sales Proforma
            if (saleSettings?.EnableSalesProforma == true)
            {
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "SalesProforma",
                    DisplayName = "Sales Proforma",
                    Icon = "fas fa-file-invoice-dollar",
                    IsEnabled = true,
                    SortOrder = 3,
                    Category = "Customer"
                });
            }
            
            // 4. Delivery Challan
            if (saleSettings?.EnableDeliveryChallan == true)
            {
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "DeliveryChallan",
                    DisplayName = "Delivery Challan",
                    Icon = "fas fa-truck",
                    IsEnabled = true,
                    SortOrder = 4,
                    Category = "Customer"
                });
            }
            
            // 5. Sales Invoice
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "Sales",
                DisplayName = "Sales Invoice",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 5,
                Category = "Customer"
            });
            
            // 6. Debit Note
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "DebitNote",
                DisplayName = "Debit Note",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 6,
                Category = "Customer"
            });
            
            // 7. Bill Of Supply
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "BillOfSupply",
                DisplayName = "Bill Of Supply",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 7,
                Category = "Customer"
            });
            
            // 8. Credit Note
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "CreditNote",
                DisplayName = "Credit Note",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 8,
                Category = "Customer"
            });
            
            // 9. Payment Link
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "PaymentLink",
                DisplayName = "Payment Link",
                Icon = "fas fa-link",
                IsEnabled = true,
                SortOrder = 9,
                Category = "Customer"
            });
            
            // 10. Invoice Payment
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "InvoicePayment",
                DisplayName = "Invoice Payment",
                Icon = "fas fa-credit-card",
                IsEnabled = true,
                SortOrder = 10,
                Category = "Customer"
            });
            
            // Supplier (Purchase) invoice types - exact sequence from screenshot
            if (purchaseSettings != null)
            {
                // 1. Purchase Quotation
                if (purchaseSettings.EnablePurchaseQuotation == true)
                {
                    invoiceTypes.Add(new InvoiceTypeConfig
                    {
                        InvoiceTypeKey = "PurchaseQuotation",
                        DisplayName = "Purchase Quotation",
                        Icon = "fas fa-file-alt",
                        IsEnabled = true,
                        SortOrder = 101,
                        Category = "Supplier"
                    });
                }
                
                // 2. Purchase Order
                if (purchaseSettings.EnablePurchaseOrder == true)
                {
                    invoiceTypes.Add(new InvoiceTypeConfig
                    {
                        InvoiceTypeKey = "PurchaseOrder",
                        DisplayName = "Purchase Order",
                        Icon = "fas fa-file-contract",
                        IsEnabled = true,
                        SortOrder = 102,
                        Category = "Supplier"
                    });
                }
                
                // 3. Purchase Bill
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "Purchase",
                    DisplayName = "Purchase Bill",
                    Icon = "fas fa-file-invoice",
                    IsEnabled = true,
                    SortOrder = 103,
                    Category = "Supplier"
                });
                
                // 4. Debit Note (Purchase Debit Note)
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "PurchaseDebitNote",
                    DisplayName = "Debit Note",
                    Icon = "fas fa-file-invoice",
                    IsEnabled = true,
                    SortOrder = 104,
                    Category = "Supplier"
                });
                
                // 5. Bill Payment (Supplier Payment)
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "BillPayment",
                    DisplayName = "Bill Payment",
                    Icon = "fas fa-credit-card",
                    IsEnabled = true,
                    SortOrder = 105,
                    Category = "Supplier"
                });
            }
            
            // Also include POS and Sales Return if enabled
            if (saleSettings?.EnablePos == true)
            {
                invoiceTypes.Add(new InvoiceTypeConfig
                {
                    InvoiceTypeKey = "Pos",
                    DisplayName = "POS Invoice",
                    Icon = "fas fa-cash-register",
                    IsEnabled = true,
                    SortOrder = 15,
                    Category = "Customer"
                });
            }
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "SalesReturn",
                DisplayName = "Sales Return",
                Icon = "fas fa-undo",
                IsEnabled = true,
                SortOrder = 16,
                Category = "Customer"
            });
            
            return invoiceTypes.OrderBy(x => x.SortOrder).ToList();
        }
        
        public static Dictionary<string, List<InvoiceTypeConfig>> GetInvoiceTypesByCategory(long companyId, ClsSaleSettingsVm saleSettings, ClsPurchaseSettingsVm purchaseSettings)
        {
            var allTypes = GetAvailableInvoiceTypes(companyId, saleSettings, purchaseSettings);
            var grouped = new Dictionary<string, List<InvoiceTypeConfig>>();
            
            grouped["Customer"] = allTypes.Where(x => x.Category == "Customer").OrderBy(x => x.SortOrder).ToList();
            grouped["Supplier"] = allTypes.Where(x => x.Category == "Supplier").OrderBy(x => x.SortOrder).ToList();
            
            return grouped;
        }

        // Get all invoice types for admin use (returns all types regardless of settings)
        public static List<InvoiceTypeConfig> GetAllInvoiceTypes()
        {
            var invoiceTypes = new List<InvoiceTypeConfig>();
            
            // Customer (Sales) invoice types
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "SalesQuotation",
                DisplayName = "Sales Quotation",
                Icon = "fas fa-file-alt",
                IsEnabled = true,
                SortOrder = 1,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "SalesOrder",
                DisplayName = "Sales Order",
                Icon = "fas fa-file-contract",
                IsEnabled = true,
                SortOrder = 2,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "SalesProforma",
                DisplayName = "Sales Proforma",
                Icon = "fas fa-file-invoice-dollar",
                IsEnabled = true,
                SortOrder = 3,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "DeliveryChallan",
                DisplayName = "Delivery Challan",
                Icon = "fas fa-truck",
                IsEnabled = true,
                SortOrder = 4,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "Sales",
                DisplayName = "Sales Invoice",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 5,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "DebitNote",
                DisplayName = "Debit Note",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 6,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "BillOfSupply",
                DisplayName = "Bill Of Supply",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 7,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "CreditNote",
                DisplayName = "Credit Note",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 8,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "PaymentLink",
                DisplayName = "Payment Link",
                Icon = "fas fa-link",
                IsEnabled = true,
                SortOrder = 9,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "InvoicePayment",
                DisplayName = "Invoice Payment",
                Icon = "fas fa-credit-card",
                IsEnabled = true,
                SortOrder = 10,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "Pos",
                DisplayName = "POS Invoice",
                Icon = "fas fa-cash-register",
                IsEnabled = true,
                SortOrder = 15,
                Category = "Customer"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "SalesReturn",
                DisplayName = "Sales Return",
                Icon = "fas fa-undo",
                IsEnabled = true,
                SortOrder = 16,
                Category = "Customer"
            });
            
            // Supplier (Purchase) invoice types
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "PurchaseQuotation",
                DisplayName = "Purchase Quotation",
                Icon = "fas fa-file-alt",
                IsEnabled = true,
                SortOrder = 101,
                Category = "Supplier"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "PurchaseOrder",
                DisplayName = "Purchase Order",
                Icon = "fas fa-file-contract",
                IsEnabled = true,
                SortOrder = 102,
                Category = "Supplier"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "Purchase",
                DisplayName = "Purchase Bill",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 103,
                Category = "Supplier"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "PurchaseDebitNote",
                DisplayName = "Debit Note",
                Icon = "fas fa-file-invoice",
                IsEnabled = true,
                SortOrder = 104,
                Category = "Supplier"
            });
            
            invoiceTypes.Add(new InvoiceTypeConfig
            {
                InvoiceTypeKey = "BillPayment",
                DisplayName = "Bill Payment",
                Icon = "fas fa-credit-card",
                IsEnabled = true,
                SortOrder = 105,
                Category = "Supplier"
            });
            
            return invoiceTypes.OrderBy(x => x.SortOrder).ToList();
        }

        // Get default label visibility settings for each invoice type
        public static Dictionary<string, bool> GetDefaultLabelsForType(string invoiceType)
        {
            var labels = new Dictionary<string, bool>();
            
            switch (invoiceType.ToLower())
            {
                case "sales":
                    labels.Add("InvoiceDate", true);
                    labels.Add("InvoiceNumber", true);
                    labels.Add("DueDate", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("UnitPrice", true);
                    labels.Add("Discount", true);
                    labels.Add("Tax", true);
                    labels.Add("Total", true);
                    labels.Add("TotalQuantity", true);
                    labels.Add("GrossAmount", true);
                    labels.Add("TotalDiscount", true);
                    labels.Add("SpecialDiscount", true);
                    labels.Add("NetAmount", true);
                    labels.Add("RoundOff", true);
                    labels.Add("GrandTotal", true);
                    labels.Add("DueAmount", true);
                    labels.Add("PaymentInformation", true);
                    labels.Add("PointsEarned", true);
                    labels.Add("PointsRedeemed", true);
                    break;
                    
                case "salesquotation":
                    labels.Add("QuotationDate", true);
                    labels.Add("QuotationNumber", true);
                    labels.Add("ValidUntil", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("UnitPrice", true);
                    labels.Add("Discount", true);
                    labels.Add("Tax", true);
                    labels.Add("Total", true);
                    break;
                    
                case "salesorder":
                    labels.Add("OrderDate", true);
                    labels.Add("OrderNumber", true);
                    labels.Add("ExpectedDeliveryDate", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("UnitPrice", true);
                    labels.Add("Discount", true);
                    labels.Add("Tax", true);
                    labels.Add("Total", true);
                    break;
                    
                case "deliverychallan":
                    labels.Add("ChallanDate", true);
                    labels.Add("ChallanNumber", true);
                    labels.Add("ChallanType", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("DeliveredTo", true);
                    labels.Add("ShippingStatus", true);
                    break;
                    
                case "purchase":
                    labels.Add("BillDate", true);
                    labels.Add("ReferenceNumber", true);
                    labels.Add("DueDate", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("UnitPrice", true);
                    labels.Add("Discount", true);
                    labels.Add("Tax", true);
                    labels.Add("Total", true);
                    break;
                    
                default:
                    // Generic labels for other types
                    labels.Add("Date", true);
                    labels.Add("Number", true);
                    labels.Add("Status", true);
                    labels.Add("SKU", true);
                    labels.Add("Quantity", true);
                    labels.Add("UnitPrice", true);
                    labels.Add("Discount", true);
                    labels.Add("Tax", true);
                    labels.Add("Total", true);
                    break;
            }
            
            return labels;
        }

        // Get section-organized labels for each invoice type
        // This method organizes labels by section (Header, Company, Customer, Items, Summary, Footer)
        public static ClsSectionLabelConfig GetSectionLabelsForType(string invoiceType)
        {
            var config = new ClsSectionLabelConfig();
            var type = invoiceType.ToLower();

            switch (type)
            {
                case "sales":
                    // Header labels
                    config.HeaderLabels.Add("InvoiceDate", true);
                    config.HeaderLabels.Add("InvoiceNumber", true);
                    config.HeaderLabels.Add("DueDate", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    // Company labels
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    config.CompanyLabels.Add("CompanyPhone", true);
                    config.CompanyLabels.Add("CompanyEmail", true);
                    config.CompanyLabels.Add("CompanyGST", true);
                    config.CompanyLabels.Add("CompanyPAN", false);
                    
                    // Customer labels
                    config.CustomerLabels.Add("CustomerName", true);
                    config.CustomerLabels.Add("BillTo", true);
                    config.CustomerLabels.Add("ShippingAddress", false);
                    config.CustomerLabels.Add("CustomerGST", true);
                    config.CustomerLabels.Add("CustomerPAN", false);
                    
                    // Items labels
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("HSN", false);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Unit", true);
                    config.ItemsLabels.Add("Rate", true);
                    config.ItemsLabels.Add("Discount", true);
                    config.ItemsLabels.Add("Tax", true);
                    config.ItemsLabels.Add("Total", true);
                    
                    // Summary labels
                    config.SummaryLabels.Add("TotalQuantity", true);
                    config.SummaryLabels.Add("GrossAmount", true);
                    config.SummaryLabels.Add("TotalDiscount", true);
                    config.SummaryLabels.Add("SpecialDiscount", false);
                    config.SummaryLabels.Add("TaxBreakdown", true);
                    config.SummaryLabels.Add("NetAmount", true);
                    config.SummaryLabels.Add("RoundOff", true);
                    config.SummaryLabels.Add("GrandTotal", true);
                    config.SummaryLabels.Add("DueAmount", true);
                    config.SummaryLabels.Add("PaidAmount", false);
                    config.SummaryLabels.Add("BalanceAmount", false);
                    
                    // Footer labels
                    config.FooterLabels.Add("FooterNote", true);
                    config.FooterLabels.Add("TermsAndConditions", false);
                    config.FooterLabels.Add("PaymentInformation", true);
                    config.FooterLabels.Add("BankDetails", false);
                    config.FooterLabels.Add("PointsEarned", false);
                    config.FooterLabels.Add("PointsRedeemed", false);
                    break;

                case "salesquotation":
                    // Header labels
                    config.HeaderLabels.Add("QuotationDate", true);
                    config.HeaderLabels.Add("QuotationNumber", true);
                    config.HeaderLabels.Add("ValidUntil", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    // Company labels
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    config.CompanyLabels.Add("CompanyPhone", true);
                    config.CompanyLabels.Add("CompanyEmail", true);
                    config.CompanyLabels.Add("CompanyGST", true);
                    
                    // Customer labels
                    config.CustomerLabels.Add("CustomerName", true);
                    config.CustomerLabels.Add("BillTo", true);
                    config.CustomerLabels.Add("CustomerGST", true);
                    
                    // Items labels
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Unit", true);
                    config.ItemsLabels.Add("Rate", true);
                    config.ItemsLabels.Add("Discount", true);
                    config.ItemsLabels.Add("Tax", true);
                    config.ItemsLabels.Add("Total", true);
                    
                    // Summary labels
                    config.SummaryLabels.Add("TotalQuantity", true);
                    config.SummaryLabels.Add("GrossAmount", true);
                    config.SummaryLabels.Add("TotalDiscount", true);
                    config.SummaryLabels.Add("TaxBreakdown", true);
                    config.SummaryLabels.Add("NetAmount", true);
                    config.SummaryLabels.Add("RoundOff", true);
                    config.SummaryLabels.Add("GrandTotal", true);
                    
                    // Footer labels
                    config.FooterLabels.Add("FooterNote", true);
                    config.FooterLabels.Add("TermsAndConditions", true);
                    config.FooterLabels.Add("ValidityPeriod", true);
                    break;

                case "salesorder":
                    // Header labels
                    config.HeaderLabels.Add("OrderDate", true);
                    config.HeaderLabels.Add("OrderNumber", true);
                    config.HeaderLabels.Add("ExpectedDeliveryDate", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    // Company labels
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    config.CompanyLabels.Add("CompanyPhone", true);
                    config.CompanyLabels.Add("CompanyEmail", true);
                    config.CompanyLabels.Add("CompanyGST", true);
                    
                    // Customer labels
                    config.CustomerLabels.Add("CustomerName", true);
                    config.CustomerLabels.Add("BillTo", true);
                    config.CustomerLabels.Add("ShippingAddress", true);
                    config.CustomerLabels.Add("CustomerGST", true);
                    
                    // Items labels
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Unit", true);
                    config.ItemsLabels.Add("Rate", true);
                    config.ItemsLabels.Add("Discount", true);
                    config.ItemsLabels.Add("Tax", true);
                    config.ItemsLabels.Add("Total", true);
                    
                    // Summary labels
                    config.SummaryLabels.Add("TotalQuantity", true);
                    config.SummaryLabels.Add("GrossAmount", true);
                    config.SummaryLabels.Add("TotalDiscount", true);
                    config.SummaryLabels.Add("TaxBreakdown", true);
                    config.SummaryLabels.Add("NetAmount", true);
                    config.SummaryLabels.Add("RoundOff", true);
                    config.SummaryLabels.Add("GrandTotal", true);
                    
                    // Footer labels
                    config.FooterLabels.Add("FooterNote", true);
                    config.FooterLabels.Add("TermsAndConditions", false);
                    break;

                case "deliverychallan":
                    // Header labels
                    config.HeaderLabels.Add("ChallanDate", true);
                    config.HeaderLabels.Add("ChallanNumber", true);
                    config.HeaderLabels.Add("ChallanType", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    // Company labels
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    config.CompanyLabels.Add("CompanyPhone", true);
                    
                    // Customer labels
                    config.CustomerLabels.Add("CustomerName", true);
                    config.CustomerLabels.Add("DeliveredTo", true);
                    config.CustomerLabels.Add("ShippingAddress", true);
                    
                    // Items labels
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Unit", true);
                    config.ItemsLabels.Add("ShippingStatus", true);
                    
                    // Summary labels
                    config.SummaryLabels.Add("TotalQuantity", true);
                    
                    // Footer labels
                    config.FooterLabels.Add("FooterNote", true);
                    break;

                case "purchase":
                    // Header labels
                    config.HeaderLabels.Add("BillDate", true);
                    config.HeaderLabels.Add("ReferenceNumber", true);
                    config.HeaderLabels.Add("DueDate", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    // Company labels (Supplier in this case)
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    config.CompanyLabels.Add("CompanyPhone", true);
                    config.CompanyLabels.Add("CompanyEmail", true);
                    config.CompanyLabels.Add("CompanyGST", true);
                    
                    // Customer labels (Supplier details)
                    config.CustomerLabels.Add("SupplierName", true);
                    config.CustomerLabels.Add("SupplierAddress", true);
                    config.CustomerLabels.Add("SupplierGST", true);
                    
                    // Items labels
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Unit", true);
                    config.ItemsLabels.Add("Rate", true);
                    config.ItemsLabels.Add("Discount", true);
                    config.ItemsLabels.Add("Tax", true);
                    config.ItemsLabels.Add("Total", true);
                    
                    // Summary labels
                    config.SummaryLabels.Add("TotalQuantity", true);
                    config.SummaryLabels.Add("GrossAmount", true);
                    config.SummaryLabels.Add("TotalDiscount", true);
                    config.SummaryLabels.Add("TaxBreakdown", true);
                    config.SummaryLabels.Add("NetAmount", true);
                    config.SummaryLabels.Add("RoundOff", true);
                    config.SummaryLabels.Add("GrandTotal", true);
                    config.SummaryLabels.Add("DueAmount", true);
                    
                    // Footer labels
                    config.FooterLabels.Add("FooterNote", true);
                    config.FooterLabels.Add("TermsAndConditions", false);
                    break;

                default:
                    // Generic labels for other types
                    config.HeaderLabels.Add("Date", true);
                    config.HeaderLabels.Add("Number", true);
                    config.HeaderLabels.Add("Status", true);
                    
                    config.CompanyLabels.Add("CompanyName", true);
                    config.CompanyLabels.Add("CompanyAddress", true);
                    
                    config.CustomerLabels.Add("CustomerName", true);
                    
                    config.ItemsLabels.Add("ItemName", true);
                    config.ItemsLabels.Add("SKU", true);
                    config.ItemsLabels.Add("Quantity", true);
                    config.ItemsLabels.Add("Rate", true);
                    config.ItemsLabels.Add("Total", true);
                    
                    config.SummaryLabels.Add("TotalQuantity", true);
                    config.SummaryLabels.Add("GrandTotal", true);
                    
                    config.FooterLabels.Add("FooterNote", true);
                    break;
            }

            return config;
        }

        // Get default section configuration for an invoice type
        public static ClsEnhancedInvoiceTemplateConfig GetDefaultSectionConfigForType(string invoiceType)
        {
            var config = new ClsEnhancedInvoiceTemplateConfig();
            var sectionLabels = GetSectionLabelsForType(invoiceType);

            // Set section labels
            config.Header.LabelVisibility = sectionLabels.HeaderLabels;
            config.Company.LabelVisibility = sectionLabels.CompanyLabels;
            config.Customer.LabelVisibility = sectionLabels.CustomerLabels;
            config.Items.LabelVisibility = sectionLabels.ItemsLabels;
            config.Summary.LabelVisibility = sectionLabels.SummaryLabels;
            config.Footer.LabelVisibility = sectionLabels.FooterLabels;

            // Set invoice type specific defaults
            var type = invoiceType.ToLower();
            switch (type)
            {
                case "salesquotation":
                    config.Footer.ShowValidityPeriod = true;
                    config.Footer.ShowTermsAndConditions = true;
                    break;
                case "deliverychallan":
                    config.Summary.ShowSummarySection = false; // Minimal summary
                    config.Items.ShowItemRate = false;
                    config.Items.ShowItemDiscount = false;
                    config.Items.ShowItemTax = false;
                    break;
            }

            return config;
        }
    }
}

