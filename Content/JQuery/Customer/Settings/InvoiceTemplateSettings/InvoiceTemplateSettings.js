var baseUrl = '';
var templateData = {};
var allTemplatesData = null;
var preDefinedTemplatesData = {}; // Store pre-defined templates with HTML content by invoice type
var EnableSound = Cookies.get('SystemSetting') ? Cookies.get('SystemSetting').split('&')[4].split('=')[1] : 'False';

$(document).ready(function () {
    if (typeof availableInvoiceTypes !== 'undefined') {
        loadAllTemplates();
        
        // Load templates when invoice type tab is clicked (within Customer/Supplier)
        $(document).on('shown.bs.tab', 'a[data-invoice-type]', function (e) {
            var invoiceType = $(e.target).data('invoice-type');
            if (invoiceType) {
                // Ensure templates are loaded for this invoice type
                renderTemplatesForType(invoiceType);
            }
        });
        
        // Load templates for the first active invoice type tab when category tab is switched
        $(document).on('shown.bs.tab', '#category-tabs a[data-toggle="pill"]', function (e) {
            // Find the first active invoice type tab in the newly shown category
            var targetPane = $(e.target).attr('href');
            setTimeout(function() {
                var firstInvoiceTab = $(targetPane).find('a[data-invoice-type].active').first();
                if (firstInvoiceTab.length) {
                    var invoiceType = firstInvoiceTab.data('invoice-type');
                    if (invoiceType) {
                        renderTemplatesForType(invoiceType);
                    }
                }
            }, 100);
        });
        
        // Load templates for the first active tab on page load
        setTimeout(function() {
            var firstActiveInvoiceTab = $('a[data-invoice-type].active').first();
            if (firstActiveInvoiceTab.length) {
                var firstInvoiceType = firstActiveInvoiceTab.data('invoice-type');
                if (firstInvoiceType) {
                    renderTemplatesForType(firstInvoiceType);
                }
            }
        }, 500);
    }
});

function loadAllTemplates() {
    // Load all templates once
    var obj = {
        CompanyId: getCookieValue('CompanyId')
    };

    $.ajax({
        url: '/invoicetemplatesettings/GetTemplates',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1) {
                allTemplatesData = response.Data;
                // Render templates for each invoice type
                availableInvoiceTypes.forEach(function (invoiceType) {
                    if (invoiceType.IsEnabled) {
                        renderTemplatesForType(invoiceType.InvoiceTypeKey);
                    }
                });
            } else {
                availableInvoiceTypes.forEach(function (invoiceType) {
                    if (invoiceType.IsEnabled) {
                        $('#templates-' + invoiceType.InvoiceTypeKey.toLowerCase()).html(
                            '<div class="empty-templates">' +
                            '<i class="fas fa-exclamation-circle"></i>' +
                            '<p>' + response.Message + '</p>' +
                            '</div>'
                        );
                    }
                });
            }
        },
        error: function (xhr) {
            console.error('Error loading templates:', xhr);
            availableInvoiceTypes.forEach(function (invoiceType) {
                if (invoiceType.IsEnabled) {
                    $('#templates-' + invoiceType.InvoiceTypeKey.toLowerCase()).html(
                        '<div class="empty-templates">' +
                        '<i class="fas fa-exclamation-triangle"></i>' +
                        '<p>Error loading templates. Please try again.</p>' +
                        '</div>'
                    );
                }
            });
        }
    });
}

function renderTemplatesForType(invoiceType) {
    // Wait for templates data to be loaded
    if (!allTemplatesData) {
        return;
    }

    var templatesByType = [];

    // allTemplatesData.Templates is now a flat list of templates (one per row from tblInvoiceTemplates)
    if (allTemplatesData && allTemplatesData.Templates && allTemplatesData.Templates.length) {
        templatesByType = allTemplatesData.Templates.filter(function (t) {
            return t.InvoiceType === invoiceType;
        });
    }

    renderTemplates(invoiceType.toLowerCase(), templatesByType, invoiceType);
}

