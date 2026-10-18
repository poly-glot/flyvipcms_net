import { expect, test } from '@playwright/test';
import { signIn, signOut } from './support/auth';

test('sign in and sign out work for each role', async ({ page }) => {
    for (const role of ['admin', 'member', 'submember'] as const) {
        await signIn(page, role);
        await signOut(page);
    }
});

test('anonymous visitors are sent to the login page', async ({ page }) => {
    await page.goto('/admin');
    await expect(page).toHaveURL(/\/login$/);

    await page.goto('/portal');
    await expect(page).toHaveURL(/\/login$/);
});

test('members cannot open the admin area', async ({ page }) => {
    await signIn(page, 'member');
    await page.goto('/admin');

    await expect(page).not.toHaveURL(/\/admin$/);
});

test('sidebar navigation reaches fleet pages and marks the current one', async ({ page }) => {
    await signIn(page, 'admin');
    await page.getByRole('link', { name: 'Aircraft', exact: true }).click();

    await expect(page).toHaveURL(/\/admin\/aircrafts$/);
    await expect(page.locator('nav [aria-current="page"]')).toHaveCount(1);
    await expect(page.locator('nav [aria-current="page"]')).toHaveText(/Aircraft/);
});

test('sidebar collapse state survives a reload', async ({ page }) => {
    await signIn(page, 'admin');

    const collapser = page.locator('[data-sidebar-collapse]');
    await expect(collapser).toHaveAttribute('aria-pressed', 'false');
    await collapser.click();
    await expect(collapser).toHaveAttribute('aria-pressed', 'true');

    await page.reload();

    await expect(collapser).toHaveAttribute('aria-pressed', 'true');
    await collapser.click();
    await expect(collapser).toHaveAttribute('aria-pressed', 'false');
});

test('seat map shows taken seats and lets a free seat be picked', async ({ page }) => {
    await signIn(page, 'admin');
    await page.goto('/admin/reservations/new');

    await page.locator('select[name="aircraft_id"]').selectOption({ label: 'Citation CJ3' });
    await page.locator('input[name="flight_date"]').fill(seededFlightDate());

    const taken = page.locator('[data-seatmap] input[name="seats[]"]:disabled');
    await expect(taken.first()).toBeDisabled();

    const free = page.locator('[data-seatmap] input[name="seats[]"]:not(:disabled)').first();
    await free.check({ force: true });
    await expect(free).toBeChecked();
});

test('cancelling a reservation asks for confirmation and can be dismissed', async ({ page }) => {
    await signIn(page, 'member');
    await page.goto('/portal/reservations');

    await page
        .getByRole('button', { name: /cancel/i })
        .first()
        .click();

    const dialog = page.locator('dialog[open]');
    await expect(dialog).toBeVisible();

    await dialog
        .getByRole('button', { name: /keep|close|no/i })
        .first()
        .click();
    await expect(dialog).toBeHidden();
    await expect(page.locator('.flight').first()).toBeVisible();
});

test('password change shows inline errors for a wrong current password', async ({ page }) => {
    await signIn(page, 'member');
    await page.goto('/account/password');

    await page.getByLabel('Current password').fill('definitely-wrong');
    await page.getByLabel('New password', { exact: true }).fill('An0ther-Str0ng-Pass-456');
    await page.getByLabel('Confirm new password').fill('An0ther-Str0ng-Pass-456');
    await page.getByRole('button', { name: /update password/i }).click();

    await expect(page.getByText('Current password is incorrect.').first()).toBeVisible();
});

function seededFlightDate(): string {
    return new Date(Date.now() + 7 * 86_400_000).toISOString().slice(0, 10);
}
