$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_ShippingBillDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });

    $('#_ShippingBillDate').addClass('notranslate');

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
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
}

var _PageIndex = 1; var _id = '', c = 1, taxModalId;
var AttachDocument = "";
var FileExtensionAttachDocument = "";
var count = 1; var innerCount = 1; var dropdownHtml = '';

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var ItemDetails = [];
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            ItemDetails.push({
                DivId: _id,
                SalesDetailsId: $('#txtSalesDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                AssessableValue: $('#txtAssessableValue' + _id).val(),
                CustomDuty: $('#txtCustomDuty' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),                
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                TaxAmount: $('#hdnTaxAmount' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                IsActive: true,
                IsDeleted: false
            })
        }
    });

    var det = {
        SalesId: $('#hdnSalesId').val(),
        CustomerId: $('#hdnCustomerId').val(),
        BranchId: $('#hdnBranchId').val(),
        ShippingBillNo: $('#txtShippingBillNo').val(),
        PortCode: $('#txtPortCode').val(),
        ShippingBillDate: moment($("#txtShippingBillDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        PaidThrough: $('#ddlPaidThrough').val(),
        //ReferenceNo: $("#txtReferenceNo").val(),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        Subtotal: $('#hdnSubtotal').val(),
        TotalCustomDuty: $('#hdnTotalCustomDuty').val(),
        TotalTaxAmount: $('#hdnTotalTaxAmount').val(),
        GrandTotal: $('#hdnGrandTotal').val(),
        TotalAmountPaid: $('#hdnTotalAmountPaid').val(),
        IsActive: true,
        IsDeleted: false,
        ShippingBillDetails: ItemDetails,
        ExportWithLut: $('#chkExportWithLut').is(':checked') == true ? 1 : 2,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/ShippingBillInsert',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                window.location.href = "/sales/index";
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
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');


    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            ItemDetails.push({
                ShippingBillDetailsId: $('#txtShippingBillDetailsId' + _id).val(),
                DivId: _id,
                SalesDetailsId: $('#txtSalesDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                AssessableValue: $('#txtAssessableValue' + _id).val(),
                CustomDuty: $('#txtCustomDuty' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                TaxAmount: $('#hdnTaxAmount' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                IsActive: true,
                IsDeleted: false
            })
        }
    });

    var det = {
        ShippingBillId: window.location.href.split('=')[1],
        SalesId: $('#hdnSalesId').val(),
        CustomerId: $('#hdnCustomerId').val(),
        ShippingBillNo: $('#txtShippingBillNo').val(),
        PortCode: $('#txtPortCode').val(),
        ShippingBillDate: moment($("#txtShippingBillDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        PaidThrough: $('#ddlPaidThrough').val(),
        //ReferenceNo: $("#txtReferenceNo").val(),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        Subtotal: $('#hdnSubtotal').val(),
        TotalCustomDuty: $('#hdnTotalCustomDuty').val(),
        TotalTaxAmount: $('#hdnTotalTaxAmount').val(),
        GrandTotal: $('#hdnGrandTotal').val(),
        TotalAmountPaid: $('#hdnTotalAmountPaid').val(),
        IsActive: true,
        IsDeleted: false,
        ShippingBillDetails: ItemDetails,
        ExportWithLut: $('#chkExportWithLut').is(':checked') == true ? 1 : 2,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/ShippingBillUpdate',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                window.location.href = "/sales/index";
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

function calculate() {
    var subTotal = 0,totalCustomDuty =0,totalTax = 0, grandTotal =0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAssessableValue' + _id).val() != '') {
            var AssessableValue = parseFloat($('#txtAssessableValue' + _id).val());
            var CustomDuty = parseFloat($('#txtCustomDuty' + _id).val());
            totalCustomDuty = totalCustomDuty + CustomDuty;
            $('#txtAmountExcTax' + _id).val(AssessableValue + CustomDuty);

            var TaxPercent = parseFloat($('#ddlTax' + _id).val().split('-')[1]);
            var Tax = ((TaxPercent / 100) * (AssessableValue + CustomDuty));

            //if (TaxPercent > 0) {
                $('#lblTaxAmount' + _id).text('Tax Amount: ' + Tax);
                $('#hdnTaxAmount' + _id).val(Tax);

                totalTax = totalTax + Tax;
            //}

            $('#txtAmountIncTax' + _id).val(AssessableValue + CustomDuty + Tax);

            subTotal = subTotal + (AssessableValue + CustomDuty);
            grandTotal = grandTotal + (AssessableValue + CustomDuty + Tax);
        }
    });

    $('#divTotalCustomDuty').text(CurrencySymbol + totalCustomDuty.toFixed(2));
    $('#hdnTotalCustomDuty').val(totalCustomDuty.toFixed(2));

    $('#divTotalTaxAmount').text(CurrencySymbol + totalTax.toFixed(2));
    $('#hdnTotalTaxAmount').val(totalTax.toFixed(2));

    $('#divTotalAmountPaid').text(CurrencySymbol + (totalCustomDuty+totalTax).toFixed(2));
    $('#hdnTotalAmountPaid').val((totalCustomDuty + totalTax).toFixed(2));

    $('#hdnSubtotal').val(CurrencySymbol + subTotal.toFixed(2));
    $('#hdnGrandTotal').val(CurrencySymbol + subTotal.toFixed(2));

    //$('#divAmountToCredit').text(CurrencySymbol + subTotal.toFixed(2));
    //$('#divRemainingCredits').text(CurrencySymbol + (Balance - subTotal.toFixed(2)));

}