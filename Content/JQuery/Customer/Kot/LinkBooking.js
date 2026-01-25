function searchBookings() {
    var det = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt(Cookies.get('data').split('&')[4].split('=')[1]),
        Search: $('#txtBookingNo').val() || null,
        FromDate: $('#txtFromDate').val() ? new Date($('#txtFromDate').val()) : null,
        ToDate: $('#txtToDate').val() ? new Date($('#txtToDate').val()) : null,
        PageIndex: 1,
        PageSize: 50
    };

    $("#divLoading").show();
    $.ajax({
        url: '/kot/searchbookingsforlinking',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data.Bookings && data.Data.Bookings.length > 0) {
                var html = '<div class="table-responsive"><table class="table table-bordered table-striped">';
                html += '<thead><tr><th>Booking No</th><th>Table</th><th>Customer</th><th>Date</th><th>Time</th><th>Status</th><th>Action</th></tr></thead>';
                html += '<tbody>';
                $.each(data.Data.Bookings, function (index, booking) {
                    html += '<tr>';
                    html += '<td>' + (booking.BookingNo || '-') + '</td>';
                    html += '<td>' + (booking.TableNo || 'Not Assigned') + '</td>';
                    html += '<td>' + (booking.CustomerName || '-') + '</td>';
                    html += '<td>' + (booking.BookingDate ? new Date(booking.BookingDate).toLocaleDateString() : '-') + '</td>';
                    html += '<td>' + (booking.BookingTime || '-') + '</td>';
                    html += '<td><span class="badge badge-' + (booking.Status == 'Confirmed' ? 'success' : booking.Status == 'Pending' ? 'warning' : booking.Status == 'Cancelled' ? 'danger' : 'secondary') + '">' + (booking.Status || '-') + '</span></td>';
                    html += '<td><a href="javascript:void(0)" onclick="linkBooking(' + booking.BookingId + ')" class="btn btn-sm btn-success"><i class="fas fa-link"></i> Link</a></td>';
                    html += '</tr>';
                });
                html += '</tbody></table></div>';
                $('#divBookingsList').html(html);
            } else {
                $('#divBookingsList').html('<div class="alert alert-warning"><i class="fas fa-exclamation-triangle"></i> No bookings found.</div>');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred while searching bookings');
        }
    });
}

function linkBooking(bookingId) {
    if (!confirm('Are you sure you want to link this booking to the KOT?')) {
        return;
    }

    var kotId = $('#KotId').val();
    if (!kotId || kotId == 0) {
        toastr.error("KOT ID is missing.");
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/kot/linktobookingaction',
        datatype: "json",
        data: {
            kotId: kotId,
            bookingId: bookingId
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
            toastr.error('An error occurred while linking booking');
        }
    });
}

function clearSearch() {
    $('#txtBookingNo').val('');
    $('#txtFromDate').val('');
    $('#txtToDate').val('');
    $('#divBookingsList').html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> Use the search form above to find bookings to link.</div>');
}

