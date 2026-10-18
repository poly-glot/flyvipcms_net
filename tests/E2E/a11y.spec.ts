import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';
import { PAGES, type Role } from './support/accounts';
import { signIn } from './support/auth';

const WCAG_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'];

async function violationsOn(page: import('@playwright/test').Page, path: string) {
    await page.goto(path);
    await page.waitForLoadState('networkidle');

    const result = await new AxeBuilder({ page }).withTags(WCAG_TAGS).analyze();

    return result.violations.map((violation) => `${violation.id} (${violation.nodes.length}): ${violation.help}`);
}

for (const scheme of ['light', 'dark'] as const) {
    test.describe(`${scheme} scheme`, () => {
        test.use({ colorScheme: scheme });

        test('login page has no WCAG AA violations', async ({ page }) => {
            expect(await violationsOn(page, '/login')).toEqual([]);
        });

        for (const role of Object.keys(PAGES) as Role[]) {
            test(`${role} pages have no WCAG AA violations`, async ({ page }) => {
                await signIn(page, role);

                const report: Record<string, string[]> = {};

                for (const path of PAGES[role]) {
                    const violations = await violationsOn(page, path);

                    if (violations.length > 0) {
                        report[path] = violations;
                    }
                }

                expect(report).toEqual({});
            });
        }
    });
}
