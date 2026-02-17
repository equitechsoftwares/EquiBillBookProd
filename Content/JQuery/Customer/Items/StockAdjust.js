$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_AdjustmentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_AdjustmentDate').addClass('notranslate');

    if (window.location.href.indexOf('edit') > -1) {
        convertAvailableStock();
    }

    fetchCompanyCurrency();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var CurrencySymbol = "";//Cookies.get('data').split('&')[5].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;
var count = 1; var innerCount = 1; var dropdownHtml = '';
var _StockAdjustmentId = 0, _CheckStockPriceMismatch = true, _i;
checkAdjustmentType();

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
        BranchId: $('#ddlBranch').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        AdjustmentType: $('#ddlAdjustmentType').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/stockadjust/stockadjustmentFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
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
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function View(StockAdjustmentId) {
    var det = {
        StockAdjustmentId: StockAdjustmentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockAdjust/StockAdjustmentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ViewModal').modal('toggle');
            $("#divView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchCompanyCurrency() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/usersettings/FetchCompanyCurrency',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
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
                CurrencySymbol = data.Data.User.CurrencySymbol;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: {
                Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'stock adjustment',
                Type: $('#ddlAdjustmentType').val()
            },
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                    $('#txttags').val('');
                }
                else if (data.Status == 2) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
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
                    $('#txttags').val('');
                }
                else {
                    if (data.Data.ItemsArray.length == 1) {
                        $('#txttags').val('');
                        // Reset autocomplete term to allow re-searching the same value
                        var autocompleteInstance = $('#txttags').data('ui-autocomplete') || $('#txttags').data('autocomplete');
                        if (autocompleteInstance) {
                            autocompleteInstance.term = '';
                        }
                        var splitVal = data.Data.ItemsArray[0].split('~');
                        fetchItem(splitVal[splitVal.length - 1]);
                    }
                    else {
                        response(data.Data.ItemsArray);
                    }
                }
            },
            error: function (a, b, c) {
                HandleLookUpError(a);
            }
        });
    },
    select: function (event, ui) {
        var splitVal = ui.item.value.split('~');
        fetchItem(splitVal[splitVal.length - 1]);
    }
});

// Reset autocomplete term when input is cleared manually to allow re-searching the same value
$('#txttags').on('input', function() {
    if ($(this).val() === '') {
        var autocompleteInstance = $(this).data('ui-autocomplete') || $(this).data('autocomplete');
        if (autocompleteInstance) {
            autocompleteInstance.term = '';
        }
    }
});

