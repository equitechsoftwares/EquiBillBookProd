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

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_Date').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_Date').addClass('notranslate');

    if (window.location.href.indexOf('edit') > -1) {
        convertAvailableStock();
    }

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

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
var _StockTransferId = 0;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
        FromBranchId: $('#ddlFromBranch').val(),
        ToBranchId: $('#ddlToBranch').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        Status: $('#ddlStatus').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/stockTransfer/stockTransferFetch',
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

function View(StockTransferId) {
    var det = {
        StockTransferId: StockTransferId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockTransfer/StockTransferView',
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
            data: { Search: request.term, BranchId: $('#ddlFromBranch').val(), MenuType: 'stock transfer' },
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
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlFromBranch').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/items/SearchItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
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
                $('#divCombo tr').each(function () {
                    var _id = this.id.split('divCombo')[1];
                    var ItemDetailsId = $('#hdnItemDetailsId' + _id).val();
                    if (ItemDetailsId == data.Data.ItemDetails[i].ItemDetailsId) {
                        var StockQuantity = $('#txtStockQuantity' + _id).val();
                        var newQuantity = parseInt($('#txtQuantity' + _id).val()) + 1;
                        if (newQuantity <= StockQuantity) {
                            $('#txtQuantity' + _id).val(newQuantity);
                            $('#txtTotalAmount' + _id).val(parseFloat($('#txtQuantity' + _id).val()) * parseFloat($('#txtUnitCost' + _id).val()));
                            isPresent = true;
                        }
                        else {
                            if (EnableSound == 'True') { document.getElementById('error').play(); }
                            toastr.error('Not enough stock available');
                            isPresent = true;
                        }
                    }
                });
                if (isPresent == false) {
                    let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);
                    var variation = '';
                    if (data.Data.ItemDetails[i].VariationName) {
                        variation = '</br> <b>Variation :</b> ' + data.Data.ItemDetails[i].VariationName;
                    }
                    var ddlUnit = '<select class="form-control select2" style="width: 100px; " id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

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
                    var OnItemExpiry = $('#hdnOnItemExpiry').val();

                    //Set dropdown for lots
                    if (data.Data.ItemDetails[i].AvailableLots != null) {
                        var ddlLot = '<select class="form-control" style="width: 100px; " id="ddlLot' + count + '" onchange="fetchLotDetails(' + count + ')">';
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
                        '<td>' +
                        '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                        '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length > 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName.substring(0, 15) + '...</span>' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName + '</span>' +
                        variation +
                        ' </br> <b>SKU :</b> ' + data.Data.ItemDetails[i].SKU + '' +
                        '</td>' +
                        '<td class="' + (EnableLotNo == 'true' ? '' : 'hidden') + '"> ' + ddlLot +
                        '</td>' +
                        '<td style="min-width:150px"> ' +
                        '<div class="input-group" >' +
                        ddlUnit +
                        '</div >' +
                        '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUnitCost' + count + '" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                        '</td>' +
                        '<td style="min-width:150px" class="text-center">' +
                        '<div class="input-group">' +
                        '<input type="number" disabled class="form-control" placeholder="Quantity" value="' + data.Data.ItemDetails[i].Quantity + '" id="txtStockQuantity' + count + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuantityRemaining' + count + '" value="' + data.Data.ItemDetails[i].Quantity + '">' +
                        '</div>' +
                        '</td>' +
                        '<td style="min-width:150px"> ' +
                        '<div class="input-group" style="min-width:180px">' +
                        '<input type="number" class="form-control divQuantity' + count + '_ctrl" value="1" id="txtQuantity' + count + '" onchange="updateQuantity(' + count + ',2)">' +
                        '</div >' +
                        '<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small>' +
                        '</td>' +
                        '<td style="min-width:150px">' +
                        '<div class="input-group" style="min-width:180px">' +
                        '<input type="number" disabled class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '" onchange="updateQuantity(' + count + ',2)" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '">' +
                        '</div >' +
                        '<small class="text-red font-weight-bold errorText" id="divUnitCost' + count + '"></small>' +
                        '</td>' +
                        '<td style="min-width:150px">' +
                        '<input type="number" disabled class="form-control"  id="txtTotalAmount' + count + '" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '" >' +
                        '</td>' +
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
            if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
                convertAvailableStock();
            }

            //for (var g = 1; g <= (count - z); g++) {
            //    if (EnableLotNo == "true") {
            //        fetchLotDetails(g);
            //    }
            //}
            fetchLotDetails(count-1);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateQuantity(_id, _type) {
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
        //if (newQuantity == 0) {
        //    $('#txtQuantity' + _id).val('1');
        //    if (EnableSound == 'True') { document.getElementById('error').play(); }
        //    toastr.error('Not enough stock available');
        //    return
        //}

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
            updateQuantity(_id, _type);
        }
        updateAmount();
    }
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

    //let OtherCharges = $('#txtOtherCharges').val() == '' ? 0 : parseFloat($('#txtOtherCharges').val());
    //let PackagingCharge = $('#txtPackagingCharge').val() == '' ? 0 : parseFloat($('#txtPackagingCharge').val());
    //let ShippingCharge = $('#txtShippingCharge').val() == '' ? 0 : parseFloat($('#txtShippingCharge').val());

    $('#divTotalAmount').text(CurrencySymbol + Math.round(amount * 100) / 100);
    $('#hdndivTotalAmount').val(Math.round(amount * 100) / 100);

    $('#divTotalQty').text(qty);
    //$('#divGrandTotal').text(CurrencySymbol + Math.round((amount + parseFloat(OtherCharges) +
    //    parseFloat(PackagingCharge) + parseFloat(ShippingCharge)) * 100) / 100);

    $('#divGrandTotal').text(CurrencySymbol + Math.round((amount) * 100) / 100);
}

