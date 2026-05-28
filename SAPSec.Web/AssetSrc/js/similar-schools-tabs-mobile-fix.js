(function () {
    var MOBILE_MEDIA_QUERY = "(max-width: 40.0625em)";

    function setActivePanel(tabContainer, targetId, initial = false) {
        console.log('setActivePanel(', tabContainer.id, targetId, initial, ')');

        var tabs = tabContainer.querySelectorAll(".govuk-tabs__tab");
        var listItems = tabContainer.querySelectorAll(".govuk-tabs__list-item");
        var panels = tabContainer.querySelectorAll(".govuk-tabs__panel");

        panels.forEach(function (panel) {
            if (panel.id === targetId) {
                panel.classList.remove("govuk-tabs__panel--hidden");
            } else {
                panel.classList.add("govuk-tabs__panel--hidden");
            }
        });

        listItems.forEach(function (item) {
            item.classList.remove("govuk-tabs__list-item--selected");
        });

        tabs.forEach(function (tab) {
            var href = tab.getAttribute("href") || "";
            var id = href.startsWith("#") ? href.substring(1) : "";
            var isActive = id === targetId;
            tab.setAttribute("aria-selected", isActive ? "true" : "false");
            tab.setAttribute("tabindex", isActive ? "0" : "-1");

            if (isActive) {
                var parent = tab.closest(".govuk-tabs__list-item");
                if (parent) {
                    parent.classList.add("govuk-tabs__list-item--selected");
                }
            }
        });

        if (!initial) {
            var newUrl = window.location.href.substring(0, window.location.href.indexOf('#')) + '#' + targetId;
            console.log(newUrl);
            window.history.replaceState(null, '', newUrl);
        }

        requestAnimationFrame(function () {
            resizeVisibleCharts(tabContainer);
        });
    }

    function resizeVisibleCharts(tabContainer) {
        if (!window.Chart) {
            return;
        }

        tabContainer.querySelectorAll(".govuk-tabs__panel:not(.govuk-tabs__panel--hidden) canvas.js-chart").forEach(function (canvas) {
            var chart = window.Chart.getChart(canvas);
            if (!chart) {
                return;
            }

            chart.resize();
            chart.update("none");
        });
    }

    function getInitialTargetId(tabContainer) {
        var target = window.location.href.substring(window.location.href.indexOf('#'));
        var tabSelectedInUrl = tabContainer.querySelector(`.govuk-tabs__list-item--selected .govuk-tabs__tab[href="${target}"]`);
        if (tabSelectedInUrl) {
            console.log(target);
            return target.substring(1);
        }

        var selectedTab = tabContainer.querySelector(".govuk-tabs__list-item--selected .govuk-tabs__tab");
        if (selectedTab) {
            var selectedHref = selectedTab.getAttribute("href") || "";
            if (selectedHref.startsWith("#")) {
                return selectedHref.substring(1);
            }
        }

        var firstTab = tabContainer.querySelector(".govuk-tabs__tab");
        if (!firstTab) {
            return null;
        }

        var firstHref = firstTab.getAttribute("href") || "";
        return firstHref.startsWith("#") ? firstHref.substring(1) : null;
    }

    function initTabContainer(tabContainer) {
        var initialTargetId = getInitialTargetId(tabContainer);
        if (initialTargetId) {
            setActivePanel(tabContainer, initialTargetId, true);
        }

        if (tabContainer.dataset.mobileTabsBound === "true") {
            return;
        }

        tabContainer.addEventListener("click", function (event) {
            var tab = event.target.closest(".govuk-tabs__tab");
            if (!tab || !tabContainer.contains(tab)) {
                return;
            }

            var href = tab.getAttribute("href") || "";
            if (!href.startsWith("#")) {
                return;
            }

            event.preventDefault();
            setActivePanel(tabContainer, href.substring(1));
        });

        tabContainer.addEventListener("keydown", function (event) {
            if (event.key !== " " && event.key !== "Enter") {
                return;
            }

            var tab = event.target.closest(".govuk-tabs__tab");
            if (!tab || !tabContainer.contains(tab)) {
                return;
            }

            var href = tab.getAttribute("href") || "";
            if (!href.startsWith("#")) {
                return;
            }

            event.preventDefault();
            setActivePanel(tabContainer, href.substring(1));
        });

        tabContainer.dataset.mobileTabsBound = "true";
    }

    function init(element) {
        element.querySelectorAll(".app-ks4-tabs").forEach(initTabContainer);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => init(document));
    } else {
        init(document);
    }

    // GOV.UK tabs may re-apply classes after module init; enforce mobile state again.
    window.addEventListener("load", () => init(document));
    window.setTimeout(() => init(document), 100);
    window.addEventListener("resize", () => { console.log('resizing'); init(document); });
    window.matchMedia(MOBILE_MEDIA_QUERY).addEventListener("change", () => init(document));

    window.MeasureTabs = {
        init(element) {
            init(element);
        }
    };
})();
