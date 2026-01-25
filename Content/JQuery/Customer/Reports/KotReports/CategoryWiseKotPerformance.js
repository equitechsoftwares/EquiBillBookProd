function fetchList() {
    var det = {
        BranchId: parseInt($("#ddlBranch").val()) || 0,
        FromDate: null,
        ToDate: null
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
        url: '/kotreports/categorywisekotperformancefetch',
        type: 'POST',
        data: det,
        success: function (data) {
            $("#tblCategoryWiseKotPerformance").html(data);

            $('#tblCategoryWiseKotPerformance').DataTable({
                lengthChange: false,
                searching: false,
                autoWidth: false,
                responsive: false,
                paging: false,
                bInfo: false,
                "bDestroy": true,
                "language": {
                    "emptyTable": "No category performance data found"
                }
            });
            $("#thead").insertBefore(".table-body");
        },
        error: function () {
            toastr.error("Error loading data");
        }
    });
}

$(function () {
    $('#tblCategoryWiseKotPerformance').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false,
        "language": {
            "emptyTable": "No category performance data found"
        }
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        opens: 'left',
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    fetchList();
});

