(function () {
    'use strict';

    var doc = document;
    var CART_STORAGE_KEY = 'publicCatalogueCart';
    var searchBridge = window.publicCatalogueSearch = window.publicCatalogueSearch || {};

    function getConfig() {
        return window.publicCatalogueConfig || {};
    }

    function onReady(callback) {
        if (doc.readyState === 'loading') {
            doc.addEventListener('DOMContentLoaded', callback, { once: true });
        } else {
            callback();
        }
    }

    function decodeHtml(value) {
        if (!value) {
            return '';
        }
        var textarea = doc.createElement('textarea');
        textarea.innerHTML = value;
        return textarea.value;
    }

    function attachCopyGstListener() {
        doc.addEventListener('click', function (event) {
            var btn = event.target && event.target.closest('#copyGst');
            if (!btn) {
                return;
            }
            var value = btn.getAttribute('data-value') || '';
            if (!value) {
                return;
            }
            try {
                navigator.clipboard.writeText(value).then(function () {
                    var previous = btn.innerHTML;
                    btn.innerHTML = '<i class="bi bi-clipboard-check"></i>';
                    setTimeout(function () {
                        btn.innerHTML = previous;
                    }, 1200);
                });
            } catch (_) { /* ignore */ }
        });
    }

    function initCartFeature() {
        var cfg = getConfig();
        if (!cfg.enableAddToCart) {
            return;
        }

        (function () {
            var CART_KEY = CART_STORAGE_KEY;
            var addToCartMode = parseInt(cfg.addToCartMode || 0, 10) || 0;
            var whatsappDigits = (cfg.whatsappDigits || '').replace(/[^0-9]/g, '');
            var settings = cfg.settings || {};
            var showPrices = settings.showPrices !== false;
            var showImages = settings.showImages !== false;
            var cartDisclaimerText = typeof cfg.cartDisclaimerText === 'string'
                ? cfg.cartDisclaimerText.trim()
                : '';

            function getCart() {
                try { return JSON.parse(localStorage.getItem(CART_KEY)) || []; } catch (_) { return []; }
            }

            function setCart(items) {
                try { localStorage.setItem(CART_KEY, JSON.stringify(items || [])); } catch (_) { /* ignore */ }
                updateBadge();
            }

            function updateBadge() {
                var items = getCart();
                var count = 0;
                try {
                    count = items.reduce(function (total, item) {
                        return total + Math.max(1, parseInt(item.qty || '1', 10));
                    }, 0);
                } catch (_) {
                    count = 0;
                }
                var badge = doc.getElementById('cartBadge');
                if (badge) {
                    badge.textContent = String(count);
                    badge.style.display = count > 0 ? '' : 'none';
                }
            }

            function escapeHtml(value) {
                return String(value).replace(/[&<>"']/g, function (character) {
                    return {
                        '&': '&amp;',
                        '<': '&lt;',
                        '>': '&gt;',
                        '"': '&quot;',
                        '\'': '&#39;'
                    }[character];
                });
            }

            function renderCart() {
                var items = getCart();
                var emptyEl = doc.getElementById('cartEmpty');
                var listEl = doc.getElementById('cartList');
                var summaryEl = doc.getElementById('cartSummary');
                var totalEl = doc.getElementById('cartTotal');
                var sendBtn = doc.getElementById('btnSendQuotation');
                var noteEl = doc.getElementById('cartNote');
                if (noteEl) {
                    noteEl.textContent = cartDisclaimerText;
                }
                if (!listEl) {
                    return;
                }

                if (!items || items.length === 0) {
                    listEl.innerHTML = '';
                    if (emptyEl) emptyEl.style.display = '';
                    if (summaryEl) summaryEl.style.display = 'none';
                    if (noteEl) noteEl.style.display = 'none';
                    if (showPrices && totalEl) totalEl.textContent = '\u20B90';
                    if (!showPrices && totalEl) totalEl.textContent = '';
                    if (sendBtn) sendBtn.disabled = true;
                    return;
                }

                if (emptyEl) emptyEl.style.display = 'none';
                var html = '';
                var total = 0;
                items.forEach(function (item, index) {
                    var qty = Math.max(1, parseInt(item.qty || '1', 10));
                    var price = parseFloat(item.price || 0) || 0;
                    var imgSrc = (item.img && String(item.img).trim().length > 0) ? String(item.img) : 'https://via.placeholder.com/96x96?text=Img';
                    var amount = qty * price;
                    total += amount;
                    html += '<div class="d-flex align-items-center justify-content-between border rounded-3 p-3 mb-2 shadow-sm cart-item">';
                    html += '<div class="d-flex align-items-center gap-3">';
                    if (showImages) {
                        html += '<img src="' + escapeHtml(imgSrc) + '" alt="" class="rounded-3" style="width:56px;height:56px;object-fit:cover;" />';
                    }
                    html += '<div class="min-w-0">'
                        + '<div class="fw-semibold text-truncate" style="max-width:260px;">' + escapeHtml(item.name || 'Item') + '</div>'
                        + '<div class="d-flex align-items-center mt-1 gap-2 cart-qty-controls">'
                        + '<button type="button" class="btn btn-outline-secondary btn-sm cart-qty-btn" data-qty-action="decrease" data-qty-index="' + index + '"' + (qty <= 1 ? ' disabled' : '') + ' aria-label="Decrease quantity"><i class="bi bi-dash"></i></button>'
                        + '<span class="qty fw-semibold text-body">' + qty + '</span>'
                        + '<button type="button" class="btn btn-outline-secondary btn-sm cart-qty-btn" data-qty-action="increase" data-qty-index="' + index + '" aria-label="Increase quantity"><i class="bi bi-plus"></i></button>'
                        + (showPrices ? '<span class="text-muted small ms-2">× \u20B9' + price.toFixed(0) + '</span>' : '')
                        + '</div>'
                        + '</div>'
                        + '</div>';
                    html += '<div class="text-end ms-3">';
                    if (showPrices) {
                        html += '<div class="fw-semibold fs-6">\u20B9' + amount.toFixed(0) + '</div>';
                    }
                    html += '<button type="button" class="btn btn-link text-danger p-0 mt-1 small cart-remove" data-remove-index="' + index + '"><i class="bi bi-trash me-1"></i></button></div>'
                        + '</div>';
                });
                listEl.innerHTML = html;
                if (summaryEl) summaryEl.style.display = showPrices ? '' : 'none';
                if (noteEl) noteEl.style.display = cartDisclaimerText ? '' : 'none';
                if (showPrices && totalEl) {
                    totalEl.textContent = '\u20B9' + total.toFixed(0);
                }
                if (sendBtn) sendBtn.disabled = false;
            }

            function buildWhatsappMessage(items) {
                if (!items || items.length === 0) {
                    return 'Quotation request';
                }
                var lines = ['Quotation request', '----------------'];
                var total = 0;
                items.forEach(function (item, index) {
                    var qty = Math.max(1, parseInt(item.qty || '1', 10));
                    var price = parseFloat(item.price || 0) || 0;
                    if (showPrices) {
                        total += qty * price;
                    }
                    lines.push((index + 1) + '. ' + (item.name || 'Item') + ' — Qty ' + qty + (showPrices ? ' @@ \u20B9' + price.toFixed(0) : ''));
                });
                if (showPrices) {
                    lines.push('Total: \u20B9' + total.toFixed(0));
                }
                return lines.join('\n');
            }

            function normalizeDigits(value) {
                return String(value || '').replace(/[^0-9]/g, '');
            }

            function resolvePlaceOfSupplyName(stateId) {
                if (!stateId && stateId !== 0) {
                    return '';
                }
                var list = Array.isArray(cfg.placesOfSupply) ? cfg.placesOfSupply : [];
                for (var i = 0; i < list.length; i++) {
                    var entry = list[i];
                    if (entry && String(entry.StateId) === String(stateId)) {
                        return entry.State || '';
                    }
                }
                return '';
            }

            function setFieldError(id, show, message) {
                var el = doc.getElementById(id);
                if (!el) {
                    return;
                }
                var inputId = null;
                if (typeof id === 'string' && id.toLowerCase().endsWith('err')) {
                    inputId = id.substring(0, id.length - 3);
                }
                var inputEl = inputId ? doc.getElementById(inputId) : null;
                if (show) {
                    if (message) {
                        el.textContent = message;
                    }
                    el.classList.remove('d-none');
                    if (inputEl) {
                        inputEl.classList.add('is-invalid');
                    }
                } else {
                    el.classList.add('d-none');
                    if (inputEl) {
                        inputEl.classList.remove('is-invalid');
                    }
                }
            }

            function getInputValue(input) {
                return input && input.value != null ? String(input.value).trim() : '';
            }

            function getSelectInfo(select) {
                if (!select) {
                    return { id: 0, text: '' };
                }
                var value = select.value;
                var id = parseInt(value, 10);
                if (isNaN(id)) {
                    id = 0;
                }
                var text = '';
                if (select.options && select.selectedIndex >= 0) {
                    var option = select.options[select.selectedIndex];
                    if (option) {
                        text = String(option.text != null ? option.text : option.value || '').trim();
                    }
                } else if (value) {
                    text = String(value).trim();
                }
                return { id: id, text: text };
            }

            function extractAddress(modal, prefix, defaults) {
                defaults = defaults || {};
                var result = {
                    name: defaults.name || '',
                    mobile: defaults.mobile || '',
                    altMobile: defaults.altMobile || '',
                    email: defaults.email || '',
                    address: getInputValue(modal.querySelector('#' + prefix + 'Address')),
                    landmark: getInputValue(modal.querySelector('#' + prefix + 'Landmark')),
                    pincode: getInputValue(modal.querySelector('#' + prefix + 'Pincode')),
                    countryId: 0,
                    country: '',
                    stateId: 0,
                    state: '',
                    cityId: 0,
                    city: ''
                };
                var countryEl = modal.querySelector('#' + prefix + 'Country');
                if (countryEl) {
                    var countryInfo = getSelectInfo(countryEl);
                    result.countryId = countryInfo.id;
                    result.country = countryInfo.text;
                }
                var stateEl = modal.querySelector('#' + prefix + 'State');
                if (stateEl) {
                    var stateInfo = getSelectInfo(stateEl);
                    result.stateId = stateInfo.id;
                    result.state = stateInfo.text;
                }
                var cityEl = modal.querySelector('#' + prefix + 'City');
                if (cityEl) {
                    if (cityEl.tagName === 'SELECT') {
                        var cityInfo = getSelectInfo(cityEl);
                        result.cityId = cityInfo.id;
                        result.city = cityInfo.text;
                    } else {
                        result.city = getInputValue(cityEl);
                    }
                }
                return result;
            }

            function collectCustomerFormData(modal) {
                if (!modal) {
                    return { ok: false };
                }
                setFieldError('pcCustMobileErr', false);
                setFieldError('pcCustNameErr', false);

                var mobileInput = modal.querySelector('#pcCustMobile');
                var nameInput = modal.querySelector('#pcCustName');
                var emailInput = modal.querySelector('#pcCustEmail');

                var mobileRaw = mobileInput ? mobileInput.value || '' : '';
                var mobileDigits = normalizeDigits(mobileRaw);
                var nameValue = nameInput ? nameInput.value || '' : '';
                var emailValue = emailInput ? emailInput.value || '' : '';

                var hasError = false;
                if (!mobileDigits || mobileDigits.length < 10) {
                    setFieldError('pcCustMobileErr', true);
                    hasError = true;
                }
                if (!nameValue || !nameValue.trim()) {
                    setFieldError('pcCustNameErr', true);
                    hasError = true;
                }
                if (hasError) {
                    if (!mobileDigits || mobileDigits.length < 10) {
                        if (mobileInput) mobileInput.focus();
                    } else if (nameInput) {
                        nameInput.focus();
                    }
                    return { ok: false };
                }

                var billing = extractAddress(modal, 'pcBill', {
                    name: nameValue.trim(),
                    mobile: mobileDigits,
                    altMobile: '',
                    email: emailValue.trim()
                });

                var shipToggle = modal.querySelector('#pcShipIsDifferent');
                var isShippingDifferent = !!(shipToggle && shipToggle.checked);
                var shipping = null;
                if (isShippingDifferent) {
                    shipping = extractAddress(modal, 'pcShip', {
                        name: getInputValue(modal.querySelector('#pcShipName')) || nameValue.trim(),
                        mobile: normalizeDigits(modal.querySelector('#pcShipMobile') ? modal.querySelector('#pcShipMobile').value : ''),
                        altMobile: normalizeDigits(modal.querySelector('#pcShipAltMobile') ? modal.querySelector('#pcShipAltMobile').value : ''),
                        email: emailValue.trim()
                    });
                }

                var customer = {
                    name: nameValue.trim(),
                    mobile: mobileDigits,
                    mobileRaw: mobileRaw.trim(),
                    email: emailValue.trim(),
                    isShippingDifferent: isShippingDifferent,
                    billing: billing,
                    shipping: shipping
                };

                return { ok: true, customer: customer };
            }

            function formatAddressForWhatsapp(address) {
                if (!address) {
                    return '';
                }
                var parts = [];
                if (address.address) {
                    parts.push(address.address);
                }
                if (address.landmark) {
                    parts.push(address.landmark);
                }
                var locality = [];
                if (address.city) {
                    locality.push(address.city);
                }
                if (address.state) {
                    locality.push(address.state);
                }
                if (address.country) {
                    locality.push(address.country);
                }
                if (locality.length > 0) {
                    parts.push(locality.join(', '));
                }
                if (address.pincode) {
                    parts.push('PIN: ' + address.pincode);
                }
                return parts.join(', ');
            }

            function buildWhatsappMessageWithCustomer(items, customer, extra) {
                var heading = 'Quotation request';
                if (extra && extra.invoiceNo) {
                    heading = 'Quotation #' + extra.invoiceNo;
                }
                var lines = [heading, '----------------'];
                var includePrices = showPrices;
                if (extra && typeof extra.showPrices === 'boolean') {
                    includePrices = extra.showPrices;
                }
                var total = 0;
                if (items && items.length > 0) {
                    items.forEach(function (item, index) {
                        var qty = Math.max(1, parseFloat(item.quantity != null ? item.quantity : item.qty || 1));
                        if (!isFinite(qty) || qty <= 0) {
                            qty = 1;
                        }
                        var price = parseFloat(item.price || 0) || 0;
                        if (includePrices) {
                            total += qty * price;
                        }
                        var name = item.name || item.code || 'Item';
                        var line = (index + 1) + '. ' + name + ' — Qty ' + qty;
                        if (includePrices) {
                            line += ' @ \u20B9' + price.toFixed(0);
                        }
                        lines.push(line);
                    });
                    if (includePrices) {
                        lines.push('Total: \u20B9' + total.toFixed(0));
                    }
                } else {
                    lines.push('No items specified');
                }
                if (customer) {
                    lines.push('');
                    lines.push('Customer Details');
                    if (customer.name) {
                        lines.push('Name: ' + customer.name);
                    }
                    if (customer.mobile) {
                        lines.push('Mobile: ' + customer.mobile);
                    }
                    if (customer.email) {
                        lines.push('Email: ' + customer.email);
                    }
                    if (customer.billing) {
                        var billingLine = formatAddressForWhatsapp(customer.billing);
                        if (billingLine) {
                            lines.push('Billing: ' + billingLine);
                        }
                    }
                    if (customer.isShippingDifferent && customer.shipping) {
                        var shippingLine = formatAddressForWhatsapp(customer.shipping);
                        if (shippingLine) {
                            lines.push('Shipping: ' + shippingLine);
                        }
                        if (customer.shipping.mobile) {
                            lines.push('Shipping Mobile: ' + customer.shipping.mobile);
                        }
                        if (customer.shipping.altMobile) {
                            lines.push('Shipping Alt: ' + customer.shipping.altMobile);
                        }
                    }
                }
                return lines.join('\n');
            }

            function findProductMetaByCode(code) {
                if (!code) {
                    return null;
                }
                var compare = String(code).trim().toLowerCase();
                if (!compare) {
                    return null;
                }
                var match = null;
                var cards = doc.querySelectorAll('.product-card[data-code]');
                if (!cards || cards.length === 0) {
                    return null;
                }
                cards.forEach(function (card) {
                    if (match) {
                        return;
                    }
                    var cardCode = (card.getAttribute('data-code') || '').trim().toLowerCase();
                    if (cardCode && cardCode === compare) {
                        match = {
                            itemId: parseInt(card.getAttribute('data-item-id') || '0', 10) || 0,
                            itemDetailsId: parseInt(card.getAttribute('data-item-details-id') || '0', 10) || 0,
                            sku: card.getAttribute('data-sku') || ''
                        };
                    }
                });
                return match;
            }

            function populateSelectOptions(select, items, valueKey, textKey, selectedValue) {
                if (!select) {
                    return;
                }
                var current = selectedValue == null ? '' : String(selectedValue);
                select.innerHTML = '';
                var defaultOption = document.createElement('option');
                defaultOption.value = '';
                defaultOption.textContent = 'Select';
                select.appendChild(defaultOption);
                if (!Array.isArray(items)) {
                    return;
                }
                items.forEach(function (item) {
                    if (!item) {
                        return;
                    }
                    var option = document.createElement('option');
                    var value = item[valueKey];
                    var text = item[textKey];
                    option.value = value == null ? '' : String(value);
                    option.textContent = text == null ? '' : String(text);
                    if (current && String(value) === current) {
                        option.selected = true;
                    }
                    select.appendChild(option);
                });
            }

            function populateCustomerFromUser(modal, payload) {
                if (!modal || !payload) {
                    return;
                }
                var user = payload.User || {};
                var addresses = Array.isArray(user.Addresses) ? user.Addresses : [];
                var nameEl = modal.querySelector('#pcCustName');
                var emailEl = modal.querySelector('#pcCustEmail');
                if (nameEl && user.Name != null) {
                    nameEl.value = user.Name || '';
                }
                if (emailEl && user.EmailId != null) {
                    emailEl.value = user.EmailId || '';
                }

                var billing = addresses[0] || {};
                var billLandmark = modal.querySelector('#pcBillLandmark');
                var billCountry = modal.querySelector('#pcBillCountry');
                var billState = modal.querySelector('#pcBillState');
                var billCity = modal.querySelector('#pcBillCity');
                var billPincode = modal.querySelector('#pcBillPincode');
                var billAddress = modal.querySelector('#pcBillAddress');
                if (billLandmark && billing.Landmark != null) billLandmark.value = billing.Landmark || '';
                populateSelectOptions(billCountry, payload.Countrys, 'CountryId', 'Country', billing.CountryId);
                populateSelectOptions(billState, payload.States, 'StateId', 'State', billing.StateId);
                if (billCity) {
                    if (billCity.tagName === 'SELECT') {
                        populateSelectOptions(billCity, payload.Citys, 'CityId', 'City', billing.CityId);
                    } else if (billing.City != null) {
                        billCity.value = billing.City || '';
                    }
                }
                if (billPincode && billing.Zipcode != null) billPincode.value = billing.Zipcode || '';
                if (billAddress && billing.Address != null) billAddress.value = billing.Address || '';

                var shipToggle = modal.querySelector('#pcShipIsDifferent');
                var shippingSection = modal.querySelector('#pcShippingSection');
                if (shipToggle) {
                    shipToggle.checked = !!user.IsShippingAddressDifferent;
                    if (shippingSection) {
                        shippingSection.style.display = shipToggle.checked ? '' : 'none';
                    }
                }

                var shipping = addresses[1] || {};
                var shipName = modal.querySelector('#pcShipName');
                var shipMobile = modal.querySelector('#pcShipMobile');
                var shipAltMobile = modal.querySelector('#pcShipAltMobile');
                var shipLandmark = modal.querySelector('#pcShipLandmark');
                var shipCountry = modal.querySelector('#pcShipCountry');
                var shipState = modal.querySelector('#pcShipState');
                var shipCity = modal.querySelector('#pcShipCity');
                var shipPincode = modal.querySelector('#pcShipPincode');
                var shipAddress = modal.querySelector('#pcShipAddress');
                if (shipName && shipping.Name != null) shipName.value = shipping.Name || user.Name || '';
                if (shipMobile && shipping.MobileNo != null) shipMobile.value = shipping.MobileNo || '';
                if (shipAltMobile && shipping.MobileNo2 != null) shipAltMobile.value = shipping.MobileNo2 || '';
                if (shipLandmark && shipping.Landmark != null) shipLandmark.value = shipping.Landmark || '';
                populateSelectOptions(shipCountry, payload.Countrys, 'CountryId', 'Country', shipping.CountryId);
                populateSelectOptions(shipState, payload.AltStates, 'StateId', 'State', shipping.StateId);
                if (shipCity) {
                    populateSelectOptions(shipCity, payload.AltCitys, 'CityId', 'City', shipping.CityId);
                }
                if (shipPincode && shipping.Zipcode != null) shipPincode.value = shipping.Zipcode || '';
                if (shipAddress && shipping.Address != null) shipAddress.value = shipping.Address || '';
            }

            function toggleLoading(button, loading) {
                if (!button) {
                    return;
                }
                if (loading) {
                    if (!button.dataset.originalHtml) {
                        button.dataset.originalHtml = button.innerHTML;
                    }
                    button.disabled = true;
                    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...';
                } else {
                    button.disabled = false;
                    if (button.dataset.originalHtml) {
                        button.innerHTML = button.dataset.originalHtml;
                    }
                }
            }

            function submitQuotationRequest(modal, triggerButton, payload, normalizedItems, customer) {
                if (!payload || !Array.isArray(payload.items) || payload.items.length === 0) {
                    alert('Your cart is empty.');
                    return;
                }
                if (triggerButton) {
                    triggerButton._submitting = true;
                }
                toggleLoading(triggerButton, true);
                fetch('/PublicCatalogue/SubmitQuotation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(payload),
                    credentials: 'same-origin'
                })
                    .then(function (res) {
                        return res.text().then(function (text) {
                            var data;
                            try {
                                data = text ? JSON.parse(text) : null;
                            } catch (err) {
                                throw new Error('Unable to parse server response');
                            }
                            return data;
                        });
                    })
                    .then(function (data) {
                        if (!data || typeof data !== 'object') {
                            throw new Error('Unexpected server response');
                        }
                        if (data.Status === 2 && Array.isArray(data.Errors) && data.Errors.length > 0) {
                            alert(data.Errors[0].Message || 'Please verify the details entered.');
                            return;
                        }
                        if (data.Status !== 1) {
                            alert(data.Message || 'Unable to submit request. Please try again.');
                            return;
                        }
                        var payloadData = data.Data || {};
                        var message = data.Message || 'Quotation submitted successfully.';
                        if (message) {
                            alert(message);
                        }
                        if (modal && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                            try {
                                var modalInstance = window.bootstrap.Modal.getInstance(modal) || window.bootstrap.Modal.getOrCreateInstance(modal);
                                if (modalInstance && typeof modalInstance.hide === 'function') {
                                    modalInstance.hide();
                                }
                            } catch (_) { /* ignore */ }
                        }
                        setCart([]);
                        updateBadge();
                        renderCart();

                        var whatsappUrl = payloadData.WhatsappUrl;
                        var shouldSendWhatsapp = addToCartMode === 2 || addToCartMode === 3;
                        if (!whatsappUrl && shouldSendWhatsapp && whatsappDigits) {
                            var messageText = buildWhatsappMessageWithCustomer(normalizedItems, customer, {
                                invoiceNo: payloadData.InvoiceNo
                            });
                            whatsappUrl = 'https://wa.me/' + whatsappDigits + '?text=' + encodeURIComponent(messageText);
                        }
                        if (whatsappUrl) {
                            window.open(whatsappUrl, '_blank');
                        }
                    })
                    .catch(function (error) {
                        console.error('Submit quotation failed', error);
                        alert('Unable to submit request. Please try again.');
                    })
                    .finally(function () {
                        if (triggerButton) {
                            triggerButton._submitting = false;
                        }
                        toggleLoading(triggerButton, false);
                    });
            }

            doc.addEventListener('click', function (event) {
                var cartBtn = event.target && event.target.closest('#btnCart');
                if (cartBtn) {
                    renderCart();
                    var cartModal = doc.getElementById('cartModal');
                    if (cartModal && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        var modalInstance = window.bootstrap.Modal.getOrCreateInstance(cartModal);
                        modalInstance.show();
                    }
                    return;
                }

                var removeBtn = event.target && event.target.closest('[data-remove-index]');
                if (removeBtn && doc.getElementById('cartModal')) {
                    var removeIndex = parseInt(removeBtn.getAttribute('data-remove-index') || '-1', 10);
                    if (!isNaN(removeIndex) && removeIndex >= 0) {
                        var items = getCart();
                        items.splice(removeIndex, 1);
                        setCart(items);
                        renderCart();
                    }
                    return;
                }

                var qtyBtn = event.target && event.target.closest('[data-qty-action]');
                if (qtyBtn && doc.getElementById('cartModal')) {
                    var action = qtyBtn.getAttribute('data-qty-action');
                    var qtyIndex = parseInt(qtyBtn.getAttribute('data-qty-index') || '-1', 10);
                    if (!isNaN(qtyIndex) && qtyIndex >= 0) {
                        var cartItems = getCart();
                        var cartItem = cartItems[qtyIndex];
                        if (cartItem) {
                            var currentQty = Math.max(1, parseInt(cartItem.qty || '1', 10));
                            var updatedQty = currentQty;
                            if (action === 'increase') {
                                updatedQty = currentQty + 1;
                            } else if (action === 'decrease' && currentQty > 1) {
                                updatedQty = currentQty - 1;
                            }
                            if (updatedQty !== currentQty) {
                                cartItem.qty = updatedQty;
                                cartItems[qtyIndex] = cartItem;
                                setCart(cartItems);
                                renderCart();
                            }
                        }
                    }
                    return;
                }

                var addToCartBtn = event.target && event.target.closest('#pdAddToCart');
                if (addToCartBtn) {
                    var pd = doc.getElementById('productDetailsModal');
                    if (!pd) return;

                    if (addToCartBtn.getAttribute('data-cart-state') === 'added') {
                        renderCart();
                        var cartModalEl = doc.getElementById('cartModal');
                        if (cartModalEl && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                            var cartModalInstance = window.bootstrap.Modal.getOrCreateInstance(cartModalEl);
                            cartModalInstance.show();
                        }
                        var productModalInstance = window.bootstrap && typeof window.bootstrap.Modal === 'function'
                            ? window.bootstrap.Modal.getInstance(pd)
                            : null;
                        if (productModalInstance) {
                            productModalInstance.hide();
                        }
                        return;
                    }

                    var name = (pd.querySelector('#pdName') || pd.querySelector('#pdTitle'));
                    var priceEl = pd.querySelector('#pdPrice');
                    var mrpEl = pd.querySelector('#pdPriceOld');
                    var imgEl = pd.querySelector('#pdImage');
                    var priceText = priceEl ? (priceEl.textContent || '') : '0';
                    var mrpText = mrpEl ? (mrpEl.textContent || '') : '0';
                    var imgSrc = imgEl ? (imgEl.getAttribute('src') || '') : '';
                    var priceDataset = pd.getAttribute('data-product-price') || '';
                    var mrpDataset = pd.getAttribute('data-product-mrp') || '';
                    var skuValue = (pd.getAttribute('data-product-sku') || '').trim();
                    var itemIdValue = parseInt(pd.getAttribute('data-item-id') || '0', 10);
                    if (!isFinite(itemIdValue) || itemIdValue < 0) {
                        itemIdValue = 0;
                    }
                    var itemDetailsIdValue = parseInt(pd.getAttribute('data-item-details-id') || '0', 10);
                    if (!isFinite(itemDetailsIdValue) || itemDetailsIdValue < 0) {
                        itemDetailsIdValue = 0;
                    }
                    var price = parseFloat(priceText.replace(/[^0-9.]/g, '')) || 0;
                    if ((!price || !isFinite(price)) && priceDataset) {
                        var parsedPriceDataset = parseFloat(priceDataset);
                        if (!isNaN(parsedPriceDataset) && isFinite(parsedPriceDataset)) {
                            price = parsedPriceDataset;
                        }
                    }
                    var mrp = parseFloat(mrpText.replace(/[^0-9.]/g, '')) || 0;
                    if ((!mrp || !isFinite(mrp)) && mrpDataset) {
                        var parsedMrpDataset = parseFloat(mrpDataset);
                        if (!isNaN(parsedMrpDataset) && isFinite(parsedMrpDataset)) {
                            mrp = parsedMrpDataset;
                        }
                    }
                    var codeValue = (pd.getAttribute('data-product-code') || '').trim();
                    var items = getCart();
                    items.push({
                        code: codeValue,
                        sku: skuValue,
                        itemId: itemIdValue,
                        itemDetailsId: itemDetailsIdValue,
                        name: name ? (name.textContent || 'Item') : 'Item',
                        price: price,
                        qty: 1,
                        mrp: mrp,
                        img: imgSrc
                    });
                    setCart(items);
                    updateBadge();
                    addToCartBtn.setAttribute('data-cart-state', 'added');
                    addToCartBtn.innerHTML = '<i class="bi bi-cart-check me-1"></i> Go to Cart';
                    addToCartBtn.classList.remove('btn-danger');
                    addToCartBtn.classList.add('btn-warning');
                    return;
                }

                if (event.target && event.target.id === 'btnSendQuotation') {
                    var items = getCart();
                    if (!items || items.length === 0) {
                        return;
                    }

                    var customerModal = doc.getElementById('pcCustomerModal');
                    var modalShown = false;
                    if (customerModal && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        var cmInstance = window.bootstrap.Modal.getOrCreateInstance(customerModal);
                        try {
                            var shipToggle = customerModal.querySelector('#pcShipIsDifferent');
                            var shipSection = customerModal.querySelector('#pcShippingSection');
                            if (shipToggle && shipSection) {
                                var applyToggle = function () {
                                    shipSection.style.display = shipToggle.checked ? '' : 'none';
                                };
                                applyToggle();
                                shipToggle.removeEventListener('change', applyToggle);
                                shipToggle.addEventListener('change', applyToggle);
                            }
                        } catch (ex) { /* no-op */ }
                        setFieldError('pcCustMobileErr', false);
                        setFieldError('pcCustNameErr', false);
                        try {
                            var mobileInput = customerModal.querySelector('#pcCustMobile');
                            if (mobileInput) {
                                var cfgModal = getConfig();
                                var lastLookedUp = null;
                                var lookupHandler = function () {
                                    var raw = (mobileInput.value || '').trim();
                                    var digits = raw.replace(/[^0-9]/g, '');
                                    if (!digits || digits.length < 10) {
                                        return;
                                    }
                                    if (lastLookedUp === digits) {
                                        return;
                                    }
                                    lastLookedUp = digits;
                                    var payload = {
                                        Slug: cfgModal.slug,
                                        MobileNo: digits
                                    };
                                    fetch('/PublicCatalogue/ExistingCustomerByMobile', {
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/json'
                                        },
                                        body: JSON.stringify(payload),
                                        credentials: 'same-origin'
                                    })
                                        .then(function (res) {
                                            if (!res.ok) throw new Error('Failed to lookup customer');
                                            return res.json();
                                        })
                                        .then(function (data) {
                                            if (!data || data.Status !== 1 || !data.Data || !data.Data.User) {
                                                return;
                                            }
                                            populateCustomerFromUser(customerModal, data.Data);
                                        })
                                        .catch(function () { /* ignore */ });
                                };
                                var tabHandler = function (e) {
                                    if (e.key === 'Tab') {
                                        setTimeout(lookupHandler, 0);
                                    }
                                };
                                if (!mobileInput._pcLookupBound) {
                                    mobileInput.addEventListener('blur', lookupHandler);
                                    mobileInput.addEventListener('keydown', tabHandler);
                                    mobileInput._pcLookupBound = true;
                                }
                            }
                        } catch (ex) { /* ignore lookup issues */ }
                        cmInstance.show();
                        modalShown = true;
                    }
                    if (!modalShown && (addToCartMode === 2 || addToCartMode === 3) && whatsappDigits) {
                        var fallbackMessage = buildWhatsappMessage(items);
                        var fallbackUrl = 'https://wa.me/' + whatsappDigits + '?text=' + encodeURIComponent(fallbackMessage);
                        window.open(fallbackUrl, '_blank');
                    }

                    var modalElement = doc.getElementById('cartModal');
                    if (modalElement && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        var existing = window.bootstrap.Modal.getInstance(modalElement);
                        if (existing) existing.hide();
                    }
                    console.log('Quotation request submitted', { mode: addToCartMode, items: items });
                    return;
                }

                if (event.target && event.target.id === 'pcAddCustomerSave') {
                    var saveButton = event.target;
                    if (saveButton._submitting) {
                        return;
                    }
                    var customerModal = doc.getElementById('pcCustomerModal');
                    var cartItemsForSubmit = getCart();
                    if (!cartItemsForSubmit || cartItemsForSubmit.length === 0) {
                        alert('Your cart is empty.');
                        return;
                    }

                    var customerResult = collectCustomerFormData(customerModal);
                    if (!customerResult.ok) {
                        return;
                    }

                    var normalizedItems = cartItemsForSubmit.map(function (item) {
                        if (!item) {
                            return null;
                        }
                        var qty = parseFloat(item.quantity != null ? item.quantity : item.qty || 1);
                        if (!isFinite(qty) || qty <= 0) {
                            qty = 1;
                        }
                        var price = parseFloat(item.price || 0);
                        if (!isFinite(price) || price < 0) {
                            price = 0;
                        }
                        var mrp = parseFloat(item.mrp || 0);
                        if (!isFinite(mrp) || mrp < 0) {
                            mrp = 0;
                        }
                        var itemId = parseInt(item.itemId, 10);
                        if (!isFinite(itemId) || itemId < 0) {
                            itemId = 0;
                        }
                        var itemDetailsId = parseInt(item.itemDetailsId, 10);
                        if (!isFinite(itemDetailsId) || itemDetailsId < 0) {
                            itemDetailsId = 0;
                        }
                        if ((!itemId || !itemDetailsId) && item.code) {
                            var meta = findProductMetaByCode(item.code);
                            if (meta) {
                                if (!itemId) itemId = meta.itemId;
                                if (!itemDetailsId) itemDetailsId = meta.itemDetailsId;
                                if (!item.sku && meta.sku) {
                                    item.sku = meta.sku;
                                }
                            }
                        }
                        return {
                            itemId: itemId,
                            itemDetailsId: itemDetailsId,
                            code: item.code || '',
                            sku: item.sku || '',
                            name: item.name || '',
                            quantity: qty,
                            price: price,
                            mrp: mrp
                        };
                    }).filter(function (entry) { return entry != null; });

                    if (normalizedItems.length === 0) {
                        alert('Unable to process items in the cart. Please try re-adding them.');
                        return;
                    }

                    var selectedPlaceOfSupplyId = null;
                    try {
                        var cookiePartsPOS = ('; ' + document.cookie).split('; publicCatalogueSelectedStateId=');
                        if (cookiePartsPOS.length === 2) {
                            var cookieValuePOS = cookiePartsPOS.pop().split(';').shift();
                            if (cookieValuePOS) {
                                var decodedValuePOS = decodeURIComponent(cookieValuePOS);
                                var parsedValuePOS = parseInt(decodedValuePOS, 10);
                                if (!isNaN(parsedValuePOS) && parsedValuePOS > 0) {
                                    selectedPlaceOfSupplyId = parsedValuePOS;
                                }
                            }
                        }
                    } catch (_) { /* ignore */ }

                    if (typeof selectedPlaceOfSupplyId === 'number' && selectedPlaceOfSupplyId > 0 && customerResult.customer && customerResult.customer.billing) {
                        if (!customerResult.customer.billing.stateId || customerResult.customer.billing.stateId <= 0) {
                            customerResult.customer.billing.stateId = selectedPlaceOfSupplyId;
                        }
                        if (!customerResult.customer.billing.state) {
                            customerResult.customer.billing.state = resolvePlaceOfSupplyName(selectedPlaceOfSupplyId);
                        }
                    }

                    var payload = {
                        slug: cfg.slug || '',
                        mode: addToCartMode,
                        items: normalizedItems,
                        customer: customerResult.customer
                    };

                    if (typeof selectedPlaceOfSupplyId === 'number' && selectedPlaceOfSupplyId > 0) {
                        payload.placeOfSupplyId = selectedPlaceOfSupplyId;
                    }

                    submitQuotationRequest(customerModal, saveButton, payload, normalizedItems, customerResult.customer);
                    return;
                }
            });

            updateBadge();
            doc.addEventListener('DOMContentLoaded', updateBadge, { once: true });
        })();
    }

    function initPriceSliders() {
        var cfg = getConfig();
        var settings = cfg.settings || {};
        if (settings.showPrices === false) {
            return;
        }

        var priceRange = cfg.priceRange || {};
        var toNumber = function (value) {
            if (typeof value === 'number') {
                return isFinite(value) ? value : null;
            }
            if (typeof value === 'string') {
                var trimmed = value.trim();
                if (!trimmed) {
                    return null;
                }
                var parsed = parseFloat(trimmed.replace(/[^0-9.\-]/g, ''));
                return isFinite(parsed) ? parsed : null;
            }
            return null;
        };
        var configuredMin = toNumber(priceRange.min);
        var configuredMax = toNumber(priceRange.max);

        function setupPriceSlider(sliderId, minLabelId, maxLabelId) {
            var slider = doc.getElementById(sliderId);
            if (!slider || !window.noUiSlider) return;

            var prices = Array.prototype.slice.call(doc.querySelectorAll('.product-card .price'))
                .map(function (el) { return parseFloat((el.textContent || '').replace(/[^0-9.]/g, '')); })
                .filter(function (n) { return !isNaN(n); });

            var domMin = prices.length ? Math.floor(Math.min.apply(null, prices)) : null;
            var domMax = prices.length ? Math.ceil(Math.max.apply(null, prices)) : null;

            var rangeMin = configuredMin;
            var rangeMax = configuredMax;

            if (domMin !== null && domMin !== undefined) {
                rangeMin = rangeMin === null ? domMin : Math.min(rangeMin, domMin);
            }
            if (domMax !== null && domMax !== undefined) {
                rangeMax = rangeMax === null ? domMax : Math.max(rangeMax, domMax);
            }

            if (rangeMin === null || rangeMin === undefined || !isFinite(rangeMin)) {
                rangeMin = domMin !== null && domMin !== undefined ? domMin : 0;
            }
            if (rangeMax === null || rangeMax === undefined || !isFinite(rangeMax) || rangeMax <= rangeMin) {
                var fallbackDomMax = domMax !== null && domMax !== undefined ? domMax : rangeMin;
                rangeMax = Math.max(rangeMin + 1, fallbackDomMax);
            }

            var startMin = configuredMin !== null ? configuredMin : (domMin !== null && domMin !== undefined ? domMin : rangeMin);
            var startMax = configuredMax !== null ? configuredMax : (domMax !== null && domMax !== undefined ? domMax : rangeMax);

            startMin = Math.max(rangeMin, Math.min(startMin, rangeMax));
            startMax = Math.max(rangeMin, Math.min(startMax, rangeMax));
            if (startMax <= startMin) {
                startMax = Math.min(rangeMax, startMin + 1);
                if (startMax <= startMin) {
                    startMin = rangeMin;
                    startMax = rangeMax;
                }
            }

            window.noUiSlider.create(slider, {
                start: [startMin, startMax],
                connect: true,
                range: { min: rangeMin, max: rangeMax },
                step: 1
            });

            var minEl = doc.getElementById(minLabelId);
            var maxEl = doc.getElementById(maxLabelId);
            var fmt = function (v) { return '\u20B9' + Math.round(v); };
            slider.noUiSlider.on('update', function (values) {
                if (minEl) minEl.textContent = fmt(values[0]);
                if (maxEl) maxEl.textContent = fmt(values[1]);
            });
        }

        setupPriceSlider('priceSlider', 'priceMinLabel', 'priceMaxLabel');
        setupPriceSlider('priceSliderSm', 'priceMinLabelSm', 'priceMaxLabelSm');
    }

    function initProductFilters() {
        var cfg = getConfig();
        if (!cfg || !cfg.slug) {
            return;
        }

        var container = doc.getElementById('productsGrid');
        if (!container) {
            return;
        }
        var resultsLabel = doc.getElementById('resultsLabel');
        var categoryLinks = Array.prototype.slice.call(doc.querySelectorAll('[data-category-select]'));

        var debounceTimer = null;
        var desktopSlider = doc.getElementById('priceSlider');
        var mobileSlider = doc.getElementById('priceSliderSm');
        var includeOutDesktop = doc.getElementById('includeOutOfStock');
        var includeOutMobile = doc.getElementById('sIncludeOutOfStock');
        var sortDropdown = doc.querySelector('.sort-dropdown');
        var sortButton = sortDropdown ? sortDropdown.querySelector('.btn-sort') : null;
        var sortItems = sortDropdown ? Array.prototype.slice.call(sortDropdown.querySelectorAll('.dropdown-item[data-sort-value]')) : [];

        function resolveIncludeOutOfStockState() {
            return !!((includeOutDesktop && includeOutDesktop.checked) || (includeOutMobile && includeOutMobile.checked));
        }

        function notifyAvailabilityChange() {
            if (searchBridge && typeof searchBridge.onAvailabilityChange === 'function') {
                try {
                    searchBridge.onAvailabilityChange(resolveIncludeOutOfStockState());
                } catch (_) { /* ignore */ }
            }
        }

        if (searchBridge) {
            searchBridge.getIncludeOutOfStock = resolveIncludeOutOfStockState;
            searchBridge.getSelectedBrandIds = collectBrandIds;
            searchBridge.getPriceRange = function () {
                var range = getSliderValues();
                if (!range) {
                    return null;
                }
                return {
                    min: range.min,
                    max: range.max
                };
            };
        }

        var initialPageIndex = parseInt(cfg.initialPageIndex, 10);
        var initialNextPageIndex = parseInt(cfg.initialNextPageIndex, 10);
        var initialPageSize = parseInt(cfg.pageSize, 10);
        var initialSortOrder = parseInt(cfg.initialSortOrder, 10);
        if (isNaN(initialSortOrder)) {
            initialSortOrder = sortButton ? parseInt(sortButton.getAttribute('data-sort-value'), 10) : 0;
        }
        if (isNaN(initialSortOrder)) {
            initialSortOrder = 0;
        }
        var resolvedPageIndex = !isNaN(initialPageIndex) && initialPageIndex > 0 ? initialPageIndex : 1;
        var resolvedNextPageIndex = !isNaN(initialNextPageIndex) && initialNextPageIndex > 0 ? initialNextPageIndex : (resolvedPageIndex + 1);
        var resolvedPageSize = !isNaN(initialPageSize) && initialPageSize > 0 ? initialPageSize : 0;
        var initialTotalCount = parseInt(cfg.initialTotalCount, 10);
        if (isNaN(initialTotalCount) || initialTotalCount < 0) {
            initialTotalCount = 0;
        }
        var initialLoadedCount = parseInt(cfg.initialLoadedCount, 10);
        if (isNaN(initialLoadedCount) || initialLoadedCount < 0) {
            initialLoadedCount = 0;
        }
        var initialMinPageIndex = parseInt(cfg.initialMinPageIndex, 10);
        if (isNaN(initialMinPageIndex) || initialMinPageIndex <= 0) {
            initialMinPageIndex = resolvedPageIndex;
        }
        var productState = {
            loading: false,
            pageIndex: resolvedPageIndex,
            nextPageIndex: resolvedNextPageIndex,
            pageSize: resolvedPageSize,
            hasMore: cfg.initialHasMore === true || cfg.initialHasMore === 'true',
            sortOrder: initialSortOrder,
            totalCount: initialTotalCount,
            loadedCount: initialLoadedCount,
            minLoadedPage: initialMinPageIndex,
            maxLoadedPage: initialMinPageIndex,
            categoryId: null,
            subCategoryId: null,
            subSubCategoryId: null,
            searchTerm: ''
        };
        if (productState.loadedCount <= 0) {
            productState.loadedCount = container.querySelectorAll('.product-card').length;
        }

        var initialSearchRaw = typeof cfg.initialSearchTerm === 'string' ? String(cfg.initialSearchTerm) : '';
        productState.searchTerm = initialSearchRaw.trim();
        searchBridge.currentRawValue = initialSearchRaw;
        searchBridge.inputElement = searchBridge.inputElement || null;
        searchBridge.onTermChange = searchBridge.onTermChange || null;

        function setSearchTerm(value, options) {
            options = options || {};
            var rawValue = value == null ? '' : String(value);
            var normalizedValue = rawValue.trim();
            var normalizedChanged = normalizedValue !== productState.searchTerm;
            var forceUpdate = options.force === true;

            searchBridge.currentRawValue = rawValue;

            if (options.syncInput !== false && searchBridge.inputElement && searchBridge.inputElement.value !== rawValue) {
                searchBridge.inputElement.value = rawValue;
            }

            if (normalizedChanged) {
                productState.searchTerm = normalizedValue;
            }

            if (typeof searchBridge.onTermChange === 'function') {
                try {
                    searchBridge.onTermChange({
                        raw: rawValue,
                        normalized: productState.searchTerm || ''
                    });
                } catch (_) { /* ignore */ }
            }

            if ((normalizedChanged || forceUpdate) && !options.silent) {
                triggerFetch();
            }
        }

        searchBridge.setTerm = setSearchTerm;
        searchBridge.getTerm = function () {
            return productState.searchTerm || '';
        };
        searchBridge.getRawValue = function () {
            if (typeof searchBridge.currentRawValue === 'string') {
                return searchBridge.currentRawValue;
            }
            return productState.searchTerm || '';
        };
        searchBridge.registerInput = function (inputElement) {
            searchBridge.inputElement = inputElement || null;
            if (inputElement) {
                var raw = searchBridge.getRawValue();
                if (typeof raw === 'string') {
                    inputElement.value = raw;
                }
            }
        };
        searchBridge.clear = function () {
            setSearchTerm('', { force: true });
        };

        var categoryState = {
            level: null,
            categoryId: null,
            subCategoryId: null,
            subSubCategoryId: null,
            activeLink: null
        };

        function parseCategoryId(rawValue) {
            if (rawValue === null || rawValue === undefined) {
                return null;
            }
            if (typeof rawValue === 'number') {
                return rawValue > 0 ? rawValue : null;
            }
            var parsed = parseInt(String(rawValue), 10);
            if (isNaN(parsed) || parsed <= 0) {
                return null;
            }
            return parsed;
        }

        function updateCategoryActiveState() {
            var activeLevel = categoryState.level;
            categoryLinks.forEach(function (link) {
                var linkId = parseCategoryId(link.getAttribute('data-category-select'));
                var linkRoot = link.getAttribute('data-category-root') || '';
                var linkLevel = link.getAttribute('data-category-level') || 'category';
                var isActive = false;
                if (linkLevel === 'category') {
                    isActive = !!(categoryState.categoryId && linkId === categoryState.categoryId);
                } else if (linkLevel === 'subcategory') {
                    isActive = !!(categoryState.subCategoryId && linkId === categoryState.subCategoryId && (activeLevel === 'subcategory' || activeLevel === 'subsubcategory'));
                } else if (linkLevel === 'subsubcategory') {
                    isActive = !!(categoryState.subSubCategoryId && linkId === categoryState.subSubCategoryId && activeLevel === 'subsubcategory');
                }
                if (isActive) {
                    link.classList.add('active');
                } else {
                    link.classList.remove('active');
                }
            });
            doc.querySelectorAll('.dropdown.mega-menu').forEach(function (menu) {
                var menuRoot = menu.getAttribute('data-category-root') || '';
                if (categoryState.categoryId && parseCategoryId(menuRoot) === categoryState.categoryId) {
                    menu.classList.add('category-active');
                } else {
                    menu.classList.remove('category-active');
                }
            });
        }

        function clearCategorySelection(options) {
            options = options || {};
            categoryState.level = null;
            categoryState.categoryId = null;
            categoryState.subCategoryId = null;
            categoryState.subSubCategoryId = null;
            categoryState.activeLink = null;
            productState.categoryId = null;
            productState.subCategoryId = null;
            productState.subSubCategoryId = null;
            updateCategoryActiveState();
            if (!options.silent) {
                triggerFetch();
            }
        }

        function closeCategoryDropdown(linkElement) {
            if (!linkElement) {
                return;
            }
            var dropdownMenuElement = linkElement.closest('.dropdown-menu');
            if (!dropdownMenuElement) {
                return;
            }
            var menuItem = dropdownMenuElement.closest('.dropdown.mega-menu');
            if (!menuItem) {
                return;
            }
            var toggleElement = menuItem.querySelector('[data-bs-toggle="dropdown"]');
            var hideHandled = false;
            if (toggleElement && window.bootstrap && window.bootstrap.Dropdown) {
                try {
                    var dropdownInstance = window.bootstrap.Dropdown.getInstance(toggleElement) || window.bootstrap.Dropdown.getOrCreateInstance(toggleElement);
                    if (dropdownInstance && typeof dropdownInstance.hide === 'function') {
                        dropdownInstance.hide();
                        hideHandled = true;
                    }
                } catch (_) { /* ignore */ }
            }
            if (!hideHandled) {
                menuItem.classList.remove('show');
                if (toggleElement) {
                    toggleElement.classList.remove('show');
                    toggleElement.setAttribute('aria-expanded', 'false');
                }
                dropdownMenuElement.classList.remove('show');
            }
        }

        if (categoryLinks.length > 0) {
            var initialCategoryId = parseCategoryId(cfg.initialCategoryId);
            if (initialCategoryId) {
                productState.categoryId = initialCategoryId;
                categoryState.categoryId = initialCategoryId;
                categoryState.level = 'category';
                for (var cIdx = 0; cIdx < categoryLinks.length; cIdx++) {
                    var candidate = categoryLinks[cIdx];
                    if (parseCategoryId(candidate.getAttribute('data-category-select')) === initialCategoryId) {
                        categoryState.activeLink = candidate;
                        break;
                    }
                }
                updateCategoryActiveState();
            }

            categoryLinks.forEach(function (link) {
                link.addEventListener('click', function (event) {
                    event.preventDefault();
                    var value = parseCategoryId(this.getAttribute('data-category-select'));
                    var level = this.getAttribute('data-category-level') || 'category';
                    var rootId = parseCategoryId(this.getAttribute('data-category-root'));
                    var parentSubCategoryId = parseCategoryId(this.getAttribute('data-parent-subcategory'));
                    if (!value) {
                        clearCategorySelection();
                        return;
                    }

                    categoryState.level = level;
                    categoryState.categoryId = rootId || value;
                    categoryState.subCategoryId = null;
                    categoryState.subSubCategoryId = null;
                    if (level === 'subcategory' || level === 'subsubcategory') {
                        categoryState.subCategoryId = value;
                    }
                    if (level === 'subsubcategory') {
                        categoryState.subSubCategoryId = value;
                        categoryState.subCategoryId = parentSubCategoryId || categoryState.subCategoryId;
                    }
                    categoryState.activeLink = this;
                    productState.categoryId = categoryState.categoryId;
                    productState.subCategoryId = categoryState.subCategoryId;
                    productState.subSubCategoryId = categoryState.subSubCategoryId;
                    updateCategoryActiveState();
                    if (this.closest('.dropdown-menu')) {
                        closeCategoryDropdown(this);
                    }
                    if (level === 'category' && this.getAttribute('data-bs-toggle') === 'dropdown') {
                        try {
                            var existing = window.bootstrap.Dropdown.getInstance(this) || window.bootstrap.Dropdown.getOrCreateInstance(this);
                            if (existing && typeof existing.show === 'function') {
                                existing.show();
                            }
                        } catch (_) { /* ignore */ }
                    }
                    triggerFetch();
                });
            });
        }

        function getSortLabel(value) {
            var target = parseInt(value, 10);
            if (isNaN(target)) {
                target = 0;
            }
            if (!sortItems || sortItems.length === 0) {
                return '';
            }
            for (var i = 0; i < sortItems.length; i++) {
                var item = sortItems[i];
                var itemValue = parseInt(item.getAttribute('data-sort-value'), 10);
                if (itemValue === target) {
                    var attr = item.getAttribute('data-sort-label');
                    if (attr && attr.trim().length > 0) {
                        return attr.trim();
                    }
                    return (item.textContent || '').trim();
                }
            }
            return '';
        }

        function updateSortButton(value, label) {
            if (!sortButton) {
                return;
            }
            sortButton.setAttribute('data-sort-value', String(value));
            if (typeof label === 'string') {
                var trimmed = label.trim();
                if (trimmed.length > 0) {
                    sortButton.textContent = trimmed;
                }
            }
        }

        function setActiveSort(value) {
            if (!sortItems || sortItems.length === 0) {
                return;
            }
            var target = parseInt(value, 10);
            if (isNaN(target)) {
                target = 0;
            }
            sortItems.forEach(function (item) {
                var itemValue = parseInt(item.getAttribute('data-sort-value'), 10);
                if (itemValue === target) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        }

        function getLoadedProductCount() {
            if (!container) {
                return 0;
            }
            return container.querySelectorAll('.product-card').length;
        }

        function updateResultsLabel() {
            if (!resultsLabel) {
                return;
            }
            var loaded = getLoadedProductCount();
            productState.loadedCount = loaded;
            var total = typeof productState.totalCount === 'number' && productState.totalCount >= 0 ? productState.totalCount : 0;
            var searchTerm = productState.searchTerm || '';
            var hasSearch = searchTerm.length > 0;
            var searchSuffix = hasSearch ? ' for "' + searchTerm + '"' : '';
            if (loaded <= 0 && total <= 0) {
                resultsLabel.textContent = hasSearch ? 'No results found' + searchSuffix : 'Showing 0 results';
                return;
            }
            if (loaded <= 0) {
                if (total > 0) {
                    resultsLabel.textContent = 'Showing 0 of ' + total + ' results' + searchSuffix;
                } else {
                    resultsLabel.textContent = hasSearch ? 'No results found' + searchSuffix : 'Showing 0 results';
                }
                return;
            }
            var start = 1;
            if (productState.pageSize > 0 && typeof productState.minLoadedPage === 'number' && productState.minLoadedPage > 0) {
                start = ((productState.minLoadedPage - 1) * productState.pageSize) + 1;
                if (start < 1) {
                    start = 1;
                }
            }
            var end = start + loaded - 1;
            if (total > 0 && end > total) {
                end = total;
            }
            if (total > 0) {
                resultsLabel.textContent = 'Showing ' + start + ' \u2013 ' + end + ' of ' + total + ' results' + searchSuffix;
            } else {
                resultsLabel.textContent = 'Showing ' + start + ' \u2013 ' + end + ' results' + searchSuffix;
            }
        }

        function applySortSelection(value, label) {
            var resolvedValue = parseInt(value, 10);
            if (isNaN(resolvedValue)) {
                resolvedValue = 0;
            }
            var labelText = '';
            if (typeof label === 'string') {
                labelText = label.trim();
            }
            if (!labelText) {
                labelText = getSortLabel(resolvedValue);
            }
            productState.sortOrder = resolvedValue;
            updateSortButton(resolvedValue, labelText);
            setActiveSort(resolvedValue);
        }

        var initialSortLabel = getSortLabel(productState.sortOrder);
        if (!initialSortLabel && sortButton) {
            initialSortLabel = (sortButton.textContent || '').trim();
        }
        applySortSelection(productState.sortOrder, initialSortLabel);

        if (sortItems && sortItems.length > 0) {
            sortItems.forEach(function (item) {
                item.addEventListener('click', function (event) {
                    event.preventDefault();
                    var value = parseInt(this.getAttribute('data-sort-value'), 10);
                    if (isNaN(value)) {
                        value = 0;
                    }
                    var labelAttr = this.getAttribute('data-sort-label');
                    var labelText = labelAttr && labelAttr.trim().length > 0 ? labelAttr.trim() : (this.textContent || '').trim();
                    var changed = value !== productState.sortOrder;
                    applySortSelection(value, labelText);
                    if (window.bootstrap && sortButton) {
                        try {
                            var dropdownInstance = window.bootstrap.Dropdown.getInstance(sortButton) || window.bootstrap.Dropdown.getOrCreateInstance(sortButton);
                            if (dropdownInstance && typeof dropdownInstance.hide === 'function') {
                                dropdownInstance.hide();
                            }
                        } catch (_) { /* ignore */ }
                    }
                    if (changed) {
                        triggerFetch();
                    }
                });
            });
        }

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting && !productState.loading && productState.hasMore) {
                    observer.unobserve(entry.target);
                    loadMoreProducts();
                }
            });
        }, { rootMargin: '0px 0px 200px 0px', threshold: 0 });

        function attachObserver(target) {
            observer.disconnect();
            if (target && productState.hasMore) {
                observer.observe(target);
            }
        }

        function syncCheckboxGroup(changedInput) {
            if (!changedInput) return;
            var brandId = changedInput.getAttribute('data-brand-id');
            if (!brandId) return;
            doc.querySelectorAll('.form-check-input[data-brand-id="' + brandId + '"]').forEach(function (input) {
                if (input !== changedInput) {
                    input.checked = changedInput.checked;
                }
            });
        }

        function syncAvailability(source, target) {
            if (target) {
                target.checked = !!(source && source.checked);
            }
            notifyAvailabilityChange();
        }

        function getSliderValues() {
            var slider = null;
            if (desktopSlider && desktopSlider.noUiSlider) {
                slider = desktopSlider;
            } else if (mobileSlider && mobileSlider.noUiSlider) {
                slider = mobileSlider;
            }

            if (!slider || !slider.noUiSlider) {
                return null;
            }

            var values = slider.noUiSlider.get();
            if (!Array.isArray(values)) {
                values = [values];
            }

            var min = parseFloat(values[0]);
            var max = values.length > 1 ? parseFloat(values[1]) : min;

            if (isNaN(min) || isNaN(max)) {
                return null;
            }

            if (min > max) {
                var temp = min;
                min = max;
                max = temp;
            }

            return { min: min, max: max };
        }

        function collectBrandIds() {
            var ids = [];
            doc.querySelectorAll('.form-check-input[data-brand-id]:checked').forEach(function (input) {
                var id = parseInt(input.getAttribute('data-brand-id'), 10);
                if (!isNaN(id) && ids.indexOf(id) === -1) {
                    ids.push(id);
                }
            });
            return ids;
        }

        function removeAllSentinels() {
            container.querySelectorAll('[data-products-meta]').forEach(function (node) {
                if (node.parentNode) {
                    node.parentNode.removeChild(node);
                }
            });
        }

        function updateStateFromMeta() {
            var metaElements = container.querySelectorAll('[data-products-meta]');
            if (!metaElements || metaElements.length === 0) {
                productState.hasMore = false;
                var fallbackLoaded = getLoadedProductCount();
                productState.loadedCount = fallbackLoaded;
                if (typeof productState.totalCount !== 'number' || productState.totalCount < fallbackLoaded) {
                    productState.totalCount = fallbackLoaded;
                }
                attachObserver(null);
                updateResultsLabel();
                return;
            }

            var sentinel = metaElements[metaElements.length - 1];
            // Remove any older sentinels to avoid duplicates
            for (var i = 0; i < metaElements.length - 1; i++) {
                var el = metaElements[i];
                if (el && el.parentNode) {
                    el.parentNode.removeChild(el);
                }
            }

            var hasMoreAttr = sentinel.getAttribute('data-has-more');
            productState.hasMore = hasMoreAttr === 'true';

            var totalAttr = parseInt(sentinel.getAttribute('data-total-count'), 10);
            if (!isNaN(totalAttr) && totalAttr >= 0) {
                productState.totalCount = totalAttr;
            }

            var pageIndexAttr = parseInt(sentinel.getAttribute('data-page-index'), 10);
            if (!isNaN(pageIndexAttr) && pageIndexAttr > 0) {
                productState.pageIndex = pageIndexAttr;
                if (typeof productState.minLoadedPage !== 'number' || productState.minLoadedPage <= 0) {
                    productState.minLoadedPage = pageIndexAttr;
                } else {
                    productState.minLoadedPage = Math.min(productState.minLoadedPage, pageIndexAttr);
                }
                productState.maxLoadedPage = Math.max(productState.maxLoadedPage || 0, pageIndexAttr);
            }

            var nextPageAttr = parseInt(sentinel.getAttribute('data-next-page'), 10);
            if (!isNaN(nextPageAttr) && nextPageAttr > 0) {
                productState.nextPageIndex = nextPageAttr;
            } else {
                productState.nextPageIndex = productState.pageIndex + 1;
            }

            var pageSizeAttr = parseInt(sentinel.getAttribute('data-page-size'), 10);
            if (!isNaN(pageSizeAttr) && pageSizeAttr > 0) {
                productState.pageSize = pageSizeAttr;
            }

            attachObserver(productState.hasMore ? sentinel : null);
            productState.loadedCount = getLoadedProductCount();
            updateResultsLabel();
        }

        function fetchProducts(options) {
            options = options || {};
            if (productState.loading) {
                return;
            }

            var append = !!options.append;
            var pageIndex = options.pageIndex && options.pageIndex > 0 ? options.pageIndex : (append ? productState.nextPageIndex : 1);
            var pageSize = options.pageSize && options.pageSize > 0 ? options.pageSize : productState.pageSize;

            var slug = cfg.slug;
            if (!slug) {
                return;
            }

            productState.loading = true;
            container.classList.add('loading');

            if (!append) {
                productState.pageIndex = pageIndex;
                productState.nextPageIndex = pageIndex + 1;
                productState.hasMore = false;
                productState.minLoadedPage = pageIndex;
                productState.maxLoadedPage = pageIndex;
            } else {
                if (typeof productState.minLoadedPage !== 'number' || productState.minLoadedPage <= 0) {
                    productState.minLoadedPage = pageIndex;
                } else {
                    productState.minLoadedPage = Math.min(productState.minLoadedPage, pageIndex);
                }
                productState.maxLoadedPage = Math.max(productState.maxLoadedPage || 0, pageIndex);
            }

            var formData = new FormData();
            formData.append('Slug', slug);
            formData.append('Search', productState.searchTerm || '');
            formData.append('PageIndex', pageIndex);
            if (pageSize > 0) {
                formData.append('PageSize', pageSize);
            }

            // Include PlaceOfSupplyId from cookie (if selected in view)
            try {
                var cookieParts = ('; ' + document.cookie).split('; publicCatalogueSelectedStateId=');
                if (cookieParts.length === 2) {
                    var cookieValue = cookieParts.pop().split(';').shift();
                    if (cookieValue) {
                        formData.append('PlaceOfSupplyId', decodeURIComponent(cookieValue));
                    }
                }
            } catch (_) { /* ignore */ }

            var brandIds = collectBrandIds();
            brandIds.forEach(function (id) {
                formData.append('BrandIds', id);
            });

            var range = getSliderValues();
            if (range) {
                formData.append('MinPrice', range.min);
                formData.append('MaxPrice', range.max);
            }

            var includeOut = (includeOutDesktop && includeOutDesktop.checked) || (includeOutMobile && includeOutMobile.checked);
            formData.append('IncludeOutOfStock', includeOut ? 'true' : 'false');
            var sortOrderValue = parseInt(productState.sortOrder, 10);
            if (isNaN(sortOrderValue)) {
                sortOrderValue = 0;
            }
            formData.append('SortOrder', sortOrderValue);
            if (typeof productState.categoryId === 'number' && productState.categoryId > 0) {
                formData.append('CategoryId', productState.categoryId);
            }
            if (typeof productState.subCategoryId === 'number' && productState.subCategoryId > 0) {
                formData.append('SubCategoryId', productState.subCategoryId);
            }
            if (typeof productState.subSubCategoryId === 'number' && productState.subSubCategoryId > 0) {
                formData.append('SubSubCategoryId', productState.subSubCategoryId);
            }

            removeAllSentinels();

            fetch('/PublicCatalogue/RenderProducts', {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to load products');
                    }
                    return response.text();
                })
                .then(function (html) {
                    if (append) {
                        container.insertAdjacentHTML('beforeend', html);
                    } else {
                        container.innerHTML = html;
                        try {
                            window.scrollTo({ top: 0, behavior: 'smooth' });
                        } catch (_) {
                            window.scrollTo(0, 0);
                        }
                    }
                    updateStateFromMeta();
                })
                .catch(function (error) {
                    console.error(error);
                })
                .finally(function () {
                    productState.loading = false;
                    container.classList.remove('loading');
                });
        }

        function triggerFetch() {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(function () {
                fetchProducts({ append: false, pageIndex: 1 });
            }, 250);
        }

        function loadMoreProducts() {
            if (!productState.hasMore || productState.loading) {
                return;
            }
            fetchProducts({ append: true, pageIndex: productState.nextPageIndex });
        }

        doc.querySelectorAll('.form-check-input[data-brand-id]').forEach(function (input) {
            input.addEventListener('change', function () {
                syncCheckboxGroup(this);
                triggerFetch();
            });
        });

        if (includeOutDesktop) {
            includeOutDesktop.addEventListener('change', function () {
                syncAvailability(includeOutDesktop, includeOutMobile);
                triggerFetch();
            });
        }

        if (includeOutMobile) {
            includeOutMobile.addEventListener('change', function () {
                syncAvailability(includeOutMobile, includeOutDesktop);
                triggerFetch();
            });
        }

        if (desktopSlider && desktopSlider.noUiSlider) {
            desktopSlider.noUiSlider.on('change', triggerFetch);
        }

        if (mobileSlider && mobileSlider.noUiSlider) {
            mobileSlider.noUiSlider.on('change', triggerFetch);
        }

        // Refetch products when Place of Supply changes
        doc.addEventListener('placesOfSupplyChanged', function () {
            triggerFetch();
        });

        doc.querySelectorAll('.clear-btn').forEach(function (btn) {
            btn.addEventListener('click', function (event) {
                event.preventDefault();
                doc.querySelectorAll('.form-check-input[data-brand-id]').forEach(function (input) {
                    input.checked = false;
                });
                if (includeOutDesktop) includeOutDesktop.checked = false;
                if (includeOutMobile) includeOutMobile.checked = false;
                if (desktopSlider && desktopSlider.noUiSlider) desktopSlider.noUiSlider.reset();
                if (mobileSlider && mobileSlider.noUiSlider) mobileSlider.noUiSlider.reset();
                clearCategorySelection({ silent: true });
                triggerFetch();
                notifyAvailabilityChange();
            });
        });

        doc.querySelectorAll('.btn-cart[data-bs-dismiss="offcanvas"]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                triggerFetch();
            });
        });

        updateStateFromMeta();
        notifyAvailabilityChange();
    }

    function hideOutOfStock() {
        var cfg = getConfig();
        var settings = cfg.settings || {};
        if (settings.showOutOfStock === false) {
            doc.querySelectorAll('.stock-status').forEach(function (el) {
                if (!el) return;
                var text = (el.textContent || '').toLowerCase();
                if (text.indexOf('out of stock') !== -1) {
                    el.style.display = 'none';
                }
            });
        }
    }

    function initNestedDropdowns() {
        doc.querySelectorAll('.dropdown-submenu > .dropdown-toggle').forEach(function (toggleEl) {
            toggleEl.addEventListener('click', function (event) {
                event.preventDefault();
                event.stopPropagation();
                var submenu = this.nextElementSibling;
                var parentMenu = this.closest('.dropdown-menu');
                if (parentMenu) {
                    parentMenu.querySelectorAll('.dropdown-menu.show').forEach(function (openMenu) {
                        if (openMenu !== submenu) {
                            openMenu.classList.remove('show');
                        }
                    });
                }
                if (submenu) {
                    submenu.classList.toggle('show');
                }
            });
        });

        doc.querySelectorAll('.dropdown').forEach(function (dropdown) {
            dropdown.addEventListener('hide.bs.dropdown', function () {
                this.querySelectorAll('.dropdown-menu.show').forEach(function (menu) {
                    menu.classList.remove('show');
                });
            });
        });
    }

    function initHeaderSearch() {
        var cfg = getConfig();
        var btn = doc.getElementById('btnHeaderSearch');
        var box = doc.getElementById('headerSearch');
        var input = doc.getElementById('headerSearchInput');
        var closeBtn = doc.getElementById('btnHeaderSearchClose');
        var bridge = searchBridge || {};
        var suggestionsWrapper;
        var suggestionIdPrefix = 'headerSearchSuggestion-';
        var suggestionState = {
            debounce: null,
            controller: null,
            items: [],
            activeIndex: -1,
            pendingTerm: ''
        };
        var currencyFormatter = null;
        var supportsAbortController = typeof window.AbortController === 'function';

        if (!btn || !box) {
            return;
        }

        if (input) {
            input.setAttribute('role', 'combobox');
            input.setAttribute('aria-autocomplete', 'list');
            input.setAttribute('aria-haspopup', 'listbox');
            input.setAttribute('aria-expanded', 'false');
        }

        suggestionsWrapper = doc.createElement('div');
        suggestionsWrapper.className = 'search-suggestions d-none';
        suggestionsWrapper.setAttribute('role', 'listbox');
        suggestionsWrapper.id = 'headerSearchSuggestions';
        box.appendChild(suggestionsWrapper);

        if (input) {
            input.setAttribute('aria-controls', suggestionsWrapper.id);
        }

        if (bridge && typeof bridge.registerInput === 'function') {
            bridge.registerInput(input);
        }

        function formatPrice(value) {
            var num = typeof value === 'number' ? value : parseFloat(value);
            if (isNaN(num) || num <= 0) {
                return '';
            }
            if (!currencyFormatter) {
                try {
                    currencyFormatter = new Intl.NumberFormat('en-IN', {
                        style: 'currency',
                        currency: 'INR',
                        maximumFractionDigits: 0
                    });
                } catch (_) {
                    currencyFormatter = null;
                }
            }
            if (currencyFormatter) {
                return currencyFormatter.format(num);
            }
            return '\u20B9' + Math.round(num);
        }

        function updateToggleState(payload) {
            var normalized = '';
            var rawValue = '';
            if (payload && typeof payload === 'object') {
                normalized = typeof payload.normalized === 'string' ? payload.normalized : '';
                rawValue = typeof payload.raw === 'string' ? payload.raw : '';
            } else if (bridge && typeof bridge.getTerm === 'function') {
                normalized = bridge.getTerm() || '';
                if (typeof bridge.getRawValue === 'function') {
                    rawValue = bridge.getRawValue() || normalized;
                } else {
                    rawValue = normalized;
                }
            }
            var hasQuery = normalized.length > 0;
            if (btn) {
                if (hasQuery) {
                    btn.classList.add('has-query');
                    btn.setAttribute('aria-pressed', 'true');
                } else {
                    btn.classList.remove('has-query');
                    btn.removeAttribute('aria-pressed');
                }
            }
            if (closeBtn) {
                closeBtn.setAttribute('aria-label', hasQuery ? 'Clear search' : 'Close search');
            }
            if (input && !box.classList.contains('d-none') && typeof rawValue === 'string' && doc.activeElement !== input) {
                input.value = rawValue;
            }
            return { normalized: normalized, raw: rawValue, hasQuery: hasQuery };
        }

        function hideSuggestions() {
            if (suggestionState.debounce) {
                clearTimeout(suggestionState.debounce);
                suggestionState.debounce = null;
            }
            if (suggestionState.controller && supportsAbortController) {
                try {
                    suggestionState.controller.abort();
                } catch (_) { /* ignore */ }
            }
            suggestionState.controller = null;
            suggestionState.items = [];
            suggestionState.activeIndex = -1;
            suggestionState.pendingTerm = '';
            suggestionsWrapper.innerHTML = '';
            if (!suggestionsWrapper.classList.contains('d-none')) {
                suggestionsWrapper.classList.add('d-none');
            }
            if (input) {
                input.setAttribute('aria-expanded', 'false');
                input.removeAttribute('aria-activedescendant');
            }
        }

        function setBridgeListener() {
            if (!bridge) {
                return;
            }
            bridge.onTermChange = function (data) {
                var state = updateToggleState(data);
                if (!state.hasQuery || state.normalized.length < 2) {
                    hideSuggestions();
                }
            };
        }

        setBridgeListener();
        updateToggleState();

        function openSearch() {
            box.classList.remove('d-none');
            btn.classList.add('d-none');
            if (input) {
                var raw = '';
                if (bridge) {
                    if (typeof bridge.getRawValue === 'function') {
                        raw = bridge.getRawValue() || '';
                    } else if (typeof bridge.getTerm === 'function') {
                        raw = bridge.getTerm() || '';
                    }
                }
                if (typeof raw === 'string') {
                    input.value = raw;
                }
                input.focus();
                try {
                    if (typeof input.select === 'function' && input.value.length > 0) {
                        input.select();
                    }
                } catch (_) { /* ignore */ }
            }
        }

        function closeSearch() {
            hideSuggestions();
            box.classList.add('d-none');
            btn.classList.remove('d-none');
            if (input) {
                input.setAttribute('aria-expanded', 'false');
                input.removeAttribute('aria-activedescendant');
            }
        }

        function clearSearch() {
            hideSuggestions();
            if (bridge && typeof bridge.clear === 'function') {
                bridge.clear();
            } else if (bridge && typeof bridge.setTerm === 'function') {
                bridge.setTerm('', { force: true });
            }
            if (input) {
                input.focus();
                input.value = '';
            }
        }

        function highlightSuggestion(index) {
            suggestionState.activeIndex = index;
            var nodes = suggestionsWrapper.querySelectorAll('.search-suggestion-item');
            for (var i = 0; i < nodes.length; i++) {
                var node = nodes[i];
                if (i === index) {
                    node.classList.add('active');
                    node.setAttribute('aria-selected', 'true');
                    if (input) {
                        input.setAttribute('aria-activedescendant', node.id || '');
                    }
                } else {
                    node.classList.remove('active');
                    node.setAttribute('aria-selected', 'false');
                }
            }
            if (index < 0 && input) {
                input.removeAttribute('aria-activedescendant');
            }
        }

        function selectSuggestion(index) {
            if (!suggestionState.items || index < 0 || index >= suggestionState.items.length) {
                return;
            }
            var suggestion = suggestionState.items[index] || {};
            var label = suggestion.label || suggestion.name || '';
            if (bridge && typeof bridge.setTerm === 'function') {
                bridge.setTerm(label, { force: true });
            } else if (input) {
                input.value = label;
            }
            hideSuggestions();
            if (input) {
                input.focus();
            }
        }

        function renderSuggestions(items) {
            if (!items || items.length === 0) {
                hideSuggestions();
                return;
            }
            var list = items.slice(0, 8);
            suggestionState.items = list;
            suggestionState.activeIndex = -1;
            suggestionsWrapper.innerHTML = '';
            var fragment = doc.createDocumentFragment();
            for (var i = 0; i < list.length; i++) {
                var item = list[i] || {};
                var button = doc.createElement('button');
                button.type = 'button';
                button.className = 'search-suggestion-item';
                button.id = suggestionIdPrefix + i;
                button.setAttribute('role', 'option');
                button.setAttribute('aria-selected', 'false');
                button.setAttribute('tabindex', '-1');
                button.setAttribute('data-index', String(i));
                button.addEventListener('click', function (event) {
                    event.preventDefault();
                    var idx = parseInt(this.getAttribute('data-index'), 10);
                    if (!isNaN(idx)) {
                        selectSuggestion(idx);
                    }
                });
                button.addEventListener('mouseenter', function () {
                    var idx = parseInt(this.getAttribute('data-index'), 10);
                    if (!isNaN(idx)) {
                        highlightSuggestion(idx);
                    }
                });
                button.addEventListener('focus', function () {
                    var idx = parseInt(this.getAttribute('data-index'), 10);
                    if (!isNaN(idx)) {
                        highlightSuggestion(idx);
                    }
                });
                var textWrapper = doc.createElement('div');
                textWrapper.className = 'suggestion-text';
                var titleEl = doc.createElement('div');
                titleEl.className = 'suggestion-title';
                titleEl.textContent = item.label || item.name || '';
                textWrapper.appendChild(titleEl);
                if (item.sku && String(item.sku).trim().length > 0) {
                    var metaEl = doc.createElement('div');
                    metaEl.className = 'suggestion-meta';
                    metaEl.textContent = String(item.sku).trim();
                    textWrapper.appendChild(metaEl);
                }
                button.appendChild(textWrapper);
                var priceValue = null;
                if (typeof item.price === 'number') {
                    priceValue = item.price;
                } else if (typeof item.price === 'string') {
                    var parsed = parseFloat(item.price);
                    if (!isNaN(parsed)) {
                        priceValue = parsed;
                    }
                }
                if (priceValue && priceValue > 0) {
                    var priceEl = doc.createElement('div');
                    priceEl.className = 'suggestion-price';
                    priceEl.textContent = formatPrice(priceValue);
                    button.appendChild(priceEl);
                }
                fragment.appendChild(button);
            }
            suggestionsWrapper.appendChild(fragment);
            suggestionsWrapper.classList.remove('d-none');
            if (input) {
                input.setAttribute('aria-expanded', 'true');
                input.removeAttribute('aria-activedescendant');
            }
        }

        function fetchSuggestions(term) {
            if (!cfg || !cfg.slug) {
                return;
            }
            suggestionState.pendingTerm = term;
            if (suggestionState.controller && supportsAbortController) {
                try {
                    suggestionState.controller.abort();
                } catch (_) { /* ignore */ }
            }
            var controller = supportsAbortController ? new AbortController() : null;
            if (controller) {
                suggestionState.controller = controller;
            } else {
                suggestionState.controller = null;
            }
            var includeOutOfStock = bridge && typeof bridge.getIncludeOutOfStock === 'function'
                ? !!bridge.getIncludeOutOfStock()
                : false;
            var brandIds = [];
            if (bridge && typeof bridge.getSelectedBrandIds === 'function') {
                var selectedIds = bridge.getSelectedBrandIds();
                if (Array.isArray(selectedIds)) {
                    brandIds = selectedIds.slice();
                }
            }
            var priceRange = bridge && typeof bridge.getPriceRange === 'function'
                ? bridge.getPriceRange()
                : null;

            var requestInit = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: (function(){
                    var payload = {
                        Slug: cfg.slug,
                        Term: term,
                        Limit: 8,
                        IncludeOutOfStock: includeOutOfStock,
                        BrandIds: brandIds,
                        MinPrice: priceRange && typeof priceRange.min === 'number' ? priceRange.min : null,
                        MaxPrice: priceRange && typeof priceRange.max === 'number' ? priceRange.max : null
                    };
                    // Include PlaceOfSupplyId from cookie if available
                    try {
                        var cookiePartsSS = ('; ' + document.cookie).split('; publicCatalogueSelectedStateId=');
                        if (cookiePartsSS.length === 2) {
                            var cookieValueSS = cookiePartsSS.pop().split(';').shift();
                            if (cookieValueSS) {
                                payload.PlaceOfSupplyId = decodeURIComponent(cookieValueSS);
                            }
                        }
                    } catch (_) { /* ignore */ }
                    return JSON.stringify(payload);
                })(),
                credentials: 'same-origin'
            };
            if (controller) {
                requestInit.signal = controller.signal;
            }
            fetch('/PublicCatalogue/SearchSuggestions', requestInit)
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to fetch suggestions');
                    }
                    return response.json();
                })
                .then(function (data) {
                    if (controller && controller.signal.aborted) {
                        return;
                    }
                    if (suggestionState.pendingTerm !== term) {
                        return;
                    }
                    var suggestions = [];
                    if (data) {
                        if (Array.isArray(data.suggestions)) {
                            suggestions = data.suggestions;
                        } else if (Array.isArray(data.items)) {
                            suggestions = data.items;
                        }
                    }
                    renderSuggestions(suggestions);
                })
                .catch(function (error) {
                    if (controller && controller.signal && controller.signal.aborted) {
                        return;
                    }
                    console.error(error);
                    hideSuggestions();
                })
                .finally(function () {
                    if (controller === suggestionState.controller) {
                        suggestionState.controller = null;
                    }
                });
        }

        function scheduleSuggestions(rawValue) {
            if (suggestionState.debounce) {
                clearTimeout(suggestionState.debounce);
            }
            var term = (rawValue || '').trim();
            if (!term || term.length < 2) {
                hideSuggestions();
                return;
            }
            suggestionState.debounce = setTimeout(function () {
                fetchSuggestions(term);
            }, 180);
        }

        function handleInputKeyDown(event) {
            if (event.key === 'ArrowDown') {
                event.preventDefault();
                if (!suggestionState.items || suggestionState.items.length === 0) {
                    scheduleSuggestions(input.value || '');
                    return;
                }
                var nextIndex = suggestionState.activeIndex + 1;
                if (nextIndex >= suggestionState.items.length) {
                    nextIndex = 0;
                }
                highlightSuggestion(nextIndex);
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                if (!suggestionState.items || suggestionState.items.length === 0) {
                    scheduleSuggestions(input.value || '');
                    return;
                }
                var prevIndex = suggestionState.activeIndex - 1;
                if (prevIndex < 0) {
                    prevIndex = suggestionState.items.length - 1;
                }
                highlightSuggestion(prevIndex);
            } else if (event.key === 'Enter') {
                var hasActiveSuggestion = suggestionState.activeIndex >= 0 && suggestionState.activeIndex < suggestionState.items.length;
                if (hasActiveSuggestion) {
                    event.preventDefault();
                    selectSuggestion(suggestionState.activeIndex);
                } else {
                    event.preventDefault();
                    hideSuggestions();
                    if (bridge && typeof bridge.setTerm === 'function') {
                        bridge.setTerm(input.value || '', { syncInput: false, force: true });
                    }
                }
            } else if (event.key === 'Escape') {
                if (suggestionState.items && suggestionState.items.length > 0 && !suggestionsWrapper.classList.contains('d-none')) {
                    event.preventDefault();
                    hideSuggestions();
                    return;
                }
                closeSearch();
            }
        }

        btn.addEventListener('click', function (event) {
            event.preventDefault();
            if (box.classList.contains('d-none')) {
                openSearch();
                if (input && input.value && input.value.trim().length >= 2) {
                    scheduleSuggestions(input.value);
                }
            } else {
                closeSearch();
            }
        });

        if (closeBtn) {
            closeBtn.addEventListener('click', function (event) {
                event.preventDefault();
                if (bridge && typeof bridge.getTerm === 'function' && (bridge.getTerm() || '').length > 0) {
                    clearSearch();
                } else {
                    closeSearch();
                }
            });
        }

        if (input) {
            input.addEventListener('input', function () {
                var raw = input.value || '';
                if (bridge && typeof bridge.setTerm === 'function') {
                    bridge.setTerm(raw, { syncInput: false, silent: true });
                }
                scheduleSuggestions(raw);
            });
            input.addEventListener('focus', function () {
                if (input.value && input.value.trim().length >= 2 && (!suggestionState.items || suggestionState.items.length === 0)) {
                    scheduleSuggestions(input.value);
                }
            });
            input.addEventListener('keydown', handleInputKeyDown);
        }

        doc.addEventListener('keydown', function (event) {
            if (event.key === 'Escape' && !box.classList.contains('d-none')) {
                if (suggestionState.items && suggestionState.items.length > 0 && !suggestionsWrapper.classList.contains('d-none')) {
                    hideSuggestions();
                    return;
                }
                closeSearch();
            }
        });

        doc.addEventListener('click', function (event) {
            if (!box.contains(event.target) && !(btn && btn.contains(event.target))) {
                if (!box.classList.contains('d-none')) {
                    closeSearch();
                }
            }
        });
    }

    function initWhatsAppEnquiry() {
        var cfg = getConfig();
        var digits = (cfg.whatsappDigits || '').replace(/[^0-9]/g, '');
        if (!digits) {
            return;
        }

        window.waPhone = digits;
        var enquiryLink = doc.getElementById('whatsappEnquiry');
        if (enquiryLink) {
            var message = encodeURIComponent('Hi! I have an enquiry about ' + doc.title + ' - ' + window.location.href);
            var link = 'https://wa.me/' + digits + '?text=' + message;
            enquiryLink.setAttribute('href', link);
        }

        var buttons = doc.querySelectorAll('.btn-wa-enquiry');
        if (buttons && buttons.length > 0) {
            buttons.forEach(function (btn) {
                btn.addEventListener('click', function (event) {
                    event.preventDefault();
                    var card = this.closest('.product-card');
                    var titleEl = card ? card.querySelector('.fw-semibold.mt-1') : null;
                    var title = titleEl ? titleEl.textContent.trim() : doc.title;
                    var msg = encodeURIComponent('Hi! I want to enquire about: ' + title + ' (' + window.location.href + ')');
                    var url = 'https://wa.me/' + digits + '?text=' + msg;
                    window.open(url, '_blank');
                });
            });
        }
    }

    // Make initOptionLists accessible globally for use in modals
    function initOptionLists() {
        var lists = doc.querySelectorAll('.options-list');
        lists.forEach(function (list) {
            var max = parseInt(list.getAttribute('data-max-visible') || '4', 10);
            var items = Array.prototype.slice.call(list.querySelectorAll('.option-chip'));
            if (items.length > max) {
                items.forEach(function (chip, index) {
                    if (index >= max) chip.style.display = 'none';
                });
                // Remove existing show-more button if any
                var existingBtn = list.querySelector('.show-more-btn');
                if (existingBtn) existingBtn.remove();
                
                var button = doc.createElement('button');
                button.type = 'button';
                button.className = 'show-more-btn';
                button.textContent = '+' + (items.length - max);
                button.addEventListener('click', function () {
                    var hidden = list.querySelectorAll('.option-chip[style*="display: none"]');
                    var isCollapsed = hidden.length > 0;
                    items.forEach(function (chip, index) {
                        if (index >= max) chip.style.display = isCollapsed ? '' : 'none';
                    });
                    button.textContent = isCollapsed ? 'Show less' : '+' + (items.length - max);
                });
                list.appendChild(button);
            }
        });
    }

    function initVariationChips() {

        function attachChipHandlers() {
            doc.addEventListener('click', function (event) {
                var chip = event.target.closest('.option-chip');
                if (!chip || chip.getAttribute('aria-disabled') === 'true') {
                    return;
                }
                var list = chip.closest('.options-list');
                if (!list) {
                    return;
                }
                
                // Update active state
                list.querySelectorAll('.option-chip').forEach(function (c) { c.classList.remove('active'); });
                chip.classList.add('active');
                
                // If this is in product details modal, find and update matching variant
                var modalEl = chip.closest('#productDetailsModal');
                if (modalEl) {
                    try {
                        var variantsJsonAttr = modalEl.getAttribute('data-variants-json');
                        if (variantsJsonAttr) {
                            var decodedVariantsJson = decodeHtml(variantsJsonAttr);
                            var variants = JSON.parse(decodedVariantsJson);
                            
                            if (Array.isArray(variants) && variants.length > 0) {
                                var selectedOptions = getSelectedOptionsFromModal(modalEl);
                                
                                // Update chip availability based on new selections
                                updateChipAvailability(modalEl, variants, selectedOptions);
                                
                                // Find and update matching variant
                                var matchedVariant = findMatchingVariant(variants, selectedOptions);
                                
                                if (matchedVariant) {
                                    var cfg = getConfig();
                                    var modalSettings = cfg.settings || {};
                                    updateProductDetailsWithVariant(modalEl, matchedVariant, modalSettings);
                                } else {
                                    // If no variant matches (partial selection), try to auto-select first available
                                    // in other attributes to create a valid combination (like Flipkart does)
                                    var variantGroups = modalEl.querySelector('#pdVariantGroups');
                                    if (variantGroups) {
                                        var optionLists = variantGroups.querySelectorAll('.options-list');
                                        var attrKeys = [];
                                        var hasAnySelection = Object.keys(selectedOptions).length > 0;
                                        
                                        optionLists.forEach(function(list) {
                                            var labelEl = list.previousElementSibling;
                                            if (labelEl && labelEl.classList.contains('small')) {
                                                attrKeys.push(labelEl.textContent.trim());
                                            }
                                        });
                                        
                                        // Only auto-select if we have at least one selection and missing others
                                        // This ensures we always try to have a valid combination
                                        if (hasAnySelection) {
                                            var updatedSelections = {};
                                            Object.keys(selectedOptions).forEach(function(k) {
                                                updatedSelections[k] = selectedOptions[k];
                                            });
                                            
                                            // Auto-select first available for unselected attributes
                                            attrKeys.forEach(function(key) {
                                                if (!selectedOptions[key]) {
                                                    var selectedValue = autoSelectFirstAvailable(modalEl, variants, updatedSelections, key);
                                                    if (selectedValue) {
                                                        updatedSelections[key] = selectedValue;
                                                    }
                                                }
                                            });
                                            
                                            // Update chip availability again after auto-selections
                                            updateChipAvailability(modalEl, variants, updatedSelections);
                                            
                                            // Get final selections and match variant
                                            var finalSelections = getSelectedOptionsFromModal(modalEl);
                                            matchedVariant = findMatchingVariant(variants, finalSelections);
                                            if (matchedVariant) {
                                                var cfg2 = getConfig();
                                                var modalSettings2 = cfg2.settings || {};
                                                updateProductDetailsWithVariant(modalEl, matchedVariant, modalSettings2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } catch (e) {
                        console.warn('Error updating variant on chip selection:', e);
                    }
                }
            });
        }

        function openVariantModal(card) {
            var modalEl = doc.getElementById('variantModal');
            if (!modalEl || !card) return;

            var titleEl = card.querySelector('.fw-semibold.mt-1');
            var title = titleEl ? titleEl.textContent.trim() : 'Select Options';
            
            // Collect all unique attribute values from all variants
            var attrs = {};
            var variantsJson = card.getAttribute('data-variants');
            var itemDetailsId = card.getAttribute('data-item-details-id');
            
            if (variantsJson) {
                try {
                    var decodedVariantsJson = decodeHtml(variantsJson);
                    var variants = JSON.parse(decodedVariantsJson);
                    
                    if (Array.isArray(variants)) {
                        // Iterate through all variants to collect unique attribute values
                        variants.forEach(function(variant) {
                            if (variant && variant.AttributesJson) {
                                try {
                                    var variantAttrsJson = variant.AttributesJson;
                                    if (typeof variantAttrsJson === 'string') {
                                        variantAttrsJson = decodeHtml(variantAttrsJson);
                                    }
                                    var variantAttrs = JSON.parse(variantAttrsJson);
                                    
                                    if (variantAttrs && typeof variantAttrs === 'object') {
                                        // Collect all unique values for each attribute key
                                        Object.keys(variantAttrs).forEach(function(key) {
                                            if (!attrs[key]) {
                                                attrs[key] = [];
                                            }
                                            var values = variantAttrs[key];
                                            if (Array.isArray(values)) {
                                                values.forEach(function(val) {
                                                    var valStr = String(val);
                                                    // Check if value already exists (case-insensitive for non-color values)
                                                    var exists = false;
                                                    if (typeof val === 'string' && /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(valStr)) {
                                                        // For colors, exact match
                                                        exists = attrs[key].some(function(existing) {
                                                            return String(existing).toLowerCase() === valStr.toLowerCase();
                                                        });
                                                    } else {
                                                        // For other values, case-insensitive match
                                                        exists = attrs[key].some(function(existing) {
                                                            return String(existing).toLowerCase() === valStr.toLowerCase();
                                                        });
                                                    }
                                                    if (!exists) {
                                                        attrs[key].push(val);
                                                    }
                                                });
                                            } else if (values !== null && values !== undefined) {
                                                // Single value
                                                var valStr = String(values);
                                                var exists = attrs[key].some(function(existing) {
                                                    return String(existing).toLowerCase() === valStr.toLowerCase();
                                                });
                                                if (!exists) {
                                                    attrs[key].push(values);
                                                }
                                            }
                                        });
                                    }
                                } catch (e) {
                                    console.warn('Failed to parse variant attributes JSON:', variant.AttributesJson, e);
                                }
                            }
                        });
                    }
                } catch (e) {
                    console.warn('Failed to parse variants JSON:', variantsJson, e);
                }
            }
            
            // Fallback to data-attrs if no variants found
            if (!attrs || Object.keys(attrs).length === 0) {
                var json = card.getAttribute('data-attrs');
                try { 
                    var decodedJson = json ? decodeHtml(json) : '';
                    attrs = decodedJson ? JSON.parse(decodedJson) : {}; 
                } catch (err) { 
                    attrs = {}; 
                }
            }

            var titleTarget = modalEl.querySelector('#variantProductTitle');
            var groupsTarget = modalEl.querySelector('#variantGroups');
            if (titleTarget) titleTarget.textContent = title;
            if (groupsTarget) {
                groupsTarget.innerHTML = '';
                var attrKeys = Object.keys(attrs);
                
                attrKeys.forEach(function (key) {
                    var values = attrs[key] || [];
                    var wrap = doc.createElement('div');
                    wrap.className = 'mb-3';

                    var label = doc.createElement('div');
                    label.className = 'small text-muted mb-1';
                    label.textContent = key;
                    wrap.appendChild(label);

                    var list = doc.createElement('div');
                    list.className = 'options-list';
                    list.setAttribute('data-max-visible', '8');
                    values.forEach(function (val, idx) {
                        var chip = doc.createElement('span');
                        if (typeof val === 'string' && /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(val)) {
                            chip.className = 'badge badge-chip option-chip px-2 py-2' + (idx === 0 ? ' active' : '');
                            chip.style.borderRadius = '999px';
                            chip.style.padding = '0.25rem';
                            var dot = doc.createElement('span');
                            dot.style.display = 'inline-block';
                            dot.style.width = '18px';
                            dot.style.height = '18px';
                            dot.style.borderRadius = '999px';
                            dot.style.border = '1px solid var(--surface-border)';
                            dot.style.background = String(val);
                            dot.title = String(val);
                            chip.appendChild(dot);
                        } else {
                            chip.className = 'badge badge-chip option-chip px-3 py-2' + (idx === 0 ? ' active' : '');
                            chip.textContent = String(val);
                        }
                        list.appendChild(chip);
                    });
                    wrap.appendChild(list);
                    groupsTarget.appendChild(wrap);
                });
                
                try { initOptionLists(); } catch (_) { /* ignore */ }
            }

            if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
                modal.show();
            }
        }

        function attachSelectOptions() {
            doc.querySelectorAll('.btn-select-options').forEach(function (button) {
                button.addEventListener('click', function () {
                    var card = this.closest('.product-card');
                    if (card) openVariantModal(card);
                });
            });
        }

        initOptionLists();
        attachChipHandlers();
        attachSelectOptions();

        try {
            doc.querySelectorAll('.product-card .fw-semibold.mt-1').forEach(function (el) {
                if (!el || !el.textContent) return;
                el.textContent = el.textContent.replace(/\.\.\.$/, '');
            });
        } catch (_) { /* ignore */ }
    }

    // Helper function to get selected options from product details modal
    function getSelectedOptionsFromModal(modalEl) {
        var selected = {};
        var variantGroups = modalEl.querySelector('#pdVariantGroups');
        if (!variantGroups) return selected;
        
        var optionLists = variantGroups.querySelectorAll('.options-list');
        optionLists.forEach(function(list) {
            var labelEl = list.previousElementSibling;
            if (!labelEl || !labelEl.classList.contains('small')) return;
            var key = labelEl.textContent.trim();
            if (!key) return;
            
            var activeChip = list.querySelector('.option-chip.active');
            if (activeChip) {
                var dot = activeChip.querySelector('span[style*="background"]');
                if (dot) {
                    // Color chip with dot - get from title attribute first, then style
                    selected[key] = dot.title || dot.style.background || '';
                } else {
                    // Text chip
                    selected[key] = activeChip.textContent.trim();
                }
            }
        });
        return selected;
    }

    // Helper function to check if a value matches variant attribute value (case-insensitive)
    function valuesMatch(val1, val2) {
        var str1 = String(val1);
        var str2 = String(val2);
        // For hex colors, case-insensitive exact match
        if (/^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(str1) || /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(str2)) {
            return str1.toLowerCase() === str2.toLowerCase();
        }
        // For other values, case-insensitive match
        return str1.toLowerCase() === str2.toLowerCase();
    }

    // Helper function to find available options for a specific attribute based on current selections
    function findAvailableOptionsForAttribute(variants, currentSelections, targetAttributeKey) {
        if (!Array.isArray(variants) || variants.length === 0) return [];
        
        var availableValues = [];
        var availableSet = new Set();
        
        for (var i = 0; i < variants.length; i++) {
            var variant = variants[i];
            if (!variant || !variant.AttributesJson) continue;
            
            try {
                var variantAttrsJson = variant.AttributesJson;
                if (typeof variantAttrsJson === 'string') {
                    variantAttrsJson = decodeHtml(variantAttrsJson);
                }
                var variantAttrs = JSON.parse(variantAttrsJson);
                
                if (!variantAttrs || typeof variantAttrs !== 'object') continue;
                
                // Check if this variant matches all current selections (except the target attribute)
                var matchesCurrentSelections = true;
                var currentKeys = Object.keys(currentSelections || {});
                
                for (var j = 0; j < currentKeys.length; j++) {
                    var key = currentKeys[j];
                    // Skip the target attribute we're checking availability for
                    if (key === targetAttributeKey) continue;
                    
                    var selectedValue = currentSelections[key];
                    var variantValues = variantAttrs[key];
                    
                    if (!variantValues) {
                        matchesCurrentSelections = false;
                        break;
                    }
                    
                    var found = false;
                    if (Array.isArray(variantValues)) {
                        for (var k = 0; k < variantValues.length; k++) {
                            if (valuesMatch(variantValues[k], selectedValue)) {
                                found = true;
                                break;
                            }
                        }
                    } else {
                        found = valuesMatch(variantValues, selectedValue);
                    }
                    
                    if (!found) {
                        matchesCurrentSelections = false;
                        break;
                    }
                }
                
                // If variant matches current selections, add its value for target attribute
                if (matchesCurrentSelections && variantAttrs[targetAttributeKey]) {
                    var targetValues = variantAttrs[targetAttributeKey];
                    if (Array.isArray(targetValues)) {
                        targetValues.forEach(function(val) {
                            var valKey = String(val).toLowerCase();
                            if (!availableSet.has(valKey)) {
                                availableSet.add(valKey);
                                availableValues.push(val);
                            }
                        });
                    } else if (targetValues !== null && targetValues !== undefined) {
                        var valKey = String(targetValues).toLowerCase();
                        if (!availableSet.has(valKey)) {
                            availableSet.add(valKey);
                            availableValues.push(targetValues);
                        }
                    }
                }
            } catch (e) {
                console.warn('Error finding available options:', e);
                continue;
            }
        }
        
        return availableValues;
    }

    // Helper function to get chip value (text or color)
    function getChipValue(chip) {
        var dot = chip.querySelector('span[style*="background"]');
        if (dot) {
            return dot.title || dot.style.background || '';
        }
        return chip.textContent.trim();
    }

    // Helper function to update chip availability based on available options
    function updateChipAvailability(modalEl, variants, currentSelections) {
        if (!modalEl || !variants || !Array.isArray(variants)) return;
        
        var variantGroups = modalEl.querySelector('#pdVariantGroups');
        if (!variantGroups) return;
        
        var optionLists = variantGroups.querySelectorAll('.options-list');
        optionLists.forEach(function(list) {
            var labelEl = list.previousElementSibling;
            if (!labelEl || !labelEl.classList.contains('small')) return;
            var attributeKey = labelEl.textContent.trim();
            if (!attributeKey) return;
            
            // Find available options for this attribute
            var availableOptions = findAvailableOptionsForAttribute(variants, currentSelections, attributeKey);
            var availableSet = new Set();
            availableOptions.forEach(function(opt) {
                availableSet.add(String(opt).toLowerCase());
            });
            
            // Check if currently active chip is still available
            var activeChip = list.querySelector('.option-chip.active');
            var activeChipAvailable = true;
            if (activeChip) {
                var activeChipValue = getChipValue(activeChip);
                var activeChipValueKey = String(activeChipValue).toLowerCase();
                activeChipAvailable = availableSet.has(activeChipValueKey);
                
                // If active chip is no longer available, deselect it
                if (!activeChipAvailable) {
                    activeChip.classList.remove('active');
                    activeChip.setAttribute('aria-disabled', 'true');
                    activeChip.style.opacity = '0.5';
                    activeChip.style.cursor = 'not-allowed';
                    activeChip.style.textDecoration = 'line-through';
                }
            }
            
            // Update all chips
            var chips = list.querySelectorAll('.option-chip');
            chips.forEach(function(chip) {
                var chipValue = getChipValue(chip);
                var chipValueKey = String(chipValue).toLowerCase();
                var isAvailable = availableSet.has(chipValueKey);
                
                if (isAvailable) {
                    chip.removeAttribute('aria-disabled');
                    chip.style.opacity = '';
                    chip.style.cursor = 'pointer';
                    chip.style.textDecoration = '';
                } else {
                    chip.setAttribute('aria-disabled', 'true');
                    chip.style.opacity = '0.5';
                    chip.style.cursor = 'not-allowed';
                    chip.style.textDecoration = 'line-through';
                    // Also remove active state if it was active
                    if (chip.classList.contains('active')) {
                        chip.classList.remove('active');
                    }
                }
            });
        });
    }

    // Helper function to auto-select first available option in an attribute if none selected
    function autoSelectFirstAvailable(modalEl, variants, currentSelections, attributeKey) {
        if (!modalEl) return null;
        
        var variantGroups = modalEl.querySelector('#pdVariantGroups');
        if (!variantGroups) return null;
        
        // Find the options list for this attribute
        var optionLists = variantGroups.querySelectorAll('.options-list');
        var targetList = null;
        var labelEl = null;
        
        optionLists.forEach(function(list) {
            var label = list.previousElementSibling;
            if (label && label.classList.contains('small') && label.textContent.trim() === attributeKey) {
                targetList = list;
                labelEl = label;
            }
        });
        
        if (!targetList) return null;
        
        // Check if already has an active chip
        var activeChip = targetList.querySelector('.option-chip.active');
        if (activeChip && activeChip.getAttribute('aria-disabled') !== 'true') {
            return getChipValue(activeChip);
        }
        
        // Find first available option
        var availableOptions = findAvailableOptionsForAttribute(variants, currentSelections, attributeKey);
        if (availableOptions.length === 0) return null;
        
        var firstAvailable = availableOptions[0];
        var chips = targetList.querySelectorAll('.option-chip');
        
        // Find and activate the chip matching first available option
        for (var i = 0; i < chips.length; i++) {
            var chip = chips[i];
            var chipValue = getChipValue(chip);
            if (valuesMatch(chipValue, firstAvailable)) {
                // Remove active from all chips in this list
                targetList.querySelectorAll('.option-chip').forEach(function(c) {
                    c.classList.remove('active');
                });
                // Activate this chip
                chip.classList.add('active');
                chip.removeAttribute('aria-disabled');
                chip.style.opacity = '';
                chip.style.cursor = 'pointer';
                chip.style.textDecoration = '';
                return chipValue;
            }
        }
        
        return null;
    }

    // Helper function to find matching variant based on selected options
    function findMatchingVariant(variants, selectedOptions) {
        if (!Array.isArray(variants) || variants.length === 0) return null;
        if (!selectedOptions || Object.keys(selectedOptions).length === 0) return null;
        
        var selectedKeys = Object.keys(selectedOptions);
        if (selectedKeys.length === 0) return null;
        
        for (var i = 0; i < variants.length; i++) {
            var variant = variants[i];
            if (!variant || !variant.AttributesJson) continue;
            
            try {
                var variantAttrsJson = variant.AttributesJson;
                if (typeof variantAttrsJson === 'string') {
                    variantAttrsJson = decodeHtml(variantAttrsJson);
                }
                var variantAttrs = JSON.parse(variantAttrsJson);
                
                if (!variantAttrs || typeof variantAttrs !== 'object') continue;
                
                var matches = true;
                for (var j = 0; j < selectedKeys.length; j++) {
                    var key = selectedKeys[j];
                    var selectedValue = selectedOptions[key];
                    var variantValues = variantAttrs[key];
                    
                    if (!variantValues) {
                        matches = false;
                        break;
                    }
                    
                    var found = false;
                    if (Array.isArray(variantValues)) {
                        for (var k = 0; k < variantValues.length; k++) {
                            var variantValue = String(variantValues[k]);
                            var selectedStr = String(selectedValue);
                            // Case-insensitive comparison, exact match for colors
                            if (/^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(selectedStr)) {
                                if (variantValue.toLowerCase() === selectedStr.toLowerCase()) {
                                    found = true;
                                    break;
                                }
                            } else {
                                if (variantValue.toLowerCase() === selectedStr.toLowerCase()) {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    } else {
                        var variantValueStr = String(variantValues);
                        var selectedStr = String(selectedValue);
                        if (/^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(selectedStr)) {
                            found = variantValueStr.toLowerCase() === selectedStr.toLowerCase();
                        } else {
                            found = variantValueStr.toLowerCase() === selectedStr.toLowerCase();
                        }
                    }
                    
                    if (!found) {
                        matches = false;
                        break;
                    }
                }
                
                if (matches) {
                    return variant;
                }
            } catch (e) {
                console.warn('Error matching variant:', e);
                continue;
            }
        }
        
        return null;
    }

    // Helper function to update product details modal with variant data
    function updateProductDetailsWithVariant(modalEl, variant, settings) {
        if (!modalEl || !variant) return;
        
        var allowPrices = settings.showPrices !== false;
        var allowMRP = settings.showMRP !== false;
        var allowDiscount = settings.showDiscount !== false;
        var allowStock = settings.showStock !== false;
        var allowImages = settings.showImages !== false;
        
        var pdPrice = modalEl.querySelector('#pdPrice');
        var pdPriceOld = modalEl.querySelector('#pdPriceOld');
        var pdOff = modalEl.querySelector('#pdOff');
        var pdPriceMeta = modalEl.querySelector('#pdPriceMeta');
        var pdImage = modalEl.querySelector('#pdImage');
        var pdStockNote = modalEl.querySelector('#pdStockNote');
        var pdCode = modalEl.querySelector('#pdCode');
        
        // Update price
        var salesPrice = parseFloat(variant.SalesIncTax) || 0;
        var mrp = parseFloat(variant.DefaultMrp) || 0;
        
        if (pdPrice && allowPrices) {
            pdPrice.textContent = '\u20B9' + salesPrice.toFixed(2);
        }
        
        // Update MRP
        if (pdPriceOld && allowPrices && allowMRP) {
            if (mrp > salesPrice && mrp > 0) {
                pdPriceOld.style.display = '';
                pdPriceOld.textContent = '\u20B9' + mrp.toFixed(2);
            } else {
                pdPriceOld.style.display = 'none';
            }
        }
        
        // Update discount
        if (pdOff && allowDiscount) {
            if (mrp > salesPrice && mrp > 0) {
                var discount = Math.round(((mrp - salesPrice) / mrp) * 100);
                pdOff.style.display = '';
                pdOff.textContent = discount + '% off';
            } else {
                pdOff.style.display = 'none';
            }
        }
        
        // Update price meta
        var save = mrp > salesPrice ? (mrp - salesPrice) : 0;
        if (pdPriceMeta) {
            pdPriceMeta.style.display = allowPrices && allowDiscount && save > 0 ? '' : 'none';
            pdPriceMeta.textContent = allowPrices && allowDiscount && save > 0 ? ('You save \u20B9' + save.toFixed(0)) : '';
        }
        
        // Update image
        if (pdImage && allowImages && variant.ProductImage) {
            var newImageSrc = variant.ProductImage;
            var currentSrc = pdImage.getAttribute('src') || '';
            
            // Only update if the source is actually different
            if (newImageSrc !== currentSrc) {
                // Reset panzoom transform and instance before changing image
                try {
                    if (window._pdPanzoomInstance && typeof window._pdPanzoomInstance.reset === 'function') {
                        window._pdPanzoomInstance.reset({ animate: false });
                    }
                } catch (_) { /* ignore */ }
                if (pdImage.style.transform) {
                    pdImage.style.transform = '';
                }
                
                // Update image source
                pdImage.setAttribute('src', newImageSrc);
                
                // Reset transform after image loads (or immediately if already loaded/cached)
                var resetOnLoad = function() {
                    try {
                        if (window._pdPanzoomInstance && typeof window._pdPanzoomInstance.reset === 'function') {
                            window._pdPanzoomInstance.reset({ animate: false });
                        }
                    } catch (_) { /* ignore */ }
                    if (pdImage.style.transform) {
                        pdImage.style.transform = '';
                    }
                    pdImage.removeEventListener('load', resetOnLoad);
                    pdImage.removeEventListener('error', resetOnLoad);
                };
                
                // Check if image is already loaded (cached images load instantly)
                if (pdImage.complete && pdImage.naturalHeight !== 0) {
                    // Image is already loaded, reset immediately
                    setTimeout(resetOnLoad, 10);
                } else {
                    // Wait for image to load
                    pdImage.addEventListener('load', resetOnLoad, { once: true });
                    pdImage.addEventListener('error', resetOnLoad, { once: true });
                }
            } else {
                // Same image source, just ensure transform is reset
                try {
                    if (window._pdPanzoomInstance && typeof window._pdPanzoomInstance.reset === 'function') {
                        window._pdPanzoomInstance.reset({ animate: false });
                    }
                } catch (_) { /* ignore */ }
                if (pdImage.style.transform) {
                    pdImage.style.transform = '';
                }
            }
        }
        
        // Update stock
        var stock = parseFloat(variant.Quantity) || 0;
        var isOut = stock <= 0;
        if (pdStockNote && allowStock) {
            if (isOut) {
                pdStockNote.style.display = '';
                pdStockNote.className = 'small text-muted fw-semibold';
                pdStockNote.textContent = 'Out of stock';
            } else {
                var stockText = stock % 1 === 0 ? Math.floor(stock).toString() : stock.toFixed(2);
                pdStockNote.style.display = '';
                pdStockNote.className = 'small fw-semibold text-success';
                pdStockNote.textContent = stockText + ' left';
            }
        }
        
        // Update code/SKU if variant has one
        if (pdCode && variant.SKU) {
            pdCode.textContent = variant.SKU;
            var pdCodeRow = modalEl.querySelector('#pdCodeRow');
            if (pdCodeRow && settings.showProductCode !== false) {
                pdCodeRow.style.display = '';
            }
        }
        
        // Update modal data attributes with variant info
        modalEl.setAttribute('data-item-details-id', (variant.ItemDetailsId || '0').toString());
        modalEl.setAttribute('data-product-price', salesPrice.toFixed(2));
        modalEl.setAttribute('data-product-mrp', mrp.toFixed(2));
        if (variant.SKU) {
            modalEl.setAttribute('data-product-sku', variant.SKU);
        }
        
        // Update cart button state if item is already in cart
        var pdAddToCart = modalEl.querySelector('#pdAddToCart');
        if (pdAddToCart) {
            var cartItems;
            try {
                cartItems = JSON.parse(localStorage.getItem(CART_STORAGE_KEY)) || [];
            } catch (_) {
                cartItems = [];
            }
            if (!Array.isArray(cartItems)) {
                cartItems = [];
            }
            
            var variantSku = (variant.SKU || '').trim().toLowerCase();
            var variantId = (variant.ItemDetailsId || 0).toString();
            var isInCart = cartItems.some(function(item) {
                if (!item) return false;
                var itemSku = (item.sku || '').trim().toLowerCase();
                var itemDetailsId = (item.itemDetailsId || 0).toString();
                return (variantSku && itemSku && itemSku === variantSku) || 
                       (variantId && itemDetailsId && itemDetailsId === variantId);
            });
            
            if (isInCart) {
                pdAddToCart.setAttribute('data-cart-state', 'added');
                pdAddToCart.innerHTML = '<i class="bi bi-cart-check me-1"></i> Go to Cart';
                pdAddToCart.classList.remove('btn-danger');
                pdAddToCart.classList.add('btn-warning');
            } else {
                pdAddToCart.innerHTML = '<i class="bi bi-cart3 me-1"></i> Add to Cart';
                pdAddToCart.removeAttribute('data-cart-state');
                pdAddToCart.classList.remove('btn-warning');
                if (!pdAddToCart.classList.contains('btn-danger')) {
                    pdAddToCart.classList.add('btn-danger');
                }
            }
        }
    }

    function initProductDetailsModal() {
        var cfg = getConfig();
        var settings = cfg.settings || {};

        function textOrNull(el) {
            return el ? (el.textContent || '').trim() : null;
        }

        function initProductImageZoom(imageEl) {
            try {
                if (!window.Panzoom || !imageEl) return function () { };
                var wrap = imageEl.closest('.pd-zoom-wrap');
                if (!wrap) return function () { };

                imageEl.style.transform = '';

                var panzoom = window.Panzoom(imageEl, {
                    maxScale: 4,
                    minScale: 1,
                    step: 0.25,
                    contain: 'outside'
                });

                // Store panzoom instance globally so we can reset it when image changes
                window._pdPanzoomInstance = panzoom;

                function wheelHandler(e) { panzoom.zoomWithWheel(e); }
                wrap.addEventListener('wheel', wheelHandler, { passive: false });

                function down() { wrap.classList.add('is-zooming'); }
                function up() { wrap.classList.remove('is-zooming'); }
                wrap.addEventListener('pointerdown', down);
                window.addEventListener('pointerup', up, { once: false });
                wrap.addEventListener('pointerleave', up);

                var onLoad = function () { try { panzoom.reset({ animate: false }); } catch (_) { } };
                imageEl.addEventListener('load', onLoad, { once: true });

                return function () {
                    try { wrap.removeEventListener('wheel', wheelHandler); } catch (_) { }
                    try { wrap.removeEventListener('pointerdown', down); } catch (_) { }
                    try { window.removeEventListener('pointerup', up); } catch (_) { }
                    try { wrap.removeEventListener('pointerleave', up); } catch (_) { }
                    try { wrap.classList.remove('is-zooming'); } catch (_) { }
                    try {
                        if (panzoom && typeof panzoom.destroy === 'function') {
                            panzoom.destroy();
                        } else if (panzoom && typeof panzoom.reset === 'function') {
                            panzoom.reset();
                        }
                    } catch (_) { }
                    // Clear the global instance reference
                    window._pdPanzoomInstance = null;
                };
            } catch (_) {
                return function () { };
            }
        }

        function initFallbackPanZoom(imageEl) {
            if (!imageEl) return function () { };
            var wrap = imageEl.closest('.pd-zoom-wrap');
            if (!wrap) return function () { };

            var scale = 1;
            var minScale = 1;
            var maxScale = 4;
            var posX = 0, posY = 0;
            var startX = 0, startY = 0;
            var isPanning = false;

            function apply() {
                imageEl.style.transform = 'translate(' + posX + 'px,' + posY + 'px) scale(' + scale + ')';
            }

            function clamp(n, min, max) { return Math.max(min, Math.min(max, n)); }

            function wheel(e) {
                e.preventDefault();
                var delta = e.deltaY < 0 ? 0.2 : -0.2;
                var newScale = clamp(scale + delta, minScale, maxScale);
                if (newScale !== scale) {
                    var rect = imageEl.getBoundingClientRect();
                    var cx = e.clientX - rect.left - rect.width / 2;
                    var cy = e.clientY - rect.top - rect.height / 2;
                    posX -= cx * (newScale - scale);
                    posY -= cy * (newScale - scale);
                    scale = newScale;
                    apply();
                }
            }

            function down(e) {
                isPanning = true;
                wrap.classList.add('is-zooming');
                startX = e.clientX - posX;
                startY = e.clientY - posY;
            }

            function move(e) {
                if (!isPanning) return;
                posX = e.clientX - startX;
                posY = e.clientY - startY;
                apply();
            }

            function up() {
                isPanning = false;
                wrap.classList.remove('is-zooming');
            }

            imageEl.style.transform = '';
            apply();

            wrap.addEventListener('wheel', wheel, { passive: false });
            wrap.addEventListener('pointerdown', down);
            window.addEventListener('pointermove', move);
            window.addEventListener('pointerup', up);
            wrap.addEventListener('pointerleave', up);

            var onLoad = function () { scale = 1; posX = 0; posY = 0; apply(); };
            imageEl.addEventListener('load', onLoad, { once: true });

            return function () {
                try { wrap.removeEventListener('wheel', wheel); } catch (_) { }
                try { wrap.removeEventListener('pointerdown', down); } catch (_) { }
                try { window.removeEventListener('pointermove', move); } catch (_) { }
                try { window.removeEventListener('pointerup', up); } catch (_) { }
                try { wrap.removeEventListener('pointerleave', up); } catch (_) { }
                wrap.classList.remove('is-zooming');
            };
        }

        function parseRs(val) {
            if (!val) return 0;
            var num = String(val).replace(/[^0-9.]/g, '');
            var n = parseFloat(num);
            return isNaN(n) ? 0 : n;
        }

        function openProductDetailsModal(card) {
            var modalEl = doc.getElementById('productDetailsModal');
            if (!modalEl || !card) return;

            var allowPrices = settings.showPrices !== false;
            var allowMRP = settings.showMRP !== false;
            var allowDiscount = settings.showDiscount !== false;
            var allowStock = settings.showStock !== false;
            var allowProductCode = settings.showProductCode !== false;
            var allowImages = settings.showImages !== false;
            var allowWhatsApp = !!cfg.enableWhatsAppItems;

            var titleEl = card.querySelector('.fw-semibold.mt-1');
            var imgEl = card.querySelector('img.thumb');
            var priceEl = card.querySelector('.price');
            var priceOldEl = card.querySelector('.price-old');
            var offEl = card.querySelector('.off');
            var stockNoteEl = card.querySelector('.small.fw-semibold');
            var outBtn = card.querySelector('button.btn-secondary[disabled]');

            var title = textOrNull(titleEl) || 'Product Details';
            var img = imgEl ? imgEl.getAttribute('src') : '';
            var price = textOrNull(priceEl) || '';
            var priceOld = textOrNull(priceOldEl) || '';
            var off = textOrNull(offEl) || '';
            var descAttr = card.getAttribute('data-desc') || '';
            var desc = descAttr ? decodeHtml(descAttr) : '';
            var fullName = card.getAttribute('data-name') || title;
            var code = card.getAttribute('data-code') || '';
            var itemIdAttr = card.getAttribute('data-item-id') || '0';
            var itemDetailsIdAttr = card.getAttribute('data-item-details-id') || '0';
            var skuAttr = card.getAttribute('data-sku') || '';
            var priceAttr = card.getAttribute('data-price') || '';
            var mrpAttr = card.getAttribute('data-mrp') || '';
            var isOut = false;
            var stockNoteText = textOrNull(stockNoteEl);
            if (outBtn) isOut = true;
            if (!isOut && stockNoteText && /out of stock/i.test(stockNoteText)) isOut = true;

            var pdTitle = modalEl.querySelector('#pdTitle');
            var pdImage = modalEl.querySelector('#pdImage');
            var pdPrice = modalEl.querySelector('#pdPrice');
            var pdPriceOld = modalEl.querySelector('#pdPriceOld');
            var pdOff = modalEl.querySelector('#pdOff');
            var pdStockNote = modalEl.querySelector('#pdStockNote');
            var pdAddToCart = modalEl.querySelector('#pdAddToCart');
            var pdDesc = modalEl.querySelector('#pdDescription');
            var pdWhatsApp = modalEl.querySelector('#pdWhatsApp');
            var pdCodeRow = modalEl.querySelector('#pdCodeRow');
            var pdCode = modalEl.querySelector('#pdCode');
            var pdName = modalEl.querySelector('#pdName');
            var pdPriceMeta = modalEl.querySelector('#pdPriceMeta');
            var pdVariantSection = modalEl.querySelector('#pdVariantSection');
            var pdVariantGroups = modalEl.querySelector('#pdVariantGroups');
            var pdPriceRow = modalEl.querySelector('#pdPriceRow');
            var pdImageWrap = modalEl.querySelector('.pd-zoom-wrap');

            modalEl.setAttribute('data-product-code', code || '');
            modalEl.setAttribute('data-product-name', fullName || title);
            modalEl.setAttribute('data-item-id', itemIdAttr || '0');
            modalEl.setAttribute('data-item-details-id', itemDetailsIdAttr || '0');
            modalEl.setAttribute('data-product-sku', skuAttr || '');
            modalEl.setAttribute('data-product-price', priceAttr || '');
            modalEl.setAttribute('data-product-mrp', mrpAttr || '');
            
            // Store variants JSON in modal for variant matching
            if (variantsJson) {
                modalEl.setAttribute('data-variants-json', variantsJson);
            } else {
                modalEl.removeAttribute('data-variants-json');
            }

            if (pdTitle) pdTitle.textContent = fullName;
            if (pdImageWrap) pdImageWrap.style.display = allowImages ? '' : 'none';
            if (pdImage) {
                if (allowImages && img) {
                    pdImage.setAttribute('src', img);
                } else {
                    pdImage.removeAttribute('src');
                }
            }

            if (pdPriceRow) pdPriceRow.style.display = allowPrices ? '' : 'none';
            if (pdPrice) pdPrice.textContent = allowPrices ? (price || '') : '';

            if (pdAddToCart) {
                var cartItems;
                try {
                    cartItems = JSON.parse(localStorage.getItem(CART_STORAGE_KEY)) || [];
                } catch (_) {
                    cartItems = [];
                }
                if (!Array.isArray(cartItems)) {
                    cartItems = [];
                }
                var compareCode = (code || '').trim().toLowerCase();
                var compareName = (fullName || '').trim().toLowerCase();
                var isInCart = cartItems.some(function (item) {
                    if (!item) {
                        return false;
                    }
                    var itemCode = (item.code || '').trim().toLowerCase();
                    if (compareCode && itemCode) {
                        return itemCode === compareCode;
                    }
                    if (!compareName) {
                        return false;
                    }
                    var itemName = (item.name || '').trim().toLowerCase();
                    return itemName && itemName === compareName;
                });

                if (isInCart) {
                    pdAddToCart.setAttribute('data-cart-state', 'added');
                    pdAddToCart.innerHTML = '<i class="bi bi-cart-check me-1"></i> Go to Cart';
                    pdAddToCart.classList.remove('btn-danger');
                    pdAddToCart.classList.add('btn-warning');
                } else {
                    pdAddToCart.innerHTML = '<i class="bi bi-cart3 me-1"></i> Add to Cart';
                    pdAddToCart.removeAttribute('data-cart-state');
                    pdAddToCart.classList.remove('btn-warning');
                    if (!pdAddToCart.classList.contains('btn-danger')) {
                        pdAddToCart.classList.add('btn-danger');
                    }
                }
            }

            if (pdPriceOld) {
                if (allowPrices && allowMRP && priceOld) {
                    pdPriceOld.style.display = '';
                    pdPriceOld.textContent = priceOld;
                } else {
                    pdPriceOld.style.display = 'none';
                }
            }

            if (pdOff) {
                if (allowDiscount && off) {
                    pdOff.style.display = '';
                    pdOff.textContent = off;
                } else {
                    pdOff.style.display = 'none';
                }
            }

            if (pdDesc) pdDesc.innerHTML = desc || 'No description provided.';
            if (fullName && /\.\.\.$/.test(fullName)) { fullName = fullName.replace(/\.\.\.$/, ''); }
            if (pdName) pdName.textContent = fullName;

            var sp = parseRs(price);
            if ((!sp || !isFinite(sp)) && priceAttr) {
                var parsedPriceAttr = parseFloat(priceAttr);
                if (!isNaN(parsedPriceAttr) && isFinite(parsedPriceAttr)) {
                    sp = parsedPriceAttr;
                }
            }
            var mrp = parseRs(priceOld);
            if ((!mrp || !isFinite(mrp)) && mrpAttr) {
                var parsedMrpAttr = parseFloat(mrpAttr);
                if (!isNaN(parsedMrpAttr) && isFinite(parsedMrpAttr)) {
                    mrp = parsedMrpAttr;
                }
            }
            var save = mrp > sp ? (mrp - sp) : 0;
            if (pdPriceMeta) {
                pdPriceMeta.style.display = allowPrices && allowDiscount && save > 0 ? '' : 'none';
                pdPriceMeta.textContent = allowPrices && allowDiscount && save > 0 ? ('You save \u20B9' + save.toFixed(0)) : '';
            }

            if (pdVariantGroups) pdVariantGroups.innerHTML = '';
            
            // Collect all unique attribute values from all variants
            var attrs = {};
            var variantsJson = card.getAttribute('data-variants');
            var itemDetailsId = card.getAttribute('data-item-details-id');
            var allVariants = [];
            
            if (variantsJson) {
                try {
                    var decodedVariantsJson = decodeHtml(variantsJson);
                    var variants = JSON.parse(decodedVariantsJson);
                    
                    if (Array.isArray(variants)) {
                        allVariants = variants;
                        // Store variants in modal for variant matching
                        try {
                            modalEl.setAttribute('data-variants-json', JSON.stringify(variants));
                        } catch (e) {
                            console.warn('Failed to store variants JSON:', e);
                        }
                        // Iterate through all variants to collect unique attribute values
                        variants.forEach(function(variant) {
                            if (variant && variant.AttributesJson) {
                                try {
                                    var variantAttrsJson = variant.AttributesJson;
                                    if (typeof variantAttrsJson === 'string') {
                                        variantAttrsJson = decodeHtml(variantAttrsJson);
                                    }
                                    var variantAttrs = JSON.parse(variantAttrsJson);
                                    
                                    if (variantAttrs && typeof variantAttrs === 'object') {
                                        // Collect all unique values for each attribute key
                                        Object.keys(variantAttrs).forEach(function(key) {
                                            if (!attrs[key]) {
                                                attrs[key] = [];
                                            }
                                            var values = variantAttrs[key];
                                            if (Array.isArray(values)) {
                                                values.forEach(function(val) {
                                                    var valStr = String(val);
                                                    // Check if value already exists (case-insensitive for non-color values)
                                                    var exists = false;
                                                    if (typeof val === 'string' && /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(valStr)) {
                                                        // For colors, exact match
                                                        exists = attrs[key].some(function(existing) {
                                                            return String(existing).toLowerCase() === valStr.toLowerCase();
                                                        });
                                                    } else {
                                                        // For other values, case-insensitive match
                                                        exists = attrs[key].some(function(existing) {
                                                            return String(existing).toLowerCase() === valStr.toLowerCase();
                                                        });
                                                    }
                                                    if (!exists) {
                                                        attrs[key].push(val);
                                                    }
                                                });
                                            } else if (values !== null && values !== undefined) {
                                                // Single value
                                                var valStr = String(values);
                                                var exists = attrs[key].some(function(existing) {
                                                    return String(existing).toLowerCase() === valStr.toLowerCase();
                                                });
                                                if (!exists) {
                                                    attrs[key].push(values);
                                                }
                                            }
                                        });
                                    }
                                } catch (e) {
                                    console.warn('Failed to parse variant attributes JSON:', variant.AttributesJson, e);
                                }
                            }
                        });
                    }
                } catch (e) {
                    console.warn('Failed to parse variants JSON:', variantsJson, e);
                }
            }
            
            // Clear variants if no variants found
            if (!allVariants || allVariants.length === 0) {
                modalEl.removeAttribute('data-variants-json');
            }
            
            // Fallback to data-attrs if no variants found
            if (!attrs || Object.keys(attrs).length === 0) {
                var attrsJson = card.getAttribute('data-attrs');
                try { 
                    var decodedAttrsJson = attrsJson ? decodeHtml(attrsJson) : '';
                    attrs = decodedAttrsJson ? JSON.parse(decodedAttrsJson) : {}; 
                } catch (e) { 
                    attrs = {}; 
                }
            }
            
            var attrKeys = Object.keys(attrs || {});
            
            if (attrKeys.length > 0) {
                attrKeys.forEach(function (key) {
                    var values = attrs[key] || [];
                    var wrap = doc.createElement('div');
                    wrap.className = 'mb-2';
                    var label = doc.createElement('div');
                    label.className = 'small text-muted mb-1';
                    label.textContent = key;
                    wrap.appendChild(label);
                    var list = doc.createElement('div');
                    list.className = 'options-list';
                    values.forEach(function (val, idx) {
                        var chip = doc.createElement('span');
                        if (typeof val === 'string' && /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(val)) {
                            chip.className = 'badge badge-chip option-chip px-2 py-2' + (idx === 0 ? ' active' : '');
                            chip.style.borderRadius = '999px';
                            chip.style.padding = '0.25rem';
                            var dot = doc.createElement('span');
                            dot.style.display = 'inline-block';
                            dot.style.width = '18px';
                            dot.style.height = '18px';
                            dot.style.borderRadius = '999px';
                            dot.style.border = '1px solid var(--surface-border)';
                            dot.style.background = String(val);
                            dot.title = String(val);
                            chip.appendChild(dot);
                        } else {
                            chip.className = 'badge badge-chip option-chip px-3 py-2' + (idx === 0 ? ' active' : '');
                            chip.textContent = String(val);
                        }
                        list.appendChild(chip);
                    });
                    wrap.appendChild(list);
                    if (pdVariantGroups) pdVariantGroups.appendChild(wrap);
                });
                
                if (pdVariantSection) {
                    pdVariantSection.style.display = '';
                    // Initialize option lists to handle show/hide logic
                    try { 
                        initOptionLists(); 
                    } catch (e) { 
                        // Ignore errors
                    }
                    
                    // Initialize variant selection with progressive filtering
                    if (allVariants && allVariants.length > 0) {
                        try {
                            // Auto-select first available option for each attribute (if not already selected)
                            var variantGroups = modalEl.querySelector('#pdVariantGroups');
                            if (variantGroups) {
                                var optionLists = variantGroups.querySelectorAll('.options-list');
                                var attrKeys = [];
                                optionLists.forEach(function(list) {
                                    var labelEl = list.previousElementSibling;
                                    if (labelEl && labelEl.classList.contains('small')) {
                                        attrKeys.push(labelEl.textContent.trim());
                                    }
                                });
                                
                                // Auto-select first available for each attribute in order
                                var currentSelections = {};
                                attrKeys.forEach(function(key) {
                                    var selectedValue = autoSelectFirstAvailable(modalEl, allVariants, currentSelections, key);
                                    if (selectedValue) {
                                        currentSelections[key] = selectedValue;
                                    }
                                });
                                
                                // Get final selected options after auto-selection
                                var selectedOptions = getSelectedOptionsFromModal(modalEl);
                                
                                // Update chip availability based on final selections
                                updateChipAvailability(modalEl, allVariants, selectedOptions);
                                
                                // Match and update variant
                                var matchedVariant = findMatchingVariant(allVariants, selectedOptions);
                                if (matchedVariant) {
                                    updateProductDetailsWithVariant(modalEl, matchedVariant, settings);
                                }
                            }
                        } catch (e) {
                            console.warn('Error initializing variant:', e);
                        }
                    }
                }
            } else {
                if (pdVariantSection) pdVariantSection.style.display = 'none';
            }

            if (pdCode) {
                if (allowProductCode && code) {
                    pdCode.textContent = code;
                    if (pdCodeRow) pdCodeRow.style.display = '';
                } else if (pdCodeRow) {
                    pdCodeRow.style.display = 'none';
                }
            }

            if (pdStockNote) {
                if (!allowStock) {
                    pdStockNote.style.display = 'none';
                } else if (isOut) {
                    pdStockNote.style.display = '';
                    pdStockNote.className = 'small text-muted fw-semibold';
                    pdStockNote.textContent = 'Out of stock';
                } else if (stockNoteText) {
                    var stockNoteTrimmed = stockNoteText.trim();
                    var isLowStock = /left/i.test(stockNoteTrimmed);
                    pdStockNote.style.display = '';
                    pdStockNote.className = 'small fw-semibold ' + (isLowStock ? 'text-danger' : 'text-success');
                    pdStockNote.textContent = stockNoteTrimmed;
                } else {
                    pdStockNote.style.display = 'none';
                }
            }

            if (pdAddToCart) {
                pdAddToCart.style.display = '';
            }

            if (pdWhatsApp) {
                if (allowWhatsApp && window.waPhone) {
                    var msg = encodeURIComponent('Hi! I want to enquire about: ' + fullName + ' (' + window.location.href + ')');
                    var url = 'https://wa.me/' + (window.waPhone || '') + '?text=' + msg;
                    pdWhatsApp.style.display = '';
                    pdWhatsApp.setAttribute('href', url);
                } else {
                    pdWhatsApp.style.display = 'none';
                }
            }

            try {
                if (window._pdPanzoomCleanup) {
                    window._pdPanzoomCleanup();
                }
                if (allowImages && pdImage) {
                    window._pdPanzoomCleanup = (window.Panzoom ? initProductImageZoom : initFallbackPanZoom)(pdImage);
                }
                var onHidden = function () {
                    try {
                        if (window._pdPanzoomCleanup) { window._pdPanzoomCleanup(); window._pdPanzoomCleanup = null; }
                    } catch (_) { /* ignore */ }
                    modalEl.removeEventListener('hidden.bs.modal', onHidden);
                };
                modalEl.addEventListener('hidden.bs.modal', onHidden);
            } catch (_) { /* ignore */ }

            if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
                modal.show();
            }
        }

        doc.addEventListener('click', function (event) {
            var imgClick = event.target.closest('.product-card .thumb');
            var titleClick = event.target.closest('.product-card .fw-semibold.mt-1');
            if (imgClick || titleClick) {
                var card = (imgClick || titleClick).closest('.product-card');
                if (card) openProductDetailsModal(card);
                return;
            }

        });
    }

    function initPlacesOfSupply() {
        var btn = doc.querySelector('.places-of-supply-btn');
        var modal = doc.getElementById('placesOfSupplyModal');
        var items = doc.querySelectorAll('.places-of-supply-item');
        var cfg = getConfig();
        
        if (!btn || !modal || !items.length) {
            return;
        }

        function getCookie(name) {
            var parts = ('; ' + document.cookie).split('; ' + name + '=');
            if (parts.length === 2) {
                return parts.pop().split(';').shift();
            }
            return null;
        }

        function setSelectedStateCookie(stateId) {
            try {
                var maxAgeDays = 30;
                var maxAge = maxAgeDays * 24 * 60 * 60;
                document.cookie = 'publicCatalogueSelectedStateId=' + encodeURIComponent(String(stateId)) + '; path=/; max-age=' + String(maxAge) + '; SameSite=Lax';
            } catch (_) {
                // ignore
            }
        }

        function resolveStateName(stateId) {
            for (var i = 0; i < items.length; i++) {
                var el = items[i];
                if (String(el.getAttribute('data-state-id')) === String(stateId)) {
                    var nm = el.getAttribute('data-state-name');
                    if (nm) return nm;
                }
            }
            try {
                var list = (cfg && Array.isArray(cfg.placesOfSupply)) ? cfg.placesOfSupply : [];
                for (var j = 0; j < list.length; j++) {
                    if (String(list[j].StateId) === String(stateId)) {
                        return list[j].State || '';
                    }
                }
            } catch (_) { /* ignore */ }
            return '';
        }

        function getSelectedState() {
            var id = getCookie('publicCatalogueSelectedStateId');
            if (!id) return null;
            return { id: id, name: resolveStateName(id) };
        }

        function updateButtonText(stateName) {
            if (btn && stateName) {
                var icon = btn.querySelector('i');
                if (icon) {
                    btn.innerHTML = '<span class="small me-1" style="text-decoration: none;">' + stateName + '</span><i class="bi bi-chevron-down" style="font-size: 1rem;"></i>';
                }
            }
        }

        function markActiveItem(stateId) {
            items.forEach(function(item) {
                var itemStateId = item.getAttribute('data-state-id');
                if (itemStateId === String(stateId)) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        }

        var allowClose = false;

        function enableModalClose() {
            allowClose = true;
            // Enable close buttons
            var closeBtn = modal.querySelector('.btn-close');
            var footerCloseBtn = modal.querySelector('.modal-footer button[data-bs-dismiss="modal"]');
            
            if (closeBtn) {
                closeBtn.style.display = '';
                closeBtn.removeAttribute('disabled');
            }
            if (footerCloseBtn) {
                footerCloseBtn.style.display = '';
                footerCloseBtn.removeAttribute('disabled');
            }
        }

        function disableModalClose() {
            allowClose = false;
            // Hide/disable close buttons
            var closeBtn = modal.querySelector('.btn-close');
            var footerCloseBtn = modal.querySelector('.modal-footer button[data-bs-dismiss="modal"]');
            
            if (closeBtn) {
                closeBtn.style.display = 'none';
            }
            if (footerCloseBtn) {
                footerCloseBtn.style.display = 'none';
            }
        }

        // Prevent modal from closing if state is not selected
        modal.addEventListener('hide.bs.modal', function(event) {
            if (!allowClose) {
                event.preventDefault();
                event.stopPropagation();
                return false;
            }
        });

        // Load saved state
        var savedState = getSelectedState();
        var hasSelectedState = !!(savedState && savedState.id);
        
        if (hasSelectedState) {
            if (savedState.name) {
                updateButtonText(savedState.name);
            }
            markActiveItem(savedState.id);
            enableModalClose();
        } else {
            // No state selected - open modal and prevent closing
            disableModalClose();
            
            // Open modal automatically
            if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                var modalInstance = new window.bootstrap.Modal(modal, {
                    backdrop: 'static',
                    keyboard: false
                });
                modalInstance.show();
            }
        }

        // Handle item clicks
        items.forEach(function(item) {
            item.addEventListener('click', function() {
                var stateId = item.getAttribute('data-state-id');
                var stateName = item.getAttribute('data-state-name');
                
                if (stateId && stateName) {
                    setSelectedStateCookie(stateId);
                    updateButtonText(stateName);
                    markActiveItem(stateId);
                    
                    // Enable modal closing now that state is selected
                    enableModalClose();
                    
                    // Close modal
                    if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        var modalInstance = window.bootstrap.Modal.getInstance(modal);
                        if (modalInstance) {
                            modalInstance.hide();
                        }
                    }
                    
                    // Trigger change event if needed (for future use)
                    var event = new CustomEvent('placesOfSupplyChanged', {
                        detail: { stateId: stateId, stateName: stateName }
                    });
                    doc.dispatchEvent(event);
                }
            });
        });

        // Handle button click to open modal (if state is already selected, allow normal behavior)
        if (btn) {
            btn.addEventListener('click', function() {
                var currentState = getSelectedState();
                if (currentState && currentState.id) {
                    // State is selected, allow normal modal behavior
                    enableModalClose();
                } else {
                    // No state selected, prevent closing
                    disableModalClose();
                }
            });
        }
    }

    function initVariantSwitching() {
        // Handle variant button clicks
        doc.addEventListener('click', function(event) {
            var variantBtn = event.target.closest('.variant-option');
            if (!variantBtn || variantBtn.disabled) return;
            
            var productCard = variantBtn.closest('.product-card-variable');
            if (!productCard) return;
            
            event.preventDefault();
            event.stopPropagation();
            
            // Get variant data
            var variantImage = variantBtn.getAttribute('data-variant-image') || '';
            var variantPrice = parseFloat(variantBtn.getAttribute('data-variant-price')) || 0;
            var variantMrp = parseFloat(variantBtn.getAttribute('data-variant-mrp')) || 0;
            var variantStock = parseFloat(variantBtn.getAttribute('data-variant-stock')) || 0;
            var variantName = variantBtn.getAttribute('data-variant-name') || '';
            var variantId = variantBtn.getAttribute('data-variant-id') || '';
            
            // Update image
            var imgEl = productCard.querySelector('.thumb');
            if (imgEl && variantImage) {
                imgEl.src = variantImage;
                imgEl.setAttribute('data-current-variant-image', variantImage);
            }
            
            // Update price
            var priceEl = productCard.querySelector('.price');
            if (priceEl) {
                priceEl.innerHTML = '₹' + variantPrice.toFixed(2);
            }
            
            // Update MRP if shown
            var mrpEl = productCard.querySelector('.price-old');
            if (mrpEl) {
                if (variantMrp > variantPrice && variantMrp > 0) {
                    mrpEl.textContent = '₹' + variantMrp.toFixed(2);
                    mrpEl.style.display = '';
                } else {
                    mrpEl.style.display = 'none';
                }
            }
            
            // Update discount
            var discountEl = productCard.querySelector('.off');
            if (discountEl) {
                if (variantMrp > variantPrice && variantMrp > 0) {
                    var discount = Math.round(((variantMrp - variantPrice) / variantMrp) * 100);
                    discountEl.textContent = discount + '% off';
                    discountEl.style.display = '';
                } else {
                    discountEl.style.display = 'none';
                }
            }
            
            // Update stock status
            var stockEl = productCard.querySelector('.stock-status');
            if (stockEl) {
                if (variantStock <= 0) {
                    stockEl.textContent = 'Out of stock';
                    stockEl.className = 'stock-status small fw-semibold text-danger';
                } else {
                    var isWhole = variantStock % 1 === 0;
                    var stockDisplay = isWhole ? 
                        Math.floor(variantStock).toString() : 
                        variantStock.toFixed(2);
                    stockEl.textContent = stockDisplay + ' in stock';
                    stockEl.className = 'stock-status small fw-semibold text-success';
                }
            }
            
            // Update active variant button
            var allVariants = productCard.querySelectorAll('.variant-option');
            allVariants.forEach(function(btn) {
                btn.classList.remove('active', 'btn-primary');
                btn.classList.add('btn-outline-secondary');
            });
            variantBtn.classList.add('active', 'btn-primary');
            variantBtn.classList.remove('btn-outline-secondary');
            
            // Update data attributes on product card
            productCard.setAttribute('data-item-details-id', variantId);
            productCard.setAttribute('data-price', variantPrice.toFixed(2));
            productCard.setAttribute('data-mrp', variantMrp.toFixed(2));
            
            // Update product name if needed (optional - can show variant name)
            // var nameEl = productCard.querySelector('.fw-semibold.mt-1');
            // if (nameEl && variantName) {
            //     var baseName = productCard.getAttribute('data-name') || '';
            //     nameEl.textContent = baseName + ' - ' + variantName;
            // }
        });
    }

    attachCopyGstListener();
    onReady(function () {
        initCartFeature();
        initPriceSliders();
        initProductFilters();
        hideOutOfStock();
        initNestedDropdowns();
        initHeaderSearch();
        initWhatsAppEnquiry();
        initVariationChips();
        initProductDetailsModal();
        initPlacesOfSupply();
        initVariantSwitching();
    });
})();