function fetchItem(SkuHsnCode) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/items/SearchItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var html = '';
            //var vari = '';
            $('#txttags').val('');
            // Reset autocomplete term to allow re-searching the same value
            var autocompleteInstance = $('#txttags').data('ui-autocomplete') || $('#txttags').data('autocomplete');
            if (autocompleteInstance) {
                autocompleteInstance.term = '';
            }
            var z = count;
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {
                var isPresent = false;

                if (data.Data.ItemDetails[i].SecondaryUnitShortName == null) {
                    if ($("#ddlAdjustmentType  option:selected").val() == "Debit") {
                        $('#divCombo tr').each(function () {
                            var _id = this.id.split('divCombo')[1];
                            var ItemDetailsId = $('#txtItemDetailsId' + _id).val();
                            if (ItemDetailsId == data.Data.ItemDetails[i].ItemDetailsId) {
                                $('#txtQuantity' + _id).val(parseInt($('#txtQuantity' + _id).val()) + 1);
                                isPresent = true;
                            }
                        });
                    }
                }

                if (isPresent == false) {
                    let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);
                    var variation = '';
                    if (data.Data.ItemDetails[i].VariationName) {
                        variation = '</br> Variation : ' + data.Data.ItemDetails[i].VariationName;
                    }
                    var ddlUnit = '<select class="form-control select2" style="width: 100 %; " id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';
                    if (data.Data.ItemDetails[i].QuaternaryUnitId != 0) {
                        if (data.Data.ItemDetails[i].PriceAddedFor == 1) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-1-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-1-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-2-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-3-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '-4-4' + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                        }
                    }
                    else if (data.Data.ItemDetails[i].TertiaryUnitId != 0) {
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-2-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-2-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-3-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '-4-3' + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                        }
                    }
                    else if (data.Data.ItemDetails[i].SecondaryUnitId != 0) {
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-3-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '-3-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        }
                        if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                            ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                        else {
                            ddlUnit = ddlUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '-4-2' + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                        }
                    }
                    else {
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        //ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                        ddlUnit = ddlUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '-4-1' + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    }

                    ddlUnit = ddlUnit + '</select >';

                    var EnableLotNo = $('#hdnEnableLotNo').val().toLocaleLowerCase();
                    var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
                    var ExpiryType = $('#hdnExpiryType').val();
                    var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
                    var OnItemExpiry = $('#hdnOnItemExpiry').val();

                    //Set dropdown for lots
                    if (data.Data.ItemDetails[i].AvailableLots != null) {
                        var ddlLot = '<select class="form-control" id="ddlLot' + count + '" onchange="fetchLotDetails(' + count + ')">';
                        //if (OnItemExpiry != 2) {
                        //    ddlLot = ddlLot + '<option selected value="0-all" > Default Stock Accounting Method</option>';
                        //}

                        for (let j = 0; j < data.Data.ItemDetails[i].AvailableLots.length; j++) {
                            ddlLot = ddlLot + '<option value="' + data.Data.ItemDetails[i].AvailableLots[j].Id + '-' + data.Data.ItemDetails[i].AvailableLots[j].Type + '"> Lot No: ' + (!data.Data.ItemDetails[i].AvailableLots[j].LotNo ? "" : data.Data.ItemDetails[i].AvailableLots[j].LotNo);
                            if (EnableItemExpiry == 'true') {
                                ddlLot = ddlLot + (!data.Data.ItemDetails[i].AvailableLots[j].ManufacturingDate ? "" : '- Mfg Date: ' + getFormattedDate(new Date(parseInt(data.Data.ItemDetails[i].AvailableLots[j].ManufacturingDate.replaceAll('/', '').replaceAll('Date', '').replace('(', '').replace(')', ''))).toUTCString()));
                                ddlLot = ddlLot + (!data.Data.ItemDetails[i].AvailableLots[j].ExpiryDate ? "" : '- Exp Date: ' + getFormattedDate(new Date(parseInt(data.Data.ItemDetails[i].AvailableLots[j].ExpiryDate.replaceAll('/', '').replaceAll('Date', '').replace('(', '').replace(')', ''))).toUTCString()));
                            }
                            ddlLot = ddlLot + '</option>';
                        }
                        ddlLot = ddlLot + '</select>';
                        //Set dropdown for lots
                    }

                    html = html + '<tr id="divCombo' + count + '">' +
                        /*'<td>' + count + '</td>' +*/
                        '<td style="min-width:200px;">' +
                        '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                        '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length > 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName.substring(0, 15) + '...</span>' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName + '</span>' +
                        variation +
                        ' </br> <b>SKU :</b> ' + data.Data.ItemDetails[i].SKU + '' +
                        '</td>' +
                        '<td style="min-width:150px" class="' + (EnableLotNo == 'true' ? '' : 'hidden') + '"> ' + ddlLot +
                        '</td>' +
                        '<td style="min-width:150px"> ' +
                        '<div class="input-group" style="min-width:180px">' +
                        ddlUnit +
                        '</div >' +
                        '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUnitCost' + count + '" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                        '</td>' +
                        '<td style="min-width:150px" class="text-center divIsDebit">' +
                        '<div class="input-group">' +
                        '<input type="number" disabled class="form-control" value="' + data.Data.ItemDetails[i].Quantity + '" id="txtStockQuantity' + count + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuantityRemaining' + count + '" value="' + data.Data.ItemDetails[i].Quantity + '">' +
                        '</div>' +
                        '</td>' +
                        '<td style="min-width:150px"> ' +
                        '<div class="input-group">' +
                        '<input type="number" class="form-control divQuantity' + count + '_ctrl" value="1" id="txtQuantity' + count + '" onkeypress="return toggleDecimal(event,' + count + ')" onchange="updateQuantity(' + count + ',2)" min="0">' +
                        '</div >' +
                        '<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small>' +
                        '</td>' +
                        //'<td style="min-width:100px">' +
                        //'<input type="number" class="form-control" value="1" id="txtQuantity' + count + '" onkeypress="return toggleDecimal(event,' + count + ')" onchange="updateQuantity(' + count + ',2)" min="0">' +
                        //'</td>' +
                        '<td style="min-width:150px">' +
                        '<div class="input-group">' +
                        '<input type="number" disabled class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '" onchange="ChangeQtyAmount(' + count + ')" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '">' +
                        '</div >' +
                        '<small class="text-red font-weight-bold errorText" id="divUnitCost' + count + '"></small>' +
                        '</td>' +
                        '<td style="min-width:150px">' +
                        '<input type="number" disabled class="form-control"  id="txtTotalAmount' + count + '" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '" >' +
                        '</td>' +
                        //'<td style="min-width:100px" class="' + (EnableLotNo == 'true' ? '' : 'hidden') + ' divIsLot">' +
                        //'<input type="text" class="form-control" id="txtLotNo' + count + '">' +
                        //'</td>' +
                        //'<td style="min-width:100px" class="' + ((EnableItemExpiry == 'true' && ExpiryType == '2') ? '' : 'hidden') + ' divIsMfg">' +
                        //'<input type="date" class="form-control" id="txtManufacturingDate' + count + '">' +
                        //'</td>' +
                        //'<td style="min-width:100px" class="' + (EnableItemExpiry == 'true' ? '' : 'hidden') + ' divIsExp">' +
                        //'<input type="date" class="form-control" id="txtExpiryDate' + count + '">' +
                        //'</td>' +
                        //'<td style="min-width:100px" class="divIsCredit">' +
                        //'<input type="number" class="form-control" id="txtSalesIncTax' + count + '"  value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        //'</td>' +
                        '<td>' +
                        '<button type="button" class="btn btn-danger btn-sm" onclick="deleteCombo(' + count + ')">' +
                        '<i class="fas fa-times">' +
                        '</i>' +
                        '</button>' +
                        '</td>' +
                        '</tr>';
                }

                count++;
            }
            $('#divCombo').append(html);

            updateAmount();
            checkAdjustmentType();

            if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
                convertAvailableStock();
            }

            for (var g = 1; g <= (count - z); g++) {
                if (EnableLotNo == "true") {
                    fetchLotDetails(g);
                }
            }

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateAmount() {
    var amount = 0;
    var qty = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (!$('#txtUnitCost' + _id).val()) {
            $('#txtUnitCost' + _id).val($('#hdnUnitCost' + _id).val());
        }
        $('#txtTotalAmount' + _id).val(Math.round((parseFloat($('#txtUnitCost' + _id).val()) * parseFloat($('#txtQuantity' + _id).val())) * 100) / 100);

        amount = amount + parseFloat($('#txtTotalAmount' + _id).val());
        qty = qty + parseFloat($('#txtQuantity' + _id).val());
    });

    $('#divTotalAmount').text(CurrencySymbol + Math.round(amount * 100) / 100);
    $('#hdndivTotalAmount').val(Math.round(amount * 100) / 100);
    $('#divTotalQty').text(qty);
    //chargecalc();
}

