// Área de envio das ferramentas de arquivo. O input já cobre a área inteira, então clicar e
// soltar funcionam sozinhos. Aqui fica só o retorno visual, igual nas quatro páginas.
(function () {
    'use strict';

    document.querySelectorAll('.envio').forEach(function (area) {
        var entrada = area.querySelector('input[type="file"]');
        var titulo = area.querySelector('.envio-titulo');

        // O :hover não dispara enquanto o arquivo é arrastado, por isso a classe.
        ['dragenter', 'dragover'].forEach(function (tipo) {
            area.addEventListener(tipo, function () { area.classList.add('arrastando'); });
        });

        ['dragleave', 'drop'].forEach(function (tipo) {
            area.addEventListener(tipo, function () { area.classList.remove('arrastando'); });
        });

        // Com o input invisível, o nome do arquivo escolhido não apareceria em lugar nenhum. Nas
        // ferramentas de juntar quem escreve o título é o lista-arquivos.js, que soma as escolhas.
        if (area.hasAttribute('data-acumula')) return;

        entrada.addEventListener('change', function () {
            var arquivos = entrada.files;
            if (!arquivos.length) return;

            titulo.textContent = arquivos.length === 1
                ? arquivos[0].name
                : (area.dataset.varios || '{0}').replace('{0}', arquivos.length);
        });
    });
})();
