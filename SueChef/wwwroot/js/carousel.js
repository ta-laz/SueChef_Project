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
        return card ? card.offsetWidth + 24 : 0; // 24px = typical gap
    }

    let cardWidth = getCardWidth();

    function scrollCarousel(direction) {
        const cards = track.querySelectorAll(".carousel-card");
        const totalCards = cards.length;
        const cardWidth = getCardWidth();

        // Find which card is currently closest to the left edge
        const currentIndex = Math.round(track.scrollLeft / cardWidth);

        // Determine the new card index
        let newIndex = currentIndex + direction * Math.floor(getVisibleCards());

        // Clamp to valid range
        if (newIndex < 0) newIndex = 0;
        if (newIndex > totalCards - Math.floor(getVisibleCards())) {
            newIndex = totalCards - Math.floor(getVisibleCards());
        }

        // Calculate exact scroll position so a card aligns flush left
        const newScroll = newIndex * cardWidth;

        // Smooth scroll to the new position
        track.scrollTo({ left: newScroll, behavior: "smooth" });
    }



    btnRight.addEventListener("click", () => scrollCarousel(1));
    btnLeft.addEventListener("click", () => scrollCarousel(-1));

    window.addEventListener("resize", () => {
        cardWidth = getCardWidth();
    });
}

// ✅ Auto-init all carousels
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-carousel]").forEach(container => {
        initCarousel(container.id);
    });
});

