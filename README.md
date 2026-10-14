# Lexicon LMS

A learning management system for Lexicon's courses. It brings together course structure, schedules, course material, assignments, submissions, feedback and an overview for two roles: **Student** and **Teacher**.

The guiding principle is *less is more*: the core features should work reliably and be easy to understand before anything extra is added.

## Tech stack

| Area | Technology |
|---|---|
| Framework | .NET 10 |
| Frontend | Blazor Web App with BFF (YARP), Bootstrap 5 |
| Backend | ASP.NET Core Web API |
| Data access | Entity Framework Core (code first) |
| Database | SQL Server / Azure SQL |
| File storage | Azure Blob Storage |
| Testing | xUnit v3, Moq |
| Infrastructure | Bicep, Azure Container Apps, Azure Container Registry, Azure Key Vault |
| CI/CD | GitHub Actions |

## Running locally

1. Set the user secrets for `LMS.API`: `JwtSettings:SecretKey` (at least 32 characters) and `password` (password for the seeded users).
2. Start `LMS.API` and `LMS.Blazor`.

The API creates the local database, applies the migrations and seeds test users the first time it starts. Running `Update-Database` is not needed.

## Running the tests

```bash
dotnet test
```

From the repository root. `global.json` makes `dotnet test` use Microsoft.Testing.Platform, which xUnit v3 requires on .NET 10.

## Documentation

| Document | Contents |
|---|---|
| [Architecture](docs/architecture.md) | Project structure, project dependencies, request flow, validation and error handling |
| [ER diagram](docs/er-diagram.md) | Data model and business rules |
| [Deployment](docs/deployment.md) | Environments, pipelines and how code reaches Azure |
| [Azure setup](docs/azure-setup.md) | Creating, moving and removing the Azure resources, secrets and troubleshooting |
| [Contributing](CONTRIBUTING.md) | Branching, pull requests, code review and Definition of Done |
