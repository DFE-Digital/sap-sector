(function () {
    document.addEventListener('click', function (event) {
        const link = event.target.closest('a');
        if (!link) return;

        const data = {
            EventType: 'linkClick',
            Url: link.href,
            Text: link.innerText.trim()
        };

        sendCustomEvent(data);
    });

    document.addEventListener('toggle', function (event) {
        const details = event.target.closest('details');
        if (!details) return;

        const h2 = details.previousElementSibling?.matches('h2') ? details.previousElementSibling : '';
        const summary = details.querySelector('summary');

        const data = {
            EventType : 'detailsClick',
            SectionName: h2.innerText.trim() ?? '',
            SectionLabel: summary.textContent?.trim() ?? '',
            InteractionType: `${details.open ? 'open' : 'closed'}`
        };

        sendCustomEvent(data);
    }, true);

    function sendCustomEvent(data) {
        fetch('/custom-event-tracking', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error("Request failed with status " + response.status);
            }
        });
    }

})();
