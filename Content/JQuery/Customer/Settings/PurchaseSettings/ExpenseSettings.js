
$(function () {
    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
});
function expenseUpdate() {
    var det = {
        EnableMileage: $('#chkEnableMileage').is(':checked'),
        MileageAccountId: $('#ddlExpenseMileageAccount').val(),
        UnitId: $('#ddlExpenseUnit').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasesettings/UpdateExpenseSettings',
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
                //fetchMenuPermissions();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};
function toggleMileage() {
    var IsAccountsAddon = $('#hdnIsAccountsAddon').val().toLocaleLowerCase();
    if ($('#chkEnableMileage').is(':checked') == true) {
        $('.divMileage').show();
        if (IsAccountsAddon == 'true') {
            $('.divMileageAccount').show();
        }        
    }
    else {
        $('.divMileage').hide();
        $('.divMileageAccount').hide();
    }
}