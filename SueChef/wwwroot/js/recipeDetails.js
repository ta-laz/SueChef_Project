//Saving all the IDs to variables so it knows where to go
document.addEventListener("DOMContentLoaded", () => {

const ingredientsBtn = document.getElementById('ingredientsButton');
const nutritionBtn = document.getElementById('nutritionButton');
const ingredientsTab = document.getElementById('ingredientsToggle');
const nutritionTab = document.getElementById('nutritionToggle');
const servingInput = document.getElementById('serving');
const baseServings = 4;
const ingredientEls = Array.from(document.querySelectorAll('.ingredient')); //Creates an array of ingredient elements 

servingInput.addEventListener('input', () => {
    let rawValue = servingInput.value.trim(); //Saving the serving input to variable 
    
    // Only try to parse if it’s not empty
    let servings = parseFloat(rawValue);
    
    // If input is empty or invalid, don’t reset yet
    if (!isNaN(servings)) {
        // Limit to between 1 and 12
        if (servings < 1) servings = 1;
        if (servings > 12) servings = 12;
        servingInput.value = servings; // update input only if valid number

        // Update ingredient quantities
        ingredientEls.forEach(el => {
            const baseQty = parseFloat(el.dataset.baseQuantity);
            const newQty = baseQty * servings;
            el.querySelector('.ingredient-quantity').textContent = newQty;
        });
    }
});
servingInput.dispatchEvent(new Event('input')); //Run the script on load so that it starts as 4 

// JS waiting for the button to be clicked 
ingredientsBtn.addEventListener('click', () => {
    ingredientsTab.style.display = 'block'; //Logic so when its clicked it either hides or shows. 
    nutritionTab.style.display = 'none';

    // Logic so that when clicked it is adding or removing styling based on whats active 
    ingredientsBtn.classList.add('bg-orange-700', 'text-white');
    ingredientsBtn.classList.remove('bg-orange-100', 'text-orange-700');

    // This resets the othe button - Logic repeats below so it goes both ways 
    nutritionBtn.classList.remove('bg-orange-700', 'text-white');
    nutritionBtn.classList.add('bg-orange-100', 'text-orange-700');
});

nutritionBtn.addEventListener('click', () => {
    ingredientsTab.style.display = 'none';
    nutritionTab.style.display = 'block';
    nutritionBtn.classList.add('bg-orange-700', 'text-white');
    nutritionBtn.classList.remove('bg-orange-100', 'text-orange-700');
    ingredientsBtn.classList.remove('bg-orange-700', 'text-white');
    ingredientsBtn.classList.add('bg-orange-100', 'text-orange-700');
    //see above notes if unsure 
});
setTimeout(() => { //Short script to make the alert messages fade away after a few seconds
    document.querySelectorAll('.alert-message').forEach(el => {
        el.style.transition = 'opacity 1s ease';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 1000);
    });
}, 4000);

const dropdownButton = document.getElementById("dropdownButton");  //Dropdown for add to meal plan on recipe page 
const dropdownMenu = document.getElementById("dropdownMenu");
const favouriteButton = document.getElementById("favourite_button"); //Favourite button
const saveMealplanButton = document.getElementById("save_mealplan_button"); //Hidden Save to Meal Plans button

if (dropdownButton && dropdownMenu) {
    // Toggle dropdown on button click
    dropdownButton.addEventListener("click", (e) => {
        e.stopPropagation();
        dropdownMenu.classList.toggle("hidden");
    

    // Toggle the buttons based on dropdown visibility
        if (dropdownMenu.classList.contains("hidden")) {
            // Dropdown closed → show Favourite button
            favouriteButton.classList.remove("hidden");
            saveMealplanButton.classList.add("hidden");
        } else {
            // Dropdown open → show Save button
            favouriteButton.classList.add("hidden");
            saveMealplanButton.classList.remove("hidden");
        }
    });

    // Prevent clicks inside the menu from closing it
    dropdownMenu.addEventListener("click", (e) => {
        e.stopPropagation(); 
    });

    // Close dropdown if clicking anywhere else
    document.addEventListener("click", (event) => {
        const clickedInsideDropdown = dropdownMenu.contains(event.target);
        const clickedDropdownButton = dropdownButton.contains(event.target);

        // If click is outside both menu and button → close + reset
        if (!clickedInsideDropdown && !clickedDropdownButton) {
            if (!dropdownMenu.classList.contains("hidden")) {
                dropdownMenu.classList.add("hidden");
            }

            // Reset button states
            favouriteButton.classList.remove("hidden");
            saveMealplanButton.classList.add("hidden");
        }
    });
}



});

