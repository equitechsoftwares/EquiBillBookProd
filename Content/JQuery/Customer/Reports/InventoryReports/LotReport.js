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

    $('.hideButton').hide();

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
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        LotNo: $('#txtLotNo').val(),
        //ItemName: $('#txtItemName').val(),
        ItemDetailsId: $('#ddlItem').val(),
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryId: $('#ddlSubSubCategory').val(),
        BrandId: $('#ddlBrand').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/lotFetch',
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

function fetchActiveSubCategories() {
    var det = {
        CategoryId: $('#ddlCategory').val()
    };
    _PageIndex = det.PageIndex;
    //$("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/ActiveSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            
            $("#p_SubCategories_Dropdown").html(data);

            $('.hideButton').hide();
            $('.select2').css('width', '100%');
            $('.select2').select2();
        },
        error: function (xhr) {
            
        }
    });
};

function fetchActiveSubSubCategories() {
    var det = {
        SubCategoryId: $('#ddlSubCategory').val()
    };
    _PageIndex = det.PageIndex;
    //$("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/ActiveSubSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            
            $("#p_SubSubCategories_Dropdown").html(data);

            $('.hideButton').hide();
            $('.select2').css('width', '100%');
            $('.select2').select2();
        },
        error: function (xhr) {
            
        }
    });
}

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