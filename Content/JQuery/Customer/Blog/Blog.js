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
});

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

var _PageIndex = 1;

function fetchList(PageIndex) {
    _PageIndex = PageIndex;
    var PageSize = $('#txtPageSize').val();
    var Search = $('#txtSearch').val();
    
    var arr = Cookies.get('data').split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    
    var obj = {
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        PageIndex: PageIndex,
        PageSize: parseInt(PageSize),
        Search: Search
    };
    
    $.ajax({
        url: '/blog/blogfetch',
        type: 'POST',
        data: obj,
        success: function (res) {
            $('#tblData').html(res);
        },
        error: function () {
            toastr.error('Error loading blog list');
        }
    });
}

function insert() {
    $('.errorText').hide();
    $('.divTitle_ctrl, .divDescription_ctrl').css('border', '');
    
    var arr = Cookies.get('data').split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    var UserType = arr[0].split('=')[1];
    var Token = arr[3].split('=')[1];
    var Id = arr[2].split('=')[1];
    
    var obj = {
        Title: $('#txtTitle').val(),
        ShortDescription: $('#txtShortDescription').val(),
        Description: $('#txtDescription').val(),
        Image: $('#txtImage').val(),
        Category: $('#txtCategory').val(),
        Taglist: $('#txtTaglist').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        MetaKeywords: $('#txtMetaKeywords').val(),
        UniqueSlug: $('#txtUniqueSlug').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        CompanyId: CompanyId,
        AddedBy: AddedBy
    };
    
    $.ajax({
        url: '/api/Blog/InsertBlog',
        type: 'POST',
        contentType: 'application/json',
        headers: {
            'Authorization': 'Basic ' + btoa(UserType + '_' + Id + ':' + Token)
        },
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                window.location.href = '/blog/index';
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

function update() {
    $('.errorText').hide();
    $('.divTitle_ctrl, .divDescription_ctrl').css('border', '');
    
    var arr = Cookies.get('data').split('&');
    var CompanyId = arr[1].split('=')[1];
    var AddedBy = arr[2].split('=')[1];
    var ModifiedBy = arr[2].split('=')[1];
    var UserType = arr[0].split('=')[1];
    var Token = arr[3].split('=')[1];
    var Id = arr[2].split('=')[1];
    
    var obj = {
        BlogId: parseInt($('#txtBlogId').val()),
        Title: $('#txtTitle').val(),
        ShortDescription: $('#txtShortDescription').val(),
        Description: $('#txtDescription').val(),
        Image: $('#txtImage').val(),
        Category: $('#txtCategory').val(),
        Taglist: $('#txtTaglist').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        MetaKeywords: $('#txtMetaKeywords').val(),
        UniqueSlug: $('#txtUniqueSlug').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        CompanyId: CompanyId,
        AddedBy: AddedBy,
        ModifiedBy: ModifiedBy
    };
    
    $.ajax({
        url: '/api/Blog/UpdateBlog',
        type: 'POST',
        contentType: 'application/json',
        headers: {
            'Authorization': 'Basic ' + btoa(UserType + '_' + Id + ':' + Token)
        },
        data: JSON.stringify(obj),
        success: function (res) {
            if (res.Status == 1) {
                sessionStorage.setItem("showMsg", "1");
                sessionStorage.setItem("Msg", res.Message);
                window.location.href = '/blog/index';
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
    if (confirm('Are you sure you want to delete "' + Title + '"?')) {
        var arr = Cookies.get('data').split('&');
        var CompanyId = arr[1].split('=')[1];
        var ModifiedBy = arr[2].split('=')[1];
        var UserType = arr[0].split('=')[1];
        var Token = arr[3].split('=')[1];
        var Id = arr[2].split('=')[1];
        
        var obj = {
            BlogId: BlogId,
            CompanyId: CompanyId,
            ModifiedBy: ModifiedBy
        };
        
        $.ajax({
            url: '/api/Blog/DeleteBlog',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'Authorization': 'Basic ' + btoa(UserType + '_' + Id + ':' + Token)
            },
            data: JSON.stringify(obj),
            success: function (res) {
                if (res.Status == 1) {
                    sessionStorage.setItem("showMsg", "1");
                    sessionStorage.setItem("Msg", res.Message);
                    fetchList(_PageIndex);
                } else {
                    toastr.error(res.Message || 'Error deleting blog');
                }
            },
            error: function () {
                toastr.error('Error deleting blog');
            }
        });
    }
}

function ActiveInactive(BlogId, IsActive) {
    var arr = Cookies.get('data').split('&');
    var CompanyId = arr[1].split('=')[1];
    var ModifiedBy = arr[2].split('=')[1];
    var UserType = arr[0].split('=')[1];
    var Token = arr[3].split('=')[1];
    var Id = arr[2].split('=')[1];
    
    // First get the blog
    var getObj = {
        BlogId: BlogId,
        CompanyId: CompanyId
    };
    
    $.ajax({
        url: '/api/Blog/Blog',
        type: 'POST',
        contentType: 'application/json',
        headers: {
            'Authorization': 'Basic ' + btoa(UserType + '_' + Id + ':' + Token)
        },
        data: JSON.stringify(getObj),
        success: function (res) {
            if (res.Status == 1 && res.Data && res.Data.Blog) {
                var blog = res.Data.Blog;
                var updateObj = {
                    BlogId: BlogId,
                    Title: blog.Title,
                    ShortDescription: blog.ShortDescription || '',
                    Description: blog.Description,
                    Image: blog.Image || '',
                    Category: blog.Category || '',
                    Taglist: blog.Taglist || '',
                    MetaTitle: blog.MetaTitle || blog.Title,
                    MetaDescription: blog.MetaDescription || blog.ShortDescription || '',
                    MetaKeywords: blog.MetaKeywords || '',
                    UniqueSlug: blog.UniqueSlug,
                    IsActive: IsActive,
                    CompanyId: CompanyId,
                    AddedBy: blog.AddedBy,
                    ModifiedBy: ModifiedBy
                };
                
                $.ajax({
                    url: '/api/Blog/UpdateBlog',
                    type: 'POST',
                    contentType: 'application/json',
                    headers: {
                        'Authorization': 'Basic ' + btoa(UserType + '_' + Id + ':' + Token)
                    },
                    data: JSON.stringify(updateObj),
                    success: function (updateRes) {
                        if (updateRes.Status == 1) {
                            fetchList(_PageIndex);
                        } else {
                            toastr.error(updateRes.Message || 'Error updating blog status');
                        }
                    },
                    error: function () {
                        toastr.error('Error updating blog status');
                    }
                });
            } else {
                toastr.error('Error fetching blog details');
            }
        },
        error: function () {
            toastr.error('Error fetching blog details');
        }
    });
}

