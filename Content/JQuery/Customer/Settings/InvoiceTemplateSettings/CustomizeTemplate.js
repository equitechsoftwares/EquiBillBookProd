var baseUrl = '';
var templateConfig = {};
var EnableSound = Cookies.get('SystemSetting') ? Cookies.get('SystemSetting').split('&')[4].split('=')[1] : 'False';

$(document).ready(function () {
    initializeForm();
    setupColorPickers();
    loadExistingTemplate();
    setupAccordionIcons();

    // Auto-fill description for brand new templates (first time add)
    // Only do this when there is no existing template and the field is empty
    try {
        if ((typeof existingTemplate === 'undefined' || !existingTemplate) &&
            (!templateId || templateId === 0)) {

            var invoiceTitle = getInvoiceTypeTitle(typeof invoiceType !== 'undefined' ? invoiceType : '');
            var friendlyTemplateKey = (typeof templateKey !== 'undefined' && templateKey)
                ? templateKey.replace(/-/g, ' ')
                : 'Custom';

            var defaultDescription = 'Custom ' + (invoiceTitle || 'invoice') +
                ' template based on the ' + friendlyTemplateKey + ' layout.';

            var $desc = $('#txtTemplateDescription');
            if ($desc.length && !$desc.val().trim()) {
                $desc.val(defaultDescription);
            }
        }
    } catch (e) {
        // Fail silently – description auto-fill is just a UX enhancement
        console.warn('Unable to auto-fill template description:', e);
    }

    // Load the actual template HTML if templateHtmlPath is provided
    if (typeof templateHtmlPath !== 'undefined' && templateHtmlPath && templateHtmlPath.trim() !== '') {
        loadTemplateHtml(templateHtmlPath);
    } else {
        // Wait a bit for the preview template to render, then update
        setTimeout(function () {
            updatePreview();
        }, 100);
    }
});

function initializeForm() {
    // Initialize enhanced config structure (ClsEnhancedInvoiceTemplateConfig)
    templateConfig = {
        Colors: {
            HeaderColor: '#3b82f6',
            PrimaryColor: '#3b82f6',
            SecondaryColor: '#1e40af',
            BodyColor: '#ffffff'
        },
        Fonts: {
            PrimaryFontFamily: "'Inter', sans-serif",
            BaseFontSize: 13,
            FontWeights: {
                Normal: 400,
                Medium: 500,
                SemiBold: 600,
                Bold: 700
            },
            FontSizes: {
                Small: 11,
                Base: 13,
                Large: 16,
                ExtraLarge: 22,
                Title: 28
            }
        },
        Header: {
            ShowHeader: true,
            ShowLogo: true,
            LogoPosition: 'Left',
            ShowInvoiceTitle: true,
            ShowInvoiceNumber: true,
            ShowDate: true,
            ShowStatus: true,
            ShowQRCode: false,
            QRCodePosition: 'Right',
            LabelVisibility: {},
            LabelStyles: {}
        },
        Company: {
            ShowCompanySection: true,
            ShowCompanyName: true,
            ShowCompanyAddress: true,
            ShowCompanyPhone: true,
            ShowCompanyEmail: true,
            ShowCompanyGST: true,
            LabelVisibility: {},
            LabelStyles: {}
        },
        Customer: {
            ShowCustomerSection: true,
            ShowCustomerName: true,
            ShowCustomerAddress: true,
            ShowCustomerPhone: true,
            ShowCustomerEmail: true,
            ShowCustomerGST: true,
            ShowShippingAddress: false,
            ShowBillingAddress: true,
            LabelVisibility: {},
            LabelStyles: {}
        },
        Items: {
            ShowItemsTable: true,
            ShowItemImages: false,
            ShowItemDescription: true,
            ShowItemSKU: true,
            ShowItemQuantity: true,
            ShowItemRate: true,
            ShowItemDiscount: true,
            ShowItemTax: true,
            ShowItemTotal: true,
            LabelVisibility: {},
            LabelStyles: {}
        },
        Summary: {
            ShowSummarySection: true,
            ShowTotalQuantity: true,
            ShowGrossAmount: true,
            ShowTotalDiscount: true,
            ShowTaxBreakdown: true,
            ShowNetAmount: true,
            ShowGrandTotal: true,
            ShowDueAmount: false,
            LabelVisibility: {},
            LabelStyles: {}
        },
        Footer: {
            ShowFooter: true,
            ShowFooterNote: true,
            FooterNoteText: 'This is a computer generated invoice. No signature is required.',
            ShowTermsAndConditions: false,
            ShowPaymentInformation: false,
            ShowValidityPeriod: false,
            LabelVisibility: {},
            LabelStyles: {}
        },
        PrintSettings: {
            ShowPrintButton: true,
            ShowExportPdfButton: true,
            ShowPayNowButton: true,
            PrintLogo: true,
            PrintCompanyDetails: true,
            PrintWatermark: false,
            WatermarkText: '',
            ShowSignature: false,
            SignatureImageUrl: '',
            PageSize: 'A4',
            PageOrientation: 'Portrait',
            MarginTop: 0,
            MarginBottom: 0,
            MarginLeft: 0,
            MarginRight: 0,
            ShowFooter: false,
            FooterText: ''
        }
    };

    // Initialize label visibility and styles from section labels
    if (typeof sectionLabels !== 'undefined' && sectionLabels) {
        var sections = ['Header', 'Company', 'Customer', 'Items', 'Summary', 'Footer'];
        sections.forEach(function(section) {
            var labelKey = section + 'Labels';
            if (sectionLabels[labelKey]) {
                templateConfig[section].LabelVisibility = {};
                templateConfig[section].LabelStyles = {};
                Object.keys(sectionLabels[labelKey]).forEach(function(key) {
                    templateConfig[section].LabelVisibility[key] = sectionLabels[labelKey][key];
                    // Initialize default label style
                    templateConfig[section].LabelStyles[key] = {
                        Color: '#1f2937'
                    };
                });
            }
        });
    }
}

// Function to toggle visibility of label styling options (color)
function toggleLabelStylingOptions($checkbox) {
    var isChecked = $checkbox.is(':checked');
    
    // Find the parent form-group and then the row containing styling options
    var $formGroup = $checkbox.closest('.form-group');
    var $stylingRow = $formGroup.find('.row.ml-3');
    
    if (isChecked) {
        $stylingRow.show();
    } else {
        $stylingRow.hide();
    }
}

