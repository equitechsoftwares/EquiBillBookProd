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
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_PaymentDate').addClass('notranslate');
    fetchCompanyCurrency();
    fetchTax();
});

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

var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";
var NotificationName = '', NotificationId = 0;
var taxList = [], BusinessSetting;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        //FromAccount: $('#ddlFAccount').val(),
        //ToAccount: $('#ddlTAccount').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/PaymentFetch',
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
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var Amount = '', TaxAmount = '', TaxId = 0;
    if (window.location.pathname.toLowerCase().indexOf('advancepaymentadd') == -1) {
        AmountExcTax = parseFloat($('#txtAmount').val());
        //Amount = parseFloat($('#txtAmountExcTax').val());
        TaxAmount = 0;
        TaxId = 0;
    }
    else {
        AmountExcTax = (100 * (parseFloat($('#txtAmount').val()) / (100 + parseFloat($('#ddlTax').val() ? $('#ddlTax').val().split('-')[1] : 0)))).toFixed(2);
        TaxAmount = (parseFloat($('#txtAmount').val()) - AmountExcTax).toFixed(2);
        //Amount = ((parseFloat($('#txtAmountExcTax').val())) + ((parseFloat($('#ddlTax').val() ? $('#ddlTax').val().split('-')[1] : 0) / 100) * (parseFloat($('#txtAmountExcTax').val())))).toFixed(2);
        //TaxAmount = (Amount - parseFloat($('#txtAmountExcTax').val())).toFixed(2);
        TaxId = $('#ddlTax').val() ? $('#ddlTax').val().split('-')[0] : 0;
    }

    var SupplierPaymentIds = [];
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            SupplierPaymentIds.push({
                Type: $('#hdnType' + _id).val(),
                PurchaseId: $('#hdnPurchaseId' + _id).val(),
                Amount: $('#txtAmount' + _id).val(),
                Due: $('#hdnDue' + _id).val(),
                IsActive: true,
                IsDeleted: false,
            })
        }
    });
    var det = {
        ReferenceNo: $('#txtReferenceNo').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        AmountExcTax: AmountExcTax,//$('#txtAmountExcTax').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        Type: "Supplier Payment",//window.location.pathname == "/supplier/AdvancePaymentAdd" ? "Supplier Direct Advance Payment" : "Supplier Payment",
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        TaxId: TaxId,
        Amount: $('#txtAmount').val(), //Amount,
        TaxAmount: TaxAmount,
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        SupplierPaymentIds: SupplierPaymentIds
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/PaymentInsert',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/supplier/payment";
                }
                else {
                    window.location.href = "/supplier/paymentadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

//function update(i) {
//    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
//    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

//    $('.errorText').hide();
//    var det = {
//        ContraId: window.location.href.split('=')[1],
//        FromAccountId: $('#ddlFromAccount').val(),
//        IsActive: true,
//        IsDeleted: false,
//        Notes: $('#txtNotes').val(),
//        Amount: $('#txtAmount').val(),
//        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
//        //PaymentTypeId: $('#ddlPaymentType').val(),
//        AttachDocument: PaymentAttachDocument,
//        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
//        //Type: "Contra",
//        ToAccountId: $('#ddlToAccount').val(),
//        ReferenceNo: $('#txtReferenceNo').val(),
//        BranchId: $('#ddlBranch').val(),
//    };
//    $("#divLoading").show();
//    $.ajax({
//        url: '/banking/ContraUpdate',
//        datatype: "json",
//        data: det,
//        type: "post",
//        success: function (data) {

//            $("#divLoading").hide();
//            if (data == "True") {
//                $('#subscriptionExpiryModal').modal('toggle');
//                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
//                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
//                return
//            }
//            else if (data == "False") {
//                $('#subscriptionExpiryModal').modal('toggle');
//                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
//                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
//                return
//            }

//            if (data.Status == 0) {
//                if (EnableSound == 'True') { document.getElementById('error').play(); }
//                toastr.error(data.Message);
//            }
//            else if (data.Status == 2) {
//                if (EnableSound == 'True') { document.getElementById('error').play(); }
//                toastr.error('Invalid inputs, check and try again !!');
//                data.Errors.forEach(function (res) {
//                    $('#' + res.Id).show();
//                    $('#' + res.Id).text(res.Message);
//                });
//            }
//            else {
//                sessionStorage.setItem('showMsg', '1');
//                sessionStorage.setItem('Msg', data.Message);
//                if (i == 1) {
//                    window.location.href = "/banking/contra";
//                }
//                else {
//                    window.location.href = "/banking/contraadd";
//                }
//            }
//        },
//        error: function (xhr) {
//            $("#divLoading").hide();
//        }
//    });
//}

function Delete(SupplierPaymentId, ReferenceNo, IsDirectPayment, ParentPaymentId) {
    var r = confirm("Are you sure you want to delete " + ReferenceNo + "?");
    if (r == true) {

        var url = "";
        if (IsDirectPayment == true) {
            url = "/purchase/PurchasePaymentDelete";
        }
        else {
            url = "/supplier/PaymentDelete";
        }

        var det = {
            SupplierPaymentId: SupplierPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: url,
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
                    //$('#tr_' + SupplierPaymentId).remove();

                    if (IsDirectPayment == true) {
                        View(ParentPaymentId);
                    }

                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function Cancel(SupplierPaymentId, ReferenceNo, IsDirectPayment) {
    var r = confirm("Are you sure you want to cancel " + ReferenceNo + "?");
    if (r == true) {

        var url = "";
        if (IsDirectPayment == true) {
            url = "/purchase/PurchasePaymentCancel";
        }
        else {
            url = "/supplier/Paymentcancel";
        }

        var det = {
            SupplierPaymentId: SupplierPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: url,
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
                    //$('#tr_' + SupplierPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function View(SupplierPaymentId) {
    var det = {
        SupplierPaymentId: SupplierPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/PaymentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ViewModal').modal('show');
            $("#divView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchDue() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    fetchDues();
    var det = {
        SupplierId: $('#ddlSupplier').val(),
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/PaymentDue',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#divDue').text('Due: ' + parseFloat(data.Data.User.OpeningBalanceDue + data.Data.User.TotalPurchaseDue));

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

            BusinessSetting = data.Data.BusinessSetting;
            setTaxList();

            $("#divLoading").hide();
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
                if ($('#ddlSourceOfSupply').val() == $('#ddlDestinationOfSupply').val()) {
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

function PrintInvoice(InvoiceUrl) {
    //$.get(InvoiceUrl, function (data) {
    //    data = data.replace('noPrint','noPrint" style="display:none"')
    //    const WinPrint = window.open(
    //        InvoiceUrl,
    //        "_blank",
    //        "left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0"
    //    );
    //    WinPrint.document.write(data);
    //    WinPrint.document.close();
    //    WinPrint.focus();
    //    setTimeout(
    //        function () {
    //            WinPrint.print();
    //            WinPrint.close();
    //        }, 100);

    //});
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
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

function openRefundModal(type, SupplierPaymentId, title, BranchId) {
    _SupplierPaymentId = SupplierPaymentId;
    _BranchId = BranchId;
    var det = {
        SupplierPaymentId: SupplierPaymentId,
        Type: title,
        Title: title,
        BranchId: _BranchId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/Refunds',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divRefunds").html(data);

            $("#refundModal").modal('show');
            if (type == true) {
                $('.refundAdd').show();
                $('.refundList').hide();
                $('#refundModalLabel').text('Add Refund');

                var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
                $('#_RefundDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
                $('#_RefundDate').addClass('notranslate');

                $("[data-toggle=popover]").popover({
                    html: true,
                    trigger: "hover",
                    placement: 'auto',
                });

            }
            else {
                $('.refundAdd').hide();
                $('.refundList').show();
                $('#refundModalLabel').text('View Refunds');
            }

            /* $('.select2').select2();*/
            $('.select2').select2({
                dropdownParent: $('#refundModal')
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

function insertRefund() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    


    var det = {
        ReferenceNo: $('#txtReferenceNo').val(),
        BranchId: $('#ddlBranch').val(),
        SupplierId: $('#ddlSupplier').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtRefundDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        Type: "Supplier Refund",
        ParentId: _SupplierPaymentId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/RefundInsert',
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
                $("#refundModal").modal('hide');

                $('#txtPaymentNotes').val('');
                $('#txtAmount').val(0);
                $('#txtPaymentDate').val('');
                $('#ddlPaymentType').val(0);
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('#ddlLAccount').val(0);

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

function deleteRefund(SupplierPaymentId, ReferenceNo, ParentPaymentId) {
    var r = confirm("Are you sure you want to delete " + ReferenceNo + "?");
    if (r == true) {
        var det = {
            SupplierPaymentId: SupplierPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/Supplier/RefundDelete',
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
                    View(ParentPaymentId);

                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function openUnpaidInvoicesModal(SupplierId, SupplierPaymentId) {
    _SupplierPaymentId = SupplierPaymentId;
    var det = {
        SupplierId: SupplierId,
        SupplierPaymentId: SupplierPaymentId,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/UnpaidPurchaseInvoices',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divUnpaidInvoices").html(data);

            $("#UnpaidInvoicesModal").modal('show');

            var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
            var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

            $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
            $('#_PaymentDate').addClass('notranslate');

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

function updateTotal() {
    var subTotal = 0, total = 0;
    var Balance = $('#hdnAmountRemaining').val();

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            var AmountExcTax = $('#txtAmount' + _id).val();

            subTotal = subTotal + parseFloat(AmountExcTax);
        }
    });

    $('#divAmountToCredit').text(CurrencySymbol + subTotal.toFixed(2));
    $('#divRemainingCredits').text(CurrencySymbol + (Balance - subTotal.toFixed(2)));

}

function ApplyCreditsToInvoices() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var SupplierPaymentIds = [];
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#txtAmount' + _id).val() != '') {
            SupplierPaymentIds.push({
                Type: $('#hdnType' + _id).val(),
                PurchaseId: $('#hdnPurchaseId' + _id).val(),
                IsReverseCharge: $('#hdnIsReverseCharge' + _id).val(),
                Amount: $('#txtAmount' + _id).val(),
                IsActive: true,
                IsDeleted: false,
                DivId: _id
            })
        }
    });

    var det = {
        IsActive: true,
        IsDeleted: false,
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        ParentId: _SupplierPaymentId,
        SupplierPaymentIds: SupplierPaymentIds
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/ApplyCreditsToInvoices',
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
                    debugger
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#UnpaidInvoicesModal").modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchDues() {
    var det = {
        SupplierId: $('#ddlSupplier').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/Dues',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divDues").html(data);
            $('.chkIsActive').bootstrapToggle();
            $("#divLoading").hide();

            $('#tblPayments').DataTable({
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
}

function setAmounts() {
    var AmountUsed = 0;
    var Amount = parseFloat(parseFloat($('#txtAmount').val()).toFixed(2));

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#hdnDue' + _id).val() != '') {
            $('#txtAmount' + _id).val('');
        }
    });

    $('#divCombo tr').each(function () {
        if (Amount > 0) {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined && $('#hdnDue' + _id).val() != '') {
                var Due = parseFloat(parseFloat($('#hdnDue' + _id).val()).toFixed(2));
                if (Due >= Amount) {
                    $('#txtAmount' + _id).val(Amount);
                    Amount = 0;
                    AmountUsed = AmountUsed + Amount;
                }
                else {
                    $('#txtAmount' + _id).val(Due);
                    Amount = Amount - Due;
                    AmountUsed = AmountUsed + Due;
                }
            }
        }
    });
    $('#divAmountReceived').text($('#txtAmount').val());
    $('#divAmountUsedForPayments').text(AmountUsed);
    $('#divAmountInExcess').text(Amount.toFixed(2));
}

function setIndividualAmounts(_id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var AmountUsed = 0;
    var Amount = parseFloat(parseFloat($('#txtAmount').val()).toFixed(2)) || 0;

    $('#divAmountReceived').text(0);
    $('#divAmountUsedForPayments').text(0);
    $('#divAmountInExcess').text(0);

    //var _Due = parseFloat(parseFloat($('#hdnDue' + _id).val()).toFixed(2));
    //var _Amount = parseFloat(parseFloat($('#txtAmount' + _id).val()).toFixed(2));
    //if (_Due < _Amount) {
    //    if (EnableSound == 'True') { document.getElementById('error').play(); }
    //    toastr.error('Invalid inputs, check and try again !!');

    //    $('#divAmount' + _id+'_M').show();
    //    $('#divAmount' + _id+'_M').text('Amount received cannot be more than due');
    //    return;
    //}

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined && $('#hdnDue' + _id).val() != '') {
            if ($('#txtAmount' + _id).val()) {
                AmountUsed = AmountUsed + parseFloat(parseFloat($('#txtAmount' + _id).val()).toFixed(2));
            }            
        }
    });
    //if (Amount < AmountUsed) {
    //    if (EnableSound == 'True') { document.getElementById('error').play(); }
    //    toastr.error('Invalid inputs, check and try again !!');

    //    $('#divAmount_M').show();
    //    $('#divAmount_M').text('The amount entered for individual invoice(s) exceeds the total amount');
    //    return;
    //}
    $('#divAmountReceived').text(Amount.toFixed(2));
    $('#divAmountUsedForPayments').text(AmountUsed);
    $('#divAmountInExcess').text(Amount.toFixed(2) - AmountUsed.toFixed(2));
}

function toggleReverseCharge() {
    if ($('#chkIsReverseCharge').is(':checked')) {
        $('.divTax').show();
    }
    else {
        $('.divTax').hide();
    }
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