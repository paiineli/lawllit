(function () {
    'use strict';

    var inputEl = document.getElementById('b64-input');
    if (!inputEl) return;

    var outputEl = document.getElementById('b64-output');
    var encodeBtn = document.getElementById('b64-encode');
    var decodeBtn = document.getElementById('b64-decode');
    var copyBtn = document.getElementById('b64-copy');
    var clearBtn = document.getElementById('b64-clear');
    var errorEl = document.getElementById('b64-error');

    function encode(str) {
        var bytes = new TextEncoder().encode(str);
        var binary = Array.from(bytes, function (b) { return String.fromCharCode(b); }).join('');
        return btoa(binary);
    }

    function decode(str) {
        try {
            var binary = atob(str.replace(/\s+/g, ''));
            var bytes = Uint8Array.from(binary, function (c) { return c.charCodeAt(0); });
            return new TextDecoder().decode(bytes);
        } catch (e) {
            return null;
        }
    }

    encodeBtn.addEventListener('click', function () {
        errorEl.hidden = true;
        outputEl.value = encode(inputEl.value);
    });

    decodeBtn.addEventListener('click', function () {
        var result = decode(inputEl.value);
        if (result === null) {
            errorEl.hidden = false;
            outputEl.value = '';
        } else {
            errorEl.hidden = true;
            outputEl.value = result;
        }
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
        errorEl.hidden = true;
    });
})();
