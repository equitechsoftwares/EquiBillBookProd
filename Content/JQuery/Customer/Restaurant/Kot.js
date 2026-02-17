var _PageIndex = 1;
var kotItemCounter = 0;
var skuCodes = [];

// Show deferred success message after redirect (stored in sessionStorage)
$(function () {
    var _kotMsg = sessionStorage.getItem('kotSuccessMessage');
    if (_kotMsg) {
        sessionStorage.removeItem('kotSuccessMessage');
        // Play success sound after page load to ensure audio element is ready
        try {
            if (typeof EnableSound !== 'undefined' && EnableSound === 'True') {
                var snd = document.getElementById('success');
                if (snd && typeof snd.play === 'function') {
                    snd.play();
                }
            }
        } catch (e) { /* ignore */ }
        toastr.success(_kotMsg);
    }
});

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val() || 10,
        Search: $('#txtSearch').val(),
        FromDate: $('#txtFromDate').val() || null,
        ToDate: $('#txtToDate').val() || null,
        OrderStatus: $('#ddlStatus').val() || null,
        TableId: $('#ddlTable').val() || null
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/kot/index',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var $response = $(data);
            $("#tblData").html($response.find("#tblData").html());
            
            // Update status counts
            var totalText = $response.find("#divTotalCount").text() || "Total : 0";
            var pendingText = $response.find("#divPendingCount").text() || "Pending : 0";
            var preparingText = $response.find("#divPreparingCount").text() || "Preparing : 0";
            var readyText = $response.find("#divReadyCount").text() || "Ready : 0";
            var servedText = $response.find("#divServedCount").text() || "Served : 0";
            var cancelledText = $response.find("#divCancelledCount").text() || "Cancelled : 0";
            
            $("#divTotalCount").text(totalText);
            $("#divPendingCount").text(pendingText);
            $("#divPreparingCount").text(preparingText);
            $("#divReadyCount").text(readyText);
            $("#divServedCount").text(servedText);
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

