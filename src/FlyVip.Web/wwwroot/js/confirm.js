(() => {
    const dialog = document.getElementById('confirm-dialog');

    if (!dialog || typeof dialog.showModal !== 'function') {
        return;
    }

    const title = dialog.querySelector('#confirm-title');
    const message = dialog.querySelector('#confirm-message');
    const accept = dialog.querySelector('#confirm-accept');
    let pending = null;

    document.addEventListener('submit', (event) => {
        const form = event.target;

        if (!(form instanceof HTMLFormElement) || !form.dataset.confirm) {
            return;
        }

        event.preventDefault();
        pending = form;

        title.textContent = form.dataset.confirmTitle || 'Are you sure?';
        message.textContent = form.dataset.confirm;
        accept.textContent = form.dataset.confirmAction || 'Confirm';
        dialog.showModal();
    });

    dialog.addEventListener('close', () => {
        const form = pending;
        pending = null;

        if (form && dialog.returnValue === 'confirm') {
            form.submit();
        }
    });
})();