function deleteCombo(id, StockTransferDetailsId) {
    if (StockTransferDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
            var det = {
                StockTransferDetailsId: StockTransferDetailsId
            }
            $("#divLoading").show();
            $.ajax({
                url: '/StockTransfer/StockTransferDetailsDelete',
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
                        $('#divCombo' + id).hide();
                        $('#txtQuantity' + id).val(0);
                        //$('#txtTotalAmount' + id).val(0);
                        updateAmount();
                        //update(false);
                    }
                },
                error: function (xhr) {
                    $("#divLoading").hide();
                }
            });
        //}
    }
    else {
        $('#divCombo' + id).remove();
        $('#txtQuantity' + id).val(0);
        //$('#txtTotalAmount' + id).val(0);
        updateAmount();
    }
}

function deleteFullCombo(StockTransferId) {
    if (StockTransferId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
            var det = {
                StockTransferId: StockTransferId
            }
            $("#divLoading").show();
            $.ajax({
                url: '/StockTransfer/StockTransferDetailsDelete',
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
                        $('#divCombo tr').each(function () {
                            var _id = this.id.split('divCombo')[1];
                            if (_id != undefined) {
                                $('#divCombo' + _id).hide();
                                $('#txtQuantity' + _id).val(0);
                                //$('#txtTotalAmount' + _id).val(0);
                            }
                        });

                        updateAmount();
                        //update(false);
                    }
                },
                error: function (xhr) {
                    $("#divLoading").hide();
                }
            });
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
        //        //$('#txtTotalAmount' + _id).val(0);
        //    }
        //});
        updateAmount();
    }
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
            if (parseFloat(totalQty) > parseFloat(availableStock)) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Not enough stock available');
                $('#txtQuantity' + i).val(0);
                return false;
            }
        }
    });
    return true;
}

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

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
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
            })
        }
    });

    var det = {
        FromBranchId: $('#ddlFromBranch').val(),
        ToBranchId: $('#ddlToBranch').val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        Status: $("#ddlStatus  option:selected").val(),
        TotalQuantity: $('#divTotalQty').text(),
        Subtotal: $("#hdndivTotalAmount").val(),
        TotalAmount: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        ShippingCharge: $("#txtShippingCharge").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        Notes: $("#txtNotes").val(),
        IsActive: true,
        IsDeleted: false,
        StockTransferDetails: ItemDetails,
        StockTransferReasonId: $('#ddlStockTransferReason').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockTransfer/StockTransferInsert',
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
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/StockTransfer/index";
                }
                else {
                    window.location.href = "/StockTransfer/add";
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
                StockTransferDetailsId: $('#txtPurchaseDetailsId' + _id).val(),
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                Amount: $('#txtTotalAmount' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
            })
        }
    });

    var det = {
        StockTransferId: window.location.href.split('=')[1],
        FromBranchId: $('#ddlFromBranch').val(),
        ToBranchId: $('#ddlToBranch').val(),
        ReferenceNo: $("#txtReferenceNo").val(),
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtDate").val(),
        Status: $("#ddlStatus  option:selected").val(),
        TotalQuantity: $('#divTotalQty').text(),
        Subtotal: $("#hdndivTotalAmount").val(),
        TotalAmount: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        ShippingCharge: $("#txtShippingCharge").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        Notes: $("#txtNotes").val(),
        StockTransferDetails: ItemDetails,
        StockTransferReasonId: $('#ddlStockTransferReason').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/StockTransfer/StockTransferUpdate',
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
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/StockTransfer/index";
                }
                else {
                    window.location.href = "/StockTransfer/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(StockTransferId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            StockTransferId: StockTransferId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/StockTransfer/StockTransferdelete',
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

function toggleUnit(i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
    var UToSValue = parseFloat($('#hdnUToSValue' + i).val());// == 0 ? 1 : $('#hdnUToSValue' + i).val());
    var SToTValue = parseFloat($('#hdnSToTValue' + i).val());// == 0 ? 1 : $('#hdnSToTValue' + i).val());
    var TToQValue = parseFloat($('#hdnTToQValue' + i).val());// == 0 ? 1 : $('#hdnTToQValue' + i).val());

    var UnitCost = $('#hdnUnitCost' + i).val();
    var SalesCost = $('#hdnSalesIncTax' + i).val();
    var newUnitCost = 0, newSalesCost = 0;

    var PrimaryUnitCost = 0, SecondaryUnitCost = 0, TertiaryUnitCost = 0, QuaternaryUnitCost = 0;
    var PrimarySalesCost = 0, SecondarySalesCost = 0, TertiarySalesCost = 0, QuaternarySalesCost = 0;

    if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
        PrimaryUnitCost = UnitCost;
        PrimarySalesCost = SalesCost;
    }
    else if (SToTValue == 0 && TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;
        }
    }
    else if (TToQValue == 0) {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesCost = SalesCost * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesCost = SalesCost / SToTValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesCost = SalesCost / UToSValue / SToTValue;
        }
    }
    else {
        if (PriceAddedFor == 4) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue * TToQValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue * TToQValue;

            SecondaryUnitCost = UnitCost * SToTValue * TToQValue;
            SecondarySalesCost = SalesCost * SToTValue * TToQValue;

            TertiaryUnitCost = UnitCost * TToQValue;
            TertiarySalesCost = SalesCost * TToQValue;

            QuaternaryUnitCost = UnitCost;
            QuaternarySalesCost = SalesCost;
        }
        else if (PriceAddedFor == 3) {
            PrimaryUnitCost = UnitCost * UToSValue * SToTValue;
            PrimarySalesCost = SalesCost * UToSValue * SToTValue;

            SecondaryUnitCost = UnitCost * SToTValue;
            SecondarySalesCost = SalesCost * SToTValue;

            TertiaryUnitCost = UnitCost;
            TertiarySalesCost = SalesCost;

            QuaternaryUnitCost = UnitCost / TToQValue;
            QuaternarySalesCost = SalesCost / TToQValue;
        }
        else if (PriceAddedFor == 2) {
            PrimaryUnitCost = UnitCost * UToSValue;
            PrimarySalesCost = SalesCost * UToSValue;

            SecondaryUnitCost = UnitCost;
            SecondarySalesCost = SalesCost;

            TertiaryUnitCost = UnitCost / SToTValue;
            TertiarySalesCost = SalesCost / SToTValue;

            QuaternaryUnitCost = UnitCost / SToTValue / TToQValue;
            QuaternarySalesCost = SalesCost / SToTValue / TToQValue;
        }
        else if (PriceAddedFor == 1) {
            PrimaryUnitCost = UnitCost;
            PrimarySalesCost = SalesCost;

            SecondaryUnitCost = UnitCost / UToSValue;
            SecondarySalesCost = SalesCost / UToSValue;

            TertiaryUnitCost = UnitCost / UToSValue / SToTValue;
            TertiarySalesCost = SalesCost / UToSValue / SToTValue;

            QuaternaryUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
            QuaternarySalesCost = SalesCost / UToSValue / SToTValue / TToQValue;
        }
    }

    if (TToQValue != 0) {
        if (index == 1) {
            newUnitCost = PrimaryUnitCost;
            newSalesCost = PrimarySalesCost;
        }
        else if (index == 2) {
            newUnitCost = SecondaryUnitCost;
            newSalesCost = SecondarySalesCost;
        }
        else if (index == 3) {
            newUnitCost = TertiaryUnitCost;
            newSalesCost = TertiarySalesCost;
        }
        else {
            newUnitCost = QuaternaryUnitCost;
            newSalesCost = QuaternarySalesCost;
        }
    }
    else if (SToTValue != 0) {
        if (index == 2) {
            newUnitCost = PrimaryUnitCost;
            newSalesCost = PrimarySalesCost;
        }
        else if (index == 3) {
            newUnitCost = SecondaryUnitCost;
            newSalesCost = SecondarySalesCost;
        }
        else if (index == 4) {
            newUnitCost = TertiaryUnitCost;
            newSalesCost = TertiarySalesCost;
        }
    }
    else if (UToSValue != 0) {
        if (index == 3) {
            newUnitCost = PrimaryUnitCost;
            newSalesCost = PrimarySalesCost;
        }
        else if (index == 4) {
            newUnitCost = SecondaryUnitCost;
            newSalesCost = SecondarySalesCost;
        }
    }
    else {
        newUnitCost = PrimaryUnitCost;
        newSalesCost = PrimarySalesCost;
    }

    $('#txtUnitCost' + i).val(parseFloat(newUnitCost).toFixed(2));
    $('#txtSalesIncTax' + i).val(parseFloat(newSalesCost).toFixed(2));
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

                    toggleUnit(_id);
                }
                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}

