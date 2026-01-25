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
        AccountId: window.location.href.split('=').length > 1 ? window.location.href.split('=')[1].split('&')[0] : 0,
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        BranchId: $('#ddlBranch').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/accountsreports/BankTransactionsFetch',
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

function fetchAccountTransactions() {
    location.href = "/accountsreports/BankTransactions?AccountId=" + $('#ddlAccount').val() + "&name=" + $("#ddlAccount option:selected").text() + "&Type=" + window.location.href.split('&')[2].split('=')[1];
}

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