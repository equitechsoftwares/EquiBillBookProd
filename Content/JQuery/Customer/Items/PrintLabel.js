
$(function () {
    $('.select2').select2();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var count = 1;

$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: {
                Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'print labels'
            },
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                    $('#txttags').val('');
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
                    //$("html, body").animate({ scrollTop: 0 }, "slow");
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
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {

                var isPresent = false;
                $('#divCombo tr').each(function () {
                    var _id = this.id.split('divCombo')[1];
                    var ItemDetailsId = $('#txtItemDetailsId' + _id).val();
                    if (ItemDetailsId == data.Data.ItemDetails[i].ItemDetailsId) {
                        $('#txtQuantity' + _id).val(parseInt($('#txtQuantity' + _id).val()) + 1);
                        isPresent = true;
                    }
                });
                if (isPresent == false) {
                    let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);

                    var variation = ''; var variationVal = '';
                    if (data.Data.ItemDetails[i].VariationName) {
                        variation = '</br> Variation : ' + data.Data.ItemDetails[i].VariationName;
                        variationVal = data.Data.ItemDetails[i].VariationName;
                    }

                    var ddlUnit = '<select class="form-control select2 ' + (data.Data.ItemDetails[i].UnitId != 0 ? '' : 'hidden') + '" style="width: 100px;" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

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

                    html = html + '<tr id="divCombo' + count + '">' +
                        '<td>' + count + '</td>' +
                        '<td style="min-width:150px">' +
                        '<input type="text" hidden class="form-control" id="txtItemName' + count + '" value="' + data.Data.ItemDetails[i].ItemName + '">' +
                        '<input type="text" hidden class="form-control" id="txtSKU' + count + '" value="' + data.Data.ItemDetails[i].SKU + '">' +
                        '<input type="text" hidden class="form-control" id="txtVariation' + count + '" value="' + variationVal + '">' +
                        '<input type="text" hidden class="form-control" id="txtSalesExcTax' + count + '" value="' + data.Data.ItemDetails[i].SalesExcTax + '">' +
                        '<input type="text" hidden class="form-control" id="txtSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        'Item Name : ' + data.Data.ItemDetails[i].ItemName +
                        variation +
                        '</br> SKU : ' + data.Data.ItemDetails[i].SKU + '' +
                        '</td>' +
                        '<td style="min-width:100px"> ' +
                        '<div class="input-group" style="min-width:180px">' +
                        '<span class="' + (data.Data.ItemDetails[i].UnitId == 0 ? '' : 'hidden') + '" style="margin-left:auto;margin-right:auto;">-</span>' +
                        ddlUnit +
                        '</div >' +
                        '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                        '<input type="hidden" id="hdnUnitCost' + count + '" value="' + data.Data.ItemDetails[i].SalesExcTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnIsManageStock' + count + '" value="' + data.Data.ItemDetails[i].IsManageStock + '">' +
                        /*'<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small>' +*/
                        '</td>' +
                        '<td style="min-width:150px">' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="1" id="txtQuantity' + count + '" min="0"> ' +
                        '</td>' +
                        '<td style="min-width:150px">' +
                        '<div class="input-group date _PackingDate" id="_PackingDate' + count + '" data-target-input="nearest">' +
                        '<input id="txtPackingDate' + count + '" type="text" class="form-control datetimepicker-input" data-target="#_PackingDate' + count + '" />' +
                        '<div class="input-group-append" data-target="#_PackingDate' + count + '" data-toggle="datetimepicker">' +
                        '<div class="input-group-text"><i class="fa fa-calendar"></i></div>' +
                        '</div>' +
                        '</div>' +
                        /*'<input type="date" class="form-control" id="txtPackingDate' + count + '">' +*/
                        '</td>' +
                        '<td style="min-width:150px">' +
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

            var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
            $('._PackingDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
            $('._PackingDate').addClass('notranslate');

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deleteCombo(id, PurchaseDetailsId) {
    if (PurchaseDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
            var det = {
                PurchaseDetailsId: PurchaseDetailsId
            }
            $("#divLoading").show();
            $.ajax({
                url: '/purchase/purchaseDetailsDelete',
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
                        $('#divCombo' + id).remove();
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
        updateAmount();
    }
}

//function togglePrice() {
//    if ($('#chkProductPrice').is(':checked')) {
//        $('.divPriceType').show();
//    }
//    else {
//        $('.divPriceType').hide();
//    }
//}

function Preview() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    var ItemDetails = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {

            //var hiddenOptionsCount = 0;
            //$("#ddlUnit" + _id + " option").each(function () {
            //    if ($(this)[0].outerHTML.indexOf('hidden') > -1) {
            //        hiddenOptionsCount++;
            //    }
            //});

            ItemDetails.push({
                ItemName: $('#txtItemName' + _id).val(),
                SKU: $('#txtSKU' + _id).val(),
                Variation: $('#txtVariation' + _id).val(),
                SalesExcTax: $('#txtSalesExcTax' + _id).val(),
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                Quantity: $('#txtQuantity' + _id).val(),
                PackingDate: $('#txtPackingDate' + _id).val(),
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
            })
        }
    });
    //if ($('#ddlBranch').val() == 0 || ItemDetails.length == 0) {
    //    if (EnableSound == 'True') { document.getElementById('error').play(); }
    //    //$("html, body").animate({ scrollTop: 0 }, "slow");

    //    if ($('#ddlBranch').val() == 0) {
    //        $('#divBranch').show();
    //        $('#divBranch').text('Branch is required');
    //    }
    //    if (ItemDetails.length == 0) {
    //        $('#divtags').show();
    //        $('#divtags').text('Search item first');
    //    }
    //    return
    //}

    var Item = {
        ShowProductName: $('#chkProductName').is(':checked'),
        ProductSize: $('#txtProductSize').val(),
        ShowProductVariation: $('#chkProductVariation').is(':checked'),
        ProductVariationSize: $('#txtProductVariationSize').val(),
        ShowProductPrice: $('#chkProductPrice').is(':checked'),
        ProductPriceSize: $('#txtProductPriceSize').val(),
        PriceType: $('#ddlPriceType').val(),
        ShowBranchName: $('#chkBranchName').is(':checked'),
        BranchNameSize: $('#txtBranchNameSize').val(),
        ShowPrintPackingDate: $('#chkPrintPackingDate').is(':checked'),
        PrintPackingDateSize: $('#txtPrintPackingDateSize').val(),
        BranchName: $('#ddlBranch').find(":selected").text(),
        BarcodeSettings: $('#ddlBarcodeSettings').val(),
        ItemDetails: ItemDetails
    }

    var det = {
        printlabelJson: JSON.stringify(Item),
        BranchId: $('#ddlBranch').val(),
        ItemDetails: ItemDetails
    }
   
    $("#divLoading").show();
    $.ajax({
        url: '/items/PrintLabelInsert',
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
                //sessionStorage.setItem('Item', JSON.stringify(Item));
                //var myWindow = window.open("/items/printlabelpreview");

                //if (sessionStorage.getItem("InvoiceUrl")) {
                //    $.get(sessionStorage.getItem("InvoiceUrl"), function (data) {
                //        data = data.replace('noPrint', 'noPrint" style="display:none"')

                        //const WinPrint = window.open(
                        //    "/items/printlabelpreview?PrintLabelId="+data.Data.PrintLabel.PrintLabelId,
                        //    "_blank",
                        //    "left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0"
                        //);
                        //WinPrint.document.write(data);
                        //WinPrint.document.close();
                        //WinPrint.focus();
                        //setTimeout(
                        //    function () {
                        //        WinPrint.print();
                        //        WinPrint.close();
                        //    }, 1000);

                //    });

                //    sessionStorage.removeItem("InvoiceUrl");
                //}

                setTimeout(() => {
                    var myWindow = window.open("/items/printlabelpreview?PrintLabelId=" + data.Data.PrintLabel.PrintLabelId, "_blank", "left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0");
                })

            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleUnit(i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
    var UToSValue = parseFloat($('#hdnUToSValue' + i).val());// == 0 ? 1 : $('#hdnUToSValue' + i).val());
    var SToTValue = parseFloat($('#hdnSToTValue' + i).val());// == 0 ? 1 : $('#hdnSToTValue' + i).val());
    var TToQValue = parseFloat($('#hdnTToQValue' + i).val());// == 0 ? 1 : $('#hdnTToQValue' + i).val());

    var UnitCost = $('#hdnUnitCost' + i).val();
    var SalesCost = $('#hdnSalesIncTax' + i).val();
    var newUnitCost = 0, newSalesCost = 0;
    var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

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

    $('#txtUnitCost' + i).val(Math.round((parseFloat(newUnitCost) / ExchangeRate) * 100) / 100);
    $('#txtSalesIncTax' + i).val(Math.round((parseFloat(newSalesCost) / ExchangeRate) * 100) / 100);
    //ChangeQtyAmount(i);
    //convertAvailableStock();

    //$('#divCombo tr').each(function () {
    //    var i = this.id.split('divCombo')[1];
    //    var IsManageStock = $('#hdnIsManageStock' + i).val();
    //    if (IsManageStock.toLowerCase() == "true") {
    //        var QuantityRemaining = parseFloat($('#txtStockQuantity' + i).val());
    //        var Quantity = parseFloat($('#txtQuantity' + i).val());

    //        if (Quantity > QuantityRemaining) {
    //            $('#txtQuantity' + i).val(QuantityRemaining);
    //        }
    //    }
    //});
}