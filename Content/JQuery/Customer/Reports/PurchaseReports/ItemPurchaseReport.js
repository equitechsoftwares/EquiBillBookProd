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
        SupplierId: $('#ddlSupplier').val(),
        //Status: $('#ddlStatus').val(),
        //PaymentStatus: $('#ddlPaymentStatus').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryId: $('#ddlSubSubCategory').val(),
        BrandId: $('#ddlBrand').val(),
        ItemDetailsId: $('#ddlItem').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/purchasereports/itempurchaseFetch',
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
        url: '/purchasereports/UsersBranchWise',
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

function fetchActiveSubCategories() {
    var det = {
        CategoryId: $('#ddlCategory').val()
    };
    _PageIndex = det.PageIndex;
    //$("#divLoading").show();
    $.ajax({
        url: '/purchasereports/ActiveSubCategories',
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
        url: '/purchasereports/ActiveSubSubCategories',
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

function ViewPurchase(PurchaseId) {
    var det = {
        PurchaseId: PurchaseId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/purchase/PurchaseView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#PurchaseViewModal').modal('toggle');
            $("#divPurchaseView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
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

function ViewItem(ItemId) {
    var det = {
        ItemId: ItemId,
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

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}