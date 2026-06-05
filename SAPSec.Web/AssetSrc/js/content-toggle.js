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

    function initialiseToggle(toggle) {
        var title = toggle.querySelector(".app-content-toggle__title");
        var button = toggle.querySelector(".app-content-toggle__button");
        var panels = Array.prototype.slice.call(toggle.querySelectorAll("[data-content-toggle-panel]"));

        if (!title || !button || panels.length < 2) {
            return;
        }

        var activeIndex = panels.findIndex(function (panel) {
            return !panel.hasAttribute("hidden");
        });

        if (activeIndex < 0) {
            activeIndex = 0;
        }

        function render(index) {
            var nextIndex = (index + 1) % panels.length;
            var activePanel = panels[index];
            var nextPanel = panels[nextIndex];
            var activeName = activePanel.getAttribute("data-content-toggle-name") || "";
            var nextName = nextPanel.getAttribute("data-content-toggle-name") || "";

            panels.forEach(function (panel, panelIndex) {
                panel.classList.toggle("app-content-toggle__panel--active", panelIndex === index);
                setHidden(panel, panelIndex !== index);
            });

            title.textContent = activeName;
            button.textContent = "Show " + nextName.toLowerCase();
            button.setAttribute("aria-pressed", index === 0 ? "false" : "true");

            resizeCharts(activePanel);
        }

        button.addEventListener("click", function () {
            activeIndex = (activeIndex + 1) % panels.length;
            render(activeIndex);
        });

        render(activeIndex);
    }

    function init() {
        document.querySelectorAll(".app-content-toggle").forEach(initialiseToggle);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
