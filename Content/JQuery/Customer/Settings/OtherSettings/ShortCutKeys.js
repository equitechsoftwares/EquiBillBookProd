
var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

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

function updateShortCutKeys() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var ShortCutKeySettings = [];

    $('#divShortcutKeys tr').each(function () {
        var _id = this.id;
        if (_id != undefined) {
            //if ($('#chkIsShortcutKey' + _id).is(':checked')) {
                ShortCutKeySettings.push({
                    //MenuId: $('#chkIsShortcutKey' + _id).val(),
                    MenuId: $('#hdnMenuId' + _id).val(),
                    Title: $('#txtTitle' + _id).val(),
                    ShortCutkey: $('#txtShortCutkey' + _id).val(),
                    ShortCutKeySettingId: $('#hdnShortCutKeySettingId' + _id).val(),
                    divId: _id
                })
            //}
        }
    });

    var det = {
        ShortCutKeySettings: ShortCutKeySettings
    };
    $("#divLoading").show();
    $.ajax({
        url: '/othersettings/ShortCutKeySettingUpdate',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.reload();
                //if (EnableSound == 'True') { document.getElementById('success').play(); }
                //toastr.success(data.Message);

                //fetchShortCutKeySetting();
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchShortCutKeySetting() {
    var det = {
    };
    //$("#divLoading").show();
    $.ajax({
        url: '/othersettings/ShortcutKeySettings',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
            }
            else {
                var html = '';

                $.each(data.Data.ShortCutKeySettings, function (index, value) {
                    if (value.IsView == true) {
                        if (value.Url == "" || value.Url == null) {
                            html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a style="color:#000;cursor:unset;" id="btnshortcutkey' + index + '" disabled href="javascript:void(0)" class="nav-link">' + value.Title;
                        }
                        else {
                            html = html + '<li class="nav-item " style="border-bottom: 1px solid rgba(0,0,0,.125);"><a id="btnshortcutkey' + index + '" href="/' + value.Url + '" class="nav-link">' + value.Title;
                        }

                        if (value.ShortCutKey != "" && value.ShortCutKey != null) {
                            html = html + ' <span class="btn btn-default btn-sm"> ' + value.ShortCutKey + '</span>';

                            Mousetrap.bind(value.ShortCutKey, function (e) {
                                e.preventDefault();
                                window.location.href = '/' + value.Url;
                            });
                        }
                        html = html + '</a></li>';
                    }
                });

                $('.tShortcutKeys').append(html);
            }

        },
        error: function (xhr) {

        }
    });


}