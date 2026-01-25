
$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_JoiningDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase(), defaultDate: new Date() });
    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');

    fetchCompanyCurrency();
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

var _PageIndex = 1; var _id = ''; var _excelJson = []; var isExcelUpload = false, _SupplierId = 0, c = 1;
var AttachDocument = "";
var FileExtensionAttachDocument = "";
var PaymentAttachDocument = "";
var PaymentFileExtensionAttachDocument = "";
var _Type = "";
var myInterval;
var BatchNo = Math.floor(Math.random() * 26) + Date.now();
var TotalCount = 0;
var _excelJson = [];
var UploadPath = "", fileExtension = "";
var isExcelUpload = false;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlSupplier').val(),
        //FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        Name: $('#txtName').val(),
        BusinessName: $('#txtBusinessName').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/UserFetch',
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

            $("[data-toggle=popover]").popover({
                html: true,
                trigger: "hover",
                placement: 'auto',
            });
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

function View(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/SupplierView',
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

function ViewPayment(UserPaymentId) {
    var det = {
        UserPaymentId: UserPaymentId,
        Type: "Supplier Payment",
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/PaymentView',
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
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
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
                    window.location.href = "/supplier/index";
                }
                else {
                    window.location.href = "/supplier/add";
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
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var Addresses = [];
    Addresses.push({
        AddressId: $('#txtAddressId').val(),
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
    if ($('#chkIsShippingAddressDifferent').is(':checked')) {
        Addresses.push({
            AddressId: $('#txtAltAddressId').val(),
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
    }
    else {
        Addresses.push({
            AddressId: $('#txtAltAddressId').val(),
            Name: '',
            MobileNo: '',
            MobileNo2: '',
            Landmark: '',
            Address: '',
            CountryId: 0,
            StateId: 0,
            CityId: 0,
            Zipcode: '',
        });
    }

    var det = {
        UserId: window.location.href.split('=')[1],
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
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        TaxId: $('#ddlTax').val(),
        Addresses: Addresses,
        CurrencyId: $('#ddlUserCurrency').val(),
        TaxPreferenceId: $('#ddlTaxPreference').val(),
        TaxExemptionId: $('#ddlTaxExemption').val(),
        SourceOfSupplyId: $('#ddlSourceOfSupply').val(),
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
        url: '/supplier/UserUpdate',
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
                    window.location.href = "/supplier/index";
                }
                else {
                    window.location.href = "/supplier/add";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(UserId, IsActive) {
    var det = {
        UserId: UserId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/UserActiveInactive',
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

function Delete(UserId, UserName) {
    var r = confirm("This will delete \"" + UserName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            UserId: UserId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/supplier/Userdelete',
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
        url: '/supplier/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
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
        url: '/supplier/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
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

function fetchCityModalActiveStates() {
    var det = {
        CountryId: $('#ddlCityModalCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/master/ActiveStatesForCityModal',
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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function copyTag(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

function uploadExcel() {
    //var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xlsx|.xls)$/;
    var regex = (/\.(xlsx|xls|xlsm|csv)$/i);
    /*Checks whether the file is a valid excel or csv file*/
    if (regex.test($("#excelfile").val().toLowerCase())) {
        var xlsxflag = false; /*Flag for checking whether excel is .xls format or .xlsx format*/
        var csvflag = false; /*Flag for checking whether file is .csv format*/
        if ($("#excelfile").val().toLowerCase().indexOf(".xlsx") > 0) {
            xlsxflag = true;
        } else if ($("#excelfile").val().toLowerCase().indexOf(".csv") > 0) {
            csvflag = true;
        }
        /*Checks whether the browser supports HTML5*/
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;

                if (csvflag) {
                    /*Parse CSV file using PapaParse*/
                    Papa.parse(data, {
                        header: true,
                        skipEmptyLines: true,
                        complete: function (results) {
                            if (results.errors.length > 0) {
                                console.error("CSV parsing errors:", results.errors);
                            }
                            _excelJson = results.data;
                            isExcelUpload = true;
                            $('#exceltable').show();
                        },
                        error: function (error) {
                            if (EnableSound == 'True') { document.getElementById('error').play(); }
                            toastr.error('Error parsing CSV file: ' + error.message);
                        }
                    });
                } else {
                    /*Converts the excel data in to object*/
                    if (xlsxflag) {
                        // Use newer XLSX library (0.18.5) with ArrayBuffer
                        var workbook = XLSX.read(data, { type: 'array' });
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
                            var exceljson = XLSX.utils.sheet_to_json(workbook.Sheets[y], {raw: false});
                        }
                        else {
                            var exceljson = XLS.utils.sheet_to_row_object_array(workbook.Sheets[y]);
                        }
                        if (exceljson.length > 0 && cnt == 0) {
                            _excelJson = exceljson;
                            cnt++;
                        }
                    });
                    isExcelUpload = true;
                    $('#exceltable').show();
                }
            }
            if (csvflag) {/*If file is .csv extension than read as text*/
                reader.readAsText($("#excelfile")[0].files[0]);
            }
            else if (xlsxflag) {/*If excel file is .xlsx extension than creates a Array Buffer from excel*/
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
        toastr.error('Please upload a valid Excel (.xlsx, .xls, .xlsm) or CSV (.csv) file!');
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

    $("#divLoading").show();
    $("#divProgressBar").show();

    myInterval = setInterval(fetchBulkInsertProgress, 10000);

    var det = {
        BatchNo: BatchNo,
        UserImports: _excelJson
    }
    $.ajax({
        url: '/supplier/ImportSupplier',
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
                    window.location.href = "/supplier/index";
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
        url: '/Customers/UserCountByBatch',
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

function openPaymentModal(type, SupplierId, title) {
    _SupplierId = SupplierId;
    _Type = title;
    var det = {
        SupplierId: SupplierId,
        Type: title,
        Title: title
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/DueSummary',
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
                var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a');
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

            $('.select2').select2();

            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });

}

function insertPayment() {
    var url = "";
    if (_Type == 'Supplier Payment') {
        url = "/supplier/PaymentInsert";
    }
    else {
        url = "/supplier/RefundInsert";
    }
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        SupplierId: _SupplierId,
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtPaymentNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: $('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: PaymentAttachDocument,
        FileExtensionAttachDocument: PaymentFileExtensionAttachDocument,
        AccountId: $('#ddlLAccount').val(),
        SmsSettingsId: $('#ddlSmsSettings').val(),
        EmailSettingsId: $('#ddlEmailSettings').val(),
        WhatsappSettingsId: $('#ddlWhatsappSettings').val(),
        ReferenceNo: $('#txtMReferenceNo').val(),
        Type: _Type
    };
    $("#divLoading").show();
    $.ajax({
        url: url,//'/supplier/paymentInsert',
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
                $("#paymentModal").modal('hide');

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

function deletePayment(UserPaymentId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            UserPaymentId: UserPaymentId,
            Type: _Type
        }
        $("#divLoading").show();
        $.ajax({
            url: '/supplier/paymentDelete',
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
                    $('#tr_' + UserPaymentId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
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

document.getElementById("txtZipcode").addEventListener("keypress", function (evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
    }
});

document.getElementById("txtAltZipcode").addEventListener("keypress", function (evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
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

function closeErrorMsg() {
    $('#divErrorMsg').hide();
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

function FetchOtherSoftwareImport(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/OtherSoftwareImportFetch',
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
            url: '/supplier/OtherSoftwareImportDelete',
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
        url: '/supplier/OtherSoftwareImportInsert',
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

function ViewUnusedAdvanceBalance(_SupplierId) {
    var det = {
        SupplierId: _SupplierId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Supplier/UnusedAdvanceBalance',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divUnusedAdvanceBalance").html(data);
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

            $('#UnusedAdvanceBalanceViewModal').modal('toggle');
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });

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

function toggleGstTreatment() {
    if ($('#ddlGstTreatment').val() == "Taxable Supply (Registered)"
        || $('#ddlGstTreatment').val() == "Composition Taxable Supply" ||
        $('#ddlGstTreatment').val() == "Supply to SEZ Unit (Zero-Rated Supply)" || $('#ddlGstTreatment').val() == "Deemed Export"
        || $('#ddlGstTreatment').val() == "Supply by SEZ Developer" || $('#ddlGstTreatment').val() == "Tax Deductor") {
        $('.divGst').show();
        $('.divSourceOfSupply').show();
        //$('.divTaxPreference').show();
    }
    else if ($('#ddlGstTreatment').val() == "Export of Goods / Services (Zero-Rated Supply)") {
        $('.divGst').show();
        $('.divSourceOfSupply').hide();
        //$('.divTaxPreference').hide();
    }
    else {
        $('.divGst').hide();
        $('.divSourceOfSupply').show();
        //$('.divTaxPreference').show();
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

function openUnpaidInvoicesModal(SupplierId, SupplierPaymentId) {
    $('#UnusedAdvanceBalanceViewModal').modal('toggle');
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