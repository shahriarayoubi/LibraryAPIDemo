# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 10 ASP.NET Core Web API for library management (books, authors, copies, borrowers, loans), backed by EF Core / SQL Server. It's an early-stage project: only `Books` and `Authors` have controllers so far; `BookCopy`, `Borrower`, and `Loan` exist only as data-layer entities with no API surface yet. There is no test project (the `tests/` directory is empty).

## Solution structure

- `LibraryAPI.slnx` — solution file (new XML-based slnx format, not `.sln`).
- `src/Library.Api` — ASP.NET Core Web API host: controllers, request/response DTOs, `Program.cs`.
- `src/Library.Data` — EF Core layer: `LibraryDbContext`, entities, `IEntityTypeConfiguration<T>` classes, migrations.

`Library.Api` references `Library.Data`; there is no separate domain/service layer — controllers talk to `LibraryDbContext` directly.

## Common commands

Run all commands from the repository root unless noted.

```powershell
# restore & build
dotnet restore
dotnet build

# run the API (from src/Library.Api)
dotnet run --project src/Library.Api

# EF Core migrations (dotnet-ef is a local tool, restored via dotnet-tools.json)
dotnet tool restore
dotnet ef migrations add <Name> --project src/Library.Data --startup-project src/Library.Api
dotnet ef database update --project src/Library.Data --startup-project src/Library.Api
```

There is no test project yet, so there is no `dotnet test` target to run.

`src/Library.Api/Library.Api.http` contains example HTTP requests usable with the VS/Rider/VS Code REST client.

## Architecture notes

**DbContext and migrations assembly split.** `LibraryDbContext` lives in `Library.Data`, but migrations are generated into that same project while `UseSqlServer` is configured in `Library.Api`'s `Program.cs` with `sqlServerOptions.MigrationsAssembly(typeof(LibraryDbContext).Assembly.FullName)`. Any `dotnet ef` command needs `--project src/Library.Data --startup-project src/Library.Api`.

**Schema-per-module convention.** Entity configurations put tables into SQL Server schemas by bounded area, not all in `dbo`:
- `catalog` schema: `Books`, `Authors`, `BookCopies`
- `circulation` schema: `Borrowers`, `Loans`

Keep new entities' `ToTable(...)` calls consistent with this split.

**Auditing via `IAuditableEntity`.** `LibraryDbContext.SaveChanges`/`SaveChangesAsync` override the base methods to stamp `CreatedUtc`/`UpdatedUtc` on any tracked entity implementing `IAuditableEntity` (`src/Library.Data/Common/IAuditableEntity.cs`). `Book` and `Author` implement this normally. **`BookCopy`, `Borrower`, and `Loan` currently implement the interface's properties by throwing `NotImplementedException`** — this looks like leftover scaffolding rather than intentional behavior; be aware that calling `SaveChanges` while any of these three entities are tracked as Added/Modified will throw. If you touch these entities, this is very likely something to fix (give them real auto-properties like `Book`/`Author`), not a constraint to work around.

**Configuration-per-entity via `ApplyConfigurationsFromAssembly`.** Each entity has a matching `internal sealed class XConfiguration : IEntityTypeConfiguration<X>` in `src/Library.Data/Configurations/`, picked up automatically in `OnModelCreating`. Follow this pattern (max lengths, indexes, check constraints, `OnDelete(DeleteBehavior.Restrict)` on FKs) rather than configuring entities via data annotations.

**Business rules enforced at the database level**, not in application code — check these when reasoning about invariants:
- One active loan per copy: unique filtered index `UX_Loans_OneActiveLoanPerCopy` on `Loans.BookCopyId` where `ReturnedUtc IS NULL`.
- Check constraints on year ranges (`Authors`, `Books`), due/return date ordering and non-negative renewal count (`Loans`).
- Unique filtered indexes for optional-but-unique columns (e.g. `Books.Isbn`, `Borrowers.IdentityUserId`) using `HasFilter("[Col] IS NOT NULL")`.
- `BookCopy.Status` is a C# enum persisted as a string (`HasConversion<string>()`), not an int.

**Controller pattern.** `BooksController`/`AuthorsController` are minimal, non-abstracted CRUD controllers: they inject `LibraryDbContext` directly, use `AsNoTracking()` for reads, manually validate FK existence (e.g. `AuthorId` on book create/update) before touching the DB, and hand-map entities to `Response` DTOs via a private static `MapToResponse` method. New controllers should follow this same shape rather than introducing a repository/service abstraction, unless asked to refactor.

**Request/response DTOs** live under `src/Library.Api/Models/{Books,Authors}/` as separate `CreateXRequest`/`UpdateXRequest`/`XResponse` records/classes per entity — there's no shared base or AutoMapper; mapping is explicit.

**Connection string**: `ConnectionStrings:LibraryAPIDatabase`, configured in `appsettings.Development.json` for local dev (Windows auth against `Server=.`). `Program.cs` throws on startup if it's missing.
