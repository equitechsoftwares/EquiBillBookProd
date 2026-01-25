$(function () {
    $('#tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });

    var DateFormat = Cookies.get('BusinessSetting').split('&')[0].split('=')[1];
    $('#_ValidFrom').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_ValidTo').datetimepicker({ widgetPositioning: { horizontal: 'auto', vertical: 'bottom' }, format: DateFormat.toUpperCase() });
    $('#_ValidFrom').addClass('notranslate');
    $('#_ValidTo').addClass('notranslate');

    $("[data-toggle=popover]").popover({
        html: true,
        trigger: "hover",
        placement: 'auto',
    });

    function toggleAddToCartMode(forceHide) {
        var wrapper = $('#divAddToCartModeWrapper');
        if (!wrapper.length) { return; }

        if (forceHide === true || !$('#chkEnableAddToCart').is(':checked')) {
            wrapper.stop(true, true).slideUp(150);
            $('#ddlAddToCartMode').val('');
            $('#ddlAddToCartMode').prop('disabled', true);
            $('#divAddToCartMode').hide();
            $('.divAddToCartMode_ctrl').css('border', '');
        } else {
            wrapper.stop(true, true).slideDown(150);
            $('#ddlAddToCartMode').prop('disabled', false);
        }
    }

    $('#chkEnableAddToCart').on('change', function () { toggleAddToCartMode(); });
    toggleAddToCartMode();

    $('.select2').select2();

    var $branchDropdown = $('#ddlBranch');
    var $catalogueItems = $('#ddlCatalogueItems');
    var preselectedItemsAttr = $catalogueItems.attr('data-preselected-items');
    if (typeof preselectedItemsAttr === 'string' && preselectedItemsAttr.length > 0) {
        initialCatalogueItemPairs = preselectedItemsAttr.split(',').map(function (value) {
            return (value || '').toString().trim();
        }).filter(function (value) {
            return value.length > 0;
        });
    }

    if ($branchDropdown.length && $catalogueItems.length) {
        var initialBranchId = parseInt($branchDropdown.data('initial-branch'), 10);
        var hasInitialItems = $catalogueItems.find('option').length > 0;

        if (!isNaN(initialBranchId) && initialBranchId > 0 && hasInitialItems) {
            $catalogueItems.data('loaded-branch', initialBranchId);
            initialCatalogueItemPairs = [];
        }
        else if (!isNaN(initialBranchId) && initialBranchId > 0 && !hasInitialItems) {
            loadCatalogueItemsForBranch(initialBranchId, initialCatalogueItemPairs);
        }
        else if (isNaN(initialBranchId) || initialBranchId === 0) {
            clearCatalogueItems();
        }

        $branchDropdown.on('change', function () {
            loadCatalogueItemsForBranch(getSelectedBranchId());
        });
    }

    updateCatalogueScopeVisibility();
    $('#ddlCatalogueType').on('change', function () {
        updateCatalogueScopeVisibility();
        resetCatalogueScopeErrors();
    });

    // Initialize error texts as hidden
    $('.errorText').hide();

    // Auto-generate slug from catalogue name
    $('#txtCatalogueName').on('input', function() {
        if ($('#txtSlug').val() == '' || $('#txtSlug').data('auto-generated')) {
            var slug = $(this).val()
                .toLowerCase()
                .replace(/[^a-z0-9]+/g, '-')
                .replace(/^-+|-+$/g, '');
            $('#txtSlug').val(slug).data('auto-generated', true);
        }
    });

    // Mark slug as manually edited when user types in it
    $('#txtSlug').on('input', function() {
        $(this).data('auto-generated', false);
    });

    // Handle Never Expires checkbox
    $('#chkNeverExpires').on('change', function() {
        if ($(this).is(':checked')) {
            $('#txtValidTo').val('').prop('disabled', true);
        } else {
            $('#txtValidTo').prop('disabled', false);
        }
    });

    if ($('#chkNeverExpires').is(':checked')) {
        $('#chkNeverExpires').trigger('change');
    }
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var interval = null;
var catalogueItemsRequest = null;
var initialCatalogueItemPairs = [];

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

function getSelectedBranchId() {
    var selected = $('#ddlBranch').val();

    if (Array.isArray(selected)) {
        selected = selected.length > 0 ? selected[0] : null;
    }

    if (selected === null || selected === undefined || selected === '' || selected === '0') {
        return 0;
    }

    var parsed = parseInt(selected, 10);

    if (isNaN(parsed)) {
        return 0;
    }

    return parsed;
}

function clearCatalogueItems() {
    var select = $('#ddlCatalogueItems');
    if (!select.length) { return; }
    select.empty();
    select.val(null).trigger('change');
    select.data('loaded-branch', 0);
}

function buildCatalogueItemDisplayName(item) {
    if (!item) { return ''; }
    var name = item.ItemName || '';
    if (item.VariationName && item.VariationName.trim().length > 0) {
        name += ' - ' + item.VariationName;
    }
    if (item.SKU && item.SKU.trim().length > 0) {
        name += ' (' + item.SKU + ')';
    }
    return name;
}

function populateCatalogueItems(items, valuesToSelect) {
    var select = $('#ddlCatalogueItems');
    if (!select.length) { return; }

    select.empty();

    var selections = [];
    var selectionSet = new Set();
    if (Array.isArray(valuesToSelect)) {
        valuesToSelect.forEach(function (value) {
            if (typeof value === 'string') {
                var trimmed = value.trim();
                if (trimmed.length > 0) {
                    selectionSet.add(trimmed);
                }
            }
        });
    }

    if (!Array.isArray(items)) {
        select.val(null);
        select.trigger('change');
        return;
    }

    items.forEach(function (item) {
        if (!item) { return; }
        var itemId = item.ItemId;
        var itemDetailsId = item.ItemDetailsId;
        if (typeof itemId === 'undefined' || typeof itemDetailsId === 'undefined') { return; }

        var optionValue = itemId + '|' + itemDetailsId;
        var displayName = buildCatalogueItemDisplayName(item);
        var option = new Option(displayName, optionValue, false, false);
        select.append(option);
        if (selectionSet.has(optionValue)) {
            selections.push(optionValue);
        }
    });

    if (selections.length > 0) {
        select.val(selections);
    } else {
        select.val(null);
    }

    select.trigger('change');
}

function loadCatalogueItemsForBranch(branchId, valuesToSelect) {
    var select = $('#ddlCatalogueItems');
    if (!select.length) { return; }

    var targetValues = [];
    if (Array.isArray(valuesToSelect)) {
        targetValues = valuesToSelect.slice();
    }

    var usedInitialPairs = false;
    if ((!Array.isArray(valuesToSelect) || valuesToSelect.length === 0) && Array.isArray(initialCatalogueItemPairs) && initialCatalogueItemPairs.length > 0) {
        targetValues = initialCatalogueItemPairs.slice();
        usedInitialPairs = true;
    }

    if (!branchId || branchId === 0) {
        clearCatalogueItems();
        if (usedInitialPairs) {
            initialCatalogueItemPairs = [];
        }
        return;
    }

    var loadedBranchId = parseInt(select.data('loaded-branch'), 10);
    if (!isNaN(loadedBranchId) && loadedBranchId === branchId && select.find('option').length > 0) {
        if (targetValues.length > 0) {
            var validValues = targetValues.filter(function (value) {
                return select.find('option[value="' + value + '"]').length > 0;
            });
            if (validValues.length > 0) {
                select.val(validValues).trigger('change');
            } else {
                select.val(null).trigger('change');
            }
        }
        if (usedInitialPairs) {
            initialCatalogueItemPairs = [];
        }
        return;
    }

    if (catalogueItemsRequest && catalogueItemsRequest.readyState !== 4) {
        catalogueItemsRequest.abort();
    }

    select.prop('disabled', true);

    catalogueItemsRequest = $.ajax({
        url: '/catalogue/itemsByBranch',
        type: 'POST',
        data: { branchId: branchId },
        success: function (response) {
            var items = [];
            if (response) {
                if (Array.isArray(response.Items)) {
                    items = response.Items;
                } else if (Array.isArray(response.items)) {
                    items = response.items;
                }
            }
            populateCatalogueItems(items, targetValues);
            select.data('loaded-branch', branchId);
            if (usedInitialPairs) {
                initialCatalogueItemPairs = [];
            }
        },
        error: function () {
            if (typeof toastr !== 'undefined') {
                toastr.error('Unable to load items for the selected branch. Please try again.');
            }
        },
        complete: function () {
            select.prop('disabled', false);
            catalogueItemsRequest = null;
        }
    });
}

function resetCatalogueScopeErrors() {
    $('#divCatalogueItems, #divCatalogueCategories, #divCatalogueBrands').text('').hide();
    ['#ddlCatalogueItems', '#ddlCatalogueCategories', '#ddlCatalogueBrands'].forEach(function (selector) {
        var element = $(selector);
        if (!element.length) { return; }
        if (element.hasClass('select2-hidden-accessible')) {
            element.next('.select2-container').find('.select2-selection').css('border', '');
        } else {
            element.css('border', '');
        }
    });
}

function markSelectError(selector, errorId, message) {
    var errorElement = $('#' + errorId);
    if (errorElement.length) {
        errorElement.text(message).show();
    }
    var element = $(selector);
    if (!element.length) { return; }
    if (element.hasClass('select2-hidden-accessible')) {
        element.next('.select2-container').find('.select2-selection').css('border', '2px solid #dc3545');
    } else {
        element.css('border', '2px solid #dc3545');
    }
}

function updateCatalogueScopeVisibility() {
    if (!$('#catalogueScopeSection').length) { return; }

    var catalogueType = $('#ddlCatalogueType').val();
    var scopeMessage = $('#catalogueScopeMessage');
    var defaultMessage = 'All active items will be included in this catalogue.';

    $('#selectedItemsContainer').hide();
    $('#categoryContainer').hide();
    $('#brandContainer').hide();

    switch (catalogueType) {
        case '2':
            if (scopeMessage.length) {
                scopeMessage.text('Select specific items or variants to publish in this catalogue.');
            }
            $('#selectedItemsContainer').show();
            break;
        case '3':
            if (scopeMessage.length) {
                scopeMessage.text('Choose one or more categories to publish. All active items inside them will be visible.');
            }
            $('#categoryContainer').show();
            break;
        case '4':
            if (scopeMessage.length) {
                scopeMessage.text('Choose one or more brands to publish. Items belonging to those brands will be visible.');
            }
            $('#brandContainer').show();
            break;
        default:
            if (scopeMessage.length) {
                scopeMessage.text(defaultMessage);
            }
            break;
    }
}

function getCatalogueScopeSelections() {
    var itemPairs = $('#ddlCatalogueItems').val();
    if (!Array.isArray(itemPairs)) { itemPairs = []; }

    var itemIds = [];
    var itemDetailIds = [];

    itemPairs.forEach(function (pair) {
        if (typeof pair !== 'string') { return; }
        var segments = pair.split('|');
        if (segments.length !== 2) { return; }
        var itemId = parseInt(segments[0], 10);
        var itemDetailId = parseInt(segments[1], 10);
        if (!isNaN(itemId)) {
            itemIds.push(itemId);
        }
        if (!isNaN(itemDetailId)) {
            itemDetailIds.push(itemDetailId);
        }
    });

    var categoryValues = $('#ddlCatalogueCategories').val();
    if (!Array.isArray(categoryValues)) { categoryValues = []; }
    var categoryIds = categoryValues.map(function (val) {
        var parsed = parseInt(val, 10);
        return isNaN(parsed) ? null : parsed;
    }).filter(function (val) { return val !== null; });

    var brandValues = $('#ddlCatalogueBrands').val();
    if (!Array.isArray(brandValues)) { brandValues = []; }
    var brandIds = brandValues.map(function (val) {
        var parsed = parseInt(val, 10);
        return isNaN(parsed) ? null : parsed;
    }).filter(function (val) { return val !== null; });

    return {
        itemIds: itemIds,
        itemDetailIds: itemDetailIds,
        categoryIds: categoryIds,
        brandIds: brandIds
    };
}

function fetchList(PageIndex) {
    var det = {
        PageIndex: PageIndex == undefined ? _PageIndex : PageIndex,
        PageSize: $('#txtPageSize').val(),
        Search: $('#txtSearch').val(),
    };
    _PageIndex = det.PageIndex;
    $("#divLoading").show();
    $.ajax({
        url: '/catalogue/catalogueFetch',
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
    resetCatalogueScopeErrors();

    var branchId = getSelectedBranchId();
    var scopeSelections = getCatalogueScopeSelections();
    var catalogueTypeValue = $('#ddlCatalogueType').val();
    var scopeValid = true;

    if (catalogueTypeValue === '2' && scopeSelections.itemIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueItems', 'divCatalogueItems', 'Select at least one item for this catalogue type.');
    } else if (catalogueTypeValue === '3' && scopeSelections.categoryIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueCategories', 'divCatalogueCategories', 'Select at least one category for this catalogue type.');
    } else if (catalogueTypeValue === '4' && scopeSelections.brandIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueBrands', 'divCatalogueBrands', 'Select at least one brand for this catalogue type.');
    }

    if (!scopeValid) {
        return;
    }
    
    var det = {
        CatalogueName: $('#txtCatalogueName').val(),
        Tagline: $('#txtTagline').val(),
        UniqueSlug: $('#txtSlug').val(),
        CatalogueType: catalogueTypeValue,
        BranchId: branchId,
        ItemIds: scopeSelections.itemIds,
        ItemDetailIds: scopeSelections.itemDetailIds,
        CategoryIds: scopeSelections.categoryIds,
        BrandIds: scopeSelections.brandIds,
        ValidFrom: $('#txtValidFrom').val(),
        ValidTo: $('#txtValidTo').val(),
        NeverExpires: $('#chkNeverExpires').is(':checked'),
        ShowPrices: $('#chkShowPrices').is(':checked'),
        ShowMRP: $('#chkShowMRP').is(':checked'),
        ShowDiscount: $('#chkShowDiscount').is(':checked'),
        ShowStock: $('#chkShowStock').is(':checked'),
        ShowOutOfStock: $('#chkShowOutOfStock').is(':checked'),
        ShowBusinessLogo: $('#chkShowBusinessLogo').is(':checked'),
        ShowBusinessName: $('#chkShowBusinessName').is(':checked'),
        ShowAddress: $('#chkShowAddress').is(':checked'),
        ShowContact: $('#chkShowContact').is(':checked'),
        ShowWhatsApp: $('#chkShowWhatsApp').is(':checked'),
        ShowEmail: $('#chkShowEmail').is(':checked'),
        ShowGstin: $('#chkShowGstin').is(':checked'),
        EnableWhatsAppEnquiry: $('#chkEnableWhatsAppEnquiry').is(':checked'),
        EnableWhatsAppEnquiryForItems: $('#chkEnableWhatsAppItems').is(':checked'),
        EnableAddToCart: $('#chkEnableAddToCart').is(':checked'),
        CartDisclaimerText: $('#txtCartDisclaimer').val(),
        AddToCartQuotationMode: $('#ddlAddToCartMode').val() === "" ? 0 : parseInt($('#ddlAddToCartMode').val(), 10),
        ShowProductCode: $('#chkShowProductCode').is(':checked'),
        ShowImages: $('#chkShowImages').is(':checked'),
        ItemsPerPage: $('#txtItemsPerPage').val(),
        DefaultSortOrder: $('#ddlDefaultSortOrder').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        ShowInInvoices: $('#chkShowInInvoices').is(':checked'),
        IsDeleted: false,
        Theme: $('#ddlTheme').val(),
        HeaderColor: $('#txtHeaderColor').val(),
        HeaderTextColor: $('#txtHeaderTextColor').val(),
        TopBarColor: $('#txtTopBarColor').val(),
        TopBarTextColor: $('#txtTopBarTextColor').val(),
        BodyBackgroundColor: $('#txtBodyBackgroundColor').val(),
        BodyTextColor: $('#txtBodyTextColor').val(),
        AddToCartButtonColor: $('#txtAddToCartButtonColor').val(),
        WhatsappSupportButtonColor: $('#txtWhatsappSupportButtonColor').val(),
        FilterBadgeColor: $('#txtFilterBadgeColor').val(),
        LinkIconAccentColor: $('#txtLinkIconAccentColor').val(),
        IsHeaderFixed: $('#chkFixedHeader').is(':checked'),
        IsPasswordProtected: $('#chkIsPasswordProtected').is(':checked'),
        Password: $('#txtPassword').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        GoogleAnalyticsId: $('#txtGoogleAnalyticsId').val(),
        GoogleReviewUrl: $('#txtGoogleReviewUrl').val(),
        FacebookUrl: $('#txtFacebookUrl').val(),
        InstagramUrl: $('#txtInstagramUrl').val(),
        TwitterUrl: $('#txtTwitterUrl').val(),
        LinkedInUrl: $('#txtLinkedInUrl').val(),
        YouTubeUrl: $('#txtYouTubeUrl').val(),
        TikTokUrl: $('#txtTikTokUrl').val(),
        PinterestUrl: $('#txtPinterestUrl').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/catalogue/catalogueInsert',
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
                    window.location.href = "/catalogue/index";
                }
                else {
                    window.location.href = "/catalogue/add";
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function update(i) {
    $('.errorText').hide();
    $('[style*="border: 2px"]').css('border', '');
    resetCatalogueScopeErrors();

    var branchId = getSelectedBranchId();
    var scopeSelections = getCatalogueScopeSelections();
    var catalogueTypeValue = $('#ddlCatalogueType').val();
    var scopeValid = true;

    if (catalogueTypeValue === '2' && scopeSelections.itemIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueItems', 'divCatalogueItems', 'Select at least one item for this catalogue type.');
    } else if (catalogueTypeValue === '3' && scopeSelections.categoryIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueCategories', 'divCatalogueCategories', 'Select at least one category for this catalogue type.');
    } else if (catalogueTypeValue === '4' && scopeSelections.brandIds.length === 0) {
        scopeValid = false;
        markSelectError('#ddlCatalogueBrands', 'divCatalogueBrands', 'Select at least one brand for this catalogue type.');
    }

    if (!scopeValid) {
        return;
    }

    var det = {
        CatalogueId: window.location.href.split('=')[1],
        CatalogueName: $('#txtCatalogueName').val(),
        Tagline: $('#txtTagline').val(),
        UniqueSlug: $('#txtSlug').val(),
        CatalogueType: catalogueTypeValue,
        BranchId: branchId,
        ItemIds: scopeSelections.itemIds,
        ItemDetailIds: scopeSelections.itemDetailIds,
        CategoryIds: scopeSelections.categoryIds,
        BrandIds: scopeSelections.brandIds,
        ValidFrom: $('#txtValidFrom').val(),
        ValidTo: $('#txtValidTo').val(),
        NeverExpires: $('#chkNeverExpires').is(':checked'),
        ShowPrices: $('#chkShowPrices').is(':checked'),
        ShowMRP: $('#chkShowMRP').is(':checked'),
        ShowDiscount: $('#chkShowDiscount').is(':checked'),
        ShowStock: $('#chkShowStock').is(':checked'),
        ShowOutOfStock: $('#chkShowOutOfStock').is(':checked'),
        ShowBusinessLogo: $('#chkShowBusinessLogo').is(':checked'),
        ShowBusinessName: $('#chkShowBusinessName').is(':checked'),
        ShowAddress: $('#chkShowAddress').is(':checked'),
        ShowContact: $('#chkShowContact').is(':checked'),
        ShowWhatsApp: $('#chkShowWhatsApp').is(':checked'),
        ShowEmail: $('#chkShowEmail').is(':checked'),
        ShowGstin: $('#chkShowGstin').is(':checked'),
        EnableWhatsAppEnquiry: $('#chkEnableWhatsAppEnquiry').is(':checked'),
        EnableWhatsAppEnquiryForItems: $('#chkEnableWhatsAppItems').is(':checked'),
        EnableAddToCart: $('#chkEnableAddToCart').is(':checked'),
        CartDisclaimerText: $('#txtCartDisclaimer').val(),
        AddToCartQuotationMode: $('#ddlAddToCartMode').val() === "" ? 0 : parseInt($('#ddlAddToCartMode').val(), 10),
        ShowProductCode: $('#chkShowProductCode').is(':checked'),
        ShowImages: $('#chkShowImages').is(':checked'),
        ItemsPerPage: $('#txtItemsPerPage').val(),
        DefaultSortOrder: $('#ddlDefaultSortOrder').val(),
        SellingPriceGroupId: $('#ddlSellingPriceGroup').val(),
        IsActive: $('#chkIsActive').is(':checked'),
        ShowInInvoices: $('#chkShowInInvoices').is(':checked'),
        Theme: $('#ddlTheme').val(),
        HeaderColor: $('#txtHeaderColor').val(),
        HeaderTextColor: $('#txtHeaderTextColor').val(),
        TopBarColor: $('#txtTopBarColor').val(),
        TopBarTextColor: $('#txtTopBarTextColor').val(),
        BodyBackgroundColor: $('#txtBodyBackgroundColor').val(),
        BodyTextColor: $('#txtBodyTextColor').val(),
        AddToCartButtonColor: $('#txtAddToCartButtonColor').val(),
        WhatsappSupportButtonColor: $('#txtWhatsappSupportButtonColor').val(),
        FilterBadgeColor: $('#txtFilterBadgeColor').val(),
        LinkIconAccentColor: $('#txtLinkIconAccentColor').val(),
        IsHeaderFixed: $('#chkFixedHeader').is(':checked'),
        IsPasswordProtected: $('#chkIsPasswordProtected').is(':checked'),
        Password: $('#txtPassword').val(),
        MetaTitle: $('#txtMetaTitle').val(),
        MetaDescription: $('#txtMetaDescription').val(),
        GoogleAnalyticsId: $('#txtGoogleAnalyticsId').val(),
        GoogleReviewUrl: $('#txtGoogleReviewUrl').val(),
        FacebookUrl: $('#txtFacebookUrl').val(),
        InstagramUrl: $('#txtInstagramUrl').val(),
        TwitterUrl: $('#txtTwitterUrl').val(),
        LinkedInUrl: $('#txtLinkedInUrl').val(),
        YouTubeUrl: $('#txtYouTubeUrl').val(),
        TikTokUrl: $('#txtTikTokUrl').val(),
        PinterestUrl: $('#txtPinterestUrl').val()
    };
    $("#divLoading").show();
    $.ajax({
        url: '/catalogue/catalogueUpdate',
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
                    window.location.href = "/catalogue/index";
                }
                else {
                    window.location.href = "/catalogue/add";
                }
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function ActiveInactive(catalogueId, IsActive) {
    var det = {
        CatalogueId: catalogueId,
        IsActive: IsActive
    };
    $("#divLoading").show();
    $.ajax({
        url: '/catalogue/catalogueActiveInactive',
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
                fetchList();
            }
            
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function View(CatalogueId) {
    var det = {
        CatalogueId: CatalogueId
    };
    $("#divLoading").show();
    $.ajax({
        url: '/catalogue/CatalogueView',
        datatype: "html",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            $('#ViewModal').modal('toggle');
            $("#divView").html(data);
        },
        error: function (xhr, status, error) {
            $("#divLoading").hide();
            if (typeof toastr !== 'undefined') {
                toastr.error('Error loading catalogue details: ' + (xhr.responseText || error || 'Unknown error'));
            } else {
                alert('Error loading catalogue details: ' + (xhr.responseText || error || 'Unknown error'));
            }
        }
    });
};

function Delete(catalogueId, catalogueName) {
    var r = confirm("This will delete \"" + catalogueName + "\" permanently. This process cannot be undone. Are you sure you want to continue?");
    if (r == true) {
        var det = {
            CatalogueId: catalogueId,
        };
        $("#divLoading").show();
        $.ajax({
            url: '/catalogue/catalogueDelete',
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
                    fetchList();
                }
                
            },
            error: function (xhr) {
                $("#divLoading").hide();
            }
        });
    }
};

function copyCatalogueUrl(slug) {
    var url = window.location.origin + '/c/' + slug;

    var tempInput = document.createElement('input');
    tempInput.value = url;
    document.body.appendChild(tempInput);
    tempInput.focus();
    tempInput.select();
    document.execCommand('copy');
    document.body.removeChild(tempInput);

    toastr.success('Catalogue URL copied to clipboard: ' + url);
}

function shareCatalogue(slug) {
    var url = window.location.origin + '/c/' + slug;

    if (navigator.share) {
        navigator.share({
            title: document.title,
            text: 'Check out this catalogue',
            url: url
        }).catch(function (error) {
            if (error && error.name !== 'AbortError') {
                copyCatalogueUrl(slug);
            }
        });
    }
    else {
        copyCatalogueUrl(slug);
    }
}

// View QR Code function (displays existing QR code)
function viewCatalogueQRCode(slug, catalogueName, qrImageUrl) {
    var qrUrl = window.location.origin + '/c/' + slug;
    var safeCatalogueName = catalogueName || 'Catalogue';
    
    // Build QR code modal
    var imageHtml = qrImageUrl
        ? `<img src="${qrImageUrl}" alt="QR Code" style="max-width: 300px; border: 2px solid #007bff; padding: 10px; background: white;" />`
        : `<div class="alert alert-warning">QR image not available, but the URL can still be used.</div>`;

    var modalHtml = `
        <div class="modal fade" id="qrCodeModal" tabindex="-1" role="dialog">
            <div class="modal-dialog modal-dialog-centered" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><i class="fas fa-qrcode"></i> QR Code - ${safeCatalogueName}</h5>
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                    </div>
                    <div class="modal-body text-center">
                        <div class="mb-3">
                            ${imageHtml}
                        </div>
                        <p class="text-muted"><small>Scan this QR code to access the catalogue</small></p>
                        <div class="mt-3 mb-3">
                            <label class="font-weight-bold">Catalogue URL:</label>
                            <div class="input-group">
                                <input type="text" class="form-control" id="qrCodeUrlInput" value="${qrUrl}" readonly style="font-size: 12px;">
                                <div class="input-group-append">
                                    <button class="btn btn-outline-secondary" type="button" onclick="copyQRCodeUrl()" title="Copy URL">
                                        <i class="fas fa-copy"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div class="mt-3">
                            <button type="button" class="btn btn-primary" ${qrImageUrl ? `onclick="downloadQRCode('${qrImageUrl}', 'Catalogue_${slug}_QRCode.png')"` : 'disabled'}>
                                <i class="fas fa-download"></i> Download QR Code
                            </button>
                            <button type="button" class="btn btn-secondary" ${qrImageUrl ? `onclick="printQRCode('${qrImageUrl}')"` : 'disabled'}>
                                <i class="fas fa-print"></i> Print QR Code
                            </button>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Remove existing modal if any
    $('#qrCodeModal').remove();

    // Add modal to body
    $('body').append(modalHtml);

    // Set URL value after modal is created
    if (qrUrl) {
        $('#qrCodeUrlInput').val(qrUrl);
    }

    // Show modal
    $('#qrCodeModal').modal('show');
}

// Helper function to copy QR code URL (used in modal)
function copyQRCodeUrl() {
    var urlInput = document.getElementById('qrCodeUrlInput');
    if (urlInput) {
        urlInput.focus();
        urlInput.select();
        try {
            document.execCommand('copy');
            if (typeof toastr !== 'undefined') {
                toastr.success('URL copied to clipboard');
            }
        } catch (err) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Failed to copy URL');
            }
        }
    }
}

// Helper function to download QR code (used in modal)
function downloadQRCode(imageUrl, filename) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = imageUrl;
    if (imageUrl.startsWith('http') && !imageUrl.startsWith('data:')) {
        fetch(imageUrl)
            .then(response => response.blob())
            .then(blob => {
                var url = window.URL.createObjectURL(blob);
                link.href = url;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            })
            .catch(function() {
                window.open(imageUrl, '_blank');
            });
    } else {
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

// Helper function to print QR code (used in modal)
function printQRCode(imageUrl) {
    var printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title>QR Code</title>
                <style>
                    body { 
                        display: flex; 
                        justify-content: center; 
                        align-items: center; 
                        height: 100vh; 
                        margin: 0; 
                    }
                    img { 
                        max-width: 100%; 
                        height: auto; 
                    }
                </style>
            </head>
            <body>
                <img src="${imageUrl}" alt="QR Code" />
                <script>
                    window.onload = function() {
                        window.print();
                    };
                </script>
            </body>
        </html>
    `);
    printWindow.document.close();
}

