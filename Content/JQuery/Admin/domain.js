var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];

var interval = null;

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

var DomainId = 0;

function addNew() {
    $('#txtDomain').val('');
    DomainId = 0;
    next(1);
}

function back() {
    fetchList();
    next(3);
}

function fetchList(t) {
    var det = {
        UserId: window.location.href.split('=')[1]
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/admindomain/DomainFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblData").html(data);
            $("#divLoading").hide();
            if (t == 1) {
                $('#divSuccessMsg').show();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function next(t) {
    $('#divNewDomain').hide();
    $('#divVerifyDomain').hide();
    $('#divConnectedDomains').hide();
    closeErrorMsg();
    $('#divSuccessMsg').hide();

    if (t == 1) {
        $('#divNewDomain').show();
    }
    else if (t == 2) {
        $('#divVerifyDomain').show();
    }
    else if (t == 3) {
        $('#divConnectedDomains').show();
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
        UserId: window.location.href.split('=')[1]
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminDomain/DomainInsert',
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
                });
            }
            else {
                $('#lblDomain').text($('#txtDomain').val());
                DomainId = data.Data.Domain.DomainId;
                next(2);
                //if (t == 3) {
                //    fetchList();
                //}
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function Delete(domainId, domain) {
    var r = confirm("This will delete \"" + domain +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            DomainId: domainId,
            UserId: window.location.href.split('=')[1]
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminDomain/DomainDelete',
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
                else if (data.Status == 3) {
                    window.location.href = '/errorpage/domain';
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
    var det = {
        DomainId: DomainId,
        Domain: $('#txtDomain').val(),
        UserId: window.location.href.split('=')[1]
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminDomain/VerifyDomainConnection',
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
                });
            }
            else {
                DomainId = 0;
                next(3);
                fetchList(1);

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