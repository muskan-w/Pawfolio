// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const mobileNavToggle = document.getElementById("mobileNavToggle");
const mobileNavLinks = document.getElementById("navbarSupportedContent");

mobileNavToggle?.addEventListener("click", () => {
    if (window.matchMedia("(max-width: 576px)").matches && mobileNavLinks) {
        const isOpen = mobileNavLinks.classList.toggle("is-open");
        mobileNavToggle.setAttribute("aria-expanded", isOpen.toString());
    }
});