function addKotItem(itemData) {
    kotItemCounter++;
    var itemName = itemData ? itemData.ItemName : '';
    var itemId = itemData ? itemData.ItemId : 0;
    var itemDetailsId = itemData ? (itemData.ItemDetailsId || 0) : 0;
    var skuCode = itemData ? itemData.SKU : '';
    var unit = itemData ? (itemData.UnitShortName || '') : '';
    var unitId = itemData ? (itemData.UnitId || 0) : 0;
    var secondaryUnitId = itemData ? (itemData.SecondaryUnitId || 0) : 0;
    var secondaryUnitShortName = itemData ? (itemData.SecondaryUnitShortName || '') : '';
    var tertiaryUnitId = itemData ? (itemData.TertiaryUnitId || 0) : 0;
    var tertiaryUnitShortName = itemData ? (itemData.TertiaryUnitShortName || '') : '';
    var quaternaryUnitId = itemData ? (itemData.QuaternaryUnitId || 0) : 0;
    var quaternaryUnitShortName = itemData ? (itemData.QuaternaryUnitShortName || '') : '';
    var priceAddedFor = itemData ? (itemData.PriceAddedFor || 1) : 1;
    var quantity = itemData ? 1 : 1;
    
    // Format item name display (truncate if > 15 chars, similar to SalesQuotation)
    var itemNameDisplay = '';
    if (itemName.length > 15) {
        itemNameDisplay = '<span><b>Name :</b> ' + itemName.substring(0, 15) + '...</span>';
    } else {
        itemNameDisplay = '<span><b>Name :</b> ' + itemName + '</span>';
    }
    
    // Build unit dropdown - only show units configured for this item (like SalesQuotation)
    var ddlUnit = '<select style="min-width:80px" class="form-control kot-item-unit ' + (unitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + kotItemCounter + '">';
    
    if (quaternaryUnitId != 0) {
        // Item has 4 units (Primary, Secondary, Tertiary, Quaternary)
        if (priceAddedFor == 1) {
            ddlUnit += '<option selected value="' + unitId + '-1-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-1-1">' + unit + '</option>';
        }
        if (priceAddedFor == 2) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-2-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-2-2">' + secondaryUnitShortName + '</option>';
        }
        if (priceAddedFor == 3) {
            ddlUnit += '<option selected value="' + tertiaryUnitId + '-3-3">' + tertiaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + tertiaryUnitId + '-3-3">' + tertiaryUnitShortName + '</option>';
        }
        if (priceAddedFor == 4) {
            ddlUnit += '<option selected value="' + quaternaryUnitId + '-4-4">' + quaternaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + quaternaryUnitId + '-4-4">' + quaternaryUnitShortName + '</option>';
        }
    } else if (tertiaryUnitId != 0) {
        // Item has 3 units (Primary, Secondary, Tertiary)
        if (priceAddedFor == 2) {
            ddlUnit += '<option selected value="' + unitId + '-2-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-2-1">' + unit + '</option>';
        }
        if (priceAddedFor == 3) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-3-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-3-2">' + secondaryUnitShortName + '</option>';
        }
        if (priceAddedFor == 4) {
            ddlUnit += '<option selected value="' + tertiaryUnitId + '-4-3">' + tertiaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + tertiaryUnitId + '-4-3">' + tertiaryUnitShortName + '</option>';
        }
    } else if (secondaryUnitId != 0) {
        // Item has 2 units (Primary, Secondary)
        if (priceAddedFor == 3) {
            ddlUnit += '<option selected value="' + unitId + '-3-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-3-1">' + unit + '</option>';
        }
        if (priceAddedFor == 4) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-4-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-4-2">' + secondaryUnitShortName + '</option>';
        }
    } else {
        // Item has only 1 unit (Primary)
        ddlUnit += '<option selected value="' + unitId + '-4-1">' + (unit || 'N/A') + '</option>';
    }
    
    ddlUnit += '</select>';
    
    var html = '<tr class="kot-item" data-item-index="' + kotItemCounter + '">';
    html += '<td style="min-width:100px; white-space: nowrap;" data-item-name="' + itemName.replace(/"/g, '&quot;') + '">';
    html += '<input type="hidden" class="kot-item-id" value="' + itemId + '">';
    html += '<input type="hidden" class="kot-item-details-id" value="' + itemDetailsId + '">';
    html += '<input type="hidden" class="kot-item-sku" value="' + skuCode + '">';
    html += '<input type="hidden" class="kot-item-name" value="' + itemName.replace(/"/g, '&quot;') + '">';
    html += itemNameDisplay;
    html += ' <br/> <b>SKU :</b> ' + skuCode;
    html += '</td>';
    html += '<td>';
    html += '<div class="input-group" style="min-width:120px">';
    html += '<input type="number" class="form-control kot-item-qty" placeholder="Qty" min="1" value="' + quantity + '" required style="width:60px">';
    html += ddlUnit;
    html += '</div>';
    html += '</td>';
    html += '<td><textarea class="form-control kot-item-instructions" placeholder="Cooking Instructions" rows="1"></textarea></td>';
    html += '<td><button type="button" class="btn btn-sm btn-danger" onclick="removeKotItem(' + kotItemCounter + ')"><i class="fas fa-times"></i></button></td>';
    html += '</tr>';
    $('#kotItemsContainer').append(html);
    
    // Clear item validation error when item is added
    $('#divtags').hide();
    $('#txttags').css('border', '');
}

function removeKotItem(index) {
    $('.kot-item[data-item-index="' + index + '"]').remove();
}

function deleteFullCombo() {
    if (confirm('Are you sure you want to remove all items?')) {
        $('#kotItemsContainer').empty();
        kotItemCounter = 0;
    }
}

function openSaltSearchModal() {
    // Placeholder for salt search modal - implement if needed
    alert('Salt search functionality to be implemented');
}

function saveKot() {
    // Clear previous validation errors
    $('.errorText').hide().text('');
    $('[style*="border: 2px"]').css('border', '');
    $('.form-control').css('border', '');
    // Clear Select2 validation styles
    $('.select2-container .select2-selection').each(function() {
        $(this).removeClass('is-invalid');
        this.style.removeProperty('border');
        this.style.removeProperty('border-color');
    });
    
    var isValid = true;
    var errors = [];
    
    // Validate Branch (if multiple branches exist)
    var branchId = $('#ddlBranch').val();
    if ($('#ddlBranch').length > 0 && !$('#ddlBranch').hasClass('hidden')) {
        if (!branchId || branchId == '' || branchId == 0) {
            $('#divBranch').text('Business Location is required').show();
            // Fix Select2 border - apply style directly to the element
            var $branchSelect = $('#ddlBranch').next('.select2-container').find('.select2-selection');
            if ($branchSelect.length > 0 && $branchSelect[0]) {
                $branchSelect.addClass('is-invalid');
                $branchSelect[0].style.setProperty('border', '2px solid #dc3545', 'important');
                $branchSelect[0].style.setProperty('border-color', '#dc3545', 'important');
            }
            isValid = false;
        }
    }
    
    // Validate Order Type
    var orderType = $('#ddlOrderType').val() || 'DineIn';
    
    // Validate Table - mandatory for DineIn, optional for Takeaway/Delivery
    var tableId = $('#ddlTableId').val();
    if (orderType === 'DineIn') {
        if (!tableId || tableId == '' || tableId == 0) {
            // Create error div if it doesn't exist
            if ($('#divTableId').length === 0) {
                $('#ddlTableId').closest('.form-group').append('<small class="text-red font-weight-bold errorText" id="divTableId"></small>');
            }
            $('#divTableId').text('Table is required for Dine In orders').show();
            // Fix Select2 border - apply style directly to the element
            var $tableSelect = $('#ddlTableId').next('.select2-container').find('.select2-selection');
            if ($tableSelect.length > 0 && $tableSelect[0]) {
                $tableSelect.addClass('is-invalid');
                $tableSelect[0].style.setProperty('border', '2px solid #dc3545', 'important');
                $tableSelect[0].style.setProperty('border-color', '#dc3545', 'important');
            }
            isValid = false;
        }
    }
    
    // Validate Guest Count
    var guestCount = parseInt($('#txtGuestCount').val()) || 0;
    if (!guestCount || guestCount <= 0) {
        // Create error div if it doesn't exist
        if ($('#divGuestCount').length === 0) {
            $('#txtGuestCount').closest('.form-group').append('<small class="text-red font-weight-bold errorText" id="divGuestCount"></small>');
        }
        $('#divGuestCount').text('Number of Guests is required and must be greater than 0').show();
        $('#txtGuestCount').css('border', '2px solid #dc3545');
        isValid = false;
    }
    
    // Validate Items - at least one item required
    var kotDetails = [];
    $('.kot-item').each(function () {
        var itemId = $(this).find('.kot-item-id').val() || 0;
        var itemDetailsId = $(this).find('.kot-item-details-id').val() || 0;
        var kotDetailsId = $(this).find('.kot-item-kot-details-id').val() || 0;
        // Get item name from hidden input field
        var itemName = $(this).find('.kot-item-name').val() || '';
        // Get unit from dropdown (get selected option value and text)
        var unitSelect = $(this).find('.kot-item-unit');
        var unitValue = unitSelect.val() || '';
        var unit = unitSelect.find('option:selected').text() || '';
        if (unit === '-') unit = '';
        
        // Extract UnitId from dropdown value (format: UnitId-PriceAddedFor-ConversionFactor)
        var unitId = 0;
        if (unitValue && unitValue.indexOf('-') > 0) {
            unitId = parseInt(unitValue.split('-')[0]) || 0;
        }
        
        // Validate quantity for each item
        var quantity = parseFloat($(this).find('.kot-item-qty').val()) || 0;
        if (quantity <= 0) {
            isValid = false;
            $(this).find('.kot-item-qty').css('border', '2px solid #dc3545');
        }
        
        kotDetails.push({
            KotDetailsId: kotDetailsId > 0 ? parseInt(kotDetailsId) : 0,
            ItemId: itemId > 0 ? parseInt(itemId) : 0,
            ItemDetailsId: itemDetailsId > 0 ? parseInt(itemDetailsId) : 0,
            Quantity: quantity,
            UnitId: unitId > 0 ? parseInt(unitId) : 0,
            CookingInstructions: $(this).find('.kot-item-instructions').val() || null,
            ItemStatus: 'Pending',
            Priority: 0
        });
    });
    
    if (kotDetails.length === 0) {
        $('#divtags').text('At least one item is required').show();
        $('#txttags').css('border', '2px solid #dc3545');
        isValid = false;
    }
    
    // If validation fails, show error and return
    if (!isValid) {
        if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
            if (document.getElementById('error')) document.getElementById('error').play();
        }
        if (typeof toastr !== 'undefined') {
            toastr.error('Please fill in all required fields correctly');
        }
        return;
    }
    
    var kotId = $('#KotId').val() || 0;
    var isEditMode = kotId > 0;
    
    // Get bookingId from hidden field or selected booking
    var bookingId = $('#BookingId').val() || $('#selectedBookingId').val() || null;
    if (bookingId == '' || bookingId == 0) bookingId = null;
    
    var det = {
        KotId: kotId,
        BookingId: bookingId,
        BranchId: parseInt(branchId) || 0,
        TableId: (tableId && tableId != '' && tableId != 0) ? parseInt(tableId) : null,
        OrderType: orderType,
        OrderStatus: isEditMode ? null : 'Pending', // Preserve existing status when editing
        GuestCount: guestCount,
        WaiterId: parseInt($('#ddlWaiterId').val()) || 0,
        SpecialInstructions: $('#txtSpecialInstructions').val() || null,
        KotDetails: kotDetails
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/kot/CreateStandalone',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                // Auto-print KOT if enabled
                if (data.Data && data.Data.ShouldAutoPrint && data.Data.Kot && data.Data.Kot.KotId) {
                    var printUrl = '/kot/print/' + data.Data.Kot.KotId;
                    var printWindow = window.open(printUrl, '_blank', 'width=400,height=600');
                    if (printWindow) {
                        printWindow.onload = function() {
                            setTimeout(function() {
                                printWindow.print();
                            }, 500);
                        };
                    }
                }
                
                // Store success message and redirect - message will be shown on index page
                sessionStorage.setItem('kotSuccessMessage', data.Message || 'KOT saved successfully');
                window.location.href = '/kot/index';
            } else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                $.each(data.Errors, function (index, value) {
                    $('#' + value.Id).text(value.Message).show();
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

function updateKotStatus(KotId, Status, isBackward) {
    // Confirmation messages for different status changes
    var confirmMessages = {
        'Preparing': isBackward ? 'Are you sure you want to move this KOT back to Preparing?' : 'Are you sure you want to mark this KOT as Preparing?',
        'Ready': isBackward ? 'Are you sure you want to move this KOT back to Ready?' : 'Are you sure you want to mark this KOT as Ready?',
        'Served': 'Are you sure you want to mark this KOT as Served?',
        'Pending': 'Are you sure you want to move this KOT back to Pending?',
        'Cancelled': 'Are you sure you want to cancel this KOT?'
    };
    
    // Show confirmation for status changes (except if already cancelled)
    if (confirmMessages[Status]) {
        if (!confirm(confirmMessages[Status])) return;
    }
    
    var det = {
        KotId: KotId,
        OrderStatus: Status
    };
    $("#divLoading").show();
    $.ajax({
        url: '/kot/UpdateKotStatus',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                window.location.reload();
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

function cancelKot(kotId) {
    // updateKotStatus will handle the confirmation
    updateKotStatus(kotId, 'Cancelled', false);
}

function deleteKot(kotId) {
    if (!confirm('Are you sure you want to delete this KOT? This action cannot be undone.')) {
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/kot/delete',
        datatype: "json",
        data: { kotId: kotId },
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('success')) document.getElementById('success').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.success(data.Message);
                // Reload the list
                fetchList(_PageIndex);
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
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while deleting KOT');
        }
    });
}

function updateItemStatus(KotDetailsId, Status, KotId) {
    // Get current status to determine if this is a backward transition
    var currentStatusBadge = $('#item-status-' + KotDetailsId);
    var currentStatus = currentStatusBadge.text().trim();
    
    // Confirmation messages for different status changes
    var confirmMessages = {
        'Preparing': (currentStatus == 'Ready') ? 'Are you sure you want to move this item back to Preparing?' : 'Are you sure you want to mark this item as Preparing?',
        'Ready': (currentStatus == 'Served') ? 'Are you sure you want to move this item back to Ready?' : 'Are you sure you want to mark this item as Ready?',
        'Served': 'Are you sure you want to mark this item as Served?',
        'Pending': 'Are you sure you want to move this item back to Pending?'
    };
    
    // Show confirmation for status changes
    if (confirmMessages[Status]) {
        if (!confirm(confirmMessages[Status])) return;
    }
    
    var det = {
        KotDetailsId: KotDetailsId,
        ItemStatus: Status
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/kot/UpdateItemStatus',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    document.getElementById('success').play(); 
                }
                toastr.success(data.Message);
                
                // Update the status badge in the UI
                var statusBadge = $('#item-status-' + KotDetailsId);
                var badgeClass = 'badge-secondary';
                if (Status == 'Served') badgeClass = 'badge-success';
                else if (Status == 'Ready') badgeClass = 'badge-info';
                else if (Status == 'Preparing') badgeClass = 'badge-warning';
                
                statusBadge.removeClass('badge-success badge-info badge-warning badge-secondary')
                           .addClass(badgeClass)
                           .text(Status);
                
                // Update action buttons - hide/show appropriate buttons based on new status
                var btnGroup = $('#item-actions-' + KotDetailsId);
                
                // Remove existing buttons
                btnGroup.empty();
                
                // Add buttons based on new status
                // Allow backward transitions except from Served (Option 2)
                if (Status == 'Pending' || Status == 'Printed' || !Status) {
                    // Can go to Preparing, Ready, or Served
                    btnGroup.append('<button type="button" class="btn btn-warning btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Preparing\', ' + KotId + ')" title="Mark as Preparing"><i class="fas fa-fire"></i></button>');
                    btnGroup.append('<button type="button" class="btn btn-success btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Ready\', ' + KotId + ')" title="Mark as Ready"><i class="fas fa-check"></i></button>');
                    btnGroup.append('<button type="button" class="btn btn-info btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Served\', ' + KotId + ')" title="Mark as Served"><i class="fas fa-check-double"></i></button>');
                } else if (Status == 'Preparing') {
                    // Can go back to Pending, forward to Ready or Served
                    btnGroup.append('<button type="button" class="btn btn-secondary btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Pending\', ' + KotId + ')" title="Back to Pending"><i class="fas fa-undo"></i></button>');
                    btnGroup.append('<button type="button" class="btn btn-success btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Ready\', ' + KotId + ')" title="Mark as Ready"><i class="fas fa-check"></i></button>');
                    btnGroup.append('<button type="button" class="btn btn-info btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Served\', ' + KotId + ')" title="Mark as Served"><i class="fas fa-check-double"></i></button>');
                } else if (Status == 'Ready') {
                    // Can go back to Preparing or forward to Served
                    btnGroup.append('<button type="button" class="btn btn-warning btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Preparing\', ' + KotId + ')" title="Back to Preparing"><i class="fas fa-undo"></i></button>');
                    btnGroup.append('<button type="button" class="btn btn-info btn-sm" onclick="updateItemStatus(' + KotDetailsId + ', \'Served\', ' + KotId + ')" title="Mark as Served"><i class="fas fa-check-double"></i></button>');
                } else if (Status == 'Served') {
                    // No buttons for served items (final state)
                    btnGroup.append('<span class="text-muted">-</span>');
                }
                
                // Check if we should update the whole KOT status
                checkAndUpdateKotStatus(KotId);
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    document.getElementById('error').play(); 
                }
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                document.getElementById('error').play(); 
            }
            toastr.error('An error occurred while updating item status');
        }
    });
}

