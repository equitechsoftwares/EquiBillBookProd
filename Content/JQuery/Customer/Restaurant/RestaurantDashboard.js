$(document).ready(function () {
    // Show loading immediately - prevents flash of content (Industry Standard)
    $("#divLoading").show();

    // Initialize Select2 for branch dropdown
    $('#ddlBranch').select2({
        placeholder: 'Select Branch',
        allowClear: false
    });

    // Load dashboard on branch selection
    $('#ddlBranch').on('change', function () {
        var branchId = $(this).val();
        if (branchId && branchId != '0') {
            // Save selected branch to localStorage
            localStorage.setItem('restaurantDashboard_selectedBranch', branchId);
            fetchDashboard(branchId);
        }
    });

    // Check for saved branch selection in localStorage
    var savedBranchId = localStorage.getItem('restaurantDashboard_selectedBranch');
    var branchToSelect = null;

    if (savedBranchId) {
        // Verify that the saved branch ID exists in the dropdown options
        var optionExists = $('#ddlBranch option[value="' + savedBranchId + '"]').length > 0;
        if (optionExists && savedBranchId != '0') {
            branchToSelect = savedBranchId;
        }
    }

    // If no valid saved branch, fallback to first branch
    if (!branchToSelect) {
        var firstBranchId = $('#ddlBranch option:not(:first)').first().val();
        if (firstBranchId && firstBranchId != '0') {
            branchToSelect = firstBranchId;
        } else {
            // No branches available - hide loading and show message
            $("#divLoading").hide();
            $('#divNoBranchSelected').show();
            return;
        }
    }

    // Select the branch and trigger change to load dashboard
    if (branchToSelect) {
        $('#ddlBranch').val(branchToSelect).trigger('change');
    } else {
        // No branch found - hide loading
        $("#divLoading").hide();
    }
});

function fetchDashboard(branchId) {
    $("#divLoading").show();
    var obj = {
        BranchId: parseInt(branchId)
    };

    $.ajax({
        url: '/restaurantdashboard/restaurantdashboardfetch',
        datatype: "json",
        data: JSON.stringify(obj),
        contentType: "application/json",
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data) {
                $("#p_div").html(data);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            toastr.error('Failed to load dashboard');
        }
    });
}

function loadDashboardData(branchId) {
    $("#divLoading").show();
    var obj = {
        BranchId: parseInt(branchId)
    };

    var companyId = $('#hdnCompanyId').val() || getCompanyIdFromCookies();
    var baseUrl = window.location.origin;
    
    $.ajax({
        url: baseUrl + '/api/RestaurantDashboard/GetRestaurantDashboard',
        datatype: "json",
        data: JSON.stringify(obj),
        contentType: "application/json",
        type: "post",
        headers: getAuthHeaders(),
        success: function (html) {
            $("#divLoading").hide();
            // The partial view is already rendered server-side with ViewBag data
            // We just need to update the dashboard content area
            // For dynamic updates, we would need to call API and update stats
            // For now, just replace the content
            if (html) {
                $('#p_div').html(html);
                // Re-initialize select2 if needed
                $('#ddlBranch').select2({
                    placeholder: 'Select Branch',
                    allowClear: false
                });
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            toastr.error('Failed to load dashboard data');
        }
    });
}


