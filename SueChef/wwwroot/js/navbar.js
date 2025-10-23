document.addEventListener('DOMContentLoaded', () => {
    // --- Search ---
    const searchButton = document.getElementById('searchButton');
    const mobileSearch = document.getElementById('mobileSearch');
    const desktopSearch = document.getElementById('desktopSearch');

    if (searchButton) {
        searchButton.addEventListener('click', (event) => {
            event.stopPropagation();

            if (window.innerWidth < 768) {
                mobileSearch?.classList.toggle('hidden');
            } else {
                desktopSearch?.classList.toggle('w-0');
                desktopSearch?.classList.toggle('w-64');
            }
        });
    }

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
});
