$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: false,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('aBusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    //$('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });

    //$('#_JoiningDate').datetimepicker({
    //    format: DateFormat.toUpperCase(), defaultDate: new Date()
    //});

    //$('#_DOB').addClass('notranslate');
    //$('#_JoiningDate').addClass('notranslate');

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
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

var _PageIndex = 1;
var file = "";
var FileExtensionProfilePic = "";

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('aBusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        UserId: $('#ddlSupplier').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        Name: $('#txtName').val(),
        UserId: window.location.href.split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/AdminUser/UserFetch',
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

function ChangePassword() {
    $('.errorText').hide();
    var det = {
        OldPassword: $('#txtOldPassword').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/ChangePassword',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.reload();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function ProfileUpdate() {
    $('.errorText').hide();
    var det = {
        Name: $('#txtName').val(),
        EmailId: $('#txtEmailId').val(),
        MobileNo: $('#txtMobileNo').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        //ProfilePic: file,
        //FileExtensionProfilePic: FileExtensionProfilePic,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/ProfileUpdate',
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

function copyText(val) {
    navigator.clipboard.writeText(val);
    toastr.success("Copied the text: " + val);
}