function checkAndUpdateKotStatus(KotId) {
    // This function checks all item statuses and updates the whole KOT status accordingly
    // It's called after an item status is updated
    // For now, we'll just reload the page to show updated KOT status
    // In a more advanced implementation, you could fetch item statuses via AJAX and calculate the overall status
    
    // Optional: You can implement logic here to automatically update KOT status
    // For example: If all items are "Served", mark KOT as "Served"
    // If all items are "Ready", mark KOT as "Ready"
    // If any item is "Preparing", mark KOT as "Preparing"
    
    // For now, we'll reload after a short delay to show the updated status
    setTimeout(function() {
        window.location.reload();
    }, 1000);
}

function loadKitchenDisplay() {
    var StationId = $('#ddlStation').val() || 0;
    var CompanyId = 0;
    var BranchId = 0;
    
    // Get CompanyId and BranchId from cookies
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            CompanyId = parseInt(Cookies.get('data').split('&')[3].split('=')[1]) || 0;
            BranchId = parseInt(Cookies.get('data').split('&')[4].split('=')[1]) || 0;
        } catch (e) {
            console.error('Error parsing cookies:', e);
        }
    }
    
    $("#divLoading").show();
    $.ajax({
        url: '/kot/GetActiveKots',
        datatype: "json",
        data: JSON.stringify({ 
            CompanyId: CompanyId,
            BranchId: BranchId,
            KitchenStationId: StationId > 0 ? parseInt(StationId) : 0
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                // Check for new KOTs or status changes (sound notifications)
                if (typeof previousKots !== 'undefined' && previousKots.length > 0) {
                    var currentKotIds = data.Data.Kots.map(function(k) { return k.KotId; });
                    var previousKotIds = previousKots.map(function(k) { return k.KotId; });
                    
                    // Check for new KOTs
                    var newKots = data.Data.Kots.filter(function(k) {
                        return previousKotIds.indexOf(k.KotId) === -1;
                    });
                    
                    // Check for status changes
                    var statusChangedKots = data.Data.Kots.filter(function(k) {
                        var prevKot = previousKots.find(function(p) { return p.KotId === k.KotId; });
                        return prevKot && prevKot.OrderStatus !== k.OrderStatus;
                    });
                    
                    // Play sound if new KOTs or status changes detected
                    if ((newKots.length > 0 || statusChangedKots.length > 0) && typeof playKotNotificationSound === 'function') {
                        playKotNotificationSound();
                    }
                }
                
                // Store current state for next comparison
                previousKots = JSON.parse(JSON.stringify(data.Data.Kots));
                
                var html = '';
                $.each(data.Data.Kots, function (index, kot) {
                    html += '<div class="col-md-4 mb-3">';
                    html += '<div class="card card-info">';
                    html += '<div class="card-header"><h5>KOT: ' + kot.KotNo + ' | Table: ' + (kot.TableNo || '-') + '</h5></div>';
                    html += '<div class="card-body">';
                    // Show Special Instructions at KOT level if available
                    if (kot.SpecialInstructions && kot.SpecialInstructions.trim() !== '') {
                        html += '<div class="alert alert-light mb-2" style="padding: 8px; font-size: 0.9em; border-left: 3px solid #adb5bd; background-color: #f8f9fa;">';
                        html += '<strong style="color: #495057;"><i class="fas fa-exclamation-circle"></i> Special Instructions:</strong> <span style="color: #495057;">' + kot.SpecialInstructions + '</span>';
                        html += '</div>';
                    }
                    $.each(kot.KotDetails, function (idx, item) {
                        html += '<div class="mb-2 pb-2' + (idx < kot.KotDetails.length - 1 ? ' border-bottom' : '') + '">';
                        html += '<strong>' + item.ItemName + '</strong> x ' + item.Quantity;
                        html += '<span class="badge badge-' + (item.ItemStatus == 'Ready' ? 'success' : item.ItemStatus == 'Preparing' ? 'warning' : 'secondary') + ' float-right">' + item.ItemStatus + '</span>';
                        // Show Cooking Instructions for each item if available
                        if (item.CookingInstructions && item.CookingInstructions.trim() !== '') {
                            html += '<div class="mt-1" style="font-size: 0.85em; color: #856404; font-style: italic;">';
                            html += '<i class="fas fa-utensils"></i> <strong>Note:</strong> ' + item.CookingInstructions;
                            html += '</div>';
                        }
                        html += '</div>';
                    });
                    html += '</div><div class="card-footer">';
                    html += '<button class="btn btn-sm btn-warning" onclick="updateKotStatus(' + kot.KotId + ', \'Preparing\', false)">Preparing</button> ';
                    html += '<button class="btn btn-sm btn-success" onclick="updateKotStatus(' + kot.KotId + ', \'Ready\', false)">Ready</button> ';
                    html += '<button class="btn btn-sm btn-info" onclick="updateKotStatus(' + kot.KotId + ', \'Served\', false)">Served</button>';
                    html += '</div></div></div>';
                });
                $('#kotDisplayContainer').html(html);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function filterByStation() {
    loadKitchenDisplay();
}

function loadKotTablesByBranch(forceLoad) {
    // Only load tables if there's no booking, unless forceLoad is true
    var bookingId = $('#BookingId').val() || 0;
    if (bookingId > 0 && !forceLoad) {
        // If booking exists, don't reload tables - they're already set from booking
        return;
    }
    
    var branchId = $('#ddlBranch').val();
    if (!branchId || branchId == 0) {
        // Clear tables if no branch selected
        $('#ddlTableId').empty().append('<option value="">Select Table</option>');
        $('#ddlTableId').trigger('change');
        return;
    }
    
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/GetTables',
        datatype: "json",
        data: JSON.stringify({
            PageIndex: 1,
            PageSize: 1000,
            BranchId: branchId
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Tables) {
                // Clear existing options
                $('#ddlTableId').empty().append('<option value="">Select Table</option>');
                
                // Add tables for selected branch with status and capacity (matching POS behavior)
                $.each(data.Data.Tables, function(index, table) {
                    var statusText = table.Status || 'Available';
                    var statusClass = '';
                    if (statusText == 'Available') statusClass = 'text-success';
                    else if (statusText == 'Occupied') statusClass = 'text-danger';
                    else if (statusText == 'Reserved' || statusText == 'Booked') statusClass = 'text-warning';
                    else if (statusText == 'Maintenance') statusClass = 'text-secondary';
                    
                    var optionText = table.TableNo + (table.TableName ? ' - ' + table.TableName : '') + 
                        ' (Capacity: ' + (table.Capacity || 'N/A') + ')' + 
                        ' [' + statusText + ']';
                    
                    var $option = $('<option></option>')
                        .val(table.TableId)
                        .text(optionText)
                        .attr('data-status', statusText)
                        .addClass(statusClass);
                    
                    $('#ddlTableId').append($option);
                });
                
                // Reinitialize select2
                $('#ddlTableId').select2({
                    dropdownAutoWidth: true,
                    width: '100%',
                    templateResult: function(data) {
                        if (!data.id) return data.text;
                        var $result = $('<span></span>');
                        var status = $(data.element).attr('data-status');
                        var statusClass = $(data.element).attr('class') || '';
                        $result.append($('<span class="' + statusClass + '">' + data.text + '</span>'));
                        return $result;
                    }
                });
                
                // Trigger custom event when tables are loaded (for booking population)
                $(document).trigger('tablesLoaded');
            } else {
                $('#ddlTableId').empty().append('<option value="">No tables available</option>');
                $('#ddlTableId').trigger('change');
                $(document).trigger('tablesLoaded');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('Failed to load tables for selected branch');
            $('#ddlTableId').empty().append('<option value="">Error loading tables</option>');
            $('#ddlTableId').trigger('change');
            $(document).trigger('tablesLoaded');
        }
    });
}

function initKotAutocomplete() {
    if ($('#txttags').length === 0) {
        return; // Autocomplete field doesn't exist on this page
    }

    // Get BranchId from dropdown or cookies
    function getBranchId() {
        if ($('#ddlBranch').length > 0 && $('#ddlBranch').val()) {
            return $('#ddlBranch').val();
        } else if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
            try {
                return parseInt(Cookies.get('data').split('&')[4].split('=')[1]) || 0;
            } catch (e) {
                return 0;
            }
        }
        return 0;
    }

    $('#txttags').autocomplete({
        type: "POST",
        minLength: 3,
        source: function (request, response) {
            $.ajax({
                url: "/items/itemAutocomplete",
                dataType: "json",
                data: { 
                    Search: request.term, 
                    BranchId: getBranchId(), 
                    MenuType: 'print labels' 
                },
                success: function (data) {
                    $('.errorText').hide();
                    $('[style*="border: 2px"]').css('border', '');

                    if (data.Status == 0) {
                        if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                            if (document.getElementById('error')) document.getElementById('error').play(); 
                        }
                        if (typeof toastr !== 'undefined') toastr.error(data.Message);
                        $('#txttags').val('');
                    }
                    else if (data.Status == 2) {
                        if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                            if (document.getElementById('error')) document.getElementById('error').play(); 
                        }
                        if (typeof toastr !== 'undefined') toastr.error('Invalid inputs, check and try again !!');
                        if (data.Errors) {
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
                    }
                    else {
                        if (data.Data && data.Data.ItemsArray && data.Data.ItemsArray.length == 1) {
                            $('#txttags').val('');
                            // Reset autocomplete term to allow re-searching the same value
                            var autocompleteInstance = $('#txttags').data('ui-autocomplete') || $('#txttags').data('autocomplete');
                            if (autocompleteInstance) {
                                autocompleteInstance.term = '';
                            }
                            var splitVal = data.Data.ItemsArray[0].split('~');
                            fetchKotItem(splitVal[splitVal.length - 1]);
                            skuCodes.push(splitVal[splitVal.length - 1]);
                        }
                        else if (data.Data && data.Data.ItemsArray) {
                            response(data.Data.ItemsArray);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                        if (document.getElementById('error')) document.getElementById('error').play(); 
                    }
                    if (typeof toastr !== 'undefined') toastr.error('An error occurred while searching items');
                }
            });
        },
        select: function (event, ui) {
            var splitVal = ui.item.value.split('~');
            fetchKotItem(splitVal[splitVal.length - 1]);
            skuCodes.push(splitVal[splitVal.length - 1]);
        }
    });

    // Reset autocomplete term when input is cleared manually to allow re-searching the same value
    $('#txttags').on('input', function() {
        if ($(this).val() === '') {
            var autocompleteInstance = $(this).data('ui-autocomplete') || $(this).data('autocomplete');
            if (autocompleteInstance) {
                autocompleteInstance.term = '';
            }
        }
    });
}

function fetchKotItem(SkuHsnCode) {
    // Get BranchId from dropdown or cookies
    function getBranchId() {
        if ($('#ddlBranch').length > 0 && $('#ddlBranch').val()) {
            return $('#ddlBranch').val();
        } else if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
            try {
                return parseInt(Cookies.get('data').split('&')[4].split('=')[1]) || 0;
            } catch (e) {
                return 0;
            }
        }
        return 0;
    }

    var det = {
        ItemCode: SkuHsnCode,
        BranchId: getBranchId(),
        CustomerId: 0,
        SellingPriceGroupId: 0,
        MenuType: 'sale',
        PlaceOfSupplyId: 0,
        Type: "Sales",
        IsBillOfSupply: false
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/items/SearchItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $('#txttags').val('');
            // Reset autocomplete term to allow re-searching the same value
            var autocompleteInstance = $('#txttags').data('ui-autocomplete') || $('#txttags').data('autocomplete');
            if (autocompleteInstance) {
                autocompleteInstance.term = '';
            }
            
            if (data.Status == 1 && data.Data && data.Data.ItemDetails && data.Data.ItemDetails.length > 0) {
                // Add the first item (or all items if multiple variations)
                for (let i = 0; i < data.Data.ItemDetails.length; i++) {
                    var itemDetail = data.Data.ItemDetails[i];
                    var itemData = {
                        ItemId: itemDetail.ItemId,
                        ItemDetailsId: itemDetail.ItemDetailsId || 0,
                        ItemName: itemDetail.ItemName,
                        SKU: itemDetail.SKU,
                        UnitShortName: itemDetail.UnitShortName || '',
                        UnitId: itemDetail.UnitId || 0,
                        SecondaryUnitId: itemDetail.SecondaryUnitId || 0,
                        SecondaryUnitShortName: itemDetail.SecondaryUnitShortName || '',
                        TertiaryUnitId: itemDetail.TertiaryUnitId || 0,
                        TertiaryUnitShortName: itemDetail.TertiaryUnitShortName || '',
                        QuaternaryUnitId: itemDetail.QuaternaryUnitId || 0,
                        QuaternaryUnitShortName: itemDetail.QuaternaryUnitShortName || '',
                        PriceAddedFor: itemDetail.PriceAddedFor || 1
                    };
                    addKotItem(itemData);
                }
            } else {
                if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                if (typeof toastr !== 'undefined') toastr.error(data.Message || 'Item not found');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while fetching item details');
        }
    });
}

