
var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

//$(document).keydown(function (event) {
//    if (event.keyCode == 123) { // Prevent F12
//        return false;
//    } else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) { // Prevent Ctrl+Shift+I        
//        return false;
//    }
//});

//$(document).on("contextmenu", function (e) {
//    e.preventDefault();
//});

function googleTranslateElementInit() {
    new google.translate.TranslateElement({ pageLanguage: '' }, 'google_translate_element');
    $('.goog-te-combo').addClass('form-control');
}

$(document).ready(function () {
    //if (Cookies.get('zoomLevel') != null) {
    //    document.body.style.zoom = Cookies.get('zoomLevel');
    //    $('#ddlZoomLevel').val(Cookies.get('zoomLevel'));
    //}
    //else {
    //    document.body.style.zoom = '90%';
    //    $('#ddlZoomLevel').val('90%');
    //}

    fetchShortCutKeySetting();

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
                                    if (value.Url == "" || value.Url == null) {
                                        if (value.Title == "Save") {
                                            $('#btnSave').click();
                                        }
                                        else if (value.Title == "Save & Add Another") {
                                            $('#btnSave_AddAnother').click();
                                        }
                                        else if (value.Title == "Update") {
                                            $('#btnUpdate').click();
                                        }
                                        else if (value.Title == "Update & Add Another") {
                                            $('#btnUpdate_AddAnother').click();
                                        }
                                        else if (value.Title == "Go Back") {
                                            $('#btnBack')[0].click()
                                        }
                                    }
                                    else {
                                        window.location.href = '/' + value.Url;
                                    }
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
});

function setZoomLevel() {
    var browser = checkBrowser();
    if (browser == "Firefox") {
        toastr.error('Firefox Browser does not support zoom option. Try pressing (ctrl & +) together for zoom in or (ctrl & -) together for zoom out. Please connect with support fo further details')
    }
    else {
        Cookies.set('zoomLevel', $('#ddlZoomLevel').val());
        document.body.style.zoom = $('#ddlZoomLevel').val();
    }
}

function checkBrowser() {
    if ((navigator.userAgent.indexOf("Opera") || navigator.userAgent.indexOf('OPR')) != -1) {
        return 'Opera';
    } else if (navigator.userAgent.indexOf("Edg") != -1) {
        return 'Edge';
    } else if (navigator.userAgent.indexOf("Chrome") != -1) {
        return 'Chrome';
    } else if (navigator.userAgent.indexOf("Safari") != -1) {
        return 'Safari';
    } else if (navigator.userAgent.indexOf("Firefox") != -1) {
        return 'Firefox';
    } else if ((navigator.userAgent.indexOf("MSIE") != -1) || (!!document.documentMode == true)) //IF IE > 10
    {
        return 'IE';
    } else {
        return 'unknown';
    }
}

function logout() {
    if (confirm('Are you sure you want to logout?')) {
        $('#divLoading').show();
        var det = {
        };
        $("#divLoading").show();
        $.ajax({
            url: '/login/logout',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    window.location.href = "/home";
                }

                $("#divLoading").hide();
            },
            error: function (data) {
                //scrollToTop();
                $("#divLoading").hide();
            }
        });
    }
};

function adminLogout() {
    if (confirm('Are you sure you want to logout?')) {
        $('#divLoading').show();
        var det = {
        };
        $("#divLoading").show();
        $.ajax({
            url: '/adminlogin/logout',
            datatype: "json",
            data: det,
            type: "post",
            success: function (data) {
                if (data.Status == 0) {
                    if (EnableSound == 'True') { document.getElementById('error').play(); }
                    toastr.error(data.Message);
                }
                else {
                    window.location.href = "/adminlogin";
                }

                $("#divLoading").hide();
            },
            error: function (data) {
                //scrollToTop();
                $("#divLoading").hide();
            }
        });
    }
};

function onlyNumberKey(evt) {
    if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
        evt.preventDefault();
    }
}

function onlyDecimalKey(evt) {
    // Only ASCII character in that range allowed
    var ASCIICode = (evt.which) ? evt.which : evt.keyCode
    if (ASCIICode > 32 && (ASCIICode < 48 || ASCIICode > 57) && ASCIICode != 46)
        return false;
    return true;
}

