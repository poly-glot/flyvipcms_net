# FlyVIP CMS (.NET) — working agreement

ASP.NET Core MVC with Areas (`Admin`, `Portal`) on .NET 10 / EF Core 10 / MySQL 9 (Oracle `MySql.EntityFrameworkCore` provider; Pomelo has no EF Core 10 release). The devcontainer db is the multi-arch `mysql:9` image; the health check is `mysqladmin ping` and EF `Migrate()` creates the databases. Rebuild of the CodeIgniter 4 FlyVIP membership CMS in `../flyvipcms` — same URLs, same UI, same CSS, same domain rules, same demo accounts. When behavior here is unclear, the CI4 app is the reference implementation. It is deployed to Cloud Run behind Firebase Hosting at https://flyvipcms-net.junaid.guru (see Deploy).

## Commands

- `./quality.sh` — the gate: `dotnet format` check → build with warnings as errors → tests → coverage floor (55% line) → `npm run lint`.
- `dotnet run --project src/FlyVip.Web -- migrate` — applies EF migrations (also applied on normal boot unless `SKIP_MIGRATIONS=1`).
- `dotnet run --project src/FlyVip.Web -- seed` — local demo data. Throws when environment is `Production`.
- `dotnet run --project src/FlyVip.Web -- create-admin <username> <email>` — password from `ADMIN_PASSWORD` or prompt (min 12 chars).
- `docker buildx build --platform linux/amd64 -t flyvipcms-net .` — the trimmed production image, whose args are the commands above.
- `npm run lint` — Prettier (`printWidth 120`, single quotes, 4 spaces), ESLint, Stylelint, `tools/check-templates.mjs`. Part of `./quality.sh`.
- `npm run test:e2e` / `npm run test:visual` — Playwright; `E2E_SERVE=1` starts its own `dotnet run` on :8081.
- `tests/smoke.sh` — walks every page as every role against `BASE_URL` (default `http://127.0.0.1:8091`). `SMOKE_ANONYMOUS=1` runs only the anonymous checks (health, redirects, the `__session`-only cookie check, a wrong-password login that must re-render rather than fail antiforgery); `SMOKE_RESTART='<command>'` signs in, runs the command, waits for `/health` and expects the session to survive.

## Code rules

- No comments of any kind in C#, cshtml, CSS, JS. Names carry intent; WHY lives in this file. Exempt: `src/FlyVip.Web/Data/Migrations` (EF-generated).
- Money and points are `decimal` (SQL `DECIMAL(12,2)`, EF `HasPrecision(12, 2)`). Never `double`.
- Property order in CSS is alphabetical. Styles live in `src/FlyVip.Web/wwwroot/css/*.css` and `wwwroot/js` is the CI4 app's JS; both are byte-identical to the CI4 app's `public/` and must not change, so every DOM hook they use (field ids/names such as `f_aircraft_id`/`aircraft_id`, `seats[]`, `data-*` attributes, CSS classes, aria attributes) is frozen. No CDN assets, no build step.
- Controllers stay thin: bind a view model → `ModelState.IsValid` → one service call → redirect with a flash. Business rules live in `Services` and `Domain`; a broken rule throws `DomainRuleException`, which the controller turns into a flash error or a model error.
- Validation lives on view models as DataAnnotations. Every message is CI4's exact text: the custom attributes in `Infrastructure/ValidationAttributes.cs` and `Infrastructure/NumberAttributes.cs` (`MaxCharacters`, `EmailFormat`, `DecimalNumber`, `GreaterThan`, `AtLeast`, `Differs`, …) format CI4's messages, and `[Display(Name = …)]` supplies the label CI4 used (often the raw field name, e.g. `plan_id`). If a framework default message differs from CI4, set it explicitly.
- C# and the database are PascalCase (conventional EF schema). snake_case survives only in form field names and ids (`[FromForm(Name = "aircraft_id")]`, `f_aircraft_id`), because the frozen JS and CSS address them.
- Views use tag helpers: `<field asp-for=… label=… type=…>` renders the whole CI4 field row (label, control, hint, inline error, `aria-*`), `<crud-field>`, `<segmented>`, `<icon>`, `<avatar>`, `<pips>`, `<sparkline>`, plus the stock form tag helper for antiforgery. `UiHelpers` keeps only formatting (`UiDate`, `UiPoints`, `UiSignedPoints`, `UiInitials`) and the nav (`UiNav`, `UiIsCurrent`, `UiNavLabel`). Flash messages render as toasts, validation errors inline next to fields plus the error summary.
- Every admin write is a POST behind antiforgery; deletes are POSTs too.
- Razor output is encoded by default; `Html.Raw` is not used anywhere (`tools/check-templates.mjs` enforces).

