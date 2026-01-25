$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

});

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

var _PageIndex = 1;
var Attachment1 = "";
var Attachment1Extension = "";
var Attachment2 = "";
var Attachment2Extension = "";
var Attachment3 = "";
var Attachment3Extension = "";
var Attachment4 = "";
var Attachment4Extension = "";
var Attachment5 = "";
var Attachment5Extension = "";


function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminSupportTicket/SupportTicketFetch',
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

function insert() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        HelpTopic: $('#ddlHelpTopic').val(),
        Subject: $('#txtSubject').val(),
        Message: $('#txtMessage').val(),
        IsActive: true,
        IsDeleted: false,
        Attachment1: Attachment1,
        Attachment1Extension: Attachment1Extension,
        Attachment2: Attachment2,
        Attachment2Extension: Attachment2Extension,
        Attachment3: Attachment3,
        Attachment3Extension: Attachment3Extension,
        Attachment4: Attachment4,
        Attachment4Extension: Attachment4Extension,
        Attachment5: Attachment5,
        Attachment5Extension: Attachment5Extension
    };
    $("#divLoading").show();
    $.ajax({
        url: '/SupportTicket/SupportTicketInsert',
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

                window.location.href = "/SupportTicket";
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getProductImageBase64(type) {
    var file1 =$("#Attachment" + type).prop("files")[0];

    //// The size of the file. 
    //if (file1.size >= 2097152) {
    //    if (EnableSound == 'True') {document.getElementById('error').play();}
    //    toastr.error('File too Big, please select a file less than 2mb');
    //    $("#ProductImage").val('');
    //    return false;
    //}
    //else {
    var reader = new FileReader();
    reader.readAsDataURL(file1);
    reader.onload = function () {
        if (type == 1) {
            Attachment1 = reader.result;
            Attachment1Extension = '.' + file1.name.split('.').pop();
            
        }
        else if (type == 2) {
            Attachment2 = reader.result;
            Attachment2Extension = '.' + file1.name.split('.').pop();
        }
        else if (type == 3) {
            Attachment3 = reader.result;
            Attachment3Extension = '.' + file1.name.split('.').pop();
        }
        else if (type == 4) {
            Attachment4 = reader.result;
            Attachment4Extension = '.' + file1.name.split('.').pop();
        }
        else if (type == 5) {
            Attachment5 = reader.result;
            Attachment5Extension = '.' + file1.name.split('.').pop();
        }
        $('#blahAttachment' + type).attr('src', reader.result);
    };
    reader.onerror = function (error) {
        console.log(error);
        ProductImage = error;
    };
    //}
}

function SupportTicketDetailsInsert() {
    $('.errorText').hide();
    var det = {
        Message: $('#txtMessage').val(),
        SupportTicketId: window.location.href.split('&')[1].split('=')[1]
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminSupportTicket/SupportTicketDetailsInsert',
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
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);

                window.location.reload();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function CloseSupportTicket() {
    if (confirm("Are you sure you want to close this support ticket? You will not be able to add more messages once closed.") == true) {
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');

        var det = {
            SupportTicketId: window.location.href.split('&')[1].split('=')[1]
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminSupportTicket/CloseSupportTicket',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {

                $("#divLoading").hide();
                if (data == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('adata').split('&')[9].split('=')[1]);
                    return
                }
                else if (data == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('adata').split('&')[9].split('=')[1]);
                    return
                }

                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else if (data.Status == 2) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error('Invalid inputs, check and try again !!');
                    if (data.Errors) {
                        data.Errors.forEach(function (res) {
                            $('#' + res.Id).show();
                            $('#' + res.Id).text(res.Message);
                        });
                    } else {
                        toastr.error(data.Message);
                    }
                }
                else {
                    sessionStorage.setItem('showMsg', '1');
                    sessionStorage.setItem('Msg', data.Message);

                    window.location.reload();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};