$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    // Initialize error texts as hidden
    $('.errorText').hide();
    
    // Initialize addon section visibility (without showing warnings on page load)
    toggleAddonSelection(false);
    
    // Initialize never expires checkbox state
    if ($('#chkNeverExpires').length > 0) {
        toggleNeverExpires();
    }
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

// Helper function to detect cookie type and base route
function getCookieAndRoute() {
    var cookieName = Cookies.get('adata') ? 'adata' : 'data';
    var baseRoute = cookieName === 'adata' ? '/admincoupon' : '/coupon';
    return { cookieName: cookieName, baseRoute: baseRoute };
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    _PageIndex = PageIndex;
    var PageSize = $('#txtPageSize').val();
    var Search = $('#txtSearch').val();
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var obj = {
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        PageIndex: PageIndex,
        PageSize: parseInt(PageSize),
        Search: Search
    };
    
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/couponfetch',
        type: 'POST',
        data: obj,
        success: function (res) {
            $('#tblData').html(res);
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
        },
        error: function () {
            $("#divLoading").hide();
            toastr.error('Error loading coupon list');
        }
    });
}

function toggleNoOfTimes() {
    var usageType = $('#ddlUsageType').val();
    if (usageType == '3') {
        $('#divNoOfTimes').show();
    } else {
        $('#divNoOfTimes').hide();
    }
}

function toggleNeverExpires() {
    var neverExpires = $('#chkNeverExpires').is(':checked');
    if (neverExpires) {
        $('#txtEndDate').prop('disabled', true).val('');
        $('#txtEndDate').closest('.form-group').find('label span.text-danger').hide();
    } else {
        $('#txtEndDate').prop('disabled', false);
        $('#txtEndDate').closest('.form-group').find('label span.text-danger').show();
    }
}

function toggleAddonSelection(showWarning) {
    // Default to showing warning if parameter not provided (for user interactions)
    if (showWarning === undefined) {
        showWarning = true;
    }
    
    var applyToBasePlan = $('#chkApplyToBasePlan').is(':checked');
    var applyToAddons = $('#chkApplyToAddons').is(':checked');
    
    // Validate: at least one must be selected (only show warning on user interaction)
    if (!applyToBasePlan && !applyToAddons) {
        if (showWarning) {
            toastr.warning('Please select at least one: Base Plan or Addons');
        }
        // Re-check "Apply to Addons" if both unchecked (silently on page load)
        $('#chkApplyToAddons').prop('checked', true);
        applyToAddons = true;
    }
    
    if (applyToAddons) {
        $('#divAddonSelection').show();
    } else {
        $('#divAddonSelection').hide();
        // Uncheck all addons when hidden
        $('.chkAddon').prop('checked', false);
    }
}

function insert(i) {
    $('.errorText').hide();
    $('.divCouponCode_ctrl, .divDiscountType_ctrl, .divDiscount_ctrl, .divUsageType_ctrl, .divStartDate_ctrl, .divEndDate_ctrl, .divNoOfTimes_ctrl').css('border', '');

    // Store original button HTML and disable buttons with spinner
    var btnSave = $('#btnSave');
    var btnSave_AddAnother = $('#btnSave_AddAnother');
    var originalBtnSaveHtml = btnSave.html();
    var originalBtnSave_AddAnotherHtml = btnSave_AddAnother.html();
    
    btnSave.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Saving...');
    btnSave_AddAnother.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Saving...');

    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];

    // Get selected term lengths
    var termLengths = [];
    $('.chkTermLength:checked').each(function () {
        termLengths.push({
            TermLengthId: parseInt($(this).val()),
            IsSelected: true
        });
    });

    // Get selected addons
    var addons = [];
    if ($('#chkApplyToAddons').is(':checked')) {
        $('.chkAddon:checked').each(function () {
            addons.push({
                PlanAddonsId: parseInt($(this).val()),
                IsSelected: true
            });
        });
    }

    // Handle never expires - following RecurringSales pattern
    var isNeverExpires = $('#chkNeverExpires').is(':checked');
    var endDate = $('#txtEndDate').val() || '';

    var obj = {
        CouponCode: $('#txtCouponCode').val(),
        CouponDescription: $('#txtCouponDescription').val(),
        DiscountType: parseInt($('#ddlDiscountType').val()),
        Discount: parseFloat($('#txtDiscount').val()) || 0,
        UsageType: parseInt($('#ddlUsageType').val()),
        NoOfTimes: parseInt($('#txtNoOfTimes').val()) || 0,
        StartDate: $('#txtStartDate').val(),
        EndDate: endDate,
        IsNeverExpires: isNeverExpires,
        TermLengths: termLengths,
        Addons: addons,
        MinimumPurchaseAmount: parseFloat($('#txtMinimumPurchaseAmount').val()) || 0,
        MaximumDiscountAmount: parseFloat($('#txtMaximumDiscountAmount').val()) || 0,
        ApplyToBasePlan: $('#chkApplyToBasePlan').is(':checked'),
        ApplyToAddons: $('#chkApplyToAddons').is(':checked'),
        OrderNo: parseInt($('#txtOrderNo').val()) || 0,
        IsActive: true,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };

    $.ajax({
        url: cookieRoute.baseRoute + '/insert',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                // Restore original button HTML on error
                btnSave.prop('disabled', false).html(originalBtnSaveHtml);
                btnSave_AddAnother.prop('disabled', false).html(originalBtnSave_AddAnotherHtml);
                
                if (res.Status == 2) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error('Invalid inputs, check and try again !!');
                    if (res.Errors) {
                        res.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    } else if (res.Data && res.Data.Errors) {
                        res.Data.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    }
                } else {
                    if (res.Data && res.Data.Errors) {
                        if (EnableSound == 'True') { document.getElementById('error').play(); }
                        toastr.error('Invalid inputs, check and try again !!');
                        res.Data.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    } else {
                        if (EnableSound == 'True') { document.getElementById('error').play(); }
                        toastr.error(res.Message || 'Error saving coupon');
                    }
                }
            }
        },
        error: function () {
            // Restore original button HTML on error
            btnSave.prop('disabled', false).html(originalBtnSaveHtml);
            btnSave_AddAnother.prop('disabled', false).html(originalBtnSave_AddAnotherHtml);
            toastr.error('Error saving coupon');
        }
    });
}

