function posUpdate() {
    var PaymentTypes = [], AdditionalCharges = [];

    $('#divAdditionalCharges tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            AdditionalCharges.push({
                AdditionalChargeId: _id,
                ShortCutkey: $('#txtAdditionalShortCutkey' + _id).val(),
            })
        }
    });

    $('#divCombo tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            PaymentTypes.push({
                PaymentTypeId: _id,
                IsPosShown: $('#chkIsPosShown' + _id).is(':checked'),
                ShortCutkey: $('#txtShortCutkey' + _id).val(),
            })
        }
    });

    var det = {
        DisableMultiplePay: $('#chkDisableMultiplePay').is(':checked'),
        DisableDraft: $('#chkDisableDraft').is(':checked'),
        DisableExpressCheckout: $('#chkDisableExpressCheckout').is(':checked'),
        DontShowProductSuggestion: $('#chkDontShowProductSuggestion').is(':checked'),
        DontShowRecentTransactions: $('#chkDontShowRecentTransactions').is(':checked'),
        DisableDicount: $('#chkDisableDicount').is(':checked'),
        DisableOrderTax: $('#chkDisableOrderTax').is(':checked'),
        SubTotalEditable: $('#chkSubTotalEditable').is(':checked'),
        EnableTransactionDate: $('#chkEnableTransactionDate').is(':checked'),
        EnableServiceStaff: $('#chkEnableServiceStaff').is(':checked'),
        IsServiceStaffRequired: $('#chkIsServiceStaffRequired').is(':checked'),
        DisableCreditSale: $('#chkDisableCreditSale').is(':checked'),
        EnableWeighingScale: $('#chkEnableWeighingScale').is(':checked'),
        ShowInvoiceScheme: $('#chkShowInvoiceScheme').is(':checked'),
        ShowInvoiceLayoutDropdown: $('#chkShowInvoiceLayoutDropdown').is(':checked'),
        PrintInvoiceOnHold: $('#chkPrintInvoiceOnHold').is(':checked'),
        ShowPricingOnProductSuggestionTooltip: $('#chkShowPricingOnProductSuggestionTooltip').is(':checked'),
        WeighingScaleBarcodePrefix: $('#txtWeighingScaleBarcodePrefix').val(),
        ProductSkuLength: $('#txtProductSkuLength').val(),
        QuantityIntegerPartLength: $('#txtQuantityIntegerPartLength').val(),
        QuantityFractionalPartLength: $('#txtQuantityFractionalPartLength').val(),
        AllowOnlinePayment: $('#chkAllowOnlinePaymentPos').is(':checked'),
        DisableHold: $('#chkDisableHold').is(':checked'),
        DisableProforma: $('#chkDisableProforma').is(':checked'),
        DisableQuotation: $('#chkDisableQuotation').is(':checked'),
        EnableNotes: $('#chkEnableNotes').is(':checked'),
        EnableSms: $('#chkPosEnableSms').is(':checked'),
        EnableEmail: $('#chkPosEnableEmail').is(':checked'),
        EnableWhatsapp: $('#chkPosEnableWhatsapp').is(':checked'),
        EnableFreeQuantity: $('#chkPosEnableFreeQuantity').is(':checked'),
        SalePriceIsMinSellingPrice: $('#chkPosPriceIsMinSellingPrice').is(':checked'),
        Draft: $('#txtDraft').val(),
        Quotation: $('#txtQuotation').val(),
        Proforma: $('#txtProforma').val(),
        CreditSale: $('#txtCreditSale').val(),
        EditShippingCharge: $('#txtEditShippingCharge').val(),
        EditPackagingCharge: $('#txtEditPackagingCharge').val(),
        Multiple: $('#txtMultiple').val(),
        Hold: $('#txtHold').val(),
        Cancel: $('#txtCancel').val(),
        GoToProductQuantity: $('#txtGoToProductQuantity').val(),
        WeighingScale: $('#txtWeighingScale').val(),
        EditDiscount: $('#txtEditDiscount').val(),
        EditOrderTax: $('#txtEditOrderTax').val(),
        AddPaymentRow: $('#txtAddPaymentRow').val(),
        FinalisePayment: $('#txtFinalisePayment').val(),
        AddNewProduct: $('#txtAddNewProduct').val(),
        RecentTransactions: $('#txtRecentTransactions').val(),
        HoldList: $('#txtHoldList').val(),
        Calculator: $('#txtCalculator').val(),
        FullScreen: $('#txtFullScreen').val(),
        RegisterDetails: $('#txtRegisterDetails').val(),
        PosExit: $('#txtPosExit').val(),
        AdditionalCharges: AdditionalCharges,
        PaymentTypes: PaymentTypes,
        EnableRoundOff: $('#chkPosEnableRoundOff').is(':checked'),
        InvoiceType: $('#ddlPosInvoiceType').val(),
        AutoPrintInvoiceFinal: $('#chkPosAutoPrintInvoiceFinal').is(':checked'),
        AutoPrintInvoiceDraft: $('#chkPosAutoPrintInvoiceDraft').is(':checked'),
        AutoPrintInvoiceQuotation: $('#chkPosAutoPrintInvoiceQuotation').is(':checked'),
        AutoPrintInvoiceProforma: $('#chkPosAutoPrintInvoiceProforma').is(':checked'),
        AutoPrintInvoiceSalesReturn: $('#chkPosAutoPrintInvoiceSalesReturn').is(':checked'),
        ChangeReturnAccountId: $('#ddlChangeReturnAccount').val(),
        EnableSpecialDiscount: $('#chkPosEnableSpecialDiscount').is(':checked'),
        EditSpecialDiscount: $('#txtEditSpecialDiscount').val(),
        EnablePlaceOfSupply: $('#chkPosEnablePlaceOfSupply').is(':checked'),
        EnableSellingPriceGroup: $('#chkPosEnableSellingPriceGroup').is(':checked'),
        ShowItemImage: $('#chkPosShowItemImage').is(':checked'),
        ShowItemSellingPrice: $('#chkPosShowItemSellingPrice').is(':checked'),
        ShowItemMrp: $('#chkPosShowItemMrp').is(':checked'),
        EnableKot: $('#chkPosEnableKot').is(':checked'),
        EnableTableBooking: $('#chkPosEnableTableBooking').is(':checked'),
        AutoCreateKot: $('#chkPosAutoCreateKot').is(':checked'),
        AllowLinkExistingKot: $('#chkPosAllowLinkExistingKot').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/salessettings/PosSettingsUpdate',
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
                //window.location.href = "/master/Branch";
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};