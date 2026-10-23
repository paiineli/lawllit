// Comportamento das telas do finance. O que é comum aos dois sites mora no lawllit.js.
(function () {
    'use strict';

    // Tira o acento comparando sem ele, para "agua" achar "água" na busca em tela.
    function normalizar(texto) {
        return texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
    }

    // Filtra na hora o que já está na tela. O Enter envia o formulário, e o servidor procura nas outras páginas.
    function buscar(campo) {
        var termo = normalizar(campo.value.trim());
        var linhas = document.querySelectorAll('[data-linha-busca]');
        var visiveis = 0;

        linhas.forEach(function (linha) {
            var bate = normalizar(linha.textContent).indexOf(termo) >= 0;
            linha.hidden = !bate;
            if (bate) visiveis++;
        });

        // Dia sem nenhum lançamento visível some inteiro, senão sobraria só o título.
        document.querySelectorAll('[data-grupo-busca]').forEach(function (grupo) {
            grupo.hidden = !grupo.querySelector('[data-linha-busca]:not([hidden])');
        });

        var vazio = document.querySelector('[data-busca-vazia]');
        if (vazio) vazio.hidden = visiveis > 0 || linhas.length === 0;
    }

    // Só as categorias do tipo marcado ficam no select, e a escolhida muda se tiver sumido.
    function filtrarCategorias(formulario) {
        var marcado = formulario.querySelector('[data-filtro-tipo]:checked');
        var select = formulario.querySelector('[data-filtro-categoria]');
        if (!marcado || !select) return;

        select.querySelectorAll('optgroup[data-tipo-grupo]').forEach(function (grupo) {
            grupo.hidden = grupo.getAttribute('data-tipo-grupo') !== marcado.value;
        });

        var escolhida = select.options[select.selectedIndex];
        if (escolhida && !escolhida.parentElement.hidden) return;

        var primeira = Array.prototype.find.call(select.options, function (opcao) { return !opcao.parentElement.hidden; });
        if (primeira) select.value = primeira.value;
    }

    function preencher(formulario, valores) {
        formulario.reset();

        Object.keys(valores).forEach(function (nome) {
            var campo = formulario.elements[nome];
            var valor = valores[nome];
            if (!campo) return;

            if (campo instanceof RadioNodeList) campo.value = valor;
            else if (campo.type === 'checkbox') campo.checked = valor === true;
            else campo.value = valor;
        });
    }

    // Uma gaveta só serve para criar e editar. Quem abre traz a ação, o título e os valores em data-*.
    function abrirGaveta(botao) {
        var gaveta = document.getElementById(botao.getAttribute('data-editar'));
        if (!gaveta) return;

        var formulario = gaveta.querySelector('[data-formulario-gaveta]');
        formulario.action = botao.getAttribute('data-acao');
        preencher(formulario, JSON.parse(botao.getAttribute('data-valores') || '{}'));
        gaveta.querySelector('[data-titulo-gaveta]').textContent = botao.getAttribute('data-titulo');

        var excluir = formulario.querySelector('[data-abrir-excluir]');
        if (excluir) {
            excluir.hidden = !botao.getAttribute('data-excluir');
            excluir.setAttribute('data-url', botao.getAttribute('data-excluir') || '');
            excluir.setAttribute('data-descricao', botao.getAttribute('data-descricao') || '');
        }

        filtrarCategorias(formulario);
        gaveta.showModal();
    }

    function abrirExclusao(botao) {
        var gaveta = document.getElementById('gaveta-excluir');
        if (!gaveta) return;

        gaveta.querySelector('[data-formulario-excluir]').action = botao.getAttribute('data-url');
        gaveta.querySelector('[data-descricao-excluir]').textContent = botao.getAttribute('data-descricao');
        botao.closest('dialog').close();
        gaveta.showModal();
    }

    function novaTransacao() {
        var botao = document.querySelector('button[data-nova-transacao]');
        if (botao) abrirGaveta(botao);
        return !!botao;
    }

    function salvarPreferencia(chave, valor) {
        var formulario = document.getElementById('form-preferencia');
        if (formulario) Lawllit.enviar(formulario.action, { chave: chave, valor: valor });
    }

    // O banco fala dark, light e high-contrast. O CSS fala escuro, claro e alto-contraste.
    var temas = { light: 'claro', dark: 'escuro', 'high-contrast': 'alto-contraste' };
    var tamanhos = { normal: null, large: 'grande', xlarge: 'maior' };

    function aplicarPreferencia(chave, valor) {
        var raiz = document.documentElement;

        if (chave === 'tema') raiz.setAttribute('data-tema', temas[valor]);

        if (chave === 'tamanhoFonte') {
            if (tamanhos[valor]) raiz.setAttribute('data-tamanho-fonte', tamanhos[valor]);
            else raiz.removeAttribute('data-tamanho-fonte');
        }
    }

    document.addEventListener('input', function (evento) {
        if (evento.target.matches('[data-busca-viva]')) buscar(evento.target);
    });

    document.addEventListener('change', function (evento) {
        if (evento.target.matches('[data-filtro-tipo]')) filtrarCategorias(evento.target.form);
    });

    document.addEventListener('click', function (evento) {
        var editar = evento.target.closest('[data-editar]');
        if (editar) {
            abrirGaveta(editar);
            return;
        }

        var excluir = evento.target.closest('[data-abrir-excluir]');
        if (excluir) {
            abrirExclusao(excluir);
            return;
        }

        // O + da barra do celular é link para quem está em outra tela. Aqui ele só abre a gaveta.
        var mais = evento.target.closest('a[data-nova-transacao]');
        if (mais && novaTransacao()) {
            evento.preventDefault();
            return;
        }

        var olho = evento.target.closest('[data-ver-senha]');
        if (olho) {
            var campo = document.getElementById(olho.getAttribute('data-ver-senha'));
            var mostrar = campo.type === 'password';
            campo.type = mostrar ? 'text' : 'password';
            olho.setAttribute('aria-pressed', String(mostrar));
            olho.setAttribute('aria-label', mostrar ? 'Esconder senha' : 'Mostrar senha');
            return;
        }

        var preferencia = evento.target.closest('[data-preferencia]');
        if (preferencia) {
            var chave = preferencia.getAttribute('data-preferencia');
            var valor = preferencia.getAttribute('data-valor');

            preferencia.parentElement.querySelectorAll('[data-preferencia]').forEach(function (opcao) {
                opcao.setAttribute('aria-pressed', String(opcao === preferencia));
            });

            aplicarPreferencia(chave, valor);
            salvarPreferencia(chave, valor);
        }
    });

    // O botão de tema do topo troca entre claro e escuro, e a conta guarda a escolha.
    document.addEventListener('lawllit:tema', function (evento) {
        salvarPreferencia('tema', evento.detail === 'claro' ? 'light' : 'dark');
    });

    // O app é só em real, então o valor digitado sempre usa vírgula decimal. O ponto que a
    // pessoa digitar vira vírgula, e o ponto de milhar sai fora quando já existe vírgula.
    document.addEventListener('submit', function (evento) {
        evento.target.querySelectorAll('[data-campo-decimal]').forEach(function (campo) {
            if (campo.value.indexOf(',') >= 0) campo.value = campo.value.replace(/\./g, '');
            else campo.value = campo.value.replace('.', ',');
        });
    });

    var buscaInicial = document.querySelector('[data-busca-viva]');
    if (buscaInicial && buscaInicial.value) buscar(buscaInicial);

    // Chegou pelo + de outra tela: abre a gaveta e tira o parâmetro, para o F5 não reabrir.
    var parametros = new URLSearchParams(window.location.search);
    if (parametros.has('novo') && novaTransacao()) {
        parametros.delete('novo');
        var resto = parametros.toString();
        history.replaceState(null, '', window.location.pathname + (resto ? '?' + resto : ''));
    }
})();
