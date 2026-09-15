(function () {
    'use strict';

    var entrada = document.getElementById('b64-input');
    if (!entrada) return;

    var saida = document.getElementById('b64-output');
    var botaoCodificar = document.getElementById('b64-encode');
    var botaoDecodificar = document.getElementById('b64-decode');
    var botaoLimpar = document.getElementById('b64-clear');
    var erro = document.getElementById('b64-error');

    // Passa por bytes em vez de btoa direto, senão acento e emoji estouram o btoa.
    function codificar(texto) {
        var bytes = new TextEncoder().encode(texto);
        var binario = Array.from(bytes, function (byte) { return String.fromCharCode(byte); }).join('');
        return btoa(binario);
    }

    function decodificar(texto) {
        try {
            var binario = atob(texto.replace(/\s+/g, ''));
            var bytes = Uint8Array.from(binario, function (caractere) { return caractere.charCodeAt(0); });
            return new TextDecoder().decode(bytes);
        } catch (excecao) {
            return null;
        }
    }

    // Uma linha de entrada vira uma linha de saída, para dar conta de lista colada de uma vez.
    botaoCodificar.addEventListener('click', function () {
        erro.hidden = true;
        saida.value = entrada.value.split('\n').map(codificar).join('\n');
    });

    botaoDecodificar.addEventListener('click', function () {
        var linhas = entrada.value.split('\n').map(decodificar);

        if (linhas.some(function (linha) { return linha === null; })) {
            erro.hidden = false;
            saida.value = '';
        } else {
            erro.hidden = true;
            saida.value = linhas.join('\n');
        }
    });

    botaoLimpar.addEventListener('click', function () {
        entrada.value = '';
        saida.value = '';
        erro.hidden = true;
    });
})();
