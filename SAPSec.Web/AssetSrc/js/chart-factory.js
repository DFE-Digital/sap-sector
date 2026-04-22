(function () {
    const datasetColorKeys = ['school', 'similarSchools', 'localAuthority', 'england'];
    const CHART_CONFIG = {
        defaults: {
            axisSuffix: '%',
            maxDevicePixelRatio: 2,
            resizeDebounceMs: 100,
            labelWrapChars: 15
        },
        legend: {
            position: 'bottom',
            pointStyle: 'circle',
            box: {
                width: 10,
                height: 10
            },
            padding: 16
        },
        line: {
            width: {
                major: 2,
                minor: 1
            },
            axis: {
                grace: '5%'
            },
            series: {
                tension: 0.2,
                pointRadius: 4,
                pointHoverRadius: 5
            },
            datalabels: {
                anchor: 'end',
                align: 'right',
                offset: 10,
                endOnly: true
            },
            layout: {
                topPadding: 24,
                rightPaddingWithDatalabels: 100
            }
        },
        bar: {
            width: {
                major: 2,
                minor: 1
            },
            labels: {
                yTickPadding: 10,
                noDataOffset: 12,
                baseContainerHeight: 260,
                rowHeight: 70,
                lineHeight: 18
            },
            dataset: {
                borderWidth: 1,
                barThickness: 'flex',
                maxBarThickness: 70,
                minBarLength: 3,
                categoryPercentage: 0.8,
                barPercentage: 0.9
            },
            datalabels: {
                anchor: 'end',
                smallValueAlign: 'end',
                defaultAlign: 'start',
                offset: 10,
                fontWeight: 'bold'
            },
            noData: {
                text: 'No available data'
            }
        },
        fallbacks: {
            legendBoxColor: '#6f777b'
        }
    };

    const charts = {};

    function gdsVars(canvas) {
        const s = getComputedStyle(canvas);

        const colorDefaults = {
            school: s.getPropertyValue('--chart-color-school').trim(),
            similarSchools: s.getPropertyValue('--chart-color-similar-schools').trim() || s.getPropertyValue('--chart-color-comparator').trim(),
            localAuthority: s.getPropertyValue('--chart-color-local-authority').trim(),
            england: s.getPropertyValue('--chart-color-england').trim(),
            fallback: s.getPropertyValue('--chart-color-fallback').trim()
        };

        return {
            fontFamily: s.getPropertyValue('--gds-font-family').trim(),
            fontSize: parseInt(s.getPropertyValue('--gds-font-size')),
            text: s.getPropertyValue('--gds-text'),
            grey: s.getPropertyValue('--gds-grey'),
            gridMinor: s.getPropertyValue('--gds-grid-minor').trim(),
            gridMajor: s.getPropertyValue('--gds-grid-major').trim(),
            gridX: s.getPropertyValue('--gds-grid-x').trim(),
            onBarLabel: s.getPropertyValue('--gds-on-bar-label').trim(),
            labelBg: s.getPropertyValue('--gds-label-bg'),
            labelBorder: s.getPropertyValue('--gds-label-border'),
            labelPadding: parseInt(s.getPropertyValue('--gds-label-padding')),
            colorDefaults
        };
    }

    function resolveColorConfig(rawColors, gdsStyles) {
        const defaults = gdsStyles.colorDefaults;

        if (Array.isArray(rawColors)) {
            const byKey = {
                school: rawColors[0] || defaults.school,
                similarSchools: rawColors[1] || defaults.similarSchools,
                localAuthority: rawColors[2] || defaults.localAuthority,
                england: rawColors[3] || rawColors[2] || defaults.england,
                fallback: defaults.fallback
            };

            return {
                byKey,
                palette: rawColors.length ? rawColors : [byKey.school, byKey.similarSchools, byKey.localAuthority, byKey.england]
            };
        }

        if (rawColors && typeof rawColors === 'object') {
            const byKey = {
                school: rawColors.school || defaults.school,
                similarSchools: rawColors.similarSchools || rawColors.comparator || defaults.similarSchools,
                localAuthority: rawColors.localAuthority || defaults.localAuthority,
                england: rawColors.england || defaults.england,
                fallback: defaults.fallback
            };

            return {
                byKey,
                palette: Object.values(byKey).filter((_, i) => i < 4)
            };
        }

        const byKey = {
            school: defaults.school,
            similarSchools: defaults.similarSchools,
            localAuthority: defaults.localAuthority,
            england: defaults.england,
            fallback: defaults.fallback
        };

        return {
            byKey,
            palette: [byKey.school, byKey.similarSchools, byKey.localAuthority, byKey.england]
        };
    }

    function getBarLabelText(value, axisSuffix) {
        if (value === null || value === undefined || Number.isNaN(value)) {
            return '';
        }

        return `${value}${axisSuffix}`;
    }

    function canBarFitLabel(ctx, axisSuffix) {
        const value = ctx.dataset.data[ctx.dataIndex];
        if (value === null || value === undefined || Number.isNaN(value)) {
            return false;
        }

        const xScale = ctx.chart?.scales?.x;
        const canvasContext = ctx.chart?.ctx;
        if (!xScale || !canvasContext) {
            return true;
        }

        const barLength = Math.abs(xScale.getPixelForValue(value) - xScale.getPixelForValue(0));
        const font = Chart.helpers.toFont(ctx.chart.options?.plugins?.datalabels?.font);

        canvasContext.save();
        canvasContext.font = font.string;
        const labelWidth = canvasContext.measureText(getBarLabelText(value, axisSuffix)).width;
        canvasContext.restore();

        return barLength >= labelWidth + (CHART_CONFIG.bar.datalabels.offset * 2);
    }

    function getBarLabelAlignment(ctx, axisSuffix, barLabelAlign) {
        if (barLabelAlign) {
            return barLabelAlign;
        }

        return canBarFitLabel(ctx, axisSuffix)
            ? CHART_CONFIG.bar.datalabels.defaultAlign
            : CHART_CONFIG.bar.datalabels.smallValueAlign;
    }

    function getBarLabelColor(ctx, gdsStyles, axisSuffix, barLabelAlign) {
        const align = getBarLabelAlignment(ctx, axisSuffix, barLabelAlign);
        return align === CHART_CONFIG.bar.datalabels.defaultAlign
            ? gdsStyles.onBarLabel
            : gdsStyles.text;
    }

    function buildExplicitTicks(axisMin, axisMax, stepSize) {
        if (axisMin === null || axisMax === null || !stepSize) {
            return undefined;
        }

        return function (axis) {
            const ticks = [];
            for (let value = axisMin; value <= axisMax; value += stepSize) {
                ticks.push({ value });
            }
            axis.ticks = ticks;
        };
    }

    function buildChartOptions(type, gdsStyles, axisStep, axisSuffix, axisMin, axisMax, axisAutoSkip, showLegend, showDataLabels, showXGrid, barLabelAlign) {
        const common = {
            responsive: true,
            maintainAspectRatio: false,
            devicePixelRatio: Math.min(window.devicePixelRatio || 1, CHART_CONFIG.defaults.maxDevicePixelRatio)
        };

        const fonts = {
            family: gdsStyles.fontFamily,
            size: gdsStyles.fontSize
        };

        const stepSize = axisStep;
        const axisTickCount = axisMin !== null && axisMax !== null && stepSize
            ? Math.floor((axisMax - axisMin) / stepSize) + 1
            : undefined;
        const explicitTicks = buildExplicitTicks(axisMin, axisMax, stepSize);

        const legendOptions = {
            display: showLegend,
            position: CHART_CONFIG.legend.position,
            labels: {
                usePointStyle: true,
                pointStyle: CHART_CONFIG.legend.pointStyle,
                boxWidth: CHART_CONFIG.legend.box.width,
                boxHeight: CHART_CONFIG.legend.box.height,
                padding: CHART_CONFIG.legend.padding
            }
        };

        if (type === 'line') {
            return {
                ...common,
                layout: {
                    padding: {
                        top: CHART_CONFIG.line.layout.topPadding,
                        right: showDataLabels ? CHART_CONFIG.line.layout.rightPaddingWithDatalabels : 0
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        min: axisMin ?? undefined,
                        max: axisMax ?? undefined,
                        grace: CHART_CONFIG.line.axis.grace,
                        afterBuildTicks: explicitTicks,
                        grid: {
                            display: true,
                            drawBorder: false,
                            color: (context) => {
                                return context.tick.value === 0 ? gdsStyles.gridMajor : gdsStyles.gridMinor;
                            },
                            lineWidth: (context) => {
                                return context.tick.value === 0 ? CHART_CONFIG.line.width.major : CHART_CONFIG.line.width.minor;
                            }
                        },
                        border: { display: false },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            autoSkip: axisAutoSkip,
                            stepSize: stepSize,
                            count: axisTickCount,
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
                        grid: {
                            display: showXGrid,
                            drawBorder: false,
                            color: gdsStyles.gridX
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
                        align: CHART_CONFIG.line.datalabels.align,
                        offset: CHART_CONFIG.line.datalabels.offset,
                        color: gdsStyles.text,
                        font: fonts,
                        display: showDataLabels
                            ? function (ctx) {
                                return CHART_CONFIG.line.datalabels.endOnly
                                    ? ctx.dataIndex === ctx.dataset.data.length - 1
                                    : true;
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
                        min: axisMin ?? undefined,
                        max: axisMax ?? undefined,
                        afterBuildTicks: explicitTicks,
                        grid: {
                            display: true,
                            drawBorder: false,
                            color: (context) => {
                                return context.tick.value === 0 ? gdsStyles.gridMajor : gdsStyles.gridMinor;
                            },
                            lineWidth: (context) => {
                                return context.tick.value === 0 ? CHART_CONFIG.bar.width.major : CHART_CONFIG.bar.width.minor;
                            }
                        },
                        border: { display: false },
                        ticks: {
                            color: gdsStyles.text,
                            font: fonts,
                            autoSkip: axisAutoSkip,
                            stepSize: stepSize,
                            count: axisTickCount,
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
                                return wrapLabel(label.toString(), CHART_CONFIG.defaults.labelWrapChars);
                            },
                            padding: CHART_CONFIG.bar.labels.yTickPadding
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
                        anchor: CHART_CONFIG.bar.datalabels.anchor,
                        align: function (ctx) {
                            return getBarLabelAlignment(ctx, axisSuffix, barLabelAlign);
                        },
                        offset: CHART_CONFIG.bar.datalabels.offset,
                        color: function (ctx) {
                            return getBarLabelColor(ctx, gdsStyles, axisSuffix, barLabelAlign);
                        },
                        font: {
                            ...fonts,
                            weight: CHART_CONFIG.bar.datalabels.fontWeight
                        },
                        display: showDataLabels,
                        formatter: function (value) {
                            if (!showDataLabels || value === null || value === undefined || Number.isNaN(value)) {
                                return null;
                            }
                            return `${value}${axisSuffix}`;
                        },
                        clamp: true,
                        clip: false
                    }
                }
            };
        }

        return common;
    }

    const noDataBarLabelsPlugin = {
        id: 'noDataBarLabels',
        afterDraw(chart, args, pluginOptions) {
            if (chart.config.type !== 'bar' || !pluginOptions || !pluginOptions.enabled) {
                return;
            }

            const { ctx, scales, chartArea } = chart;
            const yScale = scales.y;
            const xScale = scales.x;

            if (!yScale || !xScale || !chartArea) {
                return;
            }

            const labels = chart.data.labels || [];
            const dataset = Array.isArray(chart.data.datasets) ? chart.data.datasets[0] : null;
            const values = dataset && Array.isArray(dataset.data) ? dataset.data : [];

            ctx.save();
            ctx.fillStyle = pluginOptions.color;
            ctx.font = pluginOptions.font;
            ctx.textAlign = 'left';
            ctx.textBaseline = 'middle';

            values.forEach((value, index) => {
                if (value !== null && value !== undefined && !Number.isNaN(value)) {
                    return;
                }

                if (labels[index] === undefined) {
                    return;
                }

                const y = yScale.getPixelForValue(index);
                const x = Math.max(chartArea.left + pluginOptions.offset, xScale.left + pluginOptions.offset);
                ctx.fillText(pluginOptions.text, x, y);
            });

            ctx.restore();
        }
    };

    function buildDatasets(type, chartData, colorConfig, barOptions) {
        if (type === 'line') {
            return chartData.datasets.map((ds, i) => {
                const keyedColor = colorConfig.byKey[datasetColorKeys[i]];
                const color = ds.borderColor || keyedColor || colorConfig.palette[i] || colorConfig.byKey.fallback;
                return {
                    label: ds.label,
                    data: ds.data,
                    borderColor: color,
                    backgroundColor: ds.backgroundColor || color,
                    fill: ds.fill ?? false,
                    tension: ds.tension ?? CHART_CONFIG.line.series.tension,
                    pointRadius: ds.pointRadius ?? CHART_CONFIG.line.series.pointRadius,
                    pointHoverRadius: ds.pointHoverRadius ?? CHART_CONFIG.line.series.pointHoverRadius,
                    pointBackgroundColor: ds.pointBackgroundColor || color,
                    ...ds
                };
            });
        }

        if (type === 'bar') {
            const dataOptions = {
                ...CHART_CONFIG.bar.dataset
            };
            if (barOptions) {
                if (barOptions.barThickness !== null) {
                    dataOptions.barThickness = barOptions.barThickness;
                    dataOptions.maxBarThickness = barOptions.barThickness;
                }
                if (barOptions.categoryPercentage !== null) {
                    dataOptions.categoryPercentage = barOptions.categoryPercentage;
                }
                if (barOptions.barPercentage !== null) {
                    dataOptions.barPercentage = barOptions.barPercentage;
                }
            }
            if (Array.isArray(chartData.datasets)) {
                return chartData.datasets.map((ds, i) => ({
                    label: ds.label,
                    data: ds.data,
                    backgroundColor: ds.backgroundColor || colorConfig.byKey[datasetColorKeys[i]] || colorConfig.palette[i] || colorConfig.byKey.fallback,
                    ...dataOptions,
                    ...ds
                }));
            }

            return [{
                data: chartData.data,
                backgroundColor: colorConfig.palette,
                ...dataOptions
            }];
        }
    }

    function resizeBarChartContainer(canvas, chartData) {
        const container = canvas.parentElement;
        const labels = Array.isArray(chartData.labels) ? chartData.labels : [];
        if (!container || labels.length === 0) {
            return;
        }

        const maxWrappedLines = Math.max(...labels.map(label =>
            wrapLabel(label.toString(), CHART_CONFIG.defaults.labelWrapChars).length
        ));
        const rowHeight = CHART_CONFIG.bar.labels.rowHeight
            + Math.max(0, maxWrappedLines - 2) * CHART_CONFIG.bar.labels.lineHeight;
        const height = Math.max(
            CHART_CONFIG.bar.labels.baseContainerHeight,
            labels.length * rowHeight
        );

        container.style.height = `${height}px`;
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
            if (type === 'bar') {
                resizeBarChartContainer(canvas, chartData);
            }
            const showLegend = canvas.dataset.showLegend === "true";
            const showDataLabels = canvas.dataset.showDatalabels !== "false";
            const showXGrid = canvas.dataset.showXGrid === "true";
            const axisStep = canvas.dataset.axisStep
                ? parseInt(canvas.dataset.axisStep, 10)
                : CHART_CONFIG.defaults.axisStep;
            const axisMin = canvas.dataset.axisMin
                ? parseFloat(canvas.dataset.axisMin)
                : null;
            const axisMax = canvas.dataset.axisMax
                ? parseFloat(canvas.dataset.axisMax)
                : null;
            const axisAutoSkip = canvas.dataset.axisAutoSkip !== undefined
                ? canvas.dataset.axisAutoSkip !== "false"
                : undefined;
            const axisSuffix = canvas.dataset.axisSuffix !== undefined
                ? canvas.dataset.axisSuffix
                : CHART_CONFIG.defaults.axisSuffix;
            const labelDecimals = canvas.dataset.labelDecimals
                ? parseInt(canvas.dataset.labelDecimals, 10)
                : null;

            const rawColors = canvas.dataset.colors
                ? JSON.parse(canvas.dataset.colors)
                : null;
            const colorConfig = resolveColorConfig(rawColors, gdsStyles);

            const barThickness = canvas.dataset.barThickness
                ? parseInt(canvas.dataset.barThickness, 10)
                : null;
            const categoryPercentage = canvas.dataset.categoryPercentage
                ? parseFloat(canvas.dataset.categoryPercentage)
                : null;
            const barPercentage = canvas.dataset.barPercentage
                ? parseFloat(canvas.dataset.barPercentage)
                : null;

            const barLabelAlign = canvas.dataset.barLabelAlign || null;
            const showNoDataLabels = canvas.dataset.showNoDataLabels === 'true';

            const config = {
                type,
                data: {
                    labels: chartData.labels,
                    datasets: buildDatasets(type, chartData, colorConfig, {
                        barThickness,
                        categoryPercentage,
                        barPercentage
                    })
                },
                options: buildChartOptions(
                    type,
                    gdsStyles,
                    axisStep,
                    axisSuffix,
                    axisMin,
                    axisMax,
                    axisAutoSkip,
                    showLegend,
                    showDataLabels,
                    showXGrid,
                    barLabelAlign),
                plugins: [
                    ...(showDataLabels ? [ChartDataLabels] : []),
                    noDataBarLabelsPlugin
                ]
            };

            config.options.plugins.noDataBarLabels = {
                enabled: type === 'bar' && showNoDataLabels,
                text: CHART_CONFIG.bar.noData.text,
                offset: CHART_CONFIG.bar.labels.noDataOffset,
                color: gdsStyles.text,
                font: `${gdsStyles.fontSize}px ${gdsStyles.fontFamily}`
            };

            if (type === 'bar' && labelDecimals !== null && config.options?.plugins?.datalabels) {
                config.options.plugins.datalabels.formatter = function (value) {
                    if (!showDataLabels || value === null || value === undefined || Number.isNaN(value)) {
                        return null;
                    }
                    return `${Number(value).toFixed(labelDecimals)}${axisSuffix}`;
                };
            }

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
            box.style.backgroundColor = ds.backgroundColor || ds.borderColor || CHART_CONFIG.fallbacks.legendBoxColor;

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
            }, CHART_CONFIG.defaults.resizeDebounceMs);
        });
    }

    document.addEventListener('DOMContentLoaded', initCharts);
    adjustChartResize();
})();
