(function () {
    let initialised = false;
    let mapInstance = null;

    function getFocusablePopupElements(marker) {
        const popupEl = marker.getPopup()?.getElement();
        if (!popupEl) {
            return { popupEl: null, nameLink: null, closeButton: null };
        }

        return {
            popupEl,
            nameLink: popupEl.querySelector(".popup-name[href]"),
            closeButton: popupEl.querySelector(".leaflet-popup-close-button"),
        };
    }

    function getVisibleMapItems(host) {
        const items = Array.from(host.querySelectorAll("[data-map-focusable='true']"))
            .filter((element) => element.isConnected)
            .filter((element) => {
                const rect = element.getBoundingClientRect();
                return rect.width > 0 && rect.height > 0;
            });

        const controls = [];
        const markers = [];

        items.forEach((element) => {
            const group = element.dataset.mapFocusGroup || "marker";
            if (group === "control") {
                controls.push(element);
                return;
            }

            markers.push({
                element,
                rect: element.getBoundingClientRect(),
            });
        });

        const rows = [];
        const verticalTolerance = 24;

        markers
            .sort((left, right) => left.rect.top - right.rect.top)
            .forEach((item) => {
                const centerY = item.rect.top + (item.rect.height / 2);

                const matchingRow = rows.find((row) =>
                    Math.abs(row.centerY - centerY) <= verticalTolerance
                );

                if (matchingRow) {
                    matchingRow.items.push(item);
                    matchingRow.centerY =
                        (matchingRow.centerY * (matchingRow.items.length - 1) + centerY) /
                        matchingRow.items.length;
                    return;
                }

                rows.push({
                    centerY,
                    items: [item],
                });
            });

        const orderedMarkers = rows
            .sort((left, right) => left.centerY - right.centerY)
            .flatMap((row, rowIndex) => {
                const sortedRowItems = row.items
                    .sort((left, right) => left.rect.left - right.rect.left);

                if (rowIndex % 2 === 1) {
                    sortedRowItems.reverse();
                }

                return sortedRowItems.map((item) => item.element);
            });

        return [...orderedMarkers, ...controls];
    }

    function focusAdjacentMapItem(host, currentElement, direction) {
        const items = getVisibleMapItems(host);
        const currentIndex = items.indexOf(currentElement);

        if (currentIndex === -1) {
            return false;
        }

        const nextItem = items[currentIndex + direction];
        if (!nextItem) {
            return false;
        }

        nextItem.focus();
        return true;
    }

    function focusPopupStart(marker) {
        const { nameLink, closeButton } = getFocusablePopupElements(marker);
        const target = nameLink || closeButton;

        if (!target) return false;

        target.focus();
        return true;
    }

    function closePopupAndFocus(marker, focusTarget) {
        marker.closePopup();

        if (!focusTarget) {
            return;
        }

        requestAnimationFrame(() => {
            focusTarget.focus();
        });
    }

    function getSchoolMarkerLabel(school) {
        const parts = [];

        parts.push(school.name || "School");

        if (school.address) {
            parts.push(school.address);
        } else if (school.la) {
            parts.push(school.la);
        }

        return `Open map marker for ${parts.join(", ")}`;
    }

    function syncMarkerExpandedState(markerState) {
        const element = markerState.marker.getElement?.();
        if (!element) {
            return;
        }

        element.setAttribute("aria-expanded", markerState.marker.isPopupOpen() ? "true" : "false");
    }

    function enhancePopupFocus(host, markerState) {
        const { marker, school } = markerState;
        const { popupEl, nameLink, closeButton } = getFocusablePopupElements(marker);

        if (!popupEl || popupEl.dataset.focusManaged === "true") {
            return;
        }

        // Leaflet popups are not part of the normal page tab order, so we wire the
        // popup contents into the same keyboard flow as the markers.
        popupEl.dataset.focusManaged = "true";

        if (closeButton) {
            closeButton.setAttribute("aria-label", `Close ${school.name || "school"} popover`);
            closeButton.addEventListener("click", () => {
                markerState.restoreFocusOnClose = true;
            });
        }

        popupEl.addEventListener("keydown", (event) => {
            if (event.key !== "Escape") return;

            markerState.restoreFocusOnClose = true;
            marker.closePopup();
            event.preventDefault();
        });

        if (nameLink) {
            nameLink.addEventListener("keydown", (event) => {
                if (event.key !== "Tab") return;

                // Shift+Tab from the first popup item should close the popup and
                // return focus to the marker that opened it.
                if (event.shiftKey) {
                    event.preventDefault();
                    closePopupAndFocus(marker, marker.getElement?.());
                    return;
                }

                if (closeButton) {
                    event.preventDefault();
                    closeButton.focus();
                    return;
                }

                const markerElement = marker.getElement?.();
                if (!markerElement) return;

                // Tabbing past the popup should continue to the next visible map
                // item instead of dropping focus out of the map unexpectedly.
                if (focusAdjacentMapItem(host, markerElement, 1)) {
                    event.preventDefault();
                    marker.closePopup();
                }
            });
        }

        if (closeButton) {
            closeButton.addEventListener("keydown", (event) => {
                if (event.key !== "Tab") return;

                if (event.shiftKey) {
                    if (nameLink) {
                        event.preventDefault();
                        nameLink.focus();
                        return;
                    }

                    event.preventDefault();
                    closePopupAndFocus(marker, marker.getElement?.());
                    return;
                }

                const markerElement = marker.getElement?.();
                if (!markerElement) return;

                if (focusAdjacentMapItem(host, markerElement, 1)) {
                    event.preventDefault();
                    marker.closePopup();
                }
            });
        }
    }

    function enhanceMarkerFocus(host, markerState) {
        const { marker, school } = markerState;
        const element = marker.getElement?.();

        if (!element || element.dataset.focusManaged === "true") {
            syncMarkerExpandedState(markerState);
            return;
        }

        element.dataset.focusManaged = "true";
        element.dataset.mapFocusable = "true";
        element.dataset.mapFocusGroup = "marker";
        element.tabIndex = 0;
        element.setAttribute("role", "button");
        element.setAttribute("aria-haspopup", "dialog");
        const markerLabel = getSchoolMarkerLabel(school);
        element.setAttribute("aria-label", markerLabel);
        element.setAttribute("title", markerLabel);
        element.setAttribute("alt", markerLabel);
        syncMarkerExpandedState(markerState);

        element.addEventListener("keydown", (event) => {
            // Markers are rendered as images by Leaflet, so we promote them to
            // keyboard-operable buttons and move focus into the popup on open.
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();

                if (!marker.isPopupOpen()) {
                    marker.openPopup();
                }

                requestAnimationFrame(() => {
                    focusPopupStart(marker);
                });

                return;
            }

            if (event.key !== "Tab") return;

            const direction = event.shiftKey ? -1 : 1;
            if (!focusAdjacentMapItem(host, element, direction)) {
                return;
            }

            if (marker.isPopupOpen()) {
                marker.closePopup();
            }

            event.preventDefault();
        });
    }

    function enhanceClusterFocus(host) {
        host.querySelectorAll(".marker-cluster").forEach((element) => {
            if (element.dataset.focusManaged === "true") {
                return;
            }

            element.dataset.focusManaged = "true";
            element.dataset.mapFocusable = "true";
            element.dataset.mapFocusGroup = "marker";
            element.tabIndex = 0;
            element.setAttribute("role", "button");

            const count = element.textContent?.trim();
            const clusterLabel = count
                ? `Expand map cluster of ${count} schools`
                : "Open map cluster";
            element.setAttribute("aria-label", clusterLabel);
            element.setAttribute("title", clusterLabel);
            element.querySelectorAll("span").forEach((span) => {
                span.setAttribute("aria-hidden", "true");
            });

            element.addEventListener("keydown", (event) => {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    element.click();
                    return;
                }

                if (event.key !== "Tab") return;

                const direction = event.shiftKey ? -1 : 1;
                if (!focusAdjacentMapItem(host, element, direction)) {
                    return;
                }

                event.preventDefault();
            });
        });
    }

    function enhanceZoomControlFocus(host) {
        host.querySelectorAll(".leaflet-control-zoom a").forEach((element) => {
            if (element.dataset.focusManaged === "true") {
                return;
            }

            element.dataset.focusManaged = "true";
            element.dataset.mapFocusable = "true";
            element.dataset.mapFocusGroup = "control";

            if (!element.getAttribute("aria-label")) {
                const zoomLabel = element.classList.contains("leaflet-control-zoom-in")
                    ? "Zoom in"
                    : element.classList.contains("leaflet-control-zoom-out")
                        ? "Zoom out"
                        : (element.getAttribute("title") || "").trim();

                if (zoomLabel) {
                    element.setAttribute("aria-label", zoomLabel);
                }
            }

            element.addEventListener("keydown", (event) => {
                if (event.key !== "Tab") return;

                const direction = event.shiftKey ? -1 : 1;
                if (!focusAdjacentMapItem(host, element, direction)) {
                    return;
                }

                event.preventDefault();
            });
        });
    }

    function refreshMapAccessibility(host, markerStates) {
        markerStates.forEach((markerState) => {
            enhanceMarkerFocus(host, markerState);
            syncMarkerExpandedState(markerState);
        });

        enhanceClusterFocus(host);
        enhanceZoomControlFocus(host);
    }

    function parseSchools(host) {
        const el = document.getElementById("schools-data");
        const schoolsJson = el ? el.textContent : "[]";

        try {
            const raw = JSON.parse(schoolsJson);
            return (raw || [])
                .map((s) => {
                    const lat = typeof s.lat === "string" ? parseFloat(s.lat) : s.lat;
                    const lon = typeof s.lon === "string" ? parseFloat(s.lon) : s.lon;
                    if (!Number.isFinite(lat) || !Number.isFinite(lon)) return null;

                    return {
                        urn: s.urn ?? "",
                        name: s.name ?? "",
                        address: s.address ?? "",
                        la: s.la ?? "",
                        lat,
                        lon,
                        url: s.url,
                        isComparedSchool: Boolean(s.isComparedSchool),
                    };
                })
                .filter(Boolean);
        } catch (e) {
            console.warn("Could not parse schools JSON", e);
            return [];
        }
    }

    function escapeHtml(str) {
        return String(str ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function popupHtml(s) {
        const name = escapeHtml(s.name || "School");
        const address = escapeHtml(s.address || "");

        const hasUrl =
            typeof s.url === "string" &&
            s.url.trim().length > 0;

        const nameHtml = hasUrl
            ? `<a class="govuk-link govuk-link--no-visited-state popup-name" href="${s.url}">
               <strong>${name}</strong>
           </a>`
            : `<strong class="popup-name">${name}</strong>`;

        return `
        <div class="map-popup">
            ${nameHtml}
            <div class="popup-gap"></div>
            <span class="popup-address">${address}</span>
        </div>
    `;
    }

    function renderSchoolList(schools) {
        const listEl = document.getElementById("schoolList");
        if (!listEl) return;

        if (!schools.length) {
            listEl.innerHTML = `<p class="govuk-body">No schools to display.</p>`;
            return;
        }

        listEl.innerHTML = `
      <ul class="govuk-list govuk-list--bullet">
        ${schools
            .map((s) => {
                const nameHtml = s.url
                    ? `<a class="govuk-link govuk-link--no-visited-state" href="${s.url}">
                         ${escapeHtml(s.name)}
                       </a>`
                    : `<span>${escapeHtml(s.name)}</span>`;

                return `
                  <li>
                    ${nameHtml}<br/>
                    <span>${escapeHtml(s.address)}</span>
                  </li>`;
            })
            .join("")}
      </ul>
    `;
    }

    function initMap() {
        const host = document.getElementById("map");
        if (!host) return;

        // If already initialised, just fix sizing (e.g. after tab toggle)
        if (initialised) {
            mapInstance?.invalidateSize(true);
            return;
        }

        const schools = parseSchools(host);

        const loading = host.querySelector(".map-loading");
        if (loading) loading.remove();

        renderSchoolList(schools);

        if (!schools.length) {
            host.innerHTML = `<p class="govuk-body">No schools with map coordinates.</p>`;
            return;
        }

        const fixedZoom = parseInt(host.dataset.fixedZoom || "14", 10);
        const mode = (host.dataset.mapMode || "all").toLowerCase();
        const useClusters = mode !== "compare";

        // Create the Leaflet map once the host is visible and data is available.
        mapInstance = L.map(host, { scrollWheelZoom: true }).setView(
            [schools[0].lat, schools[0].lon],
            fixedZoom
        );

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "© OpenStreetMap contributors",
            referrerPolicy: "origin",
        }).addTo(mapInstance);

        let clusters = null;

        if (useClusters) {
            // Cluster schools in non-compare mode so dense areas stay navigable.
            clusters = L.markerClusterGroup({
                showCoverageOnHover: false,
                spiderfyOnMaxZoom: true,
                iconCreateFunction: (cluster) => {
                    const count = cluster.getChildCount();

                    let cls = "cluster-yellow";
                    if (count >= 10 && count <= 100) cls = "cluster-orange";
                    if (count > 100) cls = "cluster-red";

                    return L.divIcon({
                        html: `<div aria-hidden="true"><span aria-hidden="true">${count}</span></div>`,
                        className: `marker-cluster ${cls}`,
                        iconSize: L.point(40, 40),
                    });
                },
            });

            mapInstance.addLayer(clusters);
        }

        const blueSchoolIcon = L.icon({
            iconUrl: "/assets/images/marker-school.svg",
            iconSize: [20, 25],
            iconAnchor: [10, 24],
            popupAnchor: [0, -22],
        });

        const pinkSchoolIcon = L.icon({
            iconUrl: "/assets/images/marker-school-pink.svg",
            iconSize: [20, 25],
            iconAnchor: [10, 24],
            popupAnchor: [0, -22],
        });

        const markers = [];
        const markerStates = [];

        for (const s of schools) {
            const ll = L.latLng(s.lat, s.lon);

            let iconToUse = blueSchoolIcon;

            // Compare mode renders the selected school in pink because clustering is off.
            if (!useClusters) {
                iconToUse = s.isComparedSchool ? blueSchoolIcon : pinkSchoolIcon;
            }

            const markerLabel = getSchoolMarkerLabel(s);
            const m = L.marker(ll, {
                icon: iconToUse,
                keyboard: true,
                title: markerLabel,
                alt: markerLabel,
            }).bindPopup(popupHtml(s));
            const markerState = {
                marker: m,
                school: s,
                restoreFocusOnClose: false,
            };
            markerStates.push(markerState);

            m.on("add", () => {
                enhanceMarkerFocus(host, markerState);
            });

            m.on("popupopen", () => {
                enhanceMarkerFocus(host, markerState);
                enhancePopupFocus(host, markerState);
                syncMarkerExpandedState(markerState);
            });

            m.on("popupclose", () => {
                syncMarkerExpandedState(markerState);

                if (!markerState.restoreFocusOnClose) {
                    return;
                }

                markerState.restoreFocusOnClose = false;
                requestAnimationFrame(() => {
                    marker.getElement?.()?.focus();
                });
            });

            if (useClusters) {
                clusters.addLayer(m);
            } else {
                m.addTo(mapInstance);
                markers.push(m);
            }
        }

        const bounds = useClusters
            ? clusters.getBounds()
            : L.featureGroup(markers).getBounds();

        if (bounds.isValid()) {
            mapInstance.fitBounds(bounds.pad(0.1), {
                padding: [40, 40],
                maxZoom: 19,
                animate: false,
            });
        }

        const refresh = () => {
            mapInstance?.invalidateSize(true);
            refreshMapAccessibility(host, markerStates);
        };

        // Recalculate layout and focus order after map movement or cluster changes.
        mapInstance.on("zoomend moveend", () => {
            requestAnimationFrame(refresh);
        });

        if (clusters) {
            clusters.on("animationend spiderfied unspiderfied", () => {
                requestAnimationFrame(refresh);
            });
        }

        setTimeout(refresh, 0);

        initialised = true;
    }

    window.addEventListener("map:shown", initMap);

    document.addEventListener("DOMContentLoaded", function () {
        const mapView = document.getElementById("mapView");
        const isHidden = mapView?.classList.contains("govuk-!-display-none");
        if (!isHidden) initMap();
    });
})();
