var calendar;
var bookingSettingsCache = null;

// Helper function to parse .NET date format "/Date(1234567890)/" to JavaScript Date
function parseDotNetDate(dateValue) {
    if (!dateValue) return null;
    
    // If it's already a Date object, return it
    if (dateValue instanceof Date) {
        return dateValue;
    }
    
    // If it's a string in .NET format "/Date(1234567890)/"
    if (typeof dateValue === 'string') {
        var match = dateValue.match(/\/Date\((-?\d+)\)\//);
        if (match) {
            var timestamp = parseInt(match[1]);
            // Check if timestamp is valid (not the default .NET min date)
            if (timestamp > -62135596800000 && timestamp < 253402300799000) {
                return new Date(timestamp);
            }
        }
        // Try to parse as ISO string
        var parsed = new Date(dateValue);
        if (!isNaN(parsed.getTime())) {
            return parsed;
        }
    }
    
    return null;
}

function initializeCalendar() {
    var calendarEl = document.getElementById('bookingCalendar');
    calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        events: function (fetchInfo, successCallback, failureCallback) {
            // Get branch filter value - if empty or "All Branches", pass null to load all bookings
            var branchIdVal = $('#ddlBranch').val();
            var branchId = (branchIdVal && branchIdVal !== '' && branchIdVal !== '0') ? parseInt(branchIdVal) : null;
            
            $.ajax({
                url: '/tablebooking/calendarevents',
                data: {
                    start: fetchInfo.startStr,
                    end: fetchInfo.endStr,
                    branchId: branchId
                },
                type: 'get',
                success: function (data) {
                    // The controller returns the events array directly
                    console.log('Calendar events response:', data);
                    
                    var eventsArray = [];
                    
                    if (Array.isArray(data)) {
                        eventsArray = data;
                    } else if (data && data.Status == 1 && data.Data && Array.isArray(data.Data.Events)) {
                        // Handle case where response is wrapped in Status/Data structure
                        eventsArray = data.Data.Events;
                    }
                    
                    if (eventsArray.length > 0) {
                        console.log('Found ' + eventsArray.length + ' events');
                        
                        // Transform events to FullCalendar format (convert PascalCase to camelCase and handle dates)
                        var transformedEvents = eventsArray.map(function(event) {
                            var transformed = {};
                            
                            // Handle id (might be lowercase or uppercase)
                            transformed.id = event.id || event.Id || event.BookingId || null;
                            
                            // Handle title (Title -> title)
                            transformed.title = event.title || event.Title || '';
                            
                            // Handle start date (Start -> start, convert .NET date format to ISO string)
                            var startDate = null;
                            if (event.start) {
                                startDate = parseDotNetDate(event.start) || event.start;
                            } else if (event.Start) {
                                startDate = parseDotNetDate(event.Start) || event.Start;
                            } else if (event.BookingDate && event.BookingTime) {
                                // Construct from BookingDate and BookingTime
                                var bookingDate = parseDotNetDate(event.BookingDate);
                                if (bookingDate) {
                                    var bookingTime = event.BookingTime;
                                    var hours = bookingTime.Hours || 0;
                                    var minutes = bookingTime.Minutes || 0;
                                    startDate = new Date(bookingDate);
                                    startDate.setHours(hours, minutes, 0, 0);
                                }
                            }
                            
                            // Validate start date
                            if (!startDate || !(startDate instanceof Date) || isNaN(startDate.getTime())) {
                                console.warn('Invalid start date for event:', event);
                                return null; // Skip this event
                            }
                            transformed.start = startDate;
                            
                            // Handle end date (End -> end, convert .NET date format to ISO string)
                            var endDate = null;
                            if (event.end) {
                                endDate = parseDotNetDate(event.end) || event.end;
                            } else if (event.End) {
                                endDate = parseDotNetDate(event.End) || event.End;
                            } else if (transformed.start && event.Duration) {
                                // Calculate end from start + duration
                                endDate = new Date(transformed.start);
                                var duration = event.Duration || 60; // default 60 minutes
                                endDate.setMinutes(endDate.getMinutes() + duration);
                            } else if (transformed.start) {
                                // Default to 1 hour if no duration specified
                                endDate = new Date(transformed.start);
                                endDate.setHours(endDate.getHours() + 1);
                            }
                            
                            // Validate end date
                            if (!endDate || !(endDate instanceof Date) || isNaN(endDate.getTime())) {
                                // If end date is invalid but start is valid, default to 1 hour
                                endDate = new Date(transformed.start);
                                endDate.setHours(endDate.getHours() + 1);
                            }
                            transformed.end = endDate;
                            
                            // Handle allDay (AllDay -> allDay)
                            transformed.allDay = event.allDay !== undefined ? event.allDay : (event.AllDay !== undefined ? event.AllDay : false);
                            
                            // Handle color (Color -> color)
                            transformed.color = event.color || event.Color || '#3788d8';
                            
                            // Initialize extendedProps
                            transformed.extendedProps = event.extendedProps || event.ExtendedProps || {};
                            
                            // Copy any other properties that might be useful
                            if (event.BookingId) transformed.extendedProps.bookingId = event.BookingId;
                            if (event.BookingNo) transformed.extendedProps.bookingNo = event.BookingNo;
                            if (event.Status) transformed.extendedProps.status = event.Status;
                            if (event.CustomerName) transformed.extendedProps.customerName = event.CustomerName;
                            if (event.CustomerMobile) transformed.extendedProps.customerMobile = event.CustomerMobile;
                            if (event.NoOfGuests) transformed.extendedProps.noOfGuests = event.NoOfGuests;
                            
                            return transformed;
                        }).filter(function(event) {
                            // Filter out null events (invalid dates)
                            return event !== null;
                        });
                        
                        console.log('Transformed events:', transformedEvents);
                        successCallback(transformedEvents);
                    } else {
                        console.log('No events found or invalid response format');
                        successCallback([]);
                    }
                },
                error: function (ex) {
                    console.error('Error loading calendar events:', ex);
                    failureCallback();
                }
            });
        },
        eventClick: function (info) {
            // Store referrer to know we came from calendar view
            sessionStorage.setItem('tableBookingReferrer', 'calendar');
            window.location.href = '/tablebooking/details/' + info.event.id;
        },
        dateClick: function (info) {
            // Store referrer to know we came from calendar view
            sessionStorage.setItem('tableBookingReferrer', 'calendar');
            window.location.href = '/tablebooking/add?date=' + info.dateStr;
        }
    });
    calendar.render();
}

function fetchList(PageIndex) {
    var branchIdVal = $('#ddlBranch').val();
    var tableIdVal = $('#ddlTable').val();
    var det = {
        PageIndex: PageIndex == undefined ? 1 : PageIndex,
        PageSize: $('#txtPageSize').val() || 10,
        Search: $('#txtSearch').val() || null,
        FromDate: $('#txtFromDate').val() || null,
        ToDate: $('#txtToDate').val() || null,
        Status: $('#ddlStatus').val() || null,
        BranchId: branchIdVal && branchIdVal !== '' ? parseInt(branchIdVal) : null,
        TableId: tableIdVal && tableIdVal !== '' ? parseInt(tableIdVal) : null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/tablebooking/list',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var $response = $(data);
            $("#tblData").html($response.find("#tblData").html());
            
            // Update status counts
            var totalText = $response.find("#divTotalCount").text() || "Total : 0";
            var pendingText = $response.find("#divPendingCount").text() || "Pending : 0";
            var confirmedText = $response.find("#divConfirmedCount").text() || "Confirmed : 0";
            var completedText = $response.find("#divCompletedCount").text() || "Completed : 0";
            var cancelledText = $response.find("#divCancelledCount").text() || "Cancelled : 0";
            
            $("#divTotalCount").text(totalText);
            $("#divPendingCount").text(pendingText);
            $("#divConfirmedCount").text(confirmedText);
            $("#divCompletedCount").text(completedText);
            $("#divCancelledCount").text(cancelledText);
            
            // Update page size dropdown if it exists
            var pageSize = $response.find("#txtPageSize").val();
            if (pageSize && $('#txtPageSize').length > 0) {
                $('#txtPageSize').val(pageSize);
            }
            
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getSelectedTableIds() {
    var selectedTableIds = [];
    $('#bookingTableLayoutCanvas .booking-table-item.selected').each(function() {
        var tableId = $(this).data('table-id');
        if (tableId) {
            selectedTableIds.push(parseInt(tableId));
        }
    });
    return selectedTableIds;
}

/*Customer modal*/
function openCustomerModal() {
    $("#CustomerModal").modal('show');
    if (typeof toggleGstTreatment === 'function') {
        toggleGstTreatment();
    }
    var DateFormat = 'DD-MM-YYYY';
    try {
        if (Cookies.get('BusinessSetting')) {
            DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
        }
    } catch (e) {
        DateFormat = 'DD-MM-YYYY';
    }
    if ($('#_DOB').length > 0) {
        $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
        $('#_DOB').addClass('notranslate');
    }
    if ($('#_JoiningDate').length > 0) {
        $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date() });
        $('#_JoiningDate').addClass('notranslate');
    }
}

