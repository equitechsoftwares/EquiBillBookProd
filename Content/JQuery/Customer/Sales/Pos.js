
$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $('.select2').select2();

    // Add search event handlers for customer dropdown - using setTimeout to ensure select2 is fully initialized
    setTimeout(function () {
        //$('#ddlCustomer').on('select2:open', function (e) {
        //    window.lastDropdown = 'customer';
        //});

        $(document).on('input', '.select2-search__field', function () {
            var searchTerm = $(this).val();
            window.lastCustomerSearchTerm = searchTerm;
        });
    }, 100);

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('#_SalesDate').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase() + ' ' + TimeFormat,
        defaultDate: new Date(),
        icons: DateTimePickerIcons
    });
    $('#_DueDate').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase() + ' ' + TimeFormat,
        defaultDate: new Date(),
        icons: DateTimePickerIcons
    });
    $('#_SalesDate').addClass('notranslate');

    $('#_DOB').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase(),
        icons: DateTimePickerIcons
    });
    $('#_JoiningDate').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase(),
        defaultDate: new Date(),
        icons: DateTimePickerIcons
    });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');

    if (window.location.href.indexOf('edit') > -1) {
        convertAvailableStock();
    }
    else {
        fetchAdditionalCharges();
    }

    fetchTax();

    fetchTaxExemptions();

    fetchWarranty();

    fetchCompanyCurrency();

    // Initialize reward points section on page load if enabled
    // The view already controls visibility - we just fetch data if sections exist
    if (typeof fetchCustomerRewardPoints === 'function') {
        fetchCustomerRewardPoints();
    }

});

// Centralized datetimepicker icon configuration
var DateTimePickerIcons = {
    time: 'fa fa-clock',
    date: 'fa fa-calendar',
    up: 'fa fa-arrow-up',
    down: 'fa fa-arrow-down',
    previous: 'fa fa-chevron-left',
    next: 'fa fa-chevron-right',
    today: 'fa fa-calendar-check-o',
    clear: 'fa fa-trash',
    close: 'fa fa-times'
};

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

    if (sessionStorage.getItem("InvoiceUrl")) {
        if (!!window.GestureEvent == true) {
            $('#invoiceModal').modal('show');
            $('#lblInvoice').attr('src', sessionStorage.getItem("InvoiceUrl"));
            sessionStorage.removeItem("InvoiceUrl");
        }
        else {
            const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
            if (isMobile) {
                var WinPrint = window.open(sessionStorage.getItem("InvoiceUrl"), "_blank", "");
                sessionStorage.removeItem("InvoiceUrl");
            }
            else {
                $.get(sessionStorage.getItem("InvoiceUrl"), function (data) {
                    //data = data.replace('noPrint', 'noPrint" style="display:none"')
                    const WinPrint = window.open(
                        sessionStorage.getItem("InvoiceUrl"),
                        "_blank",
                        "left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0"
                    );
                    WinPrint.document.write(data);
                    WinPrint.document.close();
                    WinPrint.focus();
                    setTimeout(
                        function () {
                            WinPrint.print();
                            WinPrint.close();
                            sessionStorage.removeItem("InvoiceUrl");
                        }, 1000);
                });
            }
        }
    }
}

var _PageIndex = 1;
var count = 1; var innerCount = 1; var dropdownHtml = ''; var multiplePaymentsCount = 2;
var additionalChargesId = 0;
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";

var AttachDocument = "";
var FileExtensionAttachDocument = "";
var ShippingDocument = "";
var FileExtensionShippingDocument = "";
var _SalesId = 0, _PaymentTypeId = 0, PlaceOfSupplyId = 0, PaymentTermId = 0;
var CategoryId = 0; SubCategoryId = 0; var SubSubCategoryId = 0, BrandId = 0;
var IsBillOfSupply = false;
var skuCodes = [];

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

//function PrintReceipt(id) {
//    var myWindow = window.open("/sales/receipt?InvoiceId=" + id, "", "width=500,height=500");
//}


$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: { Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'sale' },
            success: function (data) {
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
                    if (data.Data.ItemsArray.length == 1) {
                        $('#txttags').val('');
                        var splitVal = data.Data.ItemsArray[0].split('~');
                        fetchItem(splitVal[splitVal.length - 1]);
                        /*skuCodes.push(splitVal[splitVal.length - 1]);*/
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
        //skuCodes.push(splitVal[splitVal.length - 1]);
    }
});

function fetchItem(SkuHsnCode) {
    skuCodes.push(SkuHsnCode);
    var IsBusinessRegistered = $('#hdnIsBusinessRegistered').val().toLocaleLowerCase();
    var BusinessRegistrationType = $('#hdnBusinessRegistrationType').val().toLocaleLowerCase();

    if ((window.location.pathname.toLowerCase().indexOf('billofsupply') != -1) ||
        (IsBusinessRegistered == "1" && BusinessRegistrationType == "composition")) {
        IsBillOfSupply = true;
    }
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        MenuType: 'sale',
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        Type: "Sales",
        IsBillOfSupply: IsBillOfSupply
    };

    //$("#divLoading").show();
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
            var z = count;
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {
                var isPresent = false;

                if (isPresent == false) {
                    let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);
                    var variation = '';
                    if (data.Data.ItemDetails[i].VariationName) {
                        variation = '</br> <b>Variation :</b> ' + data.Data.ItemDetails[i].VariationName;
                    }
                    var ddlUnit = '<select class="form-control select2 ' + (data.Data.ItemDetails[i].UnitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

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

                    var ddlTax = '<select class="form-control ddlTax" style="min-width:80px" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
                    for (let ss = 0; ss < taxList.length; ss++) {
                        if (taxList[ss].Tax != "Taxable") {
                            if (data.Data.BusinessSetting.CountryId == 2) {
                                if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val() &&
                                    data.Data.BusinessSetting.GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && data.Data.BusinessSetting.GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                                    && data.Data.BusinessSetting.GstTreatment != 'Supply by SEZ Developer') {
                                    if (taxList[ss].TaxTypeId != 3) {
                                        if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                                else {
                                    if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                        if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                            }
                            else {
                                if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                    ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                }
                                else {
                                    ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                }
                            }
                        }
                    }
                    ddlTax = ddlTax + '</select>';

                    var ddlWarranty = '<select class="form-control select2" id="ddlWarranty' + count + '" onchange="ChangePurAmount(' + count + ')"><option selected value="0">None</option>';
                    for (let ss = 0; ss < warrantyList.length; ss++) {
                        if (data.Data.ItemDetails[i].WarrantyId == warrantyList[ss].WarrantyId) {
                            ddlWarranty = ddlWarranty + '<option selected value="' + warrantyList[ss].WarrantyId + '">' + warrantyList[ss].Warranty + '</option>';
                        }
                        else {
                            ddlWarranty = ddlWarranty + '<option value="' + warrantyList[ss].WarrantyId + '">' + warrantyList[ss].Warranty + '</option>';
                        }
                    }
                    ddlWarranty = ddlWarranty + '</select >';

                    var ddlTaxExemption = '<select class="form-control select2" id="ddlTaxExemption' + count + '"><option selected value="0">None</option>';
                    for (let ss = 0; ss < taxExemptions.length; ss++) {
                        if (data.Data.ItemDetails[i].TaxExemptionId == taxExemptions[ss].TaxExemptionId) {
                            ddlTaxExemption = ddlTaxExemption + '<option selected value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                        }
                        else {
                            ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                        }
                    }
                    ddlTaxExemption = ddlTaxExemption + '</select >';

                    var SalePriceIsMinSellingPrice = $('#hdnSalePriceIsMinSellingPrice').val().toLocaleLowerCase();
                    var EnableLotNo = $('#hdnEnableLotNo').val().toLocaleLowerCase();
                    var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());
                    var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
                    var EnableFreeQuantity = $('#hdnEnableFreeQuantity').val().toLocaleLowerCase();
                    var OnItemExpiry = $('#hdnOnItemExpiry').val();
                    var EnableWarranty = $('#hdnEnableWarranty').val().toLocaleLowerCase();

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
                    else {
                        var ddlLot = '<span style="text-align:center;display:block">-</span>';
                    }

                    html = html + '<tr id="divCombo' + count + '">' +
                        '<td>' +
                        '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                        '<input type="text" hidden class="form-control" id="txtSkuCode' + count + '" value="' + data.Data.ItemDetails[i].SKU + '">' +
                        '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                        data.Data.ItemDetails[i].ItemName + '-' + data.Data.ItemDetails[i].SKU +
                        variation +
                        '</div>' + '<a href="javascript:void(0)" data-toggle="modal" data-target="#itemDetails' + count + '"> <i class="fa fa-edit"></i></a>' +
                        '</td>' +
                        '<td> ' +
                        '<div class="input-group">' +
                        '<div class="input-group-prepend">' +
                        '<button class="btn btn-danger btn-sm" type="button" onclick="decreaseQuantity(' + count + ')">' +
                        '<i class="fas fa-minus"></i>' +
                        '</button>' +
                        '</div>' +
                        '<input onKeyPress="toggleDecimal(event,' + count + ')" type="number" class="form-control divQuantity' + count + '_ctrl" value="1" id="txtQuantity' + count + '" onchange="updateQuantity(' + count + ')" min="0"> ' +
                        '<div class="input-group-append">' +
                        '<button class="btn btn-success btn-sm" type="button" onclick="increaseQuantity(' + count + ')">' +
                        '<i class="fas fa-plus"></i>' +
                        '</button>' +
                        '</div>' +
                        ddlUnit +
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
                        '</div >' +
                        '<small class="text-success font-weight-bold ' + (data.Data.ItemDetails[i].IsManageStock == true ? '' : 'hidden') + '" id="txtAvailableQty' + count + '">Available Qty: ' + data.Data.ItemDetails[i].Quantity + '</small>' +
                        '<input type="hidden" id="hdnQuantityRemaining' + count + '" value="' + data.Data.ItemDetails[i].Quantity + '">' +
                        '<input type="hidden" id="txtStockQuantity' + count + '" value="' + data.Data.ItemDetails[i].Quantity + '">' +
                        '</td>' +
                        '<td>' +
                        '<input type="number" disabled class="form-control"  id="txtAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax / ExchangeRate + '" >' +
                        '<input type="hidden" id="hdnAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '" >' +
                        '</td>' +
                        '<td>' +
                        '<a href="javascript:void(0)" class="btn btn-danger" onclick="deleteCombo(' + count + ')" id="Delete" data-toggle="tooltip" data-placement="bottom" title="Delete"><i class="far fa-trash-alt"></i></a>' +
                        '<div class="modal fade" id="itemDetails' + count + '">' +
                        '<div class="modal-dialog modal-lg" >' +
                        '<div class="modal-content">' +
                        '<div class="modal-header">' +
                        '<h4 class="modal-title">' + data.Data.ItemDetails[i].ItemName + '</h4>' +
                        '<button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
                        '<span aria-hidden="true">&times;</span>' +
                        '</button>' +
                        '</div>' +
                        '<div class="modal-body">' +
                        '<div class="row">' +
                        '<div class="col-md-3 ' + (EnableLotNo == 'true' ? '' : 'hidden') + '">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Lot No</label>' +
                        '<div class="input-group" style="min-width:160px">' +
                        ddlLot +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 ' + (EnableFreeQuantity == 'true' ? '' : 'hidden') + '">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Free Quantity</label>' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="0" id="txtFreeQuantity' + count + '" onchange="updateQuantity(' + count + ')" min="0"> ' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Unit Cost</label>' +
                        '<input type="number"' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '"  value="' + data.Data.ItemDetails[i].SalesExcTax / ExchangeRate + '" onchange="ChangeQtyAmount(' + count + ')">' +
                        '<small class="text-red font-weight-bold errorText" id="divUnitCost' + count + '"></small>' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Discount</label>' +
                        '<div class="input-group" style="min-width:150px">' +
                        '<input ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeQtyAmount(' + count + ')" value="' + data.Data.ItemDetails[i].Discount / ExchangeRate + '">' +
                        '<select style="min-width:100px" ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" id="ddlDiscountType' + count + '" onchange="ChangeQtyAmount(' + count + ')">' +
                        '<option value="Fixed">Fixed</option>' +
                        '<option value="Percentage">Percentage</option>' +
                        '</select>' +
                        '</div >' +
                        '<input type="text" hidden class="form-control" id="hdnExtraDiscounts' + count + '" value="0">' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 hidden">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Prie Exc Tax</label>' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" ' + (SalePriceIsMinSellingPrice == 'false' ? '' : 'disabled') + ' class="form-control" value="' + data.Data.ItemDetails[i].SalesExcTax + '" id="txtPurchaseExcTax' + count + '"  onchange="ChangePriceBefTax(' + count + ')" min="0"/>' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 hidden">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Amount Exc Tax</label>' +
                        '<input type="number" class="form-control" disabled value="' + data.Data.ItemDetails[i].SalesExcTax + '" id="txtAmountExcTax' + count + '" min="0"/>' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 divBillOfSupply" id="tdTax' + count + '">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Tax</label>' +
                        '<div class="input-group">' +
                        ddlTax +
                        '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
                        '<input hidden type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
                        '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                        '<div id="divTaxExemption' + count + '" class="form-group" style="width:100%;display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                        '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                        '<div class="input-group">' +
                        ddlTaxExemption +
                        '</div>' +
                        '</div>' +
                        '</div >' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 hidden">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Purchase Inc Tax</label>' +
                        '<input type="number" class="form-control" id="txtPurchaseIncTax' + count + '"  value="' + data.Data.ItemDetails[i].SalesIncTax / ExchangeRate + '" onchange="updateNetCost(' + count + ')">' +
                        '<input type="hidden" value="' + data.Data.ItemDetails[i].SalesIncTax + '" id="hdnPurchaseIncTax' + count + '" />' +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3 ' + (EnableWarranty == 'true' ? '' : 'hidden') + '">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Warranty</label>' +
                        '<div class="input-group">' +
                        ddlWarranty +
                        '</div>' +
                        '</div>' +
                        '<div class="col-md-3" style="min-width:200px;display:' + (data.Data.ItemDetails[i].EnableImei == true ? 'block' : 'none') + '">' +
                        '<div class="form-group">' +
                        '<label for="payment_note_1">Description</label>' +
                        '<textarea type="text" class="form-control" id="txtOtherInfo' + count + '" placeholder="IMEI, Serial number or other informations"></textarea>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="modal-footer justify-content-between">' +
                        '<button type="button" class="btn btn-default" data-dismiss="modal" style="margin-left:auto">Close</button>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '</td>' +
                        '</tr>';
                }

                count++;
            }

            $('#divCombo').append(html);
            $('.select2').select2();

            updateAmount();

            if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
                convertAvailableStock();
            }

            for (var g = 1; g <= (count - z); g++) {
                if (EnableLotNo == "true") {
                    fetchLotDetails(z);
                }
                if (data.Data.UserGroup.SellingPriceGroupId != 0 || data.Data.UserGroup.CalculationPercentage != 0) {
                    updateNetCost(z);
                }
            }

            if (IsBillOfSupply == true) {
                $('.divBillOfSupply').hide();
            }

            updateExtraDiscounts();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchTaxExemptions() {
    var det = {
        TaxExemptionType: "item"
    };
    $.ajax({
        url: '/taxsettings/ActiveTaxExemptions',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                taxExemptions = data.Data.TaxExemptions;
            }
        },
        error: function (xhr) { }
    });
}

