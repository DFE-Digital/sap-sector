(function () {
    let initialised = false;
    let mapInstance = null;

    function focusElement(el) {
        if (el && typeof el.focus === "function") {
            el.focus();
        }
    }

    function focusNextMarker(marker) {
        const markerElement = marker.getElement?.();
        const mapContainer = marker._map?.getContainer?.();
        if (!markerElement || !mapContainer) return false;

        const focusableMarkers = Array.from(mapContainer.querySelectorAll(".leaflet-marker-icon"))
            .filter((el) => el.tabIndex >= 0);

        const currentIndex = focusableMarkers.indexOf(markerElement);
        if (currentIndex < 0) return false;

        const nextMarker = focusableMarkers[currentIndex + 1];
        if (!nextMarker) return false;

        focusElement(nextMarker);
        return true;
    }

    function wirePopupFocusOrder(marker) {
        marker.on("popupopen", function (event) {
            const popupElement = event.popup?.getElement?.();
            if (!popupElement) return;

            const markerElement = marker.getElement?.();
            const openedFromMarkerFocus = document.activeElement === markerElement;
            if (!openedFromMarkerFocus) return;

            const primaryLink = popupElement.querySelector(".popup-name[href]");
            const closeButton = popupElement.querySelector(".leaflet-popup-close-button");

            if (!primaryLink && !closeButton) return;

            if (primaryLink && closeButton) {
                primaryLink.addEventListener("keydown", function onPrimaryLinkKeydown(e) {
                    if (e.key === "Tab" && !e.shiftKey) {
                        e.preventDefault();
                        focusElement(closeButton);
                    }
                });

                closeButton.addEventListener("keydown", function onCloseButtonKeydown(e) {
                    if (e.key === "Tab" && e.shiftKey) {
                        e.preventDefault();
                        focusElement(primaryLink);
                        return;
                    }

                    if (e.key === "Tab" && !e.shiftKey) {
                        e.preventDefault();
                        marker.closePopup();
                        setTimeout(function () {
                            focusNextMarker(marker);
                        }, 0);
                    }
                });
            }

            focusElement(primaryLink || closeButton);
        });
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
                        url: s.url ,
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

        // Create map
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
            // Cluster group: blue circles with numbers
            clusters = L.markerClusterGroup({
                showCoverageOnHover: false,
                spiderfyOnMaxZoom: true,
                iconCreateFunction: (cluster) => {
                    const count = cluster.getChildCount();

                    let cls = "cluster-yellow";
                    if (count >= 10 && count <= 100) cls = "cluster-orange";
                    if (count > 100) cls = "cluster-red";

                    return L.divIcon({
                        html: `<div><span>${count}</span></div>`,
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


        const markers = []; // collect markers for bounds when NOT clustering

        for (const s of schools) {
            const ll = L.latLng(s.lat, s.lon);
            
            // const m = L.marker(ll, { icon: schoolIcon }).bindPopup(popupHtml(s));

            let iconToUse = blueSchoolIcon;

            // If compare page (no clusters) and this is the main school → pink
            if (!useClusters) {
                iconToUse = s.isComparedSchool ? blueSchoolIcon : pinkSchoolIcon;
            }

            const m = L.marker(ll, {
                icon: iconToUse,
                title: s.name || "School",
                alt: s.name || "School",
            }).bindPopup(popupHtml(s));

            wirePopupFocusOrder(m);

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

        // Ensure correct render after toggle
        setTimeout(() => mapInstance?.invalidateSize(true), 0);

        initialised = true;
    }

    window.addEventListener("map:shown", initMap);

    document.addEventListener("DOMContentLoaded", function () {
        const mapView = document.getElementById("mapView");
        const isHidden = mapView?.classList.contains("govuk-!-display-none");
        if (!isHidden) initMap();
    });
})();
