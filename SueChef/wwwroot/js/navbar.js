document.addEventListener('DOMContentLoaded', function () {
    const searchButton = document.getElementById('searchButton');
    const mobileSearch = document.getElementById('mobileSearch');
    const desktopSearch = document.getElementById('desktopSearch');

    if (searchButton) {
        searchButton.addEventListener('click', (event) => {
            event.stopPropagation(); // prevent immediate closing when clicking the button

            if (window.innerWidth < 768) {
                // MOBILE toggle
                mobileSearch?.classList.toggle('hidden');
            } else {
                // DESKTOP toggle (expand/collapse width)
                desktopSearch?.classList.toggle('w-0');
                desktopSearch?.classList.toggle('w-64');
            }
        });
    }

    // Click anywhere outside search to close
    document.addEventListener('click', (event) => {
        const isClickInsideSearch =
            searchButton.contains(event.target) ||
            mobileSearch?.contains(event.target) ||
            desktopSearch?.contains(event.target);

        if (!isClickInsideSearch) {
            // Hide mobile search
            if (mobileSearch && !mobileSearch.classList.contains('hidden')) {
                mobileSearch.classList.add('hidden');
            }
            // Collapse desktop search
            if (desktopSearch && desktopSearch.classList.contains('w-64')) {
                desktopSearch.classList.remove('w-64');
                desktopSearch.classList.add('w-0');
            }
        }
    });
});
