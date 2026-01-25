$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 100);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/userroleFetch',
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

function View(RoleId) {
    var det = {
        RoleId: RoleId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/UserRoleView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ViewModal').modal('toggle');
            $("#divView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var MenuPermissions = [];
    //$('#divPermissions tr').each(function () {
    //    var _innerid = this.id.split('divPermission')[1];
    //    MenuPermissions.push({
    //        MenuId: $('#hdnMenuId' + _innerid).val(),
    //        IsAdd: $('#chkIsAdd' + _innerid).is(':checked'),
    //        IsEdit: $('#chkIsEdit' + _innerid).is(':checked'),
    //        IsDelete: $('#chkIsDelete' + _innerid).is(':checked'),
    //        IsView: $('#chkIsView' + _innerid).is(':checked'),
    //        IsExport: $('#chkIsExport' + _innerid).is(':checked')
    //    })
    //});

    $(".divPermissions").each(function () {
        $('#' + this.id + ' tr').each(function () {
            var _innerid = this.id.split('divPermission')[1];
            MenuPermissions.push({
                MenuId: $('#hdnMenuId' + _innerid).val(),
                MenuPermissionId: $('#hdnMenuPermissionId' + _innerid).val(),
                IsAdd: $('#chkIsAdd' + _innerid).is(':checked'),
                IsEdit: $('#chkIsEdit' + _innerid).is(':checked'),
                IsDelete: $('#chkIsDelete' + _innerid).is(':checked'),
                IsView: $('#chkIsView' + _innerid).is(':checked'),
                IsExport: $('#chkIsExport' + _innerid).is(':checked')
            })
        });
    });
    var det = {
        RoleCode: $('#txtRoleCode').val(),
        RoleName: $('#txtRoleName').val(),
        IsActive: true,
        IsDeleted: false,
        MenuPermissions: MenuPermissions
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/userroleInsert',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);


                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/usersettings/userrole";
                }
                else {
                    window.location.href = "/usersettings/userroleadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');


    var MenuPermissions = [];

    $(".divPermissions").each(function () {
        $('#' + this.id + ' tr').each(function () {
            var _innerid = this.id.split('divPermission')[1];
            MenuPermissions.push({
                MenuId: $('#hdnMenuId' + _innerid).val(),
                MenuPermissionId: $('#hdnMenuPermissionId' + _innerid).val(),
                IsAdd: $('#chkIsAdd' + _innerid).is(':checked'),
                IsEdit: $('#chkIsEdit' + _innerid).is(':checked'),
                IsDelete: $('#chkIsDelete' + _innerid).is(':checked'),
                IsView: $('#chkIsView' + _innerid).is(':checked'),
                IsExport: $('#chkIsExport' + _innerid).is(':checked')
            })
        });
    });

    //$('#divPermissions tr').each(function () {
    //    var _innerid = this.id.split('divPermission')[1];
    //    MenuPermissions.push({
    //        MenuId: $('#hdnMenuId' + _innerid).val(),
    //        MenuPermissionId: $('#hdnMenuPermissionId' + _innerid).val(),
    //        IsAdd: $('#chkIsAdd' + _innerid).is(':checked'),
    //        IsEdit: $('#chkIsEdit' + _innerid).is(':checked'),
    //        IsDelete: $('#chkIsDelete' + _innerid).is(':checked'),
    //        IsView: $('#chkIsView' + _innerid).is(':checked'),
    //        IsExport: $('#chkIsExport' + _innerid).is(':checked')
    //    })
    //});

    var det = {
        RoleId: window.location.href.split('=')[1],
        RoleCode: $('#txtRoleCode').val(),
        RoleName: $('#txtRoleName').val(),
        MenuPermissions: MenuPermissions
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/userroleUpdate',
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
                //$("html, body").animate({ scrollTop: 0 }, "slow");
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);


                    if ($('.' + res.Id + '_ctrl').children("select").length > 0) {
                        var element = $("." + res.Id + '_ctrl select');
                        if (element.hasClass("select2-hidden-accessible")) {
                            $('.' + res.Id + '_ctrl .select2-container .select2-selection').css('border', '2px solid #dc3545');
                        } else {
                            $('.' + res.Id + '_ctrl select').css('border', '2px solid #dc3545');
                        }
                    }
                    else {
                        $('.' + res.Id + '_ctrl').css('border', '2px solid #dc3545');
                    }
                });
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/usersettings/userrole";
                }
                else {
                    window.location.href = "/usersettings/userroleadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(RoleId, IsActive) {
    var det = {
        RoleId: RoleId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/usersettings/RoleActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                fetchList();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(RoleId, RoleName) {
    var r = confirm("This will delete \"" + RoleName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            RoleId: RoleId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/usersettings/userroledelete',
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
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};


function selectAll() {
    if ($('#chkSelectAll').is(':checked')) {
        $('input:checkbox').prop('checked', true);
    } else {
        $('input:checkbox').prop('checked', false);
    }
}

function selectAllModule(id) {
    if ($('#' + id).is(':checked')) {
        $('.' + id).prop('checked', true);
    } else {
        $('.' + id).prop('checked', false);

        $('#chkSelectAll').prop('checked', false);
    }
}

function selectedSelectAll(id) {
    if ($('#' + id).is(':checked')) {
        $('.' + id).prop('checked', true);
    } else {
        $('.' + id).prop('checked', false);

        var _class = $('#' + id).attr('class');
        $('#' + _class).prop('checked', false);

        $('#chkSelectAll').prop('checked', false);
    }
}

function toggleView(c) {
    if (!$('#chkIsView' + c).is(':checked')) {
        $('#chkIsAdd' + c).prop('checked', false);
        $('#chkIsEdit' + c).prop('checked', false);
        $('#chkIsDelete' + c).prop('checked', false);
        $('#chkIsExport' + c).prop('checked', false);

        $('#chk' + c).prop('checked', false);

        var _class = $('#chkIsView' + c).attr('class').split(" ")[1];
        $('#' + _class).prop('checked', false);

        $('#chkSelectAll').prop('checked', false);
    }
}

function toggle(c) {
    if ($('#chkIsAdd' + c).is(':checked') || $('#chkIsEdit' + c).is(':checked') || $('#chkIsDelete' + c).is(':checked') || $('#chkIsExport' + c).is(':checked')) {
        $('#chkIsView' + c).prop('checked', true);
    }

    if (!$('#chkIsAdd' + c).is(':checked') || !$('#chkIsEdit' + c).is(':checked') || !$('#chkIsDelete' + c).is(':checked') || !$('#chkIsExport' + c).is(':checked')) {
        $('#chk' + c).prop('checked', false);

        var _class = $('#chkIsAdd' + c).attr('class').split(" ")[1];
        $('#' + _class).prop('checked', false);

        $('#chkSelectAll').prop('checked', false);
    }
}