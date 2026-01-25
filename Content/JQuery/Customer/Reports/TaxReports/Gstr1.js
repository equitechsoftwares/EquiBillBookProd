$(function () {
    //$('.table').DataTable({
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
        url: '/taxreports/Gstr1Fetch',
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

function fetchGstr1ExcelView() {
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
            url: '/taxreports/Gstr1ExcelViewFetch',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#divGstr1ExcelView").html(data);
                resolve();
                //$("#divLoading").hide();
            },
            error: function (xhr) {
                //$("#divLoading").hide();
            }
        });
    });
};

function exportToExcelGstr1() {
    $("#divLoading").show();
    fetchGstr1ExcelView().then(() => {
        let wb = XLSX.utils.book_new(); // Create a new workbook

        // Convert each table into a worksheet and add it to the workbook
        let tables = [
            { id: "tblDataB2B", sheetName: "b2b,sez,de" },
            { id: "tblDataB2CL", sheetName: "b2cl" },
            { id: "tblDataB2CS", sheetName: "b2cs" },
            { id: "tblDataEXP", sheetName: "exp" },
            { id: "tblDataCDNR", sheetName: "cdnr" },
            { id: "tblDataCDNUR", sheetName: "cdnur" },
            { id: "tblDataAT", sheetName: "at" },
            { id: "tblDataATADJ", sheetName: "atadj" },
            { id: "tblDataHSN", sheetName: "hsn" },
            { id: "tblDataEXEMP", sheetName: "exemp" },
            { id: "tblDataDOCS", sheetName: "docs" },
        ];

        tables.forEach(({ id, sheetName }) => {
            let table = document.getElementById(id);
            if (table) {
                let ws = XLSX.utils.table_to_sheet(table); // Convert table to sheet
                XLSX.utils.book_append_sheet(wb, ws, sheetName); // Add to workbook
            }
        });

        // Save the file
        XLSX.writeFile(wb, "GSTR1.xlsx");
    }).catch(error => {
        console.error("Error fetching data:", error);
        $("#divLoading").hide();
    }).finally(() => {
        $("#divLoading").hide();
    });
}