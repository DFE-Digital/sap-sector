(function () {
    function buildRequestUrl(endpoint, queryKey, selectedValue) {
        var separator = endpoint.indexOf("?") === -1 ? "?" : "&";
        return endpoint + separator + queryKey + "=" + encodeURIComponent(selectedValue);
    }

    function init() {
        document.querySelectorAll("[data-view-switcher='true']").forEach(function (select) {
            var dataEndpoint = select.getAttribute("data-endpoint");
            var queryKey = select.getAttribute("data-query-key");
            var targetId = select.getAttribute("data-target-id");
            var activeRequestId = 0;

            if (!dataEndpoint || !queryKey || !targetId) {
                return;
            }

            function loadSelectedValue(selectedValue) {
                activeRequestId += 1;
                var requestId = activeRequestId;

                return fetch(buildRequestUrl(dataEndpoint, queryKey, selectedValue), {
                    headers: {
                        Accept: "text/html"
                    }
                })
                    .then(function (response) {
                        if (!response.ok) {
                            throw new Error("Request failed with status " + response.status);
                        }

                        return response.text();
                    })
                    .then(function (content) {
                        if (requestId !== activeRequestId) {
                            return;
                        }

                        var doc = new DOMParser().parseFromString(content, "text/html");
                        var measureContent = doc.getElementById(targetId);
                        var target = document.getElementById(targetId);

                        target.innerHTML = measureContent.innerHTML;
                        window.MeasureCharts.init(target);
                        window.MeasureTabs.init(target);
                    })
                    .catch(function (error) {
                        console.error("Failed to load view data.", error);
                    });
            }

            function refreshSelection() {
                loadSelectedValue(select.value);
            }

            select.addEventListener("change", refreshSelection);
            select.addEventListener("input", refreshSelection);
            window.requestAnimationFrame(refreshSelection);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
        return;
    }

    init();
})();
