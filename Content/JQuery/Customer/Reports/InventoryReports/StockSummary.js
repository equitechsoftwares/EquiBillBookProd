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
    $('#txtDateRange').daterangepicker({
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
        ItemDetailsId: $('#ddlItem').val(),
        PriceAddedFor: $('#ddlPriceAddedFor').val(),
        BranchId: $('#ddlBranch').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/inventoryreports/StockSummaryFetch',
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