
function forgotPassword() {
    $('#divLoading').show();
    var det = {
        EmailId: $('#txtEmailId').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminforgotpassword/ForgotPassword',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                document.getElementById('success').play();
                toastr.success(data.Message);
            }
            $('#txtEmailId').val('');

            $("#divLoading").hide();
        },
        error: function (data) {
            //scrollToTop();
            $("#divLoading").hide();
        }
    });
};

function ResetPassword() {
    var det = {
        Password: $('#txtPassword').val(),
        CPassword: $('#txtCPassword').val(),
        Token: window.location.href.split('=')[1]
    };
    $("#divLoading").show();
    $.ajax({
        url: '/AdminResetPassword/ResetPassword',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                //document.getElementById('success').play();
                //toastr.success(data.Message);

                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href = "/adminlogin";
            }
            $("#divLoading").hide();
        },
        error: function (xhr) {
            document.getElementById('error').play();
            toastr.error(data.Message);
        }
    });
};