function insertCustomer() {
    var DateFormat = 'DD-MM-YYYY';
    try {
        if (Cookies.get('BusinessSetting')) {
            DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
        }
    } catch (e) {
        DateFormat = 'DD-MM-YYYY';
    }
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var Addresses = [];
    Addresses.push({
        Name: $('#txtAddrName').val(),
        MobileNo: $('#txtAddrMobileNo').val(),
        MobileNo2: $('#txtAddrMobileNo2').val(),
        EmailId: $('#txtAddrEmailId').val(),
        Landmark: $('#txtAddrLandmark').val(),
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
    });
    Addresses.push({
        Name: $('#txtAddrAltName').val(),
        MobileNo: $('#txtAddrAltMobileNo').val(),
        MobileNo2: $('#txtAddrAltMobileNo2').val(),
        EmailId: $('#txtAddrAltEmailId').val(),
        Landmark: $('#txtAddrAltLandmark').val(),
        Address: $('#txtAltAddress').val(),
        CountryId: $('#ddlAltCountry').val(),
        StateId: $('#ddlAltState').val(),
        CityId: $('#ddlAltCity').val(),
        Zipcode: $('#txtAltZipcode').val(),
    });
    var det = {
        Username: $('#txtUsername').val() || '',
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        BusinessName: $('#txtBusinessName').val(),
        DOB: $("#txtDOB").val() ? (typeof moment !== 'undefined' ? moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY') : $("#txtDOB").val()) : '',
        JoiningDate: $("#txtJoiningDate").val() ? (typeof moment !== 'undefined' ? moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY') : $("#txtJoiningDate").val()) : '',
        UserGroupId: $('#ddlUserGroup').val() || 0,
        PaymentTermId: $('#ddlPaymentTerm').val() || 0,
        CreditLimit: $('#txtCreditLimit').val() || 0,
        OpeningBalance: $('#txtOpeningBalance').val() || 0,
        PayTerm: ($('#ddlPayTerm').length > 0 ? $('#ddlPayTerm').val() : 0) || 0,
        PayTermNo: ($('#txtPayTermNo').length > 0 ? $('#txtPayTermNo').val() : 0) || 0,
        UserType: 'Customer',
        IsActive: true,
        IsDeleted: false,
        Branchs: ($('#ddlBranch').length > 0 && $('#ddlBranch').val() != null && $('#ddlBranch').val() != '') ? ($('#ddlBranch').val() instanceof Array ? $('#ddlBranch').val() : [$('#ddlBranch').val()]) : [],
        IsShippingAddressDifferent: $('#chkIsShippingAddressDifferent').is(':checked'),
        Addresses: Addresses,
        CurrencyId: $('#ddlUserCurrency').val() || 0,
        TaxPreference: $("#ddlTaxPreference option:selected").length > 0 ? $("#ddlTaxPreference option:selected").text() : '',
        TaxPreferenceId: $('#ddlTaxPreference').val() || 0,
        TaxExemptionId: $('#ddlTaxExemption').val() || 0,
        PlaceOfSupplyId: $('#ddlPlaceOfSupply_M').val() || 0,
        IsBusinessRegistered: ($('#chkIsBusinessRegistered').length > 0 && $('#chkIsBusinessRegistered').is(':checked')) ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val() || '',
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val() || 0,
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val() || '',
        BusinessLegalName: $('#txtBusinessLegalName').val() || '',
        BusinessTradeName: $('#txtBusinessTradeName').val() || '',
        PanNo: $('#txtPanNo').val() || '',
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/UserInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
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
            else {
                var customerText = data.Data.User.Name;
                if (data.Data.User.MobileNo && data.Data.User.MobileNo != "") {
                    customerText += ' - ' + data.Data.User.MobileNo;
                }
                $('#ddlCustomer').append($('<option>', { value: data.Data.User.UserId, text: customerText }));
                $('#ddlCustomer').val(data.Data.User.UserId);
                $('#hdnCustomerId').val(data.Data.User.UserId);
                $('#ddlCustomer').trigger('change');
                $('#CustomerModal').modal('toggle');
                
                // Clear the modal form
                $('#CustomerModal input[type="text"], #CustomerModal input[type="email"], #CustomerModal textarea').val('');
                $('#CustomerModal select').val('0').trigger('change');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function calculateDepositAmount(settings, noOfGuests) {
    console.log('calculateDepositAmount - settings:', settings, 'noOfGuests:', noOfGuests);
    
    if (!settings) {
        console.log('No settings provided');
        return 0;
    }
    
    // Check if deposit is required
    if (!settings.RequireDeposit) {
        console.log('RequireDeposit is false');
        return 0;
    }
    
    // Calculate deposit based on mode (Fixed or PerGuest)
    var depositMode = settings.DepositMode || 'Fixed';
    var depositAmount = 0;
    
    if (depositMode === 'PerGuest') {
        var perGuestAmount = settings.DepositPerGuestAmount || 0;
        depositAmount = noOfGuests * perGuestAmount;
        console.log('Calculated deposit (PerGuest):', depositAmount, '(per guest:', perGuestAmount, 'guests:', noOfGuests + ')');
    } else {
        // Fixed mode
        depositAmount = settings.DepositFixedAmount || 0;
        console.log('Calculated deposit (Fixed):', depositAmount);
    }
    
    var roundedAmount = Math.round(depositAmount * 100) / 100; // Round to 2 decimal places
    return roundedAmount;
}

function updateDepositAmount() {
    var branchId = $('#ddlBranchId').val();
    var noOfGuests = parseInt($('#txtNoOfGuests').val()) || 0;
    
    console.log('updateDepositAmount called - branchId:', branchId, 'noOfGuests:', noOfGuests);
    
    if (!branchId || noOfGuests <= 0) {
        console.log('Missing branchId or noOfGuests <= 0, setting deposit to 0');
        $('#txtDepositAmount').val(0);
        return;
    }
    
    fetchBookingSettings(branchId).done(function (settings) {
        console.log('Settings fetched:', settings);
        var depositAmount = calculateDepositAmount(settings, noOfGuests);
        console.log('Setting deposit amount to:', depositAmount);
        $('#txtDepositAmount').val(depositAmount.toFixed(2));
        
        // Update help text based on settings
        if (settings && settings.RequireDeposit) {
            var depositMode = settings.DepositMode || 'Fixed';
            if (depositMode === 'PerGuest') {
                var perGuestAmount = settings.DepositPerGuestAmount || 0;
                $('#depositHelpText').text('Auto-calculated: ' + perGuestAmount.toFixed(2) + ' per guest (can be edited)');
            } else {
                var fixedAmount = settings.DepositFixedAmount || 0;
                $('#depositHelpText').text('Auto-calculated: Fixed amount ' + fixedAmount.toFixed(2) + ' (can be edited)');
            }
        } else {
            $('#depositHelpText').text('Enable deposit in Restaurant Settings to auto-calculate (can be edited manually)');
        }
    });
}

function saveBooking() {
    // Clear all error messages and borders
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var isValid = true;
    var bookingTimeValue = $('#txtBookingTime').val();
    var selectedTableIds = getSelectedTableIds();
    
    // Validate Branch
    if (!$('#ddlBranchId').val() || $('#ddlBranchId').val() === '') {
        $('#divBranchId').text('Please select a branch').show();
        var branchSelect = $('#ddlBranchId');
        if (branchSelect.hasClass('select2-hidden-accessible')) {
            branchSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
        } else {
            branchSelect.css('border', '2px solid #dc3545');
        }
        isValid = false;
    }
    
    // Validate Booking Date
    if (!$('#txtBookingDate').val()) {
        $('#divBookingDate').text('Please select a booking date').show();
        $('#txtBookingDate').css('border', '2px solid #dc3545');
        isValid = false;
    }
    
    // Validate Number of Guests
    var noOfGuests = parseInt($('#txtNoOfGuests').val()) || 0;
    if (!noOfGuests || noOfGuests <= 0) {
        $('#divNoOfGuests').text('Please enter number of guests').show();
        $('#txtNoOfGuests').css('border', '2px solid #dc3545');
        isValid = false;
    }
    
    // Validate Booking Time
    if (!bookingTimeValue || bookingTimeValue === '') {
        $('#divBookingTime').text('Please select a booking time').show();
        var timeSelect = $('#ddlBookingTimeSlots');
        if (timeSelect.hasClass('select2-hidden-accessible')) {
            timeSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
        } else {
            timeSelect.css('border', '2px solid #dc3545');
        }
        isValid = false;
    }
    
    // Validate Table Selection
    if (selectedTableIds.length === 0) {
        $('#divTableId').text('Please select at least one table').show();
        isValid = false;
    }
    
    // Validate Customer
    var customerId = parseInt($('#ddlCustomer').val()) || 0;
    if (customerId === 0) {
        $('#divCustomer').text('Please select a customer').show();
        var customerSelect = $('#ddlCustomer');
        if (customerSelect.hasClass('select2-hidden-accessible')) {
            customerSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
        } else {
            customerSelect.css('border', '2px solid #dc3545');
        }
        isValid = false;
    }
    
    if (!isValid) {
        if (EnableSound == 'True' && document.getElementById('error')) { document.getElementById('error').play(); }
        toastr.error('Please fill in all required fields correctly');
        return;
    }
    
    // Get deposit amount (DepositPaid is now calculated dynamically from payments)
    var depositAmount = parseFloat($('#txtDepositAmount').val()) || 0;
    
    var det = {
        BookingId: $('#BookingId').val() || 0,
        BranchId: $('#ddlBranchId').val() || null,
        TableIds: selectedTableIds,
        CustomerId: customerId,
        BookingDate: $('#txtBookingDate').val(),
        BookingTimeString: bookingTimeValue,
        Duration: parseInt($('#txtDuration').val()) || 120,
        NoOfGuests: parseInt($('#txtNoOfGuests').val()) || 0,
        Status: $('#ddlStatus').val() || 'Pending',
        BookingType: $('#ddlBookingType').val() || 'WalkIn',
        SpecialRequest: $('#txtSpecialRequest').val() || null,
        DepositAmount: depositAmount,
        WaiterId: parseInt($('#ddlWaiterId').val()) || 0
    };
    $("#divLoading").show();
    $.ajax({
        url: '/tablebooking/createstandalone',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                // Store success message in sessionStorage before redirecting
                sessionStorage.setItem('bookingSuccessMessage', data.Message);
                sessionStorage.setItem('bookingSuccessType', 'success');
                
                // Check if we came from calendar view
                var referrer = sessionStorage.getItem('tableBookingReferrer');
                var redirectUrl = (referrer === 'calendar') ? '/tablebooking/index' : '/tablebooking/list';
                
                // Clear the referrer flag after using it
                sessionStorage.removeItem('tableBookingReferrer');
                
                // Redirect appropriately
                window.location.href = redirectUrl;
            } else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
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
                    } else if (value.Id === 'divBookingDate') {
                        $('#txtBookingDate').css('border', '2px solid #dc3545');
                    } else if (value.Id === 'divNoOfGuests') {
                        $('#txtNoOfGuests').css('border', '2px solid #dc3545');
                    } else if (value.Id === 'divBookingTime') {
                        var timeSelect = $('#ddlBookingTimeSlots');
                        if (timeSelect.hasClass('select2-hidden-accessible')) {
                            timeSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            timeSelect.css('border', '2px solid #dc3545');
                        }
                    } else if (value.Id === 'divCustomer') {
                        var customerSelect = $('#ddlCustomer');
                        if (customerSelect.hasClass('select2-hidden-accessible')) {
                            customerSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            customerSelect.css('border', '2px solid #dc3545');
                        }
                    }
                    // divTableId doesn't have a direct input field (it's a canvas selection)
                });
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred');
        }
    });
}

