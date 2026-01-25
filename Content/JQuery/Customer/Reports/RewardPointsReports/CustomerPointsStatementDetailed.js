$(function () {
    $('.select2').select2();
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });
});

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var dateRange = $('#txtDateRange').val().split(' - ');
    var urlParams = new URLSearchParams(window.location.search);
    var customerId = urlParams.get('customerId') || 0;
    
    var det = {
        CustomerId: customerId,
        FromDate: moment(dateRange[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment(dateRange[1], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        CompanyId: Cookies.get('data').split('&')[9].split('=')[1]
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/rewardpointsreports/customerpointsstatementdetailed',
        datatype: "html",
        data: det,
        type: "post",
        success: function (data) {
            location.reload(); // Reload to show updated data
        },
        error: function (xhr) {
            $("#divLoading").hide();
            toastr.error("Error fetching detailed statement.");
        }
    });
}

// Custom PDF export function that includes summary cards
function exportToPdfDetailed(fileName) {
    $('.noPrint').hide();
    $('.hidden').show();
    $('.hide').hide();
    
    // Capture the entire card body including summary cards
    var elementToExport = document.querySelector('.card-body');
    if (!elementToExport) {
        elementToExport = document.getElementById('tblData').parentElement;
    }
    
    html2canvas(elementToExport, {
        backgroundColor: '#ffffff',
        useCORS: true,
        scale: 2,
        logging: false
    }).then(function (canvas) {
        var data = canvas.toDataURL();
        var docDefinition = {
            content: [{
                image: data,
                width: 500,
                alignment: 'center'
            }],
            pageOrientation: 'landscape',
            pageSize: 'A4'
        };

        pdfMake.createPdf(docDefinition).download(fileName.split('| ')[1] + ".pdf");
        $('.noPrint').show();
        $('.hidden').hide();
        $('.hide').show();
    }).catch(function (error) {
        console.error('Error generating PDF:', error);
        toastr.error("Error generating PDF. Please try again.");
        $('.noPrint').show();
        $('.hidden').hide();
        $('.hide').show();
    });
}