function loadExistingKotItems(kotDetails) {
    // Clear existing items first
    $('#kotItemsContainer').empty();
    kotItemCounter = 0;
    
    if (!kotDetails || kotDetails.length === 0) {
        return;
    }
    
    // Get BranchId for fetching item details
    function getBranchId() {
        if ($('#ddlBranch').length > 0 && $('#ddlBranch').val()) {
            return $('#ddlBranch').val();
        } else if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
            try {
                return parseInt(Cookies.get('data').split('&')[4].split('=')[1]) || 0;
            } catch (e) {
                return 0;
            }
        }
        return 0;
    }
    
    // Load each existing KOT detail item
    $.each(kotDetails, function(index, detail) {
        // Fetch item details to get unit information
        var det = {
            ItemCode: detail.ItemCode || '',
            BranchId: getBranchId(),
            CustomerId: 0,
            SellingPriceGroupId: 0,
            MenuType: 'sale',
            PlaceOfSupplyId: 0,
            Type: "Sales",
            IsBillOfSupply: false
        };
        
        // If we have ItemId, try to get item details
        if (detail.ItemId > 0) {
            $.ajax({
                url: '/items/SearchItems',
                datatype: "json",
                data: det,
                type: "post",
                async: false, // Synchronous to maintain order
                success: function (data) {
                    if (data.Status == 1 && data.Data && data.Data.ItemDetails && data.Data.ItemDetails.length > 0) {
                        // Find matching item detail
                        var itemDetail = null;
                        if (detail.ItemDetailsId > 0) {
                            itemDetail = data.Data.ItemDetails.find(function(id) {
                                return id.ItemDetailsId == detail.ItemDetailsId;
                            });
                        }
                        if (!itemDetail && data.Data.ItemDetails.length > 0) {
                            itemDetail = data.Data.ItemDetails[0];
                        }
                        
                        if (itemDetail) {
                            var itemData = {
                                ItemId: detail.ItemId,
                                ItemDetailsId: detail.ItemDetailsId || 0,
                                ItemName: detail.ItemName || itemDetail.ItemName,
                                SKU: detail.ItemCode || itemDetail.SKU,
                                UnitShortName: itemDetail.UnitShortName || '',
                                UnitId: detail.UnitId > 0 ? detail.UnitId : (itemDetail.UnitId || 0),
                                SecondaryUnitId: itemDetail.SecondaryUnitId || 0,
                                SecondaryUnitShortName: itemDetail.SecondaryUnitShortName || '',
                                TertiaryUnitId: itemDetail.TertiaryUnitId || 0,
                                TertiaryUnitShortName: itemDetail.TertiaryUnitShortName || '',
                                QuaternaryUnitId: itemDetail.QuaternaryUnitId || 0,
                                QuaternaryUnitShortName: itemDetail.QuaternaryUnitShortName || '',
                                PriceAddedFor: itemDetail.PriceAddedFor || 1,
                                Quantity: detail.Quantity || 1,
                                CookingInstructions: detail.CookingInstructions || '',
                                KotDetailsId: detail.KotDetailsId || 0
                            };
                            addKotItemWithData(itemData);
                        }
                    } else {
                        // Fallback: add item with available data
                        var itemData = {
                            ItemId: detail.ItemId,
                            ItemDetailsId: detail.ItemDetailsId || 0,
                            ItemName: detail.ItemName || 'Unknown Item',
                            SKU: detail.ItemCode || '',
                            UnitShortName: detail.Unit || '',
                            UnitId: detail.UnitId || 0,
                            SecondaryUnitId: 0,
                            SecondaryUnitShortName: '',
                            TertiaryUnitId: 0,
                            TertiaryUnitShortName: '',
                            QuaternaryUnitId: 0,
                            QuaternaryUnitShortName: '',
                            PriceAddedFor: 1,
                            Quantity: detail.Quantity || 1,
                            CookingInstructions: detail.CookingInstructions || '',
                            KotDetailsId: detail.KotDetailsId || 0
                        };
                        addKotItemWithData(itemData);
                    }
                },
                error: function (xhr) {
                    // Fallback: add item with available data
                    var itemData = {
                        ItemId: detail.ItemId,
                        ItemDetailsId: detail.ItemDetailsId || 0,
                        ItemName: detail.ItemName || 'Unknown Item',
                        SKU: detail.ItemCode || '',
                        UnitShortName: detail.Unit || '',
                        UnitId: detail.UnitId || 0,
                        SecondaryUnitId: 0,
                        SecondaryUnitShortName: '',
                        TertiaryUnitId: 0,
                        TertiaryUnitShortName: '',
                        QuaternaryUnitId: 0,
                        QuaternaryUnitShortName: '',
                        PriceAddedFor: 1,
                        Quantity: detail.Quantity || 1,
                        CookingInstructions: detail.CookingInstructions || '',
                        KotDetailsId: detail.KotDetailsId || 0
                    };
                    addKotItemWithData(itemData);
                }
            });
        } else {
            // Fallback: add item with available data
            var itemData = {
                ItemId: detail.ItemId || 0,
                ItemDetailsId: detail.ItemDetailsId || 0,
                ItemName: detail.ItemName || 'Unknown Item',
                SKU: detail.ItemCode || '',
                UnitShortName: detail.Unit || '',
                UnitId: detail.UnitId || 0,
                SecondaryUnitId: 0,
                SecondaryUnitShortName: '',
                TertiaryUnitId: 0,
                TertiaryUnitShortName: '',
                QuaternaryUnitId: 0,
                QuaternaryUnitShortName: '',
                PriceAddedFor: 1,
                Quantity: detail.Quantity || 1,
                CookingInstructions: detail.CookingInstructions || '',
                KotDetailsId: detail.KotDetailsId || 0
            };
            addKotItemWithData(itemData);
        }
    });
}

