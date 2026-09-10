// Menu do celular. Isto era o único motivo pelo qual o site público carregava o
// JavaScript do Bootstrap inteiro, 80 KB para abrir e fechar uma lista.
(function () {
    var toggle = document.getElementById('nav-toggle');
    var menu = document.getElementById('nav-menu');

    if (!toggle || !menu) return;

    function setOpen(open) {
        menu.classList.toggle('is-open', open);
        toggle.setAttribute('aria-expanded', String(open));
    }

    toggle.addEventListener('click', function () {
        setOpen(!menu.classList.contains('is-open'));
    });

    // Fecha ao clicar fora e ao apertar Esc, senão o menu aberto cobre a página e o
    // único jeito de fechar é acertar o botão de novo.
    document.addEventListener('click', function (event) {
        if (!menu.contains(event.target) && !toggle.contains(event.target)) setOpen(false);
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') setOpen(false);
    });
})();