## Architecture

- `Domain` — pure, static: `QuarterCalendar`, `FlightPricing`.
- `Services` — `PointsService` (ledger), `PaymentService`, `MemberService`, `ReservationService`. Scoped DI, constructor-injected `AppDbContext`. All multi-write operations run inside one transaction via `Atomic.Run`; services own their transactions and the `UPDLOCK` protocol. `DomainRuleException` is the business-rule exception.
- Feature folders (vertical slices): every feature is one folder holding its controller, its view models and its views, and the namespace follows the folder (`FlyVip.Web.Features.Auth`, `FlyVip.Web.Areas.Admin.Features.Members`). `Program.cs` prepends `/Features/{1}/{0}.cshtml` to `ViewLocationFormats` and `/Areas/{2}/Features/{1}/{0}.cshtml` to `AreaViewLocationFormats` (`AddRazorOptions`), so `View()` finds a feature's views by convention and layouts/partials still fall back to `Views/Shared`. Each area root (`Areas/Admin`, `Areas/Portal/Features`) has its own `_ViewImports` (one `@using` per feature namespace of that tree) and `_ViewStart`; the Admin pair sits at `Areas/Admin/` so it also covers the shared `Crud/` views.
- Shared kernel, deliberately outside the slices: a type used by more than one feature lives in `Models/` (cross-area view models: `BookingViewModel`, `ProfileViewModel`/`NamedProfileViewModel`), `Infrastructure/` (`LedgerLine`, `MemberMenu`, `CrudField`, `ValidationErrors`, `FlightList`, `EmptyState`, tag helpers) or the area root (`Areas/Portal/PortalController`, `PortalMember`, `Areas/Admin/Crud`). A feature never imports another feature's namespace; when a second feature needs a type, move it to the shared kernel. Layouts are `Views/Shared/_Layout.cshtml` (app shell), `_AuthLayout.cshtml` and `_Base.cshtml`; shared partials live in `Views/Shared/partials/` and are referenced as `partials/<name>`.

```
src/FlyVip.Web/
  Features/                 Home, Auth, Account, FlightSeats
  Areas/Admin/Crud/         CrudController, ICrudForm, CrudPages, Index/Form + _CrudRows* views (area-root shared CRUD machinery)
  Areas/Admin/Features/     Dashboard, Members, Payments, Points, Reservations, AircraftTypes, Aircrafts, AircraftRepairs,
                            Airports, AirRoutes, Pilots, PilotDocuments, PilotCertifications, PilotFlights
  Areas/Portal/Features/    Dashboard, Points, Profile, Reservations
  Areas/Portal/             PortalController, PortalMember (area base, not a feature)
  Models/  Views/Shared/  Infrastructure/  Services/  Domain/  Data/
```

