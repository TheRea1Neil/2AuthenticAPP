// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', (event) => {
    var acceptButton = document.getElementById('acceptCookies'); // Make sure this is the correct ID for your accept button

    acceptButton.addEventListener('click', function () {
        // Set the cookie to record the user's acceptance
        document.cookie = "cookieConsent=true; max-age=86400; path=/"; // 86400 seconds = 1 day

        // Hide the cookie consent element
        document.getElementById('cookieConsentContainer').style.display = 'none';
    });
});

