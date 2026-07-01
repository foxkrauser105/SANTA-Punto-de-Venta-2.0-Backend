# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build the solution
dotnet build SANTA.PoS.sln

# Run the API (from repo root)
dotnet run --project SANTA.PoS/SANTA.PoS.csproj

# Run all tests
dotnet test SANTA.PoS.sln

# Run a single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Add an EF Core migration (run from SANTA.PoS/ — it hosts the DbContext design-time factory)
dotnet ef migrations add <MigrationName> --project ../SANTA.PoS.Data/SANTA.PoS.Data.csproj

# Apply pending migrations
dotnet ef database update --project ../SANTA.PoS.Data/SANTA.PoS.Data.csproj
```

## Architecture

Four projects with a strict dependency direction: `SANTA.PoS` → `SANTA.PoS.Business` / `SANTA.PoS.Data` → `SANTA.PoS.Domain`.

```
SANTA.PoS          (API)       — Controllers, Middleware, Program.cs, DI wiring
SANTA.PoS.Business (Logic)     — Services, Interfaces, DTOs, AutoMapper profiles, Helpers
SANTA.PoS.Data     (Data)      — EF Core SantaContext, Repository implementations, Migrations
SANTA.PoS.Domain   (Domain)    — Entity classes, DomainException
SANTA.PoS.Tests    (Tests)     — xUnit unit tests for all services (Moq + real AutoMapper)
```

The **API project** references both **Business** and **Data** for DI registration purposes. **Business** interfaces live alongside services — repositories implement those interfaces but live in the **Data** project.

## Key Patterns

### Repository pattern
`IBaseRepository<TClass, TKey>` declares generic CRUD. `BaseRepository<TClass, TKey>` implements it via EF Core. Domain-specific repositories (e.g., `VentaRepository`) extend `BaseRepository` and override methods that need eager-loading (`Include`/`ThenInclude`). Register each pair (`IXxxRepository` / `XxxRepository`) as `Scoped` in `Program.cs`.

### Service layer
Services (`ProductService`, `VentaService`) contain all business logic. They receive repository interfaces and `IMapper` via primary constructor injection. Services throw `DomainException` for business-rule violations; the global `ExceptionHandlingMiddleware` catches it and returns HTTP 409 Conflict with a JSON `ErrorResponse`.

### DTOs
Use C# `record` types. Read DTOs are named `XxxDto`, create/update inputs use `CreateXxxDto` / `UpdateXxxDto`. Update DTOs use nullable properties so partial patches are possible (only non-null fields are applied).

### AutoMapper
All mappings are declared in `MappingProfile`. When a DTO needs a property from a navigation property, use `ForMember` explicitly. **AutoMapper 16 constraint:** positional `record` DTOs that have a constructor parameter mapped with `opt.Ignore()` must give that parameter a default value (e.g., `= null`) so AutoMapper can construct the record without it. See `NotaCreditoDto.Items`.

### PropertyUpdateHelper
`PropertyUpdateHelper.UpdateEntityFromDto<TEntity, TDto>` copies non-null DTO values to an entity by reflection. `UpdateEntityFromDtoSelective` does the same but only for an explicit list of property names. Use these in services to avoid writing repetitive null-check assignments.

### Transactional sale creation
`VentaRepository.CreateVentaWithDetallesAsync` is the only place that manually manages a DB transaction: it inserts the `Venta`, inserts each `RegistroVenta`, then decrements `Producto.Cantidad` (floor at 0), and rolls back the whole operation on any failure.

## Database

SQL Server Express (`localhost\SQLEXPRESS`, database `SANTA`). Connection string is in `appsettings.Development.json`. Collation is `Modern_Spanish_CI_AS`.

### Producto dual-key design
`Producto` has two identifiers:
- `Id` (int, auto-generated PK) — used as the FK in `RegistroVenta`, `Descuento`, etc.
- `IdProducto` (string, barcode) — the human-facing product code used in API routes and `CreateRegistroVentaDto`.

When creating a `RegistroVenta`, `VentaService` resolves `IdProducto` → `Id` by calling `IProductRepository.GetByIdAsync(registroDto.IdProducto)` before building the entity.

## Adding a New Feature

Follow the existing pattern exactly:

1. **Domain** — add entity class in `SANTA.PoS.Domain/Entities/`; register `DbSet` in `SantaContext` and add `modelBuilder.Entity<>` configuration.
2. **Business** — add `IXxxRepository` in `SANTA.PoS.Business/Interfaces/` (extending `IBaseRepository`), add DTOs in `SANTA.PoS.Business/DTOs/`, add mappings in `MappingProfile`, add `XxxService`.
3. **Data** — add `XxxRepository` in `SANTA.PoS.Data/Repositories/` extending `BaseRepository`.
4. **API** — add `XxxController`, register `IXxxRepository`/`XxxRepository` and `XxxService` as `Scoped` in `Program.cs`.
5. Run `dotnet ef migrations add` to capture schema changes.

## Testing

Tests live in `SANTA.PoS.Tests/Services/`. Run them with `dotnet test SANTA.PoS.sln`.

- **Framework:** xUnit — `using Xunit;` must be added explicitly (not in implicit usings).
- **Mocking:** Moq 4.x — `new Mock<IInterface>()`.
- **AutoMapper in tests:** Use `MapperFactory.Create()` from `SANTA.PoS.Tests/Helpers/MapperFactory.cs`, which creates a real mapper with `MappingProfile` via `NullLoggerFactory` (required by AutoMapper 16).
- One test class per service, one success + one failure path minimum per method.
- See `.claude/commands/test.md` for naming conventions and full guidelines.

## Rules
- Never use `!collection.Any()` to check emptiness. Use `.Count == 0` for `IList<T>`/`ICollection<T>`, or `.Count() == 0` for bare `IEnumerable<T>`.
- JWT auth is required on all endpoints (`[Authorize]`). Admin-only mutations use `[Authorize(Roles = "Admin")]`.
- `descuento` columns in `registro_ventas` and `registro_notas_credito` are `BIT` (C# `bool`), not `int`.
- NC01 and NC02 must exist as rows in the `productos` table before credit-note payments can be processed.