import { test } from '@playwright/test';
import { PAGES, type Role } from './support/accounts';
import { signIn } from './support/auth';

const VIEWPORTS = [
    { height: 900, name: 'mobile', width: 390 },
    { height: 1024, name: 'tablet', width: 820 },
    { height: 900, name: 'desktop', width: 1440 },
];

const SCHEMES = ['light', 'dark'] as const;

const slug = (path: string) => path.replace(/^\//, '').replace(/[/?=&]+/g, '-') || 'root';

for (const scheme of SCHEMES) {
    for (const viewport of VIEWPORTS) {
        test(`capture ${scheme} ${viewport.name}`, async ({ browser }) => {
            const context = await browser.newContext({
                colorScheme: scheme,
                viewport: { height: viewport.height, width: viewport.width },
            });
            const page = await context.newPage();

            await page.goto('/login');
            await page.screenshot({ fullPage: true, path: `test-results/visual/${scheme}-${viewport.name}-login.png` });

            for (const role of Object.keys(PAGES) as Role[]) {
                await signIn(page, role);

                for (const path of PAGES[role]) {
                    await page.goto(path);
                    await page.waitForLoadState('networkidle');
                    await page.screenshot({
                        fullPage: true,
                        path: `test-results/visual/${scheme}-${viewport.name}-${role}-${slug(path)}.png`,
                    });
                }

                await page.request.post('/logout', { form: {} });
                await context.clearCookies();
            }

            await context.close();
        });
    }
}