function copyCode(url) {
    /* Get the text field */
    var copyText = url;

    navigator.clipboard
        .writeText(copyText)
        .then(() => {
            toastr.success("Copied");
        })
        .catch(() => {
            toastr.success("Something went wrong");
        });
}

function View(StockTransferId) {
    var det = {
        StockTransferId: StockTransferId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/stocktransfer/StockTransferView',
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

function openStockTransferStatusModal(id, _status) {
    _StockTransferId = id;
    Status = _status;
    $('#stockTransferStatusModal').modal('toggle');
}

function UpdateStockTransferStatus() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        StockTransferId: _StockTransferId,
        Status: $("#ddlStatus_M").val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/stocktransfer/UpdateStockTransferStatus',
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
                    $('#' + res.Id + '_M').show();
                    $('#' + res.Id + '_M').text(res.Message);
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#stockTransferStatusModal").modal('hide');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearTable() {
    $('#divCombo').empty();
}

function insertStockTransferReason() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        StockTransferReason: $('#txtStockTransferReason_M').val(),
        Description: $('#txtDescription_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/StockTransferReasonInsert',
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
                $('#ddlStockTransferReason').append($('<option>', { value: data.Data.StockTransferReason.StockTransferReasonId, text: data.Data.StockTransferReason.StockTransferReason }));
                $('#ddlStockTransferReason').val(data.Data.StockTransferReason.StockTransferReasonId);

                $('#stockTransferReasonsModal').modal('toggle');

                $('#txtStockTransferReason_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};