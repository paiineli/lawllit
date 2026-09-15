// O link do currículo na home traz ?print=1 e abre a caixa de impressão sozinho. O botão
// atende quem chega pelo endereço direto.
(function () {
    var botao = document.getElementById('cv-print');

    if (botao) botao.addEventListener('click', function () { window.print(); });

    // O atraso existe porque o Chrome abre a caixa antes de terminar a paginação, e a prévia
    // sai com a quebra errada.
    if (new URLSearchParams(window.location.search).get('print') === '1') {
        window.addEventListener('load', function () { setTimeout(window.print, 300); });
    }
})();
