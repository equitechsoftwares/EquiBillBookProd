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

    $('#_Date').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat });
    $('#_Date').addClass('notranslate');
    $('#_PaymentDate').addClass('notranslate');

    fetchActiveAccountsDropdown();
    fetchCompanyCurrency();
    fetchTax();
    fetchTaxExemptions();
    fetchItemCodes();

    if (window.location.pathname.toLowerCase() == "/expense/edit") {
        toggleGstTreatment();
    }
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

var _PageIndex = 1, c = 1, taxModalId;
var AttachDocument = "";
var FileExtensionAttachDocument = "";
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";
var _excelJson = [];
var _BranchId = 0;
var accountSubTypes, taxList, counter = 100000;
var UploadPath = "", fileExtension = "";
var BatchNo = Math.floor(Math.random() * 26) + Date.now();
var skuCodes = [], taxExemptions, itemCodes, canOpenModal = true;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        AccountId: $('#ddlAccount').val(),
        //ExpenseCategoryId: $('#ddlExpenseCategoryId').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val()
        ReferenceNo: $('#txtReferenceNo').val(),
        UserId: $('#ddlUser').val(),
        CustomerId: $('#ddlCustomer').val(),
        SupplierId: $('#ddlSupplier').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseFetch',
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
            //$("#thead").insertBefore(".table-body");
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function View(ExpenseId) {
    var det = {
        ExpenseId: ExpenseId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/ExpenseView',
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

function ViewPayment(AccountsPaymentId) {
    var det = {
        AccountsPaymentId: AccountsPaymentId,
        Type: "Expense Payment",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/PaymentView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PaymentViewModal').modal('toggle');
            $("#divPaymentView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insert(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    //var totalAmount = 0;
    var ExpensePayments = [];
    var subTotal = 0, totalTaxAmount = 0, total = 0;
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var TaxAmount = 0;

            //var AmountExcTax = $('#txtAmount' + _id).val();
            //total = total + parseFloat(AmountExcTax);

            //if ($('#ddlTax' + _id).val() != 0) {
            //    AmountExcTax = (100 * (parseFloat($('#txtAmount' + _id).val()) / (100 + parseFloat($('#ddlTax' + _id).val().split('-')[1])))).toFixed(2);
            //    TaxAmount = (parseFloat($('#txtAmount' + _id).val()) - AmountExcTax).toFixed(2);
            //    totalTaxAmount = totalTaxAmount + TaxAmount;
            //}

            //subTotal = subTotal + parseFloat(AmountExcTax);

            var AmountExcTax = parseFloat($('#txtAmountExcTax' + _id).val());
            var AmountIncTax = parseFloat($('#txtAmount' + _id).val());

            subTotal = subTotal + parseFloat(AmountExcTax);
            total = total + parseFloat(AmountIncTax);

            TaxAmount = AmountIncTax - AmountExcTax;
            totalTaxAmount = totalTaxAmount + TaxAmount;

            ExpensePayments.push({
                AccountId: $('#ddlAccount' + _id).val(),
                Notes: $('#txtNotes' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Amount: $('#txtAmount' + _id).val(),
                AmountExcTax: AmountExcTax,
                TaxAmount: TaxAmount,
                IsActive: true,
                IsDeleted: false,
                DivId: _id,
                ITCType: $('#ddlITCType' + _id).val(),
                TaxExemptionId: $('#ddlTaxExemption' + _id).val(),
                ItemType: $('#ddlItemType' + _id).val(),
                ItemCodeId: $('#ddlItemCode' + _id).val(),
            })

            //if ($('#txtAmount' + _id).val() != '') {
            //    totalAmount = totalAmount + parseFloat($('#txtAmount' + _id).val());
            //}
        }
    });

    var det = {
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDate').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        CustomerId: $('#ddlCustomer').val(),
        SupplierId: $('#ddlSupplier').val(),
        AccountId: $('#ddlAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        //Amount: totalAmount,
        IsActive: true,
        IsDeleted: false,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        TaxId: $('#ddlTax').val(),
        ExpensePayments: ExpensePayments,
        Subtotal: subTotal,
        TaxAmount: totalTaxAmount,
        TotalQuantity: ExpensePayments.length,
        GrandTotal: total,
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        Type: "Expense",
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        CountryId: $('#hdnCountryId').val(),
        //Payment: {
        //    IsActive: true,
        //    IsDeleted: false,
        //    Notes: $('#txtPaymentNotes').val(),
        //    Amount: $('#txtPAmount').val(),
        //    PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        //    PaymentTypeId: $('#ddlPaymentType').val(),
        //    AttachDocument: PaymentAttachDocument,
        //    FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        //    AccountId: $('#ddlBankAccount').val(),
        //}
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseInsert',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/expense/index";
                }
                else {
                    window.location.href = "/expense/add";
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
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');



    var ExpensePayments = [];
    var subTotal = 0, totalTaxAmount = 0, total = 0;
    //var totalAmount = 0;

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var TaxAmount = 0;

            //var AmountExcTax = $('#txtAmount' + _id).val();
            //total = total + parseFloat(AmountExcTax);

            //if ($('#ddlTax' + _id).val() != 0) {
            //    AmountExcTax = (100 * (parseFloat($('#txtAmount' + _id).val()) / (100 + parseFloat($('#ddlTax' + _id).val().split('-')[1])))).toFixed(2);
            //    TaxAmount = (parseFloat($('#txtAmount' + _id).val()) - AmountExcTax).toFixed(2);
            //    totalTaxAmount = totalTaxAmount + TaxAmount;
            //}

            //subTotal = subTotal + parseFloat(AmountExcTax);

            var AmountExcTax = parseFloat($('#txtAmountExcTax' + _id).val());
            var AmountIncTax = parseFloat($('#txtAmount' + _id).val());

            subTotal = subTotal + parseFloat(AmountExcTax);
            total = total + parseFloat(AmountIncTax);

            TaxAmount = AmountIncTax - AmountExcTax;
            totalTaxAmount = totalTaxAmount + TaxAmount;

            ExpensePayments.push({
                ExpensePaymentId: $('#hdnExpensePaymentId' + _id).val(),
                AccountId: $('#ddlAccount' + _id).val(),
                Notes: $('#txtNotes' + _id).val(),
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                Amount: $('#txtAmount' + _id).val(),
                AmountExcTax: AmountExcTax,
                TaxAmount: TaxAmount,
                IsActive: true,
                //IsDeleted: false,
                IsDeleted: $("#divCombo" + _id).is(":hidden"),
                DivId: _id,
                ITCType: $('#ddlITCType' + _id).val(),
                TaxExemptionId: $('#ddlTaxExemption' + _id).val(),
                ItemType: $('#ddlItemType' + _id).val(),
                ItemCodeId: $('#ddlItemCode' + _id).val(),
            })

            //if ($('#txtAmount' + _id).val() != '') {
            //    totalAmount = totalAmount + parseFloat($('#txtAmount' + _id).val());
            //}
        }
    });

    var det = {
        ExpenseId: window.location.href.split('=')[1],
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDate').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        CustomerId: $('#ddlCustomer').val(),
        SupplierId: $('#ddlSupplier').val(),
        AccountId: $('#ddlAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        //Amount: totalAmount,
        IsActive: true,
        IsDeleted: false,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        TaxId: $('#ddlTax').val(),
        ExpensePayments: ExpensePayments,
        Subtotal: subTotal,
        TaxAmount: totalTaxAmount,
        TotalQuantity: ExpensePayments.length,
        GrandTotal: total,
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        Type: "Expense",
        IsReverseCharge: $('#chkIsReverseCharge').is(':checked') == true ? 1 : 2,
        CountryId: $('#hdnCountryId').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseUpdate',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/expense/index";
                }
                else {
                    window.location.href = "/expense/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(expenseId, IsActive) {
    var det = {
        expenseId: expenseId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
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

function Delete(expenseId, expenseName) {
    var r = confirm("This will delete \"" + expenseName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            expenseId: expenseId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/expense/expensedelete',
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

function getAttachDocumentBase64() {
    var file1 = $("#AttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('please select a file less than 2mb');
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

function insertCategory() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        ExpenseCategoryCode: $('#txtExpenseCategoryCode').val(),
        ExpenseCategory: $('#txtExpenseCategory').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/ExpenseCategoryInsert',
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
                $('#ddlExpenseCategory').append($('<option>', { value: data.Data.ExpenseCategory.ExpenseCategoryId, text: data.Data.ExpenseCategory.ExpenseCategory }));
                $('#ddlExpenseCategory').val(data.Data.ExpenseCategory.ExpenseCategoryId);
                $('#categoryModal').modal('toggle');

                $('#txtExpenseCategoryCode').val('');
                $('#txtExpenseCategory').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openPaymentModal(type, PurchaseId, title, BranchId) {
    _PurchaseId = PurchaseId;
    _BranchId = BranchId;
    var det = {
        Id: PurchaseId,
        Type: "Expense Payment",
        Title: title,
        BranchId: _BranchId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/payments',
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
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');
                $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
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
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertPayment() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Id: _PurchaseId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Expense Payment",
        AccountId: $('#ddlLAccount').val(),
        ReferenceNo: $('#txtMReferenceNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/paymentInsert',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
                $("#paymentModal").modal('hide');

                $('#txtPaymentNotes').val('');
                $('#txtAmount').val(0);
                $('#txtPaymentDate').val('');
                $('#ddlPaymentType').val(0);
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('#ddlLAccount').val(0);

            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function deletePayment(AccountsPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            AccountsPaymentId: AccountsPaymentId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/expense/paymentDelete',
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
                    $('#tr_' + AccountsPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function fetchActiveSubCategories() {
    var det = {
        ExpenseCategoryId: $('#ddlExpenseCategory').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/Expense/ActiveSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubCategories_Dropdown").html(data);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openUserModal() {
    $("#userModal").modal('show');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');
}

function insertUser() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        Username: $('#txtUsername').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        DOB: moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtDOB').val(),
        JoiningDate: moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),//$('#txtJoiningDate').val(),
        Gender: $('#ddlGender').val(),
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
        AltAddress: $('#txtAltAddress').val(),
        AltCountryId: $('#ddlAltCountry').val(),
        AltStateId: $('#ddlAltState').val(),
        AltCityId: $('#ddlAltCity').val(),
        AltZipcode: $('#txtAltZipcode').val(),
        //-UserRoleId: $('#ddlUserRole').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        UserType: 'user',
        IsActive: true,
        IsDeleted: false,
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
                $('#ddlContactPerson').append($('<option>', { value: data.Data.User.UserId, text: data.Data.User.Name + ' - ' + data.Data.User.MobileNo }));
                $('#ddlContactPerson').val(data.Data.User.UserId);
                $('#user').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

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

function uploadExcel() {
    //var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xlsx|.xls)$/;
    var regex = (/\.(xlsx|xls|xlsm)$/i);
    /*Checks whether the file is a valid excel file*/
    if (regex.test($("#excelfile").val().toLowerCase())) {
        var xlsxflag = false; /*Flag for checking whether excel is .xls format or .xlsx format*/
        if ($("#excelfile").val().toLowerCase().indexOf(".xlsx") > 0) {
            xlsxflag = true;
        }
        /*Checks whether the browser supports HTML5*/
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;
                /*Converts the excel data in to object*/
                if (xlsxflag) {
                    var workbook = XLSX.read(data, { type: 'binary' });
                }
                else {
                    var workbook = XLS.read(data, { type: 'binary' });
                }
                /*Gets all the sheetnames of excel in to a variable*/
                var sheet_name_list = workbook.SheetNames;

                var cnt = 0; /*This is used for restricting the script to consider only first sheet of excel*/
                sheet_name_list.forEach(function (y) { /*Iterate through all sheets*/
                    /*Convert the cell value to Json*/

                    if (xlsxflag) {
                        var exceljson = XLSX.utils.sheet_to_json(workbook.Sheets[y]);
                    }
                    else {
                        var exceljson = XLS.utils.sheet_to_row_object_array(workbook.Sheets[y]);
                    }
                    if (exceljson.length > 0 && cnt == 0) {
                        _excelJson = exceljson;
                    }
                });
                $('#exceltable').show();
            }
            if (xlsxflag) {/*If excel file is .xlsx extension than creates a Array Buffer from excel*/
                reader.readAsArrayBuffer($("#excelfile")[0].files[0]);
            }
            else {
                reader.readAsBinaryString($("#excelfile")[0].files[0]);
            }
        }
        else {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Sorry! Your browser does not support HTML5!');
        }
    }
    else {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please upload a valid Excel file!');
    }
}

function BulkInsert(i) {
    $("#divLoading").show();

    var det = {
        BranchId: $('#ddlBranch').val(),
        ExpenseImports: _excelJson
    }
    $.ajax({
        url: '/Expense/ImportExpense',
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
                if (i == 1) {
                    window.location.href = "/expense/index";
                }
                else {
                    window.location.reload();
                }
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchActiveUsers(showAddNew) {
    var det = {
        BranchId: $('#ddlBranch').val(),
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
                var ddlUser = '<select class="form-control select2" id="ddlUser"><option value="0">Select</option>';

                for (let ss = 0; ss < data.Data.Users.length; ss++) {
                    if (data.Data.Users[ss].UserType.toLowerCase() == "user") {
                        ddlUser = ddlUser + '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name + '</option>';
                    }
                }
                ddlUser = ddlUser + '</select>';

                if (showAddNew == true) {
                    ddlUser = ddlUser + '<span class="input-group-append">' +
                        '<a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#user"> + </a>' +
                        '</span>';
                }

                $('.divUser').empty();
                $('.divUser').append(ddlUser);

                $('.select2').select2();
            }

        },
        error: function (xhr) {

        }
    });
};

function fetchAccountMapped(_type) {
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

function openPaymentMethodModal() {
    $('#PaymentMethodModal').modal('toggle');
}

function insertPaymentMethod() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        PaymentType: $('#txtPaymentType_M').val(),
        IsActive: true,
        IsDeleted: false,
        IsPosShown: $('#chkIsPosShown').is(':checked'),
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/paymentTypeInsert',
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
                $('#ddlPaymentType').append($('<option>', { value: data.Data.PaymentType.PaymentTypeId, text: data.Data.PaymentType.PaymentType }));

                $('#ddlPaymentType').val(data.Data.PaymentType.PaymentTypeId);
                $('#PaymentMethodModal').modal('toggle');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openTaxModal(id) {
    if (canOpenModal == true) {
        taxModalId = id;

        $('.divTaxModal').show();
        $('.divTaxGroupModal').hide();

        $('#taxModal').modal('toggle');
    }
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
                $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);

                $('#ddlNewSubTax').append($('<option>', { value: data.Data.Tax.TaxId, text: data.Data.Tax.Tax }));
                var d = $('#ddlNewSubTax').val();
                if (d == null) d = [];
                d.push(data.Data.Tax.TaxId);
                //$('#ddlNewSubTax').val(d);

                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercent').val('');
                $('#txtNewTax').val('');

                updateTotal();
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
                $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                $('#taxModal').modal('toggle');

                $('#txtNewTaxpercentGroup').val('');
                $('#txtNewTaxGroup').val('');
                $('#ddlNewSubTax').val('');
                $('.select2').select2();

                updateTotal();
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
                    $('.ddlTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    $('#ddlTax' + taxModalId).val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
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

function fetCurrentDate() {
    if ($('#txtPAmount').val()) {
        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
        var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

        $('#txtPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
        $('#_PaymentDate').addClass('notranslate');
    }
    else {
        $('#txtPaymentDate').val('');
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

function addNew() {
    var _accountSubTypes = '';
    for (let i = 0; i < accountSubTypes.length; i++) {
        if (accountSubTypes[i].Accounts.length > 0) {
            if (accountSubTypes[i].Type != 'Accounts Receivable' && accountSubTypes[i].Type != 'Accounts Payable') {
                _accountSubTypes = _accountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                    if (accountSubTypes[i].Accounts[j].Type != "Opening Balance Adjustments" && accountSubTypes[i].Accounts[j].Type != "Tag Adjustments") {
                        _accountSubTypes = _accountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].DisplayAs + '</option>';
                    }
                }
            }
            '</optgroup>';
        }
    }

    var TaxListPermission = $('#hdnTaxListPermission').val().toLocaleLowerCase();

    var ddlItemCode = '<select class="form-control select2 ddlTax" id="ddlItemCode' + counter + '" ><option value="0">Select</option>';
    for (let ss = 0; ss < itemCodes.length; ss++) {
        /* if (ItemType == "Product") {*/
        if (itemCodes[ss].ItemCodeType == "HSN") {
            ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        }
        //}
        //else {
        //    if (itemCodes[ss].ItemCodeType == "SAC") {
        //        ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
        //    }
        //}
    }
    ddlItemCode = ddlItemCode + '</select>';


    var ddlTax = '<select class="form-control select2 ddlTax" id="ddlTax' + counter + '"  onchange="updateTotal(' + counter + ',0)">';
    for (let ss = 0; ss < taxList.length; ss++) {
        if (taxList[ss].Tax != "Taxable") {
            if ($('#hdnCountryId').val() == 2) {
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
    ddlTax = ddlTax + '</select >';

    var html = '<tr id="divCombo' + counter + '">' +
        '<td width="15%">' +
        '<div class="input-group">' +
        '<select class="form-control select2" id="ddlAccount' + counter + '">' +
        _accountSubTypes +
        '</select>' +
        '</div>' +
        '</td> ' +
        '<td width="5%">' +
        '<div class="form-group divItemType' + counter + '" style="margin-bottom:0;">' +
        '<div class="input-group">' +
        '<select class="form-control" id="ddlItemType' + counter + '" onchange=setItemCodes(' + counter + ')>' +
        '<option value="Product">Product</option>' +
        '<option value="Service">Service</option>' +
        '</select>' +
        '</div>' +
        '</div>' +
        '</td>' +
        '<td width="15%">' +
        '<div class="form-group" id="divItemCode' + counter + '" style="margin-bottom:0;">' +
        '<div class="input-group">' +
        ddlItemCode +
        '</div>' +
        '</div>' +
        '</td>' +
        '<td class="service" width="20%"> ' +
        '<div class="form-group"> ' +
        '<textarea id="txtNotes' + counter + '" class="form-control" ></textarea> ' +
        '</div> ' +
        '</td> ' +
        '<td class="service" width="15%">' +
        '<div class="form-group">' +
        '<input type = "number" class="form-control divAmountExcTax'+counter+'_ctrl" id="txtAmountExcTax' + counter + '" onkeypress="return onlyDecimalKey(event)" onchange="updateTotal(' + counter + ',0)">' +
        '</div>' +
        '<small class="text-red font-weight-bold errorText" id="divAmountExcTax' + counter + '"></small>' +
        '</td>' +
        '<td width="15%"> ' +
        '<div class="input-group" id="divTax' + counter + '">' +
        ddlTax +
        //'<span class="input-group-append ' + (TaxListPermission == 'true' ? '' : 'hidden') + '"><a href="javascript:void(0)" id="btnAddTax' + counter + '" class="btn btn-info" onclick="openTaxModal(' + counter + ')"> + </a></span>' +
        '</div>' +
        '<div class="form-group" id="divItc' + counter + '" style="display:none"></div>' +
        '<div class="form-group" id="divTaxExemption' + counter + '" style="display:none"></div>' +
        '</td> ' +
        '<td class="no_reversecharge" width="15%">' +
        '<div class="form-group">' +
        '<input disabled type = "number" class="form-control" id="txtAmount' + counter + '" onkeypress="return onlyDecimalKey(event)">' +
        '</div>' +        
        '</td>' +
        '<td width="5%">' +
        '<button type="button" class="btn btn-danger btn-sm" onclick="deleteCombo(' + counter + ')">' +
        '<i class="fas fa-times">' +
        '</i>' +
        '</button>' +
        '</td>' +
        '</tr>';
    $('#divCombo').append(html);
    $('.select2').select2();
    counter++;

    toggleReverseCharge();
}

function deleteCombo(id, ExpensePaymentId) {
    if (ExpensePaymentId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {

            $('#divCombo' + id).hide();
        $('#txtAmountExcTax' + id).val(0);
            updateTotal();
        //}
    }
    else {
        $('#divCombo' + id).remove();
        //$('#txtAmountExcTax' + id).val(0);
        updateTotal();
    }
}

function deleteFullCombo(ExpenseId) {
    if (ExpenseId != undefined) {
        //var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        //if (r == true) {
            $('#divCombo tr').each(function () {
                var _id = this.id.split('divCombo')[1];
                if (_id != undefined && $('#txtAmountExcTax' + _id).val() != '') {
                    $('#divCombo' + _id).hide();
                    $('#txtAmountExcTax' + _id).val(0);
                }
            });
            updateTotal();
        //}
    }
    else {
        $('#divCombo').empty();
        //$('#divCombo tr').each(function () {
        //    var _id = this.id.split('divCombo')[1];
        //    if (_id != undefined && $('#txtAmountExcTax' + _id).val() != '') {
        //        $('#divCombo' + _id).hide();
        //        $('#txtAmountExcTax' + _id).val(0);
        //    }
        //});
        updateTotal();
    }
}

function updateTotal() {
    var subTotal = 0, total = 0;
    var taxs = [];

    setItc_Exemption();

    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        //if (_id != undefined && $('#txtAmountExcTax' + _id).val() != '') {
        if (_id != undefined) {
            //var AmountExcTax = $('#txtAmount' + _id).val();
            //total = total + parseFloat(AmountExcTax);

            //if ($('#ddlTax' + _id).val() != 0) {
            //    AmountExcTax = (100 * (parseFloat($('#txtAmount' + _id).val()) / (100 + parseFloat($('#ddlTax' + _id).val().split('-')[1])))).toFixed(2);
            //}

            var AmountExcTax = parseFloat($('#txtAmountExcTax' + _id).val());
            var TaxPercentage = parseFloat($('#ddlTax' + _id).val().split('-')[1]);

            var AmountIncTax = AmountExcTax + ((TaxPercentage / 100) * AmountExcTax);
            $('#txtAmount' + _id).val(AmountIncTax);

            subTotal = subTotal + parseFloat(AmountExcTax);
            total = total + parseFloat(AmountIncTax);

            taxs.push({
                TaxId: $('#ddlTax' + _id).val().split('-')[0],
                AmountExcTax: AmountExcTax
            });
        }
    });

    if (taxs.length > 0) {
        var det = {
            Taxs: taxs,
        };
        //$("#divLoading").show();
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
                    var html = '<div class="col-12">' +
                        '<div class="row">' +
                        '<label for="inputEmail3" class="col-8 col-form-label text-right ' + ($('#chkIsReverseCharge').is(':checked') == false ? '' : 'hidden') + '">Sub Total :</label>' +
                        '<label for="inputEmail3" class="col-8 col-form-label text-right ' + ($('#chkIsReverseCharge').is(':checked') == true ? '' : 'hidden') + '">Total :</label>' +
                        '<label for="inputEmail3" class="col-4 col-form-label text-left" id="divSubTotal">' +
                        '</label>' +
                        '</div>' +
                        '</div> ';
                    data.Data.Taxs.forEach(function (res) {
                        html = html + '<div class="col-12 ' + ($('#chkIsReverseCharge').is(':checked') == false ? '' : 'hidden') + '">' +
                            '<div class="row">' +
                            '<label for="inputEmail3" class="col-8 col-form-label text-right">' + res.Tax + ' :</label>' +
                            '<label for="inputEmail3" class="col-4 col-form-label text-left">' + res.TaxAmount.toFixed(2) + '</label>' +
                            '</div>' +
                            '</div>';
                    });

                    html = html + '<div class="col-12 ' + ($('#chkIsReverseCharge').is(':checked') == false ? '' : 'hidden') + '">' +
                        '<div class="row">' +
                        '<label for="inputEmail3" class="col-8 col-form-label text-right">Total :</label>' +
                        '<label for="inputEmail3" class="col-4 col-form-label text-left" id="divTotal"></label>' +
                        '</div>' +
                        '</div>';

                    $('#divCalculationSummary').empty();
                    $('#divCalculationSummary').append(html);

                    $('#divSubTotal').text(CurrencySymbol + subTotal.toFixed(2));
                    $('#divTotal').text(CurrencySymbol + total.toFixed(2));
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

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
                taxList = data.Data.Taxs;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchTaxExemptions() {
    var det = {
        TaxExemptionType: "Item"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/taxsettings/ActiveTaxExemptions',
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
                taxExemptions = data.Data.TaxExemptions;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchItemCodes() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/othersettings/ActiveItemCodes',
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
                itemCodes = data.Data.ItemCodes;
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function copyTag(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

function uploadExcel() {
    //var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xlsx|.xls)$/;
    var regex = (/\.(xlsx|xls|xlsm)$/i);
    /*Checks whether the file is a valid excel file*/
    if (regex.test($("#excelfile").val().toLowerCase())) {
        var xlsxflag = false; /*Flag for checking whether excel is .xls format or .xlsx format*/
        if ($("#excelfile").val().toLowerCase().indexOf(".xlsx") > 0) {
            xlsxflag = true;
        }
        /*Checks whether the browser supports HTML5*/
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;
                /*Converts the excel data in to object*/
                if (xlsxflag) {
                    var workbook = XLSX.read(data, { type: 'binary' });
                }
                else {
                    var workbook = XLS.read(data, { type: 'binary' });
                }
                /*Gets all the sheetnames of excel in to a variable*/
                var sheet_name_list = workbook.SheetNames;

                var cnt = 0; /*This is used for restricting the script to consider only first sheet of excel*/
                sheet_name_list.forEach(function (y) { /*Iterate through all sheets*/
                    /*Convert the cell value to Json*/

                    if (xlsxflag) {
                        var exceljson = XLSX.utils.sheet_to_json(workbook.Sheets[y]);
                    }
                    else {
                        var exceljson = XLS.utils.sheet_to_row_object_array(workbook.Sheets[y]);
                    }
                    if (exceljson.length > 0 && cnt == 0) {
                        _excelJson = exceljson;
                    }
                });
                isExcelUpload = true;
                $('#exceltable').show();
            }
            if (xlsxflag) {/*If excel file is .xlsx extension than creates a Array Buffer from excel*/
                reader.readAsArrayBuffer($("#excelfile")[0].files[0]);
            }
            else {
                reader.readAsBinaryString($("#excelfile")[0].files[0]);
            }
        }
        else {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Sorry! Your browser does not support HTML5!');
        }
    }
    else {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please upload a valid Excel file!');
    }
}

function BulkInsert(i) {
    $('.errorText').hide();
    $('#divErrorMsg').hide();
    if (isExcelUpload == false) {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
        $('#divExcel').show();
        $('#divExcel').text('Upload Excel first');
        return;
    }

    $("#divLoading").show(); $("#divProgressBar").show();

    myInterval = setInterval(fetchBulkInsertProgress, 10000);

    var det = {
        BatchNo: BatchNo,
        ExpenseImports: _excelJson
    }
    $.ajax({
        url: '/expense/ImportExpense',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            $("#divProgressBar").hide();
            clearInterval(myInterval);

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
                $("#excelfile").val(null);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                $('#divErrorMsg').show();
                //$('#divExcel').show();
                var errorMsg = '';
                data.Errors.forEach(function (res) {
                    errorMsg = errorMsg + res.Message + '</br>';
                });

                //$('#divExcel').html(errorMsg);
                $('#txtErrorMsg').html(errorMsg);
                $("#excelfile").val(null);
                _excelJson = [];
                isExcelUpload = false;
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/expense/ExpenseImport";
                }
                else {
                    window.location.reload();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            $("#divProgressBar").hide();
            clearInterval(myInterval);
        }
    });
}

function fetchBulkInsertProgress() {
    var det = {
        BatchNo: BatchNo
    };
    $.ajax({
        url: '/expense/ExpenseCountByBatch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#divProgressBar .progress span').text(parseInt((data.Data.TotalCount / _excelJson.length) * 100) + '% Complete (success)');
            $('#divProgressBar .progress .progress-bar').css('width', parseInt((data.Data.TotalCount / _excelJson.length) * 100) + '%');
        },
        error: function (xhr) {

        }
    });
}

function FetchOtherSoftwareImport(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/expense/OtherSoftwareImportFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblOtherSoftwareImport").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function DeleteOtherSoftwareImport(OtherSoftwareImportId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            OtherSoftwareImportId: OtherSoftwareImportId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/expense/OtherSoftwareImportDelete',
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
                    document.getElementById('success').play();
                    toastr.success(data.Message);
                    FetchOtherSoftwareImport();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}
function getExcelBase64() {
    var file1 = $("#otherSoftwareExcelfile").prop("files")[0];

    var reader = new FileReader();
    reader.readAsDataURL(file1);
    reader.onload = function () {
        UploadPath = reader.result;
        fileExtension = '.' + file1.name.split('.').pop();
    };
    reader.onerror = function (error) {
        console.log(error);
        UploadPath = error;
    };
}
function InsertOtherSoftwareImport() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        UploadPath: UploadPath,
        FileExtension: fileExtension,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/OtherSoftwareImportInsert',
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
                $("#otherSoftwareExcelfile").val(null);
                UploadPath = "";
                fileExtension = "";
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                FetchOtherSoftwareImport();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleGstTreatment() {
    $('.divReverseCharge').show();
    if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)" || $('#ddlGstTreatment').val() == "Composition Taxable Supply"
        || $('#ddlGstTreatment').val() == "Taxable Supply to Unregistered Person" || $('#ddlGstTreatment').val() == "Taxable Supply to Consumer" ||
        $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
        || $('#ddlGstTreatment').val() == "Supply by SEZ Developer" || $('#ddlGstTreatment').val() == "Tax Deductor") {
        $('.divGst').show();
        $('.divSourceOfSupply').show();
        $('.divDestinationOfSupply').show();
        $('.divTaxPreference').show();

        if ($('#ddlGstTreatment').val() == "Taxable Supply to Unregistered Person" || $('#ddlGstTreatment').val() == "Taxable Supply to Consumer") {
            $('.divGst').hide();
        }

        if ($('#ddlGstTreatment').val() == "Composition Taxable Supply" || $('#ddlGstTreatment').val() == "Deemed Export") {
            $('.divReverseCharge').hide();
        }
    }
    else if ($('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)") {
        $('.divGst').show();
        $('.divSourceOfSupply').hide();
        $('.divTaxPreference').hide();
    }
    else if ($('#ddlGstTreatment').val() == "Non-GST Supply") {
        $('.divGst').hide();
        $('.divSourceOfSupply').show();
        $('.divTaxPreference').show();
        $('.divReverseCharge').hide();
    }
    else if ($('#ddlGstTreatment').val() == "Out Of Scope") {
        $('.divGst').hide();
        $('.divSourceOfSupply').hide();
        $('.divDestinationOfSupply').hide();
        $('.divTaxPreference').hide();
        $('.divReverseCharge').hide();
    }

    //$('#divCombo tr').each(function () {
    //    var _id = this.id.split('divCombo')[1];
    //    if (_id != undefined) {
    //        if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)" || $('#ddlGstTreatment').val() == "Taxable Supply to Consumer"
    //            || $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
    //            || $('#ddlGstTreatment').val() == "Tax Deductor" || $('#ddlGstTreatment').val() == "Supply by SEZ Developer") {
    //            $('#ddlTax' + _id).prop('disabled', false);
    //            canOpenModal = true;
    //        }
    //        else if ($('#ddlGstTreatment').val() == "Composition Taxable Supply" || $('#ddlGstTreatment').val() == "Taxable Supply to Unregistered Person"
    //            || $('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Non-GST Supply"
    //            || $('#ddlGstTreatment').val() == "Out Of Scope") {
    //            $('#ddlTax' + _id).prop('disabled', true);
    //            canOpenModal = false;
    //        }
    //    }
    //});
}

function fetchMileageRate() {
    $('#lblRate').text(0);

    var det = {
        VehicleId: $('#ddlVehicle').val()
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/purchasesettings/MileageRateFetch',
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
                //$('#lblRate').text(CurrencySymbol + JSON.stringify(data.Data.Vehicle.MileageRate));
                $('#txtMileageRate').val(data.Data.Vehicle.MileageRate);

                calculateMileageAmount();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleCalcuateMileageUsing() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#txtOdometerStartReading').val('');
    $('#txtOdometerEndReading').val('');
    $('#txtDistance').val('');
    $('#txtAmount').val('');
    var CalculateMileageUsing = 'Distance Travelled';
    if ($("#rdbOdometerReading").prop("checked")) {
        CalculateMileageUsing = "Odometer Reading";
        $('.divOdometerReading').show();
        $('#txtDistance').prop('disabled', true);
    }
    else {
        $('.divOdometerReading').hide();
        $('#txtDistance').prop('disabled', false);
    }
}

function calculateMileageAmount() {
    $('#txtAmount').val(parseFloat($('#txtDistance').val() || 0) * parseFloat($('#txtMileageRate').val() || 0));
}

function calculateDistance() {
    if ($("#rdbOdometerReading").prop("checked")) {
        if ($('#txtOdometerStartReading').val() && $('#txtOdometerEndReading').val()) {
            if (parseFloat($('#txtOdometerStartReading').val()) > parseFloat($('#txtOdometerEndReading').val())) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error("Odometer Start Reading cannot be greater than Odometer End Reading");
            }
            else {
                $('#txtDistance').val(parseFloat($('#txtOdometerEndReading').val()) - parseFloat($('#txtOdometerStartReading').val()));
                calculateMileageAmount();
            }
        }
    }
}
function insertMileage(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var CalculateMileageUsing = 'Distance Travelled';
    if ($("#rdbOdometerReading").prop("checked")) {
        CalculateMileageUsing = "Odometer Reading";
    }

    //var totalAmount = 0;
    var ExpensePayments = [];
    var subTotal = 0, totalTaxAmount = 0, total = 0;
    //$('#divCombo tr').each(function () {
    //    var _id = this.id.split('divCombo')[1];
    //    if (_id != undefined && $('#txtAmount' + _id).val() != '') {
    var TaxAmount = 0;

    var AmountExcTax = $('#txtAmount').val();
    total = total + parseFloat(AmountExcTax);

    //if ($('#ddlTax' + _id).val() != 0) {
    //    AmountExcTax = (100 * (parseFloat($('#txtAmount' + _id).val()) / (100 + parseFloat($('#ddlTax' + _id).val().split('-')[1])))).toFixed(2);
    //    TaxAmount = (parseFloat($('#txtAmount' + _id).val()) - AmountExcTax).toFixed(2);
    //    totalTaxAmount = totalTaxAmount + TaxAmount;
    //}

    subTotal = subTotal + parseFloat(AmountExcTax);

    ExpensePayments.push({
        AccountId: $('#ddlMileageAccount').val(),
        Notes: $('#txtNotes').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        Amount: $('#txtAmount').val(),
        AmountExcTax: AmountExcTax,
        TaxAmount: TaxAmount,
        IsActive: true,
        IsDeleted: false,
        //DivId: _id
    })
    //    }
    //});

    var det = {
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDate').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        CustomerId: $('#ddlCustomer').val(),
        SupplierId: $('#ddlSupplier').val(),
        AccountId: $('#ddlAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        //Amount: totalAmount,
        IsActive: true,
        IsDeleted: false,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        TaxId: $('#ddlTax').val(),
        ExpensePayments: ExpensePayments,
        Subtotal: subTotal,
        TaxAmount: totalTaxAmount,
        TotalQuantity: ExpensePayments.length,
        GrandTotal: total,
        //SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        //DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        //IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        //BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),
        VehicleId: $('#ddlVehicle').val(),
        CalculateMileageUsing: CalculateMileageUsing,
        OdometerStartReading: $('#txtOdometerStartReading').val(),
        OdometerEndReading: $('#txtOdometerEndReading').val(),
        Distance: $('#txtDistance').val(),
        MileageRate: $('#txtMileageRate').val(),
        UnitId: $('#hdnUnitId').val(),
        Type: "Mileage"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseInsert',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/expense/index";
                }
                else {
                    window.location.href = "/expense/mileageadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function updateMileage(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var CalculateMileageUsing = 'Distance Travelled';
    if ($("#rdbOdometerReading").prop("checked")) {
        CalculateMileageUsing = "Odometer Reading";
    }

    var ExpensePayments = [];
    var subTotal = 0, totalTaxAmount = 0, total = 0;
    //var totalAmount = 0;

    //$('#divCombo tr').each(function () {
    //    var _id = this.id.split('divCombo')[1];
    //    if (_id != undefined) {
    var TaxAmount = 0;

    var AmountExcTax = $('#txtAmount').val();
    total = total + parseFloat(AmountExcTax);

    //if ($('#ddlTax' + _id).val() != 0) {
    //    AmountExcTax = (100 * (parseFloat($('#txtAmount' + _id).val()) / (100 + parseFloat($('#ddlTax' + _id).val().split('-')[1])))).toFixed(2);
    //    TaxAmount = (parseFloat($('#txtAmount' + _id).val()) - AmountExcTax).toFixed(2);
    //    totalTaxAmount = totalTaxAmount + TaxAmount;
    //}

    subTotal = subTotal + parseFloat(AmountExcTax);
    ExpensePayments.push({
        ExpensePaymentId: $('#hdnExpensePaymentId').val(),
        AccountId: $('#ddlMileageAccount').val(),
        Notes: $('#txtNotes').val(),
        TaxId: $('#ddlTax').val() == null ? 0 : $('#ddlTax').val().split('-')[0],
        Amount: $('#txtAmount').val(),
        AmountExcTax: AmountExcTax,
        TaxAmount: TaxAmount,
        IsActive: true,
        IsDeleted: false,
        //DivId: _id
    })
    //    }
    //});

    var det = {
        ExpenseId: window.location.href.split('=')[1],
        Date: moment($("#txtDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDate').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        CustomerId: $('#ddlCustomer').val(),
        SupplierId: $('#ddlSupplier').val(),
        AccountId: $('#ddlAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        //Amount: totalAmount,
        IsActive: true,
        IsDeleted: false,
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        TaxId: $('#ddlTax').val(),
        ExpensePayments: ExpensePayments,
        Subtotal: subTotal,
        TaxAmount: totalTaxAmount,
        TotalQuantity: ExpensePayments.length,
        GrandTotal: total,
        //SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
        //DestinationOfSupplyId: $('#ddlDestinationOfSupply').val(),
        //IsBusinessRegistered: $('#chkIsBusinessRegistered').is(':checked') ? 1 : 2,
        GstTreatment: $('#ddlGstTreatment').val(),
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName').val(),
        /*BusinessRegistrationNo: $('#txtBusinessRegistrationNo').val(),*/
        VehicleId: $('#ddlVehicle').val(),
        CalculateMileageUsing: CalculateMileageUsing,
        OdometerStartReading: $('#txtOdometerStartReading').val(),
        OdometerEndReading: $('#txtOdometerEndReading').val(),
        Distance: $('#txtDistance').val(),
        MileageRate: $('#txtMileageRate').val(),
        UnitId: $('#hdnUnitId').val(),
        Type: "Mileage"
    };
    $("#divLoading").show();
    $.ajax({
        url: '/expense/expenseUpdate',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/expense/index";
                }
                else {
                    window.location.href = "/expense/mileageadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
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
                        $('#ddlGstTreatment').val(data.Data.User.GstTreatment);
                        toggleGstTreatment();
                    }
                    $('#txtBusinessRegistrationNo').val(data.Data.User.BusinessRegistrationNo);
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
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function fetchUpdatedItems() {
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            var ddlTax = '<select class="form-control select2 ddlTax" id="ddlTax' + _id + '"  onchange="updateTotal(' + _id + ',0)">';
            for (let ss = 0; ss < taxList.length; ss++) {
                var canAdd = true;
                if (taxList[ss].Tax != "Taxable") {
                    if ($('#chkIsReverseCharge').is(':checked')) {
                        if (taxList[ss].Tax == "Non-Taxable") {
                            canAdd = false;
                        }
                    }
                    if (canAdd == true) {
                        if ($('#hdnCountryId').val() == 2) {
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
            }
            ddlTax = ddlTax + '</select>';
            $('#divTax' + _id).empty();
            $('#divTax' + _id).append(ddlTax);
        }
    });
    $('.select2').select2();

    toggleGstTreatment();
}

function setItc_Exemption() {
    $('#divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {

            $('#divItc' + _id).hide();
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

                $('#divItc' + _id).empty();
                $('#divItc' + _id).append(html);

                $('#divItc' + _id).show();
            }
            else if (taxValue != "Select" && taxValue != "Non-Taxable" && taxValue != "Out of Scope" && taxValue != "Non-GST Supply") {
                if ($('#hdnCountryId').val() == 2) {
                    var html = '<label style="margin-bottom:0;margin-top:0.5rem;">Input Tax Credit</label>' +
                        '<div class="input-group">' +
                        '<select class="form-control" style="min-width: 160px;" id="ddlITCType1">' +
                        '<option value="Eligible For ITC">Eligible For ITC</option>' +
                        '<option value="Ineligible - As per Section 17 (5)">Ineligible - As per Section 17 (5)</option>' +
                        '<option value="Ineligible - Others">Ineligible - Others</option>' +
                        '</select>' +
                        '</div>';

                    $('#divItc' + _id).empty();
                    $('#divItc' + _id).append(html);

                    $('#divItc' + _id).show();
                }
            }
        }
    });
    $('.select2').select2();
}

function setItemCodes(_id) {
    $('#divItemCode' + _id).hide();
    if ($('#hdnCountryId').val() == 2) {

        var ItemType = $('#ddlItemType' + _id).val();

        var ddlItemCode = '<select class="form-control select2 ddlTax" id="ddlItemCode' + _id + '" ><option value="0">Select</option>';
        for (let ss = 0; ss < itemCodes.length; ss++) {
            if (ItemType == "Product") {
                if (itemCodes[ss].ItemCodeType == "HSN") {
                    ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                }
            }
            else {
                if (itemCodes[ss].ItemCodeType == "SAC") {
                    ddlItemCode = ddlItemCode + '<option value="' + itemCodes[ss].ItemCodeId + '">' + itemCodes[ss].Code + '</option>';
                }
            }
        }
        ddlItemCode = ddlItemCode + '</select>';

        //var html = '<label style="margin-bottom:0;margin-top:0.5rem;"> ' + (ItemType == "Product" ? "HSN" : "SAC") + ' <span class="danger">*</span></label>' +
        var html = '<div class="input-group">' +
            ddlItemCode +
            '</div>';

        $('#divItemCode' + _id).empty();
        $('#divItemCode' + _id).append(html);

        $('#divItemCode' + _id).show();
    }
    $('.select2').select2();
}

function toggleReverseCharge() {
    if ($('#chkIsReverseCharge').is(':checked')) {
        $('#lblReverseCharge').show();
        $('.no_reversecharge').hide();
    }
    else {
        $('#lblReverseCharge').hide();
        $('.no_reversecharge').show();
    }
    fetchUpdatedItems();
    updateTotal();

}

function openVehicleModal() {
    $("#vehicleModal").modal('show');
}

function insertVehicle() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        VehicleName: $('#txtVehicleName_M').val(),
        MileageRate: $('#txtMileageRate_M').val(),
        Description: $('#txtDescription_M').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchasesettings/VehicleInsert',
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
                $('#ddlVehicle').append($('<option>', { value: data.Data.Vehicle.VehicleId, text: data.Data.Vehicle.VehicleName }));
                $('#ddlVehicle').val(data.Data.Vehicle.VehicleId);

                $('#txtMileageRate').val($('#txtMileageRate_M').val());

                $('#vehicleModal').modal('toggle');

                $('#txtVehicleName_M').val('');
                $('#txtMileageRate_M').val('');
                $('#txtDescription_M').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};