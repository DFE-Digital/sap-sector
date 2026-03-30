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

    function applyEngMathsViews(gradeData, barChartCanvas, lineChartCanvas, tableCellMap, topPerformersBody) {
        if (!gradeData) {
            return;
        }

        var barChart = window.Chart && barChartCanvas ? window.Chart.getChart(barChartCanvas) : null;
        if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
            barChart.data.datasets[0].data = gradeData.bar || [];
            barChart.update();
        }

        var lineChart = window.Chart && lineChartCanvas ? window.Chart.getChart(lineChartCanvas) : null;
        if (lineChart && lineChart.data && lineChart.data.datasets && lineChart.data.datasets.length >= 4) {
            lineChart.data.datasets[0].data = gradeData.line && gradeData.line.thisSchool ? gradeData.line.thisSchool : [];
            lineChart.data.datasets[1].data = gradeData.line && gradeData.line.similarSchools ? gradeData.line.similarSchools : [];
            lineChart.data.datasets[2].data = gradeData.line && gradeData.line.localAuthority ? gradeData.line.localAuthority : [];
            lineChart.data.datasets[3].data = gradeData.line && gradeData.line.england ? gradeData.line.england : [];
            lineChart.update();
        }

        updateTableRow(tableCellMap.thisSchool, gradeData.table && gradeData.table.thisSchool ? gradeData.table.thisSchool : []);
        updateTableRow(tableCellMap.similarSchools, gradeData.table && gradeData.table.similarSchools ? gradeData.table.similarSchools : []);
        updateTableRow(tableCellMap.localAuthority, gradeData.table && gradeData.table.localAuthority ? gradeData.table.localAuthority : []);
        updateTableRow(tableCellMap.england, gradeData.table && gradeData.table.england ? gradeData.table.england : []);
        updateTopPerformers(topPerformersBody, gradeData.topPerformers);
    }

    function init() {
        var gradeSelect = document.getElementById("engMathsGrade");
        if (!gradeSelect) {
            return;
        }

        var dataEndpoint = gradeSelect.getAttribute("data-endpoint");
        if (!dataEndpoint) {
            return;
        }

        var barChartCanvas = document.getElementById("eng-maths-school-chart");
        var lineChartCanvas = document.getElementById("eng-maths-school-yearbyyear-chart");
        var topPerformersBody = document.querySelector("#eng-maths-top-performers-table tbody");

        var tableCellMap = {
            thisSchool: [
                document.querySelector("[data-eng-maths-cell='this-prev2']"),
                document.querySelector("[data-eng-maths-cell='this-prev']"),
                document.querySelector("[data-eng-maths-cell='this-current']"),
                document.querySelector("[data-eng-maths-cell='this-avg']")
            ],
            similarSchools: [
                document.querySelector("[data-eng-maths-cell='similar-prev2']"),
                document.querySelector("[data-eng-maths-cell='similar-prev']"),
                document.querySelector("[data-eng-maths-cell='similar-current']"),
                document.querySelector("[data-eng-maths-cell='similar-avg']")
            ],
            localAuthority: [
                document.querySelector("[data-eng-maths-cell='la-prev2']"),
                document.querySelector("[data-eng-maths-cell='la-prev']"),
                document.querySelector("[data-eng-maths-cell='la-current']"),
                document.querySelector("[data-eng-maths-cell='la-avg']")
            ],
            england: [
                document.querySelector("[data-eng-maths-cell='england-prev2']"),
                document.querySelector("[data-eng-maths-cell='england-prev']"),
                document.querySelector("[data-eng-maths-cell='england-current']"),
                document.querySelector("[data-eng-maths-cell='england-avg']")
            ]
        };

        function loadEngMathsViews(grade) {
            return fetch(dataEndpoint + "?grade=" + encodeURIComponent(grade), {
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
                .then(function (gradeData) {
                    applyEngMathsViews(gradeData, barChartCanvas, lineChartCanvas, tableCellMap, topPerformersBody);
                })
                .catch(function (error) {
                    console.error("Failed to load school KS4 grade data.", error);
                });
        }

        gradeSelect.addEventListener("change", function () {
            loadEngMathsViews(gradeSelect.value);
        });

        loadEngMathsViews(gradeSelect.value);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
