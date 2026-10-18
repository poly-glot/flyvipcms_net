export type Role = 'admin' | 'member' | 'submember';

export const ACCOUNTS: Record<Role, { email: string; password: string }> = {
    admin: { email: 'admin@flyvip.test', password: 'FlyVIP-Admin-2026!' },
    member: { email: 'carlos.family@flyvip.test', password: 'FlyVIP-Family-2026!' },
    submember: { email: 'lucia.family@flyvip.test', password: 'FlyVIP-Submember-2026!' },
};

export const PAGES: Record<Role, string[]> = {
    admin: [
        '/admin',
        '/admin/members',
        '/admin/members/2',
        '/admin/members/new',
        '/admin/payments',
        '/admin/payments/new?user_id=2',
        '/admin/points',
        '/admin/reservations',
        '/admin/reservations/new',
        '/admin/aircrafts',
        '/admin/aircrafts/new',
        '/admin/pilots',
        '/admin/routes',
        '/admin/flight-schedule',
    ],
    member: [
        '/portal',
        '/portal/points',
        '/portal/reservations',
        '/portal/reservations/new',
        '/portal/profile',
        '/account/password',
    ],
    submember: ['/portal', '/portal/points', '/portal/reservations', '/portal/profile'],
};
