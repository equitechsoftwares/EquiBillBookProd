/**
 * Public Booking JavaScript
 * Handles the public table booking form functionality
 */

var publicBookingConfig = null;

// Generic public booking initializer (branch-level, no pre-selected table)
function initializePublicBooking(config) {
    config.initialTableId = 0;
    config.initialFloorId = 0;
    config.lockTableSelection = false;

    publicBookingConfig = config;
    
    // Set minimum date to today
    var today = new Date().toISOString().split('T')[0];
    $('#bookingDate').attr('min', today);
    
    // Set maximum date based on advance days
    var maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + config.advanceDays);
    $('#bookingDate').attr('max', maxDate.toISOString().split('T')[0]);
    
    // Event handlers
    $('#bookingDate').on('change', function() {
        checkAvailability();
    });
    
    $('#noOfGuests').on('change', function() {
        checkAvailability();
        // Re-evaluate capacity warning when guest count changes
        updateSelectedTablesDisplay();
        // Update deposit display if deposit is per guest
        updateDepositForGuestChange();
    });
    
    $('#bookingForm').on('submit', function(e) {
        e.preventDefault();
        submitBooking();
    });
    
    // Only allow numbers in mobile field
    $('#customerMobile').on('keypress', function(e) {
        if (!/[0-9]/.test(e.key) && !['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
            e.preventDefault();
        }
    });
    
    // Initialize deposit display if deposit is required
    if (config.requireDeposit) {
        updateDepositForGuestChange();
    }
}

// Entry point used by generic /book/{slug}
function initializePublicBookingForBranch(config) {
    initializePublicBooking(config || {});
}

function checkAvailability() {
    var bookingDate = $('#bookingDate').val();
    var noOfGuests = parseInt($('#noOfGuests').val());
    
    if (!bookingDate || !noOfGuests || noOfGuests <= 0) {
        $('#timeSlotGroup').hide();
        $('#floorGroup').hide();
        $('#tableGroup').hide();
        return;
    }
    
    // Show loading
    showLoading();
    
    // First, get available time slots
    $.ajax({
        url: '/api/PublicTableBooking/GetAvailableTimeSlots',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            Slug: publicBookingConfig.slug,
            BookingDate: bookingDate,
            Duration: publicBookingConfig.defaultDuration,
            NoOfGuests: noOfGuests
        }),
        success: function(response) {
            if (response.Status == 1 && response.Data && response.Data.TimeSlots) {
                displayTimeSlots(response.Data.TimeSlots);
            } else {
                toastr.error(response.Message || 'Unable to load time slots');
                hideLoading();
            }
        },
        error: function() {
            toastr.error('An error occurred while checking availability');
            hideLoading();
        }
    });
}

function displayTimeSlots(timeSlots) {
    var container = $('#timeSlotsContainer');
    container.empty();
    
    // Clear the booking time when time slots are refreshed (e.g., date changed)
    $('#bookingTime').val('');
    $('#tableGroup').hide();
    
    if (timeSlots.length === 0) {
        container.html('<p class="text-muted">No available time slots for this date.</p>');
        $('#timeSlotGroup').show();
        $('#floorGroup').hide();
        $('#tableGroup').hide();
        hideLoading();
        return;
    }
    
    timeSlots.forEach(function(slot) {
        // Ensure TimeValue is a string (in case API returns it in a different format)
        var timeValue = typeof slot.TimeValue === 'string' ? slot.TimeValue : String(slot.TimeValue || '');
        
        var btn = $('<button>')
            .addClass('time-slot-btn')
            .attr('type', 'button')
            .attr('data-time', timeValue) // Use attr instead of data() to store as plain string
            .text(slot.Time);
        
        if (!slot.IsAvailable) {
            btn.addClass('unavailable')
                .prop('disabled', true)
                .css({
                    'opacity': '0.4',
                    'background-color': '#f5f5f5',
                    'color': '#999',
                    'border-color': '#ddd',
                    'cursor': 'not-allowed',
                    'pointer-events': 'none'
                });
        }
        
        btn.on('click', function(e) {
            // Prevent any action if button is disabled or unavailable
            if ($(this).hasClass('unavailable') || $(this).prop('disabled')) {
                e.preventDefault();
                e.stopPropagation();
                toastr.warning('This time slot is not available');
                return false;
            }
            
            $('.time-slot-btn').removeClass('selected');
            $(this).addClass('selected');
            // Retrieve as attribute to ensure we get the string value
            var selectedTime = $(this).attr('data-time') || '';
            $('#bookingTime').val(selectedTime);
            loadAvailableTables();
        });
        
        container.append(btn);
    });
    
    $('#timeSlotGroup').show();
    hideLoading();
}

function loadAvailableTables() {
    var bookingDate = $('#bookingDate').val();
    var bookingTime = $('#bookingTime').val();
    var noOfGuests = parseInt($('#noOfGuests').val());
    
    if (!bookingDate || !bookingTime || !noOfGuests) {
        return;
    }
    
    showLoading();
    
    $.ajax({
        url: '/api/PublicTableBooking/GetAvailableTables',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            Slug: publicBookingConfig.slug,
            BookingDate: bookingDate,
            BookingTimeString: bookingTime,
            NoOfGuests: noOfGuests,
            Duration: publicBookingConfig.defaultDuration
        }),
        success: function(response) {
            if (response.Status == 1 && response.Data && response.Data.AvailableTables) {
                displayTables(response.Data.AvailableTables);
            } else {
                toastr.error(response.Message || 'Unable to load available tables');
                $('#tableGroup').hide();
                hideLoading();
            }
        },
        error: function() {
            toastr.error('An error occurred while loading tables');
            $('#tableGroup').hide();
            hideLoading();
        }
    });
}

var publicTableDataCache = {};
var isListView = false;

