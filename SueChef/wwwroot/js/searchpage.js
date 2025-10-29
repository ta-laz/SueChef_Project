const dropdownBtn = document.getElementById("ingredientDropdown");
const dropdownList = document.getElementById("ingredientList");
const placeholder = document.getElementById("ingredientPlaceholder");

dropdownBtn.addEventListener("click", () => {
    dropdownList.classList.toggle("hidden");
});

dropdownList.addEventListener("change", () => {
    const checked = Array.from(dropdownList.querySelectorAll("input:checked")).map(cb => cb.value);
    placeholder.textContent = checked.length ? checked.join(", ") : "Select ingredients";
});

document.addEventListener("click", (e) => {
    if (!dropdownBtn.contains(e.target) && !dropdownList.contains(e.target)) {
        dropdownList.classList.add("hidden");
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const toggleBtn = document.getElementById("toggleFiltersBtn");
    const filters = document.getElementById("filtersContainer");
    let visible = true;

    toggleBtn.addEventListener("click", () => {
        visible = !visible;

        if (visible) {
            filters.classList.remove("hidden");
            toggleBtn.innerHTML = "Hide filters";
        } else {
            filters.classList.add("hidden");
            toggleBtn.innerHTML = "Show filters";
        }
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const toggleBtn = document.getElementById("toggleFiltersBtn");
    const filters = document.getElementById("filtersContainer");
    if (!toggleBtn || !filters) return;

    let visible = true;
    toggleBtn.addEventListener("click", () => {
        visible = !visible;
        filters.classList.toggle("hidden", !visible);
        toggleBtn.textContent = visible ? "Hide filters" : "Show filters";
    });
});

// --------------------
// GENERIC SINGLE-SELECT DROPDOWNS
// (for category, chef, difficulty, duration)
// --------------------
function setupCustomDropdown(baseId) {
    const dropdownBtn = document.getElementById(`${baseId}Dropdown`);
    const dropdownList = document.getElementById(`${baseId}List`);
    const hiddenInput = document.getElementById(`${baseId}Input`);
    const selectedText = document.getElementById(`${baseId}SelectedText`);

    if (!dropdownBtn || !dropdownList || !hiddenInput || !selectedText) return;

    // Toggle list visibility
    dropdownBtn.addEventListener("click", () => {
        dropdownList.classList.toggle("hidden");
    });

    // Option selection
    dropdownList.addEventListener("click", (e) => {
        const optionBtn = e.target.closest("button[data-value]");
        if (!optionBtn) return;

        const newValue = optionBtn.getAttribute("data-value") || "";
        const newLabel = optionBtn.querySelector("span")?.textContent?.trim() ?? "Select option";

        // Update hidden input and visible text
        hiddenInput.value = newValue;
        selectedText.textContent = newLabel;

        // Close dropdown
        dropdownList.classList.add("hidden");
    });

    // Close dropdown when clicking outside
    document.addEventListener("click", (e) => {
        if (!dropdownBtn.contains(e.target) && !dropdownList.contains(e.target)) {
            dropdownList.classList.add("hidden");
        }
    });
}

// Initialise all your single-select dropdowns
["category", "chef", "difficulty", "duration"].forEach(setupCustomDropdown);