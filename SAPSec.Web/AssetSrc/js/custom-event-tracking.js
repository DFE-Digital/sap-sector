(function () {
    document.addEventListener('click', function (event) {
        const link = event.target.closest('a');
        if (!link) return;

            const data = {
                Url: link.href,
                Text: link.innerText.trim()
            };

            fetch('/custom-event-tracking', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
    });
})();
