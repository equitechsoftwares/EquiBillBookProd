
$(function () {
    $(document).on('click', '[data-toggle="lightbox"]', function (event) {
        event.preventDefault();
        $(this).ekkoLightbox({
            alwaysShowClose: true
        });
    });

    $('.filter-container').filterizr({ gutterPixels: 3 });
    $('.btn[data-filter]').on('click', function () {
        $('.btn[data-filter]').removeClass('active');
        $(this).addClass('active');
    });
})

function addNew() {
    $('.lblTitle').text('Predefined Templates');
    $('.divPredefinedTemplates').show();
    $('.divEditTemplate').hide();
}

function editTheme() {
    $('.lblTitle').text('Template Settings');
    $('.divPredefinedTemplates').hide();
    $('.divEditTemplate').show();
}