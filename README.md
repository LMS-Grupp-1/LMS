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
| Testing | xUnit, Moq |
| CI/CD | GitHub Actions, Azure |

## Running the tests

```bash
dotnet test
```

## Documentation

| Document | Contents |
|---|---|
| [Architecture](docs/architecture.md) | Project structure, project dependencies, request flow, validation and error handling |
| [ER diagram](docs/er-diagram.md) | Data model and business rules |
| [Deployment](docs/deployment.md) | Environments and CI/CD pipeline |
| [Contributing](CONTRIBUTING.md) | Branching, pull requests, code review and Definition of Done |
