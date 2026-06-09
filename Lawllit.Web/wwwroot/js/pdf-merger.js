(function () {
    'use strict';

    var filesInput = document.getElementById('files-input');
    if (!filesInput) return;

    var fileList = document.getElementById('file-list');
    var mergeBtn = document.getElementById('merge-btn');
    var form = mergeBtn.closest('form');
    var overlay = document.getElementById('merge-overlay');
    var overlayLabel = document.getElementById('overlay-label');
    var overlayProgress = document.getElementById('overlay-progress');

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

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var files = Array.from(filesInput.files);
        if (files.length < 2) return;

        var template = overlayLabel.dataset.template || '{0} / {1}';
        var minDuration = 1500;
        var startTime = Date.now();

        overlay.hidden = false;
        setProgress(0, files.length, template);

        merge(files, template)
            .then(function () {
                var elapsed = Date.now() - startTime;
                setTimeout(function () { overlay.hidden = true; }, Math.max(0, minDuration - elapsed));
            })
            .catch(function () { overlay.hidden = true; });
    });

    async function merge(files, template) {
        var output = await PDFLib.PDFDocument.create();

        for (var i = 0; i < files.length; i++) {
            setProgress(i + 1, files.length, template);
            var bytes = await files[i].arrayBuffer();
            var input = await PDFLib.PDFDocument.load(bytes);
            var pages = await output.copyPages(input, input.getPageIndices());
            pages.forEach(function (page) { output.addPage(page); });
        }

        download(await output.save());
    }

    function setProgress(current, total, template) {
        overlayProgress.style.width = Math.round(current / total * 100) + '%';
        overlayLabel.textContent = template.replace('{0}', current).replace('{1}', total);
    }

    function download(bytes) {
        var url = URL.createObjectURL(new Blob([bytes], { type: 'application/pdf' }));
        var link = document.createElement('a');
        link.href = url;
        link.download = 'merged.pdf';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
})();