function deleteCombo(id, StockAdjustmentDetailsId) {
    if (StockAdjustmentDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {

        $('#divCombo' + id).hide();
        $('#txtQuantity' + id).val(0);
        updateAmount();

        //var det = {
        //    StockAdjustmentDetailsId: StockAdjustmentDetailsId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/StockAdjust/StockAdjustmentDetailsDelete',
        //    datatype: "json",
        //    data: det,
        //    type: "post",
        //    success: function (data) {

        //        $("#divLoading").hide();
        //        if (data == "True") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }
        //        else if (data == "False") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }

        //        if (data.Status == 0) {
        //            if (EnableSound == 'True') { document.getElementById('error').play(); }
        //            toastr.error(data.Message);
        //        }
        //        else {
        //            $('#divCombo' + id).remove();
        //            updateAmount();
        //            //update(false);
        //        }
        //    },
        //    error: function (xhr) {
        //        $("#divLoading").hide();
        //    }
        //});
        //}
    }
    else {
        $('#divCombo' + id).remove();
        //$('#txtQuantity' + id).val(0);
        updateAmount();
    }
}

function deleteFullCombo(StockAdjustmentId) {
    if (StockAdjustmentId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        $('#divCombo tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {
                $('#divCombo' + _id).hide();
                $('#txtQuantity' + _id).val(0);
            }
        });

        updateAmount();

        //var det = {
        //    StockAdjustmentId: StockAdjustmentId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/StockAdjust/StockAdjustmentDetailsDelete',
        //    datatype: "json",
        //    data: det,
        //    type: "post",
        //    success: function (data) {

        //        $("#divLoading").hide();
        //        if (data == "True") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }
        //        else if (data == "False") {
        //            $('#subscriptionExpiryModal').modal('toggle');
        //            $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
        //            $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
        //            return
        //        }

        //        if (data.Status == 0) {
        //            if (EnableSound == 'True') { document.getElementById('error').play(); }
        //            toastr.error(data.Message);
        //        }
        //        else {
        //            $('#divCombo').empty();
        //            //$('#divComboNetAmount').text(0);
        //            updateAmount();
        //            //update(false);
        //        }
        //    },
        //    error: function (xhr) {
        //        $("#divLoading").hide();
        //    }
        //});
        //}
    }
    else {
        $('#divCombo').empty();
        $('#divComboNetAmount').text(0);
        //$('#divCombo tr').each(function () {
        //    var _id = this.id.split('divCombo')[1];
        //    if (_id != undefined) {
        //        $('#divCombo' + _id).hide();
        //        $('#txtQuantity' + _id).val(0);
        //    }
        //});
        updateAmount();
    }
}

