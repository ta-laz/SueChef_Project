
    document.addEventListener('DOMContentLoaded', () => {
        const container = document.getElementById('recipes-container');
        const backToTop = document.getElementById('back-to-top');
        if (!container || !backToTop) return; // guard

        const toggleBtn = () => {
            if (container.scrollTop > 100) {
                backToTop.classList.remove('opacity-0', 'pointer-events-none');
                backToTop.classList.add('opacity-100');
            } else {
                backToTop.classList.add('opacity-0', 'pointer-events-none');
                backToTop.classList.remove('opacity-100');
            }
        };

        container.addEventListener('scroll', toggleBtn, { passive: true });
        toggleBtn(); // initialise state

        backToTop.addEventListener('click', () => {
            container.scrollTo({ top: 0, behavior: 'smooth' });
        });
    });