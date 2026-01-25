var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];

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

function sendOtp() {
    $('.errorText').hide();
    var det = {
        OldEmailId: $('#txtCurrentEmailId').val(),
        EmailId: $('#txtEmailId').val(),
        CEmailId: $('#txtCEmailId').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/ValidateLoginEmail',
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
            else if (data.Status == 2) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                $('#lblOtpSuccessMsg').text('A one time password has been sent to ' + $('#txtEmailId').val()+'. Make sure to check your spam folder');
                $('.divEmail').hide();
                $('.divEmailOtp').show();
            }
            
        },
        error: function (data) {
            if (EnableSound == 'True') {document.getElementById('error').play();}
            toastr.error(data.Message);
        }
    });
}

function changeEmailId() {
    $('.errorText').hide();
    var det = {
        OldEmailId: $('#txtCurrentEmailId').val(),
        EmailId: $('#txtEmailId').val(),
        CEmailId: $('#txtCEmailId').val(),
        Otp: $('#txtOtp').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/UpdateLoginEmail',
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
            else if (data.Status == 2) {
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href ="/adminsettings/changeloginemail"
            }
            
        },
        error: function (data) {
            if (EnableSound == 'True') {document.getElementById('error').play();}
            toastr.error(data.Message);
        }
    });
}

function back() {
    $('.divEmail').show();
    $('.divEmailOtp').hide();
}