$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    //if (window.location.href.indexOf('BranchId') > -1) {
    //    var _branchid = window.location.href.split('=')[1];
    //    $('#ddlBranch').val(_branchid);
    //}

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
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

var _PageIndex = 1,_TaxType='input';

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/reports/TaxFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartial").html(data);
            $("#divLoading").hide();

            $('.tblData').DataTable({
                lengthChange: false,
                searching: false,
                autoWidth: false,
                responsive: false,
                paging: false,
                bInfo: false,
                "bDestroy": true
            });
            //$(".thead").insertBefore(".table-body");

            //$('.tab-pane').removeClass('show');
            //$('.tab-pane').removeClass('active');
            //$('#' + _TaxType).addClass('show');
            //$('#' + _TaxType).addClass('active');

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function UsersBranchWise() {
    var det = {
        BranchId: $('#ddlBranch').val()
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/reports/UsersBranchWise',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>Supplier/Customer </label><select class="form-control select2" id="ddlUser"> <option selected="selected" value="">All</option>';
            $.each(data.Data.Users, function (index, value) {
                if (value.UserType.toLowerCase() != "user") {
                    dropdown = dropdown + '<option value="' + value.UserId + '">' + value.Name + ' - ' + value.MobileNo + '</option>';
                }
            });

            dropdown = dropdown + '</select>';
            $('#divUser').html('');
            $('#divUser').append(dropdown);
            $('.select2').select2();
            
        },
        error: function (xhr) {
            
        }
    });
};

function setTaxType(t) {
    $('#input-tax').hide();
    $('#output-tax').hide();
    $('#expense-tax').hide();
    $('#income-tax').hide();

    _TaxType = t;
    $('#'+t).show();
}

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tbl-"+_TaxType), {
        name: "Tax Report.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tbl-" + _TaxType), {
        name: "Tax Report.csv",
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
    html2canvas($("#tbl-" + _TaxType)[0], {
        onrendered: function (canvas) {
            var data = canvas.toDataURL();
            var docDefinition = {
                content: [{
                    image: data,
                    width: 500
                }]
            };

            pdfMake.createPdf(docDefinition).download("Tax Report.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function printDivReport(pageName) {
    var divToPrint = document.getElementById('tbl-' + _TaxType);
    var htmlToPrint = '' +
        '<style type="text/css">' +
        'table {' +
        'border-collapse: collapse;' +
        'width: 100 %;' +
        '}' +
        'th, td {' +
        'text-align: left;' +
        'padding: 8px;' +
        '}' +
        '.even{ background-color: #f2f2f2 }' +
        'th {' +
        'background-color: #007bff;' +
        'color: white;' +
        '}' +
        '.hide { display:none}' +
        '.noPrint { display:none}' +
        '</style>';
    htmlToPrint += divToPrint.outerHTML;
    newWin = window.open("");
    newWin.document.write("<h3 align='center'>" + pageName + "</h3>");
    newWin.document.write(htmlToPrint);
    newWin.print();
    newWin.close();
}

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

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}