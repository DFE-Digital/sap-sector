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

        // Simple list (name + address). 
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
        mapInstance = L.map(host, { scrollWheelZoom: true }).setView([schools[0].lat, schools[0].lon], fixedZoom);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "© OpenStreetMap contributors",
        }).addTo(mapInstance);

        const layer = L.featureGroup().addTo(mapInstance);

        // Marker icon (use your existing nearby icon)
        const schoolIcon = L.icon({
            iconUrl: "/assets/images/markers/marker-school-nearby.svg",
            iconSize: [36, 54],
            iconAnchor: [18, 52],
            popupAnchor: [0, -44],
        });

        schools.forEach((s) => {
            const ll = L.latLng(s.lat, s.lon);
            L.marker(ll, { icon: schoolIcon })
                .bindPopup(popupHtml(s))
                .addTo(layer);
        });

        // Fit bounds to ALL schools
        const bounds = layer.getBounds();
        if (bounds.isValid()) {
            mapInstance.fitBounds(bounds.pad(0.1), {
                padding: [40, 40],
                maxZoom: 19,
                animate: false,
            });
        }

        // Ensure correct render after toggle
        setTimeout(() => mapInstance.invalidateSize(true), 0);

        initialised = true;
    }

    window.addEventListener("map:shown", initMap);

    document.addEventListener("DOMContentLoaded", function () {
        const mapView = document.getElementById("mapView");
        const isHidden = mapView?.classList.contains("govuk-!-display-none");
        if (!isHidden) initMap();
    });
})();
