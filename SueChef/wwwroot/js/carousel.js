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
        return card ? card.offsetWidth + 24 : 0; // add small gap
    }

    // Clone first and last few cards for seamless looping
    const cards = Array.from(track.children);
    const visible = getVisibleCards();

    cards.slice(-visible).forEach(card => {
        const clone = card.cloneNode(true);
        track.insertBefore(clone, track.firstChild);
    });
    cards.slice(0, visible).forEach(card => {
        const clone = card.cloneNode(true);
        track.appendChild(clone);
    });

    // Start at the "real" first card
    const cardWidth = getCardWidth();
    track.scrollLeft = cardWidth * visible;

    function scrollCarousel(direction) {
        const cardWidth = getCardWidth();
        const visibleCards = getVisibleCards();
        const scrollAmount = cardWidth * visibleCards;

        track.scrollBy({ left: direction * scrollAmount, behavior: "smooth" });

        setTimeout(() => {
            if (track.scrollLeft >= cardWidth * (cards.length + visible)) {
                track.scrollLeft = cardWidth * visible;
            } else if (track.scrollLeft <= 0) {
                track.scrollLeft = cardWidth * cards.length;
            }
        }, 600);
    }

    btnRight.addEventListener("click", () => scrollCarousel(1));
    btnLeft.addEventListener("click", () => scrollCarousel(-1));

    window.addEventListener("resize", () => {
        track.scrollTo({ left: cardWidth * visible, behavior: "auto" });
    });
}

// Automatically initialize any carousel container found
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-carousel]").forEach(container => {
        const id = container.id;
        initCarousel(id);
    });
});
