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
        url: '/inventoryreports/StockLedgerFetch',
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

            $('#divPriceAddedFor').hide();
            $("#ddlPriceAddedFor").html('');

            var dropdown = '<label>Item </label><select class="form-control select2" id="ddlItem" onchange="fetchItemMultipleUnits()"><option value="0">Select</option>';
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

function fetchItemMultipleUnits() {
    var det = {
        ItemDetailsId: $('#ddlItem').val()
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/items/ItemMultipleUnits',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $('#divPriceAddedFor').hide();
            $("#ddlPriceAddedFor").html('');
            if (data.Data.Item.SecondaryUnitId != 0) {
                $('#divPriceAddedFor').show();
                $("#ddlPriceAddedFor").append($("<option></option>").val(1).html(data.Data.Item.UnitName));
                $("#ddlPriceAddedFor").append($("<option></option>").val(2).html(data.Data.Item.SecondaryUnitName));
            }
            if (data.Data.Item.TertiaryUnitId != 0) {
                $("#ddlPriceAddedFor").append($("<option></option>").val(3).html(data.Data.Item.TertiaryUnitName));
            }
            if (data.Data.Item.QuaternaryUnitId != 0) {
                $("#ddlPriceAddedFor").append($("<option></option>").val(4).html(data.Data.Item.QuaternaryUnitName));
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};