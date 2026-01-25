$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('.select2').select2();

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
    if (EnableSound == 'True') {document.getElementById('success').play();}
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/Subscription/MyTransactionsFetch',
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

//function insert(i) {
//    var det = {
//        CountryId: $('#ddlCountry').val(),
//        StateId: $('#ddlState').val(),
//        CityCode: $('#txtCityCode').val(),
//        City: $('#txtCity').val(),
//        IsActive: true,
//        IsDeleted: false,
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/locationsettings/CityInsert',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') {document.getElementById('error').play();}
//                toastr.error(data.Message);
//            }
//            else {
//                sessionStorage.setItem('showMsg', '1');
//                sessionStorage.setItem('Msg', data.Message);
//                if (i == 1) {
//                    window.location.href = "/master/City";
//                }
//                else {
//                    window.location.href = "/master/cityadd";
//                }
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

//function update(i) {
//    var det = {
//        CountryId: $('#ddlCountry').val(),
//        StateId: $('#ddlState').val(),
//        CityId: window.location.href.split('=')[1],
//        CityCode: $('#txtCityCode').val(),
//        City: $('#txtCity').val(),
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/master/CityUpdate',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') {document.getElementById('error').play();}
//                toastr.error(data.Message);
//            }
//            else {
//                sessionStorage.setItem('showMsg', '1');
//                sessionStorage.setItem('Msg', data.Message);
//                if (i == 1) {
//                    window.location.href = "/master/City";
//                }
//                else {
//                    window.location.href = "/master/cityadd";
//                }
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

//function ActiveInactive(CityId, IsActive) {
//    var det = {
//        CityId: CityId,
//        IsActive: IsActive
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/master/CityActiveInactive',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') {document.getElementById('error').play();}
//                toastr.error(data.Message);
//            }
//            else {
//                if (EnableSound == 'True') {document.getElementById('success').play();}
//                toastr.success(data.Message);
//                fetchList();
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

function Delete(TransactionId, TransactionNo) {
    var r = confirm("This will delete \"" + TransactionNo + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            TransactionId: TransactionId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Subscription/TransactionDelete',
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
                    if (EnableSound == 'True') {document.getElementById('error').play();}
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') {document.getElementById('success').play();}
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

//function fetchActiveStates() {
//    var det = {
//        CountryId: $('#ddlCountry').val()
//    };
//    _PageIndex = det.PageIndex;
//    $("#divLoading").show();
//    $.ajax({
//        url: '/master/ActiveStates',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            $("#divLoading").hide();
//            $("#p_States_Dropdown").html(data);
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

function PrintInvoice(InvoiceUrl) {
    //var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
    window.location.href = InvoiceUrl;
}

//function insertCountry() {
//    var det = {
//        CountryCode: $('#txtCountryCode').val(),
//        Country: $('#txtCountry').val(),
//        IsActive: true,
//        IsDeleted: false,
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/master/countryInsert',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') {document.getElementById('error').play();}
//                toastr.error(data.Message);
//            }
//            else {
//                $('#ddlCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
//                $('#ddlCountry').val(data.Data.Country.CountryId);

//                $('#ddlNewCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));

//                $('#countryModal').modal('toggle');

//                $('#ddlState').html('');

//                $('#txtCountryCode').val('');
//                $('#txtCountry').val('');
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

//function insertState() {
//    var det = {
//        CountryId: $('#ddlNewCountry').val(),
//        StateCode: $('#txtStateCode').val(),
//        State: $('#txtState').val(),
//        IsActive: true,
//        IsDeleted: false,
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/master/StateInsert',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') {document.getElementById('error').play();}
//                toastr.error(data.Message);
//            }
//            else {
//                $('#ddlState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
//                $('#ddlState').val(data.Data.State.StateId);
//                $('#stateModal').modal('toggle');

//                $('#ddlNewCountry').val($("#ddlNewCountry option:first").val());
//                $('#txtStateCode').val('');
//                $('#txtState').val('');
//                $('.select2').select2();
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};