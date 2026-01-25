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

    $('#_Date').addClass('notranslate');
    $('#_DueDate').addClass('notranslate');

    $('textarea#txtNotes').summernote({
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

    $('textarea#txtTerms').summernote({
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

    if (window.location.href.toLowerCase().indexOf('salesreturncreate') == -1) {
        convertAvailableStock();        
    }
    else {
        fetchAdditionalCharges();
    }

    if ($('#txtInvoiceNo').val()) {
        $('#txtInvoiceNo').prop('disabled', true);
    }

    //if (window.location.href.indexOf('edit') == -1) {
    //    fetchAdditionalCharges();
    //}

    fetchTax();
    fetchCompanyCurrency();
    fetchTaxExemptions();
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
var ShippingDocument = "";
var FileExtensionShippingDocument = "";
var count = 1; var innerCount = 1; var dropdownHtml = '';
var _SalesReturnId = 0;
var SalesType = "";
var _BranchId = 0;
var NotificationName = '', NotificationId = 0;
var skuCodes = [];
var IsBillOfSupply = false;
var taxExemptions = [];
// Reward points variables
var originalSalePointsEarned = 0;
var originalSalePointsRedeemed = 0;
var originalSaleGrandTotal = 0;
var rewardPointsSetting = null;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlSupplier').val(),
        Status: $('#ddlStatus').val(),
        InvoiceNo: $('#txtReferenceNo').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnFetch',
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

function deleteCombo(id, SalesReturnDetailsId) {
    if (SalesReturnDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {

            var skuRemove = $('#txtSkuCode' + id).val();
            skuCodes = jQuery.grep(skuCodes, function (value) {
                return value.trim() != skuRemove.trim();
            });

            $('#divCombo' + id).hide();
            $('#txtQuantityReturned' + id).val(0);
            updateAmount();

            //var det = {
            //    SalesReturnDetailsId: SalesReturnDetailsId
            //}
            //$("#divLoading").show();
            //$.ajax({
            //    url: '/SalesReturn/SalesReturnDetailsDelete',
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
        updateAmount();
    }
}

function deleteFullCombo(SalesId) {
    if (SalesId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
            $('#divCombo tr').each(function () {
                var _id = this.id.split('divCombo')[1];
                if (_id != undefined) {
                    $('#divCombo' + _id).hide();
                    $('#txtQuantityReturned' + _id).val(0);
                }
            });
            $('#divComboNetAmount').text(0);
            updateAmount();
            skuCodes = [];

            //var det = {
            //    SalesId: SalesId
            //}
            //$("#divLoading").show();
            //$.ajax({
            //    url: '/Sales/SalesDetailsDelete',
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
        updateAmount();
        skuCodes = [];
    }
}

function insert() {
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
                    SalesReturnDetailsId: $('#txtSalesReturnDetailsId' + _id).val(),
                    Quantity: $('#txtQuantityReturned' + _id).val(),
                    ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                    ItemId: $('#txtItemId' + _id).val(),
                    UnitCost: $('#txtUnitCost' + _id).val(),
                    Amount: $('#txtAmount' + _id).text(),
                    IsActive: true,
                    IsDeleted: $("#divCombo" + _id).is(":hidden"),
                    PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                    FreeQuantity: $('#txtFreeQuantityReturned' + _id).val(),
                    UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                    SalesDetailsId: $('#txtSalesDetailsId' + _id).val(),
                    TaxId: $('#ddlTax' + _id).val().split('-')[0],
                    DiscountType: $('#ddlDiscountType' + _id).val(),
                    Discount: $('#txtDiscount' + _id).val(),
                    //UnitCost: $('#hdnUnitCost' + _id).val(),
                    PriceIncTax: $('#txtPurchaseIncTax' + _id).val(),
                    AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                    TaxAmount: $('#txtTaxAmount' + _id).val(),
                    TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                    PriceExcTax: $('#txtPurchaseExcTax' + _id).val(),
                    AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                    ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
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
            TaxExemptionId: taxExemptionId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        BranchId: $('#ddlBranch').val(),
        SalesId: $('#ddlSalesInvoice').val(),//window.location.href.split('=')[1] == undefined ? $('#ddlSalesInvoice').val() : window.location.href.split('=')[1],
        //SalesReturnId: $("#hdnSalesReturnId").val(),
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        Status: $("#ddlStatus").val(),
        InvoiceNo: $("#txtInvoiceNo").val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        OtherCharges: $("#txtOtherCharges").val(),
        PackagingCharge: $('#txtPackagingCharge').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        TotalQuantity: $('#divTotalQty').text(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        Subtotal: $("#hdndivTotalAmount").val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Notes: $("#txtNotes").summernote('code') || $("#txtNotes").val(), Terms: $("#txtTerms").summernote('code') || $("#txtTerms").val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        IsActive: true,
        IsDeleted: false,
        SalesReturnDetails: ItemDetails,
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        SalesCreditNoteReasonId: $('#ddlSalesCreditNoteReason').val(),
        CustomerId: $('#ddlCustomer').val(),
        PayTaxForExport: $('#chkPayTaxForExport').is(':checked') == true ? 1 : 2,
        TaxCollectedFromCustomer: $('#chkTaxCollectedFromCustomer').is(':checked') == true ? 1 : 2,
        SalesReturnAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnInsert',
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
                if (data.Data.Sale.SalesType == 'Sales') {
                    if (data.Data.SaleSetting.AutoPrintInvoiceSalesReturn == true) {
                        if (data.Data.SaleSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnInvoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnReceipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }
                else if (data.Data.Sale.SalesType == 'Pos') {
                    if (data.Data.PosSetting.AutoPrintInvoiceSalesReturn == true) {
                        if (data.Data.PosSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnInvoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnReceipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }

                window.location.href = "/sales/SalesReturn";

            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update() {
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
                SalesReturnDetailsId: $('#txtSalesReturnDetailsId' + _id).val(),
                Quantity: $('#txtQuantityReturned' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                Amount: $('#txtAmount' + _id).text(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                FreeQuantity: $('#txtFreeQuantityReturned' + _id).val(),
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                SalesDetailsId: $('#txtSalesDetailsId' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                DiscountType: $('#ddlDiscountType' + _id).val(),
                Discount: $('#txtDiscount' + _id).val(),
                //UnitCost: $('#hdnUnitCost' + _id).val(),
                PriceIncTax: $('#txtPurchaseIncTax' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                PriceExcTax: $('#txtPurchaseExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
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
        var salesReturnAdditionalChargesId = $('#txtSalesReturnAdditionalChargesId' + count).val();
        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            TaxExemptionId: taxExemptionId,
            SalesReturnAdditionalChargesId: salesReturnAdditionalChargesId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        SalesReturnId: window.location.href.split('=')[1],
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        SalesId: $('#ddlSalesInvoice').val(),
        BranchId: $('#ddlBranch').val(),
        //SalesId: window.location.href.split('=')[1] == undefined ? $('#ddlSalesInvoice').val() : window.location.href.split('=')[1],
        //SalesReturnId: $("#hdnSalesReturnId").val(),        
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        Status: $("#ddlStatus").val(),
        InvoiceNo: $("#txtInvoiceNo").val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        OtherCharges: $("#txtOtherCharges").val(),
        PackagingCharge: $('#txtPackagingCharge').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        TotalQuantity: $('#divTotalQty').text(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        Subtotal: $("#hdndivTotalAmount").val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Notes: $("#txtNotes").summernote('code') || $("#txtNotes").val(), Terms: $("#txtTerms").summernote('code') || $("#txtTerms").val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        IsActive: true,
        IsDeleted: false,
        SalesReturnDetails: ItemDetails,
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        SalesCreditNoteReasonId: $('#ddlSalesCreditNoteReason').val(),
        CustomerId: $('#ddlCustomer').val(),
        PayTaxForExport: $('#chkPayTaxForExport').is(':checked') == true ? 1 : 2,
        TaxCollectedFromCustomer: $('#chkTaxCollectedFromCustomer').is(':checked') == true ? 1 : 2,
        SalesReturnAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnUpdate',
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
                if (data.Data.Sale.SalesType == 'Sales') {
                    if (data.Data.SaleSetting.AutoPrintInvoiceSalesReturn == true) {
                        if (data.Data.SaleSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnInvoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnReceipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }
                else if (data.Data.Sale.SalesType == 'Pos') {
                    if (data.Data.PosSetting.AutoPrintInvoiceSalesReturn == true) {
                        if (data.Data.PosSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnInvoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/SalesReturnReceipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }

                window.location.href = "/sales/SalesReturn";

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

function getShippingDocumentBase64() {
    var file1 = $("#ShippingDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ShippingDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            ShippingDocument = reader.result;
            FileExtensionShippingDocument = '.' + file1.name.split('.').pop();

            $('#blahShippingDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function Delete(SalesReturnId, CustomerPaymentId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SalesReturnId: SalesReturnId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/SalesReturndelete',
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
                    DeleteCustomerPayment(CustomerPaymentId);
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function Cancel(SalesReturnId, CustomerPaymentId) {
    var r = confirm("This will cancel permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SalesReturnId: SalesReturnId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/SalesReturnCancel',
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
                    DeleteCustomerPayment(CustomerPaymentId);
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function openPaymentModal(type, SalesReturnId, salesType, BranchId) {
    _SalesReturnId = SalesReturnId;
    _BranchId = BranchId;
    var det = {
        SalesReturnId: SalesReturnId,
        Type: salesType,
        BranchId: _BranchId,
        IsAdvance: false
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnPayments',
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

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        SalesReturnId: _SalesReturnId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Customer Refund",
        AccountId: $('#ddlLAccount').val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnPaymentInsert',
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
                $("#paymentModal").modal('hide');
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
    var UnitCost = $('#hdnUnitCost' + i).val();
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

    if (TToQValue != 0) {
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
    }
    else if (SToTValue != 0) {
        if (index == 2) {
            newUnitCost = PrimaryUnitCost;
            newSalesCost = PrimarySalesCost;
        }
        else if (index == 3) {
            newUnitCost = SecondaryUnitCost;
            newSalesCost = SecondarySalesCost;
        }
        else if (index == 4) {
            newUnitCost = TertiaryUnitCost;
            newSalesCost = TertiarySalesCost;
        }
    }
    else if (UToSValue != 0) {
        if (index == 3) {
            newUnitCost = PrimaryUnitCost;
            newSalesCost = PrimarySalesCost;
        }
        else if (index == 4) {
            newUnitCost = SecondaryUnitCost;
            newSalesCost = SecondarySalesCost;
        }
    }
    else {
        newUnitCost = PrimaryUnitCost;
        newSalesCost = PrimarySalesCost;
    }

    $('#txtPurchaseExcTax' + i).val(newUnitCost / ExchangeRate);
    $('#txtPurchaseIncTax' + i).val(newUnitCost / ExchangeRate);
    $('#txtUnitCost' + i).val(newUnitCost.toFixed(2) / ExchangeRate);
    $('#txtSalesIncTax' + i).val(newSalesCost.toFixed(2) / ExchangeRate);

    $('#ddlFreeQuantityReturnedUnit' + i).val($('#ddlUnit' + i).val());

    ChangeQtyAmount(i);
}

function ChangeQtyAmount(id) {
    setExemption();
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

            var Quantity = parseFloat($('#txtQuantityReturned' + _id).val());
            var FreeQuantity = parseFloat($('#txtFreeQuantityReturned' + _id).val());

            newQuantity = Quantity + FreeQuantity;
            if (newQuantity > StockQuantity) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                $('#txtQuantityReturned' + _id).val(0);
                $('#txtFreeQuantityReturned' + _id).val(0);
                //$('#divQuantity' + _id).text('Not enough stock available');
                //$('#divQuantity' + _id).show();
                $('.divQuantity' + _id + '_ctrl').css('border', '2px solid #dc3545');
            }
        }

        ChangeQtyAmount(_id);
    }
}

function updateAmount() {
    var amount = 0;
    var qty = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        amount = amount + (parseFloat($('#txtUnitCost' + _id).val()) * parseFloat($('#txtQuantityReturned' + _id).val()));
        qty = qty + parseFloat($('#txtQuantityReturned' + _id).val()) + parseFloat($('#txtFreeQuantityReturned' + _id).val());
    });

    $('#divTotalAmount').text(CurrencySymbol + Math.round(amount * 100) / 100);
    $('#hdndivTotalAmount').val(amount);
    $('#divTotalQty').text(qty);
    /*chargecalc();*/
    discallcalc();
}

function discallcalc() {

    var taxs = [];
    var _extraDiscount = 0;

    var amountExcTax = 0, amountIncTax = 0, discount = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
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
                
                // Calculate reward points impact
                calculateRewardPointsImpact();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function calculateRewardPointsImpact() {
    // Check if reward points section exists
    if ($('#divRewardPointsReturnSection').length === 0) {
        return;
    }
    
    // Get current return grand total
    var returnGrandTotal = parseFloat($('#divGrandTotal').text().replace(/[^-0-9\.]+/g, '')) || 0;
    
    // If no original sale data, return
    if (originalSaleGrandTotal <= 0) {
        $('#txtPointsToReverse').text('0');
        $('#txtPointsToRestore').text('0');
        $('#txtNetPointsImpact').text('0');
        $('#pointsReturnInfo').html('');
        return;
    }
    
    // Calculate return ratio
    var returnRatio = returnGrandTotal / originalSaleGrandTotal;
    if (returnRatio > 1) returnRatio = 1; // Cap at 100%
    if (returnRatio < 0) returnRatio = 0;
    
    // Calculate points to reverse (proportional to return amount)
    var pointsToReverse = 0;
    if (originalSalePointsEarned > 0) {
        pointsToReverse = Math.floor(originalSalePointsEarned * returnRatio);
    }
    
    // Calculate points to restore (proportional to return amount)
    var pointsToRestore = 0;
    if (originalSalePointsRedeemed > 0) {
        pointsToRestore = Math.floor(originalSalePointsRedeemed * returnRatio);
    }
    
    // Calculate net impact
    var netPointsImpact = pointsToRestore - pointsToReverse;
    
    // Update UI
    $('#txtPointsToReverse').text(pointsToReverse);
    $('#txtPointsToRestore').text(pointsToRestore);
    $('#txtNetPointsImpact').text(netPointsImpact);
    
    // Update info text
    var infoText = '';
    if (pointsToReverse > 0) {
        infoText += '<span class="text-danger">-' + pointsToReverse + ' points will be deducted</span>';
    }
    if (pointsToRestore > 0) {
        if (infoText) infoText += ' | ';
        infoText += '<span class="text-success">+' + pointsToRestore + ' points will be restored</span>';
    }
    if (!infoText) {
        infoText = '<span class="text-muted">No points impact</span>';
    }
    $('#pointsReturnInfo').html(infoText);
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
        let discount = 0;
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        }
        else {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + id).val()) / 100) * parseFloat($('#txtUnitCost' + id).val());
        }
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

function View(SalesReturnId, CustomerPaymentId) {
    var det = {
        SalesReturnId: SalesReturnId,
        CustomerPaymentId: CustomerPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnView',
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

function ViewPayment(CustomerRefundId) {
    var det = {
        CustomerRefundId: CustomerRefundId,
        Type: "Customer Refund",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/RefundView',
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

                ChangePurAmount(taxModalId);
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

                ChangePurAmount(taxModalId);
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
                ChangePurAmount(taxModalId);
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
                $('#ddlPaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPaymentTerm').val(data.Data.PaymentTerm.PaymentTermId);

                $('#paymentTermModal').modal('toggle');

                $('#txtPaymentTerm_M').val('');
                $('#txtDays_M').val('');

                setDueDate();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
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

function fetchSalesInvoices() {
    $('#divCombo').empty();
    // Reset reward points data
    originalSalePointsEarned = 0;
    originalSalePointsRedeemed = 0;
    originalSaleGrandTotal = 0;
    $('#txtOriginalPointsEarned').text('0');
    $('#txtOriginalPointsRedeemed').text('0');
    $('#txtPointsToReverse').text('0');
    $('#txtPointsToRestore').text('0');
    $('#txtNetPointsImpact').text('0');
    $('#pointsReturnInfo').html('');
    
    var det = {
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        SalesType: 'Credit Note Add'
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesInvoices',
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
                //            $('#ddlPlaceOfSupply').val(data.Data.User.PlaceOfSupplyId);
                //            $('.divPlaceOfSupply').show();
                //            $('.divPayTaxForExport').hide();
                //        }
                //        else {
                //            $('#ddlPlaceOfSupply').val(0);
                //            $('.divPlaceOfSupply').hide();
                //            $('.divPayTaxForExport').show();
                //        }
                //    }
                //}
                //else {
                //    $('#ddlPlaceOfSupply').val(0);
                //}

                var ddlSalesInvoice = '<label>Sales Invoice </label> <div class="input-group"><select class="form-control select2" id="ddlSalesInvoice"  onchange="setSalesReturnType()">';

                ddlSalesInvoice = ddlSalesInvoice + '<option value="0">Select</option>';

                for (let ss = 0; ss < data.Data.Sales.length; ss++) {
                    ddlSalesInvoice = ddlSalesInvoice + '<option value="' + data.Data.Sales[ss].SalesId + '">' + data.Data.Sales[ss].InvoiceNo + '</option>';
                }
                ddlSalesInvoice = ddlSalesInvoice + '</select></div><small class="text-red font-weight-bold errorText" id="divSalesInvoice"></small>';

                $('.divSalesInvoice').empty();
                $('.divSalesInvoice').append(ddlSalesInvoice);

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
}

function setSalesReturnType() {
    $('#divCombo').empty();
    
    // Reset reward points data when invoice changes
    if ($('#ddlSalesInvoice').val() == '0' || !$('#ddlSalesInvoice').val()) {
        originalSalePointsEarned = 0;
        originalSalePointsRedeemed = 0;
        originalSaleGrandTotal = 0;
        $('#txtOriginalPointsEarned').text('0');
        $('#txtOriginalPointsRedeemed').text('0');
        $('#txtPointsToReverse').text('0');
        $('#txtPointsToRestore').text('0');
        $('#txtNetPointsImpact').text('0');
        $('#pointsReturnInfo').html('');
        return;
    }

    var det = {
        SalesId: $('#ddlSalesInvoice').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesDetails',
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

                //$("#tblSalesDetails").html(data);
                //$('.chkIsActive').bootstrapToggle();

                $('#ddlPlaceOfSupply').val(data.Data.SalesReturn.PlaceOfSupplyId);
                debugger
                // Store original sale reward points data
                if (data.Data.SalesReturn) {
                    originalSalePointsEarned = data.Data.SalesReturn.PointsEarned || 0;
                    originalSalePointsRedeemed = data.Data.SalesReturn.RedeemPoints || 0;
                    originalSaleGrandTotal = data.Data.SalesReturn.GrandTotal || 0;
                    
                    // Display original points
                    $('#txtOriginalPointsEarned').text(originalSalePointsEarned);
                    $('#txtOriginalPointsRedeemed').text(originalSalePointsRedeemed);
                }

                $('.select2').select2();
                $("#divLoading").hide();

                //convertAvailableStock();

                fetchAdditionalCharges();
                
                // Calculate reward points impact
                calculateRewardPointsImpact();
            }

        },
        error: function (xhr) {

        }
    });
}

function setReturnItem(data) {
    var html = '';
    $('#txttags').val('');
    for (let i = 0; i < data.Data.SalesDetails.length; i++) {
        let taxamt = (Math.round((parseFloat(data.Data.SalesDetails[i].SalesIncTax) - parseFloat(data.Data.SalesDetails[i].SalesExcTax)) * 100) / 100);
        var variation = '';
        if (data.Data.SalesDetails[i].VariationName) {
            variation = '</br> Variation : ' + data.Data.SalesDetails[i].VariationName;
        }
        var ddlUnit = '<select style="min-width:80px" class="form-control ' + (data.Data.SalesDetails[i].UnitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

        if (data.Data.SalesDetails[i].QuaternaryUnitId != 0) {
            if (data.Data.SalesDetails[i].PriceAddedFor == 1) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '-1-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].UnitId + '-1-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 2) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 3) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.SalesDetails[i].TertiaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.SalesDetails[i].TertiaryUnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 4) {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.SalesDetails[i].QuaternaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.SalesDetails[i].QuaternaryUnitShortName + '</option>';
            }
        }
        else if (data.Data.SalesDetails[i].TertiaryUnitId != 0) {
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            if (data.Data.SalesDetails[i].PriceAddedFor == 2) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '-2-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].UnitId + '-2-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 3) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 4) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.SalesDetails[i].TertiaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.SalesDetails[i].TertiaryUnitShortName + '</option>';
            }
        }
        else if (data.Data.SalesDetails[i].SecondaryUnitId != 0) {
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            if (data.Data.SalesDetails[i].PriceAddedFor == 3) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '-3-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].UnitId + '-3-1' + '">' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            }
            if (data.Data.SalesDetails[i].PriceAddedFor == 4) {
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
            else {
                ddlUnit = ddlUnit + '<option value="' + data.Data.SalesDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.SalesDetails[i].SecondaryUnitShortName + '</option>';
            }
        }
        else {
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            //ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '" hidden>' + data.Data.SalesDetails[i].UnitShortName + '</option>';
            ddlUnit = ddlUnit + '<option selected value="' + data.Data.SalesDetails[i].UnitId + '-4-1' + '">' + (data.Data.SalesDetails[i].UnitShortName == null ? "N/A" : data.Data.SalesDetails[i].UnitShortName) + '</option>';
        }

        ddlUnit = ddlUnit + '</select >';

        var ddlTax = '<select class="form-control ddlTax" style="min-width:80px" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')" >';
        for (let ss = 0; ss < taxList.length; ss++) {
            if (taxList[ss].Tax != "Taxable") {
                if (data.Data.BusinessSetting.CountryId == 2) {
                    if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val()) {
                        if (taxList[ss].TaxTypeId != 3) {
                            if (data.Data.SalesDetails[i].TaxId == taxList[ss].TaxId) {
                                ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                            else {
                                ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                        }
                    }
                    else {
                        if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                            if (data.Data.SalesDetails[i].TaxId == taxList[ss].TaxId) {
                                ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                            else {
                                ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                            }
                        }
                    }
                }
                else {
                    if (data.Data.SalesDetails[i].TaxId == taxList[ss].TaxId) {
                        ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                    }
                    else {
                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                    }
                }
            }
        }
        ddlTax = ddlTax + '</select>';

        var ddlWarranty = '<select class="form-control select2" id="ddlWarranty' + count + '" onchange="ChangePurAmount(' + count + ')"><option selected value="0">None</option>';
        for (let ss = 0; ss < warrantyList.length; ss++) {
            if (data.Data.SalesDetails[i].WarrantyId == warrantyList[ss].WarrantyId) {
                ddlWarranty = ddlWarranty + '<option selected value="' + warrantyList[ss].WarrantyId + '">' + warrantyList[ss].Warranty + '</option>';
            }
            else {
                ddlWarranty = ddlWarranty + '<option value="' + warrantyList[ss].WarrantyId + '">' + warrantyList[ss].Warranty + '</option>';
            }
        }
        ddlWarranty = ddlWarranty + '</select >';

        var ddlTaxExemption = '<select class="form-control select2" id="ddlTaxExemption' + count + '"><option selected value="0">None</option>';
        for (let ss = 0; ss < taxExemptions.length; ss++) {
            if (data.Data.SalesDetails[i].TaxExemptionId == taxExemptions[ss].TaxExemptionId) {
                ddlTaxExemption = ddlTaxExemption + '<option selected value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
            }
            else {
                ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
            }
        }
        ddlTaxExemption = ddlTaxExemption + '</select >';

        //var ddlItemCode = '<select class="form-control select2" id="ddlItemCode' + count + '"><option selected value="0">None</option>';
        //for (let ss = 0; ss < itemCodes.length; ss++) {
        //    if (data.Data.SalesDetails[i].ItemType == 'Product') {
        //        if (itemCodes[ss].ItemCodeType == "HSN") {
        //            if (data.Data.SalesDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
        //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        //            }
        //            else {
        //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        //            }
        //        }
        //    }
        //    else {
        //        if (itemCodes[ss].ItemCodeType == "SAC") {
        //            if (data.Data.SalesDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
        //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        //            }
        //            else {
        //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        //            }
        //        }
        //    }
        //}
        //ddlItemCode = ddlItemCode + '</select >';

        var SalePriceIsMinSellingPrice = $('#hdnSalePriceIsMinSellingPrice').val().toLocaleLowerCase();
        var EnableLotNo = $('#hdnEnableLotNo').val().toLocaleLowerCase();
        var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());
        var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
        var EnableFreeQuantity = $('#hdnEnableFreeQuantity').val().toLocaleLowerCase();
        var OnItemExpiry = $('#hdnOnItemExpiry').val();
        var EnableWarranty = $('#hdnEnableWarranty').val().toLocaleLowerCase();

        //Set dropdown for lots
        if (data.Data.SalesDetails[i].AvailableLots != null) {
            var ddlLot = '<select class="form-control" id="ddlLot' + count + '" onchange="fetchLotDetails(' + count + ')">';
            //if (OnItemExpiry != 2) {
            //    ddlLot = ddlLot + '<option selected value="0-all" > Default Stock Accounting Method</option>';
            //}

            for (let j = 0; j < data.Data.SalesDetails[i].AvailableLots.length; j++) {
                ddlLot = ddlLot + '<option value="' + data.Data.SalesDetails[i].AvailableLots[j].Id + '-' + data.Data.SalesDetails[i].AvailableLots[j].Type + '"> Lot No: ' + (!data.Data.SalesDetails[i].AvailableLots[j].LotNo ? "" : data.Data.SalesDetails[i].AvailableLots[j].LotNo);
                if (EnableItemExpiry == 'true') {
                    ddlLot = ddlLot + (!data.Data.SalesDetails[i].AvailableLots[j].ExpiryDate ? "" : '- Exp Date: ' + getFormattedDate(new Date(parseInt(data.Data.SalesDetails[i].AvailableLots[j].ExpiryDate.replaceAll('/', '').replaceAll('Date', '').replace('(', '').replace(')', ''))).toUTCString()));
                }
                ddlLot = ddlLot + '</option>';
            }
            ddlLot = ddlLot + '</select>';
            //Set dropdown for lots
        }
        else {
            var ddlLot = '<span style="text-align:center;display:block">-</span>';
        }

        html = html + '<tr id="divCombo' + count + '">' +
            '<td style="min-width:100px">' +
            '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.SalesDetails[i].ItemId + '">' +
            '<input type="text" hidden class="form-control" id="txtSkuCode' + count + '" value="' + data.Data.SalesDetails[i].SKU + '">' +
            '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.SalesDetails[i].ItemDetailsId + '">' +
            '<span class="' + (data.Data.SalesDetails[i].ItemName.length > 15 ? '' : 'hidden') + '">Name : ' + data.Data.SalesDetails[i].ItemName.substring(0, 15) + '...</span>' +
            '<span class="' + (data.Data.SalesDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '">Name : ' + data.Data.SalesDetails[i].ItemName + '</span>' +
            variation +
            ' </br> SKU : ' + data.Data.SalesDetails[i].SKU + '' +
            '<textarea style="min-width:200px;display:' + (data.Data.SalesDetails[i].EnableImei == true ? 'block' : 'none') + '" type="text" class="form-control" id="txtOtherInfo' + count + '" placeholder="IMEI, Serial number or other informations"></textarea>' +
            //'<div class="form-group" style="display:' + (data.Data.BusinessSetting.CountryId == 2 ? '' : 'none') + '">' +
            //'<label style="margin-bottom:0;margin-top:0.5rem;">' + (data.Data.SalesDetails[i].ItemType == 'Product' ? 'HSN Code' : 'SAC Code') + ' <span class="danger">*</span></label>' +
            //'<div class="input-group">' +
            //ddlItemCode +
            //'</div>' +
            //'</div>' +
            //'<div class="form-group ' + (EnableLotNo == 'true' ? '' : 'hidden') + '">' +
            //'<label style="margin-bottom:0;margin-top:0.5rem;">Lot No </label>' +
            //'<div class="input-group">' +
            //ddlLot +
            //'</div>' +
            //'</div>' +
            '<div class="form-group ' + (EnableWarranty == 'true' ? '' : 'hidden') + '">' +
            '<label style="margin-bottom:0;margin-top:0.5rem;">Warranty </label>' +
            '<div class="input-group">' +
            ddlWarranty +
            '</div>' +
            '</div>' +
            '</td>' +
            '<td style="min-width:120px" class="' + (EnableLotNo == 'true' ? '' : 'hidden') + '"> ' + ddlLot +
            '</td>' +
            //'<td style="min-width:100px"> ' +
            //'<div class="input-group">' +
            //ddlUnit +
            //'</div >' +
            //'<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.SalesDetails[i].PriceAddedFor + '">' +
            //'<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.SalesDetails[i].UToSValue + '">' +
            //'<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.SalesDetails[i].SToTValue + '">' +
            //'<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.SalesDetails[i].TToQValue + '">' +
            //'<input type="hidden" id="hdnUnitCost' + count + '" value="' + data.Data.SalesDetails[i].SalesExcTax + '">' +
            //'<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.SalesDetails[i].SalesIncTax + '">' +
            //'<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].AllowDecimal + '">' +
            //'<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].SecondaryUnitAllowDecimal + '">' +
            //'<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].TertiaryUnitAllowDecimal + '">' +
            //'<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].QuaternaryUnitAllowDecimal + '">' +
            //'<input type="text" hidden class="form-control" id="hdnIsManageStock' + count + '" value="' + data.Data.SalesDetails[i].IsManageStock + '">' +
            //'</td>' +
            //'<td style="min-width:150px" class="text-center">' +
            //'<div class="input-group">' +
            //'<span class="' + (data.Data.SalesDetails[i].IsManageStock == false ? '' : 'hidden') + '" style="margin-left:auto;margin-right:auto;">-</span>' +
            //'<input type="number" disabled class="form-control ' + (data.Data.SalesDetails[i].IsManageStock == true ? '' : 'hidden') + '" placeholder="Quantity" value="' + data.Data.SalesDetails[i].Quantity + '" id="txtStockQuantity' + count + '">' +
            //'<input type="hidden" id="hdnQuantityRemaining' + count + '" value="' + data.Data.SalesDetails[i].Quantity + '">' +
            //'</div>' +
            //'</td>' +
            '<td> ' +
            '<div class="input-group" style="min-width:150px">' +
            '<input onKeyPress="toggleDecimal(event,' + count + ')" type="number" class="form-control divQuantity' + count + '_ctrl" value="1" id="txtQuantity' + count + '" onchange="updateQuantity(' + count + ')" min="0"> ' +
            ddlUnit +
            '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.SalesDetails[i].PriceAddedFor + '">' +
            '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.SalesDetails[i].UToSValue + '">' +
            '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.SalesDetails[i].SToTValue + '">' +
            '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.SalesDetails[i].TToQValue + '">' +
            '<input type="hidden" id="hdnUnitCost' + count + '" value="' + data.Data.SalesDetails[i].SalesExcTax + '">' +
            '<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.SalesDetails[i].SalesIncTax + '">' +
            '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].AllowDecimal + '">' +
            '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].SecondaryUnitAllowDecimal + '">' +
            '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].TertiaryUnitAllowDecimal + '">' +
            '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.SalesDetails[i].QuaternaryUnitAllowDecimal + '">' +
            '<input type="text" hidden class="form-control" id="hdnIsManageStock' + count + '" value="' + data.Data.SalesDetails[i].IsManageStock + '">' +
            '</div >' +
            /*'<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small></br>' +*/
            '<small class="text-success font-weight-bold ' + (data.Data.SalesDetails[i].IsManageStock == true ? '' : 'hidden') + '" id="txtStockQuantity' + count + '">Available Qty: ' + data.Data.SalesDetails[i].Quantity + '</small>' +
            '<input type="hidden" id="hdnQuantityRemaining' + count + '" value="' + data.Data.SalesDetails[i].Quantity + '">' +
            '</td>' +
            '<td style="min-width:100px" class="' + (EnableFreeQuantity == 'true' ? '' : 'hidden') + '"> ' +
            '<div class="input-group">' +
            '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="0" id="txtFreeQuantity' + count + '" onchange="updateQuantity(' + count + ')" min="0"> ' +
            '</div >' +
            '</td>' +
            '<td style="min-width:120px">' +
            '<input type="number"' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '"  value="' + data.Data.SalesDetails[i].SalesExcTax / ExchangeRate + '" onchange="ChangeQtyAmount(' + count + ')">' +
            '<small class="text-red font-weight-bold errorText" id="divUnitCost' + count + '"></small>' +
            '</td>' +
            '<td>' +
            '<div class="input-group" style="min-width:150px">' +
            '<input ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeQtyAmount(' + count + ')" value="' + data.Data.SalesDetails[i].Discount / ExchangeRate + '">' +
            '<select style="min-width:100px" ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" id="ddlDiscountType' + count + '" onchange="ChangeQtyAmount(' + count + ')">' +
            '<option value="Fixed">Fixed</option>' +
            '<option value="Percentage">Percentage</option>' +
            '</select>' +
            '</div >' +
            '<input type="text" hidden class="form-control" id="hdnExtraDiscounts' + count + '" value="0">' +
            '</td>' +
            '<td style="min-width:100px;display:none;">' +
            '<input onKeyPress="onlyNumberKey(event)" type="number" ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" value="' + data.Data.SalesDetails[i].SalesExcTax + '" id="txtPurchaseExcTax' + count + '"  onchange="ChangePriceBefTax(' + count + ')" min="0"/>' +
            '</td>' +
            '<td style="min-width:100px;display:none">' +
            '<input type="number" class="form-control" disabled value="' + data.Data.SalesDetails[i].SalesExcTax + '" id="txtAmountExcTax' + count + '" min="0"/>' +
            '</td>' +
            '<td id="tdTax' + count + '">' +
            '<div class="input-group" style="min-width:160px">' +
            ddlTax +
            '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
            '<input hidden type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
            '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.SalesDetails[i].TaxId + '">' +
            '<div id="divTaxExemption' + count + '" class="form-group" style="display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.SalesDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
            '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
            '<div class="input-group">' +
            ddlTaxExemption +
            '</div>' +
            '</div>' +
            '</div >' +
            /*'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +*/
            '</td>' +
            //'<td style="min-width:150px">' +
            //'<input type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '">' +
            //'<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.SalesDetails[i].TaxId + '">' +
            //'</td>' +
            '<td style="min-width:100px;display:none">' +
            '<input type="number" class="form-control" id="txtPurchaseIncTax' + count + '"  value="' + data.Data.SalesDetails[i].SalesIncTax / ExchangeRate + '" onchange="updateNetCost(' + count + ')">' +
            '<input type="hidden" value="' + data.Data.SalesDetails[i].SalesIncTax + '" id="hdnPurchaseIncTax' + count + '" />' +
            '</td>' +
            '<td style="min-width:120px">' +
            '<input type="number" disabled class="form-control"  id="txtAmountIncTax' + count + '" value="' + data.Data.SalesDetails[i].SalesIncTax / ExchangeRate + '" >' +
            '<input type="hidden" id="hdnAmountIncTax' + count + '" value="' + data.Data.SalesDetails[i].SalesIncTax + '" >' +
            '</td>' +
            //'<td style="min-width:120px;display:' + (EnableWarranty == 'true' ? 'block' : 'none') + '">' +
            //'<div class="input-group">' +
            //ddlWarranty +
            //'</div >' +
            //'</td>' +
            '<td>' +
            '<button type="button" class="btn btn-danger btn-sm" onclick="deleteCombo(' + count + ')">' +
            '<i class="fas fa-times">' +
            '</i>' +
            '</button>' +
            '</td>' +
            '</tr>';
    }
    $('#divCombo').append(html);
    $('.select2').select2();
    convertAvailableStock();
}

function ChangePriceBefTax(id) {
    if (id != undefined) {
        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        var _extraDiscount = $('#hdnExtraDiscounts' + id).val() / parseFloat($('#txtQuantityReturned' + id).val());

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * (parseFloat($('#txtPurchaseExcTax' + id).val()))) * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function updateNetCost(id) {
    if (id != undefined) {
        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];

        let PurchaseExcTax = (100 * (PurchaseIncTax / (100 + parseFloat(taxper))));

        let taxamt = parseFloat(PurchaseExcTax) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        $('#txtPurchaseExcTax' + id).val(Math.round(PurchaseExcTax * 100) / 100);

        var _extraDiscount = $('#hdnExtraDiscounts' + id).val() / parseFloat($('#txtQuantityReturned' + id).val());

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantityReturned' + id).val()) * PurchaseIncTax) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function ChangePurAmount(id) {

    if (id != undefined) {

        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];

        let PurchaseExcTax = (100 * (PurchaseIncTax / (100 + parseFloat(taxper))));
        $('#txtPurchaseExcTax' + id).val(Math.round(PurchaseExcTax * 100) / 100);

        let taxamt = parseFloat(PurchaseExcTax) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        var _extraDiscount = $('#hdnExtraDiscounts' + id).val() / parseFloat($('#txtQuantityReturned' + id).val());

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val((((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val((((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(((parseFloat($('#txtQuantityReturned' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

    }
    updateAmount();
}

function setExemption() {
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            $('#divTaxExemption' + _id).hide();

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
        }
    });
    $('.select2').select2();
}

function UpdateSalesReturnStatus(id) {
    var r = confirm("This will mark the Sales Return as Open. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');

        var det = {
            SalesReturnId: id,
            Status: "Open",
        };
        $("#divLoading").show();
        $.ajax({
            url: '/sales/UpdateSalesReturnStatus',
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
                    $("#salesStatusModal").modal('hide');

                    if (data.Data.SaleSetting.AutoPrintInvoiceBill == true) {
                        if (data.Data.SaleSetting.InvoiceType == 1) {
                            PrintInvoice('/sales/SalesReturnInvoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            PrintInvoice('/sales/SalesReturnReceipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function openRefundModal(type, CustomerPaymentId, title, BranchId) {
    _CustomerPaymentId = CustomerPaymentId;
    _BranchId = BranchId;
    var det = {
        CustomerPaymentId: CustomerPaymentId,
        Type: title,
        Title: title,
        BranchId: _BranchId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Customers/Refunds',
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
        CustomerId: $('#ddlCustomer').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtRefundDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        Type: "Customer Refund",
        ParentId: _CustomerPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Customers/RefundInsert',
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

function deleteRefund(CustomerPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            CustomerPaymentId: CustomerPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/Customers/RefundDelete',
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
                    $('#tr_' + CustomerPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function openUnpaidInvoicesModal(CustomerId, CustomerPaymentId) {
    _CustomerPaymentId = CustomerPaymentId;

    var det = {
        CustomerId: CustomerId,
        CustomerPaymentId: CustomerPaymentId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/UnpaidSalesInvoices',
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
    $('[style*="border: 2px"]').css('border', '');

    var CustomerPaymentIds = [];
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            CustomerPaymentIds.push({
                Type: $('#hdnType' + _id).val(),
                SalesId: $('#hdnSalesId' + _id).val(),
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
        ParentId: _CustomerPaymentId,
        CustomerPaymentIds: CustomerPaymentIds
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Customers/ApplyCreditsToInvoices',
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

function DeleteCustomerPayment(CustomerPaymentId) {
    var det = {
        CustomerPaymentId: CustomerPaymentId
    }
    $("#divLoading").show();
    $.ajax({
        url: '/customers/PaymentDelete',
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
            //    //$('#tr_' + CustomerPaymentId).remove();
            //    fetchList();
            //}
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deletePayment(CustomerPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            CustomerPaymentId: CustomerPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/SalesPaymentDelete',
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
                    $('#tr_' + CustomerPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function fetchItemTax(SkuHsnCode) {
    if (window.location.pathname.toLowerCase().indexOf('billofsupply') != -1) {
        IsBillOfSupply = true;
    }
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        MenuType: 'sale',
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        Type: "Sales",
        IsBillOfSupply: IsBillOfSupply
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

                            //let taxamt = (Math.round((parseFloat(data.Data.ItemDetails[i].SalesIncTax) - parseFloat(data.Data.ItemDetails[i].SalesExcTax)) * 100) / 100);
                            var ddlTax = '<select class="form-control ddlTax" style="min-width:80px" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
                            for (let ss = 0; ss < taxList.length; ss++) {
                                if (taxList[ss].Tax != "Taxable") {
                                    if (data.Data.BusinessSetting.CountryId == 2) {
                                        if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val() &&
                                            data.Data.BusinessSetting.GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && data.Data.BusinessSetting.GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                                            && data.Data.BusinessSetting.GstTreatment != 'Supply by SEZ Developer') {
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
                            ddlTax = ddlTax + '</select>';

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
                                '</div >');
                            /*'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +*/

                            ChangeQtyAmount(count);
                        }
                    }
                });
            }
            $('.select2').select2();

            if (IsBillOfSupply == true) {
                $('.divBillOfSupply').hide();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertSalesCreditNoteReason() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        SalesCreditNoteReason: $('#txtSalesCreditNoteReason_M').val(),
        Description: $('#txtDescription_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/salessettings/SalesCreditNoteReasonInsert',
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
                $('#ddlSalesCreditNoteReason').append($('<option>', { value: data.Data.SalesCreditNoteReason.SalesCreditNoteReasonId, text: data.Data.SalesCreditNoteReason.SalesCreditNoteReason }));
                $('#ddlSalesCreditNoteReason').val(data.Data.SalesCreditNoteReason.SalesCreditNoteReasonId);

                $('#salesCreditNoteReasonsModal').modal('toggle');

                $('#txtSalesCreditNoteReason_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: { Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'sales return' },
            success: function (data) {
                $('.errorText').hide();
                $('[style*="border: 2px"]').css('border', '');


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

function fetchItem(SkuHsnCode) {
    //$('#saltSearchModal').modal('toggle');
    var IsBusinessRegistered = $('#hdnIsBusinessRegistered').val().toLocaleLowerCase();
    var BusinessRegistrationType = $('#hdnBusinessRegistrationType').val().toLocaleLowerCase();

    if ((window.location.pathname.toLowerCase().indexOf('billofsupply') != -1) ||
        (IsBusinessRegistered == "1" && BusinessRegistrationType == "composition")) {
        IsBillOfSupply = true;
    }
    var det = {
        ItemCode: SkuHsnCode,//splitVal[splitVal.length - 1],
        BranchId: $('#ddlBranch').val(),
        SalesId: $('#ddlSalesInvoice').val(),
        IsBillOfSupply: IsBillOfSupply
    };
    $("#divLoading").show();
    $.ajax({
        url: '/sales/SalesReturnSearchItems',
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

function setItem(data) {

    var html = '';
    //var vari = '';
    $('#txttags').val('');
    var z = count;
    for (let i = 0; i < data.Data.ItemDetails.length; i++) {
        var isPresent = false;

        if (isPresent == false) {
            let taxamt = (Math.round((parseFloat(data.Data.ItemDetails[i].SalesIncTax) - parseFloat(data.Data.ItemDetails[i].SalesExcTax)) * 100) / 100);
            var variation = '';
            if (data.Data.ItemDetails[i].VariationName) {
                variation = '</br> <b>Variation :</b> ' + data.Data.ItemDetails[i].VariationName;
            }
            var ddlUnit = '<select disabled style="min-width:80px" class="form-control ' + (data.Data.ItemDetails[i].UnitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

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
                ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-4-1' + '">' + (data.Data.ItemDetails[i].UnitShortName == null ? "N/A" : data.Data.ItemDetails[i].UnitShortName) + '</option>';
            }

            ddlUnit = ddlUnit + '</select >';

            var ddlTax = '<select class="form-control ddlTax" style="min-width:80px" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
            for (let ss = 0; ss < taxList.length; ss++) {
                if (taxList[ss].Tax != "Taxable") {
                    if (data.Data.BusinessSetting.CountryId == 2) {
                        if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val() &&
                            data.Data.BusinessSetting.GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && data.Data.BusinessSetting.GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                            && data.Data.BusinessSetting.GstTreatment != 'Supply by SEZ Developer') {
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
            ddlTax = ddlTax + '</select>';

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

            //var ddlItemCode = '<select class="form-control select2" id="ddlItemCode' + count + '"><option selected value="0">None</option>';
            //for (let ss = 0; ss < itemCodes.length; ss++) {
            //    if (data.Data.ItemDetails[i].ItemType == 'Product') {
            //        if (itemCodes[ss].ItemCodeType == "HSN") {
            //            if (data.Data.ItemDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
            //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
            //            }
            //            else {
            //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
            //            }
            //        }
            //    }
            //    else {
            //        if (itemCodes[ss].ItemCodeType == "SAC") {
            //            if (data.Data.ItemDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
            //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
            //            }
            //            else {
            //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
            //            }
            //        }
            //    }
            //}
            //ddlItemCode = ddlItemCode + '</select >';

            var SalePriceIsMinSellingPrice = $('#hdnSalePriceIsMinSellingPrice').val().toLocaleLowerCase();
            var EnableLotNo = $('#hdnEnableLotNo').val().toLocaleLowerCase();
            var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());
            var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
            var EnableFreeQuantity = $('#hdnEnableFreeQuantity').val().toLocaleLowerCase();
            var OnItemExpiry = $('#hdnOnItemExpiry').val();
            var EnableWarranty = $('#hdnEnableWarranty').val().toLocaleLowerCase();

            //Set dropdown for lots
            if (data.Data.ItemDetails[i].LotNo != null) {
                var ddlLot = '<select disabled class="form-control" id="ddlLot' + count + '" onchange="fetchLotDetails(' + count + ')">';
                ddlLot = ddlLot + '<option selected value="' + data.Data.ItemDetails[i].LotId +'-' + data.Data.ItemDetails[i].LotType +'">' + data.Data.ItemDetails[i].LotNo +'</option></select>';
                //Set dropdown for lots
            }
            else {
                var ddlLot = '<span style="text-align:center;display:block">-</span>';
            }

            var ddlDiscountType = '<select ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" style="min-width: 100px;" id="ddlDiscountType' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
            if (data.Data.ItemDetails[i].DiscountType == "Fixed") {
                ddlDiscountType = ddlDiscountType + '<option selected value="Fixed">Fixed</option><option value="Percentage">Percentage</option>';
            }
            else {
                ddlDiscountType = ddlDiscountType + '<option value="Fixed">Fixed</option><option selected value="Percentage">Percentage</option>';
            }

            ddlDiscountType = ddlDiscountType + '</select>';

            html = html + '<tr id="divCombo' + count + '">' +
                '<td style="min-width:100px; white-space: nowrap;">' +
                '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                '<input type="text" hidden class="form-control" id="txtSkuCode' + count + '" value="' + data.Data.ItemDetails[i].SKU + '">' +
                '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                '<input type="text" hidden class="form-control" id="txtSalesDetailsId' + count + '" value="' + data.Data.ItemDetails[i].SalesDetailsId + '">' +
                '<input type="text" hidden class="form-control" id="txtSalesReturnDetailsId' + count + '" value="' + data.Data.ItemDetails[i].SalesReturnDetailsId + '">' +
                '<span class="' + (data.Data.ItemDetails[i].ItemName.length > 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName.substring(0, 15) + '...</span>' +
                '<span class="' + (data.Data.ItemDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName + '</span>' +
                variation +
                ' </br> <b>SKU :</b> ' + data.Data.ItemDetails[i].SKU + '' +
                '<textarea style="min-width:200px;display:' + (data.Data.ItemDetails[i].EnableImei == true ? 'block' : 'none') + '" type="text" class="form-control" id="txtOtherInfo' + count + '" placeholder="IMEI, Serial number or other informations"></textarea>' +
                '</td>' +
                '<td style="min-width:120px" class="' + (EnableLotNo == 'true' ? '' : 'hidden') + '"> ' + ddlLot +
                '</td>' +
                '<td> ' +
                '<div class="input-group" style="min-width:150px">' +
                '<input onKeyPress="toggleDecimal(event,' + count + ')" type="number" class="form-control divQuantity' + count + '_ctrl" value="0" id="txtQuantityReturned' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                ddlUnit +
                '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                '<input type="hidden" id="hdnUnitCost' + count + '" value="' + data.Data.ItemDetails[i].SalesExcTax + '">' +
                '<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                '<input type="text" hidden class="form-control" id="hdnIsManageStock' + count + '" value="' + data.Data.ItemDetails[i].IsManageStock + '">' +
                '</div >' +
                /*'<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small></br>' +*/
                '<small class="text-success font-weight-bold ' + (data.Data.ItemDetails[i].IsManageStock == true ? '' : 'hidden') + '" id="txtAvailableQty' + count + '">Available Qty: ' + data.Data.ItemDetails[i].QuantityRemaining + '</small>' +
                '<input type="hidden" id="hdnQuantityRemaining' + count + '" value="' + data.Data.ItemDetails[i].QuantityRemaining + '">' +
                '<input type="hidden" id="txtStockQuantity' + count + '" value="' + data.Data.ItemDetails[i].QuantityRemaining + '">' +
                '</td>' +
                '<td style="min-width:100px" class="' + (EnableFreeQuantity == 'true' ? '' : 'hidden') + '"> ' +
                '<div class="input-group">' +
                '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="0" id="txtFreeQuantityReturned' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                '</div >' +
                '</td>' +
                '<td style="min-width:120px">' +
                '<input type="number"' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '"  value="' + data.Data.ItemDetails[i].SalesExcTax / ExchangeRate + '" onchange="ChangeQtyAmount(' + count + ')">' +
                '<small class="text-red font-weight-bold errorText" id="divUnitCost' + count + '"></small>' +
                '</td>' +
                '<td>' +
                '<div class="input-group" style="min-width:150px">' +
                '<input ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeQtyAmount(' + count + ')" value="' + data.Data.ItemDetails[i].Discount / ExchangeRate + '">' +
                ddlDiscountType+
                '</div >' +
                '<input type="text" hidden class="form-control" id="hdnExtraDiscounts' + count + '" value="0">' +
                '</td>' +
                '<td style="min-width:100px;display:none;" class="divBillOfSupply">' +
                '<input onKeyPress="onlyNumberKey(event)" type="number" ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" value="' + data.Data.ItemDetails[i].SalesExcTax + '" id="txtPurchaseExcTax' + count + '"  onchange="ChangePriceBefTax(' + count + ')" min="0"/>' +
                '</td>' +
                '<td style="min-width:100px;display:none" class="divBillOfSupply">' +
                '<input type="number" class="form-control" disabled value="' + data.Data.ItemDetails[i].SalesExcTax + '" id="txtAmountExcTax' + count + '" min="0"/>' +
                '</td>' +
                '<td class="divBillOfSupply" id="tdTax' + count + '">' +
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
                '</div >' +
                /*'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +*/
                '</td>' +
                //'<td style="min-width:150px">' +
                //'<input type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '">' +
                //'<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                //'</td>' +
                '<td style="min-width:100px;display:none">' +
                '<input type="number" class="form-control" id="txtPurchaseIncTax' + count + '"  value="' + data.Data.ItemDetails[i].SalesIncTax / ExchangeRate + '" onchange="updateNetCost(' + count + ')">' +
                '<input type="hidden" value="' + data.Data.ItemDetails[i].SalesIncTax + '" id="hdnPurchaseIncTax' + count + '" />' +
                '</td>' +
                '<td style="min-width:120px">' +
                '<input type="number" disabled class="form-control"  id="txtAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax / ExchangeRate + '" >' +
                '<input type="hidden" id="hdnAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '" >' +
                '</td>' +
                //'<td style="min-width:120px;display:' + (EnableWarranty == 'true' ? 'block' : 'none') + '">' +
                //'<div class="input-group">' +
                //ddlWarranty +
                //'</div >' +
                //'</td>' +
                '<td>' +
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
    $('.select2').select2();

    convertAvailableStock();
    updateAmount();

    //if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
    //    convertAvailableStock();
    //}

    //for (var g = 1; g <= (count - z); g++) {
    //    if (EnableLotNo == "true") {
    //        fetchLotDetails(z);
    //    }
    //    if (data.Data.UserGroup.SellingPriceGroupId != 0 || data.Data.UserGroup.CalculationPercentage != 0) {
    //        updateNetCost(z);
    //    }
    //}

    if (IsBillOfSupply == true) {
        $('.divBillOfSupply').hide();
    }

    //updateExtraDiscounts();
}

function toggleDecimal(evt, i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    var isDecimal = "false";
    if (index == 1) {
        isDecimal = $('#hdnAllowDecimal' + i).val();
    }
    else if (index == 2) {
        isDecimal = $('#hdnSecondaryUnitAllowDecimal' + i).val();
    }
    else if (index == 3) {
        isDecimal = $('#hdnTertiaryUnitAllowDecimal' + i).val();
    }
    else if (index == 4) {
        isDecimal = $('#hdnQuaternaryUnitAllowDecimal' + i).val();
    }
    if (isDecimal.toLowerCase() == "false") {
        // Only ASCII character in that range allowed
        var ASCIICode = (evt.which) ? evt.which : evt.keyCode
        // if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        if (ASCIICode == 46)
            return false;
        return true;
    }

    return true;
}

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

function togglePayTaxForExport() {
    if ($('#chkPayTaxForExport').is(':checked') == false) {
        $('.divTaxCollectedFromCustomer').hide();
        $('#chkTaxCollectedFromCustomer').prop('checked', false);
    }
    else {
        $('.divTaxCollectedFromCustomer').show();
    }
}

function fetchAdditionalCharges() {
    var IsBusinessRegistered = $('#hdnIsBusinessRegistered').val().toLocaleLowerCase();
    var BusinessRegistrationType = $('#hdnBusinessRegistrationType').val().toLocaleLowerCase();

    if ((window.location.pathname.toLowerCase().indexOf('billofsupply') != -1) ||
        (IsBusinessRegistered == "1" && BusinessRegistrationType == "composition")) {
        IsBillOfSupply = true;
    }
    
    // Store existing values before clearing
    var existingValues = {};
    var hasExistingValues = false;
    
    // Check if there are any existing values in additional charges
    $('[id^="txtAdditionalChargesAmountExcTax"]').each(function() {
        var index = $(this).attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        var amount = $(this).val();
        var incTax = $('#txtAdditionalChargesAmountIncTax' + index).val();
        var chargeId = $('#txtAdditionalChargeId' + index).val();
        
        if (amount && parseFloat(amount) > 0) {
            hasExistingValues = true;
            existingValues[index] = {
                amount: amount,
                incTax: incTax,
                chargeId: chargeId
            };
        }
    });
    
    var det = {
        CustomerId: $('#ddlCustomer').val(),
        BranchId: $('#ddlBranch').val(),
        IsBillOfSupply: IsBillOfSupply
    };
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
                    if (taxList[ss].Tax != "Taxable") {
                        if (CountryId == 2) {
                            if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val() &&
                                data.Data.AdditionalCharges.GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && data.Data.AdditionalCharges.GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                                && data.Data.AdditionalCharges.GstTreatment != 'Supply by SEZ Developer') {
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
                ddlTax = ddlTax + '</select>';

                // Exemption Reason label + edit icon (no inline dropdown)
                var selectedExemptionText = 'None';
                var selectedExemptionId = 0;
                if (data.Data.AdditionalCharges[i].TaxExemptionId && data.Data.AdditionalCharges[i].TaxExemptionId != 0) {
                    var found = taxExemptions.find(function (ex) { return ex.TaxExemptionId == data.Data.AdditionalCharges[i].TaxExemptionId; });
                    if (found) selectedExemptionText = found.Reason;
                    selectedExemptionId = data.Data.AdditionalCharges[i].TaxExemptionId;
                }
                
                var exemptionDiv = '<div class="divBillOfSupply" id="divTaxExemptionAC' + count + '" style="display:none; margin-top:5px; align-items:center;">' +
                    '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
                    '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
                    '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
                    '<i class="fas fa-edit"></i>' +
                    '</a>' +
                    '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
                    '</div>';

                // Set amount value - preserve existing or use default
                var amountValue = existingValue ? existingValue.amount : '0';
                var incTaxValue = existingValue ? existingValue.incTax : '0';

                html = html +
                    '<div class="row additional-charges-row">' +
                    '<div class="col-sm-6">' +
                    '<div class="form-group row">' +
                    '<label for="inputEmail3" class="col-sm-5 col-form-label">' + data.Data.AdditionalCharges[i].Name + '</label>' +
                    '<div class="input-group col-sm-7">' +
                    '<input type="number" min="0" class="form-control" id="txtAdditionalChargesAmountExcTax' + count + '" placeholder="" onchange="updateAdditionalChargesCalculation(' + count + ')" value="' + amountValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargesAmountIncTax' + count + '" value="' + incTaxValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargeId' + count + '" value="' + data.Data.AdditionalCharges[i].AdditionalChargeId + '">' +
                    '<div class="divBillOfSupply">' +
                    ddlTax +
                    '</div>' +
                    exemptionDiv +
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

            if (IsBillOfSupply == true) {
                $('.divBillOfSupply').hide();
            }
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
    var ddl = document.getElementById('ddlAdditionalChargesTax' + count);
    var exemptionDiv = document.getElementById('divTaxExemptionAC' + count);
    if (!ddl || !exemptionDiv) return;

    // Find the selected option's text
    var selectedText = ddl.options[ddl.selectedIndex].text;

    // Show the exemption div if "Non Taxable" is selected
    if (selectedText === "Non-Taxable") {
        exemptionDiv.style.display = "flex"; // or "block" if you prefer
    } else {
        exemptionDiv.style.display = "none";
        $('#exemptionReasonId' + count).val(0);
    }

    updateAdditionalChargesCalculation(count);
}

function updateAdditionalChargesCalculation(count) {
    var amount = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
    var tax = $('#ddlAdditionalChargesTax' + count).val().split('-')[1];
    var incTax = ((tax / 100) * amount) + amount;
    $('#txtAdditionalChargesAmountIncTax' + count).val(incTax);
    discallcalc();
}

function init() {
    $('#singleproduct').show();
    $('#variableproduct').hide();
    $('#comboproduct').hide();
}