
$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_StartDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_StartDate').addClass('notranslate');
   
    $('.select2').select2();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var ProfilePic = '', fileExtensionProfilePic = '';

function businessInformationUpdate() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var BusinessRegistrationType = '';
    if ($('#chkIsBusinessRegistered').is(':checked') == true) {
        BusinessRegistrationType = $('#ddlBusinessRegistrationType').val();
    }
    else {
        BusinessRegistrationType = "";
    }

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
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') == true ? 1 : 2,
        BusinessRegistrationType: BusinessRegistrationType
    };
    $("#divLoading").show();
    $.ajax({
        url: '/organizationsettings/businesssettings',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchMenuPermissions();
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

function fetchMenuPermissions() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/dashboard/MenuPermissions',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $(".main-sidebar").html(data);
            $('.sidebar').css('overflow-y', 'auto');
            $('.Settings').addClass('menu-open');
            $('.Business-Settings').addClass('active');
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};
function TimeZone() {
    var det = {
        TimeZoneId: $('#ddlTimeZone').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/organizationsettings/TimeZone',
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
        url: '/organizationsettings/CheckTime',
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

function toggleBusinessRegistrationType() {
    if ($('#chkIsBusinessRegistered').is(':checked')) {
        $('.divBusinessRegistrationType').show();
    }
    else {
        $('.divBusinessRegistrationType').hide();
    }
}