function DateFormat(date) {
    date = new Date(date);
    var dateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
        .toISOString()
        .split("T")[0];
    return dateString;
}

function DateTimeFormat(date) {
    date = new Date(date);
    var dateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000))
        .toISOString()
        .split("T");

    var time = dateString[1].split(":");
    return dateString[0] + "T" + time[0] + ":" + time[1];
}

function getFormattedDate(d) {
    date = new Date(d)
    var dd = date.getDate();
    var mm = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    if (dd < 10) { dd = '0' + dd }
    if (mm < 10) { mm = '0' + mm };
    return d = dd + '/' + mm + '/' + yyyy;
    return date;
}

function getFormattedDateTime(d) {
    date = new Date(d)
    var dd = date.getDate();
    var mm = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    var hh = date.getHours();
    var min = date.getMinutes();
    if (dd < 10) { dd = '0' + dd }
    if (mm < 10) { mm = '0' + mm };
    return d = dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + min + ' tt';
}

//$("body").click(function () {
//    if ($(".divShortcutKeys").css("display") == "block") {
//        $(".divShortcutKeys").css('display', 'none');
//    }
//});

/*document.body.style.zoom = 1.0*/

function formatDynamicDate(dateStr, format) {
    var timestampMatch = /\/Date\((\d+)\)\//.exec(dateStr); // Match timestamp format

    var date;
    if (timestampMatch) {
        date = new Date(parseInt(timestampMatch[1])); // Convert timestamp to date
    } else {
        date = new Date(dateStr);
    }

    if (isNaN(date)) return dateStr; // Return original if parsing fails

    var day = String(date.getDate()).padStart(2, '0');
    var month = String(date.getMonth() + 1).padStart(2, '0');
    var year = date.getFullYear();

    switch (format) {
        case "DD-MM-YYYY": return `${day}-${month}-${year}`;
        case "MM-DD-YYYY": return `${month}-${day}-${year}`;
        case "YYYY-MM-DD": return `${year}-${month}-${day}`;
        case "YYYY-DD-MM": return `${year}-${day}-${month}`;
        case "YYYY-MM": return `${year}-${month}`;
        case "MM-YYYY": return `${month}-${year}`;
        case "DD/MM/YYYY": return `${day}/${month}/${year}`;
        case "MM/DD/YYYY": return `${month}/${day}/${year}`;
        case "YYYY/MM/DD": return `${year}/${month}/${day}`;
        case "YYYY/DD/MM": return `${year}/${day}/${month}`;
        case "YYYY/MM": return `${year}/${month}`;
        case "MM/YYYY": return `${month}/${year}`;
        case "DD MM YYYY": return `${day} ${month} ${year}`;
        case "MM DD YYYY": return `${month} ${day} ${year}`;
        case "YYYY MM DD": return `${year} ${month} ${day}`;
        case "YYYY DD MM": return `${year} ${day} ${month}`;
        case "YYYY MM": return `${year} ${month}`;
        case "MM YYYY": return `${month} ${year}`;
        default: return `${year}-${month}-${day}`; // Fallback
    }
}

function exportToExcel(fileName) {
    $('.hide').remove();
    TableToExcel.convert(document.getElementById("tblData"), {
        name: fileName.split('| ')[1] + ".xlsx",
        sheet: {
            name: "Sheet1"
        }
    });
}

function exportToCsv(fileName) {
    var table = document.getElementById("tblData");
    var rows = table.querySelectorAll("tr");
    var csvContent = [];

    rows.forEach(row => {
        var cols = row.querySelectorAll("td, th");
        var csvRow = [];
        cols.forEach(col => {
            csvRow.push('"' + col.innerText.replace(/"/g, '""') + '"'); // Escape double quotes
        });
        csvContent.push(csvRow.join(","));
    });

    var csvString = csvContent.join("\n");
    var blob = new Blob([csvString], { type: "text/csv" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName.split('| ')[1] + ".csv";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function exportToPdf(fileName) {
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

            pdfMake.createPdf(docDefinition).download(fileName.split('| ')[1] + ".pdf");
            $('.responsive-table').css('height', '400px');
            $('.hide').show();
            $('.hidden').hide();
        }
    });
}

function printDiv(pageName) {

    $('.noPrint').hide();
    window.print();
    $('.noPrint').show();
}