// Setup accordion icon rotation on expand/collapse
function setupAccordionIcons() {
    // Handle label sections accordion collapse events
    $('#labelSectionsAccordion').on('show.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
        $button.attr('aria-expanded', 'true');
    });

    $('#labelSectionsAccordion').on('hide.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        $button.attr('aria-expanded', 'false');
    });

    // Initialize icon state based on current collapse state for labels
    $('#labelSectionsAccordion .collapse').each(function() {
        var $collapse = $(this);
        var $button = $collapse.prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        
        if ($collapse.hasClass('show')) {
            $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            $button.attr('aria-expanded', 'true');
        } else {
            $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            $button.attr('aria-expanded', 'false');
        }
    });

    // Handle color categories accordion collapse events
    $('#colorCategoriesAccordion').on('show.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
        $button.attr('aria-expanded', 'true');
    });

    $('#colorCategoriesAccordion').on('hide.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        $button.attr('aria-expanded', 'false');
    });

    // Initialize icon state based on current collapse state for colors
    $('#colorCategoriesAccordion .collapse').each(function() {
        var $collapse = $(this);
        var $button = $collapse.prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        
        if ($collapse.hasClass('show')) {
            $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            $button.attr('aria-expanded', 'true');
        } else {
            $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            $button.attr('aria-expanded', 'false');
        }
    });

    // Handle print settings accordion collapse events
    $('#printSettingsAccordion').on('show.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
        $button.attr('aria-expanded', 'true');
    });

    $('#printSettingsAccordion').on('hide.bs.collapse', function (e) {
        var $button = $(e.target).prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        $button.attr('aria-expanded', 'false');
    });

    // Initialize icon state based on current collapse state for print settings
    $('#printSettingsAccordion .collapse').each(function() {
        var $collapse = $(this);
        var $button = $collapse.prev('.card-header').find('button');
        var $icon = $button.find('.collapse-icon');
        
        if ($collapse.hasClass('show')) {
            $icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            $button.attr('aria-expanded', 'true');
        } else {
            $icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            $button.attr('aria-expanded', 'false');
        }
    });
}

