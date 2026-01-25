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
        url: '/kitchenstation/index',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblStations").html($(data).find("#tblStations").html());
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

function insertStation(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var categoryIds = $('#ddlCategories').val() || [];
    var det = {
        KitchenStationId: 0,
        StationName: $('#txtStationName').val(),
        StationType: $('#ddlStationType').val() || 'Main',
        IsActive: true,
        CategoryIdList: categoryIds,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/kitchenstation/InsertKitchenStation',
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
                    toastr.success(data.Message);
                    window.location.href = '/kitchenstation/add';
                } else {
                    // Redirect to list page (for Save) - message will be shown on index page
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/kitchenstation/index?success=' + message;
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

function updateStation(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var categoryIds = $('#ddlCategories').val() || [];
    var det = {
        KitchenStationId: $('#KitchenStationId').val() || 0,
        StationName: $('#txtStationName').val(),
        StationType: $('#ddlStationType').val() || 'Main',
        IsActive: true,
        CategoryIdList: categoryIds,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/kitchenstation/UpdateKitchenStation',
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
                    toastr.success(data.Message);
                    window.location.href = '/kitchenstation/add';
                } else {
                    // Redirect to list page (for Update) - message will be shown on index page
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/kitchenstation/index?success=' + message;
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

function deleteStation(KitchenStationId) {
    if (confirm('Are you sure you want to delete this kitchen station?')) {
        var det = {
            KitchenStationId: KitchenStationId,
            CompanyId: null,
            AddedBy: null
        };
        $("#divLoading").show();
        $.ajax({
            url: '/kitchenstation/KitchenStationDelete',
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

function ActiveInactive(KitchenStationId, IsActive) {
    var det = {
        KitchenStationId: KitchenStationId,
        IsActive: IsActive,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/kitchenstation/KitchenStationActiveInactive',
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

function insertStationTypeFromModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        StationTypeId: 0,
        StationTypeName: $('#txtStationTypeName_M').val(),
        Description: $('#txtDescription_M').val() || '',
        IsActive: true,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/stationtype/InsertStationType',
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
                    var modalErrorId = res.Id === 'divStationTypeName' ? 'divStationTypeName_M' :
                                       res.Id === 'divDescription' ? 'divDescription_M' : res.Id + '_M';
                    $('#' + modalErrorId).show();
                    $('#' + modalErrorId).text(res.Message);

                    var ctrlClass = res.Id === 'divStationTypeName' ? 'divStationTypeName_M_ctrl' :
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
                        // Apply border to input or textarea inside the form group, not the form group itself
                        var inputElement = $('.' + ctrlClass).find('input, textarea');
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
                if (data.Data && data.Data.StationType) {
                    var st = data.Data.StationType;
                    // Add new option to dropdown and select it (value is name in this form)
                    $('#ddlStationType').append($('<option>', {
                        value: st.StationTypeName,
                        text: st.StationTypeName
                    }));
                    $('#ddlStationType').val(st.StationTypeName).trigger('change');
                }
                // Close modal and clear form
                $('#stationTypeModal').modal('toggle');
                $('#txtStationTypeName_M').val('');
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