function toggleTableView() {
    isListView = !isListView;
    if (isListView) {
        $('#tableLayoutCard').hide();
        $('#tablesContainer').show();
        $('#toggleViewBtn').html('<i class="fas fa-th"></i>').attr('title', 'Show Layout View');
    } else {
        $('#tableLayoutCard').show();
        $('#tablesContainer').hide();
        $('#toggleViewBtn').html('<i class="fas fa-list"></i>').attr('title', 'Show List View');
        renderPublicTableLayout();
    }
}

function getPublicStatusColor(status) {
    if (!status) return '#28a745'; // Default to green if no status
    
    // Make case-insensitive comparison
    var statusLower = status.toString().toLowerCase();
    
    switch(statusLower) {
        case 'available': return '#28a745'; // Green
        case 'occupied': return '#dc3545'; // Red
        case 'reserved': return '#ffc107'; // Yellow
        case 'booked': return '#6f42c1'; // Purple
        case 'maintenance': return '#6c757d'; // Gray
        default: 
            console.warn('Unknown table status: ' + status);
            return '#17a2b8'; // Light blue (default/unknown)
    }
}

function renderPublicTableLayout() {
    var canvas = $('#publicTableLayoutCanvas');
    if (!canvas.length) {
        console.error('Canvas element not found');
        return;
    }
    
    canvas.empty();
    
    if (Object.keys(publicTableDataCache).length === 0) {
        canvas.html('<div class="text-center p-5"><p class="text-muted">No tables available</p></div>');
        return;
    }
    
    // Group tables by floor
    var floors = {};
    $.each(publicTableDataCache, function(tableId, table) {
        var floorId = table.FloorId || 0;
        if (!floors[floorId]) {
            floors[floorId] = [];
        }
        floors[floorId].push(table);
    });
    
    // Populate floor dropdown
    var floorSelect = $('#publicBookingFloor');
    var currentFloor = floorSelect.val() || '0';
    floorSelect.empty().append('<option value="0">All Floors</option>');
    $.each(floors, function(floorId, floorTables) {
        if (floorId != 0 && floorTables.length > 0) {
            var floorName = floorTables[0].FloorName || 'Floor ' + floorId;
            floorSelect.append('<option value="' + floorId + '">' + floorName + '</option>');
        }
    });
    floorSelect.val(currentFloor);

    // If an initial floor is specified (direct table booking), select it
    if (publicBookingConfig && publicBookingConfig.initialFloorId && publicBookingConfig.initialFloorId > 0) {
        floorSelect.val(publicBookingConfig.initialFloorId.toString());
    }

    // Lock floor selection if required
    if (publicBookingConfig && publicBookingConfig.lockTableSelection && publicBookingConfig.initialFloorId > 0) {
        floorSelect.prop('disabled', true);
    } else {
        floorSelect.prop('disabled', false);
    }
    
    // Filter tables by selected floor
    var selectedFloor = $('#publicBookingFloor').val() || '0';
    var tablesToShow = [];
    if (selectedFloor == '0' || selectedFloor == 0) {
        $.each(publicTableDataCache, function(tableId, table) {
            tablesToShow.push(table);
        });
    } else {
        $.each(publicTableDataCache, function(tableId, table) {
            var tableFloorId = table.FloorId || 0;
            if (tableFloorId == selectedFloor || tableFloorId == parseInt(selectedFloor)) {
                tablesToShow.push(table);
            }
        });
    }
    
    if (tablesToShow.length === 0) {
        canvas.html('<div class="text-center p-5"><p class="text-muted">No tables available for this floor</p></div>');
        return;
    }
    
    // Render tables
    $.each(tablesToShow, function(index, table) {
        var posX = table.PositionX || (index % 10) * 110 + 20;
        var posY = table.PositionY || Math.floor(index / 10) * 90 + 20;
        
        var tableElement = $('<div>', {
            'class': 'public-table-item',
            'data-table-id': table.TableId,
            'css': {
                'left': posX + 'px',
                'top': posY + 'px',
                'background-color': getPublicStatusColor(table.Status || 'Available')
            }
        });
        
        tableElement.append($('<div>', {
            'text': table.TableNo || 'T' + table.TableId,
            'css': {
                'font-weight': 'bold',
                'font-size': '12px',
                'margin-bottom': '2px'
            }
        }));
        
        if (table.TableName) {
            tableElement.append($('<div>', {
                'text': table.TableName,
                'css': {
                    'font-size': '10px',
                    'opacity': 0.9
                }
            }));
        }
        
        tableElement.append($('<div>', {
            'text': 'Cap: ' + (table.Capacity || 0),
            'css': {
                'font-size': '9px',
                'opacity': 0.8,
                'margin-top': '2px'
            }
        }));
        
        tableElement.attr('title', table.TableNo + (table.TableName ? ' - ' + table.TableName : '') + ' (Capacity: ' + table.Capacity + ') | Status: ' + (table.Status || 'Available'));
        
        // Add click handler - only allow clicking if table is available
        var isAvailable = table.IsAvailable !== false; // Default to true if not specified
        if (!isAvailable) {
            tableElement.addClass('unavailable');
            tableElement.css('cursor', 'not-allowed');
            tableElement.css('opacity', '0.6');
        } else {
            tableElement.on('click', function() {
                var tableId = $(this).data('table-id');
                selectPublicTable(tableId);
            });
        }
        
        canvas.append(tableElement);
    });
    
    // Highlight selected tables
    var selectedTableIds = getSelectedTableIds();
    selectedTableIds.forEach(function(tableId) {
        $('.public-table-item[data-table-id="' + tableId + '"]').addClass('selected');
    });

    // Auto-select initial table if configured
    if (publicBookingConfig && publicBookingConfig.initialTableId && publicBookingConfig.initialTableId > 0) {
        autoSelectInitialTable();
    }
}

