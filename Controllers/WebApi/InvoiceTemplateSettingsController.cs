using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;
using EquiBillBook.Helpers;
using System.IO;
using System.Web.Hosting;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class InvoiceTemplateSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        dynamic data = null;

        [HttpPost]
        public async Task<IHttpActionResult> GetTemplates(ClsInvoiceTemplateVm obj)
        {
            try
            {
                // Get sale settings to determine available invoice types
                var saleSettings = oConnectionContext.DbClsSaleSettings
                    .FirstOrDefault(a => a.CompanyId == obj.CompanyId && a.IsActive && !a.IsDeleted);
                
                var purchaseSettings = oConnectionContext.DbClsPurchaseSettings
                    .FirstOrDefault(a => a.CompanyId == obj.CompanyId && a.IsActive && !a.IsDeleted);

                ClsSaleSettingsVm saleSettingsVm = null;
                ClsPurchaseSettingsVm purchaseSettingsVm = null;

                if (saleSettings != null)
                {
                    saleSettingsVm = new ClsSaleSettingsVm
                    {
                        EnableSalesQuotation = saleSettings.EnableSalesQuotation,
                        EnableSalesOrder = saleSettings.EnableSalesOrder,
                        EnableSalesProforma = saleSettings.EnableSalesProforma,
                        EnableDeliveryChallan = saleSettings.EnableDeliveryChallan,
                        EnablePos = saleSettings.EnablePos
                    };
                }

                if (purchaseSettings != null)
                {
                    purchaseSettingsVm = new ClsPurchaseSettingsVm
                    {
                        EnablePurchaseQuotation = purchaseSettings.EnablePurchaseQuotation,
                        EnablePurchaseOrder = purchaseSettings.EnablePurchaseOrder
                    };
                }

                var availableInvoiceTypes = InvoiceTypeHelper.GetAvailableInvoiceTypes(
                    obj.CompanyId,
                    saleSettingsVm,
                    purchaseSettingsVm
                );

                var availableTypeKeys = availableInvoiceTypes.Select(x => x.InvoiceTypeKey).ToList();

                // Get templates only for enabled invoice types (flat list, one row per template)
                var dbTemplates = oConnectionContext.DbClsInvoiceTemplates
                    .Where(a => a.CompanyId == obj.CompanyId
                             && a.IsActive
                             && !a.IsDeleted
                             && availableTypeKeys.Contains(a.InvoiceType))
                    .ToList();

                var templateVms = dbTemplates
                    .Select(t => new ClsInvoiceTemplateVm
                    {
                        InvoiceTemplateId = t.InvoiceTemplateId,
                        CompanyId = t.CompanyId,
                        InvoiceType = t.InvoiceType,
                        TemplateName = t.TemplateName,
                        TemplateKey = t.TemplateKey,
                        Description = t.Description,
                        IsDefault = t.IsDefault,
                        IsActive = t.IsActive
                    })
                    // Default templates first, then order remaining by newest (highest Id first)
                    .OrderByDescending(t => t.InvoiceTemplateId)
                    .ToList();

                var dataObj = new ClsData
                {
                    Templates = templateVms,
                    AvailableInvoiceTypes = availableInvoiceTypes
                };

                data = new ClsResponse
                {
                    Status = 1,
                    Message = "Templates found",
                    Data = dataObj
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetTemplate(ClsInvoiceTemplateVm obj)
        {
            try
            {
                var template = oConnectionContext.DbClsInvoiceTemplates
                    .FirstOrDefault(a => a.InvoiceTemplateId == obj.InvoiceTemplateId 
                                      && a.CompanyId == obj.CompanyId 
                                      && a.IsActive 
                                      && !a.IsDeleted);

                if (template == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Template not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                InvoiceTemplateConfig config = null;
                if (!string.IsNullOrEmpty(template.TemplateConfig))
                {
                    try
                    {
                        config = serializer.Deserialize<InvoiceTemplateConfig>(template.TemplateConfig);
                    }
                    catch { }
                }

                if (config == null)
                {
                    // No hardcoded defaults - return empty config
                    // Defaults should be loaded from master tables via GetTemplateDefaults endpoint
                    config = new InvoiceTemplateConfig();
                    config.LabelVisibility = InvoiceTypeHelper.GetDefaultLabelsForType(template.InvoiceType);
                    config.PrintSettings = new PrintOptions();
                }

                var templateVm = new ClsInvoiceTemplateVm
                {
                    InvoiceTemplateId = template.InvoiceTemplateId,
                    CompanyId = template.CompanyId,
                    InvoiceType = template.InvoiceType,
                    TemplateName = template.TemplateName,
                    TemplateKey = template.TemplateKey,
                    Description = template.Description,
                    TemplateConfig = config,
                    IsDefault = template.IsDefault,
                    IsActive = template.IsActive
                };

                data = new
                {
                    Status = 1,
                    Message = "Template found",
                    Data = new
                    {
                        Template = templateVm
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> SaveTemplate(ClsInvoiceTemplateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                try
                {
                    DateTime CurrentDate = DateTime.UtcNow;
                    var configJson = serializer.Serialize(obj.TemplateConfig);

                    // Validate template name is not empty
                    if (string.IsNullOrWhiteSpace(obj.TemplateName))
                    {
                        data = new { Status = 0, Message = "Template name is required", Data = new { } };
                        return await Task.FromResult(Ok(data));
                    }

                    // Check for duplicate template name (case-insensitive) within same CompanyId and InvoiceType
                    var duplicateTemplate = oConnectionContext.DbClsInvoiceTemplates
                        .FirstOrDefault(a => a.CompanyId == obj.CompanyId
                                          && a.InvoiceType == obj.InvoiceType
                                          && a.TemplateName.ToLower().Trim() == obj.TemplateName.ToLower().Trim()
                                          && a.IsActive
                                          && !a.IsDeleted
                                          && (obj.InvoiceTemplateId <= 0 || a.InvoiceTemplateId != obj.InvoiceTemplateId));

                    if (duplicateTemplate != null)
                    {
                        data = new { Status = 0, Message = "Duplicate Template Name exists", Data = new { } };
                        return await Task.FromResult(Ok(data));
                    }

                    if (obj.InvoiceTemplateId > 0)
                    {
                        // Update existing template
                        var existing = oConnectionContext.DbClsInvoiceTemplates
                            .FirstOrDefault(a => a.InvoiceTemplateId == obj.InvoiceTemplateId 
                                              && a.CompanyId == obj.CompanyId);

                        if (existing == null)
                        {
                            data = new { Status = 0, Message = "Template not found", Data = new { } };
                            return await Task.FromResult(Ok(data));
                        }

                        existing.TemplateName = obj.TemplateName;
                        existing.Description = obj.Description;
                        existing.TemplateConfig = configJson;
                        existing.ModifiedBy = obj.AddedBy;
                        existing.ModifiedOn = CurrentDate;

                        if (obj.IsDefault)
                        {
                            // Unset other defaults for same InvoiceType and CompanyId
                            string query = $@"update ""tblInvoiceTemplates"" 
                                             set ""IsDefault""=False 
                                             where ""CompanyId""={obj.CompanyId} 
                                             and ""InvoiceType""='{obj.InvoiceType}' 
                                             and ""InvoiceTemplateId""!={obj.InvoiceTemplateId}";
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                            existing.IsDefault = true;
                        }

                        oConnectionContext.SaveChanges();

                        // Activity Log
                        oCommonController.InsertActivityLog(new ClsActivityLogVm
                        {
                            AddedBy = obj.AddedBy,
                            Browser = obj.Browser,
                            Category = "Settings",
                            CompanyId = obj.CompanyId,
                            Description = $"Invoice Template \"{obj.TemplateName}\" for {obj.InvoiceType} updated",
                            Id = existing.InvoiceTemplateId,
                            IpAddress = obj.IpAddress,
                            Platform = obj.Platform,
                            Type = "Update"
                        }, CurrentDate);

                        data = new
                        {
                            Status = 1,
                            Message = "Template updated successfully",
                            Data = new { InvoiceTemplateId = existing.InvoiceTemplateId }
                        };
                    }
                    else
                    {
                        // Create new template
                        var newTemplate = new ClsInvoiceTemplate
                        {
                            CompanyId = obj.CompanyId,
                            InvoiceType = obj.InvoiceType,
                            TemplateName = obj.TemplateName,
                            TemplateKey = obj.TemplateKey ?? "custom",
                            Description = obj.Description,
                            TemplateConfig = configJson,
                            IsDefault = obj.IsDefault,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate
                        };

                        oConnectionContext.DbClsInvoiceTemplates.Add(newTemplate);
                        oConnectionContext.SaveChanges();

                        if (obj.IsDefault)
                        {
                            // Unset other defaults
                            string query = $@"update ""tblInvoiceTemplates"" 
                                             set ""IsDefault""=False 
                                             where ""CompanyId""={obj.CompanyId} 
                                             and ""InvoiceType""='{obj.InvoiceType}' 
                                             and ""InvoiceTemplateId""!={newTemplate.InvoiceTemplateId}";
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                            newTemplate.IsDefault = true;
                            oConnectionContext.SaveChanges();
                        }

                        // Activity Log
                        oCommonController.InsertActivityLog(new ClsActivityLogVm
                        {
                            AddedBy = obj.AddedBy,
                            Browser = obj.Browser,
                            Category = "Settings",
                            CompanyId = obj.CompanyId,
                            Description = $"Invoice Template \"{obj.TemplateName}\" for {obj.InvoiceType} created",
                            Id = newTemplate.InvoiceTemplateId,
                            IpAddress = obj.IpAddress,
                            Platform = obj.Platform,
                            Type = "Insert"
                        }, CurrentDate);

                        data = new
                        {
                            Status = 1,
                            Message = "Template saved successfully",
                            Data = new { InvoiceTemplateId = newTemplate.InvoiceTemplateId }
                        };
                    }

                    dbContextTransaction.Complete();
                    return await Task.FromResult(Ok(data));
                }
                catch (Exception ex)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Error: " + ex.Message,
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> SetDefault(ClsInvoiceTemplateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                try
                {
                    var template = oConnectionContext.DbClsInvoiceTemplates
                        .FirstOrDefault(a => a.InvoiceTemplateId == obj.InvoiceTemplateId 
                                          && a.CompanyId == obj.CompanyId 
                                          && a.IsActive 
                                          && !a.IsDeleted);

                    if (template == null)
                    {
                        data = new { Status = 0, Message = "Template not found", Data = new { } };
                        return await Task.FromResult(Ok(data));
                    }

                    // Unset other defaults for same InvoiceType
                    string query = $@"update ""tblInvoiceTemplates"" 
                                     set ""IsDefault""=False 
                                     where ""CompanyId""={obj.CompanyId} 
                                     and ""InvoiceType""='{template.InvoiceType}'";
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    // Set this template as default
                    template.IsDefault = true;
                    template.ModifiedBy = obj.AddedBy;
                    template.ModifiedOn = DateTime.UtcNow;
                    oConnectionContext.SaveChanges();

                    dbContextTransaction.Complete();

                    data = new
                    {
                        Status = 1,
                        Message = "Template set as default successfully",
                        Data = new { }
                    };

                    return await Task.FromResult(Ok(data));
                }
                catch (Exception ex)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Error: " + ex.Message,
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetPreDefinedTemplates(ClsInvoiceTemplatesMasterVm obj)
        {
            try
            {
                var query = oConnectionContext.DbClsInvoiceTemplatesMaster
                    .Where(t => t.IsActive && !t.IsDeleted && t.InvoiceType == obj.InvoiceType);
                
                var templates = query
                    .OrderBy(t => t.SortOrder)
                    .ThenBy(t => t.TemplateName)
                    .ToList();
                
                var templatesVm = new List<ClsInvoiceTemplatesMasterVm>();
                
                foreach (var template in templates)
                {
                    var templateVm = new ClsInvoiceTemplatesMasterVm
                    {
                        InvoiceTemplateMasterId = template.InvoiceTemplateMasterId,
                        InvoiceType = template.InvoiceType,
                        TemplateKey = template.TemplateKey,
                        TemplateName = template.TemplateName,
                        Description = template.Description,
                        PreviewColor = template.PreviewColor,
                        Icon = template.Icon,
                        PreviewImageUrl = template.PreviewImageUrl,
                        TemplateHtmlPath = template.TemplateHtmlPath,
                        SortOrder = template.SortOrder
                    };
                    
                    // Load HTML content if path exists
                    if (!string.IsNullOrEmpty(template.TemplateHtmlPath))
                    {
                        try
                        {
                            var filePath = HostingEnvironment.MapPath("~" + template.TemplateHtmlPath);
                            if (File.Exists(filePath))
                            {
                                templateVm.TemplateHtml = File.ReadAllText(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue
                            System.Diagnostics.Debug.WriteLine($"Error loading template HTML: {ex.Message}");
                        }
                    }
                    
                    templatesVm.Add(templateVm);
                }
                
                data = new
                {
                    Status = 1,
                    Message = "Templates loaded successfully",
                    Data = new { Templates = templatesVm }
                };
                
                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error loading templates: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeleteTemplate(ClsInvoiceTemplateVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                try
                {
                    var template = oConnectionContext.DbClsInvoiceTemplates
                        .FirstOrDefault(a => a.InvoiceTemplateId == obj.InvoiceTemplateId 
                                          && a.CompanyId == obj.CompanyId 
                                          && a.IsActive 
                                          && !a.IsDeleted);

                    if (template == null)
                    {
                        data = new { Status = 0, Message = "Template not found", Data = new { } };
                        return await Task.FromResult(Ok(data));
                    }

                    // Ensure at least one template remains for this company and invoice type
                    var otherActiveTemplatesCount = oConnectionContext.DbClsInvoiceTemplates
                        .Count(a => a.CompanyId == template.CompanyId
                                    && a.InvoiceType == template.InvoiceType
                                    && a.IsActive
                                    && !a.IsDeleted
                                    && a.InvoiceTemplateId != template.InvoiceTemplateId);

                    if (otherActiveTemplatesCount == 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "You must have at least one template for this invoice type. The last template cannot be deleted.",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    // Soft delete
                    template.IsDeleted = true;
                    template.IsActive = false;
                    template.ModifiedBy = obj.AddedBy;
                    template.ModifiedOn = DateTime.UtcNow;
                    oConnectionContext.SaveChanges();

                    dbContextTransaction.Complete();

                    data = new
                    {
                        Status = 1,
                        Message = "Template deleted successfully",
                        Data = new { }
                    };

                    return await Task.FromResult(Ok(data));
                }
                catch (Exception ex)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Error: " + ex.Message,
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetTemplateDefaults(ClsInvoiceTemplateVm obj)
        {
            try
            {
                long invoiceTemplateMasterId = 0;

                // Case 1: If InvoiceTemplateId is provided (user template), get the master template ID from TemplateKey
                if (obj != null && obj.InvoiceTemplateId > 0)
                {
                    var userTemplate = oConnectionContext.DbClsInvoiceTemplates
                        .FirstOrDefault(t => t.InvoiceTemplateId == obj.InvoiceTemplateId 
                                          && t.CompanyId == obj.CompanyId 
                                          && t.IsActive 
                                          && !t.IsDeleted);

                    if (userTemplate != null && !string.IsNullOrEmpty(userTemplate.TemplateKey) && !string.IsNullOrEmpty(userTemplate.InvoiceType))
                    {
                        var masterTemplate = oConnectionContext.DbClsInvoiceTemplatesMaster
                            .FirstOrDefault(m => m.TemplateKey == userTemplate.TemplateKey 
                                              && m.InvoiceType == userTemplate.InvoiceType 
                                              && m.IsActive 
                                              && !m.IsDeleted);

                        if (masterTemplate != null)
                        {
                            invoiceTemplateMasterId = masterTemplate.InvoiceTemplateMasterId;
                        }
                    }
                }
                // Case 2: If TemplateKey and InvoiceType are provided directly (pre-defined template)
                else if (obj != null && !string.IsNullOrEmpty(obj.TemplateKey) && !string.IsNullOrEmpty(obj.InvoiceType))
                {
                    var masterTemplate = oConnectionContext.DbClsInvoiceTemplatesMaster
                        .FirstOrDefault(m => m.TemplateKey == obj.TemplateKey 
                                          && m.InvoiceType == obj.InvoiceType 
                                          && m.IsActive 
                                          && !m.IsDeleted);

                    if (masterTemplate != null)
                    {
                        invoiceTemplateMasterId = masterTemplate.InvoiceTemplateMasterId;
                    }
                }

                if (invoiceTemplateMasterId <= 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Template master not found. Please provide InvoiceTemplateId or TemplateKey + InvoiceType.",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var defaults = TemplateDefaultsHelper.LoadTemplateDefaults(oConnectionContext, invoiceTemplateMasterId);

                data = new
                {
                    Status = 1,
                    Message = "Template defaults loaded successfully",
                    Data = new
                    {
                        Defaults = defaults,
                        InvoiceTemplateMasterId = invoiceTemplateMasterId
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        /// <summary>
        /// Gets labels for a specific template from tblInvoiceTemplateLabelsMaster
        /// This moves label definitions (keys + default visibility) to the database.
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GetTemplateLabels(ClsInvoiceTemplateVm obj)
        {
            try
            {
                long invoiceTemplateMasterId = 0;

                // Case 1: If InvoiceTemplateId is provided (user template), get the master template ID from TemplateKey
                if (obj != null && obj.InvoiceTemplateId > 0)
                {
                    var userTemplate = oConnectionContext.DbClsInvoiceTemplates
                        .FirstOrDefault(t => t.InvoiceTemplateId == obj.InvoiceTemplateId 
                                          && t.CompanyId == obj.CompanyId 
                                          && t.IsActive 
                                          && !t.IsDeleted);

                    if (userTemplate != null && !string.IsNullOrEmpty(userTemplate.TemplateKey) && !string.IsNullOrEmpty(userTemplate.InvoiceType))
                    {
                        var masterTemplate = oConnectionContext.DbClsInvoiceTemplatesMaster
                            .FirstOrDefault(m => m.TemplateKey == userTemplate.TemplateKey 
                                              && m.InvoiceType == userTemplate.InvoiceType 
                                              && m.IsActive 
                                              && !m.IsDeleted);

                        if (masterTemplate != null)
                        {
                            invoiceTemplateMasterId = masterTemplate.InvoiceTemplateMasterId;
                        }
                    }
                }
                // Case 2: If TemplateKey and InvoiceType are provided directly (pre-defined template)
                else if (obj != null && !string.IsNullOrEmpty(obj.TemplateKey) && !string.IsNullOrEmpty(obj.InvoiceType))
                {
                    var masterTemplate = oConnectionContext.DbClsInvoiceTemplatesMaster
                        .FirstOrDefault(m => m.TemplateKey == obj.TemplateKey 
                                          && m.InvoiceType == obj.InvoiceType 
                                          && m.IsActive 
                                          && !m.IsDeleted);

                    if (masterTemplate != null)
                    {
                        invoiceTemplateMasterId = masterTemplate.InvoiceTemplateMasterId;
                    }
                }

                if (invoiceTemplateMasterId <= 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Template master not found. Please provide InvoiceTemplateId or TemplateKey + InvoiceType.",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Join labels with label categories to get CategoryKey/CategoryName
                var labelData = (from l in oConnectionContext.DbClsInvoiceTemplateLabelMasters
                                 join c in oConnectionContext.DbClsInvoiceTemplateLabelCategoryMasters
                                     on l.InvoiceTemplateLabelCategoryMasterId equals c.InvoiceTemplateLabelCategoryMasterId
                                 where l.InvoiceTemplateMasterId == invoiceTemplateMasterId
                                       && l.IsActive
                                       && !l.IsDeleted
                                       && c.IsActive
                                       && !c.IsDeleted
                                 orderby c.CategoryKey, l.SortOrder
                                 select new
                                 {
                                     l.InvoiceTemplateLabelMasterId,
                                     l.InvoiceTemplateMasterId,
                                     c.CategoryKey,
                                     c.CategoryName,
                                     l.LabelKey,
                                     l.LabelText,
                                     l.LabelColor,
                                     l.IsVisibleByDefault,
                                     l.SortOrder,
                                     l.IsActive,
                                     l.InvoiceTemplateLabelCategoryMasterId
                                 }).ToList();

                var labelVms = labelData.Select(l => new ClsInvoiceTemplateLabelMasterVm
                {
                    InvoiceTemplateLabelMasterId = l.InvoiceTemplateLabelMasterId,
                    InvoiceTemplateMasterId = l.InvoiceTemplateMasterId,
                    InvoiceTemplateLabelCategoryMasterId = l.InvoiceTemplateLabelCategoryMasterId,
                    CategoryKey = l.CategoryKey,
                    CategoryName = l.CategoryName,
                    LabelKey = l.LabelKey,
                    LabelText = l.LabelText,
                    LabelColor = l.LabelColor,
                    IsVisibleByDefault = l.IsVisibleByDefault,
                    SortOrder = l.SortOrder,
                    IsActive = l.IsActive
                }).ToList();

                data = new
                {
                    Status = 1,
                    Message = "Template labels loaded successfully",
                    Data = new
                    {
                        Labels = labelVms,
                        InvoiceTemplateMasterId = invoiceTemplateMasterId
                    }
                };

                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }
    }
}

