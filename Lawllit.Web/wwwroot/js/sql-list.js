(function () {
    'use strict';

    var inputEl = document.getElementById('sql-input');
    if (!inputEl) return;

    var outputEl = document.getElementById('sql-output');
    var dedupeEl = document.getElementById('sql-dedupe');
    var parensEl = document.getElementById('sql-parens');
    var copyBtn = document.getElementById('sql-copy');
    var clearBtn = document.getElementById('sql-clear');

    function readValues() {
        var values = inputEl.value
            .split('\n')
            .map(function (line) { return line.trim(); })
            .filter(function (line) { return line.length > 0; });

        if (dedupeEl.checked) {
            var seen = Object.create(null);
            values = values.filter(function (value) {
                if (seen[value]) return false;
                seen[value] = true;
                return true;
            });
        }

        return values;
    }

    function quoteValue(value, quoteStyle) {
        if (quoteStyle === 'single') return "'" + value.replace(/'/g, "''") + "'";
        if (quoteStyle === 'double') return '"' + value.replace(/"/g, '""') + '"';
        return value;
    }

    function wrap(content) {
        return parensEl.checked ? '(' + content + ')' : content;
    }

    function build(quoteStyle) {
        var values = readValues();
        if (values.length === 0) {
            outputEl.value = '';
            return;
        }

        var quoted = values.map(function (value) { return quoteValue(value, quoteStyle); });
        outputEl.value = wrap(quoted.join(','));
    }

    document.querySelectorAll('[data-quote]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            build(btn.dataset.quote);
        });
    });

    copyBtn.addEventListener('click', function () {
        if (!outputEl.value) return;
        navigator.clipboard.writeText(outputEl.value).then(function () {
            var original = copyBtn.textContent;
            copyBtn.textContent = '✓';
            setTimeout(function () { copyBtn.textContent = original; }, 1200);
        });
    });

    clearBtn.addEventListener('click', function () {
        inputEl.value = '';
        outputEl.value = '';
    });
})();
