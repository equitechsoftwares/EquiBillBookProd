$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

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
        url: '/banking/AccountFetch',
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
        url: '/Banking/AccountView',
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
        DisplayAs: $('#txtAccountName').val(),
        //BranchName: $('#txtBranchName').val(),
        //BranchCode: $('#txtBranchCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/AccountInsert',
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
                    window.location.href = "/banking/index";
                }
                else {
                    window.location.href = "/banking/Accountadd";
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
        DisplayAs: $('#txtAccountName').val(),
        //BranchName: $('#txtBranchName').val(),
        //BranchCode: $('#txtBranchCode').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/AccountUpdate',
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
                    window.location.href = "/banking/index";
                }
                else {
                    window.location.href = "/banking/Accountadd";
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
        url: '/banking/AccountActiveInactive',
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
            url: '/banking/Accountdelete',
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

function openFundTransferModal(id, AccountName) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#fundTransferModal').modal('toggle');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_FTPaymentDate').datetimepicker('destroy'); // Destroy existing instance
    $('#_FTPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_FTPaymentDate').addClass('notranslate');

    $('#txtFTTransferTo').text(AccountName);
    $('.select2').select2();

    AccountId = id;
    //var det = {
    //    AccountId: id
    //};
    //$("#divLoading").show();
    //$.ajax({
    //    url: '/banking/ActiveOtherAccounts',
    //    datatype: "json",
    //    data: det,
    //    type: "post",
    //    success: function (data) {
    //        $("#divLoading").hide();
    //        var dropdown = '<label>Transfer To <span class="danger">*</span></label><div class="input-group">' +
    //            '<select class="form-control select2" id="ddlFTAccount">' +
    //            '<option value="0">Select</option>';
    //        $.each(data.Data.Accounts, function (index, value) {
    //            dropdown = dropdown + '<option value="' + value.AccountId + '">' + value.AccountName + '</option>';
    //        });

    //        dropdown = dropdown + '</select><span class="input-group-append"><a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#AccountModal"> + </a></span><small class="text-red font-weight-bold errorText" id="divAccount_FT"></small></div>';
    //        $('#divFTAccount').html('');
    //        $('#divFTAccount').append(dropdown);
    //        $('#fundTransferModal').modal('toggle');

    //        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    //        var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    //        $('#_FTPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    //        $('#_FTPaymentDate').addClass('notranslate');

    //        $('#txtFTTransferTo').text(AccountName);
    //    },
    //    error: function (xhr) {
    //        $("#divLoading").hide();
    //    }
    //});
};

function openDepositModal(id, AccountName) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#depositModal').modal('toggle');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_DPaymentDate').datetimepicker('destroy'); // Destroy existing instance
    $('#_DPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_DPaymentDate').addClass('notranslate');

    $('#txtDDepositTo').text(AccountName);
    $('.select2').select2();

    AccountId = id;
    //var det = {
    //    AccountId: id
    //};
    //$("#divLoading").show();
    //$.ajax({
    //    url: '/banking/ActiveOtherAccounts',
    //    datatype: "json",
    //    data: det,
    //    type: "post",
    //    success: function (data) {
    //        $("#divLoading").hide();
    //        var dropdown = '<label>Deposit From</label><div class="input-group">' +
    //            '<select class="form-control select2" id = "ddlDAccount">' +
    //            '<option value="0">Select </option>';

    //        $.each(data.Data.Accounts, function (index, value) {
    //            dropdown = dropdown + '<option value="' + value.AccountId + '">' + value.AccountName + '</option>';
    //        });

    //        dropdown = dropdown + '</select><span class="input-group-append"><a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#AccountModal"> + </a></span><small class="text-red font-weight-bold errorText" id="divAccount_D"></small></div>';
    //        $('#divDAccount').html('');
    //        $('#divDAccount').append(dropdown);

    //        $('#depositModal').modal('toggle');

    //        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    //        var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    //        $('#_DPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    //        $('#_DPaymentDate').addClass('notranslate');

    //        $('#txtDDepositTo').text(AccountName);
    //    },
    //    error: function (xhr) {
    //        $("#divLoading").hide();
    //    }
    //});
};

function openWithdrawModal(id, AccountName) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    $('#withdrawModal').modal('toggle');

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_WPaymentDate').datetimepicker('destroy'); // Destroy existing instance
    $('#_WPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_WPaymentDate').addClass('notranslate');

    $('#txtWWithdrawFrom').text(AccountName);
    $('.select2').select2();

    AccountId = id;
    //var det = {
    //    AccountId: id
    //};
    //$("#divLoading").show();
    //$.ajax({
    //    url: '/banking/ActiveOtherAccounts',
    //    datatype: "json",
    //    data: det,
    //    type: "post",
    //    success: function (data) {
    //        $("#divLoading").hide();
    //        var dropdown = '<label>To <span class="danger">*</span></label><div class="input-group">' +
    //            '<select class="form-control select2" id="ddlFTAccount">' +
    //            '<option value="0">Select</option>';
    //        $.each(data.Data.Accounts, function (index, value) {
    //            dropdown = dropdown + '<option value="' + value.AccountId + '">' + value.AccountName + '</option>';
    //        });

    //        dropdown = dropdown + '</select><span class="input-group-append"><a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#AccountModal"> + </a></span><small class="text-red font-weight-bold errorText" id="divAccount_FT"></small></div>';
    //        $('#divWAccount').html('');
    //        $('#divWAccount').append(dropdown);
    //        $('#withdrawModal').modal('toggle');

    //        var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    //        var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    //        $('#_WPaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    //        $('#_WPaymentDate').addClass('notranslate');

    //        $('#txtWWithdrawFrom').text(AccountName);
    //    },
    //    error: function (xhr) {
    //        $("#divLoading").hide();
    //    }
    //});
};

