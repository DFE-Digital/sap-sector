import { MobileCollapsedTabs } from '/js/mobile-collapsed-tabs.js'
import * as ChartFactory from '/js/chart-factory.js'
import * as ContentToggle from '/js/content-toggle.js'

const FILTER_CONFIG = {
    applyFiltersDebounceMs: 100
};

function init(select) {
    var targetId = select.dataset.measureFilterTargetId;
    var activeRequestId = 0;

    if (!targetId) {
        throw new Error('Measure filter target ID not set.');
    }

    var form = select.closest('form');
    if (!form) {
        throw new Error('Measure filter must appear within a <form action="" method="get">');
    }

    function applyFilters() {
        let applyFiltersTimeout;
        clearTimeout(applyFiltersTimeout);
        applyFiltersTimeout = setTimeout(() => {
            activeRequestId += 1;
            var requestId = activeRequestId;

            var formData = new FormData(form);
            var search = new URLSearchParams(formData);
            var requestUrl = '?' + search.toString();

            return fetch(requestUrl, {
                headers: {
                    Accept: "text/html"
                }
            })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error(`Request failed with status ${response.status}`);
                }

                return response.text();
            })
            .then(function (content) {
                if (requestId !== activeRequestId) {
                    return;
                }

                const targetElement = document.getElementById(targetId);

                // Get current state of tab panels
                const tabs = window.GOVUKComponents['MobileCollapsedTabs'];
                const i = tabs.findIndex(t => t.$root == targetElement.querySelector('[data-module="govuk-tabs"]'));
                var tabState = tabs[i].getState();

                // Get current state of content toggle
                var toggleActiveIndex = ContentToggle.getActiveIndex(targetElement);

                // Replace target element with same element from response
                const responseContent = new DOMParser().parseFromString(content, "text/html");
                const targetElementFromResponse = responseContent.getElementById(targetId);
                targetElement.innerHTML = targetElementFromResponse.innerHTML;

                // Re-initialise components, with saved state
                tabs[i].teardown();
                tabs[i] = new MobileCollapsedTabs(targetElement.querySelector('[data-module="govuk-tabs"]'));
                tabs[i].setState(tabState);

                ChartFactory.init(targetElement);
                ContentToggle.init(targetElement, toggleActiveIndex);
            })
            .catch(function (error) {
                console.error("Failed to load view data.", error);
            });
        }, FILTER_CONFIG.applyFiltersDebounceMs);
    }

    select.addEventListener("change", applyFilters);
    select.addEventListener("input", applyFilters);
}

function initAll() {
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initAll);
        return;
    }

    document.querySelectorAll('[data-measure-filter-target-id]').forEach(init);
}

export {
    init,
    initAll
};