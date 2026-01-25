$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    // Initialize error texts as hidden
    $('.errorText').hide();

    // Initialize Summernote rich text editor for blog description
    $('textarea#txtDescription').summernote({
        placeholder: 'Enter full blog content',
        tabsize: 2,
        height: 300,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
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

    // Auto-generate slug from title
    $('#txtTitle').on('input', function() {
        if ($('#txtUniqueSlug').val() == '' || $('#txtUniqueSlug').data('auto-generated')) {
            var slug = $(this).val()
                .toLowerCase()
                .replace(/[^a-z0-9]+/g, '-')
                .replace(/^-+|-+$/g, '');
            $('#txtUniqueSlug').val(slug).data('auto-generated', true);
        }
    });

    // Mark slug as manually edited when user types in it
    $('#txtUniqueSlug').on('input', function() {
        $(this).data('auto-generated', false);
    });

    // Update Bootstrap file input label
    $('.custom-file-input').on('change', function() {
        var fileName = $(this).val().split('\\').pop();
        $(this).siblings('.custom-file-label').addClass('selected').html(fileName);
    });

    // Load blog categories for dropdown
    loadBlogCategories();
});

// Global variables for image
var BlogImage = "";
var fileExtensionImage = "";

function getBlogImageBase64() {
    var file1 = $("#BlogImage").prop("files")[0];

    if (!file1) {
        return false;
    }

    // The size of the file. 
    if (file1.size >= 2097152) {
        if (EnableSound == 'True') { document.getElementById('error').play(); }
        toastr.error('File too Big, please select a file less than 2mb');
        $("#BlogImage").val('');
        return false;
    }
    else {
        var reader = new FileReader();
        reader.readAsDataURL(file1);
        reader.onload = function () {
            BlogImage = reader.result;
            fileExtensionImage = '.' + file1.name.split('.').pop();
            
            $('#txtImage').val(BlogImage);
            $('#txtFileExtensionImage').val(fileExtensionImage);
            $('#blahBlogImage').attr('src', reader.result).show();
        };
        reader.onerror = function (error) {
            console.log(error);
            BlogImage = error;
        };
    }
}

// Function to load blog categories into dropdown
function loadBlogCategories() {
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];

    var obj = {
        CompanyId: CompanyId
    };

    $.ajax({
        url: cookieRoute.baseRoute + '/activecategories',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1 && res.Data && res.Data.BlogCategories) {
                var categories = res.Data.BlogCategories;
                var $dropdown = $('#ddlBlogCategory');
                
                // Clear existing options except the first one
                $dropdown.find('option:not(:first)').remove();
                
                // Add categories to dropdown
                $.each(categories, function(index, category) {
                    $dropdown.append($('<option></option>')
                        .attr('value', category.BlogCategoryId)
                        .text(category.CategoryName));
                });

                // Initialize Select2 if not already initialized
                if (!$dropdown.hasClass('select2-hidden-accessible')) {
                    $dropdown.select2({
                        placeholder: 'Select category',
                        allowClear: true
                    });
                }

                // Set selected value if editing
                var selectedCategoryId = $('#txtBlogCategoryId').val();
                if (selectedCategoryId && selectedCategoryId != '0') {
                    $dropdown.val(selectedCategoryId).trigger('change');
                }
            }
        },
        error: function () {
            console.error('Error loading blog categories');
        }
    });
}

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];
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

// Helper function to detect cookie type and base route
function getCookieAndRoute() {
    var cookieName = Cookies.get('adata') ? 'adata' : 'data';
    var baseRoute = cookieName === 'adata' ? '/adminblog' : '/blog';
    return { cookieName: cookieName, baseRoute: baseRoute };
}

var _PageIndex = 1;

function fetchList(PageIndex) {
    _PageIndex = PageIndex;
    var PageSize = $('#txtPageSize').val();
    var Search = $('#txtSearch').val();
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var obj = {
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        PageIndex: PageIndex,
        PageSize: parseInt(PageSize),
        Search: Search
    };
    
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/blogfetch',
        type: 'POST',
        data: obj,
        success: function (res) {
            $('#tblData').html(res);
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
        error: function () {
            $("#divLoading").hide();
            toastr.error('Error loading blog list');
        }
    });
}

