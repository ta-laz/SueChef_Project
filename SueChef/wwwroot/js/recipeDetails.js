//Saving all the IDs to variables so it knows where to go
document.addEventListener("DOMContentLoaded", () => {

    const ingredientsBtn = document.getElementById('ingredientsButton');
    const nutritionBtn = document.getElementById('nutritionButton');
    const ingredientsTab = document.getElementById('ingredientsToggle');
    const nutritionTab = document.getElementById('nutritionToggle');
    const servingInput = document.getElementById('serving');
    const baseServings = 4;
    const ingredientEls = Array.from(document.querySelectorAll('.ingredient')); //Creates an array of ingredient elements 
    const createBtn = document.getElementById("showNewPlanForm");
    const form = document.getElementById("newMealPlanForm");
    const input = document.getElementById("newMealPlanTitle");
    const errorMsg = document.getElementById("newMealPlanError");
    const saveBtn = document.getElementById("saveNewMealPlan");
    const dropdownButton = document.getElementById("dropdownButton");  //Dropdown for add to meal plan on recipe page 
    const dropdownMenu = document.getElementById("dropdownMenu");
    const favouriteButton = document.getElementById("favourite_button"); //Favourite button
    const saveMealplanButton = document.getElementById("save_mealplan_button"); //Hidden Save to Meal Plans button
    const saveMealPlanForm = document.getElementById("saveMealPlanForm");
    const isLoggedIn = dropdownButton.dataset.loggedIn === "true";
    const mealPlansList = document.getElementById("mealPlansList");

    // HELP FUNCTION to show alerts and ensure only one is visible at a time
    function showAlert(alertId) {
        // Hide all other alerts first
        const alerts = document.querySelectorAll("#loginError, #loginError2, #loginError3, #noMealPlansError, #noMealPlansSelectedError");
        alerts.forEach(a => {
            if (a.id !== alertId) {
                a.classList.add("hidden");
                a.style.opacity = "1"; // reset opacity
            }
        });

        const alertBox = document.getElementById(alertId);
        alertBox.classList.remove("hidden");
        alertBox.style.transition = 'opacity 1s ease';
        alertBox.style.opacity = '1';
        
        // Fade after 3s
        setTimeout(() => {
            alertBox.style.opacity = '0';
            setTimeout(() => {
                alertBox.classList.add("hidden");
                    alertBox.style.opacity = '1'; // reset for next time
                }, 1000);
            }, 3000);
        }
    
    
    // HELPER FUNCTION to hide all alerts if the + or dropdown buttons are clicked:
    function hideAllAlerts() {
    document.querySelectorAll('.alert-message, #loginError, #loginError2, #loginError2, #noMealPlansError, #noMealPlansSelectedError')
        .forEach(el => {
            el.classList.add('hidden');
            el.style.opacity = '1';
        });
    }

    // HELPER FUNCTION to only show scroll bar when needed:
    function updateScrollbarClass() {
    if (dropdownMenu.scrollHeight > dropdownMenu.clientHeight) {
        dropdownMenu.classList.add("scrollbar-visible");
        } else {
        dropdownMenu.classList.remove("scrollbar-visible");
        }
    }

    // Run on page load
    updateScrollbarClass();

    // Also run when the dropdown is shown (so it can recalc if content changes)
    dropdownButton.addEventListener("click", () => {
        setTimeout(updateScrollbarClass, 50); // slight delay to ensure content is rendered
    });


    // SERVINGS:
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

    
    // NUTRITION:
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
    }, 6000);



    // DROPDOWN BUTTON:
    if (dropdownButton && dropdownMenu) {
        // Toggle dropdown on button click
        dropdownButton.addEventListener("click", (e) => {
            e.stopPropagation();
            hideAllAlerts();

            // If a user is not signed in:
            if (!isLoggedIn) {
                showAlert("loginError"); // Show dropdown login alert
                return; // Stop here, do NOT open dropdown
            }

            // If user has no meal plans:
            if (mealPlansList.children.length === 0) {
                showAlert("noMealPlansError"); // Show the new alert
                return; // Stop here — do not open dropdown
            }
            
            // Hide the New Meal Plan form if it's open:
            if (form && !form.classList.contains("hidden")) {
                form.classList.add("hidden");
            }
            dropdownMenu.classList.toggle("hidden");
            
        
            // Toggle the fav/save buttons based on dropdown visibility
            if (dropdownMenu.classList.contains("hidden")) {
                // Dropdown closed -> show Favourite button
                favouriteButton.classList.remove("hidden");
                saveMealplanButton.classList.add("hidden");
            } else {
                // Dropdown open -> show Save button
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
        

    // FAVOURITE BUTTON:
    // Get the parent form for favourite button
    const favouriteForm = favouriteButton.closest("form"); 

    // Logic for sending form with favourite button:
    favouriteButton.addEventListener("click", (e) => {
        e.preventDefault();
        e.stopPropagation();

        // If a user is not signed in:
        if (!isLoggedIn) {
            showAlert("loginError2"); // Show favourite login alert
            return; // Stop here, do NOT submit
        }

        // User is logged in → submit the form
        favouriteForm.submit();
    });
}


    // SAVE NEW MEAL PLAN BUTTON: 
    saveMealplanButton.addEventListener("click", (e) => {
        e.preventDefault();
        e.stopPropagation();

        if (!isLoggedIn) {
            showAlert("loginError");
            return;
        }

        // Ensure at least one checkbox is selected
        const selectedPlans = saveMealPlanForm.querySelectorAll("input[name='mealPlanIds']:checked");
        if (selectedPlans.length === 0) {
            showAlert("noMealPlansSelectedError");
            dropdownMenu.classList.add("hidden");
            saveMealplanButton.classList.add("hidden");
            favouriteButton.classList.remove("hidden");
            return;
        }

        // Submit form
        saveMealPlanForm.submit();
    });



// CREATE NEW MEAL PLAN BUTTON (+):
if (createBtn && form && input && saveBtn) {
    
    // Listen for clicking the + button:
    createBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        hideAllAlerts();

        // If a user is not signed in:
        if (!isLoggedIn) {
            showAlert("loginError3"); // Show + login alert
            return; // Stop here, do NOT open dropdown
        }
        
        // Hide the Dropdown menu if it's open:
        if (dropdownMenu && !dropdownMenu.classList.contains("hidden")) {
            dropdownMenu.classList.add("hidden");
            }
        if (saveMealplanButton && !saveMealplanButton.classList.contains("hidden")) {
            saveMealplanButton.classList.add("hidden");
            favouriteButton.classList.remove("hidden");
            }
        // Toggle the visibility of the form:
        form.classList.toggle("hidden");
        input.focus();
    });

    // Hide form when clicking outside dropdown
    document.addEventListener("click", (e) => {
        if (!form.contains(e.target) && e.target !== dropdownButton) {
            form.classList.add("hidden");
            createBtn.classList.remove("hidden");
            errorMsg.classList.add("hidden");
            input.value = "";
        }
    });

    // Get the parent form for favourite button
    const newMealPlanForm = saveBtn.closest("form");

    // Logic for sending form with favourite button:
        saveBtn.addEventListener("click", (e) => {
            e.preventDefault();
            e.stopPropagation();
            newMealPlanForm.submit();
        });

    }
});


