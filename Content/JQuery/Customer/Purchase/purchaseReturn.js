$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');


    $('#txtDateRange').daterangepicker({
        //timePicker: true,
        //timePickerIncrement: 1,
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_Date').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_DueDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_ExpiredBefore').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat });

    $('#_Date').addClass('notranslate');
    $('#_DueDate').addClass('notranslate');
    $('#_ExpiredBefore').addClass('notranslate');

    $('textarea#txtDescription').summernote({
        placeholder: '',
        tabsize: 2,
        height: 100,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            // ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            //['fontname', ['fontname']],
            // ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            //['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });

    if ($('#txtInvoiceNo').val()) {
        $('#txtInvoiceNo').prop('disabled', true);
    }

    //if (window.location.href.indexOf('PurchaseReturnCreate') > -1) {
    //    UsersBranchWise();
    //}

    if (window.location.href.toLowerCase().indexOf('purchasereturncreate') == -1) {
        convertAvailableStock();
    }
    else {
        fetchAdditionalCharges();
    }

    fetchTax();
    fetchTaxExemptions();
    fetchCompanyCurrency();
    //fetchItemCodes();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var CurrencySymbol = "";//Cookies.get('data').split('&')[5].split('=')[1];

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

    if (sessionStorage.getItem("InvoiceUrl")) {
        if (!!window.GestureEvent == true) {
            $('#invoiceModal').modal('show');
            $('#lblInvoice').attr('src', sessionStorage.getItem("InvoiceUrl"));
            sessionStorage.removeItem("InvoiceUrl");
        }
        else {
            const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
            if (isMobile) {
                var WinPrint = window.open(sessionStorage.getItem("InvoiceUrl"), "_blank", "");
                sessionStorage.removeItem("InvoiceUrl");
            }
            else {
                $.get(sessionStorage.getItem("InvoiceUrl"), function (data) {
                    //data = data.replace('noPrint', 'noPrint" style="display:none"')
                    const WinPrint = window.open(
                        sessionStorage.getItem("InvoiceUrl"),
                        "_blank",
                        "left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0"
                    );
                    WinPrint.document.write(data);
                    WinPrint.document.close();
                    WinPrint.focus();
                    setTimeout(
                        function () {
                            WinPrint.print();
                            WinPrint.close();
                            sessionStorage.removeItem("InvoiceUrl");
                        }, 1000);
                });
            }
        }
    }
}

var _PageIndex = 1, c = 1, taxModalId;
var AttachDocument = "";
var FileExtensionAttachDocument = "";
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";
var count = 1; var innerCount = 1; var dropdownHtml = '';
var _PurchaseReturnId = 0;
var _BranchId = 0;
var NotificationName = '', NotificationId = 0;
var skuCodes = [];
var taxExemptions = [];

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        Status: $('#ddlStatus').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseReturnFetch',
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

function View(PurchaseReturnId, SupplierPaymentId) {
    var det = {
        PurchaseReturnId: PurchaseReturnId,
        SupplierPaymentId: SupplierPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseReturnView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ViewModal').modal('toggle');
            $("#divView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPayment(SupplierRefundId) {
    var det = {
        SupplierRefundId: SupplierRefundId,
        Type: "Supplier Refund",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/RefundView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PaymentViewModal').modal('toggle');
            $("#divPaymentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchCompanyCurrency() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/usersettings/FetchCompanyCurrency',
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

function checkReturnedQuantity(i) {
    if (parseFloat($('#txtQuantityReturned' + i).val()) > parseFloat($('#txtQuantityRemaining' + i).val())) {
        $('#lblShowMsg' + i).text('Return quantity cannot be greater the remaining quantity');
        $('#txtQuantityReturned' + i).val($('#txtQuantityRemaining' + i).val());
    }
    else {
        updateAmount();
        $('#lblShowMsg' + i).text('');
    }
}

function fetchItemCodes() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/othersettings/ActiveItemCodes',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                itemCodes = data.Data.ItemCodes;
            }
        },
        error: function (xhr) {

        }
    });
};

function deleteCombo(id, PurchaseReturnDetailsId) {
    if (PurchaseReturnDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {

        var skuRemove = $('#txtSkuCode' + id).val();
        skuCodes = jQuery.grep(skuCodes, function (value) {
            return value.trim() != skuRemove.trim();
        });

        $('#divCombo' + id).hide();
        $('#txtUnitCost' + id).val(0);
        $('#ddlTax' + id).val($('#ddlTax' + id + ' option:first').val());
        $('#txtAmountExcTax' + id).val(0);
        $('#txtAmountIncTax' + id).val(0);
        $('#txtDiscAll').val(0);
        updateAmount();

        //var det = {
        //    PurchaseReturnDetailsId: PurchaseReturnDetailsId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/purchase/PurchaseReturnDetailsDelete',
        //    datatype: "json",
        //    data: det,
        //    type: "post",
        //    success: function (data) {

        //        $("#divLoading").hide();
        //        if (data == "True") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }
        //        else if (data == "False") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }

        //        if (data.Status == 0) {
        //            if (EnableSound == 'True') { document.getElementById('error').play(); }
        //            toastr.error(data.Message);
        //        }
        //        else {
        //            $('#divCombo' + id).remove();
        //            updateAmount();
        //            //update(false);
        //        }
        //    },
        //    error: function (xhr) {
        //        $("#divLoading").hide();
        //    }
        //});
        //}
    }
    else {
        //var skuRemove = $('#txtSkuCode' + id).val();
        //skuCodes = jQuery.grep(skuCodes, function (value) {
        //    return value.trim() != skuRemove.trim();
        //});

        $('#divCombo' + id).remove();
        //$('#txtUnitCost' + id).val(0);
        //$('#ddlTax' + id).val($('#ddlTax' + id + ' option:first').val());
        //$('#txtAmountExcTax' + id).val(0);
        //$('#txtAmountIncTax' + id).val(0);
        //$('#txtDiscAll').val(0);
        updateAmount();
    }
}

function deleteFullCombo(PurchaseReturnId) {
    if (PurchaseReturnId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        $('#divCombo tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {
                $('#divCombo' + _id).hide();
                $('#txtUnitCost' + _id).val(0);
                $('#ddlTax' + _id).val($('#ddlTax' + _id + ' option:first').val());
                $('#txtAmountExcTax' + _id).val(0);
                $('#txtAmountIncTax' + _id).val(0);
                $('#txtDiscAll').val(0);
            }
        });

        updateAmount();
        skuCodes = [];

        //var det = {
        //    PurchaseReturnId: PurchaseReturnId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/purchase/PurchaseReturnDetailsDelete',
        //    datatype: "json",
        //    data: det,
        //    type: "post",
        //    success: function (data) {

        //        $("#divLoading").hide();
        //        if (data == "True") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }
        //        else if (data == "False") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }

        //        if (data.Status == 0) {
        //            if (EnableSound == 'True') { document.getElementById('error').play(); }
        //            toastr.error(data.Message);
        //        }
        //        else {
        //            $('#divCombo').empty();
        //            //$('#divComboNetAmount').text(0);
        //            updateAmount();
        //            //update(false);
        //        }
        //    },
        //    error: function (xhr) {
        //        $("#divLoading").hide();
        //    }
        //});
        //}
    }
    else {
        $('#divCombo').empty();
        //$('#divComboNetAmount').text(0);

        //$('#divCombo' + _id).remove();
        //$('#txtUnitCost' + _id).val(0);
        //$('#ddlTax' + _id).val($('#ddlTax' + _id + ' option:first').val());
        //$('#txtAmountExcTax' + _id).val(0);
        //$('#txtAmountIncTax' + _id).val(0);
        //$('#txtDiscAll').val(0);

        updateAmount();
        skuCodes = [];
    }
}

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {

            var hiddenOptionsCount = 0;
            $("#ddlUnit" + _id + " option").each(function () {
                if ($(this)[0].outerHTML.indexOf('hidden') > -1) {
                    hiddenOptionsCount++;
                }
            });
            if ($("#divCombo" + _id).is(":hidden") == false) {
                ItemDetails.push({
                    DivId: _id,
                    PurchaseReturnDetailsId: $('#txtPurchaseReturnDetailsId' + _id).val(),
                    Quantity: $('#txtQuantityReturned' + _id).val(),
                    FreeQuantity: $('#txtFreeQuantityReturned' + _id).val(),
                    ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                    ItemId: $('#txtItemId' + _id).val(),
                    UnitCost: $('#txtUnitCost' + _id).val(),
                    Amount: $('#txtAmount' + _id).text(),
                    IsActive: true,
                    IsDeleted: $("#divCombo" + _id).is(":hidden"),
                    PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                    PurchaseDetailsId: $('#txtPurchaseDetailsId' + _id).val(),
                    //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                    //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                    UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                    TaxId: $('#ddlTax' + _id).val().split('-')[0],
                    DiscountType: $('#ddlDiscountType' + _id).val(),
                    Discount: $('#txtDiscount' + _id).val(),
                    PurchaseIncTax: $('#txtPurchaseIncTax' + _id).val(),
                    AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                    TaxAmount: $('#txtTaxAmount' + _id).val(),
                    TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                    PurchaseExcTax: $('#txtPurchaseExcTax' + _id).val(),
                    AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                    ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
                    ITCType: $('#ddlITCType' + _id).val()
                })
            }
            
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
        var count = index;
        var additionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
        var additionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());
        var taxId = $('#ddlAdditionalChargesTax' + count).val().split('-')[0];
        var additionalChargeId = $('#txtAdditionalChargeId' + count).val();
        var taxExemptionId = $('#exemptionReasonId' + count).val();
        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            ITCType: $('#itcTypeValue' + count).val(),
            TaxExemptionId: taxExemptionId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        //PurchaseId: $('#ddlPurchaseInvoice').val() == undefined ? window.location.href.split('=')[1] : $('#ddlPurchaseInvoice').val(),
        PurchaseReturnId: $("#hdnPurchaseReturnId").val(),
        PurchaseId: $('#ddlPurchaseInvoice').val(),
        BranchId: $('#ddlBranch').val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        IsActive: true,
        IsDeleted: false,
        Notes: $("#txtNotes").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        PurchaseReturnType: $("#ddlPurchaseStatus  option:selected").val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        SupplierId: $("#ddlSupplier").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TotalQuantity: $('#divTotalQty').text(),
        PurchaseReturnDetails: ItemDetails,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        ReturnType: $('#ddlReturnType').val(),
        ExpiredBefore: $('#txtExpiredBefore').val(),
        QuantityLessThan: $('#txtQuantityLessThan').val(),
        IsDirectReturn: window.location.href.indexOf('purchasereturnadd') != -1 ? true : false,
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        Status: $('#ddlStatus').val(),
        PurchaseDebitNoteReasonId: $('#ddlPurchaseDebitNoteReason').val(),
        CountryId: $('#hdnCountryId').val(),
        //Payment: {
        //    IsActive: true,
        //    IsDeleted: false,
        //    Notes: $('#txtPaymentNotes').val(),
        //    Amount: $('#txtAmount').val(),
        //    PaymentDate: $('#txtPaymentDate').val(),
        //    PaymentTypeId: $('#ddlPaymentType').val(),
        //    AttachDocument: PaymentAttachDocument,
        //    FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        //}
        PurchaseReturnAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseReturnInsert',
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
            }
            else {
                //if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                //    window.open(data.WhatsappUrl);
                //}

                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseReturn == true) {
                    sessionStorage.setItem('InvoiceUrl', '/purchase/PurchaseReturnInvoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                }

                if (i == 1) {
                    window.location.href = "/purchase/purchasereturn";
                }
                else {
                    window.location.href = "/purchase/PurchaseReturnCreate";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {

            var hiddenOptionsCount = 0;
            $("#ddlUnit" + _id + " option").each(function () {
                if ($(this)[0].outerHTML.indexOf('hidden') > -1) {
                    hiddenOptionsCount++;
                }
            });

            ItemDetails.push({
                DivId: _id,
                PurchaseReturnDetailsId: $('#txtPurchaseReturnDetailsId' + _id).val(),
                Quantity: $('#txtQuantityReturned' + _id).val(),
                FreeQuantity: $('#txtFreeQuantityReturned' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                Amount: $('#txtAmount' + _id).text(),
                IsActive: true,
                //IsDeleted: false,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                PurchaseDetailsId: $('#txtPurchaseDetailsId' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                DiscountType: $('#ddlDiscountType' + _id).val(),
                Discount: $('#txtDiscount' + _id).val(),
                PurchaseIncTax: $('#txtPurchaseIncTax' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                PurchaseExcTax: $('#txtPurchaseExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
                ITCType: $('#ddlITCType' + _id).val()
            })
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
        var count = index;
        var additionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
        var additionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());
        var taxId = $('#ddlAdditionalChargesTax' + count).val().split('-')[0];
        var additionalChargeId = $('#txtAdditionalChargeId' + count).val();
        var taxExemptionId = $('#exemptionReasonId' + count).val();
        var purchaseReturnAdditionalChargesId = $('#txtPurchaseReturnAdditionalChargesId' + count).val();
        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            ITCType: $('#itcTypeValue' + count).val(),
            TaxExemptionId: taxExemptionId,
            PurchaseReturnAdditionalChargesId: purchaseReturnAdditionalChargesId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        PurchaseReturnId: window.location.href.split('=')[1],
        //PurchaseId: $('#ddlPurchaseInvoice').val() == undefined ? window.location.href.split('=')[1] : $('#ddlPurchaseInvoice').val(),
        PurchaseId: $('#ddlPurchaseInvoice').val(),
        BranchId: $('#ddlBranch').val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        IsActive: true,
        IsDeleted: false,
        Notes: $("#txtNotes").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        PurchaseReturnType: $("#ddlPurchaseStatus  option:selected").val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        SupplierId: $("#ddlSupplier").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TotalQuantity: $('#divTotalQty').text(),
        PurchaseReturnDetails: ItemDetails,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        ReturnType: $('#ddlReturnType').val(),
        ExpiredBefore: $('#txtExpiredBefore').val(),
        QuantityLessThan: $('#txtQuantityLessThan').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        PurchaseDebitNoteReasonId: $('#ddlPurchaseDebitNoteReason').val(),
        Status: $('#ddlStatus').val(),
        PurchaseReturnAdditionalCharges: additionalCharges,
        CountryId: $('#hdnCountryId').val(),
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/purchasereturnUpdate',
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
            }
            else {
                //if (isBackToList == true) {
                //if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                //    window.open(data.WhatsappUrl);
                //}

                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseReturn == true) {
                    sessionStorage.setItem('InvoiceUrl', '/purchase/PurchaseReturnInvoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                }

                if (i == 1) {
                    window.location.href = "/purchase/purchasereturn";
                }
                else {
                    window.location.href = "/purchase/PurchaseReturnCreate";
                }
                //}
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getAttachDocumentBase64() {
    var file1 = $("#AttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#AttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            AttachDocument = reader.result;
            FileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function getPaymentAttachDocumentBase64() {
    var file1 = $("#PaymentAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#PaymentAttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            PaymentAttachDocument = reader.result;
            PaymentFileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahPaymentAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function Delete(PurchaseReturnId, SupplierPaymentId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            PurchaseReturnId: PurchaseReturnId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/purchase/PurchaseReturndelete',
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
                    DeleteSupplierPayment(SupplierPaymentId);
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function Cancel(PurchaseReturnId, SupplierPaymentId) {
    var r = confirm("This will cancel permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            PurchaseReturnId: PurchaseReturnId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/purchase/PurchaseReturncancel',
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
                    DeleteSupplierPayment(SupplierPaymentId);
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function openPaymentModal(type, PurchaseReturnId, title, BranchId) {
    _PurchaseReturnId = PurchaseReturnId;
    _BranchId = BranchId;
    var det = {
        PurchaseReturnId: PurchaseReturnId,
        Type: "Supplier Refund",
        Title: title,
        BranchId: _BranchId,
        IsAdvance: false
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseReturnPayments',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPayments").html(data);

            $("#paymentModal").modal('show');
            $('.divAdvanceBalance').hide();
            if (type == true) {
                $('.paymentAdd').show();
                $('.paymentList').hide();
                $('#paymentModalLabel').text('Add Payment');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
                $('#_PaymentDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });

            }
            else {
                $('.paymentAdd').hide();
                $('.paymentList').show();
                $('#paymentModalLabel').text('View Payments');
            }

            $('.select2').select2();

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertPayment() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    var det = {
        PurchaseReturnId: _PurchaseReturnId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Supplier Refund",
        AccountId: $('#ddlLAccount').val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PuchaseReturnPaymentInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);


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
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#paymentModal").modal('hide');

                $('#txtPaymentNotes').val('');
                $('#txtAmount').val(0);
                $('#txtPaymentDate').val('');
                $('#ddlPaymentType').val(0);
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('#ddlLAccount').val(0);

                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
            }
            //else {
            //    if (EnableSound == 'True') { document.getElementById('success').play(); }
            //    toastr.success(data.Message);
            //    fetchList();
            //    $("#paymentModal").modal('hide');
            //    if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
            //        window.open(data.WhatsappUrl);
            //    }
            //}
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deletePayment(SupplierPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            SupplierPaymentId: SupplierPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/Purchase/PurchasePaymentDelete',
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
                    $('#tr_' + SupplierPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}

function copyCode(url) {
    /* Get the text field */
    var copyText = url;

    navigator.clipboard
        .writeText(copyText)
        .then(() => {
            toastr.success("Copied");
        })
        .catch(() => {
            toastr.success("Something went wrong");
        });
}

function toggleUnit(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var index = $("#ddlUnit" + i)[0].selectedIndex + 1;
    var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
    var UToSValue = $('#hdnUToSValue' + i).val();
    var SToTValue = $('#hdnSToTValue' + i).val();
    var TToQValue = $('#hdnTToQValue' + i).val();
    //var PurchaseExcTax = $('#txtPurchaseExcTax' + i).val();
    //var PurchaseIncTax = $('#txtPurchaseIncTax' + i).val();
    var UnitCost = $('#txtUnitCost' + i).val();
    var SalesCost = $('#txtPurchaseIncTax' + i).val();
    var newUnitCost = 0, newSalesCost = 0;
    var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

    var PrimaryUnitCost = 0, SecondaryUnitCost = 0, TertiaryUnitCost = 0, QuaternaryUnitCost = 0;
    var PrimarySalesCost = 0, SecondarySalesCost = 0, TertiarySalesCost = 0, QuaternarySalesCost = 0;

    if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
        PrimaryUnitCost = UnitCost;
        PrimarySalesCost = SalesCost;
    }
    else if (SToTValue == 0 && TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;
        }
    }
    else if (TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesCost = SalesCost * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesCost = SalesCost / SToTValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesCost = SalesCost / UToSValue / SToTValue;
        }
    }
    else {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue * TToQValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue * TToQValue;

            SecondaryUnitCost = UnitCost * SToTValue * TToQValue;
            SecondarySalesCost = SalesCost * SToTValue * TToQValue;

            TertiaryUnitCost = UnitCost * TToQValue;
            TertiarySalesCost = SalesCost * TToQValue;

            QuaternaryUnitCost = UnitCost;
            QuaternarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesCost = SalesCost * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesCost = SalesCost;

            QuaternaryUnitCost = UnitCost / TToQValue;
            QuaternarySalesCost = SalesCost / TToQValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesCost = SalesCost / SToTValue;

            QuaternaryUnitCost = UnitCost / SToTValue / TToQValue;
            QuaternarySalesCost = SalesCost / SToTValue / TToQValue;
        }
        else if (PriceAddedFor == 1) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesCost = SalesCost / UToSValue / SToTValue;

            QuaternaryUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
            QuaternarySalesCost = SalesCost / UToSValue / SToTValue / TToQValue;
        }
    }

    if (index == 1) {
        newUnitCost = PrimaryUnitCost;
        newSalesCost = PrimarySalesCost;
    }
    else if (index == 2) {
        newUnitCost = SecondaryUnitCost;
        newSalesCost = SecondarySalesCost;
    }
    else if (index == 3) {
        newUnitCost = TertiaryUnitCost;
        newSalesCost = TertiarySalesCost;
    }
    else {
        newUnitCost = QuaternaryUnitCost;
        newSalesCost = QuaternarySalesCost;
    }

    $('#txtPurchaseExcTax' + i).val(Math.round((newUnitCost / ExchangeRate) * 100) / 100);
    $('#txtPurchaseIncTax' + i).val(Math.round((newUnitCost / ExchangeRate) * 100) / 100);
    $('#txtUnitCost' + i).val(Math.round((newUnitCost / ExchangeRate) * 100) / 100);
    $('#txtSalesIncTax' + i).val(Math.round((newSalesCost / ExchangeRate) * 100) / 100);

    $('#ddlFreeQuantityReturnedUnit' + i).val($('#ddlUnit' + i).val());

    ChangeQtyAmount(i);
}

function ChangeQtyAmount(id) {
    setExemption();

    //convertAvailableStock();
    //var r = checkStockAvailable(i);
    //if (r == true) {
    //    let chngqty = (parseFloat($('#txtQuantityReturned' + i).val()) * parseFloat($('#txtUnitCost' + i).val()));
    //    $('#txtUnitCost' + i).val(Math.round(chngqty * 100) / 100);
    //}
    debugger
    if (id != undefined) {
        var IsManageStock = $('#hdnIsManageStock' + id).val();
        if (IsManageStock.toLowerCase() == "true") {
            var StockQuantity = parseFloat($('#txtStockQuantity' + id).val());
            var newQuantity = 0;

            var QuantityReturned = parseFloat($('#txtQuantityReturned' + id).val());
            var FreeQuantityReturned = parseFloat($('#txtFreeQuantityReturned' + id).val());
            newQuantity = QuantityReturned + FreeQuantityReturned;
            if (newQuantity > StockQuantity) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                //toastr.error('Not enough stock available');
                $('#txtQuantityReturned' + id).val(StockQuantity);
                $('#txtFreeQuantityReturned' + id).val(0);
                $('#divQuantityReturned' + id).text('Not enough stock available');
                $('#divQuantityReturned' + id).show();
            }
        }

        let discount = 0;
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        }
        else {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + id).val()) / 100) * parseFloat($('#txtUnitCost' + id).val());
        }
        var _extraDiscount = parseFloat($('#hdnExtraDiscounts' + id).val()) / parseFloat($('#txtQuantityReturned' + id).val());
        $('#txtPurchaseExcTax' + id).val(Math.round(((parseFloat($('#txtUnitCost' + id).val())) - (discount + _extraDiscount)) * 100) / 100);
        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * ((parseFloat($('#txtUnitCost' + id).val())) - (discount + _extraDiscount))) * 100) / 100);

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesExcTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseExcTax' + id).val())) + parseFloat($('#txtPurchaseExcTax' + id).val())) * 100) / 100);
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }

    updateAmount();
}

function convertAvailableStock() {
    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var ind = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
        var ItemDetailsId = $('#txtItemDetailsId' + i).val();

        var UToSValue = parseFloat($('#hdnUToSValue' + i).val());
        var SToTValue = parseFloat($('#hdnSToTValue' + i).val());
        var TToQValue = parseFloat($('#hdnTToQValue' + i).val());

        var availableQuantity = parseFloat($('#hdnQuantityRemaining' + i).val());

        if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {

        }
        else if (SToTValue == 0 && TToQValue == 0) {
            if (ind == 3) {
                availableQuantity = availableQuantity / UToSValue;
            }
            else if (ind == 4) {

            }
        }
        else if (TToQValue == 0) {
            if (ind == 2) {
                availableQuantity = availableQuantity / UToSValue / SToTValue;
            }
            else if (ind == 3) {
                availableQuantity = availableQuantity / SToTValue;;
            }
            else if (ind == 4) {
                //TotalCurrentStock = TotalCurrentStock;
            }
        }
        else {
            if (ind == 1) {
                availableQuantity = availableQuantity / UToSValue / SToTValue / TToQValue;
            }
            else if (ind == 2) {
                availableQuantity = availableQuantity / SToTValue / TToQValue;
            }
            else if (ind == 3) {
                availableQuantity = availableQuantity / TToQValue;
            }
            else if (ind == 4) {
                //TotalCurrentStock= TotalCurrentStock;
            }
        }

        $('#txtStockQuantity' + i).val(availableQuantity);
        $('#txtAvailableQty' + i).text('Available Qty:' + availableQuantity);
    });
}

function checkStockAvailable(i) {
    var ItemDetailsId = $('#txtItemDetailsId' + i).val();
    var availableStock = $('#hdnQuantityRemaining' + i).val();
    var totalQty = 0;
    $('#divCombo tr').each(function () {
        var j = this.id.split('divCombo')[1];
        var innerItemDetailsId = $('#txtItemDetailsId' + j).val();
        if (innerItemDetailsId == ItemDetailsId) {
            var ind = $("#ddlUnit" + j)[0].selectedIndex + 1;

            var UToSValue = $('#hdnUToSValue' + j).val();
            var SToTValue = $('#hdnSToTValue' + j).val();
            var TToQValue = $('#hdnTToQValue' + j).val();

            var availableQuantity = $('#txtQuantityReturned' + j).val();
            if (ind == 1) {
                availableQuantity = (availableQuantity * UToSValue * SToTValue * TToQValue);
            }
            else if (ind == 2) {
                availableQuantity = (availableQuantity * SToTValue * TToQValue);
            }
            else if (ind == 3) {
                availableQuantity = (availableQuantity * TToQValue);
            }
            totalQty = totalQty + availableQuantity;
            if (parseFloat(totalQty) > parseFloat(availableStock)) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                $('#txtQuantityReturned' + i).val(0);
                return false;
            }
        }
    });
    return true;
}

function updateQuantity(_id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    if (_id != undefined) {
        var IsManageStock = $('#hdnIsManageStock' + _id).val();
        if (IsManageStock.toLowerCase() == "true") {
            var StockQuantity = parseFloat($('#txtStockQuantity' + _id).val());
            var newQuantity = 0;

            var QuantityReturned = parseFloat($('#txtQuantityReturned' + _id).val());
            var FreeQuantityReturned = parseFloat($('#txtFreeQuantityReturned' + _id).val());
            newQuantity = QuantityReturned + FreeQuantityReturned;
            if (newQuantity > StockQuantity) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                //toastr.error('Not enough stock available');
                $('#txtQuantityReturned' + _id).val(StockQuantity);
                $('#txtFreeQuantityReturned' + _id).val(0);
                $('#divQuantityReturned' + _id).text('Not enough stock available');
                $('#divQuantityReturned' + _id).show();
            }
        }
        updateAmount();
    }
}

