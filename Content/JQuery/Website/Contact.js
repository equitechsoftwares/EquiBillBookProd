
function submit() {
    var det = {
        Name: $('#txtName').val(),
        EmailId: $('#txtEmailId').val(),
        MobileNo: $('#txtMobileNo').val(),
        Subject: $('#txtSubject').val(),
        Message: $('#txtMessage').val()
    };
    $("#divLoading").show();
    $('#btnSubmit').attr('disabled', true);
    $.ajax({
        url: '/Contact/RequestQuote',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                alert(data.Message);
            }
            else {
                alert(data.Message);
                window.location.href = "/contact";
            }
            $("#divLoading").hide();
            $('#btnSubmit').attr('disabled', false);
        },
        error: function (xhr) {
            $("#divLoading").hide();
            $('#btnSubmit').attr('disabled', false);
        }
    });
};