(function () {
    'use strict';

    var entrada = document.getElementById('arquivos-entrada');
    if (!entrada) return;

    var lista = document.getElementById('arquivos-lista');
    var botaoJuntar = document.getElementById('arquivos-unificar');
    var formulario = botaoJuntar.closest('form');
    var cortina = document.getElementById('progresso');
    var rotulo = document.getElementById('progresso-rotulo');
    var barra = document.getElementById('progresso-barra');

    entrada.addEventListener('change', function () {
        var arquivos = Array.from(entrada.files);

        lista.hidden = arquivos.length === 0;
        lista.replaceChildren(...arquivos.map(function (arquivo) {
            var item = document.createElement('span');
            item.className = 'util-arquivo';
            item.textContent = arquivo.name;
            return item;
        }));

        botaoJuntar.disabled = arquivos.length < 2;
    });

    formulario.addEventListener('submit', function (evento) {
        evento.preventDefault();

        var arquivos = Array.from(entrada.files);
        if (arquivos.length < 2) return;

        var molde = rotulo.dataset.molde || '{0} / {1}';

        // Arquivo pequeno junta em milissegundos, e a cortina piscando parece defeito.
        var duracaoMinima = 1500;
        var inicio = Date.now();

        cortina.hidden = false;
        mostrarProgresso(0, arquivos.length, molde);

        juntar(arquivos, molde)
            .then(function () {
                var decorrido = Date.now() - inicio;
                setTimeout(function () { cortina.hidden = true; }, Math.max(0, duracaoMinima - decorrido));
            })
            .catch(function () { cortina.hidden = true; });
    });

    async function juntar(arquivos, molde) {
        var saida = await PDFLib.PDFDocument.create();

        for (var indice = 0; indice < arquivos.length; indice++) {
            mostrarProgresso(indice + 1, arquivos.length, molde);

            var bytes = await arquivos[indice].arrayBuffer();
            var documento = await PDFLib.PDFDocument.load(bytes);
            var paginas = await saida.copyPages(documento, documento.getPageIndices());

            paginas.forEach(function (pagina) { saida.addPage(pagina); });
        }

        baixar(await saida.save());
    }

    function mostrarProgresso(atual, total, molde) {
        barra.style.width = Math.round(atual / total * 100) + '%';
        rotulo.textContent = molde.replace('{0}', atual).replace('{1}', total);
    }

    function baixar(bytes) {
        var url = URL.createObjectURL(new Blob([bytes], { type: 'application/pdf' }));

        var link = document.createElement('a');
        link.href = url;
        link.download = 'unificado.pdf';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        URL.revokeObjectURL(url);
    }
})();