function updateAmount() {
    var unitCost = 0, amountExcTax = 0, amountIncTax = 0;
    var qty = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        unitCost = unitCost + (parseFloat($('#txtUnitCost' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
        amountExcTax = amountExcTax + (parseFloat($('#txtPurchaseExcTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
        amountIncTax = amountIncTax + (parseFloat($('#txtPurchaseIncTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));

        qty = qty + parseFloat($('#txtQuantityReturned' + _id).val()) + parseFloat($('#txtFreeQuantityReturned' + _id).val());

        $('#txtAmountExcTax' + _id).val(parseFloat($('#txtPurchaseExcTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
        $('#txtAmountExcTax' + _id).text(CurrencySymbol + parseFloat($('#txtPurchaseExcTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));

        $('#txtAmountIncTax' + _id).val(parseFloat($('#txtPurchaseIncTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
        $('#txtAmountIncTax' + _id).text(CurrencySymbol + parseFloat($('#txtPurchaseIncTax' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
    });

    //var discount = 0
    //if ($("#ddlDiscAll option:selected").val() == 'Percentage') {
    //    discount = $('#txtDiscAll').val() == '' ? 0 : (($('#txtDiscAll').val() / 100) * amount);
    //}
    //else {
    //    discount = $('#txtDiscAll').val() == '' ? 0 : (parseFloat($('#txtDiscAll').val()));
    //}

    //let taxper = $('#ddlTax').val().split('-')[1] == undefined ? 0 : $('#ddlTax').val().split('-')[1];
    //let taxamt = parseFloat(unitCost - discount) * (taxper / 100);
    //$('#hdndivTax').val(taxamt);
    //$('#divTax').text(CurrencySymbol + Math.round(taxamt*100)/100);

    //$('#divDiscount').text(CurrencySymbol + Math.round(discount*100)/100);
    //$('#hdndivDiscount').val(discount);

    $('#divTotalAmount').text(CurrencySymbol + Math.round(unitCost * 100) / 100);
    $('#hdndivTotalAmount').val(unitCost);
    $('#divTotalQty').text(qty);

    //$('#divGrandTotal').text(CurrencySymbol + Math.round(unitCost * 100) / 100);
    //$('#hdndivGrandTotal').val((unitCost));

    discallcalc();

}

function discallcalc() {
    var taxs = [];
    var _extraDiscount = 0;

    var amountExcTax = 0, amountIncTax = 0, discount = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if ($('#txtQuantityReturned' + _id).val() != 0) {
            amountExcTax = amountExcTax + parseFloat($('#txtAmountExcTax' + _id).val());
            amountIncTax = amountIncTax + parseFloat($('#txtAmountIncTax' + _id).val());

            $('#txtTotalTaxAmount' + _id).val(parseFloat($('#txtAmountIncTax' + _id).val()) - parseFloat($('#txtAmountExcTax' + _id).val()));

            if ($("#ddlDiscountType" + _id + " option:selected").val() == 'Percentage') {
                discount = discount + ($('#txtDiscount' + _id).val() == '' ? 0 : ((($('#txtDiscount' + _id).val() / 100) * parseFloat($('#txtUnitCost' + _id).val()))));
            }
            else {
                discount = discount + ($('#txtDiscount' + _id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + _id).val())));
            }

            _extraDiscount = $('#hdnExtraDiscounts' + _id).val() / parseFloat($('#txtQuantityReturned' + _id).val());

            taxs.push({
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                AmountExcTax: parseFloat($('#txtAmountExcTax' + _id).val())//amountExcTax
            });
        }
    });

    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
        var count = index;
        var amount = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());

        if (amount > 0) {
            taxs.push({
                TaxId: $('#ddlAdditionalChargesTax' + count).val().split('-')[0],
                AmountExcTax: amount
            });
        }
    });

    let amount = 0;
    let divTotalAmount = $('#hdndivTotalAmount').val() == '' ? 0 : parseFloat($('#hdndivTotalAmount').val());

    if ($("#ddlDiscAll option:selected").val() == 'Percentage') {
        amount = $('#txtDiscAll').val() == '' ? 0 : (($('#txtDiscAll').val() / 100) * divTotalAmount);
    }
    else {
        amount = $('#txtDiscAll').val() == '' ? 0 : (parseFloat($('#txtDiscAll').val()));
    }

    taxs.push({
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        AmountExcTax: amountExcTax//divTotalAmount - (amount + discount)
    });

    var det = {
        Taxs: taxs,
    };

    $.ajax({
        url: '/taxsettings/TaxBreakups',
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
            }
            else {
                var html = '';
                //data.Data.Taxs.forEach(function (res) {
                //    html = html + '<div class="row">' +
                //        '<label for="inputEmail3" class="col-8 col-form-label text-right">' + res.Tax + ' :</label>' +
                //        '<label for="inputEmail3" class="col-4 col-form-label text-left">' + res.TaxAmount.toFixed(2) + '</label>' +
                //        '</div>';
                //});

                data.Data.Taxs.forEach(function (res) {
                    html = html + '<div class="text-right">' +
                        '<label for="inputEmail3" class="col-form-label">' + res.Tax + ' : ' + CurrencySymbol + res.TaxAmount.toFixed(2) + '</label>' +
                        //'<label for="inputEmail3" class="col-form-label">' + ' ' + CurrencySymbol + res.TaxAmount.toFixed(2) + '</label>' +
                        '</div>';
                });

                $('#divTaxSummary').empty();
                $('#divTaxSummary').append(html);

                var AdditionalChargesAmountExcTax = 0, AdditionalChargesAmountIncTax = 0;
                $('#divAdditionalCharges .additional-charges-row').each(function (index) {
                    var count = index;
                    var txtAdditionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
                    var txtAdditionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());

                    if (txtAdditionalChargesAmountExcTax > 0) {
                        AdditionalChargesAmountExcTax += txtAdditionalChargesAmountExcTax;
                        AdditionalChargesAmountIncTax += txtAdditionalChargesAmountIncTax;
                    }
                });

                let taxper = $('#ddlTax').val().split('-')[1] == undefined ? 0 : $('#ddlTax').val().split('-')[1];
                let taxamt = (amountIncTax - amountExcTax) + (AdditionalChargesAmountIncTax - AdditionalChargesAmountExcTax) + (parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100));
                $('#hdndivTax').val(Math.round((parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100)) * 100) / 100);
                $('#hdndivTotalTax').val(Math.round(taxamt * 100) / 100);
                $('#divTax').text(CurrencySymbol + Math.round(taxamt * 100) / 100);

                $('#divDiscount').text(CurrencySymbol + ((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100));
                $('#hdndivDiscount').val((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100);

                let SpecialDiscount = $('#txtSpecialDiscount').val() || 0;
                $('#divSpecialDiscount').text(CurrencySymbol + (Math.round(SpecialDiscount * 100) / 100));

                var grandTotal = Math.round(((divTotalAmount + taxamt + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount))) * 100) / 100;
                var roundOffGrandTotal = grandTotal;
                if ($('#hdnEnableRoundOff').val().toLocaleLowerCase() == 'true') {
                    roundOffGrandTotal = Math.round(grandTotal * 1) / 1;
                }
                var roundOff = roundOffGrandTotal - grandTotal;

                $('#divNetAmount').text(CurrencySymbol + Math.round(grandTotal * 100) / 100);
                $('#divRoundOff').text(CurrencySymbol + Math.round(roundOff * 100) / 100);
                $('#divGrandTotal').text(CurrencySymbol + Math.round(roundOffGrandTotal * 100) / 100);

                var grandTotal_reversecharge = Math.round(((divTotalAmount + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount))) * 100) / 100;
                var roundOffGrandTotal_reversecharge = grandTotal_reversecharge;
                if ($('#hdnEnableRoundOff').val().toLocaleLowerCase() == 'true') {
                    roundOffGrandTotal_reversecharge = Math.round(grandTotal_reversecharge * 1) / 1;
                }
                var roundOff_reversecharge = roundOffGrandTotal_reversecharge - grandTotal_reversecharge;
                $('#divNetAmount_reversecharge').text(CurrencySymbol + Math.round((grandTotal_reversecharge) * 100) / 100);
                $('#divRoundOff_reversecharge').text(CurrencySymbol + Math.round((roundOff_reversecharge) * 100) / 100);
                $('#divGrandTotal_reversecharge').text(CurrencySymbol + Math.round((roundOffGrandTotal_reversecharge) * 100) / 100);

                if ($('#chkIsReverseCharge').is(':checked') == true) {
                    $('.reversecharge').show();
                    $('.no_reversecharge').hide();
                }
                else {
                    $('.reversecharge').hide();
                    $('.no_reversecharge').show();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateExtraDiscounts() {
    let amount = 0;
    let divTotalAmount = $('#hdndivTotalAmount').val() == '' ? 0 : parseFloat($('#hdndivTotalAmount').val());

    if ($("#ddlDiscAll option:selected").val() == 'Percentage') {
        amount = $('#txtDiscAll').val() == '' ? 0 : (($('#txtDiscAll').val() / 100) * divTotalAmount);
    }
    else {
        amount = $('#txtDiscAll').val() == '' ? 0 : (parseFloat($('#txtDiscAll').val()));
    }
    $('#divCombo tr').each(function () {
        var id = this.id.split('divCombo')[1];
        if ($('#txtQuantityReturned' + id).val() != 0) {
            let discount = 0;
            if ($('#ddlDiscountType' + id).val() == "Fixed") {
                discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
            }
            else {
                discount = $('#txtDiscount' + id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + id).val()) / 100) * parseFloat($('#txtUnitCost' + id).val());
            }
            debugger
            var _subTotal = parseFloat($('#txtUnitCost' + id).val()) * parseFloat($('#txtQuantityReturned' + id).val());
            var _extraDiscount = ((_subTotal / divTotalAmount) * amount);

            $('#hdnExtraDiscounts' + id).val(_extraDiscount);

            $('#txtPurchaseExcTax' + id).val(Math.round(((parseFloat($('#txtUnitCost' + id).val())) - (discount + (_extraDiscount / parseFloat($('#txtQuantityReturned' + id).val())))) * 100) / 100);
            $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

            let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
            let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
            $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

            let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
            $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

            $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * chngpur) * 100) / 100);

            let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
            $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
        }
    });

    discallcalc();
}

function fetchActiveUsers(showAddNew) {
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType: 'supplier'
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/usersettings/AllActiveUsers',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                $('#divCombo').empty();
                var ddlUser = '<select class="form-control select2" id="ddlSupplier" onchange="FetchUserCurrency()">';
                if (showAddNew == true) {
                    ddlUser = ddlUser + '<option value="0">Select</option>';
                }
                else {
                    ddlUser = ddlUser + '<option value="0">All</option>';
                }

                for (let ss = 0; ss < data.Data.Users.length; ss++) {
                    ddlUser = ddlUser + '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name + '</option>';
                }
                ddlUser = ddlUser + '</select>';

                if (showAddNew == true) {
                    ddlUser = ddlUser + '<span class="input-group-append">' +
                        '<a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#supplierModal"> + </a>' +
                        '</span>';
                }

                $('.divSupplier').empty();
                $('.divSupplier').append(ddlUser);

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchAccountMapped(_type) {
    var det = {
        BranchId: _type == 0 ? _BranchId : $('#ddlBranch').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/settings1/FetchAccountMapped',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (data.Data.BranchPaymentTypeMap == null) {
                    if (_type == 0) {
                        $('#ddlLAccount').val(0);
                    }
                    else {
                        $('#ddlAccount').val(0);
                    }
                }
                else {
                    if (_type == 0) {
                        $('#ddlLAccount').val(data.Data.BranchPaymentTypeMap.AccountId);
                    }
                    else {
                        $('#ddlAccount').val(data.Data.BranchPaymentTypeMap.AccountId);
                    }
                }

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
}

function fetchPurchaseInvoices() {
    $('#divCombo').empty();
    var det = {
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        PurchaseType: 'Debit Note'
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseInvoices',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                //if (data.Data.User) {
                //    if (data.Data.User.GstTreatment != null && data.Data.User.GstTreatment != "") {
                //        if (data.Data.User.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)") {
                //            $('#ddlSourceOfSupply').val(data.Data.User.SourceOfSupplyId);
                //            $('.divSourceOfSupply').show();
                //        }
                //        else {
                //            $('#ddlSourceOfSupply').val(0);
                //            $('.divSourceOfSupply').hide();
                //        }
                //    }
                //    $('#ddlDestinationOfSupply').val(data.Data.User.SourceOfSupplyId);
                //}
                //else {
                //    $('#ddlSourceOfSupply').val(0);
                //    $('#ddlDestinationOfSupply').val(0);
                //    $('#ddlPaymentTerm').val(0);
                //}

                var ddlPurchaseInvoice = '<label>Purchase Invoice <span class="danger">*</span></label> <div class="input-group divPurchaseInvoice_ctrl"><select class="form-control select2" id="ddlPurchaseInvoice" onchange="setPurchaseReturnType()">';

                ddlPurchaseInvoice = ddlPurchaseInvoice + '<option value="0">Select</option>';

                for (let ss = 0; ss < data.Data.Purchases.length; ss++) {
                    ddlPurchaseInvoice = ddlPurchaseInvoice + '<option value="' + data.Data.Purchases[ss].PurchaseId + '">' + data.Data.Purchases[ss].ReferenceNo + '</option>';
                }
                ddlPurchaseInvoice = ddlPurchaseInvoice + '</select></div><small class="text-red font-weight-bold errorText" id="divPurchaseInvoice"></small>';

                $('.divPurchaseInvoice').empty();
                $('.divPurchaseInvoice').append(ddlPurchaseInvoice);

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
}

function setReturnType() {
    $('#divCombo').empty();
    if ($('#ddlReturnType').val() == 0) {
        $('.divPurchaseInvoice').hide();
        $('.divExpiredBefore').hide();
        $('.divQuantityLessThan').hide();
        $('.divSearch').hide();
    }
    else if ($('#ddlReturnType').val() == 1) {
        $('.divExpiredBefore').hide();
        $('.divQuantityLessThan').hide();
        $('.divSearch').show();
        $('.divPurchaseInvoice').hide();
    }
    else if ($('#ddlReturnType').val() == 2) {
        $('.divExpiredBefore').show();
        $('.divQuantityLessThan').hide();
        $('.divSearch').hide();
        $('.divPurchaseInvoice').hide();
    }
    else if ($('#ddlReturnType').val() == 3) {
        $('.divExpiredBefore').hide();
        $('.divQuantityLessThan').show();
        $('.divSearch').hide();
        $('.divPurchaseInvoice').hide();
    }
    else if ($('#ddlReturnType').val() == 4) {
        $('.divExpiredBefore').hide();
        $('.divQuantityLessThan').hide();
        $('.divSearch').hide();
        $('.divPurchaseInvoice').show();
    }
}

function fetchItem(SkuHsnCode) {
    //var splitVal = ui.item.value.split('~');
    var det = {
        ItemCode: SkuHsnCode,//splitVal[splitVal.length - 1],
        BranchId: $('#ddlBranch').val(),
        PurchaseId: $('#ddlPurchaseInvoice').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseReturnSearchItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                $('#txttags').val('');

                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                setItem(data);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });

}

function setPurchaseReturnType() {
    $('#divCombo').empty();

    var det = {
        PurchaseId: $('#ddlPurchaseInvoice').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseDetails',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                //data.Data.ItemDetails = data.Data.SalesDetails;
                //setReturnItem(data);

                //$("#tblPurchaseDetails").html(data);
                //$('.chkIsActive').bootstrapToggle();

                $('#ddlSourceOfSupply').val(data.Data.PurchaseReturn.SourceOfSupplyId);
                $('#ddlDestinationOfSupply').val(data.Data.PurchaseReturn.DestinationOfSupplyId);

                if (data.Data.PurchaseReturn.IsReverseCharge == 1) {
                    $('#chkIsReverseCharge').prop('checked', true);
                }
                else {
                    $('#chkIsReverseCharge').prop('checked', false);
                }

                toggleReverseCharge();
                $('.select2').select2();
                $("#divLoading").hide();

                //convertAvailableStock();

                fetchAdditionalCharges();
            }

        },
        error: function (xhr) {

        }
    });
}

function setItem(data) {
    var html = '';
    //var vari = '';
    $('#txttags').val('');
    for (let i = 0; i < data.Data.ItemDetails.length; i++) {

        var isPresent = false;

        if (isPresent == false) {
            debugger
            let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);

            var variation = '';
            if (data.Data.ItemDetails[i].VariationName) {
                variation = '</br> Variation : ' + data.Data.ItemDetails[i].VariationName;
            }
            var ddlUnit = '<select disabled class="form-control ' + (data.Data.ItemDetails[i].UnitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

            if (data.Data.ItemDetails[i].QuaternaryUnitId != 0) {
                if (data.Data.ItemDetails[i].PriceAddedFor == 1) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-1-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-1-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                }
            }
            else if (data.Data.ItemDetails[i].TertiaryUnitId != 0) {
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-2-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-2-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                }
            }
            else if (data.Data.ItemDetails[i].SecondaryUnitId != 0) {
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-3-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-3-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                }
                if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
                else {
                    ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                }
            }
            else {
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-4-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
            }

            ddlUnit = ddlUnit + '</select >';

            var ddlTax = '<select class="form-control ddlTax" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')" style="min-width:80px">';
            for (let ss = 0; ss < taxList.length; ss++) {
                var canAdd = true;
                if (taxList[ss].Tax != "Taxable") {
                    if ($('#chkIsReverseCharge').is(':checked')) {
                        if (taxList[ss].Tax == "Non-Taxable") {
                            canAdd = false;
                        }
                    }
                    if (canAdd == true) {
                        if (data.Data.BusinessSetting.CountryId == 2) {
                            if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
                                if (taxList[ss].TaxTypeId != 3) {
                                    if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                        ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                    }
                                    else {
                                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                    }
                                }
                            }
                            else {
                                if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                    if (data.Data.ItemDetails[i].InterStateTaxId == taxList[ss].TaxId) {
                                        ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                    }
                                    else {
                                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                    }
                                }
                            }
                        }
                        else {
                            if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                            else {
                                ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                        }
                    }
                }
            }
            ddlTax = ddlTax + '</select >';

            var ddlTaxExemption = '<select class="form-control select2" id="ddlTaxExemption' + count + '"><option selected value="0">None</option>';
            for (let ss = 0; ss < taxExemptions.length; ss++) {
                if (data.Data.ItemDetails[i].TaxExemptionId == taxExemptions[ss].TaxExemptionId) {
                    ddlTaxExemption = ddlTaxExemption + '<option selected value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                }
                else {
                    ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                }
            }
            ddlTaxExemption = ddlTaxExemption + '</select>';

            var EnableEditingProductPrice = $('#hdnEnableEditingProductPrice').val().toLocaleLowerCase();
            var EnableFreeQuantity = $('#hdnEnableFreeQuantity').val().toLocaleLowerCase();

            var ddlDiscountType = '<select ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" style="min-width: 100px;" id="ddlDiscountType' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
            if (data.Data.ItemDetails[i].DiscountType == "Fixed") {
                ddlDiscountType = ddlDiscountType + '<option selected value="Fixed">Fixed</option><option value="Percentage">Percentage</option>';
            }
            else {
                ddlDiscountType = ddlDiscountType + '<option value="Fixed">Fixed</option><option selected value="Percentage">Percentage</option>';
            }

            ddlDiscountType = ddlDiscountType + '</select>';

            var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());
            html = html + '<tr id="divCombo' + count + '">' +
                '<td style="min-width:100px; white-space: nowrap;">' +
                '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                '<input type="text" hidden class="form-control" id="txtSkuCode' + count + '" value="' + data.Data.ItemDetails[i].SKU + '">' +
                '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                '<input type="text" hidden class="form-control" id="txtPurchaseDetailsId' + count + '" value="' + data.Data.ItemDetails[i].PurchaseDetailsId + '">' +
                '<input type="text" hidden class="form-control" id="txtPurchaseReturnDetailsId' + count + '" value="' + data.Data.ItemDetails[i].PurchaseReturnDetailsId + '">' +
                '<span class="' + (data.Data.ItemDetails[i].ItemName.length > 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName.substring(0, 15) + '...</span>' +
                '<span class="' + (data.Data.ItemDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName + '</span>' +
                variation +
                ' </br> <b>SKU :</b> ' + data.Data.ItemDetails[i].SKU + '' +
                '</td>' +
                '<td>' +
                '<div class="input-group" style="min-width:150px">' +
                '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control divQuantity' + count + '_ctrl" value="0" id="txtQuantityReturned' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                ddlUnit +
                '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnIsManageStock' + count + '" value="' + data.Data.ItemDetails[i].IsManageStock + '">' +
                '</div >' +
                '<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small>' +
                '<small class="text-success font-weight-bold ' + (data.Data.ItemDetails[i].IsManageStock == true ? '' : 'hidden') + '" id="txtAvailableQty' + count + '">Available Qty: ' + data.Data.ItemDetails[i].QuantityRemaining + '</small>' +
                '<input type="text" hidden class="form-control" id="hdnQuantityRemaining' + count + '" value="' + data.Data.ItemDetails[i].QuantityRemaining + '">' +
                '<input type="text" hidden class="form-control" id="txtStockQuantity' + count + '" value="' + data.Data.ItemDetails[i].QuantityRemaining + '">' +
                '</td>' +
                '<td style="min-width:100px" class="' + (EnableFreeQuantity == 'true' ? '' : 'hidden') + '"> ' +
                '<div class="input-group">' +
                '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="0" id="txtFreeQuantityReturned' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                '</div >' +
                '</td>' +
                '<td style="min-width:120px">' +
                '<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '"  value="' + data.Data.ItemDetails[i].UnitCost / ExchangeRate + '" onchange="ChangeQtyAmount(' + count + ')">' +
                '</td>' +
                '<td>' +
                '<div class="input-group" style="min-width:150px">' +
                '<input ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeQtyAmount(' + count + ')" value="' + data.Data.ItemDetails[i].Discount + '">' +
                ddlDiscountType +
                '</div >' +
                '<input type="text" hidden class="form-control" id="hdnExtraDiscounts' + count + '" value="0">' +
                '</td>' +
                '<td style="min-width:100px;display:none;">' +
                '<input onKeyPress="onlyNumberKey(event)" type="number" ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '" id="txtPurchaseExcTax' + count + '"  onchange="ChangePurAmount(' + count + ')" min="0"/>' +
                '</td>' +
                '<td style="min-width:100px;display:none;">' +
                '<input type="number" class="form-control" disabled value="' + data.Data.ItemDetails[i].AmountExcTax + '" id="txtAmountExcTax' + count + '" min="0"/>' +
                '</td>' +
                '<td id="tdTax' + count + '">' +
                '<div class="input-group" style="min-width:160px">' +
                ddlTax +
                '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="0" style="min-width:80px">' +
                '<input type="number" hidden class="form-control" id="txtTaxAmount' + count + '" value="0" style="min-width:80px">' +
                '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                '<div id="divTaxExemption' + count + '" class="form-group" style="width:100%;display:' + ((data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                '<div class="input-group">' +
                ddlTaxExemption +
                '</div>' +
                '</div>' +
                '</div>' +
                //'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +
                '<div id="divInputTaxCredit' + count + '" class="form-group" style="display:' + (data.Data.BusinessSetting.CountryId == 2 ? '' : 'none') + '">' +
                '<label style="margin-bottom:0;margin-top:0.5rem;">Input Tax Credit </label>' +
                '<div class="input-group">' +
                '<select class="form-control" style="min-width: 160px;" id="ddlITCType' + count + '">' +
                '<option value="Eligible For ITC">Eligible For ITC</option>' +
                '<option value="Ineligible - As per Section 17 (5)">Ineligible - As per Section 17 (5)</option>' +
                '<option value="Ineligible - Others">Ineligible - Others</option>' +
                '</select>' +
                '</div>' +
                '</div>' +
                '</td>' +
                '<td style="min-width:100px;display:none;">' +
                '<input type="number" class="form-control" id="txtPurchaseIncTax' + count + '"  value="' + data.Data.ItemDetails[i].PurchaseIncTax / ExchangeRate + '" onchange="updateNetCost(' + count + ')">' +
                //'<input type="hidden" value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '" id="hdnPurchaseIncTax' + count + '" />' +
                '</td>' +
                '<td style="min-width:120px" class="no_reversecharge">' +
                '<input type="number" disabled class="form-control"  id="txtAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].AmountIncTax / ExchangeRate + '" >' +
                //'<input type="hidden" id="hdnAmountIncTax' + count + '" value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '" >' +
                '</td>' +
                '<td style="min-width:100px">' +
                '<button type="button" class="btn btn-danger btn-sm" onclick="deleteCombo(' + count + ')">' +
                '<i class="fas fa-times">' +
                '</i>' +
                '</button>' +
                '</td>' +
                '</tr>';
        }
        count++;
    }
    $('#divCombo').append(html);

    //$('#divCombo tr').each(function () {
    //    var _id = this.id.split('divCombo')[1];
    //    if (_id != undefined) {
    //        ChangePurAmount(_id);
    //    }
    //});

    //var ExpiryDateFormat = Cookies.get('ItemSetting').split('&')[0].split('=')[1];
    //$('._ManufacturingDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
    //$('._ExpiryDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
    //$('._ManufacturingDate').addClass('notranslate');
    //$('._ExpiryDate').addClass('notranslate');

    //updateAmount();
    //if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
    //    convertAvailableStock();
    //}
    convertAvailableStock();
    toggleReverseCharge();

    setExemption();

    $('.select2').select2();

    $("#divLoading").hide();
}

function UsersBranchWise() {
    $('#divCombo').empty();
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType: "Supplier"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/reports/UsersBranchWise',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>Supplier </label><div class="input-group"><select class="form-control select2" id="ddlSupplier" onchange="fetchPurchaseInvoices()"> <option selected="selected" value="0">Select</option>';
            $.each(data.Data.Users, function (index, value) {
                dropdown = dropdown + '<option value="' + value.UserId + '">' + value.Name + ' - ' + value.MobileNo + '</option>';
            });

            /*dropdown = dropdown + '</select><span class="input-group-append"><a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#supplierModal"> + </a></span> </div><small class="text-red font-weight-bold errorText" id="divSupplier"></small>';*/
            dropdown = dropdown + '</select></div><small class="text-red font-weight-bold errorText" id="divSupplier"></small>';
            $('#divUser').html('');
            $('#divUser').append(dropdown);
            $('.select2').select2();

        },
        error: function (xhr) {

        }
    });
};

