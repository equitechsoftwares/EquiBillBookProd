function updateSystem() {
    var det = {
        DatatablePageEntries: $('#ddlDatatablePageEntries').val(),
        ShowHelpText: $('#chkShowHelpText').is(':checked'),
        EnableDarkMode: $('#chkEnableDarkMode').is(':checked'),
        FixedHeader: $('#chkFixedHeader').is(':checked'),
        FixedFooter: $('#chkFixedFooter').is(':checked'),
        EnableSound: $('#chkEnableSound').is(':checked'),
        CollapseSidebar: $('#chkCollapseSidebar').is(':checked'),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/systemsettings/SystemUpdate',
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
                if ($('#chkEnableDarkMode').is(':checked')) {
                    $('body').addClass('dark-mode');
                    $('.divShortcutKeys').removeClass('control-sidebar-light');
                    $('.divShortcutKeys').addClass('control-sidebar-dark');
                }
                else {
                    $('body').removeClass('dark-mode');
                    $('.divShortcutKeys').addClass('control-sidebar-light');
                    $('.divShortcutKeys').removeClass('control-sidebar-dark');
                }

                if ($('#chkFixedHeader').is(':checked')) {
                    $('body').addClass('layout-navbar-fixed');
                }
                else {
                    $('body').removeClass('layout-navbar-fixed');
                }

                if ($('#chkFixedFooter').is(':checked')) {
                    $('body').addClass('layout-footer-fixed');
                }
                else {
                    $('body').removeClass('layout-footer-fixed');
                }

                if ($('#chkEnableSound').is(':checked')) {
                    EnableSound = 'True';
                }
                else {
                    EnableSound = 'False';
                }

                if ($('#chkCollapseSidebar').is(':checked')) {
                    $('body').addClass('sidebar-collapse');
                }
                else {
                    $('body').removeClass('sidebar-collapse');
                }

                if (EnableSound == 'True') { document.getElementById('success').play(); }
                toastr.success(data.Message);
            }

        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};