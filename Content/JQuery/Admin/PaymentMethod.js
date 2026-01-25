var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];
function togglePaymentMethod() {
    $('.divBank').hide();
    $('.divUpiID').hide();
    $('.divGpay').hide();
    $('.divPhonePe').hide();
    $('.divPaytm').hide();
    $('.divPaypal').hide();

    if ($('#ddlPaymentMethod').val() == 1) {
        $('.divBank').show();
    }
    else if ($('#ddlPaymentMethod').val() == 2) {
        $('.divUpiID').show();
    }
    else if ($('#ddlPaymentMethod').val() == 3) {
        $('.divGpay').show();
    }
    else if ($('#ddlPaymentMethod').val() == 4) {
        $('.divPhonePe').show();
    }
    else if ($('#ddlPaymentMethod').val() == 5) {
        $('.divPaytm').show();
    }
    else if ($('#ddlPaymentMethod').val() == 6) {
        $('.divPaypal').show();
    }
}

function updateResellerPaymentMethod() {
    var det = {
        PaymentMethod: $('#ddlPaymentMethod').val(),
        BankName: $('#txtBankName').val(),
        BranchName: $('#txtBranchName').val(),
        BranchCode: $('#txtBranchCode').val(),
        AccountNumber: $('#txtAccountNumber').val(),
        AccountHolder: $('#txtAccountHolder').val(),
        IFSC: $('#txtIFSC').val(),
        BankAddress: $('#txtBankAddress').val(),
        SwiftBIC: $('#txtSwiftBIC').val(),
        UpiID: $('#txtUpiID').val(),
        Gpay: $('#txtGpay').val(),
        PhonePe: $('#txtPhonePe').val(),
        Paytm: $('#txtPaytm').val(),
        Paypal: $('#txtPaypal').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/ResellerPaymentMethodUpdate',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
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