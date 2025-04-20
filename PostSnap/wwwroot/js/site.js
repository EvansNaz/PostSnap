// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.




// --------------------------------------------
//BROKEN IMAGE FIX
// --------------------------------------------
/*
    - Removes onerror
    - Replaces the link
    - Disables click & cursor
*/
function handleBrokenImage(img) {
    //  handle the link cleanup; the image fallback is already done inline
    img.onerror = null;//If the placeholder image also fails to load, it prevents recursion
    // Set fallback image
    img.src = '/images/placeholder.png';
    setTimeout(() => {
        const link = img.closest('a');//find the nearest <a> wrapping the image
        if (link) {
            link.removeAttribute("href");
            link.style.cursor = "default";
        }
    }, 1);
}

// still run fallback on page load for edge cases
//backup safety net in case onerror fails
document.addEventListener("DOMContentLoaded", function () {//Ensures the DOM is ready to be accessed/manipulated.
    const imgs = document.querySelectorAll("img");

    //Loops through each image
    imgs.forEach(img => {

        // Assign the fallback handler
        img.onerror = function () {
            handleBrokenImage(this);
        };

        if (img.complete && img.naturalWidth === 0) {
            // Broken image not caught by onerror (e.g. from cache)
            img.src = '/images/placeholder.png';
            handleBrokenImage(img);
        }
    });
});
