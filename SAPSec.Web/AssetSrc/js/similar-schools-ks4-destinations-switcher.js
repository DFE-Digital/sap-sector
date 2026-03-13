(function () {
    function updateTableRow(cells, values) {
        cells.forEach(function (cell, index) {
            if (cell) {
                cell.textContent = values && values[index] ? values[index] : "No available data";
            }
        });
    }

    function applyDestinationViews(data, barChartCanvas, lineChartCanvas, tableCellMap) {
        if (!data) {
            return;
        }

        var barChart = window.Chart && barChartCanvas ? window.Chart.getChart(barChartCanvas) : null;
        if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
            barChart.data.datasets[0].data = data.bar || [];
            barChart.update();
        }

        var lineChart = window.Chart && lineChartCanvas ? window.Chart.getChart(lineChartCanvas) : null;
        if (lineChart && lineChart.data && lineChart.data.datasets && lineChart.data.datasets.length >= 3) {
            lineChart.data.datasets[0].data = data.line && data.line.thisSchool ? data.line.thisSchool : [];
            lineChart.data.datasets[1].data = data.line && data.line.similarSchool ? data.line.similarSchool : [];
            lineChart.data.datasets[2].data = data.line && data.line.england ? data.line.england : [];
            lineChart.update();
        }

        updateTableRow(tableCellMap.thisSchool, data.table && data.table.thisSchool ? data.table.thisSchool : []);
        updateTableRow(tableCellMap.similarSchool, data.table && data.table.similarSchool ? data.table.similarSchool : []);
        updateTableRow(tableCellMap.england, data.table && data.table.england ? data.table.england : []);
    }

    function init() {
        var destinationSelect = document.getElementById("destinationsType");
        if (!destinationSelect) {
            return;
        }

        var dataEndpoint = destinationSelect.getAttribute("data-endpoint");
        if (!dataEndpoint) {
            return;
        }

        var barChartCanvas = document.getElementById("destinations-comparison-chart");
        var lineChartCanvas = document.getElementById("destinations-comparison-yearbyyear-chart");

        var tableCellMap = {
            thisSchool: [
                document.querySelector("[data-destinations-cell='this-prev2']"),
                document.querySelector("[data-destinations-cell='this-prev']"),
                document.querySelector("[data-destinations-cell='this-current']"),
                document.querySelector("[data-destinations-cell='this-avg']")
            ],
            similarSchool: [
                document.querySelector("[data-destinations-cell='similar-prev2']"),
                document.querySelector("[data-destinations-cell='similar-prev']"),
                document.querySelector("[data-destinations-cell='similar-current']"),
                document.querySelector("[data-destinations-cell='similar-avg']")
            ],
            england: [
                document.querySelector("[data-destinations-cell='england-prev2']"),
                document.querySelector("[data-destinations-cell='england-prev']"),
                document.querySelector("[data-destinations-cell='england-current']"),
                document.querySelector("[data-destinations-cell='england-avg']")
            ]
        };

        function loadDestinationViews(destination) {
            return fetch(dataEndpoint + "?destination=" + encodeURIComponent(destination), {
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
                .then(function (destinationData) {
                    applyDestinationViews(destinationData, barChartCanvas, lineChartCanvas, tableCellMap);
                })
                .catch(function (error) {
                    console.error("Failed to load KS4 destinations data.", error);
                });
        }

        destinationSelect.addEventListener("change", function () {
            loadDestinationViews(destinationSelect.value);
        });

        loadDestinationViews(destinationSelect.value);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
