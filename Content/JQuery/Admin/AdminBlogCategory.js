$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    // Initialize error texts as hidden
    $('.errorText').hide();
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];
var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') {document.getElementById('success').play();}
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    _PageIndex = PageIndex;
    var PageSize = $('#txtPageSize').val();
    var Search = $('#txtSearch').val();
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var obj = {
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        PageIndex: PageIndex,
        PageSize: parseInt(PageSize),
        Search: Search
    };
    
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/blogcategoryfetch',
        type: 'POST',
        data: obj,
        success: function (res) {
            $('#tblData').html(res);
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
        error: function () {
            $("#divLoading").hide();
            toastr.error('Error loading blog category list');
        }
    });
}

function getCookieAndRoute() {
    var cookieName = Cookies.get('adata') ? 'adata' : 'data';
    var baseRoute = cookieName === 'adata' ? '/adminblogcategory' : '/blogcategory';
    return { cookieName: cookieName, baseRoute: baseRoute };
}

function insert(i) {
    $('.errorText').hide();
    $('.divCategoryName_ctrl').css('border', '');
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var obj = {
        CategoryName: $('#txtCategoryName').val(),
        IsActive: true,
        IsDeleted: false,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    
    $.ajax({
        url: cookieRoute.baseRoute + '/insert',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                if (res.Data && res.Data.Errors) {
                    res.Data.Errors.forEach(function(error) {
                        $('#' + error.Id).text(error.Message).show();
                        $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                    });
                } else {
                    toastr.error(res.Message || 'Error saving blog category');
                }
            }
        },
        error: function () {
            toastr.error('Error saving blog category');
        }
    });
}

function update(i) {
    $('.errorText').hide();
    $('.divCategoryName_ctrl').css('border', '');
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var ModifiedBy = arr[2].split('=')[1];
    
    var obj = {
        BlogCategoryId: parseInt($('#txtBlogCategoryId').val()),
        CategoryName: $('#txtCategoryName').val(),
        IsActive: $('#txtIsActive').val() === 'true',
        IsDeleted: false,
        CompanyId: CompanyId,
        ModifiedBy: ModifiedBy
    };
    
    $.ajax({
        url: cookieRoute.baseRoute + '/update',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                if (res.Data && res.Data.Errors) {
                    res.Data.Errors.forEach(function(error) {
                        $('#' + error.Id).text(error.Message).show();
                        $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                    });
                } else {
                    toastr.error(res.Message || 'Error updating blog category');
                }
            }
        },
        error: function () {
            toastr.error('Error updating blog category');
        }
    });
}

function Delete(BlogCategoryId, CategoryName) {
    var r = confirm("This will delete \"" + CategoryName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var cookieRoute = getCookieAndRoute();
        var arr = Cookies.get(cookieRoute.cookieName).split('&');
        var CompanyId = arr[1].split('=')[1];
        var ModifiedBy = arr[2].split('=')[1];
        
        var det = {
            BlogCategoryId: BlogCategoryId,
            CompanyId: CompanyId,
            ModifiedBy: ModifiedBy
        };
        $("#divLoading").show();
        $.ajax({
            url: cookieRoute.baseRoute + '/delete',
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
                    if (EnableSound == 'True') {document.getElementById('error').play();}
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') {document.getElementById('success').play();}
                    toastr.success(data.Message);
                    fetchList(_PageIndex);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function ActiveInactive(BlogCategoryId, IsActive) {
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var det = {
        BlogCategoryId: BlogCategoryId,
        IsActive: IsActive,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/activeinactive',
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
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') {document.getElementById('success').play();}
                toastr.success(data.Message);
                fetchList(_PageIndex);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

