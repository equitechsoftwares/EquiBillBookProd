$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_StartDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_StartDate').addClass('notranslate');
    $('#_MigrationDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_MigrationDate').addClass('notranslate');

    $('.select2').select2();

    $('textarea#txtPurchaseTerms,#txtPurchaseReturnTerms,#txtSalesTerms,#txtSalesReturnTerms,#txtDraftTerms,#txtQuotationTerms,#txtProformaInvoiceTerms').summernote({
        placeholder: '',
        followingToolbar: false,
        tabsize: 2,
        height: 200,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            ['fontname', ['fontname']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            ['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });

    fetchCompanyCurrency();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var ProfilePic = '', fileExtensionProfilePic = '', _OnlinePaymentSettingsId = 0; _EmailSettingsId = 0, _SmsSettingsId = 0, _WhatsappSettingsId = 0,
    _PwaSettingsId = 0, Image_48_48='',FileExtensionImage_48_48='', Image_72_72 = '', FileExtensionImage_72_72 = '', Image_96_96 = '', FileExtensionImage_96_96 = '', Image_128_128 = '', FileExtensionImage_128_128 = '',
    Image_144_144 = '', FileExtensionImage_144_144 = '', Image_152_152 = '', FileExtensionImage_152_152 = '', Image_192_192 = '', FileExtensionImage_192_192 = '',
    Image_284_284 = '', FileExtensionImage_284_284 = '', Image_512_512 = '', FileExtensionImage_512_512 = '';;
function fetchCompanyCurrency() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/user/FetchCompanyCurrency',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                CurrencySymbol = data.Data.User.CurrencySymbol;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}
function businessInformationUpdate() {
    $('.errorText').hide();
    var det = {
        BusinessName: $('#txtBusinessName').val(),
        WebsiteUrl: $('#txtWebsiteUrl').val(),
        StartDate: $('#txtStartDate').val(),
        CountryId: $('#ddlCountry').val(),
        CurrencySymbolPlacement: $('#ddlCurrencySymbolPlacement').val(),
        FinancialYearStartMonth: $('#ddlFinancialYearStartMonth').val(),
        TransactionEditDays: $('#ddlTransactionEditDays').val(),
        DateFormat: $('#ddlDateFormat').val(),
        TimeFormat: $('#ddlTimeFormat').val(),
        BusinessLogo: ProfilePic,
        FileExtension: fileExtensionProfilePic,
        EnableDaylightSavingTime: $('#chkEnableDaylightSavingTime').is(':checked'),
        TimeZoneId: $('#ddlTimeZone').val(),
        IndustryTypeId: $('#ddlIndustryType').val(),
        BusinessTypes: $('#ddlBusinessType').val(),
        EnableDefaultSmsBranding: $('#chkEnableDefaultSmsBranding').is(':checked'),
        EnableDefaultEmailBranding: $('#chkEnableDefaultEmailBranding').is(':checked'),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') == true ? 1 : 2
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/businesssettings',
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
                fetchMenuPermissions();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getProfilePicBase64() {
    var file1 = $("#ProfilePic").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProfilePic").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            ProfilePic = reader.result;
            fileExtensionProfilePic = '.' + file1.name.split('.').pop();

            $('#blahProfilePic').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function taxUpdate() {
    var det = {
        Tax1Name: $('#txtTax1Name').val(),
        Tax1No: $('#txtTax1No').val(),
        Tax2Name: $('#txtTax2Name').val(),
        Tax2No: $('#txtTax2No').val(),
        EnableInlineTax: $('#chkEnableInlineTax').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/TaxUpdate',
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

function creditLimitUpdate() {
    var det = {
        CreditLimitForSupplier: $('#txtCreditLimitForSupplier').val(),
        CreditLimitForCustomer: $('#txtCreditLimitForCustomer').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/CreditLimitUpdate',
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

function rewardPointsUpdate() {
    var det = {
        EnableRewardPoint: $('#chkEnableRewardPoint').is(':checked'),
        DisplayName: $('#txtDisplayName').val(),
        AmountSpentForUnitPoint: $('#txtAmountSpentForUnitPoint').val(),
        MinOrderTotalToEarnReward: $('#txtMinOrderTotalToEarnReward').val(),
        MaxPointsPerOrder: $('#txtMaxPointsPerOrder').val(),
        RedeemAmountPerUnitPoint: $('#txtRedeemAmountPerUnitPoint').val(),
        MinimumOrderTotalToRedeemPoints: $('#txtMinimumOrderTotalToRedeemPoints').val(),
        MinimumRedeemPoint: $('#txtMinimumRedeemPoint').val(),
        MaximumRedeemPointPerOrder: $('#txtMaximumRedeemPointPerOrder').val(),
        ExpiryPeriod: $('#txtExpiryPeriod').val(),
        PayTermNo: $('#txtPayTermNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/RewardPointSettingsUpdate',
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

function itemUpdate() {
    var det = {
        EnableItemExpiry: $('#chkEnableItemExpiry').is(':checked'),
        ExpiryType: $('#ddlExpiryType').val(),
        OnItemExpiry: $('#ddlOnItemExpiry').val(),
        StopSellingBeforeDays: $('#txtStopSellingBeforeDays').val(),
        EnableBrands: $('#chkEnableBrands').is(':checked'),
        EnableCategory: $('#chkEnableCategory').is(':checked'),
        EnableSubCategory: $('#chkEnableSubCategory').is(':checked'),
        EnableSubSubCategory: $('#chkEnableSubSubCategory').is(':checked'),
        EnableTax_PriceInfo: $('#chkEnableTax_PriceInfo').is(':checked'),
        DefaultUnitId: $('#ddlDefaultUnit').val(),
        EnableSecondaryUnit: $('#chkEnableSecondaryUnit').is(':checked'),
        EnableTertiaryUnit: $('#chkEnableTertiaryUnit').is(':checked'),
        EnableQuaternaryUnit: $('#chkEnableQuaternaryUnit').is(':checked'),
        EnableRacks: $('#chkEnableRacks').is(':checked'),
        EnableRow: $('#chkEnableRow').is(':checked'),
        EnablePosition: $('#chkEnablePosition').is(':checked'),
        EnableWarranty: $('#chkEnableWarranty').is(':checked'),
        DefaultProfitPercent: $('#txtDefaultProfitPercent').val(),
        EnableLotNo: $('#chkEnableLotNo').is(':checked'),
        StockAccountingMethod: $('#ddlStockAccountingMethod').val(),
        EnableProductVariation: $('#chkEnableProductVariation').is(':checked'),
        EnableComboProduct: $('#chkEnableComboProduct').is(':checked'),
        EnableProductDescription: $('#chkEnableProductDescription').is(':checked'),
        EnableImei: $('#chkEnableImei').is(':checked'),
        EnablePrintLabel: $('#chkEnablePrintLabel').is(':checked'),
        EnableStockAdjustment: $('#chkEnableStockAdjustment').is(':checked'),
        ExpiryDateFormat: $('#ddlExpiryDateFormat').val(),
        EnableMrp: $('#chkEnableMrp').is(':checked'),
        EnableStockTransfer: $('#chkEnableStockTransfer').is(':checked'),
        EnableSellingPriceGroup: $('#chkEnableSellingPriceGroup').is(':checked'),
        TaxType: $('#ddlTaxType').val(),
        EnableSalt: $('#chkEnableSalt').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/ItemSettingsUpdate',
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
                fetchPrefix();
                fetchMenuPermissions();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function saleUpdate() {
    var det = {
        PaymentTermId: $('#ddlSalesPaymentTerm').val(),
        DefaultSaleDiscount: $('#txtDefaultSaleDiscount').val(),
        DiscountType: $('#ddlDiscountType').val(),
        DefaultSaleTaxId: $('#ddlDefaultSaleTax').val(),
        SalePriceIsMinSellingPrice: $('#chkSalePriceIsMinSellingPrice').is(':checked'),
        AllowOverSelling: $('#chkAllowOverSelling').is(':checked'),
        IsPayTermRequired: $('#chkIsPayTermRequired').is(':checked'),
        SalesCommissionAgent: $('#ddlSalesCommissionAgent').val(),
        CommissionCalculationType: $('#ddlCommissionCalculationType').val(),
        IsCommissionAgentRequired: $('#chkIsCommissionAgentRequired').is(':checked'),
        AllowOnlinePayment: $('#chkAllowOnlinePaymentSales').is(':checked'),
        EnableSms: $('#chkSalesEnableSms').is(':checked'),
        EnableEmail: $('#chkSalesEnableEmail').is(':checked'),
        EnableWhatsapp: $('#chkSalesEnableWhatsapp').is(':checked'),
        EnableFreeQuantity: $('#chkSalesEnableFreeQuantity').is(':checked'),
        EnableRoundOff: $('#chkSalesEnableRoundOff').is(':checked'),
        InvoiceType: $('#ddlSalesInvoiceType').val(),
        AutoPrintInvoiceQuotation: $('#chkSalesAutoPrintInvoiceQuotation').is(':checked'),
        AutoPrintInvoiceOrder: $('#chkSalesAutoPrintInvoiceOrder').is(':checked'),
        AutoPrintInvoiceDeliveryChallan: $('#chkSalesAutoPrintInvoiceDeliveryChallan').is(':checked'),
        AutoPrintInvoiceProforma: $('#chkSalesAutoPrintInvoiceProforma').is(':checked'),
        AutoPrintInvoiceBill: $('#chkSalesAutoPrintInvoiceBill').is(':checked'),
        AutoPrintInvoiceSalesReturn: $('#chkSalesAutoPrintInvoiceSalesReturn').is(':checked'),
        DiscountAccountId: $('#ddlSalesDiscountAccount').val(),
        ShippingChargesAccountId: $('#ddlSalesShippingChargesAccount').val(),
        PackagingChargesAccountId: $('#ddlSalesPackagingChargesAccount').val(),
        OtherChargesAccountId: $('#ddlSalesOtherChargesAccount').val(),
        RoundOffAccountId: $('#ddlSalesRoundOffAccount').val(),
        EnableCustomerGroup: $('#chkEnableCustomerGroup').is(':checked'),
        EnableSalesQuotation: $('#chkSalesEnableSalesQuotation').is(':checked'),
        EnableSalesOrder: $('#chkSalesEnableSalesOrder').is(':checked'),
        EnableDeliveryChallan: $('#chkSalesEnableDeliveryChallan').is(':checked'),
        EnableSalesProforma: $('#chkSalesEnableSalesProforma').is(':checked')
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/SaleSettingsUpdate',
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

function posUpdate() {
    var PaymentTypes = [];

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
        PaymentTypes: PaymentTypes,
        EnableRoundOff: $('#chkPosEnableRoundOff').is(':checked'),
        InvoiceType: $('#ddlPosInvoiceType').val(),
        AutoPrintInvoiceFinal: $('#chkPosAutoPrintInvoiceFinal').is(':checked'),
        AutoPrintInvoiceDraft: $('#chkPosAutoPrintInvoiceDraft').is(':checked'),
        AutoPrintInvoiceQuotation: $('#chkPosAutoPrintInvoiceQuotation').is(':checked'),
        AutoPrintInvoiceProforma: $('#chkPosAutoPrintInvoiceProforma').is(':checked'),
        AutoPrintInvoiceSalesReturn: $('#chkPosAutoPrintInvoiceSalesReturn').is(':checked'),
        ChangeReturnAccountId: $('#ddlChangeReturnAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PosSettingsUpdate',
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
        ShippingChargesAccountId: $('#ddlPurchaseShippingChargesAccount').val(),
        PackagingChargesAccountId: $('#ddlPurchasePackagingChargesAccount').val(),
        OtherChargesAccountId: $('#ddlPurchaseOtherChargesAccount').val(),
        RoundOffAccountId: $('#ddlPurchaseRoundOffAccount').val(),
        VendorAdvanceAccountId: $('#ddlVendorAdvanceAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PurchaseSettingsUpdate',
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

function expenseUpdate() {
    var det = {
        EnableMileage: $('#chkEnableMileage').is(':checked'),
        MileageAccountId: $('#ddlExpenseMileageAccount').val(),
        UnitId: $('#ddlExpenseUnit').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdateExpenseSettings',
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

function sendTestSms(SmsSettingsId) {
    var det = {
        SmsSettingsId: SmsSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/sendTestSms',
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
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function emailModulesUpdate() {
    var NotificationModulesSettings = [];
    $('#divNotificationModules tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            NotificationModulesSettings.push({
                NotificationModulesId: _id,
                IsActive: $('#chkNotificationModule' + _id).is(':checked'),
                NotificationModulesSettingsId: $('#hdnNotificationModulesSettingsId' + _id).val(),
            })
        }
    });

    var det = {
        NotificationModulesSettings: NotificationModulesSettings,
        Type: 1
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/EmailModulesUpdate',
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
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleItemExpiry() {
    if ($('#chkEnableItemExpiry').is(':checked')) {
        $('.divItemExpiry').show();
    }
    else {
        $('.divItemExpiry').hide();
        $('#ddlExpiryType').val(1);
        $('#ddlOnItemExpiry').val(1);
        $('#txtStopSellingBeforeDays').val(0);
    }
}

function toggleOnExpiry() {
    if ($('#ddlOnItemExpiry').val() == 1) {
        $('#txtStopSellingBeforeDays').val(0);
        $('#txtStopSellingBeforeDays').prop('disabled', true);
    }
    else {
        $('#txtStopSellingBeforeDays').prop('disabled', false);
    }
}

function updatePrefix() {
    var PrefixUserMaps = [];
    $('.divPrefix').each(function () {
        var _innerid = this.id;
        PrefixUserMaps.push({
            PrefixId: $('#hdnPrefixId' + _innerid).val(),
            PrefixUserMapId: $('#hdnPrefixUserMapId' + _innerid).val(),
            NoOfDigits: $('#txtNoOfDigits' + _innerid).val(),
            Prefix: $('#txtPrefix' + _innerid).val(),
        })
    });
    var det = {
        PrefixUserMaps: PrefixUserMaps
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PrefixUpdate',
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
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function updateShortCutKeys() {
    $('.errorText').hide();
    var ShortCutKeySettings = [];

    $('#divShortcutKeys tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            if ($('#chkIsShortcutKey' + _id).is(':checked')) {
                ShortCutKeySettings.push({
                    MenuId: $('#chkIsShortcutKey' + _id).val(),
                    Title: $('#txtTitle' + _id).val(),
                    ShortCutkey: $('#txtShortCutkey' + _id).val(),
                    ShortCutKeySettingId: $('#hdnShortCutKeySettingId' + _id).val(),
                    divId: _id
                })
            }
        }
    });

    var det = {
        ShortCutKeySettings: ShortCutKeySettings
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/ShortCutKeySettingUpdate',
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

                fetchShortCutKeySetting();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchShortCutKeySetting() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/settings1/ShortcutKeySettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                var html = '';

                $.each(data.Data.ShortCutKeySettings, function (index, value) {
                    if (value.IsView == true) {
                        if (value.Url == "" || value.Url == null) {
                            html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a style="color:#000;cursor:unset;" id="btnshortcutkey' + index + '" disabled href="javascript:void(0)" class="nav-link">' + value.Title;
                        }
                        else {
                            html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a id="btnshortcutkey' + index + '" href="/' + value.Url + '" class="nav-link">' + value.Title;
                        }

                        if (value.ShortCutKey != "" && value.ShortCutKey != null) {
                            html = html + ' <span class="btn btn-default btn-sm"> ' + value.ShortCutKey + '</span>';

                            Mousetrap.bind(value.ShortCutKey, function (e) {
                                e.preventDefault();
                                window.location.href = '/' + value.Url;
                            });
                        }
                        html = html + '</a></li>';
                    }
                });

                $('.tShortcutKeys').append(html);
            }

        },
        error: function (xhr) {

        }
    });


}

//function updateOtherShortCutKeys() {
//    var det = {
//        AddNewForm: $('#txtAddNewForm').val(),
//        SaveForm: $('#txtSaveForm').val(),
//        SaveAddAnother: $('#txtSaveAddAnother').val(),
//        UpdateForm: $('#txtUpdateForm').val(),
//        UpdateAddAnother: $('#txtUpdateAddAnother').val(),
//        GoBack: $('#txtGoBack').val(),
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/settings1/OtherShortCutKeySettingUpdate',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {
//            if (data.Status == 0) {
//                if (EnableSound == 'True') { document.getElementById('error').play(); }
//                toastr.error(data.Message);
//            }
//            else {
//                if (EnableSound == 'True') { document.getElementById('success').play(); }
//                toastr.success(data.Message);
//            }
//            $("#divLoading").hide();
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//};

function togglePurchaseTerms() {
    if ($('#chkEnablePurchaseTerms').is(':checked') == true) {
        $('.divPurchaseTerms').show();
    }
    else {
        $('.divPurchaseTerms').hide();
    }
}

function togglePurchaseReturnTerms() {
    if ($('#chkEnablePurchaseReturnTerms').is(':checked') == true) {
        $('.divPurchaseReturnTerms').show();
    }
    else {
        $('.divPurchaseReturnTerms').hide();
    }
}

function toggleSalesTerms() {
    if ($('#chkEnableSalesTerms').is(':checked') == true) {
        $('.divSalesTerms').show();
    }
    else {
        $('.divSalesTerms').hide();
    }
}

function toggleSalesReturnTerms() {
    if ($('#chkEnableSalesReturnTerms').is(':checked') == true) {
        $('.divSalesReturnTerms').show();
    }
    else {
        $('.divSalesReturnTerms').hide();
    }
}

function toggleDraftTerms() {
    if ($('#chkEnableDraftTerms').is(':checked') == true) {
        $('.divDraftTerms').show();
    }
    else {
        $('.divDraftTerms').hide();
    }
}

function toggleQuotationTerms() {
    if ($('#chkEnableQuotationTerms').is(':checked') == true) {
        $('.divQuotationTerms').show();
    }
    else {
        $('.divQuotationTerms').hide();
    }
}

function toggleProformaInvoiceTerms() {
    if ($('#chkEnableProformaInvoiceTerms').is(':checked') == true) {
        $('.divProformaInvoiceTerms').show();
    }
    else {
        $('.divProformaInvoiceTerms').hide();
    }
}

function updateSystem() {
    var det = {
        DatatablePageEntries: $('#ddlDatatablePageEntries').val(),
        ShowHelpText: $('#chkShowHelpText').is(':checked'),
        EnableDarkMode: $('#chkEnableDarkMode').is(':checked'),
        FixedHeader: $('#chkFixedHeader').is(':checked'),
        FixedFooter: $('#chkFixedFooter').is(':checked'),
        EnableSound: $('#chkEnableSound').is(':checked'),
        CollapseSidebar: $('#chkCollapseSidebar').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/SystemUpdate',
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
                if ($('#chkEnableDarkMode').is(':checked')) {
                    $('body').addClass('dark-mode');
                    $('.divShortcutKeys').removeClass('control-sidebar-light');
                    $('.divShortcutKeys').addClass('control-sidebar-dark');
                }
                else {
                    $('body').removeClass('dark-mode');
                    $('.divShortcutKeys').addClass('control-sidebar-light');
                    $('.divShortcutKeys').removeClass('control-sidebar-dark');
                }

                if ($('#chkFixedHeader').is(':checked')) {
                    $('body').addClass('layout-navbar-fixed');
                }
                else {
                    $('body').removeClass('layout-navbar-fixed');
                }

                if ($('#chkFixedFooter').is(':checked')) {
                    $('body').addClass('layout-footer-fixed');
                }
                else {
                    $('body').removeClass('layout-footer-fixed');
                }

                if ($('#chkEnableSound').is(':checked')) {
                    EnableSound = 'True';
                }
                else {
                    EnableSound = 'False';
                }

                if ($('#chkCollapseSidebar').is(':checked')) {
                    $('body').addClass('sidebar-collapse');
                }
                else {
                    $('body').removeClass('sidebar-collapse');
                }

                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function selectAll() {
    if ($('#chkSelectAll').is(':checked')) {
        $('.chkNotificationModule').prop('checked', true);
    } else {
        $('.chkNotificationModule').prop('checked', false);
    }
}

function sendTestEmail(EmailSettingsId) {
    var det = {
        EmailSettingsId: EmailSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/sendTestEmail',
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
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleSmsService(r) {
    $('.divTwilio').hide();
    $('.divNexmo').hide();
    $('.divOthers').hide();
    if ($('#ddlSmsService' + r).val() == 1) {
        $('.divTwilio').show();
    }
    if ($('#ddlSmsService' + r).val() == 2) {
        $('.divNexmo').show();
    }
    if ($('#ddlSmsService' + r).val() == 3) {
        $('.divOthers').show();
        toggleRequestMethod(r);
    }
    $('#ddlRequestMethod' + r).val(1);
    clearSmsSettings();
}

function toggleRequestMethod(r) {
    if ($('#ddlRequestMethod' + r).val() == 2) {
        $('.divHeader').show();
    }
    else {
        $('.divHeader').hide();
    }
    clearSmsSettings();
}

function toggleWhatsappService(r) {
    if ($('#ddlWhatsappService' + r).val() == 1) {
        $('.divWTwilio').hide();
        $('.divDesktop').show();
        $('#txtWTwilioAccountSID').val('');
        $('#txtWTwilioAuthToken').val('');
        $('#txtWTwilioFrom').val('');
    }
    else if ($('#ddlWhatsappService' + r).val() == 2) {
        $('.divWTwilio').show();
        $('.divDesktop').hide();
    }
    else {
        $('.divWTwilio').hide();
        $('.divDesktop').hide();
    }
}

function sendTestWhatsapp(WhatsappSettingsId) {
    var det = {
        WhatsappSettingsId: WhatsappSettingsId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/sendTestWhatsapp',
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
                if (data.Status == 1) {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                        window.open(data.WhatsappUrl);
                    }
                }
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchOnlinePaymentSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/OnlinePaymentSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOnlinePayments").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function OnlinePaymentSetting(OnlinePaymentSettingsId) {
    _OnlinePaymentSettingsId = OnlinePaymentSettingsId;
    var det = {
        OnlinePaymentSettingsId: OnlinePaymentSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/settings1/OnlinePaymentSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOnlinePaymentsEdit").html(data);
            $("#divLoading").hide();
            toggleOnlinePayment(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertOnlinePaymentSettings() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        OnlinePaymentService: $('#ddlOnlinePaymentService').val(),
        PaypalClientId: $('#txtPaypalClientId').val(),
        PaypalCurrencyId: $('#ddlPaypalCurrency').val(),
        RazorpayKey: $('#txtRazorpayKey').val(),
        RazorpayCurrencyId: $('#ddlRazorpayCurrency').val(),
        IsDefault: $('#chkPaymentServiceIsDefault').is(':checked'),
        SaveAs: $('#txtPaymentServiceSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/InsertOnlinePaymentSettings',
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
                toggleOnlinePayment(2);
                fetchOnlinePaymentSettingsList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateOnlinePaymentSettings() {
    $('.errorText').hide();
    var det = {
        OnlinePaymentSettingsId: _OnlinePaymentSettingsId,
        OnlinePaymentService: $('#ddlOnlinePaymentService_E').val(),
        PaypalClientId: $('#txtPaypalClientId_E').val(),
        PaypalCurrencyId: $('#ddlPaypalCurrency_E').val(),
        RazorpayKey: $('#txtRazorpayKey_E').val(),
        RazorpayCurrencyId: $('#ddlRazorpayCurrency_E').val(),
        IsDefault: $('#chkPaymentServiceIsDefault_E').is(':checked'),
        SaveAs: $('#txtPaymentServiceSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdateOnlinePaymentSettings',
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
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleOnlinePayment(2);
                fetchOnlinePaymentSettingsList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearOnlinePaymentSettings() {
    $('#txtPaypalClientId').val('');
    $('#ddlPaypalCurrency').val(0);
    $('#txtRazorpayKey').val('');
    $('#ddlRazorpayCurrency').val(0);
    $('.select2').select2();
}

function toggleOnlinePaymentService() {
    $('.divRazorpay').hide();
    $('.divPaypal').hide();

    if ($('#ddlOnlinePaymentService').val() == 1) {
        $('.divPaypal').show();
    }
    else if ($('#ddlOnlinePaymentService').val() == 2) {
        $('.divRazorpay').show();
    }
    clearOnlinePaymentSettings();
}

function toggleOnlinePayment(t) {
    if (t == 1) {
        $('.divOnlinePayments').hide();
        $('.divOnlinePaymentsNew').show();
        $('.divOnlinePaymentsEdit').hide();
        toggleOnlinePaymentService();
    }
    else if (t == 2) {
        $('.divOnlinePayments').show();
        $('.divOnlinePaymentsNew').hide();
        $('.divOnlinePaymentsEdit').hide();
        toggleOnlinePaymentService();
    }
    else if (t == 3) {
        $('.divOnlinePayments').hide();
        $('.divOnlinePaymentsNew').hide();
        $('.divOnlinePaymentsEdit').show();
    }
}

function DeleteOnlinePayment(OnlinePaymentSettingsId, OnlinePaymentService) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            OnlinePaymentSettingsId: OnlinePaymentSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/OnlinePaymentSettingsDelete',
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
                    fetchOnlinePaymentSettingsList();
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function insertUnit() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        UnitCode: $('#txtUnitCode').val(),
        UnitName: $('#txtUnitName').val(),
        UnitShortName: $('#txtUnitShortName').val(),
        AllowDecimal: $('#ddlAllowDecimal').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/UnitInsert',
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
                });
            }
            else {
                $('#ddlDefaultUnit').append($('<option>', { value: data.Data.Unit.UnitId, text: data.Data.Unit.UnitName }));
                $('#ddlDefaultUnit').val(data.Data.Unit.UnitId);
                $('#unitModal').modal('toggle');

                $('#txtUnitName').val('');
                $('#txtUnitShortName').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleSubCategory() {
    if (!$('#chkEnableSubCategory').is(':checked')) {
        $('#chkEnableSubSubCategory').prop('checked', false);
    }
}

function toggleSubSubCategory() {
    if ($('#chkEnableSubSubCategory').is(':checked')) {
        $('#chkEnableSubCategory').prop('checked', true);
    }
}

function toggleSecondaryUnit() {
    if (!$('#chkEnableSecondaryUnit').is(':checked')) {
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#chkEnableQuaternaryUnit').prop('checked', false);
    }
    else {
        $('#chkEnableLotNo').prop('checked', true);
    }
}

function toggleTertiaryUnit() {
    if (!$('#chkEnableTertiaryUnit').is(':checked')) {
        $('#chkEnableQuaternaryUnit').prop('checked', false);
    }
    else {
        $('#chkEnableSecondaryUnit').prop('checked', true);

        $('#chkEnableLotNo').prop('checked', true);
    }
}

function toggleQuaternaryUnit() {
    if ($('#chkEnableQuaternaryUnit').is(':checked')) {
        $('#chkEnableTertiaryUnit').prop('checked', true);
        $('#chkEnableSecondaryUnit').prop('checked', true);

        $('#chkEnableLotNo').prop('checked', true);
    }
}

function toggleEnableRacks() {
    if (!$('#chkEnableRacks').is(':checked')) {
        $('#chkEnableRow').prop('checked', false);
        $('#chkEnablePosition').prop('checked', false);
    }
}

function toggleEnableRow() {
    if (!$('#chkEnableRow').is(':checked')) {
        $('#chkEnablePosition').prop('checked', false);
    }
    else {
        $('#chkEnableRacks').prop('checked', true);
    }
}

function toggleEnablePosition() {
    if ($('#chkEnablePosition').is(':checked')) {
        $('#chkEnableRow').prop('checked', true);
        $('#chkEnableRacks').prop('checked', true);
    }
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

function TimeZone() {
    var det = {
        TimeZoneId: $('#ddlTimeZone').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/TimeZone',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (data.Data.TimeZone.SupportsDaylightSavingTime == true) {
                    $('.divEnableDaylightSavingTime').show();
                    $('#chkEnableDaylightSavingTime').prop('checked', true);
                }
                else {
                    $('.divEnableDaylightSavingTime').hide();
                    $('#chkEnableDaylightSavingTime').prop('checked', false);
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function CheckTime() {
    var det = {
        TimeZoneId: $('#ddlTimeZone').val(),
        EnableDaylightSavingTime: $('#chkEnableDaylightSavingTime').is(':checked'),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/settings1/CheckTime',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                alert(data.Message);
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchEmailSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/EmailSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divEmailSettings").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function EmailSetting(EmailSettingsId) {
    _EmailSettingsId = EmailSettingsId;
    var det = {
        EmailSettingsId: EmailSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/settings1/EmailSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divEmailSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleEmailSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertEmailSettings() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        SmtpServer: $('#txtSmtpServer').val(),
        SmtpPort: $('#txtSmtpPort').val(),
        SmtpUser: $('#txtSmtpUser').val(),
        SmtpPass: $('#txtSmtpPass').val(),
        EnableSsl: $('#chkEnableSsl').is(':checked'),
        FromName: $('#txtFromName').val(),
        TestEmailId: $('#txtTestEmailId').val(),
        IsDefault: $('#chkEmailIsDefault').is(':checked'),
        SaveAs: $('#txtEmailSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/InsertEmailSettings',
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
                toggleEmailSetting(2);
                fetchEmailSettingsList();
                clearEmailSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateEmailSettings() {
    $('.errorText').hide();
    var det = {
        EmailSettingsId: _EmailSettingsId,
        SmtpServer: $('#txtSmtpServer_E').val(),
        SmtpPort: $('#txtSmtpPort_E').val(),
        SmtpUser: $('#txtSmtpUser_E').val(),
        SmtpPass: $('#txtSmtpPass_E').val(),
        EnableSsl: $('#chkEnableSsl_E').is(':checked'),
        FromName: $('#txtFromName_E').val(),
        TestEmailId: $('#txtTestEmailId_E').val(),
        IsDefault: $('#chkEmailIsDefault_E').is(':checked'),
        SaveAs: $('#txtEmailSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdateEmailSettings',
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
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleEmailSetting(2);
                fetchEmailSettingsList();
                clearEmailSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearEmailSettings() {
    $('#txtSmtpServer').val('');
    $('#txtSmtpPort').val('');
    $('#txtSmtpUser').val('');
    $('#txtSmtpPass').val('');
    $('#chkEnableSsl').prop('checked', false);
    $('#txtFromName').val('');
    $('#txtTestEmailId').val('');
    $('#txtEmailSaveAs').val('');
    $('#chkIsDefault').prop('checked', false);
    $('.select2').select2();
}

function toggleEmailSetting(t) {
    if (t == 1) {
        $('.divEmailSettings').hide();
        $('.divEmailSettingsNew').show();
        $('.divEmailSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divEmailSettings').show();
        $('.divEmailSettingsNew').hide();
        $('.divEmailSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divEmailSettings').hide();
        $('.divEmailSettingsNew').hide();
        $('.divEmailSettingsEdit').show();
    }
}

function DeleteEmailSetting(EmailSettingsId, SaveAs) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            EmailSettingsId: EmailSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/EmailSettingsDelete',
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
                    fetchEmailSettingsList();
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function fetchSmsSettingsList() {
    var det = {

    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/settings1/smssettingsfetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divSmsSettings").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function SmsSetting(SmsSettingsId) {
    _SmsSettingsId = SmsSettingsId;
    var det = {
        SmsSettingsId: SmsSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/settings1/SmsSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divSmsSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleSmsSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertSmsSettings() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        SmsService: $('#ddlSmsService').val(),
        RequestMethod: $('#ddlRequestMethod').val(),
        Url: $('#txtUrl').val(),
        SendToParameterName: $('#txtSendToParameterName').val(),
        MessageParameterName: $('#txtMessageParameterName').val(),
        Header1Key: $('#txtHeader1Key').val(),
        Header1Value: $('#txtHeader1Value').val(),
        Header2Key: $('#txtHeader2Key').val(),
        Header2Value: $('#txtHeader2Value').val(),
        Header3Key: $('#txtHeader3Key').val(),
        Header3Value: $('#txtHeader3Value').val(),
        Header4Key: $('#txtHeader4Key').val(),
        Header4Value: $('#txtHeader4Value').val(),
        Parameter1Key: $('#txtParameter1Key').val(),
        Parameter1Value: $('#txtParameter1Value').val(),
        Parameter2Key: $('#txtParameter2Key').val(),
        Parameter2Value: $('#txtParameter2Value').val(),
        Parameter3Key: $('#txtParameter3Key').val(),
        Parameter3Value: $('#txtParameter3Value').val(),
        Parameter4Key: $('#txtParameter4Key').val(),
        Parameter4Value: $('#txtParameter4Value').val(),
        Parameter5Key: $('#txtParameter5Key').val(),
        Parameter5Value: $('#txtParameter5Value').val(),
        Parameter6Key: $('#txtParameter6Key').val(),
        Parameter6Value: $('#txtParameter6Value').val(),
        Parameter7Key: $('#txtParameter7Key').val(),
        Parameter7Value: $('#txtParameter7Value').val(),
        Parameter8Key: $('#txtParameter8Key').val(),
        Parameter8Value: $('#txtParameter8Value').val(),
        Parameter9Key: $('#txtParameter9Key').val(),
        Parameter9Value: $('#txtParameter9Value').val(),
        Parameter10Key: $('#txtParameter10Key').val(),
        Parameter10Value: $('#txtParameter10Value').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        TwilioAccountSID: $('#txtTwilioAccountSID').val(),
        TwilioAuthToken: $('#txtTwilioAuthToken').val(),
        TwilioFrom: $('#txtTwilioFrom').val(),
        NexmoApiKey: $('#txtNexmoApiKey').val(),
        NexmoApiSecret: $('#txtNexmoApiSecret').val(),
        NexmoFrom: $('#txtNexmoFrom').val(),
        EnableCountryCode: $('#chkEnableCountryCode').is(':checked'),
        TestMobileNo: $('#txtTestMobileNo').val(),
        IsDefault: $('#chkSmsIsDefault').is(':checked'),
        SaveAs: $('#txtSmsSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/InsertSmsSettings',
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
                toggleSmsSetting(2);
                fetchSmsSettingsList();
                clearSmsSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateSmsSettings() {
    $('.errorText').hide();
    var det = {
        SmsSettingsId: _SmsSettingsId,
        SmsService: $('#ddlSmsService_E').val(),
        RequestMethod: $('#ddlRequestMethod_E').val(),
        Url: $('#txtUrl_E').val(),
        SendToParameterName: $('#txtSendToParameterName_E').val(),
        MessageParameterName: $('#txtMessageParameterName_E').val(),
        Header1Key: $('#txtHeader1Key_E').val(),
        Header1Value: $('#txtHeader1Value_E').val(),
        Header2Key: $('#txtHeader2Key_E').val(),
        Header2Value: $('#txtHeader2Value_E').val(),
        Header3Key: $('#txtHeader3Key_E').val(),
        Header3Value: $('#txtHeader3Value_E').val(),
        Header4Key: $('#txtHeader4Key_E').val(),
        Header4Value: $('#txtHeader4Value_E').val(),
        Parameter1Key: $('#txtParameter1Key_E').val(),
        Parameter1Value: $('#txtParameter1Value_E').val(),
        Parameter2Key: $('#txtParameter2Key_E').val(),
        Parameter2Value: $('#txtParameter2Value_E').val(),
        Parameter3Key: $('#txtParameter3Key_E').val(),
        Parameter3Value: $('#txtParameter3Value_E').val(),
        Parameter4Key: $('#txtParameter4Key_E').val(),
        Parameter4Value: $('#txtParameter4Value_E').val(),
        Parameter5Key: $('#txtParameter5Key_E').val(),
        Parameter5Value: $('#txtParameter5Value_E').val(),
        Parameter6Key: $('#txtParameter6Key_E').val(),
        Parameter6Value: $('#txtParameter6Value_E').val(),
        Parameter7Key: $('#txtParameter7Key_E').val(),
        Parameter7Value: $('#txtParameter7Value_E').val(),
        Parameter8Key: $('#txtParameter8Key_E').val(),
        Parameter8Value: $('#txtParameter8Value_E').val(),
        Parameter9Key: $('#txtParameter9Key').val(),
        Parameter9Value: $('#txtParameter9Value_E').val(),
        Parameter10Key: $('#txtParameter10Key_E').val(),
        Parameter10Value: $('#txtParameter10Value_E').val(),
        IsActive: $('#chkIsActive_E').is(':checked'),
        TwilioAccountSID: $('#txtTwilioAccountSID_E').val(),
        TwilioAuthToken: $('#txtTwilioAuthToken_E').val(),
        TwilioFrom: $('#txtTwilioFrom_E').val(),
        NexmoApiKey: $('#txtNexmoApiKey_E').val(),
        NexmoApiSecret: $('#txtNexmoApiSecret_E').val(),
        NexmoFrom: $('#txtNexmoFrom_E').val(),
        EnableCountryCode: $('#chkEnableCountryCode_E').is(':checked'),
        TestMobileNo: $('#txtTestMobileNo_E').val(),
        IsDefault: $('#chkSmsIsDefault_E').is(':checked'),
        SaveAs: $('#txtSmsSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdateSmsSettings',
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
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleSmsSetting(2);
                fetchSmsSettingsList();
                clearSmsSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearSmsSettings() {
    $('#txtUrl').val("");
    $('#txtSendToParameterName').val("");
    $('#txtMessageParameterName').val("");
    $('#txtHeader1Key').val("");
    $('#txtHeader1Value').val("");
    $('#txtHeader2Key').val("");
    $('#txtHeader2Value').val("");
    $('#txtHeader3Key').val("");
    $('#txtHeader3Value').val("");
    $('#txtHeader4Key').val("");
    $('#txtHeader4Value').val("");
    $('#txtParameter1Key').val("");
    $('#txtParameter1Value').val("");
    $('#txtParameter2Key').val("");
    $('#txtParameter2Value').val("");
    $('#txtParameter3Key').val("");
    $('#txtParameter3Value').val("");
    $('#txtParameter4Key').val("");
    $('#txtParameter4Value').val("");
    $('#txtParameter5Key').val("");
    $('#txtParameter5Value').val("");
    $('#txtParameter6Key').val("");
    $('#txtParameter6Value').val("");
    $('#txtParameter7Key').val("");
    $('#txtParameter7Value').val("");
    $('#txtParameter8Key').val("");
    $('#txtParameter8Value').val("");
    $('#txtParameter9Key').val("");
    $('#txtParameter9Value').val("");
    $('#txtParameter10Key').val("");
    $('#txtParameter10Value').val("");
    $('#txtTwilioAccountSID').val("");
    $('#txtTwilioAuthToken').val("");
    $('#txtTwilioFrom').val("");
    $('#txtNexmoApiKey').val("");
    $('#txtNexmoApiSecret').val("");
    $('#txtNexmoFrom').val("");
}

function toggleSmsSetting(t) {
    if (t == 1) {
        $('.divSmsSettings').hide();
        $('.divSmsSettingsNew').show();
        $('.divSmsSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divSmsSettings').show();
        $('.divSmsSettingsNew').hide();
        $('.divSmsSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divSmsSettings').hide();
        $('.divSmsSettingsNew').hide();
        $('.divSmsSettingsEdit').show();
    }
}

function DeleteSmsSetting(SmsSettingsId, SaveAs) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SmsSettingsId: SmsSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/SmsSettingsDelete',
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
                    fetchSmsSettingsList();
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function toggleSmsBranding() {
    businessInformationUpdate();
    if ($('#chkEnableDefaultSmsBranding').is(':checked')) {
        $('.divCustomSmsBranding').hide();
    }
    else {
        $('.divCustomSmsBranding').show();
    }
}

function toggleEmailBranding() {
    businessInformationUpdate();
    if ($('#chkEnableDefaultEmailBranding').is(':checked')) {
        $('.divCustomEmailBranding').hide();
    }
    else {
        $('.divCustomEmailBranding').show();
    }
}

function fetchWhatsappSettingsList() {
    var det = {

    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/settings1/WhatsappSettingsfetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divWhatsappSettings").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function WhatsappSetting(WhatsappSettingsId) {
    _WhatsappSettingsId = WhatsappSettingsId;
    var det = {
        WhatsappSettingsId: WhatsappSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/settings1/WhatsappSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divWhatsappSettingsEdit").html(data);
            $("#divLoading").hide();
            toggleWhatsappSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function InsertWhatsappSettings() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        WhatsappService: $('#ddlWhatsappService').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        TwilioAccountSID: $('#txtWTwilioAccountSID').val(),
        TwilioAuthToken: $('#txtWTwilioAuthToken').val(),
        TwilioFrom: $('#txtWTwilioFrom').val(),
        TestMobileNo: $('#txtWTestMobileNo').val(),
        IsDefault: $('#chkWhatsappIsDefault').is(':checked'),
        SaveAs: $('#txtWhatsappSaveAs').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/InsertWhatsappSettings',
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
                toggleWhatsappSetting(2);
                fetchWhatsappSettingsList();
                clearWhatsappSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdateWhatsappSettings() {
    $('.errorText').hide();
    var det = {
        WhatsappSettingsId: _WhatsappSettingsId,
        WhatsappService: $('#ddlWhatsappService_E').val(),
        IsActive: $('#chkIsActive_E').is(':checked'),
        TwilioAccountSID: $('#txtWTwilioAccountSID_E').val(),
        TwilioAuthToken: $('#txtWTwilioAuthToken_E').val(),
        TwilioFrom: $('#txtWTwilioFrom_E').val(),
        TestMobileNo: $('#txtWTestMobileNo_E').val(),
        IsDefault: $('#chkWhatsappIsDefault_E').is(':checked'),
        SaveAs: $('#txtWhatsappSaveAs_E').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdateWhatsappSettings',
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
                    $('#' + res.Id + '_E').show();
                    $('#' + res.Id + '_E').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                toggleWhatsappSetting(2);
                fetchWhatsappSettingsList();
                clearWhatsappSettings();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearWhatsappSettings() {
    $('#ddlWhatsappService').val(0);
    $('#txtWTestMobileNo').val("");
    $('#txtWTwilioAccountSID').val("");
    $('#txtWTwilioAuthToken').val("");
    $('#txtWTwilioFrom').val("");
    $('#txtWhatsappSaveAs').val("");
    $('#chkWhatsappIsDefault').prop("checked", false);
    $('.select2').select2();
    toggleWhatsappService('');
}

function toggleWhatsappSetting(t) {
    if (t == 1) {
        $('.divWhatsappSettings').hide();
        $('.divWhatsappSettingsNew').show();
        $('.divWhatsappSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divWhatsappSettings').show();
        $('.divWhatsappSettingsNew').hide();
        $('.divWhatsappSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divWhatsappSettings').hide();
        $('.divWhatsappSettingsNew').hide();
        $('.divWhatsappSettingsEdit').show();
    }
}

function DeleteWhatsappSetting(WhatsappSettingsId, SaveAs) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            WhatsappSettingsId: WhatsappSettingsId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/WhatsappSettingsDelete',
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
                    fetchWhatsappSettingsList();
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function checkLotNo() {
    if ($('#chkEnableLotNo').is(':checked')) {
        $('.divLot').show();
    }
    else {
        $('#chkEnableItemExpiry').prop('checked', false);
        $('#chkEnableSecondaryUnit').prop('checked', false);
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#chkEnableQuaternaryUnit').prop('checked', false);

        $('#chkSalesEnableFreeQuantity').prop('checked', false);
        $('#chkPosEnableFreeQuantity').prop('checked', false);
        $('#chkEnableFreeQuantity').prop('checked', false);
        $('.divLot').hide();
    }
}

function fetchPrefix() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PrefixFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialPrefix").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertCurrency(i) {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        CurrencyId: $('#ddlCurrency').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/currencyMappingInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                $('#ddlPaypalCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlPaypalCurrency').val(data.Data.Currency.CurrencyId);

                $('#ddlRazorpayCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlRazorpayCurrency').val(data.Data.Currency.CurrencyId);

                $('#ddlPaypalCurrency_E').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlPaypalCurrency_E').val(data.Data.Currency.CurrencyId);

                $('#ddlRazorpayCurrency_E').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlRazorpayCurrency_E').val(data.Data.Currency.CurrencyId);

                $('#currencyModal').modal('toggle');

                $('#ddlCurrency').val(0);
                $('#txtExchangeRate').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setCurrencyName() {
    $('#txtCurrency').text($("#ddlCurrency").children("option").filter(":selected").text());
}

function fetchOpeningBalance() {
    var det = {
        BranchId: $('#ddlBranch').val()
    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/settings1/OpeningBalanceFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOpeningBalance").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function UpdateOpeningBalance() {
    canSubmit = true;

    if (($('#lblDebitDifference').text().replace(/[^0-9.]/g, '') != '' && $('#lblDebitDifference').text().replace(/[^0-9.]/g, '') != '0')
        || ($('#lblCreditDifference').text().replace(/[^0-9.]/g, '') != '' && $('#lblCreditDifference').text().replace(/[^0-9.]/g, '') != '0')) {
        if (confirm('The total debits and credits does not match. You can adjust the balances to remove the difference, or you can continue and the difference will be transferred to the Opening Balance Adjustment account automatically. Do you want to continue?')) {
            canSubmit = true;
        }
        else {
            canSubmit = false;
        }
    }

    if (canSubmit == true) {
        $('.errorText').hide();
        var accountOpeningBalances = [];
        $('.divAccountsOpeningBalance tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {
                accountOpeningBalances.push({
                    AccountOpeningBalanceId: $('#hdnAccountOpeningBalanceId' + _id).val(),
                    AccountId: _id,
                    Debit: $('#txtDebit' + _id).val(),
                    Credit: $('#txtCredit' + _id).val(),
                    BranchId: $('#ddlBranch').val(),
                })
            }
        });

        accountOpeningBalances.push({
            AccountOpeningBalanceId: $('#hdnAccountOpeningBalanceId').val(),
            AccountId: $('#hdnOpeningBalanceAdjustments').val(),
            Debit: $('#lblDebitDifference').text().replace(/[^0-9.]/g, ''),
            Credit: $('#lblCreditDifference').text().replace(/[^0-9.]/g, ''),
            BranchId: $('#ddlBranch').val(),
        })

        var det = {
            MigrationDate: $('#txtMigrationDate').val(),
            AccountOpeningBalances: accountOpeningBalances
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/UpdateOpeningBalance',
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
                    fetchOpeningBalance();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function updateOpeningBalanceTotal(id, type) {
    var totalDebit = 0, totalCredit = 0;

    if (type == 0) {
        $('#txtCredit' + id).val('');
    }
    else {
        $('#txtDebit' + id).val('');
    }

    $('.divAccountsOpeningBalance tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if ($('#txtDebit' + _id).val()) {
            totalDebit = totalDebit + parseFloat($('#txtDebit' + _id).val());
        }

        if ($('#txtCredit' + _id).val()) {
            totalCredit = totalCredit + parseFloat($('#txtCredit' + _id).val());
        }

    });

    $('#lblSubTotalDebit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
    $('#lblSubTotalCredit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);

    //$('#lblDebitDifference').show();
    //$('#lblCreditDifference').show();

    if (totalDebit > totalCredit) {
        $('#lblCreditDifference').text(CurrencySymbol + Math.round((totalDebit - totalCredit) * 100) / 100);
        $('#lblDebitDifference').text('');
        $('#lblTotalCredit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
        $('#lblTotalDebit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
        //$('#lblDebitDifference').hide();
        //$('#divDifference').show();
    }
    else if (totalCredit > totalDebit) {
        $('#lblDebitDifference').text(CurrencySymbol + Math.round((totalCredit - totalDebit) * 100) / 100);
        $('#lblCreditDifference').text('');
        $('#lblTotalCredit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);
        $('#lblTotalDebit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);
        //$('#lblCreditDifference').hide();
        //$('#divDifference').show();
    }
    else {
        $('#lblDebitDifference').text('');
        $('#lblCreditDifference').text('');
        //$('#divDifference').hide();
    }
}

function DeleteOpeningBalance() {
    if (confirm('Are you sure you want to delete all opening balances for ' + $("#ddlBranch option:selected").text() + '?')) {
        var det = {
            BranchId: $("#ddlBranch").val()
        };
        $("#divLoading").show();
        $.ajax({
            url: '/settings1/DeleteOpeningBalance',
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
                        $('#' + res.Id + '_E').show();
                        $('#' + res.Id + '_E').text(res.Message);
                    });
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchOpeningBalance();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }

}

function checkBranchCount() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/master/BranchCount',
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
                if (data.Data.TotalCount == 1) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error("You should have atleast 2 active Business Locations to enable this Stock Transfer feature");

                    $('#chkEnableStockTransfer').prop('checked', false);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openPaymentTermModal(id) {
    _id = id;
    $('#paymentTermModal').modal('toggle');
}

function insertPaymentTerm() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        PaymentTerm: $('#txtPaymentTerm').val(),
        Days: $('#txtDays').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/paymentTermInsert',
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
                $('#ddlSalesPaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPurchasePaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));

                $('#' + _id).val(data.Data.PaymentTerm.PaymentTermId);

                $('#paymentTermModal').modal('toggle');

                $('#txtPaymentTerm').val('');
                $('#txtDays').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getPwaImageBase64(imageType) {
    var file1 = $("#" + imageType).prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProfilePic").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            if (imageType == 'Image_48_48') {
                Image_48_48 = reader.result;
                FileExtensionImage_48_48 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_72_72') {
                Image_72_72 = reader.result;
                FileExtensionImage_72_72 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_96_96') {
                Image_96_96 = reader.result;
                FileExtensionImage_96_96 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_128_128') {
                Image_128_128 = reader.result;
                FileExtensionImage_128_128 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_144_144') {
                Image_144_144 = reader.result;
                FileExtensionImage_144_144 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_152_152') {
                Image_152_152 = reader.result;
                FileExtensionImage_152_152 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_192_192') {
                Image_192_192 = reader.result;
                FileExtensionImage_192_192 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_284_284') {
                Image_284_284 = reader.result;
                FileExtensionImage_284_284 = '.' + file1.name.split('.').pop();
            }
            else if (imageType == 'Image_512_512') {
                Image_512_512 = reader.result;
                FileExtensionImage_512_512 = '.' + file1.name.split('.').pop();
            }

            $('#blah' + imageType).attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function fetchPwaSettingsList() {
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PwaSettingsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPwaSettings").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function PwaSetting(PwaSettingsId) {
    _PwaSettingsId = PwaSettingsId;
    var det = {
        PwaSettingsId: PwaSettingsId,
    };

    $("#divLoading").show();
    $.ajax({
        url: '/settings1/PwaSetting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPwaSettingsEdit").html(data);
            $("#divLoading").hide();
            togglePwaSetting(3);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function updatePwaSettings() {
    $('.errorText').hide();
    var det = {
        PwaSettingsId: _PwaSettingsId,
        PwaName: $('#txtPwaName').val(),
        PwaShortName: $('#txtPwaShortName').val(),
        BackgroundColor: $('#txtBackgroundColor').val(),
        ThemeColor: $('#txtThemeColor').val(),
        Image_48_48: Image_48_48,
        FileExtensionImage_48_48: FileExtensionImage_48_48,
        Image_72_72: Image_72_72,
        FileExtensionImage_72_72: FileExtensionImage_72_72,
        Image_96_96: Image_96_96,
        FileExtensionImage_96_96: FileExtensionImage_96_96,
        Image_128_128: Image_128_128,
        FileExtensionImage_128_128: FileExtensionImage_128_128,
        Image_144_144: Image_144_144,
        FileExtensionImage_144_144: FileExtensionImage_144_144,
        Image_152_152: Image_152_152,
        FileExtensionImage_152_152: FileExtensionImage_152_152,
        Image_192_192: Image_192_192,
        FileExtensionImage_192_192: FileExtensionImage_192_192,
        Image_284_284: Image_284_284,
        FileExtensionImage_284_284: FileExtensionImage_284_284,
        Image_512_512: Image_512_512,
        FileExtensionImage_512_512: FileExtensionImage_512_512,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/UpdatePwaSettings',
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
                togglePwaSetting(2);
                fetchPwaSettingsList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function togglePwaSetting(t) {
    if (t == 1) {
        $('.divPwaSettings').hide();
        $('.divPwaSettingsNew').show();
        $('.divPwaSettingsEdit').hide();
    }
    else if (t == 2) {
        $('.divPwaSettings').show();
        $('.divPwaSettingsNew').hide();
        $('.divPwaSettingsEdit').hide();
    }
    else if (t == 3) {
        $('.divPwaSettings').hide();
        $('.divPwaSettingsNew').hide();
        $('.divPwaSettingsEdit').show();
    }
}

function toggleMileage() {
    if ($('#chkEnableMileage').is(':checked') == true) {
        $('.divMileage').show();
    }
    else {
        $('.divMileage').hide();
    }
}