(function () {
    'use strict';

    var entrada = document.getElementById('sql-entrada');
    if (!entrada) return;

    var saida = document.getElementById('sql-saida');
    var semRepetidos = document.getElementById('sql-duplicados');
    var comParenteses = document.getElementById('sql-parenteses');
    var botaoLimpar = document.getElementById('sql-limpar');

    function lerValores() {
        var valores = entrada.value
            .split('\n')
            .map(function (linha) { return linha.trim(); })
            .filter(function (linha) { return linha.length > 0; });

        if (semRepetidos.checked) {
            var vistos = Object.create(null);
            valores = valores.filter(function (valor) {
                if (vistos[valor]) return false;
                vistos[valor] = true;
                return true;
            });
        }

        return valores;
    }

    // Aspas dobradas dentro do valor, que é como o SQL escapa a própria aspa.
    function aplicarAspas(valor, estilo) {
        if (estilo === 'simples') return "'" + valor.replace(/'/g, "''") + "'";
        if (estilo === 'duplas') return '"' + valor.replace(/"/g, '""') + '"';
        return valor;
    }

    function montar(estilo) {
        var valores = lerValores();

        if (valores.length === 0) {
            saida.value = '';
            return;
        }

        var lista = valores.map(function (valor) { return aplicarAspas(valor, estilo); }).join(',');
        saida.value = comParenteses.checked ? '(' + lista + ')' : lista;
    }

    document.querySelectorAll('[data-aspas]').forEach(function (botao) {
        botao.addEventListener('click', function () {
            montar(botao.dataset.aspas);
        });
    });

    botaoLimpar.addEventListener('click', function () {
        entrada.value = '';
        saida.value = '';
    });
})();
