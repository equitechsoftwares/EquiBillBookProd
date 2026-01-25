
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
function fetchList(PageIndex) {
    var DateFormat = Cookies.get('aBusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        Name: $('#txtName').val(),
        ImportNo: $('#txtImportNo').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/AdminOtherSoftwareImport/OtherSoftwareImportFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            //$('.chkIsActive').bootstrapToggle();
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

function UpdateStatus(OtherSoftwareImportId, Status) {
    if (confirm("Do you want to update status to " + (Status==2 ?"Uploaded?":"Rejected?")) == true) {
        $('.errorText').hide();
        var det = {
            OtherSoftwareImportId: OtherSoftwareImportId,
            Status: Status,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminothersoftwareimport/UpdateStatus',
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
}