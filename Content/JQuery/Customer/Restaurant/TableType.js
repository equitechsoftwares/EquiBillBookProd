var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val() || 10,
        Search: $('#txtSearch').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/tabletype/index',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblTableTypes").html($(data).find("#tblTableTypes").html());
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

function insertTableType(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        TableTypeId: 0,
        TableTypeName: $('#txtTableTypeName').val(),
        Description: $('#txtDescription').val(),
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
            if (data.Status == 1) {
                if (i == 2) {
                    // Redirect back to add page for "Save & Add Another"
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    window.location.href = '/tabletype/add';
                } else {
                    // Redirect to list page (for Save) - message will be shown on index page
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/tabletype/index?success=' + message;
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

function updateTableType(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var isActiveValue = $('#chkIsActive').val() === 'true';
    
    var det = {
        TableTypeId: $('#TableTypeId').val() || 0,
        TableTypeName: $('#txtTableTypeName').val(),
        Description: $('#txtDescription').val(),
        IsActive: isActiveValue,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/tabletype/UpdateTableType',
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
                    window.location.href = '/tabletype/add';
                } else {
                    // Redirect to list page (for Update) - message will be shown on index page
                    var message = encodeURIComponent(data.Message);
                    window.location.href = '/tabletype/index?success=' + message;
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

function deleteTableType(TableTypeId) {
    if (confirm('Are you sure you want to delete this table type?')) {
        var det = {
            TableTypeId: TableTypeId,
            CompanyId: null,
            AddedBy: null
        };
        $("#divLoading").show();
        $.ajax({
            url: '/tabletype/TableTypeDelete',
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

function ActiveInactive(TableTypeId, IsActive) {
    var det = {
        TableTypeId: TableTypeId,
        IsActive: IsActive,
        CompanyId: null,
        AddedBy: null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/tabletype/TableTypeActiveInactive',
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