function getSelectedTableIds() {
    var selectedTableIds = [];
    $('.public-table-item.selected, .table-card.selected').each(function() {
        var tableId = $(this).data('table-id');
        if (tableId) {
            selectedTableIds.push(parseInt(tableId));
        }
    });
    return selectedTableIds;
}

function selectPublicTable(tableId) {
    // Check if table is available before allowing selection
    var tableData = publicTableDataCache[tableId];
    if (tableData && !tableData.IsAvailable) {
        toastr.warning('This table is not available for the selected time slot');
        return;
    }
    
    // Toggle selection (allow multiple tables, unless locked to a single table)
    var tableElement = $('.public-table-item[data-table-id="' + tableId + '"], .table-card[data-table-id="' + tableId + '"]');
    if (tableElement.hasClass('selected')) {
        // Deselect
        tableElement.removeClass('selected');
        $('.public-table-item[data-table-id="' + tableId + '"]').removeClass('selected');
        $('.table-card[data-table-id="' + tableId + '"]').removeClass('selected');
        $('.table-card[data-table-id="' + tableId + '"]').find('.fa-check-circle').hide();
    } else {
        // If table selection is locked to a specific table, clear any previous selection
        if (publicBookingConfig && publicBookingConfig.lockTableSelection) {
            clearTableSelection();
        }

        // Select
        tableElement.addClass('selected');
        $('.public-table-item[data-table-id="' + tableId + '"]').addClass('selected');
        $('.table-card[data-table-id="' + tableId + '"]').addClass('selected');
        $('.table-card[data-table-id="' + tableId + '"]').find('.fa-check-circle').show();
    }
    
    // Update hidden field with comma-separated IDs (for backward compatibility, but we'll use TableIds in submit)
    var selectedIds = getSelectedTableIds();
    $('#tableId').val(selectedIds.length > 0 ? selectedIds.join(',') : '');
    
    // Show selected tables info
    updateSelectedTablesDisplay();
}

function updateSelectedTablesDisplay() {
    var selectedIds = getSelectedTableIds();
    var displayDiv = $('#selectedTablesDisplay');
    
    if (displayDiv.length === 0) {
        displayDiv = $('<div id="selectedTablesDisplay" class="mt-2 mb-2"></div>');
        $('#tableGroup').append(displayDiv);
    }
    
    if (selectedIds.length === 0) {
        displayDiv.hide();
        return;
    }
    
    var tableNames = [];
    var totalCapacity = 0;
    
    selectedIds.forEach(function(tableId) {
        var table = publicTableDataCache[tableId];
        if (table) {
            tableNames.push(table.TableNo || 'T' + tableId);
            totalCapacity += table.Capacity || 0;
        }
    });
    
    var capacityWarning = '';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    if (noOfGuests > 0 && totalCapacity > 0 && totalCapacity < noOfGuests) {
        capacityWarning = ' <span class="text-warning"><i class="fas fa-exclamation-triangle"></i> Total capacity (' + totalCapacity + ') is less than number of guests (' + noOfGuests + ')</span>';

        // Also surface this as a validation error for guests
        $('#errorNoOfGuests').text('Number of guests cannot exceed total table capacity (' + totalCapacity + ').').show();
    } else {
        // Clear capacity-related error if it is now valid
        var currentError = $('#errorNoOfGuests').text();
        if (currentError.indexOf('Number of guests cannot exceed total table capacity') === 0) {
            $('#errorNoOfGuests').text('').hide();
        }
    }
    
    displayDiv.html(
        '<div class="alert alert-info mb-0">' +
        '<strong>Selected:</strong> ' + tableNames.join(', ') +
        ' (' + selectedIds.length + ' table' + (selectedIds.length > 1 ? 's' : '') + ')' +
        (totalCapacity > 0 ? ' | <strong>Total Capacity:</strong> ' + totalCapacity + ' guests' : '') +
        capacityWarning +
        ' <button type="button" class="btn btn-sm btn-outline-secondary ml-2" onclick="clearTableSelection()"><i class="fas fa-times"></i> Clear</button>' +
        '</div>'
    ).show();
}

function clearTableSelection() {
    $('.public-table-item.selected, .table-card.selected').removeClass('selected');
    $('.table-card').find('.fa-check-circle').hide();
    $('#tableId').val('');
    $('#selectedTablesDisplay').hide();
}

function getTotalSelectedCapacity() {
    var selectedIds = getSelectedTableIds();
    var totalCapacity = 0;
    selectedIds.forEach(function (tableId) {
        var table = publicTableDataCache[tableId];
        if (table && table.Capacity) {
            totalCapacity += table.Capacity;
        }
    });
    return totalCapacity;
}

function autoSelectInitialTable() {
    var tableId = publicBookingConfig && publicBookingConfig.initialTableId ? publicBookingConfig.initialTableId : 0;
    if (!tableId || !publicTableDataCache[tableId]) {
        // Nothing to auto-select
        return;
    }

    var table = publicTableDataCache[tableId];

    // Set floor dropdown based on table's floor
    if (table.FloorId && $('#publicBookingFloor').length) {
        $('#publicBookingFloor').val(table.FloorId.toString());
    }

    // Clear any previous selections and select this table
    clearTableSelection();
    selectPublicTable(tableId);

    // If selection is locked, disable interaction with other tables and floor change
    if (publicBookingConfig.lockTableSelection) {
        $('.public-table-item, .table-card').each(function () {
            var id = $(this).data('table-id');
            if (id !== tableId) {
                $(this).addClass('unavailable')
                    .css('cursor', 'not-allowed')
                    .off('click');
            }
        });

        $('#publicBookingFloor').prop('disabled', true);
    }
}

