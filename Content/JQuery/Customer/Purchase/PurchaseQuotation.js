$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $('textarea#txtDescription').summernote({
        placeholder: '',
        tabsize: 2,
        height: 100,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            // ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            //['fontname', ['fontname']],
            // ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            //['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });

    init();
    fetchVariations();
    fetchActiveAccountsDropdown();

    var ExpiryDateFormat = Cookies.get('ItemSetting').split('&')[0].split('=')[1];
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('#txtDateRange').daterangepicker({
        //timePicker: true,
        //timePickerIncrement: 1,
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_PurchaseQuotationDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat });

    $('#_PurchaseDate').addClass('notranslate');
    $('#_PaymentDate').addClass('notranslate');

    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date() });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');

    $('._ManufacturingDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
    $('._ExpiryDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
    //$('._ManufacturingDate').addClass('notranslate');
    //$('._ExpiryDate').addClass('notranslate');

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    //if (window.location.href.indexOf('edit') == -1) {
    //    //init();
    //}


    if (window.location.href.indexOf('add') != -1) {
        fetchAdditionalCharges();
    }

    fetchTax();

    //fetchItemCodes();

    fetchTaxExemptions();

    fetchCompanyCurrency();

});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];
var ShowHelpText = Cookies.get('SystemSetting').split('&')[0].split('=')[1];

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

var _PageIndex = 1, c = 1, _variationId = 0, taxModalId;
var ProductImage = "";
var fileExtensionProductImage = "";
var ProductBrochure = "";
var fileExtensionProductBrochure = "";
var AttachDocument = "";
var FileExtensionAttachDocument = "";
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";
var count = 1; var innerCount = 1; var dropdownHtml = '';
var _PurchaseQuotationId = 0, _CheckStockPriceMismatch = true, _i, taxList;
var _BranchId = 0;
var accountSubTypes;
var NotificationName = '', NotificationId = 0;
var skuCodes = [];
var taxExemptions = [];

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        Search: $('#txtSearch').val(),
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        Status: $('#ddlStatus').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/purchasequotationFetch',
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

function View(PurchaseQuotationId) {
    var det = {
        PurchaseQuotationId: PurchaseQuotationId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/PurchasequotationView',
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

            //$("#divLoading").hide();
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
            //$("#divLoading").hide();
        }
    });
}

function fetchItemCodes() {
    var det = {
        ItemCodeType: $('#ddlItemType').val() == 'Product' ? "HSN" : "SAC"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/othersettings/ActiveItemCodes',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            //$("#p_ItemCodes_Dropdown").html(data);

            $("#ddlItemCode").empty();
            $("#ddlItemCode").append($("<option/>").val(0).text('Select'));
            for (let i = 0; i < data.Data.ItemCodes.length; i++) {
                $("#ddlItemCode").append($("<option/>").val(data.Data.ItemCodes[i].ItemCodeId).text(data.Data.ItemCodes[i].Code));
            }


            checkItemType();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function checkItemType() {
    if ($('#ddlItemType').val() == 'Product') {
        $('#lblSkuCode').html('Sku Code / Product Code <i tabindex="0" class="fa fa-info-circle text-info hover-q no-print ' + (ShowHelpText == "True" ? "" : "hidden") + '" role="button" data-toggle="popover" data-trigger="focus" title="" data-content="Leave blank to auto generate"></i>');
        $('#lblHsnCode').text('HSN Code');
        $('#lblProductName').html('Product Name <span class="danger">*</span>');
        $('#lblProductImage').text('Product Image');
        $('#lblProductBrochure').text('Product Brochure');
        $('#lblProductDescription').text('Product Description');
        $('.service').show();
        $('.divSecondaryUnit').hide();
        $('.divTertiaryUnit').hide();
        $('.divQuaternaryUnit').hide();
        $('#chkEnableSecondaryUnit').prop('checked', false);
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#chkEnableQuaternaryUnit').prop('checked', false);
        $('#chkIsManageStock').prop('checked', true);
        $('#divAlertQuantity').show();
    }
    else {
        $('#lblSkuCode').html('Sku Code / Service Code <i tabindex="0" class="fa fa-info-circle text-info hover-q no-print ' + (ShowHelpText == "True" ? "" : "hidden") + '" role="button" data-toggle="popover" data-trigger="focus" title="" data-content="Leave blank to auto generate"></i>');
        $('#lblHsnCode').text('SAC Code');
        $('#lblProductName').html('Service Name <span class="danger">*</span>');
        $('#lblProductImage').text('Service Image');
        $('#lblProductBrochure').text('Service Brochure');
        $('#lblProductDescription').text('Service Description');
        $('.service').hide();
        $('#divExpiresIn').hide();
        $('#chkIsManageStock').prop('checked', false);
        $('#divAlertQuantity').hide();
    }

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $('.select2').select2();
}

function fetchTaxExemptions() {
    var det = {
        TaxExemptionType: "item"
    };
    //$("#divLoading").show();
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
        error: function (xhr) {

        }
    });
};

function fetchItem(SkuHsnCode) {
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val()
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
            for (let i = 0; i < data.Data.ItemDetails.length; i++) {

                var isPresent = false;

                if (isPresent == false) {
                    let taxamt = Math.round(((parseFloat(data.Data.ItemDetails[i].PurchaseIncTax) - parseFloat(data.Data.ItemDetails[i].PurchaseExcTax)) * 100) / 100);

                    var variation = '';
                    if (data.Data.ItemDetails[i].VariationName) {
                        variation = '</br> Variation : ' + data.Data.ItemDetails[i].VariationName;
                    }
                    var ddlUnit = '<select class="form-control ' + (data.Data.ItemDetails[i].UnitId != 0 ? '' : 'hidden') + '" id="ddlUnit' + count + '" onchange="toggleUnit(' + count + ')">';

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

                    //var ddlFreeUnit = '<select disabled class="form-control select2" id="ddlFreeUnit' + count + '" style="min-width: 100px;">';

                    //if (data.Data.ItemDetails[i].QuaternaryUnitId != 0) {
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 1) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].QuaternaryUnitId + '">' + data.Data.ItemDetails[i].QuaternaryUnitShortName + '</option>';
                    //    }
                    //}
                    //else if (data.Data.ItemDetails[i].TertiaryUnitId != 0) {
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 2) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].TertiaryUnitId + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].TertiaryUnitId + '">' + data.Data.ItemDetails[i].TertiaryUnitShortName + '</option>';
                    //    }
                    //}
                    //else if (data.Data.ItemDetails[i].SecondaryUnitId != 0) {
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 3) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    }
                    //    if (data.Data.ItemDetails[i].PriceAddedFor == 4) {
                    //        ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //    else {
                    //        ddlFreeUnit = ddlFreeUnit + '<option value="' + data.Data.ItemDetails[i].SecondaryUnitId + '">' + data.Data.ItemDetails[i].SecondaryUnitShortName + '</option>';
                    //    }
                    //}
                    //else {
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '" hidden>' + data.Data.ItemDetails[i].UnitShortName + '</option>';
                    //    ddlFreeUnit = ddlFreeUnit + '<option selected value="' + data.Data.ItemDetails[i].UnitId + '">' + data.Data.ItemDetails[i].UnitShortName + '</option>';

                    //}

                    //ddlFreeUnit = ddlFreeUnit + '</select >';


                    var ddlTax = '<select class="form-control ddlTax" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')" style="min-width:80px">';
                    for (let ss = 0; ss < taxList.length; ss++) {
                        var canAdd = true;
                        if (taxList[ss].Tax != "Taxable") {
                            if ($('#chkIsReverseCharge').is(':checked')) {
                                if (taxList[ss].Tax == "Non-Taxable") {
                                    canAdd = false;
                                }
                            }
                            if (canAdd == true) {
                                if (data.Data.BusinessSetting.CountryId == 2) {
                                    if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
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
                                            if (data.Data.ItemDetails[i].InterStateTaxId == taxList[ss].TaxId) {
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
                    }
                    ddlTax = ddlTax + '</select >';

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

                    //var ddlItemCode = '<select class="form-control select2" id="ddlItemCode' + count + '"><option selected value="0">None</option>';
                    //for (let ss = 0; ss < itemCodes.length; ss++) {
                    //    if (data.Data.ItemDetails[i].ItemType == 'Product') {
                    //        if (itemCodes[ss].ItemCodeType == "HSN") {
                    //            if (data.Data.ItemDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
                    //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                    //            }
                    //            else {
                    //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                    //            }
                    //        }
                    //    }
                    //    else {
                    //        if (itemCodes[ss].ItemCodeType == "SAC") {
                    //            if (data.Data.ItemDetails[i].ItemCodeId == itemCodes[ss].ItemCodeId) {
                    //                ddlItemCode = ddlItemCode + '<option selected value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                    //            }
                    //            else {
                    //                ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                    //            }
                    //        }
                    //    }
                    //}
                    //ddlItemCode = ddlItemCode + '</select >';

                    var EnableEditingProductPrice = $('#hdnEnableEditingProductPrice').val().toLocaleLowerCase();
                    var EnableLotNo = $('#hdnEnableLotNo').val().toLocaleLowerCase();
                    var EnableItemExpiry = $('#hdnEnableItemExpiry').val().toLocaleLowerCase();
                    var EnableFreeQuantity = $('#hdnEnableFreeQuantity').val().toLocaleLowerCase();
                    var ExpiryType = $('#hdnExpiryType').val();
                    var EnableMrp = $('#hdnEnableMrp').val().toLocaleLowerCase();

                    var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());
                    html = html + '<tr id="divCombo' + count + '">' +
                        /*'<td>' + count + '</td>' +*/
                        '<td style="min-width:100px; white-space: nowrap;">' +
                        '<input type="text" hidden class="form-control" id="txtItemId' + count + '" value="' + data.Data.ItemDetails[i].ItemId + '">' +
                        '<input type="text" hidden class="form-control" id="txtSkuCode' + count + '" value="' + data.Data.ItemDetails[i].SKU + '">' +
                        '<input type="text" hidden class="form-control" id="txtItemDetailsId' + count + '" value="' + data.Data.ItemDetails[i].ItemDetailsId + '">' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length > 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName.substring(0, 15) + '...</span>' +
                        '<span class="' + (data.Data.ItemDetails[i].ItemName.length <= 15 ? '' : 'hidden') + '"><b>Name :</b> ' + data.Data.ItemDetails[i].ItemName + '</span>' +
                        variation +
                        ' </br> <b>SKU :</b> ' + data.Data.ItemDetails[i].SKU + '' +
                        //'<div class="form-group" style="display:' + (data.Data.BusinessSetting.CountryId == 2 ? '' : 'none') + '">' +
                        //'<label style="margin-bottom:0;margin-top:0.5rem;">' + (data.Data.ItemDetails[i].ItemType == 'Product' ? 'HSN Code' : 'SAC Code') + ' <span class="danger">*</span></label>' +
                        //'<div class="input-group">' +
                        //ddlItemCode +
                        //'</div>' +
                        //'</div>' +
                        '</td>' +
                        //'<td style="min-width:100px"> ' +
                        //'<div class="input-group" style="min-width:180px">' +
                        //ddlUnit +
                        //'</div >' +
                        //'<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                        //'<input type="hidden" id="hdnUnitCost' + count + '"  value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnMrp' + count + '" value="' + data.Data.ItemDetails[i].DefaultMrp + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                        //'<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                        //'</td>' +
                        '<td> ' +
                        '<div class="input-group" style="min-width:150px">' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control divQuantity' + count + '_ctrl" value="1" id="txtQuantity' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                        ddlUnit +
                        '<input type="text" hidden class="form-control" id="hdnPriceAddedFor' + count + '" value="' + data.Data.ItemDetails[i].PriceAddedFor + '">' +
                        '<input type="text" hidden class="form-control" id="hdnUToSValue' + count + '" value="' + data.Data.ItemDetails[i].UToSValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSToTValue' + count + '" value="' + data.Data.ItemDetails[i].SToTValue + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTToQValue' + count + '" value="' + data.Data.ItemDetails[i].TToQValue + '">' +
                        '<input type="hidden" id="hdnUnitCost' + count + '"  value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSalesExcTax' + count + '" value="' + data.Data.ItemDetails[i].SalesExcTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSalesIncTax' + count + '" value="' + data.Data.ItemDetails[i].SalesIncTax + '">' +
                        '<input type="text" hidden class="form-control" id="hdnMrp' + count + '" value="' + data.Data.ItemDetails[i].DefaultMrp + '">' +
                        '<input type="text" hidden class="form-control" id="hdnAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].AllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnSecondaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].SecondaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnTertiaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].TertiaryUnitAllowDecimal + '">' +
                        '<input type="text" hidden class="form-control" id="hdnQuaternaryUnitAllowDecimal' + count + '" value="' + data.Data.ItemDetails[i].QuaternaryUnitAllowDecimal + '">' +
                        '</div >' +
                        /*'<small class="text-red font-weight-bold errorText" id="divQuantity' + count + '"></small>' +*/
                        '</td>' +
                        //'<td style="min-width:100px">' +
                        //'<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="1" id="txtQuantity' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                        //'</td>' +
                        '<td style="min-width:100px" class="' + (EnableFreeQuantity == 'true' ? '' : 'hidden') + '"> ' +
                        '<div class="input-group">' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" value="0" id="txtFreeQuantity' + count + '" onchange="ChangeQtyAmount(' + count + ')" min="0"> ' +
                        /*   ddlFreeUnit +*/
                        '</div >' +
                        '</td>' +
                        '<td style="min-width:120px">' +
                        '<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control divUnitCost' + count + '_ctrl" id="txtUnitCost' + count + '"  value="' + data.Data.ItemDetails[i].PurchaseExcTax / ExchangeRate + '" onchange="ChangeQtyAmount(' + count + ')">' +
                        '</td>' +
                        '<td>' +
                        '<div class="input-group" style="min-width:150px">' +
                        '<input ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeQtyAmount(' + count + ')">' +
                        '<select ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" style="min-width: 100px;" id="ddlDiscountType' + count + '" onchange="ChangeQtyAmount(' + count + ')">' +
                        '<option value="Fixed">Fixed</option>' +
                        '<option value="Percentage">Percentage</option>' +
                        '</select>' +
                        '</div >' +
                        '<input type="text" hidden class="form-control" id="hdnExtraDiscounts' + count + '" value="0">' +
                        /*'<input onKeyPress="onlyNumberKey(event)" type="number" class="form-control" id="txtDiscount' + count + '" onchange="ChangeDiscAmount(' + count + ')" min="0">' +*/
                        '</td>' +
                        '<td style="min-width:100px;display:none">' +
                        '<input onKeyPress="onlyNumberKey(event)" type="number" ' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" value="' + data.Data.ItemDetails[i].PurchaseExcTax + '" id="txtPurchaseExcTax' + count + '"  onchange="ChangePurAmount(' + count + ')" min="0"/>' +
                        '</td>' +
                        '<td style="min-width:100px;display:none">' +
                        '<input type="number" class="form-control" disabled value="' + data.Data.ItemDetails[i].PurchaseExcTax + '" id="txtAmountExcTax' + count + '" min="0"/>' +
                        '</td>' +
                        '<td id="tdTax' + count + '">' +
                        '<div class="input-group" style="min-width:160px">' +
                        ddlTax +
                        '<input type="number" disabled class="form-control" id="txtTotalTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
                        '<input hidden type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '" style="min-width:80px">' +
                        '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                        '<div id="divTaxExemption' + count + '" class="form-group" style="display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                        '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                        '<div class="input-group">' +
                        ddlTaxExemption +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        /*'<small class="text-bold"><a href="javascript:void(0)" onclick="openTaxModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a></small>' +*/
                        '</td>' +
                        //'<td style="min-width:100px">' +
                        //'<input type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="' + taxamt + '">' +
                        //'<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                        //'</td>' +
                        '<td style="min-width:100px;display:none">' +
                        '<input type="number" class="form-control" id="txtPurchaseIncTax' + count + '"  value="' + data.Data.ItemDetails[i].PurchaseIncTax / ExchangeRate + '" onchange="updateNetCost(' + count + ')">' +
                        '<input type="hidden" value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '" id="hdnPurchaseIncTax' + count + '" />' +
                        '</td>' +
                        '<td style="min-width:120px" class="no_reversecharge">' +
                        '<input type="number" disabled class="form-control"  id="txtAmountIncTax' + count + '" value="' + data.Data.ItemDetails[i].PurchaseIncTax / ExchangeRate + '" >' +
                        '<input type="hidden" id="hdnAmountIncTax' + count + '" value="' + (data.Data.ItemDetails[i].TaxType.toLowerCase() == 'inclusive' ? data.Data.ItemDetails[i].PurchaseIncTax : data.Data.ItemDetails[i].PurchaseExcTax) + '" >' +
                        '</td>' +
                        '<td style="min-width:120px" class="' + (EnableLotNo == 'true' ? '' : 'hidden') + '">' +
                        '<input type="text" class="' + (data.Data.ItemDetails[i].IsManageStock == true ? '' : 'hidden') + ' form-control divLotNo' + count + '_ctrl" id="txtLotNo' + count + '">' +
                        '<span style="text-align:center;" class="' + (data.Data.ItemDetails[i].IsManageStock == false ? '' : 'hidden') + '">-</span>' +
                        '</td>' +
                        '<td style="min-width:180px" class="' + ((EnableItemExpiry == 'true' && ExpiryType == '2') ? '' : 'hidden') + '">' +
                        '<div class="input-group date _ManufacturingDate" id="_ManufacturingDate' + count + '" data-target-input="nearest">' +
                        '<input id="txtManufacturingDate' + count + '" type="text" class="form-control datetimepicker-input" data-target="#_ManufacturingDate' + count + '" />' +
                        '<div class="input-group-append" data-target="#_ManufacturingDate' + count + '" data-toggle="datetimepicker">' +
                        '<div class="input-group-text"><i class="fa fa-calendar"></i></div>' +
                        '</div>' +
                        '</div>' +
                        /*'<input type="date" class="form-control" id="txtManufacturingDate' + count + '">' +*/
                        '</td>' +
                        '<td style="min-width:180px" class="' + (EnableItemExpiry == 'true' ? '' : 'hidden') + '">' +
                        '<div class="input-group date _ExpiryDate" id="_ExpiryDate' + count + '" data-target-input="nearest">' +
                        '<input id="txtExpiryDate' + count + '" type="text" class="form-control datetimepicker-input" data-target="#_ExpiryDate' + count + '" />' +
                        '<div class="input-group-append" data-target="#_ExpiryDate' + count + '" data-toggle="datetimepicker">' +
                        '<div class="input-group-text"><i class="fa fa-calendar"></i></div>' +
                        '</div>' +
                        '</div>' +
                        /*'<input type="date" class="form-control" id="txtExpiryDate' + count + '">' +*/
                        '</td>' +
                        '<td style="min-width:100px;display:none;">' +
                        '<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" id="txtDefaultProfitMargin' + count + '"  value="' + data.Data.ItemDetails[i].DefaultProfitMargin + '" onchange="ChangePurAmount(' + count + ')">' +
                        '</td>' +
                        '<td style="min-width:100px">' +
                        '<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control divSalesExcTax' + count + '_ctrl" id="txtSalesExcTax' + count + '"  value="' + data.Data.ItemDetails[i].SalesExcTax + '">' +
                        //'<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control divSalesIncTax' + count + '_ctrl" id="txtSalesIncTax' + count + '"  value="' + data.Data.ItemDetails[i].SalesIncTax + '" onchange="updateSalePrice(' + count + ')">' +
                        '</td>' +
                        '<td style="min-width:100px" class="' + (EnableMrp == 'true' ? '' : 'hidden') + '">' +
                        '<input type="number"' + (EnableEditingProductPrice == 'true' ? '' : 'disabled') + ' class="form-control" id="txtMrp' + count + '"  value="' + data.Data.ItemDetails[i].DefaultMrp + '">' +
                        '</td>' +
                        '<td style="min-width:100px">' +
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

            $('#divCombo tr').each(function () {
                var _id = this.id.split('divCombo')[1];
                if (_id != undefined) {
                    //ChangePurAmount(_id);
                    ChangeQtyAmount(_id);
                }
            });

            var ExpiryDateFormat = Cookies.get('ItemSetting').split('&')[0].split('=')[1];
            $('._ManufacturingDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
            $('._ExpiryDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: ExpiryDateFormat.toUpperCase() });
            $('._ManufacturingDate').addClass('notranslate');
            $('._ExpiryDate').addClass('notranslate');

            updateAmount();
            if (data.Data.ItemDetails[0].SecondaryUnitShortName != null) {
                convertAvailableStock();
            }

            //var isIndTax = false;
            //$('#divCombo tr').each(function () {
            //    var _id = this.id.split('divCombo')[1];

            //    if ($('#ddlTax' + _id).val() != "0") {
            //        isIndTax = true;
            //    }

            //});

            //if (isIndTax == false) {
            //    $('#ddlTax').prop('disabled', false);
            //}
            //else {
            //    $('#ddlTax').prop('disabled', true);
            //}

            setExemption();

            $('.select2').select2();

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
        amount = amount + (parseFloat($('#txtUnitCost' + _id).val()) * parseFloat($('#txtQuantity' + _id).val()));
        qty = qty + parseFloat($('#txtQuantity' + _id).val()) + parseFloat($('#txtFreeQuantity' + _id).val());
    });

    $('#divTotalAmount').text(CurrencySymbol + Math.round(amount * 100) / 100);
    $('#hdndivTotalAmount').val(Math.round(amount * 100) / 100);
    $('#divTotalQty').text(qty);
    /*chargecalc();*/
    discallcalc();
}

function deleteCombo(id, PurchaseQuotationDetailsId) {
    if (PurchaseQuotationDetailsId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        //var det = {
        //    PurchaseQuotationDetailsId: PurchaseQuotationDetailsId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/purchasequotation/purchasequotationDetailsDelete',
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
        //update(false);
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

function deleteFullCombo(PurchaseQuotationId) {
    if (PurchaseQuotationId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
        //var det = {
        //    PurchaseQuotationId: PurchaseQuotationId
        //}
        //$("#divLoading").show();
        //$.ajax({
        //    url: '/purchasequotation/purchasequotationDetailsDelete',
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

                    //update(false);
                    skuCodes = [];
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
        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * ((parseFloat($('#txtUnitCost' + id).val())) - (discount + _extraDiscount))) * 100) / 100);

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesExcTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseExcTax' + id).val())) + parseFloat($('#txtPurchaseExcTax' + id).val())) * 100) / 100);
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function ChangePurAmount(id) {

    if (id != undefined) {
        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];
        let taxamt = parseFloat($('#txtPurchaseExcTax' + id).val()) * (taxper / 100);
        $('#txtTaxAmount' + id).val(Math.round(taxamt * 100) / 100);

        var _extraDiscount = Math.round((parseFloat($('#hdnExtraDiscounts' + id).val()) / parseFloat($('#txtQuantity' + id).val())) * 100) / 100;

        let chngpur = parseFloat($('#txtPurchaseExcTax' + id).val()) + parseFloat($('#txtTaxAmount' + id).val());
        $('#txtPurchaseIncTax' + id).val(Math.round(chngpur * 100) / 100);

        let discount = $('#txtDiscount' + id).val() == '' ? 0 : parseFloat($('#txtDiscount' + id).val());
        if ($('#ddlDiscountType' + id).val() == "Fixed") {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val())) + (discount + _extraDiscount)) * 100) / 100);
        }
        else {
            $('#txtUnitCost' + id).val(Math.round(((parseFloat($('#txtPurchaseExcTax' + id).val()) * 100) / (100 - (discount + _extraDiscount))) * 100) / 100);
        }

        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * (parseFloat($('#txtPurchaseExcTax' + id).val()))) * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * chngpur) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesExcTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseExcTax' + id).val())) + parseFloat($('#txtPurchaseExcTax' + id).val())) * 100) / 100);
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
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

