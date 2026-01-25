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

    $('#_RegisteredOn_TS').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date() });
    $('#_RegisteredOn_TS').addClass('notranslate');

    if (window.location.href.indexOf('super') != -1) {
        $('.divAdd').show();
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

var _PageIndex = 1; var Accounts;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchFetch',
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

function View(BranchId) {
    var det = {
        BranchId: BranchId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchView',
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
    var PaymentTypes = [];

    $('#divCombo tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            if ($('#chkIsPaymentTypeChecked' + _id).is(':checked')) {
                PaymentTypes.push({
                    PaymentTypeId: _id,
                    AccountId: $('#ddlAccount' + _id).val(),
                    IsPosShown: $('#chkIsPosShown' + _id).is(':checked'),
                    IsDefault: $('#rdbIsDefault' + _id).is(':checked')
                })
            }
        }
    });
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        BranchCode: $('#txtBranchCode').val(),
        Branch: $('#txtBranch').val(),
        ContactName: $('#txtContactName').val(),
        Mobile: $('#txtMobile').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        Email: $('#txtEmail').val(),
        Zipcode: $('#txtZipcode').val(),
        CityId: $('#ddlCity').val(),
        IsActive: true,
        IsDeleted: false,
        Address: $('#txtAddress').val(),
        CurrencyId: $('#ddlCurrency').val(),
        PaymentTypes: $('#ddlPaymentType').val() == null ? [] : $('#ddlPaymentType').val(),
        ContactPersonId: $('#ddlContactPerson').val(),
        TaxId: $('#ddlTax').val(),
        TaxNo: $('#txtTaxNo').val(),
        EnableInlineTax: $('#chkEnableInlineTax').is(':checked'),
        PaymentTypes: PaymentTypes,
        TaxSettingId: $('#ddlTaxSetting').val(),
        PrefixId: $('#ddlPrefix').val(),
        WhatsappNo: $('#txtWhatsappNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchInsert',
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
                    window.location.href = "/locationsettings/Branch";
                }
                else {
                    window.location.href = "/locationsettings/branchadd";
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    var PaymentTypes = [];

    $('#divCombo tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            if ($('#chkIsPaymentTypeChecked' + _id).is(':checked')) {
                PaymentTypes.push({
                    PaymentTypeId: _id,
                    AccountId: $('#ddlAccount' + _id).val(),
                    IsPosShown: $('#chkIsPosShown' + _id).is(':checked'),
                    IsDefault: $('#rdbIsDefault' + _id).is(':checked')
                })
            }
        }
    });

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');   

    var det = {
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        BranchCode: $('#txtBranchCode').val(),
        Branch: $('#txtBranch').val(),
        ContactName: $('#txtContactName').val(),
        Mobile: $('#txtMobile').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        Email: $('#txtEmail').val(),
        Zipcode: $('#txtZipcode').val(),
        CityId: $('#ddlCity').val(),
        BranchId: window.location.href.split('=')[1],
        Address: $('#txtAddress').val(),
        CurrencyId: $('#ddlCurrency').val(),
        PaymentTypes: $('#ddlPaymentType').val() == null ? [] : $('#ddlPaymentType').val(),
        ContactPersonId: $('#ddlContactPerson').val(),
        TaxId: $('#ddlTax').val(),
        TaxNo: $('#txtTaxNo').val(),
        EnableInlineTax: $('#chkEnableInlineTax').is(':checked'),
        PaymentTypes: PaymentTypes,
        TaxSettingId: $('#ddlTaxSetting').val(),
        PrefixId: $('#ddlPrefix').val(),
        WhatsappNo: $('#txtWhatsappNo').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchUpdate',
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
                    window.location.href = "/locationsettings/Branch";
                }
                else {
                    window.location.href = "/locationsettings/branchadd";
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(BranchId, IsActive) {
    var det = {
        BranchId: BranchId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/BranchActiveInactive',
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
                fetchList(_PageIndex);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(BranchId, BranchName) {
    var r = confirm("This will delete \"" + BranchName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            BranchId: BranchId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/locationsettings/Branchdelete',
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

function fetchActiveStates() {
    var det = {
        CountryId: $('#ddlCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_States_Dropdown").html(data);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveCitys() {
    var det = {
        StateId: $('#ddlState').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_Citys_Dropdown").html(data);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertCity() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        CountryId: $('#ddlCountry').val(),
        //StateId: $('#ddlCityModalState').val(),
        StateId: $('#ddlState').val(),
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
                $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                $('#ddlCity').val(data.Data.City.CityId);
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

function fetchCityModalActiveStates() {
    var det = {
        CountryId: $('#ddlCityModalCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/ActiveStatesForCityModal',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var dropdown = '<label>State <span class="danger">*</span></label><select class="form-control select2" id="ddlCityModalState">';
            $.each(data.Data.States, function (index, value) {
                dropdown = dropdown + '<option value="' + value.StateId + '">' + value.State + '</option>';
            });

            dropdown = dropdown + '</select>';
            $('#p_CityModalStates_Dropdown').html('');
            $('#p_CityModalStates_Dropdown').append(dropdown);
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

document.getElementById("txtZipcode").addEventListener("keypress", function (evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
    }
});

function insertCurrency() {
    var det = {
        CurrencyName: $('#txtCurrencyName').val(),
        CurrencySymbol: $('#txtCurrencySymbol').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/currencyInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                $('#ddlCurrency').append($('<option>', { value: data.Data.Currency.CurrencyId, text: data.Data.Currency.CurrencyName }));
                $('#ddlCurrency').val(data.Data.Currency.CurrencyId);
                $('#currencyModal').modal('toggle');
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertPaymentType() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        PaymentType: $('#txtPaymentType').val(),
        IsActive: true,
        IsDeleted: false,
        IsPosShown: $('#chkIsPosShown').is(':checked')
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/paymentTypeInsert',
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
                //$('#ddlPaymentType').append($('<option>', { value: data.Data.PaymentType.PaymentTypeId, text: data.Data.PaymentType.PaymentType }));
                //var d = $('#ddlPaymentType').val();
                //if (d == null) d = [];
                //d.push(data.Data.PaymentType.PaymentTypeId);
                //$('#ddlPaymentType').val(d);

                //var ddlAccount = '<select class="form-control select2" id="ddlAccount' + data.Data.PaymentType.PaymentTypeId + '"><option value="0">Select</option>';
                //for (let ss = 0; ss < Accounts.length; ss++) {
                //    ddlAccount = ddlAccount + '<option value="' + Accounts[ss].AccountId + '">' + Accounts[ss].AccountName + '</option>';
                //}
                //ddlAccount = ddlAccount + '</select>';

                //var AccountPermission = $('#hdnAccountPermission').val().toLocaleLowerCase();
                var PosPermission = $('#hdnPosPermission').val().toLocaleLowerCase();

                var html = '<tr id="' + data.Data.PaymentType.PaymentTypeId + '">' +
                    '<td>' +
                    '<div class="form-group">' +
                    '<div class="checkbox icheck-turquoise">' +
                    '<input class="chkSelectAll" checked type="checkbox" id="chkIsPaymentTypeChecked' + data.Data.PaymentType.PaymentTypeId + '" />' +
                    '<label for="chkIsPaymentTypeChecked' + data.Data.PaymentType.PaymentTypeId + '" class="mt-2"></label>' +
                    '</div>' +
                    '</div>' +
                    '</td >' +
                    '<td>' + data.Data.PaymentType.PaymentType + '</td>' +
                    //'<td class="' + (AccountPermission == 'true' ? '' : 'hidden') + '">' +
                    //'<div class="form-group">' +
                    //'<div class="input-group">' +
                    //ddlAccount +
                    //'</div>' +
                    //'</div>' +
                    //'</td>' +
                    '<td class="' + (PosPermission == 'true' ? '' : 'hidden') + '">' +
                    '<div class="form-group">' +
                    '<div class="checkbox icheck-turquoise">' +
                    '<input class="chkIsPosSeen" type="checkbox" id="chkIsPosShown' + data.Data.PaymentType.PaymentTypeId + '" />' +
                    '<label for="chkIsPosShown' + data.Data.PaymentType.PaymentTypeId + '" class="mt-2"></label>' +
                    '</div>' +
                    '</div>' +
                    '</td>' +
                    '</tr >';

                $('#divCombo').append(html);
                $('#paymentTypeModal').modal('toggle');

                $('#txtPaymentType').val('');
                $('#chkIsPosShown').prop('checked', false);
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function openCityModal() {
    if ($('#ddlState').val() == 0) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please select State first');
    }
    else {
        $('#cityModal').modal('toggle');
    }
}

function selectedSelectAll(id) {
    if ($('#' + id).is(':checked')) {
        $('.' + id).prop('checked', true);
    } else {
        $('.' + id).prop('checked', false);
    }
}

function insertTaxSetting(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    
    var det = {
        BusinessRegistrationNameId: $('#ddlBusinessRegistrationName_TS').val(),
        BusinessRegistrationNo: $('#txtBusinessRegistrationNo_TS').val(),
        BusinessLegalName: $('#txtBusinessLegalName_TS').val(),
        BusinessTradeName: $('#txtBusinessTradeName_TS').val(),
        RegisteredOn: moment($("#txtRegisteredOn_TS").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        IsCompositionScheme: $('#chkIsCompositionScheme_TS').is(':checked') ? 1 : 2,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/taxsettings/TaxSettingInsert',
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
                    $('#' + res.Id+'_TS').show();
                    $('#' + res.Id + '_TS').text(res.Message);

                    var ctrl = $('.' + res.Id + '_TS_ctrl select').prop('tagName');
                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_TS_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_TS_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('#' + res.Id + '_TS_ctrl').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_TS_ctrl').css('border', '2px solid #dc3545');
                    }
                });
            }
            else {
                $('#ddlTaxSetting').append($('<option>', { value: data.Data.TaxSetting.TaxSettingId, text: data.Data.TaxSetting.BusinessRegistrationNo }));
                $('#ddlTaxSetting').val(data.Data.TaxSetting.TaxSettingId);
                $('#taxSettingModal').modal('toggle');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setBusinessRegistrationName() {
    $('.lblBusinessRegistrationName').html($('#ddlBusinessRegistrationName option:selected').text() + ' <span class="danger">*</span>');
}

function insertPrefix() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var PrefixUserMaps = [];
    $('.divPrefix').each(function () {
        var _innerid = this.id;
        PrefixUserMaps.push({
            PrefixMasterId: $('#hdnPrefixMasterId' + _innerid).val(),
            PrefixUserMapId: $('#hdnPrefixUserMapId' + _innerid).val(),
            NoOfDigits: $('#txtNoOfDigits' + _innerid).val(),
            Prefix: $('#txtPrefix' + _innerid).val(),
            Id: _innerid
        })
    });

    var det = {
        PrefixName: $('#txtPrefixName').val(),
        PrefixUserMaps: PrefixUserMaps,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/PrefixInsert',
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
                $('#ddlPrefix').append($('<option>', { value: data.Data.Prefix.PrefixId, text: data.Data.Prefix.PrefixName }));
                $('#ddlPrefix').val(data.Data.Prefix.PrefixId);
                $('#prefixModal').modal('toggle');
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}