function loadTemplatesByType(invoiceType) {
    // Reload all templates and re-render
    loadAllTemplates();
}

function renderTemplates(invoiceType, templates, invoiceTypeKey) {
    var container = $('#templates-' + invoiceType);
    container.removeClass('loading-templates');
    container.empty();

    var totalTemplates = (templates && templates.length) ? templates.length : 0;

    if (!templates || templates.length === 0) {
        // No templates returned from API (tblInvoiceTemplates)
        // Show an informational empty state and let user add/create from DB-backed flows
        container.html(
            '<div class="empty-templates">' +
            '<i class="fas fa-file-invoice"></i>' +
            '<p>No templates found for this invoice type.</p>' +
            '<p class="text-muted">Click "Add" to create a template from the available pre-defined designs.</p>' +
            '</div>'
        );
        return;
    }
    
    templates.forEach(function (template) {
        var cardHtml = createTemplateCard(template, invoiceTypeKey || invoiceType, totalTemplates);
        container.append(cardHtml);
    });
}

function createTemplateCard(template, invoiceType, totalTemplates) {
    var isDefault = template.IsDefault || false;
    var templateId = template.InvoiceTemplateId || 0;
    var templateKey = template.TemplateKey || 'modern';
    var templateName = template.TemplateName || 'Default Template';
    var description = template.Description || 'Professional invoice design';

    var canDelete = totalTemplates > 1; // Disallow delete if only one template exists for this type

    var cardHtml = '<div class="template-card">' +
        '<div class="template-card-top">' +
            (isDefault
                ? '<span class="template-default-badge"><i class="fas fa-star"></i><span>Default</span></span>'
                : '') +
            '<div class="dropdown template-card-menu">' +
                '<button class="btn btn-link btn-sm" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">' +
                    '<i class="fas fa-ellipsis-v"></i>' +
                '</button>' +
                '<div class="dropdown-menu dropdown-menu-right">';

    // Customize is always available
    cardHtml += '<a class="dropdown-item" href="javascript:void(0)" ' +
        'onclick="customizeTemplate(\'' + invoiceType + '\', \'' + templateKey + '\', ' + templateId + ')">' +
        '<i class="fas fa-edit mr-2"></i>Edit</a>';

    // Set Default (only when not already default)
    if (!isDefault && templateId > 0) {
        cardHtml += '<a class="dropdown-item" href="javascript:void(0)" ' +
            'onclick="setDefaultTemplate(' + templateId + ', \'' + invoiceType + '\')">' +
            '<i class="fas fa-star mr-2"></i>Set Default</a>';
    }

    // Delete (only when allowed and not default)
    if (canDelete && templateId > 0 && !isDefault) {
        cardHtml += '<a class="dropdown-item text-danger" href="javascript:void(0)" ' +
            'onclick="deleteTemplate(' + templateId + ', \'' + invoiceType + '\')">' +
            '<i class="fas fa-trash-alt mr-2"></i>Delete</a>';
    }

    cardHtml += '</div></div></div>' + // close dropdown + top bar
        '<div class="template-preview">' +
            '<i class="fas fa-file-invoice"></i>' +
        '</div>' +
        '<h5>' + templateName + '</h5>' +
        '<p class="text-muted">' + description + '</p>' +
        '</div>';

    return cardHtml;
}

var currentInvoiceType = '';
var selectedTemplateForUse = null;

function addNewTemplate(invoiceType) {
    currentInvoiceType = invoiceType;
    
    // Get invoice type display name
    var invoiceTypeName = 'Invoice';
    availableInvoiceTypes.forEach(function(type) {
        if (type.InvoiceTypeKey === invoiceType) {
            invoiceTypeName = type.DisplayName;
        }
    });
    
    $('#modalInvoiceTypeName').text(invoiceTypeName);
    
    // Load pre-invoice templates
    loadPreInvoiceTemplates(invoiceType);
    
    // Show modal
    $('#preInvoiceTemplatesModal').modal('show');
}

