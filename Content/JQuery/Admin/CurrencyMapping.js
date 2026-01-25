$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });
    $('.select2').select2();
});

var EnableSound = Cookies.get('aSystemSetting').split('&')[4].split('=')[1];

var interval = null;

if (sessionStorage.getItem("showMsg") == '1') {
    interval = setInterval(playSound, 100);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('success').play(); }
    toastr.success(sessionStorage.getItem("Msg"));
    sessionStorage.removeItem("showMsg");
    sessionStorage.removeItem("Msg");
    clearInterval(interval);
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/currencyMappingFetch',
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

function insert(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        CurrencyId: $('#ddlCurrency').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/currencyMappingInsert',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/adminsettings/currencymappinglist";
                }
                else {
                    window.location.reload();
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        UserCurrencyMapId: window.location.href.split('=')[1],
        CurrencyId: $('#ddlCurrency').val(),
        ExchangeRate: $('#txtExchangeRate').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/currencyMappingUpdate',
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
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                data.Errors.forEach(function (res) {
                    $('#' + res.Id).show();
                    $('#' + res.Id).text(res.Message);
                });
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                window.location.href = "/adminsettings/currencymappinglist";
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(UserCurrencyMapId, IsActive) {
    var det = {
        UserCurrencyMapId: UserCurrencyMapId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/adminsettings/currencyMappingActiveInactive',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            if (data == "True") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }
            else if (data == "False") {
                $('#subscriptionExpiryModal').modal('toggle');
                $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + Cookies.get('data').split('&')[9].split('=')[1]);
                fetchList(_PageIndex);
                return
            }

            if (data.Status == 0) {
                if (EnableSound == 'True') { document.getElementById('error').play(); }
                toastr.error(data.Message);
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
};

function Delete(UserCurrencyMapId, currencyName) {
    var r = confirm("This will delete \"" + currencyName +"\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            UserCurrencyMapId: UserCurrencyMapId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/currencyMappingdelete',
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

function exportToExcel() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Currency.xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
    fetchList();
}

function exportToCsv() {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: "Currency.csv",
        sheet: {
            name: "Sheet1"
        }
    });
    fetchList();
}

function exportToPdf() {
    $('.hidden').show();
    $('.hide').hide();
    $('.responsive-table').css('height', '100%');
    html2canvas($('#tblData')[0], {
        onrendered: function (canvas) {
            var data = canvas.toDataURL();
            var docDefinition = {
                content: [{
                    image: data,
                    width: 500
                }]
            };

            pdfMake.createPdf(docDefinition).download("Currency.pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function setCurrencyName() {
    $('#txtCurrency').text($("#ddlCurrency").children("option").filter(":selected").text());
}