
$(function () {

    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    if (window.location.href.toLocaleLowerCase().indexOf('domainadd') > -1) {
        checkConnection(window.location.href.split('&')[0].split('=')[1], window.location.href.split('&')[1].split('=')[1]);
    }
});
var DomainId = 0, _PageIndex = 1;

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    //toastr.success(sessionStorage.getItem("Msg"));
    $('#divSuccessMsg').show();
    sessionStorage.removeItem("showMsg");
    clearInterval(interval);
}

function addNew() {
    $('#txtDomain').val('');
    DomainId = 0;
    next(1);
}

function back() {
    fetchList();
    next(3);
}

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/systemsettings/DomainFetch',
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

function next(t) {
    $('#divNewDomain').hide();
    $('#divVerifyDomain').hide();
    closeErrorMsg();
    $('#divSuccessMsg').hide();

    if (t == 1) {
        $('#divNewDomain').show();
    }
    else if (t == 2) {
        $('#divVerifyDomain').show();
    }
}

function insert() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        DomainId: DomainId,
        Domain: $('#txtDomain').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/systemsettings/DomainInsert',
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
            }
            else {
                $('#lblDomain').text($('#txtDomain').val());
                DomainId = data.Data.Domain.DomainId;
                next(2);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(domainId, domain) {
    var r = confirm("This will delete \"" + domain + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            DomainId: domainId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/systemsettings/DomainDelete',
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
                else if (data.Status == 3)
                {
                    window.location.href ='/errorpage/domain';
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

function checkConnection(domainId, domain) {
    $('#lblDomain').text(domain);
    $('#txtDomain').val(domain);

    DomainId = domainId;
    next(2);
}

function VerifyDomainConnection() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        DomainId: DomainId,
        Domain: $('#txtDomain').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/systemsettings/VerifyDomainConnection',
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
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#divCurrentIp').text(res.Message);

                    
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
            }
            else {
                //DomainId = 0;
                //next(3);
                //fetchList(1);
                sessionStorage.setItem('showMsg', '1');
                window.location.href = "/systemsettings/domains";
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function closeErrorMsg() {
    $('#divErrorMsg').hide();
}

function copyCode(url) {
    /* Get the text field */
    var copyText = url;

    navigator.clipboard
        .writeText(copyText)
        .then(() => {
            toastr.success("Copied");
        })
        .catch(() => {
            toastr.success("Something went wrong");
        });
}