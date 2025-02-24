// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// I only realized this site.js existed on Saturaday 31/08/2024,
// So I have decided not to go back and add all of the javascript as it could cause more errors.

// adds game to wishlist for the home and store index pages
function addToWishlist(productId) {
    fetch('/Wishlist/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ productId: productId })
    }).then(response => {
        if (response.ok) {
            alert('Added to wishlist!');
        } else {
            alert('Failed to add to wishlist');
        }
    });
}
