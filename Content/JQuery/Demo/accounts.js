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
        Type: $('#ddlType').val(),
        AccountName: $('#txtAccountName').val(),
        AccountNumber: $('#txtAccountNumber').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountFetch',
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

function insert(i) {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var AccountDetails = [];
    $('#addrow .divAccountDetails').each(function () {
        var id = this.id.split('row')[1];
        AccountDetails.push({ Label: $('#txtLabel' + this.id.split('rowNew')[1]).val(), Value: $('#txtValue' + this.id.split('rowNew')[1]).val() });
    });

    var det = {
        AccountName: $('#txtAccountName').val(),
        AccountNumber: $('#txtAccountNumber').val(),
        Type: $('#ddlType').val(),
        //AccountTypeId: $('#ddlAccountType').val(),
        //AccountSubTypeId: $('#ddlAccountSubType').val(),
        CurrencyId: $('#ddlUserCurrency').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        Notes: $('#txtNotes').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName').val(),
        ParentId: 0,
        DisplayAs: $('#ddlAccount').val() == 0 ? $('#txtAccountName').val() : $("#ddlAccount option:selected").text() + ' -> ' + $('#txtAccountName').val(),
        //BranchName: $('#txtBranchName').val(),
        //BranchCode: $('#txtBranchCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountInsert',
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
                });
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/accounts/index";
                }
                else {
                    window.location.href = "/accounts/Accountadd";
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
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

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
        Type: $('#ddlType').val(),
        //AccountTypeId: $('#ddlAccountType').val(),
        //AccountSubTypeId: $('#ddlAccountSubType').val(),
        CurrencyId: $('#ddlUserCurrency').val(),
        OpeningBalance: $('#txtOpeningBalance').val(),
        Notes: $('#txtNotes').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName').val(),
        ParentId: 0,
        DisplayAs: $('#ddlAccount').val() == 0 ? $('#txtAccountName').val() : $("#ddlAccount option:selected").text() + ' -> ' + $('#txtAccountName').val(),
        //BranchName: $('#txtBranchName').val(),
        //BranchCode: $('#txtBranchCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountUpdate',
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
                });
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/accounts/index";
                }
                else {
                    window.location.href = "/accounts/Accountadd";
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
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
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

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Account.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Account.csv",
        sheet: {
            name: "Sheet1"
        }
    });
    fetchList();
}

