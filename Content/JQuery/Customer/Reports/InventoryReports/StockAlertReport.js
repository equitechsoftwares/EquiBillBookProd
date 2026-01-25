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
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        //ItemId: $('#ddlItem').val(),
        //ProductType: $('#ddlProductType').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/stockalertFetch',
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
        BranchId: $('#ddlBranch').val()
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/UsersBranchWise',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>User </label><select class="form-control select2" id="ddlUser"> <option selected="selected" value="">All</option>';
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


function ViewItem(ItemId) {
    var det = {
        ItemId: ItemId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/items/itemsview',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ItemViewModal').modal('toggle');
            $("#divItemView").html(data);
            //$("#ViewModal").parent().appendTo($("form:first"));
            //checkItemType();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};