function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex,
        PageSize: parseInt($("#txtPageSize").val()),
        BranchId: parseInt($("#ddlBranch").val()) || 0,
        FromDate: null,
        ToDate: null,
        Status: $("#ddlStatus").val() || ""
    };

    var dateRange = $("#txtDateRange").val();
    if (dateRange && dateRange.indexOf(" - ") > -1) {
        var dates = dateRange.split(" - ");
        if (dates.length == 2) {
            det.FromDate = moment(dates[0], "DD/MM/YYYY").toDate();
            det.ToDate = moment(dates[1], "DD/MM/YYYY").toDate();
        }
    }

    $.ajax({
        url: '/bookingreports/bookingsummaryfetch',
        type: 'POST',
        data: det,
        success: function (data) {
            $("#tblBookingSummary").html(data);

            $('#tblBookingSummary').DataTable({
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
        error: function () {
            toastr.error("Error loading data");
        }
    });
}

$(function () {
    $('#tblBookingSummary').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        opens: 'left',
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    // Read URL parameters and apply filters
    var urlParams = new URLSearchParams(window.location.search);
    var statusParam = urlParams.get('status');
    var dateParam = urlParams.get('date');
    var dateRangeParam = urlParams.get('daterange');

    // Apply status filter if provided
    if (statusParam) {
        $('#ddlStatus').val(statusParam).trigger('change');
    }

    // Apply date filter if provided
    if (dateParam === 'today') {
        var today = moment().format('DD/MM/YYYY');
        $('#txtDateRange').val(today + ' - ' + today);
    } else if (dateRangeParam === '7days') {
        // Next 7 days
        var today = moment();
        var next7Days = moment().add(7, 'days');
        $('#txtDateRange').val(today.format('DD/MM/YYYY') + ' - ' + next7Days.format('DD/MM/YYYY'));
    }

    fetchList(1);
});

