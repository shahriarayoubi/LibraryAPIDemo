# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 10 ASP.NET Core Web API for library management (books, authors, copies, borrowers, loans), backed by EF Core / SQL Server, with ASP.NET Core Identity + JWT authentication layered on top. A React 19 + TypeScript frontend (`client/`) is being built incrementally against this API. There is a starter xUnit test project (`tests/Library.Api.Tests`).

## Solution / repo structure

- `LibraryAPI.slnx` — the .NET solution file (new XML-based slnx format, not `.sln`). Only `.csproj` projects belong here.
- `src/Library.Api` — ASP.NET Core Web API host: controllers, request/response DTOs, auth/security code, `Program.cs`.
- `src/Library.Data` — EF Core layer: `LibraryDbContext`, entities, `IEntityTypeConfiguration<T>` classes, migrations.
- `tests/Library.Api.Tests` — xUnit tests.
- `client/` — React 19 + TypeScript + Vite frontend. Deliberately kept as a sibling of `src/`/`tests/`, not inside `src/`, since it has its own Node toolchain (`node_modules`, `package.json`) that has nothing to do with the .NET build — `dotnet build`/`dotnet restore` never need to touch it, and it carries its own `.gitignore`.

`Library.Api` references `Library.Data`; there is no separate domain/service layer — controllers talk to `LibraryDbContext` directly.

## Common commands

Run all commands from the repository root unless noted.

```powershell
# .NET: restore & build
dotnet restore
dotnet build

# .NET: run the API
dotnet run --project src/Library.Api

# .NET: EF Core migrations (dotnet-ef is a local tool, restored via dotnet-tools.json)
dotnet tool restore
dotnet ef migrations add <Name> --project src/Library.Data --startup-project src/Library.Api
dotnet ef database update --project src/Library.Data --startup-project src/Library.Api
```

```powershell
# React client (run from client/)
npm install
npm run dev      # Vite dev server, http://localhost:5173
npm run build    # tsc -b && vite build
```

To develop against a live API, run the .NET API (`dotnet run --project src/Library.Api`) and the Vite dev server (`npm run dev` in `client/`) side by side in two terminals. The dev-only CORS policy (see below) only allows `http://localhost:5173`, so keep the client on Vite's default port in development.

`src/Library.Api/Library.Api.http` contains example HTTP requests usable with the VS/Rider/VS Code REST client. In `Development`, the API also exposes an interactive endpoint browser via Scalar at `/scalar/v1` (backed by the OpenAPI document at `/openapi/v1.json`).

## Architecture notes

**DbContext and migrations assembly split.** `LibraryDbContext` lives in `Library.Data`, but migrations are generated into that same project while `UseSqlServer` is configured in `Library.Api`'s `Program.cs` with `sqlServerOptions.MigrationsAssembly(typeof(LibraryDbContext).Assembly.FullName)`. Any `dotnet ef` command needs `--project src/Library.Data --startup-project src/Library.Api`.

**Schema-per-module convention.** Entity configurations put tables into SQL Server schemas by bounded area, not all in `dbo`:
- `catalog` schema: `Books`, `Authors`, `BookCopies`
- `circulation` schema: `Borrowers`, `Loans`

ASP.NET Core Identity's own tables (`AspNetUsers`, `AspNetRoles`, etc., added by `AddEntityFrameworkStores<LibraryDbContext>()`) are **not** part of this convention — they sit in the default `dbo` schema since Identity's own `IdentityDbContext` conventions configure them, not a custom `IEntityTypeConfiguration<T>`. Keep new domain entities' `ToTable(...)` calls consistent with the `catalog`/`circulation` split.

**Auditing via `IAuditableEntity`.** `LibraryDbContext.SaveChanges`/`SaveChangesAsync` override the base methods to stamp `CreatedUtc`/`UpdatedUtc` on any tracked entity implementing `IAuditableEntity` (`src/Library.Data/Common/IAuditableEntity.cs`). All five domain entities (`Author`, `Book`, `BookCopy`, `Borrower`, `Loan`) implement it with plain auto-properties.

**Configuration-per-entity via `ApplyConfigurationsFromAssembly`.** Each entity has a matching `internal sealed class XConfiguration : IEntityTypeConfiguration<X>` in `src/Library.Data/Configurations/`, picked up automatically in `OnModelCreating`. Follow this pattern (max lengths, indexes, check constraints, `OnDelete(DeleteBehavior.Restrict)` on FKs) rather than configuring entities via data annotations.

