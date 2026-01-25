
$(function () {
    calculatePrice();
});

var e1 = true; var e2 = false; e3 = false; e4 = false;
var TransactionDetails = [];
var TransactionId = 0;

function next(id, type, isScroll) {

    if (id == 1) {
        if (type == true) {
            $('.div1').show();
            $('.div2').hide();
            $('.div3').hide();

            $('.div11').hide();
            $('.div22').show();
            $('.div33').show();
        }
        else {
            $('.div1').hide();
            $('.div2').hide();
            $('.div3').hide();

            $('.div11').show();
        }
    }
    else if (id == 2) {
        if (type == true) {
            $('.div1').hide();
            $('.div2').show();
            $('.div3').hide();

            $('.div11').show();
            $('.div22').hide();
            $('.div33').show();
        }
        else {
            $('.div1').hide();
            $('.div2').hide();
            $('.div3').hide();

            $('.div22').show();
        }
    }
    else if (id == 3) {
        if (type == true) {
            $('.div1').hide();
            $('.div2').hide();
            $('.div3').show();

            $('.div11').show();
            $('.div22').show();
            $('.div33').hide();
        }
        else {
            $('.div1').hide();
            $('.div2').hide();
            $('.div3').hide();

            $('.div33').show();
        }
    }

    if (isScroll) {
        //$("html, body").animate({ scrollTop: 0 }, "slow");
        $('html, body').animate({
            scrollTop: $(".subcripBox").offset().top
        }, 500);
    };
}

function fetchAdons(id) {
    var det = {
        IsAutoFetch: $('#chkAutoFetch').is(':checked'),
        TermLengthId: $('input[name="chkTermLength"]:checked').val(),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/pricing/fetchAdons',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divAddons").html(data);

            for (let t = 0; t < TransactionDetails.length; t++) {
                for (var i = 0; i < 20; i++) {
                    var Type = $('#hdnType' + i).val();
                    if (Type == TransactionDetails[t].Type) {
                        $('#chkPlanAddon' + i).prop('checked', true);
                        $('#txtPlanAddonQuantity' + i).val(TransactionDetails[t].Quantity);
                        i = 20;
                    }
                }
            }

            next(id, true);
            calculatePrice();
        },
        error: function (xhr) {

        }
    });
};

function setAddons() {
    var TermLengthId = $('input[name="chkTermLength"]:checked').val();
    var Months = $('#hdnTermLength' + TermLengthId).val();
    var SellingPrice = parseFloat($('#hdnPlanSellingPrice' + TermLengthId).val()).toFixed(2);
    var MRP = parseFloat($('#hdnPlanMRP' + TermLengthId).val()).toFixed(2);
    var SubTotal = (MRP * Months).toFixed(2);
    var PayableCost = (SellingPrice * Months).toFixed(2);

    TransactionDetails = [{
        PlanAddonsId: 0,
        AddonName: 'Base Plan',
        UnitCost: SellingPrice,
        Quantity: Months,
        MRP: MRP,
        hdnDiscountPercentage: 0,
        SubTotal: SubTotal,
        PayableCost: PayableCost
    }];

    for (var i = 0; i < 20; i++) {
        if ($('#chkPlanAddon' + i).is(':checked') == true) {
            $('#txtPlanAddonQuantity' + i).removeAttr('disabled');
            var Type = $('#hdnType' + i).val();

            var quantity = 1;
            if ($('#txtPlanAddonQuantity' + i).val() && $('#txtPlanAddonQuantity' + i).val() != 0) {
                quantity = $('#txtPlanAddonQuantity' + i).val();
            }
            else {
                $('#txtPlanAddonQuantity' + i).val('1');
            }

            if ($('#hdnPricingType' + i).val() == 2) {
                //$('#lblPlanAddonSellingPrice' + i).text(($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat($('#hdnUnitCost' + i).val()) * quantity).toFixed(2));
                TransactionDetails.push({
                    PlanAddonsId: $('#chkPlanAddon' + i).val(),
                    AddonName: $('#hdnPlanAddon' + i).val(),
                    UnitCost: $('#hdnUnitCost' + i).val(),
                    Quantity: $('#txtPlanAddonQuantity' + i).val(),
                    MRP: $('#hdnMRP' + i).val(),
                    hdnDiscountPercentage: $('#hdnDiscountPercentage' + i).val(),
                    SubTotal: (parseFloat($('#hdnMRP' + i).val()) * quantity).toFixed(2),
                    PayableCost: (parseFloat($('#hdnUnitCost' + i).val()) * quantity).toFixed(2),
                    Type: Type
                });
            }
            else {
                //$('#lblPlanAddonSellingPrice' + i).text(($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat($('#hdnUnitCost' + i).val()) * Months * quantity).toFixed(2));
                TransactionDetails.push({
                    PlanAddonsId: $('#chkPlanAddon' + i).val(),
                    AddonName: $('#hdnPlanAddon' + i).val(),
                    UnitCost: $('#hdnUnitCost' + i).val(),
                    Quantity: $('#txtPlanAddonQuantity' + i).val(),
                    MRP: $('#hdnMRP' + i).val(),
                    hdnDiscountPercentage: $('#hdnDiscountPercentage' + i).val(),
                    SubTotal: (parseFloat($('#hdnMRP' + i).val()) * Months * quantity).toFixed(2),
                    PayableCost: (parseFloat($('#hdnUnitCost' + i).val()) * Months * quantity).toFixed(2),
                    Type: Type
                });
            }
        }
        else {
            //if ($('#hdnPricingType' + i).val() == 2) {
            //    $('#lblPlanAddonSellingPrice' + i).text(($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat($('#hdnUnitCost' + i).val())).toFixed(2));
            //}
            //else {
            //    $('#lblPlanAddonSellingPrice' + i).text(($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat($('#hdnUnitCost' + i).val()) * Months).toFixed(2));
            //}
            $('#txtPlanAddonQuantity' + i).attr('disabled', 'disabled');
            $('#txtPlanAddonQuantity' + i).val('');
        }
    }
}

