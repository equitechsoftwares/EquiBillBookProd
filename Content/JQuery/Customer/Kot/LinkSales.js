function searchSales() {
    var det = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt(Cookies.get('data').split('&')[4].split('=')[1]),
        InvoiceNo: $('#txtSalesNo').val() || null,
        FromDate: $('#txtFromDate').val() ? new Date($('#txtFromDate').val()) : null,
        ToDate: $('#txtToDate').val() ? new Date($('#txtToDate').val()) : null,
        PageIndex: 1,
        PageSize: 50
    };

    $("#divLoading").show();
    $.ajax({
        url: '/kot/searchsalesforlinking',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data.Sales && data.Data.Sales.length > 0) {
                var html = '<div class="table-responsive"><table class="table table-bordered table-striped">';
                html += '<thead><tr><th>Invoice No</th><th>Customer</th><th>Date</th><th>Amount</th><th>Status</th><th>Action</th></tr></thead>';
                html += '<tbody>';
                $.each(data.Data.Sales, function (index, sale) {
                    // Format date - prefer SalesDate over AddedOn, and handle invalid dates
                    // Handle ASP.NET JSON date format: /Date(timestamp)/
                    var dateStr = '-';
                    var dateToFormat = sale.SalesDate || sale.AddedOn;
                    
                    if (dateToFormat) {
                        var dateObj = null;
                        
                        // Check if it's ASP.NET JSON date format: /Date(timestamp)/
                        if (typeof dateToFormat === 'string' && dateToFormat.indexOf('/Date(') === 0) {
                            // Extract timestamp from /Date(timestamp)/
                            var timestamp = dateToFormat.match(/\d+/);
                            if (timestamp && timestamp[0]) {
                                dateObj = new Date(parseInt(timestamp[0]));
                            }
                        } else {
                            // Try standard date parsing
                            dateObj = new Date(dateToFormat);
                        }
                        
                        // Check if date is valid and not the minimum date
                        if (dateObj && !isNaN(dateObj.getTime()) && dateObj.getFullYear() > 1900) {
                            dateStr = dateObj.toLocaleDateString();
                        }
                    }
                    
                    // Use GrandTotal for TotalAmount if TotalAmount doesn't exist
                    var totalAmount = sale.TotalAmount || sale.GrandTotal || 0;
                    
                    html += '<tr>';
                    html += '<td>' + (sale.InvoiceNo || '-') + '</td>';
                    html += '<td>' + (sale.CustomerName || '-') + '</td>';
                    html += '<td>' + dateStr + '</td>';
                    html += '<td>' + parseFloat(totalAmount).toFixed(2) + '</td>';
                    html += '<td><span class="badge badge-' + (sale.Status == 'Paid' ? 'success' : sale.Status == 'Pending' ? 'warning' : 'secondary') + '">' + (sale.Status || '-') + '</span></td>';
                    html += '<td><a href="javascript:void(0)" onclick="linkSales(' + sale.SalesId + ')" class="btn btn-sm btn-success"><i class="fas fa-link"></i> Link</a></td>';
                    html += '</tr>';
                });
                html += '</tbody></table></div>';
                $('#divSalesList').html(html);
            } else {
                $('#divSalesList').html('<div class="alert alert-warning"><i class="fas fa-exclamation-triangle"></i> No sales invoices found.</div>');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred while searching sales');
        }
    });
}

function linkSales(salesId) {
    if (!confirm('Are you sure you want to link this sales invoice to the KOT?')) {
        return;
    }

    // kotId is defined as a JavaScript variable in the view
    if (typeof kotId === 'undefined' || !kotId || kotId == 0) {
        toastr.error("KOT ID is missing.");
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/kot/linktosalesaction',
        datatype: "json",
        data: {
            kotId: kotId,
            salesId: salesId
        },
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                setTimeout(function () {
                    window.location.href = '/kot/details/' + kotId;
                }, 1000);
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred while linking sales');
        }
    });
}

function clearSearch() {
    $('#txtSalesNo').val('');
    $('#txtFromDate').val('');
    $('#txtToDate').val('');
    $('#divSalesList').html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> Use the search form above to find sales invoices to link.</div>');
}

