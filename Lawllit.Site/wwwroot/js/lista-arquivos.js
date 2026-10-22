// Lista das ferramentas de juntar. Cada escolha soma à anterior, e o x tira um arquivo, porque
// no celular escolher tudo de uma vez é difícil e começar de novo por um engano cansa.
window.ListaArquivos = function (entrada, lista, botao, minimo) {
    'use strict';

    var arquivos = [];
    var area = entrada.closest('.envio');
    var titulo = area.querySelector('.envio-titulo');
    var tituloInicial = titulo.innerHTML;

    function mesmoArquivo(a, b) {
        return a.name === b.name && a.size === b.size && a.lastModified === b.lastModified;
    }

    function montarItem(arquivo, indice) {
        var item = document.createElement('li');
        item.className = 'lista-item';

        // O número na frente mostra a ordem em que o arquivo entra no resultado.
        var ordem = document.createElement('span');
        ordem.className = 'arquivo-ordem';
        ordem.textContent = indice + 1;

        var nome = document.createElement('span');
        nome.className = 'lista-item-corpo lista-item-titulo';
        nome.textContent = arquivo.name;

        var remover = document.createElement('button');
        remover.type = 'button';
        remover.className = 'botao botao-icone botao-icone-plano';
        remover.setAttribute('aria-label', 'Tirar ' + arquivo.name + ' da lista');
        remover.textContent = '×';
        remover.addEventListener('click', function () {
            arquivos.splice(indice, 1);
            atualizar();
        });

        item.append(ordem, nome, remover);
        return item;
    }

    function atualizar() {
        lista.hidden = arquivos.length === 0;
        lista.replaceChildren.apply(lista, arquivos.map(montarItem));
        botao.disabled = arquivos.length < minimo;

        if (arquivos.length === 0) titulo.innerHTML = tituloInicial;
        else titulo.textContent = arquivos.length === 1 ? '1 arquivo na lista, toque para somar mais' : arquivos.length + ' arquivos na lista, toque para somar mais';
    }

    entrada.addEventListener('change', function () {
        Array.from(entrada.files).forEach(function (novo) {
            if (!arquivos.some(function (atual) { return mesmoArquivo(atual, novo); })) arquivos.push(novo);
        });

        // Limpa o campo para a próxima escolha disparar o change de novo, mesmo com o mesmo arquivo.
        entrada.value = '';
        atualizar();
    });

    return {
        arquivos: function () { return arquivos.slice(); }
    };
};
