(function () {
    'use strict';

    var entrada = document.getElementById('pdf-entrada');
    if (!entrada) return;

    var botoesNivel = document.querySelectorAll('[data-nivel]');
    var erro = document.getElementById('pdf-erro');
    var resultado = document.getElementById('pdf-resultado');
    var tamanhoOriginalTexto = document.getElementById('pdf-original');
    var tamanhoFinalTexto = document.getElementById('pdf-comprimido');
    var reducaoTexto = document.getElementById('pdf-reducao');
    var avisoSemGanho = document.getElementById('pdf-sem-ganho');
    var botaoBaixar = document.getElementById('pdf-baixar');
    var cortina = document.getElementById('progresso');
    var rotulo = document.getElementById('progresso-rotulo');
    var barra = document.getElementById('progresso-barra');

    // A versão sai da própria biblioteca carregada, assim a página e o worker nunca divergem.
    pdfjsLib.GlobalWorkerOptions.workerSrc =
        'https://cdn.jsdelivr.net/npm/pdfjs-dist@' + pdfjsLib.version + '/build/pdf.worker.min.js';

    // Escala 1 é 72 dpi. Abaixo disso o texto da página já fica ilegível.
    var niveis = {
        alta: { escala: 2, qualidade: 0.8 },
        equilibrado: { escala: 1.5, qualidade: 0.65 },
        menor: { escala: 1, qualidade: 0.5 },
    };

    var documento = null;
    var tamanhoOriginal = 0;
    var urlComprimida = null;
    var nomeSaida = 'comprimido.pdf';

    function formatarTamanho(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    }

    function descartarUrlAnterior() {
        if (urlComprimida) {
            URL.revokeObjectURL(urlComprimida);
            urlComprimida = null;
        }
    }

    function habilitarNiveis(habilitado) {
        botoesNivel.forEach(function (botao) { botao.disabled = !habilitado; });
    }

    function mostrarProgresso(atual, total, molde) {
        barra.style.width = Math.round(atual / total * 100) + '%';
        rotulo.textContent = molde.replace('{0}', atual).replace('{1}', total);
    }

    function paraBlob(tela, qualidade) {
        return new Promise(function (resolver) {
            tela.toBlob(resolver, 'image/jpeg', qualidade);
        });
    }

    // Sem como reencodar as imagens internas do pdf no navegador, cada página é redesenhada como JPEG.
    async function comprimir(nivel) {
        var preset = niveis[nivel];
        var saida = await PDFLib.PDFDocument.create();
        var total = documento.numPages;
        var molde = rotulo.dataset.molde || '{0} / {1}';

        for (var numero = 1; numero <= total; numero++) {
            mostrarProgresso(numero, total, molde);

            var pagina = await documento.getPage(numero);
            var tamanhoPagina = pagina.getViewport({ scale: 1 });
            var viewport = pagina.getViewport({ scale: preset.escala });

            var tela = document.createElement('canvas');
            tela.width = Math.floor(viewport.width);
            tela.height = Math.floor(viewport.height);

            await pagina.render({ canvasContext: tela.getContext('2d'), viewport: viewport }).promise;

            var blob = await paraBlob(tela, preset.qualidade);
            var jpeg = await saida.embedJpg(await blob.arrayBuffer());

            saida.addPage([tamanhoPagina.width, tamanhoPagina.height]).drawImage(jpeg, {
                x: 0,
                y: 0,
                width: tamanhoPagina.width,
                height: tamanhoPagina.height,
            });

            // Zerar a tela libera a memória na hora, senão pdf longo estoura o limite de canvas do celular.
            tela.width = 0;
            tela.height = 0;
            pagina.cleanup();
        }

        return saida.save();
    }

    function mostrarResultado(bytes) {
        descartarUrlAnterior();
        urlComprimida = URL.createObjectURL(new Blob([bytes], { type: 'application/pdf' }));

        tamanhoOriginalTexto.textContent = formatarTamanho(tamanhoOriginal);
        tamanhoFinalTexto.textContent = formatarTamanho(bytes.length);

        var reducao = tamanhoOriginal > 0
            ? Math.max(0, Math.round((1 - bytes.length / tamanhoOriginal) * 100))
            : 0;

        // Pdf só de texto já é pequeno, e virar imagem costuma deixar o arquivo maior.
        var semGanho = bytes.length >= tamanhoOriginal;

        reducaoTexto.textContent = '-' + reducao + '%';
        avisoSemGanho.hidden = !semGanho;
        botaoBaixar.disabled = semGanho;
        resultado.hidden = false;
    }

    function executar(nivel) {
        if (!documento) return;

        // Pdf de uma página comprime em milissegundos, e a cortina piscando parece defeito.
        var duracaoMinima = 1500;
        var inicio = Date.now();

        habilitarNiveis(false);
        cortina.hidden = false;

        comprimir(nivel)
            .then(mostrarResultado)
            .catch(function () { erro.hidden = false; })
            .finally(function () {
                var decorrido = Date.now() - inicio;
                setTimeout(function () {
                    cortina.hidden = true;
                    habilitarNiveis(true);
                }, Math.max(0, duracaoMinima - decorrido));
            });
    }

    entrada.addEventListener('change', async function () {
        var arquivo = entrada.files[0];
        if (!arquivo) return;

        erro.hidden = true;
        resultado.hidden = true;
        habilitarNiveis(false);
        descartarUrlAnterior();

        if (documento) {
            documento.destroy();
            documento = null;
        }

        tamanhoOriginal = arquivo.size;
        nomeSaida = arquivo.name.replace(/\.pdf$/i, '') + '-comprimido.pdf';

        try {
            // Sem isto o pdf.js testa eval, que a CSP bloqueia, e suja o console a cada arquivo.
            documento = await pdfjsLib.getDocument({
                data: await arquivo.arrayBuffer(),
                isEvalSupported: false,
            }).promise;
        } catch {
            erro.hidden = false;
            return;
        }

        executar('equilibrado');
    });

    botoesNivel.forEach(function (botao) {
        botao.addEventListener('click', function () {
            executar(botao.dataset.nivel);
        });
    });

    botaoBaixar.addEventListener('click', function () {
        if (!urlComprimida) return;

        var link = document.createElement('a');
        link.href = urlComprimida;
        link.download = nomeSaida;
        link.click();
    });
})();
