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

    function showMap({ persist = true } = {}) {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");
        if (!listView || !mapView || !toggle) return;

        listView.classList.add("govuk-!-display-none");
        mapView.classList.remove("govuk-!-display-none");

        setToggleText(toggle, "View as a list");
        toggle.dataset.view = "map";
        toggle.setAttribute("aria-expanded", "true");

        // mount into map header (sometimes needs a tick after display change)
        requestAnimationFrame(() => mountToggle(MAP_SLOT_ID));
        setTimeout(() => mountToggle(MAP_SLOT_ID), 0);

        if (persist) sessionStorage.setItem(STORAGE_KEY, "map");

        window.dispatchEvent(new Event("map:shown"));
    }

    function showList({ persist = true } = {}) {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");
        if (!listView || !mapView || !toggle) return;

        mapView.classList.add("govuk-!-display-none");
        listView.classList.remove("govuk-!-display-none");

        setToggleText(toggle, "View on map");
        toggle.dataset.view = "list";
        toggle.setAttribute("aria-expanded", "false");

        mountToggle(LIST_SLOT_ID);

        if (persist) sessionStorage.setItem(STORAGE_KEY, "list");
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
            if (isList) showMap();
            else showList();
            return;
        }

        const toggleToList = e.target.closest("#toggleToListLink");
        if (toggleToList) {
            e.preventDefault();
            showList();
        }
    });

    // If other scripts fire map:shown, ensure toggle ends up in the map slot
    window.addEventListener("map:shown", function () {
        mountToggle(MAP_SLOT_ID);
    });
})();
