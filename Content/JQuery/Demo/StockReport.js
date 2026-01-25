$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
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
    //var CategoryId = 0; var SubCategoryId = 0; var SubSubCategoryId = 0;
    //var val = $('#ddlCategory').val().split('_');
    //if (val.length > 1) {
    //    if (val[1] == "category") {
    //        CategoryId = val[0];
    //    }
    //    else if (val[1] == "sub category") {
    //        SubCategoryId = val[0];
    //    }
    //    else if (val[1] == "sub sub category") {
    //        SubSubCategoryId = val[0];
    //    }
    //}
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        BranchId: $('#ddlBranch').val(),
        /*ItemsArray: $('#ddlItem').val(),*/
        ItemDetailsId: $('#ddlItem').val(),
        BrandId: $('#ddlBrand').val(),
        CategoryId: $('#ddlCategory').val(),
        SubCategoryId: $('#ddlSubCategory').val(),
        SubSubCategoryId: $('#ddlSubSubCategory').val(),
        PriceAddedFor: $('#ddlPriceAddedFor').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/reports/StockFetch',
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

function ItemsBranchWise() {
    var det = {
        BranchId: $('#ddlBranch').val()
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/items/ActiveItems',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            var dropdown = '<label>Item </label><select class="form-control select2" id="ddlItem"><option value="0">--Select--</option>';
            $.each(data.Data.Items, function (index, value) {
                if (value.IsManageStock == true) {
                    if (value.VariationName) {
                        dropdown = dropdown + '<option value="' + value.ItemDetailsId + '">' + value.ItemName + '(' + value.VariationName + ')' + '-' + value.SKU + '</option>';
                    }
                    else {
                        dropdown = dropdown + '<option value="' + value.ItemDetailsId + '">' + value.ItemName + '-' + value.SKU + '</option>';
                    }
                }
            });

            dropdown = dropdown + '</select>';
            $('#divItem').html('');
            $('#divItem').append(dropdown);
            $('.select2').select2();
            
        },
        error: function (xhr) {
            
        }
    });
};

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Stock Report.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Stock Report.csv",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
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

            pdfMake.createPdf(docDefinition).download("Stock Report.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function exportToExcelStockDetails() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Stock Details Report.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToCsvStockDetails() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Stock Details Report.csv",
        sheet: {
            name: "Sheet1"
        }
    });
    //fetchList();
}

function exportToPdfStockDetails() {
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

            pdfMake.createPdf(docDefinition).download("Stock Details Report.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function fetchStockDetailsList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        ItemId: window.location.href.split('=')[1].split('&')[0],
        ItemDetailsId: window.location.href.split('=')[2].split('&')[0] == 0 ? $('#ddlItemDetails').val() : window.location.href.split('=')[2].split('&')[0],
        BranchId: window.location.href.split('=')[3].split('&')[0] == 0 ? $('#ddlBranch').val() : window.location.href.split('=')[3].split('&')[0],
        PriceAddedFor: $('#ddlPriceAddedFor').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/reports/stockdetailsFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#p_div").html(data);

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

function fetchStockDetailsInnerList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        ItemId: window.location.href.split('=')[1].split('&')[0],
        ItemDetailsId: window.location.href.split('=')[2].split('&')[0] == 0 ? $('#ddlItemDetails').val() : window.location.href.split('=')[2].split('&')[0],
        BranchId: window.location.href.split('=')[3].split('&')[0],
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/reports/stockdetailsInnerFetch',
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
    $("#divLoading").show();
    $.ajax({
        url: '/items/ActiveSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubCategories_Dropdown").html(data);
            $('.hideButton').hide();
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
        url: '/items/ActiveSubSubCategories',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $("#p_SubSubCategories_Dropdown").html(data);
            $('.hideButton').hide();
            $('.select2').select2();
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