(function () {
    var focusTargetParameter = 'focusTarget';
    var filterFocusTarget = 'filters';
    var sortFocusTarget = 'sort';

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFilterProtection);
    } else {
        initFilterProtection();
    }

    function focusElement(element) {
        if (!element) return;

        if (!element.hasAttribute('tabindex')) {
            element.setAttribute('tabindex', '-1');
        }

        element.focus();
    }

    function focusAfterPageLoad() {
        var focusTarget = new URLSearchParams(window.location.search).get(focusTargetParameter);
        if (!focusTarget) return;

        if (focusTarget === sortFocusTarget) {
            focusElement(document.getElementById('sort-by'));
            return;
        }

        if (focusTarget === filterFocusTarget) {
            focusElement(
                document.getElementById('selected-filters')
                || document.getElementById('similar-schools-results-count')
                || document.getElementById('similar-schools-results'));
        }
    }

    function setFocusTarget(value) {
        var focusTargetInput = document.getElementById('similar-schools-focus-target');
        if (focusTargetInput) {
            focusTargetInput.value = value;
        }
    }

    function appendFocusTarget(url, value) {
        var parsedUrl = new URL(url, window.location.origin);
        parsedUrl.searchParams.set(focusTargetParameter, value);
        return parsedUrl.pathname + parsedUrl.search + parsedUrl.hash;
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

            setFocusTarget(filterFocusTarget);

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

        filterForm.querySelectorAll('.app-filter__tag, .app-clear-filters-spacing a').forEach(function (link) {
            link.href = appendFocusTarget(link.href, filterFocusTarget);
        });

        var sortSelect = document.getElementById('sort-by');
        if (sortSelect) {
            sortSelect.addEventListener('change', function () {
                setFocusTarget(sortFocusTarget);
                filterForm.submit();
            });
        }
    }

    focusAfterPageLoad();
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
