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
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('.select2').select2();

    //Date range picker
    $('#txtDate').daterangepicker({
        locale: {
            format: 'DD-MM-YYYY'
        }
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var _PageIndex = 1;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/purchasepurchasereports/PurchaseReturnDetailsBySupplierFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            //$('.chkIsActive').bootstrapToggle();
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
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewCustomer(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/CustomerView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#CustomerViewModal').modal('toggle');
            $("#divCustomerView").html(data);
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

function ViewSupplier(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/SupplierView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SupplierViewModal').modal('toggle');
            $("#divSupplierView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

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

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}