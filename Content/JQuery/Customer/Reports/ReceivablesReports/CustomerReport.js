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
        CustomerId: $('#ddlCustomer').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        UserGroupId: $('#ddlUserGroup').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/receivablesreports/CustomersFetch',
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

function UsersBranchWise() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        UserType:"Customer"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/receivablesreports/UsersBranchWise',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>Customer </label><select class="form-control select2" id="ddlCustomer"> <option selected="selected" value="">All</option>';
            $.each(data.Data.Users, function (index, value) {
                dropdown = dropdown + '<option value="' + value.UserId + '">' + value.Name + ' - ' + value.MobileNo + '</option>';
            });

            dropdown = dropdown + '</select>';
            $('#divUser').html('');
            $('#divUser').append(dropdown);
            $('.select2').select2();
            
        },
        error: function (xhr) {
            
        }
    });
};


function ViewCustomer(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/CustomerView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#CustomerViewModal').modal('toggle');
            $("#divCustomerView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ViewUnusedAdvanceBalance(_CustomerId) {
    var det = {
        CustomerId: _CustomerId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/customers/UnusedAdvanceBalance',
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