function insert(i) {
    $('.errorText').hide();
    $('.divTitle_ctrl, .divDescription_ctrl').css('border', '');
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var blogCategoryId = $('#ddlBlogCategory').val();
    
    var obj = {
        Title: $('#txtTitle').val(),
        ShortDescription: $('#txtShortDescription').val(),
        Description: $('textarea#txtDescription').summernote('code'),
        Image: BlogImage,
        FileExtensionImage: fileExtensionImage,
        BlogCategoryId: blogCategoryId && blogCategoryId != '' ? parseInt(blogCategoryId) : 0,
        Taglist: $('#txtTaglist').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        MetaKeywords: $('#txtMetaKeywords').val(),
        UniqueSlug: $('#txtUniqueSlug').val(),
        IsActive: true,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    
    $.ajax({
        url: cookieRoute.baseRoute + '/insert',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                if (res.Data && res.Data.Errors) {
                    res.Data.Errors.forEach(function(error) {
                        $('#' + error.Id).text(error.Message).show();
                        $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                    });
                } else {
                    toastr.error(res.Message || 'Error saving blog');
                }
            }
        },
        error: function () {
            toastr.error('Error saving blog');
        }
    });
}

function update(i) {
    $('.errorText').hide();
    $('.divTitle_ctrl, .divDescription_ctrl').css('border', '');
    
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    var ModifiedBy = arr[2].split('=')[1];
    
    var blogCategoryId = $('#ddlBlogCategory').val();
    
    // For update, follow Items pattern:
    // - Send base64 + extension only when a new image is uploaded
    // - Otherwise send empty string so backend keeps existing image
    var imageValue = '';
    var fileExtensionValue = '';
    
    if (BlogImage && BlogImage != '') {
        // New image uploaded
        imageValue = BlogImage;
        fileExtensionValue = fileExtensionImage;
    }
    
    var obj = {
        BlogId: parseInt($('#txtBlogId').val()),
        Title: $('#txtTitle').val(),
        ShortDescription: $('#txtShortDescription').val(),
        Description: $('textarea#txtDescription').summernote('code'),
        Image: imageValue,
        FileExtensionImage: fileExtensionValue,
        BlogCategoryId: blogCategoryId && blogCategoryId != '' ? parseInt(blogCategoryId) : 0,
        Taglist: $('#txtTaglist').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        MetaKeywords: $('#txtMetaKeywords').val(),
        UniqueSlug: $('#txtUniqueSlug').val(),
        IsActive: $('#txtIsActive').val() === 'true',
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        ModifiedBy: ModifiedBy
    };
    
    $.ajax({
        url: cookieRoute.baseRoute + '/update',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                if (i == 1) {
                    window.location.href = cookieRoute.baseRoute + '/index';
                } else {
                    window.location.href = cookieRoute.baseRoute + '/add';
                }
            } else {
                if (res.Data && res.Data.Errors) {
                    res.Data.Errors.forEach(function(error) {
                        $('#' + error.Id).text(error.Message).show();
                        $('.' + error.Id + '_ctrl').css('border', '1px solid red');
                    });
                } else {
                    toastr.error(res.Message || 'Error updating blog');
                }
            }
        },
        error: function () {
            toastr.error('Error updating blog');
        }
    });
}

function Delete(BlogId, Title) {
    var r = confirm("This will delete \"" + Title + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var cookieRoute = getCookieAndRoute();
        var arr = Cookies.get(cookieRoute.cookieName).split('&');
        var CompanyId = arr[1].split('=')[1];
        var ModifiedBy = arr[2].split('=')[1];
        
        var det = {
            BlogId: BlogId,
            CompanyId: CompanyId,
            ModifiedBy: ModifiedBy
        };
        $("#divLoading").show();
        $.ajax({
            url: cookieRoute.baseRoute + '/delete',
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
                    fetchList(_PageIndex);
                }
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
}

function ActiveInactive(BlogId, IsActive) {
    var cookieRoute = getCookieAndRoute();
    var arr = Cookies.get(cookieRoute.cookieName).split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var det = {
        BlogId: BlogId,
        IsActive: IsActive,
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    $("#divLoading").show();
    $.ajax({
        url: cookieRoute.baseRoute + '/activeinactive',
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
                if (EnableSound == 'True') {document.getElementById('error').play();}
                toastr.error(data.Message);
            }
            else {
                if (EnableSound == 'True') {document.getElementById('success').play();}
                toastr.success(data.Message);
                fetchList(_PageIndex);
            }
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
}

