(() => {
    const root = document.querySelector('[data-seatmap]');

    if (!root) {
        return;
    }

    const grid = root.querySelector('[data-seatmap-grid]');
    const status = root.querySelector('[data-seatmap-status]');
    const form = root.closest('form');
    const seatsPerRow = 4;

    const parseSeats = (value) => (value ? value.split(',').map(Number).filter(Boolean) : []);

    const seatElement = ({ number, taken, selected, interactive }) => {
        const wrapper = document.createElement(interactive ? 'label' : 'span');
        const face = document.createElement('span');
        const caption = document.createElement('span');

        wrapper.className = 'seat';
        face.className = 'seat__face';
        face.setAttribute('aria-hidden', 'true');
        face.textContent = String(number);
        caption.className = 'sr-only';

        if (!interactive) {
            wrapper.classList.toggle('seat--selected', selected);
            caption.textContent = selected ? `Seat ${number}, reserved` : `Seat ${number}`;
            wrapper.append(face, caption);

            return wrapper;
        }

        const input = document.createElement('input');
        input.type = 'checkbox';
        input.name = 'seats[]';
        input.value = String(number);
        input.checked = selected;
        input.disabled = taken;
        caption.textContent = taken ? `Seat ${number}, taken` : `Seat ${number}`;
        wrapper.append(input, face, caption);

        return wrapper;
    };

    const cabinRows = (capacity, makeSeat) => {
        const rows = [];

        for (let start = 1; start <= capacity; start += seatsPerRow) {
            const row = document.createElement('div');
            row.className = 'cabin__row';
            row.setAttribute('role', 'group');
            row.setAttribute('aria-label', `Row ${rows.length + 1}`);

            for (let offset = 0; offset < seatsPerRow; offset += 1) {
                const number = start + offset;

                if (offset === seatsPerRow / 2) {
                    row.append(Object.assign(document.createElement('span'), { className: 'cabin__aisle' }));
                }

                row.append(
                    number <= capacity
                        ? makeSeat(number)
                        : Object.assign(document.createElement('span'), { className: 'cabin__gap' }),
                );
            }

            rows.push(row);
        }

        return rows;
    };

    if (!form) {
        const reserved = new Set(parseSeats(root.dataset.selected));
        const capacity = Number(root.dataset.capacity || 0);

        grid.replaceChildren(
            ...cabinRows(capacity, (number) =>
                seatElement({ number, taken: false, selected: reserved.has(number), interactive: false }),
            ),
        );

        return;
    }

    const aircraft = form.elements.aircraft_id;
    const date = form.elements.flight_date;
    const kinds = form.querySelectorAll('input[name="kind"]');
    const completeNote = document.querySelector('[data-complete-note]');
    const capacities = JSON.parse(aircraft.dataset.capacities);
    const selected = new Set(parseSeats(root.dataset.selected));
    let taken = new Set();
    let ticket = 0;

    const capacity = () => Number(capacities[aircraft.value] || 0);
    const isPartial = () => form.querySelector('input[name="kind"]:checked')?.value !== 'complete';

    const describe = () => {
        if (!date.value) {
            status.textContent = 'Choose a flight date to see which seats are taken.';

            return;
        }

        status.textContent = `${selected.size} selected, ${taken.size} taken, ${capacity() - taken.size - selected.size} free`;
    };

    const draw = () => {
        root.hidden = !isPartial();

        if (completeNote) {
            completeNote.hidden = isPartial();
        }

        if (!isPartial()) {
            grid.replaceChildren();

            return;
        }

        [...selected].forEach((number) => {
            if (number > capacity() || taken.has(number)) {
                selected.delete(number);
            }
        });

        grid.replaceChildren(
            ...cabinRows(capacity(), (number) =>
                seatElement({ number, taken: taken.has(number), selected: selected.has(number), interactive: true }),
            ),
        );
        describe();
    };

    const load = async () => {
        ticket += 1;
        const current = ticket;
        taken = new Set();

        if (aircraft.value && date.value) {
            const query = new URLSearchParams({ aircraft_id: aircraft.value, flight_date: date.value });

            try {
                const response = await fetch(`${root.dataset.url}?${query}`, {
                    credentials: 'same-origin',
                    headers: { Accept: 'application/json' },
                });
                const body = await response.json();
                taken = new Set(body.taken || []);
            } catch {
                taken = new Set();
            }
        }

        if (current === ticket) {
            draw();
        }
    };

    grid.addEventListener('change', (event) => {
        const number = Number(event.target.value);

        if (event.target.checked) {
            selected.add(number);
        } else {
            selected.delete(number);
        }

        describe();
    });

    grid.addEventListener('keydown', (event) => {
        const step = { ArrowRight: 1, ArrowDown: 1, ArrowLeft: -1, ArrowUp: -1 }[event.key];

        if (!step || event.target.type !== 'checkbox') {
            return;
        }

        const seats = [...grid.querySelectorAll('input:not(:disabled)')];
        const next = seats[seats.indexOf(event.target) + step];

        if (next) {
            event.preventDefault();
            next.focus();
        }
    });

    aircraft.addEventListener('change', load);
    date.addEventListener('change', load);
    kinds.forEach((kind) => kind.addEventListener('change', draw));
    load();
})();