function setupColorPickers() {
    // Setup color picker change handlers - dynamically handle all color pickers with pattern color_*
    // This supports both category-grouped colors and flat color structure
    $(document).on('change', 'input[type="color"][id^="color_"]', function () {
        var $picker = $(this);
        var pickerId = $picker.attr('id');
        var colorKey = pickerId.replace('color_', ''); // Extract color key from ID
        var colorValue = $picker.val();
        
        // Update the Colors dictionary in templateConfig
        if (!templateConfig.Colors) {
            templateConfig.Colors = {};
        }
        templateConfig.Colors[colorKey] = colorValue;
        
        updatePreview();
    });

    // Font settings handlers
    $('#ddlPrimaryFontFamily').on('change', function () {
        templateConfig.Fonts.PrimaryFontFamily = $(this).val();
        updatePreview();
    });

    $('#txtBaseFontSize').on('change', function () {
        templateConfig.Fonts.BaseFontSize = parseInt($(this).val()) || 13;
        updatePreview();
    });

    $('#ddlFontWeightNormal').on('change', function () {
        templateConfig.Fonts.FontWeights.Normal = parseInt($(this).val()) || 400;
        updatePreview();
    });

    $('#ddlFontWeightBold').on('change', function () {
        templateConfig.Fonts.FontWeights.Bold = parseInt($(this).val()) || 700;
        updatePreview();
    });

    // Label visibility handlers - section-based
    $('.label-visibility-checkbox').on('change', function () {
        var section = $(this).data('section');
        var key = $(this).data('label-key');
        if (section && templateConfig[section] && templateConfig[section].LabelVisibility) {
            templateConfig[section].LabelVisibility[key] = $(this).is(':checked');
            updatePreview();
        }
        // Toggle visibility of styling options (color)
        toggleLabelStylingOptions($(this));
    });

    // Initialize visibility of styling options on page load
    $('.label-visibility-checkbox').each(function() {
        toggleLabelStylingOptions($(this));
    });

    // Label color picker handlers - per-label styling
    $(document).on('change', '.label-color-picker', function () {
        var section = $(this).data('section');
        var key = $(this).data('label-key');
        var color = $(this).val();
        if (section && templateConfig[section] && templateConfig[section].LabelStyles) {
            if (!templateConfig[section].LabelStyles[key]) {
                templateConfig[section].LabelStyles[key] = {};
            }
            templateConfig[section].LabelStyles[key].Color = color;
            updatePreview();
        }
    });


    // Header section handlers
    $('#chkShowHeader').on('change', function () {
        templateConfig.Header.ShowHeader = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowLogo').on('change', function () {
        templateConfig.Header.ShowLogo = $(this).is(':checked');
        updatePreview();
    });

    $('#ddlLogoPosition').on('change', function () {
        templateConfig.Header.LogoPosition = $(this).val();
        updatePreview();
    });

    // Company section handlers
    $('#chkShowCompanySection').on('change', function () {
        templateConfig.Company.ShowCompanySection = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowCompanyName').on('change', function () {
        templateConfig.Company.ShowCompanyName = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowCompanyAddress').on('change', function () {
        templateConfig.Company.ShowCompanyAddress = $(this).is(':checked');
        updatePreview();
    });

    // Customer section handlers
    $('#chkShowCustomerSection').on('change', function () {
        templateConfig.Customer.ShowCustomerSection = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowShippingAddress').on('change', function () {
        templateConfig.Customer.ShowShippingAddress = $(this).is(':checked');
        updatePreview();
    });

    // Items section handlers
    $('#chkShowItemsTable').on('change', function () {
        templateConfig.Items.ShowItemsTable = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowItemImages').on('change', function () {
        templateConfig.Items.ShowItemImages = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowItemDescription').on('change', function () {
        templateConfig.Items.ShowItemDescription = $(this).is(':checked');
        updatePreview();
    });

    // Summary section handlers
    $('#chkShowSummarySection').on('change', function () {
        templateConfig.Summary.ShowSummarySection = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowTaxBreakdown').on('change', function () {
        templateConfig.Summary.ShowTaxBreakdown = $(this).is(':checked');
        updatePreview();
    });

    // Footer section handlers
    $('#chkShowFooter').on('change', function () {
        templateConfig.Footer.ShowFooter = $(this).is(':checked');
        updatePreview();
    });

    $('#chkShowFooterNote').on('change', function () {
        templateConfig.Footer.ShowFooterNote = $(this).is(':checked');
        updatePreview();
    });

    // Print settings handlers
    // Print Settings Event Handlers
    $('#chkShowPrintButton').on('change', function () {
        templateConfig.PrintSettings.ShowPrintButton = $(this).is(':checked');
    });

    $('#chkShowExportPdfButton').on('change', function () {
        templateConfig.PrintSettings.ShowExportPdfButton = $(this).is(':checked');
    });

    $('#chkShowPayNowButton').on('change', function () {
        templateConfig.PrintSettings.ShowPayNowButton = $(this).is(':checked');
    });

    $('#chkPrintLogo').on('change', function () {
        templateConfig.PrintSettings.PrintLogo = $(this).is(':checked');
    });

    $('#chkPrintCompanyDetails').on('change', function () {
        templateConfig.PrintSettings.PrintCompanyDetails = $(this).is(':checked');
    });

    $('#chkPrintWatermark').on('change', function () {
        var isChecked = $(this).is(':checked');
        templateConfig.PrintSettings.PrintWatermark = isChecked;
        $('#watermarkTextGroup').toggle(isChecked);
    });

    $('#txtWatermarkText').on('input', function () {
        templateConfig.PrintSettings.WatermarkText = $(this).val() || '';
    });

    $('#chkShowSignature').on('change', function () {
        var isChecked = $(this).is(':checked');
        templateConfig.PrintSettings.ShowSignature = isChecked;
        $('#signatureUploadGroup').toggle(isChecked);
    });

    // Handle file selection - update label
    $('#fileSignature').on('change', function () {
        var fileName = $(this).val().split('\\').pop();
        if (fileName) {
            $('#fileSignatureLabel').text(fileName).removeClass('text-muted').addClass('text-dark');
            $('#btnUploadSignature').prop('disabled', false);
        } else {
            $('#fileSignatureLabel').text('Choose signature image').removeClass('text-dark').addClass('text-muted');
            $('#btnUploadSignature').prop('disabled', true);
        }
    });

    // Handle upload button click
    $('#btnUploadSignature').on('click', function () {
        var fileInput = $('#fileSignature')[0];
        if (!fileInput.files || fileInput.files.length === 0) {
            alert('Please select a file first.');
            return;
        }

        var file = fileInput.files[0];
        
        // Validate file type
        if (!file.type.match('image.*')) {
            alert('Please select an image file (PNG, JPG, or GIF).');
            return;
        }
        
        // Validate file size (max 2MB)
        if (file.size > 2 * 1024 * 1024) {
            alert('File size must be less than 2MB.');
            return;
        }

        // Create FormData for upload
        var formData = new FormData();
        formData.append('file', file);
        formData.append('uploadType', 'signature');

        // Show loading state
        var $btn = $(this);
        var originalHtml = $btn.html();
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin mr-1"></i> Uploading...');

        // Upload file
        $.ajax({
            url: '/api/invoicetemplatesettings/uploadsignature',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response && response.success && response.url) {
                    templateConfig.PrintSettings.SignatureImageUrl = response.url;
                    $('#txtSignatureImageUrl').val(response.url);
                    
                    // Show preview
                    if ($('#signaturePreview').length) {
                        // Update existing preview
                        $('#signaturePreview').attr('src', response.url);
                    } else {
                        // Insert new preview
                        var previewHtml = '<div class="mb-3">' +
                            '<div class="border rounded p-3 text-center" style="background-color: #f8f9fa;">' +
                            '<img id="signaturePreview" src="' + response.url + '" alt="Signature Preview" ' +
                            'style="max-width: 200px; max-height: 100px; border: 1px solid #ddd; padding: 5px; background: #fff; display: block; margin: 0 auto;" />' +
                            '<button type="button" class="btn btn-sm btn-danger mt-2" onclick="removeSignature()">' +
                            '<i class="fas fa-times"></i> Remove Signature</button>' +
                            '</div></div>' +
                            '<div class="text-muted small mb-3">Or upload a new signature image:</div>';
                        $('#signatureUploadGroup label').after(previewHtml);
                    }
                    
                    // Reset file input
                    $('#fileSignature').val('');
                    $('#fileSignatureLabel').text('Choose signature image').removeClass('text-dark').addClass('text-muted');
                    
                    alert('Signature uploaded successfully!');
                } else {
                    alert(response.message || 'Failed to upload signature.');
                }
            },
            error: function (xhr, status, error) {
                alert('Error uploading signature: ' + (xhr.responseJSON?.message || error));
            },
            complete: function () {
                $btn.prop('disabled', false).html(originalHtml);
            }
        });
    });

    $('#chkShowFooterText').on('change', function () {
        var isChecked = $(this).is(':checked');
        templateConfig.PrintSettings.ShowFooter = isChecked;
        $('#footerTextGroup').toggle(isChecked);
    });

    $('#ddlPageSize').on('change', function () {
        templateConfig.PrintSettings.PageSize = $(this).val();
    });

    $('#ddlPageOrientation').on('change', function () {
        templateConfig.PrintSettings.PageOrientation = $(this).val();
    });

    $('#txtMarginTop').on('input', function () {
        templateConfig.PrintSettings.MarginTop = parseFloat($(this).val()) || 0;
    });

    $('#txtMarginBottom').on('input', function () {
        templateConfig.PrintSettings.MarginBottom = parseFloat($(this).val()) || 0;
    });

    $('#txtMarginLeft').on('input', function () {
        templateConfig.PrintSettings.MarginLeft = parseFloat($(this).val()) || 0;
    });

    $('#txtMarginRight').on('input', function () {
        templateConfig.PrintSettings.MarginRight = parseFloat($(this).val()) || 0;
    });

    $('#txtFooterText').on('input', function () {
        templateConfig.PrintSettings.FooterText = $(this).val() || '';
        updatePreview();
    });

    // Update preview on any input change
    $('#txtTemplateName').on('input', function () {
        $('#templateNameDisplay').text($(this).val() || 'Modern');
        // Clear error when user starts typing
        $('#divTemplateName').hide().text('');
        $('#txtTemplateName').css('border', '');
    });
}

