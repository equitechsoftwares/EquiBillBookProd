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
    $('#txtPurchaseDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#txtSalesDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    //Date range picker
    $('#txtDate').daterangepicker({
        locale: {
            format: 'DD-MM-YYYY'
        }
    });

    //if (window.location.href.indexOf('BranchId') > -1) {
    //    var _branchid = window.location.href.split('=')[1];
    //    $('#ddlBranch').val(_branchid);
    //}

    $('.select2').select2();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var _PageIndex = 1;

function fetchList(PageIndex) {

    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryId: $('#ddlSubSubCategory').val(),
        PurchaseFromDate: $('#txtPurchaseDateRange').val() == undefined ? '' : $('#txtPurchaseDateRange').val().split(' ')[0],
        PurchaseToDate: $('#txtPurchaseDateRange').val() == undefined ? '' : $('#txtPurchaseDateRange').val().split(' ')[2],
        SalesFromDate: $('#txtSalesDateRange').val().split(' ')[0],
        SalesToDate: $('#txtSalesDateRange').val().split(' ')[2],
        //FromDate: $('#txtFromDate').val(),
        //ToDate: $('#txtToDate').val(),
        BrandId: $('#ddlBrand').val(),
        SKU: $('#txtSkuCode').val(),
        //ItemName: $('#txtItemName').val(),
        ItemDetailsId: $('#ddlItem').val(),
        SupplierId: $('#ddlSupplier').val(),
        CustomerId: $('#ddlCustomer').val(),
        PriceAddedFor: $('#ddlPriceAddedFor').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/ItemsFetch',
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
            var ddlSupplier = '<label>Supplier </label><select class="form-control select2" id="ddlSupplier"> <option selected="selected" value="">All</option>';
            var ddlCustomer = '<label>Customer </label><select class="form-control select2" id="ddlCustomer"> <option selected="selected" value="">All</option>';

            for (let ss = 0; ss < data.Data.Users.length; ss++) {
                if (data.Data.Users[ss].UserType.toLowerCase() == "supplier") {
                    ddlSupplier = ddlSupplier + '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name + '</option>';
                }
                else {
                    ddlCustomer = ddlCustomer + '<option value="' + data.Data.Users[ss].UserId + '">' + data.Data.Users[ss].Name + '</option>';
                }
            }
            ddlSupplier = ddlSupplier + '</select>';
            ddlCustomer = ddlCustomer + '</select>';

            //if (showAddNew == true) {
            //    ddlSupplier = ddlSupplier + '<span class="input-group-append">' +
            //        '<a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#user"> + </a>' +
            //        '</span>';
            //    ddlCustomer= ddlCustomer + '<span class="input-group-append">' +
            //        '<a href="javascript:void(0)" class="btn btn-info" data-toggle="modal" data-target="#user"> + </a>' +
            //        '</span>';
            //}

            $('#divSupplier').empty();
            $('#divSupplier').append(ddlSupplier);

            $('#divCustomer').empty();
            $('#divCustomer').append(ddlCustomer);

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
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubCategories_Dropdown").html(data);
            $(".hideButton").hide();
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchActiveSubSubCategories() {
    var det = {
        SubCategoryId: $('#ddlSubCategory').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/itemsettings/ActiveSubSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubSubCategories_Dropdown").html(data);
            $(".hideButton").hide();
            $('.select2').select2();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function ViewSale(SalesId) {
    var det = {
        SalesId: SalesId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/sales/SalesView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#SaleViewModal').modal('toggle');
            $("#divSaleView").html(data);
            $('.divReport').hide();
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
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

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}