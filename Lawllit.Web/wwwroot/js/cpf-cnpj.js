(function () {
    'use strict';

    var inputEl = document.getElementById('doc-input');
    if (!inputEl) return;

    var outputEl = document.getElementById('doc-output');
    var copyBtn = document.getElementById('doc-copy');
    var clearBtn = document.getElementById('doc-clear');

    var cnpjFirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    var cnpjSecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    function onlyDigits(value) {
        return value.replace(/\D/g, '');
    }

    function cpfCheckDigit(digits) {
        var sum = 0;
        var weight = digits.length + 1;
        for (var index = 0; index < digits.length; index++) {
            sum += Number(digits[index]) * (weight - index);
        }
        return (sum * 10) % 11 % 10;
    }

    function cnpjCheckDigit(digits) {
        var weights = digits.length === 12 ? cnpjFirstWeights : cnpjSecondWeights;
        var sum = 0;
        for (var index = 0; index < digits.length; index++) {
            sum += Number(digits[index]) * weights[index];
        }
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    function isValidCpf(digits) {
        if (digits.length !== 11 || /^(\d)\1{10}$/.test(digits)) return false;
        if (cpfCheckDigit(digits.slice(0, 9)) !== Number(digits[9])) return false;
        return cpfCheckDigit(digits.slice(0, 10)) === Number(digits[10]);
    }

    function isValidCnpj(digits) {
        if (digits.length !== 14 || /^(\d)\1{13}$/.test(digits)) return false;
        if (cnpjCheckDigit(digits.slice(0, 12)) !== Number(digits[12])) return false;
        return cnpjCheckDigit(digits.slice(0, 13)) === Number(digits[13]);
    }

    function formatDocument(digits) {
        if (digits.length === 11) {
            return digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
        }
        if (digits.length === 14) {
            return digits.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
        }
        return digits;
    }

    function randomDigits(count) {
        var result = '';
        for (var index = 0; index < count; index++) {
            result += Math.floor(Math.random() * 10);
        }
        return result;
    }

    function generateCpf() {
        var base = randomDigits(9);
        base += cpfCheckDigit(base);
        base += cpfCheckDigit(base);
        return formatDocument(base);
    }

    function generateCnpj() {
        var base = randomDigits(8) + '0001';
        base += cnpjCheckDigit(base);
        base += cnpjCheckDigit(base);
        return formatDocument(base);
    }

    function readLines() {
        return inputEl.value
            .split('\n')
            .map(function (line) { return line.trim(); })
            .filter(function (line) { return line.length > 0; });
    }

    function isValidDocument(digits) {
        if (digits.length === 11) return isValidCpf(digits);
        if (digits.length === 14) return isValidCnpj(digits);
        return false;
    }

    function validate() {
        outputEl.value = readLines().map(function (line) {
            var digits = onlyDigits(line);
            return isValidDocument(digits) ? outputEl.dataset.valid : outputEl.dataset.invalid;
        }).join('\n');
    }

    function transform(mapper) {
        outputEl.value = readLines().map(mapper).join('\n');
    }

    function appendLine(value) {
        outputEl.value = outputEl.value ? outputEl.value + '\n' + value : value;
    }

    var actions = {
        validate: validate,
        format: function () { transform(function (line) { return formatDocument(onlyDigits(line)); }); },
        clean: function () { transform(function (line) { return onlyDigits(line); }); },
        'generate-cpf': function () { appendLine(generateCpf()); },
        'generate-cnpj': function () { appendLine(generateCnpj()); },
    };

    document.querySelectorAll('[data-doc]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var action = actions[btn.dataset.doc];
            if (action) action();
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
