if ("serviceWorker" in navigator) {
    window.addEventListener("load", function () {
        // Use vanilla JavaScript to avoid jQuery dependency
        var divLoading = document.getElementById('divLoading');
        if (divLoading) {
            divLoading.style.display = 'none';
        }
        
        navigator.serviceWorker
            .register("/Content/PWA/serviceWorker.js")
            .then(res => {
            })
            .catch(err => console.log("service worker not registered", err))
    })

    // Use vanilla JavaScript for beforeunload handler
    window.addEventListener('beforeunload', function () {
        const isInStandaloneMode = () =>
            (window.matchMedia('(display-mode: standalone)').matches) || (window.navigator.standalone) || document.referrer.includes('android-app://');

        if (isInStandaloneMode()) {
            var divLoading = document.getElementById('divLoading');
            if (divLoading) {
                divLoading.style.display = 'block';
            }
        }
    });
}