function exportToPdf() {
    $('.hidden').show();
    $('.hide').hide();
    $('.responsive-table').css('height', '100%');
    html2canvas($('#tblData')[0], {
        onrendered: function (canvas) {
            var data = canvas.toDataURL();
            var docDefinition = {
                content: [{
                    image: data,
                    width: 500
                }]
            };

            pdfMake.createPdf(docDefinition).download("Account.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function insertAccountType() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        AccountTypeCode: $('#txtAccountTypeCode').val(),
        AccountType: $('#txtAccountType').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountTypeInsert',
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
                });
            }
            else {
                $('#ddlAccountType').append($('<option>', { value: data.Data.AccountType.AccountTypeId, text: data.Data.AccountType.AccountType }));
                $('#ddlAccountType').val(data.Data.AccountType.AccountTypeId);
                $('#AccountTypeModal').modal('toggle');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openAccountSubTypeModal() {
    if ($('#ddlAccountType').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select Account Type first');
    }
    else {
        $('#AccountSubTypeModal').modal('toggle');
    }
}

function insertAccountSubType() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        AccountTypeId: $('#ddlAccountType').val(),
        //AccountTypeCode: $('#txtAccountTypeCode').val(),
        AccountSubType: $('#txtAccountSubType').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountSubTypeInsert',
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
                });
            }
            else {
                $('#ddlAccountSubType').append($('<option>', { value: data.Data.AccountSubType.AccountSubTypeId, text: data.Data.AccountSubType.AccountSubType }));
                $('#ddlAccountSubType').val(data.Data.AccountSubType.AccountSubTypeId);
                $('#AccountSubTypeModal').modal('toggle');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertAccount(i) {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var AccountDetails = [];
    $('#addrow1 .divAccountDetails').each(function () {
        var id = this.id.split('row')[1];
        AccountDetails.push({ Label: $('#txtLabel' + this.id.split('rowNew')[1]).val(), Value: $('#txtValue' + this.id.split('rowNew')[1]).val() });
    });

    var det = {
        AccountName: $('#txtAccountName_N').val(),
        AccountNumber: $('#txtAccountNumber_N').val(),
        AccountTypeId: $('#ddlAccountType_N').val(),
        AccountSubTypeId: $('#ddlAccountSubType_N').val(),
        OpeningBalance: $('#txtAccountOpeningBalance').val(),
        Notes: $('#txtNotes').val(),
        AccountDetails: AccountDetails,
        IsActive: true,
        IsDeleted: false,
        BankName: $('#txtBankName').val(),
        BranchName: $('#txtBranchName').val(),
        BranchCode: $('#txtBranchCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountInsert',
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
                });
            }
            else {
                $('#ddlFTAccount').append($('<option>', { value: data.Data.Account.AccountId, text: data.Data.Account.AccountName + '-' + data.Data.Account.AccountNumber }));
                $('#ddlFTAccount').val(data.Data.Account.AccountId);

                $('#ddlDAccount').append($('<option>', { value: data.Data.Account.AccountId, text: data.Data.Account.AccountName + '-' + data.Data.Account.AccountNumber }));
                $('#ddlDAccount').val(data.Data.Account.AccountId);

                $('#AccountModal').modal('toggle');

                $('#txtAccountName_N').val('');
                $('#txtAccountNumber_N').val('');
                $('#ddlAccountType_N').val(0);
                $("#ddlAccountSubType_N").html('');
                $('#txtAccountOpeningBalance').val('');
                $('#txtNotes').val('');
                $('#txtBankName').val('');
                $('#txtBranchName').val('');
                $('#txtBranchCode').val('');
                $("#addrow1").empty();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveAccountSubTypes_N() {
    var det = {
        AccountTypeId: $('#ddlAccountType_N').val()
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

            $("#ddlAccountSubType_N").html('');
            //$("#ddlQuaternaryUnit").append($("<option></option>").val(0).html('Select'));
            $.each(data.Data.AccountSubTypes, function (i, value) {
                $("#ddlAccountSubType_N").append($("<option></option>").val(value.AccountSubTypeId).html(value.AccountSubType));
            });

            //$("#p_SubCategories_Dropdown").html(data);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertAccountType_N() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        AccountTypeCode: $('#txtAccountTypeCode').val(),
        AccountType: $('#txtAccountType').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountTypeInsert',
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
                });
            }
            else {
                $('#ddlAccountType_N').append($('<option>', { value: data.Data.AccountType.AccountTypeId, text: data.Data.AccountType.AccountType }));
                $('#ddlAccountType_N').val(data.Data.AccountType.AccountTypeId);
                $('#AccountTypeModal').modal('toggle');

                $('#txtAccountType').val('');
                $("#ddlAccountSubType_N").html('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertAccountSubType_N() {
    $('.errorText').hide();
    $('.form-control').css('border', '1px solid #ced4da');
    $('.select2-container .select2-selection').css('border', '1px solid #ced4da');

    var det = {
        AccountTypeId: $('#ddlAccountType_N').val(),
        //AccountTypeCode: $('#txtAccountTypeCode').val(),
        AccountSubType: $('#txtAccountSubType').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/accounts/AccountSubTypeInsert',
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
                });
            }
            else {
                $('#ddlAccountSubType_N').append($('<option>', { value: data.Data.AccountSubType.AccountSubTypeId, text: data.Data.AccountSubType.AccountSubType }));
                $('#ddlAccountSubType_N').val(data.Data.AccountSubType.AccountSubTypeId);
                $('#AccountSubTypeModal').modal('toggle');

                $('#txtAccountSubType').val('');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleAccountType() {
    if ($('#ddlType').val() == 'Bank') {
        $('#lblAccountNumber').text('Account Number');
    }
    else {
        $('#lblAccountNumber').text('Card Number');
    }
}
