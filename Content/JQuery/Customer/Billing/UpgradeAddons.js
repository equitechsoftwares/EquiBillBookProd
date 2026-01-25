$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    Summary();

    // Toggle conditions section on "Read more" / "Read less" click
    $(document).on('click', '.toggle-conditions', function (e) {
        e.preventDefault();
        var $toggleLink = $(this);
        var $toggleText = $toggleLink.find('.toggle-text');
        // Find the conditions section in the parent container
        var $conditionsSection = $toggleLink.siblings('.conditions-section');
        
        if ($conditionsSection.length > 0) {
            if ($conditionsSection.is(':visible')) {
                $conditionsSection.slideUp(300);
                $toggleText.text('Read more');
            } else {
                $conditionsSection.removeAttr('style').hide().slideDown(300);
                $toggleText.text('Read less');
            }
        }
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

//var CurrencySymbol = Cookies.get('data').split('&')[5].split('=')[1];
var e3 = true; e4 = false;
var TransactionDetails = [];
var TransactionNo = 0;

function next(id, isScroll) {
    if (id == 3) {
        $('.div1').hide();
        $('.div2').hide();
        $('.div3').show();
        $('.div4').hide();

        e3 = true;

        if (e3 == true) {
            $('.div33').hide();
        }
        if (e4 == true) {
            $('.div44').show();
        }
    }
    else if (id == 4) {
        $('.div1').hide();
        $('.div2').hide();
        $('.div3').hide();
        $('.div4').show();

        e4 = true;

        if (e3 == true) {
            $('.div33').show();
        }
        if (e4 == true) {
            $('.div44').hide();
        }

        if (isScroll) {
            $('html, body').animate({
                scrollTop: $("#divAddons").offset().top
            }, 500);
        };

        setAddons();
        InitPrice();
    }
    setAddons();
    Summary();
}

function insert(i) {
    var det = {
        IsActive: true,
        IsDeleted: false,
        TransactionDetails: TransactionDetails,
        CouponId: $('#hdnCouponId').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/InsertAddonTransaction',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (i == 1) {
                    sessionStorage.setItem('showMsg', '1');
                    sessionStorage.setItem('Msg', data.Message);

                    window.location.href = "/subscription/mytransactions";
                }
                else {
                    TransactionNo = data.Data.Transaction.TransactionNo;

                    var isDefault = false;

                    if (data.Data.Transaction.MainCountryId == data.Data.Transaction.CountryId) {
                        isDefault = true;
                    }

                    if (data.Data.Transaction.OnlinePaymentSettings != null) {
                        $.each(data.Data.Transaction.OnlinePaymentSettings, function (index, value) {
                            if (isDefault == true) {
                                if (value.IsDefault == true) {
                                    if (value.OnlinePaymentService == 1) {
                                        InititatePaypal(data.Data.Transaction);
                                        $('#btnRazorPay').hide();
                                    }
                                    else if (value.OnlinePaymentService == 2) {
                                        $('#paypal-button-container').remove();
                                        InitiateRazorPay(data.Data.Transaction, value.RazorpayKey, value.RazorpayCurrencyCode);
                                        $('#btnRazorPay').show();
                                    }
                                }
                            }
                            else {
                                if (value.IsDefault == false) {
                                    if (value.OnlinePaymentService == 1) {
                                        InititatePaypal(data.Data.Transaction);
                                        $('#btnRazorPay').hide();
                                    }
                                    else if (value.OnlinePaymentService == 2) {
                                        $('#paypal-button-container').remove();
                                        InitiateRazorPay(data.Data.Transaction, value.RazorpayKey, value.RazorpayCurrencyCode);
                                        $('#btnRazorPay').show();
                                    }
                                }
                            }
                        });
                    }

                    //if (data.Data.Transaction.CountryId != 2) {
                    //    InititatePaypal(data.Data.Transaction);
                    //    $('#btnRazorPay').hide();
                    //}
                    //else {
                    //    $('#paypal-button-container').remove();
                    //    InitiateRazorPay(data.Data.Transaction);
                    //    $('#btnRazorPay').show();
                    //}

                    //InitiateRazorPay(data.Data.Transaction);
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function setAddons() {
    var Months = $('#hdnMonths').val();
    //var Total = 0;

    TransactionDetails = [];

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
        html = html + '<li class="nav-item"><h6 class="nav-link">' + value.AddonName + ' <span class="float-right text-bold"> ' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + value.SubTotal + '</span></h6></li>'
    });

    var _couponDiscount = 0, _yearlyPlanDiscount = 0;

    _yearlyPlanDiscount = SubTotal - PayableCost;

    if (_yearlyPlanDiscount > 0) {
        discountBreakups = discountBreakups + 'Yearly Plan Discount -' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + _yearlyPlanDiscount + '</br>';
        showDiscountBreakups = true;
    }

    var DiscountType = $('#hdnDiscountType').val();
    var CalculatedDiscount = $('#hdnCalculatedDiscount').val();
    if (DiscountType != null && DiscountType != 0 && CalculatedDiscount && CalculatedDiscount != '' && CalculatedDiscount != '0') {
        // Use the calculated discount from backend (already includes Maximum Discount Amount cap)
        _couponDiscount = parseFloat(CalculatedDiscount);

        discountBreakups = discountBreakups + 'Coupon Discount -' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + _couponDiscount + '</br>';
        showDiscountBreakups = true;
    }

    html = html + '<li class="nav-item ml-5" style="border-bottom:0px;"><h6 class="nav-link text-bold">Sub Total <span class="float-right text-bold"> ' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + SubTotal.toFixed(2) + '</span></h6></li>';
    /*html = html + '<li class="nav-item ml-5" style="border-bottom:0px;"><h6 class="nav-link text-bold">Discount ' + '<i tabindex="0" class="fa fa-info-circle text-info hover-q no-print ' + (showDiscountBreakups == true ? '' : 'hidden') + '" role="button" data-toggle="popover" data-trigger="focus" data-content="Discount Breakups: </br>' + discountBreakups + '"></i>' + '<span class="float-right text-bold"> ' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat(_couponDiscount.toFixed(2)) + parseFloat(_yearlyPlanDiscount.toFixed(2))) + '</span></h6></li>';*/
    html = html + '<li class="nav-item ml-5" style="border-bottom:0px;"><h6 class="nav-link text-bold">Discount <span class="float-right text-bold"> ' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (parseFloat(_couponDiscount.toFixed(2)) + parseFloat(_yearlyPlanDiscount.toFixed(2))) + '</span></h6></li>';
    html = html + '<li class="nav-item ml-5"><h6 class="nav-link text-bold">Total Payable <span class="float-right text-bold"> ' + ($('#hdnCountryId').val() == $('#hdnMainCountryId').val() ? $('#hdnCurrencySymbol').val() : '$') + (SubTotal - _couponDiscount - _yearlyPlanDiscount).toFixed(2) + '</span></h6></li>';

    $('.divSummary').append(html);

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
}

function InitiateRazorPay(item, RazorpayKey, RazorpayCurrencyCode) {
    var options = {
        "key": RazorpayKey,
        "amount": item.PayableCost * 100,
        "currency": RazorpayCurrencyCode,
        "name": item.BusinessName,
        "image": item.BusinessLogo,
        //"order_id": "order_9A33XWu170gUtm", //This is a sample Order ID. Pass the `id` obtained in the response of Step 1
        "handler": function (response) {
            PaymentSuccess(response.razorpay_payment_id, 'Razor Pay');
        },
        "prefill": {
            "name": item.Name,
            "email": item.EmailId,
            "contact": item.MobileNo
        },
        "notes": {
            "address": ""
        },
        "theme": {
            "color": "#F37254"
        }
    };
    var rzp1 = new Razorpay(options);
    rzp1.open();
}

function PaymentSuccess(LiveTransactionId, PaymentGatewayType) {
    var det = {
        TransactionNo: TransactionNo,
        LiveTransactionId: LiveTransactionId,
        PaymentGatewayType: PaymentGatewayType,
        PaymentMethodType: ''
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/PaymentSuccess',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                window.location.href = "/subscription/success?invoiceid=" + data.Data.Transaction.TransactionNo;
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function InititatePaypal(item) {
    $('#paypal-button-container').remove();
    $('#divPaypal').append('<div class="" id="paypal-button-container" ></div>');

    paypal.Buttons({
        style: {
            layout: 'horizontal',
            color: 'blue',
            shape: 'rect',
            label: 'pay',
            size: 'responsive',
            height: 37,
        },
        // Set up the transaction
        createOrder: function (data, actions) {
            return actions.order.create({
                purchase_units: [{
                    amount: {
                        value: item.PayableCost
                    }
                }]
            });
        },

        // Finalize the transaction
        onApprove: function (data, actions) {
            return actions.order.capture().then(function (orderData) {
                // Successful capture! For demo purposes:
                var transaction = orderData.purchase_units[0].payments.captures[0];
                LiveTransactionId = transaction.id;
                PaymentSuccess(transaction.id, 'Paypal');
            });
        }

    }).render('#paypal-button-container');
}

function InitPrice() {
    if (TransactionDetails.length > 0) {
        var det = {
            TransactionDetails: TransactionDetails
        };
        $("#divLoading").show();
        $.ajax({
            url: '/subscription/InitAddonPrice',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    var isDefault = false;

                    if (data.Data.Transaction.MainCountryId == data.Data.Transaction.CountryId) {
                        isDefault = true;
                    }

                    if (data.Data.Transaction.OnlinePaymentSettings != null) {
                        $.each(data.Data.Transaction.OnlinePaymentSettings, function (index, value) {
                            if (isDefault == true) {
                                if (value.IsDefault == true) {
                                    if (value.OnlinePaymentService == 1) {
                                        InititatePaypal(data.Data.Transaction);
                                        $('#btnRazorPay').hide();
                                    }
                                    else if (value.OnlinePaymentService == 2) {
                                        $('#paypal-button-container').remove();
                                        $('#btnRazorPay').show();
                                    }
                                }
                            }
                            else {
                                if (value.IsDefault == false) {
                                    if (value.OnlinePaymentService == 1) {
                                        InititatePaypal(data.Data.Transaction);
                                        $('#btnRazorPay').hide();
                                    }
                                    else if (value.OnlinePaymentService == 2) {
                                        $('#paypal-button-container').remove();
                                        $('#btnRazorPay').show();
                                    }
                                }
                            }
                        });
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
}

function InitRazorPayPrice() {
    var det = {
        TransactionDetails: TransactionDetails
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/InitAddonPrice',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                //InitiateRazorPay(data.Data.Transaction);
                insert_pay(Math.floor((Math.random() * 10000)), 'Razor Pay');
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insert_pay(LiveTransactionId, PaymentGatewayType) {
    var det = {
        IsActive: true,
        IsDeleted: false,
        LiveTransactionId: LiveTransactionId,
        PaymentGatewayType: PaymentGatewayType,
        PaymentMethodType: '',
        TransactionDetails: TransactionDetails,
        CouponId: $('#hdnCouponId').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/InsertAddonTransaction_Paid',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                window.location.href = "/subscription/success?invoiceid=" + data.Data.Transaction.TransactionNo;
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openCoupon() {
    $('.divCoupon').hide();
    $('.divApplyCoupon').show();
}

function ApplyCoupon(couponCode) {
    if (couponCode) {
        $('#txtCouponCode').val(couponCode);
    }
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    // Calculate totals from TransactionDetails (addons only - no base plan)
    var SubTotal = 0;
    var PlanSubTotal = 0; // Always 0 for addons only
    var AddonsSubTotal = 0;
    var SelectedAddonIds = [];
    var AddonSubTotals = [];
    
    $.each(TransactionDetails, function (index, value) {
        SubTotal = parseFloat(SubTotal) + parseFloat(value.SubTotal);
        AddonsSubTotal = parseFloat(AddonsSubTotal) + parseFloat(value.SubTotal);
        if (value.PlanAddonsId > 0) {
            SelectedAddonIds.push(parseInt(value.PlanAddonsId));
            AddonSubTotals.push({
                PlanAddonsId: parseInt(value.PlanAddonsId),
                SubTotal: parseFloat(value.SubTotal)
            });
        }
    });
    
    var det = {
        CouponCode: $('#txtCouponCode').val(),
        TermLengthId: 0, // No term length selection for addons only
        SubTotal: SubTotal,
        PlanSubTotal: PlanSubTotal,
        AddonsSubTotal: AddonsSubTotal,
        SelectedAddonIds: SelectedAddonIds,
        AddonSubTotals: AddonSubTotals
    };
    $("#divLoading").show();
    $.ajax({
        url: '/subscription/ApplyCoupon',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {

                $('#hdnCouponId').val(0);
                $('#hdnDiscountType').val(0);
                $('#hdnDiscount').val(0);
                $('#hdnCalculatedDiscount').val(0);

                $('#txtCouponCode').prop('disabled', false);

                Summary();

                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {

                $('#hdnCouponId').val(0);
                $('#hdnDiscountType').val(0);
                $('#hdnDiscount').val(0);
                $('#hdnCalculatedDiscount').val(0);

                $('#txtCouponCode').prop('disabled', false);

                Summary();

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
            }
            else {
                $('#divCouponSuccessMsg').text(data.Message);
                $('#divCouponSuccessMsg').show();

                $('#hdnCouponId').val(data.Data.Coupon.CouponId);
                $('#hdnDiscountType').val(data.Data.Coupon.DiscountType);
                $('#hdnDiscount').val(data.Data.Coupon.Discount);
                $('#hdnCalculatedDiscount').val(data.Data.Coupon.CalculatedDiscount || 0);

                $('#btnApplyCoupon').hide();
                $('#btnRemoveCoupon').show();

                $('#txtCouponCode').prop('disabled', true);

                Summary();
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function RemoveCoupon() {
    $('#divCouponSuccessMsg').text('');
    $('#divCouponSuccessMsg').hide();

    $('#hdnCouponId').val(0);
    $('#hdnDiscountType').val(0);
    $('#hdnDiscount').val(0);
    $('#hdnCalculatedDiscount').val(0);

    $('#btnApplyCoupon').show();
    $('#btnRemoveCoupon').hide();

    $('#txtCouponCode').val('');

    $('#txtCouponCode').prop('disabled', false);

    Summary();
}

function calculatePrice() {
    // Remove coupon if applied when addons change
    var couponId = $('#hdnCouponId').val();
    if (couponId && couponId != '0' && couponId != 0) {
        RemoveCoupon();
    }
    
    setAddons();
    Summary();
    InitPrice();
}