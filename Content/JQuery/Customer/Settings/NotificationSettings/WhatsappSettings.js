$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

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

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/WhatsappSettingsfetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            $('.chkIsActive').bootstrapToggle();
            $("#divLoading").hide();

            $('#tblData').DataTable({
                lengthChange: false,
                searching: false,
                autoWidth: false,
                responsive: false,
                paging: false,
                bInfo: false,
                "bDestroy": true
            });
            $("#thead").insertBefore(".table-body");
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

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
        url: '/notificationsettings/WhatsappSettingInsert',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/notificationsettings/whatsappsettings";
                }
                else {
                    window.location.href = "/notificationsettings/whatsappsettingadd";
                }
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        WhatsappSettingsId: window.location.href.split('=')[1],
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
        url: '/notificationsettings/WhatsappSettingUpdate',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                if (i == 1) {
                    window.location.href = "/notificationsettings/whatsappsettings";
                }
                else {
                    window.location.href = "/notificationsettings/whatsappsettingadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(WhatsappSettingsId, IsActive) {
    var det = {
        WhatsappSettingsId: WhatsappSettingsId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/WhatsappSettingActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(WhatsappSettingsId, SaveAs) {
    var r = confirm("This will delete \"" + SaveAs + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            WhatsappSettingsId: WhatsappSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/notificationsettings/WhatsappSettingDelete',
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
                    fetchList();
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function toggleWhatsappService() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if ($('#ddlWhatsappService').val() == 1) {
        $('.divWTwilio').hide();
        $('.divDesktop').show();
        $('.divWNodeJS').hide();
        $('#txtWTwilioAccountSID').val('');
        $('#txtWTwilioAuthToken').val('');
        $('#txtWTwilioFrom').val('');
    }
    else if ($('#ddlWhatsappService').val() == 2) {
        $('.divWTwilio').show();
        $('.divDesktop').hide();
        $('.divWNodeJS').hide();
    }
    else if ($('#ddlWhatsappService').val() == 3) {
        // Node.js service - show QR code section
        $('.divWTwilio').hide();
        $('.divDesktop').hide();
        $('.divWNodeJS').show();
        // Fetch QR code when this option is selected
        if (typeof fetchQRCode === 'function') {
            setTimeout(fetchQRCode, 500);
        }
    }
    else {
        $('.divWTwilio').hide();
        $('.divDesktop').hide();
        $('.divWNodeJS').hide();
    }
    clearWhatsappSettings();
}

function sendTestWhatsapp(WhatsappSettingsId) {
    var det = {
        WhatsappSettingsId: WhatsappSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/sendTestWhatsapp',
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

function clearWhatsappSettings() {
    //$('#ddlWhatsappService').val(0);
    $('#txtWTestMobileNo').val("");
    $('#txtWTwilioAccountSID').val("");
    $('#txtWTwilioAuthToken').val("");
    $('#txtWTwilioFrom').val("");
    $('#txtWhatsappSaveAs').val("");
    //$('#chkWhatsappIsDefault').prop("checked", false);
    $('.select2').select2();
}

