$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var ProfilePic = '', fileExtensionProfilePic = '';


function fetchActiveStates() {
    var det = {
        CountryId: $('#ddlBusinessCountry').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/ActiveStatesForBusiness',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_States_Dropdown").html(data);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveCitys() {
    var det = {
        StateId: $('#ddlBusinessState').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/ActiveCitysForBusiness',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_Citys_Dropdown").html(data);
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update() {
    var det = {
        BusinessName: $('#txtBusinessName').val(),
        Tagline: $('#txtTagline').val(),
        BusinessType: $('#txtBusinessType').val(),
        OwnerName: $('#txtOwnerName').val(),
        BusinessMobileNo: $('#txtBusinessMobileNo').val(),
        BusinessAltMobileNo: $('#txtBusinessAltMobileNo').val(),
        BusinessEmailId: $('#txtBusinessEmailId').val(),
        TaxNo: $('#txtTaxNo').val(),
        Notes: $('#txtNotes').val(),
        CurrencyId: $('#ddlCurrency').val(),
        BusinessCountryId: $('#ddlBusinessCountry').val(),
        BusinessStateId: $('#ddlBusinessState').val(),
        BusinessCityId: $('#ddlBusinessCity').val(),
        BusinessZipcode: $('#txtBusinessZipcode').val(),
        BusinessAddress: $('#txtBusinessAddress').val(),
        BusinessLogo: ProfilePic,
        FileExtensionProfilePic: fileExtensionProfilePic
    };
    $("#divLoading").show();
    $.ajax({
        url: '/settings1/businessinformation',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') {document.getElementById('success').play();}
                toastr.success(data.Message);
                //window.location.href = "/master/Branch";
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getProfilePicBase64() {
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
            ProfilePic = reader.result;
            fileExtensionProfilePic = '.' + file1.name.split('.').pop();
            
            $('#blahProfilePic').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            ProductImage = error;
        };
    }
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
                $('#ddlBusinessCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#ddlBusinessCountry').val(data.Data.Country.CountryId);
                $('#countryModal').modal('toggle');

                $('#ddlNewCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));
                $('#ddlCityModalCountry').append($('<option>', { value: data.Data.Country.CountryId, text: data.Data.Country.Country }));

                $('#ddlBusinessState').html('');
                $('#ddlBusinessCity').html('');

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
                $('#ddlBusinessState').append($('<option>', { value: data.Data.State.StateId, text: data.Data.State.State }));
                $('#ddlBusinessState').val(data.Data.State.StateId);
                $('#stateModal').modal('toggle');

                $('#ddlBusinessCity').html('');

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
                $('#ddlBusinessCity').append($('<option>', { value: data.Data.City.CityId, text: data.Data.City.City }));
                $('#ddlBusinessCity').val(data.Data.City.CityId);
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