function addKotItemWithData(itemData) {
    kotItemCounter++;
    var itemName = itemData ? itemData.ItemName : '';
    var itemId = itemData ? itemData.ItemId : 0;
    var itemDetailsId = itemData ? (itemData.ItemDetailsId || 0) : 0;
    var skuCode = itemData ? itemData.SKU : '';
    var unit = itemData ? (itemData.UnitShortName || '') : '';
    var unitId = itemData ? (itemData.UnitId || 0) : 0;
    var secondaryUnitId = itemData ? (itemData.SecondaryUnitId || 0) : 0;
    var secondaryUnitShortName = itemData ? (itemData.SecondaryUnitShortName || '') : '';
    var tertiaryUnitId = itemData ? (itemData.TertiaryUnitId || 0) : 0;
    var tertiaryUnitShortName = itemData ? (itemData.TertiaryUnitShortName || '') : '';
    var quaternaryUnitId = itemData ? (itemData.QuaternaryUnitId || 0) : 0;
    var quaternaryUnitShortName = itemData ? (itemData.QuaternaryUnitShortName || '') : '';
    var priceAddedFor = itemData ? (itemData.PriceAddedFor || 1) : 1;
    var quantity = itemData ? (itemData.Quantity || 1) : 1;
    var cookingInstructions = itemData ? (itemData.CookingInstructions || '') : '';
    var kotDetailsId = itemData ? (itemData.KotDetailsId || 0) : 0;
    
    // Format item name display
    var itemNameDisplay = '';
    if (itemName.length > 15) {
        itemNameDisplay = '<span><b>Name :</b> ' + itemName.substring(0, 15) + '...</span>';
    } else {
        itemNameDisplay = '<span><b>Name :</b> ' + itemName + '</span>';
    }
    
    // Build unit dropdown - only show units configured for this item
    var ddlUnit = '<select style="min-width:80px" class="form-control kot-item-unit ' + (unitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + kotItemCounter + '">';
    
    // Determine which unit to select based on the existing unitId
    var selectedUnitId = unitId;
    var selectedPriceAddedFor = priceAddedFor;
    
    if (quaternaryUnitId != 0) {
        // Item has 4 units
        if (selectedUnitId == unitId) {
            ddlUnit += '<option selected value="' + unitId + '-1-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-1-1">' + unit + '</option>';
        }
        if (selectedUnitId == secondaryUnitId) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-2-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-2-2">' + secondaryUnitShortName + '</option>';
        }
        if (selectedUnitId == tertiaryUnitId) {
            ddlUnit += '<option selected value="' + tertiaryUnitId + '-3-3">' + tertiaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + tertiaryUnitId + '-3-3">' + tertiaryUnitShortName + '</option>';
        }
        if (selectedUnitId == quaternaryUnitId) {
            ddlUnit += '<option selected value="' + quaternaryUnitId + '-4-4">' + quaternaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + quaternaryUnitId + '-4-4">' + quaternaryUnitShortName + '</option>';
        }
    } else if (tertiaryUnitId != 0) {
        // Item has 3 units
        if (selectedUnitId == unitId) {
            ddlUnit += '<option selected value="' + unitId + '-2-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-2-1">' + unit + '</option>';
        }
        if (selectedUnitId == secondaryUnitId) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-3-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-3-2">' + secondaryUnitShortName + '</option>';
        }
        if (selectedUnitId == tertiaryUnitId) {
            ddlUnit += '<option selected value="' + tertiaryUnitId + '-4-3">' + tertiaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + tertiaryUnitId + '-4-3">' + tertiaryUnitShortName + '</option>';
        }
    } else if (secondaryUnitId != 0) {
        // Item has 2 units
        if (selectedUnitId == unitId) {
            ddlUnit += '<option selected value="' + unitId + '-3-1">' + unit + '</option>';
        } else {
            ddlUnit += '<option value="' + unitId + '-3-1">' + unit + '</option>';
        }
        if (selectedUnitId == secondaryUnitId) {
            ddlUnit += '<option selected value="' + secondaryUnitId + '-4-2">' + secondaryUnitShortName + '</option>';
        } else {
            ddlUnit += '<option value="' + secondaryUnitId + '-4-2">' + secondaryUnitShortName + '</option>';
        }
    } else {
        // Item has only 1 unit
        ddlUnit += '<option selected value="' + unitId + '-4-1">' + (unit || 'N/A') + '</option>';
    }
    
    ddlUnit += '</select>';
    
    var html = '<tr class="kot-item" data-item-index="' + kotItemCounter + '">';
    html += '<td style="min-width:100px; white-space: nowrap;" data-item-name="' + itemName.replace(/"/g, '&quot;') + '">';
    html += '<input type="hidden" class="kot-item-id" value="' + itemId + '">';
    html += '<input type="hidden" class="kot-item-details-id" value="' + itemDetailsId + '">';
    html += '<input type="hidden" class="kot-item-sku" value="' + skuCode + '">';
    html += '<input type="hidden" class="kot-item-name" value="' + itemName.replace(/"/g, '&quot;') + '">';
    html += '<input type="hidden" class="kot-item-kot-details-id" value="' + kotDetailsId + '">';
    html += itemNameDisplay;
    html += ' <br/> <b>SKU :</b> ' + skuCode;
    html += '</td>';
    html += '<td>';
    html += '<div class="input-group" style="min-width:120px">';
    html += '<input type="number" class="form-control kot-item-qty" placeholder="Qty" min="1" value="' + quantity + '" required style="width:60px">';
    html += ddlUnit;
    html += '</div>';
    html += '</td>';
    html += '<td><textarea class="form-control kot-item-instructions" placeholder="Cooking Instructions" rows="1">' + cookingInstructions + '</textarea></td>';
    html += '<td><button type="button" class="btn btn-sm btn-danger" onclick="removeKotItem(' + kotItemCounter + ')"><i class="fas fa-times"></i></button></td>';
    html += '</tr>';
    $('#kotItemsContainer').append(html);
    
    // Clear item validation error when item is added
    $('#divtags').hide();
    $('#txttags').css('border', '');
    
    // Set the correct unit selection
    if (selectedUnitId > 0) {
        var unitSelect = $('#ddlUnit' + kotItemCounter);
        // Find and select the option matching the unitId
        unitSelect.find('option').each(function() {
            var optionValue = $(this).val();
            if (optionValue && optionValue.indexOf('-') > 0) {
                var optionUnitId = parseInt(optionValue.split('-')[0]);
                if (optionUnitId == selectedUnitId) {
                    $(this).prop('selected', true);
                    return false; // break
                }
            }
        });
        unitSelect.trigger('change');
    }
}

function convertKotToSales(kotId) {
    if (!confirm('Convert this served KOT to a sales invoice? This will create a new POS invoice with all items from the KOT.')) {
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/kot/convertkotstosalesaction',
        datatype: "json",
        data: { kotIds: [kotId] },
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') document.getElementById('success').play();
                toastr.success(data.Message);
                setTimeout(function () {
                    window.location.reload();
                }, 1000);
            } else {
                if (EnableSound == 'True') document.getElementById('error').play();
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') document.getElementById('error').play();
            toastr.error('An error occurred while converting KOT to sales');
        }
    });
}

