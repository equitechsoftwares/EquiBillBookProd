$(function () {
    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    $('textarea.txtEmailBody').summernote({
        placeholder: '',
        followingToolbar: false,
        tabsize: 2,
        height: 200,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
            ['fontname', ['fontname']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'hr']],
            ['view', ['fullscreen', 'codeview']],
            ['help', ['help']]
        ],
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

function NotificationTemplateUpdate(NotificationModulesId, NotificationModulesDetailsId, NotificationTemplatesId) {
    var det = {
        NotificationModulesId: NotificationModulesId,
        NotificationModulesDetailsId: NotificationModulesDetailsId,
        NotificationTemplatesId: NotificationTemplatesId,
        AutoSendEmail: $('#chkAutoSendEmail' + NotificationTemplatesId).is(':checked'),
        EmailSubject: $('#txtEmailSubject' + NotificationTemplatesId).val(),
        CC: $('#txtCC' + NotificationTemplatesId).val(),
        BCC: $('#txtBCC' + NotificationTemplatesId).val(),
        EmailBody: $('#txtEmailBody' + NotificationTemplatesId).val(),
        AutoSendSms: $('#chkAutoSendSms' + NotificationTemplatesId).is(':checked'),
        SmsBody: $('#txtSmsBody' + NotificationTemplatesId).val(),
        AutoSendWhatsapp: $('#chkAutoSendWhatsapp' + NotificationTemplatesId).is(':checked'),
        WhatsappBody: $('#txtWhatsappBody' + NotificationTemplatesId).val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/notificationsettings/NotificationTemplateUpdate',
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
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function setAvailableTags(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}