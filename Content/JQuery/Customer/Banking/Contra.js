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

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('#txtDateRange').daterangepicker({
        locale: {
            format: DateFormat.toUpperCase()
        }
    });

    $('#_PaymentDate').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() + ' ' + TimeFormat, defaultDate: new Date() });
    $('#_PaymentDate').addClass('notranslate');
});

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

var _PageIndex = 1;

var AttachDocument = "", FileExtensionAttachDocument = "";
var UploadPath = "", fileExtension = "";
var BatchNo = Math.floor(Math.random() * 26) + Date.now();
var _excelJson = [];

function fetchList(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        FromAccountId: $('#ddlFromAccount').val(),
        ToAccountId: $('#ddlToAccount').val(),
        BranchId: $('#ddlBranch').val(),
        FromDate: moment($('#txtDateRange').val().split(' ')[0], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        ToDate: moment($('#txtDateRange').val().split(' ')[2], DateFormat.toUpperCase()).format('DD-MM-YYYY'),
        Type: $('#ddlType').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraFetch',
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
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        FromAccountId: $('#ddlFromAccount').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        Type: $('#ddlType').val(),
        ToAccountId: $('#ddlToAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraInsert',
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
                if (i == 1) {
                    window.location.href = "/banking/contra";
                }
                else {
                    window.location.href = "/banking/contraadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function update(i) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var TimeFormat = Cookies.get('BusinessSetting').split('&')[1].split('=')[1].replace('tt', 'a').replace('tt', 'a');

    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        ContraId: window.location.href.split('=')[1],
        FromAccountId: $('#ddlFromAccount').val(),
        IsActive: true,
        IsDeleted: false,
        Notes: $('#txtNotes').val(),
        Amount: $('#txtAmount').val(),
        PaymentDate: moment($("#txtPaymentDate").val(), DateFormat.toUpperCase() + ' ' + TimeFormat + ' a').format('YYYY-MM-DDTHH:mm'),//$('#txtPaymentDate').val(),
        PaymentTypeId: $('#ddlPaymentType').val(),
        AttachDocument: AttachDocument,
        FileExtensionAttachDocument: FileExtensionAttachDocument,
        Type: $('#ddlType').val(),
        ToAccountId: $('#ddlToAccount').val(),
        ReferenceNo: $('#txtReferenceNo').val(),
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraUpdate',
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
                if (i == 1) {
                    window.location.href = "/banking/contra";
                }
                else {
                    window.location.href = "/banking/contraadd";
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

function Delete(ContraId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            ContraId: ContraId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/banking/ContraDelete',
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
                    //$('#tr_' + ContraId).remove();
                    fetchList();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function View(ContraId) {
    var det = {
        ContraId: ContraId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/ContraView',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#ViewModal').modal('toggle');
            $("#divView").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function toggleType() {
    if ($('#ddlType').val() == "Deposit") {
        $('.divpaymentMethod').show();
    }
    else {
        $('.divpaymentMethod').hide();
    }
}

function copyTag(tag) {
    navigator.clipboard.writeText(tag);
    toastr.success("Copied the text: " + tag);
}

function uploadExcel() {
    //var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xlsx|.xls)$/;
    var regex = (/\.(xlsx|xls|xlsm|csv)$/i);
    /*Checks whether the file is a valid excel or csv file*/
    if (regex.test($("#excelfile").val().toLowerCase())) {
        var xlsxflag = false; /*Flag for checking whether excel is .xls format or .xlsx format*/
        var csvflag = false; /*Flag for checking whether file is .csv format*/
        
        if ($("#excelfile").val().toLowerCase().indexOf(".xlsx") > 0) {
            xlsxflag = true;
        } else if ($("#excelfile").val().toLowerCase().indexOf(".csv") > 0) {
            csvflag = true;
        }
        /*Checks whether the browser supports HTML5*/
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;
                
                if (csvflag) {
                    /*Parse CSV file using PapaParse*/
                    Papa.parse(data, {
                        header: true,
                        skipEmptyLines: true,
                        complete: function (results) {
                            if (results.errors.length > 0) {
                                console.error("CSV parsing errors:", results.errors);
                            }
                            _excelJson = results.data;
                            isExcelUpload = true;
                            $('#exceltable').show();
                        },
                        error: function (error) {
                            if (EnableSound == 'True') { document.getElementById('error').play(); }
                            toastr.error('Error parsing CSV file: ' + error.message);
                        }
                    });
                } else {
                    /*Converts the excel data in to object*/
                    if (xlsxflag) {
                        // Use newer XLSX library (0.18.5) with ArrayBuffer
                        var workbook = XLSX.read(data, { type: 'array' });
                    }
                    else {
                        var workbook = XLS.read(data, { type: 'binary' });
                    }
                    /*Gets all the sheetnames of excel in to a variable*/
                    var sheet_name_list = workbook.SheetNames;

                    var cnt = 0; /*This is used for restricting the script to consider only first sheet of excel*/
                    sheet_name_list.forEach(function (y) { /*Iterate through all sheets*/
                        /*Convert the cell value to Json*/

                        if (xlsxflag) {
                            var exceljson = XLSX.utils.sheet_to_json(workbook.Sheets[y], {raw: false});
                        }
                        else {
                            var exceljson = XLS.utils.sheet_to_row_object_array(workbook.Sheets[y]);
                        }
                        
                        if (exceljson.length > 0 && cnt == 0) {
                            _excelJson = exceljson;
                            cnt++;
                        }
                    });
                    isExcelUpload = true;
                    $('#exceltable').show();
                }
            }
            if (csvflag) {/*If file is .csv extension than read as text*/
                reader.readAsText($("#excelfile")[0].files[0]);
            }
            else if (xlsxflag) {/*If excel file is .xlsx extension than creates a Array Buffer from excel*/
                reader.readAsArrayBuffer($("#excelfile")[0].files[0]);
            }
            else {
                reader.readAsBinaryString($("#excelfile")[0].files[0]);
            }
        }
        else {
            if (EnableSound == 'True') { document.getElementById('error').play(); }
            toastr.error('Sorry! Your browser does not support HTML5!');
        }
    }
    else {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('Please upload a valid Excel (.xlsx, .xls, .xlsm) or CSV (.csv) file!');
    }
}

function BulkInsert(i) {
    $('.errorText').hide();
    $('#divErrorMsg').hide();
    if (isExcelUpload == false) {
        if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
        $('#divExcel').show();
        $('#divExcel').text('Upload Excel first');
        return;
    }

    $("#divLoading").show(); $("#divProgressBar").show();

    myInterval = setInterval(fetchBulkInsertProgress, 10000);

    var det = {
        BatchNo: BatchNo,
        ContraImports: _excelJson
    }
    $.ajax({
        url: '/banking/ImportContra',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {

            $("#divLoading").hide();
            $("#divProgressBar").hide();
            clearInterval(myInterval);

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
                $("#excelfile").val(null);
            }
            else if (data.Status == 2) {
                if (EnableSound == 'True') { document.getElementById('error').play(); } toastr.error('Invalid inputs, check and try again !!');
                $('#divErrorMsg').show();
                //$('#divExcel').show();
                var errorMsg = '';
                data.Errors.forEach(function (res) {
                    errorMsg = errorMsg + res.Message + '</br>';
                });

                //$('#divExcel').html(errorMsg);
                $('#txtErrorMsg').html(errorMsg);
                $("#excelfile").val(null);
                _excelJson = [];
                isExcelUpload = false;
                //$("html, body").animate({ scrollTop: 0 }, "slow");
            }
            else {
                sessionStorage.setItem('showMsg', '1');
                sessionStorage.setItem('Msg', data.Message);
                if (i == 1) {
                    window.location.href = "/banking/Contra";
                }
                else {
                    window.location.reload();
                }
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
            $("#divProgressBar").hide();
            clearInterval(myInterval);
        }
    });
}

function fetchBulkInsertProgress() {
    var det = {
        BatchNo: BatchNo
    };
    $.ajax({
        url: '/banking/ContraCountByBatch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $('#divProgressBar .progress span').text(parseInt((data.Data.TotalCount / _excelJson.length) * 100) + '% Complete (success)');
            $('#divProgressBar .progress .progress-bar').css('width', parseInt((data.Data.TotalCount / _excelJson.length) * 100) + '%');
        },
        error: function (xhr) {

        }
    });
}

function FetchOtherSoftwareImport(PageIndex) {
    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    var det = {

    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/banking/OtherSoftwareImportFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#tblOtherSoftwareImport").html(data);
            $("#divLoading").hide();
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function DeleteOtherSoftwareImport(OtherSoftwareImportId) {
    var r = confirm("Are you sure you want to delete?");
    if (r == true) {
        var det = {
            OtherSoftwareImportId: OtherSoftwareImportId
        }
        $("#divLoading").show();
        $.ajax({
            url: '/banking/OtherSoftwareImportDelete',
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
                    document.getElementById('success').play();
                    toastr.success(data.Message);
                    FetchOtherSoftwareImport();
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}
function getExcelBase64() {
    var file1 = $("#otherSoftwareExcelfile").prop("files")[0];

    var reader = new FileReader();
    reader.readAsDataURL(file1);
    reader.onload = function () {
        UploadPath = reader.result;
        fileExtension = '.' + file1.name.split('.').pop();
    };
    reader.onerror = function (error) {
        console.log(error);
        UploadPath = error;
    };
}
function InsertOtherSoftwareImport() {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    

    var det = {
        UploadPath: UploadPath,
        FileExtension: fileExtension,
        IsActive: true,
        IsDeleted: false,
    };
    $("#divLoading").show();
    $.ajax({
        url: '/banking/OtherSoftwareImportInsert',
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
                $("#otherSoftwareExcelfile").val(null);
                UploadPath = "";
                fileExtension = "";
                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
                FetchOtherSoftwareImport();
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function getAttachDocumentBase64() {
    var file1 = $("#AttachDocument").prop("files")[0];

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('please select a file less than 2mb');
        $("#AttachDocument").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            AttachDocument = reader.result;
            FileExtensionAttachDocument = '.' + file1.name.split('.').pop();

            $('#blahAttachDocument').attr('src', reader.result);
        };
        reader.onerror = function (error) {
            console.log(error);
            file = error;
        };
    }
}