function loadTablesByBranch() {
    var BranchId = $('#ddlBranchId').val();
    // reset cached settings and time slot selections on branch change
    bookingSettingsCache = null;
    $('#txtBookingTime').val('');
    $('#timeSlotsContainer').empty();
    if (!BranchId) {
        $('#ddlTableId').val('');
        updateTableLayoutSelection();
        $('#timeSlotsContainer').html('<small class="text-muted">Select branch, date, and guests to see time slots.</small>');
        return;
    }
    
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/GetTables',
        datatype: "json",
        data: JSON.stringify({
            PageIndex: 1,
            PageSize: 1000,
            BranchId: BranchId
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Tables) {
                $('#ddlTableId').val('');
                updateTableLayoutSelection();
                loadBookingFloors();
                loadBookingTimeSlots();
            } else {
                $('#ddlTableId').val('');
                updateTableLayoutSelection();
                loadBookingFloors();
                loadBookingTimeSlots();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to load tables');
            $('#ddlTableId').val('');
            updateTableLayoutSelection();
            loadBookingFloors();
            loadBookingTimeSlots();
        }
    });
}

function checkAvailability() {
    var TableId = $('#ddlTableId').val();
    var BookingDate = $('#txtBookingDate').val();
    var BookingTime = $('#txtBookingTime').val();
    var Duration = parseInt($('#txtDuration').val()) || 120;
    
    if (!TableId || !BookingDate || !BookingTime) return;
    
    var startDateTime = new Date(BookingDate + 'T' + BookingTime);
    var endDateTime = new Date(startDateTime.getTime() + Duration * 60000);
    
    $.ajax({
        url: '/tablebooking/checkavailability',
        datatype: "json",
        data: JSON.stringify({
            TableId: TableId,
            StartDateTime: startDateTime.toISOString(),
            EndDateTime: endDateTime.toISOString()
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            if (data.Status == 1) {
                if (!data.Data.IsAvailable) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.warning('Table is not available for the selected date and time');
                }
            }
        }
    });
}

function fetchBookingSettings(branchId) {
    var deferred = $.Deferred();
    var companyId = parseInt($('#hdnCompanyId').val()) || 0;
    if (!branchId || !companyId) {
        deferred.resolve(null);
        return deferred.promise();
    }
    if (bookingSettingsCache && bookingSettingsCache.BranchId == branchId) {
        deferred.resolve(bookingSettingsCache);
        return deferred.promise();
    }
    $("#divLoading").show();
    $.ajax({
        url: '/tablebooking/GetRestaurantSettings',
        datatype: "json",
        data: JSON.stringify({
            BranchId: parseInt(branchId),
            CompanyId: companyId
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.RestaurantSettings) {
                bookingSettingsCache = data.Data.RestaurantSettings;
                deferred.resolve(bookingSettingsCache);
            } else {
                bookingSettingsCache = null;
                toastr.error(data.Message || 'Unable to load booking settings');
                deferred.resolve(null);
            }
        },
        error: function () {
            $("#divLoading").hide();
            bookingSettingsCache = null;
            toastr.error('Failed to load booking settings');
            deferred.resolve(null);
        }
    });
    return deferred.promise();
}

function applyDurationFromSettings(settings) {
    var durationInput = $('#txtDuration');
    var durationFromSettings = settings && settings.DefaultBookingDuration ? parseInt(settings.DefaultBookingDuration) : null;
    var duration = durationFromSettings && durationFromSettings > 0 ? durationFromSettings : (parseInt(durationInput.val()) || 120);
    durationInput.val(duration);
}

