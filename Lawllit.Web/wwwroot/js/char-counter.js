(function () {
    'use strict';

    var inputEl = document.getElementById('cc-input');
    if (!inputEl) return;

    var charsEl = document.getElementById('cc-chars');
    var wordsEl = document.getElementById('cc-words');
    var clearBtn = document.getElementById('cc-clear');

    function update() {
        var text = inputEl.value;
        var trimmed = text.trim();

        charsEl.textContent = text.length;
        wordsEl.textContent = trimmed ? trimmed.split(/\s+/).length : 0;
    }

    inputEl.addEventListener('input', update);

    clearBtn.addEventListener('click', function () {
        inputEl.value = '';
        update();
    });
})();
