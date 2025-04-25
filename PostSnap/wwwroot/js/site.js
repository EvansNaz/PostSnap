
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

// --------------------------------------------
// POST EDIT VIEW – Enable Save Button When Changes Occur
// --------------------------------------------

function initEditPostForm() {
    document.addEventListener("DOMContentLoaded", function () {

        // Grab references to key form elements
        const saveBtn = document.querySelector("#saveChangesBtn");
        const titleInput = document.querySelector("input[name='Title']");
        const bodyInput = document.querySelector("textarea[name='Body']");
        const imageInput = document.querySelector("input[type='file'][name='ImageUpload']");

        // If one of the variables don't exist we leave (Not edit view = leave)
        if (!saveBtn || !titleInput || !bodyInput || !imageInput) {
            return;
        }

        // Store the original trimmed values of the title and body fields to compare later
        const originalTitle = titleInput.value.trim();
        const originalBody = bodyInput.value.trim();

        /**
         * Checks if any field has been modified.
         * If yes, enables the Save Changes button.
         */
        function checkIfPostChanged() {
            const currentTitle = titleInput.value.trim(); // current trimmed title
            const currentBody = bodyInput.value.trim();   // current trimmed body
            const imageChanged = imageInput.files.length > 0; // true if an image was selected

            const titleChanged = currentTitle !== originalTitle;
            const bodyChanged = currentBody !== originalBody;

            // Enable Save Changes button only if there's any change
            saveBtn.disabled = !(titleChanged || bodyChanged || imageChanged);
        }

        // Listen for user input in title, body and image fields
        titleInput.addEventListener("input", checkIfPostChanged);
        bodyInput.addEventListener("input", checkIfPostChanged);
        imageInput.addEventListener("change", checkIfPostChanged);

        // Also recheck when a user leaves (blurs) a field to avoid whitespace-only changes
        titleInput.addEventListener("blur", () => {
            titleInput.value = titleInput.value.trim();
            checkIfPostChanged();// re-check in case trim caused a change
        });

        bodyInput.addEventListener("blur", () => {
            bodyInput.value = bodyInput.value.trim();
            checkIfPostChanged();
        });

        // Trim the values just before the form is submitted (extra safety)
        document.querySelector("form").addEventListener("submit", function () {
            titleInput.value = titleInput.value.trim();
            bodyInput.value = bodyInput.value.trim();
        });

    });
}

/**
* Admin-only: Submits the hard delete form after confirmation.
*/
function submitHardDelete(id) {
    if (confirm('Are you sure you want to permanently delete this?')) {
        document.querySelector(`#hard-delete-form-${id}`).submit();
    }
}