function displayTables(tables) {
    // Store tables in cache for layout view
    publicTableDataCache = {};
    tables.forEach(function(table) {
        publicTableDataCache[table.TableId] = table;
    });
    
    var container = $('#tablesContainer');
    container.empty();
    
    if (tables.length === 0) {
        container.html('<p class="text-muted">No tables available for the selected date and time.</p>');
        $('#tableGroup').show();
        $('#tableLayoutCard').hide();
        $('#tablesContainer').show();
        $('#floorGroup').hide();
        hideLoading();
        return;
    }
    
    // Render list view
    tables.forEach(function(table) {
        var isAvailable = table.IsAvailable !== false; // Default to true if not specified
        // Create compact status badge
        var statusColor = getPublicStatusColor(table.Status);
        var statusBadge = '<span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background-color: ' + statusColor + '; margin-right: 4px;"></span>';
        
        var card = $('<div>')
            .addClass('table-card')
            .data('table-id', table.TableId)
            .html(
                '<div class="table-info">' +
                    '<div style="flex: 1;">' +
                        '<div class="table-name">' + table.TableNo + 
                        (table.TableName ? ' - ' + table.TableName : '') + '</div>' +
                        '<div class="table-capacity">' + 
                        statusBadge + table.Status + 
                        ' • ' + table.Capacity + ' guests' +
                        (table.FloorName ? ' • ' + table.FloorName : '') + 
                        '</div>' +
                    '</div>' +
                    '<div style="margin-left: 8px;"><i class="fas fa-check-circle text-success" style="display: none; font-size: 1.1rem;"></i></div>' +
                '</div>'
            );
        
        if (!isAvailable) {
            card.addClass('unavailable');
        }
        
        card.on('click', function() {
            if (isAvailable) {
                selectPublicTable($(this).data('table-id'));
            } else {
                toastr.warning('This table is not available for the selected time slot');
            }
        });
        
        container.append(card);
    });
    
    // Initialize layout view first
    renderPublicTableLayout();
    
    // Show appropriate view - default to layout view (isListView = false)
    if (isListView) {
        $('#tableLayoutCard').hide();
        $('#tablesContainer').show();
        $('#toggleViewBtn').html('<i class="fas fa-th"></i>').attr('title', 'Show Layout View');
    } else {
        // Default: Show layout view, hide list view
        $('#tableLayoutCard').show();
        $('#tablesContainer').hide();
        $('#toggleViewBtn').html('<i class="fas fa-list"></i>').attr('title', 'Show List View');
    }
    
    // Floor change handler
    $('#publicBookingFloor').off('change').on('change', function() {
        renderPublicTableLayout();
    });
    
    $('#tableGroup').show();
    $('#floorGroup').show();
    hideLoading();
    
    // Update selected tables display if any are already selected
    updateSelectedTablesDisplay();

    // Auto-select initial table if configured and present in the data
    if (publicBookingConfig && publicBookingConfig.initialTableId && publicBookingConfig.initialTableId > 0) {
        autoSelectInitialTable();
    }
}

function submitBooking() {
    // Clear previous errors
    $('.error-message').text('').hide();
    
    // Validate form
    var isValid = true;
    
    if (!$('#bookingDate').val()) {
        showError('errorBookingDate', 'Please select a booking date');
        isValid = false;
    }
    
    var noOfGuestsVal = parseInt($('#noOfGuests').val());
    if (!$('#noOfGuests').val() || noOfGuestsVal <= 0) {
        showError('errorNoOfGuests', 'Please enter number of guests');
        isValid = false;
    }
    
    if (!$('#bookingTime').val()) {
        showError('errorBookingTime', 'Please select a time slot');
        isValid = false;
    }
    
    var selectedTableIds = getSelectedTableIds();
    if (selectedTableIds.length === 0) {
        showError('errorTableId', 'Please select at least one table');
        isValid = false;
    }

    // Capacity validation: total capacity of selected tables must be >= number of guests
    var totalCapacity = getTotalSelectedCapacity();
    if (noOfGuestsVal > 0 && totalCapacity > 0 && noOfGuestsVal > totalCapacity) {
        showError('errorNoOfGuests', 'Number of guests cannot exceed total table capacity (' + totalCapacity + ').');
        isValid = false;
    }
    
    if (!$('#customerName').val().trim()) {
        showError('errorCustomerName', 'Please enter your name');
        isValid = false;
    }
    
    if (!$('#customerMobile').val().trim() || $('#customerMobile').val().length < 10) {
        showError('errorCustomerMobile', 'Please enter a valid mobile number');
        isValid = false;
    }
    
    if (!$('#customerEmail').val().trim() || !isValidEmail($('#customerEmail').val())) {
        showError('errorCustomerEmail', 'Please enter a valid email address');
        isValid = false;
    }
    
    if (!isValid) {
        toastr.error('Please fill in all required fields correctly');
        return;
    }
    
    // Check if deposit payment is required
    initBookingDepositPayment();
}

function initBookingDepositPayment() {
    // Disable submit button
    var submitBtn = $('#submitBtn');
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    showLoading();
    
    // Prepare booking data for deposit calculation
    var bookingTimeVal = $('#bookingTime').val() || '';
    bookingTimeVal = String(bookingTimeVal).trim();
    
    var bookingData = {
        Slug: publicBookingConfig.slug,
        BookingDate: $('#bookingDate').val(),
        BookingTimeString: bookingTimeVal,
        NoOfGuests: parseInt($('#noOfGuests').val()),
        TableIds: getSelectedTableIds(),
        CustomerName: $('#customerName').val().trim(),
        CustomerMobile: $('#customerMobile').val().trim(),
        CustomerEmail: $('#customerEmail').val().trim(),
        SpecialRequest: $('#specialRequest').val().trim(),
        Duration: publicBookingConfig.defaultDuration
    };
    
    // Initialize payment gateway
    $.ajax({
        url: '/api/PublicTableBooking/InitBookingDepositPayment',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            hideLoading();
            
            if (response.Status == 1 && response.Data) {
                var depositAmount = response.Data.DepositAmount || 0;
                
                // If no deposit required, proceed directly to booking creation
                if (depositAmount <= 0) {
                    createBookingAfterPayment(bookingData, null, null);
                    return;
                }
                
                // Show deposit amount
                updateDepositDisplay(depositAmount);
                
                // Show deposit amount and payment gateway
                var paymentSetting = response.Data.OnlinePaymentSetting;
                var businessSetting = response.Data.BusinessSetting;
                
                // Store booking data for later use
                window.pendingBookingData = bookingData;
                
                // Initialize payment gateway based on type
                if (paymentSetting.OnlinePaymentService == 1) {
                    // PayPal
                    initPayPalPayment(depositAmount, paymentSetting, businessSetting);
                } else {
                    // Razorpay
                    initRazorpayPayment(depositAmount, paymentSetting, businessSetting);
                }
            } else {
                toastr.error(response.Message || 'Failed to initialize payment');
                submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        },
        error: function(xhr) {
            hideLoading();
            var errorMsg = 'An error occurred while initializing payment';
            
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMsg = xhr.responseJSON.Message;
            }
            
            toastr.error(errorMsg);
            submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    });
}

