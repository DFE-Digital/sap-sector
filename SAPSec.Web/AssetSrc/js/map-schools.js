(function () {
    let initialised = false;
    let mapInstance = null;

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
                        url: s.url ?? "#",
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
        const la = escapeHtml(s.la || "");
        const url = s.url || "#";

        return `
      <div class="map-popup">
        <a class="govuk-link govuk-link--no-visited-state" href="${url}">
          <strong>${name}</strong>
        </a><br/>
        <span>${address}</span><br/>
        <span>${la}</span>
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
            .map(
                (s) => `
          <li>
            <a class="govuk-link govuk-link--no-visited-state" href="${s.url}">
              ${escapeHtml(s.name)}
            </a><br/>
            <span>${escapeHtml(s.address)}</span>
          </li>`
            )
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

        // Render the visible list (name + address)
        renderSchoolList(schools);

        if (!schools.length) {
            host.innerHTML = `<p class="govuk-body">No schools with map coordinates.</p>`;
            return;
        }

        const fixedZoom = parseInt(host.dataset.fixedZoom || "14", 10);

        // Create map
        mapInstance = L.map(host, { scrollWheelZoom: true }).setView(
            [schools[0].lat, schools[0].lon],
            fixedZoom
        );

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "© OpenStreetMap contributors",
        }).addTo(mapInstance);

        // Cluster group: blue circles with numbers
        const clusters = L.markerClusterGroup({
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


        // Blue SVG pin at correct size (your svg is 20x25)
        const schoolIcon = L.icon({
            iconUrl: "/assets/images/markers/marker-school.svg",
            iconSize: [20, 25],
            iconAnchor: [10, 24],
            popupAnchor: [0, -22],
        });

        // Add markers to clusters
        for (const s of schools) {
            const ll = L.latLng(s.lat, s.lon);
            const m = L.marker(ll, { icon: schoolIcon }).bindPopup(popupHtml(s));
            clusters.addLayer(m);
        }

        // Fit bounds to all markers (including clusters)
        const bounds = clusters.getBounds();
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
