(function () {
    'use strict';

    var entrada = document.getElementById('arquivos-entrada');
    if (!entrada) return;

    var botaoJuntar = document.getElementById('arquivos-unificar');
    var formulario = botaoJuntar.closest('form');
    var cortina = document.getElementById('progresso');
    var rotulo = document.getElementById('progresso-rotulo');
    var barra = document.getElementById('progresso-barra');

    var lista = ListaArquivos(entrada, document.getElementById('arquivos-lista'), botaoJuntar, 2);

    formulario.addEventListener('submit', function (evento) {
        evento.preventDefault();

        var arquivos = lista.arquivos();
        if (arquivos.length < 2) return;

        var molde = rotulo.dataset.molde || '{0} / {1}';
        var linhas = [];

        // Arquivo pequeno junta em milissegundos, e a cortina piscando parece defeito.
        var duracaoMinima = 1500;
        var inicio = Date.now();

        cortina.hidden = false;
        mostrarProgresso(0, arquivos.length, molde);

        // O FileReader é assíncrono, então a leitura encadeia em vez de usar laço, senão a
        // ordem das planilhas na saída dependeria de qual terminasse primeiro.
        function lerArquivo(indice) {
            if (indice >= arquivos.length) {
                baixar(linhas);

                var decorrido = Date.now() - inicio;
                setTimeout(function () { cortina.hidden = true; }, Math.max(0, duracaoMinima - decorrido));
                return;
            }

            mostrarProgresso(indice + 1, arquivos.length, molde);

            var leitor = new FileReader();
            // Arquivo que não abre para tudo e avisa, senão a planilha saía incompleta sem ninguém saber.
            function falhar() {
                cortina.hidden = true;
                Lawllit.avisar('Não deu para ler ' + arquivos[indice].name + '. Confira se é uma planilha válida.', 'erro');
            }

            leitor.onload = function (evento) {
                try {
                    var planilha = XLSX.read(new Uint8Array(evento.target.result), { type: 'array' });
                    var abaInicial = planilha.Sheets[planilha.SheetNames[0]];
                    var conteudo = XLSX.utils.sheet_to_json(abaInicial, { header: 1, defval: '' });

                    // Só o primeiro arquivo traz o cabeçalho, senão ele se repetiria no meio.
                    linhas = indice === 0 ? conteudo : linhas.concat(conteudo.slice(1));
                } catch (erro) {
                    falhar();
                    return;
                }

                lerArquivo(indice + 1);
            };
            leitor.onerror = falhar;
            leitor.readAsArrayBuffer(arquivos[indice]);
        }

        lerArquivo(0);
    });

    function mostrarProgresso(atual, total, molde) {
        barra.style.width = Math.round(atual / total * 100) + '%';
        rotulo.textContent = molde.replace('{0}', atual).replace('{1}', total);
    }

    function baixar(linhas) {
        var aba = XLSX.utils.aoa_to_sheet(linhas);
        var planilha = XLSX.utils.book_new();

        XLSX.utils.book_append_sheet(planilha, aba, 'Unificado');
        XLSX.writeFile(planilha, 'unificado.xlsx');
    }
})();
