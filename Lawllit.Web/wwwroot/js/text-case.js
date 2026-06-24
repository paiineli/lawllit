(function () {
    'use strict';

    var inputEl = document.getElementById('tc-input');
    if (!inputEl) return;

    var outputEl = document.getElementById('tc-output');
    var copyBtn = document.getElementById('tc-copy');
    var clearBtn = document.getElementById('tc-clear');

    function toWords(str) {
        return str
            .replace(/([a-z])([A-Z])/g, '$1 $2')
            .replace(/[\s_-]+/g, ' ')
            .trim();
    }

    var transforms = {
        upper: function (s) { return s.toUpperCase(); },
        lower: function (s) { return s.toLowerCase(); },
        title: function (s) { return toWords(s).toLowerCase().replace(/\b\w/g, function (c) { return c.toUpperCase(); }); },
        sentence: function (s) { var t = toWords(s).toLowerCase(); return t.charAt(0).toUpperCase() + t.slice(1); },
        camel: function (s) { return toWords(s).toLowerCase().replace(/ (\w)/g, function (_, c) { return c.toUpperCase(); }); },
        pascal: function (s) { return toWords(s).toLowerCase().replace(/(?:^| )(\w)/g, function (_, c) { return c.toUpperCase(); }); },
        snake: function (s) { return toWords(s).toLowerCase().replace(/ /g, '_'); },
        kebab: function (s) { return toWords(s).toLowerCase().replace(/ /g, '-'); },
    };

    document.querySelectorAll('[data-case]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var fn = transforms[btn.dataset.case];
            if (fn) outputEl.value = inputEl.value.split('\n').map(fn).join('\n');
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
