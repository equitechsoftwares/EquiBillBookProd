$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    //if (window.location.href.indexOf('BranchId') > -1) {
    //    var _branchid = window.location.href.split('=')[1];
    //    $('#ddlBranch').val(_branchid);
    //}

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
        SupplierId: $('#ddlSupplier').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        UserGroupId: $('#ddlUserGroup').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/payablesreports/SuppliersFetch',
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
        UserType: "Supplier"
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/payablesreports/UsersBranchWise',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>Supplier </label><select class="form-control select2" id="ddlSupplier"> <option selected="selected" value="">All</option>';
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

function ViewSupplier(UserId) {
    var det = {
        UserId: UserId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/supplier/SupplierView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SupplierViewModal').modal('toggle');
            $("#divSupplierView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

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