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


// --------------------------------------------
//  PREVIEW IMAGE WITH JS VALIDATION
// --------------------------------------------

    // Helper function to display error messages with a flash effect
    function showError(message) {
        const errorSpan = document.getElementById('imageError'); // The span for inline error messages

    errorSpan.textContent = message; // Set the error message text

    // Remove the flash class if it was already applied (to reset animation)
    errorSpan.classList.remove('flash-error');

    // Force browser reflow to allow re-triggering the animation
    void errorSpan.offsetWidth;

    // Add the class again to trigger the flash animation
    errorSpan.classList.add('flash-error');
    }

    // Main function triggered when user selects a file
function previewImage(event) {
    const currentImage = document.getElementById('currentImage');
        const input = event.target; // The file input element
    const preview = document.getElementById('imagePreview'); // The image preview element
    const errorSpan = document.getElementById('imageError'); // The span for showing validation errors
    const file = input.files[0]; // Get the selected file

    // Reset preview and error message
    preview.classList.add('d-none'); // Hide preview image
    preview.src = "#"; // Reset preview src
    errorSpan.textContent = ""; // Clear error text
    errorSpan.classList.remove('flash-error'); // Remove previous animation

    if (!file) return; // Exit if no file was selected

    // Define valid extensions and size limit
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif'];
    const maxSize = 2 * 1024 * 1024; // 2 MB

    // Get file extension in lowercase
    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    // Validate extension
    if (!allowedExtensions.includes(fileExtension)) {
        input.value = ""; // Clear invalid file from input
    showError("Only .jpg, .jpeg, .png, and .gif files are allowed."); // Show inline error
    return;
        }

        // Validate file size
        if (file.size > maxSize) {
        input.value = ""; // Clear large file from input
    showError("File size must be less than 2MB."); // Show inline error
    return;
        }
    if (currentImage) {
        currentImage.classList.add('d-none'); // hide old image
    }
    // If valid, read file and show preview
    const reader = new FileReader();
    reader.onload = function (e) {
        preview.src = e.target.result; // Set image source to the uploaded file
    preview.classList.remove('d-none'); // Show the image preview
        };
    reader.readAsDataURL(file); // Convert file to base64 for preview
    }
