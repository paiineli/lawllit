(function () {
    'use strict';

    var inputEl = document.getElementById('qr-input');
    if (!inputEl) return;

    var generateBtn = document.getElementById('qr-generate');
    var downloadBtn = document.getElementById('qr-download');
    var clearBtn = document.getElementById('qr-clear');
    var containerEl = document.getElementById('qr-canvas');
    var wrapperEl = containerEl.parentElement;

    var qrCode = new QRCode(containerEl, {
        width: 256,
        height: 256,
        correctLevel: QRCode.CorrectLevel.M,
    });

    function generate() {
        var text = inputEl.value.trim();
        if (!text) return;

        qrCode.makeCode(text);
        wrapperEl.hidden = false;
        downloadBtn.disabled = false;
    }

    generateBtn.addEventListener('click', generate);

    downloadBtn.addEventListener('click', function () {
        if (downloadBtn.disabled) return;
        var canvasEl = containerEl.querySelector('canvas');
        if (!canvasEl) return;

        var link = document.createElement('a');
        link.href = canvasEl.toDataURL('image/png');
        link.download = 'qrcode.png';
        link.click();
    });

    clearBtn.addEventListener('click', function () {
        inputEl.value = '';
        qrCode.clear();
        wrapperEl.hidden = true;
        downloadBtn.disabled = true;
    });
})();
