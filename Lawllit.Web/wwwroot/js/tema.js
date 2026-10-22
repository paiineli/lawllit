// Roda no <head>, antes do CSS pintar, para a página não piscar clara ao abrir no escuro.
(function () {
    var raiz = document.documentElement;
    var chave = 'lawllit-tema';
    var aparelho = window.matchMedia('(prefers-color-scheme: dark)');

    function lerSalvo() {
        try { return localStorage.getItem(chave); } catch (erro) { return null; }
    }

    function salvar(tema) {
        try { localStorage.setItem(chave, tema); } catch (erro) { }
    }

    function doAparelho() {
        return aparelho.matches ? 'escuro' : 'claro';
    }

    // O finance escreve o tema da conta no HTML, e aí quem manda é a conta, não o navegador.
    var temaDaConta = raiz.hasAttribute('data-tema-conta');

    if (!temaDaConta) {
        raiz.setAttribute('data-tema', lerSalvo() || doAparelho());

        aparelho.addEventListener('change', function () {
            if (!lerSalvo()) raiz.setAttribute('data-tema', doAparelho());
        });
    }

    document.addEventListener('click', function (evento) {
        var botao = evento.target.closest('[data-alternar-tema]');
        if (!botao) return;

        var novo = raiz.getAttribute('data-tema') === 'claro' ? 'escuro' : 'claro';
        raiz.setAttribute('data-tema', novo);

        if (!temaDaConta) salvar(novo);

        // O finance escuta para gravar na conta.
        document.dispatchEvent(new CustomEvent('lawllit:tema', { detail: novo }));
    });
})();
