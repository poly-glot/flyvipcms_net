(() => {
    document.addEventListener('change', (event) => {
        const control = event.target;

        if (control instanceof HTMLElement && control.hasAttribute('data-autosubmit')) {
            control.form.submit();
        }
    });

    document.querySelectorAll('[data-when]').forEach((target) => {
        const [name, values] = target.dataset.when.split(':');
        const allowed = values.split(',');
        const form = target.closest('form');

        const sync = () => {
            const checked = form.querySelector(`[name="${name}"]:checked`);
            const current = checked ? checked.value : form.elements[name]?.value;
            target.hidden = !allowed.includes(current);
        };

        form.addEventListener('change', sync);
        sync();
    });
})();
