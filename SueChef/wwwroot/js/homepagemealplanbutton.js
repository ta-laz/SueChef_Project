
    function toggleMealPlan(button) {
        const plusIcon = button.querySelector('.plus-icon');
        const tickIcon = button.querySelector('.tick-icon');
        const isActive = button.classList.contains('bg-rose-800');

        if (isActive) {
            // Back to plus
            button.classList.remove('bg-rose-800');
            button.classList.add('bg-orange-500');
            tickIcon.classList.add('hidden');
            plusIcon.classList.remove('hidden');
        } else {
            // Change to tick
            button.classList.remove('bg-orange-500');
            button.classList.add('bg-rose-800');
            plusIcon.classList.add('hidden');
            tickIcon.classList.remove('hidden');
        }
    }
