import { expect, type Page } from '@playwright/test';
import { ACCOUNTS, type Role } from './accounts';

export async function signIn(page: Page, role: Role): Promise<void> {
    const { email, password } = ACCOUNTS[role];

    await page.goto('/login');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Password').fill(password);
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page).toHaveURL(role === 'admin' ? /\/admin$/ : /\/portal$/);
}

export async function signOut(page: Page): Promise<void> {
    await page.getByRole('button', { name: /sign out/i }).click();
    await expect(page).toHaveURL(/\/login$/);
}
