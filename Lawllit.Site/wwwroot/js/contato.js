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
