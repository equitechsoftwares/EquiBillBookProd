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

    //Date range picker
    $('#txtDate').daterangepicker({
        locale: {
            format: 'DD-MM-YYYY'
        }
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var _PageIndex = 1;

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        UserId: $('#ddlUser').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        Type: $('#ddlType').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/employeereports/RegisterReportFetch',
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
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveUsers(showAddNew) {
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType: 'user'
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/usersettings/AllActiveUsers',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                var ddlUser = '<select class="form-control select2" id="ddlUser">';
                ddlUser = ddlUser + '<option value="0">All</option>';

                for (let ss = 0; ss < data.Data.Users.length; ss++) {
                    ddlUser = ddlUser + '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name + '</option>';
                }
                ddlUser = ddlUser + '</select>';

                $('.divUser').empty();
                $('.divUser').append(ddlUser);

                $('.select2').select2();
            }
            
        },
        error: function (xhr) {
            
        }
    });
};

function ViewUser(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/user/UserView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#UserViewModal').modal('toggle');
            $("#divUserView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewBranch(BranchId) {
    var det = {
        BranchId: BranchId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/master/BranchView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#BranchViewModal').modal('toggle');
            $("#divBranchView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};