(() => {
    const container = document.querySelector('[data-toasts]');

    if (!container) {
        return;
    }

    const dismiss = (toast) => {
        toast.dataset.leaving = '';
        window.setTimeout(() => toast.remove(), 220);
    };

    container.addEventListener('click', (event) => {
        const close = event.target.closest('[data-toast-close]');

        if (close) {
            dismiss(close.closest('[data-toast]'));
        }
    });

    container.querySelectorAll('[data-toast][data-sticky="false"]').forEach((toast) => {
        window.setTimeout(() => dismiss(toast), 5000);
    });
})();
