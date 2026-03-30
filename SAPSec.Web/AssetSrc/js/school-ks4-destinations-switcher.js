(function () {
    function updateTableRow(cells, values) {
        cells.forEach(function (cell, index) {
            if (cell) {
                cell.textContent = values && values[index] ? values[index] : "No available data";
            }
        });
    }

    function buildSimilarSchoolComparisonHref(currentPath, similarSchoolUrn) {
        if (!currentPath || !similarSchoolUrn) {
            return "";
        }

        var normalizedPath = currentPath.replace(/\/+$/, "");
        var schoolMatch = normalizedPath.match(/^\/school\/([^/]+)/i);
        if (!schoolMatch || !schoolMatch[1]) {
            return "";
        }

        return "/school/" + encodeURIComponent(schoolMatch[1]) + "/view-similar-schools/" + encodeURIComponent(similarSchoolUrn);
    }

    function updateTopPerformers(tableBody, rows) {
        if (!tableBody) {
            return;
        }

        var items = Array.isArray(rows) ? rows : [];
        var currentPath = window.location && typeof window.location.pathname === "string"
            ? window.location.pathname
            : "";
        tableBody.innerHTML = "";

        items.forEach(function (row) {
            var tr = document.createElement("tr");
            tr.className = "govuk-table__row";

            var rank = document.createElement("td");
            rank.className = "govuk-table__cell";
            rank.textContent = row.rank;

            var name = document.createElement("th");
            name.scope = "row";
            name.className = "govuk-table__header";

            var link = document.createElement("a");
            link.className = "govuk-link";
            link.href = buildSimilarSchoolComparisonHref(currentPath, row.urn);
            link.textContent = row.name;
            name.appendChild(link);

            var value = document.createElement("td");
            value.className = "govuk-table__cell govuk-table__cell--numeric";
            value.textContent = row.displayValue;

            tr.appendChild(rank);
            tr.appendChild(name);
            tr.appendChild(value);
            tableBody.appendChild(tr);
        });
    }

    function applyDestinationViews(data, barChartCanvas, lineChartCanvas, tableCellMap, topPerformersBody) {
        if (!data) {
            return;
        }

        var barChart = window.Chart && barChartCanvas ? window.Chart.getChart(barChartCanvas) : null;
        if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
            barChart.data.datasets[0].data = data.bar || [];
            barChart.update();
        }

        var lineChart = window.Chart && lineChartCanvas ? window.Chart.getChart(lineChartCanvas) : null;
        if (lineChart && lineChart.data && lineChart.data.datasets && lineChart.data.datasets.length >= 4) {
            lineChart.data.datasets[0].data = data.line && data.line.thisSchool ? data.line.thisSchool : [];
            lineChart.data.datasets[1].data = data.line && data.line.similarSchools ? data.line.similarSchools : [];
            lineChart.data.datasets[2].data = data.line && data.line.localAuthority ? data.line.localAuthority : [];
            lineChart.data.datasets[3].data = data.line && data.line.england ? data.line.england : [];
            lineChart.update();
        }

        updateTableRow(tableCellMap.thisSchool, data.table && data.table.thisSchool ? data.table.thisSchool : []);
        updateTableRow(tableCellMap.similarSchools, data.table && data.table.similarSchools ? data.table.similarSchools : []);
        updateTableRow(tableCellMap.localAuthority, data.table && data.table.localAuthority ? data.table.localAuthority : []);
        updateTableRow(tableCellMap.england, data.table && data.table.england ? data.table.england : []);
        updateTopPerformers(topPerformersBody, data.topPerformers);
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

        var barChartCanvas = document.getElementById("destinations-school-chart");
        var lineChartCanvas = document.getElementById("destinations-school-yearbyyear-chart");
        var topPerformersBody = document.querySelector("#destinations-top-performers-table tbody");

        var tableCellMap = {
            thisSchool: [
                document.querySelector("[data-destinations-cell='this-prev2']"),
                document.querySelector("[data-destinations-cell='this-prev']"),
                document.querySelector("[data-destinations-cell='this-current']"),
                document.querySelector("[data-destinations-cell='this-avg']")
            ],
            similarSchools: [
                document.querySelector("[data-destinations-cell='similar-prev2']"),
                document.querySelector("[data-destinations-cell='similar-prev']"),
                document.querySelector("[data-destinations-cell='similar-current']"),
                document.querySelector("[data-destinations-cell='similar-avg']")
            ],
            localAuthority: [
                document.querySelector("[data-destinations-cell='la-prev2']"),
                document.querySelector("[data-destinations-cell='la-prev']"),
                document.querySelector("[data-destinations-cell='la-current']"),
                document.querySelector("[data-destinations-cell='la-avg']")
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
                    applyDestinationViews(destinationData, barChartCanvas, lineChartCanvas, tableCellMap, topPerformersBody);
                })
                .catch(function (error) {
                    console.error("Failed to load school KS4 destinations data.", error);
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
