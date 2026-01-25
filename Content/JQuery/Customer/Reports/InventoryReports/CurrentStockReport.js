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
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        ItemDetailsId: $('#ddlItem').val(),
        PriceAddedFor: $('#ddlPriceAddedFor').val(),
        BranchId: $('#ddlBranch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/CurrentStockFetch',
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
        url: '/inventoryreports/stockdetailsFetch',
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
        url: '/inventoryreports/stockdetailsInnerFetch',
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

function PrintInvoice(InvoiceUrl) {
    var myWindow = window.open(InvoiceUrl, "", "width=1000,height=1000");
}

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
            var dropdown = '<label>Item </label><select class="form-control select2" id="ddlItem"><option value="0">Select</option>';
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