function updateDepositDisplay(amount) {
    var currencySymbol = '₹'; // Default, can be enhanced to get from API
    var depositMode = publicBookingConfig && publicBookingConfig.depositMode ? publicBookingConfig.depositMode : 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig && publicBookingConfig.depositPerGuestAmount ? publicBookingConfig.depositPerGuestAmount : 0;
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
    } else {
        $('#depositAmount').html(currencySymbol + amount.toFixed(2));
        $('#depositType').html('(fixed amount)');
    }
    
    $('#depositInfoGroup').show();
}

function updateDepositForGuestChange() {
    if (!publicBookingConfig || !publicBookingConfig.requireDeposit) {
        return;
    }
    
    var depositMode = publicBookingConfig.depositMode || 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig.depositPerGuestAmount || 0;
        var totalDeposit = noOfGuests * perGuestAmount;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
        $('#depositInfoGroup').show();
    } else if (depositMode === 'Fixed') {
        var fixedAmount = publicBookingConfig.depositFixedAmount || 0;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + fixedAmount.toFixed(2));
        $('#depositType').html('(fixed amount)');
        $('#depositInfoGroup').show();
    }
}

function initRazorpayPayment(depositAmount, paymentSetting, businessSetting) {
    // Load Razorpay script if not already loaded
    if (typeof Razorpay === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://checkout.razorpay.com/v1/checkout.js';
        script.onload = function() {
            openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openRazorpayGateway(depositAmount, paymentSetting, businessSetting) {
    var options = {
        "key": paymentSetting.RazorpayKey,
        "amount": depositAmount * 100, // Convert to paise
        "currency": paymentSetting.RazorpayCurrencyCode,
        "name": businessSetting.BusinessName,
        "image": businessSetting.BusinessLogo || "",
        "handler": function (response) {
            // Payment successful
            createBookingAfterPayment(window.pendingBookingData, response.razorpay_payment_id, 'Razorpay');
        },
        "prefill": {
            "name": window.pendingBookingData.CustomerName,
            "email": window.pendingBookingData.CustomerEmail,
            "contact": window.pendingBookingData.CustomerMobile
        },
        "notes": {
            "booking_type": "Table Booking Deposit"
        },
        "theme": {
            "color": "#667eea"
        },
        "modal": {
            "ondismiss": function() {
                // User closed payment modal
                $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        }
    };
    
    var rzp1 = new Razorpay(options);
    rzp1.open();
}

function initPayPalPayment(depositAmount, paymentSetting, businessSetting) {
    // Load PayPal script if not already loaded
    if (typeof paypal === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://www.paypal.com/sdk/js?client-id=' + paymentSetting.PaypalClientId + '&currency=' + paymentSetting.PaypalCurrencyCode;
        script.onload = function() {
            openPayPalGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openPayPalGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openPayPalGateway(depositAmount, paymentSetting, businessSetting) {
    // Remove existing PayPal button container if any
    $('#paypal-button-container').remove();
    
    // Create container for PayPal button
    if ($('#paypalDepositContainer').length === 0) {
        $('body').append('<div id="paypalDepositContainer" style="position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: white; padding: 20px; border-radius: 10px; box-shadow: 0 4px 20px rgba(0,0,0,0.3); z-index: 10000;"><div id="paypal-button-container"></div><button id="closePayPalModal" style="margin-top: 10px; padding: 5px 15px; background: #dc3545; color: white; border: none; border-radius: 5px; cursor: pointer;">Cancel</button></div>');
        
        $('#closePayPalModal').on('click', function() {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        });
    }
    
    paypal.Buttons({
        style: {
            layout: 'vertical',
            color: 'blue',
            shape: 'rect',
            label: 'pay'
        },
        createOrder: function (data, actions) {
            return actions.order.create({
                purchase_units: [{
                    amount: {
                        value: depositAmount.toString()
                    },
                    description: 'Table Booking Deposit'
                }]
            });
        },
        onApprove: function (data, actions) {
            return actions.order.capture().then(function (orderData) {
                var transaction = orderData.purchase_units[0].payments.captures[0];
                $('#paypalDepositContainer').remove();
                // Payment successful
                createBookingAfterPayment(window.pendingBookingData, transaction.id, 'PayPal');
            });
        },
        onCancel: function(data) {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        },
        onError: function(err) {
            $('#paypalDepositContainer').remove();
            toastr.error('Payment failed. Please try again.');
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    }).render('#paypal-button-container');
}

function createBookingAfterPayment(bookingData, paymentTransactionId, paymentGatewayType) {
    // Disable submit button
    var submitBtn = $('#submitBtn');
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Creating Booking...');
    showLoading();
    
    // Add payment information to booking data
    bookingData.PaymentTransactionId = paymentTransactionId || null;
    bookingData.PaymentGatewayType = paymentGatewayType || null;
    
    // Submit booking
    $.ajax({
        url: '/PublicBooking/CreatePublicBooking',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            hideLoading();
            
            if (response.Status == 1) {
                // Success - redirect to confirmation page
                toastr.success('Booking created successfully!');
                setTimeout(function() {
                    window.location.href = '/publicbooking/bookingconfirmation?bookingNo=' + response.Data.BookingNo;
                }, 1000);
            } else {
                // Show errors
                if (response.Errors && response.Errors.length > 0) {
                    response.Errors.forEach(function(error) {
                        showError('error' + error.Id.replace('div', ''), error.Message);
                    });
                }
                toastr.error(response.Message || 'Failed to create booking');
                submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        },
        error: function(xhr) {
            hideLoading();
            var errorMsg = 'An error occurred while creating your booking';
            
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMsg = xhr.responseJSON.Message;
            }
            
            toastr.error(errorMsg);
            submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    });
}

function submitBookingOld() {
    // Clear previous errors
    $('.error-message').text('').hide();
    
    // Validate form
    var isValid = true;
    
    if (!$('#bookingDate').val()) {
        showError('errorBookingDate', 'Please select a booking date');
        isValid = false;
    }
    
    var noOfGuestsVal = parseInt($('#noOfGuests').val());
    if (!$('#noOfGuests').val() || noOfGuestsVal <= 0) {
        showError('errorNoOfGuests', 'Please enter number of guests');
        isValid = false;
    }
    
    if (!$('#bookingTime').val()) {
        showError('errorBookingTime', 'Please select a time slot');
        isValid = false;
    }
    
    var selectedTableIds = getSelectedTableIds();
    if (selectedTableIds.length === 0) {
        showError('errorTableId', 'Please select at least one table');
        isValid = false;
    }

    // Capacity validation: total capacity of selected tables must be >= number of guests
    var totalCapacity = getTotalSelectedCapacity();
    if (noOfGuestsVal > 0 && totalCapacity > 0 && noOfGuestsVal > totalCapacity) {
        showError('errorNoOfGuests', 'Number of guests cannot exceed total table capacity (' + totalCapacity + ').');
        isValid = false;
    }
    
    if (!$('#customerName').val().trim()) {
        showError('errorCustomerName', 'Please enter your name');
        isValid = false;
    }
    
    if (!$('#customerMobile').val().trim() || $('#customerMobile').val().length < 10) {
        showError('errorCustomerMobile', 'Please enter a valid mobile number');
        isValid = false;
    }
    
    if (!$('#customerEmail').val().trim() || !isValidEmail($('#customerEmail').val())) {
        showError('errorCustomerEmail', 'Please enter a valid email address');
        isValid = false;
    }
    
    if (!isValid) {
        toastr.error('Please fill in all required fields correctly');
        return;
    }
    
    // Check if deposit payment is required
    initBookingDepositPayment();
}

function initBookingDepositPayment() {
    // Disable submit button
    var submitBtn = $('#submitBtn');
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    showLoading();
    
    // Prepare booking data for deposit calculation
    var bookingTimeVal = $('#bookingTime').val() || '';
    bookingTimeVal = String(bookingTimeVal).trim();
    
    var bookingData = {
        Slug: publicBookingConfig.slug,
        BookingDate: $('#bookingDate').val(),
        BookingTimeString: bookingTimeVal,
        NoOfGuests: parseInt($('#noOfGuests').val()),
        TableIds: getSelectedTableIds(),
        CustomerName: $('#customerName').val().trim(),
        CustomerMobile: $('#customerMobile').val().trim(),
        CustomerEmail: $('#customerEmail').val().trim(),
        SpecialRequest: $('#specialRequest').val().trim(),
        Duration: publicBookingConfig.defaultDuration
    };
    
    // Initialize payment gateway
    $.ajax({
        url: '/api/PublicTableBooking/InitBookingDepositPayment',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            hideLoading();
            
            if (response.Status == 1 && response.Data) {
                var depositAmount = response.Data.DepositAmount || 0;
                
                // If no deposit required, proceed directly to booking creation
                if (depositAmount <= 0) {
                    createBookingAfterPayment(bookingData, null, null);
                    return;
                }
                
                // Show deposit amount
                updateDepositDisplay(depositAmount);
                
                // Show deposit amount and payment gateway
                var paymentSetting = response.Data.OnlinePaymentSetting;
                var businessSetting = response.Data.BusinessSetting;
                
                // Store booking data for later use
                window.pendingBookingData = bookingData;
                
                // Initialize payment gateway based on type
                if (paymentSetting.OnlinePaymentService == 1) {
                    // PayPal
                    initPayPalPayment(depositAmount, paymentSetting, businessSetting);
                } else {
                    // Razorpay
                    initRazorpayPayment(depositAmount, paymentSetting, businessSetting);
                }
            } else {
                toastr.error(response.Message || 'Failed to initialize payment');
                submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        },
        error: function(xhr) {
            hideLoading();
            var errorMsg = 'An error occurred while initializing payment';
            
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMsg = xhr.responseJSON.Message;
            }
            
            toastr.error(errorMsg);
            submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    });
}

function updateDepositDisplay(amount) {
    var currencySymbol = '₹'; // Default, can be enhanced to get from API
    var depositMode = publicBookingConfig && publicBookingConfig.depositMode ? publicBookingConfig.depositMode : 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig && publicBookingConfig.depositPerGuestAmount ? publicBookingConfig.depositPerGuestAmount : 0;
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
    } else {
        $('#depositAmount').html(currencySymbol + amount.toFixed(2));
        $('#depositType').html('(fixed amount)');
    }
    
    $('#depositInfoGroup').show();
}

function updateDepositForGuestChange() {
    if (!publicBookingConfig || !publicBookingConfig.requireDeposit) {
        return;
    }
    
    var depositMode = publicBookingConfig.depositMode || 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig.depositPerGuestAmount || 0;
        var totalDeposit = noOfGuests * perGuestAmount;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
        $('#depositInfoGroup').show();
    } else if (depositMode === 'Fixed') {
        var fixedAmount = publicBookingConfig.depositFixedAmount || 0;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + fixedAmount.toFixed(2));
        $('#depositType').html('(fixed amount)');
        $('#depositInfoGroup').show();
    }
}

function initRazorpayPayment(depositAmount, paymentSetting, businessSetting) {
    // Load Razorpay script if not already loaded
    if (typeof Razorpay === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://checkout.razorpay.com/v1/checkout.js';
        script.onload = function() {
            openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openRazorpayGateway(depositAmount, paymentSetting, businessSetting) {
    var options = {
        "key": paymentSetting.RazorpayKey,
        "amount": depositAmount * 100, // Convert to paise
        "currency": paymentSetting.RazorpayCurrencyCode,
        "name": businessSetting.BusinessName,
        "image": businessSetting.BusinessLogo || "",
        "handler": function (response) {
            // Payment successful
            createBookingAfterPayment(window.pendingBookingData, response.razorpay_payment_id, 'Razorpay');
        },
        "prefill": {
            "name": window.pendingBookingData.CustomerName,
            "email": window.pendingBookingData.CustomerEmail,
            "contact": window.pendingBookingData.CustomerMobile
        },
        "notes": {
            "booking_type": "Table Booking Deposit"
        },
        "theme": {
            "color": "#667eea"
        },
        "modal": {
            "ondismiss": function() {
                // User closed payment modal
                $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        }
    };
    
    var rzp1 = new Razorpay(options);
    rzp1.open();
}

function initPayPalPayment(depositAmount, paymentSetting, businessSetting) {
    // Load PayPal script if not already loaded
    if (typeof paypal === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://www.paypal.com/sdk/js?client-id=' + paymentSetting.PaypalClientId + '&currency=' + paymentSetting.PaypalCurrencyCode;
        script.onload = function() {
            openPayPalGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openPayPalGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openPayPalGateway(depositAmount, paymentSetting, businessSetting) {
    // Remove existing PayPal button container if any
    $('#paypal-button-container').remove();
    
    // Create container for PayPal button
    if ($('#paypalDepositContainer').length === 0) {
        $('body').append('<div id="paypalDepositContainer" style="position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: white; padding: 20px; border-radius: 10px; box-shadow: 0 4px 20px rgba(0,0,0,0.3); z-index: 10000;"><div id="paypal-button-container"></div><button id="closePayPalModal" style="margin-top: 10px; padding: 5px 15px; background: #dc3545; color: white; border: none; border-radius: 5px; cursor: pointer;">Cancel</button></div>');
        
        $('#closePayPalModal').on('click', function() {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        });
    }
    
    paypal.Buttons({
        style: {
            layout: 'vertical',
            color: 'blue',
            shape: 'rect',
            label: 'pay'
        },
        createOrder: function (data, actions) {
            return actions.order.create({
                purchase_units: [{
                    amount: {
                        value: depositAmount.toString()
                    },
                    description: 'Table Booking Deposit'
                }]
            });
        },
        onApprove: function (data, actions) {
            return actions.order.capture().then(function (orderData) {
                var transaction = orderData.purchase_units[0].payments.captures[0];
                $('#paypalDepositContainer').remove();
                // Payment successful
                createBookingAfterPayment(window.pendingBookingData, transaction.id, 'PayPal');
            });
        },
        onCancel: function(data) {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        },
        onError: function(err) {
            $('#paypalDepositContainer').remove();
            toastr.error('Payment failed. Please try again.');
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    }).render('#paypal-button-container');
}

function createBookingAfterPayment(bookingData, paymentTransactionId, paymentGatewayType) {
    // Disable submit button
    var submitBtn = $('#submitBtn');
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Creating Booking...');
    showLoading();
    
    // Add payment information to booking data
    bookingData.PaymentTransactionId = paymentTransactionId || null;
    bookingData.PaymentGatewayType = paymentGatewayType || null;
    
    // Submit booking
    $.ajax({
        url: '/PublicBooking/CreatePublicBooking',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            hideLoading();
            
            if (response.Status == 1) {
                // Success - redirect to confirmation page
                toastr.success('Booking created successfully!');
                setTimeout(function() {
                    window.location.href = '/publicbooking/bookingconfirmation?bookingNo=' + response.Data.BookingNo;
                }, 1000);
            } else {
                // Show errors
                if (response.Errors && response.Errors.length > 0) {
                    response.Errors.forEach(function(error) {
                        showError('error' + error.Id.replace('div', ''), error.Message);
                    });
                }
                toastr.error(response.Message || 'Failed to create booking');
                submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        },
        error: function(xhr) {
            hideLoading();
            var errorMsg = 'An error occurred while creating your booking';
            
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMsg = xhr.responseJSON.Message;
            }
            
            toastr.error(errorMsg);
            submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    });
}

function showError(elementId, message) {
    $('#' + elementId).text(message).show();
}

function showLoading() {
    $('#loadingSpinner').show();
}

function hideLoading() {
    $('#loadingSpinner').hide();
}

function isValidEmail(email) {
    var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function updateDepositDisplay(amount) {
    var currencySymbol = '₹'; // Default, can be enhanced to get from API
    var depositMode = publicBookingConfig && publicBookingConfig.depositMode ? publicBookingConfig.depositMode : 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig && publicBookingConfig.depositPerGuestAmount ? publicBookingConfig.depositPerGuestAmount : 0;
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
    } else {
        $('#depositAmount').html(currencySymbol + amount.toFixed(2));
        $('#depositType').html('(fixed amount)');
    }
    
    $('#depositInfoGroup').show();
}

function updateDepositForGuestChange() {
    if (!publicBookingConfig || !publicBookingConfig.requireDeposit) {
        return;
    }
    
    var depositMode = publicBookingConfig.depositMode || 'Fixed';
    var noOfGuests = parseInt($('#noOfGuests').val()) || 0;
    
    if (depositMode === 'PerGuest' && noOfGuests > 0) {
        var perGuestAmount = publicBookingConfig.depositPerGuestAmount || 0;
        var totalDeposit = noOfGuests * perGuestAmount;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + perGuestAmount.toFixed(2));
        $('#depositType').html('(per guest)');
        $('#depositInfoGroup').show();
    } else if (depositMode === 'Fixed') {
        var fixedAmount = publicBookingConfig.depositFixedAmount || 0;
        var currencySymbol = '₹';
        
        $('#depositAmount').html(currencySymbol + fixedAmount.toFixed(2));
        $('#depositType').html('(fixed amount)');
        $('#depositInfoGroup').show();
    }
}

function initRazorpayPayment(depositAmount, paymentSetting, businessSetting) {
    // Load Razorpay script if not already loaded
    if (typeof Razorpay === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://checkout.razorpay.com/v1/checkout.js';
        script.onload = function() {
            openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openRazorpayGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openRazorpayGateway(depositAmount, paymentSetting, businessSetting) {
    var options = {
        "key": paymentSetting.RazorpayKey,
        "amount": depositAmount * 100, // Convert to paise
        "currency": paymentSetting.RazorpayCurrencyCode,
        "name": businessSetting.BusinessName,
        "image": businessSetting.BusinessLogo || "",
        "handler": function (response) {
            // Payment successful
            createBookingAfterPayment(window.pendingBookingData, response.razorpay_payment_id, 'Razorpay');
        },
        "prefill": {
            "name": window.pendingBookingData.CustomerName,
            "email": window.pendingBookingData.CustomerEmail,
            "contact": window.pendingBookingData.CustomerMobile
        },
        "notes": {
            "booking_type": "Table Booking Deposit"
        },
        "theme": {
            "color": "#667eea"
        },
        "modal": {
            "ondismiss": function() {
                // User closed payment modal
                $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        }
    };
    
    var rzp1 = new Razorpay(options);
    rzp1.open();
}

function initPayPalPayment(depositAmount, paymentSetting, businessSetting) {
    // Load PayPal script if not already loaded
    if (typeof paypal === 'undefined') {
        var script = document.createElement('script');
        script.src = 'https://www.paypal.com/sdk/js?client-id=' + paymentSetting.PaypalClientId + '&currency=' + paymentSetting.PaypalCurrencyCode;
        script.onload = function() {
            openPayPalGateway(depositAmount, paymentSetting, businessSetting);
        };
        document.head.appendChild(script);
    } else {
        openPayPalGateway(depositAmount, paymentSetting, businessSetting);
    }
}

function openPayPalGateway(depositAmount, paymentSetting, businessSetting) {
    // Remove existing PayPal button container if any
    $('#paypal-button-container').remove();
    
    // Create container for PayPal button
    if ($('#paypalDepositContainer').length === 0) {
        $('body').append('<div id="paypalDepositContainer" style="position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: white; padding: 20px; border-radius: 10px; box-shadow: 0 4px 20px rgba(0,0,0,0.3); z-index: 10000;"><div id="paypal-button-container"></div><button id="closePayPalModal" style="margin-top: 10px; padding: 5px 15px; background: #dc3545; color: white; border: none; border-radius: 5px; cursor: pointer;">Cancel</button></div>');
        
        $('#closePayPalModal').on('click', function() {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        });
    }
    
    paypal.Buttons({
        style: {
            layout: 'vertical',
            color: 'blue',
            shape: 'rect',
            label: 'pay'
        },
        createOrder: function (data, actions) {
            return actions.order.create({
                purchase_units: [{
                    amount: {
                        value: depositAmount.toString()
                    },
                    description: 'Table Booking Deposit'
                }]
            });
        },
        onApprove: function (data, actions) {
            return actions.order.capture().then(function (orderData) {
                var transaction = orderData.purchase_units[0].payments.captures[0];
                $('#paypalDepositContainer').remove();
                // Payment successful
                createBookingAfterPayment(window.pendingBookingData, transaction.id, 'PayPal');
            });
        },
        onCancel: function(data) {
            $('#paypalDepositContainer').remove();
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        },
        onError: function(err) {
            $('#paypalDepositContainer').remove();
            toastr.error('Payment failed. Please try again.');
            $('#submitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    }).render('#paypal-button-container');
}

function createBookingAfterPayment(bookingData, paymentTransactionId, paymentGatewayType) {
    // Disable submit button
    var submitBtn = $('#submitBtn');
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Creating Booking...');
    showLoading();
    
    // Add payment information to booking data
    bookingData.PaymentTransactionId = paymentTransactionId || null;
    bookingData.PaymentGatewayType = paymentGatewayType || null;
    
    // Submit booking
    $.ajax({
        url: '/PublicBooking/CreatePublicBooking',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            hideLoading();
            
            if (response.Status == 1) {
                // Success - redirect to confirmation page
                toastr.success('Booking created successfully!');
                setTimeout(function() {
                    window.location.href = '/publicbooking/bookingconfirmation?bookingNo=' + response.Data.BookingNo;
                }, 1000);
            } else {
                // Show errors
                if (response.Errors && response.Errors.length > 0) {
                    response.Errors.forEach(function(error) {
                        showError('error' + error.Id.replace('div', ''), error.Message);
                    });
                }
                toastr.error(response.Message || 'Failed to create booking');
                submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
            }
        },
        error: function(xhr) {
            hideLoading();
            var errorMsg = 'An error occurred while creating your booking';
            
            if (xhr.responseJSON && xhr.responseJSON.Message) {
                errorMsg = xhr.responseJSON.Message;
            }
            
            toastr.error(errorMsg);
            submitBtn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirm Booking');
        }
    });
}

// Initialize toastr
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

