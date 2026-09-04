const chartColors = ['#4ade80', '#60a5fa', '#f97316', '#f472b6', '#a78bfa', '#34d399'];
const isLightTheme = document.documentElement.getAttribute('data-theme') === 'light';
const legendColor = isLightTheme ? '#374151' : '#e5e7eb';
const gridColor = isLightTheme ? 'rgba(0,0,0,0.08)' : 'rgba(255,255,255,0.06)';
const fontFamily = { family: 'JetBrains Mono', size: 11 };

function initDashboard(data) {
    if (data.categories.length > 0) buildPieChart(data.categories, data.othersLabel, data.currencySymbol, data.currencyLocale);
    buildBarChart(data);
}

function buildPieChart(categoryData, othersLabel, currencySymbol, currencyLocale) {
    const collapsed = collapseCategories(categoryData, othersLabel);
    new Chart(document.getElementById('pieChart'), {
        type: 'doughnut',
        data: {
            labels: collapsed.map(category => category.label),
            datasets: [{ data: collapsed.map(category => category.value), backgroundColor: chartColors, borderWidth: 0 }],
        },
        options: {
            cutout: '60%',
            plugins: {
                legend: { position: 'right', labels: { color: legendColor, font: fontFamily, padding: 14, boxWidth: 12 } },
                tooltip: { callbacks: { label: ctx => ` ${currencySymbol} ${ctx.parsed.toLocaleString(currencyLocale, { minimumFractionDigits: 2 })}` } },
            },
        },
    });
}

function buildBarChart(data) {
    const trendData = data.trend;
    if (!trendData || !trendData.length) return;

    const currencySymbol = data.currencySymbol;
    const currencyLocale = data.currencyLocale;
    const labels = trendData.map(trend => data.months[trend.month - 1].slice(0, 3) + '/' + String(trend.year).slice(-2));

    // Verde receita, vermelho despesa, azul investimento, mesmas cores dos cards do topo.
    const series = [
        { label: data.incomeLabel, key: 'income', fill: 'rgba(74, 222, 128, 0.7)', border: '#4ade80' },
        { label: data.expensesLabel, key: 'expenses', fill: 'rgba(248, 113, 113, 0.7)', border: '#f87171' },
        { label: data.investmentsLabel, key: 'investments', fill: 'rgba(96, 165, 250, 0.7)', border: '#60a5fa' },
    ];

    new Chart(document.getElementById('barChart'), {
        type: 'bar',
        data: {
            labels,
            datasets: series.map(serie => ({
                label: serie.label,
                data: trendData.map(trend => trend[serie.key]),
                backgroundColor: serie.fill,
                borderColor: serie.border,
                borderWidth: 1,
                borderRadius: 4,
            })),
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { labels: { color: legendColor, font: fontFamily, boxWidth: 12, padding: 12 } },
                tooltip: {
                    callbacks: {
                        label: ctx => ` ${ctx.dataset.label}: ${currencySymbol} ${ctx.parsed.y.toLocaleString(currencyLocale, { minimumFractionDigits: 2 })}`,
                    },
                },
            },
            scales: {
                x: { ticks: { color: legendColor, font: fontFamily }, grid: { color: gridColor } },
                y: {
                    ticks: {
                        color: legendColor,
                        font: fontFamily,
                        callback: value => currencySymbol + ' ' + value.toLocaleString(currencyLocale, { minimumFractionDigits: 0 }),
                    },
                    grid: { color: gridColor },
                },
            },
        },
    });
}

function collapseCategories(data, othersLabel, max = 5) {
    if (data.length <= max) return data;
    const top = data.slice(0, max);
    const othersTotal = data.slice(max).reduce((sum, category) => sum + category.value, 0);
    return [...top, { label: othersLabel, value: othersTotal }];
}

function initRankingBars() {
    document.querySelectorAll('.ranking-fill[data-fill-width]').forEach(function (element, index) {
        element.style.width = element.dataset.fillWidth + '%';
        element.style.backgroundColor = chartColors[index % chartColors.length];
    });
}

(function () {
    var dataElement = document.getElementById('dashboardData');
    if (!dataElement) return;
    var data = JSON.parse(dataElement.textContent);
    initDashboard(data);
    initRankingBars();
})();
