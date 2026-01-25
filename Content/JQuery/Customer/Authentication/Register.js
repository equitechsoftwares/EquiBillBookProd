
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

    $('#ddlCountry').val(2);
    fetchDialingCode();

});

var interval;

function register() {
    $('#divLoading').show();

    var BusinessRegistrationType = '';
    if ($('#chkIsBusinessRegistered').is(':checked') == true) {
        BusinessRegistrationType = $('#ddlBusinessRegistrationType').val();
    }
    else {
        BusinessRegistrationType = "";
    }

    var det = {
        Name: $('#txtName').val(),
        //Username: $('#txtUserCode').val(),
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
        Username: window.location.href.split('=')[1],
        StateId: $('#ddlState').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') == true ? 1 : 2,
        BusinessRegistrationType: BusinessRegistrationType
    };
    $("#divLoading").show();
    $.ajax({
        url: '/register/Register',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                window.location.href = "/dashboard?type=login";
                //window.location.href = "/master/branchadd";
            }

            $("#divLoading").hide();
        },
        error: function (data) {
            //scrollToTop();
            $("#divLoading").hide();
        }
    });
};

function RegisterOtp() {
    
    $('#divLoading').show();

    var BusinessRegistrationType = '';
    if ($('#chkIsBusinessRegistered').is(':checked') == true) {
        BusinessRegistrationType = $('#ddlBusinessRegistrationType').val();
    }
    else {
        BusinessRegistrationType = "";
    }

    var det = {
        Name: $('#txtName').val(),
        //Username: $('#txtUserCode').val(),
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
        StateId: $('#ddlState').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') == true ? 1 : 2,
        BusinessRegistrationType: BusinessRegistrationType
    };

    $.ajax({
        url: '/Register/RegisterOtp',
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
    var timer2 = "5:01";
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
        if ($('#ddlCountry').val() == 2) {
            fetchActiveStates();
            $('#p_States_Dropdown').hide();
        }
        else {
            $('#p_States_Dropdown').hide();
        }
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
                    var dropdown = '<select class="form-control select2" id="ddlCurrency"><option value="0">Select Currency</option>';
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
                    var dropdown = '<select class="form-control select2" id="ddlTimeZone"><option value="0">Select Time Zone</option>';
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

function fetchActiveStates() {
    var det = {
        CountryId: $('#ddlCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/register/ActiveStates',
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

function toggleBusinessRegistrationType() {
    if ($('#chkIsBusinessRegistered').is(':checked')) {
        $('.divBusinessRegistrationType').show();
    }
    else {
        $('.divBusinessRegistrationType').hide();
    }
}