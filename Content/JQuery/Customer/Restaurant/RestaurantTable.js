var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val() || 10,
        Search: $('#txtSearch').val(),
        BranchId: $('#ddlBranch').val() || 0,
        FloorId: $('#ddlFloor').val() || null,
        TableTypeId: $('#ddlTableType').val() || null,
        Status: $('#ddlStatus').val() || null
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/index',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblTables").html($(data).find("#tblTables").html());
            var totalText = $(data).find("#divTotalCount").text() || "Total : 0";
            var activeText = $(data).find("#divActiveCount").text() || "Active : 0";
            var inactiveText = $(data).find("#divInactiveCount").text() || "In-Active : 0";
            $("#divTotalCount").text("Total : " + (totalText.toString().replace(/^[^:]*:\s*/, "") || 0));
            $("#divActiveCount").text("Active : " + (activeText.toString().replace(/^[^:]*:\s*/, "") || 0));
            $("#divInactiveCount").text("In-Active : " + (inactiveText.toString().replace(/^[^:]*:\s*/, "") || 0));
            // Reinitialize toggle switches
            $('.chkIsActive').bootstrapToggle();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function saveTable(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    var det = {
        TableId: $('#TableId').val() || 0,
        TableNo: $('#txtTableNo').val(),
        TableName: $('#txtTableName').val(),
        TableSlug: $('#txtTableSlug').val() || '',
        Capacity: parseInt($('#txtCapacity').val()) || 0,
        FloorId: $('#ddlFloorId').val() || null,
        TableTypeId: $('#ddlTableTypeId').val() || null,
        PositionX: $('#txtPositionX').val() ? parseInt($('#txtPositionX').val()) : null,
        PositionY: $('#txtPositionY').val() ? parseInt($('#txtPositionY').val()) : null,
        IsActive: true,
        BranchId: $('#ddlBranchId').val() || null,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/InsertRestaurantTable',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (i == 2) {
                    // Redirect back to add page for "Save & Add Another"
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message || 'Table saved successfully');
                    window.location.href = '/restauranttable/add';
                } else {
                    // Redirect to list page (for Save/Update) - message will be shown on index page
                    var successMessage = encodeURIComponent(data.Message || 'Table saved successfully');
                    window.location.href = '/restauranttable/index?success=' + successMessage;
                }
            } else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                $.each(data.Errors, function (index, value) {
                    $('#' + value.Id).text(value.Message).show();
                    
                    // Add red border to corresponding input field
                    if (value.Id === 'divTableSlug') {
                        $('#txtTableSlug').css('border', '2px solid #dc3545');
                    } else if (value.Id === 'divTableNo') {
                        $('#txtTableNo').css('border', '2px solid #dc3545');
                    } else if (value.Id === 'divBranchId') {
                        // Handle select2 dropdown - apply border to select2 container
                        var branchSelect = $('#ddlBranchId');
                        if (branchSelect.hasClass('select2-hidden-accessible')) {
                            branchSelect.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            branchSelect.css('border', '2px solid #dc3545');
                        }
                    } else if (value.Id === 'divCapacity') {
                        $('#txtCapacity').css('border', '2px solid #dc3545');
                    }
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

function deleteTable(TableId) {
    if (confirm('Are you sure you want to delete this table?')) {
        var det = {
            TableId: TableId,
            BranchId: null,
            CompanyId: null,
            AddedBy: null
        };
        $("#divLoading").show();
        $.ajax({
            url: '/restauranttable/RestaurantTableDelete',
            datatype: "json",
            data: JSON.stringify(det),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 1) {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchList(1);
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

function setTableActive(TableId, isActive, branchId) {
    var det = {
        TableId: TableId,
        IsActive: isActive,
        BranchId: branchId || null,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/RestaurantTableActiveInactive',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList(_PageIndex);
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

function ActiveInactive(TableId, IsActive, BranchId) {
    var det = {
        TableId: TableId,
        IsActive: IsActive,
        BranchId: BranchId || null,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/RestaurantTableActiveInactive',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList(_PageIndex);
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

function loadTableLayout() {
    var FloorId = $('#ddlFloor').val() || 0;
    if (FloorId == 0) {
        $('#tableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">Please select a floor to view table layout</p></div>');
        return;
    }
    
    $("#divLoading").show();
    var det = {
        PageIndex: 1,
        PageSize: 1000,
        FloorId: FloorId || null,
        BranchId: 0
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
                renderTableLayout(data.Data.Tables);
            } else {
                $('#tableLayoutCanvas').html('<div class="text-center p-5"><p class="text-muted">No tables found for this floor</p></div>');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to load table layout');
            $('#tableLayoutCanvas').html('<div class="text-center p-5"><p class="text-danger">Error loading table layout</p></div>');
        }
    });
}

var tableDataCache = {};

function renderTableLayout(tables) {
    var canvas = $('#tableLayoutCanvas');
    canvas.empty();
    tableDataCache = {};
    
    // Filter tables for selected floor and active tables only
    var floorId = $('#ddlFloor').val() || 0;
    var floorTables = tables.filter(function(t) {
        return (floorId == 0 || t.FloorId == floorId) && t.IsActive && !t.IsDeleted;
    });
    
    if (floorTables.length === 0) {
        canvas.html('<div class="text-center p-5"><p class="text-muted">No active tables found for this floor</p></div>');
        return;
    }
    
    // Create draggable table elements
    $.each(floorTables, function(index, table) {
        // Store full table data for later use
        tableDataCache[table.TableId] = table;
        
        var statusClass = getStatusClass(table.Status);
        var posX = table.PositionX || (index % 10) * 120 + 20;
        var posY = table.PositionY || Math.floor(index / 10) * 120 + 20;
        
        var tableElement = $('<div>', {
            'class': 'table-item',
            'data-table-id': table.TableId,
            'data-table-no': table.TableNo,
            'css': {
                'position': 'absolute',
                'left': posX + 'px',
                'top': posY + 'px',
                'width': '100px',
                'height': '80px',
                'border': '2px solid #333',
                'border-radius': '8px',
                'background-color': getStatusColor(table.Status),
                'color': '#fff',
                'text-align': 'center',
                'display': 'flex',
                'flex-direction': 'column',
                'justify-content': 'center',
                'align-items': 'center',
                'cursor': 'move',
                'z-index': 10,
                'box-shadow': '0 2px 4px rgba(0,0,0,0.2)'
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
        
        tableElement.attr('title', table.TableNo + (table.TableName ? ' - ' + table.TableName : '') + ' (Capacity: ' + table.Capacity + ')');
        
        canvas.append(tableElement);
    });
    
    // Make tables draggable
    canvas.find('.table-item').draggable({
        containment: 'parent',
        cursor: 'move',
        stop: function(event, ui) {
            // Update position data attribute
            $(this).data('pos-x', ui.position.left);
            $(this).data('pos-y', ui.position.top);
        }
    });
}

function getStatusClass(status) {
    if (!status) return 'success'; // Default to success if no status
    
    // Make case-insensitive comparison
    var statusLower = status.toString().toLowerCase();
    
    switch(statusLower) {
        case 'available': return 'success';
        case 'occupied': return 'danger';
        case 'reserved': return 'warning';
        case 'booked': return 'info'; // Use info class, but we'll override with custom purple color
        case 'maintenance': return 'secondary';
        default: return 'info';
    }
}

function getStatusColor(status) {
    if (!status) return '#28a745'; // Default to green if no status
    
    // Make case-insensitive comparison
    var statusLower = status.toString().toLowerCase();
    
    switch(statusLower) {
        case 'available': return '#28a745'; // Green
        case 'occupied': return '#dc3545'; // Red
        case 'reserved': return '#ffc107'; // Yellow
        case 'booked': return '#6f42c1'; // Purple
        case 'maintenance': return '#6c757d'; // Gray
        default: return '#17a2b8'; // Light blue (default/unknown)
    }
}

function saveLayout() {
    var FloorId = $('#ddlFloor').val() || 0;
    if (FloorId == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select a floor first');
        return;
    }
    
    var tablesToSave = [];
    $('#tableLayoutCanvas .table-item').each(function() {
        var $table = $(this);
        var tableId = $table.data('table-id');
        var originalTable = tableDataCache[tableId];
        
        if (!originalTable) {
            console.warn('Table data not found for table ID: ' + tableId);
            return;
        }
        
        var posX = Math.round($table.position().left);
        var posY = Math.round($table.position().top);
        
        tablesToSave.push({
            TableId: tableId,
            TableNo: originalTable.TableNo,
            TableName: originalTable.TableName || null,
            TableSlug: originalTable.TableSlug || '',
            Capacity: originalTable.Capacity || 0,
            FloorId: originalTable.FloorId,
            TableTypeId: originalTable.TableTypeId || null,
            PositionX: posX,
            PositionY: posY,
            IsActive: originalTable.IsActive !== false,
            BranchId: originalTable.BranchId,
            CompanyId: null,
            AddedBy: null
        });
    });
    
    if (tablesToSave.length === 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('No tables to save');
        return;
    }
    
    $("#divLoading").show();
    
    // Save each table position sequentially to avoid conflicts
    var saveCount = 0;
    var errorCount = 0;
    
    function saveNext() {
        if (saveCount >= tablesToSave.length) {
            $("#divLoading").hide();
            if (errorCount === 0) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success('Layout saved successfully');
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.warning('Layout saved with ' + errorCount + ' error(s)');
            }
            return;
        }
        
        var tableData = tablesToSave[saveCount];
        saveCount++;
        
        $.ajax({
            url: '/restauranttable/InsertRestaurantTable',
            datatype: "json",
            data: JSON.stringify(tableData),
            contentType: "application/json",
            type: "post",
            success: function (data) {
                if (data.Status != 1) {
                    errorCount++;
                    console.error('Failed to save table ' + tableData.TableNo + ': ' + (data.Message || 'Unknown error'));
                }
                saveNext();
            },
            error: function (xhr) {
                errorCount++;
                console.error('Error saving table ' + tableData.TableNo, xhr);
                saveNext();
            }
        });
    }
    
    saveNext();
}

function loadTableStatus() {
    $("#divLoading").show();
    var floorIdVal = $('#ddlFloor').val();
    var det = {
        PageIndex: 1,
        PageSize: 1000,
        BranchId: 0,
        FloorId: floorIdVal && floorIdVal !== '' && floorIdVal !== '0' ? parseInt(floorIdVal) : 0,
        TableTypeId: null,
        Search: '',
        CompanyId: null
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
                var tables = data.Data.Tables;
                var html = '';
                
                // Update summary statistics
                var statusCounts = {
                    'Available': 0,
                    'Occupied': 0,
                    'Reserved': 0,
                    'Booked': 0,
                    'Maintenance': 0
                };
                
                $.each(tables, function (index, table) {
                    var status = table.Status || 'Available';
                    if (statusCounts.hasOwnProperty(status)) {
                        statusCounts[status]++;
                    } else {
                        statusCounts['Available']++;
                    }
                });
                
                $('#totalCount').text(tables.length);
                $('#availableCount').text(statusCounts['Available']);
                $('#occupiedCount').text(statusCounts['Occupied']);
                $('#reservedCount').text(statusCounts['Reserved']);
                $('#bookedCount').text(statusCounts['Booked']);
                $('#maintenanceCount').text(statusCounts['Maintenance']);
                
                if (tables.length === 0) {
                    html = '<div class="col-12"><div class="alert alert-info text-center"><i class="fas fa-info-circle"></i> No tables found</div></div>';
                } else {
                    $.each(tables, function (index, table) {
                        var statusClass = getStatusClass(table.Status);
                        var statusColor = getStatusColor(table.Status);
                        var statusIcon = getStatusIcon(table.Status);
                        
                        // Check if QR code exists
                        var hasQrImage = table.QRCodeImage && table.QRCodeImage.trim() !== '';
                        var qrImageUrl = hasQrImage ? (table.QRCodeImage.startsWith('/') ? table.QRCodeImage : '/' + table.QRCodeImage) : '';
                        var tableSlug = (table.TableSlug && table.TableSlug.trim() !== '') ? table.TableSlug.trim() : '';
                        var tableUrl = window.location.origin;
                        if (tableSlug) {
                            tableUrl += '/booktable/' + tableSlug;
                        } else {
                            tableUrl += '/publicbooking/booktable?tableId=' + table.TableId;
                        }
                        var safeTableId = table.TableId;
                        var safeTableNo = (table.TableNo || 'N/A').replace(/'/g, "\\'");
                        var safeTableSlug = (tableSlug || '').replace(/'/g, "\\'");
                        
                        html += '<div class="col-md-3 mb-4">';
                        html += '<div class="card table-status-card" style="--status-color: ' + statusColor + ';">';
                        html += '<div class="card-body p-3">';
                        // Header with title and menu
                        html += '<div class="table-status-card-header">';
                        html += '<h5 class="table-status-card-title"><i class="fas fa-table"></i> ' + (table.TableNo || 'N/A') + '</h5>';
                        html += '<div class="table-status-card-menu">';
                        html += '<button type="button" class="btn btn-sm btn-light" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="Actions">';
                        html += '<i class="fas fa-ellipsis-v"></i>';
                        html += '</button>';
                        html += '<div class="dropdown-menu dropdown-menu-right">';
                        html += '<a class="dropdown-item" href="/restauranttable/edit/' + table.TableId + '"><i class="fas fa-edit"></i> Edit</a>';
                        html += '<div class="dropdown-divider"></div>';
                        html += '<a class="dropdown-item" href="' + tableUrl + '" target="_blank"><i class="fas fa-external-link-alt"></i> View Public Booking</a>';
                        html += '<a class="dropdown-item" href="javascript:void(0)" onclick="copyTableUrl(' + safeTableId + ', \'' + safeTableSlug + '\')"><i class="fas fa-copy"></i> Copy Public URL</a>';
                        html += '<a class="dropdown-item" href="javascript:void(0)" onclick="shareTable(' + safeTableId + ', \'' + safeTableSlug + '\')"><i class="fas fa-share-alt"></i> Share</a>';
                        html += '<div class="dropdown-divider"></div>';
                        if (hasQrImage) {
                            html += '<a class="dropdown-item" href="javascript:void(0)" onclick="viewTableQRCode(' + safeTableId + ', \'' + safeTableNo + '\')"><i class="fas fa-qrcode"></i> View QR Code</a>';
                            html += '<a class="dropdown-item" href="' + qrImageUrl + '" download="Table-' + safeTableNo + '-QRCode.png"><i class="fas fa-download"></i> Download QR Code</a>';
                        }
                        html += '</div>';
                        html += '</div>';
                        html += '</div>';
                        // Table name tag
                        if (table.TableName) {
                            html += '<div class="table-status-card-tag"><i class="fas fa-tag"></i> ' + table.TableName + '</div>';
                        }
                        // Status badge
                        html += '<div class="mb-3">';
                        html += '<span class="table-status-badge badge-' + statusClass + '">' + statusIcon + ' ' + (table.Status || 'Available') + '</span>';
                        html += '</div>';
                        // Info section (Capacity and Floor)
                        html += '<div class="table-status-info">';
                        html += '<div class="table-status-info-item">';
                        html += '<div class="table-status-info-label">Capacity</div>';
                        html += '<div class="table-status-info-value">' + (table.Capacity || 0) + '</div>';
                        html += '</div>';
                        if (table.FloorName) {
                            html += '<div class="table-status-info-item">';
                            html += '<div class="table-status-info-label">Floor</div>';
                            html += '<div class="table-status-info-value" style="font-size: 1.1rem;" title="' + table.FloorName + '">' + table.FloorName + '</div>';
                            html += '</div>';
                        }
                        html += '</div>';
                        // Branch name
                        if (table.BranchName) {
                            html += '<div class="table-status-branch"><i class="fas fa-building"></i> ' + table.BranchName + '</div>';
                        }
                        // Status change buttons
                        html += '<div class="table-status-actions">';
                        html += '<div class="table-status-btn-group">';
                        html += '<button type="button" class="table-status-action-btn btn-success ' + (table.Status === 'Available' ? 'active' : '') + '" onclick="setTableStatus(' + table.TableId + ', \'Available\')" title="Set as Available"><i class="fas fa-check-circle"></i></button>';
                        html += '<button type="button" class="table-status-action-btn btn-danger ' + (table.Status === 'Occupied' ? 'active' : '') + '" onclick="setTableStatus(' + table.TableId + ', \'Occupied\')" title="Set as Occupied"><i class="fas fa-users"></i></button>';
                        html += '<button type="button" class="table-status-action-btn btn-warning ' + (table.Status === 'Reserved' ? 'active' : '') + '" onclick="setTableStatus(' + table.TableId + ', \'Reserved\')" title="Set as Reserved"><i class="fas fa-clock"></i></button>';
                        html += '<button type="button" class="table-status-action-btn btn-secondary ' + (table.Status === 'Maintenance' ? 'active' : '') + '" onclick="setTableStatus(' + table.TableId + ', \'Maintenance\')" title="Set as Maintenance"><i class="fas fa-tools"></i></button>';
                        html += '</div>';
                        html += '</div>';
                        html += '</div></div></div>';
                    });
                }
                $('#tableStatusCards').html(html);
            } else {
                $('#tableStatusCards').html('<div class="col-12"><div class="alert alert-warning text-center"><i class="fas fa-exclamation-triangle"></i> ' + (data.Message || 'Failed to load table status') + '</div></div>');
                // Reset summary on error
                $('#totalCount, #availableCount, #occupiedCount, #reservedCount, #bookedCount, #maintenanceCount').text('0');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to load table status');
            $('#tableStatusCards').html('<div class="col-12"><div class="alert alert-danger text-center"><i class="fas fa-exclamation-circle"></i> Error loading table status. Please try again.</div></div>');
        }
    });
}

function setTableStatus(tableId, status) {
    if (!confirm('Are you sure you want to set this table status to "' + status + '"?')) {
        return;
    }
    
    // Get user ID and company ID from cookies or hidden fields
    var userId = 0;
    var companyId = 0;
    if (typeof Cookies !== 'undefined' && Cookies.get('data')) {
        try {
            var cookieData = Cookies.get('data');
            userId = parseInt(cookieData.split('&')[2].split('=')[1]) || 0;
            companyId = parseInt(cookieData.split('&')[3].split('=')[1]) || 0;
        } catch (e) {
            console.error('Error parsing cookies:', e);
        }
    }
    
    $("#divLoading").show();
    var det = {
        TableId: tableId,
        Status: status,
        AddedBy: userId,
        CompanyId: companyId
    };
    
    $.ajax({
        url: '/restauranttable/SetTableStatus',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message || 'Table status updated successfully');
                loadTableStatus(); // Refresh the table status
            } else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message || 'Failed to update table status');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to update table status');
        }
    });
}


function getStatusIcon(status) {
    if (!status) return '<i class="fas fa-check-circle"></i>'; // Default icon
    
    // Make case-insensitive comparison
    var statusLower = status.toString().toLowerCase();
    
    switch(statusLower) {
        case 'available': return '<i class="fas fa-check-circle"></i>';
        case 'occupied': return '<i class="fas fa-users"></i>';
        case 'reserved': return '<i class="fas fa-clock"></i>';
        case 'booked': return '<i class="fas fa-calendar-check"></i>';
        case 'maintenance': return '<i class="fas fa-tools"></i>';
        default: return '<i class="fas fa-question-circle"></i>';
    }
}

function insertTableType() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        TableTypeId: 0,
        TableTypeName: $('#txtTableTypeName_M').val(),
        Description: $('#txtDescription_M').val() || '',
        IsActive: true,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/tabletype/InsertTableType',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    // Map error IDs to modal IDs
                    var modalErrorId = res.Id === 'divTableTypeName' ? 'divTableTypeName_M' : 
                                      res.Id === 'divDescription' ? 'divDescription_M' : res.Id + '_M';
                    $('#' + modalErrorId).show();
                    $('#' + modalErrorId).text(res.Message);

                    var ctrlClass = res.Id === 'divTableTypeName' ? 'divTableTypeName_M_ctrl' : 
                                   res.Id === 'divDescription' ? 'divDescription_M_ctrl' : res.Id + '_M_ctrl';
                    if ($('.' + ctrlClass).children("select").length > 0) {
                        var element = $("." + ctrlClass + ' select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + ctrlClass + ' .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + ctrlClass + ' select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + ctrlClass).css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                // Add new option to dropdown and select it
                $('#ddlTableTypeId').append($('<option>', { 
                    value: data.Data.TableType.TableTypeId, 
                    text: data.Data.TableType.TableTypeName 
                }));
                $('#ddlTableTypeId').val(data.Data.TableType.TableTypeId).trigger('change');
                // Close modal and clear form
                $('#tableTypeModal').modal('toggle');
                $('#txtTableTypeName_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred');
        }
    });
}

function insertFloorFromModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var branchIdValue = $('#ddlBranchId_M').val();
    var branchId = branchIdValue && branchIdValue !== '' ? parseInt(branchIdValue) : 0;
    var floorName = $('#txtFloorName_M').val();

    // Client-side validation
    var hasError = false;
    
    if (!branchId || branchId === 0) {
        $('#divBranchId_M').text('Business Location is required').show();
        $('#ddlBranchId_M').next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
        hasError = true;
    }
    
    if (!floorName || floorName.trim() === '') {
        $('#divFloorName_M').text('Floor Name is required').show();
        $('#txtFloorName_M').css('border', '2px solid #dc3545');
        hasError = true;
    }
    
    if (hasError) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please fill in all required fields');
        return;
    }

    var det = {
        FloorId: 0,
        FloorName: floorName,
        FloorNumber: $('#txtFloorNumber_M').val() ? parseInt($('#txtFloorNumber_M').val()) : null,
        BranchId: branchId,
        IsActive: true,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/InsertRestaurantFloor',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    // Map error IDs to modal IDs
                    var modalErrorId = res.Id === 'divFloorName' ? 'divFloorName_M' : 
                                      res.Id === 'divFloorNumber' ? 'divFloorNumber_M' : 
                                      res.Id === 'divBranchId' ? 'divBranchId_M' : res.Id + '_M';
                    $('#' + modalErrorId).show();
                    $('#' + modalErrorId).text(res.Message);

                    var ctrlClass = res.Id === 'divFloorName' ? 'divFloorName_M_ctrl' : 
                                   res.Id === 'divFloorNumber' ? 'divFloorNumber_M_ctrl' : 
                                   res.Id === 'divBranchId' ? 'divBranchId_M_ctrl' : res.Id + '_M_ctrl';
                    if ($('.' + ctrlClass).children("select").length > 0) {
                        var element = $("." + ctrlClass + ' select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + ctrlClass + ' .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + ctrlClass + ' select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        // Apply border to input inside the form group
                        var inputElement = $('.' + ctrlClass).find('input');
                        if (inputElement.length > 0) {
                            inputElement.css('border', '2px solid #dc3545');
                        } else {
                            $('.' + ctrlClass).css('border', '2px solid #dc3545');
                        }
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                if (data.Data && data.Data.Floor) {
                    var floor = data.Data.Floor;
                    // Reload floors to ensure the new floor appears in the dropdown
                    // This ensures the floor is properly filtered by branch
                    loadFloorsByBranch();
                    // After floors are loaded, select the newly created floor
                    setTimeout(function() {
                        $('#ddlFloorId').val(floor.FloorId).trigger('change');
                    }, 500);
                }
                // Close modal and clear form
                $('#floorModal').modal('toggle');
                $('#ddlBranchId_M').val('').trigger('change');
                $('#txtFloorName_M').val('');
                $('#txtFloorNumber_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred');
        }
    });
}

// QR Code functions for Restaurant Tables
function generateTableQRCode(tableId, tableNo) {
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/GenerateQrCode',
        datatype: "json",
        data: JSON.stringify({ TableId: tableId }),
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
                                    <h5 class="modal-title"><i class="fas fa-qrcode"></i> QR Code - Table ${tableNo}</h5>
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                </div>
                                <div class="modal-body text-center">
                                    <div class="mb-3">
                                        ${imageHtml}
                                    </div>
                                    <p class="text-muted"><small>Scan this QR code to access table booking</small></p>
                                    <div class="mt-3 mb-3">
                                        <label class="font-weight-bold">Table Booking URL:</label>
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
                                        <button type="button" class="btn btn-primary" ${qrImage ? `onclick="downloadQRCode('${qrImage}', 'Table_${tableNo}_QRCode.png')"` : 'disabled'}>
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

// Helper functions for table URL operations (used in TableStatus action menus)
function copyTableUrl(tableId, tableSlug) {
    if (!tableId || tableId <= 0) {
        if (typeof toastr !== 'undefined') {
            toastr.error('Invalid table ID');
        } else {
            alert('Invalid table ID');
        }
        return;
    }
    
    // Use slug if available, otherwise fall back to tableId
    var url = window.location.origin;
    if (tableSlug && tableSlug.trim() !== '') {
        url += '/booktable/' + tableSlug.trim();
    } else {
        url += '/publicbooking/booktable?tableId=' + tableId;
    }
    
    var tempInput = document.createElement('input');
    tempInput.value = url;
    document.body.appendChild(tempInput);
    tempInput.focus();
    tempInput.select();
    try {
        document.execCommand('copy');
        document.body.removeChild(tempInput);
        if (typeof toastr !== 'undefined') {
            toastr.success('Table booking URL copied to clipboard: ' + url);
        } else {
            alert('Table booking URL copied to clipboard: ' + url);
        }
    } catch (err) {
        document.body.removeChild(tempInput);
        if (typeof toastr !== 'undefined') {
            toastr.error('Failed to copy URL');
        } else {
            alert('Failed to copy URL');
        }
    }
}

function shareTable(tableId, tableSlug) {
    if (!tableId || tableId <= 0) {
        if (typeof toastr !== 'undefined') {
            toastr.error('Invalid table ID');
        } else {
            alert('Invalid table ID');
        }
        return;
    }
    
    // Use slug if available, otherwise fall back to tableId
    var shareUrl = window.location.origin;
    if (tableSlug && tableSlug.trim() !== '') {
        shareUrl += '/booktable/' + tableSlug.trim();
    } else {
        shareUrl += '/publicbooking/booktable?tableId=' + tableId;
    }
    
    if (navigator.share) {
        navigator.share({
            title: document.title,
            text: 'Book this table at our restaurant',
            url: shareUrl
        }).catch(function (error) {
            if (error && error.name !== 'AbortError') {
                copyTableUrl(tableId, tableSlug);
            }
        });
    } else {
        copyTableUrl(tableId, tableSlug);
    }
}

// View QR Code function (displays existing QR code without generating)
function viewTableQRCode(tableId, tableNo) {
    $("#divLoading").show();
    $.ajax({
        url: '/restauranttable/GenerateQrCode',
        datatype: "json",
        data: JSON.stringify({ TableId: tableId }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();

            if (data.Status == 1 && data.Data) {
                var qrImage = data.Data.QrCodeImageUrl || null;
                var qrUrl = data.Data.QrCodeUrl || '';

                // Build QR code modal
                var imageHtml = qrImage
                    ? `<img src="${qrImage}" alt="QR Code" style="max-width: 300px; border: 2px solid #007bff; padding: 10px; background: white;" />`
                    : `<div class="alert alert-warning">QR image not available, but the URL can still be used.</div>`;

                var modalHtml = `
                    <div class="modal fade" id="qrCodeModal" tabindex="-1" role="dialog">
                        <div class="modal-dialog modal-dialog-centered" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title"><i class="fas fa-qrcode"></i> QR Code - Table ${tableNo}</h5>
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                </div>
                                <div class="modal-body text-center">
                                    <div class="mb-3">
                                        ${imageHtml}
                                    </div>
                                    <p class="text-muted"><small>Scan this QR code to access table booking</small></p>
                                    <div class="mt-3 mb-3">
                                        <label class="font-weight-bold">Table Booking URL:</label>
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
                                        <button type="button" class="btn btn-primary" ${qrImage ? `onclick="downloadQRCode('${qrImage}', 'Table_${tableNo}_QRCode.png')"` : 'disabled'}>
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
                if (typeof toastr !== 'undefined') {
                    toastr.error(data.Message || 'QR code not available. Please generate QR code first.');
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (typeof EnableSound !== 'undefined' && EnableSound == 'True') {
                if (document.getElementById('error')) document.getElementById('error').play();
            }
            if (typeof toastr !== 'undefined') toastr.error('An error occurred while loading QR code');
        }
    });
}

function loadFloorsByBranch() {
    var BranchId = $('#ddlBranchId').val();
    if (!BranchId || BranchId === '') {
        $('#ddlFloorId').html('<option value="">Select Floor</option>');
        $('#ddlFloorId').val('').trigger('change');
        return;
    }
    
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/getfloors',
        datatype: "json",
        data: JSON.stringify({
            PageIndex: 1,
            PageSize: 1000,
            BranchId: BranchId || 0,
            CompanyId: null
        }),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Floors) {
                var options = '<option value="">Select Floor</option>';
                $.each(data.Data.Floors, function(index, item) {
                    if (item.IsActive && !item.IsDeleted) {
                        options += '<option value="' + item.FloorId + '">' + item.FloorName + '</option>';
                    }
                });
                var currentFloorId = $('#ddlFloorId').val();
                $('#ddlFloorId').html(options);
                // Try to restore previous selection if it still exists in the new list
                if (currentFloorId && $('#ddlFloorId option[value="' + currentFloorId + '"]').length > 0) {
                    $('#ddlFloorId').val(currentFloorId).trigger('change');
                } else {
                    $('#ddlFloorId').val('').trigger('change');
                }
            } else {
                $('#ddlFloorId').html('<option value="">Select Floor</option>');
                $('#ddlFloorId').val('').trigger('change');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Failed to load floors');
            $('#ddlFloorId').html('<option value="">Select Floor</option>');
            $('#ddlFloorId').val('').trigger('change');
        }
    });
}


