import { defineConfig, devices } from '@playwright/test';
import { existsSync } from 'node:fs';
import { homedir } from 'node:os';

const ownServer = process.env.E2E_SERVE === '1';
const ownServerUrl = 'http://127.0.0.1:8081';
const baseURL = ownServer ? ownServerUrl : (process.env.E2E_BASE_URL ?? 'http://localhost:8091');

const bundledDotnet = `${homedir()}/.dotnet/dotnet`;
const dotnet = existsSync(bundledDotnet) ? bundledDotnet : 'dotnet';
const dbServer = process.env.E2E_DB_SERVER ?? 'Server=127.0.0.1;Port=13306;User Id=root;Password=FlyVip!Dev2026';
const database = 'flyvip_e2e';
const project = `"${dotnet}" run --no-launch-profile --project src/FlyVip.Web`;
const dropDatabase = `"${dotnet}" tool restore && "${dotnet}" ef database drop --force --project src/FlyVip.Web`;

export default defineConfig({
    fullyParallel: false,
    outputDir: 'test-results/artifacts',
    projects: [
        {
            name: 'chromium',
            testIgnore: /visual\.spec\.ts/,
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'visual',
            testMatch: /visual\.spec\.ts/,
            use: { ...devices['Desktop Chrome'] },
        },
    ],
    reporter: process.env.CI ? [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }]] : 'list',
    retries: process.env.CI ? 1 : 0,
    testDir: './tests/E2E',
    use: {
        baseURL,
        screenshot: 'only-on-failure',
        trace: 'on-first-retry',
    },
    webServer: ownServer
        ? {
              command: `${dropDatabase} && ${project} -- migrate && ${project} -- seed && ${project}`,
              env: {
                  ASPNETCORE_ENVIRONMENT: 'Development',
                  ASPNETCORE_URLS: ownServerUrl,
                  ConnectionStrings__Default: `${dbServer};Database=${database}`,
              },
              reuseExistingServer: true,
              timeout: 300_000,
              url: `${ownServerUrl}/login`,
          }
        : undefined,
    workers: 1,
});
