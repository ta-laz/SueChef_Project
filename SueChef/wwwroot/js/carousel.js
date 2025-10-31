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
    const visibleCards = Math.floor(getVisibleCards());
    const cardWidth = getCardWidth();

    // The maximum scroll position (no white space beyond last card)
    const maxScroll = Math.max(0, (totalCards * cardWidth) - track.clientWidth);

    // Calculate the proposed scroll amount
    let newScroll = track.scrollLeft + (direction * cardWidth * visibleCards);

    // Clamp to valid range
    if (newScroll < 0) newScroll = 0;
    if (newScroll > maxScroll) newScroll = maxScroll;

    // Smooth scroll
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

