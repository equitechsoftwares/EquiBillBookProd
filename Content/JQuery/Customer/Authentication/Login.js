
var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 500);
}

function playSound() {
    document.getElementById('success').play();
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

function login() {
    $('#divLoading').show();
    var det = {
        EmailId: $('#txtEmailId').val(),
        Password: $('#txtPassword').val(),
        IsRememberMe: $('#chkIsRememberMe').is(":checked")
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Login/Login',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                document.getElementById('error').play();
                toastr.error(data.Message);
            }
            else {
                window.location.href = "/dashboard/index?type=login";
            }

            $("#divLoading").hide();
        },
        error: function (data) {
            //scrollToTop();
            $("#divLoading").hide();
        }
    });
};

document.getElementById('txtEmailId').onkeydown = function (e) {
    if (e.keyCode == 13) {
        login();
    }
};

document.getElementById('txtPassword').onkeydown = function (e) {
    if (e.keyCode == 13) {
        login();
    }
};