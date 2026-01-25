$(function () {
    $('.select2').select2();
});

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        ExpiryPeriod: $('#txtExpiryPeriod').val() || 30,
        CompanyId: Cookies.get('data').split('&')[9].split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/rewardpointsreports/expiringpointsfetch',
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
            toastr.error("Error fetching expiring points.");
        }
    });
}

