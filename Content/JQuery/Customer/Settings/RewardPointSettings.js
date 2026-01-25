
$(function () {
    $("[data-toggle=tooltip]").tooltip();
    $('.select2').select2();
    
    // Show empty message if no visible tiers exist
    if ($('#tblTiersBody .tier-row:visible').length === 0) {
        if ($('#tblTiersBody .empty-message-row').length === 0) {
            $('#tblTiersBody').append('<tr class="empty-message-row"><td colspan="6" class="text-center text-muted">No tiers defined. Click "Add Tier" to create one.</td></tr>');
        }
    }
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

function toggleRewardPointsConfig() {
    if ($('#chkEnableRewardPoint').is(':checked')) {
        $('#divRewardPointsConfig').show();
    } else {
        $('#divRewardPointsConfig').hide();
    }
}

function collectTiers() {
    var tiers = [];
    var companyId = parseInt(Cookies.get('data').split('&')[4].split('=')[1]);
    var rewardPointSettingsId = parseInt($('#hdnRewardPointSettingsId').val() || '0');
    var addedBy = parseInt(Cookies.get('data').split('&')[2].split('=')[1]);
    
    // Collect all tier data from the table (including hidden rows for deletion)
    $('#tblTiersBody .tier-row').each(function(index) {
        var $row = $(this);
        var isHidden = $row.is(":hidden");
        var tierId = parseInt($row.attr('data-tier-id')) || 0;
        
        // If row is hidden and it's an existing tier (has tierId), mark it for deletion
        if (isHidden && tierId > 0) {
            tiers.push({
                RewardPointTierId: tierId,
                RewardPointSettingsId: rewardPointSettingsId,
                CompanyId: companyId,
                MinAmount: 0,
                MaxAmount: null,
                AmountSpentForUnitPoint: 0,
                Priority: 0,
                IsActive: false,
                IsDeleted: true, // Mark for soft delete
                AddedBy: addedBy
            });
            return true; // Continue to next row
        }
        
        // Skip hidden rows that are new (no tierId) - they were never saved
        if (isHidden && tierId === 0) {
            return true; // Skip this row - it's a new row that was deleted before saving
        }
        
        var priority = parseInt($row.find('.tier-priority').val()) || 0;
        var minAmount = parseFloat($row.find('.tier-min-amount').val()) || 0;
        var maxAmountVal = $row.find('.tier-max-amount').val();
        var maxAmount = maxAmountVal && maxAmountVal.trim() !== '' ? parseFloat(maxAmountVal) : null;
        var amountSpentForUnitPoint = parseFloat($row.find('.tier-amount-for-point').val()) || 0;
        var isActive = $row.find('.tier-status').val() === 'true';
        
        tiers.push({
            RewardPointTierId: tierId,
            RewardPointSettingsId: rewardPointSettingsId,
            CompanyId: companyId,
            MinAmount: minAmount,
            MaxAmount: maxAmount,
            AmountSpentForUnitPoint: amountSpentForUnitPoint,
            Priority: priority,
            IsActive: isActive,
            IsDeleted: false,
            AddedBy: addedBy
        });
    });
    
    return tiers;
}

function update() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    // Collect tiers first to validate them
    var tiers = collectTiers();
    var hasErrors = false;
    
    // Validate tiers
    tiers.forEach(function(tier, index) {
        if (tier.IsDeleted) return; // Skip validation for deleted tiers
        
        if (tier.Priority < 1) {
            toastr.error('Priority must be at least 1 in row ' + (index + 1));
            hasErrors = true;
        }
        
        if (tier.MinAmount < 0) {
            toastr.error('Min Amount must be 0 or greater in row ' + (index + 1));
            hasErrors = true;
        }
        
        if (tier.MaxAmount !== null && tier.MaxAmount <= tier.MinAmount) {
            toastr.error('Max Amount must be greater than Min Amount in row ' + (index + 1));
            hasErrors = true;
        }
        
        if (tier.AmountSpentForUnitPoint <= 0) {
            toastr.error('Amount for 1 Point must be greater than 0 in row ' + (index + 1));
            hasErrors = true;
        }
    });
    
    if (hasErrors) {
        return;
    }

    var det = {
        RewardPointSettingsId: parseInt($('#hdnRewardPointSettingsId').val()) || 0,
        EnableRewardPoint: $('#chkEnableRewardPoint').is(':checked'),
        DisplayName: $('#txtDisplayName').val(),
        AmountSpentForUnitPoint: parseFloat($('#txtAmountSpentForUnitPoint').val()) || 0,
        MinOrderTotalToEarnReward: parseFloat($('#txtMinOrderTotalToEarnReward').val()) || 0,
        MaxPointsPerOrder: parseFloat($('#txtMaxPointsPerOrder').val()) || 0,
        RedeemAmountPerUnitPoint: parseFloat($('#txtRedeemAmountPerUnitPoint').val()) || 0,
        MinimumOrderTotalToRedeemPoints: parseFloat($('#txtMinimumOrderTotalToRedeemPoints').val()) || 0,
        MinimumRedeemPoint: parseFloat($('#txtMinimumRedeemPoint').val()) || 0,
        MaximumRedeemPointPerOrder: parseFloat($('#txtMaximumRedeemPointPerOrder').val()) || 0,
        ExpiryPeriod: parseInt($('#txtExpiryPeriod').val()) || 0,
        ExpiryPeriodType: parseInt($('#ddlExpiryPeriodType').val()) || 1,
        RewardPointTiers: tiers // Include tiers in the update call
    };
    
    // Update settings and tiers together in one transaction
    $("#divLoading").show();
    $.ajax({
        url: '/rewardpointsettings/update',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return;
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return;
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                // Reload page to refresh tier IDs after saving
                setTimeout(function() {
                    window.location.reload();
                }, 1000);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('An error occurred while saving settings');
        }
    });
}

