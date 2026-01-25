function linkKot(kotId) {
    if (!confirm('Are you sure you want to link this KOT to the booking?')) {
        return;
    }

    var bookingId = $('#BookingId').val();
    if (!bookingId || bookingId == 0) {
        toastr.error("Booking ID is missing.");
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/tablebooking/linktokotaction',
        datatype: "json",
        data: {
            bookingId: bookingId,
            kotId: kotId
        },
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                setTimeout(function () {
                    window.location.href = '/tablebooking/details/' + bookingId;
                }, 1000);
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred while linking KOT');
        }
    });
}

