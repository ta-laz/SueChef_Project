//Saving all the IDs to variables so it knows where to go
document.addEventListener("DOMContentLoaded", () => {

    const ingredientsBtn = document.getElementById('ingredientsButton');
    const nutritionBtn = document.getElementById('nutritionButton');
    const ingredientsTab = document.getElementById('ingredientsToggle');
    const nutritionTab = document.getElementById('nutritionToggle');
    const servingInput = document.getElementById('serving');
    const baseServings = 4;
    const ingredientEls = Array.from(document.querySelectorAll('.ingredient')); //Creates an array of ingredient elements 

    // Helper function to show alerts and ensure only one is visible at a time
    function showAlert(alertId) {
        // Hide all other alerts first
        const alerts = document.querySelectorAll("#loginError, #loginError2");
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
    }, 6000);

    const dropdownButton = document.getElementById("dropdownButton");  //Dropdown for add to meal plan on recipe page 
    const dropdownMenu = document.getElementById("dropdownMenu");
    const favouriteButton = document.getElementById("favourite_button"); //Favourite button
    const saveMealplanButton = document.getElementById("save_mealplan_button"); //Hidden Save to Meal Plans button
    const isLoggedIn = dropdownButton.dataset.loggedIn === "true";

    if (dropdownButton && dropdownMenu) {
        // Toggle dropdown on button click
        dropdownButton.addEventListener("click", (e) => {
            e.stopPropagation();

            // If a user is not signed in:
            if (!isLoggedIn) {
                showAlert("loginError"); // Show dropdown login alert
                return; // Stop here, do NOT open dropdown
            }

            // If user has no meal plans:
            if (dropdownMenu.children.length === 0) {
                showAlert("noMealPlansError"); // Show the new alert
                return; // Stop here — do not open dropdown
            }

            dropdownMenu.classList.toggle("hidden");
        
            // Toggle the buttons based on dropdown visibility
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

    const createBtn = document.getElementById("showNewPlanForm");
    const form = document.getElementById("newMealPlanForm");
    const input = document.getElementById("newMealPlanTitle");
    const errorMsg = document.getElementById("newMealPlanError");
    const dropdown = document.getElementById("dropdownMenu");
    
    if (createBtn && form && input) {
    // --- Show the input form ---
    createBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        createBtn.classList.add("hidden");
        form.classList.remove("hidden");
        input.focus();
    });
    
    // --- Hide form when clicking outside dropdown ---
    document.addEventListener("click", (e) => {
        if (!dropdown.contains(e.target) && e.target !== dropdownButton) {
            form.classList.add("hidden");
            createBtn.classList.remove("hidden");
            errorMsg.classList.add("hidden");
            input.value = "";
        }
    });
    
    // --- Submit handler for creating new meal plan ---
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const title = input.value.trim();
        errorMsg.classList.add("hidden");
    
        if (!title) {
            errorMsg.textContent = "Please enter a name.";
            errorMsg.classList.remove("hidden");
            return;
        }
    
    
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    
        try {
            const response = await fetch("/MealPlans/CreateInline", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `mealPlanTitle=${encodeURIComponent(title)}&__RequestVerificationToken=${token}`
    });

    // If server returned a non-2xx status, read body for diagnostics and throw
    if (!response.ok) {
        const text = await response.text();
        console.error("CreateInline returned non-OK status:", response.status, text);
        // show server message if it's short, otherwise generic
        errorMsg.textContent = text && text.length < 200 ? text : "Server error creating meal plan.";
        errorMsg.classList.remove("hidden");
        return;
    }

    // Try parse JSON (guard in case server returned HTML or empty)
    let data;
    try {
        data = await response.json();
    } catch (parseErr) {
        const text = await response.text(); // raw response to inspect
        console.error("Failed to parse JSON from CreateInline response. Raw response:", text);
        errorMsg.textContent = "Unexpected response from server.";
        errorMsg.classList.remove("hidden");
        return;
    }

    // Validate shape of response
    if (!data || data.success !== true || !Array.isArray(data.mealPlans)) {
        console.error("CreateInline returned unexpected JSON:", data);
        // If the endpoint returns a single item (older version), support that too:
        if (data && data.success === true && data.id && data.title) {
            // fallback: insert single returned plan (backwards compatible)
            const createNewItem = document.getElementById("createNewMealPlan");
            const newItem = document.createElement("li");
            newItem.className = "flex items-center px-3 py-2 border-b border-orange-200 last:border-b-0";
            newItem.innerHTML = `
                <label class="flex items-center gap-2 cursor-pointer w-full">
                    <input type="checkbox" name="mealPlanIds" value="${data.id}" class="hidden peer" checked />
                    <span class="w-4 h-4 rounded-full border-2 border-orange-700 peer-checked:bg-orange-700 transition"></span>
                    <span class="text-gray-700 text-sm truncate max-w-[80%]">${data.title}</span>
                </label>
            `;
            dropdown.insertBefore(newItem, createNewItem.nextSibling);

            input.value = "";
            form.classList.add("hidden");
            createBtn.classList.remove("hidden");
            return;
        }

        errorMsg.textContent = "Server returned an unexpected response.";
        errorMsg.classList.remove("hidden");
        return;
    }

    // --- Success: rebuild dropdown from returned mealPlans ---
    const createNewItem = document.getElementById("createNewMealPlan");
    // Remove all current plan <li>s except the Create New element
    dropdown.querySelectorAll('li').forEach(li => {
        if (li !== createNewItem) li.remove();
    });

    // Insert returned plans after CreateNew element
    data.mealPlans.forEach(plan => {
        const newItem = document.createElement("li");
        newItem.className = "flex items-center px-3 py-2 border-b border-orange-200 last:border-b-0";
        newItem.innerHTML = `
            <label class="flex items-center gap-2 cursor-pointer w-full">
                <input type="checkbox" name="mealPlanIds" value="${plan.id}" class="hidden peer" />
                <span class="w-4 h-4 rounded-full border-2 border-orange-700 peer-checked:bg-orange-700 transition"></span>
                <span class="text-gray-700 text-sm truncate max-w-[80%]">${plan.title}</span>
            </label>
        `;
        dropdown.insertBefore(newItem, createNewItem.nextSibling);
    });

                // Reset + hide the form again
                input.value = "";
                form.classList.add("hidden");
                createBtn.classList.remove("hidden");
    
            
        } catch (err) {
            console.error(err);
            errorMsg.textContent = "An unexpected error occurred.";
            errorMsg.classList.remove("hidden");
        }
    
    });

}
    
    // --- Allow Enter key to submit ---
    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            form.dispatchEvent(new Event("submit"));
        }
    });
});