function ChangeQtyAmount(i) {
    convertAvailableStock();
    if ($("#ddlAdjustmentType  option:selected").val() == "Debit") {
        var r = checkStockAvailable(i);
        if (r == true) {
            let chngqty = (parseFloat($('#txtQuantity' + i).val()) * parseFloat($('#txtUnitCost' + i).val()));
            $('#txtTotalAmount' + i).val(Math.round(chngqty * 100) / 100);
        }
    }
    else {
        let chngqty = (parseFloat($('#txtQuantity' + i).val()) * parseFloat($('#txtUnitCost' + i).val()));
        $('#txtTotalAmount' + i).val(Math.round(chngqty * 100) / 100);
    }

    updateAmount();
}

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    _i = i;
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var _lot = $('#ddlLot' + _id).val();

            //var hiddenOptionsCount = 0;
            //$("#ddlUnit" + _id + " option").each(function () {
            //    if ($(this)[0].outerHTML.indexOf('hidden') > -1) {
            //        hiddenOptionsCount++;
            //    }
            //});

            ItemDetails.push({
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                Amount: $('#txtTotalAmount' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                //LotNo: $('#txtLotNo' + _id).val(),
                //ManufacturingDate: $('#txtManufacturingDate' + _id).val(),
                //ExpiryDate: $('#txtExpiryDate' + _id).val(),
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                //SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
            })
        }
    });

    var det = {
        BranchId: $('#ddlBranch').val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        AdjustmentDate: moment($("#txtAdjustmentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtAdjustmentDate").val(),
        AdjustmentType: $("#ddlAdjustmentType  option:selected").val(),
        TotalQuantity: $('#divTotalQty').text(),
        TotalAmount: $("#hdndivTotalAmount").val(),
        TotalAmountRecovered: $("#txtTotalAmountRecovered").val(),
        Notes: $("#txtNotes").val(),
        IsActive: true,
        IsDeleted: false,
        StockAdjustmentDetails: ItemDetails,
        CheckStockPriceMismatch: _CheckStockPriceMismatch,
        StockDeductionType: $('#ddlStockDeductionType').val(),
        AccountId: $('#ddlAccount').val(),
        StockAdjustmentReasonId: $('#ddlStockAdjustmentReason').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockAdjust/StockAdjustmentInsert',
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
            else if (data.Status == 3) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                $('#divStockAlertMsg').html('');
                $('#divStockAlertMsg').append('<label> Stock Items Selling Price mismatch. Check details below</label><br /><br />');

                var stockAlertCounter = 1;
                data.Errors.forEach(function (res) {
                    $('#divStockAlertMsg').append('<div>' + stockAlertCounter + '. ' + res.Message + '</div>');
                    stockAlertCounter = stockAlertCounter + 1;
                });

                $('#divStockAlertMsg').append('<br /><br /><br /><div style="font-size:0.9rem"> Your can click cancel button to wait till your previous stock gets cleared out or you can click on continue button and add these stock and start selling at new price</div> ');

                $('#stockAlertModal').modal('toggle');
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/StockAdjust/index";
                }
                else {
                    window.location.href = "/StockAdjust/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    _i = i;
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var _lot = $('#ddlLot' + _id).val();

            //var hiddenOptionsCount = 0;
            //$("#ddlUnit" + _id + " option").each(function () {
            //    if ($(this)[0].outerHTML.indexOf('hidden') > -1) {
            //        hiddenOptionsCount++;
            //    }
            //});

            ItemDetails.push({
                StockAdjustmentDetailsId: $('#txtPurchaseDetailsId' + _id).val(),
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                Amount: $('#txtTotalAmount' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                //LotNo: $('#txtLotNo' + _id).val(),
                //ManufacturingDate: $('#txtManufacturingDate' + _id).val(),
                //ExpiryDate: $('#txtExpiryDate' + _id).val(),
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                //SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
            })
        }
    });

    var det = {
        StockAdjustmentId: window.location.href.split('=')[1],
        BranchId: $('#ddlBranch').val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        AdjustmentDate: moment($("#txtAdjustmentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtAdjustmentDate").val(),
        AdjustmentType: $("#ddlAdjustmentType  option:selected").val(),
        TotalQuantity: $('#divTotalQty').text(),
        TotalAmount: $("#hdndivTotalAmount").val(),
        TotalAmountRecovered: $("#txtTotalAmountRecovered").val(),
        Notes: $("#txtNotes").val(),
        StockAdjustmentDetails: ItemDetails,
        CheckStockPriceMismatch: _CheckStockPriceMismatch,
        StockDeductionType: $('#ddlStockDeductionType').val(),
        AccountId: $('#ddlAccount').val(),
        StockAdjustmentReasonId: $('#ddlStockAdjustmentReason').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockAdjust/StockAdjustmentUpdate',
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
            else if (data.Status == 3) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                $('#divStockAlertMsg').html('');
                $('#divStockAlertMsg').append('<label> Stock Items Selling Price mismatch. Check details below</label><br /><br />');

                var stockAlertCounter = 1;
                data.Errors.forEach(function (res) {
                    $('#divStockAlertMsg').append('<div>' + stockAlertCounter + '. ' + res.Message + '</div>');
                    stockAlertCounter = stockAlertCounter + 1;
                });

                $('#divStockAlertMsg').append('<br /><br /><br /><div style="font-size:0.9rem"> Your can click cancel button to wait till your previous stock gets cleared out or you can click on continue button and add these stock and start selling at new price</div> ');

                $('#stockAlertModal').modal('toggle');
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/StockAdjust/index";
                }
                else {
                    window.location.href = "/StockAdjust/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function confirmStockAlertInsert() {
    _CheckStockPriceMismatch = false;
    insert(_i);
}

function confirmStockAlertUpdate() {
    _CheckStockPriceMismatch = false;
    update(_i);
}

function Delete(StockAdjustmentId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            StockAdjustmentId: StockAdjustmentId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/StockAdjust/StockAdjustmentdelete',
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
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function checkAdjustmentType() {
    $('.divIsLot').hide();

    var adjType = $('#ddlAdjustmentType').val();

    if (adjType == 'Credit') {
        $('.divIsDebit').hide();
        $('.divStockDeductionType').hide();
    }
    else {
        $('.divIsDebit').show();
        $('.divStockDeductionType').show();

        $('#divCombo tr').each(function () {
            var i = this.id.split('divCombo')[1];
            var QuantityRemaining = parseFloat($('#txtStockQuantity' + i).val());
            var Quantity = parseFloat($('#txtQuantity' + i).val());

            if (Quantity > QuantityRemaining) {
                $('#txtQuantity' + i).val(QuantityRemaining);
            }
        });
    }
}

function toggleUnit(i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
    var UToSValue = parseFloat($('#hdnUToSValue' + i).val());
    var SToTValue = parseFloat($('#hdnSToTValue' + i).val());
    var TToQValue = parseFloat($('#hdnTToQValue' + i).val());

    var UnitCost = $('#hdnUnitCost' + i).val();
    var SalesExcTax = $('#hdnSalesExcTax' + i).val();
    var SalesIncTax = $('#hdnSalesIncTax' + i).val();
    var Mrp = $('#hdnMrp' + i).val();

    var newUnitCost = 0, newSalesExcTax = 0, newSalesIncTax = 0, newMrp = 0;

    var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

    var PrimaryUnitCost = 0, SecondaryUnitCost = 0, TertiaryUnitCost = 0, QuaternaryUnitCost = 0;
    var PrimarySalesExcTax = 0, SecondarySalesExcTax = 0, TertiarySalesExcTax = 0, QuaternarySalesExcTax = 0;
    var PrimarySalesIncTax = 0, SecondarySalesIncTax = 0, TertiarySalesIncTax = 0, QuaternarySalesIncTax = 0;
    var PrimaryMrp = 0, SecondaryMrp = 0, TertiaryMrp = 0, QuaternaryMrp = 0;

    if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
        PrimaryUnitCost = UnitCost;
        PrimarySalesExcTax = SalesExcTax;
        PrimarySalesIncTax = SalesIncTax;
        PrimaryMrp = Mrp;
    }
    else if (SToTValue == 0 && TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue;
            PrimaryMrp = Mrp * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesExcTax = SalesExcTax;
            SecondarySalesIncTax = SalesIncTax;
            SecondaryMrp = Mrp;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesExcTax = SalesExcTax;
            PrimarySalesIncTax = SalesIncTax;
            PrimaryMrp = Mrp;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesExcTax = SalesExcTax / UToSValue;
            SecondarySalesIncTax = SalesIncTax / UToSValue;
            SecondaryMrp = Mrp / UToSValue;
        }
    }
    else if (TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue * SToTValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue * SToTValue;
            PrimaryMrp = Mrp * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesExcTax = SalesExcTax * SToTValue;
            SecondarySalesIncTax = SalesIncTax * SToTValue;
            SecondaryMrp = Mrp * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesExcTax = SalesExcTax;
            TertiarySalesIncTax = SalesIncTax;
            TertiaryMrp = Mrp;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue;
            PrimaryMrp = Mrp * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesExcTax = SalesExcTax;
            SecondarySalesIncTax = SalesIncTax;
            SecondaryMrp = Mrp;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesExcTax = SalesExcTax / SToTValue;
            TertiarySalesIncTax = SalesIncTax / SToTValue;
            TertiaryMrp = Mrp / SToTValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesExcTax = SalesExcTax;
            PrimarySalesIncTax = SalesIncTax;
            PrimaryMrp = Mrp;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesExcTax = SalesExcTax / UToSValue;
            SecondarySalesIncTax = SalesIncTax / UToSValue;
            SecondaryMrp = Mrp / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesExcTax = SalesExcTax / UToSValue / SToTValue;
            TertiarySalesIncTax = SalesIncTax / UToSValue / SToTValue;
            TertiaryMrp = Mrp / UToSValue / SToTValue;
        }
    }
    else {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue * TToQValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue * SToTValue * TToQValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue * SToTValue * TToQValue;
            PrimaryMrp = Mrp * UToSValue * SToTValue * TToQValue;

            SecondaryUnitCost = UnitCost * SToTValue * TToQValue;
            SecondarySalesExcTax = SalesExcTax * SToTValue * TToQValue;
            SecondarySalesIncTax = SalesIncTax * SToTValue * TToQValue;
            SecondaryMrp = Mrp * SToTValue * TToQValue;

            TertiaryUnitCost = UnitCost * TToQValue;
            TertiarySalesExcTax = SalesExcTax * TToQValue;
            TertiarySalesIncTax = SalesIncTax * TToQValue;
            TertiaryMrp = Mrp * TToQValue;

            QuaternaryUnitCost = UnitCost;
            QuaternarySalesExcTax = SalesExcTax;
            QuaternarySalesIncTax = SalesIncTax;
            QuaternaryMrp = Mrp;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue * SToTValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue * SToTValue;
            PrimaryMrp = Mrp * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesExcTax = SalesExcTax * SToTValue;
            SecondarySalesIncTax = SalesIncTax * SToTValue;
            SecondaryMrp = Mrp * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesExcTax = SalesExcTax;
            TertiarySalesIncTax = SalesIncTax;
            TertiaryMrp = Mrp;

            QuaternaryUnitCost = UnitCost / TToQValue;
            QuaternarySalesExcTax = SalesExcTax / TToQValue;
            QuaternarySalesIncTax = SalesIncTax / TToQValue;
            QuaternaryMrp = Mrp / TToQValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesExcTax = SalesExcTax * UToSValue;
            PrimarySalesIncTax = SalesIncTax * UToSValue;
            PrimaryMrp = Mrp * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesExcTax = SalesExcTax;
            SecondarySalesIncTax = SalesIncTax;
            SecondaryMrp = Mrp;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesExcTax = SalesExcTax / SToTValue;
            TertiarySalesIncTax = SalesIncTax / SToTValue;
            TertiaryMrp = Mrp / SToTValue;

            QuaternaryUnitCost = UnitCost / SToTValue / TToQValue;
            QuaternarySalesExcTax = SalesExcTax / SToTValue / TToQValue;
            QuaternarySalesIncTax = SalesIncTax / SToTValue / TToQValue;
            QuaternaryMrp = Mrp / SToTValue / TToQValue;
        }
        else if (PriceAddedFor == 1) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesExcTax = SalesExcTax;
            PrimarySalesIncTax = SalesIncTax;
            PrimaryMrp = Mrp;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesExcTax = SalesExcTax / UToSValue;
            SecondarySalesIncTax = SalesIncTax / UToSValue;
            SecondaryMrp = Mrp / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesExcTax = SalesExcTax / UToSValue / SToTValue;
            TertiarySalesIncTax = SalesIncTax / UToSValue / SToTValue;
            TertiaryMrp = Mrp / UToSValue / SToTValue;

            QuaternaryUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
            QuaternarySalesExcTax = SalesExcTax / UToSValue / SToTValue / TToQValue;
            QuaternarySalesIncTax = SalesIncTax / UToSValue / SToTValue / TToQValue;
            QuaternaryMrp = Mrp / UToSValue / SToTValue / TToQValue;
        }
    }

    if (TToQValue != 0) {
        if (index == 1) {
            newUnitCost = PrimaryUnitCost;
            newSalesExcTax = PrimarySalesExcTax;
            newSalesIncTax = PrimarySalesIncTax;
            newMrp = PrimaryMrp;
        }
        else if (index == 2) {
            newUnitCost = SecondaryUnitCost;
            newSalesExcTax = SecondarySalesExcTax;
            newSalesIncTax = SecondarySalesIncTax;
            newMrp = SecondaryMrp;
        }
        else if (index == 3) {
            newUnitCost = TertiaryUnitCost;
            newSalesExcTax = TertiarySalesExcTax;
            newSalesIncTax = TertiarySalesIncTax;
            newMrp = TertiaryMrp;
        }
        else {
            newUnitCost = QuaternaryUnitCost;
            newSalesExcTax = QuaternarySalesExcTax;
            newSalesIncTax = QuaternarySalesIncTax;
            newMrp = QuaternaryMrp;
        }
    }
    else if (SToTValue != 0) {
        if (index == 2) {
            newUnitCost = PrimaryUnitCost;
            newSalesExcTax = PrimarySalesExcTax;
            newSalesIncTax = PrimarySalesIncTax;
            newMrp = PrimaryMrp;
        }
        else if (index == 3) {
            newUnitCost = SecondaryUnitCost;
            newSalesExcTax = SecondarySalesExcTax;
            newSalesIncTax = SecondarySalesIncTax;
            newMrp = SecondaryMrp;
        }
        else if (index == 4) {
            newUnitCost = TertiaryUnitCost;
            newSalesExcTax = TertiarySalesExcTax;
            newSalesIncTax = TertiarySalesIncTax;
            newMrp = TertiaryMrp;
        }
    }
    else if (UToSValue != 0) {
        if (index == 3) {
            newUnitCost = PrimaryUnitCost;
            newSalesExcTax = PrimarySalesExcTax;
            newSalesIncTax = PrimarySalesIncTax;
            newMrp = PrimaryMrp;
        }
        else if (index == 4) {
            newUnitCost = SecondaryUnitCost;
            newSalesExcTax = SecondarySalesExcTax;
            newSalesIncTax = SecondarySalesIncTax;
            newMrp = SecondaryMrp;
        }
    }
    else {
        newUnitCost = PrimaryUnitCost;
        newSalesExcTax = PrimarySalesExcTax;
        newSalesIncTax = PrimarySalesIncTax;
        newMrp = PrimaryMrp;
    }

    $('#txtUnitCost' + i).val(Math.round((parseFloat(newUnitCost) / ExchangeRate) * 100) / 100);
    $('#txtSalesExcTax' + i).val(Math.round((parseFloat(newSalesExcTax) / ExchangeRate) * 100) / 100);
    $('#txtSalesIncTax' + i).val(Math.round((parseFloat(newSalesIncTax) / ExchangeRate) * 100) / 100);
    $('#txtMrp' + i).val(parseFloat(newMrp).toFixed(2));

    updateOpeningStockSubTotal(i);
    convertAvailableStock();

    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var QuantityRemaining = parseFloat($('#txtStockQuantity' + i).val());
        var Quantity = parseFloat($('#txtQuantity' + i).val());

        if (Quantity > QuantityRemaining) {
            $('#txtQuantity' + i).val(QuantityRemaining);
        }
    });
    //updateQuantity(i,2);

    updateAmount();
}

