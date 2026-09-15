(function () {
    'use strict';

    var entrada = document.getElementById('cep-input');
    if (!entrada) return;

    var botaoConsultar = document.getElementById('cep-lookup');
    var resultado = document.getElementById('cep-result');
    var erro = document.getElementById('cep-error');
    var botaoCopiar = document.getElementById('cep-copy');

    var campos = ['cep', 'logradouro', 'complemento', 'bairro', 'localidade', 'uf', 'ddd'];
    var camposEndereco = ['logradouro', 'bairro', 'localidade', 'uf'];

    function mostrarErro(mensagem) {
        resultado.hidden = true;
        erro.textContent = mensagem;
        erro.hidden = false;
    }

    function preencher(dados) {
        campos.forEach(function (campo) {
            var elemento = document.getElementById('cep-' + campo);
            if (elemento) elemento.textContent = dados[campo] || '—';
        });

        erro.hidden = true;
        resultado.hidden = false;
    }

    function consultar() {
        var digitos = entrada.value.replace(/\D/g, '');

        if (digitos.length !== 8) {
            mostrarErro(erro.dataset.invalido);
            return;
        }

        botaoConsultar.disabled = true;

        // O ViaCEP responde 200 com { erro: true } quando o CEP não existe, então o status
        // não basta para saber se achou.
        fetch('https://viacep.com.br/ws/' + digitos + '/json/')
            .then(function (resposta) { return resposta.json(); })
            .then(function (dados) {
                if (dados.erro) {
                    mostrarErro(erro.dataset.naoencontrado);
                } else {
                    preencher(dados);
                }
            })
            .catch(function () {
                mostrarErro(erro.dataset.naoencontrado);
            })
            .finally(function () {
                botaoConsultar.disabled = false;
            });
    }

    botaoConsultar.addEventListener('click', consultar);

    entrada.addEventListener('keydown', function (evento) {
        if (evento.key === 'Enter') {
            evento.preventDefault();
            consultar();
        }
    });

    // Copia o endereço montado, e não um campo, por isso não usa o data-copiar do site.js.
    botaoCopiar.addEventListener('click', function () {
        var endereco = camposEndereco
            .map(function (campo) {
                var elemento = document.getElementById('cep-' + campo);
                return elemento ? elemento.textContent : '';
            })
            .filter(function (valor) { return valor && valor !== '—'; })
            .join(', ');

        window.copiar(botaoCopiar, endereco);
    });
})();
