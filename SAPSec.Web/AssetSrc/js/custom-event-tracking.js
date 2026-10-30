(function () {
    document.addEventListener('click', function (event) {
        const link = event.target.closest('a');
        if (!link) return;

        const data = {
            EventType: 'link_click',
            Url: link.href,
            Text: link.innerText.trim()
        };

        sendCustomEvent(data);
    });

    //need to pass
    //Key = section_name - e.g. Progress 8
    //Value 1 = details_lable e.g. - information on Progress 8
    //Value 2 = interaction_type - open/close


    document.addEventListener('toggle', function (event) {
        const details = event.target.closest('details');
        if (!details) return;

        const h2 = details.previousElementSibling?.matches('h2') ? details.previousElementSibling : '';
        const summary = details.querySelector('summary');

        const data = {
            Eventtype : 'details-click'
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
