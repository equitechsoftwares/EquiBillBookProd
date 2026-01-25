
$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_MigrationDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_MigrationDate').addClass('notranslate');

    $('.select2').select2();

    fetchCompanyCurrency();
});
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

function fetchOpeningBalance() {
    var det = {
        BranchId: $('#ddlBranch').val()
    };
    _pageindex = det.pageindex;
    $("#divloading").show();
    $.ajax({
        url: '/organizationsettings/OpeningBalanceFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divOpeningBalance").html(data);
            $("#divloading").hide();
        },
        error: function (xhr) {
            $("#divloading").hide();
        }
    });
};

function UpdateOpeningBalance() {
    canSubmit = true;

    if (($('#lblDebitDifference').text().replace(/[^0-9.]/g, '') != '' && $('#lblDebitDifference').text().replace(/[^0-9.]/g, '') != '0')
        || ($('#lblCreditDifference').text().replace(/[^0-9.]/g, '') != '' && $('#lblCreditDifference').text().replace(/[^0-9.]/g, '') != '0')) {
        if (confirm('The total debits and credits does not match. You can adjust the balances to remove the difference, or you can continue and the difference will be transferred to the Opening Balance Adjustment account automatically. Do you want to continue?')) {
            canSubmit = true;
        }
        else {
            canSubmit = false;
        }
    }

    if (canSubmit == true) {
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');
        

        var accountOpeningBalances = [];
        $('.divAccountsOpeningBalance tr').each(function () {
            var _id = this.id.split('divCombo')[1];
            if (_id != undefined) {
                accountOpeningBalances.push({
                    AccountOpeningBalanceId: $('#hdnAccountOpeningBalanceId' + _id).val(),
                    AccountId: _id,
                    Debit: $('#txtDebit' + _id).val(),
                    Credit: $('#txtCredit' + _id).val(),
                    BranchId: $('#ddlBranch').val(),
                })
            }
        });

        accountOpeningBalances.push({
            AccountOpeningBalanceId: $('#hdnAccountOpeningBalanceId').val(),
            AccountId: $('#hdnOpeningBalanceAdjustments').val(),
            Debit: $('#lblDebitDifference').text().replace(/[^0-9.]/g, ''),
            Credit: $('#lblCreditDifference').text().replace(/[^0-9.]/g, ''),
            BranchId: $('#ddlBranch').val(),
        })

        var det = {
            MigrationDate: $('#txtMigrationDate').val(),
            AccountOpeningBalances: accountOpeningBalances
        };
        $("#divLoading").show();
        $.ajax({
            url: '/organizationsettings/UpdateOpeningBalance',
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
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchOpeningBalance();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function updateOpeningBalanceTotal(id, type) {
    var totalDebit = 0, totalCredit = 0;

    if (type == 0) {
        $('#txtCredit' + id).val('');
    }
    else {
        $('#txtDebit' + id).val('');
    }

    $('.divAccountsOpeningBalance tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if ($('#txtDebit' + _id).val()) {
            totalDebit = totalDebit + parseFloat($('#txtDebit' + _id).val());
        }

        if ($('#txtCredit' + _id).val()) {
            totalCredit = totalCredit + parseFloat($('#txtCredit' + _id).val());
        }

    });

    $('#lblSubTotalDebit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
    $('#lblSubTotalCredit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);

    //$('#lblDebitDifference').show();
    //$('#lblCreditDifference').show();

    if (totalDebit > totalCredit) {
        $('#lblCreditDifference').text(CurrencySymbol + Math.round((totalDebit - totalCredit) * 100) / 100);
        $('#lblDebitDifference').text('');
        $('#lblTotalCredit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
        $('#lblTotalDebit').text(CurrencySymbol + Math.round(totalDebit * 100) / 100);
        //$('#lblDebitDifference').hide();
        //$('#divDifference').show();
    }
    else if (totalCredit > totalDebit) {
        $('#lblDebitDifference').text(CurrencySymbol + Math.round((totalCredit - totalDebit) * 100) / 100);
        $('#lblCreditDifference').text('');
        $('#lblTotalCredit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);
        $('#lblTotalDebit').text(CurrencySymbol + Math.round(totalCredit * 100) / 100);
        //$('#lblCreditDifference').hide();
        //$('#divDifference').show();
    }
    else {
        $('#lblDebitDifference').text('');
        $('#lblCreditDifference').text('');
        //$('#divDifference').hide();
    }
}

function DeleteOpeningBalance() {
    if (confirm('Are you sure you want to delete all opening balances for ' + $("#ddlBranch option:selected").text() + '?')) {
        var det = {
            BranchId: $("#ddlBranch").val()
        };
        $("#divLoading").show();
        $.ajax({
            url: '/organizationsettings/DeleteOpeningBalance',
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
                        $('#' + res.Id + '_E').show();
                        $('#' + res.Id + '_E').text(res.Message);

                        
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
                    fetchOpeningBalance();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }

}