- Reads for pages live in `*Queries` classes (suffix is the registration convention: `AddQueries()` scans for non-abstract classes ending in `Queries` and registers them scoped). A feature's page data comes from its own `XQueries` (`Members/MemberQueries`, `Dashboard/AdminDashboardQueries`, `Payments/PaymentQueries`, `Points/PointsQueries`, `Portal/Features/*Queries`, `PortalMemberQueries`); reads shared by Admin and Portal sit in `Services/ReservationQueries` and `Services/BookingQueries`. Controllers and view models never touch `AppDbContext`; the nine CRUD resource controllers and `CrudController` are the exception (generic persistence plus `Db.*Menu()` options), as are `AuthController`/`AccountController` (identity writes).
- Views render flat read models, never entities. `ReservationListing`, `LedgerLine`, `PaymentLine`, `ActivityLine`, `MemberDetail`, `PortalMember` are projected in the query, so EF loads only the columns shown and nothing is tracked. Rules a page needs are properties of those models (`ReservationListing.IsUpcoming(today)`, `SeatsHeld`, `ReservationDetail.CanConfirm`/`CanCancel`/`HeldSeats`, `MemberRow.IsOverdue`, `OverdueMember.DaysOverdue`, `StatTile.Icon`, `PaymentTypeOption.AmountText`) or are precomputed by the controller (`FlightList.Of(...)` partitions upcoming/earlier and attaches per-listing `FlightAction`s). A view contains no domain predicate and no clock read; the upcoming rule exists as `ReservationListing.IsUpcoming` (in memory) beside `ReservationQueries.IsUpcoming` (the same predicate as an EF expression) and the two must change together.
- The clock is `TimeProvider` (registered singleton; `TimeProvider.Today()` extension). Query classes, controllers and `ReservationService` take it; entity timestamps in services still use `DateTime.Now`.
- `Menus.*Menu()` are async. `CrudController.Fields()` is static metadata (names via `nameof(Form.Property)`, resolved to the binder name and label from form metadata in `Page()`), and select options come from the async `Options()` override keyed the same way. `MemberService.PrimaryMembers(withBalances: true)` returns every balance in one query; never loop `PointsService.Balance` over members.
- View models do no I/O. Choice lists are loaded by the controller from a query and applied with `UseChoices`/`UsePlans`/`UseMembers`/`Choices =`.
- `Areas/Admin/Crud` — area-root shared machinery (like `PortalController`): `CrudController<TEntity, TForm>`, one generic CRUD base for the nine fleet/pilot resources (list/new/create/edit/update/soft-delete, 404 on missing or soft-deleted rows, the frozen flash strings). A resource is `[CrudResource("slug")]` + an entity implementing `ISoftDeletable` + an annotated form class implementing `ICrudForm<TEntity>` (`Load`/`Apply`, explicit mapping, no reflection) + a `CrudLayout` of two `CrudField` groups, `Essentials` (form top, index columns minus textareas) and `Details` ("More details"), declared in rendered order, that is view metadata only (name, input type; select options come from the async `Options()` override). Each of the nine resources is its own feature folder under `Areas/Admin/Features/` (`AircraftTypes`, `Aircrafts`, `AircraftRepairs`, `Airports`, `AirRoutes`, `Pilots`, `PilotDocuments`, `PilotCertifications`, `PilotFlights`) owning only its controller and form. The nine controllers have nine names, so the base renders `Index`/`Form` by `~/Areas/Admin/Crud/…` path, and the `_CrudRows*`/`_CrudRowActions` partials are named with their `.cshtml` extension, which Razor resolves relative to the calling view's folder.
- `Areas/Portal/PortalController` — base for portal controllers; `CurrentMember()` resolves the signed-in member and the account they act for (a sub-member acts for the primary member).
- `Infrastructure` — tag helpers, validation attributes, `ValidationErrors` (error summary), `FormValues` (invariant parsing of string-bound form values), `PasswordStrength`, `IsoDateModelBinder`, small view records.
- Auth is cookie auth over our own `Users` table (`PasswordHasher<AppUser>`), login by **email**, role claim from `Users.Role`: `admin`, `member`, `submember`. Registration, magic links and remember-me do not exist. A `banned` member is refused at login.
- `Infrastructure/SessionCookieJar` — middleware between `UseStaticFiles` and `UseAuthentication`; packs every cookie the app sets into one `__session` cookie and unpacks it on the way in (see Deploy).
- `Data/DatabaseConnection` — builds the connection string from `DB_*`, else uses `ConnectionStrings:Default` (see Deploy).
- Data-protection keys persist in the `DataProtectionKeys` table via `AddDataProtection().SetApplicationName("FlyVip").PersistKeysToDbContext<AppDbContext>()` (see Deploy).

## MySQL mapping (back to the CI4 engine, EF conventions)