function updateNetCost(id) {
    if (id != undefined) {
        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());

        let taxper = $('#ddlTax' + id).val().split('-')[1] == undefined ? 0 : $('#ddlTax' + id).val().split('-')[1];

        let PurchaseExcTax = (100 * (PurchaseIncTax / (100 + parseFloat(taxper))))

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

        $('#txtAmountExcTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * (parseFloat($('#txtPurchaseExcTax' + id).val()))) * 100) / 100);

        $('#txtAmountIncTax' + id).val(Math.round((parseFloat($('#txtQuantity' + id).val()) * PurchaseIncTax) * 100) / 100);

        let ProfitMargin = $('#txtDefaultProfitMargin' + id).val() == undefined ? 0 : $('#txtDefaultProfitMargin' + id).val();
        $('#txtSalesExcTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseExcTax' + id).val())) + parseFloat($('#txtPurchaseExcTax' + id).val())) * 100) / 100);
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);
    }
    updateAmount();
}

function updateSalePrice(id) {
    if (id != undefined) {
        let PurchaseIncTax = parseFloat($('#txtPurchaseIncTax' + id).val());
        let SalesIncTax = parseFloat($('#txtSalesIncTax' + id).val());

        let ProfitMargin = ((SalesIncTax - PurchaseIncTax) / PurchaseIncTax) * 100;

        $('#txtDefaultProfitMargin' + id).val(Math.round(ProfitMargin * 100) / 100);
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

    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
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

    taxs.push({
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        AmountExcTax: amountExcTax//divTotalAmount - (amount + discount)
    });

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
                //data.Data.Taxs.forEach(function (res) {
                //    html = html + '<div class="row">' +
                //        '<label for="inputEmail3" class="col-8 col-form-label text-right">' + res.Tax + ' :</label>' +
                //        '<label for="inputEmail3" class="col-4 col-form-label text-left">' + res.TaxAmount.toFixed(2) + '</label>' +
                //        '</div>';
                //});

                data.Data.Taxs.forEach(function (res) {
                    html = html + '<div class="text-right">' +
                        '<label for="inputEmail3" class="col-form-label">' + res.Tax + ' : ' + CurrencySymbol + res.TaxAmount.toFixed(2) + '</label>' +
                        //'<label for="inputEmail3" class="col-form-label">' + ' ' + CurrencySymbol + res.TaxAmount.toFixed(2) + '</label>' +
                        '</div>';
                });

                $('#divTaxSummary').empty();
                $('#divTaxSummary').append(html);

                var AdditionalChargesAmountExcTax = 0, AdditionalChargesAmountIncTax = 0;
                $('#divAdditionalCharges .additional-charges-row').each(function (index) {
                    var count = index;
                    var txtAdditionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
                    var txtAdditionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());

                    if (txtAdditionalChargesAmountExcTax > 0) {
                        AdditionalChargesAmountExcTax += txtAdditionalChargesAmountExcTax;
                        AdditionalChargesAmountIncTax += txtAdditionalChargesAmountIncTax;
                    }
                });

                let taxper = $('#ddlTax').val().split('-')[1] == undefined ? 0 : $('#ddlTax').val().split('-')[1];
                let taxamt = (amountIncTax - amountExcTax) + (AdditionalChargesAmountIncTax - AdditionalChargesAmountExcTax) + (parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100));
                $('#hdndivTax').val(Math.round((parseFloat(divTotalAmount - (amount + discount + _extraDiscount)) * (taxper / 100)) * 100) / 100);
                $('#hdndivTotalTax').val(Math.round(taxamt * 100) / 100);
                $('#divTax').text(CurrencySymbol + Math.round(taxamt * 100) / 100);

                $('#divDiscount').text(CurrencySymbol + ((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100));
                $('#hdndivDiscount').val((Math.round(discount * 100) / 100) + Math.round(amount * 100) / 100);

                let SpecialDiscount = $('#txtSpecialDiscount').val() || 0;
                $('#divSpecialDiscount').text(CurrencySymbol + (Math.round(SpecialDiscount * 100) / 100));

                var grandTotal = Math.round(((divTotalAmount + taxamt + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount))) * 100) / 100;
                var roundOffGrandTotal = grandTotal;
                if ($('#hdnEnableRoundOff').val().toLocaleLowerCase() == 'true') {
                    roundOffGrandTotal = Math.round(grandTotal * 1) / 1;
                }
                var roundOff = roundOffGrandTotal - grandTotal;

                $('#divNetAmount').text(CurrencySymbol + Math.round(grandTotal * 100) / 100);
                $('#divRoundOff').text(CurrencySymbol + Math.round(roundOff * 100) / 100);
                $('#divGrandTotal').text(CurrencySymbol + Math.round(roundOffGrandTotal * 100) / 100);

                var grandTotal_reversecharge = Math.round(((divTotalAmount + AdditionalChargesAmountExcTax) - (parseFloat($('#hdndivDiscount').val()) + parseFloat(SpecialDiscount))) * 100) / 100;
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
        debugger
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
        $('#txtSalesExcTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseExcTax' + id).val())) + parseFloat($('#txtPurchaseExcTax' + id).val())) * 100) / 100);
        $('#txtSalesIncTax' + id).val(Math.round((((parseFloat(ProfitMargin) / 100) * parseFloat($('#txtPurchaseIncTax' + id).val())) + parseFloat($('#txtPurchaseIncTax' + id).val())) * 100) / 100);

    });

    discallcalc();
}