function updateAmount() {
    var amount = 0;
    var qty = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        amount = amount + (parseFloat($('#txtUnitCost' + _id).val()) * parseFloat($('#txtQuantity' + _id).val()));
        qty = qty + parseFloat($('#txtQuantity' + _id).val()) + parseFloat($('#txtFreeQuantity' + _id).val());
    });

    $('#divTotalAmount').text(CurrencySymbol + Math.round(amount * 100) / 100);
    $('#hdndivTotalAmount').val(amount);
    $('#divTotalQty').text(qty);
    /*chargecalc();*/
    discallcalc();
}

function updateNetCost(id) {
    if (id != undefined) {
        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];

        let PurchaseExcTax = (100 * (PurchaseIncTax / (100 + parseFloat(taxper))));

        let taxamt = parseFloat(PurchaseExcTax) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        $('#txtPurchaseExcTax' + id).val(Math.round(PurchaseExcTax * 100) / 100);

        var _extraDiscount = Math.round((parseFloat($('#hdnExtraDiscounts' + id).val()) / parseFloat($('#txtQuantity' + id).val())) * 100) / 100;

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * PurchaseIncTax) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function discallcalc() {
    var taxs = [];
    var _extraDiscount = 0;

    var amountExcTax = 0, amountIncTax = 0, discount = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        amountExcTax = amountExcTax + parseFloat($('#txtAmountExcTax' + _id).val());
        amountIncTax = amountIncTax + parseFloat($('#txtAmountIncTax' + _id).val());

        $('#txtTotalTaxAmount' + _id).val(parseFloat($('#txtAmountIncTax' + _id).val()) - parseFloat($('#txtAmountExcTax' + _id).val()));

        if ($("#ddlDiscountType" + _id + " option:selected").val() == 'Percentage') {
            discount = discount + ($('#txtDiscount' + _id).val() == '' ? 0 : ((($('#txtDiscount' + _id).val() / 100) * parseFloat($('#txtUnitCost' + _id).val()))));
        }
        else {
            discount = discount + ($('#txtDiscount' + _id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + _id).val())));
        }

        _extraDiscount = $('#hdnExtraDiscounts' + _id).val() / parseFloat($('#txtQuantity' + _id).val());

        taxs.push({
            TaxId: $('#ddlTax' + _id).val().split('-')[0],
            AmountExcTax: parseFloat($('#txtAmountExcTax' + _id).val())//amountExcTax
        });
    });

    $('#divAdditionalCharges .charge-item').each(function (index) {
        var count = index;
        var amount = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());

        if (amount > 0) {
            taxs.push({
                TaxId: $('#ddlAdditionalChargesTax' + count).val().split('-')[0],
                AmountExcTax: amount
            });
        }
    });

    let amount = 0;
    let divTotalAmount = $('#hdndivTotalAmount').val() == '' ? 0 : parseFloat($('#hdndivTotalAmount').val());

    if ($("#ddlDiscAll option:selected").val() == 'Percentage') {
        amount = $('#txtDiscAll').val() == '' ? 0 : (($('#txtDiscAll').val() / 100) * divTotalAmount);
    }
    else {
        amount = $('#txtDiscAll').val() == '' ? 0 : (parseFloat($('#txtDiscAll').val()));
    }

    var det = {
        Taxs: taxs,
    };

    $.ajax({
        url: '/taxsettings/TaxBreakups',
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
                var html = '';
                data.Data.Taxs.forEach(function (res) {
                    html = html + '<div class="amount-item d-flex justify-content-between align-items-center py-1">' +
                        '<label class="mb-0 text-sm">' + res.Tax +
                        '</label> ' +
                        '<span class="font-weight-bold text-primary" style="font-size: 16px;" id="divDiscount">' + CurrencySymbol + res.TaxAmount.toFixed(2) + '</span>' +
                        '<input type="hidden" id="hdndivDiscount" />' +
                        '</div>';
                });

                $('#divTaxSummary').empty();
                $('#divTaxSummary').append(html);

                var AdditionalChargesAmountExcTax = 0, AdditionalChargesAmountIncTax = 0;
                $('#divAdditionalCharges .charge-item').each(function (index) {
                    var count = index;
                    var txtAdditionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
                    var txtAdditionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());

                    if (txtAdditionalChargesAmountExcTax > 0) {
                        AdditionalChargesAmountExcTax += txtAdditionalChargesAmountExcTax;
                        AdditionalChargesAmountIncTax += txtAdditionalChargesAmountIncTax;
                    }
                });

                let taxper = 0;//$('#ddlTax').val().split('-')[1] == undefined ? 0 : $('#ddlTax').val().split('-')[1];
                let taxamt = (amountIncTax - amountExcTax) + (AdditionalChargesAmountIncTax - AdditionalChargesAmountExcTax) + (parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100));
                $('#hdndivTax').val(Math.round((parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100)) * 100) / 100);
                $('#hdndivTotalTax').val(Math.round(taxamt * 100) / 100);
                $('#divTax').text(CurrencySymbol + Math.round(taxamt * 100) / 100);

                $('#divDiscount').text(CurrencySymbol + ((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100));
                $('#hdndivDiscount').val((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100);

                let SpecialDiscount = $('#txtSpecialDiscount').val() || 0;
                $('#divSpecialDiscount').text(CurrencySymbol + (Math.round(SpecialDiscount * 100) / 100));

                // Get points discount
                var pointsDiscount = parseFloat($('#divPointsDiscount').text().replace(CurrencySymbol, '').replace(/,/g, '')) || 0;

                var grandTotal = Math.round(((divTotalAmount + taxamt + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount) + pointsDiscount)) * 100) / 100;
                var roundOffGrandTotal = grandTotal;
                if ($('#hdnEnableRoundOff').val().toLocaleLowerCase() == 'true') {
                    roundOffGrandTotal = Math.round(grandTotal * 1) / 1;
                }
                var roundOff = roundOffGrandTotal - grandTotal;

                $('#divNetAmount').text(CurrencySymbol + Math.round(grandTotal * 100) / 100);
                $('#divRoundOff').text(CurrencySymbol + Math.round(roundOff * 100) / 100);
                $('#divGrandTotal').text(CurrencySymbol + Math.round(roundOffGrandTotal * 100) / 100);
                $('.divGrandTotal').text(CurrencySymbol + Math.round(roundOffGrandTotal * 100) / 100);

                // Update points earned info when totals change
                if (typeof updatePointsEarnedInfoOnTotalChange === 'function') {
                    updatePointsEarnedInfoOnTotalChange();
                }

                var grandTotal_reversecharge = Math.round(((divTotalAmount + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount) + pointsDiscount)) * 100) / 100;
                var roundOffGrandTotal_reversecharge = grandTotal_reversecharge;
                if ($('#hdnEnableRoundOff').val().toLocaleLowerCase() == 'true') {
                    roundOffGrandTotal_reversecharge = Math.round(grandTotal_reversecharge * 1) / 1;
                }
                var roundOff_reversecharge = roundOffGrandTotal_reversecharge - grandTotal_reversecharge;
                $('#divNetAmount_reversecharge').text(CurrencySymbol + Math.round((grandTotal_reversecharge) * 100) / 100);
                $('#divRoundOff_reversecharge').text(CurrencySymbol + Math.round((roundOff_reversecharge) * 100) / 100);
                $('#divGrandTotal_reversecharge').text(CurrencySymbol + Math.round((roundOffGrandTotal_reversecharge) * 100) / 100);

                if ($('#chkIsReverseCharge').is(':checked') == true) {
                    $('.reversecharge').show();
                    $('.no_reversecharge').hide();
                }
                else {
                    $('.reversecharge').hide();
                    $('.no_reversecharge').show();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateExtraDiscounts() {
    let amount = 0;
    let divTotalAmount = $('#hdndivTotalAmount').val() == '' ? 0 : parseFloat($('#hdndivTotalAmount').val());

    if ($("#ddlDiscAll option:selected").val() == 'Percentage') {
        amount = $('#txtDiscAll').val() == '' ? 0 : (($('#txtDiscAll').val() / 100) * divTotalAmount);
    }
    else {
        amount = $('#txtDiscAll').val() == '' ? 0 : (parseFloat($('#txtDiscAll').val()));
    }
    $('#divCombo tr').each(function () {
        var id = this.id.split('divCombo')[1];
        let discount = 0;
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        }
        else {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + id).val()) / 100) * parseFloat($('#txtUnitCost' + id).val());
        }

        var _subTotal = parseFloat($('#txtUnitCost' + id).val()) * parseFloat($('#txtQuantity' + id).val());
        var _extraDiscount = ((_subTotal / divTotalAmount) * amount);

        $('#hdnExtraDiscounts' + id).val(_extraDiscount);

        $('#txtPurchaseExcTax' + id).val(Math.round(((parseFloat($('#txtUnitCost' + id).val())) - (discount + (_extraDiscount / parseFloat($('#txtQuantity' + id).val())))) * 100) / 100);
        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);

    });

    discallcalc();
}

function deleteCombo(id, SalesDetailsId) {
    if (SalesDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        var skuRemove = $('#txtSkuCode' + id).val();
        skuCodes = jQuery.grep(skuCodes, function (value) {
            return value.trim() != skuRemove.trim();
        });

        $('#divCombo' + id).hide();
        $('#txtUnitCost' + id).val(0);
        $('#ddlTax' + id).val($('#ddlTax' + id + ' option:first').val());
        $('#txtAmountExcTax' + id).val(0);
        $('#txtAmountIncTax' + id).val(0);
        $('#txtDiscAll').val(0);
        updateAmount();

        //var det = {
        //    SalesDetailsId: SalesDetailsId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/Sales/SalesDetailsDelete',
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
        //var skuRemove = $('#txtSkuCode' + id).val();
        //skuCodes = jQuery.grep(skuCodes, function (value) {
        //    return value.trim() != skuRemove.trim();
        //});

        $('#divCombo' + id).remove();
        //$('#txtUnitCost' + id).val(0);
        //$('#ddlTax' + id).val($('#ddlTax' + id + ' option:first').val());
        //$('#txtAmountExcTax' + id).val(0);
        //$('#txtAmountIncTax' + id).val(0);
        //$('#txtDiscAll').val(0);
        updateAmount();
    }
}

function deleteFullCombo(SalesId) {
    if (SalesId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        $('#divCombo tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {
                $('#divCombo' + _id).hide();
                $('#txtUnitCost' + _id).val(0);
                $('#ddlTax' + _id).val($('#ddlTax' + _id + ' option:first').val());
                $('#txtAmountExcTax' + _id).val(0);
                $('#txtAmountIncTax' + _id).val(0);
                $('#txtDiscAll').val(0);
            }
        });
        updateAmount();
        skuCodes = [];

        //var det = {
        //    SalesId: SalesId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/Sales/SalesDetailsDelete',
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
        //$('#divComboNetAmount').text(0);
        //$('#divCombo tr').each(function () {
        //    var _id = this.id.split('divCombo')[1];
        //    if (_id != undefined) {
        //        $('#divCombo' + _id).hide();
        //        $('#txtUnitCost' + _id).val(0);
        //        $('#ddlTax' + _id).val($('#ddlTax' + _id + ' option:first').val());
        //        $('#txtAmountExcTax' + _id).val(0);
        //        $('#txtAmountIncTax' + _id).val(0);
        //        $('#txtDiscAll').val(0);
        //    }
        //});
        updateAmount();
        skuCodes = [];
    }
}

function increaseQuantity(_id) {
    var currentQuantity = parseFloat($('#txtQuantity' + _id).val()) || 0;
    var newQuantity = currentQuantity + 1;
    $('#txtQuantity' + _id).val(newQuantity);
    updateQuantity(_id);
}

function decreaseQuantity(_id) {
    var currentQuantity = parseFloat($('#txtQuantity' + _id).val()) || 0;
    var newQuantity = Math.max(0, currentQuantity - 1);
    $('#txtQuantity' + _id).val(newQuantity);
    updateQuantity(_id);
}

function updateQuantity(_id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if (_id != undefined) {
        var IsManageStock = $('#hdnIsManageStock' + _id).val();
        if (IsManageStock.toLowerCase() == "true") {
            var StockQuantity = parseFloat($('#txtStockQuantity' + _id).val());
            var newQuantity = 0;

            var Quantity = parseFloat($('#txtQuantity' + _id).val());
            var FreeQuantity = parseFloat($('#txtFreeQuantity' + _id).val());

            newQuantity = Quantity + FreeQuantity;
            if (newQuantity > StockQuantity) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                //toastr.error('Not enough stock available');
                $('#txtQuantity' + _id).val(StockQuantity);
                $('#txtFreeQuantity' + _id).val(0);
                $('#divQuantity' + _id).text('Not enough stock available');
                $('#divQuantity' + _id).show();
            }
        }

        ChangeQtyAmount(_id);
    }
}

function cancel() {
    var r = confirm("Are yo sure you want to Cancel the existing form and Add New?");
    if (r == true) {
        location.href = '/sales/posadd';
    }
}

function openCustomerModal() {
    $("#CustomerModal").modal('show');
    toggleGstTreatment();

    // Assign the search value to txtMobileNo if available
    if (window.lastCustomerSearchTerm) {
        $('#txtMobileNo').val(window.lastCustomerSearchTerm);
    }

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_DOB').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase(),
        defaultDate: new Date(),
        icons: DateTimePickerIcons
    });

    $('#_JoiningDate').datetimepicker({
        widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
        format: DateFormat.toUpperCase(),
        defaultDate: new Date(),
        icons: DateTimePickerIcons
    });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');
}

