(function () {
    function setHidden(element, hidden) {
        if (!element) {
            return;
        }

        if (hidden) {
            element.setAttribute("hidden", "hidden");
        } else {
            element.removeAttribute("hidden");
        }
    }

    function resizeCharts(container) {
        if (!window.Chart || !container) {
            return;
        }

        container.querySelectorAll("canvas").forEach(function (canvas) {
            var chart = window.Chart.getChart(canvas);
            if (!chart) {
                return;
            }

            chart.resize();
            chart.update("none");
        });
    }

    function getTabTarget(tabLink) {
        if (!tabLink) {
            return "";
        }

        return tabLink.getAttribute("href") || "";
    }

    function isAverageTabTarget(target) {
        return /^#.+three-year-average$/i.test(target) || target === "#three-year-average";
    }

    function isYearByYearTabTarget(target) {
        return /^#.+year-by-year$/i.test(target) || target === "#year-by-year";
    }

    function buildToggleHeader() {
        var header = document.createElement("div");
        header.className = "app-content-toggle__header";

        var title = document.createElement("h3");
        title.className = "govuk-heading-m app-content-toggle__title";
        title.textContent = "3-year average";

        var button = document.createElement("button");
        button.type = "button";
        button.className = "govuk-button govuk-button--secondary";
        button.textContent = "Show year by year";
        button.setAttribute("aria-pressed", "false");
        button.setAttribute("data-module", "govuk-button");

        header.appendChild(title);
        header.appendChild(button);

        return { header: header, title: title, button: button };
    }

    function moveChartBlock(chartContainer, targetPanel) {
        if (!chartContainer || !targetPanel) {
            return;
        }

        var chartCanvas = chartContainer.querySelector("canvas");
        var expectedChartId = chartCanvas ? chartCanvas.id : "";
        var previousSibling = chartContainer.previousElementSibling;
        var legend = previousSibling
            && previousSibling.classList.contains("chart-legend")
            && previousSibling.getAttribute("data-chart-id") === expectedChartId
            ? previousSibling
            : null;

        if (legend) {
            targetPanel.appendChild(legend);
        }

        targetPanel.appendChild(chartContainer);
    }

    function initialiseTabSet(tabSet) {
        var listItems = tabSet.querySelectorAll(".govuk-tabs__list-item");
        if (listItems.length < 2) {
            return;
        }

        var firstTab = listItems[0].querySelector(".govuk-tabs__tab");
        var secondTab = listItems[1].querySelector(".govuk-tabs__tab");
        var firstPanel = tabSet.querySelector(".govuk-tabs__panel");
        var secondPanel = listItems[1] && secondTab
            ? tabSet.querySelector(secondTab.getAttribute("href"))
            : null;

        if (!firstTab || !secondTab || !firstPanel || !secondPanel) {
            return;
        }

        var firstTabTarget = getTabTarget(firstTab);
        var secondTabTarget = getTabTarget(secondTab);

        if (!isAverageTabTarget(firstTabTarget) || !isYearByYearTabTarget(secondTabTarget)) {
            return;
        }

        var averageChart = firstPanel.querySelector(".app-ks4-chart-container");
        var yearlyChart = secondPanel.querySelector(".app-ks4-chart-container");

        if (!averageChart || !yearlyChart || firstPanel.querySelector(".app-content-toggle")) {
            return;
        }

        var chartsPanelId = firstTabTarget.slice(1).replace(/three-year-average$/i, "charts");

        firstTab.textContent = "Charts";
        firstTab.setAttribute("href", "#" + chartsPanelId);
        firstPanel.id = chartsPanelId;
        listItems[1].remove();

        var toggleContainer = document.createElement("div");
        toggleContainer.className = "app-content-toggle";
        toggleContainer.setAttribute("data-module", "app-content-toggle");

        var toggle = buildToggleHeader();
        toggleContainer.appendChild(toggle.header);

        var averagePanel = document.createElement("div");
        averagePanel.className = "app-content-toggle__panel app-content-toggle__panel--active";
        averagePanel.setAttribute("data-content-toggle-panel", "true");
        averagePanel.setAttribute("data-content-toggle-name", "3-year average");
        averagePanel.id = firstTabTarget.slice(1);
        moveChartBlock(averageChart, averagePanel);

        var yearlyPanel = document.createElement("div");
        yearlyPanel.className = "app-content-toggle__panel";
        yearlyPanel.setAttribute("data-content-toggle-panel", "true");
        yearlyPanel.setAttribute("data-content-toggle-name", "Year by year");
        yearlyPanel.id = secondTabTarget.slice(1);
        yearlyPanel.setAttribute("hidden", "hidden");
        moveChartBlock(yearlyChart, yearlyPanel);

        toggleContainer.appendChild(averagePanel);
        toggleContainer.appendChild(yearlyPanel);

        firstPanel.insertBefore(toggleContainer, firstPanel.firstChild);

        var showingYearly = false;

        toggle.button.addEventListener("click", function () {
            showingYearly = !showingYearly;

            averagePanel.classList.toggle("app-content-toggle__panel--active", !showingYearly);
            yearlyPanel.classList.toggle("app-content-toggle__panel--active", showingYearly);

            setHidden(averagePanel, showingYearly);
            setHidden(yearlyPanel, !showingYearly);

            toggle.title.textContent = showingYearly ? "Year by year" : "3-year average";
            toggle.button.textContent = showingYearly ? "Show 3-year average" : "Show year by year";
            toggle.button.setAttribute("aria-pressed", showingYearly ? "true" : "false");

            resizeCharts(showingYearly ? yearlyPanel : averagePanel);
        });

        secondPanel.remove();
    }

    function init() {
        document.querySelectorAll(".app-ks4-tabs").forEach(initialiseTabSet);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
