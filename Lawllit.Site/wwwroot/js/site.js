// Menu do celular. Isto era o único motivo pelo qual o site público carregava o
// JavaScript do Bootstrap inteiro, 80 KB para abrir e fechar uma lista.
(function () {
    var botao = document.getElementById('nav-toggle');
    var menu = document.getElementById('nav-menu');

    if (!botao || !menu) return;

    function abrir(aberto) {
        menu.classList.toggle('is-open', aberto);
        botao.setAttribute('aria-expanded', String(aberto));
    }

    botao.addEventListener('click', function () {
        abrir(!menu.classList.contains('is-open'));
    });

    // Fecha ao clicar fora e no Esc, senão o menu aberto cobre a página e só o botão fecha.
    document.addEventListener('click', function (evento) {
        if (!menu.contains(evento.target) && !botao.contains(evento.target)) abrir(false);
    });

    document.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape') abrir(false);
    });
})();

// Botão de copiar, um só para as cinco ferramentas que têm um. Cada arquivo repetia estas
// mesmas linhas, incluindo o ✓ e o tempo de voltar ao rótulo original.
window.copiar = function (botao, texto) {
    if (!texto) return;

    navigator.clipboard.writeText(texto).then(function () {
        var rotulo = botao.textContent;
        botao.textContent = '✓';
        setTimeout(function () { botao.textContent = rotulo; }, 1200);
    });
};

// data-copiar guarda o seletor do campo cuja saída vai para a área de transferência.
document.querySelectorAll('[data-copiar]').forEach(function (botao) {
    botao.addEventListener('click', function () {
        var origem = document.querySelector(botao.dataset.copiar);
        if (origem) window.copiar(botao, origem.value);
    });
});
