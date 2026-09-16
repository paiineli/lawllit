// Menu do celular, em tela cheia. Isto era o único motivo pelo qual o site público
// carregava o JavaScript do Bootstrap inteiro, 80 KB para abrir e fechar uma lista.
(function () {
    var botao = document.getElementById('menu-botao');
    var menu = document.getElementById('menu-itens');

    if (!botao || !menu) return;

    function abrir(aberto) {
        menu.classList.toggle('aberto', aberto);
        document.body.classList.toggle('menu-aberto', aberto);
        botao.setAttribute('aria-expanded', String(aberto));
        botao.setAttribute('aria-label', aberto ? 'Fechar menu' : 'Abrir menu');
    }

    botao.addEventListener('click', function () {
        abrir(!menu.classList.contains('aberto'));
    });

    // Fecha ao tocar num item, senão o menu cobre a seção que acabou de ser aberta.
    menu.addEventListener('click', function (evento) {
        if (evento.target.closest('.menu-item')) abrir(false);
    });

    document.addEventListener('click', function (evento) {
        if (!menu.contains(evento.target) && !botao.contains(evento.target)) abrir(false);
    });

    document.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape') abrir(false);
    });
})();

// Entrada ao rolar. Quem já está visível no carregamento não recebe classe nenhuma, para
// a primeira dobra nascer pronta em vez de aparecer depois.
(function () {
    var alvos = document.querySelectorAll('[data-revelar]');
    var pendentes = [];

    alvos.forEach(function (alvo) {
        if (alvo.getBoundingClientRect().top > window.innerHeight * 0.9) {
            alvo.classList.add('oculto');
            pendentes.push(alvo);
        }
    });

    if (!pendentes.length || !('IntersectionObserver' in window)) return;

    var observador = new IntersectionObserver(function (entradas) {
        entradas.forEach(function (entrada) {
            if (!entrada.isIntersecting) return;

            entrada.target.classList.add('revelado');
            entrada.target.classList.remove('oculto');
            observador.unobserve(entrada.target);
        });
    }, { threshold: 0.12 });

    pendentes.forEach(function (alvo) { observador.observe(alvo); });
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
