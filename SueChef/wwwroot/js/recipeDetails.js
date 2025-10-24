//Saving all the IDs to variables so it knows where to go
document.addEventListener("DOMContentLoaded", () => {
    
const ingredientsBtn = document.getElementById('ingredientsButton');
const nutritionBtn = document.getElementById('nutritionButton');
const ingredientsTab = document.getElementById('ingredientsToggle');
const nutritionTab = document.getElementById('nutritionToggle');
const servingInput = document.getElementById('serving');
const baseServings = 4;
const ingredientEls = Array.from(document.querySelectorAll('.ingredient')); //Creates an array of ingredient elements 

servingInput.addEventListener('input', () => { //When input changes execute the following
    let servings = parseFloat(servingInput.value) || baseServings; //Turns the value into a float and defaults to base servings if not filled in. 
    if (servings < 1) servings = 1;
    if (servings > 12) servings = 12;
    servingInput.value = servings;


    ingredientEls.forEach(el => { //Logic to take the quantity and multiply it by the servings, to scale 
        const baseQty = parseFloat(el.dataset.baseQuantity);
        const newQty = baseQty * servings;
        el.querySelector('.ingredient-quantity').textContent = newQty; //Find the .ingredient-quantity class and update it to the new amount 
    });
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

    const dropdownButton = document.getElementById("dropdownButton");
    const dropdownMenu = document.getElementById("dropdownMenu");

    if (dropdownButton && dropdownMenu) {
        dropdownButton.addEventListener("click", (e) => {
            e.stopPropagation();
            dropdownMenu.classList.toggle("hidden");
        });

        document.addEventListener("click", () => {
            if (!dropdownMenu.classList.contains("hidden")) {
                dropdownMenu.classList.add("hidden");
            }
        });

        dropdownMenu.querySelectorAll("button").forEach((option) => {
            option.addEventListener("click", () => {
                dropdownMenu.classList.add("hidden");
            });
        });
    }
});

