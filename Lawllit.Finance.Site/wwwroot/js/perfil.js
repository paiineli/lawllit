(function () {
    'use strict';

    // O alto contraste é um tema nosso, o Bootstrap só conhece dark e light, por isso os dois
    // atributos: um manda no Bootstrap e o outro nas nossas regras.
    function aplicarTema(valor) {
        document.documentElement.setAttribute('data-bs-theme', valor === 'high-contrast' ? 'dark' : valor);
        document.documentElement.setAttribute('data-theme', valor);
    }

    function aplicarTamanhoFonte(valor) {
        if (valor === 'normal') document.documentElement.removeAttribute('data-font-size');
        else document.documentElement.setAttribute('data-font-size', valor);
    }

    window.ligarPreferencia({
        area: 'opcoes-tema',
        atributo: 'valor-tema',
        chaveDataset: 'valorTema',
        classeAtiva: 'theme-card--active',
        chave: 'tema',
        aoEscolher: aplicarTema,
    });

    window.ligarPreferencia({
        area: 'opcoes-tamanho-fonte',
        atributo: 'valor-tamanho-fonte',
        chaveDataset: 'valorTamanhoFonte',
        classeAtiva: 'font-size-card--active',
        chave: 'tamanhoFonte',
        aoEscolher: aplicarTamanhoFonte,
    });
})();