function loadPreInvoiceTemplates(invoiceType) {
    var grid = $('#preInvoiceTemplatesGrid');
    grid.html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"><span class="sr-only">Loading...</span></div></div>');
    
    // Fetch pre-defined templates from database
    var obj = {
        InvoiceType: invoiceType,
        CompanyId: getCookieValue('CompanyId')
    };
    
    $.ajax({
        url: '/invoicetemplatesettings/GetPreDefinedTemplates',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1 && response.Data && response.Data.Templates) {
                var preTemplates = response.Data.Templates;
                renderPreDefinedTemplates(grid, preTemplates, invoiceType);
            } else {
                // Fallback to hardcoded templates if API fails or returns no data
                loadHardcodedTemplates(grid, invoiceType);
            }
        },
        error: function (xhr) {
            console.error('Error loading pre-defined templates:', xhr);
            // Fallback to hardcoded templates on error
            loadHardcodedTemplates(grid, invoiceType);
        }
    });
}

function renderPreDefinedTemplates(grid, preTemplates, invoiceType) {
    grid.empty();
    
    if (!preTemplates || preTemplates.length === 0) {
        grid.html('<div class="text-center py-4 text-muted"><i class="fas fa-info-circle"></i><p>No templates available for this invoice type.</p></div>');
        return;
    }
    
    // Store templates data for this invoice type
    preDefinedTemplatesData[invoiceType] = preTemplates;
    
    preTemplates.forEach(function(template) {
        var templateCard = createPreInvoiceTemplateCard(template, invoiceType);
        var $card = $(templateCard);
        grid.append($card);
        
        // Load preview after card is in DOM
        var cardId = $card.attr('id');
        if (cardId) {
            setTimeout(function() {
                loadTemplatePreviewOnCard(cardId, template, invoiceType);
            }, 200);
        }
    });
}

function loadHardcodedTemplates(grid, invoiceType) {
    // Show error message - templates should come from database
    grid.html(
        '<div class="text-center py-5">' +
        '<i class="fas fa-exclamation-triangle" style="font-size: 48px; color: #f39c12;"></i>' +
        '<h5 class="mt-3">Unable to Load Templates</h5>' +
        '<p class="text-muted">There was an error loading templates from the database.</p>' +
        '<p class="text-muted">Please check your database connection or contact your system administrator.</p>' +
        '<button class="btn btn-primary mt-3" onclick="loadPreInvoiceTemplates(\'' + invoiceType + '\');">' +
        '<i class="fas fa-sync-alt"></i> Retry' +
        '</button>' +
        '</div>'
    );
}

function createPreInvoiceTemplateCard(template, invoiceType) {
    var cardId = 'template-card-' + invoiceType + '-' + template.TemplateKey + '-' + Date.now();
    var cardHtml = '<div class="pre-invoice-template-card" data-template-key="' + template.TemplateKey + '" id="' + cardId + '">' +
        '<div class="pre-invoice-template-preview" data-template-path="' + (template.TemplateHtmlPath || '') + '" data-preview-color="' + (template.PreviewColor || '#667eea') + '">' +
        '<div class="pre-invoice-template-preview-loading">' +
        '<div class="spinner-border spinner-border-sm text-primary" role="status" style="width: 24px; height: 24px;">' +
        '<span class="sr-only">Loading...</span>' +
        '</div>' +
        '</div>' +
        '<div class="pre-invoice-template-preview-fallback" style="background: ' + (template.PreviewColor || '#667eea') + '; display: none;">' +
        '<i class="' + (template.Icon || 'fas fa-file-invoice') + '"></i>' +
        '</div>' +
        '</div>' +
        '<h5>' + template.TemplateName + '</h5>' +
        '<p class="text-muted">' + template.Description + '</p>' +
        '<div class="pre-invoice-template-actions">' +
        '<button class="btn btn-sm btn-outline-primary" onclick="previewTemplate(\'' + invoiceType + '\', \'' + template.TemplateKey + '\', \'' + template.TemplateName + '\')">' +
        '<i class="fas fa-eye"></i> Preview' +
        '</button>' +
        '<button class="btn btn-sm btn-primary" onclick="useTemplate(\'' + invoiceType + '\', \'' + template.TemplateKey + '\')">' +
        '<i class="fas fa-check"></i> Use This' +
        '</button>' +
        '</div>' +
        '</div>';
    
    return cardHtml;
}

