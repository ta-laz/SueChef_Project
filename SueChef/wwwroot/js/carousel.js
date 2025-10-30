// wwwroot/js/carousel.js
function initCarousel(carouselId) {
    const container = document.getElementById(carouselId);
    if (!container) return;

    const track = container.querySelector(".carousel-track");
    const btnLeft = container.querySelector(".carousel-left");
    const btnRight = container.querySelector(".carousel-right");

    function getVisibleCards() {
        if (window.innerWidth < 640) return 1;
        if (window.innerWidth < 768) return 1.5;
        if (window.innerWidth < 1024) return 3;
        return 4;
    }

    function getCardWidth() {
        const card = track.querySelector(".carousel-card");
        return card ? card.offsetWidth + 24 : 0; // +24 for the gap
    }

    // --- Clone cards for infinite loop ---
    const originalCards = Array.from(track.children);
    const visible = Math.ceil(getVisibleCards()); // round up to fill space

    if (!track.dataset.cloned) {
    const visible = Math.ceil(getVisibleCards());
    const originalCards = Array.from(track.children);

    // Clone last N to the start
    originalCards.slice(-visible).forEach(card => {
        const clone = card.cloneNode(false); // shallow clone
        clone.innerHTML = card.innerHTML;    // just the visuals
        clone.classList.add("clone", "pointer-events-none"); // prevent click
        track.insertBefore(clone, track.firstChild);
    });

    // Clone first N+1 to the end
    originalCards.slice(0, visible + 1).forEach(card => {
        const clone = card.cloneNode(false);
        clone.innerHTML = card.innerHTML;
        clone.classList.add("clone", "pointer-events-none");
        track.appendChild(clone);
    });

    track.dataset.cloned = "true";
}

    const cardWidth = getCardWidth();
    const allCards = Array.from(track.children);
    const totalCards = allCards.length;
    const realCards = totalCards - (visible * 2 + 1);

    const startScroll = cardWidth * visible;

    // ✅ Wait for DOM layout before initial scroll
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            track.scrollLeft = startScroll;
        });
    });

    // --- Scroll handler for infinite wrap ---
    function handleScroll() {
        const endScroll = cardWidth * (visible + realCards);
        if (track.scrollLeft >= endScroll) {
            // Snap instantly back to start clones (seamless)
            track.scrollLeft = startScroll;
        } else if (track.scrollLeft <= 0) {
            // If scrolled too far left, jump to real end
            track.scrollLeft = endScroll - cardWidth;
        }
    }

    track.addEventListener("scroll", handleScroll);

    // --- Smooth scroll on button click ---
    function scrollCarousel(direction) {
        const scrollAmount = cardWidth * Math.floor(getVisibleCards());
        const endScroll = cardWidth * (visible + realCards);
        const targetScroll = track.scrollLeft + direction * scrollAmount;

        if (direction > 0 && targetScroll >= endScroll) {
            // Smoothly scroll to first clones
            track.scrollTo({ left: startScroll, behavior: "smooth" });
            return;
        }
        if (direction < 0 && targetScroll <= 0) {
            // Smoothly scroll to end clones
            track.scrollTo({ left: endScroll - cardWidth, behavior: "smooth" });
            return;
        }

        track.scrollBy({ left: direction * scrollAmount, behavior: "smooth" });
    }

    btnRight.addEventListener("click", () => scrollCarousel(1));
    btnLeft.addEventListener("click", () => scrollCarousel(-1));
}

// ✅ Auto-init all carousels when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-carousel]").forEach(container => {
        initCarousel(container.id);
    });
});
