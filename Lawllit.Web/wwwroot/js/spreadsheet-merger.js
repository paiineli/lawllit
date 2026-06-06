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
        var allRows = [];
        var minDuration = 2500;
        var startTime = Date.now();

        overlay.hidden = false;
        setProgress(0, files.length, template);

        function processFile(index) {
            if (index >= files.length) {
                download(allRows);
                var elapsed = Date.now() - startTime;
                setTimeout(function () { overlay.hidden = true; }, Math.max(0, minDuration - elapsed));
                return;
            }

            setProgress(index + 1, files.length, template);

            var reader = new FileReader();
            reader.onload = function (evt) {
                var wb = XLSX.read(new Uint8Array(evt.target.result), { type: 'array' });
                var rows = XLSX.utils.sheet_to_json(wb.Sheets[wb.SheetNames[0]], { header: 1, defval: '' });
                allRows = index === 0 ? rows : allRows.concat(rows.slice(1));
                processFile(index + 1);
            };
            reader.onerror = function () { processFile(index + 1); };
            reader.readAsArrayBuffer(files[index]);
        }

        processFile(0);
    });

    function setProgress(current, total, template) {
        overlayProgress.style.width = Math.round(current / total * 100) + '%';
        overlayLabel.textContent = template.replace('{0}', current).replace('{1}', total);
    }

    function download(rows) {
        var ws = XLSX.utils.aoa_to_sheet(rows);
        var wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Merged');
        XLSX.writeFile(wb, 'merged.xlsx');
    }
})();
