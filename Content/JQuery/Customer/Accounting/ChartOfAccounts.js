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

var _PageIndex = 1, c = 1, AccountId = 0;
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        AccountTypeId: $('#ddlAccountType').val(),
        AccountName: $('#txtAccountName').val(),
        AccountNumber: $('#txtAccountNumber').val(),
        AccountSubTypeId: $('#ddlAccountSubType').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/ChartOfAccountFetch',
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

function View(AccountId) {
    var det = {
        AccountId: AccountId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountView',
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

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var AccountDetails = [];
    $('#addrow .divAccountDetails').each(function () {
        var id = this.id.split('row')[1];
        AccountDetails.push({ Label: $('#txtLabel' + this.id.split('rowNew')[1]).val(), Value: $('#txtValue' + this.id.split('rowNew')[1]).val() });
    });

    var det = {
        AccountName: $('#txtAccountName').val(),
        AccountNumber: $('#txtAccountNumber').val(),
        AccountTypeId: $('#ddlAccountType').val(),
        AccountSubTypeId: $('#ddlAccountSubType').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        Notes: $('#txtNotes').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName').val(),
        BranchName: $('#txtBranchName').val(),
        BranchCode: $('#txtBranchCode').val(),
        ParentId: $('#ddlAccount').val(),
        DisplayAs: $('#ddlAccount').val() == 0 ? $('#txtAccountName').val() : $("#ddlAccount option:selected").text() + ' -> ' + $('#txtAccountName').val(),
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
                    window.location.href = "/accounts/chartofaccount";
                }
                else {
                    window.location.href = "/accounts/chartofAccountadd";
                }
            }
            /*$("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var AccountDetails = [];
    $('#addrow .divAccountDetails').each(function () {
        //var id = this.id.split('row')[1];
        if (this.id.indexOf('New') == -1) {
            AccountDetails.push({ Label: $('#txtLabel' + this.id.split('row')[1]).val(), Value: $('#txtValue' + this.id.split('row')[1]).val(), AccountDetailsId: this.id.split('row')[1] });
        }
        else {
            AccountDetails.push({ Label: $('#txtLabel' + this.id.split('rowNew')[1]).val(), Value: $('#txtValue' + this.id.split('rowNew')[1]).val() });
        }
    });

    var det = {
        AccountId: window.location.href.split('=')[1].replace('#', ''),
        AccountName: $('#txtAccountName').val(),
        AccountNumber: $('#txtAccountNumber').val(),
        AccountTypeId: $('#ddlAccountType').val(),
        AccountSubTypeId: $('#ddlAccountSubType').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        Notes: $('#txtNotes').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName').val(),
        BranchName: $('#txtBranchName').val(),
        BranchCode: $('#txtBranchCode').val(),
        ParentId: $('#ddlAccount').val(),
        DisplayAs: $('#ddlAccount').val() == 0 ? $('#txtAccountName').val() : $("#ddlAccount option:selected").text() + ' -> ' + $('#txtAccountName').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/ChartOfAccountUpdate',
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
                    window.location.href = "/accounts/chartofaccount";
                }
                else {
                    window.location.href = "/accounts/ChartofAccountadd";
                }
            }
            /*$("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(AccountId, IsActive) {
    var det = {
        AccountId: AccountId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountActiveInactive',
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
            /*$("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(AccountId, AccountName) {
    var r = confirm("This will delete \"" + AccountName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            AccountId: AccountId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/accounts/Accountdelete',
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
                /*$("#divLoading").hide();*/
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function fetchActiveAccountSubTypes() {
    var det = {
        AccountTypeId: $('#ddlAccountType').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/ActiveAccountSubTypes',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();

            $("#ddlAccountSubType").html('');
            $("#ddlAccountSubType").append($("<option></option>").val(0).html("All"));
            $.each(data.Data.AccountSubTypes, function (i, value) {
                if (value.Type != "Bank" && value.Type != "Credit Card") {
                    $("#ddlAccountSubType").append($("<option></option>").val(value.AccountSubTypeId).html(value.DisplayAs));
                }
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveAccounts() {
    if ($('#ddlAccountSubType option:selected').text() == "Bank" || $('#ddlAccountSubType option:selected').text() == "Credit Card") {

        $('.divBank').show();
        $('.divAccount').hide();
        $('#lblAccountNumber').text('Account Number');
    }
    else {
        $('.divBank').hide();
        $('.divAccount').show();
        $('#lblAccountNumber').text('General Ledger (GL) Code');
    }

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
    };
    /*$("#divLoading").show();*/
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
                var _accountSubTypes = '';
                for (let i = 0; i < accountSubTypes.length; i++) {
                    if ($('#ddlAccountSubType option:selected').text() == accountSubTypes[i].DisplayAs) {
                        if (accountSubTypes[i].Accounts.length > 0) {
                            _accountSubTypes = _accountSubTypes + '<optgroup label="' + accountSubTypes[i].DisplayAs + '">';
                            for (let j = 0; j < accountSubTypes[i].Accounts.length; j++) {
                                _accountSubTypes = _accountSubTypes + '<option value="' + accountSubTypes[i].Accounts[j].AccountId + '">' + accountSubTypes[i].Accounts[j].DisplayAs + '</option>';
                            }
                            '</optgroup>';
                        }
                    }
                }

                var html = '<select class="form-control select2" id="ddlAccount"><option value="0">Select</option> ' + _accountSubTypes + '</select>';
                $('#divParentAccount').empty();
                $('#divParentAccount').append(html);

                $('.select2').select2();
            }
        },
        error: function (xhr) {
            /*$("#divLoading").hide();*/
        }
    });
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
    $("#addrow").append(markup);
});

$('#addrow').on('click', '.delete-row', function () {
    var i = $(this).attr('id');
    if (i.indexOf('btnNew') == -1) {
        var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
        if (r == true) {
            var det = {
                AccountDetailsId: i.split('btn')[1],
            };
            $("#divLoading").show();
            $.ajax({
                url: '/banking/AccountDetailsdelete',
                datatype: "json",
                data: det,
                type: "post",
                success: function (data) {
                    if (data.Status == 0) {
                        if (EnableSound == 'True') { document.getElementById('error').play(); }
                        toastr.error(data.Message);
                    }
                    else {
                        if (EnableSound == 'True') { document.getElementById('success').play(); }
                        toastr.success(data.Message);
                        $("#row" + i.split('btn')[1]).remove();
                    }
                    $("#divLoading").hide();
                },
                error: function (xhr) {
                    $("#divLoading").hide();
                }
            });
        }
    }
    else {
        $("#row" + i.split('btn')[1]).remove();
    }
});