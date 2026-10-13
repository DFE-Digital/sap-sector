(function () {
    const LIST_SLOT_ID = "resultsBarActions";
    const MAP_SLOT_ID = "mapBarActions";
    const STORAGE_KEY = "schoolSearchView"; // "list" or "map"

    document.documentElement.classList.add('js-enabled');

    function mountToggle(where) {
        const slot = document.getElementById(where);
        const wrap = document.getElementById("toggleWrap");
        if (slot && wrap && wrap.parentElement !== slot) slot.appendChild(wrap);
    }

    function setToggleText(toggle, text) {
        const textEl = toggle?.querySelector(".toggle-text");
        if (textEl) textEl.textContent = text;
    }

    function getResultsLabel(toggle) {
        return toggle.dataset.resultsLabel || "schools";
    }

    function setToggleLabel(toggle, nextView, currentView) {
        const resultsLabel = getResultsLabel(toggle);
        toggle.setAttribute("aria-label", `View ${nextView}, currently showing ${currentView} of ${resultsLabel}`);
    }

    function announceView(toggle, currentView) {
        const notificationContainer = document.getElementById("notification-container");
        if (!notificationContainer) return;

        notificationContainer.textContent = `Showing ${currentView} of ${getResultsLabel(toggle)}`;
    }

    function focusListStart() {
        const listView = document.getElementById("listView");
        const list = listView?.querySelector(".app-school-results");
        const target = list || listView?.querySelector("a[href], button, input, select, textarea, [tabindex]:not([tabindex='-1'])");

        if (!target) return;

        if (!target.hasAttribute("tabindex")) {
            target.setAttribute("tabindex", "-1");
        }

        requestAnimationFrame(() => target.focus());
    }

    function showMap({ persist = true } = {}) {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");
        if (!listView || !mapView || !toggle) return;
        const shouldRestoreToggleFocus = document.activeElement === toggle;

        listView.classList.add("govuk-!-display-none");
        mapView.classList.remove("govuk-!-display-none");

        setToggleText(toggle, "View as a list");
        toggle.dataset.view = "map";
        setToggleLabel(toggle, "as a list", "map");

        mountToggle(MAP_SLOT_ID);

        if (shouldRestoreToggleFocus) {
            toggle.focus();
        }

        if (persist) sessionStorage.setItem(STORAGE_KEY, "map");

        window.dispatchEvent(new Event("map:shown"));
    }

    function showList({ persist = true, focusResults = false } = {}) {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");
        if (!listView || !mapView || !toggle) return;

        mapView.classList.add("govuk-!-display-none");
        listView.classList.remove("govuk-!-display-none");

        setToggleText(toggle, "View on map");
        toggle.dataset.view = "list";
        setToggleLabel(toggle, "on map", "list");

        mountToggle(LIST_SLOT_ID);

        if (persist) sessionStorage.setItem(STORAGE_KEY, "list");

        if (focusResults) {
            focusListStart();
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        // Default view is list unless previously stored
        const saved = sessionStorage.getItem(STORAGE_KEY);

        if (saved === "map") {
            showMap({ persist: false });
        } else {
            showList({ persist: false });
        }
    });

    document.addEventListener("click", function (e) {
        const toggleLink = e.target.closest("#toggleViewLink");
        if (toggleLink) {
            e.preventDefault();
            const isList = toggleLink.dataset.view === "list";
            if (isList) {
                showMap();
                announceView(toggleLink, "map");
            }
            else {
                showList({ focusResults: true });
                announceView(toggleLink, "list");
            }
            return;
        }

        const toggleToList = e.target.closest("#toggleToListLink");
        if (toggleToList) {
            e.preventDefault();
            showList({ focusResults: true });
        }
    });

    // If other scripts fire map:shown, ensure toggle ends up in the map slot
    window.addEventListener("map:shown", function () {
        mountToggle(MAP_SLOT_ID);
    });
})();
