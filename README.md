# Generic CRUD Engine — Phase 1

One controller (`CrudController`) + one set of views drive Create/Edit/Delete/List
for every entity in `AppDbContext`, with auto-detected FK dropdowns.

## Included in Phase 1
- EF Core metadata detection (`EntityMetadataService`) — PK, FKs, MaxLength, required, types
- Generic list, create, edit, delete via reflection (no per-entity code)
- FK dropdowns auto-populated from the principal entity (Name/Title/Description/Code/first string prop)
- DataAnnotations validation (Required, MaxLength, Range, EmailAddress, etc.) via `Validator.TryValidateObject`
- Bootstrap 5 UI, client-side table search

## Deferred to Phase 2
Sorting, server-side search/pagination, file upload persistence, Details page,
concurrency/error-page polish, ValidationService/DynamicQueryService split.

## Run it

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add Init
dotnet ef database update
dotnet run
```

Then visit:
- `/Crud/Department` — create a few departments first (Employee has a required FK to Department)
- `/Crud/Employee` — create/edit/delete employees, Department renders as a dropdown automatically

## Add a new entity — zero controller/view changes required
1. Add the POCO to `Models/`, with `[Key]`, `[Required]`, `[MaxLength]` etc. as needed.
2. Add `public DbSet<YourEntity> YourEntities => Set<YourEntity>();` to `Data/AppDbContext.cs`.
3. `dotnet ef migrations add Add_YourEntity && dotnet ef database update`
4. Visit `/Crud/YourEntity` — list/create/edit/delete/FK-dropdowns all work immediately.

Connection string is in `appsettings.json` (LocalDB by default) — point it at your SQL Server instance.

## DynamicCrud engine (DB-config-driven, separate from the reflection engine above)

Configure an entity entirely from the database — no C# model, no migration per entity:

1. `dotnet ef migrations add AddDynamicCrudConfig && dotnet ef database update`
   (adds the `DynamicEntities`/`DynamicFields`/`DynamicFieldValidations`/`DynamicFieldOptions`/
   `DynamicForeignKeys`/`DynamicFileConfigs`/`DynamicFieldSections` config tables).
2. Go to `/DynamicConfig` → **New Entity**. `EntityName` must match an existing physical table.
3. Open **Fields** for that entity and add one row per column: label, input type, form/table
   order, show-in-form/table, required, validation, static options or an FK source, file-upload
   settings, section grouping, and conditional display (show only when another field = X).
4. Visit `/DynamicCrud/Index/{EntityName}` — full list/create/edit/delete is live immediately.
   Metadata is cached 30 minutes and invalidated automatically whenever you save config changes.

Under the hood `DynamicCrudService` talks to the physical table via raw parameterized ADO.NET
(there's no POCO for these entities), so any table already in your database works without a
matching C# class or a new migration.
