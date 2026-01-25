
$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $('.select2').select2();
});

function purchaseUpdate() {
    var det = {
        PaymentTermId: $('#ddlPurchasePaymentTerm').val(),
        EnableEditingProductPrice: $('#chkEnableEditingProductPrice').is(':checked'),
        EnablePurchaseStatus: $('#chkEnablePurchaseStatus').is(':checked'),
        EnablePurchaseQuotation: $('#chkEnablePurchaseQuotation').is(':checked'),
        EnablePurchaseOrder: $('#chkEnablePurchaseOrder').is(':checked'),
        EnableSms: $('#chkPurchaseEnableSms').is(':checked'),
        EnableEmail: $('#chkPurchaseEnableEmail').is(':checked'),
        EnableWhatsapp: $('#chkPurchaseEnableWhatsapp').is(':checked'),
        EnableFreeQuantity: $('#chkEnableFreeQuantity').is(':checked'),
        EnableRoundOff: $('#chkPurchaseEnableRoundOff').is(':checked'),
        AutoPrintInvoicePurchaseQuotation: $('#chkPurchaseAutoPrintInvoicePurchaseQuotation').is(':checked'),
        AutoPrintInvoicePurchaseOrder: $('#chkPurchaseAutoPrintInvoicePurchaseOrder').is(':checked'),
        AutoPrintInvoicePurchaseBill: $('#chkPurchaseAutoPrintInvoicePurchaseBill').is(':checked'),
        AutoPrintInvoicePurchaseReturn: $('#chkPurchaseAutoPrintInvoicePurchaseReturn').is(':checked'),
        DiscountAccountId: $('#ddlPurchaseDiscountAccount').val(),
        RoundOffAccountId: $('#ddlPurchaseRoundOffAccount').val(),
        VendorAdvanceAccountId: $('#ddlVendorAdvanceAccount').val(),
        EnableSpecialDiscount: $('#chkPurchaseEnableSpecialDiscount').is(':checked'),
        SpecialDiscountAccountId: $('#ddlPurchaseSpecialDiscountAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasesettings/PurchaseSettingsUpdate',
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
                fetchMenuPermissions();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openPaymentTermModal(id) {
    _id = id;
    $('#paymentTermModal').modal('toggle');
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

function insertPaymentTerm() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        PaymentTerm: $('#txtPaymentTerm_M').val(),
        Days: $('#txtDays_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/paymentTermInsert',
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
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);


                    if ($('.' + res.Id + '_M_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_M_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_M_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_M_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_M_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                $('#ddlPurchasePaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPurchasePaymentTerm').val(data.Data.PaymentTerm.PaymentTermId);

                $('#paymentTermModal').modal('toggle');

                $('#txtPaymentTerm_M').val('');
                $('#txtDays_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}