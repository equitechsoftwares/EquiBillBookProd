using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using EquiBillBook.Helpers;

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class InvoiceTemplateSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: InvoiceTemplateSettings
        public async Task<ActionResult> Index()
        {
            ClsHeaderVm obj = new ClsHeaderVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.HeaderType = "settings";

            // Load menu permissions
            WebApi.HeaderController headerController = new WebApi.HeaderController();
            var headerPermissionsResult = await headerController.HeaderPermissions(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(headerPermissionsResult);

            // Load SaleSettings to check enabled invoice types
            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingsResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponseSaleSettings = await oCommonController.ExtractResponseFromActionResult(saleSettingsResult);

            // Load PurchaseSettings if available
            ClsResponse oClsResponsePurchaseSettings = null;
            try
            {
                WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
                ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
                var purchaseSettingsResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
                oClsResponsePurchaseSettings = await oCommonController.ExtractResponseFromActionResult(purchaseSettingsResult);
            }
            catch { /* Purchase addon might not be enabled */ }

            // Get dynamically available invoice types
            var saleSettings = oClsResponseSaleSettings.Data?.SaleSetting != null ? 
                serializer.Deserialize<ClsSaleSettingsVm>(serializer.Serialize(oClsResponseSaleSettings.Data.SaleSetting)) : null;
            var purchaseSettings = oClsResponsePurchaseSettings?.Data?.PurchaseSetting != null ?
                serializer.Deserialize<ClsPurchaseSettingsVm>(serializer.Serialize(oClsResponsePurchaseSettings.Data.PurchaseSetting)) : null;

            var invoiceTypesByCategory = InvoiceTypeHelper.GetInvoiceTypesByCategory(
                obj.CompanyId,
                saleSettings,
                purchaseSettings
            );

            ViewBag.Headers = oClsResponse.Data.Headers;
            ViewBag.InvoiceTypesByCategory = invoiceTypesByCategory;

            return View();
        }

        // GET: InvoiceTemplateSettings/Customize
        public async Task<ActionResult> Customize(string InvoiceType, string TemplateKey, long? TemplateId)
        {
            ClsHeaderVm obj = new ClsHeaderVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            ClsInvoiceTemplateVm templateObj = new ClsInvoiceTemplateVm
            {
                CompanyId = obj.CompanyId,
                InvoiceType = InvoiceType ?? "Sales",
                TemplateKey = TemplateKey ?? "modern",
                InvoiceTemplateId = TemplateId ?? 0
            };

            ViewBag.InvoiceType = InvoiceType ?? "Sales";
            ViewBag.TemplateKey = TemplateKey ?? "modern";
            ViewBag.TemplateId = TemplateId ?? 0;

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();

            // Load template if TemplateId provided
            if (TemplateId.HasValue && TemplateId.Value > 0)
            {
                var getTemplateResult = await invoiceTemplateSettingsController.GetTemplate(templateObj);
                ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getTemplateResult);
                
                if (oClsResponse.Status == 1 && oClsResponse.Data != null && oClsResponse.Data.Template != null)
                {
                    ViewBag.Template = oClsResponse.Data.Template;
                }

                // Load template defaults from master tables
                try
                {
                    var defaultsRequest = new ClsInvoiceTemplateVm
                    {
                        InvoiceTemplateId = TemplateId.Value,
                        CompanyId = obj.CompanyId
                    };
                    var defaultsResult = await invoiceTemplateSettingsController.GetTemplateDefaults(defaultsRequest);
                    ClsResponse defaultsResponse = await oCommonController.ExtractResponseFromActionResult(defaultsResult);
                    
                    if (defaultsResponse.Status == 1 && defaultsResponse.Data != null && defaultsResponse.Data.Defaults != null)
                    {
                        ViewBag.TemplateDefaults = defaultsResponse.Data.Defaults;
                    }
                    else
                    {
                        ViewBag.TemplateDefaults = new ClsInvoiceTemplateDefaultsMasterVm();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue - defaults will be empty
                    System.Diagnostics.Debug.WriteLine($"Error loading template defaults: {ex.Message}");
                    ViewBag.TemplateDefaults = new ClsInvoiceTemplateDefaultsMasterVm();
                }
            }
            else
            {
                // For new templates or pre-defined templates (TemplateKey only), load defaults from master tables
                // Note: CompanyId is not needed for master table queries as they are shared across companies
                try
                {
                    var defaultsRequest = new ClsInvoiceTemplateVm
                    {
                        TemplateKey = TemplateKey ?? "modern",
                        InvoiceType = InvoiceType ?? "Sales"
                    };
                    var defaultsResult = await invoiceTemplateSettingsController.GetTemplateDefaults(defaultsRequest);
                    ClsResponse defaultsResponse = await oCommonController.ExtractResponseFromActionResult(defaultsResult);
                    
                    if (defaultsResponse.Status == 1 && defaultsResponse.Data != null && defaultsResponse.Data.Defaults != null)
                    {
                        ViewBag.TemplateDefaults = defaultsResponse.Data.Defaults;
                    }
                    else
                    {
                        ViewBag.TemplateDefaults = new ClsInvoiceTemplateDefaultsMasterVm();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue - defaults will be empty
                    System.Diagnostics.Debug.WriteLine($"Error loading template defaults: {ex.Message}");
                    ViewBag.TemplateDefaults = new ClsInvoiceTemplateDefaultsMasterVm();
                }
            }

            // Get section-organized labels for this template from database (if available)
            // No fallback: if not present in DB, labels section will be empty
            try
            {
                var sectionLabelConfig = new ClsSectionLabelConfig();

                ClsInvoiceTemplateVm labelsRequest = null;

                if (TemplateId.HasValue && TemplateId.Value > 0)
                {
                    labelsRequest = new ClsInvoiceTemplateVm
                    {
                        InvoiceTemplateId = TemplateId.Value,
                        CompanyId = obj.CompanyId
                    };
                }
                else
                {
                    // For pre-defined templates, use TemplateKey and InvoiceType
                    // Note: CompanyId is not needed for master table queries as they are shared across companies
                    labelsRequest = new ClsInvoiceTemplateVm
                    {
                        TemplateKey = TemplateKey ?? "modern",
                        InvoiceType = InvoiceType ?? "Sales"
                    };
                }

                if (labelsRequest != null)
                {
                    var labelsResult = await invoiceTemplateSettingsController.GetTemplateLabels(labelsRequest);
                    ClsResponse labelsResponse = await oCommonController.ExtractResponseFromActionResult(labelsResult);

                    if (labelsResponse.Status == 1 && labelsResponse.Data != null && labelsResponse.Data.Labels != null)
                    {
                        var labelsData = labelsResponse.Data.Labels;
                        
                        // Deserialize labels to typed objects
                        var labelsDataJson = serializer.Serialize(labelsData);
                        var labels = serializer.Deserialize<List<ClsInvoiceTemplateLabelMasterVm>>(labelsDataJson);

                        if (labels != null && labels.Any())
                        {
                            sectionLabelConfig = new ClsSectionLabelConfig();

                            foreach (var label in labels)
                            {
                                var dict = sectionLabelConfig.HeaderLabels;
                                switch (label.CategoryKey)
                                {
                                    case "Header":
                                        dict = sectionLabelConfig.HeaderLabels;
                                        break;
                                    case "Company":
                                        dict = sectionLabelConfig.CompanyLabels;
                                        break;
                                    case "Customer":
                                        dict = sectionLabelConfig.CustomerLabels;
                                        break;
                                    case "Items":
                                        dict = sectionLabelConfig.ItemsLabels;
                                        break;
                                    case "Summary":
                                        dict = sectionLabelConfig.SummaryLabels;
                                        break;
                                    case "Footer":
                                        dict = sectionLabelConfig.FooterLabels;
                                        break;
                                }
                                if (!dict.ContainsKey(label.LabelKey))
                                {
                                    dict.Add(label.LabelKey, label.IsVisibleByDefault);
                                }
                            }
                            
                            // Also pass the full label list with category information for category-wise grouping
                            ViewBag.LabelsByCategory = labels;
                        }
                    }
                }

                ViewBag.SectionLabels = sectionLabelConfig;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading template section labels: {ex.Message}");
                // On error, do not load any labels from code; keep labels DB-only
                ViewBag.SectionLabels = new ClsSectionLabelConfig();
            }

            // Get template HTML path from pre-defined templates
            try
            {
                var preDefinedTemplateObj = new ClsInvoiceTemplatesMasterVm
                {
                    InvoiceType = InvoiceType ?? "Sales"
                };
                
                var preDefinedResult = await invoiceTemplateSettingsController.GetPreDefinedTemplates(preDefinedTemplateObj);
                ClsResponse preDefinedResponse = await oCommonController.ExtractResponseFromActionResult(preDefinedResult);
                
                if (preDefinedResponse.Status == 1 && preDefinedResponse.Data != null)
                {
                    // The Data object contains Templates array
                    var dataJson = serializer.Serialize(preDefinedResponse.Data);
                    var dataObj = serializer.Deserialize<Dictionary<string, object>>(dataJson);
                    
                    if (dataObj != null && dataObj.ContainsKey("Templates"))
                    {
                        var templatesJson = serializer.Serialize(dataObj["Templates"]);
                        var templatesList = serializer.Deserialize<List<ClsInvoiceTemplatesMasterVm>>(templatesJson);
                        
                        if (templatesList != null && templatesList.Count > 0)
                        {
                            var selectedTemplate = templatesList.FirstOrDefault(t => t.TemplateKey == (TemplateKey ?? "modern"));
                            
                            if (selectedTemplate != null && !string.IsNullOrEmpty(selectedTemplate.TemplateHtmlPath))
                            {
                                ViewBag.TemplateHtmlPath = selectedTemplate.TemplateHtmlPath;
                                ViewBag.TemplateName = selectedTemplate.TemplateName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue - will use default template
                System.Diagnostics.Debug.WriteLine($"Error loading template HTML path: {ex.Message}");
            }

            return View();
        }

        // POST: InvoiceTemplateSettings/GetTemplates (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> GetTemplates(ClsInvoiceTemplateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var getTemplatesResult = await invoiceTemplateSettingsController.GetTemplates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getTemplatesResult);

            return Json(oClsResponse);
        }

        // POST: InvoiceTemplateSettings/GetTemplate (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> GetTemplate(ClsInvoiceTemplateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var getTemplateResult = await invoiceTemplateSettingsController.GetTemplate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getTemplateResult);

            return Json(oClsResponse);
        }

        // POST: InvoiceTemplateSettings/SaveTemplate (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> SaveTemplate(ClsInvoiceTemplateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Browser = Request.Browser.Browser;
                obj.IpAddress = Request.UserHostAddress;
                obj.Platform = Request.Browser.Platform;
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var saveTemplateResult = await invoiceTemplateSettingsController.SaveTemplate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(saveTemplateResult);

            return Json(oClsResponse);
        }

        // POST: InvoiceTemplateSettings/SetDefault (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> SetDefault(ClsInvoiceTemplateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Browser = Request.Browser.Browser;
                obj.IpAddress = Request.UserHostAddress;
                obj.Platform = Request.Browser.Platform;
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var setDefaultResult = await invoiceTemplateSettingsController.SetDefault(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(setDefaultResult);

            return Json(oClsResponse);
        }

        // POST: InvoiceTemplateSettings/GetPreDefinedTemplates (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> GetPreDefinedTemplates(ClsInvoiceTemplatesMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var getPreDefinedTemplatesResult = await invoiceTemplateSettingsController.GetPreDefinedTemplates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(getPreDefinedTemplatesResult);

            return Json(oClsResponse);
        }

        // POST: InvoiceTemplateSettings/DeleteTemplate (Wrapper for WebAPI)
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> DeleteTemplate(ClsInvoiceTemplateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Browser = Request.Browser.Browser;
                obj.IpAddress = Request.UserHostAddress;
                obj.Platform = Request.Browser.Platform;
            }

            WebApi.InvoiceTemplateSettingsController invoiceTemplateSettingsController = new WebApi.InvoiceTemplateSettingsController();
            var deleteTemplateResult = await invoiceTemplateSettingsController.DeleteTemplate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(deleteTemplateResult);

            return Json(oClsResponse);
        }
    }
}

