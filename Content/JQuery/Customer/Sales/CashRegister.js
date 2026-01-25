
$(function () {
    $('.select2').select2();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

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