$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    // Initialize toggle switches on page load (for server-side rendered data)
    $('.chkIsActive').bootstrapToggle();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    // Clear inline error for PublicBookingSlug when user starts typing
    $(document).on('input', '#editTxtPublicBookingSlug', function() {
        $('#divPublicBookingSlug').hide().text('');
        $(this).css('border', '');
    });

});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') {document.getElementById('success').play();}
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantsettings/RestaurantSettingsFetch',
        datatype: "html",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            $("#divLoading").hide();

            // Reinitialize toggle switches after table content is loaded
            $('.chkIsActive').bootstrapToggle();

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
        }
    });
};

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var timeSlots = []; // Can be populated from a time slot picker
    
    // Get cookie data
    var cookieData = Cookies.get('data');
    var companyId = 0;
    var branchId = 0;
    var addedBy = 0;
    
    if (cookieData) {
        var cookieParts = cookieData.split('&');
        for (var i = 0; i < cookieParts.length; i++) {
            var part = cookieParts[i].split('=');
            if (part[0] === 'CompanyId') companyId = parseInt(part[1]) || 0;
            if (part[0] === 'BranchId') branchId = parseInt(part[1]) || 0;
            if (part[0] === 'Id') addedBy = parseInt(part[1]) || 0;
        }
    }
    
    var urlParams = new URLSearchParams(window.location.search);
    var restaurantSettingsId = urlParams.get('RestaurantSettingsId');
    var editBranchId = parseInt($('#editBranchId').val()) || branchId;
    
    // Collect Operating Days
    var operatingDays = [];
    $('.operating-day-checkbox:checked').each(function() {
        operatingDays.push(parseInt($(this).val()));
    });

    // Collect Manual Time Slots
    // All manual time slots are active by default (no need for toggle)
    var bookingTimeSlotsNormalized = [];
    $('#editTimeSlotsContainer .time-slot-item').each(function() {
        var slotId = $(this).data('slot-id') || 0;
        var timeSlot = $(this).find('.time-slot-input').val();
        if (timeSlot) {
            bookingTimeSlotsNormalized.push({
                BookingTimeSlotId: slotId,
                TimeSlotString: timeSlot,  // Send as string, same as date override slots
                IsActive: true,  // All manual time slots are active
                DisplayOrder: bookingTimeSlotsNormalized.length
            });
        }
    });

    // Collect Date Overrides
    var dateOverrides = [];
    $('#editDateOverridesContainer .date-override-item').each(function() {
        var overrideId = $(this).data('override-id') || 0;
        var overrideDate = $(this).find('.override-date').val();
        var isClosed = $(this).find('.override-is-closed').val() === 'true';
        var reason = $(this).find('.override-reason').val() || '';
        
        // Collect time slots for this override
        var overrideTimeSlots = [];
        $(this).find('.override-slot-item').each(function() {
            var slotTime = $(this).find('.override-slot-time').val();
            if (slotTime) {
                overrideTimeSlots.push({
                    TimeSlotString: slotTime  // Send as string, same as BookingStartTimeString
                });
            }
        });

        if (overrideDate) {
            dateOverrides.push({
                BookingDateOverrideId: overrideId,
                OverrideDate: overrideDate,
                IsClosed: isClosed,
                Reason: reason,
                TimeSlots: overrideTimeSlots
            });
        }
    });

    // Get BookingStartTime and BookingEndTime as strings (following table booking pattern)
    var bookingTimeSlotMode = $('#editSelBookingTimeSlotMode').val() || 'Auto';
    var bookingStartTimeString = $('#editTxtBookingStartTime').val() || null;
    var bookingEndTimeString = $('#editTxtBookingEndTime').val() || null;
    
    // When mode is "Manual", set to null (not used)
    if (bookingTimeSlotMode !== 'Auto') {
        bookingStartTimeString = null;
        bookingEndTimeString = null;
    }

    var det = {
        RestaurantSettingsId: parseInt(restaurantSettingsId) || 0,
        BranchId: editBranchId,
        EnableKitchenDisplay: $('#editChkEnableKitchenDisplay').is(':checked'),
        AutoPrintKot: $('#editChkAutoPrintKot').is(':checked'),
        EnableTableBooking: $('#editChkEnableTableBooking').is(':checked'),
        EnableRecurringBooking: $('#editChkEnableRecurringBooking').is(':checked'),
        BookingAdvanceDays: parseInt($('#editTxtBookingAdvanceDays').val()) || 30,
        BookingTimeSlotMode: bookingTimeSlotMode,
        BookingStartTimeString: bookingStartTimeString,
        BookingEndTimeString: bookingEndTimeString,
        BookingTimeSlotsList: timeSlots,
        DefaultBookingDuration: parseInt($('#editTxtDefaultBookingDuration').val()) || 120,
        RequireDeposit: $('#editChkRequireDeposit').is(':checked'),
        DepositMode: $('#editSelDepositMode').val(),
        DepositFixedAmount: ($('#editChkRequireDeposit').is(':checked') && $('#editSelDepositMode').val() === 'Fixed') ? (parseFloat($('#editTxtDepositFixedAmount').val()) || 0) : 0,
        DepositPerGuestAmount: ($('#editChkRequireDeposit').is(':checked') && $('#editSelDepositMode').val() === 'PerGuest') ? (parseFloat($('#editTxtDepositPerGuestAmount').val()) || 0) : 0,
        EnablePublicBooking: $('#editChkEnablePublicBooking').is(':checked'),
        PublicBookingSlug: $('#editTxtPublicBookingSlug').val(),
        PublicBookingAdvanceDays: parseInt($('#editTxtPublicBookingAdvanceDays').val()) || 30,
        PublicBookingRequireDeposit: $('#editChkPublicBookingRequireDeposit').is(':checked'),
        PublicBookingDepositMode: $('#editSelPublicBookingDepositMode').val(),
        PublicBookingDepositFixedAmount: ($('#editChkPublicBookingRequireDeposit').is(':checked') && $('#editSelPublicBookingDepositMode').val() === 'Fixed') ? (parseFloat($('#editTxtPublicBookingDepositFixedAmount').val()) || 0) : 0,
        PublicBookingDepositPercentage: ($('#editChkPublicBookingRequireDeposit').is(':checked') && $('#editSelPublicBookingDepositMode').val() === 'Percentage') ? (parseFloat($('#editTxtPublicBookingDepositPercentage').val()) || 0) : 0,
        PublicBookingDepositPerGuestAmount: ($('#editChkPublicBookingRequireDeposit').is(':checked') && $('#editSelPublicBookingDepositMode').val() === 'PerGuest') ? (parseFloat($('#editTxtPublicBookingDepositPerGuestAmount').val()) || 0) : 0,
        PublicBookingAutoConfirm: $('#editChkPublicBookingAutoConfirm').is(':checked'),
        EnablePublicBookingCancellation: $('#editChkPublicBookingCancellation').is(':checked'),
        AllowCancelAfterConfirm: $('#editChkAllowCancelAfterConfirm').is(':checked'),
        PublicBookingCancellationDaysBefore: $('#editChkPublicBookingCancellation').is(':checked') ? (parseInt($('#editTxtCancellationDaysBefore').val()) || 0) : 0,
        PublicBookingCancellationChargeMode: $('#editChkPublicBookingCancellation').is(':checked') ? ($('#editSelCancellationChargeMode').val() || 'None') : 'None',
        PublicBookingCancellationFixedCharge: ($('#editSelCancellationChargeMode').val() === 'Fixed') ? (parseFloat($('#editTxtCancellationFixedCharge').val()) || 0) : 0,
        PublicBookingCancellationPercentage: ($('#editSelCancellationChargeMode').val() === 'Percentage') ? (parseFloat($('#editTxtCancellationPercentage').val()) || 0) : 0,
        PublicBookingCancellationPerGuestCharge: ($('#editSelCancellationChargeMode').val() === 'PerGuest') ? (parseFloat($('#editTxtCancellationPerGuestCharge').val()) || 0) : 0,
        // Normalized properties
        OperatingDays: operatingDays,
        OperatingDaysInt: operatingDays, // Also send as integers for model binding
        BookingTimeSlotsNormalized: bookingTimeSlotsNormalized,
        DateOverrides: dateOverrides,
        // IsActive is managed from the list view, not from edit page - don't send it to preserve existing value
        CompanyId: companyId,
        AddedBy: addedBy
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantsettings/RestaurantSettingsUpdate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                // Check if error is related to booking slug - show inline
                if (data.Message && (data.Message.toLowerCase().indexOf('booking slug') !== -1 || data.Message.toLowerCase().indexOf('slug') !== -1)) {
                    // Show inline error for PublicBookingSlug field
                    $('#divPublicBookingSlug').text(data.Message).show();
                    $('#editTxtPublicBookingSlug').css('border', '2px solid #dc3545');
                    // Ensure the field is visible
                    $('#editDivPublicBookingSlug').show();
                    // Scroll to the field
                    $('html, body').animate({
                        scrollTop: $('#editDivPublicBookingSlug').offset().top - 100
                    }, 500);
                } else {
                    // For other errors, show toastr
                    if (EnableSound == 'True') {document.getElementById('error').play();}
                    toastr.error(data.Message);
                }
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);

                    
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                // Success: store message and redirect to index (toast/sound shown on index load like Category)
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);                
                window.location.href = "/restaurantsettings/index";
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

