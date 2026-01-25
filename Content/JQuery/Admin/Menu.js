
$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('aBusinessSetting').split('&')[0].split('=')[1];
    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('.select2').select2();

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    fetchActiveParentMenus();
});

var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];

var CurrencySymbol = Cookies.get('data').split('&')[5].split('=')[1];
var interval = null, count = 10000, ParentMenus;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

function fetchList() {
    var det = {
        Search: $('#txtSearch').val(),
        MenuType: $('#ddlMenuType').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/AdminMenu/MenuFetch',
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
            $("#thead").insertBefore(".table-body");
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    _i = i;
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var InnerMenus = [];

    $('.divCombo tr').each(function () {
        var _id = this.id.split('divCombo')[1];
        if (_id != undefined) {
            InnerMenus.push({
                MenuId: $('#hdnMenu' + _id).val(),
                QuickLinkParentId: $('#ddlQuickLinkParent' + _id).val(),
                ParentId: $('#ddlParent' + _id).val(),
                Menu: $('#txtMenu' + _id).val(),
                Title: $('#txtTitle' + _id).val(),
                Class: $('#txtClass' + _id).val(),
                Url: $('#txtUrl' + _id).val(),
                Sequence: $('#txtSequence' + _id).val(),
                Icon: $('#txtIcon' + _id).val(),
                HasChildren: $('#chkHasChildren' + _id).is(':checked'),
                MenuType: $('#ddlMenuType' + _id).val(),
                HeaderId: $('#ddlHeader' + _id).val(),
                IsMenu: $('#chkIsMenu' + _id).is(':checked'),
                IsView: $('#chkIsView' + _id).is(':checked'),
                IsAdd: $('#chkIsAdd' + _id).is(':checked'),
                IsEdit: $('#chkIsEdit' + _id).is(':checked'),
                IsDelete: $('#chkIsDelete' + _id).is(':checked'),
                IsExport: $('#chkIsExport' + _id).is(':checked'),
                IsQuickLink: $('#chkIsQuickLink' + _id).is(':checked'),
                IsActive: $('#chkIsActive' + _id).is(':checked'),
                IsDeleted: $('#chkIsDeleted' + _id).is(':checked'),
            })
        }
    });
    var det = {
        InnerMenus: InnerMenus
    };
    $("#divLoading").show();
    $.ajax({
        url: '/AdminMenu/UpdateMenu',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                window.location.reload();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function addNewMenu() {
    var ddlParent = '<select class="form-control" style="min-width:150px" id="ddlParent' + count + '"><option value="0">None</option>';

    for (let ss = 0; ss < ParentMenus.length; ss++) {
        ddlParent = ddlParent + '<option value="' + ParentMenus[ss].MenuId + '">' + ParentMenus[ss].Menu + '</option>';
    }
    ddlParent = ddlParent + '</select>';

    var ddlMenuType =
        '<select class="form-control" id="ddlMenuType' + count + '" style="min-width:180px">' +
        '<option value="Base Plan">Base Plan</option>' +
        '<option value="Purchase">Purchase</option>' +
        '<option value="Customer Group">Customer Group</option>' +
        '<option value="Pos">Pos</option>' +
        '<option value="Stock Transfer">Stock Transfer</option>' +
        '<option value="Expense Tracking">Expense Tracking</option>' +
        '<option value="Accounts">Accounts</option>' +
        '<option value="Activity Log">Activity Log</option>' +
        '<option value="Income Tracking">Income Tracking</option>' +
        '<option value="Selling Price Group">Selling Price Group</option>' +
        '<option value="Domain">Domain</option>' +
        '<option value="Multiple Currency">Multiple Currency</option>' +
        '<option value="Sms">Sms</option>' +
        '<option value="Email">Email</option>' +
        '<option value="Payment Attachment">Payment Attachment</option>' +
        '</select>';

    var html = '<tr id="divCombo' + count + '">' +
        '<td>' + count + '' +
        '<input type="hidden" id="hdnMenu' + count + '" value="0" />' +
        '</td>' +
        '<td> <input type="checkbox" id = "chkIsQuickLink' + count + '" /></td>' +
        '<td>' +
        ddlParent +
        '</td>' +
        '<td> <input class="form-control" type="text" value="" style="min-width:180px" id="txtMenu' + count + '" /> </td>' +
        '<td> <input class="form-control" type="text" value="" style="min-width:180px" id="txtTitle' + count + '" /></td>' +
        '<td> <textarea class="form-control" style="min-width:200px" id="txtClass' + count + '"></textarea></td>' +
        '<td> <input class="form-control" type="text" value="" style="min-width:180px" id="txtUrl' + count + '" /></td>' +
        '<td> <input class="form-control" type="number" value="" id="txtSequence' + count + '" /> </td>' +
        '<td> <input class="form-control" type="text" value="" style="min-width:120px" id="txtIcon' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkHasChildren' + count + '" /> </td>' +
        '<td>' +
        ddlMenuType +
        '</td>' +
        '<td> <input type="checkbox" id = "chkIsMenu' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsView' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsAdd' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsEdit' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsDelete' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsExport' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsActive' + count + '" /></td>' +
        '<td> <input type="checkbox" id = "chkIsDeleted' + count + '" /></td>' +
        '<td><input type="button" id="" onclick="deleteMenu(' + count + ')" value="Delete"/></td>' +
        '</tr>';
    count++;

    $('.divCombo').prepend(html);
}

function fetchActiveParentMenus() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/AdminMenu/ActiveParentMenus',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                ParentMenus = data.Data.Menus;
            }
            
        },
        error: function (xhr) {
            
        }
    });
}

function deleteMenu(id) {
    $('#divCombo' + id).remove();
}