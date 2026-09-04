(function () {
    'use strict';

    var lengthEl = document.getElementById('pwd-length');
    if (!lengthEl) return;

    var lengthValueEl = document.getElementById('pwd-length-value');
    var upperEl = document.getElementById('pwd-upper');
    var lowerEl = document.getElementById('pwd-lower');
    var numbersEl = document.getElementById('pwd-numbers');
    var symbolsEl = document.getElementById('pwd-symbols');
    var generateBtn = document.getElementById('pwd-generate');
    var outputEl = document.getElementById('pwd-output');
    var strengthEl = document.getElementById('pwd-strength');
    var copyBtn = document.getElementById('pwd-copy');

    var charsets = {
        upper: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
        lower: 'abcdefghijklmnopqrstuvwxyz',
        numbers: '0123456789',
        symbols: '!@#$%&*?-_=+',
    };

    function buildCharset() {
        var charset = '';
        if (upperEl.checked) charset += charsets.upper;
        if (lowerEl.checked) charset += charsets.lower;
        if (numbersEl.checked) charset += charsets.numbers;
        if (symbolsEl.checked) charset += charsets.symbols;
        return charset;
    }

    function showStrength(length, charsetSize) {
        var entropy = length * (Math.log(charsetSize) / Math.log(2));
        var level = entropy < 40 ? 'weak' : entropy < 70 ? 'medium' : 'strong';
        strengthEl.textContent = strengthEl.dataset.label + ': ' + strengthEl.dataset[level];
        strengthEl.className = 'tool-strength tool-strength-' + level;
    }

    function generate() {
        var charset = buildCharset();
        if (!charset) {
            outputEl.value = '';
            strengthEl.textContent = '';
            return;
        }

        var length = parseInt(lengthEl.value, 10);
        var randomValues = new Uint32Array(length);
        window.crypto.getRandomValues(randomValues);

        var password = '';
        for (var index = 0; index < length; index++) {
            password += charset[randomValues[index] % charset.length];
        }

        outputEl.value = password;
        showStrength(length, charset.length);
    }

    lengthEl.addEventListener('input', function () {
        lengthValueEl.textContent = lengthEl.value;
    });

    generateBtn.addEventListener('click', generate);

    copyBtn.addEventListener('click', function () {
        if (!outputEl.value) return;
        navigator.clipboard.writeText(outputEl.value).then(function () {
            var original = copyBtn.textContent;
            copyBtn.textContent = '✓';
            setTimeout(function () { copyBtn.textContent = original; }, 1200);
        });
    });

    generate();
})();
