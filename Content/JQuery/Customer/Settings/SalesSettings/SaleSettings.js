
$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $('.select2').select2();

    // Initialize Summernote for Default Notes and Terms if they are visible
    initializeDefaultNotesTermsEditors();

    // Show/hide Default Notes field based on EnableNotes checkbox
    $('#chkSalesEnableNotes').on('change', function () {
        if ($(this).is(':checked')) {
            $('.divDefaultNotes').show();
            // Initialize summernote if not already initialized
            if (!$('#txtDefaultNotes').next('.note-editor').length) {
                initializeDefaultNotesEditor();
            }
        } else {
            $('.divDefaultNotes').hide();
        }
        // Show/hide the entire section based on whether any field is visible
        toggleDefaultNotesTermsSection();
    });

    // Show/hide Default Terms field based on EnableTerms checkbox
    $('#chkSalesEnableTerms').on('change', function () {
        if ($(this).is(':checked')) {
            $('.divDefaultTerms').show();
            // Initialize summernote if not already initialized
            if (!$('#txtDefaultTerms').next('.note-editor').length) {
                initializeDefaultTermsEditor();
            }
        } else {
            $('.divDefaultTerms').hide();
        }
        // Show/hide the entire section based on whether any field is visible
        toggleDefaultNotesTermsSection();
    });

    // Function to show/hide the entire Default Notes & Terms section
    function toggleDefaultNotesTermsSection() {
        if ($('#chkSalesEnableNotes').is(':checked') || $('#chkSalesEnableTerms').is(':checked')) {
            $('.divDefaultNotesTermsSection').show();
        } else {
            $('.divDefaultNotesTermsSection').hide();
        }
    }

    // Initialize Summernote editors for Default Notes and Terms
    function initializeDefaultNotesTermsEditors() {
        if ($('#chkSalesEnableNotes').is(':checked')) {
            initializeDefaultNotesEditor();
        }
        if ($('#chkSalesEnableTerms').is(':checked')) {
            initializeDefaultTermsEditor();
        }
    }

    // Initialize Summernote for Default Notes
    function initializeDefaultNotesEditor() {
        $('textarea#txtDefaultNotes').summernote({
            placeholder: '',
            tabsize: 2,
            height: 100,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['height', ['height']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'hr']],
                ['help', ['help']]
            ],
        });
    }

    // Initialize Summernote for Default Terms
    function initializeDefaultTermsEditor() {
        $('textarea#txtDefaultTerms').summernote({
            placeholder: '',
            tabsize: 2,
            height: 100,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['height', ['height']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'hr']],
                ['help', ['help']]
            ],
        });
    }
});
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
        RoundOffAccountId: $('#ddlSalesRoundOffAccount').val(),
        EnableCustomerGroup: $('#chkEnableCustomerGroup').is(':checked'),
        EnableSalesQuotation: $('#chkSalesEnableSalesQuotation').is(':checked'),
        EnableSalesOrder: $('#chkSalesEnableSalesOrder').is(':checked'),
        EnableDeliveryChallan: $('#chkSalesEnableDeliveryChallan').is(':checked'),
        EnableSalesProforma: $('#chkSalesEnableSalesProforma').is(':checked'),
        EnablePos: $('#chkSalesEnablePos').is(':checked'),
        EnableSpecialDiscount: $('#chkSalesEnableSpecialDiscount').is(':checked'),
        SpecialDiscountAccountId: $('#ddlSalesSpecialDiscountAccount').val(),
        EnableNotes: $('#chkSalesEnableNotes').is(':checked'),
        EnableTerms: $('#chkSalesEnableTerms').is(':checked'),
        EnableRecurringSales: $('#chkSalesEnableRecurringSales').is(':checked'),
        DefaultNotes: $('#txtDefaultNotes').summernote('code') || $('#txtDefaultNotes').val(),
        DefaultTerms: $('#txtDefaultTerms').summernote('code') || $('#txtDefaultTerms').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/salessettings/SaleSettingsUpdate',
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
                $('#ddlSalesPaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlSalesPaymentTerm').val(data.Data.PaymentTerm.PaymentTermId);

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