function getFTBase64() {
    var file1 = $("#FTAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {

        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#FTAttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            PaymentAttachDocument = reader.result;
            PaymentFileExtensionAttachDocument = '.' + file1.name.split('.').pop();
            $('#blahFTAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function FundTransfer() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        FromAccountId: AccountId,
        IsActive: true,
        IsDeleted: false,
        ToAccountId: $('#ddlFTAccount').val(),
        Notes: $('#txtFTNotes').val(),
        Amount: $('#txtFTAmount').val(),
        PaymentDate: moment($("#txtFTPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtFTPaymentDate').val(),
        PaymentTypeId: $('#ddlFTPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Fund Transfer",
        ReferenceNo: $('#txtFTReferenceNo').val(),
        BranchId: $('#ddlFTBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraInsert',
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
                    $('#' + res.Id + '_FT').show();
                    $('#' + res.Id + '_FT').text(res.Message);

                    var ctrl = $('.' + res.Id + '_FT_ctrl select').prop('tagName');
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_FT_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_FT_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_FT_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_FT_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();

                $("#fundTransferModal").modal('hide');

                $("#ddlFTAccount").prop("selectedIndex", 0).val(); 
                $('#txtFTNotes').val('');
                $('#txtFTAmount').val('');
                $('#txtFTPaymentDate').val('');
                $("#ddlFTPaymentType").prop("selectedIndex", 0).val(); 
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('.select2').select2();
            }
            /* $("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getDBase64() {
    var file1 = $("#DAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {

        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#DAttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            PaymentAttachDocument = reader.result;
            PaymentFileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahDttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function Deposit() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        FromAccountId: $('#ddlDAccount').val(),
        IsActive: true,
        IsDeleted: false,
        ToAccountId: AccountId,
        Notes: $('#txtDNotes').val(),
        Amount: $('#txtDAmount').val(),
        PaymentDate: moment($("#txtDPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDPaymentDate').val(),
        PaymentTypeId: $('#ddlDPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Deposit",
        ReferenceNo: $('#txtDReferenceNo').val(),
        BranchId: $('#ddlDBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraInsert',
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
                    $('#' + res.Id + '_D').show();
                    $('#' + res.Id + '_D').text(res.Message);

                    var ctrl = $('.' + res.Id + '_D_ctrl select').prop('tagName');
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_D_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_D_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_D_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_D_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();

                $("#depositModal").modal('hide');

                $("#ddlDAccount").prop("selectedIndex", 0).val(); 
                $('#txtDNotes').val('');
                $('#txtDAmount').val('');
                $('#txtDPaymentDate').val('');
                $("#ddlDPaymentType").prop("selectedIndex", 0).val(); 
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('.select2').select2();
            }
            /*  $("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function getWBase64() {
    var file1 = $("#WAttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {

        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#FTAttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            PaymentAttachDocument = reader.result;
            PaymentFileExtensionAttachDocument = '.' + file1.name.split('.').pop();
            $('#blahWAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}

function Withdraw() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        FromAccountId: AccountId,
        IsActive: true,
        IsDeleted: false,
        ToAccountId: $('#ddlWAccount').val(),
        Notes: $('#txtWNotes').val(),
        Amount: $('#txtWAmount').val(),
        PaymentDate: moment($("#txtWPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtDPaymentDate').val(),
        PaymentTypeId: $('#ddlWPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        Type: "Withdraw",
        ReferenceNo: $('#txtWReferenceNo').val(),
        BranchId: $('#ddlWBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraInsert',
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
                    $('#' + res.Id + '_W').show();
                    $('#' + res.Id + '_W').text(res.Message);

                    var ctrl = $('.' + res.Id + '_W_ctrl select').prop('tagName');
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_W_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_W_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_W_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_W_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();

                $("#withdrawModal").modal('hide');

                $("#ddlWAccount").prop("selectedIndex", 0).val(); 
                $('#txtWNotes').val('');
                $('#txtWAmount').val('');
                $('#txtWPaymentDate').val('');
                $("#ddlWPaymentType").prop("selectedIndex", 0).val(); 
                PaymentAttachDocument = '';
                PaymentFileExtensionAttachDocument = '';
                $('#blahPaymentAttachDocument').prop('src', '');
                $('.select2').select2();
            }
            /* $("#divLoading").hide();*/
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function toggleAccountType() {
    if ($('#ddlType').val() == 'Bank') {
        $('#lblAccountNumber').text('Account Number');
    }
    else {
        $('#lblAccountNumber').text('Card Number');
    }
}