(function () {
    function updateTableRow(cells, values) {
        cells.forEach(function (cell, index) {
            if (cell) {
                cell.textContent = values && values[index] ? values[index] : "No available data";
            }
        });
    }

    function buildTopPerformerHref(baseUrl, urn) {
        if (!baseUrl || !urn) {
            return "";
        }

        return baseUrl.replace(/\/+$/, "") + "/" + encodeURIComponent(urn);
    }

    function updateTopPerformers(tableBody, rows, baseUrl) {
        if (!tableBody) {
            return;
        }

        var items = Array.isArray(rows) ? rows : [];
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
            link.href = buildTopPerformerHref(baseUrl, row.urn);
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
    function readJsonAttribute(element, attributeName) {
        var raw = element.getAttribute(attributeName);
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch (error) {
            console.error("Failed to parse data view switcher config.", error);
            return null;
        }
    }

    function buildTableCellMap(seriesKeys, cellAttribute, cellPrefixes) {
        var suffixes = ["prev2", "prev", "current", "avg"];
        var map = {};

        seriesKeys.forEach(function (seriesKey) {
            var cellPrefix = cellPrefixes[seriesKey] || seriesKey;
            map[seriesKey] = suffixes.map(function (suffix) {
                return document.querySelector("[" + cellAttribute + "='" + cellPrefix + "-" + suffix + "']");
            });
        });

        return map;
    }

    function hasAnyTableCells(tableCellMap, seriesKeys) {
        return seriesKeys.some(function (seriesKey) {
            return (tableCellMap[seriesKey] || []).some(function (cell) {
                return !!cell;
            });
        });
    }

    function buildRequestUrl(endpoint, queryKey, selectedValue) {
        var separator = endpoint.indexOf("?") === -1 ? "?" : "&";
        return endpoint + separator + queryKey + "=" + encodeURIComponent(selectedValue);
    }

    function applyDataView(data, config) {
        if (!data) {
            return;
        }

        var barChart = window.Chart && config.barChartCanvas ? window.Chart.getChart(config.barChartCanvas) : null;
        if (barChart && barChart.data && barChart.data.datasets && barChart.data.datasets[0]) {
            barChart.data.datasets[0].data = data.bar || [];
            barChart.update();
        }

        var lineChart = window.Chart && config.lineChartCanvas ? window.Chart.getChart(config.lineChartCanvas) : null;
        if (lineChart && lineChart.data) {
            if (Array.isArray(data.years)) {
                lineChart.data.labels = data.years;
            }

            if (lineChart.data.datasets && lineChart.data.datasets.length >= config.seriesKeys.length) {
                config.seriesKeys.forEach(function (seriesKey, index) {
                    lineChart.data.datasets[index].data = data.line && data.line[seriesKey] ? data.line[seriesKey] : [];
                });
                lineChart.update();
            }
        }

        config.seriesKeys.forEach(function (seriesKey) {
            updateTableRow(
                config.tableCellMap[seriesKey] || [],
                data.table && data.table[seriesKey] ? data.table[seriesKey] : []);
        });

        updateTopPerformers(
            config.topPerformersTableBody,
            data.topPerformers,
            config.topPerformerBaseUrl);
    }

    function init() {
        document.querySelectorAll("[data-view-switcher='true']").forEach(function (select) {
            var dataEndpoint = select.getAttribute("data-endpoint");
            var barChartId = select.getAttribute("data-bar-chart-id");
            var lineChartId = select.getAttribute("data-line-chart-id");
            var queryKey = select.getAttribute("data-query-key");
            var cellAttribute = select.getAttribute("data-cell-attribute") || "data-view-cell";
            var seriesKeys = readJsonAttribute(select, "data-series-keys") || [];
            var cellPrefixes = readJsonAttribute(select, "data-cell-prefixes") || {};
            var topPerformersTableId = select.getAttribute("data-top-performers-table-id");

            var config = {
                barChartCanvas: barChartId ? document.getElementById(barChartId) : null,
                lineChartCanvas: lineChartId ? document.getElementById(lineChartId) : null,
                seriesKeys: seriesKeys,
                tableCellMap: buildTableCellMap(seriesKeys, cellAttribute, cellPrefixes),
                topPerformersTableBody: topPerformersTableId
                    ? document.querySelector("#" + topPerformersTableId + " tbody")
                    : null,
                topPerformerBaseUrl: select.getAttribute("data-top-performer-base-url") || ""
            };
            var activeRequestId = 0;

            if (!dataEndpoint || !queryKey || seriesKeys.length === 0) {
                return;
            }

            if (!config.barChartCanvas && !config.lineChartCanvas && !hasAnyTableCells(config.tableCellMap, seriesKeys)) {
                return;
            }

            function loadSelectedValue(selectedValue) {
                activeRequestId += 1;
                var requestId = activeRequestId;

                return fetch(buildRequestUrl(dataEndpoint, queryKey, selectedValue), {
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
                        if (requestId !== activeRequestId) {
                            return;
                        }

                        applyDataView(
                            data,
                            config);
                    })
                    .catch(function (error) {
                        console.error("Failed to load view data.", error);
                    });
            }

            function refreshSelection() {
                loadSelectedValue(select.value);
            }

            select.addEventListener("change", refreshSelection);
            select.addEventListener("input", refreshSelection);
            window.requestAnimationFrame(refreshSelection);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
