
    // function toggleMealPlan(button) {
    //     const heartOutline = button.querySelector('.plus-icon');
    //     const heartfilled = button.querySelector('.tick-icon');
    //     const isActive = button.classList.contains('bg-rose-800');

    //     if (isActive) {
    //         // Back to plus
    //         button.classList.remove('bg-rose-800');
    //         button.classList.add('bg-orange-500');
    //         heartfilled.classList.add('hidden');
    //         heartOutline.classList.remove('hidden');
    //     } else {
    //         // Change to tick
    //         button.classList.remove('bg-orange-500');
    //         button.classList.add('bg-rose-800');
    //         heartOutline.classList.add('hidden');
    //         heartfilled.classList.remove('hidden');
    //     }
    // }

function getAntiForgeryToken() {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
}

async function toggleFavourite(button) {
    if (button.tagName !== 'BUTTON') {
        button = button.closest('button');
        if (!button) return;
    }

    var recipeId = button.getAttribute('data-recipe-id');
    var isFavourite = button.getAttribute('data-is-favourite') === 'true';
    var heartOutline = button.querySelector('.heart-outline');
    var heartfilled = button.querySelector('.heart-filled');
    var token = getAntiForgeryToken();

    // Optimistically update UI
    isFavourite = !isFavourite;
    button.setAttribute('data-is-favourite', String(isFavourite));
    button.setAttribute('aria-pressed', String(isFavourite));

    if (isFavourite) {
        heartOutline.classList.add('hidden');
        heartfilled.classList.remove('hidden');
    } else {
        heartfilled.classList.add('hidden');
        heartOutline.classList.remove('hidden');
    }

    var formData = new URLSearchParams();
    formData.append('recipeId', recipeId);

    try {
        var resp = await fetch('/Favourites/ToggleAjax', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        var result = await resp.json();

        if (resp.status === 401) {
            // User not signed in: revert UI
            isFavourite = !isFavourite; // revert
            button.setAttribute('data-is-favourite', String(isFavourite));
            button.setAttribute('aria-pressed', String(isFavourite));

            if (isFavourite) {
                heartOutline.classList.add('hidden');
                heartfilled.classList.remove('hidden');
            } else {
                heartfilled.classList.add('hidden');
                heartOutline.classList.remove('hidden');
            }

            // Show inline message
            showFavouriteMessage("You must be signed in to favourite recipes.");
            return;
        }

        if (!result.success) {
            throw new Error(result.message || 'Failed to update favourites.');
        }

        // Sync UI with server response (in case optimistic update differs)
        isFavourite = result.isFavourite;
        button.setAttribute('data-is-favourite', String(isFavourite));
        button.setAttribute('aria-pressed', String(isFavourite));

        if (isFavourite) {
            heartOutline.classList.add('hidden');
            heartfilled.classList.remove('hidden');
        } else {
            heartfilled.classList.add('hidden');
            heartOutline.classList.remove('hidden');
        }

    } catch (err) {
        console.error(err);

        // Revert UI on error
        isFavourite = !isFavourite;
        button.setAttribute('data-is-favourite', String(isFavourite));
        button.setAttribute('aria-pressed', String(isFavourite));

        if (isFavourite) {
            heartOutline.classList.add('hidden');
            heartfilled.classList.remove('hidden');
        } else {
            heartfilled.classList.add('hidden');
            heartOutline.classList.remove('hidden');
        }

        showFavouriteMessage(err.message || 'Failed to update favourites. Please try again.');
    }
}

// Helper to show a error messages
function showFavouriteMessage(message) {
    const box = document.getElementById("floatingMessage");
    box.textContent = message;
    box.classList.remove("hidden");

    // Fade after 3s
    setTimeout(() => {
        box.style.opacity = '0';
        setTimeout(() => {
                box.classList.add("hidden");
                box.style.opacity = '1'; // reset for next time so that when you click again it does the same thing
            }, 500);
    }, 3000);
}