function loadTemplatePreviewOnCard(cardId, template, invoiceType) {
    var card = $('#' + cardId);
    if (!card.length) return;
    
    var previewContainer = card.find('.pre-invoice-template-preview');
    if (!previewContainer.length) return;
    
    var loadingIndicator = previewContainer.find('.pre-invoice-template-preview-loading');
    var fallbackDiv = previewContainer.find('.pre-invoice-template-preview-fallback');
    var templatePath = template.TemplateHtmlPath || previewContainer.data('template-path') || '';
    
    if (!templatePath) {
        loadingIndicator.hide();
        fallbackDiv.show();
        return;
    }
    
    // Load the HTML template
    $.get(templatePath)
        .done(function(html) {
            if (!html || !html.trim()) {
                loadingIndicator.hide();
                fallbackDiv.show();
                return;
            }
            
            // Extract styles
            var styles = '';
            var styleRegex = /<style[^>]*>([\s\S]*?)<\/style>/gi;
            var styleMatch;
            while ((styleMatch = styleRegex.exec(html)) !== null) {
                styles += styleMatch[1] + '\n';
            }
            
            // Extract body content
            var bodyMatch = html.match(/<body[^>]*>([\s\S]*?)<\/body>/i);
            var bodyContent = bodyMatch ? bodyMatch[1] : '';
            
            if (!bodyContent) {
                loadingIndicator.hide();
                fallbackDiv.show();
                return;
            }
            
            // Create iframe
            var iframeId = 'preview-' + cardId.replace(/[^a-zA-Z0-9]/g, '-');
            previewContainer.find('.template-card-preview-iframe').remove();
            
            var iframe = $('<iframe>', {
                id: iframeId,
                class: 'template-card-preview-iframe',
                css: {
                    position: 'absolute',
                    border: 'none',
                    background: 'white'
                }
            });
            
            loadingIndicator.hide();
            previewContainer.append(iframe);
            
            // Load content after a short delay
            setTimeout(function() {
                try {
                    var iframeEl = document.getElementById(iframeId);
                    if (!iframeEl) {
                        fallbackDiv.show();
                        return;
                    }
                    
                    var containerWidth = previewContainer.width() || 250;
                    var containerHeight = previewContainer.height() || 140;
                    var scale = Math.min(containerWidth / 800, containerHeight / 1000, 0.2) * 0.9;
                    
                    var iframeDoc = iframeEl.contentDocument || iframeEl.contentWindow.document;
                    iframeDoc.open();
                    iframeDoc.write('<!DOCTYPE html><html><head><meta charset="UTF-8">');
                    iframeDoc.write('<style>' + (styles || '') + '</style>');
                    iframeDoc.write('<style>');
                    iframeDoc.write('body { margin: 0; padding: 0; background: white; }');
                    iframeDoc.write('#preview-wrapper { transform: scale(' + scale + '); transform-origin: top left; width: ' + (100/scale) + '%; }');
                    iframeDoc.write('</style></head>');
                    iframeDoc.write('<body><div id="preview-wrapper">' + bodyContent + '</div></body></html>');
                    iframeDoc.close();
                    
                    // Size iframe and center it
                    setTimeout(function() {
                        try {
                            var wrapper = iframeDoc.getElementById('preview-wrapper');
                            if (wrapper) {
                                // Get the actual rendered dimensions before scaling
                                var contentWidth = wrapper.scrollWidth || 800;
                                var contentHeight = wrapper.scrollHeight || 1000;
                                
                                // Calculate scaled dimensions
                                var scaledWidth = contentWidth * scale;
                                var scaledHeight = Math.min(contentHeight * scale, containerHeight);
                                
                                // Set iframe size to scaled dimensions
                                iframeEl.style.width = scaledWidth + 'px';
                                iframeEl.style.height = scaledHeight + 'px';
                                
                                // Use CSS transform for perfect centering
                                iframeEl.style.left = '50%';
                                iframeEl.style.top = '50%';
                                iframeEl.style.transform = 'translate(-50%, -50%)';
                                iframeEl.style.margin = '0';
                                iframeEl.style.right = 'auto';
                            }
                        } catch(e) {
                            console.log('Centering error:', e);
                        }
                    }, 400);
                    
                    // Re-center after content fully loads (double-check)
                    setTimeout(function() {
                        try {
                            if (iframeEl) {
                                iframeEl.style.left = '50%';
                                iframeEl.style.top = '50%';
                                iframeEl.style.transform = 'translate(-50%, -50%)';
                            }
                        } catch(e) {}
                    }, 600);
                    
                } catch(e) {
                    console.error('Preview error:', e);
                    fallbackDiv.show();
                }
            }, 150);
        })
        .fail(function() {
            loadingIndicator.hide();
            fallbackDiv.show();
        });
}

