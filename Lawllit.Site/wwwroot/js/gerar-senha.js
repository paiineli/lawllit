(function () {
    'use strict';

    var tamanho = document.getElementById('senha-tamanho');
    if (!tamanho) return;

    var tamanhoTexto = document.getElementById('senha-tamanho-valor');
    var usarMaiusculas = document.getElementById('senha-maiusculas');
    var usarMinusculas = document.getElementById('senha-minusculas');
    var usarNumeros = document.getElementById('senha-numeros');
    var usarSimbolos = document.getElementById('senha-simbolos');
    var botaoGerar = document.getElementById('senha-gerar');
    var saida = document.getElementById('senha-resultado');
    var forca = document.getElementById('senha-forca');

    var alfabetos = {
        maiusculas: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
        minusculas: 'abcdefghijklmnopqrstuvwxyz',
        numeros: '0123456789',
        simbolos: '!@#$%&*?-_=+',
    };

    function montarAlfabeto() {
        var alfabeto = '';

        if (usarMaiusculas.checked) alfabeto += alfabetos.maiusculas;
        if (usarMinusculas.checked) alfabeto += alfabetos.minusculas;
        if (usarNumeros.checked) alfabeto += alfabetos.numeros;
        if (usarSimbolos.checked) alfabeto += alfabetos.simbolos;

        return alfabeto;
    }

    // Entropia em bits, que é o tamanho vezes o log na base 2 do alfabeto.
    function mostrarForca(comprimento, tamanhoAlfabeto) {
        var entropia = comprimento * (Math.log(tamanhoAlfabeto) / Math.log(2));
        var nivel = entropia < 40 ? 'fraca' : entropia < 70 ? 'media' : 'forte';

        forca.textContent = forca.dataset.rotulo + ': ' + forca.dataset[nivel];
        forca.className = 'util-forca forca-' + nivel;
    }

    function gerar() {
        var alfabeto = montarAlfabeto();

        if (!alfabeto) {
            saida.value = '';
            forca.textContent = '';
            return;
        }

        // getRandomValues e não Math.random: senha precisa de fonte criptográfica.
        var comprimento = parseInt(tamanho.value, 10);
        var sorteio = new Uint32Array(comprimento);
        window.crypto.getRandomValues(sorteio);

        var senha = '';
        for (var indice = 0; indice < comprimento; indice++) {
            senha += alfabeto[sorteio[indice] % alfabeto.length];
        }

        saida.value = senha;
        mostrarForca(comprimento, alfabeto.length);
    }

    tamanho.addEventListener('input', function () {
        tamanhoTexto.textContent = tamanho.value;
    });

    botaoGerar.addEventListener('click', gerar);

    gerar();
})();
