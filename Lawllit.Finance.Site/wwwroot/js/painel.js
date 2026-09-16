(function () {
    'use strict';

    var dados = document.getElementById('dados-painel');
    if (!dados) return;

    var painel = JSON.parse(dados.textContent);

    var cores = ['#4ade80', '#60a5fa', '#f97316', '#f472b6', '#a78bfa', '#34d399'];
    var claro = document.documentElement.getAttribute('data-theme') === 'light';
    var corLegenda = claro ? '#374151' : '#e5e7eb';
    var corGrade = claro ? 'rgba(0,0,0,0.08)' : 'rgba(255,255,255,0.06)';
    var fonte = { family: 'JetBrains Mono', size: 11 };

    // O app é só em real, então moeda e formato não vêm mais da view.
    function formatarMoeda(valor, casas) {
        return 'R$ ' + valor.toLocaleString('pt-BR', { minimumFractionDigits: casas });
    }

    // Mais de cinco fatias viram um confete ilegível, então o excedente soma em "Outros".
    function agruparCategorias(categorias, maximo) {
        if (categorias.length <= maximo) return categorias;

        var principais = categorias.slice(0, maximo);
        var resto = categorias.slice(maximo).reduce(function (soma, categoria) {
            return soma + categoria.valor;
        }, 0);

        return principais.concat([{ rotulo: painel.rotuloOutros, valor: resto }]);
    }

    function montarRosca() {
        var categorias = agruparCategorias(painel.categorias, 5);

        new Chart(document.getElementById('grafico-rosca'), {
            type: 'doughnut',
            data: {
                labels: categorias.map(function (categoria) { return categoria.rotulo; }),
                datasets: [{
                    data: categorias.map(function (categoria) { return categoria.valor; }),
                    backgroundColor: cores,
                    borderWidth: 0,
                }],
            },
            options: {
                cutout: '60%',
                plugins: {
                    legend: { position: 'right', labels: { color: corLegenda, font: fonte, padding: 14, boxWidth: 12 } },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) { return ' ' + formatarMoeda(contexto.parsed, 2); },
                        },
                    },
                },
            },
        });
    }

    function montarBarras() {
        var tendencia = painel.tendencia;
        if (!tendencia || !tendencia.length) return;

        var rotulos = tendencia.map(function (mes) {
            return painel.meses[mes.mes - 1].slice(0, 3) + '/' + String(mes.ano).slice(-2);
        });

        // Verde receita, vermelho despesa, azul investimento, mesmas cores dos cards do topo.
        var series = [
            { rotulo: painel.rotuloReceita, chave: 'receitas', preenchimento: 'rgba(74, 222, 128, 0.7)', borda: '#4ade80' },
            { rotulo: painel.rotuloDespesa, chave: 'despesas', preenchimento: 'rgba(248, 113, 113, 0.7)', borda: '#f87171' },
            { rotulo: painel.rotuloInvestimento, chave: 'investimentos', preenchimento: 'rgba(96, 165, 250, 0.7)', borda: '#60a5fa' },
        ];

        new Chart(document.getElementById('grafico-barras'), {
            type: 'bar',
            data: {
                labels: rotulos,
                datasets: series.map(function (serie) {
                    return {
                        label: serie.rotulo,
                        data: tendencia.map(function (mes) { return mes[serie.chave]; }),
                        backgroundColor: serie.preenchimento,
                        borderColor: serie.borda,
                        borderWidth: 1,
                        borderRadius: 4,
                    };
                }),
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { labels: { color: corLegenda, font: fonte, boxWidth: 12, padding: 12 } },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) {
                                return ' ' + contexto.dataset.label + ': ' + formatarMoeda(contexto.parsed.y, 2);
                            },
                        },
                    },
                },
                scales: {
                    x: { ticks: { color: corLegenda, font: fonte }, grid: { color: corGrade } },
                    y: {
                        ticks: {
                            color: corLegenda,
                            font: fonte,
                            callback: function (valor) { return formatarMoeda(valor, 0); },
                        },
                        grid: { color: corGrade },
                    },
                },
            },
        });
    }

    function montarBarrasRanking() {
        var indicePaleta = 0;

        document.querySelectorAll('.ranking-fill[data-largura]').forEach(function (barra) {
            barra.style.width = barra.dataset.largura + '%';

            // Barra com cor própria não consome slot da paleta, senão a categoria escorregaria
            // de cor cada vez que outro card ganhasse ou perdesse uma barra.
            barra.style.backgroundColor = barra.dataset.cor || cores[indicePaleta++ % cores.length];
        });
    }

    if (painel.categorias.length > 0) montarRosca();
    montarBarras();
    montarBarrasRanking();
})();
