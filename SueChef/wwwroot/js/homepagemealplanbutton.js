
    // function toggleMealPlan(button) {
    //     const plusIcon = button.querySelector('.plus-icon');
    //     const tickIcon = button.querySelector('.tick-icon');
    //     const isActive = button.classList.contains('bg-rose-800');

    //     if (isActive) {
    //         // Back to plus
    //         button.classList.remove('bg-rose-800');
    //         button.classList.add('bg-orange-500');
    //         tickIcon.classList.add('hidden');
    //         plusIcon.classList.remove('hidden');
    //     } else {
    //         // Change to tick
    //         button.classList.remove('bg-orange-500');
    //         button.classList.add('bg-rose-800');
    //         plusIcon.classList.add('hidden');
    //         tickIcon.classList.remove('hidden');
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
    var plusIcon = button.querySelector('.plus-icon');
    var tickIcon = button.querySelector('.tick-icon');
    var token = getAntiForgeryToken();

    // Optimistically update UI
    isFavourite = !isFavourite;
    button.setAttribute('data-is-favourite', String(isFavourite));
    button.setAttribute('aria-pressed', String(isFavourite));

    if (isFavourite) {
        plusIcon.classList.add('hidden');
        tickIcon.classList.remove('hidden');
    } else {
        tickIcon.classList.add('hidden');
        plusIcon.classList.remove('hidden');
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
                plusIcon.classList.add('hidden');
                tickIcon.classList.remove('hidden');
            } else {
                tickIcon.classList.add('hidden');
                plusIcon.classList.remove('hidden');
            }

            // Show inline message
            showFavouriteMessage(button, "You must be signed in to favourite recipes.");
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
            plusIcon.classList.add('hidden');
            tickIcon.classList.remove('hidden');
        } else {
            tickIcon.classList.add('hidden');
            plusIcon.classList.remove('hidden');
        }

    } catch (err) {
        console.error(err);

        // Revert UI on error
        isFavourite = !isFavourite;
        button.setAttribute('data-is-favourite', String(isFavourite));
        button.setAttribute('aria-pressed', String(isFavourite));

        if (isFavourite) {
            plusIcon.classList.add('hidden');
            tickIcon.classList.remove('hidden');
        } else {
            tickIcon.classList.add('hidden');
            plusIcon.classList.remove('hidden');
        }

        showFavouriteMessage(button, err.message || 'Failed to update favourites. Please try again.');
    }
}

// Helper to show a small inline message below the button
function showFavouriteMessage(button, message) {
    let msgEl = button.parentElement.querySelector('.favourite-message');
    if (!msgEl) {
        msgEl = document.createElement('div');
        msgEl.className = 'favourite-message text-red-600 text-sm mt-1';
        button.parentElement.appendChild(msgEl);
    }
    msgEl.textContent = message;
    // auto-hide after 5 seconds
    setTimeout(() => { msgEl.textContent = ''; }, 5000);
}
