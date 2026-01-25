$(function () {
    //$('#tblData').DataTable({
    //    lengthChange: false,
    //    searching: false,
    //    autoWidth: false,
    //    responsive: true,
    //    paging: false,
    //    bInfo: false
    //});

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
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        BranchId: $('#ddlBranch').val(),
        TaxSettingId: $('#ddlTaxSetting').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/taxreports/Gstr4Fetch',
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

function fetchGstr4ExcelView() {
    return new Promise((resolve, reject) => {
        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
        var det = {
            FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
            ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
            BranchId: $('#ddlBranch').val(),
            TaxSettingId: $('#ddlTaxSetting').val(),
        };
        //$("#divLoading").show();
        $.ajax({
            url: '/taxreports/Gstr4ExcelViewFetch',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#divGstr4ExcelView").html(data);
                resolve();
                //$("#divLoading").hide();
            },
            error: function (xhr) {
                //$("#divLoading").hide();
            }
        });
    });
};

function exportToExcelGstr4() {
    $("#divLoading").show();
    fetchGstr4ExcelView().then(() => {
        let wb = XLSX.utils.book_new(); // Create a new workbook

        // Convert each table into a worksheet and add it to the workbook
        let tables = [
            { id: "tblData4A", sheetName: "4A. B2B" },
            { id: "tblData4B", sheetName: "4B. B2BRC" },
            { id: "tblData4C", sheetName: "4C. URP" },
            { id: "tblData4D", sheetName: "4D. IMPS" },
            { id: "tblData6", sheetName: "6. Inward outward supplies" }
        ];

        tables.forEach(({ id, sheetName }) => {
            let table = document.getElementById(id);
            if (table) {
                let ws = XLSX.utils.table_to_sheet(table); // Convert table to sheet
                XLSX.utils.book_append_sheet(wb, ws, sheetName); // Add to workbook
            }
        });

        // Save the file
        XLSX.writeFile(wb, "GSTR4.xlsx");
    }).catch(error => {
        console.error("Error fetching data:", error);
        $("#divLoading").hide();
    }).finally(() => {
        $("#divLoading").hide();
    });
}

