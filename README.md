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

### Prerequisites

| Tool | Notes |
|---|---|
| .NET 10 SDK | Included with Visual Studio 2026 |
| SQL Server LocalDB | Included with Visual Studio's *ASP.NET and web development* workload. The connection string in `LMS.API/appsettings.json` uses `(localdb)\mssqllocaldb`. |
| Azurite (local Blob Storage) | Included with Visual Studio's Azure tools. In Rider, on Linux or on macOS, install it with `npm install -g azurite` (requires Node.js). |

### 1. Set the user secrets

The secrets are never stored in the repository. Run from the repository root:

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "<at least 32 characters>" --project LMS.API
dotnet user-secrets set "password" "<password for the seeded users>" --project LMS.API
```

Choose your own. It is only used for your local database.

### 2. Start the solution

Start both `LMS.API` and `LMS.Blazor`. In Visual Studio: right-click the solution, choose **Configure Startup Projects**, select **Multiple startup projects** and set both to **Start**.

| Project | URL |
|---|---|
| LMS.Blazor (the app) | https://localhost:7249 |
| LMS.API (Swagger) | https://localhost:7003/swagger |

The first time the API starts, it:

1. Starts Azurite if it is not already running. Visual Studio's own copy is used first, then a global npm install. Azurite opens in its own window and keeps running after the API stops.
2. Creates the local database `LMS`, applies the migrations and seeds test data.
3. Creates the blob container `documents-dev` and uploads the files for the seeded submissions.

Running `Update-Database` is not needed.

### 3. Log in

All seeded users have the password you set in step 1.

| Role | Email | Course |
|---|---|---|
| Teacher | admin@lms.com | – |
| Student | anna.andersson@lms.com | Lexicon LTU |
| Student | johan.nilsson@lms.com | Lexicon GBG |

Anna and Johan have test assignments with every status: upcoming, handed in on time with feedback, handed in late, and no submission required.

### Where local data is stored

| Data | Location |
|---|---|
| Database | LocalDB, database `LMS` |
| Uploaded files (Azurite) | Windows: `%LOCALAPPDATA%\azurite-lms` · Linux and macOS: `~/.local/share/azurite-lms` |

To start over with fresh test data, delete the database. The blob container is emptied automatically when the database is created again:

```bash
dotnet ef database drop --project LMS.Infrastructure --startup-project LMS.API
```

### Troubleshooting

| Error when the API starts | What to do |
|---|---|
| `Azurite was not found` | Install the Azure tools in Visual Studio, or run `npm install -g azurite`. After an npm install, restart Visual Studio so it picks up the new PATH. |
| `Azurite was started but did not listen on port 10000 within 60 seconds` | Read the Azurite window for the error. Another program may be using port 10000, or the Azurite data was written by a newer Azurite version: delete the Azurite data folder **and** the database (see above). |
| `JwtSettings:SecretKey` or `password` is missing | Set the user secrets in step 1. |
| A connection error to `(localdb)\mssqllocaldb` | Check that LocalDB is installed: `sqllocaldb info` should list `MSSQLLocalDB`. |

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
