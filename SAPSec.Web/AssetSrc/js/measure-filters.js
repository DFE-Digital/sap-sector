import { MobileCollapsedTabs } from '/js/mobile-collapsed-tabs.js'
import * as ChartFactory from '/js/chart-factory.js'

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

    var submitButton = select.parentElement.querySelector('button[type="submit"]');
    if (submitButton) {
        submitButton.style.display = "none";
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

                const responseContent = new DOMParser().parseFromString(content, "text/html");
                const measureFromResponse = responseContent.getElementById(targetId);
                const target = document.getElementById(targetId);
                const selectedTab = target.querySelector('a[aria-selected="true"]')?.getAttribute('href');
                target.innerHTML = measureFromResponse.innerHTML;

                ChartFactory.init(target);
                const tabs = new MobileCollapsedTabs(target.querySelector('[data-module="govuk-tabs"]'));
                tabs.selectTabById(selectedTab);
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