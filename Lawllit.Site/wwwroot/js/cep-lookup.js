(function () {
    'use strict';

    var inputEl = document.getElementById('cep-input');
    if (!inputEl) return;

    var lookupBtn = document.getElementById('cep-lookup');
    var resultEl = document.getElementById('cep-result');
    var errorEl = document.getElementById('cep-error');
    var copyBtn = document.getElementById('cep-copy');

    var resultFields = ['cep', 'logradouro', 'complemento', 'bairro', 'localidade', 'uf', 'ddd'];
    var addressFields = ['logradouro', 'bairro', 'localidade', 'uf'];

    function showError(message) {
        resultEl.hidden = true;
        errorEl.textContent = message;
        errorEl.hidden = false;
    }

    function fillResult(data) {
        resultFields.forEach(function (field) {
            var valueEl = document.getElementById('cep-' + field);
            if (valueEl) valueEl.textContent = data[field] || '—';
        });
        errorEl.hidden = true;
        resultEl.hidden = false;
    }

    function lookup() {
        var digits = inputEl.value.replace(/\D/g, '');
        if (digits.length !== 8) {
            showError(errorEl.dataset.invalid);
            return;
        }

        lookupBtn.disabled = true;
        fetch('https://viacep.com.br/ws/' + digits + '/json/')
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (data.erro) {
                    showError(errorEl.dataset.notfound);
                } else {
                    fillResult(data);
                }
            })
            .catch(function () {
                showError(errorEl.dataset.notfound);
            })
            .finally(function () {
                lookupBtn.disabled = false;
            });
    }

    lookupBtn.addEventListener('click', lookup);

    inputEl.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            lookup();
        }
    });

    copyBtn.addEventListener('click', function () {
        var address = addressFields
            .map(function (field) {
                var valueEl = document.getElementById('cep-' + field);
                return valueEl ? valueEl.textContent : '';
            })
            .filter(function (value) { return value && value !== '—'; })
            .join(', ');

        if (!address) return;
        navigator.clipboard.writeText(address).then(function () {
            var original = copyBtn.textContent;
            copyBtn.textContent = '✓';
            setTimeout(function () { copyBtn.textContent = original; }, 1200);
        });
    });
})();
