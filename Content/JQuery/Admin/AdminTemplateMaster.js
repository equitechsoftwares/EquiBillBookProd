// Admin Template Master JavaScript Functions

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    // Get form data based on current page
    var formType = getFormType();
    var det = getFormData(formType);
    
    $("#divLoading").show();
    $.ajax({
        url: '/AdminTemplateMaster/' + formType + 'Insert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                    
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                if (i == 1) {
                    window.location.href = getRedirectUrl(formType);
                }
                else {
                    window.location.reload();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    // Get form data based on current page
    var formType = getFormType();
    var det = getFormData(formType);
    
    $("#divLoading").show();
    $.ajax({
        url: '/AdminTemplateMaster/' + formType + 'Update',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                    
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                if (i == 1) {
                    window.location.href = getRedirectUrl(formType);
                }
                else {
                    window.location.reload();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getFormType() {
    var path = window.location.pathname.toLowerCase();
    if (path.includes('predefinedtemplate')) return 'PreDefinedTemplate';
    if (path.includes('color')) return 'Color';
    if (path.includes('font')) return 'Font';
    if (path.includes('printsetting')) return 'PrintSetting';
    if (path.includes('invoicetemplate')) return 'InvoiceTemplate';
    return 'PreDefinedTemplate';
}

function getFormData(formType) {
    var det = {};
    
    switch(formType) {
        case 'PreDefinedTemplate':
            det = {
                InvoiceTemplateMasterId: $('#InvoiceTemplateMasterId').val() || 0,
                InvoiceType: $('#txtInvoiceType').val(),
                TemplateKey: $('#txtTemplateKey').val(),
                TemplateName: $('#txtTemplateName').val(),
                Description: $('#txtDescription').val(),
                PreviewColor: $('#txtPreviewColor').val(),
                Icon: $('#txtIcon').val(),
                PreviewImageUrl: $('#txtPreviewImageUrl').val(),
                TemplateHtmlPath: $('#txtTemplateHtmlPath').val(),
                TemplateConfig: $('#txtTemplateConfig').val(),
                SortOrder: $('#txtSortOrder').val() || 0,
                IsActive: $('#IsActive').is(':checked')
            };
            break;
        case 'Color':
            det = {
                InvoiceTemplateColorMasterId: $('#InvoiceTemplateColorMasterId').val() || 0,
                InvoiceTemplateId: $('#InvoiceTemplateId').val(),
                ColorKey: $('#txtColorKey').val(),
                ColorName: $('#txtColorName').val(),
                DefaultValue: $('#txtDefaultValue').val(),
                Category: $('#txtCategory').val(),
                SortOrder: $('#txtSortOrder').val() || 0,
                Description: $('#txtDescription').val(),
                IsActive: $('#IsActive').is(':checked')
            };
            break;
        case 'Font':
            det = {
                InvoiceTemplateFontMasterId: $('#InvoiceTemplateFontMasterId').val() || 0,
                InvoiceTemplateId: $('#InvoiceTemplateId').val(),
                FontKey: $('#txtFontKey').val(),
                FontName: $('#txtFontName').val(),
                DefaultValue: $('#txtDefaultValue').val(),
                DataType: $('#txtDataType').val(),
                SortOrder: $('#txtSortOrder').val() || 0,
                Description: $('#txtDescription').val(),
                IsActive: $('#IsActive').is(':checked')
            };
            break;
        case 'SectionSetting':
            det = {
                InvoiceTemplateSectionSettingMasterId: $('#InvoiceTemplateSectionSettingMasterId').val() || 0,
                InvoiceTemplateId: $('#InvoiceTemplateId').val(),
                SectionName: $('#txtSectionName').val(),
                SettingKey: $('#txtSettingKey').val(),
                SettingName: $('#txtSettingName').val(),
                DefaultValue: $('#txtDefaultValue').val(),
                DataType: $('#txtDataType').val(),
                SortOrder: $('#txtSortOrder').val() || 0,
                Description: $('#txtDescription').val(),
                IsActive: $('#IsActive').is(':checked')
            };
            break;
        case 'PrintSetting':
            det = {
                InvoiceTemplatePrintSettingMasterId: $('#InvoiceTemplatePrintSettingMasterId').val() || 0,
                InvoiceTemplateId: $('#InvoiceTemplateId').val(),
                SettingKey: $('#txtSettingKey').val(),
                SettingName: $('#txtSettingName').val(),
                DefaultValue: $('#txtDefaultValue').val(),
                DataType: $('#txtDataType').val(),
                SortOrder: $('#txtSortOrder').val() || 0,
                Description: $('#txtDescription').val(),
                IsActive: $('#IsActive').is(':checked')
            };
            break;
        case 'InvoiceTemplate':
            det = {
                InvoiceTemplateId: $('#InvoiceTemplateId').val() || 0,
                CompanyId: $('#txtCompanyId').val(),
                InvoiceType: $('#txtInvoiceType').val(),
                TemplateName: $('#txtTemplateName').val(),
                TemplateKey: $('#txtTemplateKey').val(),
                Description: $('#txtDescription').val(),
                IsDefault: $('#IsDefault').is(':checked'),
                IsActive: $('#IsActive').is(':checked')
            };
            break;
    }
    
    return det;
}

function getRedirectUrl(formType) {
    var invoiceTemplateMasterId = $('#InvoiceTemplateMasterId').val();
    switch(formType) {
        case 'PreDefinedTemplate':
            return '/admintemplatemaster/index';
        case 'Color':
            return '/admintemplatemaster/colors?invoiceTemplateMasterId=' + invoiceTemplateMasterId;
        case 'Font':
            return '/admintemplatemaster/fonts?invoiceTemplateMasterId=' + invoiceTemplateMasterId;
        case 'PrintSetting':
            return '/admintemplatemaster/printsettings?invoiceTemplateMasterId=' + invoiceTemplateMasterId;
        case 'InvoiceTemplate':
            return '/admintemplatemaster/invoicetemplates';
        default:
            return '/admintemplatemaster/index';
    }
}

function getDeleteEndpoint() {
    var path = window.location.pathname.toLowerCase();
    if (path.includes('/admintemplatemaster/index') || (path.includes('/admintemplatemaster') && !path.includes('/colors') && !path.includes('/fonts') && !path.includes('/print') && !path.includes('/section') && !path.includes('/invoice'))) {
        return 'DeletePreDefinedTemplate';
    }
    if (path.includes('/admintemplatemaster/colors')) {
        return 'DeleteColor';
    }
    if (path.includes('/admintemplatemaster/fonts')) {
        return 'DeleteFont';
    }
    if (path.includes('/admintemplatemaster/printsettings')) {
        return 'DeletePrintSetting';
    }
    if (path.includes('/admintemplatemaster/invoicetemplates')) {
        return 'DeleteInvoiceTemplate';
    }
    return 'DeletePreDefinedTemplate'; // default
}

function getAntiForgeryToken() {
    // Try to get token from form
    var token = $('input[name="__RequestVerificationToken"]').val();
    if (!token) {
        // Try to get from meta tag (if available)
        token = $('meta[name="__RequestVerificationToken"]').attr('content');
    }
    if (!token) {
        // Try to get from cookie
        var cookies = document.cookie.split(';');
        for (var i = 0; i < cookies.length; i++) {
            var cookie = cookies[i].trim();
            if (cookie.indexOf('__RequestVerificationToken') === 0) {
                token = cookie.split('=')[1];
                break;
            }
        }
    }
    return token;
}

function ActiveInactive(id, IsActive) {
    // Determine which endpoint to call based on current page
    var path = window.location.pathname.toLowerCase();
    var endpoint = '';
    
    if (path.includes('/admintemplatemaster/index') || (path.includes('/admintemplatemaster') && !path.includes('/colors') && !path.includes('/fonts') && !path.includes('/print') && !path.includes('/section') && !path.includes('/invoice'))) {
        endpoint = 'PreDefinedTemplateActiveInactive';
    }
    else if (path.includes('/admintemplatemaster/invoicetemplates')) {
        endpoint = 'InvoiceTemplateActiveInactive';
    }
    else if (path.includes('/admintemplatemaster/colors')) {
        endpoint = 'ColorActiveInactive';
    }
    else if (path.includes('/admintemplatemaster/fonts')) {
        endpoint = 'FontActiveInactive';
    }
    else if (path.includes('/admintemplatemaster/printsettings')) {
        endpoint = 'PrintSettingActiveInactive';
    }
    else {
        // Default to PreDefinedTemplate
        endpoint = 'PreDefinedTemplateActiveInactive';
    }
    
    var det = {
        id: id,
        isActive: IsActive
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/AdminTemplateMaster/' + endpoint,
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
                // Reload page to revert toggle state
                window.location.reload();
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                // Reload page to reflect the change
                window.location.reload();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            toastr.error('An error occurred while updating the status.');
            // Reload page to revert toggle state
            window.location.reload();
        }
    });
}

function Delete(id, name) {
    var r = confirm("This will delete \"" + name + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var endpoint = getDeleteEndpoint();
        var form = $('<form></form>');
        form.attr('method', 'POST');
        form.attr('action', '/AdminTemplateMaster/' + endpoint + '/' + id);
        form.css('display', 'none');
        
        var token = getAntiForgeryToken();
        if (token) {
            form.append($('<input>').attr('type', 'hidden').attr('name', '__RequestVerificationToken').val(token));
        }
        
        $('body').append(form);
        form.submit();
    }
}

function ActiveInactiveLabelCategory(id, IsActive) {
    var det = {
        id: id,
        isActive: IsActive
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/AdminTemplateMaster/LabelCategoryActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                toastr.success(data.Message);
            } else {
                toastr.error(data.Message);
            }
        },
        error: function () {
            $("#divLoading").hide();
            toastr.error("An error occurred while updating the status.");
            window.location.reload();
        }
    });
}

function ActiveInactiveLabel(id, IsActive) {
    var det = {
        id: id,
        isActive: IsActive
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/AdminTemplateMaster/LabelActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                toastr.success(data.Message);
            } else {
                toastr.error(data.Message);
            }
        },
        error: function () {
            $("#divLoading").hide();
            toastr.error("An error occurred while updating the status.");
            window.location.reload();
        }
    });
}

