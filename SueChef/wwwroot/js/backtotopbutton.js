document.addEventListener('DOMContentLoaded', () => {
    const backToTop = document.getElementById('back-to-top');
    if (!backToTop) return;

    const toggleBtn = () => {
        if (window.scrollY > 200) { // show after 200px of scrolling
            backToTop.classList.remove('opacity-0', 'pointer-events-none');
            backToTop.classList.add('opacity-100');
        } else {
            backToTop.classList.add('opacity-0', 'pointer-events-none');
            backToTop.classList.remove('opacity-100');
        }
    };

    window.addEventListener('scroll', toggleBtn, { passive: true });
    toggleBtn();

    backToTop.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});
