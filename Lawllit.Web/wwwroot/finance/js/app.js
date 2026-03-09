function normalizeText(text) {
    return text.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
}

function applyLiveSearch(searchInput) {
    var searchValue = normalizeText(searchInput.value.trim());
    var tableBody = document.querySelector('[data-search-table]');
    if (!tableBody) return;
    var dataRows = tableBody.querySelectorAll('[data-search-row]');
    var visibleCount = 0;
    dataRows.forEach(function (row) {
        var matches = normalizeText(row.textContent).includes(searchValue);
        row.hidden = !matches;
        if (matches) visibleCount++;
    });
    var emptyRow = tableBody.querySelector('[data-search-empty]');
    if (emptyRow) emptyRow.hidden = visibleCount > 0 || dataRows.length === 0;
}

document.addEventListener('input', function (event) {
    if (event.target.matches('[data-live-search]')) applyLiveSearch(event.target);
});

var initialSearchInput = document.querySelector('[data-live-search]');
if (initialSearchInput && initialSearchInput.value) applyLiveSearch(initialSearchInput);

document.addEventListener('change', function (event) {
    if (event.target.matches('[data-auto-submit]')) event.target.form.submit();
    if (event.target.matches('[data-filter-type]')) filterCategoriesByType(event.target);
});

document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-toggle-pwd]');
    if (!button) return;
    var input = document.getElementById(button.dataset.togglePwd);
    if (!input) return;
    var showing = input.type === 'text';
    input.type = showing ? 'password' : 'text';
    button.querySelector('i').className = showing ? 'bi bi-eye' : 'bi bi-eye-slash';
});

document.addEventListener('show.bs.modal', function (event) {
    var typeSelect = event.target.querySelector('[data-filter-type]');
    if (typeSelect) filterCategoriesByType(typeSelect);
});

document.addEventListener('submit', function (event) {
    event.target.querySelectorAll('[data-decimal-input]').forEach(function (input) {
        if (input.value.includes(',')) {
            input.value = input.value.replace(/\./g, '');
        } else {
            input.value = input.value.replace('.', ',');
        }
    });
});

setTimeout(function () {
    document.querySelectorAll('.alert-dismissible').forEach(function (alertElement) {
        alertElement.addEventListener('transitionend', function () { alertElement.remove(); }, { once: true });
        alertElement.classList.remove('show');
    });
}, 4000);

function filterCategoriesByType(typeSelect) {
    var modalBody = typeSelect.closest('.modal-body');
    if (!modalBody) return;
    var categorySelect = modalBody.querySelector('[data-filter-category]');
    if (!categorySelect) return;
    var selectedType = typeSelect.value;
    categorySelect.querySelectorAll('optgroup[data-group-type]').forEach(function (optgroup) {
        optgroup.hidden = optgroup.dataset.groupType !== selectedType;
    });
    var selectedOption = categorySelect.options[categorySelect.selectedIndex];
    if (selectedOption && selectedOption.closest('optgroup') && selectedOption.closest('optgroup').hidden) {
        var firstVisibleOption = Array.from(categorySelect.options).find(function (option) {
            var parentGroup = option.closest('optgroup');
            return !parentGroup || !parentGroup.hidden;
        });
        if (firstVisibleOption) categorySelect.value = firstVisibleOption.value;
    }
}
