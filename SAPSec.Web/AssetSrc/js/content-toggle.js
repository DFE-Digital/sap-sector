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

function initialiseToggle(toggle, activeIndex) {
    var title = toggle.querySelector(".app-content-toggle__title");
    var button = toggle.querySelector(".app-content-toggle__header button[type='button']");
    var panels = Array.prototype.slice.call(toggle.querySelectorAll("[data-content-toggle-panel]"));

    if (!title || !button || panels.length < 2) {
        return;
    }

    if (!activeIndex) {
        activeIndex = panels.findIndex(function (panel) {
            return !panel.hasAttribute("hidden");
        });
    }

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
        button.setAttribute("aria-label", index === 0 ? "Show " + activeName : "Show year by year");

        resizeCharts(activePanel);
    }

    button.addEventListener("click", function () {
        activeIndex = (activeIndex + 1) % panels.length;
        render(activeIndex);
    });

    render(activeIndex);
}

function getActiveIndex(element) {
    var panels = Array.prototype.slice.call(element.querySelectorAll(".app-content-toggle [data-content-toggle-panel]"));
    var activeIndex = panels.findIndex(function (panel) {
        return !panel.hasAttribute("hidden");
    });

    if (activeIndex < 0) {
        activeIndex = 0;
    }

    return activeIndex;
}

function init(element, activeIndex) {
    element.querySelectorAll('.app-content-toggle').forEach(t => initialiseToggle(t, activeIndex));
}

function initAll() {
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initAll);
        return;
    }

    init(document);
}

export {
    init,
    initAll,
    getActiveIndex
};