function convertBookingKotsToSales(bookingId) {
    if (!confirm('Convert all served KOTs from this booking to a single sales invoice? This will consolidate all items from multiple KOTs into one bill.')) {
        return;
    }

    $("#divLoading").show();
    $.ajax({
        url: '/kot/convertbookingkotstosalesaction',
        datatype: "json",
        data: { bookingId: bookingId },
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') document.getElementById('success').play();
                toastr.success(data.Message);
                setTimeout(function () {
                    window.location.reload();
                }, 1000);
            } else {
                if (EnableSound == 'True') document.getElementById('error').play();
                toastr.error(data.Message);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') document.getElementById('error').play();
            toastr.error('An error occurred while converting booking KOTs to sales');
        }
    });
}

// Booking search and selection functions for KOT Add page
function openBookingSearchModal() {
    $('#bookingSearchModal').modal('show');
    // Clear previous search results
    $('#modalBookingsList').html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> Use the search form above to find bookings.</div>');
}

// Store booking search results for later use
var bookingSearchResults = [];

function searchBookingsForKot() {
    var det = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt(Cookies.get('data').split('&')[4].split('=')[1]),
        Search: $('#modalTxtBookingNo').val() || null,
        FromDate: $('#modalTxtFromDate').val() ? new Date($('#modalTxtFromDate').val()) : null,
        ToDate: $('#modalTxtToDate').val() ? new Date($('#modalTxtToDate').val()) : null,
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
                // Store results for later use
                bookingSearchResults = data.Data.Bookings;
                
                var html = '<div class="table-responsive"><table class="table table-bordered table-striped">';
                html += '<thead><tr><th>Booking No</th><th>Table</th><th>Customer</th><th>Date</th><th>Time</th><th>Status</th><th>Action</th></tr></thead>';
                html += '<tbody>';
                $.each(data.Data.Bookings, function (index, booking) {
                    // Format date
                    var formattedDate = '-';
                    if (booking.BookingDate) {
                        try {
                            var dateObj;
                            // Handle different date formats
                            if (typeof booking.BookingDate === 'string') {
                                // Check if it's a timestamp format like "/Date(1234567890)/"
                                var timestampMatch = /\/Date\((\d+)\)\//.exec(booking.BookingDate);
                                if (timestampMatch) {
                                    dateObj = new Date(parseInt(timestampMatch[1]));
                                } else {
                                    dateObj = new Date(booking.BookingDate);
                                }
                            } else if (booking.BookingDate instanceof Date) {
                                dateObj = booking.BookingDate;
                            } else {
                                dateObj = new Date(booking.BookingDate);
                            }
                            
                            if (!isNaN(dateObj.getTime())) {
                                var day = String(dateObj.getDate()).padStart(2, '0');
                                var month = String(dateObj.getMonth() + 1).padStart(2, '0');
                                var year = dateObj.getFullYear();
                                var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
                                formattedDate = day + '-' + monthNames[dateObj.getMonth()] + '-' + year;
                            }
                        } catch (e) {
                            console.error('Error formatting date:', e, booking.BookingDate);
                            formattedDate = '-';
                        }
                    }
                    
                    // Format time
                    var formattedTime = '-';
                    if (booking.BookingTime) {
                        try {
                            // BookingTime can be a TimeSpan object with Days, Hours, Minutes, Seconds
                            // or a string in format "HH:mm:ss" or "HH:mm"
                            if (typeof booking.BookingTime === 'string') {
                                // If it's a string, try to parse it (format: "HH:mm:ss" or "HH:mm")
                                var timeParts = booking.BookingTime.split(':');
                                if (timeParts.length >= 2) {
                                    var hours = parseInt(timeParts[0]) || 0;
                                    var minutes = parseInt(timeParts[1]) || 0;
                                    formattedTime = String(hours).padStart(2, '0') + ':' + String(minutes).padStart(2, '0');
                                } else {
                                    formattedTime = booking.BookingTime;
                                }
                            } else if (typeof booking.BookingTime === 'object' && booking.BookingTime !== null) {
                                // If it's a TimeSpan object (from .NET)
                                // TimeSpan serializes as object with properties: Days, Hours, Minutes, Seconds, Milliseconds, Ticks, TotalDays, TotalHours, etc.
                                var hours = booking.BookingTime.Hours || booking.BookingTime.hours || 0;
                                var minutes = booking.BookingTime.Minutes || booking.BookingTime.minutes || 0;
                                // Handle total hours if Days property exists
                                if (booking.BookingTime.Days !== undefined || booking.BookingTime.days !== undefined) {
                                    hours += (booking.BookingTime.Days || booking.BookingTime.days || 0) * 24;
                                }
                                formattedTime = String(hours).padStart(2, '0') + ':' + String(minutes).padStart(2, '0');
                            } else if (typeof booking.BookingTime === 'number') {
                                // If it's a number (ticks or total milliseconds)
                                var totalSeconds = Math.floor(booking.BookingTime / 1000);
                                var hours = Math.floor(totalSeconds / 3600);
                                var minutes = Math.floor((totalSeconds % 3600) / 60);
                                formattedTime = String(hours).padStart(2, '0') + ':' + String(minutes).padStart(2, '0');
                            } else {
                                formattedTime = String(booking.BookingTime);
                            }
                        } catch (e) {
                            console.error('Error formatting time:', e, booking.BookingTime);
                            formattedTime = '-';
                        }
                    }
                    
                    html += '<tr>';
                    html += '<td>' + (booking.BookingNo || '-') + '</td>';
                    html += '<td>' + (booking.TableNo || 'Not Assigned') + '</td>';
                    html += '<td>' + (booking.CustomerName || '-') + '</td>';
                    html += '<td>' + formattedDate + '</td>';
                    html += '<td>' + formattedTime + '</td>';
                    html += '<td><span class="badge badge-' + (booking.Status == 'Confirmed' ? 'success' : booking.Status == 'Pending' ? 'warning' : booking.Status == 'Cancelled' ? 'danger' : 'secondary') + '">' + (booking.Status || '-') + '</span></td>';
                    html += '<td><a href="javascript:void(0)" onclick="selectBookingForKot(' + index + ')" class="btn btn-sm btn-success"><i class="fas fa-check"></i> Select</a></td>';
                    html += '</tr>';
                });
                html += '</tbody></table></div>';
                $('#modalBookingsList').html(html);
            } else {
                $('#modalBookingsList').html('<div class="alert alert-warning"><i class="fas fa-exclamation-triangle"></i> No bookings found.</div>');
                bookingSearchResults = [];
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') { 
                if (document.getElementById('error')) document.getElementById('error').play(); 
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while searching bookings');
            bookingSearchResults = [];
        }
    });
}

