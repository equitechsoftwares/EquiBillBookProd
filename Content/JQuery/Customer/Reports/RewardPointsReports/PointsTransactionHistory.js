$(function () {
    $('.select2').select2();
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });
});

var _PageIndex = 1;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var dateRange = $('#txtDateRange').val().split(' - ');
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        CustomerId: $('#ddlCustomer').val() || 0,
        TransactionType: $('#ddlTransactionType').val() || '',
        FromDate: moment(dateRange[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment(dateRange[1], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        CompanyId: Cookies.get('data').split('&')[9].split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/rewardpointsreports/pointstransactionhistoryfetch',
        datatype: "html",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
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
            toastr.error("Error fetching transaction history.");
        }
    });
}

