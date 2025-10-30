setTimeout(() => { //Short script to make the alert messages fade away after a few seconds
        document.querySelectorAll('.alert-message').forEach(el => {
            el.style.transition = 'opacity 1s ease';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 1000);
        });
    }, 4000);