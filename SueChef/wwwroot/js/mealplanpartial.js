// script for the three dots menu

function toggleMenu(button) {
    const menu = button.nextElementSibling;
    document.querySelectorAll('.group .absolute.right-0').forEach(m => {
        if (m !== menu) m.classList.add('hidden');
    });
    menu.classList.toggle('hidden');
}

document.addEventListener('click', (e) => {
    if (!e.target.closest('.group')) {
        document.querySelectorAll('.group .absolute.right-0').forEach(m => m.classList.add('hidden'));
    }
});

// script for the edit form to appear

function openEditForm(id, currentTitle) {
    const modal = document.getElementById(`editModal-${id}`);
    const input = document.getElementById(`edit-mealplan-input-${id}`);
    modal.classList.remove('hidden');
    input.value = currentTitle; // prefill with current title
    input.focus();
}

function closeEditForm(id) {
    const modal = document.getElementById(`editModal-${id}`);
    modal.classList.add('hidden');
}

document.addEventListener('click', (e) => {
    const modal = e.target.closest('[id^="editModal-"]');
    if (modal && e.target === modal) {
        modal.classList.add('hidden');
    }
});