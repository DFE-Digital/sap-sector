(function () {
    function showMap() {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");

        if (!listView || !mapView || !toggle) return;

        listView.classList.add("govuk-!-display-none");
        mapView.classList.remove("govuk-!-display-none");

        const textEl = toggle.querySelector(".toggle-text");
        if (textEl) textEl.textContent = "View as list";
        toggle.dataset.view = "map";

        window.dispatchEvent(new Event("map:shown"));
    }

    function showList() {
        const listView = document.getElementById("listView");
        const mapView = document.getElementById("mapView");
        const toggle = document.getElementById("toggleViewLink");

        if (!listView || !mapView || !toggle) return;

        mapView.classList.add("govuk-!-display-none");
        listView.classList.remove("govuk-!-display-none");

        const textEl = toggle.querySelector(".toggle-text");
        if (textEl) textEl.textContent = "View on map";
        toggle.dataset.view = "list";
    }

    document.addEventListener("click", function (e) {
        // ✅ works even if user clicks svg/path/span inside the link
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
})();