function selectBookingForKot(index) {
    if (!bookingSearchResults || !bookingSearchResults[index]) {
        if (typeof toastr !== 'undefined') toastr.error('Booking data not found. Please search again.');
        return;
    }
    
    var booking = bookingSearchResults[index];
    
    // Store booking ID
    $('#selectedBookingId').val(booking.BookingId);
    $('#BookingId').val(booking.BookingId);
    
    // Update display
    var displayText = (booking.BookingNo || '') + ' - ' + (booking.CustomerName || '');
    $('#txtSelectedBooking').val(displayText);
    $('#btnClearBooking').show();
    
    // Close modal
    $('#bookingSearchModal').modal('hide');
    
    // Auto-populate fields from booking data
    populateFieldsFromBooking(booking);
}

function populateFieldsFromBooking(booking) {
    if (!booking) return;
    
    // Store the table ID to set after tables are loaded
    // Check both TableId (primary) and TableIds array (for backward compatibility)
    var tableIdToSet = null;
    if (booking.TableId && booking.TableId > 0) {
        tableIdToSet = booking.TableId;
    } else if (booking.TableIds && Array.isArray(booking.TableIds) && booking.TableIds.length > 0 && booking.TableIds[0] > 0) {
        tableIdToSet = booking.TableIds[0]; // Use first table from array
    }
    
    // Auto-populate guest count
    if (booking.NoOfGuests && booking.NoOfGuests > 0) {
        $('#txtGuestCount').val(booking.NoOfGuests);
    }
    
    // Auto-populate special instructions
    if (booking.SpecialRequest) {
        $('#txtSpecialInstructions').val(booking.SpecialRequest);
    }
    
    // Auto-populate waiter from booking (industry standard: one waiter per booking)
    if (booking.WaiterId && booking.WaiterId > 0 && $('#ddlWaiterId').length > 0) {
        $('#ddlWaiterId').val(booking.WaiterId).trigger('change');
    }
    
    // Set branch if available and different
    if (booking.BranchId && booking.BranchId > 0 && $('#ddlBranch').length > 0) {
        var currentBranch = $('#ddlBranch').val();
        if (currentBranch != booking.BranchId) {
            // Branch needs to change - load tables for new branch first
            // Set up a one-time listener for when tables are loaded
            var tableSetHandler = function() {
                $(document).off('tablesLoaded', tableSetHandler);
                if (tableIdToSet) {
                    // Check if the table option exists in the dropdown
                    if ($('#ddlTableId option[value="' + tableIdToSet + '"]').length > 0) {
                        $('#ddlTableId').val(tableIdToSet).trigger('change');
                    } else {
                        console.warn('Table ID ' + tableIdToSet + ' not found in dropdown for branch ' + booking.BranchId);
                    }
                }
            };
            $(document).on('tablesLoaded', tableSetHandler);
            
            // Change branch and force load tables
            $('#ddlBranch').val(booking.BranchId);
            loadKotTablesByBranch(true); // Force load even with booking ID set
        } else {
            // Branch is the same - check if tables are already loaded
            if (tableIdToSet) {
                // Check if the table option exists
                if ($('#ddlTableId option[value="' + tableIdToSet + '"]').length > 0) {
                    $('#ddlTableId').val(tableIdToSet).trigger('change');
                } else {
                    // Tables might not be loaded yet, wait for them
                    var tableSetHandler = function() {
                        $(document).off('tablesLoaded', tableSetHandler);
                        if (tableIdToSet && $('#ddlTableId option[value="' + tableIdToSet + '"]').length > 0) {
                            $('#ddlTableId').val(tableIdToSet).trigger('change');
                        }
                    };
                    $(document).on('tablesLoaded', tableSetHandler);
                    // Trigger table load if needed
                    loadKotTablesByBranch(true);
                }
            }
        }
    } else if (tableIdToSet) {
        // No branch change needed, but check if tables are loaded
        if ($('#ddlTableId option[value="' + tableIdToSet + '"]').length > 0) {
            $('#ddlTableId').val(tableIdToSet).trigger('change');
        } else {
            // Wait a bit for tables to load
            setTimeout(function() {
                if ($('#ddlTableId option[value="' + tableIdToSet + '"]').length > 0) {
                    $('#ddlTableId').val(tableIdToSet).trigger('change');
                }
            }, 300);
        }
    }
    
    // Set order type to DineIn if it's a table booking
    if (booking.BookingType && (booking.BookingType.toLowerCase().indexOf('dine') >= 0 || booking.BookingType.toLowerCase().indexOf('table') >= 0)) {
        $('#ddlOrderType').val('DineIn').trigger('change');
    }
    
    if (typeof toastr !== 'undefined') {
        toastr.success('Booking selected. Fields have been auto-populated.');
    }
}

function clearSelectedBooking() {
    if (confirm('Are you sure you want to clear the booking selection? The booking link will be removed, but current field values will be kept.')) {
        $('#selectedBookingId').val('');
        $('#BookingId').val('');
        $('#txtSelectedBooking').val('No booking selected');
        $('#btnClearBooking').hide();
        
        // Clear the booking search results cache
        bookingSearchResults = [];
        
        if (typeof toastr !== 'undefined') {
            toastr.info('Booking selection cleared. Field values are preserved.');
        }
    }
}

function clearBookingSearch() {
    $('#modalTxtBookingNo').val('');
    $('#modalTxtFromDate').val('');
    $('#modalTxtToDate').val('');
    $('#modalBookingsList').html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> Use the search form above to find bookings.</div>');
}


