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
        CustomerId: window.location.href.split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/reports/CustomerPaymentsFetch',
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

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Customer Payments.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Customer Payments.csv",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToPdf() {
    $('.hidden').show();
    $('.hide').hide();
    $('.responsive-table').css('height', '100%');
    html2canvas($('#tblData')[0], {
        onrendered: function (canvas) {
            var data = canvas.toDataURL();
            var docDefinition = {
                content: [{
                    image: data,
                    width: 500
                }]
            };

            pdfMake.createPdf(docDefinition).download("Customer Payments.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

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

function ViewIncome(IncomeId) {
    var det = {
        IncomeId: IncomeId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/income/IncomeView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#IncomeViewModal').modal('toggle');
            $("#divIncomeView").html(data);
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
        url: '/sales/SalesView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SaleViewModal').modal('toggle');
            $("#divSaleView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewPayment(PaymentId) {
    var det = {
        CustomerPaymentId: PaymentId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/sales/PaymentView',
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

function ViewSaleReturn(SalesId) {
    var det = {
        SalesId: SalesId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesReturnView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SaleReturnViewModal').modal('toggle');
            $("#divSaleReturnView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};