- Conventional EF schema: PascalCase tables named after the `DbSet`s (`Members`, `PointsLedger`, `ReservationSeats`, …) and PascalCase columns (`Member.UserId` → `UserId`), plus a `Users` table replacing Shield's. One `InitialCreate` migration.
- Navigations are used wherever they simplify a query (`Member.User`, `AppUser.Profile`, `Reservation.Aircraft`/`Route`/`Seats`, …). `AppUser.Member` and `AppUser.Profile` are optional, so going from a user to its member or profile is a LEFT JOIN: admins have neither.
- MySQL `ENUM` → C# enum stored as the exact lowercase snake_case text (`'active'`, `'partial'`, `'transfer_in'`, …) with a check constraint (`LowercaseEnum` in `AppDbContext`). `EnumText.Of` produces the same text for pill labels.
- `SELECT Id FROM Users WHERE Id = … FOR UPDATE` raw SQL inside the service's transaction. `Atomic.Run` opens transactions at `READ COMMITTED`: under InnoDB's default `REPEATABLE READ` the snapshot is taken at the transaction's first plain read, so a balance read after the lock could be stale and two bookings could overdraw.
- `DATETIME`/`DATE` → `datetime(6)`/`date` (`DateTime`/`DateOnly`). Every unsized `decimal` is `DECIMAL(12,2)`.
- Soft deletes: entities with `DeletedAt` (all `ISoftDeletable`) have a global query filter. `IgnoreQueryFilters()` switches off the filters of every entity in the query, including ones reached through navigations, so a query that must see one soft-deleted entity but not another adds explicit `DeletedAt == null` predicates.

## Frozen-markup notes (why the odd bits exist)

- **Antiforgery token first.** CI4 forms emit `csrf_field()` as the form's first child, so `ui.css`'s `.form-section:first-child` never matches the first section of a CI4 form. The stock form tag helper appends the token at the end; `AntiforgeryFirstTagHelper` moves it to the front so the CSS behaves exactly as in CI4.
- **`<!form method="dialog">`** in `_Base.cshtml` opts the confirm dialog's form out of tag-helper processing, so no antiforgery token or other attributes are injected into a form that never posts.
- **`"field  field--invalid"`.** The field row's class is `"field " + extra + " field--invalid"` trimmed, which leaves a double space when there is no extra class. CI4 renders the same bytes; tests assert it. Do not "fix" it.
- **Error-summary order** comes from view-model metadata (`ValidationErrors.Of` walks `ModelMetadata.Properties`, then any keys not on the model). Where a hierarchy interleaves base and derived fields (the admin member forms extend the profile model), `[Display(Order = …)]` puts the errors back in CI4's rule order.
- **Password strength after the current-password check.** `AccountController` runs `PasswordStrength.Weakness` only after verifying the current password, because CI4 reports a wrong current password before any strength problem. It is not a validation attribute for that reason.
- **Number fields bind as strings.** Numeric form properties are `string?` with `DecimalNumber`/`WholeNumber`/`GreaterThan`/`AtLeast`/`AtMost` attributes and are parsed in `Apply` via `FormValues`. Binding them as `decimal`/`int` would replace CI4's messages with the model binder's "The value 'x' is not valid".

## Domain rules and why

**Points are a signed ledger.** `PointsLedger.Amount` is signed (negative = spend); `BalanceAfter` is stored per row; the balance is the last row. A payment credits points immediately — nothing to convert, no cron. Each post locks the user row (`UPDLOCK`) before reading the balance so concurrent bookings cannot overdraw. Overdraft is refused except the forced re-charge when a seat-joiner cancels (see below).