function previewTemplate(invoiceType, templateKey, templateName) {
    $('#lightboxTemplateName').text(templateName);
    selectedTemplateForUse = { invoiceType: invoiceType, templateKey: templateKey };
    
    var previewContent = $('#lightboxPreviewContent');
    previewContent.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="sr-only">Loading preview...</span></div></div>');
    
    // Try to get the HTML path from loaded data
    var templatePath = null;
    if (preDefinedTemplatesData[invoiceType]) {
        var templates = preDefinedTemplatesData[invoiceType];
        for (var i = 0; i < templates.length; i++) {
            if (templates[i].TemplateKey === templateKey) {
                // TemplateHtmlPath comes from tblInvoiceTemplatesMaster (e.g. /Content/InvoiceTemplates/...)
                // We prefer loading from this path so each template can have its own full layout.
                templatePath = templates[i].TemplateHtmlPath || null;
                break;
            }
        }
    }
    
    // If a template HTML path is provided, load it; otherwise fall back to generated preview
    if (templatePath) {
        $.get(templatePath, function (html) {
            // Extract styles from <style> tags in the HTML
            var styles = '';
            var styleRegex = /<style[^>]*>([\s\S]*?)<\/style>/gi;
            var styleMatch;
            while ((styleMatch = styleRegex.exec(html)) !== null) {
                styles += styleMatch[1] + '\n';
            }
            
            // Extract body content - look for <body> tag
            var bodyContent = '';
            var bodyMatch = html.match(/<body[^>]*>([\s\S]*?)<\/body>/i);
            if (bodyMatch && bodyMatch[1]) {
                bodyContent = bodyMatch[1];
            } else {
                // If no body tag found, try to extract content after </head> or </style>
                var contentMatch = html.match(/(?:<\/head>|<\/style>)([\s\S]*?)(?:<\/html>|$)/i);
                if (contentMatch && contentMatch[1]) {
                    bodyContent = contentMatch[1].trim();
                } else {
                    // Last resort: remove html/head tags and use remaining content
                    bodyContent = html
                        .replace(/<html[^>]*>/gi, '')
                        .replace(/<\/html>/gi, '')
                        .replace(/<head[^>]*>[\s\S]*?<\/head>/gi, '')
                        .trim();
                }
            }
            
            // Use iframe for complete isolation to prevent CSS conflicts
            var iframeId = 'template-preview-iframe-' + Date.now();
            var previewHtml = '<iframe id="' + iframeId + '" style="width: 100%; border: none; min-height: 600px; background: white;"></iframe>';
            previewContent.html(previewHtml);
            
            // Wait for iframe to be in DOM, then write content
            setTimeout(function() {
                var iframe = document.getElementById(iframeId);
                if (!iframe) {
                    console.error('Iframe not found');
                    return;
                }
                
                var writeContent = function() {
                    try {
                        var iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
                        iframeDoc.open();
                        iframeDoc.write('<!DOCTYPE html><html><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0">');
                        if (styles) {
                            iframeDoc.write('<style type="text/css">' + styles + '</style>');
                        }
                        iframeDoc.write('</head><body>' + bodyContent + '</body></html>');
                        iframeDoc.close();
                        
                        // Adjust iframe height to content after it renders
                        setTimeout(function() {
                            try {
                                var iframeBody = iframe.contentDocument.body;
                                var html = iframe.contentDocument.documentElement;
                                var height = Math.max(
                                    html.scrollHeight, html.offsetHeight,
                                    iframeBody.scrollHeight, iframeBody.offsetHeight,
                                    600 // minimum height
                                );
                                iframe.style.height = (height + 40) + 'px';
                            } catch (e) {
                                console.log('Could not adjust iframe height:', e);
                            }
                        }, 200);
                    } catch (e) {
                        console.error('Error writing to iframe:', e);
                    }
                };
                
                iframe.onload = writeContent;
                iframe.src = 'about:blank';
            }, 50);
        }).fail(function () {
            // If loading the file fails, fall back to the generic preview
            var previewHtml = generateTemplatePreview(templateKey);
            previewContent.html(previewHtml);
        });
    } else {
        var previewHtml = generateTemplatePreview(templateKey);
        previewContent.html(previewHtml);
    }
    
    // Show lightbox
    $('#templatePreviewLightbox').modal('show');
}

