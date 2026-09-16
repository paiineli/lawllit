(function () {
    'use strict';

    var entrada = document.getElementById('base64-entrada');
    if (!entrada) return;

    var saida = document.getElementById('base64-saida');
    var botaoCodificar = document.getElementById('base64-codificar');
    var botaoDecodificar = document.getElementById('base64-decodificar');
    var botaoLimpar = document.getElementById('base64-limpar');
    var erro = document.getElementById('base64-erro');

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
