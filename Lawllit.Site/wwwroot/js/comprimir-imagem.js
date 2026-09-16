(function () {
    'use strict';

    var entrada = document.getElementById('imagem-entrada');
    if (!entrada) return;

    var botoesNivel = document.querySelectorAll('[data-nivel]');
    var resultado = document.getElementById('imagem-resultado');
    var previa = document.getElementById('imagem-previa');
    var tamanhoOriginalTexto = document.getElementById('imagem-original');
    var tamanhoFinalTexto = document.getElementById('imagem-comprimida');
    var reducaoTexto = document.getElementById('imagem-reducao');
    var botaoBaixar = document.getElementById('imagem-baixar');

    var niveis = {
        alta: { qualidade: 0.85, larguraMaxima: 2560 },
        equilibrado: { qualidade: 0.7, larguraMaxima: 1920 },
        menor: { qualidade: 0.5, larguraMaxima: 1280 },
    };

    var imagem = null;
    var tamanhoOriginal = 0;
    var urlComprimida = null;

    // PNG sai PNG. Reencodar para JPEG achatava a transparência em preto, e o site aceita
    // .png na entrada. Sem controle de qualidade no PNG, o ganho vem do redimensionamento.
    var tipoSaida = 'image/jpeg';

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

    function comprimir(nivel) {
        if (!imagem) return;

        var preset = niveis[nivel];
        var escala = imagem.width > preset.larguraMaxima ? preset.larguraMaxima / imagem.width : 1;

        var tela = document.createElement('canvas');
        tela.width = Math.round(imagem.width * escala);
        tela.height = Math.round(imagem.height * escala);
        tela.getContext('2d').drawImage(imagem, 0, 0, tela.width, tela.height);

        tela.toBlob(function (blob) {
            if (!blob) return;

            descartarUrlAnterior();
            urlComprimida = URL.createObjectURL(blob);

            previa.src = urlComprimida;
            tamanhoOriginalTexto.textContent = formatarTamanho(tamanhoOriginal);
            tamanhoFinalTexto.textContent = formatarTamanho(blob.size);

            var reducao = tamanhoOriginal > 0
                ? Math.max(0, Math.round((1 - blob.size / tamanhoOriginal) * 100))
                : 0;

            reducaoTexto.textContent = '-' + reducao + '%';
            resultado.hidden = false;
        }, tipoSaida, preset.qualidade);
    }

    entrada.addEventListener('change', function () {
        var arquivo = entrada.files[0];
        if (!arquivo) return;

        tamanhoOriginal = arquivo.size;
        tipoSaida = arquivo.type === 'image/png' ? 'image/png' : 'image/jpeg';

        var leitor = new FileReader();
        leitor.onload = function () {
            var carregada = new Image();
            carregada.onload = function () {
                imagem = carregada;
                botoesNivel.forEach(function (botao) { botao.disabled = false; });
                comprimir('equilibrado');
            };
            carregada.src = leitor.result;
        };
        leitor.readAsDataURL(arquivo);
    });

    botoesNivel.forEach(function (botao) {
        botao.addEventListener('click', function () {
            comprimir(botao.dataset.nivel);
        });
    });

    botaoBaixar.addEventListener('click', function () {
        if (!urlComprimida) return;

        var link = document.createElement('a');
        link.href = urlComprimida;
        link.download = tipoSaida === 'image/png' ? 'comprimida.png' : 'comprimida.jpg';
        link.click();
    });
})();
