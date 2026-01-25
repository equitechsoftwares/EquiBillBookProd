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
        url: '/bookingreports/customervisitfrequencyreportfetch',
        type: 'POST',
        data: det,
        success: function (data) {
            $("#tblCustomerVisitFrequencyReport").html(data);

            $('#tblCustomerVisitFrequencyReport').DataTable({
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
    $('#tblCustomerVisitFrequencyReport').DataTable({
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

    fetchList();
});