function insert(i) {
    var ExpiryDateFormat = Cookies.get('ItemSetting').split('&')[0].split('=')[1];
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    _i = i;
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
                DivId: _id,
                Quantity: $('#txtQuantity' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                PurchaseExcTax: $('#txtPurchaseExcTax' + _id).val(),
                PurchaseIncTax: $('#txtPurchaseIncTax' + _id).val(),
                Tax: $('#txtTaxAmount' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Discount: $('#txtDiscount' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                LotNo: $('#txtLotNo' + _id).val(),
                ManufacturingDate: moment($("#txtManufacturingDate" + _id).val(), ExpiryDateFormat.toUpperCase()).format('DD-MM-YYYY'),
                ExpiryDate: moment($("#txtExpiryDate" + _id).val(), ExpiryDateFormat.toUpperCase()).format('DD-MM-YYYY'),
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                SalesExcTax: $('#txtSalesExcTax' + _id).val(),
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                FreeQuantity: $('#txtFreeQuantity' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                DiscountType: $('#ddlDiscountType' + _id).val(),
                DefaultProfitMargin: $('#txtDefaultProfitMargin' + _id).val(),
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                Mrp: $('#txtMrp' + _id).val(),
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
            })
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
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
            ITCType: $('#itcTypeValue' + count).val(),
            TaxExemptionId: taxExemptionId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        BranchId: $('#ddlBranch').val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        IsActive: true,
        IsDeleted: false,
        Notes: $("#txtNotes").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        PurchaseQuotationDate: moment($("#txtPurchaseQuotationDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Status: $("#ddlPurchaseQuotationStatus  option:selected").val(),
        ReferenceNo: $("#txtPurchaseQuotationReferenceNo").val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        SupplierId: $("#ddlSupplier option:selected").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        TotalQuantity: $('#divTotalQty').text(),
        PurchaseQuotationDetails: ItemDetails,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        CheckStockPriceMismatch: _CheckStockPriceMismatch,
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        ReferenceId: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[0].split('=')[1].replace('%20', " ") : '',
        ReferenceType: window.location.href.indexOf('&') > -1 ? window.location.href.split('&')[1].split('=')[1].replace('%20', " ") : '',
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        PurchaseQuotationAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/purchasequotationsInsert',
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
                //if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                //    window.open(data.WhatsappUrl);
                //}

                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                /*if ($("#ddlPurchaseStatus option:selected").val() != 'Draft') {*/
                if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseQuotation == true) {
                    sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.PurchaseQuotation.InvoiceId);
                }
                /*}*/
                //else if ($("#ddlPurchaseStatus option:selected").val() == 'Pending') {
                //    if (data.Data.PurchaseSetting.AutoPrintInvoicePending == true) {
                //        sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                //    }
                //}
                //else if ($("#ddlPurchaseStatus option:selected").val() == 'Ordered') {
                //    if (data.Data.PurchaseSetting.AutoPrintInvoiceOrdered == true) {
                //        sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                //    }
                //}

                if (i == 1) {
                    window.location.href = "/purchasequotation/index";
                }
                else {
                    window.location.href = "/purchasequotation/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    var ExpiryDateFormat = Cookies.get('ItemSetting').split('&')[0].split('=')[1];
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    _i = i;
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
                DivId: _id,
                PurchaseQuotationDetailsId: $('#txtPurchaseDetailsId' + _id).val(),
                Quantity: $('#txtQuantity' + _id).val(),
                AmountExcTax: $('#txtAmountExcTax' + _id).val(),
                AmountIncTax: $('#txtAmountIncTax' + _id).val(),
                ItemDetailsId: $('#txtItemDetailsId' + _id).val(),
                ItemId: $('#txtItemId' + _id).val(),
                PurchaseExcTax: $('#txtPurchaseExcTax' + _id).val(),
                PurchaseIncTax: $('#txtPurchaseIncTax' + _id).val(),
                Tax: $('#txtTaxAmount' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Discount: $('#txtDiscount' + _id).val(),
                UnitCost: $('#txtUnitCost' + _id).val(),
                IsActive: true,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                LotNo: $('#txtLotNo' + _id).val(),
                ManufacturingDate: moment($("#txtManufacturingDate" + _id).val(), ExpiryDateFormat.toUpperCase()).format('DD-MM-YYYY'),
                ExpiryDate: moment($("#txtExpiryDate" + _id).val(), ExpiryDateFormat.toUpperCase()).format('DD-MM-YYYY'),
                //PriceAddedFor: $("#ddlUnit" + _id)[0].selectedIndex + 1,
                PriceAddedFor: $("#ddlUnit" + _id).val().split('-')[1],
                SalesExcTax: $('#txtSalesExcTax' + _id).val(),
                SalesIncTax: $('#txtSalesIncTax' + _id).val(),
                FreeQuantity: $('#txtFreeQuantity' + _id).val(),
                //FreeQuantityPriceAddedFor: $("#ddlFreeUnit" + _id)[0].selectedIndex + 1,
                TaxAmount: $('#txtTaxAmount' + _id).val(),
                TotalTaxAmount: $('#txtTotalTaxAmount' + _id).val(),
                DiscountType: $('#ddlDiscountType' + _id).val(),
                DefaultProfitMargin: $('#txtDefaultProfitMargin' + _id).val(),
                //UnitAddedFor: ($("#ddlUnit" + _id)[0].selectedIndex + 1) - hiddenOptionsCount,
                UnitAddedFor: $("#ddlUnit" + _id).val().split('-')[2],
                Mrp: $('#txtMrp' + _id).val(),
                ExtraDiscount: $('#hdnExtraDiscounts' + _id).val(),
            })
        }
    });

    var additionalCharges = [];
    $('#divAdditionalCharges .additional-charges-row').each(function (index) {
        var count = index;
        var additionalChargesAmountExcTax = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
        var additionalChargesAmountIncTax = $('#txtAdditionalChargesAmountIncTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountIncTax' + count).val());
        var taxId = $('#ddlAdditionalChargesTax' + count).val().split('-')[0];
        var additionalChargeId = $('#txtAdditionalChargeId' + count).val();
        var taxExemptionId = $('#exemptionReasonId' + count).val();
        var purchaseQuotationAdditionalChargesId = $('#txtPurchaseQuotationAdditionalChargesId' + count).val();
        additionalCharges.push({
            AdditionalChargeId: additionalChargeId,
            TaxId: taxId,
            AmountExcTax: additionalChargesAmountExcTax,
            AmountIncTax: additionalChargesAmountIncTax,
            ITCType: $('#itcTypeValue' + count).val(),
            TaxExemptionId: taxExemptionId,
            PurchaseQuotationAdditionalChargesId: purchaseQuotationAdditionalChargesId,
            IsActive: true,
            IsDeleted: false
        })
    });

    var det = {
        PurchaseQuotationId: window.location.href.split('=')[1].replace('#', ''),
        BranchId: $('#ddlBranch').val(),
        DeliveredTo: $('#txtDeliveredTo').val(),
        Discount: $("#txtDiscAll").val(),
        DiscountType: $('#ddlDiscAll').val(),
        TotalDiscount: $("#hdndivDiscount").val(),
        GrandTotal: $("#divGrandTotal").text().replace(/[^-0-9\.]+/g, ""),
        RoundOff: $("#divRoundOff").text().replace(/[^-0-9\.]+/g, ""),
        NetAmount: $("#divNetAmount").text().replace(/[^-0-9\.]+/g, ""),
        GrandTotalReverseCharge: $("#divGrandTotal_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        RoundOffReverseCharge: $("#divRoundOff_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        NetAmountReverseCharge: $("#divNetAmount_reversecharge").text().replace(/[^-0-9\.]+/g, ""),
        IsActive: true,
        IsDeleted: false,
        Notes: $("#txtNotes").val(),
        OtherCharges: $("#txtOtherCharges").val(),
        PackingCharge: $('#txtPackagingCharge').val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        PurchaseQuotationDate: moment($("#txtPurchaseQuotationDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Status: $("#ddlPurchaseQuotationStatus  option:selected").val(),
        ReferenceNo: $("#txtPurchaseQuotationReferenceNo").val(),
        ShippingAddress: $('#txtShippingAddress').val(),
        ShippingCharge: $('#txtShippingCharge').val(),
        ShippingDetails: $('#txtShippingDetails').val(),
        ShippingStatus: $('#ddlShippingStatus').val(),
        Subtotal: $("#hdndivTotalAmount").val(),
        SupplierId: $("#ddlSupplier option:selected").val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        TaxAmount: $('#hdndivTax').val(),
        TotalTaxAmount: $('#hdndivTotalTax').val(),
        TotalQuantity: $('#divTotalQty').text(),
        PurchaseQuotationDetails: ItemDetails,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        PurchaseQuotationAdditionalCharges: additionalCharges,
        SpecialDiscount: $('#txtSpecialDiscount').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/purchasequotationUpdate',
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
                //if (data.WhatsappUrl != "" && data.WhatsappUrl != null) {
                //    window.open(data.WhatsappUrl);
                //}

                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                /*if ($("#ddlPurchaseStatus option:selected").val() != 'Draft') {*/
                if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseQuotation == true) {
                    sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.PurchaseQuotation.InvoiceId);
                }
                /*}*/
                //else if ($("#ddlPurchaseStatus option:selected").val() == 'Pending') {
                //    if (data.Data.PurchaseSetting.AutoPrintInvoicePending == true) {
                //        sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                //    }
                //}
                //else if ($("#ddlPurchaseStatus option:selected").val() == 'Ordered') {
                //    if (data.Data.PurchaseSetting.AutoPrintInvoiceOrdered == true) {
                //        sessionStorage.setItem('InvoiceUrl', '/purchasequotation/invoice?InvoiceId=' + data.Data.Purchase.InvoiceId);
                //    }
                //}

                if (i == 1) {
                    window.location.href = "/purchasequotation/index";
                }
                else {
                    window.location.href = "/purchasequotation/add";
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

function getAttachDocumentBase64() {
    var file1 = $("#AttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#AttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            AttachDocument = reader.result;
            FileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function getPaymentAttachDocumentBase64() {
    var file1 = $("#PaymentAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {

        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
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
            console.log(error);
            file = error;
        };
    }
}

function Delete(PurchaseQuotationId) {
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            PurchaseQuotationId: PurchaseQuotationId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/purchasequotation/purchasequotationdelete',
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

function openSupplierModal() {
    $("#supplierModal").modal('show');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');
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
        url: '/purchasequotation/ActiveStates',
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
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveCitys(type) {
    var id = 0;
    if (type == '') {
        id = $('#ddlState').val();
    }
    else {
        id = $('#ddlAltState').val();
    }
    var det = {
        StateId: id,
        Type: type
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_Citys_Dropdown").html(data);
            }
            else {
                $("#p_Alt_Citys_Dropdown").html(data);
            }
            $('.select2').select2();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertBranch() {
    var det = {
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        BranchCode: $('#txtBranchCode').val(),
        Branch: $('#txtBranch').val(),
        ContactName: $('#txtContactName').val(),
        Mobile: $('#txtMobile').val(),
        Email: $('#txtEmail').val(),
        Zipcode: $('#txtZipcode').val(),
        CityId: $('#ddlCity').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/master/BranchInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                $('#ddlBranch').append($('<option>', { value: data.Data.Branch.BranchId, text: data.Data.Branch.Branch }));
                $('#ddlBranch').val(data.Data.Branch.BranchId);
                $('#branchModal').modal('toggle');
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function validateNumber(evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
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

function toggleUnit(i) {
    var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
    var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
    var UToSValue = parseFloat($('#hdnUToSValue' + i).val());// == 0 ? 1 : $('#hdnUToSValue' + i).val());
    var SToTValue = parseFloat($('#hdnSToTValue' + i).val());// == 0 ? 1 : $('#hdnSToTValue' + i).val());
    var TToQValue = parseFloat($('#hdnTToQValue' + i).val());// == 0 ? 1 : $('#hdnTToQValue' + i).val());

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

    $('#ddlFreeUnit' + i).val($('#ddlUnit' + i).val());
    ChangeQtyAmount(i);
}

function convertAvailableStock() {
    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var ind = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
        var ItemDetailsId = $('#txtItemDetailsId' + i).val();

        var UToSValue = $('#hdnUToSValue' + i).val();
        var SToTValue = $('#hdnSToTValue' + i).val();
        var TToQValue = $('#hdnTToQValue' + i).val();

        var j_Qty = 0;
        $('#divCombo tr').each(function () {

            var j = this.id.split('divCombo')[1];
            var j_ind = $("#ddlUnit" + j).val().split('-')[1];//[0].selectedIndex + 1;
            var j_ItemDetailsId = $('#txtItemDetailsId' + j).val();
            if (j_ItemDetailsId == ItemDetailsId && i != j) {
                var q = parseFloat($('#txtQuantity' + j).val());
                if (j_ind == 1) {
                    q = (q * UToSValue * SToTValue * TToQValue);
                }
                else if (j_ind == 2) {
                    q = (q * SToTValue * TToQValue);
                }
                else if (j_ind == 3) {
                    q = (q * TToQValue);
                }

                j_Qty = j_Qty + q;
            }
        });

        var availableQuantity = $('#hdnQuantityRemaining' + i).val();
        availableQuantity = availableQuantity - j_Qty;

        if (ind == 1) {
            availableQuantity = (availableQuantity / UToSValue / SToTValue / TToQValue);
        }
        else if (ind == 2) {
            availableQuantity = (availableQuantity / SToTValue / TToQValue);
        }
        else if (ind == 3) {
            availableQuantity = (availableQuantity / TToQValue);
        }

        $('#txtStockQuantity' + i).val(availableQuantity);
    });
}

function insertSupplier() {
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
        //Address: $('#txtAddress').val(),
        //CountryId: $('#ddlCountry').val(),
        //StateId: $('#ddlState').val(),
        //CityId: $('#ddlCity').val(),
        //Zipcode: $('#txtZipcode').val(),
        //AltAddress: $('#txtAltAddress').val(),
        //AltCountryId: $('#ddlAltCountry').val(),
        //AltStateId: $('#ddlAltState').val(),
        //AltCityId: $('#ddlAltCity').val(),
        //AltZipcode: $('#txtAltZipcode').val(),
        PaymentTermId: $('#ddlPaymentTerm').val(),
        TaxNo: $('#txtTaxNo').val(),
        CreditLimit: $('#txtCreditLimit').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        PayTerm: $('#ddlPayTerm').val(),
        PayTermNo: $('#txtPayTermNo').val(),
        UserType: 'Supplier',
        IsActive: true,
        IsDeleted: false,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        TaxId: $('#ddlTax').val(),
        Addresses: Addresses,
        CurrencyId: $('#ddlUserCurrency').val(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply_M').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        //BusinessLegalName: $('#txtBusinessLegalName').val(),
        //BusinessTradeName: $('#txtBusinessTradeName').val(),
        PanNo: $('#txtPanNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/UserInsert',
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
                $('#ddlSupplier').append($('<option>', { value: data.Data.User.UserId, text: data.Data.User.Name }));

                $('#ddlSupplier').val(data.Data.User.UserId);
                $('#supplierModal').modal('toggle');

                FetchUserCurrency();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openPurchaseStatusModal(id) {
    _PurchaseQuotationId = id;
    $('#purchaseStatusModal').modal('toggle');
}

function UpdatePurchaseStatus() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        PurchaseQuotationId: _PurchaseQuotationId,
        Status: $("#ddlPurchaseStatus_M").val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasequotation/UpdatePurchasequotationStatus',
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
                $("#purchaseStatusModal").modal('hide');

                if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseQuotation == true) {
                    PrintInvoice('/purchasequotation/invoice?InvoiceId=' + data.Data.PurchaseQuotation.InvoiceId);
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function AcceptDeclinePurchaseStatus(_id, Status) {
    var r = confirm("This will mark the Purchase Quotation as " + Status + ". This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');


        var det = {
            PurchaseQuotationId: _id,
            Status: Status,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/purchasequotation/UpdatePurchasequotationStatus',
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

                    if (data.Data.PurchaseSetting.AutoPrintInvoicePurchaseQuotation == true) {
                        PrintInvoice('/purchasequotation/invoice?InvoiceId=' + data.Data.PurchaseQuotation.InvoiceId);
                    }
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function FetchUserCurrency() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlSupplier').val(),
    };
    $("#divLoading").show();
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
                if (data.Data.User) {
                    if (data.Data.User.GstTreatment != null && data.Data.User.GstTreatment != "") {
                        if (data.Data.User.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)") {
                            $('#ddlSourceOfSupply').val(data.Data.User.SourceOfSupplyId);
                            $('.divSourceOfSupply').show();
                        }
                        else {
                            $('#ddlSourceOfSupply').val(0);
                            $('.divSourceOfSupply').hide();
                        }
                    }
                    $('#ddlDestinationOfSupply').val(data.Data.BusinessSetting.StateId);
                    $('#ddlPaymentTerm').val(data.Data.User.PaymentTermId);
                }
                else {
                    $('#ddlSourceOfSupply').val(0);
                    $('#ddlDestinationOfSupply').val(0);
                    $('#ddlPaymentTerm').val(0);
                }

                $('.select2').select2();

                fetchUpdatedItems();

                $('#txtAdvBalance').text(data.Data.User.CurrencySymbol + data.Data.User.AdvanceBalance);
                if (data.Data.User.ExchangeRate == 0) {
                    $('.divCurrencyExchange').hide();
                }
                else {
                    $('.divCurrencyExchange').show();
                }

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
                        var SalesExcTax = $('#hdnSalesExcTax' + i).val();
                        var SalesIncTax = $('#hdnSalesIncTax' + i).val();
                        var newUnitCost = 0, newSalesExcTax = 0, newSalesIncTax = 0;
                        //var ExchangeRate = (!ExchangeRate || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

                        if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
                            newUnitCost = UnitCost;
                            newSalesExcTax = SalesExcTax;
                            newSalesIncTax = SalesIncTax;
                        }
                        else if (SToTValue == 0 && TToQValue == 0) {
                            if (index == 3) {
                                newUnitCost = UnitCost;
                                newSalesExcTax = SalesExcTax;
                                newSalesIncTax = SalesIncTax;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesExcTax = SalesExcTax / UToSValue;
                                newSalesIncTax = SalesIncTax / UToSValue;
                            }
                        }
                        else if (TToQValue == 0) {
                            if (index == 2) {
                                newUnitCost = UnitCost;
                                newSalesExcTax = SalesExcTax;
                                newSalesIncTax = SalesIncTax;
                            }
                            else if (index == 3) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesExcTax = SalesExcTax / UToSValue;
                                newSalesIncTax = SalesIncTax / UToSValue;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue / SToTValue;
                                newSalesExcTax = SalesExcTax / UToSValue / SToTValue;
                                newSalesIncTax = SalesIncTax / UToSValue / SToTValue;
                            }
                        }
                        else {
                            if (index == 1) {
                                newUnitCost = UnitCost;
                                newSalesExcTax = SalesExcTax;
                                newSalesIncTax = SalesIncTax;
                            }
                            else if (index == 2) {
                                newUnitCost = UnitCost / UToSValue;
                                newSalesExcTax = SalesExcTax / UToSValue;
                                newSalesIncTax = SalesIncTax / UToSValue;
                            }
                            else if (index == 3) {
                                newUnitCost = UnitCost / UToSValue / SToTValue;
                                newSalesExcTax = SalesExcTax / UToSValue / SToTValue;
                                newSalesIncTax = SalesIncTax / UToSValue / SToTValue;
                            }
                            else if (index == 4) {
                                newUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
                                newSalesExcTax = SalesExcTax / UToSValue / SToTValue / TToQValue;
                                newSalesIncTax = SalesIncTax / UToSValue / SToTValue / TToQValue;
                            }
                        }

                        $('#txtUnitCost' + i).val(Math.round((parseFloat(newUnitCost) / ExchangeRate) * 100) / 100);
                        $('#txtSalesExcTax' + i).val(Math.round((parseFloat(newSalesExcTax) / ExchangeRate) * 100) / 100);
                        $('#txtSalesIncTax' + i).val(Math.round((parseFloat(newSalesIncTax) / ExchangeRate) * 100) / 100);
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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchUpdatedItems() {
    //$('#divCombo').empty();
    $.each(skuCodes, function (index, value) {
        fetchItemTax(value);
    });

    fetchAdditionalCharges();
}

function updateCurrencyExchange() {
    $('#divCombo tr').each(function () {
        var i = this.id.split('divCombo')[1];
        var index = $("#ddlUnit" + i).val().split('-')[1];//[0].selectedIndex + 1;
        //var PriceAddedFor = $('#hdnPriceAddedFor' + i).val();
        var UToSValue = parseFloat($('#hdnUToSValue' + i).val());// == 0 ? 1 : $('#hdnUToSValue' + i).val());
        var SToTValue = parseFloat($('#hdnSToTValue' + i).val());// == 0 ? 1 : $('#hdnSToTValue' + i).val());
        var TToQValue = parseFloat($('#hdnTToQValue' + i).val());// == 0 ? 1 : $('#hdnTToQValue' + i).val());

        var UnitCost = $('#hdnUnitCost' + i).val();
        var SalesExcTax = $('#hdnSalesExcTax' + i).val();
        var SalesIncTax = $('#hdnSalesIncTax' + i).val();
        var newUnitCost = 0, newSalesExcTax = 0, newSalesIncTax = 0;
        var ExchangeRate = (!$('#txtExchangeRate').val() || $('#txtExchangeRate').val() == '0') ? 1 : parseFloat($('#txtExchangeRate').val());

        if (UToSValue == 0 && SToTValue == 0 && TToQValue == 0) {
            newUnitCost = UnitCost;
            newSalesExcTax = SalesExcTax;
            newSalesIncTax = SalesIncTax;
        }
        else if (SToTValue == 0 && TToQValue == 0) {
            if (index == 3) {
                newUnitCost = UnitCost;
                newSalesExcTax = SalesExcTax;
                newSalesIncTax = SalesIncTax;
            }
            else if (index == 4) {
                newUnitCost = UnitCost / UToSValue;
                newSalesExcTax = SalesExcTax / UToSValue;
                newSalesIncTax = SalesIncTax / UToSValue;
            }
        }
        else if (TToQValue == 0) {
            if (index == 2) {
                newUnitCost = UnitCost;
                newSalesExcTax = SalesExcTax;
                newSalesIncTax = SalesIncTax;
            }
            else if (index == 3) {
                newUnitCost = UnitCost / UToSValue;
                newSalesExcTax = SalesExcTax / UToSValue;
                newSalesIncTax = SalesIncTax / UToSValue;
            }
            else if (index == 4) {
                newUnitCost = UnitCost / UToSValue / SToTValue;
                newSalesExcTax = SalesExcTax / UToSValue / SToTValue;
                newSalesIncTax = SalesIncTax / UToSValue / SToTValue;
            }
        }
        else {
            if (index == 1) {
                newUnitCost = UnitCost;
                newSalesExcTax = SalesExcTax;
                newSalesIncTax = SalesIncTax;
            }
            else if (index == 2) {
                newUnitCost = UnitCost / UToSValue;
                newSalesExcTax = SalesExcTax / UToSValue;
                newSalesIncTax = SalesIncTax / UToSValue;
            }
            else if (index == 3) {
                newUnitCost = UnitCost / UToSValue / SToTValue;
                newSalesExcTax = SalesExcTax / UToSValue / SToTValue;
                newSalesIncTax = SalesIncTax / UToSValue / SToTValue;
            }
            else if (index == 4) {
                newUnitCost = UnitCost / UToSValue / SToTValue / TToQValue;
                newSalesExcTax = SalesExcTax / UToSValue / SToTValue / TToQValue;
                newSalesIncTax = SalesIncTax / UToSValue / SToTValue / TToQValue;
            }
        }

        $('#txtUnitCost' + i).val(Math.round((parseFloat(newUnitCost) / ExchangeRate) * 100) / 100);
        $('#txtSalesExcTax' + i).val(Math.round((parseFloat(newSalesExcTax) / ExchangeRate) * 100) / 100);
        $('#txtSalesIncTax' + i).val(Math.round((parseFloat(newSalesIncTax) / ExchangeRate) * 100) / 100);
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

function fetchActiveUsers(showAddNew) {
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType: 'supplier'
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/usersettings/AllActiveUsers',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                $('#divCombo').empty();
                var ddlUser = '<select class="form-control select2" id="ddlSupplier" onchange="FetchUserCurrency()">';
                if (showAddNew == true) {
                    ddlUser = ddlUser + '<option value="0">Select</option>';
                }
                else {
                    ddlUser = ddlUser + '<option value="0">All</option>';
                }

                for (let ss = 0; ss < data.Data.Users.length; ss++) {
                    var option = '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name;
                    if (data.Data.Users[ss].MobileNo != "" && data.Data.Users[ss].MobileNo != null) {
                        option = option + ' - ' + data.Data.Users[ss].MobileNo;
                    }

                    ddlUser = ddlUser + option + '</option>';
                }
                ddlUser = ddlUser + '</select>';

                if (showAddNew == true) {
                    ddlUser = ddlUser + '<span class="input-group-append">' +
                        '<a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#supplierModal"> + </a>' +
                        '</span>';
                }

                $('.divSupplier').empty();
                $('.divSupplier').append(ddlUser);

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchAccountMapped(_type) {
    if ($('#ddlPaymentType option:selected').text() == "Advance") {
        $('.divLAccount').hide();
    }
    else {
        $('.divLAccount').show();
        var det = {
            BranchId: _type == 0 ? _BranchId : $('#ddlBranch').val(),
            PaymentTypeId: $('#ddlPaymentType').val(),
        };
        //$("#divLoading").show();
        $.ajax({
            url: '/settings1/FetchAccountMapped',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (data.Data.BranchPaymentTypeMap == null) {
                        if (_type == 0) {
                            $('#ddlLAccount').val(0);
                        }
                        else {
                            $('#ddlAccount').val(0);
                        }
                    }
                    else {
                        if (_type == 0) {
                            $('#ddlLAccount').val(data.Data.BranchPaymentTypeMap.AccountId);
                        }
                        else {
                            $('#ddlAccount').val(data.Data.BranchPaymentTypeMap.AccountId);
                        }
                    }

                    $('.select2').select2();
                }

            },
            error: function (xhr) {

            }
        });
    }

}

function fetCurrentDate() {
    if ($('#txtAmount').val()) {
        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
        var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

        $('#txtPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
        $('#_PaymentDate').addClass('notranslate');
    }
    else {
        $('#txtPaymentDate').val('');
    }
}

function openTaxModal(_type) {
    type = _type;

    $('.divTaxModal').show();
    $('.divTaxGroupModal').hide();

    $('#taxModal').modal('toggle');
}

function toggleTaxModal(d) {

    $('.divTaxModal').hide();
    $('.divTaxGroupModal').hide();

    if (d == 'TaxGroup') {
        $('.divTaxGroupModal').show();
    }
    else {
        $('.divTaxModal').show();
    }
}

function toggleForTaxGroupOnly(c) {
    if (c == 1) {
        if ($('#chkNewForTaxGroupOnly').is(':checked') == true) {
            $('.divNewAccounts').hide();
        }
        else {
            $('.divNewAccounts').show();
        }
    }
    else {
        if ($('#chkForTaxGroupOnly_N').is(':checked') == true) {
            $('.divAccounts_N').hide();
        }
        else {
            $('.divAccounts_N').show();
        }
    }
}

function insertTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercent').val(),
        Tax: $('#txtNewTax').val(),
        IsActive: true,
        IsDeleted: false,
        ForTaxGroupOnly: $('#chkNewForTaxGroupOnly').is(':checked'),
        PurchaseAccountId: $('#ddlNewPurchaseAccount').val(),
        SalesAccountId: $('#ddlNewSalesAccount').val(),
        SupplierPaymentAccountId: $('#ddlNewSupplierPaymentAccount').val(),
        CustomerPaymentAccountId: $('#ddlNewCustomerPaymentAccount').val(),
        ExpenseAccountId: $('#ddlNewExpenseAccount').val(),
        IncomeAccountId: $('#ddlNewIncomeAccount').val(),
        TaxTypeId: $('#ddlNewTaxType').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxInsert',
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
                $('#ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);

                $('#ddlIntraStateTax_N').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (type == 'IntraStateTax') {
                    $('#ddlIntraStateTax_N').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }

                $('#ddlInterStateTax_N').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (type == 'InterStateTax') {
                    $('#ddlInterStateTax_N').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }

                $('#ddlNewSubTax').append($('<option>', { value: data.Data.Tax.TaxId, text: data.Data.Tax.Tax }));
                var d = $('#ddlNewSubTax').val();
                if (d == null) d = [];
                d.push(data.Data.Tax.TaxId);
                //$('#ddlNewSubTax').val(d);

                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercent').val('');
                $('#txtNewTax').val('');

                calulation();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertTaxGroup(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercentGroup').val(),
        Tax: $('#txtNewTaxGroup').val(),
        SubTaxs: $('#ddlNewSubTax').val(),
        IsActive: true,
        IsDeleted: false,
        PurchaseAccountId: $('#ddlNewSubTaxPurchaseAccount').val(),
        SalesAccountId: $('#ddlNewSubTaxSalesAccount').val(),
        SupplierPaymentAccountId: $('#ddlNewSubTaxSupplierPaymentAccount').val(),
        CustomerPaymentAccountId: $('#ddlNewSubTaxCustomerPaymentAccount').val(),
        ExpenseAccountId: $('#ddlNewSubTaxExpenseAccount').val(),
        IncomeAccountId: $('#ddlNewSubTaxIncomeAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxgroupInsert',
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
                $('#ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercentGroup').val('');
                $('#txtNewTaxGroup').val('');
                $('#ddlNewSubTax').val('');
                $('.select2').select2();

                calulation();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertInnerTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Taxpercent: $('#txtNewTaxpercent_N').val(),
        Tax: $('#txtNewTax_N').val(),
        IsActive: true,
        IsDeleted: false,
        ForTaxGroupOnly: $('#chkForTaxGroupOnly_N').is(':checked'),
        PurchaseAccountId: $('#ddlPurchaseAccount_N').val(),
        SalesAccountId: $('#ddlSalesAccount_N').val(),
        SupplierPaymentAccountId: $('#ddlSupplierPaymentAccount_N').val(),
        CustomerPaymentAccountId: $('#ddlCustomerPaymentAccount_N').val(),
        ExpenseAccountId: $('#ddlExpenseAccount_N').val(),
        IncomeAccountId: $('#ddlIncomeAccount_N').val(),
        TaxTypeId: $('#ddlTaxType_N').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxInsert',
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
                    $('#' + res.Id + '_N').show();
                    $('#' + res.Id + '_N').text(res.Message);

                    if ($('.' + res.Id + '_N_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_N_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_N_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_N_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_N_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if ($('#chkForTaxGroupOnly_N').is(':checked') == false) {
                    $('#ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    $('#ddlTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);

                    $('#ddlIntraStateTax_N').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    if (type == 'IntraStateTax') {
                        $('#ddlIntraStateTax_N').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                    }

                    $('#ddlInterStateTax_N').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    if (type == 'InterStateTax') {
                        $('#ddlInterStateTax_N').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                    }
                }

                $('#ddlNewSubTax').append($('<option>', { value: data.Data.Tax.TaxId, text: data.Data.Tax.Tax }));
                var d = $('#ddlNewSubTax').val();
                if (d == null) d = [];
                d.push(data.Data.Tax.TaxId);
                $('#ddlNewSubTax').val(d);

                $('#taxInnerModal').modal('toggle');

                $('#txtNewTaxpercent_N').val('');
                $('#txtNewTax_N').val('');

                FetchTaxPercent();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function FetchTaxPercent() {
    var det = {
        SubTaxs: $('#ddlNewSubTax').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/FetchTaxPercent',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Data == null) {
                $('#txtNewTaxpercentGroup').val(0);
            }
            else {
                $('#txtNewTaxpercentGroup').val(data.Data.TaxPercent);
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveSecondaryUnits() {
    if ($('#chkEnableSecondaryUnit').is(':checked')) {
        if ($('#ddlUnit').val() == '0') {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Select Unit first');

            $('#chkEnableSecondaryUnit').attr('checked', false);
            return;
        }
        $('.divSecondaryUnit').show();
        var det = {
            UnitId: $('#ddlUnit').val()
        };
        _PageIndex = det.PageIndex;
        $("#divLoading").show();
        $.ajax({
            url: '/itemsettings/ActiveSecondaryUnits',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#ddlSecondaryUnit").html('');
                //$("#ddlSecondaryUnit").append($("<option></option>").val(0).html('Select'));
                $.each(data.Data.SecondaryUnits, function (i, value) {
                    $("#ddlSecondaryUnit").append($("<option></option>").val(value.SecondaryUnitId).html(value.SecondaryUnitShortName));
                });
                $('.txtUnit').text('1 ' + $("#ddlUnit").children("option").filter(":selected").text() + ' =');
                $('.txtSecondaryUnit').text($("#ddlSecondaryUnit").children("option").filter(":selected").text());

                $('#ddlPriceAddedFor').html('');
                $('#ddlPriceAddedFor').append($('<option selected="selected" value="3">Unit</option><option value="4"> Secondary Unit</option>'));

                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
    else {
        $('.divSecondaryUnit').hide();
        $('#ddlSecondaryUnit').html('');
        $('#txtUToSValue').val(0);

        $('.divTertiaryUnit').hide();
        $('#chkEnableTertiaryUnit').prop('checked', false);
        $('#ddlTertiaryUnit').html('');
        $('#txtSToTValue').val(0);

        $('.divQuaternaryUnit').hide();
        $('#chkEnableQuaternaryUnit').prop('checked', false);
        $('#ddlQuaternaryUnit').html('');
        $('#txtTToQValue').val(0);

        $('#ddlPriceAddedFor').html('');
        $('#ddlPriceAddedFor').append($('<option selected="selected" value="4">Unit</option>'));
    }
};

function fetchActiveTertiaryUnits() {
    if ($('#chkEnableTertiaryUnit').is(':checked')) {
        if ($('#ddlSecondaryUnit').val() == '0') {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Select Secondary Unit first');

            $('#chkEnableTertiaryUnit').attr('checked', false);
            return;
        }
        $('.divTertiaryUnit').show();
        var det = {
            SecondaryUnitId: $('#ddlSecondaryUnit').val()
        };
        _PageIndex = det.PageIndex;
        $("#divLoading").show();
        $.ajax({
            url: '/itemsettings/ActiveTertiaryUnits',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#ddlTertiaryUnit").html('');
                //$("#ddlTertiaryUnit").append($("<option></option>").val(0).html('Select'));
                $.each(data.Data.TertiaryUnits, function (i, value) {
                    $("#ddlTertiaryUnit").append($("<option></option>").val(value.TertiaryUnitId).html(value.TertiaryUnitShortName));
                });
                $('.txtSecondaryUnit_').text('1 ' + $("#ddlSecondaryUnit").children("option").filter(":selected").text() + ' =');
                $('.txtTertiaryUnit').text($("#ddlTertiaryUnit").children("option").filter(":selected").text());

                $('#ddlPriceAddedFor').html('');
                $('#ddlPriceAddedFor').append($('<option selected="selected" value="2">Unit</option><option value="3"> Secondary Unit</option> <option value="4">Tertiary Unit</option>'));

                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
    else {
        $('.txtSecondaryUnit').text($("#ddlSecondaryUnit").children("option").filter(":selected").text());
        $('.divTertiaryUnit').hide();
        $('#ddlTertiaryUnit').html('');
        $('#txtSToTValue').val(0);

        $('.divQuaternaryUnit').hide();
        $('#chkEnableQuaternaryUnit').prop('checked', false);
        $('#ddlQuaternaryUnit').html('');
        $('#txtTToQValue').val(0);

        $('#ddlPriceAddedFor').html('');
        $('#ddlPriceAddedFor').append($('<option selected="selected" value="3">Unit</option><option value="4"> Secondary Unit</option>'));
    }
}

function fetchActiveQuaternaryUnits() {
    if ($('#chkEnableQuaternaryUnit').is(':checked')) {
        if ($('#ddlTertiaryUnit').val() == '0') {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Select Tertiary Unit first');

            $('#chkEnableQuaternaryUnit').attr('checked', false);
            return;
        }
        $('.divQuaternaryUnit').show();
        var det = {
            TertiaryUnitId: $('#ddlTertiaryUnit').val()
        };
        _PageIndex = det.PageIndex;
        $("#divLoading").show();
        $.ajax({
            url: '/itemsettings/ActiveQuaternaryUnits',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#ddlQuaternaryUnit").html('');
                //$("#ddlQuaternaryUnit").append($("<option></option>").val(0).html('Select'));
                $.each(data.Data.QuaternaryUnits, function (i, value) {
                    $("#ddlQuaternaryUnit").append($("<option></option>").val(value.QuaternaryUnitId).html(value.QuaternaryUnitName));
                });
                $('.txtTertiaryUnit_').text('1 ' + $("#ddlTertiaryUnit").children("option").filter(":selected").text() + ' =');
                $('.txtQuaternaryUnit').text($("#ddlQuaternaryUnit").children("option").filter(":selected").text());

                $('#ddlPriceAddedFor').html('');
                $('#ddlPriceAddedFor').append($('<option selected="selected" value="1">Unit</option><option value="2"> Secondary Unit</option> <option value="3">Tertiary Unit</option><option value="4">Quaternary Unit</option>'));

                $("#divLoading").hide();
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
    else {
        $('.txtTertiaryUnit').text($("#ddlTertiaryUnit").children("option").filter(":selected").text());
        $('.divQuaternaryUnit').hide();
        $('#ddlQuaternaryUnit').html('');
        $('#txtTToQValue').val(0);

        $('#ddlPriceAddedFor').html('');
        $('#ddlPriceAddedFor').append($('<option selected="selected" value="2">Unit</option><option value="3"> Secondary Unit</option> <option value="4">Tertiary Unit</option>'));
    }
}

function toggleQuaternaryUnit() {
    $('.txtQuaternaryUnit').text($("#ddlQuaternaryUnit").children("option").filter(":selected").text());
}

function updateOpeningStockSubTotal(c) {
    $('#txtSubTotal' + c).text((parseFloat($('#txtQuantity' + c).val()) * parseFloat($('#txtUnitCost' + c).val())).toFixed(2));
}

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
                $('#ddlUnit').append($('<option>', { value: data.Data.Unit.UnitId, text: data.Data.Unit.UnitShortName }));
                $('#ddlUnit').val(data.Data.Unit.UnitId);
                $('#unitModal').modal('toggle');

                $('#ddlSecondaryUnit').html('');
                $('#ddlTertiaryUnit').html('');
                $('#ddlQuaternaryUnit').html('');


                $('#txtUnitName').val('');
                $('#txtUnitShortName').val('');

            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertSecondaryUnit() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        SecondaryUnitName: $('#txtSecondaryUnitName').val(),
        UnitId: $('#ddlUnit').val(),
        SecondaryUnitShortName: $('#txtSecondaryUnitShortName').val(),
        SecondaryUnitAllowDecimal: $('#ddlAllowDecimal').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/SecondaryUnitInsert',
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
                $('#ddlSecondaryUnit').append($('<option>', { value: data.Data.SecondaryUnit.SecondaryUnitId, text: data.Data.SecondaryUnit.SecondaryUnitShortName }));
                $('#ddlSecondaryUnit').val(data.Data.SecondaryUnit.SecondaryUnitId);
                $('#secondaryUnitModal').modal('toggle');

                $('.txtSecondaryUnit').text(data.Data.SecondaryUnit.SecondaryUnitShortName);

                $('#ddlTertiaryUnit').html('');
                $('#ddlQuaternaryUnit').html('');

                $('#txtSecondaryUnitName').val('');
                $('#txtSecondaryUnitShortName').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertTertiaryUnit() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        UnitId: $('#ddlUnit').val(),
        SecondaryUnitId: $('#ddlSecondaryUnit').val(),
        TertiaryUnitName: $('#txtTertiaryUnitName').val(),
        TertiaryUnitShortName: $('#txtTertiaryUnitShortName').val(),
        TertiaryUnitAllowDecimal: $('#ddlAllowDecimal').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/TertiaryUnitInsert',
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
                $('#ddlTertiaryUnit').append($('<option>', { value: data.Data.TertiaryUnit.TertiaryUnitId, text: data.Data.TertiaryUnit.TertiaryUnitShortName }));
                $('#ddlTertiaryUnit').val(data.Data.TertiaryUnit.TertiaryUnitId);
                $('#tertiaryUnitModal').modal('toggle');

                $('.txtTertiaryUnit').html(data.Data.TertiaryUnit.TertiaryUnitShortName);

                $('#ddlQuaternaryUnit').html('');

                $('#txtTertiaryUnitName').val('');
                $('#txtTertiaryUnitShortName').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertQuaternaryUnit() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        UnitId: $('#ddlUnit').val(),
        SecondaryUnitId: $('#ddlSecondaryUnit').val(),
        TertiaryUnitId: $('#ddlTertiaryUnit').val(),
        QuaternaryUnitName: $('#txtQuaternaryUnitName').val(),
        QuaternaryUnitShortName: $('#txtQuaternaryUnitShortName').val(),
        QuaternaryUnitAllowDecimal: $('#ddlAllowDecimal').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/QuaternaryUnitInsert',
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
                $('#ddlQuaternaryUnit').append($('<option>', { value: data.Data.QuaternaryUnit.QuaternaryUnitId, text: data.Data.QuaternaryUnit.QuaternaryUnitShortName }));
                $('#ddlQuaternaryUnit').val(data.Data.QuaternaryUnit.QuaternaryUnitId);
                $('#quaternaryUnitModal').modal('toggle');

                $('.txtQuaternaryUnit').text(data.Data.QuaternaryUnit.QuaternaryUnitShortName);

                $('#txtQuaternaryUnitName').val('');
                $('#txtQuaternaryUnitShortName').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openSecondaryUnitModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if ($('#ddlUnit').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Unit first');
    }
    else {
        $('#secondaryUnitModal').modal('toggle');
    }
}

function openTertiaryUnitModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    if ($('#ddlUnit').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Unit first');
    }
    else {
        if ($('#ddlSecondaryUnit').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select Secondary Unit first');
        }
        else {
            $('#tertiaryUnitModal').modal('toggle');
        }
    }
}

function openQuaternaryUnitModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    if ($('#ddlUnit').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Unit first');
    }
    else {
        if ($('#ddlSecondaryUnit').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select Secondary Unit first');
        }
        else {
            if ($('#ddlSecondaryUnit').val() == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Please select Tertiary Unit first');
            }
            else {
                $('#quaternaryUnitModal').modal('toggle');
            }
        }
    }
}

function openSubCategoryModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    if ($('#ddlCategory').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Category first');
    }
    else {
        $('#subCategoryModal').modal('toggle');
    }
}

function openSubSubCategoryModal() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    if ($('#ddlCategory').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Category first');
    }
    else {
        if ($('#ddlSubCategory').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select Sub Category first');
        }
        else {
            $('#subSubCategoryModal').modal('toggle');
        }
    }
}

function init() {
    $('#singleproduct').show();
    $('#variableproduct').hide();
    $('#comboproduct').hide();
}

function fetchVariations() {
    var det = {

    };
    //_PageIndex = det.PageIndex;
    //$("#divLoading").show();
    $.ajax({
        url: '/itemsettings/Activevariations',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            var dropdown = '';
            for (let i = 0; i < data.Data.Variations.length; i++) {
                dropdown = dropdown + '<option value="' + data.Data.Variations[i].VariationId + '">' + data.Data.Variations[i].Variation + '</option>';
            }
            dropdownHtml = dropdown;
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveAccountsDropdown() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/accounts/ActiveAccountsDropdown',
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
                accountSubTypes = data.Data.AccountSubTypes;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveSubCategories() {
    var det = {
        CategoryId: $('#ddlCategory').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubCategories_Dropdown").html(data);

            //if (window.location.href.toLowerCase().indexOf('itemsedit') == -1 && window.location.href.toLowerCase().indexOf('itemsduplicate') == -1
            //    && window.location.href.toLowerCase().indexOf('itemsadd') == -1) {
            //    $('.hideButton').hide();
            //    $('.select2').css('width', '100%');
            //}
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveSubSubCategories() {
    var det = {
        SubCategoryId: $('#ddlSubCategory').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveSubSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubSubCategories_Dropdown").html(data);

            //if (window.location.href.toLowerCase().indexOf('itemsedit') == -1 && window.location.href.toLowerCase().indexOf('itemsduplicate') == -1
            //    && window.location.href.toLowerCase().indexOf('itemsadd') == -1) {
            //    $('.hideButton').hide();
            //    $('.select2').css('width', '100%');
            //}
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getProductImageBase64() {
    var file1 = $("#ProductImage").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProductImage").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            ProductImage = reader.result;
            fileExtensionProductImage = '.' + file1.name.split('.').pop();

            $('#blahProductImage').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function getProductBrochureBase64() {
    var file1 = $("#ProductBrochure").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProductBrochure").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            ProductBrochure = reader.result;
            fileExtensionProductBrochure = '.' + file1.name.split('.').pop();

            $('#blahProductBrochure').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductBrochure = error;
        };
    }
}

function getImageBase64(id) {
    var file1 = $("#ProductImage" + id).prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProductImage" + 0).val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            $('#txtHiddenFile' + id).val(reader.result);
            $('#txtHiddenFileExtension' + id).val('.' + file1.name.split('.').pop());

            $('#blahProductImage' + id).attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
}

function checkTaxType() {
    if ($('#ddlTaxType').val() == 'Inclusive') {
        $('.divSellingPriceExcTax').hide();
        $('.divSellingPriceIncTax').show();
        $('.lblTaxName').text('Inc. Tax');
    }
    else {
        $('.divSellingPriceExcTax').show();
        $('.divSellingPriceIncTax').hide();
        $('.lblTaxName').text('Exc. Tax');
    }

    if ($('#ddlProductType').val() == 'Single') {
        UpdateSingleAmount('PExc');
    }
    else if ($('#ddlProductType').val() == 'Variable') {
        UpdateVariationAmount(1);
    }
}

function AddNewVariation() {
    var incomeAccountSubTypes = '', expenseAccountSubTypes = '', inventoryAccountSubTypes = '';
    for (let i = 0; i < accountSubTypes.length; i++) {
        if (accountSubTypes[i].Accounts.length > 0) {
            if (accountSubTypes[i].AccountType == "Income") {
                incomeAccountSubTypes = incomeAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Sales") {
                        incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                }
                '</optgroup>';
            }
            else if (accountSubTypes[i].AccountType == "Expense") {
                expenseAccountSubTypes = expenseAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Cost of Goods Sold") {
                        expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                }
                '</optgroup>';
            }
            else if (accountSubTypes[i].Type == "Stock") {
                inventoryAccountSubTypes = inventoryAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Inventory Asset") {
                        inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }

                }
                '</optgroup>';
            }
        }
    }

    var IsPurchaseAddon = $('#hdnIsPurchaseAddon').val().toLocaleLowerCase();
    var IsAccountsAddon = $('#hdnIsAccountsAddon').val().toLocaleLowerCase();
    var EnableMrp = $('#hdnEnableMrp').val().toLocaleLowerCase();

    var VariationPermissionIsAdd = $('#hdnVariationPermissionIsAdd').val().toLocaleLowerCase();
    var html = '<table class="table table-bordered add-product-price-table table-condensed" id="product_variation_form_part' + count + '">' +
        '<thead>' +
        '<tr>' +
        '<th class="col-12 col-sm-6 col-md-4 col-lg-3 col-xl-2">Variation</th>' +
        '<th class="col-sm-10">Variation Values <button type="button" class="btn btn-danger btn-sm float-right" onclick="deleteVariable(' + count + ')"><i class="fas fa-times"></i></button></th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>' +
        '<tr class="variation_row">' +
        '<td style="min-width:150px" class="divVariation' + count + '_ctrl">' +
        '<select onchange="FetchVariationDetails(' + count + ')" id="ddlVariation' + count + '" class="form-control input-sm variation_template valid select2" required="" name="product_variation[0][variation_template_id]" aria-required="true" aria-invalid="false">' +
        /*'<option value="0">Select</option>' +*/
        dropdownHtml +
        '</select><a href="javascript:void(0)" class="btn-xs ml-2 text-decoration-none float-right font-weight-bold ' + (VariationPermissionIsAdd == 'true' ? '' : 'hidden') + '" onclick="openVariationModal(' + count + ')"><i class="fas fa-plus"></i> Add New</a>' +
        '<small class="text-red font-weight-bold errorText" id="divVariation' + count + '"></small>' +
        '</td>' +
        '<td>' +
        '<table class="table table-bordered table-striped blue-header variation_value_table">' +
        '<thead>' +
        '<tr>' +
        '<th class="divInventoryAccount service" ' + ((IsPurchaseAddon == 'true' && IsAccountsAddon == 'true') ? '' : 'hidden') + '>Inventory Account </th>' +
        '<th>SKU <span class="danger">*</span> <i tabindex="0" class="fa fa-info-circle text-info hover-q no-print" ' + (ShowHelpText == "True" ? "" : "hidden") + ' role="button" data-toggle="popover" data-trigger="focus" title="" data-content="Leave blank to auto generate"></i></th>' +
        '<th>Value <span class="danger">*</span></th>' +
        '<th class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '>' +
        'Purchase Price <span class="danger">*</span><br /> <small class="lblTaxName"></small>' +
        '</th>' +
        '<th class="" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
        'Purchase Account' +
        '</th>' +
        '<th class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '> Profit Margin(%)</th>' +
        '<th class="">' +
        'Selling Price <span class="danger">*</span><br /> <small class="lblTaxName"></small>' +
        '</th>' +
        '<th class="" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
        'Sales Account<br>' +
        '</th>' +
        '<th class="" ' + (EnableMrp == 'true' ? '' : 'hidden') + '> MRP </th>' +
        '<th> Variation Images</th>' +
        '<th> <button type="button" class="btn btn-success btn-sm" onclick="AddNewInnerVariation(' + count + ')"><i class="fas fa-plus"></i></button></th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="divInnerVariation' + count + '">' +
        //'<tr id="product_variation_form_part_inner' + innerCount + '">' +
        //'<td style="min-width:150px" class="divInventoryAccount ' + ((IsPurchaseAddon == 'true' && IsAccountsAddon == 'true') ? '' : 'hidden') + '">' +
        //'<div class="row">' +
        //'<div class="col-sm-12" >' +
        //'<div class="form-group">' +
        //'<div class="input-group">' +
        //'<select class="form-control select2" id="ddlInventoryAccount' + innerCount + '">' +
        //inventoryAccountSubTypes +
        //'</select>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</td>' +
        //'<td style="min-width:150px">' +
        //'<input class="form-control input-sm input_sub_sku" name="product_variation[0][variations][0][sub_sku]" type="text" placeholder="" id="txtSku' + innerCount + '">' +
        //'<small class="text-red font-weight-bold errorText" id="divSku' + innerCount + '"></small>' +
        //'</td>' +
        //'<td style="min-width:150px">' +
        //'<input class="form-control input-sm variation_value_name" required="" name="product_variation" type="text" value="" id="txtValue' + innerCount + '" placeholder=""> ' +
        //'<input name="product_variation[0][variations][0][variation_value_id]" type="hidden" id="hdnValue' + innerCount + '" value="0">' +
        //'<small class="text-red font-weight-bold errorText" id="divValue' + innerCount + '"></small>' +
        //'</td>' +
        //'<td style="min-width:150px" class="' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '">' +
        //'<div class="row">' +
        //'<div class="col-12 ">' +
        //'<input class="form-control input-sm variable_dpp input_number" placeholder="" required="" name="product_variation" type="number" id="txtPurchaseExcTax' + innerCount + '" onchange="UpdateVariationAmount(1,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divPurchaseExcTax' + innerCount + '"></small>' +
        //'</div>' +
        ////'<div class="col-6 ">' +
        ////'<input class="form-control input-sm variable_dpp_inc_tax input_number" placeholder="Inc. Tax" required="" name="product_variation" type="number" id="txtPurchaseIncTax' + innerCount + '" onchange="UpdateVariationAmount(2,' + innerCount + ')">' +
        ////'<small class="text-red font-weight-bold errorText" id="divPurchaseIncTax' + innerCount + '"></small>' +
        ////'</div>' +
        //'</div>' +
        //'</td >' +
        //'<td style="min-width:150px" class="' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '">' +
        //'<div class="row">' +
        //'<div class="col-sm-12" >' +
        //'<div class="form-group">' +
        //'<div class="input-group">' +
        //'<select style="min-width:150px" class="form-control select2" id="ddlPurchaseAccount' + innerCount + '">' +
        //expenseAccountSubTypes +
        //'</select>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</td >' +
        //'<td style="min-width:150px" class="' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '">' +
        //'<input class="form-control input-sm variable_profit_percent input_number" required="" name="product_variation" type="number" value="' + $('#hdnDefaultProfitMargin').val() + '" placeholder="" id="txtDefaultProfitMargin' + innerCount + '" onchange="UpdateVariationAmount(3,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divDefaultProfitMargin' + innerCount + '"></small>' +
        //'</td>' +
        //'<td style="min-width:150px">' +
        //'<div class="row">' +
        //'<div class="col-12">' +
        //'<input class="form-control input-sm variable_dsp input_number" placeholder="" required="" name="product_variation" type="number" id="txtSalesExcTax' + innerCount + '" onchange="UpdateVariationAmount(4,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
        //'</div>' +
        //'<div class="col-12">' +
        //'<input class="form-control input-sm variable_dsp input_number" placeholder="" required="" name="product_variation" type="number" id="txtSalesIncTax' + innerCount + '" onchange="UpdateVariationAmount(5,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
        //'</div>' +
        //'</td>' +
        //'<td style="min-width:200px" class="' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '">' +
        //'<div class="row">' +
        //'<div class="col-sm-12" >' +
        //'<div class="form-group" id="divFTAccount">' +
        //'<div class="input-group">' +
        //'<select style="min-width:50px" class="form-control select2" id="ddlSalesAccount' + innerCount + '">' +
        //incomeAccountSubTypes +
        //'</select>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</div>' +
        //'</td >' +
        //'<td style="min-width:200px" class="' + (EnableMrp == 'true' ? '' : 'hidden') + '">' +
        //'<input class="form-control input-sm variable_profit_percent input_number" required="" name="product_variation" type="number" placeholder="" id="txtDefaultMrp' + innerCount + '">' +
        //'<small class="text-red font-weight-bold errorText" id="divDefaultMrp' + innerCount + '"></small>' +
        //'</td>' +
        //'<td style="min-width:300px">' +
        //'<div class="custom-file">' +
        //'<input type="file" class="custom-file-input" id="ProductImage' + innerCount + '" accept=".png,.jpg,.jpeg" onchange="getImageBase64(' + innerCount + ')">' +
        //'<input type="hidden" id="txtHiddenFile' + innerCount + '">' +
        //'<input type="hidden" id="txtHiddenFileExtension' + innerCount + '">' +
        //'<label class="custom-file-label" for="ProductImage' + innerCount + '"> Choose file</label>' +
        //'</div></br>' +
        //'<img id="blahProductImage' + innerCount + '" alt="" height="60" style="padding-top:10px;">' +
        //'</td>' +
        //'<td >' +
        //'<button type="button" class="btn btn-danger btn-sm" onclick="deleteInnerVariable(' + innerCount + ')">' +
        //'<i class="fas fa-times">' +
        //'</i>' +
        //'</button>' +
        //'</td>' +
        //'</tr>' +
        '</tbody>' +
        '</table>' +
        '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>';
    $('#divVariation').append(html);
    FetchVariationDetails(count);
    count++;
    innerCount++;
    //checkTaxType();

    $('.select2').select2();
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
}

function AddNewInnerVariation(id) {
    var incomeAccountSubTypes = '', expenseAccountSubTypes = '', inventoryAccountSubTypes = '';
    for (let i = 0; i < accountSubTypes.length; i++) {
        if (accountSubTypes[i].Accounts.length > 0) {
            if (accountSubTypes[i].AccountType == "Income") {
                incomeAccountSubTypes = incomeAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Sales") {
                        incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                }
                '</optgroup>';
            }
            else if (accountSubTypes[i].AccountType == "Expense") {
                expenseAccountSubTypes = expenseAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Cost of Goods Sold") {
                        expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }

                }
                '</optgroup>';
            }
            else if (accountSubTypes[i].Type == "Stock") {
                inventoryAccountSubTypes = inventoryAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type == "Inventory Asset") {
                        inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }
                    else {
                        inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                    }

                }
                '</optgroup>';
            }
        }
    }

    var IsPurchaseAddon = $('#hdnIsPurchaseAddon').val().toLocaleLowerCase();
    var IsAccountsAddon = $('#hdnIsAccountsAddon').val().toLocaleLowerCase();
    var EnableMrp = $('#hdnEnableMrp').val().toLocaleLowerCase();

    var html = '<tr id="product_variation_form_part_inner' + innerCount + '">' +
        '<td style="min-width:150px" class="divInventoryAccount service" ' + ((IsPurchaseAddon == 'true' && IsAccountsAddon == 'true') ? '' : 'hidden') + '>' +
        '<div class="row">' +
        '<div class="col-sm-12">' +
        '<div class="form-group">' +
        '<div class="input-group">' +
        '<select class="form-control select2" id="ddlInventoryAccount' + innerCount + '">' +
        inventoryAccountSubTypes +
        '</select>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</td>' +
        '<td style="min-width:150px">' +
        '<input class="form-control input-sm input_sub_sku divSku' + innerCount + '_ctrl" name="product_variation[0][variations][0][sub_sku]" type="text" id="txtSku' + innerCount + '">' +
        '<small class="text-red font-weight-bold errorText" id="divSku' + innerCount + '"></small>' +
        '</td>' +
        '<td style="min-width:150px">' +
        '<input class="form-control input-sm variation_value_name divValue' + innerCount + '_ctrl" required="" name="product_variation" type="text" value="" id="txtValue' + innerCount + '"> ' +
        '<input name="product_variation[0][variations][0][variation_value_id]" type="hidden" id="hdnValue' + innerCount + '" value="0">' +
        '<small class="text-red font-weight-bold errorText" id="divValue' + innerCount + '"></small>' +
        '</td>' +
        '<td style="min-width:150px" class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '>' +
        '<div class="row">' +
        '<div class="col-12">' +
        '<input class="form-control input-sm variable_dpp input_number divPurchaseExcTax' + innerCount + '_ctrl" required="" name="product_variation" type="number" id="txtPurchaseExcTax' + innerCount + '" onchange="UpdateVariationAmount(1,' + innerCount + ')">' +
        '<small class="text-red font-weight-bold errorText" id="divPurchaseExcTax' + innerCount + '"></small>' +
        '</div>' +
        //'<div class="col-6 ">' +
        //'<input class="form-control input-sm variable_dpp_inc_tax input_number" required="" name="product_variation" type="number" id="txtPurchaseIncTax' + innerCount + '" onchange="UpdateVariationAmount(2,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divPurchaseIncTax' + innerCount + '"></small>' +
        //'</div>' +
        '</div>' +
        '</td >' +
        '<td style="min-width:150px" class="" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
        '<div class="row">' +
        '<div class="col-sm-12">' +
        '<div class="form-group">' +
        /*'<label> Purchase Account<span class="danger">*</span></label>' +*/
        '<div class="input-group">' +
        '<select class="form-control select2" id="ddlPurchaseAccount' + innerCount + '">' +
        expenseAccountSubTypes +
        '</select>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</td >' +
        '<td class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + ' style="min-width:150px" >' +
        '<input class="form-control input-sm variable_profit_percent input_number divDefaultProfitMargin' + innerCount + '_ctrl" required="" name="product_variation" type="number" value="" id="txtDefaultProfitMargin' + innerCount + '" onchange="UpdateVariationAmount(3,' + innerCount + ')">' +
        '<small class="text-red font-weight-bold errorText" id="divDefaultProfitMargin' + innerCount + '"></small>' +
        '</td>' +
        '<td style="min-width:150px">' +
        '<div class="row">' +
        '<div class="col-12">' +
        '<input class="form-control input-sm variable_dsp input_number divSalesTax' + innerCount + '_ctrl" required="" name="product_variation" type="number" id="txtSalesExcTax' + innerCount + '" onchange="UpdateVariationAmount(4,' + innerCount + ')">' +
        '<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
        '</div>' +
        //'<div class="col-12 divSellingPriceIncTax">' +
        //'<input class="form-control input-sm variable_dsp input_number divSellingPriceIncTax" required="" name="product_variation" type="number" id="txtSalesIncTax' + innerCount + '" onchange="UpdateVariationAmount(5,' + innerCount + ')">' +
        //'<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
        //'</div>' +
        '</div>' +
        '</td>' +
        '<td style="min-width:150px" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
        '<div class="row">' +
        '<div class="col-sm-12">' +
        '<div class="form-group" id="divFTAccount">' +
        /*'<label> Sales Account<span class="danger">*</span></label>' +*/
        '<div class="input-group">' +
        '<select class="form-control select2" id="ddlSalesAccount' + innerCount + '">' +
        incomeAccountSubTypes +
        '</select>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</td>' +
        '<td style="min-width:150px" class="" ' + (EnableMrp == 'true' ? '' : 'hidden') + '>' +
        '<input class="form-control input-sm variable_profit_percent input_number" required="" name="product_variation" type="number" placeholder="" id="txtDefaultMrp' + innerCount + '">' +
        '<small class="text-red font-weight-bold errorText" id="divDefaultMrp' + innerCount + '"></small>' +
        '</td>' +
        '<td style="min-width:200px">' +
        '<div class="custom-file">' +
        '<input type="file" class="custom-file-input" id="ProductImage' + innerCount + '" accept=".png,.jpg,.jpeg" onchange="getImageBase64(' + innerCount + ')">' +
        '<input type="hidden" id="txtHiddenFile' + innerCount + '">' +
        '<input type="hidden" id="txtHiddenFileExtension' + innerCount + '">' +
        '<label class="custom-file-label" for="ProductImage' + innerCount + '"> Choose file</label>' +
        '</div></br>' +
        '<img id="blahProductImage' + innerCount + '" alt="" height="60" style="padding-top:10px;">' +
        '</td>' +
        '<td>' +
        '<button type="button" class="btn btn-danger btn-sm" onclick="deleteInnerVariable(' + innerCount + ')">' +
        '<i class="fas fa-times">' +
        '</i>' +
        '</button>' +
        '</td>' +
        '</tr>';
    $('#divInnerVariation' + id).append(html);
    innerCount++;
    //checkTaxType();
    checkItemType();
    $('.select2').select2();
}

function FetchVariationDetails(id) {
    var det = {
        VariationId: $('#ddlVariation' + id).val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveVariationDetails',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $('#divInnerVariation' + id).empty();

            var incomeAccountSubTypes = '', expenseAccountSubTypes = '', inventoryAccountSubTypes = '';
            for (let i = 0; i < accountSubTypes.length; i++) {
                if (accountSubTypes[i].Accounts.length > 0) {
                    if (accountSubTypes[i].AccountType == "Income") {
                        incomeAccountSubTypes = incomeAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                        for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                            if (accountSubTypes[i].Accounts[j].Type == "Sales") {
                                incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }
                            else {
                                incomeAccountSubTypes = incomeAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }
                        }
                        '</optgroup>';
                    }
                    else if (accountSubTypes[i].AccountType == "Expense") {
                        expenseAccountSubTypes = expenseAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                        for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                            if (accountSubTypes[i].Accounts[j].Type == "Cost of Goods Sold") {
                                expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }
                            else {
                                expenseAccountSubTypes = expenseAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }

                        }
                        '</optgroup>';
                    }
                    else if (accountSubTypes[i].Type == "Stock") {
                        inventoryAccountSubTypes = inventoryAccountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                        for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                            if (accountSubTypes[i].Accounts[j].Type == "Inventory Asset") {
                                inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '" selected>' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }
                            else {
                                inventoryAccountSubTypes = inventoryAccountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].AccountName + '</option>';
                            }
                        }
                        '</optgroup>';
                    }
                }
            }

            var IsPurchaseAddon = $('#hdnIsPurchaseAddon').val().toLocaleLowerCase();
            var IsAccountsAddon = $('#hdnIsAccountsAddon').val().toLocaleLowerCase();
            var EnableMrp = $('#hdnEnableMrp').val().toLocaleLowerCase();

            var html = '';
            for (let i = 0; i < data.Data.VariationDetails.length; i++) {
                html = html + '<tr id="product_variation_form_part_inner' + innerCount + '">' +
                    '<td style="min-width:150px" class="divInventoryAccount service" ' + ((IsPurchaseAddon == 'true' && IsAccountsAddon == 'true') ? '' : 'hidden') + '>' +
                    '<div class="row">' +
                    '<div class="col-sm-12" >' +
                    '<div class="form-group">' +
                    '<div class="input-group">' +
                    '<select class="form-control select2" id="ddlInventoryAccount' + innerCount + '">' +
                    inventoryAccountSubTypes +
                    '</select>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</td>' +
                    '<td style="min-width:150px">' +
                    '<input class="form-control input-sm input_sub_sku divSku' + innerCount + '_ctrl" name="product_variation[0][variations][0][sub_sku]" type="text" id="txtSku' + innerCount + '">' +
                    '<small class="text-red font-weight-bold errorText" id="divSku' + innerCount + '"></small>' +
                    '</td>' +
                    '<td style="min-width:150px">' +
                    '<input class="form-control input-sm variation_value_name divValue' + innerCount + '_ctrl" required="" disabled name="product_variation" type="text" id="txtValue' + innerCount + '" value="' + data.Data.VariationDetails[i].VariationDetails + '">' +
                    '<input name="product_variation[0][variations][0][variation_value_id]" type="hidden" id="hdnValue' + innerCount + '" value="' + data.Data.VariationDetails[i].VariationDetailsId + '">' +
                    '<small class="text-red font-weight-bold errorText" id="divValue' + innerCount + '"></small>' +
                    '</td>' +
                    '<td style="min-width:150px" class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + '>' +
                    '<div class="row">' +
                    '<div class="col-12 ">' +
                    '<input class="form-control input-sm variable_dpp input_number divPurchaseExcTax' + innerCount + '_ctrl" required="" name="product_variation" type="number" id="txtPurchaseExcTax' + innerCount + '" onchange="UpdateVariationAmount(1,' + innerCount + ')">' +
                    '<small class="text-red font-weight-bold errorText" id="divPurchaseExcTax' + innerCount + '"></small>' +
                    '</div>' +
                    //'<div class="col-6 ">' +
                    //'<input class="form-control input-sm variable_dpp_inc_tax input_number" required="" name="product_variation" type="number" id="txtPurchaseIncTax' + innerCount + '" onchange="UpdateVariationAmount(2,' + innerCount + ')">' +
                    //'<small class="text-red font-weight-bold errorText" id="divPurchaseIncTax' + innerCount + '"></small>' +
                    //'</div>' +
                    '</div>' +
                    '</td >' +
                    '<td style="min-width:150px" class="" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
                    '<div class="row">' +
                    '<div class="col-sm-12">' +
                    '<div class="form-group">' +
                    '<div class="input-group">' +
                    '<select class="form-control select2" id="ddlPurchaseAccount' + innerCount + '">' +
                    expenseAccountSubTypes +
                    '</select>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</td >' +
                    '<td class="" ' + (IsPurchaseAddon == 'true' ? '' : 'hidden') + ' style="min-width:150px;">' +
                    '<input class="form-control input-sm variable_profit_percent input_number divDefaultProfitMargin' + innerCount + '_ctrl" required="" name="product_variation" type="number" value="' + ($('#hdnDefaultProfitMargin').val() > 0 ? $('#hdnDefaultProfitMargin').val() : "") + '" id="txtDefaultProfitMargin' + innerCount + '" onchange="UpdateVariationAmount(3,' + innerCount + ')">' +
                    '<small class="text-red font-weight-bold errorText" id="divDefaultProfitMargin' + innerCount + '"></small>' +
                    '</td>' +
                    '<td style="min-width:150px">' +
                    '<div class="row">' +
                    /*'<div class="col-12 divSellingPriceExcTax">' +*/
                    '<div class="col-12">' +
                    '<input class="form-control input-sm variable_dsp input_number divSalesTax' + innerCount + '_ctrl" required="" name="product_variation" type="number" id="txtSalesExcTax' + innerCount + '" onchange="UpdateVariationAmount(4,' + innerCount + ')">' +
                    '<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
                    '</div>' +
                    //'<div class="col-12 divSellingPriceIncTax">' +
                    //'<input class="form-control input-sm variable_dsp input_number divSellingPriceIncTax" required="" name="product_variation" type="number" id="txtSalesIncTax' + innerCount + '" onchange="UpdateVariationAmount(5,' + innerCount + ')">' +
                    //'<small class="text-red font-weight-bold errorText" id="divSalesTax' + innerCount + '"></small>' +
                    //'</div>' +
                    '</div>' +
                    '</td>' +
                    '<td style="min-width:150px" ' + (IsAccountsAddon == 'true' ? '' : 'hidden') + '>' +
                    '<div class="row">' +
                    '<div class="col-sm-12">' +
                    '<div class="form-group" id="divFTAccount">' +
                    '<div class="input-group">' +
                    '<select class="form-control select2" id="ddlSalesAccount' + innerCount + '">' +
                    incomeAccountSubTypes +
                    '</select>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</td>' +
                    '<td style="min-width:150px" class="" ' + (EnableMrp == 'true' ? '' : 'hidden') + '>' +
                    '<input class="form-control input-sm variable_profit_percent input_number" required="" name="product_variation" type="number" id="txtDefaultMrp' + innerCount + '">' +
                    '<small class="text-red font-weight-bold errorText" id="divDefaultMrp' + innerCount + '"></small>' +
                    '</td>' +
                    '<td style="min-width:200px">' +
                    '<div class="custom-file">' +
                    '<input type="file" class="custom-file-input" id="ProductImage' + innerCount + '" accept=".png,.jpg,.jpeg" onchange="getImageBase64(' + innerCount + ')">' +
                    '<input type="hidden" id="txtHiddenFile' + innerCount + '">' +
                    '<input type="hidden" id="txtHiddenFileExtension' + innerCount + '">' +
                    '<label class="custom-file-label" for="ProductImage' + innerCount + '"> Choose file</label>' +
                    '</div></br>' +
                    '<img id="blahProductImage' + innerCount + '" alt="" height="60" style="padding-top:10px;">' +
                    '</td>' +
                    '<td>' +
                    '<button type="button" class="btn btn-danger btn-sm" onclick="deleteInnerVariable(' + innerCount + ')">' +
                    '<i class="fas fa-times">' +
                    '</i>' +
                    '</button>' +
                    '</td>' +
                    '</tr>';
                innerCount++;
            }
            $('#divInnerVariation' + id).append(html);
            //checkTaxType();
            checkItemType();
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deleteVariable(id, ItemId, VariationId) {
    if (ItemId != undefined) {
        var r = confirm("Are you sure you want to delete?");
        if (r == true) {

            $('#divVariation table').each(function () {
                var _id = this.id.split('product_variation_form_part')[1];
                if (_id != undefined) {
                    $('#divInnerVariation' + _id + ' tr').each(function () {
                        var _innerid = this.id.split('product_variation_form_part_inner')[1];
                        $('#product_variation_form_part' + id).hide();
                    })
                }
            });

            //var det = {
            //    ItemId: ItemId,
            //    VariationId: VariationId
            //}
            //$("#divLoading").show();
            //$.ajax({
            //    url: '/items/itemDetailsDelete',
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
            //            $('#product_variation_form_part' + id).remove();
            //        }
            //    },
            //    error: function (xhr) {
            //        $("#divLoading").hide();
            //    }
            //});
        }
    }
    else {
        $('#product_variation_form_part' + id).remove();
    }
}

function deleteInnerVariable(id, ItemDetailsId) {
    if (ItemDetailsId != undefined) {
        var r = confirm("Are you sure you want to delete?");
        if (r == true) {

            $('#product_variation_form_part_inner' + id).hide();

            //var det = {
            //    ItemDetailsId: ItemDetailsId
            //}
            //$("#divLoading").show();
            //$.ajax({
            //    url: '/items/itemDetailsDelete',
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
            //            $('#product_variation_form_part_inner' + id).remove();
            //        }
            //    },
            //    error: function (xhr) {
            //        $("#divLoading").hide();
            //    }
            //});
        }
    }
    else {
        $('#product_variation_form_part_inner' + id).remove();
    }
}

function UpdateVariationAmount(type, id) {
    var IsPurchaseAddon = $('#hdnIsPurchaseAddon').val().toLocaleLowerCase();

    if (id == undefined) {
        $('#divVariation table').each(function () {
            var _id = this.id.split('product_variation_form_part')[1];
            if (_id != undefined) {
                $('#divInnerVariation' + _id + ' tr').each(function () {
                    var _innerid = this.id.split('product_variation_form_part_inner')[1];
                    var PurchaseExcTax = parseFloat($('#txtPurchaseExcTax' + _innerid).val());
                    var PurchaseIncTax = $('#txtPurchaseIncTax' + _innerid).val() == '' ? 0 : parseFloat($('#txtPurchaseIncTax' + _innerid).val());
                    var DefaultProfitMargin = $('#txtDefaultProfitMargin' + _innerid).val() == '' ? 0 : parseFloat($('#txtDefaultProfitMargin' + _innerid).val());
                    var SalesIncTax = $('#txtSalesIncTax' + _innerid).val() == '' ? 0 : parseFloat($('#txtSalesIncTax' + _innerid).val());
                    var SalesExcTax = $('#txtSalesExcTax' + _innerid).val() == '' ? 0 : parseFloat($('#txtSalesExcTax' + _innerid).val());
                    var taxPer = 0;
                    if ($('#ddlTax').val() != 0) {
                        taxPer = $("#ddlTax").val().split('-')[1];
                    }

                    if (IsPurchaseAddon == 'false') {
                        if ($('#ddlTaxType').val() == 'Exclusive') {
                            $('#txtSalesIncTax' + _innerid).val((((taxPer / 100) * SalesExcTax) + SalesExcTax).toFixed(2));
                        }
                        else {
                            $('#txtSalesExcTax' + _innerid).val(((SalesIncTax * 100) / (100 + parseFloat(taxPer))).toFixed(2));
                        }
                    }
                    else {
                        if (type == '1') {
                            if (taxPer == 0) {
                                PurchaseIncTax = PurchaseExcTax;
                            }
                            else {
                                PurchaseIncTax = ((taxPer / 100) * PurchaseExcTax) + PurchaseExcTax;
                            }
                            $('#txtPurchaseIncTax' + _innerid).val(PurchaseIncTax.toFixed(2));
                            $('#txtSalesIncTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                            $('#txtSalesExcTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
                        }
                        else if (type == '3') {
                            $('#txtSalesIncTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                            $('#txtSalesExcTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
                        }
                        else if (type == '2') {
                            PurchaseExcTax = (PurchaseIncTax * 100) / (100 + parseFloat(taxPer));
                            $('#txtPurchaseExcTax' + _innerid).val(PurchaseExcTax.toFixed(2));
                            $('#txtSalesIncTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                            $('#txtSalesExcTax' + _innerid).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
                        }
                        else {
                            if ($('#ddlTaxType').val() == 'Exclusive') {
                                DefaultProfitMargin = ((SalesExcTax - PurchaseExcTax) / PurchaseExcTax) * 100;
                            }
                            else {
                                DefaultProfitMargin = ((SalesIncTax - PurchaseIncTax) / PurchaseIncTax) * 100;
                            }
                            $('#txtDefaultProfitMargin' + _innerid).val(DefaultProfitMargin.toFixed(2));
                        }
                    }

                });
            }
        });
    }
    else {
        var PurchaseExcTax = parseFloat($('#txtPurchaseExcTax' + id).val());
        var PurchaseIncTax = $('#txtPurchaseIncTax' + id).val() == '' ? 0 : parseFloat($('#txtPurchaseIncTax' + id).val());
        var DefaultProfitMargin = $('#txtDefaultProfitMargin' + id).val() == '' ? 0 : parseFloat($('#txtDefaultProfitMargin' + id).val());
        var SalesIncTax = $('#txtSalesIncTax' + id).val() == '' ? 0 : parseFloat($('#txtSalesIncTax' + id).val());
        var SalesExcTax = $('#txtSalesExcTax' + id).val() == '' ? 0 : parseFloat($('#txtSalesExcTax' + id).val());
        var taxPer = 0;
        if ($('#ddlTax').val() != 0) {
            taxPer = $("#ddlTax").val().split('-')[1];
        }

        if (IsPurchaseAddon == 'false') {
            if ($('#ddlTaxType').val() == 'Exclusive') {
                $('#txtSalesIncTax' + id).val((((taxPer / 100) * SalesExcTax) + SalesExcTax).toFixed(2));
            }
            else {
                $('#txtSalesExcTax' + id).val(((SalesIncTax * 100) / (100 + parseFloat(taxPer))).toFixed(2));
            }
        }
        else {
            if (type == '1') {
                if (taxPer == 0) {
                    PurchaseIncTax = PurchaseExcTax;
                }
                else {
                    PurchaseIncTax = ((taxPer / 100) * PurchaseExcTax) + PurchaseExcTax;
                }
                $('#txtPurchaseIncTax' + id).val(PurchaseIncTax.toFixed(2));
                $('#txtSalesIncTax' + id).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                $('#txtSalesExcTax' + id).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
            }
            else if (type == '3') {
                $('#txtSalesIncTax' + id).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                $('#txtSalesExcTax' + id).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
            }
            else if (type == '2') {
                PurchaseExcTax = (PurchaseIncTax * 100) / (100 + parseFloat(taxPer));
                $('#txtPurchaseExcTax' + id).val(PurchaseExcTax.toFixed(2));
                $('#txtSalesIncTax' + id).val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
                $('#txtSalesExcTax' + id).val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
            }
            else {
                if ($('#ddlTaxType').val() == 'Exclusive') {
                    DefaultProfitMargin = ((SalesExcTax - PurchaseExcTax) / PurchaseExcTax) * 100;
                }
                else {
                    DefaultProfitMargin = ((SalesIncTax - PurchaseIncTax) / PurchaseIncTax) * 100;
                }
                $('#txtDefaultProfitMargin' + id).val(DefaultProfitMargin.toFixed(2));
            }
        }
    }
}

function UpdateSingleAmount(type) {
    var IsPurchaseAddon = $('#hdnIsPurchaseAddon').val().toLocaleLowerCase();
    var PurchaseExcTax = parseFloat($('#txtPurchaseExcTax').val());
    var PurchaseIncTax = $('#txtPurchaseIncTax').val() == '' ? 0 : parseFloat($('#txtPurchaseIncTax').val());
    var DefaultProfitMargin = $('#txtDefaultProfitMargin').val() == '' ? 0 : parseFloat($('#txtDefaultProfitMargin').val());
    var SalesIncTax = $('#txtSalesIncTax').val() == '' ? 0 : parseFloat($('#txtSalesIncTax').val());
    var SalesExcTax = $('#txtSalesExcTax').val() == '' ? 0 : parseFloat($('#txtSalesExcTax').val());
    var taxPer = 0;
    if ($('#ddlTax').val() != 0) {
        taxPer = $("#ddlTax").val().split('-')[1];
    }

    if (IsPurchaseAddon == 'false') {
        if ($('#ddlTaxType').val() == 'Exclusive') {
            $('#txtSalesIncTax').val((((taxPer / 100) * SalesExcTax) + SalesExcTax).toFixed(2));
        }
        else {
            $('#txtSalesExcTax').val(((SalesIncTax * 100) / (100 + parseFloat(taxPer))).toFixed(2));
        }
    }
    else {
        if (type == 'PExc') {
            if (taxPer == 0) {
                PurchaseIncTax = PurchaseExcTax;
            }
            else {
                PurchaseIncTax = ((taxPer / 100) * PurchaseExcTax) + PurchaseExcTax;
            }
            $('#txtPurchaseIncTax').val(PurchaseIncTax.toFixed(2));
            $('#txtSalesIncTax').val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
            $('#txtSalesExcTax').val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
        }
        else if (type == 'Margin') {
            $('#txtSalesIncTax').val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
            $('#txtSalesExcTax').val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
        }
        else if (type == 'PInc') {
            PurchaseExcTax = (PurchaseIncTax * 100) / (100 + parseFloat(taxPer));
            $('#txtPurchaseExcTax').val(PurchaseExcTax.toFixed(2));
            $('#txtSalesIncTax').val((((DefaultProfitMargin / 100) * PurchaseIncTax) + PurchaseIncTax).toFixed(2));
            $('#txtSalesExcTax').val((((DefaultProfitMargin / 100) * PurchaseExcTax) + PurchaseExcTax).toFixed(2));
        }
        else {
            if ($('#ddlTaxType').val() == 'Exclusive') {
                DefaultProfitMargin = ((SalesExcTax - PurchaseExcTax) / PurchaseExcTax) * 100;
            }
            else {
                DefaultProfitMargin = ((SalesIncTax - PurchaseIncTax) / PurchaseIncTax) * 100;
            }
            $('#txtDefaultProfitMargin').val(DefaultProfitMargin.toFixed(2));
        }
    }

}

function calulation() {
    if ($('#ddlProductType').val() == 'Single') {
        UpdateSingleAmount('PExc');
    }
    else if ($('#ddlProductType').val() == 'Variable') {
        UpdateVariationAmount(1);
    }
    else {
        var amount = 0;
        $('#divCombo tr').each(function () {

            var _id = this.id.split('divCombo')[1];
            amount = amount + (parseFloat($('#txtComboQuantity' + _id).val()) * parseFloat($('#hdnComboPurchaseExcTax' + _id).val()));
        });
        $('#divComboNetAmount').text(amount);
    }
}

function insertBrand() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        brandCode: $('#txtBrandCode').val(),
        brand: $('#txtBrand').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/brandInsert',
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
                $('#ddlBrand').append($('<option>', { value: data.Data.Brand.BrandId, text: data.Data.Brand.Brand }));
                $('#ddlBrand').val(data.Data.Brand.BrandId);
                $('#brandModal').modal('toggle');

                $('#txtBrandCode').val('');
                $('#txtBrand').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertCategory() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CategoryCode: $('#txtCategoryCode').val(),
        Category: $('#txtCategory').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/CategoryInsert',
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
                $('#ddlCategory').append($('<option>', { value: data.Data.Category.CategoryId, text: data.Data.Category.Category }));
                $('#ddlCategory').val(data.Data.Category.CategoryId);
                $('#categoryModal').modal('toggle');

                $('#ddlNewCategory').append($('<option>', { value: data.Data.Category.CategoryId, text: data.Data.Category.Category }));
                $('#ddlModalCategory').append($('<option>', { value: data.Data.Category.CategoryId, text: data.Data.Category.Category }));

                $('#ddlSubCategory').html('');
                $('#ddlSubSubCategory').html('');

                $('#txtCategoryCode').val('');
                $('#txtCategory').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertSubCategory() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CategoryId: $('#ddlCategory').val(),
        SubCategoryCode: $('#txtSubCategoryCode').val(),
        SubCategory: $('#txtSubCategory').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/SubCategoryInsert',
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
                $('#ddlSubCategory').append($('<option>', { value: data.Data.SubCategory.SubCategoryId, text: data.Data.SubCategory.SubCategory }));
                $('#ddlSubCategory').val(data.Data.SubCategory.SubCategoryId);
                $('#subCategoryModal').modal('toggle');

                $('#ddlSubSubCategory').html('');

                $('#txtSubCategoryCode').val('');
                $('#txtSubCategory').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveModalSubCategories() {
    var det = {
        CategoryId: $('#ddlModalCategory').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveModalSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var dropdown = '<label>Sub Category <span class="danger">*</span></label><select class="form-control select2" id="ddlModalSubCategory">';
            $.each(data.Data.SubCategories, function (index, value) {
                dropdown = dropdown + '<option value="' + value.SubCategoryId + '">' + value.SubCategory + '</option>';
            });

            dropdown = dropdown + '</select>';
            $('#p_ModalSubCategories_Dropdown').html('');
            $('#p_ModalSubCategories_Dropdown').append(dropdown);

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertSubSubCategory() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryCode: $('#txtSubSubCategoryCode').val(),
        SubSubCategory: $('#txtSubSubCategory').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/SubSubCategoryInsert',
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
                $('#ddlSubSubCategory').append($('<option>', { value: data.Data.SubSubCategory.SubSubCategoryId, text: data.Data.SubSubCategory.SubSubCategory }));
                $('#ddlSubSubCategory').val(data.Data.SubSubCategory.SubSubCategoryId);
                $('#subSubCategoryModal').modal('toggle');

                $('#txtSubSubCategoryCode').val('');
                $('#txtSubSubCategory').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertWarranty() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Warranty: $('#txtWarranty').val(),
        Description: $('#txtDescription').val(),
        DurationNo: $('#txtDurationNo').val(),
        Duration: $('#ddlDuration').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/warrantiesInsert',
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
                $('#ddlWarranty').append($('<option>', { value: data.Data.Warranty.WarrantyId, text: data.Data.Warranty.Warranty }));
                $('#ddlWarranty').val(data.Data.Warranty.WarrantyId);
                $('#warrantyModal').modal('toggle');

                $('#txtWarranty').val('');
                $('#txtDescription').val('');
                $('#txtDurationNo').val('');
                $('#ddlDuration').val('Days');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openVariationModal(_count) {
    _variationId = _count;
    $('#variationModal').modal('toggle');
}

function insertVariation() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var variationDetails = [];
    $('#addNewRow input[type="text"]').each(function () {
        if ($('#' + this.id).val()) {
            variationDetails.push({ VariationDetails: $('#' + this.id).val() });
        }
    });

    var det = {
        VariationCode: $('#txtVariationCode').val(),
        Variation: $('#txtVariation').val(),
        VariationDetails: variationDetails,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/VariationInsert',
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
                $('.variation_template').append($('<option>', { value: data.Data.Variation.VariationId, text: data.Data.Variation.Variation }));
                $('#variationModal').modal('toggle');
                $('#ddlVariation' + _variationId).val(data.Data.Variation.VariationId);
                FetchVariationDetails(_variationId);

                $('.errorText').hide();
                $('[style*="border: 2px"]').css('border', '');


                $('#addNewRow').html('');
                $('#txtVariationCode').val('');
                $('#txtVariation').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

$(".add-new-row").click(function () {
    c = c + 1
    var markup = '<div class="input-group mb-1" id="row' + c + '">' +
        '    <input type="text" class="form-control" id="txtNew' + c + '">' +
        '     <span class="input-group-append">' +
        '       <a href="javascript:void(0)" class="btn btn-danger btn-sm delete-new-row" id="btnNew' + c + '"><i class="fas fa-minus pt-2"></i></a>' +
        '     </span>' +
        '  </div>';
    $("#addNewRow").append(markup);
});

$('#addNewRow').on('click', '.delete-new-row', function () {
    var i = $(this).attr('id');
    $("#row" + i.split('btnNew')[1]).remove();
});

function hideAll() {
    $('#singleproduct').hide();
    $('#variableproduct').hide();
    $('#comboproduct').hide();
}

function chooseProductType() {
    hideAll();
    $('.divChkSecondaryUnit').show();
    $('#chkIsManageStock').prop('disabled', false);
    if ($('#ddlProductType').val() == "Single") {
        $('#singleproduct').show();
    }
    else if ($('#ddlProductType').val() == "Variable") {
        $('#variableproduct').show();
        AddNewVariation();
    }
    else {
        $('#comboproduct').show();
        $('#chkIsManageStock').prop('checked', false);
        $('#chkIsManageStock').prop('disabled', true);
        CheckManageStock();

        $('#chkEnableSecondaryUnit').prop('checked', false);
        fetchActiveSecondaryUnits();
        $('.divChkSecondaryUnit').hide();
    }
}

function CheckManageStock() {
    if ($('#chkIsManageStock').is(":checked")) {
        $('#divAlertQuantity').show();
        $('#divExpiresIn').show();
    }
    else {
        $('#divAlertQuantity').hide();
        $('#divExpiresIn').hide();
    }
}

function insertItem(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('.form-control').css('border-color', '#ced4da');

    var ItemDetails = []; var ItemBranchMaps = [];
    if ($('#ddlProductType').val() == 'Single') {

        ItemDetails.push({
            SKU: $('#txtSkuHsnCode').val(),
            PurchaseExcTax: $('#txtPurchaseExcTax').val(),
            PurchaseIncTax: $('#txtPurchaseIncTax').val(),
            DefaultProfitMargin: $('#txtDefaultProfitMargin').val(),
            SalesExcTax: $('#txtSalesExcTax').val(),
            SalesIncTax: $('#txtSalesIncTax').val(),
            DefaultMrp: $('#txtDefaultMRP').val(),
            ProductImage: $('#txtHiddenFile0').val(),
            FileExtensionProductImage: $('#txtHiddenFileExtension0').val(),
            PurchaseAccountId: $('#ddlPurchaseAccount').val(),
            SalesAccountId: $('#ddlSalesAccount').val(),
            InventoryAccountId: $('#ddlInventoryAccount').val(),
        })
    }
    else if ($('#ddlProductType').val() == 'Variable') {
        var variationDetails = [];
        $('#divVariation table').each(function () {
            var _id = this.id.split('product_variation_form_part')[1];
            if (_id != undefined) {
                $('#divInnerVariation' + _id + ' tr').each(function () {
                    var _innerid = this.id.split('product_variation_form_part_inner')[1];

                    //if (!$('#ddlVariation' + _id).val() || $('#ddlVariation' + _id).val() == 0) {
                    //    isError = true;
                    //    $('#divVariation' + _id).show();
                    //    $('#divVariation' + _id).text('This field is required');
                    //}

                    //if (IsPurchaseAddon == 'true') {
                    //    if (!$('#txtPurchaseExcTax' + _innerid).val()) {
                    //        isError = true;
                    //        $('#divPurchaseExcTax' + _innerid).show();
                    //        $('#divPurchaseExcTax' + _innerid).text('This field is required');
                    //    }

                    //    //if (!$('#txtPurchaseIncTax' + _innerid).val()) {
                    //    //    isError = true;
                    //    //    $('#divPurchaseIncTax' + _innerid).show();
                    //    //    $('#divPurchaseIncTax' + _innerid).text('This field is required');
                    //    //}

                    //    if (!$('#txtDefaultProfitMargin' + _innerid).val()) {
                    //        isError = true;
                    //        $('#divDefaultProfitMargin' + _innerid).show();
                    //        $('#divDefaultProfitMargin' + _innerid).text('This field is required');
                    //    }
                    //}
                    //else {
                    //    $('#txtPurchaseExcTax' + _innerid).val($('#txtSalesExcTax' + _innerid).val());
                    //    $('#txtPurchaseIncTax' + _innerid).val($('#txtSalesIncTax' + _innerid).val());
                    //}

                    //if (!$('#txtSalesExcTax' + _innerid).val() && !$('#txtSalesIncTax' + _innerid).val()) {
                    //    isError = true;
                    //    $('#divSalesTax' + _innerid).show();
                    //    $('#divSalesTax' + _innerid).text('This field is required');
                    //}

                    //if (!$('#txtValue' + _innerid).val()) {
                    //    isError = true;
                    //    $('#divValue' + _innerid).show();
                    //    $('#divValue' + _innerid).text('This field is required');
                    //}

                    ItemDetails.push({
                        SKU: $('#txtSku' + _innerid).val(),
                        VariationId: $('#ddlVariation' + _id).val(),
                        VariationDetailsId: $('#hdnValue' + _innerid).val(),
                        PurchaseExcTax: $('#txtPurchaseExcTax' + _innerid).val(),
                        PurchaseIncTax: $('#txtPurchaseIncTax' + _innerid).val(),
                        DefaultProfitMargin: $('#txtDefaultProfitMargin' + _innerid).val(),
                        SalesExcTax: $('#txtSalesExcTax' + _innerid).val(),
                        SalesIncTax: $('#txtSalesIncTax' + _innerid).val(),
                        DefaultMrp: $('#txtDefaultMrp' + _innerid).val(),
                        ProductImage: $('#txtHiddenFile' + _innerid).val(),
                        FileExtensionProductImage: $('#txtHiddenFileExtension' + _innerid).val(),
                        ItemDetailsId: $('#hdnItemDetailsId' + _innerid).val(),
                        VariationName: $('#txtValue' + _innerid).val(),
                        DivId: _innerid,
                        PurchaseAccountId: $('#ddlPurchaseAccount' + _innerid).val(),
                        SalesAccountId: $('#ddlSalesAccount' + _innerid).val(),
                        InventoryAccountId: $('#ddlInventoryAccount' + _innerid).val(),
                    })
                });
            }
        });
    }
    else if ($('#ddlProductType').val() == 'Combo') {
        var variationDetails = [];
        $('#divCombo tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {

                //if (!$('#txtComboQuantity' + _id).val() || $('#txtComboQuantity' + _id).val() <= 0) {
                //    isError = true;
                //    $('#divComboQuantity' + _id).show();
                //    $('#divComboQuantity' + _id).text('Quantity is required');
                //}

                ItemDetails.push({
                    DivId: _id,
                    ItemDetailsId: $('#hdnItemDetailsId' + _id).val(),
                    ComboItemDetailsId: $('#hdnComboItemDetailsId' + _id).val(),
                    Quantity: $('#txtComboQuantity' + _id).val(),
                    TotalCost: $('#hdnComboTotalAmount' + _id).val(),
                    DefaultProfitMargin: $('#txtComboDefaultMargin').val(),
                    SalesExcTax: $('#txtComboSalesExcTax').val(),
                    SalesIncTax: $('#txtComboSalesIncTax').val(),
                    PriceAddedFor: $("#ddlComboUnit" + _id).val().split('-')[1],
                    DefaultMrp: $('#txtComboDefaultMrp').val(),
                    SalesAccountId: $('#ddlComboSalesAccount').val(),
                })
            }
        });
        //if (ItemDetails.length == 0) {
        //    isError = true;
        //    $('#divtags').show();
        //    $('#divtags').text('Search Item first');
        //}
    }

    //if (isError == true) {
    //    if (EnableSound == 'True') { document.getElementById('error').play(); }
    //    toastr.error('Invalid inputs, check and try again !!');
    //    return;
    //}

    $('.divBranch').each(function () {
        var _innerid = this.id.split('tr')[1];
        if ($('#chkBranch' + _innerid).is(':checked')) {
            ItemBranchMaps.push({
                BranchId: _innerid,
                Rack: $('#txtRack' + _innerid).val(),
                Row: $('#txtRow' + _innerid).val(),
                Position: $('#txtPosition' + _innerid).val()
            });
        }
    });

    var det = {
        ItemId: window.location.href.split('=')[1],
        ItemType: $('#ddlItemType').val(),
        ItemCode: $('#txtItemCode').val(),
        ItemName: $('#txtItemName').val(),
        Description: $('#txtDescription').val(),
        SkuCode: $('#txtSkuCode').val(),
        HsnCode: $('#txtHsnCode').val(),
        BarcodeType: $('#ddlBarcodeType').val(),
        UnitId: $('#ddlUnit').val(),
        SecondaryUnitId: $('#ddlSecondaryUnit').val(),
        UToSValue: $('#txtUToSValue').val(),
        TertiaryUnitId: $('#ddlTertiaryUnit').val(),
        SToTValue: $('#txtSToTValue').val(),
        QuaternaryUnitId: $('#ddlQuaternaryUnit').val(),
        TToQValue: $('#txtTToQValue').val(),
        BrandId: $('#ddlBrand').val(),
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryId: $('#ddlSubSubCategory').val(),
        IsManageStock: $('#chkIsManageStock').is(':checked'),
        AlertQuantity: $('#txtAlertQuantity').val(),
        ProductImage: ProductImage,
        FileExtensionProductImage: fileExtensionProductImage,
        ProductBrochure: ProductBrochure,
        FileExtensionProductBrochure: fileExtensionProductBrochure,
        TaxId: $('#ddlIntraStateTax').val() == null ? 0 : $('#ddlIntraStateTax').val().split('-')[0],
        InterStateTaxId: $('#ddlInterStateTax').val() == null ? 0 : $('#ddlInterStateTax').val().split('-')[0],
        TaxType: $('#ddlTaxType').val(),
        ProductType: $('#ddlProductType').val(),
        ManufacturingDate: $('#txtManufacturingDate').val(),
        ExpiryPeriod: $('#txtExpiryPeriod').val(),
        ExpiryPeriodType: $('#ddlExpiryPeriodType').val(),
        PackagingCharge: $('#txtPackagingCharge').val(),
        IsActive: true,
        IsDeleted: false,
        ItemDetails: ItemDetails,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        WarrantyId: $('#ddlWarranty').val(),
        ItemBranchMaps: ItemBranchMaps,
        PriceAddedFor: $('#ddlPriceAddedFor').val(),
        EnableImei: $('#chkEnableImei').is(':checked'),
        ItemCodeId: $('#ddlItemCode').val(),
        TaxPreference: $("#ddlTaxPreference option:selected").text(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        SaltId: $('#ddlSalt').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/items/itemsInsert',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                fetchItem(data.Data.Item.SkuCode);
                $('#ItemModal').modal('hide');
                clearItem();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function clearItem() {
    $('#txtItemCode').val('');
    $('#txtItemName').val('');
    $('#txtDescription').summernote('reset');
    $('#txtSkuCode').val('');
    $('#txtHsnCode').val('');
    $('#ddlBarcodeType').val(0);
    $('#ddlUnit').val(0);
    $('#chkEnableSecondaryUnit').prop('checked', false);
    $('#ddlSecondaryUnit').val(0);
    $('#txtUToSValue').val('');
    $('#ddlTertiaryUnit').val(0);
    $('#txtSToTValue').val('');
    $('#ddlQuaternaryUnit').val(0);
    $('#txtTToQValue').val('');
    $('#ddlBrand').val(0);
    $('#ddlCategory').val(0);
    $('#ddlSubCategory').val(0);
    $('#ddlSubSubCategory').val(0);
    $('#txtAlertQuantity').val('');
    $('#ddlTax_Item').val(0);
    $('#ddlTaxType').val('Exclusive');
    $('#ddlProductType').val('Single');
    $('#txtManufacturingDate').val('');
    $('#txtExpiryPeriod').val('');
    $('#ddlExpiryPeriodType').val('Year');
    $('#txtPackagingCharge').val('');
    $('#ddlWarranty').val(0);
    $('#ddlPriceAddedFor').val(4);
    $('#chkEnableImei').prop('checked', false);
    ProductImage: "";
    FileExtensionProductImage: "";
    ProductBrochure: "";
    FileExtensionProductBrochure: "";
    $('#blahProductImage').prop('src', '');
    $('#ProductImage').val('');

    $('#ProductImage0').val('');
    $('#blahProductImage0').prop('src', '');

    $('.divBranch').each(function () {
        var _innerid = this.id.split('tr')[1];
        $('#chkBranch' + _innerid).prop('checked', false);
        $('#txtRack' + _innerid).val('');
        $('#txtRow' + _innerid).val('');
        $('#txtPosition' + _innerid).val('');
    });

    $('#txtSkuHsnCode').val('');
    $('#txtPurchaseExcTax').val('');
    $('#txtPurchaseIncTax').val('');
    //$('#txtDefaultProfitMargin').val();
    $('#txtSalesExcTax').val('');
    $('#txtSalesIncTax').val('');
    $('#txtHiddenFile0').val('');
    $('#txtHiddenFileExtension0').val('');

    $('#divVariation table').each(function () {
        var _id = this.id.split('product_variation_form_part')[1];
        $('#product_variation_form_part' + _id).remove();
        //if (_id != undefined) {
        //    $('#divInnerVariation' + _id + ' tr').each(function () {
        //        var _innerid = this.id.split('product_variation_form_part_inner')[1];               
        //});
    });

    fetchActiveSecondaryUnits();
    chooseProductType();
    $('.select2').select2();
    $('#chkIsManageStock').prop('disabled', true);
}

$('#txttags').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/items/itemAutocomplete",
            dataType: "json",
            data: {
                Search: request.term, BranchId: $('#ddlBranch').val(), MenuType: 'purchase'
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
                }
                else {
                    if (data.Data.ItemsArray.length == 1) {
                        $('#txttags').val('');
                        var splitVal = data.Data.ItemsArray[0].split('~');
                        fetchItem(splitVal[splitVal.length - 1]);
                        skuCodes.push(splitVal[splitVal.length - 1]);
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
        skuCodes.push(splitVal[splitVal.length - 1]);
    }
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

function checkStatus() {
    $('#txtPaymentNotes').val('');
    $('#txtAmount').val('');
    $("#txtPaymentDate").val('');
    $('#ddlPaymentType').val(0);
    //$('#ddlAccount').val(0);
    $('#txtReferenceNo').val('');

    $('.select2').select2();

    if ($('#ddlPurchaseStatus').val() == "Due") {
        $('.divPaymentInfo').show();
    }
    else {
        $('.divPaymentInfo').hide();
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

function setAvailableTags(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

function ViewPurchaseInvoices(ReferenceId, ReferenceType) {
    var det = {
        ReferenceId: ReferenceId,
        ReferenceType: ReferenceType
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Purchase/PurchaseInvoicesByReference',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divPurchaseInvoices").html(data);
            $("#purchaseInvoicesModal").modal('show');

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleTaxPreference() {
    $('.divTaxable').hide();
    $('.divNonTaxable').hide();

    if ($('#ddlTaxPreference option:selected').text() == 'Taxable') {
        $('.divTaxable').show();
        $('#ddlTaxExemption').val(0);
    }
    else if ($('#ddlTaxPreference option:selected').text() == 'Non-Taxable') {
        $('.divNonTaxable').show();
        $('#ddlIntraStateTax').val(0);
        $('#ddlInterStateTax').val(0);
    }

    if ($('#ddlTaxPreference option:selected').text() == 'Taxable') {
        $('#ddlTaxType').val('Inclusive');
        $('.divTaxType').show();
        checkTaxType();
    }
    else {
        $('#ddlTaxType').val('Exclusive');
        $('.divTaxType').hide();
        checkTaxType();
    }

    $('.select2').select2();
}

function toggleGstTreatment() {
    if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)" || $('#ddlGstTreatment').val() == "Composition Taxable Supply" ||
        $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
        || $('#ddlGstTreatment').val() == "Supply by SEZ Developer") {
        $('.divGst').show();
        $('.divPlaceOfSupply').show();
        $('.divTaxPreference').show();
    }
    else if ($('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)") {
        $('.divSourceOfSupply').hide();
        $('.divTaxPreference').hide();
    }
    else {
        $('.divGst').hide();
        $('.divSourceOfSupply').show();
        $('.divTaxPreference').show();
    }
}

function toggleBusinessRegistered() {
    if ($('#chkIsBusinessRegistered').is(':checked')) {
        $('.divBusinessRegistered').show();
    }
    else {
        $('.divBusinessRegistered').hide();
    }
}

function insertTaxExemption() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var _type = 'Customer';
    if ($("#rdbItem").prop("checked")) {
        _type = "Item";
    }

    var det = {
        Reason: $('#txtReason_TaxExemption').val(),
        Description: $('#txtDescription_TaxExemption').val(),
        TaxExemptionType: _type,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxExemptionInsert',
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
                    $('#' + res.Id + '_TaxExemption').show();
                    $('#' + res.Id + '_TaxExemption').text(res.Message);

                    var ctrl = $('.' + res.Id + '_TaxExemption_ctrl select').prop('tagName');
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_TaxExemption_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_TaxExemption_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_TaxExemption_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_TaxExemption_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                $('#ddlTaxExemption').append($('<option>', { value: data.Data.TaxExemption.TaxExemptionId, text: data.Data.TaxExemption.Reason }));
                $('#ddlTaxExemption').val(data.Data.TaxExemption.TaxExemptionId);
                $('#taxExemptionModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

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

function insertPaymentTerm() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        PaymentTerm: $('#txtPaymentTerm_M').val(),
        Days: $('#txtDays_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/paymentTermInsert',
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
                $('#ddlPaymentTerm').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPaymentTerm').val(data.Data.PaymentTerm.PaymentTermId);

                $('#paymentTermModal').modal('toggle');

                $('#txtPaymentTerm_M').val('');
                $('#txtDays_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleShippingAddress() {
    if ($('#chkIsShippingAddressDifferent').is(':checked')) {
        $('#divShippingAddress').show();
    }
    else {
        $('#divShippingAddress').hide();
    }
}

function setName() {
    //if (!$('#txtAddrName').val()) {
    $('#txtAddrName').val($('#txtName').val())
    //}
}

function setMobile() {
    //if (!$('#txtAddrMobileNo').val()) {
    $('#txtAddrMobileNo').val($('#txtMobileNo').val())
    //}
}

function setAlternativeMobile() {
    //if (!$('#txtAddrMobileNo2').val()) {
    $('#txtAddrMobileNo2').val($('#txtAltMobileNo').val());
    //}
}

function setEmail() {
    //if (!$('#txtAddrMobileNo2').val()) {
    $('#txtAddrEmailId').val($('#txtEmailId').val());
    //}
}

function openCityModal(id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    _id = id;
    if (_id == 'ddlCity') {
        if (!$('#ddlState').val() || $('#ddlState').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select State first');
            return
        }
    }
    else {
        if (!$('#ddlAltState').val() || $('#ddlAltState').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select State first');
            return
        }
    }
    $('#cityModal').modal('toggle');
}

function insertCity() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CountryId: _id == 'ddlCity' ? $('#ddlCountry').val() : $('#ddlAltCountry').val(),
        //StateId: $('#ddlCityModalState').val(),
        StateId: _id == 'ddlCity' ? $('#ddlState').val() : $('#ddlAltState').val(),
        CityCode: $('#txtCityCode').val(),
        City: $('#txtCity').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/CityInsert',
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
                if (_id == 'ddlCity') {
                    $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    if ($('#ddlState').val() == $('#ddlAltState').val()) {
                        $('#ddlAltCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    }
                }

                if (_id == 'ddlAltCity') {
                    $('#ddlAltCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    if ($('#ddlState').val() == $('#ddlAltState').val()) {
                        $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                    }
                }
                $('#' + _id).val(data.Data.City.CityId);
                $('#cityModal').modal('toggle');

                $('#ddlCityModalCountry').val($("#ddlCityModalCountry option:first").val());
                $('#ddlCityModalState').html('');
                $('#txtCityCode').val('');
                $('#txtCity').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openItemCodeModal() {
    $('#itemCodeModal').modal('toggle');
    if ($('#ddlItemType').val() == "Product") {
        $('#ddlItemCodeType_N').val('HSN');
        $('#lblCode').html('HSN Code <span class="danger">*</span>');
    }
    else {
        $('#ddlItemCodeType_N').val('SAC');
        $('#lblCode').html('SAC Code <span class="danger">*</span>');
    }
    $('#ddlItemCodeType_N').prop('disabled', true);
    $('.select2').select2();
}

function insertItemCode(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        ItemCodeType: $('#ddlItemCodeType_N').val(),
        Code: $('#txtCode_N').val(),
        Description: $('#txtDescription_N').val(),
        TaxPreferenceId: $('#ddlTaxPreference_N').val(),
        TaxPreference: $("#ddlTaxPreference_N option:selected").text(),
        TaxExemptionId: $('#ddlTaxExemption_N').val(),
        IntraStateTaxId: $('#ddlIntraStateTax_N').val().split('-')[0],
        InterStateTaxId: $('#ddlInterStateTax_N').val().split('-')[0],
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/ItemCodeInsert',
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
                    $('#' + res.Id + '_N').show();
                    $('#' + res.Id + '_N').text(res.Message);


                    if ($('.' + res.Id + '_N_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_N_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_N_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_N_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_N_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                $('#ddlItemCode').append($('<option>', { value: data.Data.ItemCode.ItemCodeId, text: data.Data.ItemCode.Code }));
                $('#ddlItemCode').val(data.Data.ItemCode.ItemCodeId);

                $('#itemCodeModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchItemCodeTax() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        ItemCodeId: $('#ddlItemCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/fetchItemCodeTax',
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
                $('#ddlTaxPreference').val(data.Data.ItemCode.TaxPreferenceId);
                $('#ddlTax').val(data.Data.ItemCode.IntraStateTaxId + '-' + data.Data.ItemCode.IntraStateTaxPercentage);
                $('#ddlInterStateTax').val(data.Data.ItemCode.InterStateTaxId + '-' + data.Data.ItemCode.InterStateTaxPercentage);
                $('#ddlTaxExemption').val(data.Data.ItemCode.TaxExemptionId);

                $('.select2').select2();
                toggleTaxPreference();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openSaltModal() {
    $('#saltModal').modal('toggle');
    $('textarea#txtIndication,#txtDosage,#txtSideEffects,#txtSpecialPrecautions,#txtDrugInteractions,#txtNotes').summernote({
        placeholder: '',
        tabsize: 2,
        height: 100,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            // ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            //['fontname', ['fontname']],
            // ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            //['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });
}

function insertSalt(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        SaltName: $('#txtSaltName').val(),
        Indication: $('#txtIndication').val(),
        Dosage: $('#txtDosage').val(),
        SideEffects: $('#txtSideEffects').val(),
        SpecialPrecautions: $('#txtSpecialPrecautions').val(),
        DrugInteractions: $('#txtDrugInteractions').val(),
        Notes: $('#txtNotes').val(),
        TBItem: $('#ddlTBItem').val(),
        IsNarcotic: $('#chkIsNarcotic').is(':checked'),
        IsScheduleH: $('#chkIsScheduleH').is(':checked'),
        IsScheduleH1: $('#chkIsScheduleH1').is(':checked'),
        IsDiscontinued: $('#chkIsDiscontinued').is(':checked'),
        IsProhibited: $('#chkIsProhibited').is(':checked'),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/SaltInsert',
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
                $('#ddlSalt').append($('<option>', { value: data.Data.Salt.SaltId, text: data.Data.Salt.SaltName }));
                $('#ddlSalt').val(data.Data.Salt.SaltId);
                $('#saltModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchItemTax(SkuHsnCode) {
    var det = {
        ItemCode: SkuHsnCode,
        BranchId: $('#ddlBranch').val(),
        Supplierid: $('#ddlSupplier').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        Type: "Purchase"
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

                            var ddlTax = '<select class="form-control ddlTax" id="ddlTax' + count + '" onchange="ChangeQtyAmount(' + count + ')" style="min-width:80px">';
                            for (let ss = 0; ss < taxList.length; ss++) {
                                var canAdd = true;
                                if (taxList[ss].Tax != "Taxable") {
                                    if ($('#chkIsReverseCharge').is(':checked')) {
                                        if (taxList[ss].Tax == "Non-Taxable") {
                                            canAdd = false;
                                        }
                                    }
                                    if (canAdd == true) {
                                        if (data.Data.BusinessSetting.CountryId == 2) {
                                            if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
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
                                                    if (data.Data.ItemDetails[i].InterStateTaxId == taxList[ss].TaxId) {
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
                            }
                            ddlTax = ddlTax + '</select >';

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
                                '<div class="input-group" style="min-width:160px">' +
                                ddlTax +
                                '<input type="number" disabled class="form-control" id="txtTaxAmount' + count + '" value="" style="min-width:80px">' +
                                '<input type="text" hidden class="form-control" id="txtTaxId' + count + '" value="' + data.Data.ItemDetails[i].TaxId + '">' +
                                '<div id="divTaxExemption' + count + '" class="form-group" style="display:' + ((data.Data.BusinessSetting.CustomerTaxPreference == 'Taxable' && data.Data.ItemDetails[i].TaxExemptionId != 0) ? '' : 'none') + '">' +
                                '<label style="margin-bottom:0;margin-top:0.5rem;">Exemption Reason <span class="danger">*</span></label>' +
                                '<div class="input-group">' +
                                ddlTaxExemption +
                                '</div>' +
                                '</div>' +
                                '</div>');

                            ChangeQtyAmount(count);
                        }
                    }
                });
            }
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchAdditionalCharges() {
    var det = {
        SupplierId: $('#ddlSupplier').val(),
        BranchId: $('#ddlBranch').val(),
    };
    
    // Store existing values before clearing
    var existingValues = {};
    var hasExistingValues = false;
    
    // Check if there are any existing values in additional charges
    $('[id^="txtAdditionalChargesAmountExcTax"]').each(function() {
        var index = $(this).attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        var amount = $(this).val();
        var incTax = $('#txtAdditionalChargesAmountIncTax' + index).val();
        var chargeId = $('#txtAdditionalChargeId' + index).val();
        var purchaseQuotationAdditionalChargesId = $('#txtPurchaseQuotationAdditionalChargesId' + index).val();
        
        if (amount && parseFloat(amount) > 0) {
            hasExistingValues = true;
            existingValues[index] = {
                amount: amount,
                incTax: incTax,
                chargeId: chargeId,
                purchaseQuotationAdditionalChargesId: purchaseQuotationAdditionalChargesId
            };
        }
    });
    
    $("#divLoading").show();
    $.ajax({
        url: '/OtherSettings/ActiveAdditionalCharges',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var CountryId = $('#hdnCountryId').val();

            var html = '';
            for (let i = 0; i < data.Data.AdditionalCharges.length; i++) {
                var count = i;
                
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

                var ddlTax = '<select class="form-control select2" style="min-width:80px" id="ddlAdditionalChargesTax' + count + '" onchange="toggleExemptionDropdownAC(' + count + ');">';
                for (let ss = 0; ss < taxList.length; ss++) {
                    var canAdd = true;
                    if (taxList[ss].Tax != "Taxable") {
                        if ($('#chkIsReverseCharge').is(':checked')) {
                            if (taxList[ss].Tax == "Non-Taxable") {
                                canAdd = false;
                            }
                        }
                        if (canAdd == true) {
                            if (CountryId == 2) {
                                if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
                                    if (taxList[ss].TaxTypeId != 3) {
                                        if (data.Data.AdditionalCharges[i].IntraStateTaxId == taxList[ss].TaxId) {
                                            ddlTax = ddlTax + '<option selected value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                        else {
                                            ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                                        }
                                    }
                                }
                                else {
                                    if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                                        if (data.Data.AdditionalCharges[i].InterStateTaxId == taxList[ss].TaxId) {
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
                }
                ddlTax = ddlTax + '</select>';

                // Exemption Reason label + edit icon (no inline dropdown)
                var selectedExemptionText = 'None';
                var selectedExemptionId = 0;
                if (data.Data.AdditionalCharges[i].TaxExemptionId && data.Data.AdditionalCharges[i].TaxExemptionId != 0) {
                    var found = taxExemptions.find(function (ex) { return ex.TaxExemptionId == data.Data.AdditionalCharges[i].TaxExemptionId; });
                    if (found) selectedExemptionText = found.Reason;
                    selectedExemptionId = data.Data.AdditionalCharges[i].TaxExemptionId;
                }
                
                var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:none; margin-top:5px; align-items:center;">' +
                    '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
                    '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
                    '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
                    '<i class="fas fa-edit"></i>' +
                    '</a>' +
                    '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
                    '</div>';

                var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:none; margin-top:5px; align-items:center;">' +
                    '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
                    '<span id="itcTypeLabel' + count + '" class="small">Eligible For ITC</span>' +
                    '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
                    '<i class="fas fa-edit"></i>' +
                    '</a>' +
                    '<input type="hidden" id="itcTypeValue' + count + '" value="Eligible For ITC">' +
                    '</div>';

                // Set amount value - preserve existing or use default
                var amountValue = existingValue ? existingValue.amount : '0';
                var incTaxValue = existingValue ? existingValue.incTax : '0';
                var purchaseQuotationAdditionalChargesIdValue = existingValue ? existingValue.purchaseQuotationAdditionalChargesId : data.Data.AdditionalCharges[i].PurchaseQuotationAdditionalChargesId;

                html = html +
                    '<div class="row additional-charges-row">' +
                    '<div class="col-sm-6">' +
                    '<div class="form-group row">' +
                    '<label for="inputEmail3" class="col-sm-5 col-form-label">' + data.Data.AdditionalCharges[i].Name + '</label>' +
                    '<div class="input-group col-sm-7">' +
                    '<input type="number" min="0" class="form-control" id="txtAdditionalChargesAmountExcTax' + count + '" placeholder="" onchange="updateAdditionalChargesCalculation(' + count + ')" value="' + amountValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargesAmountIncTax' + count + '" value="' + incTaxValue + '">' +
                    '<input type="hidden" id="txtAdditionalChargeId' + count + '" value="' + data.Data.AdditionalCharges[i].AdditionalChargeId + '">' +
                    '<input type="hidden" id="txtPurchaseQuotationAdditionalChargesId' + count + '" value="' + purchaseQuotationAdditionalChargesIdValue + '">' +
                    ddlTax +
                    exemptionDiv +
                    itcDiv +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div> ';
            }

            $('#divAdditionalCharges').empty();
            $('#divAdditionalCharges').html(html);
            $('.select2').select2();
            
            // Ensure exemption icon/label is shown if tax is Non-Taxable on load
            for (let i = 0; i < data.Data.AdditionalCharges.length; i++) {
                toggleExemptionDropdownAC(i);
            }
            
            // Apply ITC/Exemption logic for additional charges
            applyITCExemptionLogicForAdditionalCharges();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

// Update modal save logic to store the selected exemption id as well as text
function saveExemptionSelection() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if (currentExemptionEditIndex == null) {
        return;
    }

    var $dropdown = $('#ddlExemptionReason');
    var $label = $('#exemptionReasonLabel' + currentExemptionEditIndex);
    var $hiddenId = $('#exemptionReasonId' + currentExemptionEditIndex);

    if ($dropdown.length === 0) {
        return;
    }

    if ($label.length === 0) {
        return;
    }

    if ($hiddenId.length === 0) {
        return;
    }

    // Update the label and hidden input with selected values
    var selectedText = $dropdown.find('option:selected').text();
    var selectedValue = $dropdown.val();

    if (selectedValue == "0") {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');

        $('#divExemptionReason').show();
        $('#divExemptionReason').text('This field is required');

        $('#ddlExemptionReason').next('.select2').find('.select2-selection').css('border', '2px solid #dc3545');
        return
    }

    $label.text(selectedText);
    $hiddenId.val(selectedValue);

    // Hide the modal
    $('#exemptionModal').modal('hide');
    currentExemptionEditIndex = null;
}

// Add supporting JS functions at the end of the file
var currentExemptionEditIndex = null;
function openExemptionModal(count) {
    currentExemptionEditIndex = count;
    var $dropdown = $('#ddlExemptionReason');

    $dropdown.empty().append(
        $('<option>', {
            value: 0,
            text: 'Select'
        })
    );

    if (taxExemptions && taxExemptions.length > 0) {
        $.each(taxExemptions, function (i, exemption) {
            $dropdown.append(
                $('<option>', {
                    value: exemption.TaxExemptionId,
                    text: exemption.Reason
                })
            );
        });
    }

    // Set the current value if it exists
    var currentValue = $('#exemptionReasonId' + count).val();
    if (currentValue && currentValue != '0') {
        $dropdown.val(currentValue).trigger('change');
    }

    $('#ddlExemptionReason').select2();
    $('#exemptionModal').modal('show');
}

function toggleExemptionDropdownAC(count) {
    // Apply the complete ITC/Exemption logic for this specific additional charge
    applyITCExemptionLogicForSingleAdditionalCharge(count);
    updateAdditionalChargesCalculation(count);
}

function applyITCExemptionLogicForSingleAdditionalCharge(count) {
    // Hide both sections initially
    $('#divTaxExemptionAC' + count).hide();
    $('#divInputTaxCreditAC' + count).hide();
    
    // Clear existing content
    $('#divTaxExemptionAC' + count).empty();
    $('#divInputTaxCreditAC' + count).empty();
    
    var taxValue = $("#ddlAdditionalChargesTax" + count + " :selected").text();
    
    if (taxValue == "Non-Taxable") {
        // Show exemption reason for Non-Taxable
        var selectedExemptionText = $('#exemptionReasonLabel' + count).text() || 'None';
        var selectedExemptionId = $('#exemptionReasonId' + count).val() || '0';
        
        var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
            '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
            '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
            '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
            '<i class="fas fa-edit"></i>' +
            '</a>' +
            '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
            '</div>';
        
        $('#divTaxExemptionAC' + count).html(exemptionDiv);
        $('#divTaxExemptionAC' + count).show();
    }
    else if (taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
        // Show ITC for taxable items
        var selectedITCText = $('#itcTypeLabel' + count).text() || 'Eligible For ITC';
        var selectedITCValue = $('#itcTypeValue' + count).val() || 'Eligible For ITC';
        
        var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
            '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
            '<span id="itcTypeLabel' + count + '" class="small">' + selectedITCText + '</span>' +
            '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
            '<i class="fas fa-edit"></i>' +
            '</a>' +
            '<input type="hidden" id="itcTypeValue' + count + '" value="' + selectedITCValue + '">' +
            '</div>';
        
        $('#divInputTaxCreditAC' + count).html(itcDiv);
        $('#divInputTaxCreditAC' + count).show();
    }
}

function updateAdditionalChargesCalculation(count) {
    var amount = $('#txtAdditionalChargesAmountExcTax' + count).val() == '' ? 0 : parseFloat($('#txtAdditionalChargesAmountExcTax' + count).val());
    var tax = $('#ddlAdditionalChargesTax' + count).val().split('-')[1];
    var incTax = ((tax / 100) * amount) + amount;
    $('#txtAdditionalChargesAmountIncTax' + count).val(incTax);
    discallcalc();
}

function applyITCExemptionLogicForAdditionalCharges() {
    // Loop through all additional charges
    $('.additional-charges-row').each(function () {
        var count = $(this).find('input[id^="txtAdditionalChargesAmountExcTax"]').attr('id').replace('txtAdditionalChargesAmountExcTax', '');
        
        // Hide both sections initially
        $('#divTaxExemptionAC' + count).hide();
        $('#divInputTaxCreditAC' + count).hide();
        
        // Clear existing content
        $('#divTaxExemptionAC' + count).empty();
        $('#divInputTaxCreditAC' + count).empty();
        
        var taxValue = $("#ddlAdditionalChargesTax" + count + " :selected").text();
        
        if (taxValue == "Non-Taxable") {
            // Show exemption reason for Non-Taxable
            var selectedExemptionText = $('#exemptionReasonLabel' + count).text() || 'None';
            var selectedExemptionId = $('#exemptionReasonId' + count).val() || '0';
            
            var exemptionDiv = '<div id="divTaxExemptionAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
                '<span class="small text-muted text-bold">Exemption Reason:&nbsp;</span>' +
                '<span id="exemptionReasonLabel' + count + '" class="small">' + selectedExemptionText + '</span>' +
                '<a href="javascript:void(0)" onclick="openExemptionModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit Exemption">' +
                '<i class="fas fa-edit"></i>' +
                '</a>' +
                '<input type="hidden" id="exemptionReasonId' + count + '" value="' + selectedExemptionId + '">' +
                '</div>';
            
            $('#divTaxExemptionAC' + count).html(exemptionDiv);
            $('#divTaxExemptionAC' + count).show();
        }
        else if (taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
            // Show ITC for taxable items
            var selectedITCText = $('#itcTypeLabel' + count).text() || 'Eligible For ITC';
            var selectedITCValue = $('#itcTypeValue' + count).val() || 'Eligible For ITC';
            
            var itcDiv = '<div id="divInputTaxCreditAC' + count + '" style="display:flex; margin-top:5px; align-items:center;">' +
                '<span class="small text-muted text-bold">Input Tax Credit:&nbsp;</span>' +
                '<span id="itcTypeLabel' + count + '" class="small">' + selectedITCText + '</span>' +
                '<a href="javascript:void(0)" onclick="openITCModal(' + count + ')" style="margin-left:6px; vertical-align:middle;" title="Edit ITC Type">' +
                '<i class="fas fa-edit"></i>' +
                '</a>' +
                '<input type="hidden" id="itcTypeValue' + count + '" value="' + selectedITCValue + '">' +
                '</div>';
            
            $('#divInputTaxCreditAC' + count).html(itcDiv);
            $('#divInputTaxCreditAC' + count).show();
        }
    });
}

// Add supporting JS functions for ITC modal
var currentITCEditIndex = null;
function openITCModal(count) {
    currentITCEditIndex = count;
    var $dropdown = $('#ddlITCType');

    $dropdown.empty().append(
        $('<option>', {
            value: 'Eligible For ITC',
            text: 'Eligible For ITC'
        }),
        $('<option>', {
            value: 'Ineligible - As per Section 17 (5)',
            text: 'Ineligible - As per Section 17 (5)'
        }),
        $('<option>', {
            value: 'Ineligible - Others',
            text: 'Ineligible - Others'
        })
    );

    // Set the current value if it exists
    var currentValue = $('#itcTypeValue' + count).val();
    if (currentValue) {
        $dropdown.val(currentValue).trigger('change');
    }

    $('#ddlITCType').select2();
    $('#itcModal').modal('show');
}

function saveITCSelection() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    if (currentITCEditIndex == null) {
        return;
    }

    var $dropdown = $('#ddlITCType');
    var $label = $('#itcTypeLabel' + currentITCEditIndex);
    var $hiddenValue = $('#itcTypeValue' + currentITCEditIndex);

    if ($dropdown.length === 0) {
        return;
    }

    if ($label.length === 0) {
        return;
    }

    if ($hiddenValue.length === 0) {
        return;
    }

    // Update the label and hidden input with selected values
    var selectedText = $dropdown.find('option:selected').text();
    var selectedValue = $dropdown.val();

    if (!selectedValue) {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');

        $('#divITCType').show();
        $('#divITCType').text('This field is required');

        $('#ddlITCType').next('.select2').find('.select2-selection').css('border', '2px solid #dc3545');
        return
    }

    $label.text(selectedText);
    $hiddenValue.val(selectedValue);

    // Hide the modal
    $('#itcModal').modal('hide');
    currentITCEditIndex = null;
}


// Initialize autocomplete for HSN/SAC Code
$('#txtCode_N').autocomplete({
    minLength: 2,
    source: function (request, response) {
        $.ajax({
            url: "/othersettings/ItemCodeAutocomplete",
            dataType: "json",
            type: "POST",
            data: {
                Search: request.term,
                ItemCodeType: $('#ddlItemCodeType_N').val()
            },
            success: function (data) {
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
                    });
                }
                else {
                    response(data.Data.ItemsArray);
                }
            },
            error: function (xhr) {
                // Handle error silently
            }
        });
    },
    select: function (event, ui) {
        var selectedValue = ui.item.value;
        var parts = selectedValue.split(' ~ ');
        if (parts.length === 2) {
            $('#txtCode_N').val(parts[0]);
            $('#txtDescription_N').val(parts[1]);
        }
        return false;
    }
});

// Initialize autocomplete for Description
$('#txtDescription_N').autocomplete({
    minLength: 2,
    source: function (request, response) {
        $.ajax({
            url: "/othersettings/ItemCodeAutocomplete",
            dataType: "json",
            type: "POST",
            data: {
                Search: request.term,
                ItemCodeType: $('#ddlItemCodeType_N').val()
            },
            success: function (data) {
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
                    });
                }
                else {
                    response(data.Data.ItemsArray);
                }
            },
            error: function (xhr) {
                // Handle error silently
            }
        });
    },
    select: function (event, ui) {
        var selectedValue = ui.item.value;
        var parts = selectedValue.split(' ~ ');
        if (parts.length === 2) {
            $('#txtCode_N').val(parts[0]);
            $('#txtDescription_N').val(parts[1]);
        }
        return false;
    }
});