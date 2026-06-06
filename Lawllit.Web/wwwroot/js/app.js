(function () {
    'use strict';

    var filesInput = document.getElementById('files-input');
    if (!filesInput) return;

    var fileList = document.getElementById('file-list');
    var mergeBtn = document.getElementById('merge-btn');
    var form = mergeBtn.closest('form');
    var originalBtnText = mergeBtn.textContent;

    filesInput.addEventListener('change', function () {
        var files = Array.from(filesInput.files);
        fileList.hidden = files.length === 0;
        fileList.innerHTML = files.map(function (f) {
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(f.name));
            return '<span class="tool-file-item">' + div.innerHTML + '</span>';
        }).join('');
        mergeBtn.disabled = files.length < 2;
    });

    form.addEventListener('submit', function () {
        var loadingText = mergeBtn.dataset.loading || '...';
        mergeBtn.disabled = true;
        mergeBtn.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' +
            loadingText;
        setTimeout(function () {
            mergeBtn.textContent = originalBtnText;
            mergeBtn.disabled = Array.from(filesInput.files).length < 2;
        }, 15000);
    });
})();
