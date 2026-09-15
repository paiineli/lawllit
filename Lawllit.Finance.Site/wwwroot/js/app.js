(function () {
    'use strict';

    // Tira o acento comparando sem ele, para "agua" achar "água" na busca em tela.
    function normalizar(texto) {
        return texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
    }

    // Filtra a tabela já renderizada, sem ida ao servidor, porque a página inteira do filtro
    // já está na tela.
    function buscaAoVivo(campo) {
        var busca = normalizar(campo.value.trim());
        var corpoTabela = document.querySelector('[data-tabela-busca]');
        if (!corpoTabela) return;

        var linhas = corpoTabela.querySelectorAll('[data-linha-busca]');
        var visiveis = 0;

        linhas.forEach(function (linha) {
            var bate = normalizar(linha.textContent).includes(busca);
            linha.hidden = !bate;
            if (bate) visiveis++;
        });

        var linhaVazia = corpoTabela.querySelector('[data-linha-vazia]');
        if (linhaVazia) linhaVazia.hidden = visiveis > 0 || linhas.length === 0;
    }

    // Esconde as categorias que não são do tipo escolhido e reposiciona a seleção quando a
    // categoria que estava marcada some.
    function filtrarCategoriasPorTipo(selectTipo) {
        var corpoModal = selectTipo.closest('.modal-body');
        if (!corpoModal) return;

        var selectCategoria = corpoModal.querySelector('[data-filtro-categoria]');
        if (!selectCategoria) return;

        var tipoEscolhido = selectTipo.value;

        selectCategoria.querySelectorAll('optgroup[data-tipo-grupo]').forEach(function (grupo) {
            grupo.hidden = grupo.dataset.tipoGrupo !== tipoEscolhido;
        });

        var escolhida = selectCategoria.options[selectCategoria.selectedIndex];
        if (!escolhida || !escolhida.closest('optgroup') || !escolhida.closest('optgroup').hidden) return;

        var primeiraVisivel = Array.from(selectCategoria.options).find(function (opcao) {
            var grupo = opcao.closest('optgroup');
            return !grupo || !grupo.hidden;
        });

        if (primeiraVisivel) selectCategoria.value = primeiraVisivel.value;
    }

    document.addEventListener('input', function (evento) {
        if (evento.target.matches('[data-busca-viva]')) buscaAoVivo(evento.target);
    });

    var campoBuscaInicial = document.querySelector('[data-busca-viva]');
    if (campoBuscaInicial && campoBuscaInicial.value) buscaAoVivo(campoBuscaInicial);

    document.addEventListener('change', function (evento) {
        if (evento.target.matches('[data-envio-automatico]')) evento.target.form.submit();
        if (evento.target.matches('[data-filtro-tipo]')) filtrarCategoriasPorTipo(evento.target);
    });

    document.addEventListener('click', function (evento) {
        var botao = evento.target.closest('[data-ver-senha]');
        if (!botao) return;

        var campo = document.getElementById(botao.dataset.verSenha);
        if (!campo) return;

        var visivel = campo.type === 'text';
        campo.type = visivel ? 'password' : 'text';
        botao.querySelector('i').className = visivel ? 'bi bi-eye' : 'bi bi-eye-slash';
    });

    // O modal nasce com o select de tipo já preenchido, então o filtro roda ao abrir.
    document.addEventListener('show.bs.modal', function (evento) {
        var selectTipo = evento.target.querySelector('[data-filtro-tipo]');
        if (selectTipo) filtrarCategoriasPorTipo(selectTipo);
    });

    // O app é só em real, então o valor digitado sempre usa vírgula decimal. O ponto que a
    // pessoa digitar vira vírgula, e o ponto de milhar sai fora quando já existe vírgula.
    document.addEventListener('submit', function (evento) {
        evento.target.querySelectorAll('[data-campo-decimal]').forEach(function (campo) {
            if (campo.value.includes(',')) campo.value = campo.value.replace(/\./g, '');
            else campo.value = campo.value.replace('.', ',');
        });
    });

    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (alerta) {
            alerta.addEventListener('transitionend', function () { alerta.remove(); }, { once: true });
            alerta.classList.remove('show');
        });
    }, 4000);

    // Grava a preferência sem recarregar a tela. Exposta porque o perfil.js registra as duas,
    // tema e tamanho de fonte, que mudam de nome de atributo mas seguem o mesmo fluxo.
    window.ligarPreferencia = function (config) {
        var area = document.getElementById(config.area);
        var formulario = document.getElementById('pref-form');
        if (!area || !formulario) return;

        var token = formulario.querySelector('[name="__RequestVerificationToken"]').value;

        area.addEventListener('click', function (evento) {
            var cartao = evento.target.closest('[data-' + config.atributo + ']');
            if (!cartao) return;

            var valor = cartao.dataset[config.chaveDataset];

            area.querySelectorAll('[data-' + config.atributo + ']').forEach(function (opcao) {
                opcao.classList.remove(config.classeAtiva);
            });
            cartao.classList.add(config.classeAtiva);

            if (config.aoEscolher) config.aoEscolher(valor);

            fetch(formulario.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: '__RequestVerificationToken=' + encodeURIComponent(token)
                    + '&chave=' + encodeURIComponent(config.chave)
                    + '&valor=' + encodeURIComponent(valor)
            });
        });
    };
})();
