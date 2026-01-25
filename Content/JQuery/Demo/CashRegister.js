
$(function () {
    $('.select2').select2();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

function insert(i) {
    var det = {
        BranchId: $('#ddlBranch').val(),
        CashInHand: $('#txtCashInHand').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/sales/CashRegisterInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                //sessionStorage.setItem('showMsg', '1');
                //sessionStorage.setItem('Msg', data.Message);
                if (window.location.href.indexOf('SalesId') != -1) {
                    window.location.href = "/sales/posedit?SalesId=" + window.location.href.split('=')[1] + "&BranchId=" + $('#ddlBranch').val();
                }
                else {
                    window.location.href = "/sales/posadd?BranchId=" + $('#ddlBranch').val();
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};