function buildBookingTimeSlots(settings, bookingDate) {
    var slots = [];
    
    if (!settings) {
        return slots;
    }
    
    // If booking date is provided, check date overrides and operating days
    if (bookingDate) {
        var dateStr = bookingDate; // Format: YYYY-MM-DD
        // Parse date in local timezone to avoid day-of-week calculation issues
        var dateParts = dateStr.split('-');
        var dayOfWeek = 0; // Default to Sunday
        if (dateParts.length === 3) {
            try {
                var dateObj = new Date(parseInt(dateParts[0]), parseInt(dateParts[1]) - 1, parseInt(dateParts[2]));
                if (!isNaN(dateObj.getTime())) {
                    dayOfWeek = dateObj.getDay(); // 0=Sunday, 1=Monday, etc.
                }
            } catch (e) {
                console.error('Error parsing booking date:', e);
            }
        }
        
        // Step 1: Check for date override first
        var dateOverride = null;
        if (settings.DateOverrides && Array.isArray(settings.DateOverrides)) {
            console.log('[buildBookingTimeSlots] Checking date overrides. Booking date:', bookingDate, 'Total overrides:', settings.DateOverrides.length);
            
            // Helper function to normalize date to YYYY-MM-DD format (using local timezone to avoid date shifts)
            var normalizeDate = function(dateValue) {
                if (!dateValue) return null;
                
                var dateStr = null;
                
                // Handle Microsoft JSON date format: /Date(1767205800000)/
                if (typeof dateValue === 'string' && dateValue.indexOf('/Date(') === 0) {
                    var timestampMatch = dateValue.match(/\/Date\((\d+)\)\//);
                    if (timestampMatch && timestampMatch[1]) {
                        var timestamp = parseInt(timestampMatch[1]);
                        var dateObj = new Date(timestamp);
                        // Use local date methods to avoid timezone conversion issues
                        var year = dateObj.getFullYear();
                        var month = (dateObj.getMonth() + 1).toString().padStart(2, '0');
                        var day = dateObj.getDate().toString().padStart(2, '0');
                        dateStr = year + '-' + month + '-' + day;
                        console.log('[buildBookingTimeSlots] Parsed Microsoft JSON date:', dateValue, '->', dateStr, '(local date)');
                    }
                }
                // Handle Date object
                else if (dateValue instanceof Date) {
                    // Use local date methods to avoid timezone conversion issues
                    var year = dateValue.getFullYear();
                    var month = (dateValue.getMonth() + 1).toString().padStart(2, '0');
                    var day = dateValue.getDate().toString().padStart(2, '0');
                    dateStr = year + '-' + month + '-' + day;
                }
                // Handle string formats
                else if (typeof dateValue === 'string') {
                    dateStr = dateValue;
                    // Handle different date formats: "YYYY-MM-DD", "YYYY-MM-DDTHH:mm:ss", etc.
                    if (dateStr.indexOf('T') > 0) {
                        dateStr = dateStr.split('T')[0];
                    } else if (dateStr.indexOf(' ') > 0) {
                        dateStr = dateStr.split(' ')[0];
                    }
                    dateStr = dateStr.trim();
                }
                // Handle number (timestamp in milliseconds)
                else if (typeof dateValue === 'number') {
                    var dateObj = new Date(dateValue);
                    // Use local date methods to avoid timezone conversion issues
                    var year = dateObj.getFullYear();
                    var month = (dateObj.getMonth() + 1).toString().padStart(2, '0');
                    var day = dateObj.getDate().toString().padStart(2, '0');
                    dateStr = year + '-' + month + '-' + day;
                }
                // Try to convert to string
                else {
                    dateStr = String(dateValue);
                    if (dateStr.indexOf('T') > 0) {
                        dateStr = dateStr.split('T')[0];
                    }
                }
                
                return dateStr;
            };
            
            dateOverride = settings.DateOverrides.find(function(override) {
                var overrideDateStr = normalizeDate(override.OverrideDate);
                var normalizedBookingDate = normalizeDate(dateStr);
                
                console.log('[buildBookingTimeSlots] Comparing - OverrideDate:', overrideDateStr, 'BookingDate:', normalizedBookingDate);
                
                if (!overrideDateStr || !normalizedBookingDate) {
                    return false;
                }
                
                var matches = overrideDateStr === normalizedBookingDate;
                if (matches) {
                    console.log('[buildBookingTimeSlots] Date override MATCH found! OverrideDate:', overrideDateStr, 'BookingDate:', normalizedBookingDate);
                    console.log('[buildBookingTimeSlots] Full override object:', JSON.stringify(override, null, 2));
                }
                return matches;
            });
            
            if (!dateOverride) {
                console.log('[buildBookingTimeSlots] No matching date override found. Available override dates:', 
                    settings.DateOverrides.map(function(o) { 
                        return normalizeDate(o.OverrideDate);
                    }));
            }
        }
        
        // If date has override
        if (dateOverride) {
            // If date is closed, return empty slots
            if (dateOverride.IsClosed) {
                console.log('[buildBookingTimeSlots] Date override is CLOSED - returning empty slots');
                return []; // Empty - restaurant is closed
            }
            // Date is open but has override time slots - use those
            console.log('[buildBookingTimeSlots] Date override found - IsClosed:', dateOverride.IsClosed, 'TimeSlots:', dateOverride.TimeSlots);
            console.log('[buildBookingTimeSlots] TimeSlots type:', typeof dateOverride.TimeSlots, 'IsArray:', Array.isArray(dateOverride.TimeSlots), 'Length:', dateOverride.TimeSlots ? dateOverride.TimeSlots.length : 'N/A');
            
            if (dateOverride.TimeSlots && Array.isArray(dateOverride.TimeSlots) && dateOverride.TimeSlots.length > 0) {
                console.log('[buildBookingTimeSlots] Processing', dateOverride.TimeSlots.length, 'override time slots. First slot:', dateOverride.TimeSlots[0]);
                
                slots = dateOverride.TimeSlots.map(function(slot, index) {
                    // Handle TimeSpan format - might be "HH:mm:ss" or just "HH:mm" or an object with TimeSlot property
                    var timeValue = slot.TimeSlot || slot;
                    var timeStr = '';
                    
                    console.log('[buildBookingTimeSlots] Processing slot', index, ':', slot, 'timeValue:', timeValue, 'type:', typeof timeValue);
                    
                    if (typeof timeValue === 'string') {
                        // Extract HH:mm from "HH:mm:ss" format if needed
                        if (timeValue.indexOf(':') >= 0) {
                            var parts = timeValue.split(':');
                            timeStr = parts[0] + ':' + parts[1];
                        } else {
                            timeStr = timeValue;
                        }
                    } else if (typeof timeValue === 'object' && timeValue !== null) {
                        // If it's an object, try to extract time string
                        // Handle TimeSpan object with Hours and Minutes properties
                        if (timeValue.Hours !== undefined && timeValue.Minutes !== undefined) {
                            var h = parseInt(timeValue.Hours) || 0;
                            var m = parseInt(timeValue.Minutes) || 0;
                            timeStr = (h < 10 ? '0' : '') + h + ':' + (m < 10 ? '0' : '') + m;
                        } else if (timeValue.TotalMinutes !== undefined) {
                            // Handle TimeSpan with TotalMinutes
                            var totalMins = parseInt(timeValue.TotalMinutes) || 0;
                            var h = Math.floor(totalMins / 60);
                            var m = totalMins % 60;
                            timeStr = (h < 10 ? '0' : '') + h + ':' + (m < 10 ? '0' : '') + m;
                        } else {
                            timeStr = timeValue.toString();
                            if (timeStr.indexOf(':') >= 0) {
                                var parts = timeStr.split(':');
                                timeStr = parts[0] + ':' + parts[1];
                            }
                        }
                    } else {
                        timeStr = String(timeValue);
                    }
                    
                    console.log('[buildBookingTimeSlots] Mapped slot', index, 'to:', timeStr);
                    return timeStr;
                }).filter(function(slot) {
                    return slot && slot.length > 0; // Filter out empty slots
                });
                console.log('[buildBookingTimeSlots] Final mapped override slots:', slots);
                // If override has slots, use them
                if (slots.length > 0) {
                    return slots;
                }
            }
            // If date override exists but is open with no time slots configured, return empty array
            // (Don't fall through to regular slots - override takes precedence)
            console.log('[buildBookingTimeSlots] Date override exists but has no time slots - returning empty array');
            return []; // Empty - override exists but no slots configured
        }
        
        // No date override - check operating days
        var isOperatingDay = false;
        if (settings.OperatingDays && Array.isArray(settings.OperatingDays)) {
            isOperatingDay = settings.OperatingDays.some(function(day) {
                return day.DayOfWeek === dayOfWeek;
            });
        }
        
        // If not an operating day, return empty slots
        if (!isOperatingDay) {
            return []; // Empty - restaurant is closed on this day
        }
    }
    
    // It's an operating day (or no date provided) - get regular time slots
    // Check the booking time slot mode
    var bookingMode = (settings.BookingTimeSlotMode || '').trim().toLowerCase();
    
    if (bookingMode === 'manual') {
        // Manual mode - only use configured slots from normalized table
        // Try BookingTimeSlotsList first (array of strings in "HH:mm" format)
        if (settings.BookingTimeSlotsList && Array.isArray(settings.BookingTimeSlotsList) && settings.BookingTimeSlotsList.length > 0) {
            slots = settings.BookingTimeSlotsList.map(function(slot) {
                // Ensure slots are in "HH:mm" format
                if (typeof slot === 'string') {
                    // Extract HH:mm from "HH:mm:ss" format if needed
                    if (slot.indexOf(':') >= 0) {
                        var parts = slot.split(':');
                        return parts[0] + ':' + parts[1];
                    }
                    return slot;
                }
                return String(slot);
            });
        } 
        // Try BookingTimeSlotsNormalized (array of objects with TimeSlot property)
        else if (settings.BookingTimeSlotsNormalized && Array.isArray(settings.BookingTimeSlotsNormalized) && settings.BookingTimeSlotsNormalized.length > 0) {
            slots = settings.BookingTimeSlotsNormalized.map(function(slotObj) {
                var timeSlot = slotObj.TimeSlot || slotObj.timeSlot;
                if (timeSlot) {
                    // Handle TimeSpan format - convert to "HH:mm"
                    if (typeof timeSlot === 'string') {
                        // Extract HH:mm from "HH:mm:ss" format if needed
                        if (timeSlot.indexOf(':') >= 0) {
                            var parts = timeSlot.split(':');
                            return parts[0] + ':' + parts[1];
                        }
                        return timeSlot;
                    } else if (typeof timeSlot === 'object' && timeSlot !== null) {
                        // Handle TimeSpan object
                        if (timeSlot.Hours !== undefined && timeSlot.Minutes !== undefined) {
                            var h = parseInt(timeSlot.Hours) || 0;
                            var m = parseInt(timeSlot.Minutes) || 0;
                            var hStr = (h < 10 ? '0' : '') + h;
                            var mStr = (m < 10 ? '0' : '') + m;
                            return hStr + ':' + mStr;
                        }
                    }
                }
                return null;
            }).filter(function(slot) {
                return slot !== null && slot.length > 0;
            });
        }
        // Fallback to old BookingTimeSlots JSON string format
        else if (settings.BookingTimeSlots) {
            try {
                var parsed = JSON.parse(settings.BookingTimeSlots);
                if (Array.isArray(parsed)) {
                    slots = parsed.map(function(slot) {
                        if (typeof slot === 'string') {
                            if (slot.indexOf(':') >= 0) {
                                var parts = slot.split(':');
                                return parts[0] + ':' + parts[1];
                            }
                            return slot;
                        }
                        return String(slot);
                    });
                }
            } catch (e) {
                console.error('Error parsing BookingTimeSlots:', e);
                // ignore parse errors - return empty slots in Manual mode
            }
        }
        // In Manual mode, return only configured slots (even if empty) - NO FALLBACK
        return slots;
    } else if (bookingMode === 'auto') {
        // Auto mode - generate slots based on start/end time and duration
        var duration = parseInt($('#txtDuration').val()) || (settings && settings.DefaultBookingDuration) || 120;
        if (duration <= 0) duration = 120;
        
        // Only generate if start and end times are configured
        if (settings.BookingStartTime && settings.BookingEndTime) {
            // Parse start and end times (format: "HH:mm" or "HH:mm:ss" or TimeSpan object)
            var parseTime = function(timeValue) {
                if (!timeValue) return null;
                
                // Convert to string if it's not already
                var timeStr = '';
                if (typeof timeValue === 'string') {
                    timeStr = timeValue;
                } else if (typeof timeValue === 'object' && timeValue !== null) {
                    // Handle TimeSpan-like objects (might have Hours, Minutes, or TotalMinutes properties)
                    if (timeValue.Hours !== undefined && timeValue.Minutes !== undefined) {
                        var h = parseInt(timeValue.Hours) || 0;
                        var m = parseInt(timeValue.Minutes) || 0;
                        return h * 60 + m; // Return in minutes
                    } else if (timeValue.TotalMinutes !== undefined) {
                        return parseInt(timeValue.TotalMinutes) || 0;
                    } else {
                        // Try to convert object to string
                        timeStr = String(timeValue);
                    }
                } else {
                    timeStr = String(timeValue);
                }
                
                // Extract time from string (handle formats like "HH:mm", "HH:mm:ss", "HH:mm:ss.fff")
                if (timeStr.indexOf(':') >= 0) {
                    var parts = timeStr.split(':');
                    if (parts.length >= 2) {
                        var hours = parseInt(parts[0]) || 0;
                        var minutes = parseInt(parts[1]) || 0;
                        return hours * 60 + minutes; // Return in minutes
                    }
                }
                return null;
            };
            
            var startMinutes = parseTime(settings.BookingStartTime);
            var endMinutes = parseTime(settings.BookingEndTime);
            
            if (startMinutes !== null && endMinutes !== null && startMinutes < endMinutes) {
                for (var m = startMinutes; m <= endMinutes; m += duration) {
                    var hours = Math.floor(m / 60);
                    var minutes = m % 60;
                    var timeStr = (hours < 10 ? '0' : '') + hours + ':' + (minutes < 10 ? '0' : '') + minutes;
                    slots.push(timeStr);
                }
            }
        }
        // If Auto mode but no start/end times configured, return empty slots (no fallback)
        return slots;
    } else {
        // Mode is null, empty, or invalid - return empty slots (no fallback, no auto-generation)
        return [];
    }
}

function createEmptyBookingTimeDropdown() {
    var container = $('#timeSlotsContainer');
    container.empty();
    var select = $('<select>', {
        id: 'ddlBookingTimeSlots',
        class: 'form-control'
    });
    select.append($('<option>', { value: '', text: 'Select time slot' }));
    select.on('change', function () {
        var value = $(this).val();
        $('#txtBookingTime').val(value);
        if (value) {
            checkAvailability();
            if ($('#ddlBranchId').val() && $('#ddlBookingFloor').val()) {
                loadBookingTableLayout();
            }
        }
    });
    container.append(select);
    $('#txtBookingTime').val('');
}

function renderBookingTimeSlots(slots) {
    var container = $('#timeSlotsContainer');
    container.empty();

    if (!slots || slots.length === 0) {
        // Keep dropdown visible but empty
        createEmptyBookingTimeDropdown();
        return;
    }

    var selectedTime = $('#txtBookingTime').val();
    var select = $('<select>', {
        id: 'ddlBookingTimeSlots',
        class: 'form-control'
    });

    select.append($('<option>', { value: '', text: 'Select time slot' }));

    $.each(slots, function (index, slot) {
        var option = $('<option>', {
            value: slot,
            text: slot
        });

        if (slot === selectedTime) {
            option.prop('selected', true);
        }

        select.append(option);
    });

    select.on('change', function () {
        var value = $(this).val();
        $('#txtBookingTime').val(value);
        if (value) {
            checkAvailability();
            // Reload table layout when time changes to show updated availability
            // Use a small delay to ensure the hidden field is updated
            setTimeout(function() {
                if ($('#ddlBranchId').val() && $('#ddlBookingFloor').val()) {
                    loadBookingTableLayout();
                }
            }, 100);
        }
    });

    container.append(select);
}

function loadBookingTimeSlots() {
    // Try to get date from BookingDate field, fallback to StartDate for recurring bookings
    var bookingDate = $('#txtBookingDate').val();
    
    // If BookingDate doesn't exist (recurring booking form), use StartDate
    if (!bookingDate || bookingDate === '') {
        var startDateValue = $('#txtStartDate').val();
        if (startDateValue) {
            // Convert StartDate format (DD-MMM-YYYY) to ISO format (YYYY-MM-DD) for time slots
            if (typeof convertToISOFormat === 'function') {
                bookingDate = convertToISOFormat(startDateValue);
            } else if (typeof moment !== 'undefined') {
                // Use moment.js if available
                try {
                    var date = moment(startDateValue, ['DD-MMM-YYYY', 'DD/MM/YYYY', 'YYYY-MM-DD'], true);
                    if (date.isValid()) {
                        bookingDate = date.format('YYYY-MM-DD');
                    }
                } catch (e) {
                    console.error('Date conversion error:', e);
                }
            } else {
                // Fallback: try basic date parsing
                try {
                    var date = new Date(startDateValue);
                    if (!isNaN(date.getTime())) {
                        var year = date.getFullYear();
                        var month = String(date.getMonth() + 1).padStart(2, '0');
                        var day = String(date.getDate()).padStart(2, '0');
                        bookingDate = year + '-' + month + '-' + day;
                    }
                } catch (e) {
                    console.error('Date conversion error:', e);
                }
            }
        }
    }
    
    var noOfGuests = parseInt($('#txtNoOfGuests').val()) || 0;
    var branchId = $('#ddlBranchId').val();

    if (!branchId || !bookingDate || noOfGuests <= 0) {
        // Keep dropdown visible but empty
        createEmptyBookingTimeDropdown();
        $('#divBookingTime').hide();
        return;
    }

    // Clear any previous error messages
    $('#divBookingTime').hide();
    
    // Show loading state
    $('#timeSlotsContainer').html('<small class="text-muted">Loading time slots...</small>');

    fetchBookingSettings(branchId).done(function (settings) {
        if (!settings) {
            // Keep dropdown visible but empty
            createEmptyBookingTimeDropdown();
            return;
        }
        
        applyDurationFromSettings(settings);
        var slots = buildBookingTimeSlots(settings, bookingDate);
        
        // Debug logging (can be removed in production)
        console.log('Booking Date:', bookingDate);
        console.log('Booking Mode:', settings.BookingTimeSlotMode);
        console.log('BookingTimeSlotsList:', settings.BookingTimeSlotsList);
        console.log('BookingTimeSlotsNormalized:', settings.BookingTimeSlotsNormalized);
        console.log('Operating Days:', settings.OperatingDays);
        console.log('Date Overrides:', settings.DateOverrides);
        console.log('Generated Slots:', slots);
        
        // Show message if no slots available (restaurant closed)
        if (slots.length === 0 && bookingDate) {
            // Keep dropdown visible but empty
            createEmptyBookingTimeDropdown();
            $('#divBookingTime').text('Restaurant is closed on this date.').show();
            return;
        }
        
        // Clear error message if slots are available
        $('#divBookingTime').hide();
        renderBookingTimeSlots(slots);
    }).fail(function() {
        // Keep dropdown visible but empty
        createEmptyBookingTimeDropdown();
    });
}

function confirmBooking(BookingId) {
    if (confirm('Are you sure you want to confirm this booking?')) {
        var det = { BookingId: BookingId };
        $("#divLoading").show();
        $.ajax({
            url: '/tablebooking/confirmbooking',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    // Check if we're on the details page
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        // Store success message to show after reload
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        // Reload the page to show updated booking status
                        window.location.reload();
                    } else {
                        // On list page, show message and refresh the list
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        fetchList(1);
                    }
                } else {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function cancelBooking(BookingId) {
    var reason = prompt('Please enter cancellation reason:');
    if (reason != null) {
        var det = {
            BookingId: BookingId,
            CancellationReason: reason
        };
        $("#divLoading").show();
        $.ajax({
            url: '/tablebooking/cancelbooking',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    // Check if we're on the details page
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        // Store success message to show after reload
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        // Reload the page to show updated booking status
                        window.location.reload();
                    } else {
                        // On list page, show message and refresh the list
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        fetchList(1);
                    }
                } else {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function completeBooking(BookingId) {
    if (confirm('Are you sure you want to mark this booking as completed? This will make the table(s) available for new bookings.')) {
        var det = { BookingId: BookingId };
        $("#divLoading").show();
        $.ajax({
            url: '/tablebooking/completebooking',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    // Check if we're on the details page
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        // Store success message to show after reload
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        // Reload the page to show updated booking status
                        window.location.reload();
                    } else {
                        // On list page, show message and refresh the list
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        fetchList(1);
                    }
                } else {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function markAsNoShow(BookingId) {
    if (confirm('Are you sure you want to mark this booking as No-Show? This will make the table(s) available for new bookings.')) {
        var det = { BookingId: BookingId };
        $("#divLoading").show();
        $.ajax({
            url: '/tablebooking/markasnoshow',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    // Check if we're on the details page
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        // Store success message to show after reload
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        // Reload the page to show updated booking status
                        window.location.reload();
                    } else {
                        // On list page, show message and refresh the list
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        fetchList(1);
                    }
                } else {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function deleteBooking(BookingId) {
    if (confirm('Are you sure you want to permanently delete this booking? This action cannot be undone.')) {
        var det = { BookingId: BookingId };
        $("#divLoading").show();
        $.ajax({
            url: '/tablebooking/deletebooking',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    // If we're on the details page, redirect back based on where we came from
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        
                        // Check if we came from calendar view
                        var referrer = sessionStorage.getItem('tableBookingReferrer');
                        var redirectUrl = (referrer === 'calendar') ? '/tablebooking/index' : '/tablebooking/list';
                        
                        // Clear the referrer flag after using it
                        sessionStorage.removeItem('tableBookingReferrer');
                        
                        window.location.href = redirectUrl;
                    } else {
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        fetchList(1);
                    }
                } else {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

// Table Layout Functions for Booking
var bookingTableDataCache = {};

function getStatusColor(status) {
    if (!status) return '#28a745'; // Default to green if no status
    
    // Make case-insensitive comparison
    var statusLower = status.toString().toLowerCase();
    
    switch(statusLower) {
        case 'available': return '#28a745'; // Green
        case 'occupied': return '#dc3545'; // Red
        case 'reserved': return '#ffc107'; // Yellow
        case 'booked': return '#6f42c1'; // Purple
        case 'unavailable': return '#ffc107'; // Yellow
        case 'maintenance': return '#6c757d'; // Gray
        default: return '#17a2b8'; // Light blue (default/unknown)
    }
}

function loadBookingFloors() {
    var BranchId = $('#ddlBranchId').val();
    if (!BranchId) {
        $('#ddlBookingFloor').html('<option value="0">All Floors</option>');
        $('#ddlBookingFloor').val('0').trigger('change');
        $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">Please select a branch first</p></div>');
        return;
    }
    
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/getfloors',
        datatype: "json",
        data: JSON.stringify({
            PageIndex: 1,
            PageSize: 1000,
            BranchId: BranchId || 0
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Floors) {
                var options = '<option value="0">All Floors</option>';
                $.each(data.Data.Floors, function(index, item) {
                    if (item.IsActive && !item.IsDeleted) {
                        options += '<option value="' + item.FloorId + '">' + item.FloorName + '</option>';
                    }
                });
                $('#ddlBookingFloor').html(options);
                $('#ddlBookingFloor').val('0').trigger('change');
                // Auto-load layout for first floor if available
                if (data.Data.Floors.length > 0 && data.Data.Floors[0].IsActive && !data.Data.Floors[0].IsDeleted) {
                    $('#ddlBookingFloor').val(data.Data.Floors[0].FloorId).trigger('change');
                }
            } else {
                $('#ddlBookingFloor').html('<option value="0">All Floors</option>');
                $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">No floors available for this branch</p></div>');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            $('#ddlBookingFloor').html('<option value="0">All Floors</option>');
            $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-danger">Error loading floors</p></div>');
        }
    });
}

function loadBookingTableLayout() {
    var BranchId = $('#ddlBranchId').val();
    var FloorId = $('#ddlBookingFloor').val() || 0;
    // Support both regular bookings (txtBookingDate) and recurring bookings (txtStartDate)
    var bookingDate = $('#txtBookingDate').val() || $('#txtStartDate').val();
    // Get booking time from hidden field or from dropdown if not set
    var bookingTime = $('#txtBookingTime').val() || $('#ddlBookingTimeSlots').val();
    
    // If bookingDate is in datetimepicker format (DD-MMM-YYYY), convert to YYYY-MM-DD
    if (bookingDate && !/^\d{4}-\d{2}-\d{2}/.test(bookingDate)) {
        // Try to parse the date format from datetimepicker
        if (typeof moment !== 'undefined') {
            try {
                var DateFormat = 'DD-MMM-YYYY';
                if (typeof Cookies !== 'undefined' && Cookies.get('BusinessSetting')) {
                    try {
                        DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1] || 'DD-MMM-YYYY';
                    } catch (e) {
                        // Use default
                    }
                }
                var parsedDate = moment(bookingDate, [DateFormat.toUpperCase(), 'DD-MMM-YYYY', 'DD/MMM/YYYY', 'DD-MM-YYYY', 'DD/MM/YYYY'], true);
                if (parsedDate.isValid()) {
                    bookingDate = parsedDate.format('YYYY-MM-DD');
                }
            } catch (e) {
                // If parsing fails, try native Date parsing as fallback
                try {
                    var nativeDate = new Date(bookingDate);
                    if (!isNaN(nativeDate.getTime())) {
                        var year = nativeDate.getFullYear();
                        var month = String(nativeDate.getMonth() + 1).padStart(2, '0');
                        var day = String(nativeDate.getDate()).padStart(2, '0');
                        bookingDate = year + '-' + month + '-' + day;
                    }
                } catch (e2) {
                    // If all parsing fails, use empty string
                    bookingDate = '';
                }
            }
        } else {
            // Fallback: try native Date parsing
            try {
                var nativeDate = new Date(bookingDate);
                if (!isNaN(nativeDate.getTime())) {
                    var year = nativeDate.getFullYear();
                    var month = String(nativeDate.getMonth() + 1).padStart(2, '0');
                    var day = String(nativeDate.getDate()).padStart(2, '0');
                    bookingDate = year + '-' + month + '-' + day;
                }
            } catch (e) {
                bookingDate = '';
            }
        }
    }
    
    if (!BranchId) {
        $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">Please select a branch first</p></div>');
        return;
    }
    
    if (FloorId == 0) {
        $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">Please select a floor to view table layout</p></div>');
        return;
    }
    
    $("#divLoading").show();
    // Build ForDateTime from selected booking date and time
    var forDateTime = null;
    if (bookingDate && bookingTime) {
        // Combine date and time to create a DateTime object
        // bookingDate is in format YYYY-MM-DD, bookingTime is in format HH:mm
        // Parse time components
        var timeParts = bookingTime.split(':');
        var hours = parseInt(timeParts[0]) || 0;
        var minutes = parseInt(timeParts[1]) || 0;
        
        // Parse date components
        var dateParts = bookingDate.split('-');
        var year = parseInt(dateParts[0]);
        var month = parseInt(dateParts[1]) - 1; // JavaScript months are 0-based
        var day = parseInt(dateParts[2]);
        
        // Create a Date object representing the desired local time (IST)
        // Then convert to UTC for sending to backend
        // Backend will convert back to local time (IST) when it receives it
        var localDate = new Date(year, month, day, hours, minutes, 0);
        
        // Convert to ISO string - this will be in UTC
        // The backend's ToLocalTime() will convert it back to IST
        forDateTime = localDate.toISOString();
        
        // Debug logging
        console.log('loadBookingTableLayout - ForDateTime:', {
            bookingDate: bookingDate,
            bookingTime: bookingTime,
            'Desired local time (IST)': localDate.toString(),
            'UTC being sent': forDateTime,
            'Local date object': localDate
        });
    } else {
        console.log('loadBookingTableLayout - No ForDateTime (missing date or time):', {
            bookingDate: bookingDate,
            bookingTime: bookingTime
        });
    }
    
    var det = {
        PageIndex: 1,
        PageSize: 1000,
        FloorId: FloorId || null,
        BranchId: BranchId,
        ForDateTime: forDateTime
    };
    
    $.ajax({
        url: '/restauranttable/GetTables',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Tables) {
                // Debug logging
                console.log('loadBookingTableLayout - Tables received:', data.Data.Tables.length);
                if (data.Data.Tables.length > 0) {
                    console.log('loadBookingTableLayout - First table status:', {
                        TableId: data.Data.Tables[0].TableId,
                        TableNo: data.Data.Tables[0].TableNo,
                        Status: data.Data.Tables[0].Status
                    });
                }
                renderBookingTableLayout(data.Data.Tables);
            } else {
                $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">No tables found for this floor</p></div>');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to load table layout');
            $('#bookingTableLayoutCanvas').html('<div class="text-center p-5"><p class="text-danger">Error loading table layout</p></div>');
        }
    });
}

function renderBookingTableLayout(tables) {
    var canvas = $('#bookingTableLayoutCanvas');
    canvas.empty();
    bookingTableDataCache = {};
    
    // Filter tables for selected floor and active tables only
    var floorId = $('#ddlBookingFloor').val() || 0;
    var branchId = $('#ddlBranchId').val();
    var floorTables = tables.filter(function(t) {
        return (floorId == 0 || t.FloorId == floorId) && 
               t.BranchId == branchId &&
               t.IsActive && !t.IsDeleted;
    });
    
    if (floorTables.length === 0) {
        canvas.html('<div class="text-center p-5"><p class="text-muted">No active tables found for this floor</p></div>');
        return;
    }
    
    // Create clickable table elements
    $.each(floorTables, function(index, table) {
        // Store full table data for later use
        bookingTableDataCache[table.TableId] = table;
        
        var posX = table.PositionX || (index % 10) * 120 + 20;
        var posY = table.PositionY || Math.floor(index / 10) * 120 + 20;
        
        // Check if table is available for selection (not booked, occupied, or maintenance)
        var statusLower = (table.Status || '').toString().toLowerCase();
        var isSelectable = statusLower === 'available' || statusLower === 'reserved';
        
        var tableElement = $('<div>', {
            'class': 'booking-table-item',
            'data-table-id': table.TableId,
            'data-table-no': table.TableNo,
            'css': {
                'position': 'absolute',
                'left': posX + 'px',
                'top': posY + 'px',
                'width': '100px',
                'height': '80px',
                'border': '3px solid #333',
                'border-radius': '8px',
                'background-color': getStatusColor(table.Status),
                'color': '#fff',
                'text-align': 'center',
                'display': 'flex',
                'flex-direction': 'column',
                'justify-content': 'center',
                'align-items': 'center',
                'cursor': isSelectable ? 'pointer' : 'not-allowed',
                'z-index': 10,
                'box-shadow': '0 2px 4px rgba(0,0,0,0.2)',
                'transition': 'all 0.3s ease',
                'opacity': isSelectable ? '1' : '0.6'
            }
        });
        
        tableElement.append($('<div>', {
            'text': table.TableNo || 'T' + table.TableId,
            'css': {
                'font-weight': 'bold',
                'font-size': '14px',
                'margin-bottom': '4px'
            }
        }));
        
        if (table.TableName) {
            tableElement.append($('<div>', {
                'text': table.TableName,
                'css': {
                    'font-size': '11px',
                    'opacity': 0.9
                }
            }));
        }
        
        tableElement.append($('<div>', {
            'text': 'Capacity: ' + (table.Capacity || 0),
            'css': {
                'font-size': '10px',
                'opacity': 0.8,
                'margin-top': '4px'
            }
        }));
        
        tableElement.attr('title', table.TableNo + (table.TableName ? ' - ' + table.TableName : '') + ' (Capacity: ' + table.Capacity + ') - Status: ' + table.Status);
        
        if (!isSelectable) {
            // Make non-selectable tables visually distinct and non-clickable
            tableElement.addClass('unavailable');
        } else {
            // Add click handler - support multi-select with Ctrl/Cmd key
            tableElement.on('click', function(e) {
                var tableId = $(this).data('table-id');
                var isCtrlClick = e.ctrlKey || e.metaKey;
                
                if (isCtrlClick || e.shiftKey) {
                    // Multi-select mode - toggle selection
                    $(this).toggleClass('selected');
                    if (!$(this).hasClass('selected')) {
                        // If unselected, also clear from hidden field if it was the only one
                        if ($('#ddlTableId').val() == tableId) {
                            $('#ddlTableId').val('');
                        }
                    }
                } else {
                    // Single-select mode - clear all and select this one
                    $('#bookingTableLayoutCanvas .booking-table-item').removeClass('selected');
                    $(this).addClass('selected');
                    $('#ddlTableId').val(tableId);
                }
                
                updateTableLayoutSelection();
                updateSelectedTablesDisplay();
                checkAvailability();
            });
        }
        
        canvas.append(tableElement);
    });
    
    // Highlight selected table
    updateTableLayoutSelection();
}

function updateTableLayoutSelection() {
    var selectedTableIds = getSelectedTableIds();
    var singleTableId = $('#ddlTableId').val();
    
    // Remove previous selection highlight
    $('#bookingTableLayoutCanvas .booking-table-item').each(function() {
        if (!$(this).hasClass('selected')) {
            $(this).css({
                'border': '3px solid #333',
                'transform': 'scale(1)',
                'box-shadow': '0 2px 4px rgba(0,0,0,0.2)'
            });
        }
    });
    
    // Highlight selected tables
    if (selectedTableIds.length > 0) {
        selectedTableIds.forEach(function(tableId) {
            var selectedTable = $('#bookingTableLayoutCanvas .booking-table-item[data-table-id="' + tableId + '"]');
            if (selectedTable.length > 0) {
                selectedTable.css({
                    'border': '4px solid #007bff',
                    'transform': 'scale(1.1)',
                    'box-shadow': '0 4px 8px rgba(0,123,255,0.5)',
                    'z-index': 20
                }).addClass('selected');
            }
        });
    } else if (singleTableId) {
        // Fallback to single selection
        var selectedTable = $('#bookingTableLayoutCanvas .booking-table-item[data-table-id="' + singleTableId + '"]');
        if (selectedTable.length > 0) {
            selectedTable.css({
                'border': '4px solid #007bff',
                'transform': 'scale(1.1)',
                'box-shadow': '0 4px 8px rgba(0,123,255,0.5)',
                'z-index': 20
            });
        }
    }
}

function updateSelectedTablesDisplay() {
    var selectedTableIds = getSelectedTableIds();
    var displayDiv = $('#selectedTablesDisplay');
    
    // Clear table selection error when tables are selected
    if (selectedTableIds.length > 0) {
        $('#divTableId').hide();
    }
    
    // Update hidden input with all selected table IDs as comma-separated string
    if ($('#ddlTableId').length > 0) {
        if (selectedTableIds.length > 0) {
            $('#ddlTableId').val(selectedTableIds.join(','));
        } else {
            $('#ddlTableId').val('');
        }
    }
    
    if (displayDiv.length === 0) {
        // Create display div if it doesn't exist
        displayDiv = $('<div id="selectedTablesDisplay" class="mt-2 mb-2"></div>');
        $('#bookingTableLayoutCanvas').after(displayDiv);
    }
    
    if (selectedTableIds.length > 0) {
        var tableNames = [];
        selectedTableIds.forEach(function(tableId) {
            var table = $('#bookingTableLayoutCanvas .booking-table-item[data-table-id="' + tableId + '"]');
            if (table.length > 0) {
                var tableNo = table.data('table-no') || 'T' + tableId;
                tableNames.push(tableNo);
            }
        });
        
        var totalCapacity = 0;
        selectedTableIds.forEach(function(tableId) {
            if (bookingTableDataCache[tableId]) {
                totalCapacity += bookingTableDataCache[tableId].Capacity || 0;
            }
        });
        
        var noOfGuests = parseInt($('#txtNoOfGuests').val()) || 0;
        var capacityWarning = '';
        if (noOfGuests > 0 && totalCapacity > 0) {
            if (noOfGuests > totalCapacity) {
                var overflowPercent = ((noOfGuests - totalCapacity) / totalCapacity * 100).toFixed(0);
                if (noOfGuests > totalCapacity * 1.20) {
                    capacityWarning = ' <span class="text-danger"><i class="fas fa-exclamation-triangle"></i> Capacity exceeded by ' + overflowPercent + '%</span>';
                } else {
                    capacityWarning = ' <span class="text-warning"><i class="fas fa-info-circle"></i> Capacity exceeded by ' + overflowPercent + '%</span>';
                }
            }
        }
        
        displayDiv.html(
            '<div class="alert alert-info mb-0">' +
            '<strong>Selected Tables:</strong> ' + tableNames.join(', ') +
            ' (' + selectedTableIds.length + ' table' + (selectedTableIds.length > 1 ? 's' : '') + ')' +
            (totalCapacity > 0 ? ' | <strong>Total Capacity:</strong> ' + totalCapacity + ' guests' : '') +
            capacityWarning +
            ' <button type="button" class="btn btn-sm btn-outline-secondary ml-2" onclick="clearTableSelection()"><i class="fas fa-times"></i> Clear</button>' +
            '</div>'
        );
    } else {
        displayDiv.html('');
    }
}

function clearTableSelection() {
    $('#bookingTableLayoutCanvas .booking-table-item').removeClass('selected');
    $('#ddlTableId').val('');
    updateTableLayoutSelection();
    updateSelectedTablesDisplay();
}

// Override loadTablesByBranch to also load floors and layout
var originalLoadTablesByBranch = loadTablesByBranch;
loadTablesByBranch = function() {
    originalLoadTablesByBranch();
    if ($('#ddlBranchId').val()) {
        loadBookingFloors();
    }
};

// Helper functions for Customer Modal
function fetchActiveStates(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlCountry').val();
    }
    else {
        id = $('#ddlAltCountry').val();
    }
    var det = {
        CountryId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_States_Dropdown").html(data);
            }
            else {
                $("#p_Alt_States_Dropdown").html(data);
            }
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveCitys(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlState').val();
    }
    else {
        id = $('#ddlAltState').val();
    }
    var det = {
        StateId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_Citys_Dropdown").html(data);
            }
            else {
                $("#p_Alt_Citys_Dropdown").html(data);
            }
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleShippingAddress() {
    if ($('#chkIsShippingAddressDifferent').is(':checked')) {
        $('#divShippingAddress').show();
    }
    else {
        $('#divShippingAddress').hide();
    }
}

function toggleTaxPreference() {
    $('.divNonTaxable').hide();
    $('#ddlTaxExemption').val(0);

    if ($('#ddlTaxPreference option:selected').text() == 'Non-Taxable') {
        $('.divNonTaxable').show();
    }

    $('.select2').select2();
}

function toggleGstTreatment() {
    if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)"
        || $('#ddlGstTreatment').val() == "Composition Taxable Supply" ||
        $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
        || $('#ddlGstTreatment').val() == "Supply by SEZ Developer" || $('#ddlGstTreatment').val() == "Tax Deductor") {
        $('.divGst').show();
        $('.divPlaceOfSupply_M').show();
        $('.divTaxPreference').show();
    }
    else if ($('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)") {
        $('.divGst').hide();
        $('.divPlaceOfSupply_M').hide();
        $('.divTaxPreference').hide();
        $('.divNonTaxable').hide();
    }
    else {
        $('.divGst').hide();
        $('.divPlaceOfSupply_M').show();
        $('.divTaxPreference').show();
    }
}

function toggleBusinessRegistered() {
    if ($('#chkIsBusinessRegistered').is(':checked')) {
        $('.divBusinessRegistered').show();
    }
    else {
        $('.divBusinessRegistered').hide();
    }
}

function setBusinessRegistrationName() {
    if ($('#ddlBusinessRegistrationName option:selected').text()) {
        $('.lblBusinessRegistrationName').text($('#ddlBusinessRegistrationName option:selected').text());
    }
}

// Booking Payment Functions
var BookingPaymentAttachDocument = "";
var BookingPaymentFileExtensionAttachDocument = "";
var _BookingId = 0;

function openBookingPaymentModal(type, BookingId, title) {
    _BookingId = BookingId;
    var det = {
        BookingId: BookingId,
        Type: title || "Booking Payment",
        Title: title || "Booking Payment"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/BookingPayments',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divBookingPayments").html(data);
            $("#bookingPaymentModal").modal('show');
            if (type == true) {
                $('.paymentAdd').show();
                $('.paymentList').hide();
                $('#bookingPaymentModalLabel').text('Add Payment');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
                $('#_PaymentDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });
            }
            else {
                $('.paymentAdd').hide();
                $('.paymentList').show();
                $('#bookingPaymentModalLabel').text('View Payments');
            }

            $('.select2').select2({
                dropdownParent: $('#bookingPaymentModal')
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertBookingPayment() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        BookingId: _BookingId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: BookingPaymentAttachDocument,
        FileExtensionAttachDocument: BookingPaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        ReferenceNo: $('#txtMReferenceNo').val(),
        PaymentTransactionId: $('#txtPaymentTransactionId').val(),
        PaymentGatewayType: $('#ddlPaymentGatewayType').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/PaymentInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
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
                // Check if we're on the details page
                if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                    // Store success message to show after reload (don't show toastr now)
                    sessionStorage.setItem('bookingSuccessMessage', data.Message);
                    sessionStorage.setItem('bookingSuccessType', 'success');
                    $('#bookingPaymentModal').modal('hide');
                    // Reload the page to show updated payment list
                    window.location.reload();
                } else {
                    // On list page or other context, show message and refresh
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    $('#bookingPaymentModal').modal('hide');
                    if (window.location.pathname.toLowerCase().indexOf('list') > -1) {
                        fetchList(1);
                    } else {
                        window.location.reload();
                    }
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred');
        }
    });
}

function deleteBookingPayment(BookingPaymentId) {
    var r = confirm("Are you sure you want to delete this payment?");
    if (r == true) {
        var det = {
            BookingPaymentId: BookingPaymentId
        };
        $("#divLoading").show();
        $.ajax({
            url: '/TableBooking/PaymentDelete',
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
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    // Check if we're on the details page
                    if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                        // Store success message to show after reload (don't show toastr now)
                        sessionStorage.setItem('bookingSuccessMessage', data.Message);
                        sessionStorage.setItem('bookingSuccessType', 'success');
                        // Reload the page to show updated payment list
                        window.location.reload();
                    } else {
                        // In modal or other context, show message and refresh payment list
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        $('#tr_' + BookingPaymentId).remove();
                        openBookingPaymentModal(false, _BookingId, "Booking Payment");
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function applyCreditsToBooking(BookingId, CustomerId, DepositAmount, Due, BookingNo) {
    openBookingAvailableCredits(BookingId, CustomerId, DepositAmount, Due, BookingNo);
}

function openBookingAvailableCredits(BookingId, CustomerId, DepositAmount, Due, BookingNo) {
    $("#bookingPaymentModal").modal('hide');
    _BookingId = BookingId;
    _CustomerId = CustomerId;

    var det = {
        BookingId: BookingId,
        CustomerId: CustomerId,
        BookingNo: BookingNo,
        DepositAmount: DepositAmount,
        Due: Due
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/AvailableCredits',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divAvailableCredits").html(data);
            $('.chkIsActive').bootstrapToggle();
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

            // Initialize datetimepicker for PaymentDate after modal is shown
            // Remove any existing handler first to avoid duplicates, then attach new one
            $('#AvailableCreditsModal').off('shown.bs.modal.creditsDatepicker');
            $('#AvailableCreditsModal').on('shown.bs.modal.creditsDatepicker', function () {
                // Use setTimeout to ensure DOM is fully ready
                setTimeout(function() {
                    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                    // Scope selector to AvailableCreditsModal to avoid conflicts with duplicate IDs in other modals
                    var $paymentDate = $('#AvailableCreditsModal #_CreditsAppliedDate');
                    
                    // Destroy existing datetimepicker instance if it exists
                    if ($paymentDate.length > 0 && $paymentDate.data('DateTimePicker')) {
                        try {
                            $paymentDate.data('DateTimePicker').destroy();
                            $paymentDate.removeData('DateTimePicker');
                        } catch (e) {
                            // Ignore errors if destroy fails
                        }
                    }
                    
                    // Initialize datetimepicker on the container div
                    if ($paymentDate.length > 0) {
                        $paymentDate.datetimepicker({ 
                            widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, 
                            format: DateFormat.toUpperCase() + ' ' + TimeFormat, 
                            defaultDate: new Date() 
                        });
                        $paymentDate.addClass('notranslate');
                    }
                }, 100);
            });

            $('#AvailableCreditsModal').modal('toggle');
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateBookingCreditsTotal() {
    var subTotal = 0, total = 0;
    // Scope selector to AvailableCreditsModal to avoid conflicts with duplicate IDs in other modals
    var Balance = $('#AvailableCreditsModal #hdnAmountRemaining').val();
    var CurrencySymbol = Cookies.get('data') ? Cookies.get('data').split('&')[5].split('=')[1] : '';

    $('#AvailableCreditsModal #divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#AvailableCreditsModal #txtAmount' + _id).val() != '' && $('#AvailableCreditsModal #txtAmount' + _id).val() != undefined) {
            var AmountExcTax = $('#AvailableCreditsModal #txtAmount' + _id).val();

            subTotal = subTotal + parseFloat(AmountExcTax);
        }
    });

    $('#AvailableCreditsModal #divAmountToCredit').text(CurrencySymbol + subTotal.toFixed(2));
    $('#AvailableCreditsModal #divRemainingCredits').text(CurrencySymbol + (Balance - subTotal).toFixed(2));
}

function ApplyCreditsToBooking() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var CustomerPaymentIds = [];
    // Scope selector to AvailableCreditsModal to avoid conflicts with duplicate IDs in other modals
    $('#AvailableCreditsModal #divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#AvailableCreditsModal #txtAmount' + _id).val() != '') {
            CustomerPaymentIds.push({
                Type: 'Booking Deposit Payment',
                CustomerPaymentId: $('#AvailableCreditsModal #hdnCustomerPaymentId' + _id).val(),
                AmountRemaining: $('#AvailableCreditsModal #hdnAmountRemaining' + _id).val(),
                Amount: $('#AvailableCreditsModal #txtAmount' + _id).val(),
                AccountId: $('#AvailableCreditsModal #hdnAccountId' + _id).val(),
                IsActive: true,
                IsDeleted: false,
            })
        }
    });

    var det = {
        PaymentType: 'Advance',
        IsActive: true,
        IsDeleted: false,
        // Scope selectors to AvailableCreditsModal to avoid conflicts with duplicate IDs in other modals
        PaymentDate: moment($("#AvailableCreditsModal #txtCreditsAppliedDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        BookingId: _BookingId,
        CustomerId: $('#AvailableCreditsModal #hdnCustomerId').val(),
        BranchId: $('#AvailableCreditsModal #hdnBranchId').val(),
        Type: "Booking Deposit Payment",
        CustomerPaymentIds: CustomerPaymentIds
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/PaymentInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);


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
                // Store success message to show after reload (don't show toastr now)
                sessionStorage.setItem('bookingSuccessMessage', data.Message);
                sessionStorage.setItem('bookingSuccessType', 'success');
                // Close the credits modal
                $("#AvailableCreditsModal").modal('hide');
                // Check if we're on the details page
                if (window.location.pathname.indexOf('/tablebooking/details/') !== -1) {
                    // Reload the page to show updated payment list
                    window.location.reload();
                } else {
                    // On list page or other context, refresh
                    if (window.location.pathname.toLowerCase().indexOf('list') > -1) {
                        fetchList(1);
                    } else {
                        window.location.reload();
                    }
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function checkPaymentType() {
    if ($('#ddlPaymentType').find(":selected").text() == "Advance") {
        $('.divReferenceNo').hide();
        $('#txtReferenceNo').val('');
        $('#txtMReferenceNo').val('');
    }
    else {
        $('.divReferenceNo').show();
    }
}

function getBookingPaymentAttachDocumentBase64() {
    var file = document.getElementById("PaymentAttachDocument").files[0];
    if (file) {
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function () {
            BookingPaymentAttachDocument = reader.result;
            BookingPaymentFileExtensionAttachDocument = file.name.split('.').pop();
            $('#blahBookingPaymentAttachDocument').attr('src', BookingPaymentAttachDocument);
        };
        reader.onerror = function (error) {
            console.log('Error: ', error);
        };
    }
}

// Booking Refund Functions
var _RefundOfPaymentId = 0;

function openBookingRefundModal(type, BookingPaymentId, title) {
    _RefundOfPaymentId = BookingPaymentId;
    var det = {
        RefundOfPaymentId: BookingPaymentId,
        Type: title || "Booking Refund",
        Title: title || "Booking Refund"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/BookingRefunds',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divBookingRefunds").html(data);
            $("#bookingRefundModal").modal('show');
            if (type == true) {
                $('.refundAdd').show();
                $('.refundList').hide();
                $('#bookingRefundModalLabel').text('Add Refund');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_RefundDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
                $('#_RefundDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });
            }
            else {
                $('.refundAdd').hide();
                $('.refundList').show();
                $('#bookingRefundModalLabel').text('View Refunds');
            }

            $('.select2').select2({
                dropdownParent: $('#bookingRefundModal')
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertBookingRefund() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        RefundOfPaymentId: _RefundOfPaymentId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtRefundNotes').val(),
        Amount: $('#txtRefundAmount').val(),
        PaymentDate: moment($("#txtRefundDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        PaymentTypeId: $('#ddlRefundPaymentType').val(),
        AttachDocument: BookingPaymentAttachDocument,
        FileExtensionAttachDocument: BookingPaymentFileExtensionAttachDocument,
        AccountId: $('#ddlRefundAccount').val(),
        ReferenceNo: $('#txtRefundReferenceNo').val(),
        PaymentTransactionId: $('#txtRefundTransactionId').val(),
        PaymentGatewayType: $('#ddlRefundGatewayType').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/TableBooking/RefundInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                $('#bookingRefundModal').modal('hide');
                // Refresh payment list
                openBookingPaymentModal(false, _BookingId, "Booking Payment");
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred');
        }
    });
}

function deleteBookingRefund(BookingPaymentId) {
    var r = confirm("Are you sure you want to delete this refund?");
    if (r == true) {
        var det = {
            BookingPaymentId: BookingPaymentId
        };
        $("#divLoading").show();
        $.ajax({
            url: '/TableBooking/RefundDelete',
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
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    $('#tr_' + BookingPaymentId).remove();
                    // Refresh refund list
                    openBookingRefundModal(false, _RefundOfPaymentId, "Booking Refund");
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('An error occurred');
            }
        });
    }
}

function getBookingRefundAttachDocumentBase64() {
    var file = document.getElementById("RefundAttachDocument").files[0];
    if (file) {
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function () {
            BookingPaymentAttachDocument = reader.result;
            BookingPaymentFileExtensionAttachDocument = file.name.split('.').pop();
            $('#blahBookingRefundAttachDocument').attr('src', BookingPaymentAttachDocument);
        };
        reader.onerror = function (error) {
            console.log('Error: ', error);
        };
    }
}

function setName() {
    $('#txtAddrName').val($('#txtName').val());
}

function setMobile() {
    $('#txtAddrMobileNo').val($('#txtMobileNo').val());
}

function setAlternativeMobile() {
    $('#txtAddrMobileNo2').val($('#txtAltMobileNo').val());
}

function setEmail() {
    $('#txtAddrEmailId').val($('#txtEmailId').val());
}


