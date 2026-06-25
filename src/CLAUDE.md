# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Lazy.Blog is the backend API for a blogging platform (notlazy.org). It's an ASP.NET Core (.NET 10) Web API built with Clean Architecture, CQRS via MediatR, and a functional `Result` error model. SQL Server is the database (EF Core), with Azure Blob Storage for files and Application Insights / OpenTelemetry for observability. The solution lives under `src/`; there is no separate test project.

## Commands

All commands run from `src/` (where `LazyBlog.sln` lives).

```bash
dotnet build LazyBlog.sln
dotnet run --project Lazy.Blog.Api          # starts the API; Scalar UI at /scalar, OpenAPI at /openapi
```

EF Core migrations (the API project holds `Microsoft.EntityFrameworkCore.Tools`, migrations live in `Lazy.Blog.Persistence`):

```bash
dotnet ef migrations add <Name> --project Lazy.Blog.Persistence --startup-project Lazy.Blog.Api
dotnet ef database update           --project Lazy.Blog.Persistence --startup-project Lazy.Blog.Api
```

Note: on startup `Program.cs` calls `ApplyMigrationsAsync(app)`, which applies any pending migrations via `context.Database.MigrateAsync()` in a scoped/async startup scope. A migration failure is fatal (the exception propagates and crashes startup) rather than being swallowed. For multi-instance production deploys, prefer applying migrations out-of-band (`dotnet ef database update` / `efbundle`) before the process starts.

Configuration/secrets: the connection string `ConnectionStrings:Database` plus JWT, Google external auth, SendGrid, and Azure Blob Storage options are expected via User Secrets (`UserSecretsId` in `Lazy.Blog.Api.csproj`) or environment — `appsettings.json` only ships non-secret keys.

## Architecture

Five projects form a strict Clean Architecture dependency chain (dependencies point inward toward Domain):

- **Lazy.Blog.Domain** (`Lazy.Domain.*`) — entities, value objects, domain events, repository *interfaces*, and the `Result`/`Error` primitives. Depends on nothing. Entities are rich: construct via static factory methods (e.g. `Post.Create(...)`) that return `Result<T>`; value objects (`Title`, `Slug`, `Body`, …) self-validate the same way.
- **Lazy.Blog.Application** (`Lazy.Application.*`) — CQRS handlers, FluentValidation validators, and abstraction interfaces. Depends only on Domain.
- **Lazy.Blog.Persistence** (`Lazy.Persistence.*`) — `LazyBlogDbContext` (extends `IdentityDbContext`), EF `IEntityTypeConfiguration` classes, repository *implementations*, `UnitOfWork`, and interceptors. Depends on Domain.
- **Lazy.Blog.Infrastructure** (`Lazy.Infrastructure.*`) — external service implementations: JWT (`JwtProvider`), `CurrentUserContext`, `FileService` (Azure Blob), email (`SendGridEmailSender`). Depends on Application + Persistence.
- **Lazy.Blog.Presentation** (`Lazy.Presentation.*`) — MVC controllers and request/response contracts. Compiled into the API via `AddApplicationPart` (controllers are *not* in the API project). Depends on Application + Infrastructure.
- **Lazy.Blog.Api** — composition root only: `Program.cs` wiring, options setup, auth, OpenAPI, exception handling.

### CQRS request flow

1. A controller in `Lazy.Blog.Presentation/Controllers` maps an HTTP request + contract to a `Command`/`Query` record and sends it via `ISender` (MediatR).
2. MediatR routes to the matching handler in `Lazy.Blog.Application/<Feature>/<Operation>/`. Each operation is its own folder holding the command/query, handler, validator, and response (e.g. `Posts/CreatePost/`).
3. `ValidationPipelineBehavior` runs FluentValidation validators *before* the handler; failures short-circuit into a `ValidationResult` (never throws for validation).
4. Handlers return `Result` / `Result<T>`. Controllers inspect `IsSuccess`/`IsFailure` and use the inherited `HandleFailure(result)` helper (from `ApiController`) to turn failures into RFC-7807 `ProblemDetails`.

When adding a feature: create a new folder under the relevant `Application/<Feature>/`, define the command/query as a `record` implementing `ICommand<T>`/`IQuery<T>`, add a handler implementing `ICommandHandler<,>`/`IQueryHandler<,>`, a validator, and the response; then add the controller action. Mirror an existing folder like `Posts/CreatePost/`.

### Conventions that span files

- **Errors**: never throw for expected failures. Return `Result.Failure(...)` with an `Error` from `Lazy.Blog.Domain/Errors/DomainErrors`. The `Error` record carries a `Code` and `Message`.
- **DI registration is convention-based.** `Program.cs` uses **Scrutor** `.AsMatchingInterface()` to auto-register classes against their same-named interface (`PostRepository` → `IPostRepository`) with scoped lifetime, scanning the Infrastructure, Application, and Persistence assemblies. A new repository/service following the `Foo`/`IFoo` naming is wired up automatically — no manual registration. The DI container validates scopes and validates on build (`ValidateOnBuild = true`), so a missing/mis-scoped dependency fails fast at startup.
- **Repository interface in Domain, implementation in Persistence.** Persistence work goes through `IUnitOfWork.SaveChangesAsync` (handlers call repositories to stage changes, then the unit of work to commit).
- **EF mapping** is via `IEntityTypeConfiguration` classes in `Lazy.Blog.Persistence/Configurations`, auto-applied by `ApplyConfigurationsFromAssembly`. Add a configuration class rather than fluent config in the context.
- **Auth**: ASP.NET Identity (`User`/`Role` with `Guid` keys) backs the DB; cookie auth (`nl.auth`, domain `.notlazy.org`) and JWT bearer coexist. Most controllers derive from `BaseJwtController` (JWT scheme, `[Authorize]`) and opt specific actions out with `[AllowAnonymous]`. Get the caller's id via `ICurrentUserContext`.
- **Auditing**: entities implementing `IAuditableEntity` get timestamps set automatically by `UpdateAuditableEntitiesInterceptor`.