**Business rules enforced at the database level**, not in application code — check these when reasoning about invariants:
- One active loan per copy: unique filtered index `UX_Loans_OneActiveLoanPerCopy` on `Loans.BookCopyId` where `ReturnedUtc IS NULL`.
- Check constraints on year ranges (`Authors`, `Books`), due/return date ordering and non-negative renewal count (`Loans`).
- Unique filtered indexes for optional-but-unique columns (e.g. `Books.Isbn`, `Borrowers.IdentityUserId`) using `HasFilter("[Col] IS NOT NULL")`.
- `BookCopy.Status` is a C# enum persisted as a string (`HasConversion<string>()`), not an int.
- `LoanRequest`/`CreateLoanRequest` mirror the `Loans` date-ordering check constraints with `IValidatableObject`, so violations come back as a 400 with a clear message instead of a raw DB constraint failure.

**Controller pattern.** `BooksController`, `AuthorsController`, `BookCopiesController`, `BorrowersController`, and `LoansController` are minimal, non-abstracted CRUD controllers: they inject `LibraryDbContext` directly, use `AsNoTracking()` for reads, manually validate FK existence (e.g. `AuthorId` on book create/update, `BookCopyId`/`BorrowerId` on loan create/update) before touching the DB, and hand-map entities to `Response` DTOs via a private static `MapToResponse` method. New controllers should follow this same shape rather than introducing a repository/service abstraction, unless asked to refactor. `AuthenticationController` and `MeController` are the exception — see below.

**Request/response DTOs** live under `src/Library.Api/Models/{Books,Authors,BookCopies,Borrowers,Loans}/` as separate `CreateXRequest`/`UpdateXRequest`/`XResponse` classes per entity — there's no shared base or AutoMapper; mapping is explicit. `BorrowerRequest` deliberately omits `IdentityUserId` (clients can't set which Identity user a borrower is linked to — that's wired up server-side during registration) even though `BorrowerResponse` exposes it read-only. `MeController` is an outlier: it returns anonymous objects instead of dedicated response DTOs — if you extend it, consider whether it's worth bringing in line with the rest.

**Authentication & authorization.** ASP.NET Core Identity (`IdentityUser`/`IdentityRole`, via `AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<LibraryDbContext>()`) plus JWT bearer tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`), **not** cookies — the client is expected to send `Authorization: Bearer <token>`.
- `AuthenticationController` (`api/authentication`): `POST /login` checks credentials via `UserManager`/`JwtTokenService` and returns `{ userName, token }`; `POST /register` creates an `IdentityUser` *and* a linked `Borrower` row (with a generated `MembershipNumber`) in one DB transaction via `dbContext.Database.CreateExecutionStrategy()` (required because `EnableRetryOnFailure` is on — manual transactions must go through the execution strategy, not a bare `BeginTransactionAsync`); `GET /test` is `[Authorize(Roles = RoleNames.Admin)]`, a smoke-test endpoint.
- `Borrower.IdentityUserId` is the link between a `Borrower` row and its `IdentityUser` (nullable — a `Borrower` can exist without a login). `MeController` (`api/me`, `[Authorize]`) resolves "the current borrower" by matching `ClaimTypes.NameIdentifier` from the JWT against `Borrower.IdentityUserId`.
- Two roles exist (`RoleNames.Admin`, `RoleNames.Borrower`). `IdentitySeeder` (`src/Library.Api/Data/IdentitySeeder.cs`) runs on every startup and seeds both roles plus two dev accounts if missing: `admin`/`Admin123!` (Admin role) and `borrower`/`Borrower123!` (Borrower role, but **no** linked `Borrower` row — only accounts created through `/register` get one). Useful for manually testing login without registering first.
- JWT signing key/issuer/audience come from the `Jwt` config section (`JwtOptions`); issuer/audience/expiry are in `appsettings.json`, but the signing `Key` is not — it's expected in user secrets or environment config (the project has a `UserSecretsId`). Startup throws if the `Jwt` section is missing entirely.

**CORS.** No CORS policy exists outside `Development`. In dev, a named policy (`ReactDevClient`, defined inline in `Program.cs`) allows only `http://localhost:5173` (Vite's default port) and is applied via `app.UseCors(...)` inside the `IsDevelopment()` block, before `UseAuthentication()`. When this moves toward a real deployment, this hardcoded dev-only policy will need a real policy (allowed origins from config, scoped per environment) — don't assume it already handles production.

**Connection string**: `ConnectionStrings:LibraryAPIDatabase`, configured in `appsettings.Development.json` for local dev (Windows auth against `Server=.`). `Program.cs` throws on startup if it's missing.

## Frontend (`client/`)

React 19 + TypeScript, scaffolded with Vite (`npm create vite@latest client -- --template react-ts`), currently at a very early stage — deliberately built in small, incremental steps rather than all at once, since this is also a vehicle for learning React. As of now it's just the unmodified Vite starter template; no routing, state management, or styling library has been added yet, and none should be assumed to exist until actually added. Talks to the API over plain `fetch` calls (no generated client/SDK), targeting whichever port `dotnet run --project src/Library.Api` is using locally, with the JWT from `/api/authentication/login` sent as a `Bearer` token for authenticated requests. Don't introduce Tailwind, React Router, a global state library, etc. speculatively — wait until a step actually calls for it.