function insertCustomer() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var Addresses = [];
    Addresses.push({
        Name: $('#txtAddrName').val(),
        MobileNo: $('#txtAddrMobileNo').val(),
        MobileNo2: $('#txtAddrMobileNo2').val(),
        EmailId: $('#txtAddrEmailId').val(),
        Landmark: $('#txtAddrLandmark').val(),
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
    });
    Addresses.push({
        Name: $('#txtAddrAltName').val(),
        MobileNo: $('#txtAddrAltMobileNo').val(),
        MobileNo2: $('#txtAddrAltMobileNo2').val(),
        EmailId: $('#txtAddrAltEmailId').val(),
        Landmark: $('#txtAddrAltLandmark').val(),
        Address: $('#txtAltAddress').val(),
        CountryId: $('#ddlAltCountry').val(),
        StateId: $('#ddlAltState').val(),
        CityId: $('#ddlAltCity').val(),
        Zipcode: $('#txtAltZipcode').val(),
    });
    var det = {
        Username: $('#txtUsername').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        BusinessName: $('#txtBusinessName').val(),
        DOB: moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtDOB').val(),
        JoiningDate: moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtJoiningDate').val(),
        UserGroupId: $('#ddlUserGroup').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        CreditLimit: $('#txtCreditLimit').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        UserType: 'Customer',
        IsActive: true,
        IsDeleted: false,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        IsShippingAddressDifferent: $('#chkIsShippingAddressDifferent').is(':checked'),
        Addresses: Addresses,
        CurrencyId: $('#ddlUserCurrency').val(),
        TaxPreference: $("#ddlTaxPreference option:selected").text(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        PlaceOfSupplyId: $('#ddlPlaceOfSupply_M').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        BusinessLegalName: $('#txtBusinessLegalName').val(),
        BusinessTradeName: $('#txtBusinessTradeName').val(),
        PanNo: $('#txtPanNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/UserInsert',
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
                $('#ddlCustomer').append($('<option>', { value: data.Data.User.UserId, text: data.Data.User.Name + ' - ' + data.Data.User.MobileNo }));
                $('#ddlCustomer').val(data.Data.User.UserId);
                $('#CustomerModal').modal('toggle');

                FetchUserCurrency();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};
function toggleGstTreatment() {
    if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)"
        || $('#ddlGstTreatment').val() == "Composition Taxable Supply" ||
        $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
        || $('#ddlGstTreatment').val() == "Supply by SEZ Developer" || $('#ddlGstTreatment').val() == "Tax Deductor") {
        $('.divGst').show();
        $('.divPlaceOfSupply_M').show();
        $('.divTaxPreference').show();
    }
    else if ($('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)") {
        $('.divGst').hide();
        $('.divPlaceOfSupply_M').hide();
        $('.divTaxPreference').hide();
        $('.divNonTaxable').hide();
    }
    else {
        $('.divGst').hide();
        $('.divPlaceOfSupply_M').show();
        $('.divTaxPreference').show();
    }
}
function addNewPayment() {
    var dropdown = $('#ddlSinglePaymentType').html();
    var depositToDropdown = $('#ddlSingleAccount').html();

    var html = '';
    html = '<div class="card card-solid bg-gray-light" id="divMultiplePayments' + multiplePaymentsCount + '">' +
        '<div class="card-body">' +
        '<div style="display:block;text-align:right">' +
        '<a href="javascript:void(0)" class="btn btn-danger" onclick="deletePayment(0,' + multiplePaymentsCount + ')" id="Delete" data-toggle="tooltip" data-placement="bottom" title = "Delete"> <i class="far fa-trash-alt"></i></a>' +
        '</div>' +
        '<div class="row">' +
        '<div class="col-md-3">' +
        '<div class="">' +
        '<label for="amount_1">Amount <span class="danger">*</span></label>' +
        '<input type="number" class="form-control text-right payment" id="txtMultipleAmount' + multiplePaymentsCount + '" name="amount_1" placeholder="" onchange="calculate_payments(1)">' +
        '<span id="amount_1_msg" style="display:none" class="text-danger"></span>' +
        '</div>' +
        '</div>' +
        '<div class="col-md-3">' +
        '<div class="form-group">' +
        '<label>Payment Method <span class="danger">*</span></label>' +
        '<select class="form-control select2" style="width: 100%;" id="ddlMultiplePaymentType' + multiplePaymentsCount + '">' + dropdown + '</select>' +
        '</div>' +
        '</div>' +
        '<div class="col-md-3">' +
        '<div class="form-group">' +
        '<label>Deposit To</label>' +
        '<select class="form-control select2" style="width: 100%;" id="ddlAccount' + multiplePaymentsCount + '">' + depositToDropdown + '</select>' +
        '</div>' +
        '</div>' +
        '<div class="col-md-3">' +
        '<div class="">' +
        '<label for="payment_note_1">Payment Note</label>' +
        '<textarea style="height:37px" type="text" class="form-control" id="txtMultipleNotes' + multiplePaymentsCount + '" name="payment_note_1" placeholder=""></textarea>' +
        '<span id="payment_note_1_msg" style="display:none" class="text-danger"></span>' +
        '</div>' +
        '</div>' +
        '<div class="clearfix"></div>' +
        '</div>' +
        '</div>';
    $('#payments_div').append(html);

    $('.select2').select2();
    multiplePaymentsCount++;
}

function finalizePayment(Status, PaymentType) {
    if (window.location.href.toLowerCase().indexOf('add') > -1) {
        insert(Status, PaymentType);
    }
    else {
        update(Status, PaymentType);
    }
}

function finalizeHold(Status, PaymentType) {
    if (window.location.href.toLowerCase().indexOf('add') > -1) {
        insert(Status, PaymentType);
    }
    else {
        update(Status, PaymentType);
    }
}

function insert(Status, PaymentType) {
    if (PaymentType.toLowerCase() != "single") {
        var _totalPaying = 0;
        $('#payments_div .card-solid').each(function () {
            var _id = this.id.split('divMultiplePayments')[1];
            if (_id != undefined) {
                _totalPaying = _totalPaying + parseFloat($('#txtMultipleAmount' + _id).val());
            }
        });
        if (_totalPaying < parseFloat($("#divGrandTotal").text().replace(/[^0-9.]/g, ''))) {
            if (confirm('Paid amount is less than the payable amount. Do you still want to continue?')) {
                finalInsert(Status, PaymentType);
            }
        }
        else {
            finalInsert(Status, PaymentType);
        }
    }
    else {
        finalInsert(Status, PaymentType);
    }
};

function insertDirect(Status, PaymentTypeId) {
    _PaymentTypeId = PaymentTypeId;
    insert(Status, 'Single');
}

function finalInsert(Status, PaymentType) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#divCustomer').show();

    var ItemDetails = [];
    var Payments = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var _lot = $('#ddlLot' + _id).val();
            ItemDetails.push({
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                PriceExcTax: $('#txtPurchaseExcTax' + _id).val(),
                PriceIncTax: $('#txtPurchaseIncTax' + _id).val(),
                Tax: $('#txtTaxAmount' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Discount: $('#txtDiscount' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                LotNo: $('#txtLotNo' + _id).val(),
                ManufacturingDate: $('#txtManufacturingDate' + _id).val(),
                ExpiryDate: $('#txtExpiryDate' + _id).val(),
                /*PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,*/
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                FreeQuantity: $('#txtFreeQuantity' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                DiscountType: $('#ddlDiscountType' + _id).val(),
                DefaultProfitMargin: $('#txtDefaultProfitMargin' + _id).val(),
                OtherInfo: $('#txtOtherInfo' + _id).val(),
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                WarrantyId: $('#ddlWarranty' + _id).val(),
                /*UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,*/
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
                TaxExemptionId: $('#ddlTaxExemption' + _id).val(),
                ItemCodeId: $('#ddlItemCode' + _id).val(),
            })
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .charge-item').each(function (index) {
        var count = index;
        var additionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
        var additionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());
        var taxId = $('#ddlAdditionalChargesTax' + count).val().split('-')[0];
        var additionalChargeId = $('#txtAdditionalChargeId' + count).val();
        var taxExemptionId = $('#exemptionReasonId' + count).val();

        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            TaxExemptionId: taxExemptionId,
            IsActive: true,
            IsDeleted: false
        })
    });

    if (Status == 'Due') {
        if (PaymentType.toLowerCase() == "single") {
            Payments.push({
                IsActive: true,
                IsDeleted: false,
                PaymentDate: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $('#txtSalesDate').val() : null,
                //PaymentTypeId: $('#ddlSinglePaymentType').val(),
                Amount: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, "") : 0,
                PaymentTypeId: _PaymentTypeId,
                Type: "Sales Payment"
            })
        }
        else if (PaymentType.toLowerCase() == "multiple") {
            $('#payments_div .card-solid').each(function () {
                var _id = this.id.split('divMultiplePayments')[1];
                if (_id != undefined) {
                    Payments.push({
                        IsActive: true,
                        IsDeleted: false,
                        Notes: $('#txtMultipleNotes' + _id).val(),
                        Amount: $('#txtMultipleAmount' + _id).val(),
                        PaymentTypeId: $('#ddlMultiplePaymentType' + _id).val(),
                        Type: "Sales Payment",
                        AccountId: $('#ddlAccount' + _id).val(),
                    })
                }
            });
        }
    }

    var det = {
        PaymentType: PaymentType,
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        TaxExemptionId: $('#txtTaxExemptionId').val(),
        SalesDate: moment($("#txtSalesDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtSalesDate").val(),
        Status: Status,//$("#ddlStatus").val(),
        InvoiceNo: $("#txtInvoiceNo").val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm_M').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        ShippingDocument: ShippingDocument,
        FileExtensionShippingDocument: FileExtensionShippingDocument,
        OtherCharges: $("#txtOtherCharges").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        TotalQuantity: $('#divTotalQty').text(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#divDiscount").text().replace(/[^0-9.]/g, ''),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        PackagingCharge: $('#txtPackagingCharge').val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Notes: $("#txtNotes").val(), Terms: $("#txtTerms").val(),
        IsActive: true,
        IsDeleted: false,
        SalesDetails: ItemDetails,
        SalesType: 'Sales',
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        OnlinePaymentSettingsId: $('#ddlOnlinePaymentSettings').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ReferenceId: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[0].split('=')[1].replace('%20', " ") : '',
        ReferenceType: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[1].split('=')[1].replace('%20', " ") : '',
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        CountryId: $('#hdnCountryId').val(),
        PayTaxForExport: $('#chkPayTaxForExport').is(':checked') == true ? 1 : 2,
        TaxCollectedFromCustomer: $('#chkTaxCollectedFromCustomer').is(':checked') == true ? 1 : 2,
        SalesAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val(),
        RedeemPoints: $('#hdnRedeemPoints').val() || 0,
        TotalPaying: $('#TotalPaying').text().replace(/[^0-9.]/g, ''),
        Balance: $('#Balance').text().replace(/[^0-9.]/g, ''),
        ChangeReturn: $('#ChangeReturn').text().replace(/[^0-9.]/g, ''),
        HoldReason: $('#txtHoldReason').val(),
        AccountId: $('#ddlChangeReturnAccount').val(),
        PaymentTypeId: $('#ddlChangeReturnPaymentType').val(),
        Payments: Payments,
        // Restaurant/KOT Integration
        TableId: $('#ddlTableId').length > 0 && $('#ddlTableId').val() != '0' ? parseInt($('#ddlTableId').val()) : null,
        BookingId: $('#hdnBookingId').length > 0 && $('#hdnBookingId').val() != '0' ? parseInt($('#hdnBookingId').val()) : null,
        KotId: $('#hdnKotId').length > 0 && $('#hdnKotId').val() != '0' ? parseInt($('#hdnKotId').val()) : null
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesInsert',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                if (Status == 'Due') {
                    if (data.Data.PosSetting.AutoPrintInvoiceFinal == true) {
                        if (data.Data.PosSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/invoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/receipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }
                location.reload();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function update(Status, PaymentType) {
    if (PaymentType.toLowerCase() != "single") {
        var _totalPaying = 0;
        $('#payments_div .card-solid').each(function () {
            var _id = this.id.split('divMultiplePayments')[1];
            if (_id != undefined) {
                _totalPaying = _totalPaying + parseFloat($('#txtMultipleAmount' + _id).val());
            }
        });
        if (_totalPaying < parseFloat($("#divGrandTotal").text().replace(/[^0-9.]/g, ''))) {
            if (confirm('Paid amount is less than the payable amount. Do you still want to continue?')) {
                finalUpdate(Status, PaymentType);
            }
        }
        else {
            finalUpdate(Status, PaymentType);
        }
    }
    else {
        finalUpdate(Status, PaymentType);
    }
};

function updateDirect(Status, PaymentTypeId) {
    _PaymentTypeId = PaymentTypeId;
    update(Status, 'Single');
}

function finalUpdate(Status, PaymentType) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#divCustomer').show();

    var ItemDetails = [];
    var Payments = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var _lot = $('#ddlLot' + _id).val();
            ItemDetails.push({
                SalesDetailsId: $('#txtSalesDetailsId' + _id).val(),
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                PriceExcTax: $('#txtPurchaseExcTax' + _id).val(),
                PriceIncTax: $('#txtPurchaseIncTax' + _id).val(),
                Tax: $('#txtTaxAmount' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Discount: $('#txtDiscount' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                LotNo: $('#txtLotNo' + _id).val(),
                ManufacturingDate: $('#txtManufacturingDate' + _id).val(),
                ExpiryDate: $('#txtExpiryDate' + _id).val(),
                /*PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,*/
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                FreeQuantity: $('#txtFreeQuantity' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                DiscountType: $('#ddlDiscountType' + _id).val(),
                DefaultProfitMargin: $('#txtDefaultProfitMargin' + _id).val(),
                OtherInfo: $('#txtOtherInfo' + _id).val(),
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                WarrantyId: $('#ddlWarranty' + _id).val(),
                /*UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,*/
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
                TaxExemptionId: $('#ddlTaxExemption' + _id).val(),
                ItemCodeId: $('#ddlItemCode' + _id).val(),
            })
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .charge-item').each(function (index) {
        var count = index;
        var additionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
        var additionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());
        var taxId = $('#ddlAdditionalChargesTax' + count).val().split('-')[0];
        var additionalChargeId = $('#txtAdditionalChargeId' + count).val();
        var taxExemptionId = $('#exemptionReasonId' + count).val();
        var salesAdditionalChargesId = $('#txtSalesAdditionalChargesId' + count).val();
        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            TaxExemptionId: taxExemptionId,
            SalesAdditionalChargesId: salesAdditionalChargesId,
            IsActive: true,
            IsDeleted: false
        })
    });

    if (Status == 'Due') {
        if (PaymentType.toLowerCase() == "single") {
            Payments.push({
                IsActive: true,
                IsDeleted: false,
                PaymentDate: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $('#txtSalesDate').val() : null,
                //PaymentTypeId: $('#ddlSinglePaymentType').val(),
                Amount: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, "") : 0,
                PaymentTypeId: _PaymentTypeId,
                Type: "Sales Payment"
            })
        }
        else if (PaymentType.toLowerCase() == "multiple") {
            $('#payments_div .card-solid').each(function () {
                var _id = this.id.split('divMultiplePayments')[1];
                if (_id != undefined) {
                    Payments.push({
                        IsActive: true,
                        IsDeleted: false,
                        Notes: $('#txtMultipleNotes' + _id).val(),
                        Amount: $('#txtMultipleAmount' + _id).val(),
                        PaymentTypeId: $('#ddlMultiplePaymentType' + _id).val(),
                        Type: "Sales Payment",
                        AccountId: $('#ddlAccount' + _id).val(),
                    })
                }
            });
        }
    }

    var det = {
        SalesId: window.location.href.split('=')[1].replace(/[^0-9.]/g, ''),
        PaymentType: PaymentType,
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        TaxExemptionId: $('#txtTaxExemptionId').val(),
        SalesDate: moment($("#txtSalesDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$("#txtSalesDate").val(),
        Status: Status,//$("#ddlStatus").val(),
        InvoiceNo: $("#txtInvoiceNo").val(),
        //PayTerm: $('#ddlPayTerm').val(),
        //PayTermNo: $('#txtPayTermNo').val(),
        PaymentTermId: $('#ddlPaymentTerm_M').val(),
        DueDate: moment($("#txtDueDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        ShippingDocument: ShippingDocument,
        FileExtensionShippingDocument: FileExtensionShippingDocument,
        OtherCharges: $("#txtOtherCharges").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        TotalQuantity: $('#divTotalQty').text(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#divDiscount").text().replace(/[^0-9.]/g, ''),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        PackagingCharge: $('#txtPackagingCharge').val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Notes: $("#txtNotes").val(), Terms: $("#txtTerms").val(),
        IsActive: true,
        IsDeleted: false,
        SalesDetails: ItemDetails,
        SalesType: 'Sales',
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        OnlinePaymentSettingsId: $('#ddlOnlinePaymentSettings').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ReferenceId: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[0].split('=')[1].replace('%20', " ") : '',
        ReferenceType: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[1].split('=')[1].replace('%20', " ") : '',
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        CountryId: $('#hdnCountryId').val(),
        PayTaxForExport: $('#chkPayTaxForExport').is(':checked') == true ? 1 : 2,
        TaxCollectedFromCustomer: $('#chkTaxCollectedFromCustomer').is(':checked') == true ? 1 : 2,
        SalesAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val(),
        RedeemPoints: $('#hdnRedeemPoints').val() || 0,
        TotalPaying: $('#TotalPaying').text().replace(/[^0-9.]/g, ''),
        Balance: $('#Balance').text().replace(/[^0-9.]/g, ''),
        ChangeReturn: $('#ChangeReturn').text().replace(/[^0-9.]/g, ''),
        HoldReason: $('#txtHoldReason').val(),
        AccountId: $('#ddlChangeReturnAccount').val(),
        PaymentTypeId: $('#ddlChangeReturnPaymentType').val(),
        Payments: Payments
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesUpdate',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (Status == 'Due') {
                    if (data.Data.PosSetting.AutoPrintInvoiceFinal == true) {
                        if (data.Data.PosSetting.InvoiceType == 1) {
                            sessionStorage.setItem('InvoiceUrl', '/sales/invoice?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                        else {
                            sessionStorage.setItem('InvoiceUrl', '/sales/receipt?InvoiceId=' + data.Data.Sale.InvoiceId);
                        }
                    }
                }
                window.location.href = "/sales/posadd";
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function calculate_payments(type) {
    if (type == 0) {
        var payingAmount = (parseFloat($('#txtSingleAmount').val())).toFixed(2);
        $('.TotalPaying').text(CurrencySymbol + payingAmount);
        /*var payableAmount = $('#hdndivTotalAmount').val();*/
        var payableAmount = $('#divGrandTotal').text().replace(/[^0-9.]/g, '');
        if (parseFloat(payingAmount) > parseFloat(payableAmount)) {
            $('.ChangeReturn').text(CurrencySymbol + (parseFloat(payingAmount) - parseFloat(payableAmount)).toFixed(2));
            $('.Balance').text(CurrencySymbol + 0);
        }
        else {
            $('.ChangeReturn').text(CurrencySymbol + 0);
            $('.Balance').text(CurrencySymbol + (parseFloat(payableAmount) - parseFloat(payingAmount)).toFixed(2));
        }
    }
    else {
        var payingAmount = 0;
        //var payableAmount = $('#hdndivTotalAmount').val();
        var payableAmount = $('#divGrandTotal').text().replace(/[^0-9.]/g, '');
        $('#payments_div .card-solid').each(function () {
            var _id = this.id.split('divMultiplePayments')[1];
            if (_id != undefined) {
                if ($('#txtMultipleAmount' + _id).val() != "" && $('#txtMultipleAmount' + _id).val() != '') {
                    payingAmount = payingAmount + parseFloat($('#txtMultipleAmount' + _id).val());
                }
            }
        });
        $('.TotalPaying').text(CurrencySymbol + payingAmount.toFixed(2));
        if (parseFloat(payingAmount) > parseFloat(payableAmount)) {
            $('.ChangeReturn').text(CurrencySymbol + (parseFloat(payingAmount) - parseFloat(payableAmount)).toFixed(2));
            $('.Balance').text(CurrencySymbol + 0);
        }
        else {
            $('.ChangeReturn').text(CurrencySymbol + 0);
            $('.Balance').text(CurrencySymbol + (parseFloat(payableAmount) - parseFloat(payingAmount)).toFixed(2));
        }
    }

    if ($('.ChangeReturn').text().replace(/[^0-9.]/g, '') == 0) {
        $('.divChangeReturn').hide();
    }
    else {
        //$('.divChangeReturn').show();
        $('.divChangeReturn').css('display', 'flex');
    }
}

function fullScreen() {
    if ((document.fullScreenElement && document.fullScreenElement !== null) || (!document.mozFullScreen && !document.webkitIsFullScreen)) {
        if (document.documentElement.requestFullScreen) {
            document.documentElement.requestFullScreen();
        }
        else if (document.documentElement.mozRequestFullScreen) { /* Firefox */
            document.documentElement.mozRequestFullScreen();
        }
        else if (document.documentElement.webkitRequestFullScreen) {   /* Chrome, Safari & Opera */
            document.documentElement.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
        }
        else if (document.msRequestFullscreen) { /* IE/Edge */
            document.documentElement.msRequestFullscreen();
        }
    }
    else {
        if (document.cancelFullScreen) {
            document.cancelFullScreen();
        }
        else if (document.mozCancelFullScreen) { /* Firefox */
            document.mozCancelFullScreen();
        }
        else if (document.webkitCancelFullScreen) {   /* Chrome, Safari and Opera */
            document.webkitCancelFullScreen();
        }
        else if (document.msExitFullscreen) { /* IE/Edge */
            document.msExitFullscreen();
        }
    }
}

function Delete(SalesId, t) {
    var r = confirm("This will delete \"" + t + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SalesId: SalesId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/Salesdelete',
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
                    if (t != null) {
                        $('#' + t).remove();
                        //fetchSalesByUser();
                    }
                    else {
                        fetchList();
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function DeleteHoldList(SalesId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SalesId: SalesId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/Salesdelete',
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

                    if (window.location.href.toLowerCase().indexOf('edit') > -1) {
                        if (window.location.href.split('=')[1] == SalesId) {
                            window.location.href = "/sales/posadd";
                        }
                        else {
                            fetchPosHoldList();
                        }
                    }
                    else {
                        fetchPosHoldList();
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function openPaymentModal(type, SalesId, title, BranchId) {
    _SalesId = SalesId;
    _BranchId = BranchId;
    var det = {
        SalesId: SalesId,
        Type: title,
        Title: title,
        BranchId: _BranchId,
        IsAdvance: true
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesPayments',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPayments").html(data);

            $("#paymentModal").modal('show');
            if (type == true) {
                $('.paymentAdd').show();
                $('.paymentList').hide();
                $('#paymentModalLabel').text('Add Payment');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_PaymentDate').datetimepicker({
                    widgetPositioning: { horizontal: 'auto', vertical: 'bottom' },
                    format: DateFormat.toUpperCase() + ' ' + TimeFormat,
                    defaultDate: new Date(),
                    icons: DateTimePickerIcons
                });
                $('#_PaymentDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });

            }
            else {
                $('.paymentAdd').hide();
                $('.paymentList').show();
                $('#paymentModalLabel').text('View Payments');
            }

            $('.select2').select2();

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertPayment() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Id: _SalesId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: $('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Pos",
        AccountId: $('#ddlLAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/paymentInsert',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#paymentModal").modal('hide');
                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deletePayment(PaymentId, divId) {
    if (PaymentId == 0) {
        $('#divMultiplePayments' + divId).remove();
        calculate_payments(1);
    }
    else {
        var r = confirm("Are you sure you want to delete?");
        if (r == true) {
            var det = {
                PaymentId: PaymentId
            }
            $("#divLoading").show();
            $.ajax({
                url: '/Sales/paymentDelete',
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
                        //fetchList();
                        //$('#tr_' + PaymentId).remove();
                        $('#divMultiplePayments' + divId).remove();
                        calculate_payments(1);
                    }
                },
                error: function (xhr) {
                    $("#divLoading").hide();
                }
            });
        }
    }

}

function getPaymentAttachDocumentBase64() {
    var file1 = $("#PaymentAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error("File too Big, please select a file less than 2mb");
        $("#PaymentAttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            PaymentAttachDocument = reader.result;
            PaymentFileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahPaymentAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            file = error;
        };
    }
}

function openCalculator() {
    window.open('Calculator:///');
}

function fetchPosHoldList(PageIndex) {
    var det = {
        PageIndex: 1
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/PosHoldFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#divPartialPosHold").html(data);
            var c = parseFloat($('#HoldBadge').text()) - 1;
            $('#HoldBadge').text(c);
            $('#HoldCount').text(c + ' Orders On Hold');
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function EditHoldList(SalesId) {
    window.location.href = "/sales/posedit?SalesId=" + SalesId;
}

function openModal(type) {
    if (type == 1) {
        $('#modal-multiple').modal('toggle');
    }
    else {
        $('#modal-cash').modal('toggle');
    }
    updateAmount();
    calculate_payments(type);

}

function goBack() {
    window.location.href = "/sales/index";
}

$(document).ready(function () {
    fetchPosSettings();
    fetchPaymentSettings();

    function fetchPosSettings() {
        var det = {
        };
        //$("#divLoading").show();
        $.ajax({
            url: '/salessettings/PosSetting',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    var html = '';
                    var shortcutKeys = [{ "button": "btnDraft", "ShortCutKey": data.Data.PosSetting.Draft, "Title": "Draft" },
                    { "button": "btnQuotation", "ShortCutKey": data.Data.PosSetting.Quotation, "Title": "Quotation" },
                    { "button": "btnProforma", "ShortCutKey": data.Data.PosSetting.Proforma, "Title": "Proforma" },
                    { "button": "btnCreditSale", "ShortCutKey": data.Data.PosSetting.CreditSale, "Title": "Credit Sale" },
                    { "button": "btnMultiple", "ShortCutKey": data.Data.PosSetting.CreditSale, "Title": "Multiple" },
                    { "button": "btnHold", "ShortCutKey": data.Data.PosSetting.Hold, "Title": "Hold" },
                    { "button": "btnCancel", "ShortCutKey": data.Data.PosSetting.Cancel, "Title": "Cancel" },
                    { "button": "discount", "ShortCutKey": data.Data.PosSetting.EditDiscount, "Title": "Edit Discount" },
                    { "button": "other", "ShortCutKey": data.Data.PosSetting.EditOrderTax, "Title": "Edit Order Tax" },
                    { "button": "shipping", "ShortCutKey": data.Data.PosSetting.EditShippingCharge, "Title": "Edit Shipping Charge" },
                    { "button": "packaging", "ShortCutKey": data.Data.PosSetting.EditPackagingCharge, "Title": "Edit Packaging Charge" },
                    { "button": "addNewPayment", "ShortCutKey": data.Data.PosSetting.AddPaymentRow, "Title": "Add Payment Row" },
                    { "button": "btnFinalise", "ShortCutKey": data.Data.PosSetting.FinalisePayment, "Title": "Finalise Payment" },
                    { "button": "txttags", "ShortCutKey": data.Data.PosSetting.AddNewProduct, "Title": "Add New Product" },
                    { "button": "btnRecentTransactions", "ShortCutKey": data.Data.PosSetting.RecentTransactions, "Title": "Recent Transactions" },
                    { "button": "btnHoldList", "ShortCutKey": data.Data.PosSetting.HoldList, "Title": "Hold List" },
                    { "button": "btnCalculator", "ShortCutKey": data.Data.PosSetting.Calculator, "Title": "Calculator" },
                    { "button": "btnFullScreen", "ShortCutKey": data.Data.PosSetting.FullScreen, "Title": "Full Screen" },
                    { "button": "btnRegisterDetails", "ShortCutKey": data.Data.PosSetting.RegisterDetails, "Title": "Register Details" },
                    { "button": "btnPosExit", "ShortCutKey": data.Data.PosSetting.PosExit, "Title": "Pos Exit" },
                    ]

                    $.each(shortcutKeys, function (index, value) {
                        $('#' + value.button).prop('title', 'Shortcut Key [' + value.ShortCutKey + ']');
                        //html = html + '<tr><td>Draft</td><td>' + data.Data.PosSetting.Draft + '</td></tr>';
                        html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a style="color:#000;cursor:unset;" disabled href="javascript:void(0)" class="nav-link">' + value.Title;
                        if (value.ShortCutKey != "" && value.ShortCutKey != null) {
                            html = html + ' <span class="btn btn-default btn-sm"> ' + value.ShortCutKey + '</span>';

                            Mousetrap.bind(value.ShortCutKey, function (e) {
                                e.preventDefault();
                                if (value.button == "discount" || value.button == "other" || value.button == "shipping" || value.button == "packaging") {
                                    $('#' + value.button).modal('toggle');
                                }
                                else if (value.button == "addNewPayment") {
                                    addNewPayment();
                                }
                                else if (value.button == "txttags") {
                                    $('#txttags').focus();
                                }
                                else {
                                    $('#' + value.button).click();
                                }

                            });
                        }
                        html = html + '</a></li>';
                    });

                    $('.tShortcutKeys').append(html);
                }

            },
            error: function (xhr) {

            }
        });
    };

    function fetchPaymentSettings() {
        var det = {
            BranchId: $('#ddlBranch').val(),
        };
        //$("#divLoading").show();
        $.ajax({
            url: '/sales/PaymentTypes',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    var html = '';

                    $.each(data.Data.PaymentTypes, function (index, value) {
                        $('#btnPaymentType' + value.PaymentTypeId).prop('title', 'Shortcut Key [' + value.ShortCutkey + ']');
                        //html = html + '<tr><td>' + value.PaymentType + '</td><td>' + value.ShortCutKey + '</td></tr>';
                        html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a style="color:#000;cursor:unset;" id="btnshortcutkey' + index + '" disabled href="javascript:void(0)" class="nav-link">' + value.PaymentType;

                        if (value.ShortCutKey != "" && value.ShortCutKey != null) {
                            html = html + ' <span class="btn btn-default btn-sm"> [' + value.ShortCutKey + ']</span>';

                            Mousetrap.bind(value.ShortCutKey, function (e) {
                                e.preventDefault();
                                $('#btnPaymentType' + value.PaymentTypeId).click();
                            });
                        }
                        html = html + '</a></li>';
                    });

                    $('.tShortcutKeys').append(html);
                }

            },
            error: function (xhr) {

            }
        });
    };
});

function fetchSalesByUser() {
    $('#recentTransactionsModal').modal('toggle');
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Sales/SalesByUser',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#custom-tabs-four-tabContent").html(data);

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchCurrentRegister() {

    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/Sales/fetchCashRegister',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialCashRegister").html(data);
            $('#PartialCashRegisterModal').modal('toggle');
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function CloseCashRegister() {
    if (confirm("Are you sure you want to close the register?") == true) {
        var det = {
            ClosingNote: $('#txtClosingNote').val()
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/CloseCashRegister',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    sessionStorage.setItem('showMsg', '1');
                    sessionStorage.setItem('Msg', data.Message);

                    window.location.href = "/sales/index";

                }
                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function exportInvoiceToPdf() {
    $('.hidden').show();
    $('.hide').hide();
    $('.responsive-table').css('height', '100%');
    html2canvas($('.printableArea')[0], {
        onrendered: function (canvas) {
            var data = canvas.toDataURL();
            var docDefinition = {
                content: [{
                    image: data,
                    width: 500
                }]
            };

            pdfMake.createPdf(docDefinition).download("Invoice.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
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
    ChangeQtyAmount(i);
    convertAvailableStock();

    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var IsManageStock = $('#hdnIsManageStock' + i).val();
        if (IsManageStock.toLowerCase() == "true") {
            var QuantityRemaining = parseFloat($('#txtStockQuantity' + i).val());
            var Quantity = parseFloat($('#txtQuantity' + i).val());

            if (Quantity > QuantityRemaining) {
                $('#txtQuantity' + i).val(QuantityRemaining);
            }
        }
    });
}

function ChangePurAmount(id) {

    if (id != undefined) {

        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];

        let PurchaseExcTax = (100 * (PurchaseIncTax / (100 + parseFloat(taxper))));
        $('#txtPurchaseExcTax' + id).val(Math.round(PurchaseExcTax * 100) / 100);

        let taxamt = parseFloat(PurchaseExcTax) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        var _extraDiscount = Math.round((parseFloat($('#hdnExtraDiscounts' + id).val()) / parseFloat($('#txtQuantity' + id).val())) * 100) / 100;

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val((((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val((((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(((parseFloat($('#txtQuantity' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

    }
    updateAmount();
}

function setExemption() {
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            $('#divTaxExemption' + _id).hide();

            var taxValue = $("#ddlTax" + _id + " :selected").text();

            if (taxValue == "Non-Taxable") {

                var ddlTaxExemption = '<select class="form-control select2 ddlTax" id="ddlTaxExemption' + _id + '" >';
                for (let ss = 0; ss < taxExemptions.length; ss++) {
                    ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                }
                ddlTaxExemption = ddlTaxExemption + '</select>';

                var html = '<label style="margin-bottom:0;margin-top:0.5rem;"> Exemption Reason <span class="danger">*</span></label>' +
                    '<div class="input-group">' +
                    ddlTaxExemption +
                    '</div>';

                $('#divTaxExemption' + _id).empty();
                $('#divTaxExemption' + _id).append(html);

                $('#divTaxExemption' + _id).show();
            }
        }
    });
    $('.select2').select2();
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
            }
        }

        $('#txtStockQuantity' + i).val(availableQuantity);
        $('#txtAvailableQty' + i).text('Available Qty:' + availableQuantity);
    });
}

function toggleDecimal(evt, i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
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
        // Only ASCII character in that range allowed
        var ASCIICode = (evt.which) ? evt.which : evt.keyCode
        // if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
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

function onlyNumberKey(evt) {

    // Only ASCII character in that range allowed
    var ASCIICode = (evt.which) ? evt.which : evt.keyCode
    if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57))
        return false;
    return true;
}

function fetchActiveStates(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlCountry').val();
    }
    else {
        id = $('#ddlAltCountry').val();
    }
    var det = {
        CountryId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/sales/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_States_Dropdown").html(data);
            }
            else {
                $("#p_Alt_States_Dropdown").html(data);
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ChangeQtyAmount(id) {
    setExemption();
    if (id != undefined) {
        let discount = 0;
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        }
        else {
            discount = $('#txtDiscount' + id).val() == '' ? 0 : (parseFloat($('#txtDiscount' + id).val()) / 100) * parseFloat($('#txtUnitCost' + id).val());
        }
        var _extraDiscount = Math.round((parseFloat($('#hdnExtraDiscounts' + id).val()) / parseFloat($('#txtQuantity' + id).val())) * 100) / 100;
        $('#txtPurchaseExcTax' + id).val(Math.round(((parseFloat($('#txtUnitCost' + id).val())) - (discount + _extraDiscount)) * 100) / 100);
        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * ((parseFloat($('#txtPurchaseExcTax' + id).val())))) * 100) / 100);

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function FetchUserCurrency() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        UserId: $('#ddlCustomer').val(),
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/customers/FetchUserCurrency',
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
                var EnablePlaceOfSupply = $('#hdnEnablePlaceOfSupply').val().toLocaleLowerCase();
                if (EnablePlaceOfSupply == "true") {
                    if (data.Data.User) {

                        if (data.Data.User.GstTreatment != null && data.Data.User.GstTreatment != "") {
                            if (data.Data.User.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)") {
                                $('#ddlPlaceOfSupply').val(data.Data.User.PlaceOfSupplyId);
                                //$('.divPlaceOfSupply').show();
                                if (data.Data.User.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || data.Data.User.GstTreatment == "Supply by SEZ Developer") {
                                    $('.divPayTaxForExport').show();
                                }
                                else {
                                    $('.divPayTaxForExport').hide();
                                }
                            }
                            else {
                                var stateId = $('#hdnStateId').val();
                                $('#ddlPlaceOfSupply').val(stateId);
                                //$('.divPlaceOfSupply').hide();
                                $('.divPayTaxForExport').show();
                            }
                        }
                    }
                    else {
                        $('#ddlPlaceOfSupply').val(0);
                    }
                }

                $('#ddlPaymentTerm_M').val(data.Data.User.PaymentTermId);

                $('.select2').select2();

                fetchUpdatedItems();
                $('#txtTaxExemptionId').val(data.Data.User.TaxExemptionId);

                //fetchAdditionalCharges();         

                $('#txtAdvBalance').text(data.Data.User.CurrencySymbol + data.Data.User.AdvanceBalance);
                if (data.Data.User.TotalSalesDue > 0) {
                    $('#divCustomer').show();
                    $('#divCustomer').text("Due: " + data.Data.User.CurrencySymbol + data.Data.User.TotalSalesDue);
                }
                else {
                    $('#divCustomer').hide();
                }

                if (data.Data.User.ExchangeRate == 0) {
                    $('.divCurrencyExchange').hide();
                }
                else {
                    $('.divCurrencyExchange').show();
                }

                //if (data.Data.User.CurrencySymbol.trim() != CurrencySymbol.trim() && data.Data.User.CurrencyCode.trim() != CurrencySymbol.trim()) {
                if (data.Data.User.CurrencySymbol != CurrencySymbol) {

                    var ExchangeRate = data.Data.User.ExchangeRate == 0 ? 1 : data.Data.User.ExchangeRate;

                    $('#divCombo tr').each(function () {
                        var i = this.id.split('divCombo')[1];
                        var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
                        //var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
                        var UToSValue = parseFloat($('#hdnUToSValue' + i).val());// == 0 ? 1 : $('#hdnUToSValue' + i).val());
                        var SToTValue = parseFloat($('#hdnSToTValue' + i).val());// == 0 ? 1 : $('#hdnSToTValue' + i).val());
                        var TToQValue = parseFloat($('#hdnTToQValue' + i).val());// == 0 ? 1 : $('#hdnTToQValue' + i).val());

                        var UnitCost = $('#hdnUnitCost' + i).val();
                        var SalesCost = $('#hdnSalesIncTax' + i).val();
                        var newUnitCost = 0, newSalesCost = 0;
                        //var ExchangeRate = (!ExchangeRate || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

                        if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
                            newUnitCost = UnitCost;
                            newSalesCost = SalesCost;
                        }
                        else if (SToTValue == 0 && TToQValue == 0) {
                            if (index == 3) {
                                newUnitCost = UnitCost;
                                newSalesCost = SalesCost;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesCost = SalesCost / UToSValue;
                            }
                        }
                        else if (TToQValue == 0) {
                            if (index == 2) {
                                newUnitCost = UnitCost;
                                newSalesCost = SalesCost;
                            }
                            else if (index == 3) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesCost = SalesCost / UToSValue;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue / SToTValue;
                                newSalesCost = SalesCost / UToSValue / SToTValue;
                            }
                        }
                        else {
                            if (index == 1) {
                                newUnitCost = UnitCost;
                                newSalesCost = SalesCost;
                            }
                            else if (index == 2) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesCost = SalesCost / UToSValue;
                            }
                            else if (index == 3) {
                                newUnitCost = UnitCost / UToSValue / SToTValue;
                                newSalesCost = SalesCost / UToSValue / SToTValue;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
                                newSalesCost = SalesCost / UToSValue / SToTValue / TToQValue;
                            }
                        }

                        $('#txtUnitCost' + i).val(Math.round((parseFloat(newUnitCost) / ExchangeRate) * 100) / 100);
                        $('#txtSalesIncTax' + i).val(Math.round((parseFloat(newSalesCost) / ExchangeRate) * 100) / 100);
                        ChangeQtyAmount(i);
                        convertAvailableStock();

                        $('#divCombo tr').each(function () {
                            var i = this.id.split('divCombo')[1];
                            var QuantityRemaining = parseFloat($('#txtStockQuantity' + i).val());
                            var Quantity = parseFloat($('#txtQuantity' + i).val());

                            if (Quantity > QuantityRemaining) {
                                $('#txtQuantity' + i).val(QuantityRemaining);
                            }
                        });
                    });

                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error("Currency is changed");
                }
                else {
                    var ExchangeRate = 1;
                }
                CurrencySymbol = data.Data.User.CurrencySymbol;
                $('#txtExchangeRate').val(ExchangeRate);
                $('.lblCurrencySymbol').text(data.Data.User.CurrencySymbol);

                $('.lblDefaultCurrencySymbol').text(data.Data.User.DefaultCurrencySymbol);
            }

            // Fetch customer reward points when customer is selected
            if (typeof fetchCustomerRewardPoints === 'function') {
                fetchCustomerRewardPoints();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function updateCurrencyExchange() {
    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
        var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
        var UToSValue = $('#hdnUToSValue' + i).val();
        var SToTValue = $('#hdnSToTValue' + i).val();
        var TToQValue = $('#hdnTToQValue' + i).val();
        //var PurchaseExcTax = $('#txtPurchaseExcTax' + i).val();
        //var PurchaseIncTax = $('#txtPurchaseIncTax' + i).val();
        var UnitCost = $('#hdnUnitCost' + i).val();
        var SalesCost = $('#hdnSalesIncTax' + i).val();
        var newUnitCost = 0, newSalesCost = 0;
        var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

        if (index == PriceAddedFor) {
            $('#txtPurchaseExcTax' + i).val(UnitCost / ExchangeRate);
            $('#txtPurchaseIncTax' + i).val(UnitCost / ExchangeRate);
            $('#txtUnitCost' + i).val(UnitCost / ExchangeRate);
            $('#txtSalesIncTax' + i).val(SalesCost / ExchangeRate);
            //updateOpeningStockSubTotal(i);
            ChangeQtyAmount(i);
        }
        else {
            if (index > PriceAddedFor) {
                if (index == 2) {
                    newUnitCost = UnitCost / UToSValue;
                    newSalesCost = SalesCost / UToSValue;
                }
                else if (index == 3) {
                    newUnitCost = UnitCost / UToSValue / SToTValue;
                    newSalesCost = SalesCost / UToSValue / SToTValue;
                }
                else if (index == 4) {
                    newUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
                    newSalesCost = SalesCost / UToSValue / SToTValue / TToQValue;
                }
            }
            else {
                if (index == 1) {
                    newUnitCost = UnitCost * UToSValue;
                    newSalesCost = SalesCost * UToSValue;
                }
                else if (index == 2) {
                    newUnitCost = UnitCost * UToSValue * SToTValue;
                    newSalesCost = SalesCost * UToSValue * SToTValue;
                }
                else if (index == 3) {
                    newUnitCost = UnitCost * UToSValue * SToTValue * TToQValue;
                    newSalesCost = SalesCost * UToSValue * SToTValue * TToQValue;
                }
            }

            $('#txtPurchaseExcTax' + i).val(newUnitCost / ExchangeRate);
            $('#txtPurchaseIncTax' + i).val(newUnitCost / ExchangeRate);
            $('#txtUnitCost' + i).val(newUnitCost.toFixed(2) / ExchangeRate);
            $('#txtSalesIncTax' + i).val(newSalesCost.toFixed(2) / ExchangeRate);


        }
    });
    updateAmount();
    otherchargescalc();
    discallcalc();

    //var ExchangeRate = parseFloat($('#txtExchangeRate').val());

    //$('#divCombo tr').each(function () {
    //    var _id = this.id.split('divCombo')[1];

    //    $('#txtPurchaseIncTax' + _id).val(parseFloat($('#hdnPurchaseIncTax' + _id).val()) / ExchangeRate);
    //    $('#txtUnitCost' + _id).val(parseFloat($('#hdnUnitCost' + _id).val()) / ExchangeRate);
    //    $('#txtTotalAmount' + _id).val(parseFloat($('#txtQuantity' + _id).val()) * (parseFloat($('#hdnUnitCost' + _id).val()) / ExchangeRate));
    //});
    //updateAmount();
    //otherchargescalc();
    //discallcalc();
}

function fetchLotDetails(_id) {
    var _itemId = $('#txtItemId' + _id).val();
    var _ItemDetailsId = $('#txtItemDetailsId' + _id).val();
    var _lot = $('#ddlLot' + _id).val();
    if (_lot) {
        var det = {
            CustomerId: $('#ddlCustomer').val(),
            SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
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
                    $('#hdnUnitCost' + _id).val(data.Data.ItemDetail.SalesExcTax);
                    $('#hdnPurchaseIncTax' + _id).val(data.Data.ItemDetail.SalesIncTax);
                    $('#hdnQuantityRemaining' + _id).val(data.Data.ItemDetail.Quantity);
                    //$('#txtStockQuantity' + _id).val(data.Data.ItemDetail.Quantity);

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

function fetchTax() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/taxsettings/ActiveAllTaxs',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                taxList = data.Data.Taxs;
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchWarranty() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveWarrantys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                warrantyList = data.Data.Warrantys;
            }

        },
        error: function (xhr) {

        }
    });
};

function DateFormat(date) {
    date = new Date(date);
    var dateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
        .toISOString()
        .split("T")[0];

    return dateString;
}

function DateTimeFormat(date) {
    date = new Date(date);
    var dateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
        .toISOString()
        .split("T");

    var time = dateString[1].split(":");
    return dateString[0] + "T" + time[0] + ":" + time[1];
}

function getFormattedDate(d) {
    date = new Date(d)
    var dd = date.getDate();
    var mm = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    if (dd < 10) { dd = '0' + dd }
    if (mm < 10) { mm = '0' + mm };
    return d = dd + '/' + mm + '/' + yyyy;
    return date;
}

function onlyNumberKey(evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
    }
}

function ValidateStock(Status, PaymentType, type) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var ItemDetails = [];
    var Payments = [];

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var _lot = $('#ddlLot' + _id).val();
            ItemDetails.push({
                SalesDetailsId: $('#hdnSalesDetailsId' + _id).val(),
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                PriceExcTax: $('#txtPurchaseExcTax' + _id).val(),
                PriceIncTax: $('#txtPurchaseIncTax' + _id).val(),
                Tax: $('#txtTaxAmount' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Discount: $('#txtDiscount' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: false,
                LotNo: $('#txtLotNo' + _id).val(),
                ManufacturingDate: $('#txtManufacturingDate' + _id).val(),
                ExpiryDate: $('#txtExpiryDate' + _id).val(),
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],//[0].selectedIndex + 1,
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                FreeQuantity: $('#txtFreeQuantity' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id).val().split('-')[1];//[0].selectedIndex + 1,
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                DiscountType: $('#ddlDiscountType' + _id).val(),
                DefaultProfitMargin: $('#txtDefaultProfitMargin' + _id).val(),
                OtherInfo: $('#txtOtherInfo' + _id).val(),
                LotId: _lot ? _lot.split('-')[0] : 0,
                LotType: _lot ? _lot.split('-')[1] : "",
                WarrantyId: $('#ddlWarranty' + _id).val(),
            })
        }
    });

    if (Status == 'Due') {
        if (PaymentType.toLowerCase() == "single") {
            Payments.push({
                IsActive: true,
                IsDeleted: false,
                PaymentDate: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $('#txtSalesDate').val() : null,
                Amount: (PaymentType.toLowerCase() == 'single' || PaymentType.toLowerCase() == 'multiple') ? $$("#divGrandTotal").text().replace(/[^-0-9\.]+/g, "") : 0,
                PaymentTypeId: _PaymentTypeId,
                Type: "Sales Payment"
            })
        }
        else if (PaymentType.toLowerCase() == "multiple") {
            $('#payments_div .card-solid').each(function () {
                var _id = this.id.split('divMultiplePayments')[1];
                if (_id != undefined) {
                    Payments.push({
                        IsActive: true,
                        IsDeleted: false,
                        Notes: $('#txtMultipleNotes' + _id).val(),
                        Amount: $('#txtMultipleAmount' + _id).val(),
                        PaymentTypeId: $('#ddlMultiplePaymentType' + _id).val(),
                        Type: "Sales Payment",
                        PaymentId: $('#hdnPaymentId' + _id).val()
                    })
                }
            });
        }
    }

    var det = {
        SalesId: window.location.href.split('=')[1],
        PaymentType: PaymentType,
        BranchId: $('#ddlBranch').val(),
        Status: Status,
        CustomerId: $('#ddlCustomer').val(),
        SalesDate: $("#txtSalesDate").val(),
        InvoiceNo: $("#txtInvoiceNo").val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        ShippingDocument: ShippingDocument,
        FileExtensionShippingDocument: FileExtensionShippingDocument,
        OtherCharges: $("#txtOtherCharges").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#divTax').text().replace(/[^0-9.]/g, ''),
        TotalQuantity: $('#divTotalQty').text().replace(/[^0-9.]/g, ''),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#divDiscount").text().replace(/[^0-9.]/g, ''),
        GrandTotal: $("#divGrandTotal").text().replace(/[^0-9.]/g, ''),
        PackagingCharge: $('#txtPackagingCharge').val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Notes: $("#txtNotes").val(), Terms: $("#txtTerms").val(),
        IsActive: true,
        IsDeleted: false,
        SalesDetails: ItemDetails,
        SalesType: 'Pos',
        TotalPaying: $('#TotalPaying').text().replace(/[^0-9.]/g, ''),
        Balance: $('#Balance').text().replace(/[^0-9.]/g, ''),
        ChangeReturn: $('#ChangeReturn').text().replace(/[^0-9.]/g, ''),
        HoldReason: $('#txtHoldReason').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        Status: 'Final',
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        OnlinePaymentSettingsId: $('#ddlOnlinePaymentSettings').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        Payments: Payments
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/ValidateStock',
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
                $('#' + type).modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function setName() {
    //if (!$('#txtAddrName').val()) {
    $('#txtAddrName').val($('#txtName').val())
    //}
}

function setMobile() {
    if (!$('#txtAddrMobileNo').val()) {
        $('#txtAddrMobileNo').val($('#txtMobileNo').val())
    }
}

function setAlternativeMobile() {
    if (!$('#txtAddrMobileNo2').val()) {
        $('#txtAddrMobileNo2').val($('#txtAltMobileNo').val())
    }
}

function setEmail() {
    //if (!$('#txtAddrMobileNo2').val()) {
    $('#txtAddrEmailId').val($('#txtEmailId').val());
    //}
}

function toggleShippingAddress() {
    if ($('#chkIsShippingAddressDifferent').is(':checked')) {
        $('#divShippingAddress').show();
    }
    else {
        $('#divShippingAddress').hide();
    }
}

function changeSellingPriceGroup() {
    var hasItems = false;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            hasItems = true;
        }
    });

    if (hasItems == true) {
        var r = confirm("This will clear all items from the cart & you need to add them again. Are you sure you want to continue ?");
        if (r == true) {
            $('#divCombo').empty();
            updateAmount();
            return;
        }
    }
}

$(".add-row").click(function () {
    c = c + 1
    var markup = '<div class="input-group mb-1 divAccountDetails" id="rowNew' + c + '">' +
        '    <input type="text" class="form-control" id="txtLabel' + c + '" placeholder="Label">' +
        '    <input type="text" class="form-control" id="txtValue' + c + '" placeholder="value">' +
        '     <span class="input-group-append">' +
        '       <a href="javascript:void(0)" class="btn btn-danger btn-sm delete-row" id="btnNew' + c + '"><i class="fas fa-minus pt-2"></i></a>' +
        '     </span>' +
        '  </div>';
    $("#addrow1").append(markup);
});

$('#addrow1').on('click', '.delete-row', function () {
    var i = $(this).attr('id');
    $("#row" + i.split('btn')[1]).remove();
});

function insertCurrency(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CurrencyId: $('#ddlCurrency').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/currencyMappingInsert',
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
                $('#ddlUserCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName + ' (' + data.Data.Currency.CurrencyCode + ')' }));
                $('#ddlUserCurrency').val(data.Data.Currency.CurrencyId);

                $('#currencyModal').modal('toggle');

                $('#ddlCurrency').val(0);
                $('#txtExchangeRate').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setCurrencyName() {
    $('#txtCurrency').text($("#ddlCurrency").children("option").filter(":selected").text());
}

function setZoomLevel() {
    var browser = checkBrowser();
    if (browser == "Firefox") {
        toastr.error('Firefox Browser does not support zoom option. Try pressing (ctrl & +) together for zoom in or (ctrl & -) together for zoom out. Please connect with support fo further details')
    }
    else {
        Cookies.set('zoomLevel', $('#ddlZoomLevel').val());
        document.body.style.zoom = $('#ddlZoomLevel').val();
    }
}

function checkBrowser() {
    if ((navigator.userAgent.indexOf("Opera") || navigator.userAgent.indexOf('OPR')) != -1) {
        return 'Opera';
    } else if (navigator.userAgent.indexOf("Edg") != -1) {
        return 'Edge';
    } else if (navigator.userAgent.indexOf("Chrome") != -1) {
        return 'Chrome';
    } else if (navigator.userAgent.indexOf("Safari") != -1) {
        return 'Safari';
    } else if (navigator.userAgent.indexOf("Firefox") != -1) {
        return 'Firefox';
    } else if ((navigator.userAgent.indexOf("MSIE") != -1) || (!!document.documentMode == true)) //IF IE > 10
    {
        return 'IE';
    } else {
        return 'unknown';
    }
}

function openInvoiceCopyModal(invUrl, invNo) {
    $('#lblInvoiceNo').text('Invoice No: ' + invNo);
    $('#lblInvoiceUrl').val(invUrl);

    $('#divInvoiceInputs').html('<input disabled type="text" class="form-control" id="lblInvoiceUrl" value="\'' + invUrl + '\'" style="width:80%">' +
        '<button type="button" class="btn btn-secondary" onclick="copyCode(\'' + invUrl + '\')" style="width:20%">COPY</button>');

    $('#divInvoiceButtons').html('<button type="button" class="btn btn-default" data-dismiss="modal" style="margin-left:auto">Close</button>' +
        '<button type="button" class= "btn btn-primary" onclick="PrintInvoice(\'' + invUrl + '\')" id="btnInvoiceView">View</button>');
    $('#InvoiceCopyModal').modal('toggle');
}

function ViewPayment(CustomerPaymentId) {
    var det = {
        CustomerPaymentId: CustomerPaymentId,
        Type: "Sales Payment",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/PaymentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PaymentViewModal').modal('toggle');
            $("#divPaymentView").html(data);
            $("#divLoading").hide();

            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openNotificationModal(ntype, nid) {
    NotificationName = ntype;
    NotificationId = nid;
    var det = {
        Name: ntype,
        Id: NotificationId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/FetchNotificationModule',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divNotification").html(data);

            $('#NotificationModal').modal('toggle');

            $('textarea.txtEmailBody').summernote({
                placeholder: '',
                followingToolbar: false,
                tabsize: 2,
                height: 200,
                toolbar: [
                    ['style', ['style']],
                    ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
                    ['fontname', ['fontname']],
                    ['fontsize', ['fontsize']],
                    ['color', ['color']],
                    ['para', ['ul', 'ol', 'paragraph']],
                    ['height', ['height']],
                    ['table', ['table']],
                    ['insert', ['link', 'picture', 'hr']],
                    ['view', ['fullscreen', 'codeview']],
                    ['help', ['help']]
                ],
            });

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function SendNotifications() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        AutoSendEmail: $('#chkAutoSendEmail').is(':checked'),
        AutoSendSms: $('#chkAutoSendSms').is(':checked'),
        AutoSendWhatsapp: $('#chkAutoSendWhatsapp').is(':checked'),
        EmailSubject: $('#txtEmailSubject').val(),
        CC: $('#txtCC').val(),
        BCC: $('#txtBCC').val(),
        EmailBody: $('#txtEmailBody').val(),
        SmsBody: $('#txtSmsBody').val(),
        WhatsappBody: $('#txtWhatsappBody').val(),
        Id: NotificationId,
        Name: NotificationName
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/SendNotifications',
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
                $('#NotificationModal').modal('toggle');
                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openReminderModal(ntype, nid) {
    ReminderName = ntype;
    ReminderId = nid;
    var det = {
        ReminderBeforeAfter: ReminderName
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/FetchReminderModule',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divReminder").html(data);

            $('#ReminderModal').modal('toggle');

            $('textarea.txtEmailBody').summernote({
                placeholder: '',
                followingToolbar: false,
                tabsize: 2,
                height: 200,
                toolbar: [
                    ['style', ['style']],
                    ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
                    ['fontname', ['fontname']],
                    ['fontsize', ['fontsize']],
                    ['color', ['color']],
                    ['para', ['ul', 'ol', 'paragraph']],
                    ['height', ['height']],
                    ['table', ['table']],
                    ['insert', ['link', 'picture', 'hr']],
                    ['view', ['fullscreen', 'codeview']],
                    ['help', ['help']]
                ],
            });

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function SendReminders() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        AutoSendEmail: $('#chkAutoSendEmail').is(':checked'),
        AutoSendSms: $('#chkAutoSendSms').is(':checked'),
        AutoSendWhatsapp: $('#chkAutoSendWhatsapp').is(':checked'),
        EmailSubject: $('#txtEmailSubject').val(),
        CC: $('#txtCC').val(),
        BCC: $('#txtBCC').val(),
        EmailBody: $('#txtEmailBody').val(),
        SmsBody: $('#txtSmsBody').val(),
        WhatsappBody: $('#txtWhatsappBody').val(),
        Id: ReminderId,
        ReminderBeforeAfter: ReminderName,
        ReminderTo: $('#ddlReminderTo').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/NotificationSettings/SendReminders',
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
                if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                    window.open(data.WhatsappUrl);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setAvailableTags(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

function openWriteOffModal(SalesId) {
    _SalesId = SalesId;
    $('#WriteOffModal').modal('toggle');
}

function UpdateWriteOff() {
    var det = {
        SalesId: _SalesId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/WriteOffUpdate',
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
};

function CancelWriteOff(SalesId) {
    var r = confirm("Are you sure you want to continue?");
    if (r == true) {
        var det = {
            SalesId: SalesId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/Sales/WriteOffCancel',
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

function toggleReverseCharge() {
    if ($('#chkIsReverseCharge').is(':checked')) {
        $('#lblReverseCharge').show();
    }
    else {
        $('#lblReverseCharge').hide();
    }

    fetchUpdatedItems();
    discallcalc();
}

function ItemsPos(type, id) {
    if (type == 'category') {
        CategoryId = id;
        SubCategoryId = 0;
        SubSubCategoryId = 0;
    }
    else if (type == 'sub category') {
        SubCategoryId = id;
        SubSubCategoryId = 0;
    }
    else if (type == 'sub sub category') {
        SubSubCategoryId = id;
    }
    else {
        BrandId = id;
    }

    var det = {
        BranchId: $('#ddlBranch').val(),
        CategoryId: CategoryId,
        SubCategoryId: SubCategoryId,
        SubSubCategoryId: SubSubCategoryId,
        BrandId: BrandId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/ItemsPos',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialItemsPos").html(data);
            $("#divLoading").hide();

            $('#divPartialItemsPos').show();
            $('#divPartialBrandsPos').hide();
            $('#divPartialCategoriesPos').hide();
            $('#divPartialSubCategoriesPos').hide();
            $('#divPartialSubSubCategoriesPos').hide();
            $('#divCategory_Brand').show();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveBrands() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/BrandsPos',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialBrandsPos").html(data);
            $("#divLoading").hide();

            $('#divPartialItemsPos').hide();
            $('#divPartialBrandsPos').show();
            $('#divPartialCategoriesPos').hide();
            $('#divPartialSubCategoriesPos').hide();
            $('#divPartialSubSubCategoriesPos').hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveCategories() {
    var det = {
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/CategoriesPos',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialCategoriesPos").html(data);
            $("#divLoading").hide();

            $('#divPartialItemsPos').hide();
            $('#divPartialBrandsPos').hide();
            $('#divPartialCategoriesPos').show();
            $('#divPartialSubCategoriesPos').hide();
            $('#divPartialSubSubCategoriesPos').hide();
            $('#divCategory_Brand').hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveSubCategories(id) {
    if (id == 0) {
        id = CategoryId;
    }
    else {
        CategoryId = id;
    }
    var det = {
        CategoryId: id
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SubCategoriesPos',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialSubCategoriesPos").html(data);
            $("#divLoading").hide();

            $('#divPartialItemsPos').hide();
            $('#divPartialBrandsPos').hide();
            $('#divPartialCategoriesPos').hide();
            $('#divPartialSubCategoriesPos').show();
            $('#divPartialSubSubCategoriesPos').hide();
            $('#divCategory_Brand').hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveSubSubCategories(id) {
    var det = {
        SubCategoryId: id
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/SubSubCategoriesPos',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPartialSubSubCategoriesPos").html(data);
            $("#divLoading").hide();

            $('#divPartialItemsPos').hide();
            $('#divPartialBrandsPos').hide();
            $('#divPartialCategoriesPos').hide();
            $('#divPartialSubCategoriesPos').hide();
            $('#divPartialSubSubCategoriesPos').show();
            $('#divCategory_Brand').hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function resetItemsPos() {
    CategoryId = 0;
    SubCategoryId = 0;
    SubSubCategoryId = 0;
    BrandId = 0;

    ItemsPos('', 0);

}

function fetchAdditionalCharges() {
    var IsBusinessRegistered = $('#hdnIsBusinessRegistered').val().toLocaleLowerCase();
    var BusinessRegistrationType = $('#hdnBusinessRegistrationType').val().toLocaleLowerCase();

    if (IsBusinessRegistered == "1" && BusinessRegistrationType == "composition") {
        IsBillOfSupply = true;
    }

    // Store existing values before clearing
    var existingValues = {};
    var hasExistingValues = false;

    // Check if there are any existing values in additional charges
    $('[id^="txtAdditionalChargesAmountExcTax"]').each(function () {
        var index = $(this).attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        var amount = $(this).val();
        var incTax = $('#txtAdditionalChargesAmountIncTax' + index).val();
        var chargeId = $('#txtAdditionalChargeId' + index).val();
        var ddlAdditionalChargesTax = $('#ddlAdditionalChargesTax' + index).val();

        if (amount && parseFloat(amount) > 0) {
            hasExistingValues = true;
            existingValues[index] = {
                amount: amount,
                incTax: incTax,
                chargeId: chargeId,
                ddlAdditionalChargesTax: ddlAdditionalChargesTax
            };
        }
    });

    var det = {
        CustomerId: $('#ddlCustomer').val(),
        BranchId: $('#ddlBranch').val(),
        IsBillOfSupply: IsBillOfSupply
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/OtherSettings/ActiveAdditionalCharges',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var CountryId = $('#hdnCountryId').val();
            var StateId = $('#hdnStateId').val();

            if (data.Data.AdditionalCharges.length == 0) {
                // Only hide additional charges section, not reward points section
                $('.charges-section').not('#divRewardPointsInputSection').hide();
            }
            else {
                // Only show additional charges section, reward points visibility is controlled separately
                $('.charges-section').not('#divRewardPointsInputSection').show();
                var html = '';
                for (let i = 0; i < data.Data.AdditionalCharges.length; i++) {
                    // Check if we have existing values for this charge
                    var existingValue = null;
                    if (hasExistingValues) {
                        // Find existing value by charge ID
                        for (var key in existingValues) {
                            if (existingValues[key].chargeId == data.Data.AdditionalCharges[i].AdditionalChargeId) {
                                existingValue = existingValues[key];
                                break;
                            }
                        }
                    }

                    // Set amount value - preserve existing or use default
                    var amountValue = existingValue ? existingValue.amount : '0';
                    var incTaxValue = existingValue ? existingValue.incTax : '0';

                    var ddlAdditionalChargesTax = '0';
                    if (existingValue) {
                        ddlAdditionalChargesTax = existingValue ? existingValue.ddlAdditionalChargesTax : '0';
                    }
                    else {
                        if (StateId == $('#ddlPlaceOfSupply').val()) {
                            ddlAdditionalChargesTax = data.Data.AdditionalCharges[i].IntraStateTaxId;
                        }
                        else {
                            ddlAdditionalChargesTax = data.Data.AdditionalCharges[i].InterStateTaxId;
                        }
                    }

                    html = html +
                        '<div class="charge-item d-flex justify-content-between align-items-center py-1">' +
                        '<div class="charge-label">' +
                        '<label class="mb-0 text-sm">' +
                        data.Data.AdditionalCharges[i].Name + ':' +
                        '<a href="javascript:void(0)" onclick="openAdditionalChargesModal(' + i + ', \'' + data.Data.AdditionalCharges[i].Name + '\')" class="ml-2 text-primary"> <i class="fa fa-edit"></i></a>' +
                        '</label>' +
                        '</div>' +
                        '<div class="charge-value">' +
                        '<span id="divAdditionalCharges' + i + '" class="font-weight-bold text-primary">' +
                        CurrencySymbol + amountValue +
                        '<i class="fa fa-pencil-square-o cursor-pointer ml-1"></i>' +
                        '</span>' +
                        '<input type="hidden" id="txtAdditionalChargesAmountExcTax' + i + '" value="' + amountValue + '">' +
                        '<input type="hidden" id="txtAdditionalChargesAmountIncTax' + i + '" value="' + incTaxValue + '">' +
                        '<input type="hidden" id="txtAdditionalChargeId' + i + '" value="' + data.Data.AdditionalCharges[i].AdditionalChargeId + '">' +
                        '<input type="hidden" id="hdnIntraStateTaxId' + i + '" value="' + data.Data.AdditionalCharges[i].IntraStateTaxId + '">' +
                        '<input type="hidden" id="hdnInterStateTaxId' + i + '" value="' + data.Data.AdditionalCharges[i].InterStateTaxId + '">' +
                        '<input type="hidden" id="hdnGstTreatment' + i + '" value="' + data.Data.AdditionalCharges[i].GstTreatment + '">' +
                        '<input type="hidden" id="ddlAdditionalChargesTax' + i + '" value="' + ddlAdditionalChargesTax + '">' +
                        '<input type="hidden" id="exemptionReasonId' + i + '" value="0">' +
                        '</div>' +
                        '</div>';
                }

                $('#divAdditionalCharges').empty();
                $('#divAdditionalCharges').html(html);
                $('.select2').select2();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openAdditionalChargesModal(id, name) {
    additionalChargesId = id;
    var IsBusinessRegistered = $('#hdnIsBusinessRegistered').val().toLocaleLowerCase();
    var BusinessRegistrationType = $('#hdnBusinessRegistrationType').val().toLocaleLowerCase();
    var GstTreatment = $('#hdnGstTreatment' + id).val().toLocaleLowerCase();
    var IntraStateTaxId = $('#hdnIntraStateTaxId' + id).val().toLocaleLowerCase();
    var InterStateTaxId = $('#hdnInterStateTaxId' + id).val().toLocaleLowerCase();
    var ddlAdditionalChargesTax = $('#ddlAdditionalChargesTax' + id).val().toLocaleLowerCase();
    var AdditionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + id).val();
    var CountryId = $('#hdnCountryId').val();
    var StateId = $('#hdnStateId').val();

    if (IsBusinessRegistered == "1" && BusinessRegistrationType == "composition") {
        IsBillOfSupply = true;
    }

    if (ddlAdditionalChargesTax != 0) {
        IntraStateTaxId = ddlAdditionalChargesTax;
        InterStateTaxId = ddlAdditionalChargesTax;
    }

    var html = '';

    var ddlTax = '<select class="form-control select2" style="width:100%" id="ddlAdditionalChargesTax" onchange="toggleExemptionDropdown();updateAdditionalChargesCalculation();">';
    for (let ss = 0; ss < taxList.length; ss++) {
        if (taxList[ss].Tax != "Taxable") {
            if (CountryId == 2) {
                if (StateId == $('#ddlPlaceOfSupply').val() &&
                    GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                    && GstTreatment != 'Supply by SEZ Developer') {
                    if (taxList[ss].TaxTypeId != 3) {
                        if (IntraStateTaxId == taxList[ss].TaxId) {
                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                        }
                        else {
                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                        }
                    }
                }
                else {
                    if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                        if (InterStateTaxId == taxList[ss].TaxId) {
                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                        }
                        else {
                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                        }
                    }
                }
            }
            else {
                if (data.Data.AdditionalCharges[i].TaxId == taxList[ss].TaxId) {
                    ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                }
                else {
                    ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                }
            }
        }
    }
    ddlTax = ddlTax + '</select>';

    var ddlTaxExemption = '<select class="form-control style="width:100%" select2" id="ddlAdditionalChargesTaxExemption">';
    for (let ss = 0; ss < taxExemptions.length; ss++) {
        ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
    }
    ddlTaxExemption = ddlTaxExemption + '</select>';

    html = html +
        '<div class="col-md-4">' +
        '<div class="">' +
        '<label for="payment_note_1">' + name + '</label>' +
        '<input type="number" class="form-control" id="txtAdditionalCharge" value="' + AdditionalChargesAmountExcTax + '" onchange="updateAdditionalChargesCalculation()">' +
        '</div>' +
        '</div>' +
        '<div class="col-md-4">' +
        '<div class="divAdditionalChargesTax">' +
        '<label for="payment_note_1">Tax</label>' +
        ddlTax +
        '</div>' +
        '</div>' +
        '<div class="col-md-4 hidden" id="divTaxExemption">' +
        '<div class="">' +
        '<label for="payment_note_1">Exemption Reason</label>' +
        ddlTaxExemption +
        '</div>' +
        '</div> ';

    $('#divAdditionalCharge').empty();
    $('#divAdditionalCharge').html(html);
    $('.select2').select2();

    $('.additional-charges-modal-title').text(name);

    if (IsBillOfSupply == true) {
        $('.divBillOfSupply').hide();
    }
    $('#additionalChargesModal').modal('toggle');
}

function toggleExemptionDropdown() {
    var tax = $('#ddlAdditionalChargesTax option:selected').text();
    if (tax == 'Non-Taxable') {
        $('#divTaxExemption').show();
    } else {
        $('#divTaxExemption').hide();
    }
}

function updateAdditionalChargesCalculation() {
    var amount = parseFloat($('#txtAdditionalCharge').val());
    var tax = $('#ddlAdditionalChargesTax').val().split('-')[1];
    var incTax = ((tax / 100) * amount) + amount;
    $('#txtAdditionalChargesAmountExcTax' + additionalChargesId).val(amount);
    $('#txtAdditionalChargesAmountIncTax' + additionalChargesId).val(incTax);
    $('#divAdditionalCharges' + additionalChargesId).text(CurrencySymbol + amount);

    var taxId = $('#ddlAdditionalChargesTax').val().split('-')[0];
    $('#ddlAdditionalChargesTax' + additionalChargesId).val(taxId);

    var taxExemptionid = $('#ddlAdditionalChargesTaxExemption').val();
    $('#exemptionReasonId' + additionalChargesId).val(taxExemptionid);
    discallcalc();
}

function fetchUpdatedItems() {
    //$('#divCombo').empty();
    $.each(skuCodes, function (index, value) {
        fetchItemTax(value);
    });
    fetchAdditionalCharges();

}

function fetchItemTax(SkuHsnCode) {
    if (window.location.pathname.toLowerCase().indexOf('billofsupply') != -1) {
        IsBillOfSupply = true;
    }
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
        CustomerId: $('#ddlCustomer').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        MenuType: 'sale',
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        Type: "Sales",
        IsBillOfSupply: IsBillOfSupply
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
            //var z = count;
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {

                $('#divCombo tr').each(function () {
                    var _id = this.id.split('divCombo')[1];
                    if (_id != undefined) {
                        var _skuCode = $('#txtSkuCode' + _id).val();
                        if (_skuCode.toLowerCase().trim() == SkuHsnCode.toLowerCase().trim()) {
                            var count = _id;

                            //let taxamt = (Math.round((parseFloat(data.Data.ItemDetails[i].SalesIncTax) - parseFloat(data.Data.ItemDetails[i].SalesExcTax)) * 100) / 100);
                            var ddlTax = '<select class="form-control ddlTax" style="min-width:80px" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')">';
                            for (let ss = 0; ss < taxList.length; ss++) {
                                if (taxList[ss].Tax != "Taxable") {
                                    if (data.Data.BusinessSetting.CountryId == 2) {
                                        if (data.Data.BusinessSetting.StateId == $('#ddlPlaceOfSupply').val() &&
                                            data.Data.BusinessSetting.GstTreatment != 'Export of Goods / Services (Zero-Rated Supply)' && data.Data.BusinessSetting.GstTreatment != 'Supply to SEZ Unit (Zero-Rated Supply)'
                                            && data.Data.BusinessSetting.GstTreatment != 'Supply by SEZ Developer') {
                                            if (taxList[ss].TaxTypeId != 3) {
                                                if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                                    ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                }
                                                else {
                                                    ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                }
                                            }
                                        }
                                        else {
                                            if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                                if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                                    ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                }
                                                else {
                                                    ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                                }
                                            }
                                        }
                                    }
                                    else {
                                        if (data.Data.ItemDetails[i].TaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                            }
                            ddlTax = ddlTax + '</select>';

                            var ddlTaxExemption = '<select class="form-control select2" id="ddlTaxExemption' + count + '"><option selected value="0">None</option>';
                            for (let ss = 0; ss < taxExemptions.length; ss++) {
                                if (data.Data.ItemDetails[i].TaxExemptionId == taxExemptions[ss].TaxExemptionId) {
                                    ddlTaxExemption = ddlTaxExemption + '<option selected value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                                }
                                else {
                                    ddlTaxExemption = ddlTaxExemption + '<option value="' + taxExemptions[ss].TaxExemptionId + '">' + taxExemptions[ss].Reason + '</option>';
                                }
                            }
                            ddlTaxExemption = ddlTaxExemption + '</select >';

                            $('#tdTax' + count).empty();
                            $('#tdTax' + count).html(
                                '<div class="form-group">' +
                                '<label for="payment_note_1">Tax</label>' +
                                '<div class="input-group">' +
                                ddlTax +
                                '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="0" style="min-width:80px">' +
                                '<input hidden type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="0" style="min-width:80px">' +
                                '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                                '<div id="divTaxExemption' + count + '" class="form-group" style="width:100%;display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                                '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                                '<div class="input-group">' +
                                ddlTaxExemption +
                                '</div>' +
                                '</div>' +
                                '</div >' +
                                '</div>' +
                                '</div >'
                            );
                            /*'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +*/

                            ChangeQtyAmount(count);
                        }
                    }
                });
            }
            $('.select2').select2();

            if (IsBillOfSupply == true) {
                $('.divBillOfSupply').hide();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleTaxPreference() {
    $('.divNonTaxable').hide();
    $('#ddlTaxExemption').val(0);

    if ($('#ddlTaxPreference option:selected').text() == 'Non-Taxable') {
        $('.divNonTaxable').show();
    }

    $('.select2').select2();
}

// Restaurant/KOT Integration Functions
function loadTables() {
    if ($('#ddlTableId').length == 0) return;
    
    var obj = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt($('#ddlBranch').val()),
        PageIndex: 1,
        PageSize: 1000
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/Sales/GetTablesForPos',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 1 && data.Data && data.Data.Tables) {
                $('#ddlTableId').empty().append('<option value="0">Select Table</option>');
                $.each(data.Data.Tables, function (index, table) {
                    var statusText = table.Status || 'Available';
                    var statusClass = '';
                    if (statusText == 'Available') statusClass = 'text-success';
                    else if (statusText == 'Occupied') statusClass = 'text-danger';
                    else if (statusText == 'Reserved' || statusText == 'Booked') statusClass = 'text-warning';
                    else if (statusText == 'Maintenance') statusClass = 'text-secondary';
                    
                    $('#ddlTableId').append('<option value="' + table.TableId + '" data-status="' + statusText + '" class="' + statusClass + '">' + 
                        table.TableNo + (table.TableName ? ' - ' + table.TableName : '') + 
                        ' (Capacity: ' + table.Capacity + ')' + 
                        ' [' + statusText + ']</option>');
                });
                $('#ddlTableId').select2();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function onTableSelected() {
    var tableId = $('#ddlTableId').val();
    $('#hdnBookingId').val('0');
    $('#hdnKotId').val('0');
    $('#divTableStatus').text('');
    $('#divTableBooking').hide();
    
    if (tableId == '0' || !tableId) {
        return;
    }
    
    // Get table status
    var selectedOption = $('#ddlTableId option:selected');
    var status = selectedOption.data('status') || 'Available';
    var statusClass = '';
    if (status == 'Available') statusClass = 'text-success';
    else if (status == 'Occupied') statusClass = 'text-danger';
    else if (status == 'Reserved' || status == 'Booked') statusClass = 'text-warning';
    else if (status == 'Maintenance') statusClass = 'text-secondary';
    
    $('#divTableStatus').html('<span class="' + statusClass + '">Status: ' + status + '</span>');
    
    // Check for active booking
    checkTableBooking(tableId);
    
    // Check for existing standalone KOTs
    if ($('#chkPosAllowLinkExistingKot').length > 0 && $('#chkPosAllowLinkExistingKot').is(':checked')) {
        loadExistingKots(tableId);
    }
}

function checkTableBooking(tableId) {
    var obj = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt($('#ddlBranch').val()),
        TableId: parseInt(tableId),
        Status: 'Confirmed',
        PageIndex: 1,
        PageSize: 10
    };
    
    $.ajax({
        url: '/Sales/GetBookingsForPos',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (data) {
            if (data.Status == 1 && data.Data && data.Data.Bookings && data.Data.Bookings.length > 0) {
                var booking = data.Data.Bookings[0];
                $('#hdnBookingId').val(booking.BookingId);
                var bookingDate = new Date(booking.BookingDate);
                var bookingTime = booking.BookingTime;
                $('#divTableBooking').html('<small class="text-info"><i class="fas fa-calendar"></i> Booking: ' + 
                    booking.BookingNo + ' - ' + booking.CustomerName + 
                    ' (' + bookingDate.toLocaleDateString() + ' ' + bookingTime + ')</small>').show();
            }
        },
        error: function (xhr) {
            // Silent fail
        }
    });
}

function loadExistingKots(tableId) {
    var obj = {
        CompanyId: parseInt(Cookies.get('data').split('&')[3].split('=')[1]),
        BranchId: parseInt($('#ddlBranch').val()),
        TableId: parseInt(tableId),
        WithSales: false,
        OrderStatus: 'Pending',
        PageIndex: 1,
        PageSize: 10
    };
    
    $.ajax({
        url: '/Sales/GetStandaloneKotsForPos',
        datatype: "json",
        data: obj,
        type: "post",
        success: function (data) {
            if (data.Status == 1 && data.Data && data.Data.Kots && data.Data.Kots.length > 0) {
                // Show option to link existing KOT
                var kot = data.Data.Kots[0];
                $('#hdnKotId').val(kot.KotId);
                $('#divTableBooking').append('<br><small class="text-warning"><i class="fas fa-receipt"></i> Existing KOT: ' + 
                    kot.KotNo + ' - Will be linked to this order</small>');
            }
        },
        error: function (xhr) {
            // Silent fail
        }
    });
}

// Reward Points Functions
var rewardPointsSetting = null;
var availableRewardPoints = 0;

function fetchCustomerRewardPoints() {
    // Note: divRewardPointsInputSection is conditionally rendered in the view based on ViewBag.RewardPointSetting
    // If it exists in the DOM, reward points are enabled (view already checked the condition)
    // We should NOT hide/show it programmatically - the view controls its visibility
    var inputSectionExists = $('#divRewardPointsInputSection').length > 0;
    var pointsSectionExists = $('#divRewardPointsSection').length > 0;
    
    // If sections don't exist in DOM, reward points are disabled (view didn't render them)
    // Simply return - no need to hide anything since they were never rendered
    if (!inputSectionExists && !pointsSectionExists) {
        return;
    }
    
    // Sections exist in DOM - reward points are enabled (view already checked)
    // Try to get setting from ViewBag, or create temporary one from DOM
    if (typeof ViewBagRewardPointSetting !== 'undefined' && ViewBagRewardPointSetting && ViewBagRewardPointSetting.EnableRewardPoint) {
        rewardPointsSetting = ViewBagRewardPointSetting;
    } else {
        // Create temporary setting from DOM elements (fallback if ViewBag not available yet)
        rewardPointsSetting = { 
            DisplayName: $('#rewardPointsDisplayNameHeader').text() || 'Points', 
            EnableRewardPoint: true 
        };
    }

    // Update display names if rewardPointsSetting is available
    if (rewardPointsSetting && rewardPointsSetting.DisplayName) {
        $('#rewardPointsDisplayName').text(rewardPointsSetting.DisplayName);
        $('#rewardPointsDisplayNameHeader').text(rewardPointsSetting.DisplayName);
    }

    var customerId = $('#ddlCustomer').val();
    if (!customerId || customerId == 0) {
        $('#txtAvailablePoints').val('0');
        availableRewardPoints = 0;
        $('#txtRedeemPoints').val('');
        return;
    }

    var det = {
        CustomerId: customerId,
        CompanyId: Cookies.get('data').split('&')[3].split('=')[1]
    };

    $.ajax({
        url: '/customers/GetCustomerRewardPoints',
        dataType: "json",
        data: det,
        type: "post",
        success: function (data) {
            try {
                // Handle the response structure - Data might be directly available or nested
                var availablePoints = 0;
                if (data && data.Status == 1) {
                    // Check if Data.CustomerRewardPointsData exists (for dynamic data)
                    if (data.Data && data.Data.CustomerRewardPointsData && data.Data.CustomerRewardPointsData.AvailablePoints !== undefined) {
                        availablePoints = parseFloat(data.Data.CustomerRewardPointsData.AvailablePoints) || 0;
                    }
                    // Check if Data.AvailablePoints exists directly
                    else if (data.Data && data.Data.AvailablePoints !== undefined) {
                        availablePoints = parseFloat(data.Data.AvailablePoints) || 0;
                    }
                    // Try accessing via any property that might contain the data
                    else if (data.Data && typeof data.Data === 'object') {
                        // The API returns an anonymous object, which might be preserved in a dynamic property
                        for (var key in data.Data) {
                            if (data.Data[key] && typeof data.Data[key] === 'object' && data.Data[key].AvailablePoints !== undefined) {
                                availablePoints = parseFloat(data.Data[key].AvailablePoints) || 0;
                                break;
                            }
                        }
                    }
                }
                
                // In edit mode, exclude points earned from the current sale
                // This prevents redeeming points that were earned from the same transaction
                if (typeof CurrentSaleInfo !== 'undefined' && CurrentSaleInfo && CurrentSaleInfo.SalesId > 0 && CurrentSaleInfo.PointsEarned > 0) {
                    // Subtract points earned from this sale from available points
                    availablePoints = Math.max(0, availablePoints - CurrentSaleInfo.PointsEarned);
                }
                
                availableRewardPoints = availablePoints;
                
                // Display available points (base amount before any redemption in this transaction)
                // Note: If user redeems points, validation in calculatePointsDiscount will ensure
                // they don't exceed availableRewardPoints
                $('#txtAvailablePoints').val(availableRewardPoints.toFixed(0));
                calculatePointsDiscount();
            } catch (e) {
                availableRewardPoints = 0;
                $('#txtAvailablePoints').val('0');
                calculatePointsDiscount();
            }
        },
        error: function (xhr, status, error) {
            availableRewardPoints = 0;
            $('#txtAvailablePoints').val('0');
            calculatePointsDiscount();
        }
    });
}

function calculatePointsDiscount() {
    if (!rewardPointsSetting || !rewardPointsSetting.EnableRewardPoint) {
        $('#divPointsDiscount').text(CurrencySymbol + '0.00');
        $('#hdnRedeemPoints').val(0);
        return;
    }

    var redeemPoints = parseFloat($('#txtRedeemPoints').val()) || 0;
    var netAmount = parseFloat($('#divNetAmount').text().replace(CurrencySymbol, '').replace(/,/g, '')) || 0;
    
    // Validation
    if (redeemPoints < 0) {
        redeemPoints = 0;
        $('#txtRedeemPoints').val(0);
    }

    // Check minimum order total
    if (rewardPointsSetting.MinimumOrderTotalToRedeemPoints > 0 && netAmount < rewardPointsSetting.MinimumOrderTotalToRedeemPoints) {
        $('#pointsDiscountInfo').text('Minimum order total of ' + CurrencySymbol + rewardPointsSetting.MinimumOrderTotalToRedeemPoints + ' required to redeem points').addClass('text-danger');
        redeemPoints = 0;
        $('#txtRedeemPoints').val(0);
    } else {
        $('#pointsDiscountInfo').text('').removeClass('text-danger');
    }

    // Check minimum points
    if (redeemPoints > 0 && rewardPointsSetting.MinimumRedeemPoint > 0 && redeemPoints < rewardPointsSetting.MinimumRedeemPoint) {
        $('#pointsDiscountInfo').text('Minimum ' + rewardPointsSetting.MinimumRedeemPoint + ' points required to redeem').addClass('text-danger');
        redeemPoints = 0;
        $('#txtRedeemPoints').val(0);
    }

    // Check maximum points per order
    if (redeemPoints > 0 && rewardPointsSetting.MaximumRedeemPointPerOrder > 0 && redeemPoints > rewardPointsSetting.MaximumRedeemPointPerOrder) {
        $('#pointsDiscountInfo').text('Maximum ' + rewardPointsSetting.MaximumRedeemPointPerOrder + ' points allowed per order').addClass('text-danger');
        redeemPoints = rewardPointsSetting.MaximumRedeemPointPerOrder;
        $('#txtRedeemPoints').val(redeemPoints);
    }

    // Check available points (validate against base available points)
    if (redeemPoints > availableRewardPoints) {
        $('#pointsDiscountInfo').text('Only ' + availableRewardPoints + ' points available').addClass('text-danger');
        redeemPoints = availableRewardPoints;
        $('#txtRedeemPoints').val(redeemPoints);
    }

    // Calculate discount
    var pointsDiscount = 0;
    if (redeemPoints > 0 && rewardPointsSetting.RedeemAmountPerUnitPoint > 0) {
        pointsDiscount = redeemPoints * rewardPointsSetting.RedeemAmountPerUnitPoint;
    }

    $('#divPointsDiscount').text(CurrencySymbol + pointsDiscount.toFixed(2));
    $('#hdnRedeemPoints').val(redeemPoints);
    
    // Recalculate totals (this will call updatePointsEarnedInfo via updatePointsEarnedInfoOnTotalChange)
    discallcalc();
}

// Call updatePointsEarnedInfo when totals change
var updatePointsEarnedInfoTimeout = null;
var pointsEarnedAjaxRequest = null; // Store AJAX request to allow cancellation
function updatePointsEarnedInfoOnTotalChange() {
    if (typeof updatePointsEarnedInfo === 'function') {
        // Debounce to prevent multiple rapid calls - increased to 500ms for better performance
        clearTimeout(updatePointsEarnedInfoTimeout);
        updatePointsEarnedInfoTimeout = setTimeout(function() {
            updatePointsEarnedInfo();
        }, 500); // Increased from 100ms to 500ms to reduce excessive calls
    }
}

var isUpdatingPointsEarned = false;
function updatePointsEarnedInfo() {
    if (!rewardPointsSetting || !rewardPointsSetting.EnableRewardPoint) {
        $('#divPointsEarnedInfo').hide();
        return;
    }

    // Cancel any pending AJAX request before starting a new one
    if (pointsEarnedAjaxRequest && pointsEarnedAjaxRequest.readyState !== 4) {
        pointsEarnedAjaxRequest.abort();
        pointsEarnedAjaxRequest = null;
    }

    // Prevent multiple simultaneous calls
    if (isUpdatingPointsEarned) {
        return;
    }
    isUpdatingPointsEarned = true;

    var grandTotal = parseFloat($('#divGrandTotal').text().replace(CurrencySymbol, '').replace(/,/g, '')) || 0;
    var pointsDiscount = parseFloat($('#divPointsDiscount').text().replace(CurrencySymbol, '').replace(/,/g, '')) || 0;
    var orderAmountForPoints = grandTotal + pointsDiscount; // Amount before points discount (Grand Total + points discount)

    // Check minimum order total to earn
    if (rewardPointsSetting.MinOrderTotalToEarnReward > 0 && orderAmountForPoints < rewardPointsSetting.MinOrderTotalToEarnReward) {
        $('#divPointsEarnedInfo').hide();
        isUpdatingPointsEarned = false;
        return;
    }

    // Use controller to calculate points (handles tier-based calculation)
    // Fixed: Changed from [3] to [4] to match Sales.js
    var cookieData = Cookies.get('data');
    var companyId = cookieData ? cookieData.split('&')[4].split('=')[1] : null;
    
    if (!companyId) {
        isUpdatingPointsEarned = false;
        // Fallback to local calculation
        var pointsEarned = 0;
        if (rewardPointsSetting.AmountSpentForUnitPoint > 0) {
            pointsEarned = Math.floor(orderAmountForPoints / rewardPointsSetting.AmountSpentForUnitPoint);
        }
        if (rewardPointsSetting.MaxPointsPerOrder > 0 && pointsEarned > rewardPointsSetting.MaxPointsPerOrder) {
            pointsEarned = rewardPointsSetting.MaxPointsPerOrder;
        }
        if (pointsEarned > 0) {
            $('#pointsEarned').text(pointsEarned.toFixed(0));
            $('#divPointsEarnedInfo').show();
        } else {
            $('#divPointsEarnedInfo').hide();
        }
        return;
    }
    
    pointsEarnedAjaxRequest = $.ajax({
        url: '/customers/CalculatePointsEarned',
        type: "POST",
        data: { CompanyId: parseInt(companyId), OrderAmount: orderAmountForPoints },
        dataType: "json",
        success: function (data) {
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                isUpdatingPointsEarned = false;
                pointsEarnedAjaxRequest = null;
                return;
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                isUpdatingPointsEarned = false;
                pointsEarnedAjaxRequest = null;
                return;
            }

            if (data.Status == 1 && data.Data) {
                var pointsEarned = parseFloat(data.Data.PointsEarned) || 0;
                if (pointsEarned > 0) {
                    $('#pointsEarned').text(pointsEarned.toFixed(0));
                    $('#divPointsEarnedInfo').show();
                } else {
                    $('#divPointsEarnedInfo').hide();
                }
            } else {
                $('#divPointsEarnedInfo').hide();
            }
            isUpdatingPointsEarned = false;
            pointsEarnedAjaxRequest = null;
        },
        error: function (xhr, status, error) {
            // Don't log error if request was aborted
            if (status === 'abort') {
                isUpdatingPointsEarned = false;
                pointsEarnedAjaxRequest = null;
                return;
            }
            
            // Fallback to simple calculation if API fails
            var pointsEarned = 0;
            if (rewardPointsSetting.AmountSpentForUnitPoint > 0) {
                pointsEarned = Math.floor(orderAmountForPoints / rewardPointsSetting.AmountSpentForUnitPoint);
            }
            if (rewardPointsSetting.MaxPointsPerOrder > 0 && pointsEarned > rewardPointsSetting.MaxPointsPerOrder) {
                pointsEarned = rewardPointsSetting.MaxPointsPerOrder;
            }
            if (pointsEarned > 0) {
                $('#pointsEarned').text(pointsEarned.toFixed(0));
                $('#divPointsEarnedInfo').show();
            } else {
                $('#divPointsEarnedInfo').hide();
            }
            isUpdatingPointsEarned = false;
            pointsEarnedAjaxRequest = null;
        }
    });
}