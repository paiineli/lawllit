// Troca o antes e depois da home e leva o assunto escolhido para o texto do formulário.
(function () {
    var chips = document.querySelectorAll('[data-cenario]');
    var paineis = document.querySelectorAll('[data-painel-cenario]');
    var mensagem = document.getElementById('contato-mensagem');

    chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            var escolhido = chip.getAttribute('data-cenario');

            chips.forEach(function (outro) {
                outro.setAttribute('aria-pressed', String(outro === chip));
            });

            paineis.forEach(function (painel) {
                var ativo = painel.getAttribute('data-painel-cenario') === escolhido;
                painel.hidden = !ativo;

                if (ativo && mensagem) mensagem.placeholder = painel.getAttribute('data-frase');
            });
        });
    });
})();

// Impede o clique duplo no envio. O formulário é um POST normal, então sem isto a segunda
// submissão sai antes da primeira responder e o limite de envio é gasto à toa.
(function () {
    var formulario = document.querySelector('form[action*="Contato"]');
    var botao = document.getElementById('contato-enviar');

    if (!formulario || !botao) return;

    formulario.addEventListener('submit', function () {
        // A validação nativa pode barrar o envio, então o estado só muda com os campos válidos.
        if (!formulario.checkValidity()) return;

        botao.disabled = true;
        botao.textContent = botao.dataset.enviando;
    });
})();
