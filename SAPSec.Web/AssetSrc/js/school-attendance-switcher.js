(function () {
    function updateChartAxis(chart, axisSettings) {
        if (!chart || !chart.options || !chart.options.scales || !axisSettings) {
            return;
        }

        var axis = chart.options.scales.x;
        if (!axis || !axis.ticks) {
            return;
        }

        if (typeof axisSettings.max === "number") {
            axis.max = axisSettings.max;
        }

        if (typeof axisSettings.step === "number") {
            axis.ticks.stepSize = axisSettings.step;
        }
    }

    function readAxisSettings(select, absenceType) {
        var raw = select.getAttribute("data-axis-config");
        if (!raw || !absenceType) {
            return null;
        }

        try {
            var config = JSON.parse(raw);
            return config[absenceType] || null;
        } catch (error) {
            console.error("Failed to parse attendance axis config.", error);
            return null;
        }
    }

    function init() {
        var absenceTypeSelect = document.getElementById("attendanceAbsenceType");
        if (!absenceTypeSelect) {
            return;
        }

        var dataEndpoint = absenceTypeSelect.getAttribute("data-endpoint");
        var barChartId = absenceTypeSelect.getAttribute("data-bar-chart-id");
        var lineChartId = absenceTypeSelect.getAttribute("data-line-chart-id");
        var barChartCanvas = barChartId ? document.getElementById(barChartId) : null;
        var lineChartCanvas = lineChartId ? document.getElementById(lineChartId) : null;
        if (!dataEndpoint || !barChartCanvas) {
            return;
        }

        var tableCellMap = {
            school: [
                document.querySelector("[data-attendance-cell='school-prev2']"),
                document.querySelector("[data-attendance-cell='school-prev']"),
                document.querySelector("[data-attendance-cell='school-current']"),
                document.querySelector("[data-attendance-cell='school-avg']")
            ],
            localAuthority: [
                document.querySelector("[data-attendance-cell='la-prev2']"),
                document.querySelector("[data-attendance-cell='la-prev']"),
                document.querySelector("[data-attendance-cell='la-current']"),
                document.querySelector("[data-attendance-cell='la-avg']")
            ],
            england: [
                document.querySelector("[data-attendance-cell='england-prev2']"),
                document.querySelector("[data-attendance-cell='england-prev']"),
                document.querySelector("[data-attendance-cell='england-current']"),
                document.querySelector("[data-attendance-cell='england-avg']")
            ]
        };

        function updateTableRow(cells, values) {
            cells.forEach(function (cell, index) {
                if (cell) {
                    cell.textContent = values && values[index] ? values[index] : "No available data";
                }
            });
        }

        function loadAttendance(absenceType) {
            return fetch(dataEndpoint + "?absenceType=" + encodeURIComponent(absenceType), {
                headers: {
                    Accept: "application/json"
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error("Request failed with status " + response.status);
                    }

                    return response.json();
                })
                .then(function (data) {
                    var barChart = window.Chart ? window.Chart.getChart(barChartCanvas) : null;
                    if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
                        barChart.data.datasets[0].data = data.bar || [];
                        updateChartAxis(barChart, readAxisSettings(absenceTypeSelect, data.absenceType || absenceType));
                        barChart.update();
                    }

                    var lineChart = window.Chart && lineChartCanvas ? window.Chart.getChart(lineChartCanvas) : null;
                    if (lineChart && lineChart.data && lineChart.data.datasets && lineChart.data.datasets.length >= 3) {
                        if (Array.isArray(data.years)) {
                            lineChart.data.labels = data.years;
                        }
                        lineChart.data.datasets[0].data = data.line && data.line.school ? data.line.school : [];
                        lineChart.data.datasets[1].data = data.line && data.line.localAuthority ? data.line.localAuthority : [];
                        lineChart.data.datasets[2].data = data.line && data.line.england ? data.line.england : [];
                        updateChartAxis(lineChart, readAxisSettings(absenceTypeSelect, data.absenceType || absenceType));
                        lineChart.update();
                    }

                    updateTableRow(tableCellMap.school, data.table && data.table.school ? data.table.school : []);
                    updateTableRow(tableCellMap.localAuthority, data.table && data.table.localAuthority ? data.table.localAuthority : []);
                    updateTableRow(tableCellMap.england, data.table && data.table.england ? data.table.england : []);
                })
                .catch(function (error) {
                    console.error("Failed to load school attendance data.", error);
                });
        }

        function refreshSelection() {
            loadAttendance(absenceTypeSelect.value);
        }

        absenceTypeSelect.addEventListener("change", refreshSelection);
        absenceTypeSelect.addEventListener("input", refreshSelection);
        refreshSelection();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
