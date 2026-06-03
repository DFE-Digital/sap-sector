(function () {
    function getTabText(tabLink) {
        return (tabLink && tabLink.textContent ? tabLink.textContent : "").trim().toLowerCase();
    }

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

    function buildToggleHeader() {
        var header = document.createElement("div");
        header.className = "app-ks4-chart-toggle";

        var title = document.createElement("h3");
        title.className = "govuk-heading-m app-ks4-chart-toggle__title";
        title.textContent = "3-year average";

        var button = document.createElement("button");
        button.type = "button";
        button.className = "app-ks4-chart-toggle__button";
        button.textContent = "Show year by year";
        button.setAttribute("aria-pressed", "false");

        header.appendChild(title);
        header.appendChild(button);

        return { header: header, title: title, button: button };
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

        if (getTabText(firstTab) !== "3-year average" || getTabText(secondTab) !== "year by year") {
            return;
        }

        var averageChart = firstPanel.querySelector(".app-ks4-chart-container");
        var yearlyChart = secondPanel.querySelector(".app-ks4-chart-container");

        if (!averageChart || !yearlyChart) {
            return;
        }

        firstTab.textContent = "Charts";
        listItems[1].remove();

        averageChart.classList.add("app-ks4-chart-view");
        yearlyChart.classList.add("app-ks4-chart-view");
        averageChart.classList.add("app-ks4-chart-view--active");

        setHidden(yearlyChart, true);

        firstPanel.insertBefore(yearlyChart, firstPanel.firstChild);
        firstPanel.insertBefore(averageChart, yearlyChart);

        var toggle = buildToggleHeader();
        firstPanel.insertBefore(toggle.header, firstPanel.firstChild);

        var showingYearly = false;

        toggle.button.addEventListener("click", function () {
            showingYearly = !showingYearly;

            averageChart.classList.toggle("app-ks4-chart-view--active", !showingYearly);
            yearlyChart.classList.toggle("app-ks4-chart-view--active", showingYearly);

            setHidden(averageChart, showingYearly);
            setHidden(yearlyChart, !showingYearly);

            toggle.title.textContent = showingYearly ? "Year by year" : "3-year average";
            toggle.button.textContent = showingYearly ? "Show 3-year average" : "Show year by year";
            toggle.button.setAttribute("aria-pressed", showingYearly ? "true" : "false");

            resizeCharts(showingYearly ? yearlyChart : averageChart);
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
