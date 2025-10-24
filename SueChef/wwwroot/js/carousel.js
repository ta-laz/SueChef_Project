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
    let originalCards = Array.from(track.children);
    const visible = getVisibleCards();

    // Avoid double cloning if reinitialized
    if (!track.dataset.cloned) {
        // Clone last 'visible' cards to the start
        originalCards.slice(-visible).forEach(card => {
            const clone = card.cloneNode(true);
            clone.classList.add("clone");
            track.insertBefore(clone, track.firstChild);
        });

        // Clone first 'visible' cards to the end
        originalCards.slice(0, visible).forEach(card => {
            const clone = card.cloneNode(true);
            clone.classList.add("clone");
            track.appendChild(clone);
        });

        track.dataset.cloned = "true";
    }

    // Update references
    const allCards = Array.from(track.children);
    const totalCards = allCards.length;
    const realCards = totalCards - visible * 2;
    const cardWidth = getCardWidth();

    // Start at the first *real* card
    track.scrollLeft = cardWidth * visible;

    // --- Smooth scroll on button click ---
    function scrollCarousel(direction) {
        const scrollAmount = cardWidth * Math.floor(getVisibleCards());
        track.scrollBy({ left: direction * scrollAmount, behavior: "smooth" });
    }

    btnRight.addEventListener("click", () => scrollCarousel(1));
    btnLeft.addEventListener("click", () => scrollCarousel(-1));

    // --- Infinite looping logic ---
    track.addEventListener("scroll", () => {
        const maxScroll = cardWidth * (visible + realCards);
        const minScroll = 0;

        // If scrolled past last real card, jump back to start
        if (track.scrollLeft >= maxScroll) {
            track.scrollLeft = cardWidth * visible;
        }
        // If scrolled before first real card, jump to end
        else if (track.scrollLeft <= minScroll) {
            track.scrollLeft = cardWidth * (visible + realCards - 1);
        }
    });

    // --- Handle resize dynamically ---
    window.addEventListener("resize", () => {
        const newCardWidth = getCardWidth();
        track.scrollLeft = newCardWidth * visible;
    });
}

// --- Auto-init all carousels ---
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-carousel]").forEach(container => {
        initCarousel(container.id);
    });
});