function removeTierRow(button) {
    // Hide the row instead of removing it - will be soft deleted on update
    $(button).closest('tr').hide();
    
    // Show empty message if no visible rows left
    if ($('#tblTiersBody .tier-row:visible').length === 0) {
        // Check if empty message already exists
        if ($('#tblTiersBody tr:contains("No tiers defined")').length === 0) {
            $('#tblTiersBody').append('<tr class="empty-message-row"><td colspan="6" class="text-center text-muted">No tiers defined. Click "Add Tier" to create one.</td></tr>');
        }
    }
}

// Tier Management Functions - now handled inline in the table

var tierRowCounter = 0;

function addTier() {
    var currencySymbol = Cookies.get('data').split('&')[5].split('=')[1];
    var tbody = $('#tblTiersBody');
    
    // Get the highest priority from existing rows
    var maxPriority = 1;
    $('#tblTiersBody .tier-priority').each(function() {
        var priority = parseInt($(this).val()) || 0;
        if (priority > maxPriority) {
            maxPriority = priority;
        }
    });
    
    // Create a new row with textboxes
    var newRow = '<tr data-tier-id="0" data-row-index="new-' + (++tierRowCounter) + '" class="tier-row new-tier-row">' +
        '<td>' +
            '<input type="number" class="form-control form-control-sm tier-priority" value="' + (maxPriority + 1) + '" min="1" step="1">' +
        '</td>' +
        '<td>' +
            '<div class="input-group input-group-sm">' +
                '<div class="input-group-prepend">' +
                    '<span class="input-group-text">' + currencySymbol + '</span>' +
                '</div>' +
                '<input type="number" class="form-control tier-min-amount" value="" min="0" step="0.01">' +
            '</div>' +
        '</td>' +
        '<td>' +
            '<div class="input-group input-group-sm">' +
                '<div class="input-group-prepend">' +
                    '<span class="input-group-text">' + currencySymbol + '</span>' +
                '</div>' +
                '<input type="number" class="form-control tier-max-amount" value="" min="0" step="0.01">' +
            '</div>' +
        '</td>' +
        '<td>' +
            '<div class="input-group input-group-sm">' +
                '<div class="input-group-prepend">' +
                    '<span class="input-group-text">' + currencySymbol + '</span>' +
                '</div>' +
                '<input type="number" class="form-control tier-amount-for-point" value="" min="0.01" step="0.01">' +
            '</div>' +
        '</td>' +
        '<td>' +
            '<select class="form-control form-control-sm tier-status">' +
                '<option value="true" selected>Active</option>' +
                '<option value="false">Inactive</option>' +
            '</select>' +
        '</td>' +
        '<td>' +
            '<button type="button" class="btn btn-sm btn-danger" onclick="removeTierRow(this)" title="Remove">' +
                '<i class="fas fa-trash"></i>' +
            '</button>' +
        '</td>' +
    '</tr>';
    
    // Remove empty message if exists
    $('#tblTiersBody .empty-message-row').remove();
    
    // Append new row
    tbody.append(newRow);
    
    // Scroll to the new row
    $('html, body').animate({
        scrollTop: tbody.find('tr:last').offset().top - 100
    }, 300);
    
    // Focus on the first input
    tbody.find('tr:last .tier-priority').focus();
}

// Old modal-based functions removed - now using inline editing

