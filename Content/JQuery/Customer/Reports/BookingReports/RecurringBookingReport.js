function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex,
        PageSize: parseInt($("#txtPageSize").val()),
        Search: $("#txtSearch").val() || "",
        SearchText: $("#txtSearch").val() || ""
    };

    var recurrenceType = $("#ddlRecurrenceType").val();
    var status = $("#ddlStatus").val();

    $.ajax({
        url: '/bookingreports/recurringbookingreportfetch',
        type: 'POST',
        data: det,
        success: function (data) {
            $("#tblRecurringBookingReport").html(data);

            $('#tblRecurringBookingReport').DataTable({
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
    $('#tblRecurringBookingReport').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });
});

