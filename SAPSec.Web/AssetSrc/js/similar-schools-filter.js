(function () {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFilterProtection);
    } else {
        initFilterProtection();
    }

    function initFilterProtection() {
        var filterForm = document.getElementById('app-filter-panel');
        if (!filterForm) return;

        const applyButtons = document.getElementsByClassName('app-filter__apply-button');

        filterForm.onsubmit = function (e) {
            const submitEvent = e;
            const submitter = submitEvent.submitter;
            const isApplyButton = [...applyButtons].indexOf(submitter) > -1;

            if (!isApplyButton) {
                e.preventDefault();
                return false;
            }

            for (var applyButton of applyButtons) {
                applyButton.classList.add('govuk-button--loading');
                applyButton.disabled = true;
            }
        };

        var checkboxes = filterForm.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(function (cb) {
            var clone = cb.cloneNode(true);
            cb.parentNode.replaceChild(clone, cb);
        });
    }
})();

(function () {
    var sections = document.querySelectorAll('[data-module="app-filter-section"]');
    sections.forEach(function (section) {
        var toggle = section.querySelector('.app-filter-section__toggle');
        var content = section.querySelector('.app-filter-section__content');
        if (toggle && content) {
            toggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                var expanded = toggle.getAttribute('aria-expanded') === 'true';
                toggle.setAttribute('aria-expanded', !expanded);
                expanded ? content.setAttribute('hidden', '') : content.removeAttribute('hidden');
            });
        }
    });

    var filterToggle = document.querySelector('[data-module="app-filter-toggle"]');
    var filterPanel = document.getElementById('app-filter-panel');
    if (filterToggle && filterPanel) {
        filterToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var expanded = filterToggle.getAttribute('aria-expanded') === 'true';
            filterToggle.setAttribute('aria-expanded', !expanded);
            expanded
                ? filterPanel.classList.remove('app-filter-panel--visible')
                : filterPanel.classList.add('app-filter-panel--visible');
        });
    }
})();
