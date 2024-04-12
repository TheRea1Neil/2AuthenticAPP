document.addEventListener('DOMContentLoaded', (event) => {
    var acceptButton = document.getElementById('acceptCookies');

    acceptButton.addEventListener('click', function () {
        // Existing code to set the cookie and hide the consent element.
        document.cookie = "cookieConsent=true; max-age=86400; path=/;";
        document.getElementById('cookieConsentContainer').style.display = 'none';
    });

    // Add your search functionality here
    var searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('keyup', function () {
            searchProducts();
        });
    }
});
