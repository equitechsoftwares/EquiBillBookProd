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

    //Date range picker
    $('#txtDateRange').daterangepicker({
        locale: {
            format: 'DD-MM-YYYY'
        }
    });

    $('.select2').select2();
});

var _PageIndex = 1;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        SupplierId: window.location.href.split('=')[1].split('&')[0],
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        BranchId: $('#ddlBranch').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/accountsreports/SundryCreditorDetailsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            //$('.chkIsActive').bootstrapToggle();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewExpense(ExpenseId) {
    var det = {
        ExpenseId: ExpenseId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/ExpenseView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ExpenseViewModal').modal('toggle');
            $("#divExpenseView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewJournal(JournalId) {
    var det = {
        JournalId: JournalId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/JournalView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#JournalViewModal').modal('toggle');
            $("#divJournalView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewContra(ContraId) {
    var det = {
        ContraId: ContraId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ContraViewModal').modal('toggle');
            $("#divContraView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewSale(SalesId) {
    var det = {
        SalesId: SalesId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SaleViewModal').modal('toggle');
            $("#divSaleView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewSalesReturn(SalesReturnId) {
    var det = {
        SalesReturnId: SalesReturnId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SalesReturnViewModal').modal('toggle');
            $("#divSalesReturnView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPurchase(PurchaseId) {
    var det = {
        PurchaseId: PurchaseId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PurchaseViewModal').modal('toggle');
            $("#divPurchaseView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPurchaseReturn(PurchaseReturnId) {
    var det = {
        PurchaseReturnId: PurchaseReturnId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseReturnView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PurchaseReturnViewModal').modal('toggle');
            $("#divPurchaseReturnView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewSalesPayment(CustomerPaymentId) {
    var det = {
        CustomerPaymentId: CustomerPaymentId,
        Type: "Sales Payment",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/PaymentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SalesPaymentViewModal').modal('toggle');
            $("#divSalesPaymentView").html(data);
            $("#divLoading").hide();

            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewSalesReturnPayment(CustomerRefundId) {
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
            $('#SalesReturnPaymentViewModal').modal('toggle');
            $("#divSalesReturnPaymentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPurchasePayment(SupplierPaymentId) {
    var det = {
        SupplierPaymentId: SupplierPaymentId,
        Type: "Purchase Payment",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/PaymentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PurchasePaymentViewModal').modal('toggle');
            $("#divPurchasePaymentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPurchaseReturnPayment(SupplierRefundId) {
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
            $('#PurchaseReturnPaymentViewModal').modal('toggle');
            $("#divPurchaseReturnPaymentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewStockAdjustment(StockAdjustmentId) {
    var det = {
        StockAdjustmentId: StockAdjustmentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockAdjust/StockAdjustmentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#StockAdjustmentViewModal').modal('toggle');
            $("#divStockAdjustmentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};