function updateOpeningStockSubTotal(c) {
    $('#txtTotalAmount' + c).val(parseFloat($('#txtQuantity' + c).val()) * parseFloat($('#txtUnitCost' + c).val()));
}

function convertAvailableStock() {
    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var ind = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
        var ItemDetailsId = $('#txtItemDetailsId' + i).val();

        var UToSValue = parseFloat($('#hdnUToSValue' + i).val());
        var SToTValue = parseFloat($('#hdnSToTValue' + i).val());
        var TToQValue = parseFloat($('#hdnTToQValue' + i).val());

        var availableQuantity = parseFloat($('#hdnQuantityRemaining' + i).val());

        if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {

        }
        else if (SToTValue == 0 && TToQValue == 0) {
            if (ind == 3) {
                availableQuantity = availableQuantity / UToSValue;
            }
            else if (ind == 4) {

            }
        }
        else if (TToQValue == 0) {
            if (ind == 2) {
                availableQuantity = availableQuantity / UToSValue / SToTValue;
            }
            else if (ind == 3) {
                availableQuantity = availableQuantity / SToTValue;;
            }
            else if (ind == 4) {
                //TotalCurrentStock = TotalCurrentStock;
            }
        }
        else {
            if (ind == 1) {
                availableQuantity = availableQuantity / UToSValue / SToTValue / TToQValue;
            }
            else if (ind == 2) {
                availableQuantity = availableQuantity / SToTValue / TToQValue;
            }
            else if (ind == 3) {
                availableQuantity = availableQuantity / TToQValue;
            }
            else if (ind == 4) {
                //TotalCurrentStock= TotalCurrentStock;
            }
        }

        $('#txtStockQuantity' + i).val(availableQuantity);
    });
}