function loadTemplateHtml(templatePath) {
    var $preview = $('#invoicePreview');
    $preview.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="sr-only">Loading template...</span></div></div>');
    
    $.get(templatePath)
        .done(function(html) {
            if (!html || !html.trim()) {
                console.error('Template HTML is empty');
                loadDefaultPreview();
                return;
            }
            
            // Extract styles from <style> tags
            var styles = '';
            var styleRegex = /<style[^>]*>([\s\S]*?)<\/style>/gi;
            var styleMatch;
            while ((styleMatch = styleRegex.exec(html)) !== null) {
                styles += styleMatch[1] + '\n';
            }
            
            // Extract body content
            var bodyContent = '';
            var bodyMatch = html.match(/<body[^>]*>([\s\S]*?)<\/body>/i);
            if (bodyMatch && bodyMatch[1]) {
                bodyContent = bodyMatch[1];
            } else {
                // If no body tag, try to extract content after </head> or </style>
                var contentMatch = html.match(/(?:<\/head>|<\/style>)([\s\S]*?)(?:<\/html>|$)/i);
                if (contentMatch && contentMatch[1]) {
                    bodyContent = contentMatch[1].trim();
                } else {
                    // Last resort: remove html/head tags
                    bodyContent = html
                        .replace(/<html[^>]*>/gi, '')
                        .replace(/<\/html>/gi, '')
                        .replace(/<head[^>]*>[\s\S]*?<\/head>/gi, '')
                        .trim();
                }
            }
            
            if (!bodyContent) {
                console.error('Could not extract body content from template');
                loadDefaultPreview();
                return;
            }
            
            // Replace the preview with the actual template HTML
            var previewHtml = '<style>' + (styles || '') + '</style>' + bodyContent;
            $preview.html(previewHtml);
            
            // Wait for template to render, then update preview with config
            setTimeout(function () {
                updatePreview();
            }, 200);
        })
        .fail(function(xhr, status, error) {
            console.error('Error loading template HTML:', error);
            loadDefaultPreview();
        });
}

function loadDefaultPreview() {
    // Fallback to default preview template
    var $preview = $('#invoicePreview');
    $preview.load('/invoicetemplatesettings/_InvoicePreviewTemplate', function() {
        setTimeout(function () {
            updatePreview();
        }, 100);
    });
}

function loadExistingTemplate() {
    if (typeof existingTemplate !== 'undefined' && existingTemplate) {
        // Load existing template configuration
        if (existingTemplate.TemplateConfig) {
            // Check if it's already enhanced config or legacy config
            if (typeof existingTemplate.TemplateConfig === 'object' && existingTemplate.TemplateConfig.Colors) {
                // Already enhanced config
                templateConfig = existingTemplate.TemplateConfig;
            } else if (typeof existingTemplate.TemplateConfig === 'string') {
                // Try to parse as JSON
                try {
                    var parsed = JSON.parse(existingTemplate.TemplateConfig);
                    if (parsed.Colors) {
                        templateConfig = parsed;
                    } else {
                        // Legacy config - initialize with defaults
                        initializeForm();
                    }
                } catch (e) {
                    console.warn('Could not parse template config, using defaults');
                    initializeForm();
                }
            } else {
                templateConfig = existingTemplate.TemplateConfig;
            }
            
            // Update form fields - Colors
            $('#txtTemplateName').val(existingTemplate.TemplateName || 'Custom Template');
            $('#txtTemplateDescription').val(existingTemplate.Description || '');
            
            // Update all color pickers dynamically based on templateConfig.Colors
            if (templateConfig.Colors) {
                Object.keys(templateConfig.Colors).forEach(function(colorKey) {
                    var $colorPicker = $('#color_' + colorKey);
                    if ($colorPicker.length) {
                        $colorPicker.val(templateConfig.Colors[colorKey]);
                    }
                });
            }

            // Update form fields - Fonts
            $('#ddlPrimaryFontFamily').val(templateConfig.Fonts?.PrimaryFontFamily || "'Inter', sans-serif");
            $('#txtBaseFontSize').val(templateConfig.Fonts?.BaseFontSize || 13);
            $('#ddlFontWeightNormal').val(templateConfig.Fonts?.FontWeights?.Normal || 400);
            $('#ddlFontWeightBold').val(templateConfig.Fonts?.FontWeights?.Bold || 700);

            $('#chkShowHeader').prop('checked', templateConfig.Header?.ShowHeader !== false);
            $('#chkShowLogo').prop('checked', templateConfig.Header?.ShowLogo !== false);
            $('#ddlLogoPosition').val(templateConfig.Header?.LogoPosition || 'Left');

            // Update form fields - Sections
            $('#chkShowCompanySection').prop('checked', templateConfig.Company?.ShowCompanySection !== false);
            $('#chkShowCompanyName').prop('checked', templateConfig.Company?.ShowCompanyName !== false);
            $('#chkShowCompanyAddress').prop('checked', templateConfig.Company?.ShowCompanyAddress !== false);
            $('#chkShowCustomerSection').prop('checked', templateConfig.Customer?.ShowCustomerSection !== false);
            $('#chkShowShippingAddress').prop('checked', templateConfig.Customer?.ShowShippingAddress === true);
            $('#chkShowItemsTable').prop('checked', templateConfig.Items?.ShowItemsTable !== false);
            $('#chkShowItemImages').prop('checked', templateConfig.Items?.ShowItemImages === true);
            $('#chkShowItemDescription').prop('checked', templateConfig.Items?.ShowItemDescription !== false);
            $('#chkShowSummarySection').prop('checked', templateConfig.Summary?.ShowSummarySection !== false);
            $('#chkShowTaxBreakdown').prop('checked', templateConfig.Summary?.ShowTaxBreakdown !== false);
            $('#chkShowFooter').prop('checked', templateConfig.Footer?.ShowFooter !== false);
            $('#chkShowFooterNote').prop('checked', templateConfig.Footer?.ShowFooterNote !== false);

            // Update form fields - Print
            $('#chkShowPrintButton').prop('checked', templateConfig.PrintSettings?.ShowPrintButton !== false);
            $('#chkShowExportPdfButton').prop('checked', templateConfig.PrintSettings?.ShowExportPdfButton !== false);
            $('#chkShowPayNowButton').prop('checked', templateConfig.PrintSettings?.ShowPayNowButton !== false);
            $('#chkPrintLogo').prop('checked', templateConfig.PrintSettings?.PrintLogo !== false);
            $('#chkPrintCompanyDetails').prop('checked', templateConfig.PrintSettings?.PrintCompanyDetails !== false);
            $('#chkPrintWatermark').prop('checked', templateConfig.PrintSettings?.PrintWatermark === true);
            $('#watermarkTextGroup').toggle(templateConfig.PrintSettings?.PrintWatermark === true);
            $('#txtWatermarkText').val(templateConfig.PrintSettings?.WatermarkText || '');
            $('#chkShowSignature').prop('checked', templateConfig.PrintSettings?.ShowSignature === true);
            $('#signatureUploadGroup').toggle(templateConfig.PrintSettings?.ShowSignature === true);
            // Initialize signature upload button state
            if (!$('#fileSignature').val()) {
                $('#btnUploadSignature').prop('disabled', true);
            }
            // Update signature preview if URL exists
            var signatureUrl = templateConfig.PrintSettings?.SignatureImageUrl || '';
            if (signatureUrl && $('#signaturePreview').length === 0) {
                var previewHtml = '<div class="mb-3">' +
                    '<div class="border rounded p-3 text-center" style="background-color: #f8f9fa;">' +
                    '<img id="signaturePreview" src="' + signatureUrl + '" alt="Signature Preview" ' +
                    'style="max-width: 200px; max-height: 100px; border: 1px solid #ddd; padding: 5px; background: #fff; display: block; margin: 0 auto;" />' +
                    '<button type="button" class="btn btn-sm btn-danger mt-2" onclick="removeSignature()">' +
                    '<i class="fas fa-times"></i> Remove Signature</button>' +
                    '</div></div>' +
                    '<div class="text-muted small mb-3">Or upload a new signature image:</div>';
                $('#signatureUploadGroup label').after(previewHtml);
            } else if (signatureUrl && $('#signaturePreview').length > 0) {
                $('#signaturePreview').attr('src', signatureUrl);
            }
            $('#ddlPageSize').val(templateConfig.PrintSettings?.PageSize || 'A4');
            $('#ddlPageOrientation').val(templateConfig.PrintSettings?.PageOrientation || 'Portrait');
            $('#txtMarginTop').val(templateConfig.PrintSettings?.MarginTop || 0);
            $('#txtMarginBottom').val(templateConfig.PrintSettings?.MarginBottom || 0);
            $('#txtMarginLeft').val(templateConfig.PrintSettings?.MarginLeft || 0);
            $('#txtMarginRight').val(templateConfig.PrintSettings?.MarginRight || 0);
            $('#chkShowFooterText').prop('checked', templateConfig.PrintSettings?.ShowFooter === true);
            $('#footerTextGroup').toggle(templateConfig.PrintSettings?.ShowFooter === true);
            $('#txtFooterText').val(templateConfig.PrintSettings?.FooterText || '');

            // Update label visibility checkboxes - section-based
            var sections = ['Header', 'Company', 'Customer', 'Items', 'Summary', 'Footer'];
            sections.forEach(function(section) {
                var sectionLower = section.toLowerCase();
                if (templateConfig[section]?.LabelVisibility) {
                    Object.keys(templateConfig[section].LabelVisibility).forEach(function (key) {
                        var $checkbox = $('#label_' + sectionLower + '_' + key);
                        $checkbox.prop('checked', templateConfig[section].LabelVisibility[key]);
                        // Toggle styling options visibility based on checkbox state
                        toggleLabelStylingOptions($checkbox);
                    });
                }
                // Load label styles (from database only, no fallback)
                if (templateConfig[section]?.LabelStyles) {
                    Object.keys(templateConfig[section].LabelStyles).forEach(function (key) {
                        var style = templateConfig[section].LabelStyles[key];
                        if (style && style.Color) {
                            $('#label_' + sectionLower + '_' + key + '_color').val(style.Color);
                        }
                    });
                }
            });
        }
    }
}