function generateTemplatePreview(templateKey) {
    // Template-specific styling - supporting all template variations
    var templateStyles = {
        // Default Sales Invoice Templates
        'modern': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'classic': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'minimal': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // Quotation Templates
        'professional-quote': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'simple-quote': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        },
        'detailed-proposal': {
            headerBg: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            primaryColor: '#fa709a',
            accentColor: '#fee140',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        // Sales Order Templates
        'standard-order': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'compact-order': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'detailed-order': {
            headerBg: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            primaryColor: '#a8edea',
            accentColor: '#fed6e3',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#fce7f3'
        },
        // Proforma Templates
        'proforma-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'proforma-export': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'proforma-simple': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // Delivery Challan Templates
        'shipping-challan': {
            headerBg: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            primaryColor: '#fa709a',
            accentColor: '#fee140',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'return-challan': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'transfer-challan': {
            headerBg: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            primaryColor: '#a8edea',
            accentColor: '#fed6e3',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#fce7f3'
        },
        // Payment Templates
        'payment-request': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'online-payment': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        },
        'quick-pay': {
            headerBg: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            primaryColor: '#fa709a',
            accentColor: '#fee140',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'payment-receipt': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'payment-confirmation': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        },
        'detailed-payment': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // Credit/Debit Note Templates
        'credit-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'credit-refund': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'credit-adjustment': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'debit-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'debit-additional': {
            headerBg: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            primaryColor: '#fa709a',
            accentColor: '#fee140',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'debit-correction': {
            headerBg: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            primaryColor: '#a8edea',
            accentColor: '#fed6e3',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#fce7f3'
        },
        // Bill of Supply Templates
        'bill-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'bill-simple': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        },
        'bill-detailed': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // Purchase Templates
        'rfq-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'rfq-simple': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'rfq-detailed': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        'po-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'po-simple': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'po-detailed': {
            headerBg: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            primaryColor: '#a8edea',
            accentColor: '#fed6e3',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#fce7f3'
        },
        'purchase-modern': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'purchase-classic': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'purchase-minimal': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        'purchase-debit-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'purchase-debit-return': {
            headerBg: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            primaryColor: '#fa709a',
            accentColor: '#fee140',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'purchase-debit-adjustment': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'bill-payment-receipt': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'bill-payment-confirmation': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        },
        'bill-payment-detailed': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // POS Templates
        'pos-receipt': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'pos-thermal': {
            headerBg: 'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            primaryColor: '#30cfd0',
            accentColor: '#330867',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdfa',
            totalBg: '#ccfbf1'
        },
        'pos-detailed': {
            headerBg: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            primaryColor: '#4facfe',
            accentColor: '#00f2fe',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0f9ff',
            totalBg: '#e0f2fe'
        },
        // Sales Return Templates
        'return-standard': {
            headerBg: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            primaryColor: '#667eea',
            accentColor: '#764ba2',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f9fafb',
            totalBg: '#eff6ff'
        },
        'return-refund': {
            headerBg: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            primaryColor: '#f5576c',
            accentColor: '#f093fb',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#fef2f2',
            totalBg: '#fee2e2'
        },
        'return-exchange': {
            headerBg: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            primaryColor: '#43e97b',
            accentColor: '#38f9d7',
            borderColor: '#e5e7eb',
            tableHeaderBg: '#f0fdf4',
            totalBg: '#dcfce7'
        }
    };
    
    var style = templateStyles[templateKey] || templateStyles['modern'];
    
    var previewHtml = '<div class="template-preview-wrapper" style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); font-family: \'Inter\', -apple-system, BlinkMacSystemFont, \'Segoe UI\', Roboto, sans-serif;">' +
        '<div style="text-align: center; padding: 40px; background: ' + style.headerBg + '; border-radius: 8px; color: white; margin-bottom: 20px;">' +
        '<h2 style="margin: 0; font-size: 28px; font-weight: 700;">Sample Invoice</h2>' +
        '<p style="margin: 10px 0 0 0; opacity: 0.9; font-size: 14px;">Your Company Name</p>' +
        '</div>' +
        '<div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 20px;">' +
        '<div style="padding: 15px; background: ' + style.tableHeaderBg + '; border-radius: 6px; border: 1px solid ' + style.borderColor + ';">' +
        '<strong style="color: #6b7280; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px;">From</strong>' +
        '<p style="margin: 8px 0 0 0; color: #1f2937; font-size: 13px;">Your Business Address<br>City, State, ZIP<br>Phone: +1 234 567 8900</p>' +
        '</div>' +
        '<div style="padding: 15px; background: ' + style.tableHeaderBg + '; border-radius: 6px; border: 1px solid ' + style.borderColor + ';">' +
        '<strong style="color: #6b7280; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px;">Bill To</strong>' +
        '<p style="margin: 8px 0 0 0; color: #1f2937; font-size: 13px;">Customer Name<br>Customer Address<br>City, State, ZIP</p>' +
        '</div>' +
        '</div>' +
        '<table style="width: 100%; border-collapse: collapse; margin-bottom: 20px; border: 1px solid ' + style.borderColor + '; border-radius: 6px; overflow: hidden;">' +
        '<thead style="background: ' + style.tableHeaderBg + ';">' +
        '<tr>' +
        '<th style="padding: 12px 16px; text-align: left; font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 600;">Item</th>' +
        '<th style="padding: 12px 16px; text-align: right; font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 600;">Qty</th>' +
        '<th style="padding: 12px 16px; text-align: right; font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 600;">Price</th>' +
        '<th style="padding: 12px 16px; text-align: right; font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 600;">Total</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>' +
        '<tr style="border-bottom: 1px solid ' + style.borderColor + ';">' +
        '<td style="padding: 12px 16px; color: #1f2937;">Sample Product Item</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937;">2</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937;">$100.00</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937; font-weight: 600;">$200.00</td>' +
        '</tr>' +
        '<tr style="border-bottom: 1px solid ' + style.borderColor + ';">' +
        '<td style="padding: 12px 16px; color: #1f2937;">Another Product</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937;">1</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937;">$50.00</td>' +
        '<td style="padding: 12px 16px; text-align: right; color: #1f2937; font-weight: 600;">$50.00</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +
        '<div style="text-align: right; padding: 20px; background: ' + style.totalBg + '; border-radius: 6px; border: 1px solid ' + style.borderColor + ';">' +
        '<div style="margin-bottom: 8px; color: #6b7280; font-size: 13px;">Subtotal: <span style="color: #1f2937; font-weight: 600;">$250.00</span></div>' +
        '<div style="margin-bottom: 8px; color: #6b7280; font-size: 13px;">Tax: <span style="color: #1f2937; font-weight: 600;">$25.00</span></div>' +
        '<div style="padding-top: 12px; border-top: 2px solid ' + style.primaryColor + '; margin-top: 12px;">' +
        '<strong style="font-size: 20px; color: ' + style.primaryColor + ';">Total: $275.00</strong>' +
        '</div>' +
        '</div>' +
        '</div>';
    
    return previewHtml;
}

