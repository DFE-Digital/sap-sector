(function () {
   
    // document.addEventListener("click", function (e) {
    //     const toggleLink = e.target.closest("#toggleViewLink");
    //     if (toggleLink) {
    //         e.preventDefault();
    //         const isList = toggleLink.dataset.view === "list";
    //         if (isList) showMap();
    //         else showList();
    //         return;
    //     }

    //     const toggleToList = e.target.closest("#toggleToListLink");
    //     if (toggleToList) {
    //         e.preventDefault();
    //         showList();
    //     }
    // });

    document.addEventListener('click', function (event) {
        const link = event.target.closest('a');
        if (!link) return;

        //if (link.href.startsWith('http')) {
           // const linkUrl = btoa(link.href); // Base64 encoding avoids path slash conflicts
            //const linkText = encodeURIComponent(link.innerText.trim() || "empty");

            const data = {
                Url: link.href,
                Text: link.innerText.trim()
            };

            fetch('/tracking', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
       // }

    });

})();