function update(i) {
    $('.errorText').hide();
    $('.divCouponCode_ctrl, .divDiscountType_ctrl, .divDiscount_ctrl, .divUsageType_ctrl, .divStartDate_ctrl, .divEndDate_ctrl, .divNoOfTimes_ctrl').css('border', '');

    // Store original button HTML and disable button with spinner
    var btnUpdate = $('#btnUpdate');
    var originalBtnUpdateHtml = btnUpdate.html();
    
    btnUpdate.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Updating...');

    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    var ModifiedBy = arr[2].split('=')[1];

    // Get selected term lengths
    var termLengths = [];
    $('.chkTermLength:checked').each(function () {
        termLengths.push({
            TermLengthId: parseInt($(this).val()),
            IsSelected: true
        });
    });

    // Get selected addons
    var addons = [];
    if ($('#chkApplyToAddons').is(':checked')) {
        $('.chkAddon:checked').each(function () {
            addons.push({
                PlanAddonsId: parseInt($(this).val()),
                IsSelected: true
            });
        });
    }

    // Handle never expires - following RecurringSales pattern
    var isNeverExpires = $('#chkNeverExpires').is(':checked');
    var endDate = $('#txtEndDate').val() || '';

    var obj = {
        CouponId: parseInt($('#txtCouponId').val()),
        CouponCode: $('#txtCouponCode').val(),
        CouponDescription: $('#txtCouponDescription').val(),
        DiscountType: parseInt($('#ddlDiscountType').val()),
        Discount: parseFloat($('#txtDiscount').val()) || 0,
        UsageType: parseInt($('#ddlUsageType').val()),
        NoOfTimes: parseInt($('#txtNoOfTimes').val()) || 0,
        StartDate: $('#txtStartDate').val(),
        EndDate: endDate,
        IsNeverExpires: isNeverExpires,
        TermLengths: termLengths,
        Addons: addons,
        MinimumPurchaseAmount: parseFloat($('#txtMinimumPurchaseAmount').val()) || 0,
        MaximumDiscountAmount: parseFloat($('#txtMaximumDiscountAmount').val()) || 0,
        ApplyToBasePlan: $('#chkApplyToBasePlan').is(':checked'),
        ApplyToAddons: $('#chkApplyToAddons').is(':checked'),
        OrderNo: parseInt($('#txtOrderNo').val()) || 0,
        IsActive: $('#txtIsActive').val() === 'true',
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        ModifiedBy: ModifiedBy
    };

    $.ajax({
        url: cookieRoute.baseRoute + '/update',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                // Restore original button HTML on error
                btnUpdate.prop('disabled', false).html(originalBtnUpdateHtml);
                
                if (res.Status == 2) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error('Invalid inputs, check and try again !!');
                    if (res.Errors) {
                        res.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    } else if (res.Data && res.Data.Errors) {
                        res.Data.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    }
                } else {
                    if (res.Data && res.Data.Errors) {
                        if (EnableSound == 'True') { document.getElementById('error').play(); }
                        toastr.error('Invalid inputs, check and try again !!');
                        res.Data.Errors.forEach(function (error) {
                            $('#' + error.Id).text(error.Message).show();
                            $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                        });
                    } else {
                        if (EnableSound == 'True') { document.getElementById('error').play(); }
                        toastr.error(res.Message || 'Error updating coupon');
                    }
                }
            }
        },
        error: function () {
            // Restore original button HTML on error
            btnUpdate.prop('disabled', false).html(originalBtnUpdateHtml);
            toastr.error('Error updating coupon');
        }
    });
}

function Delete(CouponId, CouponCode) {
    var r = confirm("This will delete \"" + CouponCode + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var cookieRoute = getCookieAndRoute();
        var arr = Cookies.get(cookieRoute.cookieName).split('&');
        var CompanyId = arr[1].split('=')[1];
        var ModifiedBy = arr[2].split('=')[1];

        var det = {
            CouponId: CouponId,
            CompanyId: CompanyId,
            ModifiedBy: ModifiedBy
        };
        $("#divLoading").show();
        $.ajax({
            url: cookieRoute.baseRoute + '/delete',
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
                    fetchList(_PageIndex);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function ActiveInactive(CouponId, IsActive) {
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];

    var det = {
        CouponId: CouponId,
        IsActive: IsActive,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/activeinactive',
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
                fetchList(_PageIndex);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

