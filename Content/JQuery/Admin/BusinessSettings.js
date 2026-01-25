$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('aBusinessSetting').split('&')[0].split('=')[1];
    $('#_StartDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_StartDate').addClass('notranslate');

    $('.select2').select2();

    $('textarea#txtPurchaseTerms,#txtPurchaseReturnTerms,#txtSalesTerms,#txtSalesReturnTerms,#txtDraftTerms,#txtQuotationTerms,#txtProformaInvoiceTerms').summernote({
        placeholder: '',
        followingToolbar: false,
        tabsize: 2,
        height: 200,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            ['fontname', ['fontname']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            ['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });
});

var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var ProfilePic = '', fileExtensionProfilePic = '', Favicon = '', fileExtensionFavicon = '',
    BusinessIcon = '', fileExtensionIcon = '',    _OnlinePaymentSettingsId = 0,
    _EmailSettingsId = 0, _SmsSettingsId = 0, _WhatsappSettingsId = 0, Image_48_48 = '', FileExtensionImage_48_48 = '', 
    Image_72_72 = '', FileExtensionImage_72_72 = '', Image_96_96 = '', FileExtensionImage_96_96 = '', Image_128_128 = '', FileExtensionImage_128_128 = '',
    Image_144_144 = '', FileExtensionImage_144_144 = '', Image_152_152 = '', FileExtensionImage_152_152 = '', Image_192_192 = '', FileExtensionImage_192_192 = '',
    Image_284_284 = '', FileExtensionImage_284_284 = '', Image_512_512 = '', FileExtensionImage_512_512 = '';

function businessInformationUpdate() {
    $('.errorText').hide();
    var det = {
        BusinessName: $('#txtBusinessName').val(),
        WebsiteUrl: $('#txtWebsiteUrl').val(),
        StartDate: $('#txtStartDate').val(),
        CountryId: $('#ddlCountry').val(),
        CurrencySymbolPlacement: $('#ddlCurrencySymbolPlacement').val(),
        FinancialYearStartMonth: $('#ddlFinancialYearStartMonth').val(),
        TransactionEditDays: $('#ddlTransactionEditDays').val(),
        DateFormat: $('#ddlDateFormat').val(),
        TimeFormat: $('#ddlTimeFormat').val(),
        BusinessLogo: ProfilePic,
        FileExtension: fileExtensionProfilePic,
        EnableDaylightSavingTime: $('#chkEnableDaylightSavingTime').is(':checked'),
        TimeZoneId: $('#ddlTimeZone').val(),
        IndustryTypeId: $('#ddlIndustryType').val(),
        BusinessTypes: $('#ddlBusinessType').val(),
        EnableDefaultSmsBranding: $('#chkEnableDefaultSmsBranding').is(':checked'),
        EnableDefaultEmailBranding: $('#chkEnableDefaultEmailBranding').is(':checked'),
        Favicon: Favicon,
        FileExtensionFavicon: fileExtensionFavicon,
        BusinessIcon: BusinessIcon,
        FileExtensionIcon: fileExtensionIcon,
        ParentBusinessName: $('#txtParentBusinessName').val(),
        ParentWebsiteUrl: $('#txtParentWebsiteUrl').val(),
        FreeTrialDays: $('#txtFreeTrialDays').val(),
        ResellerTermsUrl: $('#txtResellerTermsUrl').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/businesssettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getProfilePicBase64() {
    var file1 = $("#ProfilePic").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProfilePic").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            ProfilePic = reader.result;
            fileExtensionProfilePic = '.' + file1.name.split('.').pop();

            $('#blahProfilePic').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function getFaviconBase64() {
    var file1 = $("#Favicon").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#Favicon").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            Favicon = reader.result;
            fileExtensionFavicon = '.' + file1.name.split('.').pop();

            $('#blahFavicon').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function getBusinessIconBase64() {
    var file1 = $("#BusinessIcon").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#BusinessIcon").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            BusinessIcon = reader.result;
            fileExtensionIcon = '.' + file1.name.split('.').pop();

            $('#blahBusinessIcon').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function sendTestSms(SmsSettingsId) {
    var det = {
        SmsSettingsId: SmsSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/sendTestSms',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function emailModulesUpdate() {
    var NotificationModulesSettings = [];
    $('#divNotificationModules tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            NotificationModulesSettings.push({
                NotificationModulesId: _id,
                IsActive: $('#chkNotificationModule' + _id).is(':checked'),
                NotificationModulesSettingsId: $('#hdnNotificationModulesSettingsId' + _id).val(),
            })
        }
    });

    var det = {
        NotificationModulesSettings: NotificationModulesSettings,
        Type: 1
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/EmailModulesUpdate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function updateSystem() {
    var det = {
        DatatablePageEntries: $('#ddlDatatablePageEntries').val(),
        ShowHelpText: $('#chkShowHelpText').is(':checked'),
        EnableDarkMode: $('#chkEnableDarkMode').is(':checked'),
        FixedHeader: $('#chkFixedHeader').is(':checked'),
        FixedFooter: $('#chkFixedFooter').is(':checked'),
        EnableSound: $('#chkEnableSound').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/SystemUpdate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if ($('#chkEnableDarkMode').is(':checked')) {
                    $('body').addClass('dark-mode');
                    $('.divShortcutKeys').removeClass('control-sidebar-light');
                    $('.divShortcutKeys').addClass('control-sidebar-dark');
                }
                else {
                    $('body').removeClass('dark-mode');
                    $('.divShortcutKeys').addClass('control-sidebar-light');
                    $('.divShortcutKeys').removeClass('control-sidebar-dark');
                }

                if ($('#chkFixedHeader').is(':checked')) {
                    $('body').addClass('layout-navbar-fixed');
                }
                else {
                    $('body').removeClass('layout-navbar-fixed');
                }

                if ($('#chkFixedFooter').is(':checked')) {
                    $('body').addClass('layout-footer-fixed');
                }
                else {
                    $('body').removeClass('layout-footer-fixed');
                }

                if ($('#chkEnableSound').is(':checked')) {
                    EnableSound = 'True';
                }
                else {
                    EnableSound = 'False';
                }

                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function selectAll() {
    if ($('#chkSelectAll').is(':checked')) {
        $('.chkNotificationModule').prop('checked', true);
    } else {
        $('.chkNotificationModule').prop('checked', false);
    }
}

function sendTestEmail(EmailSettingsId) {
    var det = {
        EmailSettingsId: EmailSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/sendTestEmail',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleSmsService(r) {
    $('.divTwilio').hide();
    $('.divNexmo').hide();
    $('.divOthers').hide();
    if ($('#ddlSmsService' + r).val() == 1) {
        $('.divTwilio').show();
    }
    if ($('#ddlSmsService' + r).val() == 2) {
        $('.divNexmo').show();
    }
    if ($('#ddlSmsService' + r).val() == 3) {
        $('.divOthers').show();
        toggleRequestMethod(r);
    }
    $('#ddlRequestMethod' + r).val(1);
    clearSmsSettings();
}

function toggleRequestMethod(r) {
    if ($('#ddlRequestMethod' + r).val() == 2) {
        $('.divHeader').show();
    }
    else {
        $('.divHeader').hide();
    }
    clearSmsSettings();
}

function toggleWhatsappService(r) {
    if ($('#ddlWhatsappService' + r).val() == 1) {
        $('.divWTwilio').hide();
        $('.divDesktop').show();
        $('.divWNodeJS').hide();
        $('#txtWTwilioAccountSID' + r).val('');
        $('#txtWTwilioAuthToken' + r).val('');
        $('#txtWTwilioFrom' + r).val('');
    }
    else if ($('#ddlWhatsappService' + r).val() == 2) {
        $('.divWTwilio').show();
        $('.divDesktop').hide();
        $('.divWNodeJS').hide();
    }
    else if ($('#ddlWhatsappService' + r).val() == 3) {
        // Node.js service - show QR code section
        $('.divWTwilio').hide();
        $('.divDesktop').hide();
        $('.divWNodeJS').show();
        // Fetch QR code when this option is selected
        if (typeof fetchQRCode_E === 'function') {
            setTimeout(fetchQRCode_E, 500);
        }
    }
    else {
        $('.divWTwilio').hide();
        $('.divDesktop').hide();
        $('.divWNodeJS').hide();
    }
}

function sendTestWhatsapp(WhatsappSettingsId) {
    var det = {
        WhatsappSettingsId: WhatsappSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/sendTestWhatsapp',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (data.Status == 1) {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                        window.open(data.WhatsappUrl);
                    }
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchOnlinePaymentSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/OnlinePaymentSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOnlinePayments").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function OnlinePaymentSetting(OnlinePaymentSettingsId) {
    _OnlinePaymentSettingsId = OnlinePaymentSettingsId;
    var det = {
        OnlinePaymentSettingsId: OnlinePaymentSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/OnlinePaymentSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOnlinePaymentsEdit").html(data);
            $("#divLoading").hide();
            toggleOnlinePayment(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertOnlinePaymentSettings() {
    $('.errorText').hide();
    var det = {
        OnlinePaymentService: $('#ddlOnlinePaymentService').val(),
        PaypalClientId: $('#txtPaypalClientId').val(),
        PaypalCurrencyId: $('#ddlPaypalCurrency').val(),
        RazorpayKey: $('#txtRazorpayKey').val(),
        RazorpayCurrencyId: $('#ddlRazorpayCurrency').val(),
        IsDefault: $('#chkPaymentServiceIsDefault').is(':checked'),
        SaveAs: $('#txtPaymentServiceSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/InsertOnlinePaymentSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleOnlinePayment(2);
                fetchOnlinePaymentSettingsList();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateOnlinePaymentSettings() {
    $('.errorText').hide();
    var det = {
        OnlinePaymentSettingsId: _OnlinePaymentSettingsId,
        OnlinePaymentService: $('#ddlOnlinePaymentService_E').val(),
        PaypalClientId: $('#txtPaypalClientId_E').val(),
        PaypalCurrencyId: $('#ddlPaypalCurrency_E').val(),
        RazorpayKey: $('#txtRazorpayKey_E').val(),
        RazorpayCurrencyId: $('#ddlRazorpayCurrency_E').val(),
        IsDefault: $('#chkPaymentServiceIsDefault_E').is(':checked'),
        SaveAs: $('#txtPaymentServiceSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateOnlinePaymentSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleOnlinePayment(2);
                fetchOnlinePaymentSettingsList();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearOnlinePaymentSettings() {
    $('#txtPaypalClientId').val('');
    $('#ddlPaypalCurrency').val(0);
    $('#txtRazorpayKey').val('');
    $('#ddlRazorpayCurrency').val(0);
    $('.select2').select2();
}

function toggleOnlinePaymentService() {
    $('.divRazorpay').hide();
    $('.divPaypal').hide();

    if ($('#ddlOnlinePaymentService').val() == 1) {
        $('.divPaypal').show();
    }
    else if ($('#ddlOnlinePaymentService').val() == 2) {
        $('.divRazorpay').show();
    }
    clearOnlinePaymentSettings();
}

function toggleOnlinePayment(t) {
    if (t == 1) {
        $('.divOnlinePayments').hide();
        $('.divOnlinePaymentsNew').show();
        $('.divOnlinePaymentsEdit').hide();
        toggleOnlinePaymentService();
    }
    else if (t == 2) {
        $('.divOnlinePayments').show();
        $('.divOnlinePaymentsNew').hide();
        $('.divOnlinePaymentsEdit').hide();
        toggleOnlinePaymentService();
    }
    else if (t == 3) {
        $('.divOnlinePayments').hide();
        $('.divOnlinePaymentsNew').hide();
        $('.divOnlinePaymentsEdit').show();
    }
}

function DeleteOnlinePayment(OnlinePaymentSettingsId, OnlinePaymentService) {
    var r = confirm("This will delete \"" + OnlinePaymentService +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            OnlinePaymentSettingsId: OnlinePaymentSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/OnlinePaymentSettingsDelete',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }
                else if (data == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }

                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchOnlinePaymentSettingsList();
                }
                
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function TimeZone() {
    var det = {
        TimeZoneId: $('#ddlTimeZone').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/TimeZone',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (data.Data.TimeZone.SupportsDaylightSavingTime == true) {
                    $('.divEnableDaylightSavingTime').show();
                    $('#chkEnableDaylightSavingTime').prop('checked', true);
                }
                else {
                    $('.divEnableDaylightSavingTime').hide();
                    $('#chkEnableDaylightSavingTime').prop('checked', false);
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function CheckTime() {
    var det = {
        TimeZoneId: $('#ddlTimeZone').val(),
        EnableDaylightSavingTime: $('#chkEnableDaylightSavingTime').is(':checked'),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/adminsettings/CheckTime',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                alert(data.Message);
            }
            
        },
        error: function (xhr) {
            
        }
    });
};

function fetchEmailSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/EmailSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divEmailSettings").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function EmailSetting(EmailSettingsId) {
    _EmailSettingsId = EmailSettingsId;
    var det = {
        EmailSettingsId: EmailSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/EmailSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divEmailSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleEmailSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertEmailSettings() {
    $('.errorText').hide();
    var det = {
        SmtpServer: $('#txtSmtpServer').val(),
        SmtpPort: $('#txtSmtpPort').val(),
        SmtpUser: $('#txtSmtpUser').val(),
        SmtpPass: $('#txtSmtpPass').val(),
        EnableSsl: $('#chkEnableSsl').is(':checked'),
        FromName: $('#txtFromName').val(),
        TestEmailId: $('#txtTestEmailId').val(),
        IsDefault: $('#chkEmailIsDefault').is(':checked'),
        SaveAs: $('#txtEmailSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/InsertEmailSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleEmailSetting(2);
                fetchEmailSettingsList();
                clearEmailSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateEmailSettings() {
    $('.errorText').hide();
    var det = {
        EmailSettingsId: _EmailSettingsId,
        SmtpServer: $('#txtSmtpServer_E').val(),
        SmtpPort: $('#txtSmtpPort_E').val(),
        SmtpUser: $('#txtSmtpUser_E').val(),
        SmtpPass: $('#txtSmtpPass_E').val(),
        EnableSsl: $('#chkEnableSsl_E').is(':checked'),
        FromName: $('#txtFromName_E').val(),
        TestEmailId: $('#txtTestEmailId_E').val(),
        IsDefault: $('#chkEmailIsDefault_E').is(':checked'),
        SaveAs: $('#txtEmailSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateEmailSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleEmailSetting(2);
                fetchEmailSettingsList();
                clearEmailSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearEmailSettings() {
    $('#txtSmtpServer').val('');
    $('#txtSmtpPort').val('');
    $('#txtSmtpUser').val('');
    $('#txtSmtpPass').val('');
    $('#chkEnableSsl').prop('checked', false);
    $('#txtFromName').val('');
    $('#txtTestEmailId').val('');
    $('#txtEmailSaveAs').val('');
    $('#chkIsDefault').prop('checked', false);
    $('.select2').select2();
}

function toggleEmailSetting(t) {
    if (t == 1) {
        $('.divEmailSettings').hide();
        $('.divEmailSettingsNew').show();
        $('.divEmailSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divEmailSettings').show();
        $('.divEmailSettingsNew').hide();
        $('.divEmailSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divEmailSettings').hide();
        $('.divEmailSettingsNew').hide();
        $('.divEmailSettingsEdit').show();
    }
}

function DeleteEmailSetting(EmailSettingsId, SaveAs) {
    var r = confirm("This will delete \"" + SaveAs +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            EmailSettingsId: EmailSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/EmailSettingsDelete',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }
                else if (data == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }

                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchEmailSettingsList();
                }
                
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function fetchSmsSettingsList() {
    var det = {

    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/adminsettings/smssettingsfetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divSmsSettings").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function SmsSetting(SmsSettingsId) {
    _SmsSettingsId = SmsSettingsId;
    var det = {
        SmsSettingsId: SmsSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/SmsSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divSmsSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleSmsSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertSmsSettings() {
    $('.errorText').hide();
    var det = {
        SmsService: $('#ddlSmsService').val(),
        RequestMethod: $('#ddlRequestMethod').val(),
        Url: $('#txtUrl').val(),
        SendToParameterName: $('#txtSendToParameterName').val(),
        MessageParameterName: $('#txtMessageParameterName').val(),
        Header1Key: $('#txtHeader1Key').val(),
        Header1Value: $('#txtHeader1Value').val(),
        Header2Key: $('#txtHeader2Key').val(),
        Header2Value: $('#txtHeader2Value').val(),
        Header3Key: $('#txtHeader3Key').val(),
        Header3Value: $('#txtHeader3Value').val(),
        Header4Key: $('#txtHeader4Key').val(),
        Header4Value: $('#txtHeader4Value').val(),
        Parameter1Key: $('#txtParameter1Key').val(),
        Parameter1Value: $('#txtParameter1Value').val(),
        Parameter2Key: $('#txtParameter2Key').val(),
        Parameter2Value: $('#txtParameter2Value').val(),
        Parameter3Key: $('#txtParameter3Key').val(),
        Parameter3Value: $('#txtParameter3Value').val(),
        Parameter4Key: $('#txtParameter4Key').val(),
        Parameter4Value: $('#txtParameter4Value').val(),
        Parameter5Key: $('#txtParameter5Key').val(),
        Parameter5Value: $('#txtParameter5Value').val(),
        Parameter6Key: $('#txtParameter6Key').val(),
        Parameter6Value: $('#txtParameter6Value').val(),
        Parameter7Key: $('#txtParameter7Key').val(),
        Parameter7Value: $('#txtParameter7Value').val(),
        Parameter8Key: $('#txtParameter8Key').val(),
        Parameter8Value: $('#txtParameter8Value').val(),
        Parameter9Key: $('#txtParameter9Key').val(),
        Parameter9Value: $('#txtParameter9Value').val(),
        Parameter10Key: $('#txtParameter10Key').val(),
        Parameter10Value: $('#txtParameter10Value').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        TwilioAccountSID: $('#txtTwilioAccountSID').val(),
        TwilioAuthToken: $('#txtTwilioAuthToken').val(),
        TwilioFrom: $('#txtTwilioFrom').val(),
        NexmoApiKey: $('#txtNexmoApiKey').val(),
        NexmoApiSecret: $('#txtNexmoApiSecret').val(),
        NexmoFrom: $('#txtNexmoFrom').val(),
        EnableCountryCode: $('#chkEnableCountryCode').is(':checked'),
        TestMobileNo: $('#txtTestMobileNo').val(),
        IsDefault: $('#chkSmsIsDefault').is(':checked'),
        SaveAs: $('#txtSmsSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/InsertSmsSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleSmsSetting(2);
                fetchSmsSettingsList();
                clearSmsSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateSmsSettings() {
    $('.errorText').hide();
    var det = {
        SmsSettingsId: _SmsSettingsId,
        SmsService: $('#ddlSmsService_E').val(),
        RequestMethod: $('#ddlRequestMethod_E').val(),
        Url: $('#txtUrl_E').val(),
        SendToParameterName: $('#txtSendToParameterName_E').val(),
        MessageParameterName: $('#txtMessageParameterName_E').val(),
        Header1Key: $('#txtHeader1Key_E').val(),
        Header1Value: $('#txtHeader1Value_E').val(),
        Header2Key: $('#txtHeader2Key_E').val(),
        Header2Value: $('#txtHeader2Value_E').val(),
        Header3Key: $('#txtHeader3Key_E').val(),
        Header3Value: $('#txtHeader3Value_E').val(),
        Header4Key: $('#txtHeader4Key_E').val(),
        Header4Value: $('#txtHeader4Value_E').val(),
        Parameter1Key: $('#txtParameter1Key_E').val(),
        Parameter1Value: $('#txtParameter1Value_E').val(),
        Parameter2Key: $('#txtParameter2Key_E').val(),
        Parameter2Value: $('#txtParameter2Value_E').val(),
        Parameter3Key: $('#txtParameter3Key_E').val(),
        Parameter3Value: $('#txtParameter3Value_E').val(),
        Parameter4Key: $('#txtParameter4Key_E').val(),
        Parameter4Value: $('#txtParameter4Value_E').val(),
        Parameter5Key: $('#txtParameter5Key_E').val(),
        Parameter5Value: $('#txtParameter5Value_E').val(),
        Parameter6Key: $('#txtParameter6Key_E').val(),
        Parameter6Value: $('#txtParameter6Value_E').val(),
        Parameter7Key: $('#txtParameter7Key_E').val(),
        Parameter7Value: $('#txtParameter7Value_E').val(),
        Parameter8Key: $('#txtParameter8Key_E').val(),
        Parameter8Value: $('#txtParameter8Value_E').val(),
        Parameter9Key: $('#txtParameter9Key').val(),
        Parameter9Value: $('#txtParameter9Value_E').val(),
        Parameter10Key: $('#txtParameter10Key_E').val(),
        Parameter10Value: $('#txtParameter10Value_E').val(),
        IsActive: $('#chkIsActive_E').is(':checked'),
        TwilioAccountSID: $('#txtTwilioAccountSID_E').val(),
        TwilioAuthToken: $('#txtTwilioAuthToken_E').val(),
        TwilioFrom: $('#txtTwilioFrom_E').val(),
        NexmoApiKey: $('#txtNexmoApiKey_E').val(),
        NexmoApiSecret: $('#txtNexmoApiSecret_E').val(),
        NexmoFrom: $('#txtNexmoFrom_E').val(),
        EnableCountryCode: $('#chkEnableCountryCode_E').is(':checked'),
        TestMobileNo: $('#txtTestMobileNo_E').val(),
        IsDefault: $('#chkSmsIsDefault_E').is(':checked'),
        SaveAs: $('#txtSmsSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateSmsSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleSmsSetting(2);
                fetchSmsSettingsList();
                clearSmsSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearSmsSettings() {
    $('#txtUrl').val("");
    $('#txtSendToParameterName').val("");
    $('#txtMessageParameterName').val("");
    $('#txtHeader1Key').val("");
    $('#txtHeader1Value').val("");
    $('#txtHeader2Key').val("");
    $('#txtHeader2Value').val("");
    $('#txtHeader3Key').val("");
    $('#txtHeader3Value').val("");
    $('#txtHeader4Key').val("");
    $('#txtHeader4Value').val("");
    $('#txtParameter1Key').val("");
    $('#txtParameter1Value').val("");
    $('#txtParameter2Key').val("");
    $('#txtParameter2Value').val("");
    $('#txtParameter3Key').val("");
    $('#txtParameter3Value').val("");
    $('#txtParameter4Key').val("");
    $('#txtParameter4Value').val("");
    $('#txtParameter5Key').val("");
    $('#txtParameter5Value').val("");
    $('#txtParameter6Key').val("");
    $('#txtParameter6Value').val("");
    $('#txtParameter7Key').val("");
    $('#txtParameter7Value').val("");
    $('#txtParameter8Key').val("");
    $('#txtParameter8Value').val("");
    $('#txtParameter9Key').val("");
    $('#txtParameter9Value').val("");
    $('#txtParameter10Key').val("");
    $('#txtParameter10Value').val("");
    $('#txtTwilioAccountSID').val("");
    $('#txtTwilioAuthToken').val("");
    $('#txtTwilioFrom').val("");
    $('#txtNexmoApiKey').val("");
    $('#txtNexmoApiSecret').val("");
    $('#txtNexmoFrom').val("");
}

function toggleSmsSetting(t) {
    if (t == 1) {
        $('.divSmsSettings').hide();
        $('.divSmsSettingsNew').show();
        $('.divSmsSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divSmsSettings').show();
        $('.divSmsSettingsNew').hide();
        $('.divSmsSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divSmsSettings').hide();
        $('.divSmsSettingsNew').hide();
        $('.divSmsSettingsEdit').show();
    }
}

function DeleteSmsSetting(SmsSettingsId, SaveAs) {
    var r = confirm("This will delete \"" + SaveAs +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SmsSettingsId: SmsSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/SmsSettingsDelete',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }
                else if (data == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }

                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchSmsSettingsList();
                }
                
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function toggleSmsBranding() {
    businessInformationUpdate();
    if ($('#chkEnableDefaultSmsBranding').is(':checked')) {
        $('.divCustomSmsBranding').hide();
    }
    else {
        $('.divCustomSmsBranding').show();
    }
}

function toggleEmailBranding() {
    businessInformationUpdate();
    if ($('#chkEnableDefaultEmailBranding').is(':checked')) {
        $('.divCustomEmailBranding').hide();
    }
    else {
        $('.divCustomEmailBranding').show();
    }
}

function fetchWhatsappSettingsList() {
    var det = {

    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/adminsettings/WhatsappSettingsfetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divWhatsappSettings").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function WhatsappSetting(WhatsappSettingsId) {
    _WhatsappSettingsId = WhatsappSettingsId;
    var det = {
        WhatsappSettingsId: WhatsappSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/WhatsappSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divWhatsappSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleWhatsappSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertWhatsappSettings() {
    $('.errorText').hide();
    var det = {
        WhatsappService: $('#ddlWhatsappService').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        TwilioAccountSID: $('#txtWTwilioAccountSID').val(),
        TwilioAuthToken: $('#txtWTwilioAuthToken').val(),
        TwilioFrom: $('#txtWTwilioFrom').val(),
        TestMobileNo: $('#txtWTestMobileNo').val(),
        IsDefault: $('#chkWhatsappIsDefault').is(':checked'),
        SaveAs: $('#txtWhatsappSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/InsertWhatsappSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleWhatsappSetting(2);
                fetchWhatsappSettingsList();
                clearWhatsappSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateWhatsappSettings() {
    $('.errorText').hide();
    var det = {
        WhatsappSettingsId: _WhatsappSettingsId,
        WhatsappService: $('#ddlWhatsappService_E').val(),
        IsActive: $('#chkIsActive_E').is(':checked'),
        TwilioAccountSID: $('#txtWTwilioAccountSID_E').val(),
        TwilioAuthToken: $('#txtWTwilioAuthToken_E').val(),
        TwilioFrom: $('#txtWTwilioFrom_E').val(),
        TestMobileNo: $('#txtWTestMobileNo_E').val(),
        IsDefault: $('#chkWhatsappIsDefault_E').is(':checked'),
        SaveAs: $('#txtWhatsappSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateWhatsappSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleWhatsappSetting(2);
                fetchWhatsappSettingsList();
                clearWhatsappSettings();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearWhatsappSettings() {
    $('#ddlWhatsappService').val(0);
    $('#txtWTestMobileNo').val("");
    $('#txtWTwilioAccountSID').val("");
    $('#txtWTwilioAuthToken').val("");
    $('#txtWTwilioFrom').val("");
    $('#txtWhatsappSaveAs').val("");
    $('#chkWhatsappIsDefault').prop("checked", false);
    $('.select2').select2();
    toggleWhatsappService('');
}

function toggleWhatsappSetting(t) {
    if (t == 1) {
        $('.divWhatsappSettings').hide();
        $('.divWhatsappSettingsNew').show();
        $('.divWhatsappSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divWhatsappSettings').show();
        $('.divWhatsappSettingsNew').hide();
        $('.divWhatsappSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divWhatsappSettings').hide();
        $('.divWhatsappSettingsNew').hide();
        $('.divWhatsappSettingsEdit').show();
    }
}

function DeleteWhatsappSetting(WhatsappSettingsId, SaveAs) {
    var r = confirm("This will delete \"" + SaveAs +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            WhatsappSettingsId: WhatsappSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/WhatsappSettingsDelete',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }
                else if (data == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                    return
                }

                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchWhatsappSettingsList();
                }
                
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function insertCurrency(i) {
    $('.errorText').hide();
    var det = {
        CurrencyId: $('#ddlCurrency').val(),
        ExchangeRate: 1,//$('#txtExchangeRate').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/currencyMappingInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                $('#ddlPaypalCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode+')' }));
                $('#ddlPaypalCurrency').val(data.Data.Currency.CurrencyId);

                $('#ddlRazorpayCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlRazorpayCurrency').val(data.Data.Currency.CurrencyId);

                $('#ddlPaypalCurrency_E').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlPaypalCurrency_E').val(data.Data.Currency.CurrencyId);

                $('#ddlRazorpayCurrency_E').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlRazorpayCurrency_E').val(data.Data.Currency.CurrencyId);

                $('#currencyModal').modal('toggle');

                $('#ddlCurrency').val(0);
                $('#txtExchangeRate').val('');

                //FetchTaxPercent();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function CountrySelectAll() {
    if ($('#chkCountrySelectAll').is(':checked')) {
        $('.chkIsCountry').prop('checked', true);
    } else {
        $('.chkIsCountry').prop('checked', false);
    }
}

function UpdateCountryMapped() {
    $('.errorText').hide();

    var UserCountryMaps = [];

    $('#divCountrys tr').each(function () {
        var _id = this.id;
        if ($('#chkIsCountry' + _id).is(':checked')) {
            UserCountryMaps.push({
                CountryId: $('#chkIsCountry' + _id).val(),
                PriceHikePercentage: $('#txtPriceHikePercentage' + _id).val(),
                UserCountryMapId: $('#hdnUserCountryMapId' + _id).val(),
                DivId:_id
            })
        }
    });

    var det = {
        UserCountryMaps: UserCountryMaps
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateCountryMapped',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveStates() {
    var det = {
        CountryId: $('#ddlCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminSettings/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_States_Dropdown").html(data);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveCitys() {
    var det = {
        StateId: $('#ddlState').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminSettings/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_Citys_Dropdown").html(data);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertState() {
    $('.errorText').hide();
    var det = {
        CountryId: $('#ddlCountry').val(),
        StateCode: $('#txtStateCode').val(),
        State: $('#txtState').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminSettings/StateInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);
                });
            }
            else {
                $('#ddlState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#ddlState').val(data.Data.State.StateId);
                $('#stateModal').modal('toggle');

                $('#ddlCity').html('');

                $('#ddlNewCountry').val($("#ddlNewCountry option:first").val());
                $('#txtStateCode').val('');
                $('#txtState').val('');
                $('.select2').select2();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertCity() {
    $('.errorText').hide();
    var det = {
        CountryId: $('#ddlCountry').val(),
        //StateId: $('#ddlCityModalState').val(),
        StateId: $('#ddlState').val(),
        CityCode: $('#txtCityCode').val(),
        City: $('#txtCity').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminSettings/CityInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);
                });
            }
            else {
                $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                $('#ddlCity').val(data.Data.City.CityId);
                $('#cityModal').modal('toggle');

                $('#ddlCityModalCountry').val($("#ddlCityModalCountry option:first").val());
                $('#ddlCityModalState').html('');
                $('#txtCityCode').val('');
                $('#txtCity').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openCityModal() {
    if ($('#ddlState').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select State first');
    }
    else {
        $('#cityModal').modal('toggle');
    }
}

function openStateModal() {
    if ($('#ddlCountry').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Country first');
    }
    else {
        $('#stateModal').modal('toggle');
    }
}

function UpdateBranch() {
    $('.errorText').hide();
    var det = {
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        BranchCode: $('#txtBranchCode').val(),
        Branch: $('#txtBranch').val(),
        ContactName: $('#txtContactName').val(),
        Mobile: $('#txtMobile').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        Email: $('#txtEmail').val(),
        Zipcode: $('#txtZipcode').val(),
        CityId: $('#ddlCity').val(),
        BranchId: window.location.href.split('=')[1],
        Address: $('#txtAddress').val(),
        CurrencyId: $('#ddlCurrency').val(),
        PaymentTypes: $('#ddlPaymentType').val() == null ? [] : $('#ddlPaymentType').val(),
        ContactPersonId: $('#ddlContactPerson').val(),
        TaxId: $('#ddlTax').val(),
        TaxNo: $('#txtTaxNo').val(),
        EnableInlineTax: $('#chkEnableInlineTax').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateBranch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function SameForAll(c) {
    if ($('#chkIsSameForAll' + c).is(':checked') == true) {
        if ($('#txtPriceHikePercentage').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Invalid inputs, check and try again !!');

            $('#' + res.Id).show();
            $('#' + res.Id).text(res.Message);
            return
        }
        var PriceHikePercentage = $('#txtPriceHikePercentage' + c).val();
        $('#divCountrys tr').each(function () {
            var _id = this.id;
            if ($('#chkIsCountry' + _id).is(':checked')) {
                $('#txtPriceHikePercentage' + _id).val(PriceHikePercentage);
            }
        });
    }
}

function getPwaImageBase64(imageType) {
    var file1 = $("#" + imageType).prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProfilePic").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            if (imageType == 'Image_48_48') {
                Image_48_48 = reader.result;
                FileExtensionImage_48_48 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_72_72') {
                Image_72_72 = reader.result;
                FileExtensionImage_72_72 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_96_96') {
                Image_96_96 = reader.result;
                FileExtensionImage_96_96 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_128_128') {
                Image_128_128 = reader.result;
                FileExtensionImage_128_128 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_144_144') {
                Image_144_144 = reader.result;
                FileExtensionImage_144_144 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_152_152') {
                Image_152_152 = reader.result;
                FileExtensionImage_152_152 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_192_192') {
                Image_192_192 = reader.result;
                FileExtensionImage_192_192 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_284_284') {
                Image_284_284 = reader.result;
                FileExtensionImage_284_284 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_512_512') {
                Image_512_512 = reader.result;
                FileExtensionImage_512_512 = '.' + file1.name.split('.').pop();
            }

            $('#blah' + imageType).attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function fetchPwaSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/PwaSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPwaSettings").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function PwaSetting(PwaSettingsId) {
    _PwaSettingsId = PwaSettingsId;
    var det = {
        PwaSettingsId: PwaSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/PwaSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPwaSettingsEdit").html(data);
            $("#divLoading").hide();
            togglePwaSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function updatePwaSettings() {
    $('.errorText').hide();
    var det = {
        PwaSettingsId: _PwaSettingsId,
        PwaName: $('#txtPwaName').val(),
        PwaShortName: $('#txtPwaShortName').val(),
        BackgroundColor: $('#txtBackgroundColor').val(),
        ThemeColor: $('#txtThemeColor').val(),
        Image_48_48: Image_48_48,
        FileExtensionImage_48_48: FileExtensionImage_48_48,
        Image_72_72: Image_72_72,
        FileExtensionImage_72_72: FileExtensionImage_72_72,
        Image_96_96: Image_96_96,
        FileExtensionImage_96_96: FileExtensionImage_96_96,
        Image_128_128: Image_128_128,
        FileExtensionImage_128_128: FileExtensionImage_128_128,
        Image_144_144: Image_144_144,
        FileExtensionImage_144_144: FileExtensionImage_144_144,
        Image_152_152: Image_152_152,
        FileExtensionImage_152_152: FileExtensionImage_152_152,
        Image_192_192: Image_192_192,
        FileExtensionImage_192_192: FileExtensionImage_192_192,
        Image_284_284: Image_284_284,
        FileExtensionImage_284_284: FileExtensionImage_284_284,
        Image_512_512: Image_512_512,
        FileExtensionImage_512_512: FileExtensionImage_512_512,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdatePwaSettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
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
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                togglePwaSetting(2);
                fetchPwaSettingsList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function togglePwaSetting(t) {
    if (t == 1) {
        $('.divPwaSettings').hide();
        $('.divPwaSettingsNew').show();
        $('.divPwaSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divPwaSettings').show();
        $('.divPwaSettingsNew').hide();
        $('.divPwaSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divPwaSettings').hide();
        $('.divPwaSettingsNew').hide();
        $('.divPwaSettingsEdit').show();
    }
}