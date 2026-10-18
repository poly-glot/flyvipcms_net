import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

const [minimumArgument = '55', resultsDirectory = 'test-results'] = process.argv.slice(2);
const minimum = Number(minimumArgument);

const report = readdirSync(resultsDirectory, { recursive: true }).find((path) =>
    String(path).endsWith('coverage.cobertura.xml'),
);

if (!report) {
    process.stderr.write(`No cobertura report under ${resultsDirectory}\n`);
    process.exit(1);
}

const lineRate = /<coverage line-rate="([\d.]+)"/.exec(readFileSync(join(resultsDirectory, String(report)), 'utf8'));
const percent = Number(lineRate?.[1] ?? 0) * 100;

process.stdout.write(`Line coverage ${percent.toFixed(2)}% (minimum ${minimum.toFixed(2)}%)\n`);
process.exit(percent >= minimum ? 0 : 1);
