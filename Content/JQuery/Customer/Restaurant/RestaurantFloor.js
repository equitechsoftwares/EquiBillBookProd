var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val() || 10,
        Search: $('#txtSearch').val(),
        BranchId: $('#ddlBranch').val() || 0
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/index',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblFloors").html($(data).find("#tblFloors").html());
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

function insertFloor(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var branchIdValue = $('#ddlBranchId').val();
    var branchId = branchIdValue && branchIdValue !== '' ? parseInt(branchIdValue) : 0;
    
    var det = {
        FloorId: 0,
        FloorName: $('#txtFloorName').val(),
        FloorNumber: $('#txtFloorNumber').val() ? parseInt($('#txtFloorNumber').val()) : null,
        BranchId: branchId,
        IsActive: $('#chkIsActive').is(':checked'),
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
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                if (i == 2) {
                    // Redirect back to add page for "Save & Add Another"
                    window.location.href = '/restaurantfloor/add';
                } else {
                    // Redirect to list page (for Save)
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/restaurantfloor/index?success=' + message;
                }
            } else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
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

function updateFloor(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var branchIdValue = $('#ddlBranchId').val();
    var branchId = branchIdValue && branchIdValue !== '' ? parseInt(branchIdValue) : 0;
    
    var det = {
        FloorId: $('#FloorId').val() || 0,
        FloorName: $('#txtFloorName').val(),
        FloorNumber: $('#txtFloorNumber').val() ? parseInt($('#txtFloorNumber').val()) : null,
        BranchId: branchId,
        IsActive: $('#chkIsActive').is(':checked'),
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/UpdateRestaurantFloor',
        datatype: "json",
        data: JSON.stringify(det),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1) {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                if (i == 2) {
                    // Redirect back to add page for "Save & Add Another"
                    window.location.href = '/restaurantfloor/add';
                } else {
                    // Redirect to list page (for Update)
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/restaurantfloor/index?success=' + message;
                }
            } else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
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

function deleteFloor(FloorId) {
    if (confirm('Are you sure you want to delete this floor?')) {
        var det = {
            FloorId: FloorId,
            BranchId: null,
            CompanyId: null,
            AddedBy: null
        };
        $("#divLoading").show();
        $.ajax({
            url: '/restaurantfloor/RestaurantFloorDelete',
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

function ActiveInactive(FloorId, IsActive) {
    var det = {
        FloorId: FloorId,
        IsActive: IsActive,
        BranchId: null,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/restaurantfloor/RestaurantFloorActiveInactive',
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