function toggleDecimal(evt, i) {
    var index = $("#ddlUnit" + i)[0].selectedIndex;
    var isDecimal = "false";
    if (index == 1) {
        isDecimal = $('#hdnAllowDecimal' + i).val();
    }
    else if (index == 2) {
        isDecimal = $('#hdnSecondaryUnitAllowDecimal' + i).val();
    }
    else if (index == 3) {
        isDecimal = $('#hdnTertiaryUnitAllowDecimal' + i).val();
    }
    else if (index == 4) {
        isDecimal = $('#hdnQuaternaryUnitAllowDecimal' + i).val();
    }
    if (isDecimal.toLowerCase() == "false") {
        var ASCIICode = (evt.which) ? evt.which : evt.keyCode
        if (ASCIICode == 46)
            return false;
        return true;
    }

    return true;
}

function checkSameUnit(i) {
    var ItemDetailsId = $('#txtItemDetailsId' + i).val();
    var UnitIndex = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    $('#divCombo tr').each(function () {
        var j = this.id.split('divCombo')[1];
        var innerItemDetailsId = $('#txtItemDetailsId' + j).val();
        if (innerItemDetailsId == ItemDetailsId && i != j) {
            var ind = $("#ddlUnit" + j).val().split('-')[1];//[0].selectedIndex + 1;

            if (ind == UnitIndex) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Same unit is already added. Please increase or decrease quantity from there');
                $('#ddlUnit' + i).val(parseFloat($('#txtQuantity' + i).val()) - 1);
                return false;
            }
        }
    });
    return true;
}

