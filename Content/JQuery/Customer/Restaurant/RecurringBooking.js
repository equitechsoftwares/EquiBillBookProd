// Recurring Booking Functions
var previousKots = []; // For KDS sound notifications

function showRecurringBookingModal(bookingId) {
    // Create modal for recurring booking
    var modalHtml = `
        <div class="modal fade" id="recurringBookingModal" tabindex="-1" role="dialog">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><i class="fas fa-repeat"></i> Create Recurring Booking</h5>
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                    </div>
                    <div class="modal-body">
                        <form id="recurringBookingForm">
                            <input type="hidden" id="recurringBookingId" value="0" />
                            <input type="hidden" id="recurringBookingBookingId" value="${bookingId}" />
                            <div class="form-group">
                                <label>Recurrence Type <span class="text-danger">*</span></label>
                                <select class="form-control" id="recurringRecurrenceType" required>
                                    <option value="Daily">Daily</option>
                                    <option value="Weekly" selected>Weekly</option>
                                    <option value="Monthly">Monthly</option>
                                </select>
                            </div>
                            <div class="form-group" id="weeklyOptions">
                                <label>Days of Week <span class="text-danger">*</span></label>
                                <div class="row">
                                    <div class="col-md-12">
                                        <label class="checkbox-inline"><input type="checkbox" value="0" class="dayOfWeek"> Sunday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="1" class="dayOfWeek"> Monday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="2" class="dayOfWeek"> Tuesday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="3" class="dayOfWeek"> Wednesday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="4" class="dayOfWeek"> Thursday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="5" class="dayOfWeek"> Friday</label>
                                        <label class="checkbox-inline"><input type="checkbox" value="6" class="dayOfWeek"> Saturday</label>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group" id="monthlyOptions" style="display: none;">
                                <label>Day of Month <span class="text-danger">*</span></label>
                                <input type="number" class="form-control" id="recurringDayOfMonth" min="1" max="31" value="1" />
                            </div>
                            <div class="form-group">
                                <label>Repeat Every (Interval) <span class="text-danger">*</span></label>
                                <input type="number" class="form-control" id="recurringInterval" min="1" value="1" required />
                                <small class="form-text text-muted">Repeat every X days/weeks/months</small>
                            </div>
                            <div class="form-group">
                                <label>Start Date <span class="text-danger">*</span></label>
                                <input type="date" class="form-control" id="recurringStartDate" required />
                            </div>
                            <div class="form-group">
                                <label>End Date</label>
                                <input type="date" class="form-control" id="recurringEndDate" />
                                <small class="form-text text-muted">Leave empty for no end date</small>
                            </div>
                            <div class="form-group">
                                <div class="custom-control custom-switch">
                                    <input type="checkbox" class="custom-control-input" id="recurringIsActive" checked>
                                    <label class="custom-control-label" for="recurringIsActive">Active</label>
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary" onclick="saveRecurringBooking()">Save Recurring Booking</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    $('#recurringBookingModal').remove();
    
    // Add modal to body
    $('body').append(modalHtml);
    
    // Set default start date to today
    var today = new Date().toISOString().split('T')[0];
    $('#recurringStartDate').val(today);
    
    // Show modal
    $('#recurringBookingModal').modal('show');
    
    // Handle recurrence type change
    $('#recurringRecurrenceType').on('change', function() {
        if ($(this).val() === 'Weekly') {
            $('#weeklyOptions').show();
            $('#monthlyOptions').hide();
        } else if ($(this).val() === 'Monthly') {
            $('#weeklyOptions').hide();
            $('#monthlyOptions').show();
        } else {
            $('#weeklyOptions').hide();
            $('#monthlyOptions').hide();
        }
    });
}

function saveRecurringBooking() {
    var bookingId = $('#recurringBookingBookingId').val();
    var recurrenceType = $('#recurringRecurrenceType').val();
    var interval = parseInt($('#recurringInterval').val()) || 1;
    var startDate = $('#recurringStartDate').val();
    var endDate = $('#recurringEndDate').val() || null;
    var isActive = $('#recurringIsActive').is(':checked');
    
    var daysOfWeek = [];
    var dayOfMonthValue = null;
    
    if (recurrenceType === 'Weekly') {
        $('.dayOfWeek:checked').each(function() {
            daysOfWeek.push(parseInt($(this).val()));
        });
        if (daysOfWeek.length === 0) {
            toastr.error('Please select at least one day of the week');
            return;
        }
    } else if (recurrenceType === 'Monthly') {
        dayOfMonthValue = parseInt($('#recurringDayOfMonth').val()) || null;
    }
    
    var det = {
        RecurringBookingId: parseInt($('#recurringBookingId').val()) || 0,
        BookingId: parseInt(bookingId),
        RecurrenceType: recurrenceType,
        RepeatEveryNumber: interval,
        RepeatEvery: null,
        DayOfMonth: dayOfMonthValue,
        DaysOfWeek: recurrenceType === 'Weekly' ? daysOfWeek : null,
        StartDate: startDate,
        EndDate: endDate,
        IsActive: isActive
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/tablebooking/CreateRecurringBooking',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('success')) document.getElementById('success').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.success(data.Message);
                $('#recurringBookingModal').modal('hide');
                if (typeof loadRecurringBookingInfo === 'function') {
                    loadRecurringBookingInfo(bookingId);
                }
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred');
        }
    });
}

function loadRecurringBookingInfo(bookingId) {
    var companyId = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
        }
    }
    
    $.ajax({
        url: '/tablebooking/GetRecurringBookings',
        datatype: "json",
        data: JSON.stringify({ CompanyId: companyId }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            if (data.Status == 1 && data.Data && data.Data.RecurringBookings) {
                var recurring = data.Data.RecurringBookings.find(function(r) { return r.BookingId == bookingId; });
                if (recurring) {
                    $('#recurringBookingSection').show();
                    var html = '<p><strong>Recurrence Type:</strong> ' + recurring.RecurrenceType + '</p>';
                    if (recurring.RepeatEveryNumber) {
                        html += '<p><strong>Interval:</strong> Every ' + recurring.RepeatEveryNumber + ' ' + (recurring.RepeatEvery || (recurring.RecurrenceType === 'Daily' ? 'Day' : recurring.RecurrenceType === 'Weekly' ? 'Week' : 'Month')) + '</p>';
                    }
                    html += '<p><strong>Start Date:</strong> ' + new Date(recurring.StartDate).toLocaleDateString() + '</p>';
                    if (recurring.IsNeverExpires) {
                        html += '<p><strong>End Date:</strong> Never Expires</p>';
                    } else if (recurring.EndDate) {
                        html += '<p><strong>End Date:</strong> ' + new Date(recurring.EndDate).toLocaleDateString() + '</p>';
                    }
                    html += '<p><strong>Status:</strong> <span class="badge badge-' + (recurring.IsActive ? 'success' : 'secondary') + '">' + (recurring.IsActive ? 'Active' : 'Inactive') + '</span></p>';
                    html += '<button type="button" class="btn btn-sm btn-primary" onclick="editRecurringBooking(' + recurring.RecurringBookingId + ', ' + bookingId + ')"><i class="fas fa-edit"></i> Edit</button> ';
                    html += '<button type="button" class="btn btn-sm btn-danger" onclick="deleteRecurringBooking(' + recurring.RecurringBookingId + ', ' + bookingId + ')"><i class="fas fa-trash"></i> Delete</button>';
                    $('#recurringBookingContent').html(html);
                } else {
                    $('#recurringBookingSection').hide();
                }
            } else {
                $('#recurringBookingSection').hide();
            }
        },
        error: function() {
            $('#recurringBookingSection').hide();
        }
    });
}

function editRecurringBooking(recurringBookingId, bookingId) {
    var companyId = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
        }
    }
    
    // Load recurring booking data and show modal
    $.ajax({
        url: '/tablebooking/GetRecurringBookings',
        datatype: "json",
        data: JSON.stringify({ CompanyId: companyId }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            if (data.Status == 1 && data.Data && data.Data.RecurringBookings) {
                var recurring = data.Data.RecurringBookings.find(function(r) { return r.RecurringBookingId == recurringBookingId; });
                if (recurring) {
                    showRecurringBookingModal(bookingId);
                    $('#recurringBookingId').val(recurring.RecurringBookingId);
                    $('#recurringRecurrenceType').val(recurring.RecurrenceType);
                    $('#recurringStartDate').val(recurring.StartDate.split('T')[0]);
                    if (!recurring.IsNeverExpires && recurring.EndDate) {
                        $('#recurringEndDate').val(recurring.EndDate.split('T')[0]);
                    }
                    $('#recurringIsActive').prop('checked', recurring.IsActive);
                    
                    // Use new normalized columns
                    if (recurring.RepeatEveryNumber) {
                        $('#recurringInterval').val(recurring.RepeatEveryNumber);
                    } else {
                        $('#recurringInterval').val(1);
                    }
                    
                    if (recurring.RecurrenceType === 'Weekly' && recurring.DaysOfWeek && recurring.DaysOfWeek.length > 0) {
                        recurring.DaysOfWeek.forEach(function(day) {
                            $('.dayOfWeek[value="' + day + '"]').prop('checked', true);
                        });
                    }
                    
                    if (recurring.RecurrenceType === 'Monthly' && recurring.DayOfMonth) {
                        $('#recurringDayOfMonth').val(recurring.DayOfMonth);
                    }
                    
                    // Fallback to JSON pattern for backward compatibility
                    if (!recurring.RepeatEveryNumber && recurring.RecurrencePattern) {
                        try {
                            var pattern = JSON.parse(recurring.RecurrencePattern || '{}');
                            if (pattern.interval) {
                                $('#recurringInterval').val(pattern.interval);
                            }
                            if (recurring.RecurrenceType === 'Weekly' && pattern.daysOfWeek) {
                                pattern.daysOfWeek.forEach(function(day) {
                                    $('.dayOfWeek[value="' + day + '"]').prop('checked', true);
                                });
                            }
                            if (recurring.RecurrenceType === 'Monthly' && pattern.dayOfMonth) {
                                $('#recurringDayOfMonth').val(pattern.dayOfMonth);
                            }
                        } catch (e) {
                            // Ignore JSON parse errors
                        }
                    }
                }
            }
        }
    });
}

function deleteRecurringBooking(recurringBookingId, bookingId) {
    if (!confirm('Are you sure you want to delete this recurring booking?')) {
        return;
    }
    
    var companyId = 0;
    var addedBy = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
            addedBy = parseInt(Cookies.get('data').split('&')[2].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
            addedBy = 0;
        }
    }
    
    var det = {
        RecurringBookingId: recurringBookingId,
        CompanyId: companyId,
        AddedBy: addedBy
    };
    
    $("#divLoading").show();
    var url = bookingId ? '/tablebooking/DeleteRecurringBooking' : '/recurringbooking/DeleteRecurringBooking';
    $.ajax({
        url: url,
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('success')) document.getElementById('success').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.success(data.Message);
                if (bookingId) {
                    $('#recurringBookingSection').hide();
                    if (typeof loadRecurringBookingInfo === 'function') {
                        loadRecurringBookingInfo(bookingId);
                    }
                } else {
                    if (typeof fetchList === 'function') {
                        fetchList(1);
                    } else {
                        window.location.reload();
                    }
                }
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred');
        }
    });
}

// List page functions
$(function () {
    // Only initialize if on list page
    if ($('#tblData').length && window.location.href.indexOf('/recurringbooking/index') > -1) {
        // Check if table already has data from server-side rendering
        var hasData = $('#tblData tbody tr').length > 0;
        
        if (hasData) {
            // Initialize toggle checkboxes for server-side rendered data
            $('.chkIsActive').bootstrapToggle();
            
            // Initialize DataTable on existing server-side data
            // Use a small delay to ensure DOM is ready
            setTimeout(function() {
                if (!$.fn.DataTable.isDataTable('#tblData')) {
                    $('#tblData').DataTable({
                        lengthChange: false,
                        searching: false,
                        autoWidth: false,
                        responsive: true,
                        paging: false,
                        bInfo: false,
                        ordering: true, // Enable sorting
                        order: [[0, 'asc']], // Default sort by first column
                        columnDefs: [
                            {
                                targets: 1, // Action column (index 1)
                                orderable: false // Disable sorting on Action column
                            }
                        ]
                    });
                }
            }, 100);
        } else {
            // No data, fetch it
            fetchList(1);
        }
    }

    // Form page initialization
    if (($('#RecurringBookingId').length || $('#ddlBookingId').length) && window.location.href.indexOf('/recurringbooking/') > -1) {
        // Initialize Select2 for dropdowns
        $('.select2').select2({
            width: '100%'
        });

        // Initialize datetime pickers
        var DateFormat = 'DD-MMM-YYYY';
        var TimeFormat = 'hh:mm A';
        
        // Try to get from cookies if available
        if (typeof Cookies !== 'undefined' && Cookies.get('BusinessSetting')) {
            try {
                DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1] || 'DD-MMM-YYYY';
                TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a') || 'hh:mm A';
            } catch (e) {
                // Use defaults
            }
        }

        // Initialize Start Date picker (date only, no time needed)
        // Use a flag to prevent multiple initializations
        if ($('#_StartDate').length && !$('#_StartDate').data('datetimepicker-initialized')) {
            // Store the original value BEFORE any initialization
            var startDateValue = $('#txtStartDate').val();
            
            var startDateOptions = {
                widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
                format: DateFormat.toUpperCase(),
                ignoreReadonly: true,
                useCurrent: false,
                keepOpen: false
            };
            
            // Parse the existing value and set it as the date option BEFORE initialization
            if (startDateValue && startDateValue.trim() !== '') {
                if (typeof moment !== 'undefined') {
                    try {
                        // Try multiple format variations to parse the date
                        var parsedDate = moment(startDateValue, [
                            DateFormat.toUpperCase(),
                            'DD-MMM-YYYY',
                            'DD/MMM/YYYY',
                            'DD-MM-YYYY',
                            'DD/MM/YYYY',
                            'YYYY-MM-DD',
                            'DD MMM YYYY',
                            'DD-MMM-YY'
                        ], true);
                        
                        if (parsedDate.isValid()) {
                            // Set the date option directly - this is the key!
                            startDateOptions.date = parsedDate.toDate();
                        } else {
                            // Fallback: try native Date parsing
                            var nativeDate = new Date(startDateValue);
                            if (!isNaN(nativeDate.getTime())) {
                                startDateOptions.date = nativeDate;
                            }
                        }
                    } catch (e) {
                        // If parsing fails, don't set date option
                    }
                }
            } else {
                // Only set defaultDate if there's no existing value
                startDateOptions.defaultDate = new Date();
            }
            
            // Initialize the datetimepicker with the date already set
            $('#_StartDate').datetimepicker(startDateOptions);
            $('#_StartDate').data('datetimepicker-initialized', true);
            $('#_StartDate').addClass('notranslate');
            
            // Immediately after initialization, ensure the value is preserved
            if (startDateValue && startDateValue.trim() !== '') {
                // Use multiple timeouts to catch any clearing that might happen
                [0, 50, 100, 200, 500].forEach(function(delay) {
                    setTimeout(function() {
                        var picker = $('#_StartDate').data('DateTimePicker');
                        var currentValue = $('#txtStartDate').val();
                        
                        // If the value was cleared or doesn't match, restore it
                        if (!currentValue || currentValue.trim() === '' || currentValue !== startDateValue) {
                            if (picker && typeof moment !== 'undefined') {
                                try {
                                    var parsedDate = moment(startDateValue, [
                                        DateFormat.toUpperCase(),
                                        'DD-MMM-YYYY',
                                        'DD/MMM/YYYY',
                                        'DD-MM-YYYY',
                                        'DD/MM/YYYY',
                                        'YYYY-MM-DD'
                                    ], true);
                                    if (parsedDate.isValid()) {
                                        picker.date(parsedDate);
                                        // Also set the input directly as backup
                                        $('#txtStartDate').val(startDateValue);
                                    } else {
                                        // If parsing fails, just set the input value directly
                                        $('#txtStartDate').val(startDateValue);
                                    }
                                } catch (e) {
                                    // If all else fails, just set the input value directly
                                    $('#txtStartDate').val(startDateValue);
                                }
                            } else {
                                // If picker not available, set input value directly
                                $('#txtStartDate').val(startDateValue);
                            }
                        }
                    }, delay);
                });
            }
        }
        
        // Add event handler for datetimepicker change event (outside initialization check to ensure it's always attached)
        // This ensures loadBookingTimeSlots() is called when StartDate changes via the picker
        if ($('#_StartDate').length) {
            // Remove any existing handler first to prevent duplicates
            $('#_StartDate').off('change.datetimepicker');
            $('#_StartDate').on('change.datetimepicker', function(e) {
                // Trigger if a date is selected (e.date is a valid date object)
                // This will fire when user selects a date from the picker
                if (e.date !== false && e.date !== null && e.date !== undefined) {
                    // Small delay to ensure the input value is updated
                    setTimeout(function() {
                        if (typeof loadBookingTimeSlots === 'function') {
                            loadBookingTimeSlots();
                        }
                        // Reload table layout when date changes to show updated availability
                        if ($('#ddlBranchId').val()) {
                            if (typeof loadBookingTableLayout === 'function') {
                                loadBookingTableLayout();
                            }
                        }
                    }, 100);
                }
            });
        }

        // Initialize End Date picker (date only, no time needed)
        // Use a flag to prevent multiple initializations
        if ($('#_EndDate').length && !$('#_EndDate').data('datetimepicker-initialized')) {
            // Store the original value BEFORE any initialization
            var endDateValue = $('#txtEndDate').val();
            
            var endDateOptions = {
                widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
                format: DateFormat.toUpperCase(),
                ignoreReadonly: true,
                useCurrent: false,
                keepOpen: false
            };
            
            // Parse the existing value and set it as the date option BEFORE initialization
            if (endDateValue && endDateValue.trim() !== '') {
                if (typeof moment !== 'undefined') {
                    try {
                        // Try multiple format variations to parse the date
                        var parsedDate = moment(endDateValue, [
                            DateFormat.toUpperCase(),
                            'DD-MMM-YYYY',
                            'DD/MMM/YYYY',
                            'DD-MM-YYYY',
                            'DD/MM/YYYY',
                            'YYYY-MM-DD',
                            'DD MMM YYYY',
                            'DD-MMM-YY'
                        ], true);
                        
                        if (parsedDate.isValid()) {
                            // Set the date option directly - this is the key!
                            endDateOptions.date = parsedDate.toDate();
                        } else {
                            // Fallback: try native Date parsing
                            var nativeDate = new Date(endDateValue);
                            if (!isNaN(nativeDate.getTime())) {
                                endDateOptions.date = nativeDate;
                            }
                        }
                    } catch (e) {
                        // If parsing fails, don't set date option
                    }
                }
            }
            
            // Initialize the datetimepicker with the date already set
            $('#_EndDate').datetimepicker(endDateOptions);
            $('#_EndDate').data('datetimepicker-initialized', true);
            $('#_EndDate').addClass('notranslate');
            
            // Immediately after initialization, ensure the value is preserved
            if (endDateValue && endDateValue.trim() !== '') {
                // Use multiple timeouts to catch any clearing that might happen
                [0, 50, 100, 200, 500].forEach(function(delay) {
                    setTimeout(function() {
                        var picker = $('#_EndDate').data('DateTimePicker');
                        var currentValue = $('#txtEndDate').val();
                        
                        // If the value was cleared or doesn't match, restore it
                        if (!currentValue || currentValue.trim() === '' || currentValue !== endDateValue) {
                            if (picker && typeof moment !== 'undefined') {
                                try {
                                    var parsedDate = moment(endDateValue, [
                                        DateFormat.toUpperCase(),
                                        'DD-MMM-YYYY',
                                        'DD/MMM/YYYY',
                                        'DD-MM-YYYY',
                                        'DD/MM/YYYY',
                                        'YYYY-MM-DD'
                                    ], true);
                                    if (parsedDate.isValid()) {
                                        picker.date(parsedDate);
                                        // Also set the input directly as backup
                                        $('#txtEndDate').val(endDateValue);
                                    } else {
                                        // If parsing fails, just set the input value directly
                                        $('#txtEndDate').val(endDateValue);
                                    }
                                } catch (e) {
                                    // If all else fails, just set the input value directly
                                    $('#txtEndDate').val(endDateValue);
                                }
                            } else {
                                // If picker not available, set input value directly
                                $('#txtEndDate').val(endDateValue);
                            }
                        }
                    }, delay);
                });
            }
        }

        // Initialize Never Expires checkbox state
        if ($('#chkIsNeverExpires').is(':checked')) {
            $('.divEndDate').hide();
        }

        // Initialize based on interval unit on page load
        var initialIntervalUnit = $('#ddlIntervalUnit').val() || 'Week';
        updateRecurrenceOptions(initialIntervalUnit);

        // Handle interval unit change - show/hide appropriate options
        $('#ddlIntervalUnit').on('change', function () {
            var unit = $(this).val();
            updateRecurrenceOptions(unit);
        });

        // Handle booking selection change - show/hide booking details
        $('#ddlBookingId').on('change', function () {
            var bookingId = $(this).val();
            if (bookingId && bookingId !== '') {
                // Hide booking details section, use template booking
                $('.bookingDetailsSection').hide();
            } else {
                // Show booking details section, user will enter manually
                $('.bookingDetailsSection').show();
            }
        });

        // Initialize booking details visibility based on initial booking selection
        if ($('#ddlBookingId').val() && $('#ddlBookingId').val() !== '') {
            $('.bookingDetailsSection').hide();
        } else {
            $('.bookingDetailsSection').show();
        }

        // Helper function to update recurrence options based on interval unit
        function updateRecurrenceOptions(unit) {
            if (unit === 'Week') {
                $('#weeklyOptionsColumn').show();
                $('#weeklyOptions').show();
            } else {
                // Day, Month, or Year - hide weekly options and its column
                $('#weeklyOptionsColumn').hide();
                $('#weeklyOptions').hide();
            }
        }

    }
});

// Toggle Never Expires functionality
function toggleNeverExpires() {
    if ($('#chkIsNeverExpires').is(':checked')) {
        $('.divEndDate').hide();
        $('#txtEndDate').val('');
    } else {
        $('.divEndDate').show();
    }
}

// Flag to prevent multiple simultaneous DataTable initializations
var isInitializingDataTable = false;

function fetchList(PageIndex) {
    var companyId = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
        }
    }

    var obj = {
        CompanyId: companyId,
        PageIndex: PageIndex || 1,
        PageSize: parseInt($('#txtPageSize').val()) || 25,
        RecurrenceType: $('#ddlRecurrenceType').val() || '',
        IsActive: $('#ddlStatus').val() === '' ? null : ($('#ddlStatus').val() === 'true'),
        SearchText: $('#txtSearch').val() || ''
    };

    $("#divLoading").show();
    $.ajax({
        url: '/recurringbooking/RecurringBookingFetch',
        datatype: "html",
        data: obj,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            
            // Prevent multiple simultaneous initializations
            if (isInitializingDataTable) {
                return;
            }
            isInitializingDataTable = true;
            
            // Destroy existing DataTable if it exists
            try {
                if ($.fn.DataTable.isDataTable('#tblData')) {
                    $('#tblData').DataTable().destroy();
                }
            } catch (e) {
                // Ignore errors during destroy
                console.log('Error destroying DataTable:', e);
            }
            
            // Clear and replace table content
            $('#tblData').empty();
            $('#tblData').html(data);
            
            // Reinitialize toggle checkboxes (required after HTML replacement)
            $('.chkIsActive').bootstrapToggle();
            
            // Small delay to ensure DOM is updated before initializing DataTable
            setTimeout(function() {
                try {
                    // Double-check if DataTable was initialized (in case of race condition)
                    if ($.fn.DataTable.isDataTable('#tblData')) {
                        $('#tblData').DataTable().destroy();
                    }
                    
                    // Initialize DataTable with sorting enabled
                    $('#tblData').DataTable({
                        lengthChange: false,
                        searching: false,
                        autoWidth: false,
                        responsive: true,
                        paging: false,
                        bInfo: false,
                        ordering: true, // Explicitly enable sorting
                        order: [[0, 'asc']], // Default sort by first column
                        columnDefs: [
                            {
                                targets: 1, // Action column (index 1)
                                orderable: false // Disable sorting on Action column
                            }
                        ],
                        destroy: true // Ensure proper cleanup on reinitialization
                    });
                } catch (e) {
                    console.error('Error initializing DataTable:', e);
                } finally {
                    isInitializingDataTable = false;
                }
            }, 50);
        },
        error: function (xhr) {
            $("#divLoading").hide();
            isInitializingDataTable = false;
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while loading data');
        }
    });
}

function ActiveInactive(RecurringBookingId, IsActive) {
    var companyId = 0;
    var addedBy = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
            addedBy = parseInt(Cookies.get('data').split('&')[2].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
            addedBy = 0;
        }
    }

    var det = {
        RecurringBookingId: RecurringBookingId,
        IsActive: IsActive,
        CompanyId: companyId,
        AddedBy: addedBy
    };

    $("#divLoading").show();
    $.ajax({
        url: '/recurringbooking/RecurringBookingActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                if (typeof $('#subscriptionExpiryModal') !== 'undefined') {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                }
                if (typeof fetchList === 'function') {
                    fetchList(_PageIndex);
                }
                return;
            }
            else if (data == "False") {
                if (typeof $('#subscriptionExpiryModal') !== 'undefined') {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                }
                if (typeof fetchList === 'function') {
                    fetchList(_PageIndex);
                }
                return;
            }

            if (data.Status == 0) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    if (document.getElementById('error')) document.getElementById('error').play();
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message);
            }
            else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    if (document.getElementById('success')) document.getElementById('success').play();
                }
                if (typeof toastr !== 'undefined') toastr.success(data.Message);
                if (typeof fetchList === 'function') {
                    fetchList();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                if (document.getElementById('error')) document.getElementById('error').play();
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred');
        }
    });
}

function Delete(RecurringBookingId) {
    deleteRecurringBooking(RecurringBookingId, null);
}

function saveRecurringBookingForm(actionType) {
    // actionType: 1 = Save/Update and redirect to list, 2 = Save/Update and redirect to Add form
    if (!actionType) actionType = 1;
    
    // Clear all error messages and borders first (matching pattern from TableBooking.js)
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var isValid = true;
    var bookingId = $('#ddlBookingId').val();
    var interval = parseInt($('#txtInterval').val()) || 1;
    var intervalUnit = $('#ddlIntervalUnit').val() || 'Week';
    
    // Derive recurrence type from interval unit
    var recurrenceType;
    if (intervalUnit === 'Day') {
        recurrenceType = 'Daily';
    } else if (intervalUnit === 'Week') {
        recurrenceType = 'Weekly';
    } else if (intervalUnit === 'Month') {
        recurrenceType = 'Monthly';
    } else if (intervalUnit === 'Year') {
        recurrenceType = 'Yearly';
    } else {
        recurrenceType = 'Weekly'; // Default fallback
    }
    
    // Get date values from datetimepicker
    var startDate = $('#txtStartDate').val();
    var isNeverExpires = $('#chkIsNeverExpires').is(':checked');
    var endDate = $('#txtEndDate').val() || '';
    var isActive = true; // Always active since toggle was removed
    var recurringBookingId = parseInt($('#RecurringBookingId').val()) || 0;

    // Validate - if no booking selected, validate booking details
    // Support both Add.cshtml (ddlCustomer/hdnCustomerId) and Edit.cshtml (ddlCustomerId) field names
    var customerId = $('#hdnCustomerId').val() || $('#ddlCustomerId').val() || $('#ddlCustomer').val();
    var bookingTime = $('#txtBookingTime').val();
    var duration = parseInt($('#txtDuration').val()) || 0;
    var noOfGuests = parseInt($('#txtNoOfGuests').val()) || 0;
    // Support both Add.cshtml (table layout canvas with getSelectedTableIds) and Edit.cshtml (ddlTableId) field names
    var tableIds = [];
    if (typeof getSelectedTableIds === 'function') {
        // Add.cshtml and Edit.cshtml use table layout canvas - get selected tables from canvas
        tableIds = getSelectedTableIds();
    } else {
        // Fallback: check for hidden input with comma-separated string (Edit.cshtml)
        var tableIdValue = $('#ddlTableId').val() || '';
        if (tableIdValue && tableIdValue.trim() !== '') {
            // Parse comma-separated string into array
            tableIds = tableIdValue.split(',').map(function(id) {
                return parseInt(id.trim());
            }).filter(function(id) {
                return !isNaN(id) && id > 0;
            });
        }
    }
    var specialRequest = $('#txtSpecialRequest').val() || null;
    var branchId = $('#ddlBranchId').val();
    var floorId = $('#ddlBookingFloor').val() || '0';
    floorId = parseInt(floorId) || 0;

    if (!bookingId || bookingId == '') {
        // No template booking - validate booking details
        
        // Validate Customer
        if (!customerId || customerId === '' || customerId === '0') {
            $('#divCustomer, #divCustomerId').text('Please select a customer').show();
            var customerSelect = $('#ddlCustomer, #ddlCustomerId');
            if (customerSelect.hasClass('select2-hidden-accessible')) {
                customerSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
            } else if (customerSelect.length > 0) {
                customerSelect.css('border', '2px solid #dc3545');
            }
            isValid = false;
        }

        // Validate Branch
        if (!branchId || branchId === '') {
            $('#divBranchId').text('Please select a branch').show();
            var branchSelect = $('#ddlBranchId');
            if (branchSelect.hasClass('select2-hidden-accessible')) {
                branchSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
            } else {
                branchSelect.css('border', '2px solid #dc3545');
            }
            isValid = false;
        }

        // Validate Booking Time
        if (!bookingTime || bookingTime === '') {
            $('#divBookingTime').text('Please select a booking time').show();
            var timeSelect = $('#ddlBookingTimeSlots');
            if (timeSelect.hasClass('select2-hidden-accessible')) {
                timeSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
            } else {
                timeSelect.css('border', '2px solid #dc3545');
            }
            isValid = false;
        }

        // Validate Duration
        if (!duration || duration <= 0) {
            $('#divDuration').text('Please enter duration').show();
            $('#txtDuration').css('border', '2px solid #dc3545');
            isValid = false;
        }

        // Validate Number of Guests
        if (!noOfGuests || noOfGuests <= 0) {
            $('#divNoOfGuests').text('Please enter number of guests').show();
            $('#txtNoOfGuests').css('border', '2px solid #dc3545');
            isValid = false;
        }

        // Validate Table Selection
        if (!tableIds || tableIds.length === 0) {
            // Support both Add.cshtml (divTableId) and Edit.cshtml (divTableIds) error div IDs
            $('#divTableId, #divTableIds').text('Please select at least one table').show();
            isValid = false;
        }
    }

    // Validate Start Date
    if (!startDate || startDate.trim() === '') {
        $('#divStartDate').text('Please select a start date').show();
        $('#txtStartDate').css('border', '2px solid #dc3545');
        isValid = false;
    }

    // Validate Interval
    if (!interval || interval <= 0) {
        $('#divInterval').text('Please enter a valid interval').show();
        $('#txtInterval').css('border', '2px solid #dc3545');
        isValid = false;
    }

    // Validate Days of Week for Weekly recurrence
    var daysOfWeekArray = [];
    if (recurrenceType === 'Weekly') {
        $('.dayOfWeek:checked').each(function () {
            daysOfWeekArray.push(parseInt($(this).val()));
        });
        if (daysOfWeekArray.length === 0) {
            $('#divDaysOfWeek').text('Please select at least one day of the week').show();
            isValid = false;
        } else if (startDate && startDate.trim() !== '') {
            // Check if StartDate matches any selected day of week
            var startDateISO = convertToISOFormat(startDate);
            if (startDateISO) {
                try {
                    var startDateObj = new Date(startDateISO);
                    if (!isNaN(startDateObj.getTime())) {
                        var startDayOfWeek = startDateObj.getDay(); // 0 = Sunday, 6 = Saturday
                        
                        if (!daysOfWeekArray.includes(startDayOfWeek)) {
                            // StartDate doesn't match any selected day - warn user
                            var dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
                            var selectedDayNames = daysOfWeekArray.map(function(d) { 
                                return dayNames[d]; 
                            }).join(', ');
                            var startDayName = dayNames[startDayOfWeek];
                            
                            // Format the start date for display
                            var formattedStartDate = startDateISO;
                            if (typeof moment !== 'undefined') {
                                try {
                                    formattedStartDate = moment(startDateISO).format('DD-MMM-YYYY');
                                } catch (e) {
                                    // Use ISO format if moment fails
                                }
                            }
                            
                            var proceed = confirm(
                                'Warning: The Start Date (' + formattedStartDate + ' - ' + startDayName + ') does not match any of the selected days (' + selectedDayNames + ').\n\n' +
                                'The first booking will be created on the next matching day after the Start Date, not on the Start Date itself.\n\n' +
                                'Do you want to continue?'
                            );
                            
                            if (!proceed) {
                                isValid = false;
                                // Highlight the StartDate and Days of Week fields
                                $('#txtStartDate').css('border', '2px solid #ffc107');
                                $('#weeklyOptions').css('border', '2px solid #ffc107');
                                $('#divStartDate').html('<i class="fas fa-exclamation-triangle"></i> Start Date does not match selected days').show();
                                $('#divDaysOfWeek').html('<i class="fas fa-exclamation-triangle"></i> Selected days do not include Start Date').show();
                            }
                        }
                    }
                } catch (e) {
                    // Date parsing error - continue anyway
                    console.error('Error checking StartDate match:', e);
                }
            }
        }
    }

    // If validation failed, show error and return
    if (!isValid) {
        if (typeof EnableSound !== 'undefined' && EnableSound == 'True' && document.getElementById('error')) {
            document.getElementById('error').play();
        }
        if (typeof toastr !== 'undefined') {
            toastr.error('Please fill in all required fields correctly');
        }
        return;
    }

    // Convert date format if needed (from datetimepicker format to ISO)
    var startDateISO = convertToISOFormat(startDate);
    var endDateISO = endDate && endDate.trim() !== '' ? convertToISOFormat(endDate) : (isNeverExpires ? '0001-01-01' : '0001-01-01');

    // Calculate DayOfMonth for Monthly recurrence (use day from start date)
    var dayOfMonthValue = null;
    if (recurrenceType === 'Monthly') {
        try {
            var startDateObj = new Date(startDateISO);
            dayOfMonthValue = startDateObj.getDate();
        } catch (e) {
            dayOfMonthValue = null; // Backend will use startDate.Day
        }
    }

    var companyId = 0;
    var addedBy = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            companyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
            addedBy = parseInt(Cookies.get('data').split('&')[2].split('=')[1]) || 0;
        } catch (e) {
            companyId = 0;
            addedBy = 0;
        }
    }

    var det = {
        RecurringBookingId: recurringBookingId,
        BookingId: bookingId && bookingId !== '' ? parseInt(bookingId) : 0,
        CustomerId: bookingId && bookingId !== '' ? 0 : (customerId ? parseInt(customerId) : 0),
        BookingTimeString: bookingId && bookingId !== '' ? null : bookingTime,
        Duration: bookingId && bookingId !== '' ? 0 : duration,
        NoOfGuests: bookingId && bookingId !== '' ? 0 : noOfGuests,
        BranchId: bookingId && bookingId !== '' ? 0 : (branchId ? parseInt(branchId) : 0),
        FloorId: bookingId && bookingId !== '' ? 0 : floorId,
        TableIds: bookingId && bookingId !== '' ? null : tableIds,
        SpecialRequest: bookingId && bookingId !== '' ? null : specialRequest,
        RecurrenceType: recurrenceType,
        RepeatEveryNumber: interval,
        RepeatEvery: intervalUnit === 'Year' ? intervalUnit : null,
        DayOfMonth: dayOfMonthValue,
        DaysOfWeek: recurrenceType === 'Weekly' ? daysOfWeekArray : null,
        StartDate: startDateISO,
        EndDate: endDateISO,
        IsNeverExpires: isNeverExpires,
        IsActive: isActive,
        CompanyId: companyId,
        AddedBy: addedBy
    };

    $("#divLoading").show();
    var url = recurringBookingId > 0 ? '/recurringbooking/UpdateRecurringBooking' : '/recurringbooking/CreateRecurringBooking';
    
    $.ajax({
        url: url,
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                // Store success message in sessionStorage to show after redirect
                if (typeof sessionStorage !== 'undefined') {
                    sessionStorage.setItem('recurringBookingSuccessMessage', data.Message);
                    sessionStorage.setItem('recurringBookingSuccessType', 'success');
                }
                // Redirect based on actionType
                if (actionType == 2) {
                    // Redirect to Add form for "Save & Add Another" or "Update & Add Another"
                    window.location.href = '/recurringbooking/add';
                } else {
                    // Default: redirect to list
                    window.location.href = '/recurringbooking/index';
                }
            } else if (data.Status == 2) {
                // Backend validation errors - show inline errors with red borders
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    if (document.getElementById('error')) document.getElementById('error').play();
                }
                if (typeof toastr !== 'undefined') toastr.error('Invalid inputs, check and try again !!');
                if (data.Errors && data.Errors.length > 0) {
                    $.each(data.Errors, function (index, value) {
                        $('#' + value.Id).text(value.Message).show();
                        
                        // Add red border to corresponding input field
                        if (value.Id === 'divBranchId') {
                            var branchSelect = $('#ddlBranchId');
                            if (branchSelect.hasClass('select2-hidden-accessible')) {
                                branchSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                            } else {
                                branchSelect.css('border', '2px solid #dc3545');
                            }
                        } else if (value.Id === 'divNoOfGuests') {
                            $('#txtNoOfGuests').css('border', '2px solid #dc3545');
                        } else if (value.Id === 'divBookingTime') {
                            var timeSelect = $('#ddlBookingTimeSlots');
                            if (timeSelect.hasClass('select2-hidden-accessible')) {
                                timeSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                            } else {
                                timeSelect.css('border', '2px solid #dc3545');
                            }
                        } else if (value.Id === 'divCustomer' || value.Id === 'divCustomerId') {
                            var customerSelect = $('#ddlCustomer, #ddlCustomerId');
                            if (customerSelect.hasClass('select2-hidden-accessible')) {
                                customerSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                            } else if (customerSelect.length > 0) {
                                customerSelect.css('border', '2px solid #dc3545');
                            }
                        } else if (value.Id === 'divStartDate') {
                            $('#txtStartDate').css('border', '2px solid #dc3545');
                        } else if (value.Id === 'divInterval') {
                            $('#txtInterval').css('border', '2px solid #dc3545');
                        } else if (value.Id === 'divDaysOfWeek') {
                            // Days of week checkboxes - highlight the container
                            $('#weeklyOptions').css('border', '2px solid #dc3545');
                        } else if (value.Id === 'divDuration') {
                            $('#txtDuration').css('border', '2px solid #dc3545');
                        }
                        // divTableId doesn't have a direct input field (it's a canvas selection)
                    });
                }
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                    if (document.getElementById('error')) document.getElementById('error').play();
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                if (document.getElementById('error')) document.getElementById('error').play();
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred');
        }
    });
}

// Helper function to check if StartDate matches selected days of week (for real-time feedback)
function checkStartDateMatchesDays() {
    // Only check if Weekly recurrence is selected
    if ($('#ddlIntervalUnit').val() !== 'Week') {
        return;
    }
    
    var startDate = $('#txtStartDate').val();
    if (!startDate || startDate.trim() === '') {
        return;
    }
    
    var daysOfWeekArray = [];
    $('.dayOfWeek:checked').each(function () {
        daysOfWeekArray.push(parseInt($(this).val()));
    });
    
    if (daysOfWeekArray.length === 0) {
        return; // No days selected yet
    }
    
    var startDateISO = convertToISOFormat(startDate);
    if (!startDateISO) {
        return;
    }
    
    try {
        var startDateObj = new Date(startDateISO);
        if (!isNaN(startDateObj.getTime())) {
            var startDayOfWeek = startDateObj.getDay();
            
            if (!daysOfWeekArray.includes(startDayOfWeek)) {
                // StartDate doesn't match - show warning
                var dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
                var selectedDayNames = daysOfWeekArray.map(function(d) { 
                    return dayNames[d]; 
                }).join(', ');
                var startDayName = dayNames[startDayOfWeek];
                
                // Format the start date for display
                var formattedStartDate = startDateISO;
                if (typeof moment !== 'undefined') {
                    try {
                        formattedStartDate = moment(startDateISO).format('DD-MMM-YYYY');
                    } catch (e) {
                        // Use ISO format if moment fails
                    }
                }
                
                // Show warning message (non-blocking, just informational)
                $('#divStartDate').html('<i class="fas fa-info-circle text-warning"></i> Start Date (' + startDayName + ') does not match selected days (' + selectedDayNames + '). First booking will be on next matching day.').show();
                $('#txtStartDate').css('border', '1px solid #ffc107');
            } else {
                // StartDate matches - clear warnings
                $('#divStartDate').hide();
                $('#txtStartDate').css('border', '');
            }
        }
    } catch (e) {
        // Date parsing error - ignore
        console.error('Error checking StartDate match:', e);
    }
}

// Helper function to check if StartDate matches selected days of week (for real-time feedback)
function checkStartDateMatchesDays() {
    // Only check if Weekly recurrence is selected
    if ($('#ddlIntervalUnit').val() !== 'Week') {
        return;
    }
    
    var startDate = $('#txtStartDate').val();
    if (!startDate || startDate.trim() === '') {
        return;
    }
    
    var daysOfWeekArray = [];
    $('.dayOfWeek:checked').each(function () {
        daysOfWeekArray.push(parseInt($(this).val()));
    });
    
    if (daysOfWeekArray.length === 0) {
        return; // No days selected yet
    }
    
    var startDateISO = convertToISOFormat(startDate);
    if (!startDateISO) {
        return;
    }
    
    try {
        var startDateObj = new Date(startDateISO);
        if (!isNaN(startDateObj.getTime())) {
            var startDayOfWeek = startDateObj.getDay();
            
            if (!daysOfWeekArray.includes(startDayOfWeek)) {
                // StartDate doesn't match - show warning
                var dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
                var selectedDayNames = daysOfWeekArray.map(function(d) { 
                    return dayNames[d]; 
                }).join(', ');
                var startDayName = dayNames[startDayOfWeek];
                
                // Format the start date for display
                var formattedStartDate = startDateISO;
                if (typeof moment !== 'undefined') {
                    try {
                        formattedStartDate = moment(startDateISO).format('DD-MMM-YYYY');
                    } catch (e) {
                        // Use ISO format if moment fails
                    }
                }
                
                // Show warning message (non-blocking, just informational)
                $('#divStartDate').html('<i class="fas fa-info-circle text-warning"></i> Start Date (' + startDayName + ') does not match selected days (' + selectedDayNames + '). First booking will be on next matching day.').show();
                $('#txtStartDate').css('border', '1px solid #ffc107');
            } else {
                // StartDate matches - clear warnings
                $('#divStartDate').hide();
                $('#txtStartDate').css('border', '');
            }
        }
    } catch (e) {
        // Date parsing error - ignore
        console.error('Error checking StartDate match:', e);
    }
}

// Helper function to convert datetimepicker format to ISO
function convertToISOFormat(dateString) {
    if (!dateString) return null;
    
    // If already in ISO format (YYYY-MM-DD), return as is
    if (/^\d{4}-\d{2}-\d{2}/.test(dateString)) {
        return dateString;
    }
    
    // Try to parse the date string using moment.js if available
    if (typeof moment !== 'undefined') {
        try {
            // Handle common formats like "DD-MMM-YYYY HH:mm A"
            var date = moment(dateString, ['DD-MMM-YYYY HH:mm A', 'DD-MMM-YYYY', 'DD/MM/YYYY HH:mm A', 'DD/MM/YYYY', 'YYYY-MM-DD HH:mm A', 'YYYY-MM-DD']);
            if (date.isValid()) {
                return date.format('YYYY-MM-DD');
            }
        } catch (e) {
            console.error('Date conversion error:', e);
        }
    } else {
        // Fallback: try basic date parsing
        try {
            var date = new Date(dateString);
            if (!isNaN(date.getTime())) {
                var year = date.getFullYear();
                var month = String(date.getMonth() + 1).padStart(2, '0');
                var day = String(date.getDate()).padStart(2, '0');
                return year + '-' + month + '-' + day;
            }
        } catch (e) {
            console.error('Date conversion error:', e);
        }
    }
    
    return dateString;
}

// Customer Modal Functions
function openCustomerModal() {
    $("#CustomerModal").modal('show');
    // Clear form fields
    $('#txtName').val('');
    $('#txtMobileNo').val('');
    $('#txtEmailId').val('');
    $('#txtAltMobileNo').val('');
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
}

function insertCustomer() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        UserType: 'Customer',
        IsActive: true,
        IsDeleted: false,
        Addresses: []
    };

    if (!det.Name || det.Name.trim() === '') {
        $('#divName_M').text('Name is required').show();
        $('.divName_M_ctrl').css('border', '2px solid #dc3545');
        return;
    }

    if (!det.MobileNo || det.MobileNo.trim() === '') {
        $('#divMobileNo_M').text('Mobile number is required').show();
        $('.divMobileNo_M_ctrl').css('border', '2px solid #dc3545');
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/customers/UserInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                if (typeof $('#subscriptionExpiryModal') !== 'undefined') {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                }
                return;
            }
            else if (data == "False") {
                if (typeof $('#subscriptionExpiryModal') !== 'undefined') {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                }
                return;
            }

            if (data.Status == 0) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.error('Invalid inputs, check and try again !!');
                if (data.Errors) {
                    data.Errors.forEach(function (res) {
                        $('#' + res.Id + '_M').show();
                        $('#' + res.Id + '_M').text(res.Message);

                        if ($('.' + res.Id + '_M_ctrl').children("select").length > 0) {
                            var element = $("." + res.Id + '_M_ctrl select');
                            if (element.hasClass("select2-hidden-accessible")) {
                                $('.' + res.Id + '_M_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                            } else {
                                $('.' + res.Id + '_M_ctrl select').css('border', '2px solid #dc3545');
                            }
                        }
                        else {
                            $('.' + res.Id + '_M_ctrl').css('border', '2px solid #dc3545');
                        }
                    });
                }
            }
            else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('success')) document.getElementById('success').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.success('Customer added successfully');
                
                // Add customer to dropdown
                if (data.Data && data.Data.User) {
                    var customerText = data.Data.User.Name;
                    if (data.Data.User.MobileNo && data.Data.User.MobileNo != "") {
                        customerText += ' - ' + data.Data.User.MobileNo;
                    }
                    var newOption = new Option(customerText, data.Data.User.UserId, true, true);
                    $('#ddlCustomerId').append(newOption).trigger('change');
                }
                
                $('#CustomerModal').modal('hide');
                
                // Clear the modal form
                $('#txtName').val('');
                $('#txtMobileNo').val('');
                $('#txtEmailId').val('');
                $('#txtAltMobileNo').val('');
                $('.errorText').hide();
                $('[style*="border: 2px"]').css('border', '');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while adding customer');
        }
    });
}

// Helper function for number input validation
function onlyNumberKey(evt) {
    var ASCIICode = (evt.which) ? evt.which : evt.keyCode
    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        return false;
    return true;
}

