import { readdirSync, readFileSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

const root = join(import.meta.dirname, '..');
const scannedRoots = ['src', 'tests'];
const skippedSegments = new Set(['bin', 'obj', 'node_modules']);
const commentExempt = ['src/FlyVip.Web/Data/Migrations/'];
const inlineStyleAllowed = new Set(['src/FlyVip.Web/Views/Shared/partials/icons.cshtml']);

const sourceFiles = (directory) =>
    readdirSync(join(root, directory), { recursive: true, withFileTypes: true })
        .filter((entry) => entry.isFile())
        .map((entry) => relative(root, join(entry.parentPath, entry.name)).split(sep).join('/'))
        .filter((path) => !path.split('/').some((segment) => skippedSegments.has(segment)))
        .filter((path) => path.endsWith('.cs') || path.endsWith('.cshtml'))
        .sort();

const lineAt = (source, index) => source.slice(0, index).split('\n').length;

const csharpComments = (source) => {
    const found = [];
    const modes = [{ kind: 'code', depth: 0 }];
    let index = 0;

    const enterString = (start) => {
        let cursor = start;
        let interpolated = false;
        let verbatim = false;

        while (source[cursor] === '$' || source[cursor] === '@') {
            interpolated ||= source[cursor] === '$';
            verbatim ||= source[cursor] === '@';
            cursor += 1;
        }

        let quotes = 0;

        while (source[cursor + quotes] === '"') {
            quotes += 1;
        }

        if (quotes >= 3) {
            modes.push({ kind: 'raw', quotes });
            return cursor + quotes;
        }

        modes.push({ kind: 'string', interpolated, verbatim });
        return cursor + 1;
    };

    while (index < source.length) {
        const mode = modes.at(-1);
        const char = source[index];
        const next = source[index + 1];

        if (mode.kind === 'raw') {
            const closing = '"'.repeat(mode.quotes);
            const end = source.indexOf(closing, index);
            index = end < 0 ? source.length : end + mode.quotes;
            modes.pop();
            continue;
        }

        if (mode.kind === 'string') {
            if (!mode.verbatim && char === '\\') {
                index += 2;
            } else if (mode.verbatim && char === '"' && next === '"') {
                index += 2;
            } else if (mode.interpolated && char === '{' && next === '{') {
                index += 2;
            } else if (mode.interpolated && char === '{') {
                modes.push({ kind: 'code', depth: 0 });
                index += 1;
            } else {
                if (char === '"') {
                    modes.pop();
                }
                index += 1;
            }
            continue;
        }

        if (char === '/' && next === '/') {
            found.push(lineAt(source, index));
            const end = source.indexOf('\n', index);
            index = end < 0 ? source.length : end;
        } else if (char === '/' && next === '*') {
            found.push(lineAt(source, index));
            const end = source.indexOf('*/', index + 2);
            index = end < 0 ? source.length : end + 2;
        } else if (char === "'") {
            const end = source.indexOf("'", index + (next === '\\' ? 3 : 2));
            index = end < 0 ? source.length : end + 1;
        } else if (char === '"' || ((char === '$' || char === '@') && /^[$@]*"/.test(source.slice(index, index + 4)))) {
            index = enterString(index);
        } else {
            if (char === '{') {
                mode.depth += 1;
            } else if (char === '}' && modes.length > 1 && mode.depth === 0) {
                modes.pop();
            } else if (char === '}') {
                mode.depth -= 1;
            }
            index += 1;
        }
    }

    return found.map((line) => [line, 'C# comment']);
};

const razorViolations = (path, source) => {
    const violations = [];

    source.split('\n').forEach((line, index) => {
        const number = index + 1;
        const code = line.trim();

        if (line.includes('@*') || line.includes('<!--')) {
            violations.push([number, 'Razor or HTML comment']);
        }

        if (code.startsWith('//') || code.startsWith('/*') || /[;{}]\s*\/\//.test(line)) {
            violations.push([number, 'C# comment in a view']);
        }

        if (/\son[a-z]+\s*=\s*"/i.test(line)) {
            violations.push([number, 'inline event handler attribute']);
        }

        if (/\sstyle\s*=\s*"/i.test(line) && !inlineStyleAllowed.has(path)) {
            violations.push([number, 'inline style attribute']);
        }

        if (/#[0-9a-f]{3,8}\b/i.test(line) && !line.includes('href="#') && !line.includes("'#")) {
            violations.push([number, 'hex colour in a template; use a token']);
        }
    });

    return violations;
};

const failures = [];

for (const path of scannedRoots.flatMap(sourceFiles)) {
    const source = readFileSync(join(root, path), 'utf8');
    const violations = [];

    if (!commentExempt.some((prefix) => path.startsWith(prefix))) {
        violations.push(...(path.endsWith('.cs') ? csharpComments(source) : razorViolations(path, source)));
    }

    if (source.includes('Html.Raw')) {
        violations.push([lineAt(source, source.indexOf('Html.Raw')), 'Html.Raw is not allowed']);
    }

    failures.push(...violations.map(([line, message]) => `${path}:${line}: ${message}`));
}

for (const failure of failures) {
    process.stderr.write(`${failure}\n`);
}

process.stdout.write(`${failures.length} template/comment violation(s)\n`);
process.exit(failures.length === 0 ? 0 : 1);
