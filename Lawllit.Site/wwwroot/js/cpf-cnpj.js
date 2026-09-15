(function () {
    'use strict';

    var entrada = document.getElementById('doc-input');
    if (!entrada) return;

    var saida = document.getElementById('doc-output');
    var botaoLimpar = document.getElementById('doc-clear');

    var pesosCnpjPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    var pesosCnpjSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    function somenteDigitos(valor) {
        return valor.replace(/\D/g, '');
    }

    // O peso do CPF é decrescente a partir do tamanho mais um, o que serve para os dois
    // dígitos: 10 a 2 sobre os nove primeiros, e 11 a 2 sobre os dez.
    function digitoCpf(digitos) {
        var soma = 0;
        var peso = digitos.length + 1;

        for (var indice = 0; indice < digitos.length; indice++) {
            soma += Number(digitos[indice]) * (peso - indice);
        }

        return (soma * 10) % 11 % 10;
    }

    function digitoCnpj(digitos) {
        var pesos = digitos.length === 12 ? pesosCnpjPrimeiro : pesosCnpjSegundo;
        var soma = 0;

        for (var indice = 0; indice < digitos.length; indice++) {
            soma += Number(digitos[indice]) * pesos[indice];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    // Todos os dígitos iguais passam na conta do verificador, por isso a recusa explícita.
    function cpfValido(digitos) {
        if (digitos.length !== 11 || /^(\d)\1{10}$/.test(digitos)) return false;
        if (digitoCpf(digitos.slice(0, 9)) !== Number(digitos[9])) return false;
        return digitoCpf(digitos.slice(0, 10)) === Number(digitos[10]);
    }

    function cnpjValido(digitos) {
        if (digitos.length !== 14 || /^(\d)\1{13}$/.test(digitos)) return false;
        if (digitoCnpj(digitos.slice(0, 12)) !== Number(digitos[12])) return false;
        return digitoCnpj(digitos.slice(0, 13)) === Number(digitos[13]);
    }

    function documentoValido(digitos) {
        if (digitos.length === 11) return cpfValido(digitos);
        if (digitos.length === 14) return cnpjValido(digitos);
        return false;
    }

    function formatar(digitos) {
        if (digitos.length === 11) {
            return digitos.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
        }

        if (digitos.length === 14) {
            return digitos.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
        }

        return digitos;
    }

    function digitosAleatorios(quantidade) {
        var resultado = '';

        for (var indice = 0; indice < quantidade; indice++) {
            resultado += Math.floor(Math.random() * 10);
        }

        return resultado;
    }

    function gerarCpf() {
        var base = digitosAleatorios(9);
        base += digitoCpf(base);
        base += digitoCpf(base);
        return formatar(base);
    }

    // O 0001 é o número da matriz, que é o que se espera de um CNPJ gerado para teste.
    function gerarCnpj() {
        var base = digitosAleatorios(8) + '0001';
        base += digitoCnpj(base);
        base += digitoCnpj(base);
        return formatar(base);
    }

    function lerLinhas() {
        return entrada.value
            .split('\n')
            .map(function (linha) { return linha.trim(); })
            .filter(function (linha) { return linha.length > 0; });
    }

    function validar() {
        saida.value = lerLinhas().map(function (linha) {
            var digitos = somenteDigitos(linha);
            return documentoValido(digitos) ? saida.dataset.valido : saida.dataset.invalido;
        }).join('\n');
    }

    function transformar(conversao) {
        saida.value = lerLinhas().map(conversao).join('\n');
    }

    function acrescentar(valor) {
        saida.value = saida.value ? saida.value + '\n' + valor : valor;
    }

    var acoes = {
        validar: validar,
        formatar: function () { transformar(function (linha) { return formatar(somenteDigitos(linha)); }); },
        limpar: function () { transformar(somenteDigitos); },
        'gerar-cpf': function () { acrescentar(gerarCpf()); },
        'gerar-cnpj': function () { acrescentar(gerarCnpj()); },
    };

    document.querySelectorAll('[data-doc]').forEach(function (botao) {
        botao.addEventListener('click', function () {
            var acao = acoes[botao.dataset.doc];
            if (acao) acao();
        });
    });

    botaoLimpar.addEventListener('click', function () {
        entrada.value = '';
        saida.value = '';
    });
})();
