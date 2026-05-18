const chartColors = ['#4ade80', '#60a5fa', '#f97316', '#f472b6', '#a78bfa', '#34d399'];
const isLightTheme = document.documentElement.getAttribute('data-theme') === 'light';
const legendColor = isLightTheme ? '#374151' : '#e5e7eb';
const gridColor = isLightTheme ? 'rgba(0,0,0,0.08)' : 'rgba(255,255,255,0.06)';
const fontFamily = { family: 'JetBrains Mono', size: 11 };

function initDashboard(data) {
    if (data.categories.length > 0) buildPieChart(data.categories, data.othersLabel, data.currencySymbol, data.currencyLocale);
    buildBarChart(data.trend, data.months, data.incomeLabel, data.expensesLabel, data.currencySymbol, data.currencyLocale);
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

function buildBarChart(trendData, months, incomeLabel, expensesLabel, currencySymbol, currencyLocale) {
    if (!trendData || !trendData.length) return;

    const labels = trendData.map(trend => months[trend.month - 1].slice(0, 3) + '/' + String(trend.year).slice(-2));
    const incomes = trendData.map(trend => trend.income);
    const expenses = trendData.map(trend => trend.expenses);

    new Chart(document.getElementById('barChart'), {
        type: 'bar',
        data: {
            labels,
            datasets: [
                {
                    label: incomeLabel,
                    data: incomes,
                    backgroundColor: 'rgba(74, 222, 128, 0.7)',
                    borderColor: '#4ade80',
                    borderWidth: 1,
                    borderRadius: 4,
                },
                {
                    label: expensesLabel,
                    data: expenses,
                    backgroundColor: 'rgba(248, 113, 113, 0.7)',
                    borderColor: '#f87171',
                    borderWidth: 1,
                    borderRadius: 4,
                },
            ],
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
