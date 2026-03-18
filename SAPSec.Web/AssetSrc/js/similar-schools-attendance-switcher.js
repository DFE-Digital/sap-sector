(function () {
    function updateTableRow(cells, values) {
        cells.forEach(function (cell, index) {
            if (cell) {
                cell.textContent = values && values[index] ? values[index] : "No available data";
            }
        });
    }

    function updateChartAxis(chart, axisSettings) {
        if (!chart || !chart.options || !chart.options.scales || !axisSettings) {
            return;
        }

        var axisKey = chart.config && chart.config.type === "bar" ? "x" : "y";
        var axis = chart.options.scales[axisKey];
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

    function applyAttendanceData(data, barChartCanvas, lineChartCanvas, tableCellMap, axisSettings) {
        if (!data) {
            return;
        }

        var barChart = window.Chart && barChartCanvas ? window.Chart.getChart(barChartCanvas) : null;
        if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
            barChart.data.datasets[0].data = data.bar || [];
            updateChartAxis(barChart, axisSettings);
            barChart.update();
        }

        var lineChart = window.Chart && lineChartCanvas ? window.Chart.getChart(lineChartCanvas) : null;
        if (lineChart && lineChart.data) {
            if (Array.isArray(data.years)) {
                lineChart.data.labels = data.years;
            }

            if (lineChart.data.datasets && lineChart.data.datasets.length >= 3) {
                lineChart.data.datasets[0].data = data.line && data.line.thisSchool ? data.line.thisSchool : [];
                lineChart.data.datasets[1].data = data.line && data.line.similarSchool ? data.line.similarSchool : [];
                lineChart.data.datasets[2].data = data.line && data.line.england ? data.line.england : [];
                updateChartAxis(lineChart, axisSettings);
                lineChart.update();
            }
        }

        updateTableRow(tableCellMap.thisSchool, data.table && data.table.thisSchool ? data.table.thisSchool : []);
        updateTableRow(tableCellMap.similarSchool, data.table && data.table.similarSchool ? data.table.similarSchool : []);
        updateTableRow(tableCellMap.england, data.table && data.table.england ? data.table.england : []);
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
        if (!dataEndpoint) {
            return;
        }

        var barChartCanvas = document.getElementById("attendance-comparison-three-year-chart");
        var lineChartCanvas = document.getElementById("attendance-comparison-year-by-year-chart");
        var tableCellMap = {
            thisSchool: [
                document.querySelector("[data-attendance-cell='this-prev2']"),
                document.querySelector("[data-attendance-cell='this-prev']"),
                document.querySelector("[data-attendance-cell='this-current']"),
                document.querySelector("[data-attendance-cell='this-avg']")
            ],
            similarSchool: [
                document.querySelector("[data-attendance-cell='similar-prev2']"),
                document.querySelector("[data-attendance-cell='similar-prev']"),
                document.querySelector("[data-attendance-cell='similar-current']"),
                document.querySelector("[data-attendance-cell='similar-avg']")
            ],
            england: [
                document.querySelector("[data-attendance-cell='england-prev2']"),
                document.querySelector("[data-attendance-cell='england-prev']"),
                document.querySelector("[data-attendance-cell='england-current']"),
                document.querySelector("[data-attendance-cell='england-avg']")
            ]
        };

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
                    applyAttendanceData(
                        data,
                        barChartCanvas,
                        lineChartCanvas,
                        tableCellMap,
                        readAxisSettings(absenceTypeSelect, data.absenceType || absenceType));
                })
                .catch(function (error) {
                    console.error("Failed to load attendance comparison data.", error);
                });
        }

        absenceTypeSelect.addEventListener("change", function () {
            loadAttendance(absenceTypeSelect.value);
        });

        loadAttendance(absenceTypeSelect.value);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
