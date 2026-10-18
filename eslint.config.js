import js from '@eslint/js';
import globals from 'globals';
import tseslint from 'typescript-eslint';

const noComments = {
    meta: { type: 'problem', schema: [] },
    create: (context) => ({
        Program() {
            for (const comment of context.sourceCode.getAllComments()) {
                context.report({
                    loc: comment.loc,
                    message: 'Comments are not allowed; express intent through naming and structure.',
                });
            }
        },
    }),
};

const flyvip = { rules: { 'no-comments': noComments } };

export default [
    { ignores: ['node_modules/**', '**/bin/**', '**/obj/**', 'test-results/**', 'playwright-report/**'] },
    js.configs.recommended,
    {
        files: ['src/FlyVip.Web/wwwroot/js/**/*.js'],
        languageOptions: { globals: globals.browser, sourceType: 'module' },
        plugins: { flyvip },
        rules: {
            eqeqeq: 'error',
            'flyvip/no-comments': 'error',
            'no-alert': 'error',
            'no-console': 'error',
            'no-eval': 'error',
            'no-restricted-syntax': [
                'error',
                {
                    message: 'Assigning to innerHTML is not allowed; build nodes with createElement/textContent.',
                    selector: "AssignmentExpression[left.property.name='innerHTML']",
                },
            ],
            'no-var': 'error',
            'prefer-const': 'error',
        },
    },
    ...tseslint.configs.recommended.map((config) => ({
        ...config,
        files: ['tests/E2E/**/*.ts', 'playwright.config.ts'],
    })),
    {
        files: ['tests/E2E/**/*.ts', 'playwright.config.ts', 'tools/**/*.mjs', 'eslint.config.js'],
        languageOptions: { globals: globals.node, sourceType: 'module' },
        plugins: { flyvip },
        rules: { 'flyvip/no-comments': 'error' },
    },
];