**Membership payments** (`PaymentService`):
- A member must pay the `joining` fee first; until then it is the only allowed type.
- Joining fee activates the membership and sets `Members.NextDueOn` to the 5th day after the end of the next calendar quarter (Jan–Mar → Apr 05, … Oct–Dec → Jan 05).
- Amounts for `joining`, `yearly`, `quarterly` are computed server-side from the member's stored fees; the client value is ignored. `bonus` and `points` take a free amount and credit points.
- `yearly`: 4 quarter rows, the whole yearly amount credited at once. If a previous term exists the new term starts the day after it ends (prepay); otherwise on the current or next calendar quarter.
- `quarterly`: pays the next *n* open quarters (1–4), crediting `YearlyFee / 4` each, continuing the open term or opening a new one. Yearly is refused while a term is partly paid.
- `NextDueOn` = end of last paid period + 5 days (`QuarterCalendar.GraceDays`).
- Only primary members (`Members.PlanId IS NOT NULL`) are payable; sub-members have no fees and no points of their own — they see their primary member's account.

**Reservations** (`ReservationService`): price per seat = `AirRoutes.Cost + Aircrafts.TotalCost`.
- `complete`: charges `seat price × capacity` and takes every seat.
- `partial`, first booker (flight owner): charged `seat price × capacity` up front for the whole aircraft, holds the chosen seats.
- `partial`, joiner: charged `seat price × seats`, status `pending`; the owner is refunded the same share immediately and the owner's reservation `Amount` drops by it. The owner (or an admin) confirms.
- A flight is identified by `(AircraftId, FlightDate)`. `ReservationSeats` has a unique key on `(AircraftId, FlightDate, SeatNumber)` so double booking is impossible even under concurrency; cancelling deletes the seat rows.
- Owner cancels → owner and every joiner are refunded their current amounts and all become `cancelled`. Joiner cancels → joiner refunded, owner re-charged the share with an **allowed overdraft** (a member must always be able to leave a flight, so the owner's balance may go negative).
- Cancelled reservations are kept (status `cancelled`). History also lives in `ReservationActivities` and the ledger.
- Only active primary members can book; dates must be today or later; the route's aircraft must match. Editing a reservation is cancel + rebook.

**Member codes.** Primary members: `{plan OwnerPrefix}-NNN` (`PO-`, `FO-`, `CO-`). Sub-members: `{plan code}{contact-type suffix}-NNN`.

**Boot and migrations.** The app applies EF migrations on boot (`SKIP_MIGRATIONS=1` to skip), so `/health` only answers once the schema is current. Cloud Run sets `SKIP_MIGRATIONS=1`, and the `flyvipcms-net-migrate` job applies migrations before traffic moves (see Deploy).

**Config contract.** The connection comes from `ConnectionStrings__Default`, or from `DB_HOST`, `DB_PORT` (default 3306), `DB_USER`, `DB_PASS` and `DB_NAME`. The `DB_*` variables always use TLS, and `DB_HOST` wins (see Deploy). `ASPNETCORE_ENVIRONMENT` selects behavior (`DemoSeeder` refuses in Production; password-hash iterations drop to 1000 under `Testing` so the suite stays fast). `SKIP_MIGRATIONS=1` skips migrate-on-boot, and `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` is set behind a TLS-terminating proxy.

## Tests

- `tests/FlyVip.Tests` — xUnit. `Unit/` (pure domain, formatting, tag-helper markup snapshots), `Service/` (real MySQL, `flyvip_test` database, every non-reference table truncated in the per-test reset — not a wrapping transaction: services own their transactions, and a wrapper would hide rollback bugs), `Feature/` (`WebApplicationFactory`, in-process HTTP, same test database).
- `Feature/SessionCookieTest` — the whole sign-in → portal → sign-out flow sets only `__session`, under 3,000 characters.
- `Unit/SessionCookieJarTest` and `Unit/DatabaseConnectionTest` — the jar's pack/unpack/expiry rules and both branches of the connection string.
- Coverage floor is 55% line coverage, enforced by `quality.sh`.
- Playwright E2E (`tests/E2E`): sign-in/out, seat map, dialogs, axe WCAG 2.2 AA light + dark on every page for every role; `visual.spec.ts` is report-only screenshots.
- CI's `e2e` job tests the production image itself.

## Deploy (Cloud Run behind Firebase Hosting)

**Topology.** https://flyvipcms-net.web.app and https://flyvipcms-net.junaid.guru are Firebase Hosting, which rewrites every path to Cloud Run service `flyvipcms-net` (europe-west2, project `firebase-cloud-491613`). The database is `flyvipcms_net` on the shared OCI HeatWave MySQL 9. The infrastructure is in poly-glot/firebase-cloud, `terraform/apps/flyvipcms-net.tf`. The database and its user come from personal-cloud's `terraform-mysql-apps`, through the `mysql-app-catalog` entry.

**`__session` is the only cookie.** Hosting forwards only `__session` to Cloud Run, so `SessionCookieJar` packs the auth, antiforgery and TempData cookies into it. It uses raw header values, packed once and sorted, so an unchanged jar writes no `Set-Cookie`.

- Never add a second cookie. Never read or write `__session` through `Request.Cookies` or `Response.Cookies`.
- Local requests get the same treatment (other cookies are dropped), so local behavior equals live.
- `SessionCookieTest` fails at 3,000 characters. Browsers drop cookies over 4,096 bytes, and the happy-path peak is about 870.
- `beforeLoginUrl` is remembered only up to `AuthController.BeforeLoginUrlMaxLength` (512) characters. A longer, attacker-supplied URL would push `__session` past 4,096 bytes; the browser would drop it, and every login POST would fail antiforgery until the visitor cleared cookies.
- Names and values are packed with `Uri.EscapeDataString`, so a crafted `__session` cannot carry `;` or `,` into the outgoing cookie, which made every page that changes the jar return 500.
- Two concurrent requests that both change the jar: the last writer wins.

**Data-protection keys live in MySQL.**

- Container file systems are ephemeral and min instances is 0. Without the table, every cold start, revision and second instance would mint a new key ring, signing everyone out and failing antiforgery across instances.
- Keys are unencrypted at rest: Linux has no DPAPI and no certificate is provisioned. Expect "No XML encryptor configured" once per new key.

**Forwarded headers.**

- TLS ends at Google's front end. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, set on the service, makes `Request.IsHttps` true, so cookies carry `Secure`.
- Redirects are relative, so the Host header never matters.

**`DB_*` over TLS, with a pool cap.**

- Cloud Run maps one secret to one env var, so the connection comes from `DB_HOST`, `DB_PORT` (3306), `DB_USER`, `DB_PASS` and `DB_NAME`, the CI4 app's contract.
- The string is built with `MySqlConnectionStringBuilder`, never by concatenation, because generated passwords contain punctuation.
- `DB_HOST` wins over `ConnectionStrings:Default`: `appsettings.json` ships a localhost default, and the test fixture pins `DB_HOST` empty.
- `SslMode=Required`, because users are `REQUIRE SSL` and the server certificate has no SAN for the NLB address. Never use VerifyCA or VerifyFull.
- `MaximumPoolSize=10` equals Cloud Run's `max_instance_request_concurrency` (10), and the two change together: MySql.Data's pool stalls requests in steps of about a second once they outnumber its connections. Two instances stay at no more than 20 connections, on a server shared by 12 apps.

**Migrations run before traffic moves.**

- The deploy job updates and executes `flyvipcms-net-migrate` (args `["migrate"]`, `max_retries = 0`) before it deploys the candidate revision. The service runs with `SKIP_MIGRATIONS=1`.
- The previous revision keeps serving on the migrated schema, so every migration must stay compatible with the previous release.

**Deploy flow.**

- On a push to `main`: quality → e2e → deploy.
  - e2e tests the production image itself and hands that exact image to deploy as an artifact, so deploy never rebuilds.
  - deploy runs: push `:<sha12>` and `:latest`, migrate job, `--no-traffic --tag candidate`, anonymous smoke of the candidate, `--to-latest`, anonymous smoke of https://flyvipcms-net.web.app. The anonymous smoke's wrong-password POST proves `__session` survives the round trip through Hosting.
  - CI's MySQL services carry a `mysqladmin ping` health check that the runner waits for; a TCP probe passes against Docker's port proxy before mysqld is ready.
- Roll back with `gcloud run services update-traffic flyvipcms-net --to-revisions=<revision>=100 --region europe-west2 --project firebase-cloud-491613`. Terraform ignores traffic, so the rollback sticks.

**Terraform owns the service and job definitions.**

- Terraform ignores only the image, plus traffic on the service.
- So the pipeline changes only the image, traffic and tags. Env, secrets, args and memory change in firebase-cloud, or the next apply reverts them.

**Trimmed image.**

- The image is self-contained, `PublishTrimmed` with `TrimMode=partial`, on `runtime-deps:10.0-noble-chiseled`: no shell, UID 1654.
- Partial trimming works only with the twelve MVC, Razor, Antiforgery and DataAnnotations assemblies rooted, plus `MvcEnhancedModelMetadataSupport=true` and `JsonSerializerIsReflectionEnabledByDefault=true`. Without them, every page returns 500.
- Fix a trimming break with a `TrimmerRootAssembly`, never with a code workaround.
- `BouncyCastle.Cryptography` is marked trimmable. MySql.Data is kept whole, so everything it calls survives; the trimmer reports 0 warnings in it; this saves about 2 MB.
- The build stage deletes `createdump`, `libmscordaccore.so`, `libmscordbi.so`, `libcoreclrtraceptprovider.so` and `libclrgc*.so`: debugger, tracing and alternate-GC files, about 2.6 MB. The `rm` is strict, so a missing file fails the build.
- The trim settings live in csproj groups conditioned on `PublishTrimmed=true`, so normal builds and tests are untouched.
- Use `PublishDocumentationFile=false`, never `GenerateDocumentationFile=false`: its warning is an error here.
- The Docker restore must see the same RID, `SelfContained` and `PublishTrimmed` values as the publish.
- The build context is an allowlist that includes `.editorconfig`. Its `generated_code = true` section keeps CA1861 out of the generated migrations under warnings-as-errors.

**Not AOT, ReadyToRun or single-file.**

- MVC and Razor don't support native AOT.
- ReadyToRun makes the image bigger.
- Compressed single-file is 53 MB and doubles RSS.

**amd64 only.**

- Cloud Run runs amd64. The Dockerfile cross-compiles with `--platform=$BUILDPLATFORM` and `-a $TARGETARCH`, so building on Apple Silicon needs no emulation.
- Running the amd64 image locally under QEMU needs `DOTNET_EnableWriteXorExecute=0`. This is emulator-only.

**Size budget.**

- CI fails when `docker save | gzip | wc -c` of the image exceeds `IMAGE_BUDGET_BYTES`, which is 28,000,000: the measured 24.9 MB amd64 plus 10%, rounded up.
- Recalibrate it when the base image or the trim set changes.

**Onboarding is two firebase-cloud PRs.**

- Cloud Run resolves `secret_key_ref: latest` at create time, and personal-cloud writes the DB secret versions only after the first apply. The admin password version is added by hand in between.
- Removing the catalog entry makes personal-cloud drop the database.

**Narrow roles.**

- The CI/CD service account holds `run.admin`, `artifactregistry.writer`, `iam.serviceAccountUser` and `serviceusage.serviceUsageConsumer`, and no secret access.
- The runtime account reads exactly its five secrets.
- The WIF condition is the exact, case-sensitive `assertion.repository == 'poly-glot/flyvipcms_net'`.

**gcloud.** Every command passes `--project=firebase-cloud-491613` explicitly, and every Cloud Run command passes `--region=europe-west2`, so none depends on the ambient config.

**First admin.**

- `gcloud run jobs execute flyvipcms-net-migrate --args=create-admin,admin,<email> --region europe-west2 --project firebase-cloud-491613 --wait`, with `ADMIN_PASSWORD` from the `flyvipcms-net-admin-password` secret.
- `create-admin` drops a trailing CR/LF from `ADMIN_PASSWORD`, because a secret piped from `openssl` ends with a newline that nobody types.
- Production has no demo data: `DemoSeeder` refuses Production.

## Deliberately not ported

Whatever the CI4 rebuild already dropped (self-registration, friends/groups, `/cron`, avatars, round-trip booking) stays dropped. Additionally not ported here: the database-backed session store (`SessionCookieJar` carries the cookies inside `__session` instead), and PHP's `Money` string arithmetic (C# `decimal` is exact; `MoneyTest` has no equivalent).
