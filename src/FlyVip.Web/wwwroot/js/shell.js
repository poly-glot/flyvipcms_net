(() => {
    const root = document.documentElement;
    const sidebar = document.getElementById('sidebar');

    if (!sidebar) {
        return;
    }

    const stage = document.querySelector('.stage');
    const backdrop = document.querySelector('[data-sidebar-backdrop]');
    const opener = document.querySelector('[data-sidebar-open]');
    const closer = document.querySelector('[data-sidebar-close]');
    const collapser = document.querySelector('[data-sidebar-collapse]');
    const mobile = window.matchMedia('(max-width: 860px)');
    const storageKey = 'flyvip.sidebar';

    const remember = (state) => {
        try {
            localStorage.setItem(storageKey, state);
        } catch {
            return;
        }
    };

    const syncCollapser = () => {
        const collapsed = root.dataset.sidebar === 'collapsed';
        collapser.setAttribute('aria-pressed', String(collapsed));
        collapser.setAttribute('aria-label', collapsed ? 'Expand sidebar' : 'Collapse sidebar');
        collapser.title = collapsed ? 'Expand sidebar' : 'Collapse sidebar';
    };

    const setDrawer = (open) => {
        sidebar.toggleAttribute('data-open', open);
        backdrop.toggleAttribute('data-open', open);
        opener.setAttribute('aria-expanded', String(open));
        stage.inert = open;

        if (open) {
            sidebar.querySelector('.nav__link[aria-current="page"], .nav__link').focus();
            return;
        }

        if (sidebar.contains(document.activeElement)) {
            opener.focus();
        }
    };

    collapser.addEventListener('click', () => {
        const collapsed = root.dataset.sidebar !== 'collapsed';

        if (collapsed) {
            root.dataset.sidebar = 'collapsed';
        } else {
            delete root.dataset.sidebar;
        }

        remember(collapsed ? 'collapsed' : 'expanded');
        syncCollapser();
    });

    opener.addEventListener('click', () => setDrawer(true));
    closer.addEventListener('click', () => setDrawer(false));
    backdrop.addEventListener('click', () => setDrawer(false));

    sidebar.addEventListener('click', (event) => {
        if (mobile.matches && event.target.closest('a')) {
            setDrawer(false);
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && sidebar.hasAttribute('data-open')) {
            setDrawer(false);
        }
    });

    mobile.addEventListener('change', () => {
        if (!mobile.matches) {
            setDrawer(false);
        }
    });

    syncCollapser();
})();