function updatePreview() {
    var $preview = $('#invoicePreview');
    
    // Check if we're using the default preview template or actual template HTML
    var $container = $preview.find('.preview-invoice-container');
    
    // Look for template root elements (e.g., .simple-quote-root, .compact-quote-root, etc.)
    // Try multiple selectors to find the root element
    var $templateRoot = $preview.find('[class*="-root"]').first();
    if ($templateRoot.length === 0) {
        // Try finding by invoice-container or similar
        $templateRoot = $preview.find('.invoice-container').parent();
    }
    if ($templateRoot.length === 0) {
        // Try finding the first div that's not the preview wrapper
        $templateRoot = $preview.children('div').first();
    }
    
    // If we have an actual template loaded (has invoice-container or similar structure), use that
    var hasActualTemplate = $preview.find('.invoice-container, .simple-quote-root, .compact-quote-root, [class*="-quote-root"], [class*="-invoice-root"]').length > 0;
    
    if (hasActualTemplate && $templateRoot.length > 0) {
        // Working with actual template HTML (e.g., simple-quote.html)
        updateActualTemplatePreview($templateRoot);
    } else if ($container.length > 0) {
        // Working with default preview template
        updateDefaultPreviewTemplate($container);
    } else {
        // Preview template not loaded yet
        return;
    }
}

