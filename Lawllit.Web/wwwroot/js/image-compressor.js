(function () {
    'use strict';

    var inputEl = document.getElementById('img-input');
    if (!inputEl) return;

    var levelButtons = document.querySelectorAll('[data-level]');
    var resultEl = document.getElementById('img-result');
    var previewEl = document.getElementById('img-preview');
    var originalEl = document.getElementById('img-original');
    var compressedEl = document.getElementById('img-compressed');
    var reductionEl = document.getElementById('img-reduction');
    var downloadBtn = document.getElementById('img-download');

    var levels = {
        high: { quality: 0.85, maxWidth: 2560 },
        balanced: { quality: 0.7, maxWidth: 1920 },
        smallest: { quality: 0.5, maxWidth: 1280 },
    };

    var loadedImage = null;
    var originalSize = 0;
    var compressedUrl = null;

    function formatSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    }

    function revokeCompressed() {
        if (compressedUrl) {
            URL.revokeObjectURL(compressedUrl);
            compressedUrl = null;
        }
    }

    function compress(level) {
        if (!loadedImage) return;

        var preset = levels[level];
        var scale = loadedImage.width > preset.maxWidth ? preset.maxWidth / loadedImage.width : 1;

        var canvas = document.createElement('canvas');
        canvas.width = Math.round(loadedImage.width * scale);
        canvas.height = Math.round(loadedImage.height * scale);

        var context = canvas.getContext('2d');
        context.drawImage(loadedImage, 0, 0, canvas.width, canvas.height);

        canvas.toBlob(function (blob) {
            if (!blob) return;
            revokeCompressed();
            compressedUrl = URL.createObjectURL(blob);

            previewEl.src = compressedUrl;
            originalEl.textContent = formatSize(originalSize);
            compressedEl.textContent = formatSize(blob.size);

            var reduction = originalSize > 0
                ? Math.max(0, Math.round((1 - blob.size / originalSize) * 100))
                : 0;
            reductionEl.textContent = '-' + reduction + '%';

            resultEl.hidden = false;
        }, 'image/jpeg', preset.quality);
    }

    inputEl.addEventListener('change', function () {
        var file = inputEl.files[0];
        if (!file) return;

        originalSize = file.size;
        var reader = new FileReader();
        reader.onload = function () {
            var image = new Image();
            image.onload = function () {
                loadedImage = image;
                levelButtons.forEach(function (btn) { btn.disabled = false; });
                compress('balanced');
            };
            image.src = reader.result;
        };
        reader.readAsDataURL(file);
    });

    levelButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            compress(btn.dataset.level);
        });
    });

    downloadBtn.addEventListener('click', function () {
        if (!compressedUrl) return;
        var link = document.createElement('a');
        link.href = compressedUrl;
        link.download = 'comprimida.jpg';
        link.click();
    });
})();
