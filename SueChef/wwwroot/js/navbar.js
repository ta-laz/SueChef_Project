document.addEventListener('DOMContentLoaded', () => {
    // --- Search ---
    const searchButton = document.getElementById('searchButton');
    const mobileSearch = document.getElementById('mobileSearch');
    const desktopSearch = document.getElementById('desktopSearch');

    // get their input fields
    const mobileInput = mobileSearch?.querySelector('input');
    const desktopInput = desktopSearch?.querySelector('input');

    if (searchButton) {
        searchButton.addEventListener('click', (event) => {
            event.stopPropagation();

            const isMobile = window.innerWidth < 768;
            const input = isMobile ? mobileInput : desktopInput;
            const container = isMobile ? mobileSearch : desktopSearch;

            if (!input || !container) return;

            const query = input.value.trim();

            // If there's text, perform the search
            if (query !== '') {
                const encoded = encodeURIComponent(query);
                window.location.href = `/search?searchQuery=${encoded}`;
                return;
            }

            // Otherwise toggle visibility
            if (isMobile) {
                container.classList.toggle('hidden');
                if (!container.classList.contains('hidden')) input.focus();
            } else {
                const open = container.classList.contains('w-0');
                container.classList.toggle('w-0', !open);
                container.classList.toggle('w-64', open);
                if (open) input.focus();
            }
        });
    }

    // Close search when clicking outside
    document.addEventListener('click', (event) => {
        const target = event.target;
        const clickInSearch =
            (searchButton && searchButton.contains(target)) ||
            (mobileSearch && mobileSearch.contains(target)) ||
            (desktopSearch && desktopSearch.contains(target));

        if (!clickInSearch) {
            if (mobileSearch && !mobileSearch.classList.contains('hidden')) {
                mobileSearch.classList.add('hidden');
            }
            if (desktopSearch && desktopSearch.classList.contains('w-64')) {
                desktopSearch.classList.remove('w-64');
                desktopSearch.classList.add('w-0');
            }
        }
    });

    // Allow pressing Enter to search
    [mobileInput, desktopInput].forEach((input) => {
        if (!input) return;
        input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = input.value.trim();
                if (query) window.location.href = `/search?searchQuery=${encodeURIComponent(query)}`;
            }
        });
    });


    // --- Side menus & overlay ---
    const menuButton = document.getElementById('menuButton');            // left opener
    const sideMenu = document.getElementById('sideMenu');                // left drawer (starts with -translate-x-full)
    const accountButton = document.getElementById('accountButton');      // right opener
    const sideMenuAccount = document.getElementById('sideMenuAccount');  // right drawer (starts with translate-x-full)
    const overlay = document.getElementById('overlay');                  // shared overlay

    // Safety: bail if overlay not present
    // (Add <div id="overlay" class="hidden fixed inset-0 bg-black/40 z-30"></div> after header)
    const showOverlay = () => overlay && overlay.classList.remove('hidden');
    const hideOverlay = () => overlay && overlay.classList.add('hidden');

    // Helpers to open/close each drawer (and keep only one open at a time)
    const openLeft = () => {
        if (!sideMenu) return;
        sideMenu.classList.remove('-translate-x-full');
        if (sideMenuAccount) sideMenuAccount.classList.add('translate-x-full'); // ensure right is closed
        showOverlay();
    };

    const closeLeft = () => sideMenu && sideMenu.classList.add('-translate-x-full');

    const openRight = () => {
        if (!sideMenuAccount) return;
        sideMenuAccount.classList.remove('translate-x-full');
        if (sideMenu) sideMenu.classList.add('-translate-x-full'); // ensure left is closed
        showOverlay();
    };

    const closeRight = () => sideMenuAccount && sideMenuAccount.classList.add('translate-x-full');

    // Button clicks
    menuButton && menuButton.addEventListener('click', () => {
        const isClosed = sideMenu?.classList.contains('-translate-x-full');
        if (isClosed) openLeft(); else { closeLeft(); hideOverlay(); }
    });

    accountButton && accountButton.addEventListener('click', () => {
        const isClosed = sideMenuAccount?.classList.contains('translate-x-full');
        if (isClosed) openRight(); else { closeRight(); hideOverlay(); }
    });

    // Overlay click closes whichever is open
    overlay && overlay.addEventListener('click', () => {
        closeLeft();
        closeRight();
        hideOverlay();
    });
    // Close left/right menus when clicking outside of them (safety net)
    document.addEventListener('click', (event) => {
        const target = event.target;

        const clickInMenu =
            (sideMenu && sideMenu.contains(target)) ||
            (menuButton && menuButton.contains(target)) ||
            (sideMenuAccount && sideMenuAccount.contains(target)) ||
            (accountButton && accountButton.contains(target));

        if (!clickInMenu) {
            // Close both if open
            if (sideMenu && !sideMenu.classList.contains('-translate-x-full')) {
                closeLeft();
                hideOverlay();
            }
            if (sideMenuAccount && !sideMenuAccount.classList.contains('translate-x-full')) {
                closeRight();
                hideOverlay();
            }
        }
    });
});
