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
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        CustomerId: $('#ddlCustomer').val() || 0,
        CompanyId: Cookies.get('data').split('&')[9].split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/rewardpointsreports/customerpointsstatementfetch',
        datatype: "html",
        data: det,
        type: "post",
        success: function (data) {
            // Destroy existing DataTable instance if it exists
            if ($.fn.DataTable.isDataTable('#tblData')) {
                $('#tblData').DataTable().destroy();
            }

            $("#tblData").html(data);
            $("#divLoading").hide();

            // Ensure thead is in the correct position before DataTable initialization
            $("#thead").insertBefore(".table-body");

            // Initialize DataTable with column definitions to preserve button styling
            $('#tblData').DataTable({
                lengthChange: false,
                searching: false,
                autoWidth: false,
                responsive: false,
                paging: false,
                bInfo: false,
                columnDefs: [
                    {
                        targets: -1, // Last column (Action column)
                        orderable: false,
                        className: 'text-center',
                        width: 'auto',
                        autoWidth: false
                    }
                ],
                "bDestroy": true
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
            toastr.error("Error fetching customer points statement.");
        }
    });
}