function checkStockAvailable(i) {
    var ItemDetailsId = $('#txtItemDetailsId' + i).val();
    var availableStock = $('#hdnQuantityRemaining' + i).val();
    var totalQty = 0;
    $('#divCombo tr').each(function () {
        var j = this.id.split('divCombo')[1];
        var innerItemDetailsId = $('#txtItemDetailsId' + j).val();
        if (innerItemDetailsId == ItemDetailsId) {
            var ind = $("#ddlUnit" + j).val().split('-')[1];//[0].selectedIndex + 1;

            var UToSValue = $('#hdnUToSValue' + j).val();
            var SToTValue = $('#hdnSToTValue' + j).val();
            var TToQValue = $('#hdnTToQValue' + j).val();

            var availableQuantity = $('#txtQuantity' + j).val();
            if (ind == 1) {
                availableQuantity = (availableQuantity * UToSValue * SToTValue * TToQValue);
            }
            else if (ind == 2) {
                availableQuantity = (availableQuantity * SToTValue * TToQValue);
            }
            else if (ind == 3) {
                availableQuantity = (availableQuantity * TToQValue);
            }
            totalQty = totalQty + availableQuantity;
            if (totalQty > availableStock) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                $('#txtQuantity' + i).val(0);
                return false;
            }
        }
    });
    return true;
}

