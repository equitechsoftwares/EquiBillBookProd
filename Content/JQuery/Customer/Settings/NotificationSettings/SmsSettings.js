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
        url: '/notificationsettings/smssettingsfetch',
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
        url: '/notificationsettings/smssettingInsert',
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
                    window.location.href = "/notificationsettings/smssettings";
                }
                else {
                    window.location.href = "/notificationsettings/smssettingadd";
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
        SmsSettingsId: window.location.href.split('=')[1],
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
        url: '/notificationsettings/smssettingUpdate',
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
                    window.location.href = "/notificationsettings/smssettings";
                }
                else {
                    window.location.href = "/notificationsettings/smssettingadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(SmsSettingsId, IsActive) {
    var det = {
        SmsSettingsId: SmsSettingsId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/smssettingActiveInactive',
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

function Delete(SmsSettingsId, SaveAs) {
    var r = confirm("This will delete \"" + SaveAs + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SmsSettingsId: SmsSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/notificationsettings/smssettingdelete',
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

function toggleSmsService() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('.divTwilio').hide();
    $('.divNexmo').hide();
    $('.divOthers').hide();
    if ($('#ddlSmsService').val() == 1) {
        $('.divTwilio').show();
    }
    if ($('#ddlSmsService').val() == 2) {
        $('.divNexmo').show();
    }
    if ($('#ddlSmsService').val() == 3) {
        $('.divOthers').show();
        toggleRequestMethod();
    }
    $('#ddlRequestMethod').val(1);
    clearSmsSettings();
}

function sendTestSms(SmsSettingsId) {
    var det = {
        SmsSettingsId: SmsSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/sendTestSms',
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

function toggleRequestMethod() {
    if ($('#ddlRequestMethod').val() == 2) {
        $('.divHeader').show();
    }
    else {
        $('.divHeader').hide();
    }
    clearSmsSettings();
}

