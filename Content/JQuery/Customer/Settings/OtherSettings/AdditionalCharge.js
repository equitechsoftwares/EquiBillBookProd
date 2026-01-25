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
    
    // Initialize account modal select2
    $('#accountModal').on('shown.bs.modal', function () {
        $('.select2').select2();
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];
var interval = null;
if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 100);
}
function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/AdditionalChargeFetch',
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
}

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    var det = {
        Name: $('#txtAdditionalCharge').val(),
        ItemCodeId: $('#ddlItemCode').val(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxPreference: $('#ddlTaxPreference option:selected').text(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        IntraStateTaxId: $('#ddlIntraStateTax').val().split('-')[0],
        InterStateTaxId: $('#ddlInterStateTax').val().split('-')[0],
        Description: $('#txtDescription').val(),
        PurchaseAccountId: $('#ddlPurchaseAccount').val(),
        SalesAccountId: $('#ddlSalesAccount').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/AdditionalChargeInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            } else if (data.Status == 2) {
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
                    } else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            } else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/othersettings/additionalchargelist";
                } else {
                    window.location.href = "/othersettings/additionalchargeadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function update(i) {
    $('[style*="border: 2px"]').css('border', '');
    var det = {
        AdditionalChargeId: window.location.href.split('=')[1],
        Name: $('#txtAdditionalCharge').val(),
        ItemCodeId: $('#ddlItemCode').val(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxPreference: $('#ddlTaxPreference option:selected').text(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        IntraStateTaxId: $('#ddlIntraStateTax').val().split('-')[0],
        InterStateTaxId: $('#ddlInterStateTax').val().split('-')[0],
        Description: $('#txtDescription').val(),
        PurchaseAccountId: $('#ddlPurchaseAccount').val(),
        SalesAccountId: $('#ddlSalesAccount').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/AdditionalChargeUpdate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            } else if (data.Status == 2) {
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
                    } else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            } else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/othersettings/additionalchargelist";
                } else {
                    window.location.href = "/othersettings/additionalchargeadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function ActiveInactive(additionalChargeId, IsActive) {
    var det = {
        AdditionalChargeId: additionalChargeId,
        IsActive: IsActive,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/AdditionalChargeActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            } else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList(_PageIndex);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function Delete(additionalChargeId, name) {
    var r = confirm("This will delete '" + name + "' permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            AdditionalChargeId: additionalChargeId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/othersettings/AdditionalChargeDelete',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                $("#divLoading").hide();
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                } else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchList(_PageIndex);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
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
                $('#ddlIntraStateTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (type == 'IntraStateTax') {
                    $('#ddlIntraStateTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                }

                $('#ddlInterStateTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                if (type == 'InterStateTax') {
                    $('#ddlInterStateTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
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
                    $('#ddlIntraStateTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    if (type == 'IntraStateTax') {
                        $('#ddlIntraStateTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
                    }

                    $('#ddlInterStateTax').append($('<option>', { value: data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent, text: data.Data.Tax.Tax }));
                    if (type == 'InterStateTax') {
                        $('#ddlInterStateTax').val(data.Data.Tax.TaxId + '-' + data.Data.Tax.TaxPercent);
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
        url: '/othersettings/FetchTaxPercent',
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

    $('.select2').select2();
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
                $('#ddlIntraStateTax').val(data.Data.ItemCode.IntraStateTaxId + '-' + data.Data.ItemCode.IntraStateTaxPercentage);
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

// Account Modal Functions
function openAccountModal(accountType) {
    $('#accountModal').modal('toggle');
    $('#accountModal .modal-title').text('Add ' + accountType + ' Account');
    
    // Reset form
    $('#ddlAccountSubType_Modal').val('0');
    $('#ddlAccount_Modal').empty();
    $('#txtAccountName_Modal').val('');
    $('#txtAccountNumber_Modal').val('');
    $('#txtBankName_Modal').val('');
    $('#txtNotes_Modal').val('');
    $('#addrow_modal').empty();
    
    // Hide error messages
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    // Store account type for later use
    $('#accountModal').data('accountType', accountType);
    
    // Populate account groups based on account type
    populateAccountGroups(accountType);
    
    // Initialize select2
    $('.select2').select2();
}

function populateAccountGroups(accountType) {
    // Clear existing options
    $('#ddlAccountSubType_Modal').empty();
    
    // Use the ViewBag data that's now available in JavaScript
    if (typeof accountTypesData !== 'undefined' && accountTypesData) {
        var html = '';
        
        accountTypesData.forEach(function(accountTypeItem) {
            if (accountTypeItem.AccountSubTypes && accountTypeItem.AccountSubTypes.length > 0) {
                var relevantSubTypes = [];
                
                // Filter account sub types based on the account type (Sales vs Purchase)
                accountTypeItem.AccountSubTypes.forEach(function(subType) {
                    if (accountType === 'Sales') {
                        // For Sales Account, show only Income related account groups
                        if (subType.Type === 'Income') {
                            relevantSubTypes.push(subType);
                        }
                    } else if (accountType === 'Purchase') {
                        // For Purchase Account, show only Expense related account groups
                        if (subType.Type === 'Expense') {
                            relevantSubTypes.push(subType);
                        }
                    }
                });
                
                // Add optgroup if there are relevant sub types
                if (relevantSubTypes.length > 0) {
                    html += '<optgroup label="' + accountTypeItem.AccountType + '">';
                    relevantSubTypes.forEach(function(subType) {
                        html += '<option value="' + subType.AccountSubTypeId + '">' + subType.DisplayAs + '</option>';
                    });
                    html += '</optgroup>';
                }
            }
        });
        
        $('#ddlAccountSubType_Modal').append(html);
    }

    fetchActiveAccountsModal();
}

function fetchActiveAccountsModal() {
    // if ($('#ddlAccountSubType_Modal option:selected').text() == "Bank" || $('#ddlAccountSubType_Modal option:selected').text() == "Credit Card") {
    //     $('#accountModal .divBank').show();
    //     $('#accountModal .divAccount').hide();
    //     $('#lblAccountNumber_Modal').text('Account Number');
    // }
    // else {
    //     $('#accountModal .divBank').hide();
    //     $('#accountModal .divAccount').show();
    //     $('#lblAccountNumber_Modal').html('(GL) Code <i tabindex="0" class="fa fa-info-circle text-info hover-q no-print @(Request.Cookies["SystemSetting"].Value.Split('&')[0].Split('=')[1] == "True" ? "":"hidden")" role="button" data-toggle="popover" data-trigger="focus" title="" data-content="General Ledger Code"></i>');
    // }

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {};
    $("#divLoading").show();
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
                    $('#' + res.Id + '_Modal').show();
                    $('#' + res.Id + '_Modal').text(res.Message);
                });
            }
            else {
                var accountSubTypes = data.Data.AccountSubTypes;
                var _accountSubTypes = '';
                for (let i = 0; i < accountSubTypes.length; i++) {
                    if ($('#ddlAccountSubType_Modal option:selected').text() == accountSubTypes[i].DisplayAs) {
                        if (accountSubTypes[i].Accounts.length > 0) {
                            _accountSubTypes = _accountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                            for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                                _accountSubTypes = _accountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].DisplayAs + '</option>';
                            }
                            _accountSubTypes = _accountSubTypes + '</optgroup>';
                        }
                    }
                }

                var html = '<select class="form-control select2" id="ddlAccount_Modal"><option value="0">Select</option> ' + _accountSubTypes + '</select>';
                $('#divParentAccount_Modal').empty();
                $('#divParentAccount_Modal').append(html);

                $('.select2').select2();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function insertAccount() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var AccountDetails = [];
    $('#addrow_modal .divAccountDetails').each(function () {
        var id = this.id.split('row')[1];
        AccountDetails.push({ Label: $('#txtLabel' + this.id.split('rowNew')[1]).val(), Value: $('#txtValue' + this.id.split('rowNew')[1]).val() });
    });

    var det = {
        AccountName: $('#txtAccountName_Modal').val(),
        AccountNumber: $('#txtAccountNumber_Modal').val(),
        AccountTypeId: $('#ddlAccountSubType_Modal').val(),
        AccountSubTypeId: $('#ddlAccountSubType_Modal').val(),
        OpeningBalance: '0',
        Notes: $('#txtNotes_Modal').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName_Modal').val(),
        BranchName: '',
        BranchCode: '',
        ParentId: $('#ddlAccount_Modal').val(),
        DisplayAs: $('#ddlAccount_Modal').val() == 0 ? $('#txtAccountName_Modal').val() : $("#ddlAccount_Modal option:selected").text() + ' -> ' + $('#txtAccountName_Modal').val(),
    };
    
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/ChartOfAccountInsert',
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
                    $('#' + res.Id + '_Modal').show();
                    $('#' + res.Id + '_Modal').text(res.Message);

                    if ($('.' + res.Id + '_Modal_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_Modal_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_Modal_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_Modal_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_Modal_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                // Add the new account to the appropriate dropdown
                var accountType = $('#accountModal').data('accountType');
                var accountName = data.Data.Account.DisplayAs;
                var accountId = data.Data.Account.AccountId;
                
                if (accountType === 'Purchase') {
                    $('#ddlPurchaseAccount').append($('<option>', { value: accountId, text: accountName }));
                    $('#ddlPurchaseAccount').val(accountId);
                } else if (accountType === 'Sales') {
                    $('#ddlSalesAccount').append($('<option>', { value: accountId, text: accountName }));
                    $('#ddlSalesAccount').val(accountId);
                }
                
                // Close modal and show success message
                $('#accountModal').modal('toggle');
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success('Account added successfully!');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}