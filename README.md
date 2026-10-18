# FlyVIP CMS (.NET)

Private-jet membership CMS: the .NET 10 version of [poly-glot/flyvipcms](https://github.com/poly-glot/flyvipcms), a faithful rebuild of its CodeIgniter 4 app — same URLs, same UI and CSS, same domain rules, same demo accounts. ASP.NET Core MVC, EF Core 10, MySQL 9.

It is live at https://flyvipcms-net.junaid.guru (also https://flyvipcms-net.web.app), on Cloud Run behind Firebase Hosting, with the shared OCI HeatWave MySQL 9.

## Run it locally (devcontainer)

```bash
devcontainer up --workspace-folder .    # .NET 10 SDK + MySQL container, restores, migrates, seeds, serves
open http://localhost:8091
```

The db service is the multi-arch `mysql:9` image (native on Apple Silicon and amd64), published on `127.0.0.1:13306` (root / `FlyVip!Dev2026`). EF `Migrate()` creates the databases.

Host-side instead of the devcontainer: run any MySQL container on `127.0.0.1:13306` (root / `FlyVip!Dev2026`), then

```bash
~/.dotnet/dotnet run --project src/FlyVip.Web -- migrate
~/.dotnet/dotnet run --project src/FlyVip.Web -- seed
ASPNETCORE_URLS=http://127.0.0.1:8091 ~/.dotnet/dotnet run --project src/FlyVip.Web --no-launch-profile
```

### Demo accounts (local only — the seeder refuses to run in Production)

| Role | Sign in with (email) | Password | What it shows |
|---|---|---|---|
| Admin | `admin@flyvip.test` | `FlyVIP-Admin-2026!` | Whole CMS under `/admin` |
| Personal member | `ana.personal@flyvip.test` | `FlyVIP-Personal-2026!` | Active, a pending seat request on a shared flight |
| Family member | `carlos.family@flyvip.test` | `FlyVIP-Family-2026!` | Active, owns the shared flight, has a sub-member |
| Sub-member | `lucia.family@flyvip.test` | `FlyVIP-Submember-2026!` | Read-only view of the family account |
| Corporate member | `acme.corporate@flyvip.test` | `FlyVIP-Corporate-2026!` | Paid two quarters, 24,000 pts |
| Prospect | `new.prospect@flyvip.test` | `FlyVIP-Prospect-2026!` | Joining fee unpaid: can only be charged the joining fee |
| Banned member | `banned.member@flyvip.test` | `FlyVIP-Banned-2026!` | Login is refused |

## Quality gates

```bash
./quality.sh         # dotnet format check → build -warnaserror → tests → coverage floor (55%) → npm run lint
npm run lint         # Prettier, ESLint, Stylelint, template/comment checks
npm run lint:fix     # auto-fix what the linters can
npm run test:e2e     # Playwright: flows + axe WCAG 2.2 AA light/dark (starts its own server on :8081 with its own seeded db)
npm run test:visual  # report-only: screenshots of every page, 3 widths × light/dark → test-results/visual/
tests/smoke.sh       # HTTP smoke test of every page for every role against a running server
docker buildx build --platform linux/amd64 -t flyvipcms-net .   # production image, described below
```

The production image is trimmed and self-contained on `runtime-deps:10.0-noble-chiseled`, about 25 MB compressed, with no shell. Its args are the commands `migrate`, `seed` and `create-admin`; with no args it serves on 8080.

`tests/smoke.sh` has two modes:

- `SMOKE_ANONYMOUS=1` runs only the anonymous checks: health, redirects, the `__session`-only cookie check, and a wrong-password login.
- `SMOKE_RESTART='<command>'` signs in, runs the command, waits for `/health`, and expects the session to survive.

CI (`.github/workflows/ci.yml`):

- `quality`: `quality.sh` against a MySQL 9 service.
- `e2e`: builds the production image and enforces the 28 MB compressed budget; migrates and seeds over TLS with the Cloud Run `DB_*` env; runs the smoke, including a session check across a brand-new container; runs Playwright against the container; fails on any `Exception` in the container log; upgrades a database migrated and seeded by the previous release and smokes it; on `main`, hands the tested image to `deploy`.
- `visual`: non-blocking.
- On `main`, `deploy` follows (see Deploy).

## Areas

- `/admin` — members & sub-members, payments, points ledger and transfers, reservations, aircraft, aircraft types, maintenance, airports, routes, pilots (+ documents, certifications, flight schedule). Admin only.
- `/portal` — members see balance, points history, reservations (book, cancel, confirm seat requests) and profile; sub-members get a read-only view of the primary member's account.
- `/account/password` — any signed-in user changes their password.

Runtime configuration: `ConnectionStrings__Default`, or `DB_HOST`, `DB_PORT` (default 3306), `DB_USER`, `DB_PASS` and `DB_NAME` (always over TLS; `DB_HOST` wins); `ASPNETCORE_ENVIRONMENT`; `SKIP_MIGRATIONS=1` to skip migrate-on-boot; `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` behind a TLS-terminating proxy. First admin: `ADMIN_PASSWORD=… dotnet run --project src/FlyVip.Web -- create-admin <username> <email>`.

## Deploy

1. **Infrastructure.** It lives in `poly-glot/firebase-cloud`, in `terraform/apps/flyvipcms-net.tf`, as two PRs.
   - First PR: the identity and WIF provider, the secret shells `flyvipcms-net-db-*` and `flyvipcms-net-admin-password`, the `flyvipcms-net` Hosting site, the `flyvipcms-net.junaid.guru` custom domain, and the `mysql-app-catalog` entry.
   - Second PR: the Cloud Run service `flyvipcms-net`, the job `flyvipcms-net-migrate`, and the Hosting release.

   It takes two PRs because Cloud Run resolves secret versions when it creates a service or job.

2. **Database.** personal-cloud's `terraform-mysql-apps` workflow, dispatched by the firebase-cloud apply, creates the database `flyvipcms_net` and the `REQUIRE SSL` user `flyvipcms-net`. It fills `flyvipcms-net-db-user|pass|name`.
   1. Add a version to `flyvipcms-net-admin-password`: a generated value piped into `gcloud secrets versions add flyvipcms-net-admin-password --data-file=- --project firebase-cloud-491613`.
   2. Merge the second PR.

3. **Secrets and DNS.** Set the repo secrets from the Terraform outputs: `WIF_PROVIDER` ← `flyvipcms_net_wif_provider`, `GCP_SA_EMAIL` ← `flyvipcms_net_gcp_sa_email`. Add the DNS record in Cloudflare: CNAME `flyvipcms-net` → `flyvipcms-net.web.app`, DNS only, as listed in `flyvipcms_net_required_dns`.

4. **Push to `main`.** The `deploy` job:
   1. pushes the image the `e2e` job tested as `europe-west2-docker.pkg.dev/firebase-cloud-491613/firebase-cloud/flyvipcms-net:<sha12>` and `:latest`;
   2. runs the migrate job;
   3. deploys a candidate revision with no traffic (`--tag candidate`) and smokes it;
   4. promotes it;
   5. smokes https://flyvipcms-net.web.app.

   The service runs with `SKIP_MIGRATIONS=1`. Roll back with `gcloud run services update-traffic flyvipcms-net --to-revisions=<revision>=100 --region europe-west2 --project firebase-cloud-491613`.

5. **First admin** (once): `gcloud run jobs execute flyvipcms-net-migrate --args=create-admin,admin,<email> --region europe-west2 --project firebase-cloud-491613 --wait`. The job reads `ADMIN_PASSWORD` from the `flyvipcms-net-admin-password` secret. Production has no demo data.

See `CLAUDE.md` for the domain rules and the reasoning behind the non-obvious choices.
