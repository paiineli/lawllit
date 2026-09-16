// Máquina de escrever das duas linhas da entrada. O texto vem do data-texto porque quem
// escreve o conteúdo da tela é a view, não o script.
(function () {
    'use strict';

    var linha1 = document.getElementById('linha-digitada-1');
    var linha2 = document.getElementById('linha-digitada-2');
    if (!linha1 || !linha2) return;

    var intervaloLetra = 60;

    function digitar(elemento, texto, aoTerminar) {
        var indice = 0;

        var timer = setInterval(function () {
            elemento.textContent += texto[indice];
            indice++;

            if (indice >= texto.length) {
                clearInterval(timer);
                if (aoTerminar) aoTerminar();
            }
        }, intervaloLetra);
    }

    digitar(linha1, linha1.dataset.texto, function () {
        setTimeout(function () { digitar(linha2, linha2.dataset.texto); }, 150);
    });
})();