function useTemplate(invoiceType, templateKey) {
    // Close the pre-invoice templates modal
    $('#preInvoiceTemplatesModal').modal('hide');
    
    // Navigate to customize page
    var url = '/invoicetemplatesettings/customize?InvoiceType=' + invoiceType +
        '&TemplateKey=' + templateKey;
    window.location.href = url;
}

// Handle "Use This Template" button in lightbox
$(document).on('click', '#useThisTemplateBtn', function() {
    if (selectedTemplateForUse) {
        useTemplate(selectedTemplateForUse.invoiceType, selectedTemplateForUse.templateKey);
    }
});

function customizeTemplate(invoiceType, templateKey, templateId) {
    var url = '/invoicetemplatesettings/customize?InvoiceType=' + invoiceType +
        '&TemplateKey=' + templateKey;
    
    if (templateId > 0) {
        url += '&TemplateId=' + templateId;
    }
    
    window.location.href = url;
}

function setDefaultTemplate(templateId, invoiceType) {
    if (!confirm('Are you sure you want to set this template as default?')) {
        return;
    }

    var obj = {
        InvoiceTemplateId: templateId,
        CompanyId: getCookieValue('CompanyId'),
        AddedBy: getCookieValue('Id')
    };

    $.ajax({
        url: '/invoicetemplatesettings/SetDefault',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1) {
                toastr.success(response.Message);
                loadTemplatesByType(invoiceType);
            } else {
                toastr.error(response.Message);
            }
        },
        error: function (xhr) {
            console.error('Error setting default template:', xhr);
            toastr.error('Error setting default template');
        }
    });
}

