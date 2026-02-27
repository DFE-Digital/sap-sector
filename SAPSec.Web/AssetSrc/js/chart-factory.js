(function () {
    const defaultColors = [
        '#DE6B24',
        '#27A0CC',
        '#003C56'
    ];

    const charts = {};

    function gdsVars(canvas) {
        const s = getComputedStyle(canvas);

        return {
            fontFamily: s.getPropertyValue('--gds-font-family').trim(),
            fontSize: parseInt(s.getPropertyValue('--gds-font-size')),
            text: s.getPropertyValue('--gds-text'),
            grey: s.getPropertyValue('--gds-grey'),
            labelBg: s.getPropertyValue('--gds-label-bg'),
            labelBorder: s.getPropertyValue('--gds-label-border'),
            labelPadding: parseInt(s.getPropertyValue('--gds-label-padding'))
        };
    }

    function buildChartOptions(type, gdsStyles, axisStep, axisSuffix, axisMax, showLegend, showDataLabels) {
        const common = {
            responsive: true,
            maintainAspectRatio: false,
            devicePixelRatio: Math.min(window.devicePixelRatio || 1, 2)
        };

        const fonts = {
            family: gdsStyles.fontFamily,
            size: gdsStyles.fontSize
        };

        const stepSize = axisStep;
        const legendOptions = {
            display: showLegend,
            position: 'bottom',
            labels: {
                usePointStyle: true,
                pointStyle: 'circle',
                boxWidth: 10,
                boxHeight: 10,
                padding: 16
            }
        };

        if (type === 'line') {
            return {
                ...common,
                layout: {
                    padding: { right: showDataLabels ? 100 : 0 }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: axisMax ?? undefined,
                        grid: {
                            display: true,
                            drawBorder: false,
                            color: (context) => {
                                return context.tick.value === 0 ? '#000' : '#ccc';
                            },
                            lineWidth: (context) => {
                                return context.tick.value === 0 ? 2 : 1;
                            }
                        },
                        border: { display: false },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            stepSize: stepSize,
                            callback: (value) => `${value}${axisSuffix}`
                        }
                    },
                    x: {
                        title: { display: false },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            display: true
                        },
                        grid: { display: false }
                    }
                },
                plugins: {
                    tooltip: { enabled: false },
                    legend: legendOptions,
                    title: {
                        display: false,
                        font: fonts
                    },
                    datalabels: {
                        anchor: 'end',
                        align: 'right',
                        offset: 10,
                        color: gdsStyles.text,
                        font: fonts,
                        display: showDataLabels
                            ? function (ctx) {
                                return ctx.dataIndex === ctx.dataset.data.length - 1;
                            }
                            : false,
                        formatter: function (value, context) {
                            return context.dataset.label;
                        },
                        clamp: true,
                        clip: false
                    }
                }
            };
        }

        if (type === 'bar') {
            return {
                ...common,
                indexAxis: 'y',
                scales: {
                    x: {
                        beginAtZero: true,
                        max: axisMax ?? undefined,
                        grid: {
                            display: true,
                            drawBorder: false,
                            color: (context) => {
                                return context.tick.value === 0 ? '#000' : '#ccc';
                            },
                            lineWidth: (context) => {
                                return context.tick.value === 0 ? 2 : 1;
                            }
                        },
                        border: { display: false },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            stepSize: stepSize,
                            callback: (value) => `${value}${axisSuffix}`
                        }
                    },
                    y: {
                        grid: {
                            display: false,
                            drawBorder: false
                        },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            callback: function (value) {
                                const label = this.getLabelForValue(value);
                                return wrapLabel(label.toString(), 15);
                            },
                            padding: 10
                        }
                    }
                },
                plugins: {
                    tooltip: { enabled: false },
                    legend: legendOptions,
                    title: {
                        display: false,
                        font: fonts
                    },
                    datalabels: {
                        anchor: 'end',
                        align: ctx => ctx.dataset.data[ctx.dataIndex] < 10 ? 'end' : 'start',
                        offset: 10,
                        color: () => '#ffffff',
                        font: {
                            ...fonts,
                            weight: 'bold'
                        },
                        display: showDataLabels,
                        formatter: function (value) {
                            return showDataLabels ? `${value}${axisSuffix}` : null;
                        },
                        clamp: true,
                        clip: false
                    }
                }
            };
        }

        return common;
    }

    function buildDatasets(type, chartData, colors) {
        if (type === 'line') {
            return chartData.datasets.map((ds, i) => {
                const color = ds.borderColor || colors[i] || '#999';
                return {
                    label: ds.label,
                    data: ds.data,
                    borderColor: color,
                    backgroundColor: ds.backgroundColor || color,
                    fill: ds.fill ?? false,
                    tension: ds.tension ?? 0.2,
                    pointRadius: ds.pointRadius ?? 4,
                    pointHoverRadius: ds.pointHoverRadius ?? 5,
                    pointBackgroundColor: ds.pointBackgroundColor || color,
                    ...ds
                };
            });
        }

        if (type === 'bar') {
            const dataOptions = {
                borderWidth: 1,
                barThickness: 'flex',
                maxBarThickness: 70,
                minBarLength: 3
            };
            if (Array.isArray(chartData.datasets)) {
                return chartData.datasets.map((ds, i) => ({
                    label: ds.label,
                    data: ds.data,
                    backgroundColor: ds.backgroundColor || colors[i] || defaultColors[i],
                    ...dataOptions,
                    ...ds
                }));
            }

            return [{
                data: chartData.data,
                backgroundColor: colors.length ? colors : defaultColors,
                ...dataOptions
            }];
        }
    }

    function initCharts() {
        document.querySelectorAll('.js-chart').forEach(canvas => {
            if (charts[canvas.id]) {
                charts[canvas.id].destroy();
            }

            const gdsStyles = gdsVars(canvas);

            Chart.defaults.font.fontFamily = gdsStyles.fontFamily;
            Chart.defaults.font.size = gdsStyles.fontSize;
            Chart.defaults.color = gdsStyles.text;

            const chartData = JSON.parse(canvas.dataset.chart);
            const type = canvas.dataset.type;
            const showLegend = canvas.dataset.showLegend === "true";
            const showDataLabels = canvas.dataset.showDatalabels !== "false";
            const axisStep = canvas.dataset.axisStep
                ? parseInt(canvas.dataset.axisStep, 10)
                : 20;
            const axisMax = canvas.dataset.axisMax
                ? parseFloat(canvas.dataset.axisMax)
                : null;
            const axisSuffix = canvas.dataset.axisSuffix !== undefined
                ? canvas.dataset.axisSuffix
                : '%';

            const colors = canvas.dataset.colors
                ? JSON.parse(canvas.dataset.colors)
                : [];

            const config = {
                type,
                data: {
                    labels: chartData.labels,
                    datasets: buildDatasets(type, chartData, colors)
                },
                options: buildChartOptions(type, gdsStyles, axisStep, axisSuffix, axisMax, showLegend, showDataLabels),
                plugins: showDataLabels ? [ChartDataLabels] : []
            };

            const chart = new Chart(canvas, config);
            charts[canvas.id] = chart;

            if (showLegend) {
                const legendContainer = document.querySelector(
                    `.chart-legend[data-chart-id="${canvas.id}"]`
                );
                if (legendContainer) {
                    buildVerticalLegend(chart, legendContainer);
                }
            }
        });
    }

    function wrapLabel(label, maxChars) {
        const words = label.split(' ');
        const lines = [];
        let line = '';

        words.forEach(word => {
            if ((line + word).length > maxChars) {
                lines.push(line.trim());
                line = word + ' ';
            } else {
                line += word + ' ';
            }
        });

        lines.push(line.trim());
        return lines;
    }

    function buildVerticalLegend(chart, container) {
        const datasets = Array.isArray(chart.data.datasets)
            ? chart.data.datasets
            : [chart.data.datasets];

        const ul = document.createElement('ul');

        datasets.forEach(ds => {
            const li = document.createElement('li');
            const box = document.createElement('span');
            box.classList.add('legend-box');
            box.style.backgroundColor = ds.backgroundColor || ds.borderColor || '#000';

            const label = document.createElement('span');
            label.textContent = ds.label;

            li.appendChild(box);
            li.appendChild(label);
            ul.appendChild(li);
        });

        container.appendChild(ul);
    }

    function adjustChartResize() {
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                Object.values(charts).forEach(chart => {
                    const fontSizePx = gdsVars(chart.canvas).fontSize;

                    if (chart.options.scales.x.ticks.font) {
                        chart.options.scales.x.ticks.font.size = fontSizePx;
                    }

                    if (chart.options.scales.y.ticks.font) {
                        chart.options.scales.y.ticks.font.size = fontSizePx;
                    }

                    if (chart.options.plugins.title.font) {
                        chart.options.plugins.title.font.size = fontSizePx;
                    }

                    if (chart.options.plugins.datalabels.font) {
                        chart.options.plugins.datalabels.font.size = fontSizePx;
                    }

                    chart.update();
                });
            }, 100);
        });
    }

    document.addEventListener('DOMContentLoaded', initCharts);
    adjustChartResize();
})();
