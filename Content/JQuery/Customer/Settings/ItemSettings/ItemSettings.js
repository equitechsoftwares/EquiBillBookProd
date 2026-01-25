
$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $('.select2').select2();
});
function itemUpdate() {
    var det = {
        EnableItemExpiry: $('#chkEnableItemExpiry').is(':checked'),
        ExpiryType: $('#ddlExpiryType').val(),
        OnItemExpiry: $('#ddlOnItemExpiry').val(),
        StopSellingBeforeDays: $('#txtStopSellingBeforeDays').val(),
        EnableBrands: $('#chkEnableBrands').is(':checked'),
        EnableCategory: $('#chkEnableCategory').is(':checked'),
        EnableSubCategory: $('#chkEnableSubCategory').is(':checked'),
        EnableSubSubCategory: $('#chkEnableSubSubCategory').is(':checked'),
        EnableTax_PriceInfo: $('#chkEnableTax_PriceInfo').is(':checked'),
        DefaultUnitId: $('#ddlDefaultUnit').val(),
        EnableSecondaryUnit: $('#chkEnableSecondaryUnit').is(':checked'),
        EnableTertiaryUnit: $('#chkEnableTertiaryUnit').is(':checked'),
        EnableQuaternaryUnit: $('#chkEnableQuaternaryUnit').is(':checked'),
        EnableRacks: $('#chkEnableRacks').is(':checked'),
        EnableRow: $('#chkEnableRow').is(':checked'),
        EnablePosition: $('#chkEnablePosition').is(':checked'),
        EnableWarranty: $('#chkEnableWarranty').is(':checked'),
        DefaultProfitPercent: $('#txtDefaultProfitPercent').val(),
        EnableLotNo: $('#chkEnableLotNo').is(':checked'),
        StockAccountingMethod: $('#ddlStockAccountingMethod').val(),
        EnableProductVariation: $('#chkEnableProductVariation').is(':checked'),
        EnableComboProduct: $('#chkEnableComboProduct').is(':checked'),
        EnableProductDescription: $('#chkEnableProductDescription').is(':checked'),
        EnableImei: $('#chkEnableImei').is(':checked'),
        EnablePrintLabel: $('#chkEnablePrintLabel').is(':checked'),
        EnableStockAdjustment: $('#chkEnableStockAdjustment').is(':checked'),
        ExpiryDateFormat: $('#ddlExpiryDateFormat').val(),
        EnableMrp: $('#chkEnableMrp').is(':checked'),
        EnableStockTransfer: $('#chkEnableStockTransfer').is(':checked'),
        EnableSellingPriceGroup: $('#chkEnableSellingPriceGroup').is(':checked'),
        TaxType: $('#ddlTaxType').val(),
        EnableSalt: $('#chkEnableSalt').is(':checked'),
        EnableItemImage: $('#chkEnableItemImage').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ItemSettingsUpdate',
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
                //fetchPrefix();
                fetchMenuPermissions();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertUnit() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        UnitCode: $('#txtUnitCode').val(),
        UnitName: $('#txtUnitName').val(),
        UnitShortName: $('#txtUnitShortName').val(),
        AllowDecimal: $('#ddlAllowDecimal').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/UnitInsert',
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
                $('#ddlDefaultUnit').append($('<option>', { value: data.Data.Unit.UnitId, text: data.Data.Unit.UnitName }));
                $('#ddlDefaultUnit').val(data.Data.Unit.UnitId);
                $('#unitModal').modal('toggle');

                $('#txtUnitName').val('');
                $('#txtUnitShortName').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleSubCategory() {
    if (!$('#chkEnableSubCategory').is(':checked')) {
        $('#chkEnableSubSubCategory').prop('checked', false);
    }
}

function toggleSubSubCategory() {
    if ($('#chkEnableSubSubCategory').is(':checked')) {
        $('#chkEnableSubCategory').prop('checked', true);
    }
}

function toggleSecondaryUnit() {
    if (!$('#chkEnableSecondaryUnit').is(':checked')) {
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#chkEnableQuaternaryUnit').prop('checked', false);
    }
    //else {
    //    $('#chkEnableLotNo').prop('checked', true);
    //}
}

function toggleTertiaryUnit() {
    if (!$('#chkEnableTertiaryUnit').is(':checked')) {
        $('#chkEnableQuaternaryUnit').prop('checked', false);
    }
    else {
        $('#chkEnableSecondaryUnit').prop('checked', true);

        //$('#chkEnableLotNo').prop('checked', true);
    }
}

function toggleQuaternaryUnit() {
    if ($('#chkEnableQuaternaryUnit').is(':checked')) {
        $('#chkEnableTertiaryUnit').prop('checked', true);
        $('#chkEnableSecondaryUnit').prop('checked', true);

        //$('#chkEnableLotNo').prop('checked', true);
    }
}

function toggleEnableRacks() {
    if (!$('#chkEnableRacks').is(':checked')) {
        $('#chkEnableRow').prop('checked', false);
        $('#chkEnablePosition').prop('checked', false);
    }
}

function toggleEnableRow() {
    if (!$('#chkEnableRow').is(':checked')) {
        $('#chkEnablePosition').prop('checked', false);
    }
    else {
        $('#chkEnableRacks').prop('checked', true);
    }
}

function toggleEnablePosition() {
    if ($('#chkEnablePosition').is(':checked')) {
        $('#chkEnableRow').prop('checked', true);
        $('#chkEnableRacks').prop('checked', true);
    }
}

function checkBranchCount() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchCount',
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
                if (data.Data.TotalCount == 1) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error("You should have atleast 2 active Business Locations to enable this Stock Transfer feature");

                    $('#chkEnableStockTransfer').prop('checked', false);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function checkLotNo() {
    if ($('#chkEnableLotNo').is(':checked')) {
        $('.divLot').show();
    }
    else {
        $('#chkEnableItemExpiry').prop('checked', false);
        $('#chkEnableSecondaryUnit').prop('checked', false);
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#chkEnableQuaternaryUnit').prop('checked', false);

        $('#chkSalesEnableFreeQuantity').prop('checked', false);
        $('#chkPosEnableFreeQuantity').prop('checked', false);
        $('#chkEnableFreeQuantity').prop('checked', false);
        $('.divLot').hide();
        $('.divItemExpiry').hide();
    }
}

function toggleItemExpiry() {
    if ($('#chkEnableItemExpiry').is(':checked')) {
        $('.divItemExpiry').show();
    }
    else {
        $('.divItemExpiry').hide();
        $('#ddlExpiryType').val(1);
        $('#ddlOnItemExpiry').val(1);
        $('#txtStopSellingBeforeDays').val(0);
    }
}

function fetchMenuPermissions() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/dashboard/MenuPermissions',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $(".main-sidebar").html(data);
            $('.sidebar').css('overflow-y', 'auto');
            $('.Settings').addClass('menu-open');
            $('.Business-Settings').addClass('active');
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleOnExpiry() {
    if ($('#ddlOnItemExpiry').val() == 1) {
        $('#txtStopSellingBeforeDays').val(0);
        $('#txtStopSellingBeforeDays').prop('disabled', true);
    }
    else {
        $('#txtStopSellingBeforeDays').prop('disabled', false);
    }
}