function deleteTemplate(templateId, invoiceType) {
    // Disallow delete if this is the only template for the type at UI level
    // (createTemplateCard already hides the button when only one template exists)
    if (!confirm('Are you sure you want to delete this template? This action cannot be undone.')) {
        return;
    }

    var obj = {
        InvoiceTemplateId: templateId,
        CompanyId: getCookieValue('CompanyId'),
        AddedBy: getCookieValue('Id')
    };

    $.ajax({
        url: '/invoicetemplatesettings/DeleteTemplate',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(response.Message || 'Template deleted successfully');
                // Reload templates only for this invoice type
                loadTemplatesByType(invoiceType);
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(response.Message || 'Unable to delete template');
            }
        },
        error: function (xhr) {
            console.error('Error deleting template:', xhr);
            toastr.error('Error deleting template');
            if (EnableSound == 'True') { document.getElementById('error').play(); }
        }
    });
}

function getCookieValue(cookieName) {
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].trim();
        if (cookie.indexOf(cookieName + '=') === 0) {
            var cookieParts = cookie.split('&');
            for (var j = 0; j < cookieParts.length; j++) {
                if (cookieParts[j].indexOf(cookieName + '=') === 0) {
                    return cookieParts[j].split('=')[1];
                }
            }
            return cookie.split('=')[1];
        }
    }
    return '';
}