function updateActualTemplatePreview($templateRoot) {
    // Apply CSS variables for actual template styling - using simplified Colors object
    var colors = templateConfig.Colors || {};
    var fonts = templateConfig.Fonts || {};
    
    $templateRoot.css({
        '--header-color': colors.HeaderColor || '#3b82f6',
        '--primary-color': colors.PrimaryColor || '#3b82f6',
        '--secondary-color': colors.SecondaryColor || '#1e40af',
        '--body-color': colors.BodyColor || '#ffffff',
        '--font-family': fonts.PrimaryFontFamily || "'Inter', sans-serif",
        '--font-size': (fonts.BaseFontSize || 13) + 'px',
        '--font-weight-normal': fonts.FontWeights?.Normal || 400,
        '--font-weight-bold': fonts.FontWeights?.Bold || 700
    });

    // Also apply to the preview wrapper to ensure variables cascade
    var $preview = $('#invoicePreview');
    $preview.css({
        '--header-color': colors.HeaderColor || '#3b82f6',
        '--primary-color': colors.PrimaryColor || '#3b82f6',
        '--secondary-color': colors.SecondaryColor || '#1e40af',
        '--body-color': colors.BodyColor || '#ffffff',
        '--font-family': fonts.PrimaryFontFamily || "'Inter', sans-serif",
        '--font-size': (fonts.BaseFontSize || 13) + 'px',
        '--font-weight-normal': fonts.FontWeights?.Normal || 400,
        '--font-weight-bold': fonts.FontWeights?.Bold || 700
    });

    // Update quotation badge/invoice title color
    var $badge = $templateRoot.find('.quotation-badge, .invoice-badge, [class*="badge"]');
    if ($badge.length > 0) {
        $badge.css({
            'background': colors.HeaderColor || colors.PrimaryColor || '#3b82f6',
            'color': '#ffffff'
        });
    }

    // Show/hide sections based on configuration
    var header = templateConfig.Header || {};
    var company = templateConfig.Company || {};
    var customer = templateConfig.Customer || {};
    var items = templateConfig.Items || {};
    var summary = templateConfig.Summary || {};
    var footer = templateConfig.Footer || {};

    // Header section
    var $headerSection = $templateRoot.find('.header-section, .invoice-header');
    if ($headerSection.length > 0) {
        if (header.ShowHeader !== false) {
            $headerSection.show();
            $headerSection.css({
                'background': colors.HeaderColor || colors.PrimaryColor || '#3b82f6',
                'color': '#ffffff'
            });
        } else {
            $headerSection.hide();
        }
    }

    // Show/hide logo
    var $logo = $templateRoot.find('.company-logo, img[alt*="Logo"], .logo');
    if ($logo.length > 0) {
        var $logoContainer = $logo.closest('.company-section, .logo-container, .header-section');
        if ($logoContainer.length > 0) {
            if (header.ShowLogo !== false) {
                $logoContainer.show();
                $logo.show();
            } else {
                $logo.hide();
            }
        }
    }

    // Company section
    var $companySection = $templateRoot.find('.company-section, .from-section');
    if ($companySection.length > 0) {
        if (company.ShowCompanySection !== false) {
            $companySection.show();
        } else {
            $companySection.hide();
        }
    }

    // Customer section
    var $customerSection = $templateRoot.find('.customer-section, .bill-to-section');
    if ($customerSection.length > 0) {
        if (customer.ShowCustomerSection !== false) {
            $customerSection.show();
        } else {
            $customerSection.hide();
        }
    }

    // Items table
    var $itemsTable = $templateRoot.find('.items-table, table');
    if ($itemsTable.length > 0) {
        if (items.ShowItemsTable !== false) {
            $itemsTable.show();
        } else {
            $itemsTable.hide();
        }
    }

    // Summary section
    var $summarySection = $templateRoot.find('.summary-section, .totals-section');
    if ($summarySection.length > 0) {
        if (summary.ShowSummarySection !== false) {
            $summarySection.show();
        } else {
            $summarySection.hide();
        }
    }

    // Footer section
    var $footerSection = $templateRoot.find('.footer-section, .invoice-footer');
    if ($footerSection.length > 0) {
        if (footer.ShowFooter !== false) {
            $footerSection.show();
            $footerSection.css({
                'background': colors.SecondaryColor || '#1e40af'
            });
        } else {
            $footerSection.hide();
        }
    }

    // Update invoice/quotation title text
    var invoiceTitle = getInvoiceTypeTitle(invoiceType);
    var $titleBadge = $templateRoot.find('.quotation-badge, .invoice-badge, [class*="badge"]');
    if ($titleBadge.length > 0) {
        $titleBadge.text(invoiceTitle.toUpperCase());
    }

    // Update label visibility and styling based on section-based configuration
    var sections = ['Header', 'Company', 'Customer', 'Items', 'Summary', 'Footer'];
    sections.forEach(function(section) {
        var sectionConfig = templateConfig[section];
        if (sectionConfig && sectionConfig.LabelVisibility) {
            Object.keys(sectionConfig.LabelVisibility).forEach(function (key) {
                var $element = $templateRoot.find('[data-label="' + key + '"], [data-section="' + section + '"][data-label="' + key + '"]');
                if ($element.length > 0) {
                    // Show/hide based on visibility
                    if (sectionConfig.LabelVisibility[key]) {
                        $element.show();
                        
                        // Apply per-label styling
                        if (sectionConfig.LabelStyles && sectionConfig.LabelStyles[key]) {
                            var labelStyle = sectionConfig.LabelStyles[key];
                            $element.css({
                                'color': labelStyle.Color || '#1f2937'
                            });
                        }
                    } else {
                        $element.hide();
                    }
                }
            });
        }
    });
}

function updateDefaultPreviewTemplate($container) {
    // Apply CSS variables for default preview template styling - using simplified Colors object
    var colors = templateConfig.Colors || {};
    var fonts = templateConfig.Fonts || {};
    
    $container.css({
        '--preview-header-color': colors.HeaderColor || '#3b82f6',
        '--preview-primary-color': colors.PrimaryColor || '#3b82f6',
        '--preview-secondary-color': colors.SecondaryColor || '#1e40af',
        '--preview-body-color': colors.BodyColor || '#ffffff',
        '--preview-font-family': fonts.PrimaryFontFamily || "'Inter', sans-serif",
        '--preview-font-size': (fonts.BaseFontSize || 13) + 'px',
        '--preview-font-weight-normal': fonts.FontWeights?.Normal || 400,
        '--preview-font-weight-bold': fonts.FontWeights?.Bold || 700
    });

    // Update header section
    var $headerSection = $container.find('.preview-header-section');
    $headerSection.css({
        'background': colors.HeaderColor || colors.PrimaryColor || '#3b82f6',
        'color': '#ffffff'
    });

    // Show/hide sections based on enhanced config
    var header = templateConfig.Header || {};
    var company = templateConfig.Company || {};
    var customer = templateConfig.Customer || {};
    var items = templateConfig.Items || {};
    var summary = templateConfig.Summary || {};
    var footer = templateConfig.Footer || {};

    // Show/hide logo
    var $logoContainer = $container.find('#previewLogoContainer');
    if (header.ShowLogo !== false) {
        $logoContainer.show();
    } else {
        $logoContainer.hide();
    }

    // Update logo position
    if (header.LogoPosition === 'Center') {
        $container.find('.preview-header-content').css('justify-content', 'center');
    } else if (header.LogoPosition === 'Right') {
        $container.find('.preview-header-content').css('justify-content', 'flex-end');
    } else {
        $container.find('.preview-header-content').css('justify-content', 'flex-start');
    }

    // Show/hide shipping address
    var $shipTo = $container.find('#previewShipTo');
    if (customer.ShowShippingAddress === true) {
        $shipTo.show();
    } else {
        $shipTo.hide();
    }

    // Show/hide action buttons
    var $actionButtons = $container.find('#previewActionButtons');
    var showButtons = templateConfig.PrintSettings?.ShowPrintButton || templateConfig.PrintSettings?.ShowExportPdfButton;
    if (showButtons) {
        $actionButtons.show();
    } else {
        $actionButtons.hide();
    }

    // Show/hide footer note
    var $footerNote = $container.find('#previewFooterNote');
    if (footer.ShowFooterNote !== false) {
        $footerNote.show();
        if (footer.FooterNoteText) {
            $footerNote.find('p').html('<i class="fas fa-info-circle"></i> ' + footer.FooterNoteText);
        }
    } else {
        $footerNote.hide();
    }

    // Update footer section background color
    var $footerSection = $container.find('.preview-footer-section, .footer-section');
    if ($footerSection.length > 0) {
        $footerSection.css({
            'background': colors.SecondaryColor || '#1e40af'
        });
    }

    // Update invoice title based on invoice type
    var invoiceTitle = getInvoiceTypeTitle(invoiceType);
    $container.find('#previewInvoiceTitle').text(invoiceTitle);

    // Update label visibility and styling based on section-based configuration
    var sections = ['Header', 'Company', 'Customer', 'Items', 'Summary', 'Footer'];
    sections.forEach(function(section) {
        var sectionConfig = templateConfig[section];
        if (sectionConfig && sectionConfig.LabelVisibility) {
            Object.keys(sectionConfig.LabelVisibility).forEach(function (key) {
                var $element = $container.find('[data-label="' + key + '"], [data-section="' + section + '"][data-label="' + key + '"]');
                if ($element.length > 0) {
                    // Show/hide based on visibility
                    if (sectionConfig.LabelVisibility[key]) {
                        $element.show();
                        
                        // Apply per-label styling
                        if (sectionConfig.LabelStyles && sectionConfig.LabelStyles[key]) {
                            var labelStyle = sectionConfig.LabelStyles[key];
                            $element.css({
                                'color': labelStyle.Color || '#1f2937'
                            });
                        }
                    } else {
                        $element.hide();
                    }
                }
            });
        }
    });
}

