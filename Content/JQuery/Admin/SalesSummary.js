
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
function PayNow(Month, Year, UserId, Amount) {
    if (confirm("Are you sure you want to make a payment?") == true) {
        $('.errorText').hide();
        var det = {
            Month: Month,
            Year: Year,
            UserId: UserId,
            Amount: Amount
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminuser/ResellerPaymentInsert',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    if (EnableSound == 'True') { document.getElementById('success').play(); }
                    toastr.success(data.Message);
                    window.location.reload();
                    //fetchList(_PageIndex);
                }

            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}