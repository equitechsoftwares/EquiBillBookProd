$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: false,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_DOB').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });

    $('#_JoiningDate').datetimepicker({
        format: DateFormat.toUpperCase(), defaultDate: new Date()
    });

    $('#_DOB').addClass('notranslate');
    $('#_JoiningDate').addClass('notranslate');

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
    interval = setInterval(playSound, 100);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1, file = "", FileExtensionProfilePic = "", UserId = 0;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlSupplier').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        Name: $('#txtName').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UserFetch',
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

function View(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UserView',
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
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var Addresses = [];
    Addresses.push({
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
    });
    Addresses.push({
        Address: $('#txtAltAddress').val(),
        CountryId: $('#ddlAltCountry').val(),
        StateId: $('#ddlAltState').val(),
        CityId: $('#ddlAltCity').val(),
        Zipcode: $('#txtAltZipcode').val(),
    });

    var det = {
        Username: $('#txtUsername').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        ReligionId: $('#ddlReligion').val(),
        UserRoleId: $('#ddlUserRole').val(),
        DOB: moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        JoiningDate: moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        Gender: $('#ddlGender').val(),
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
        UserType: 'User',
        ProfilePic: file,
        FileExtensionProfilePic: FileExtensionProfilePic,
        IsActive: true,
        IsDeleted: false,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        Addresses: Addresses
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UserInsert',
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
                    window.location.href = "/usersettings/users";
                }
                else {
                    window.location.href = "/usersettings/useradd";
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
        Address: $('#txtAddress').val(),
        CountryId: $('#ddlCountry').val(),
        StateId: $('#ddlState').val(),
        CityId: $('#ddlCity').val(),
        Zipcode: $('#txtZipcode').val(),
    });
    Addresses.push({
        AddressId: $('#txtAltAddressId').val(),
        Address: $('#txtAltAddress').val(),
        CountryId: $('#ddlAltCountry').val(),
        StateId: $('#ddlAltState').val(),
        CityId: $('#ddlAltCity').val(),
        Zipcode: $('#txtAltZipcode').val(),
    });
    var det = {
        UserId: window.location.href.split('=')[1],
        Username: $('#txtUsername').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        ReligionId: $('#ddlReligion').val(),
        UserRoleId: $('#ddlUserRole').val(),
        DOB: moment($("#txtDOB").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        JoiningDate: moment($("#txtJoiningDate").val(), DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        Gender: $('#ddlGender').val(),
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
        ProfilePic: file,
        FileExtensionProfilePic: FileExtensionProfilePic,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
        UserType: 'User',
        Addresses: Addresses
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UserUpdate',
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
                    window.location.href = "/usersettings/users";
                }
                else {
                    window.location.href = "/usersettings/useradd";
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
        url: '/usersettings/UserActiveInactive',
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
            url: '/usersettings/Userdelete',
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
        url: '/usersettings/ActiveStates',
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
    if (type == '' || type == undefined) {
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
        url: '/usersettings/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            if (type == '' || type == undefined) {
                $("#p_Citys_Dropdown").html(data);
            }
            else {
                $("#p_Alt_Citys_Dropdown").html(data);
            }
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getProfileBase64() {
    var file1 = $("#ProfilePic").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#ProfilePic").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            file = reader.result;
            FileExtensionProfilePic = '.' + file1.name.split('.').pop();

            $('#blah').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }

}

function insertRole() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        RoleCode: $('#txtRoleCode').val(),
        RoleName: $('#txtRoleName').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/userroleInsert',
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
                $('#ddlUserRole').append($('<option>', { value: data.Data.Role.RoleId, text: data.Data.Role.RoleName }));
                $('#ddlUserRole').val(data.Data.Role.RoleId);
                $('#roleModal').modal('toggle');

                $('#txtRoleCode').val('');
                $('#txtRoleName').val('');
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openCountryModal(id) {
    _id = id;
    $('#countryModal').modal('toggle');
}

function openStateModal(id) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    _id = id;
    if (_id == 'ddlState') {
        if ($('#ddlCountry').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select Country first');
            return
        }
    }
    else {
        if ($('#ddlAltCountry').val() == 0) {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Please select Country first');
            return
        }
    }
    $('#stateModal').modal('toggle');
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

function insertCountry() {
    var det = {
        CountryCode: $('#txtCountryCode').val(),
        Country: $('#txtCountry').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/countryInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                $('#ddlCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#ddlAltCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#' + _id).val(data.Data.Country.CountryId);
                $('#countryModal').modal('toggle');

                $('#ddlNewCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#ddlCityModalCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));

                $('#txtCountryCode').val('');
                $('#txtCountry').val('');
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertState() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var det = {
        CountryId: _id == 'ddlState' ? $('#ddlCountry').val() : $('#ddlAltCountry').val(),
        StateCode: $('#txtStateCode').val(),
        State: $('#txtState').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/locationsettings/StateInsert',
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
                $('#ddlState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#ddlAltState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#' + _id).val(data.Data.State.StateId);
                $('#stateModal').modal('toggle');

                $('#ddlCityModalState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));

                $('#ddlNewCountry').val($("#ddlNewCountry option:first").val());
                $('#txtStateCode').val('');
                $('#txtState').val('');
                $('.select2').select2();

                if (_id == "ddlState") {
                    $('#ddlCity').html('');
                } else {
                    $('#ddlAltCity').html('');
                }
            }
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
        CountryId: _id == 'ddlCity' ? $('#ddlCountry').val() : $('#ddlAltCountry').val(),
        /*StateId: $('#ddlCityModalState').val(),*/
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

                $('.select2').select2();
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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function openChangePasswordModal(id) {
    UserId = id;

    $('#changePasswordModal').modal('toggle');
}

function UpdatePassword() {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');

    var det = {
        UserId: UserId,
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UpdateUserPassword',
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
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);

                $('#changePasswordModal').modal('toggle');
            }
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

document.getElementById("txtAltZipcode").addEventListener("keypress", function (evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
    }
});