function getInvoiceTypeTitle(type) {
    var titles = {
        'Sales': 'Sales Invoice',
        'SalesQuotation': 'Sales Quotation',
        'SalesOrder': 'Sales Order',
        'SalesProforma': 'Sales Proforma',
        'DeliveryChallan': 'Delivery Challan',
        'Pos': 'POS Invoice',
        'SalesReturn': 'Sales Return',
        'Purchase': 'Purchase Bill',
        'PurchaseQuotation': 'Purchase Quotation',
        'PurchaseOrder': 'Purchase Order',
        'PurchaseReturn': 'Purchase Return'
    };
    return titles[type] || 'Invoice';
}

function gatherTemplateConfig() {
    // Gather Colors - dynamically collect from all color pickers
    templateConfig.Colors = {};
    $('input[type="color"][id^="color_"]').each(function() {
        var $picker = $(this);
        var pickerId = $picker.attr('id');
        var colorKey = pickerId.replace('color_', '');
        var colorValue = $picker.val();
        if (colorValue) {
            templateConfig.Colors[colorKey] = colorValue;
        }
    });
    
    // Ensure backward compatibility with common color keys if they exist
    if (!templateConfig.Colors.HeaderColor && $('#color_HeaderColor').length) {
        templateConfig.Colors.HeaderColor = $('#color_HeaderColor').val() || '#3b82f6';
    }
    if (!templateConfig.Colors.PrimaryColor && $('#color_PrimaryColor').length) {
        templateConfig.Colors.PrimaryColor = $('#color_PrimaryColor').val() || '#3b82f6';
    }
    if (!templateConfig.Colors.SecondaryColor && $('#color_SecondaryColor').length) {
        templateConfig.Colors.SecondaryColor = $('#color_SecondaryColor').val() || '#1e40af';
    }
    if (!templateConfig.Colors.BodyColor && $('#color_BodyColor').length) {
        templateConfig.Colors.BodyColor = $('#color_BodyColor').val() || '#ffffff';
    }

    // Gather Fonts
    templateConfig.Fonts = {
        PrimaryFontFamily: $('#ddlPrimaryFontFamily').val() || "'Inter', sans-serif",
        BaseFontSize: parseInt($('#txtBaseFontSize').val()) || 13,
        FontWeights: {
            Normal: parseInt($('#ddlFontWeightNormal').val()) || 400,
            Medium: templateConfig.Fonts?.FontWeights?.Medium || 500,
            SemiBold: templateConfig.Fonts?.FontWeights?.SemiBold || 600,
            Bold: parseInt($('#ddlFontWeightBold').val()) || 700
        },
        FontSizes: templateConfig.Fonts?.FontSizes || {
            Small: 11,
            Base: 13,
            Large: 16,
            ExtraLarge: 22,
            Title: 28
        }
    };

    // Gather Header section
    templateConfig.Header = {
        ShowHeader: $('#chkShowHeader').is(':checked'),
        ShowLogo: $('#chkShowLogo').is(':checked'),
        LogoPosition: $('#ddlLogoPosition').val() || 'Left',
        ShowInvoiceTitle: true,
        ShowInvoiceNumber: true,
        ShowDate: true,
        ShowStatus: true,
        ShowQRCode: false,
        QRCodePosition: 'Right',
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Header"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Header.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Header.LabelStyles[key] = {
            Color: $('#label_header_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Company section
    templateConfig.Company = {
        ShowCompanySection: $('#chkShowCompanySection').is(':checked'),
        ShowCompanyName: $('#chkShowCompanyName').is(':checked'),
        ShowCompanyAddress: $('#chkShowCompanyAddress').is(':checked'),
        ShowCompanyPhone: true,
        ShowCompanyEmail: true,
        ShowCompanyGST: true,
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Company"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Company.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Company.LabelStyles[key] = {
            Color: $('#label_company_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Customer section
    templateConfig.Customer = {
        ShowCustomerSection: $('#chkShowCustomerSection').is(':checked'),
        ShowCustomerName: true,
        ShowCustomerAddress: true,
        ShowCustomerPhone: true,
        ShowCustomerEmail: true,
        ShowCustomerGST: true,
        ShowShippingAddress: $('#chkShowShippingAddress').is(':checked'),
        ShowBillingAddress: true,
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Customer"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Customer.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Customer.LabelStyles[key] = {
            Color: $('#label_customer_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Items section
    templateConfig.Items = {
        ShowItemsTable: $('#chkShowItemsTable').is(':checked'),
        ShowItemImages: $('#chkShowItemImages').is(':checked'),
        ShowItemDescription: $('#chkShowItemDescription').is(':checked'),
        ShowItemSKU: true,
        ShowItemQuantity: true,
        ShowItemRate: true,
        ShowItemDiscount: true,
        ShowItemTax: true,
        ShowItemTotal: true,
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Items"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Items.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Items.LabelStyles[key] = {
            Color: $('#label_items_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Summary section
    templateConfig.Summary = {
        ShowSummarySection: $('#chkShowSummarySection').is(':checked'),
        ShowTotalQuantity: true,
        ShowGrossAmount: true,
        ShowTotalDiscount: true,
        ShowTaxBreakdown: $('#chkShowTaxBreakdown').is(':checked'),
        ShowNetAmount: true,
        ShowGrandTotal: true,
        ShowDueAmount: false,
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Summary"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Summary.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Summary.LabelStyles[key] = {
            Color: $('#label_summary_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Footer section
    templateConfig.Footer = {
        ShowFooter: $('#chkShowFooter').is(':checked'),
        ShowFooterNote: $('#chkShowFooterNote').is(':checked'),
        FooterNoteText: 'This is a computer generated invoice. No signature is required.',
        ShowTermsAndConditions: false,
        ShowPaymentInformation: false,
        ShowValidityPeriod: false,
        LabelVisibility: {},
        LabelStyles: {}
    };
    $('.label-visibility-checkbox[data-section="Footer"]').each(function () {
        var key = $(this).data('label-key');
        templateConfig.Footer.LabelVisibility[key] = $(this).is(':checked');
        // Gather label style
        templateConfig.Footer.LabelStyles[key] = {
            Color: $('#label_footer_' + key + '_color').val() || '#1f2937'
        };
    });

    // Gather Print Settings
    templateConfig.PrintSettings = {
        ShowPrintButton: $('#chkShowPrintButton').is(':checked'),
        ShowExportPdfButton: $('#chkShowExportPdfButton').is(':checked'),
        ShowPayNowButton: $('#chkShowPayNowButton').is(':checked'),
        PrintLogo: $('#chkPrintLogo').is(':checked'),
        PrintCompanyDetails: $('#chkPrintCompanyDetails').is(':checked'),
        PrintWatermark: $('#chkPrintWatermark').is(':checked'),
        WatermarkText: $('#txtWatermarkText').val() || '',
        ShowSignature: $('#chkShowSignature').is(':checked'),
        SignatureImageUrl: $('#txtSignatureImageUrl').val() || '',
        PageSize: $('#ddlPageSize').val() || 'A4',
        PageOrientation: $('#ddlPageOrientation').val() || 'Portrait',
        MarginTop: parseFloat($('#txtMarginTop').val()) || 0,
        MarginBottom: parseFloat($('#txtMarginBottom').val()) || 0,
        MarginLeft: parseFloat($('#txtMarginLeft').val()) || 0,
        MarginRight: parseFloat($('#txtMarginRight').val()) || 0,
        ShowFooter: $('#chkShowFooterText').is(':checked'),
        FooterText: $('#txtFooterText').val() || ''
    };

    return templateConfig;
}

function saveTemplate() {
    var templateName = $('#txtTemplateName').val().trim();
    var description = $('#txtTemplateDescription').val().trim();
    // clear previous inline error
    $('#divTemplateName').hide().text('');
    $('#txtTemplateName').css('border', '');

    if (!templateName) {
        $('#divTemplateName').text('This field is required').show();
        $('#txtTemplateName').css('border', '2px solid #dc3545');
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        return;
    }

    var config = gatherTemplateConfig();

    var obj = {
        InvoiceTemplateId: templateId || 0,
        CompanyId: getCookieValue('CompanyId'),
        InvoiceType: invoiceType,
        TemplateName: templateName,
        TemplateKey: templateKey,
        Description: description,
        TemplateConfig: config,
        IsDefault: false,
        AddedBy: getCookieValue('Id'),
        Browser: navigator.userAgent,
        IpAddress: '',
        Platform: navigator.platform
    };

    $("#divLoading").show();

    $.ajax({
        url: '/invoicetemplatesettings/SaveTemplate',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(response.Message);
                if (response.Data && response.Data.InvoiceTemplateId) {
                    templateId = response.Data.InvoiceTemplateId;
                }
                setTimeout(function () {
                    window.location.href = '/invoicetemplatesettings/index';
                }, 1500);
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                // show duplicate / other validation messages inline when possible
                if (response.Message && response.Message.toLowerCase().indexOf('template') >= 0) {
                    $('#divTemplateName').text(response.Message).show();
                    $('#txtTemplateName').css('border', '2px solid #dc3545');
                } else {
                    toastr.error(response.Message);
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            console.error('Error saving template:', xhr);
            toastr.error('Error saving template');
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            $("#divLoading").hide();
        }
    });
}

function saveAndSetDefault() {
    var templateName = $('#txtTemplateName').val().trim();
    var description = $('#txtTemplateDescription').val().trim();
    // clear previous inline error
    $('#divTemplateName').hide().text('');
    $('#txtTemplateName').css('border', '');

    if (!templateName) {
        $('#divTemplateName').text('This field is required').show();
        $('#txtTemplateName').css('border', '2px solid #dc3545');
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        return;
    }

    if (!confirm('Set this template as default for ' + invoiceType + ' invoices?')) {
        return;
    }

    var config = gatherTemplateConfig();

    var obj = {
        InvoiceTemplateId: templateId || 0,
        CompanyId: getCookieValue('CompanyId'),
        InvoiceType: invoiceType,
        TemplateName: templateName,
        TemplateKey: templateKey,
        Description: description,
        TemplateConfig: config,
        IsDefault: true,
        AddedBy: getCookieValue('Id'),
        Browser: navigator.userAgent,
        IpAddress: '',
        Platform: navigator.platform
    };

    $("#divLoading").show();

    $.ajax({
        url: '/invoicetemplatesettings/SaveTemplate',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (response) {
            if (response.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(response.Message);
                if (response.Data && response.Data.InvoiceTemplateId) {
                    templateId = response.Data.InvoiceTemplateId;
                }
                setTimeout(function () {
                    window.location.href = '/invoicetemplatesettings/index';
                }, 1500);
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                // show duplicate / other validation messages inline when possible
                if (response.Message && response.Message.toLowerCase().indexOf('template') >= 0) {
                    $('#divTemplateName').text(response.Message).show();
                    $('#txtTemplateName').css('border', '2px solid #dc3545');
                } else {
                    toastr.error(response.Message);
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            console.error('Error saving template:', xhr);
            toastr.error('Error saving template');
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            $("#divLoading").hide();
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

function removeSignature() {
    if (confirm('Are you sure you want to remove the signature?')) {
        templateConfig.PrintSettings.SignatureImageUrl = '';
        $('#txtSignatureImageUrl').val('');
        $('#fileSignature').val('');
        $('#fileSignatureLabel').text('Choose signature image').removeClass('text-dark').addClass('text-muted');
        $('#btnUploadSignature').prop('disabled', true);
        
        // Remove preview if exists
        var $previewContainer = $('#signaturePreview').closest('.mb-3');
        if ($previewContainer.length) {
            $previewContainer.next('.text-muted').remove(); // Remove "Or upload a new signature image" text
            $previewContainer.remove();
        }
    }
}