// QR Code functions for Restaurant Settings
function generateRestaurantSettingsQRCode(restaurantSettingsId, publicBookingSlug) {
    $("#divLoading").show();
    
    // Get cookie data
    var cookieData = Cookies.get('data');
    var companyId = 0;
    
    if (cookieData) {
        var cookieParts = cookieData.split('&');
        for (var i = 0; i < cookieParts.length; i++) {
            var part = cookieParts[i].split('=');
            if (part[0] === 'CompanyId') companyId = parseInt(part[1]) || 0;
        }
    }
    
    $.ajax({
        url: '/restaurantsettings/GenerateQrCode',
        datatype: "json",
        data: JSON.stringify({ 
            RestaurantSettingsId: restaurantSettingsId,
            CompanyId: companyId
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();

            if (data.Status == 1 && data.Data) {
                // Use specific API keys directly
                var qrImage = data.Data.QrCodeImageUrl || null;
                var qrUrl = data.Data.QrCodeUrl || '';

                // Play success sound (if enabled)
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    var successAudio = document.getElementById('success');
                    if (successAudio) successAudio.play();
                }

                // Show green success toast
                if (typeof toastr !== 'undefined') {
                    toastr.success(data.Message || 'QR code generated successfully');
                }

                // Build QR code modal
                var imageHtml = qrImage
                    ? `<img src="${qrImage}" alt="QR Code" style="max-width: 300px; border: 2px solid #007bff; padding: 10px; background: white;" />`
                    : `<div class="alert alert-warning">QR image not available, but the URL can still be used.</div>`;

                var modalHtml = `
                    <div class="modal fade" id="qrCodeModal" tabindex="-1" role="dialog">
                        <div class="modal-dialog modal-dialog-centered" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title"><i class="fas fa-qrcode"></i> QR Code - Public Booking</h5>
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                </div>
                                <div class="modal-body text-center">
                                    <div class="mb-3">
                                        ${imageHtml}
                                    </div>
                                    <p class="text-muted"><small>Scan this QR code to access public booking</small></p>
                                    <div class="mt-3 mb-3">
                                        <label class="font-weight-bold">Public Booking URL:</label>
                                        <div class="input-group">
                                            <input type="text" class="form-control" id="qrCodeUrlInput" value="${qrUrl}" readonly style="font-size: 12px;">
                                            <div class="input-group-append">
                                                <button class="btn btn-outline-secondary" type="button" onclick="copyQRCodeUrl()" title="Copy URL">
                                                    <i class="fas fa-copy"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="mt-3">
                                        <button type="button" class="btn btn-primary" ${qrImage ? `onclick="downloadQRCode('${qrImage}', 'Restaurant_PublicBooking_QRCode.png')"` : 'disabled'}>
                                            <i class="fas fa-download"></i> Download QR Code
                                        </button>
                                        <button type="button" class="btn btn-secondary" ${qrImage ? `onclick="printQRCode('${qrImage}')"` : 'disabled'}>
                                            <i class="fas fa-print"></i> Print QR Code
                                        </button>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;

                // Remove existing modal if any
                $('#qrCodeModal').remove();

                // Add modal to body
                $('body').append(modalHtml);

                // Set URL value after modal is created
                if (qrUrl) {
                    $('#qrCodeUrlInput').val(qrUrl);
                }

                // Show modal
                $('#qrCodeModal').modal('show');
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    if (document.getElementById('error')) document.getElementById('error').play();
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message || 'Failed to generate QR code');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                if (document.getElementById('error')) document.getElementById('error').play();
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while generating QR code');
        }
    });
}

function downloadQRCode(imageUrl, filename) {
    // Works with both regular URLs and data URLs
    var link = document.createElement('a');
    link.download = filename;
    link.href = imageUrl;
    // For cross-origin URLs, we may need to fetch and convert to blob
    if (imageUrl.startsWith('http') && !imageUrl.startsWith('data:')) {
        fetch(imageUrl)
            .then(response => response.blob())
            .then(blob => {
                var url = window.URL.createObjectURL(blob);
                link.href = url;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            })
            .catch(function() {
                // Fallback: open in new tab if fetch fails
                window.open(imageUrl, '_blank');
            });
    } else {
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

function printQRCode(imageUrl) {
    // Works with both regular URLs and data URLs
    var printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title>QR Code</title>
                <style>
                    body { 
                        display: flex; 
                        justify-content: center; 
                        align-items: center; 
                        height: 100vh; 
                        margin: 0; 
                    }
                    img { 
                        max-width: 100%; 
                        height: auto; 
                    }
                </style>
            </head>
            <body>
                <img src="${imageUrl}" alt="QR Code" />
                <script>
                    window.onload = function() {
                        window.print();
                    };
                </script>
            </body>
        </html>
    `);
    printWindow.document.close();
}

function copyQRCodeUrl() {
    var urlInput = document.getElementById('qrCodeUrlInput');
    if (urlInput) {
        urlInput.select();
        urlInput.setSelectionRange(0, 99999); // For mobile devices
        try {
            document.execCommand('copy');
            if (typeof toastr !== 'undefined') {
                toastr.success('URL copied to clipboard!');
            } else {
                alert('URL copied to clipboard!');
            }
        } catch (err) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Failed to copy URL');
            } else {
                alert('Failed to copy URL');
            }
        }
    }
}

function ActiveInactive(RestaurantSettingsId, IsActive) {
    var det = {
        RestaurantSettingsId: RestaurantSettingsId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantsettings/RestaurantSettingsActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

