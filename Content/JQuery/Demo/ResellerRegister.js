
$(function () {

    $('.select2').select2();
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $(document).on('select2:open', () => {
        document.querySelector('.select2-search__field').focus();
    });
});

var interval;
var file = "";
var FileExtensionIdProof = "";

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    //toastr.success(sessionStorage.getItem("Msg"));
    $('#divErrorMsg').show();
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

function register() {
    $('#divLoading').show();
    if (!$('#chkTerms_Conditions').is(':checked')) {
        document.getElementById('error').play();
        toastr.error('Please accept the Terms & Conditions first');
        return
    }
    var det = {
        Name: $('#txtName').val(),
        Username: $('#txtUsername').val(),
        EmailId: $('#txtEmailId').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        //BusinessName: $('#txtBusinessName').val(),
        //Branch: $('#txtBranch').val(),
        MobileNo: $('#txtMobileNo').val(),
        CountryId: $('#ddlCountry').val(),
        TimeZoneId: $('#ddlTimeZone').val(),
        CurrencyId: $('#ddlCurrency').val(),
        Otp: $('#txtOtp').val(),
        CompetitorId: $('#ddlCompetitor').val(),
        IdProof: file,
        FileExtensionIdProof: FileExtensionIdProof,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/register/ResellerRegister',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.reload();
            }

            $("#divLoading").hide();
        },
        error: function (data) {
            //scrollToTop();
            $("#divLoading").hide();
        }
    });
};

function getProfileBase64() {
    var file1 = $("#IdProof").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#IdProof").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            file = reader.result;
            FileExtensionIdProof= '.' + file1.name.split('.').pop();

            $('#blah').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }

}

function RegisterOtp() {
    if (!$('#chkTerms_Conditions').is(':checked')) {
        document.getElementById('error').play();
        toastr.error('Please accept the Terms & Conditions first');
        return
    }
    var det = {
        Name: $('#txtName').val(),
        Username: $('#txtUsername').val(),
        EmailId: $('#txtEmailId').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        //BusinessName: $('#txtBusinessName').val(),
        //Branch: $('#txtBranch').val(),
        MobileNo: $('#txtMobileNo').val(),
        CountryId: $('#ddlCountry').val(),
        CurrencyId: $('#ddlCurrency').val(),
        TimeZoneId: $('#ddlTimeZone').val(),
        CompetitorId: $('#ddlCompetitor').val(),
        IdProof: file,
        FileExtensionIdProof: FileExtensionIdProof,
        UserType: "Reseller"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Register/ResellerRegisterOtp',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                $('.lblOtp').text('A one time password has been sent to ' + $('#txtEmailId').val());
                timer();
                $('#txtOtp').val('');
                $('.divRegister').hide();
                $('.divOtp').show();
            }

            $("#divLoading").hide();
        },
        error: function (data) {
            swal("Error", data.Message, "error");
            $("#divLoading").hide();
            scrollToTop();
        }
    });

}

function previous() {
    $('.divRegister').show();
    $('.divOtp').hide();
}

function timer() {
    $('.divResend').hide();
    clearInterval(interval);
    var timer2 = "2:01";
    interval = setInterval(function () {
        var timer = timer2.split(':');
        //by parsing integer, I avoid all extra string processing
        var minutes = parseInt(timer[0], 10);
        var seconds = parseInt(timer[1], 10);
        --seconds;
        minutes = (seconds < 0) ? --minutes : minutes;
        if (minutes == 0 && seconds == 0) {
            clearInterval(interval);
            $('#timer').html('0:00');
            $('.divResend').show();
        }

        seconds = (seconds < 0) ? 59 : seconds;
        seconds = (seconds < 10) ? '0' + seconds : seconds;
        //minutes = (minutes < 10) ?  minutes : minutes;
        $('#timer').html(minutes + ':' + seconds);
        timer2 = minutes + ':' + seconds;
    }, 1000);

}

function fetchDialingCode() {
    if ($('#ddlCountry').val() == 0) {
        $('.lblDialingCode').text('');
        $('.divDialingCode').hide();
    }
    else {
        var det = {
            CountryId: $('#ddlCountry').val(),
        };
        //$("#divLoading").show();
        $.ajax({
            url: '/register/Country',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    $('.lblDialingCode').text(data.Data.Country.DialingCode);
                    $('.divDialingCode').show();

                    //---------- currency set
                    var dropdown = '<select class="form-control select2" id="ddlCurrency"><option value="0">Select Currency *</option>';
                    $.each(data.Data.Currencys, function (index, value) {
                        if (data.Data.Currencys.length == 1) {
                            dropdown = dropdown + '<option selected value="' + value.CurrencyId + '">' + value.CurrencyName + '(' + value.CurrencyCode + ')' + '</option>';
                        }
                        else {
                            dropdown = dropdown + '<option value="' + value.CurrencyId + '">' + value.CurrencyName + '(' + value.CurrencyCode + ')' + '</option>';
                        }
                    });

                    dropdown = dropdown + '</select>';

                    $('.divCurrency').html('');
                    $('.divCurrency').append(dropdown);
                    if (data.Data.Currencys.length > 1) {
                        $('.divCurrency').show();
                    }
                    else {
                        $('.divCurrency').hide();
                    }
                    //---------- currency set

                    //---------- timezone set
                    var dropdown = '<select class="form-control select2" id="ddlTimeZone"><option value="0">Select Time Zone *</option>';
                    $.each(data.Data.Country.TimeZones, function (index, value) {
                        dropdown = dropdown + '<option value="' + value.TimeZoneId + '">' + '(UTC' + value.Utc + ') ' + value.DisplayName + '</option>';
                    });

                    //dropdown = dropdown + '</select><div class="input-group-append"><div class="input-group-text"><span class="fas fa-clock"></span></div></div>';
                    dropdown = dropdown + '</select>';

                    $('.divTimeZone').html('');
                    $('.divTimeZone').append(dropdown);

                    if ($('#ddlCountry').val() == 2) { //India
                        $('#ddlTimeZone').val(93);
                        $('.divTimeZone').hide();
                    }
                    else if ($('#ddlCountry').val() == 8) { //Bangladesh
                        $('#ddlTimeZone').val(97);
                        $('.divTimeZone').hide();
                    }
                    else {
                        $('.divTimeZone').show();
                    }
                    //---------- timezone set


                    $('.select2').select2();
                }


            },
            error: function (data) {
                //scrollToTop();
            }
        });
    }
}

function checkUserName() {
    $('.errorText').text();
    $('#divUsername').text('');
    var det = {
        Username: $('#txtUsername').val(),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Register/CheckUserName',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);

                $('#divUsername').text(data.Message);
            }
            //else {
            //    $('#divUsername').text(data.Message);
            //}

            $("#divLoading").hide();
        },
        error: function (data) {
            swal("Error", data.Message, "error");
            $("#divLoading").hide();
            scrollToTop();
        }
    });
}