function Summary() {
    var discountBreakups = '', showDiscountBreakups = false;
    var SubTotal = 0, PayableCost = 0;
    $('.divSummary').html('');
    var html = '';
    $.each(TransactionDetails, function (index, value) {
        SubTotal = parseFloat(SubTotal) + parseFloat(value.SubTotal);
        PayableCost = parseFloat(PayableCost) + parseFloat(value.PayableCost);
        html = html + '<ul><li> ' + value.AddonName + '</li ><li><span>' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + value.SubTotal + '</span></li></ul>';
    });

    var _couponDiscount = 0, _yearlyPlanDiscount = 0;

    _yearlyPlanDiscount = SubTotal - PayableCost;

    if (_yearlyPlanDiscount > 0) {
        discountBreakups = discountBreakups + 'Yearly Plan Discount -' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + _yearlyPlanDiscount + '</br>';
        showDiscountBreakups = true;
    }

    var DiscountType = $('#hdnDiscountType').val();
    var Discount = $('#hdnDiscount').val();
    if (DiscountType != null && DiscountType != 0) {
        if (DiscountType == 1) {
            _couponDiscount = parseFloat(Discount);
        }
        else {
            _couponDiscount = (parseFloat(Discount) / 100) * SubTotal;
        }

        discountBreakups = discountBreakups + 'Coupon Discount -' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + _couponDiscount + '</br>';
        showDiscountBreakups = true;
    }

    html = html + '<ul style="margin-left:3rem;font-weight:bold;border-bottom:0"><li>Sub Total </li><li><span>' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + SubTotal.toFixed(2) + '</span></li></ul>';
    html = html + '<ul style="margin-left:3rem;font-weight:bold;border-bottom:0;display:' + (showDiscountBreakups == true ? '' : 'none') + '"><li>Discount </li><li><span>' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat(_couponDiscount.toFixed(2)) + parseFloat(_yearlyPlanDiscount.toFixed(2))) + '</span></li></ul>';
    html = html + '<ul style="margin-left:3rem;font-weight:bold;border-bottom:0"><li>Total Payable </li><li><span>' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (SubTotal - _couponDiscount - _yearlyPlanDiscount).toFixed(2) + '</span></li></ul>';

    $('.divSummary').append(html);

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
}

function InitPrice() {
    var det = {
        TermLengthId: $('input[name="chkTermLength"]:checked').val(),
        IsActive: true,
        IsDeleted: false,
        TransactionDetails: TransactionDetails,
        CountryId: $('#ddlCountry').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/InitPlanPrice',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (data.Data.Transaction.WarningMsg != "") {
                    $('#lblWarningMsg').html(data.Data.Transaction.WarningMsg);
                    $('#lblWarningMsg').show();
                }
                else {
                    $('#lblWarningMsg').hide();
                }

                //if (data.Data.Transaction.CountryId != 2) {
                //    InititatePaypal(data.Data.Transaction);
                //    $('#btnRazorPay').hide();
                //}
                //else {
                //    $('#paypal-button-container').remove();
                //    $('#btnRazorPay').show();
                //}
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchPricing() {
    var det = {
        CountryId: $('#ddlCountry').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/Pricing/FetchPricing',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPricing").html(data);
            calculatePrice();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function calculatePrice() {
    setAddons();
    Summary();
    InitPrice();
}