function updateQuantity(_id, _type) {
    if ($('#ddlAdjustmentType').val() == 'Debit') {
        if (_id != undefined) {
            var StockQuantity = parseFloat($('#txtStockQuantity' + _id).val());
            var newQuantity = 0;
            if (_type == 2) {
                newQuantity = parseFloat($('#txtQuantity' + _id).val());
            }
            else {
                if (parseFloat($('#txtQuantity' + _id).val()) != 1 || _type == 1) {
                    if (_type == 1) {
                        newQuantity = parseFloat($('#txtQuantity' + _id).val()) + 1;
                    }
                    else {
                        newQuantity = parseFloat($('#txtQuantity' + _id).val()) - 1;
                    }
                }
            }
            if (newQuantity == 0) {
                $('#txtQuantity' + _id).val('1');
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                return
            }

            if (newQuantity <= StockQuantity) {
                if (_type == 1) {
                    $('#txtQuantity' + _id).val(parseFloat($('#txtQuantity' + _id).val()) + 1);
                }
                else if (_type == 0) {
                    $('#txtQuantity' + _id).val(parseFloat($('#txtQuantity' + _id).val()) - 1);
                }
                let chngqty = (parseFloat($('#txtQuantity' + _id).val()) * parseFloat($('#txtUnitCost' + _id).val()));
                $('#txtTotalAmount' + _id).val(chngqty.toFixed(2));
            }
            else {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                $('#txtQuantity' + _id).val(StockQuantity);
            }
            updateAmount();
        }
    }
    else {
        updateAmount();
    }
}

function fetchLotDetails(_id) {
    var _itemId = $('#txtItemId' + _id).val();
    var _ItemDetailsId = $('#txtItemDetailsId' + _id).val();
    var _lot = $('#ddlLot' + _id).val();
    if (_lot) {
        var det = {
            ItemId: _itemId,
            ItemDetailsId: _ItemDetailsId,
            Id: _lot.split('-')[0],
            Type: _lot.split('-')[1],
            BranchId: $('#ddlBranch').val()
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/SearchLot',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    $('#hdnPriceAddedFor' + _id).val(data.Data.ItemDetail.PriceAddedFor);
                    $('#hdnUnitCost' + _id).val(data.Data.ItemDetail.PurchaseExcTax);
                    $('#hdnPurchaseIncTax' + _id).val(data.Data.ItemDetail.SalesIncTax);
                    $('#hdnQuantityRemaining' + _id).val(data.Data.ItemDetail.Quantity);
                    $('#txtStockQuantity' + _id).val(data.Data.ItemDetail.Quantity);


                    toggleUnit(_id);
                    updateAmount();
                }
                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }

}

function clearTable() {
    $('#divCombo').empty();
}

function insertStockAdjustmentReason() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        StockAdjustmentReason: $('#txtStockAdjustmentReason_M').val(),
        Description: $('#txtDescription_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/StockAdjustmentReasonInsert',
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
                $('#ddlStockAdjustmentReason').append($('<option>', { value: data.Data.StockAdjustmentReason.StockAdjustmentReasonId, text: data.Data.StockAdjustmentReason.StockAdjustmentReason }));
                $('#ddlStockAdjustmentReason').val(data.Data.StockAdjustmentReason.StockAdjustmentReasonId);

                $('#stockAdjustmentReasonsModal').modal('toggle');

                $('#txtStockAdjustmentReason_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};