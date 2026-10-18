(() => {
    document.querySelectorAll('[data-filter]').forEach((scope) => {
        const input = scope.querySelector('[data-filter-input]');
        const chips = [...scope.querySelectorAll('[data-filter-chip]')];
        const items = [...scope.querySelectorAll('[data-filter-item]')];
        const empty = scope.querySelector('[data-filter-empty]');
        const count = scope.querySelector('[data-filter-count]');

        const apply = () => {
            const query = input ? input.value.trim().toLowerCase() : '';
            const active = chips.find((chip) => chip.getAttribute('aria-pressed') === 'true');
            const status = active ? active.dataset.filterChip : '';
            let visible = 0;

            items.forEach((item) => {
                const matchesText = item.dataset.filterText.includes(query);
                const matchesStatus = status === '' || item.dataset.filterStatus === status;
                const show = matchesText && matchesStatus;

                item.hidden = !show;
                visible += show ? 1 : 0;
            });

            if (empty) {
                empty.hidden = visible > 0;
            }

            if (count) {
                count.textContent = String(visible);
            }
        };

        chips.forEach((chip) => {
            chip.addEventListener('click', () => {
                chips.forEach((other) => other.setAttribute('aria-pressed', String(other === chip)));
                apply();
            });
        });

        if (input) {
            input.addEventListener('input', apply);
        }
    });
})();
