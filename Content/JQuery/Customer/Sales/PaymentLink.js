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

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), icons: DateTimePickerIcons });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date(), icons: DateTimePickerIcons });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');

    $('#txtDateRange').daterangepicker({
        //timePicker: true,
        //timePickerIncrement: 1,
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_LinkExpirationDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, minDate: new Date(), icons: DateTimePickerIcons });
    $('#_LinkExpirationDate').addClass('notranslate');
    fetchTax();
});

// Centralized datetimepicker icon configuration (same as POS)
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
var taxList = [], BusinessSetting;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        CustomerId: $('#ddlCustomer').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ReferenceNo: $('#txtReferenceNo').val(),
        Status: $('#ddlStatus').val(),
        BranchId: $('#ddlBranch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/PaymentLink/PaymentLinkFetch',
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

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        BranchId: $('#ddlBranch').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        CustomerId: $('#ddlCustomer').val(),
        Amount: $('#txtAmount').val(),
        LinkExpirationDate: moment($("#txtLinkExpirationDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Notes: $('#txtNotes').val(),
        IsActive: true,
        IsDeleted: false,
        OnlinePaymentSettingsId: $('#ddlOnlinePaymentSettings').val(),
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        AccountId: $('#ddlLAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/PaymentLink/PaymentLinkInsert',
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
                    window.location.href = "/PaymentLink/index";
                }
                else {
                    window.location.href = "/PaymentLink/add";
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
    

    var det = {
        PaymentLinkId: window.location.href.split('=')[1],
        CustomerId: $('#ddlCustomer').val(),
        Amount: $('#txtAmount').val(),
        LinkExpirationDate: moment($("#txtLinkExpirationDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),
        Notes: $('#txtNotes').val(),
        PlaceOfSupplyId: $('#ddlPlaceOfSupply').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
    };
    $("#divLoading").show();
    $.ajax({
        url: '/PaymentLink/PaymentLinkUpdate',
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
                    window.location.href = "/PaymentLink/index";
                }
                else {
                    window.location.href = "/PaymentLink/add";
                }
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(PaymentLinkId, PaymentLinkName) {
    var r = confirm("This will delete \"" + PaymentLinkName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            PaymentLinkId: PaymentLinkId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/PaymentLink/PaymentLinkdelete',
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

function FetchUserCurrency() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        UserId: $('#ddlCustomer').val(),
        BranchId: $('#ddlBranch').val()
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

                $('#txtPayTermNo').val(data.Data.User.PayTermNo);
                $('#ddlPayTerm').val(data.Data.User.PayTerm);

                if (data.Data.User) {
                    if (data.Data.User.GstTreatment != null && data.Data.User.GstTreatment != "") {
                        if (data.Data.User.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)") {
                            $('#ddlPlaceOfSupply').val(data.Data.User.PlaceOfSupplyId);
                            $('.divPlaceOfSupply').show();
                        }
                        else {
                            $('#ddlPlaceOfSupply').val(0);
                            $('.divPlaceOfSupply').hide();
                        }
                    }
                }
                else {
                    $('#ddlPlaceOfSupply').val(0);
                }
                BusinessSetting = data.Data.BusinessSetting;
                setTaxList();

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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function setTaxList() {
    $('#divTax').empty();
    var ddlTax = '<select class="form-control select2" id="ddlTax"><option value="0-0">Select</option>';
    for (let ss = 0; ss < taxList.length; ss++) {
        if (taxList[ss].CanDelete == true) {
            if (BusinessSetting.CountryId == 2) {
                if (BusinessSetting.StateId == $('#ddlPlaceOfSupply').val()) {
                    if (taxList[ss].TaxTypeId != 3) {
                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                    }
                }
                else {
                    if (taxList[ss].CanDelete == false || taxList[ss].TaxTypeId == 3 || taxList[ss].TaxTypeId == 5) {
                        ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
                    }
                }
            }
            else {
                ddlTax = ddlTax + '<option value="' + taxList[ss].TaxId + '-' + taxList[ss].TaxPercent + '">' + taxList[ss].Tax + '</option>';
            }
        }
    }
    ddlTax = ddlTax + '</select>';
    $('#divTax').append(ddlTax);
    $('.select2').select2();
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

/*Customer modal*/
function openCustomerModal() {
    $("#CustomerModal").modal('show');
    toggleGstTreatment();
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date() });
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
                    $('#' + res.Id+'_M').show();
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

function insertGroup() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        UserGroup: $('#txtUserGroup').val(),
        PriceCalculationType: $('#ddlPriceCalculationType').val(),
        CalculationPercentage: $('#txtCalculationPercentage').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        Description: $('#txtDescription').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/salessettings/customerGroupInsert',
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
                $('#ddlUserGroup_M').append($('<option>', { value: data.Data.UserGroup.UserGroupId, text: data.Data.UserGroup.UserGroup }));
                $('#ddlUserGroup_M').val(data.Data.UserGroup.UserGroupId);
                $('#customerGroupModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

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
                $('#ddlPaymentTerm_M').append($('<option>', { value: data.Data.PaymentTerm.PaymentTermId, text: data.Data.PaymentTerm.PaymentTerm }));
                $('#ddlPaymentTerm_M').val(data.Data.PaymentTerm.PaymentTermId);

                $('#paymentTermModal').modal('toggle');

                $('#txtPaymentTerm').val('');
                $('#txtDays').val('');
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

function togglePriceCalculationType() {
    if ($('#ddlPriceCalculationType').val() == '1') {
        $('.divPercentage').show();
        $('.divSellingPriceGroup').hide();
        $('#ddlSellingPriceGroup').val('0');
    }
    else {
        $('.divPercentage').hide();
        $('.divSellingPriceGroup').show();
        $('#txtCalculationPercentage').val('');
    }
}

function toggleTaxPreference() {
    $('.divNonTaxable').hide();
    $('#ddlTaxExemption').val(0);

    if ($('#ddlTaxPreference option:selected').text() == 'Non-Taxable') {
        $('.divNonTaxable').show();
    }

    $('.select2').select2();
}

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
                    if ($('.' + res.Id + '_TaxExemption_ctrl').children("select").length > 0) {
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
                $('#ddlTaxExemption_M').append($('<option>', { value: data.Data.TaxExemption.TaxExemptionId, text: data.Data.TaxExemption.Reason }));
                $('#ddlTaxExemption_M').val(data.Data.TaxExemption.TaxExemptionId);
                $('#taxExemptionModal').modal('toggle');
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
    //if (!$('#txtAddrMobileNo').val()) {
        $('#txtAddrMobileNo').val($('#txtMobileNo').val())
    //}
}

function setAlternativeMobile() {
    //if (!$('#txtAddrMobileNo2').val()) {
        $('#txtAddrMobileNo2').val($('#txtAltMobileNo').val())
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
        url: '/customers/ActiveStates',
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
        url: '/customers/ActiveCitys',
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

/*Customer modal*/