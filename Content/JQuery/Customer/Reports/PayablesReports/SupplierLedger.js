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

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: window.location.href.split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/payablesreports/SupplierLedgerFetch',
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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openLedgerDiscountModal(type) {
    var det = {
        Id: window.location.href.split('=')[1],
        Type: "Supplier Ledger Discount",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/reports/LedgerDiscounts',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLedgerDiscounts").html(data);

            $("#ledgerDiscountModal").modal('show');
            if (type == true) {
                $('.paymentAdd').show();
                $('.paymentList').hide();
                $('#ledgerDiscountModalLabel').text('Add Discount');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');
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
                $('#ledgerDiscountModalLabel').text('View Ledger Discounts');
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertLedgerDiscount() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        Id: window.location.href.split('=')[1],
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        Type: "Supplier Ledger Discount",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/reports/LedgerDiscountInsert',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#ledgerDiscountModal").modal('hide');

                $('#txtPaymentNotes').val('');
                $('#txtAmount').val(0);
                $('#txtPaymentDate').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deleteLedgerDiscount(LedgerDiscountId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            LedgerDiscountId: LedgerDiscountId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/reports/LedgerDiscountDelete',
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
                    $('#tr_' + LedgerDiscountId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}