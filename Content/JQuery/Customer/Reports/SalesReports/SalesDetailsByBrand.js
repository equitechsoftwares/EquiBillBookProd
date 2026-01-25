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
        ItemId: window.location.href.split('&')[1].split('=')[1],
        FromDate: moment(window.location.href.split('&')[3].split('=')[1]).format('DD-MM-YYYY'),
        ToDate: moment(window.location.href.split('&')[4].split('=')[1]).format('DD-MM-YYYY'),
        BranchId: window.location.href.split('&')[2].split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/salesreports/SalesDetailsByBrandFetch',
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