function fetchExpiredBefore() {
    $('#divCombo').empty();
    var det = {
        ExpiredBefore: $('#txtExpiredBefore').val(),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseDetails',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                setItem(data);
            }

        },
        error: function (xhr) {

        }
    });
}

function fetchQuantityLessThan() {
    $('#divCombo').empty();
    var det = {
        QuantityLessThan: $('#txtQuantityLessThan').val(),
        BranchId: $('#ddlBranch').val()
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseDetails',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                setItem(data);
            }

        },
        error: function (xhr) {

        }
    });
}

function openTaxModal(id) {
    if (id != undefined) {
        if ($('.ddlTax').prop('disabled') == true) {
            return
        }
    }
    else {
        if ($('#ddlTax').prop('disabled') == true) {
            return
        }
    }
    taxModalId = id;

    $('.divTaxModal').show();
    $('.divTaxGroupModal').hide();

    $('#taxModal').modal('toggle');
}

function toggleTaxModal(d) {

    $('.divTaxModal').hide();
    $('.divTaxGroupModal').hide();

    if (d == 'TaxGroup') {
        $('.divTaxGroupModal').show();
    }
    else {
        $('.divTaxModal').show();
    }
}

function toggleForTaxGroupOnly(c) {
    if (c == 1) {
        if ($('#chkNewForTaxGroupOnly').is(':checked') == true) {
            $('.divNewAccounts').hide();
        }
        else {
            $('.divNewAccounts').show();
        }
    }
    else {
        if ($('#chkForTaxGroupOnly_N').is(':checked') == true) {
            $('.divAccounts_N').hide();
        }
        else {
            $('.divAccounts_N').show();
        }
    }
}

function insertTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercent').val(),
        Tax: $('#txtNewTax').val(),
        IsActive: true,
        IsDeleted: false,
        ForTaxGroupOnly: $('#chkNewForTaxGroupOnly').is(':checked'),
        PurchaseAccountId: $('#ddlNewPurchaseAccount').val(),
        SalesAccountId: $('#ddlNewSalesAccount').val(),
        SupplierPaymentAccountId: $('#ddlNewSupplierPaymentAccount').val(),
        CustomerPaymentAccountId: $('#ddlNewCustomerPaymentAccount').val(),
        ExpenseAccountId: $('#ddlNewExpenseAccount').val(),
        IncomeAccountId: $('#ddlNewIncomeAccount').val(),
        TaxTypeId: $('#ddlNewTaxType').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxInsert',
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
                $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (taxModalId == 0) {
                    $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }
                else {
                    $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }

                $('#ddlNewSubTax').append($('<option>', { value: data.Data.Tax.TaxId, text: data.Data.Tax.Tax }));
                var d = $('#ddlNewSubTax').val();
                if (d == null) d = [];
                d.push(data.Data.Tax.TaxId);
                //$('#ddlNewSubTax').val(d);

                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercent').val('');
                $('#txtNewTax').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertTaxGroup(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercentGroup').val(),
        Tax: $('#txtNewTaxGroup').val(),
        SubTaxs: $('#ddlNewSubTax').val(),
        IsActive: true,
        IsDeleted: false,
        PurchaseAccountId: $('#ddlNewSubTaxPurchaseAccount').val(),
        SalesAccountId: $('#ddlNewSubTaxSalesAccount').val(),
        SupplierPaymentAccountId: $('#ddlNewSubTaxSupplierPaymentAccount').val(),
        CustomerPaymentAccountId: $('#ddlNewSubTaxCustomerPaymentAccount').val(),
        ExpenseAccountId: $('#ddlNewSubTaxExpenseAccount').val(),
        IncomeAccountId: $('#ddlNewSubTaxIncomeAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxgroupInsert',
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
                $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (taxModalId == 0) {
                    $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }
                else {
                    $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }
                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercentGroup').val('');
                $('#txtNewTaxGroup').val('');
                $('#ddlNewSubTax').val('');
                $('.select2').select2();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertInnerTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercent_N').val(),
        Tax: $('#txtNewTax_N').val(),
        IsActive: true,
        IsDeleted: false,
        ForTaxGroupOnly: $('#chkForTaxGroupOnly_N').is(':checked'),
        PurchaseAccountId: $('#ddlPurchaseAccount_N').val(),
        SalesAccountId: $('#ddlSalesAccount_N').val(),
        SupplierPaymentAccountId: $('#ddlSupplierPaymentAccount_N').val(),
        CustomerPaymentAccountId: $('#ddlCustomerPaymentAccount_N').val(),
        ExpenseAccountId: $('#ddlExpenseAccount_N').val(),
        IncomeAccountId: $('#ddlIncomeAccount_N').val(),
        TaxTypeId: $('#ddlTaxType_N').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxInsert',
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
                    $('#' + res.Id + '_N').show();
                    $('#' + res.Id + '_N').text(res.Message);

                    if ($('.' + res.Id + '_N_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_N_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_N_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_N_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_N_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if ($('#chkForTaxGroupOnly_N').is(':checked') == false) {
                    $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    if (taxModalId == 0) {
                        $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                    }
                    else {
                        $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                    }
                }

                $('#ddlNewSubTax').append($('<option>', { value: data.Data.Tax.TaxId, text: data.Data.Tax.Tax }));
                var d = $('#ddlNewSubTax').val();
                if (d == null) d = [];
                d.push(data.Data.Tax.TaxId);
                $('#ddlNewSubTax').val(d);

                $('#taxInnerModal').modal('toggle');

                $('#txtNewTaxpercent_N').val('');
                $('#txtNewTax_N').val('');

                FetchTaxPercent();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function FetchTaxPercent() {
    var det = {
        SubTaxs: $('#ddlNewSubTax').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/FetchTaxPercent',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Data == null) {
                $('#txtNewTaxpercentGroup').val(0);
            }
            else {
                $('#txtNewTaxpercentGroup').val(data.Data.TaxPercent);
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchTax() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/taxsettings/ActiveAllTaxs',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                taxList = data.Data.Taxs;
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchTaxExemptions() {
    var det = {
        TaxExemptionType: "item"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/taxsettings/ActiveTaxExemptions',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                taxExemptions = data.Data.TaxExemptions;
            }
        },
        error: function (xhr) {

        }
    });
};

$(".add-row").click(function () {
    c = c + 1
    var markup = '<div class="input-group mb-1 divAccountDetails" id="rowNew' + c + '">' +
        '    <input type="text" class="form-control" id="txtLabel' + c + '" placeholder="Label">' +
        '    <input type="text" class="form-control" id="txtValue' + c + '" placeholder="value">' +
        '     <span class="input-group-append">' +
        '       <a href="javascript:void(0)" class="btn btn-danger btn-sm delete-row" id="btnNew' + c + '"><i class="fas fa-minus pt-2"></i></a>' +
        '     </span>' +
        '  </div>';
    $("#addrow1").append(markup);
});

$('#addrow1').on('click', '.delete-row', function () {
    var i = $(this).attr('id');
    $("#row" + i.split('btn')[1]).remove();
});

function insertCurrency(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


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
            }
            else {
                $('#ddlUserCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlUserCurrency').val(data.Data.Currency.CurrencyId);

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

function openInvoiceCopyModal(invUrl, invNo) {
    $('#lblInvoiceNo').text('Invoice No: ' + invNo);
    $('#lblInvoiceUrl').val(invUrl);

    $('#divInvoiceInputs').html('<input disabled type="text" class="form-control" id="lblInvoiceUrl" value="\'' + invUrl + '\'" style="width:80%">' +
        '<button type="button" class="btn btn-secondary" onclick="copyCode(\'' + invUrl + '\')" style="width:20%">COPY</button>');

    $('#divInvoiceButtons').html('<button type="button" class="btn btn-default" data-dismiss="modal" style="margin-left:auto">Close</button>' +
        '<button type="button" class= "btn btn-primary" onclick="PrintInvoice(\'' + invUrl + '\')" id="btnInvoiceView">View</button>');
    $('#InvoiceCopyModal').modal('toggle');
}

function openNotificationModal(ntype, nid) {
    NotificationName = ntype;
    NotificationId = nid;
    var det = {
        Name: ntype,
        Id: NotificationId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/FetchNotificationModule',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divNotification").html(data);

            $('#NotificationModal').modal('toggle');

            $('textarea.txtEmailBody').summernote({
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

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function SendNotifications() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        AutoSendEmail: $('#chkAutoSendEmail').is(':checked'),
        AutoSendSms: $('#chkAutoSendSms').is(':checked'),
        AutoSendWhatsapp: $('#chkAutoSendWhatsapp').is(':checked'),
        EmailSubject: $('#txtEmailSubject').val(),
        CC: $('#txtCC').val(),
        BCC: $('#txtBCC').val(),
        EmailBody: $('#txtEmailBody').val(),
        SmsBody: $('#txtSmsBody').val(),
        WhatsappBody: $('#txtWhatsappBody').val(),
        Id: NotificationId,
        Name: NotificationName
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/SendNotifications',
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
            }
            else {
                $('#NotificationModal').modal('toggle');
                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setAvailableTags(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

$("#_Date").on("change.datetimepicker", ({
    date,
    oldDate
}) => {
    if (oldDate != null) {
        setDueDate();
    }
});

$("#_DueDate").on("change.datetimepicker", ({
    date,
    oldDate
}) => {
    if (oldDate != null) {
        $("#ddlPaymentTerm").val('0');
        $('.select2').select2();
    }
});

$("#_ExpiredBefore").on("change.datetimepicker", ({
    date,
    oldDate
}) => {
    if (oldDate != null) {
        fetchExpiredBefore();
    }
});

function setDueDate() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    var det = {
        PaymentTermId: $("#ddlPaymentTerm").val(),
        InvoiceDate: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm')
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/CalculateDueDate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#txtDueDate").val(moment(new Date(parseInt(data.Data.PaymentTerm.DueDate.replace(/[^\d.]/g, '')))).format(DateFormat.toUpperCase() + ' ' + TimeFormat));

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: { Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'sale' },
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                    $('#txttags').val('');
                }
                else if (data.Status == 2) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
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
                }
                else {
                    if (data.Data.ItemsArray.length == 1) {
                        $('#txttags').val('');
                        var splitVal = data.Data.ItemsArray[0].split('~');
                        fetchItem(splitVal[splitVal.length - 1]);
                        skuCodes.push(splitVal[splitVal.length - 1]);
                    }
                    else {
                        response(data.Data.ItemsArray);
                    }
                }
            },
            error: function (a, b, c) {
                HandleLookUpError(a);
            }
        });
    },
    select: function (event, ui) {
        var splitVal = ui.item.value.split('~');
        fetchItem(splitVal[splitVal.length - 1]);
        skuCodes.push(splitVal[splitVal.length - 1]);
    }
});

function openSupplierModal() {
    $("#supplierModal").modal('show');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');
}

function insertSupplier() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var Addresses = [];
    Addresses.push({
        Name: $('#txtAddrName').val(),
        MobileNo: $('#txtAddrMobileNo').val(),
        MobileNo2: $('#txtAddrMobileNo2').val(),
        EmailId: $('#txtAddrEmailId').val(),
        Landmark: $('#txtAddrLandmark').val(),
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
    });
    Addresses.push({
        Name: $('#txtAddrAltName').val(),
        MobileNo: $('#txtAddrAltMobileNo').val(),
        MobileNo2: $('#txtAddrAltMobileNo2').val(),
        EmailId: $('#txtAddrAltEmailId').val(),
        Landmark: $('#txtAddrAltLandmark').val(),
        Address: $('#txtAltAddress').val(),
        CountryId: $('#ddlAltCountry').val(),
        StateId: $('#ddlAltState').val(),
        CityId: $('#ddlAltCity').val(),
        Zipcode: $('#txtAltZipcode').val(),
    });

    var det = {
        Username: $('#txtUsername').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        BusinessName: $('#txtBusinessName').val(),
        DOB: moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtDOB').val(),
        JoiningDate: moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtJoiningDate').val(),
        //Address: $('#txtAddress').val(),
        //CountryId: $('#ddlCountry').val(),
        //StateId: $('#ddlState').val(),
        //CityId: $('#ddlCity').val(),
        //Zipcode: $('#txtZipcode').val(),
        //AltAddress: $('#txtAltAddress').val(),
        //AltCountryId: $('#ddlAltCountry').val(),
        //AltStateId: $('#ddlAltState').val(),
        //AltCityId: $('#ddlAltCity').val(),
        //AltZipcode: $('#txtAltZipcode').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        TaxNo: $('#txtTaxNo').val(),
        CreditLimit: $('#txtCreditLimit').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        UserType: 'Supplier',
        IsActive: true,
        IsDeleted: false,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        TaxId: $('#ddlTax').val(),
        Addresses: Addresses,
        CurrencyId: $('#ddlUserCurrency').val(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply_M').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        //BusinessLegalName: $('#txtBusinessLegalName').val(),
        //BusinessTradeName: $('#txtBusinessTradeName').val(),
        PanNo: $('#txtPanNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/UserInsert',
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
                $('#ddlSupplier').append($('<option>', { value: data.Data.User.UserId, text: data.Data.User.Name }));

                $('#ddlSupplier').val(data.Data.User.UserId);
                $('#supplierModal').modal('toggle');

                FetchUserCurrency();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UpdatePurchaseReturnStatus(id) {
    var r = confirm("This will mark the Purchase Return as Open. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');

        var det = {
            PurchaseReturnId: id,
            Status: "Open",
        };
        $("#divLoading").show();
        $.ajax({
            url: '/purchase/UpdatePurchaseReturnStatus',
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
                        $('#' + res.Id + '_M').show();
                        $('#' + res.Id + '_M').text(res.Message);


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
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchList();
                    //$("#purchaseStatusModal").modal('hide');

                    if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseBill == true) {
                        PrintInvoice('/purchase/PurchaseReturnInvoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function openRefundModal(type, SupplierPaymentId, title, BranchId) {
    _SupplierPaymentId = SupplierPaymentId;
    _BranchId = BranchId;
    var det = {
        SupplierPaymentId: SupplierPaymentId,
        Type: title,
        Title: title,
        BranchId: _BranchId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/Refunds',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divRefunds").html(data);

            $("#refundModal").modal('show');
            if (type == true) {
                $('.refundAdd').show();
                $('.refundList').hide();
                $('#refundModalLabel').text('Add Refund');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_RefundDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
                $('#_RefundDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });

            }
            else {
                $('.refundAdd').hide();
                $('.refundList').show();
                $('#refundModalLabel').text('View Refunds');
            }

            //$('.select2').select2();
            $('.select2').select2({
                dropdownParent: $('#refundModal')
            });

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertRefund() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        ReferenceNo: $('#txtReferenceNo').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtRefundDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        Type: "Supplier Refund",
        ParentId: _SupplierPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/RefundInsert',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#refundModal").modal('hide');

                $('#txtPaymentNotes').val('');
                $('#txtAmount').val(0);
                $('#txtPaymentDate').val('');
                $('#ddlPaymentType').val(0);
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('#ddlLAccount').val(0);

                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deleteRefund(SupplierPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            SupplierPaymentId: SupplierPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/Supplier/RefundDelete',
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
                    fetchList();
                    $('#tr_' + SupplierPaymentId).remove();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function openUnpaidInvoicesModal(SupplierId, SupplierPaymentId) {
    _SupplierPaymentId = SupplierPaymentId;

    var det = {
        SupplierId: SupplierId,
        SupplierPaymentId: SupplierPaymentId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/UnpaidPurchaseInvoices',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divUnpaidInvoices").html(data);

            $("#UnpaidInvoicesModal").modal('show');

            var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
            var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

            $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
            $('#_PaymentDate').addClass('notranslate');

            $('.select2').select2();

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateTotal() {
    var subTotal = 0, total = 0;
    var Balance = $('#hdnAmountRemaining').val();

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            var AmountExcTax = $('#txtAmount' + _id).val();

            subTotal = subTotal + parseFloat(AmountExcTax);
        }
    });

    $('#divAmountToCredit').text(CurrencySymbol + subTotal.toFixed(2));
    $('#divRemainingCredits').text(CurrencySymbol + (Balance - subTotal.toFixed(2)));

}

function ApplyCreditsToInvoices() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();

    var SupplierPaymentIds = [];
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            SupplierPaymentIds.push({
                Type: $('#hdnType' + _id).val(),
                PurchaseId: $('#hdnPurchaseId' + _id).val(),
                IsReverseCharge: $('#hdnIsReverseCharge' + _id).val(),
                Amount: $('#txtAmount' + _id).val(),
                IsActive: true,
                IsDeleted: false,
                DivId: _id
            })
        }
    });

    var det = {
        IsActive: true,
        IsDeleted: false,
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        ParentId: _SupplierPaymentId,
        SupplierPaymentIds: SupplierPaymentIds
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/ApplyCreditsToInvoices',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#UnpaidInvoicesModal").modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function DeleteSupplierPayment(SupplierPaymentId) {
    var det = {
        SupplierPaymentId: SupplierPaymentId
    }
    $("#divLoading").show();
    $.ajax({
        url: "/supplier/PaymentDelete",
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            //if (data == "True") {
            //    $('#subscriptionExpiryModal').modal('toggle');
            //    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
            //    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
            //    return
            //}
            //else if (data == "False") {
            //    $('#subscriptionExpiryModal').modal('toggle');
            //    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
            //    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
            //    return
            //}

            //if (data.Status == 0) {
            //    if (EnableSound == 'True') { document.getElementById('error').play(); }
            //    toastr.error(data.Message);
            //}
            //else {
            //    if (EnableSound == 'True') { document.getElementById('success').play(); }
            //    toastr.success(data.Message);
            //    //$('#tr_' + SupplierPaymentId).remove();
            //    fetchList();
            //}
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchUpdatedItems() {
    //$('#divCombo').empty();
    $.each(skuCodes, function (index, value) {
        fetchItemTax(value);
    });

    fetchAdditionalCharges();
}

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
                $('#ddlPaymentTerm_M').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPaymentTerm_M').val(data.Data.PaymentTerm.PaymentTermId);

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

function toggleShippingAddress() {
    if ($('#chkIsShippingAddressDifferent').is(':checked')) {
        $('#divShippingAddress').show();
    }
    else {
        $('#divShippingAddress').hide();
    }
}

function setName() {
    //if (!$('#txtAddrName').val()) {
    $('#txtAddrName').val($('#txtName').val())
    //}
}

function setMobile() {
    //if (!$('#txtAddrMobileNo').val()) {
    $('#txtAddrMobileNo').val($('#txtMobileNo').val())
    //}
}

function setAlternativeMobile() {
    //if (!$('#txtAddrMobileNo2').val()) {
    $('#txtAddrMobileNo2').val($('#txtAltMobileNo').val());
    //}
}

function setEmail() {
    //if (!$('#txtAddrMobileNo2').val()) {
    $('#txtAddrEmailId').val($('#txtEmailId').val());
    //}
}

function openCityModal(id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    _id = id;
    if (_id == 'ddlCity') {
        if (!$('#ddlState').val() || $('#ddlState').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select State first');
            return
        }
    }
    else {
        if (!$('#ddlAltState').val() || $('#ddlAltState').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select State first');
            return
        }
    }
    $('#cityModal').modal('toggle');
}

function insertCity() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CountryId: _id == 'ddlCity' ? $('#ddlCountry').val() : $('#ddlAltCountry').val(),
        //StateId: $('#ddlCityModalState').val(),
        StateId: _id == 'ddlCity' ? $('#ddlState').val() : $('#ddlAltState').val(),
        CityCode: $('#txtCityCode').val(),
        City: $('#txtCity').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/CityInsert',
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
                if (_id == 'ddlCity') {
                    $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    if ($('#ddlState').val() == $('#ddlAltState').val()) {
                        $('#ddlAltCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    }
                }

                if (_id == 'ddlAltCity') {
                    $('#ddlAltCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    if ($('#ddlState').val() == $('#ddlAltState').val()) {
                        $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    }
                }
                $('#' + _id).val(data.Data.City.CityId);
                $('#cityModal').modal('toggle');

                $('#ddlCityModalCountry').val($("#ddlCityModalCountry option:first").val());
                $('#ddlCityModalState').html('');
                $('#txtCityCode').val('');
                $('#txtCity').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openItemCodeModal() {
    $('#itemCodeModal').modal('toggle');
    if ($('#ddlItemType').val() == "Product") {
        $('#ddlItemCodeType_N').val('HSN');
        $('#lblCode').html('HSN Code <span class="danger">*</span>');
    }
    else {
        $('#ddlItemCodeType_N').val('SAC');
        $('#lblCode').html('SAC Code <span class="danger">*</span>');
    }
    $('#ddlItemCodeType_N').prop('disabled', true);
    $('.select2').select2();
}

function insertItemCode(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        ItemCodeType: $('#ddlItemCodeType_N').val(),
        Code: $('#txtCode_N').val(),
        Description: $('#txtDescription_N').val(),
        TaxPreferenceId: $('#ddlTaxPreference_N').val(),
        TaxPreference: $("#ddlTaxPreference_N option:selected").text(),
        TaxExemptionId: $('#ddlTaxExemption_N').val(),
        IntraStateTaxId: $('#ddlIntraStateTax_N').val().split('-')[0],
        InterStateTaxId: $('#ddlInterStateTax_N').val().split('-')[0],
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/ItemCodeInsert',
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
                    $('#' + res.Id + '_N').show();
                    $('#' + res.Id + '_N').text(res.Message);


                    if ($('.' + res.Id + '_N_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_N_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_N_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_N_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_N_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                $('#ddlItemCode').append($('<option>', { value: data.Data.ItemCode.ItemCodeId, text: data.Data.ItemCode.Code }));
                $('#ddlItemCode').val(data.Data.ItemCode.ItemCodeId);

                $('#itemCodeModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchItemCodeTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        ItemCodeId: $('#ddlItemCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/fetchItemCodeTax',
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
            }
            else {
                $('#ddlTaxPreference').val(data.Data.ItemCode.TaxPreferenceId);
                $('#ddlTax').val(data.Data.ItemCode.IntraStateTaxId + '-' + data.Data.ItemCode.IntraStateTaxPercentage);
                $('#ddlInterStateTax').val(data.Data.ItemCode.InterStateTaxId + '-' + data.Data.ItemCode.InterStateTaxPercentage);
                $('#ddlTaxExemption').val(data.Data.ItemCode.TaxExemptionId);

                $('.select2').select2();
                toggleTaxPreference();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openSaltModal() {
    $('#saltModal').modal('toggle');
    $('textarea#txtIndication,#txtDosage,#txtSideEffects,#txtSpecialPrecautions,#txtDrugInteractions,#txtNotes').summernote({
        placeholder: '',
        tabsize: 2,
        height: 100,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            // ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            //['fontname', ['fontname']],
            // ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            //['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });
}

function insertSalt(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        SaltName: $('#txtSaltName').val(),
        Indication: $('#txtIndication').val(),
        Dosage: $('#txtDosage').val(),
        SideEffects: $('#txtSideEffects').val(),
        SpecialPrecautions: $('#txtSpecialPrecautions').val(),
        DrugInteractions: $('#txtDrugInteractions').val(),
        Notes: $('#txtNotes').val(),
        TBItem: $('#ddlTBItem').val(),
        IsNarcotic: $('#chkIsNarcotic').is(':checked'),
        IsScheduleH: $('#chkIsScheduleH').is(':checked'),
        IsScheduleH1: $('#chkIsScheduleH1').is(':checked'),
        IsDiscontinued: $('#chkIsDiscontinued').is(':checked'),
        IsProhibited: $('#chkIsProhibited').is(':checked'),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/SaltInsert',
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
            }
            else {
                $('#ddlSalt').append($('<option>', { value: data.Data.Salt.SaltId, text: data.Data.Salt.SaltName }));
                $('#ddlSalt').val(data.Data.Salt.SaltId);
                $('#saltModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchItemTax(SkuHsnCode) {
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
        Supplierid: $('#ddlSupplier').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        Type: "Purchase"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/items/SearchItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var html = '';
            //var z = count;
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {

                $('#divCombo tr').each(function () {
                    var _id = this.id.split('divCombo')[1];
                    if (_id != undefined) {
                        var _skuCode = $('#txtSkuCode' + _id).val();
                        if (_skuCode.toLowerCase().trim() == SkuHsnCode.toLowerCase().trim()) {
                            var count = _id;
                            debugger
                            var ddlTax = '<select class="form-control ddlTax" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')" style="min-width:80px">';
                            for (let ss = 0; ss < taxList.length; ss++) {
                                var canAdd = true;
                                if (taxList[ss].Tax != "Taxable") {
                                    if ($('#chkIsReverseCharge').is(':checked')) {
                                        if (taxList[ss].Tax == "Non-Taxable") {
                                            canAdd = false;
                                        }
                                    }
                                    if (canAdd == true) {
                                        if (data.Data.BusinessSetting.CountryId == 2) {
                                            if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
                                                if (taxList[ss].TaxTypeId != 3) {
                                                    if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                                        ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                    }
                                                    else {
                                                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                    }
                                                }
                                            }
                                            else {
                                                if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                                    if (data.Data.ItemDetails[i].InterStateTaxId == taxList[ss].TaxId) {
                                                        ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                    }
                                                    else {
                                                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                    }
                                                }
                                            }
                                        }
                                        else {
                                            if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                                ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                            }
                                            else {
                                                ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                            }
                                        }
                                    }
                                }
                            }
                            ddlTax = ddlTax + '</select >';

                            var ddlTaxExemption = '<select class="form-control select2" id="ddlTaxExemption' + count + '"><option selected value="0">None</option>';
                            for (let ss = 0; ss < taxExemptions.length; ss++) {
                                if (data.Data.ItemDetails[i].TaxExemptionId == taxExemptions[ss].TaxExemptionId) {
                                    ddlTaxExemption = ddlTaxExemption + '<option selected value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                                }
                                else {
                                    ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                                }
                            }
                            ddlTaxExemption = ddlTaxExemption + '</select >';

                            $('#tdTax' + count).empty();
                            $('#tdTax' + count).html(
                                '<div class="input-group" style="min-width:160px">' +
                                ddlTax +
                                '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="0" style="min-width:80px">' +
                                '<input type="number" hidden class="form-control" id="txtTaxAmount' + count + '" value="0" style="min-width:80px">' +
                                '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                                '<div id="divTaxExemption' + count + '" class="form-group" style="display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                                '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                                '<div class="input-group">' +
                                ddlTaxExemption +
                                '</div>' +
                                '</div>' +
                                '</div>' +
                                //'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +
                                '<div id="divInputTaxCredit' + count + '" class="form-group" style="display:' + (data.Data.BusinessSetting.CountryId == 2 ? '' : 'none') + '">' +
                                '<label style="margin-bottom:0;margin-top:0.5rem;">Input Tax Credit </label>' +
                                '<div class="input-group">' +
                                '<select class="form-control" style="min-width: 160px;" id="ddlITCType' + count + '">' +
                                '<option value="Eligible For ITC">Eligible For ITC</option>' +
                                '<option value="Ineligible - As per Section 17 (5)">Ineligible - As per Section 17 (5)</option>' +
                                '<option value="Ineligible - Others">Ineligible - Others</option>' +
                                '</select>' +
                                '</div>' +
                                '</div>');

                            ChangeQtyAmount(count);
                        }
                    }
                });
            }
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveStates(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlCountry').val();
    }
    else {
        id = $('#ddlAltCountry').val();
    }
    var det = {
        CountryId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_States_Dropdown").html(data);
            }
            else {
                $("#p_Alt_States_Dropdown").html(data);
            }
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveCitys(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlState').val();
    }
    else {
        id = $('#ddlAltState').val();
    }
    var det = {
        StateId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_Citys_Dropdown").html(data);
            }
            else {
                $("#p_Alt_Citys_Dropdown").html(data);
            }
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleReverseCharge() {
    if ($('#chkIsReverseCharge').is(':checked')) {
        $('#lblReverseCharge').show();
    }
    else {
        $('#lblReverseCharge').hide();
    }

    fetchUpdatedItems();
    discallcalc();
}

function insertPurchaseDebitNoteReason() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        PurchaseDebitNoteReason: $('#txtPurchaseDebitNoteReason_M').val(),
        Description: $('#txtDescription_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Purchasesettings/PurchaseDebitNoteReasonInsert',
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
                $('#ddlPurchaseDebitNoteReason').append($('<option>', { value: data.Data.PurchaseDebitNoteReason.PurchaseDebitNoteReasonId, text: data.Data.PurchaseDebitNoteReason.PurchaseDebitNoteReason }));
                $('#ddlPurchaseDebitNoteReason').val(data.Data.PurchaseDebitNoteReason.PurchaseDebitNoteReasonId);

                $('#purchaseDebitNoteReasonsModal').modal('toggle');

                $('#txtPurchaseDebitNoteReason_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setExemption() {
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            $('#divTaxExemption' + _id).hide();
            $('#divInputTaxCredit' + _id).hide();

            $('#divTaxExemption' + _id).empty();
            $('#divInputTaxCredit' + _id).empty();

            var taxValue = $("#ddlTax" + _id + " :selected").text();

            if (taxValue == "Non-Taxable") {

                var ddlTaxExemption = '<select class="form-control select2 ddlTax" id="ddlTaxExemption' + _id + '" >';
                for (let ss = 0; ss < taxExemptions.length; ss++) {
                    ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                }
                ddlTaxExemption = ddlTaxExemption + '</select>';

                var html = '<label style="margin-bottom:0;margin-top:0.5rem;"> Exemption Reason <span class="danger">*</span></label>' +
                    '<div class="input-group">' +
                    ddlTaxExemption +
                    '</div>';

                $('#divTaxExemption' + _id).empty();
                $('#divTaxExemption' + _id).append(html);

                $('#divTaxExemption' + _id).show();
            }
            else if (taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
                var ddlITCType = '<select class="form-control select2 ddlTax" id="ddlITCType' + _id + '" >' +
                    '<option value="Eligible For ITC">Eligible For ITC</option>' +
                    '<option value="Ineligible - As per Section 17 (5)">Ineligible - As per Section 17 (5)</option>' +
                    '<option value="Ineligible - Others">Ineligible - Others</option>' +
                    '</select>';

                var html = '<label style="margin-bottom:0;margin-top:0.5rem;"> Input Tax Credit <span class="danger">*</span></label>' +
                    '<div class="input-group">' +
                    ddlITCType +
                    '</div>';

                $('#divInputTaxCredit' + _id).empty();
                $('#divInputTaxCredit' + _id).append(html);

                $('#divInputTaxCredit' + _id).show();
            }
        }
    });
    $('.select2').select2();
}

function fetchAdditionalCharges() {
    var det = {
        SupplierId: $('#ddlSupplier').val(),
        BranchId: $('#ddlBranch').val(),
    };
    
    // Store existing values before clearing
    var existingValues = {};
    var hasExistingValues = false;
    
    // Check if there are any existing values in additional charges
    $('[id^="txtAdditionalChargesAmountExcTax"]').each(function() {
        var index = $(this).attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        var amount = $(this).val();
        var incTax = $('#txtAdditionalChargesAmountIncTax' + index).val();
        var chargeId = $('#txtAdditionalChargeId' + index).val();
        var purchaseReturnAdditionalChargesId = $('#txtPurchaseReturnAdditionalChargesId' + index).val();
        
        if (amount && parseFloat(amount) > 0) {
            hasExistingValues = true;
            existingValues[index] = {
                amount: amount,
                incTax: incTax,
                chargeId: chargeId,
                purchaseReturnAdditionalChargesId: purchaseReturnAdditionalChargesId
            };
        }
    });
    
    $("#divLoading").show();
    $.ajax({
        url: '/OtherSettings/ActiveAdditionalCharges',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var CountryId = $('#hdnCountryId').val();

            var html = '';
            for (let i = 0; i < data.Data.AdditionalCharges.length; i++) {
                var count = i;
                
                // Check if we have existing values for this charge
                var existingValue = null;
                if (hasExistingValues) {
                    // Find existing value by charge ID
                    for (var key in existingValues) {
                        if (existingValues[key].chargeId == data.Data.AdditionalCharges[i].AdditionalChargeId) {
                            existingValue = existingValues[key];
                            break;
                        }
                    }
                }

                var ddlTax = '<select class="form-control select2" style="min-width:80px" id="ddlAdditionalChargesTax' + count + '" onchange="toggleExemptionDropdownAC(' + count + ');">';
                for (let ss = 0; ss < taxList.length; ss++) {
                    var canAdd = true;
                    if (taxList[ss].Tax != "Taxable") {
                        if ($('#chkIsReverseCharge').is(':checked')) {
                            if (taxList[ss].Tax == "Non-Taxable") {
                                canAdd = false;
                            }
                        }
                        if (canAdd == true) {
                            if (CountryId == 2) {
                                if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
                                    if (taxList[ss].TaxTypeId != 3) {
                                        if (data.Data.AdditionalCharges[i].IntraStateTaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                                else {
                                    if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                        if (data.Data.AdditionalCharges[i].InterStateTaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                            }
                            else {
                                if (data.Data.AdditionalCharges[i].TaxId == taxList[ss].TaxId) {
                                    ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                }
                                else {
                                    ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                }
                            }
                        }
                    }
                }
                ddlTax = ddlTax + '</select>';

                // Exemption Reason label + edit icon (no inline dropdown)
                var selectedExemptionText = 'None';
                var selectedExemptionId = 0;
                if (data.Data.AdditionalCharges[i].TaxExemptionId && data.Data.AdditionalCharges[i].TaxExemptionId != 0) {
                    var found = taxExemptions.find(function (ex) { return ex.TaxExemptionId == data.Data.AdditionalCharges[i].TaxExemptionId; });
                    if (found) selectedExemptionText = found.Reason;
                    selectedExemptionId = data.Data.AdditionalCharges[i].TaxExemptionId;
                }
                
                var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:none; margin-top:5px; align-items:center;">' +
                    '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
                    '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
                    '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
                    '<i class="fas fa-edit"></i>' +
                    '</a>' +
                    '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
                    '</div>';

                var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:none; margin-top:5px; align-items:center;">' +
                    '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
                    '<span id="itcTypeLabel' + count + '" class="small">Eligible For ITC</span>' +
                    '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
                    '<i class="fas fa-edit"></i>' +
                    '</a>' +
                    '<input type="hidden" id="itcTypeValue' + count + '" value="Eligible For ITC">' +
                    '</div>';

                // Set amount value - preserve existing or use default
                var amountValue = existingValue ? existingValue.amount : '0';
                var incTaxValue = existingValue ? existingValue.incTax : '0';
                var purchaseReturnAdditionalChargesIdValue = existingValue ? existingValue.purchaseReturnAdditionalChargesId : data.Data.AdditionalCharges[i].PurchaseReturnAdditionalChargesId;

                html = html +
                    '<div class="row additional-charges-row">' +
                    '<div class="col-sm-6">' +
                    '<div class="form-group row">' +
                    '<label for="inputEmail3" class="col-sm-5 col-form-label">' + data.Data.AdditionalCharges[i].Name + '</label>' +
                    '<div class="input-group col-sm-7">' +
                    '<input type="number" min="0" class="form-control" id="txtAdditionalChargesAmountExcTax' + count + '" placeholder="" onchange="updateAdditionalChargesCalculation(' + count + ')" value="' + amountValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargesAmountIncTax' + count + '" value="' + incTaxValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargeId' + count + '" value="' + data.Data.AdditionalCharges[i].AdditionalChargeId + '">' +
                    '<input type="hidden" id="txtPurchaseReturnAdditionalChargesId' + count + '" value="' + purchaseReturnAdditionalChargesIdValue + '">' +
                    ddlTax +
                    exemptionDiv +
                    itcDiv +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div> ';
            }

            $('#divAdditionalCharges').empty();
            $('#divAdditionalCharges').html(html);
            $('.select2').select2();
            
            // Ensure exemption icon/label is shown if tax is Non-Taxable on load
            for (let i = 0; i < data.Data.AdditionalCharges.length; i++) {
                toggleExemptionDropdownAC(i);
            }
            
            // Apply ITC/Exemption logic for additional charges
            applyITCExemptionLogicForAdditionalCharges();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

// Update modal save logic to store the selected exemption id as well as text
function saveExemptionSelection() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if (currentExemptionEditIndex == null) {
        return;
    }

    var $dropdown = $('#ddlExemptionReason');
    var $label = $('#exemptionReasonLabel' + currentExemptionEditIndex);
    var $hiddenId = $('#exemptionReasonId' + currentExemptionEditIndex);

    if ($dropdown.length === 0) {
        return;
    }

    if ($label.length === 0) {
        return;
    }

    if ($hiddenId.length === 0) {
        return;
    }

    // Update the label and hidden input with selected values
    var selectedText = $dropdown.find('option:selected').text();
    var selectedValue = $dropdown.val();

    if (selectedValue == "0") {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');

        $('#divExemptionReason').show();
        $('#divExemptionReason').text('This field is required');

        $('#ddlExemptionReason').next('.select2').find('.select2-selection').css('border', '2px solid #dc3545');
        return
    }

    $label.text(selectedText);
    $hiddenId.val(selectedValue);

    // Hide the modal
    $('#exemptionModal').modal('hide');
    currentExemptionEditIndex = null;
}

// Add supporting JS functions at the end of the file
var currentExemptionEditIndex = null;
function openExemptionModal(count) {
    currentExemptionEditIndex = count;
    var $dropdown = $('#ddlExemptionReason');

    $dropdown.empty().append(
        $('<option>', {
            value: 0,
            text: 'Select'
        })
    );

    if (taxExemptions && taxExemptions.length > 0) {
        $.each(taxExemptions, function (i, exemption) {
            $dropdown.append(
                $('<option>', {
                    value: exemption.TaxExemptionId,
                    text: exemption.Reason
                })
            );
        });
    }

    // Set the current value if it exists
    var currentValue = $('#exemptionReasonId' + count).val();
    if (currentValue && currentValue != '0') {
        $dropdown.val(currentValue).trigger('change');
    }

    $('#ddlExemptionReason').select2();
    $('#exemptionModal').modal('show');
}

function toggleExemptionDropdownAC(count) {
    // Apply the complete ITC/Exemption logic for this specific additional charge
    applyITCExemptionLogicForSingleAdditionalCharge(count);
    updateAdditionalChargesCalculation(count);
}

function applyITCExemptionLogicForSingleAdditionalCharge(count) {
    // Hide both sections initially
    $('#divTaxExemptionAC' + count).hide();
    $('#divInputTaxCreditAC' + count).hide();
    
    // Clear existing content
    $('#divTaxExemptionAC' + count).empty();
    $('#divInputTaxCreditAC' + count).empty();
    
    var taxValue = $("#ddlAdditionalChargesTax" + count + " :selected").text();
    
    if (taxValue == "Non-Taxable") {
        // Show exemption reason for Non-Taxable
        var selectedExemptionText = $('#exemptionReasonLabel' + count).text() || 'None';
        var selectedExemptionId = $('#exemptionReasonId' + count).val() || '0';
        
        var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
            '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
            '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
            '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
            '<i class="fas fa-edit"></i>' +
            '</a>' +
            '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
            '</div>';
        
        $('#divTaxExemptionAC' + count).html(exemptionDiv);
        $('#divTaxExemptionAC' + count).show();
    }
    else if (taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
        // Show ITC for taxable items
        var selectedITCText = $('#itcTypeLabel' + count).text() || 'Eligible For ITC';
        var selectedITCValue = $('#itcTypeValue' + count).val() || 'Eligible For ITC';
        
        var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
            '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
            '<span id="itcTypeLabel' + count + '" class="small">' + selectedITCText + '</span>' +
            '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
            '<i class="fas fa-edit"></i>' +
            '</a>' +
            '<input type="hidden" id="itcTypeValue' + count + '" value="' + selectedITCValue + '">' +
            '</div>';
        
        $('#divInputTaxCreditAC' + count).html(itcDiv);
        $('#divInputTaxCreditAC' + count).show();
    }
}

function updateAdditionalChargesCalculation(count) {
    var amount = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
    var tax = $('#ddlAdditionalChargesTax' + count).val().split('-')[1];
    var incTax = ((tax / 100) * amount) + amount;
    $('#txtAdditionalChargesAmountIncTax' + count).val(incTax);
    discallcalc();
}

function applyITCExemptionLogicForAdditionalCharges() {
    // Loop through all additional charges
    $('.additional-charges-row').each(function () {
        var count = $(this).find('input[id^="txtAdditionalChargesAmountExcTax"]').attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        
        // Hide both sections initially
        $('#divTaxExemptionAC' + count).hide();
        $('#divInputTaxCreditAC' + count).hide();
        
        // Clear existing content
        $('#divTaxExemptionAC' + count).empty();
        $('#divInputTaxCreditAC' + count).empty();
        
        var taxValue = $("#ddlAdditionalChargesTax" + count + " :selected").text();
        
        if (taxValue == "Non-Taxable") {
            // Show exemption reason for Non-Taxable
            var selectedExemptionText = $('#exemptionReasonLabel' + count).text() || 'None';
            var selectedExemptionId = $('#exemptionReasonId' + count).val() || '0';
            
            var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
                '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
                '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
                '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
                '<i class="fas fa-edit"></i>' +
                '</a>' +
                '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
                '</div>';
            
            $('#divTaxExemptionAC' + count).html(exemptionDiv);
            $('#divTaxExemptionAC' + count).show();
        }
        else if (taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
            // Show ITC for taxable items
            var selectedITCText = $('#itcTypeLabel' + count).text() || 'Eligible For ITC';
            var selectedITCValue = $('#itcTypeValue' + count).val() || 'Eligible For ITC';
            
            var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
                '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
                '<span id="itcTypeLabel' + count + '" class="small">' + selectedITCText + '</span>' +
                '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
                '<i class="fas fa-edit"></i>' +
                '</a>' +
                '<input type="hidden" id="itcTypeValue' + count + '" value="' + selectedITCValue + '">' +
                '</div>';
            
            $('#divInputTaxCreditAC' + count).html(itcDiv);
            $('#divInputTaxCreditAC' + count).show();
        }
    });
}

// Add supporting JS functions for ITC modal
var currentITCEditIndex = null;
function openITCModal(count) {
    currentITCEditIndex = count;
    var $dropdown = $('#ddlITCType');

    $dropdown.empty().append(
        $('<option>', {
            value: 'Eligible For ITC',
            text: 'Eligible For ITC'
        }),
        $('<option>', {
            value: 'Ineligible - As per Section 17 (5)',
            text: 'Ineligible - As per Section 17 (5)'
        }),
        $('<option>', {
            value: 'Ineligible - Others',
            text: 'Ineligible - Others'
        })
    );

    // Set the current value if it exists
    var currentValue = $('#itcTypeValue' + count).val();
    if (currentValue) {
        $dropdown.val(currentValue).trigger('change');
    }

    $('#ddlITCType').select2();
    $('#itcModal').modal('show');
}

function saveITCSelection() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if (currentITCEditIndex == null) {
        return;
    }

    var $dropdown = $('#ddlITCType');
    var $label = $('#itcTypeLabel' + currentITCEditIndex);
    var $hiddenValue = $('#itcTypeValue' + currentITCEditIndex);

    if ($dropdown.length === 0) {
        return;
    }

    if ($label.length === 0) {
        return;
    }

    if ($hiddenValue.length === 0) {
        return;
    }

    // Update the label and hidden input with selected values
    var selectedText = $dropdown.find('option:selected').text();
    var selectedValue = $dropdown.val();

    if (!selectedValue) {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');

        $('#divITCType').show();
        $('#divITCType').text('This field is required');

        $('#ddlITCType').next('.select2').find('.select2-selection').css('border', '2px solid #dc3545');
        return
    }

    $label.text(selectedText);
    $hiddenValue.val(selectedValue);

    // Hide the modal
    $('#itcModal').modal('hide');
    currentITCEditIndex = null;
}