$(function () {
    $('#tblSeoSettings').DataTable({
        "responsive": true,
        "lengthChange": true,
        "autoWidth": false,
        "order": [[0, "asc"]]
    });
    
    // Check for success message from sessionStorage
    var EnableSound = Cookies.get('aSystemSetting') ? Cookies.get('aSystemSetting').split('&')[4].split('=')[1] : 'False';
    if (sessionStorage.getItem("showMsg") == '1') {
        if (EnableSound == 'True') { 
            if (document.getElementById('success')) document.getElementById('success').play(); 
        }
        toastr.success(sessionStorage.getItem("Msg"));
        sessionStorage.removeItem("showMsg");
        sessionStorage.removeItem("Msg");
    }
});

var EnableSound = Cookies.get('aSystemSetting') ? Cookies.get('aSystemSetting').split('&')[4].split('=')[1] : 'False';

function deleteSeoSettings(id) {
    var r = confirm("This will delete the SEO settings permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        $("#divLoading").show();
        $.ajax({
            url: '/adminsettings/seosettingsdelete',
            type: 'POST',
            data: { PageSeoSettingsId: id },
            success: function(response) {
                $("#divLoading").hide();
                
                if (response == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + (Cookies.get('data') ? Cookies.get('data').split('&')[9].split('=')[1] : ''));
                    return;
                }
                else if (response == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + (Cookies.get('data') ? Cookies.get('data').split('&')[9].split('=')[1] : ''));
                    return;
                }
                
                if (response.Status == 0) {
                    if (EnableSound == 'True') { 
                        if (document.getElementById('error')) document.getElementById('error').play(); 
                    }
                    toastr.error(response.Message);
                }
                else {
                    if (EnableSound == 'True') { 
                        if (document.getElementById('success')) document.getElementById('success').play(); 
                    }
                    toastr.success(response.Message);
                    location.reload();
                }
            },
            error: function(xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                toastr.error('An error occurred while deleting.');
            }
        });
    }
}

$(document).ready(function() {
    // Initialize Select2
    $('.select2').select2();

    var EnableSound = Cookies.get('aSystemSetting') ? Cookies.get('aSystemSetting').split('&')[4].split('=')[1] : 'False';

    $('#seoSettingsForm').on('submit', function(e) {
        e.preventDefault();
        
        // Clear previous errors
        $('.errorText').hide();
        $('[style*="border: 2px"]').css('border', '');
        
        var formData = {
            PageSeoSettingsId: $('#PageSeoSettingsId').val(),
            PageIdentifier: $('#PageIdentifier').val(),
            PageTitle: $('#PageTitle').val(),
            MetaDescription: $('#MetaDescription').val(),
            MetaKeywords: $('#MetaKeywords').val(),
            OgTitle: $('#OgTitle').val(),
            OgDescription: $('#OgDescription').val(),
            OgImage: $('#OgImage').val(),
            OgUrl: $('#OgUrl').val(),
            TwitterTitle: $('#TwitterTitle').val(),
            TwitterDescription: $('#TwitterDescription').val(),
            TwitterImage: $('#TwitterImage').val(),
            CanonicalUrl: $('#CanonicalUrl').val(),
            IsActive: $('#IsActive').is(':checked')
        };
        
        $("#divLoading").show();
        
        $.ajax({
            url: '/adminsettings/seosettingssave',
            type: 'POST',
            data: formData,
            success: function(response) {
                $("#divLoading").hide();
                
                if (response == "True") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your free trial has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please buy plan to continue billing with ' + (Cookies.get('data') ? Cookies.get('data').split('&')[9].split('=')[1] : ''));
                    return;
                }
                else if (response == "False") {
                    $('#subscriptionExpiryModal').modal('toggle');
                    $('#lblsubscriptionExpiryMsg1').html('<i class="icon fas fa-exclamation-triangle"></i> Your subscription plan has expired!!');
                    $('#lblsubscriptionExpiryMsg2').html('Please renew plan to continue billing with ' + (Cookies.get('data') ? Cookies.get('data').split('&')[9].split('=')[1] : ''));
                    return;
                }

                if (response.Status == 0) {
                    if (EnableSound == 'True') { 
                        if (document.getElementById('error')) document.getElementById('error').play(); 
                    }
                    toastr.error(response.Message);
                }
                else if (response.Status == 2) {
                    if (EnableSound == 'True') { 
                        if (document.getElementById('error')) document.getElementById('error').play(); 
                    }
                    toastr.error('Invalid inputs, check and try again !!');
                    if (response.Errors && response.Errors.length > 0) {
                        response.Errors.forEach(function (res) {
                            $('#' + res.Id).show();
                            $('#' + res.Id).text(res.Message);
                            
                            // Add error border to the input field
                            var fieldId = res.Id.replace('div', '');
                            var $field = $('#' + fieldId);
                            if ($field.length > 0) {
                                if ($field.hasClass('select2-hidden-accessible')) {
                                    $field.closest('.form-group').find('.select2-container .select2-selection').css('border', '2px solid #dc3545');
                                } else {
                                    $field.css('border', '2px solid #dc3545');
                                }
                            }
                        });
                    }
                }
                else {
                    if (EnableSound == 'True') { 
                        if (document.getElementById('success')) document.getElementById('success').play(); 
                    }
                    sessionStorage.setItem('showMsg', '1');
                    sessionStorage.setItem('Msg', response.Message);
                    window.location.href = '/adminsettings/seosettings';
                }
            },
            error: function(xhr) {
                $("#divLoading").hide();
                if (EnableSound == 'True') { 
                    if (document.getElementById('error')) document.getElementById('error').play(); 
                }
                toastr.error('An error occurred while saving.');
            }
        });
    });
});

