(function () {
    function applyTheme(value) {
        document.documentElement.setAttribute('data-bs-theme', value === 'high-contrast' ? 'dark' : value);
        document.documentElement.setAttribute('data-theme', value);
    }

    function applyFontSize(value) {
        if (value === 'normal') document.documentElement.removeAttribute('data-font-size');
        else document.documentElement.setAttribute('data-font-size', value);
    }

    bindPreference({ containerId: 'theme-options', attr: 'theme-value', datasetKey: 'themeValue', activeClass: 'theme-card--active', key: 'theme', onSelect: applyTheme });
    bindPreference({ containerId: 'font-size-options', attr: 'font-size-value', datasetKey: 'fontSizeValue', activeClass: 'font-size-card--active', key: 'fontSize', onSelect: applyFontSize });
    bindPreference({ containerId: 'language-options', attr: 'language-value', datasetKey: 'languageValue', activeClass: 'font-size-card--active', key: 'language', afterSave: function () { location.reload(); } });
    bindPreference({ containerId: 'currency-options', attr: 'currency-value', datasetKey: 'currencyValue', activeClass: 'font-size-card--active', key: 'currency', afterSave: function () { location.reload(); } });
})();
