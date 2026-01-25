$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: false,
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
    if (EnableSound == 'True') {document.getElementById('success').play();}
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;
var file = "";
var FileExtensionProfilePic = "";

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
        BranchId: $('#ddlBranch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/user/salesagentFetch',
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

function insert() {
    var det = {
        Username: $('#txtUsername').val(),
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        Gender: $('#ddlGender').val(),
        Religion: $('#ddlReligion').val(),
        CommissionPercent: $('#txtCommissionPercent').val(),
        DOB: $('#txtDOB').val(),
        JoiningDate: $('#txtJoiningDate').val(),
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
        UserType: 'sales',
        ProfilePic: file,
        FileExtensionProfilePic: FileExtensionProfilePic,
        IsActive: true,
        IsDeleted: false,
        Branchs: $('#ddlBranch').val() == null ? [] : $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/user/salesagentInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href = "/permission/salesagent";
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update() {
    var det = {
        UserId: window.location.href.split('=')[1],
        Username: $('#txtUsername').val(),
        Password: $('#txtPassword').val(),
        Name: $('#txtName').val(),
        MobileNo: $('#txtMobileNo').val(),
        EmailId: $('#txtEmailId').val(),
        Gender: $('#ddlGender').val(),
        AltMobileNo: $('#txtAltMobileNo').val(),
        Religion: $('#ddlReligion').val(),
        CommissionPercent: $('#txtCommissionPercent').val(),
        DOB: $('#txtDOB').val(),
        JoiningDate: $('#txtJoiningDate').val(),
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
        ProfilePic: file,
        FileExtensionProfilePic: FileExtensionProfilePic,
        UserType: 'sales',
    };
    $("#divLoading").show();
    $.ajax({
        url: '/user/salesagentUpdate',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href = "/permission/salesagent";
            }
            $("#divLoading").hide();
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
        url: '/user/salesagentActiveInactive',
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
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') {document.getElementById('success').play();}
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
    var r = confirm("This will delete permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            UserId: UserId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/user/salesagentdelete',
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
                    if (EnableSound == 'True') {document.getElementById('error').play();}
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') {document.getElementById('success').play();}
                    toastr.success(data.Message);
                    //window.location.href = "/permission/User";
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
        url: '/user/ActiveStates',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_States_Dropdown").html(data);
            }
            else {
                $("#p_Alt_States_Dropdown").html(data);
            }
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
        url: '/location/ActiveCitys',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (type == '') {
                $("#p_Citys_Dropdown").html(data);
            }
            else {
                $("#p_Alt_Citys_Dropdown").html(data);
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Sales Agent.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Sales Agent.csv",
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

            pdfMake.createPdf(docDefinition).download("Sales Agent.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}


function getProfileBase64() {
    var file1 = $("#ProfilePic").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') {document.getElementById('error').play();}
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

function openCountryModal(id) {
    _id = id;
    $('#countryModal').modal('toggle');
}

function openStateModal(id) {
    _id = id;
    $('#stateModal').modal('toggle');
}

function openCityModal(id) {
    _id = id;
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
        url: '/master/countryInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                $('#ddlCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#ddlAltCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#' + _id).val(data.Data.Country.CountryId);
                $('#countryModal').modal('toggle');

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
    var det = {
        CountryId: $('#ddlNewCountry').val(),
        StateCode: $('#txtStateCode').val(),
        State: $('#txtState').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/master/StateInsert',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                $('#ddlState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#ddlAltState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#' + _id).val(data.Data.State.StateId);
                $('#stateModal').modal('toggle');

                $('#ddlNewCountry').val($("#ddlNewCountry option:first").val());
                $('#txtStateCode').val('');
                $('#txtState').val('');
                $('.select2').select2();
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insertCity() {
    var det = {
        CountryId: $('#ddlCityModalCountry').val(),
        StateId: $('#ddlCityModalState').val(),
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
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                $('#ddlCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                $('#ddlAltCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                $('#' + _id).val(data.Data.City.CityId);
                $('#cityModal').modal('toggle');

                $('#ddlCityModalCountry').val($("#ddlCityModalCountry option:first").val());
                $('#ddlCityModalState').html('');
                $('#txtCityCode').val('');
                $('#txtCity').val('');
            }
            $("#divLoading").hide();
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

$('#txtMobileNo').autocomplete({
    type: "POST",
    minLength: 3,
    source: function (request, response) {
        $.ajax({
            url: "/permission/UserAutocomplete",
            dataType: "json",
            data: { MobileNo: request.term, UserType: 'sales', BranchId: $('#ddlBranch').val() },
            success: function (data) {
                //var _json = $.parseJSON(data.Data.Items);
                response(data.Data.UsersArray);
            },
            error: function (a, b, c) {
                HandleLookUpError(a);
            }
        });
    },
    select: function (event, ui) {
        window.location.href = "/permission/ExistingSalesAgent?MobileNo=" + ui.item.value.split('-')[1];
    }
});

function AddExisting() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType: 'sales',
        UserId: $('#txtUserId').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/user/AddExisting',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href = "/permission";
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};