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