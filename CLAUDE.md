# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

AnyFood is an ASP.NET Core (.NET 10) REST API backing an Angular frontend. It handles products with nutrition and price data, recipes, day plans, shopping lists and a per-user fridge. The data store is PostgreSQL via EF Core (Npgsql). Unit tests (xUnit) live in `Application.Tests`, which tests Application services against hand-written fakes.

## Commands

```bash
dotnet build AnyFood.sln                       # build all projects
dotnet test Application.Tests                  # run unit tests
dotnet test Application.Tests --filter "FullyQualifiedName~UserServiceBodyParamsTests"  # one class (or a method name)
dotnet run --project API                       # run API (needs Postgres on localhost:5432, db "anyfood", postgres/postgres)
docker compose up -d db                        # start only Postgres for local dev
docker compose up --build                      # full stack: db + API + nginx on http://localhost:8080

# EF Core migrations (DbContext lives in Infrastructure, startup project is API)
dotnet ef migrations add <Name> --project Infrastructure --startup-project API --output-dir Persistence/Migrations
dotnet ef database update --project Infrastructure --startup-project API
```

Migrations are applied automatically on startup (`db.Database.MigrateAsync()` in `API/Program.cs`), and the Admin role and admin user are seeded at the same point. Swagger UI is always enabled at `/swagger`.

## Architecture

The code follows a Clean Architecture layout, with project references running `API → Application + Infrastructure`, `Infrastructure → Application`, `Application → Domain`:

- **Domain**: EF entities (`Domain/Entities`) and `BaseEntity`, which collects MediatR `INotification` domain events.
- **Application**: one folder per feature (`Products/`, `Recipes/`, `DayPlans/`, `ShoppingList/`, `Fridge/`, …), each containing `DTOs/`, `Interfaces/` (`I<X>Service`, `I<X>Repository`) and `Services/`. Business logic lives in the services. Repository interfaces are declared here.
- **Infrastructure**: repository implementations (one folder per feature), `Persistence/AppDbContext.cs` (an `IdentityDbContext<User>`), entity configurations picked up through `ApplyConfigurationsFromAssembly`, migrations and seeders.
- **API**: controllers, `ExceptionHandlingMiddleware` and `Program.cs`. All DI registration is done by hand in `Program.cs`; a new service or repository has to be registered there.

### Conventions that span layers

- **Error handling through exceptions**: services throw `KeyNotFoundException` (→404), `InvalidOperationException` (→400) and `UnauthorizedAccessException` (→401). The middleware maps them to `{ "message": ... }`, and any other exception becomes a 500. Controllers return `Ok(...)` directly and do not check for null. User-facing messages are written in Ukrainian, and so are many code comments.
- **JSON enums** are serialized and accepted as camelCase strings only (`JsonStringEnumConverter` with `allowIntegerValues: false`, configured globally in `Program.cs`). Store enum columns as strings (`HasConversion<string>()`).
- **Time-dependent logic** takes the registered `TimeProvider` singleton so tests can pin "now" (see `UserService`).
- **Mapping**: Mapster `.Adapt<T>()`. Custom maps live in `Application/Mapping/MappingConfig.cs`, which is called once at startup. Add `Ignore` rules there when a mapping would overwrite navigation or computed properties.
- **Generic CRUD**: `API/Base/BaseController` together with `IBaseService`/`IBaseRepository` in `Application/Base` provides the standard CRUD endpoints for simple entities such as categories.
- **Auth**: JWT bearer tokens (`Jwt` config section) plus Google ID-token login (`AuthService.LoginWithGoogleAsync`). Controllers read the user from `ClaimTypes.NameIdentifier` and check the `"Admin"` role through `User.IsInRole("Admin")`.

### Non-obvious domain behavior

- **Product hierarchy**: products form a tree through `ParentProductId`, and `Path` is a materialized path such as `"3.17.42"` used for cycle checks. Each product stores `Own*` values (its own protein/fat/carbs/price/GI) and effective values (`Protein`, `Price`, …). A non-leaf product's effective values are the average of its children's values, recalculated up the ancestor chain by `ProductService.RecalculateChainAsync`. Nutrition and price cannot be edited on a product that has children.
- **Recipe price is maintained in Postgres**: a database trigger (`Infrastructure/Persistence/Scripts/Triggers/recalculate_recipe_prices.sql`) recalculates `Recipes.Price` whenever `Products.Price` changes. The `.sql` files are embedded resources, and migrations load them with `GetManifestResourceStream` (see the `AddProductPriceTrigger` migration). Changes to the trigger need a new migration.
- **Price history**: `ProductPriceHistoryInterceptor` (an EF `SaveChangesInterceptor`) writes `ProductPriceHistory` rows when products are created or their price changes. MediatR is referenced and `ProductPriceChangedHandler` exists, but `AddMediatR` is never called, so domain events are not dispatched.
- **Shopping list generation**: `ShoppingListService` runs every registered `IShoppingSourceResolver` (Recipe, Product and DayPlan resolvers), then aggregates ingredient weights per product. To add a new source, implement the resolver and register it in `Program.cs`.
- **Images**: `ImageProcessingService` hashes the upload (first 16 hex characters of its SHA-256) and writes WebP variants at widths 200/500/1200 to `Images:StoragePath` (default `/app/data/images`), using paths of the form `/images/{hash}/{width}.webp`. The `Enqueue` path hands the job to `ImageProcessingWorker` (a hosted service reading from a `Channel`), which calls back with a scoped `IServiceProvider` when processing finishes. In Docker, nginx serves `/images/` directly from the shared volume and proxies everything else to the API. Image processing uses SixLabors ImageSharp, and the license file `Application/sixlabors.lic` is gitignored.
