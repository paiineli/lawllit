(function () {
    'use strict';

    var entrada = document.getElementById('qr-input');
    if (!entrada) return;

    var botaoGerar = document.getElementById('qr-generate');
    var botaoBaixar = document.getElementById('qr-download');
    var botaoLimpar = document.getElementById('qr-clear');
    var area = document.getElementById('qr-canvas');
    var moldura = area.parentElement;

    // Correção de erro média, que aguenta o código sujo ou parcialmente coberto sem
    // inchar o desenho como o nível alto faz.
    var codigo = new QRCode(area, {
        width: 256,
        height: 256,
        correctLevel: QRCode.CorrectLevel.M,
    });

    function gerar() {
        var texto = entrada.value.trim();
        if (!texto) return;

        codigo.makeCode(texto);
        moldura.hidden = false;
        botaoBaixar.disabled = false;
    }

    botaoGerar.addEventListener('click', gerar);

    botaoBaixar.addEventListener('click', function () {
        if (botaoBaixar.disabled) return;

        var tela = area.querySelector('canvas');
        if (!tela) return;

        var link = document.createElement('a');
        link.href = tela.toDataURL('image/png');
        link.download = 'qrcode.png';
        link.click();
    });

    botaoLimpar.addEventListener('click', function () {
        entrada.value = '';
        codigo.clear();
        moldura.hidden = true;
        botaoBaixar.disabled = true;
    });
})();
