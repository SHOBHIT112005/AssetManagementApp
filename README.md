# Asset Management Application

A production-focused Asset Management Application built with ASP.NET Core Blazor Server, Entity Framework Core, Dapper, Microsoft SQL Server, and a layered architecture.

The project is designed with two goals in mind:

- Build a maintainable asset management system that can grow toward production use.
- Learn ASP.NET Core, Blazor Server, EF Core, Dapper, SQL Server, clean architecture, and modern UI development through real implementation.

## Project Overview

This application helps manage company employees, assets, and future asset assignments. It currently includes employee management and the foundation for asset CRUD, with a roadmap toward assignment tracking, dashboards, reporting, authentication, seed data, UI animation, and deployment preparation.

Core business goals:

- Maintain employee records.
- Maintain asset inventory records.
- Track asset status and condition.
- Preserve history for deactivated employees and future asset assignments.
- Provide secure user authentication via ASP.NET Core Identity.
- Keep business logic separate from persistence and UI code.

## Technology Stack

| Area | Technology |
| --- | --- |
| Frontend | Blazor Server (.NET 10), HTML5, CSS3 |
| Backend | ASP.NET Core |
| ORM | Entity Framework Core |
| Micro ORM | Dapper |
| Database | Microsoft SQL Server |
| UI Enhancements | Custom CSS, JavaScript Interop |
| Architecture | Clean Architecture |
| Authentication | ASP.NET Core Identity (Cookie-based) |

## Solution Structure

```text
src/
  AssetManagement.Domain
  AssetManagement.Application
  AssetManagement.Infrastructure
  AssetManagement.Web
```

### Project Responsibilities

| Project | Responsibility |
| --- | --- |
| `AssetManagement.Domain` | Core entities and enums such as `Employee`, `Asset`, `AssetAssignment`, `EmployeeStatus`, `AssetStatus`, and `AssetCondition`. |
| `AssetManagement.Application` | Service and repository contracts, plus business logic implemented in services. |
| `AssetManagement.Infrastructure` | Entity Framework Core `DbContext`, repositories, migrations, SQL Server access, and future Dapper read queries. |
| `AssetManagement.Web` | Blazor Server UI, routing, forms, dependency injection setup, and application composition. |

## Architecture

The application follows strict layered architecture.

### Dependency Flow

```text
Web
  |
  v
Application
  |
  v
Infrastructure
  |
  v
Domain
```

### Runtime Flow

```text
Blazor UI
  |
  v
Service
  |
  v
Repository
  |
  v
DbContext
  |
  v
SQL Server
```

### Core Architecture Rules

- UI never accesses `DbContext`.
- UI never communicates directly with repositories.
- UI consumes services.
- Services contain business logic.
- Services consume repositories.
- Repositories perform persistence only.
- Repositories consume `DbContext`.
- Database access occurs only through the Infrastructure layer.
- Business logic belongs in the Service layer.
- Persistence logic belongs in the Repository layer.

## Current Features

### Employee Module

Implemented:

- Employee list at `/employees`
- Employee creation at `/employees/create`
- Employee editing at `/employees/edit/{id}`
- Employee soft deactivation by setting status to inactive
- Validation before save
- Employee existence checks before retrieval and update
- Inactive employees remain visible for historical purposes

Employee fields include:

- Full Name
- Department
- Email
- Phone Number
- Designation
- Status

### Asset Module

Implemented foundation:

- `Asset` domain entity
- `IAssetRepository`
- `AssetRepository`
- `IAssetService`
- `AssetService`
- Asset validation rules
- Asset UI routes currently present for list, create, and edit

Asset fields include:

- Asset Name
- Asset Type
- Serial Number
- Purchase Date
- Warranty Expiry Date
- Status
- Condition

The asset model is intentionally lightweight. Make and Model are deferred because they are not part of the current business requirements.

### AI Admin Agent (Python)

Implemented:
- `admin_agent/agent.py`: A Google Gemini-powered conversational agent that maps natural language queries to structured filtering rules for Asset Management.
- `admin_agent/fast_api_app.py`: A FastAPI application exposing the agent over HTTP.
- **Testing & Evals**: Comprehensive unit and integration test suites using `pytest` and `pydantic` validation. An LLM-as-a-judge evaluation suite for testing the agent against complex queries and potential injections.

### Frontend UI Mockup

Implemented:
- `frontend/mock.html`: A conceptual UI dashboard reflecting the application's branding (Yellow and Purple palette) tailored specifically to the admin's experience. It highlights core application features, system metrics, and the upcoming AI-powered Natural Language Search capability.
## Current Status

### Database Configuration

The default connection string is configured in:

```text
src/AssetManagement.Web/appsettings.json
```

### Restore and Build

From the repository root:

```bash
dotnet restore
dotnet build
```

### Apply Migrations

From the repository root:

```bash
dotnet ef database update --project src/AssetManagement.Infrastructure --startup-project src/AssetManagement.Web
```

If the EF Core CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

### Run the Application

```bash
dotnet run --project src/AssetManagement.Web
```

Then open the local URL printed by the CLI.

## Main Routes

| Route | Purpose |
| --- | --- |
| `/` | Home page |
| `/employees` | Employee list |
| `/employees/create` | Create employee |
| `/employees/edit/{id}` | Edit employee |
| `/assets` | Asset list |
| `/assets/create` | Create asset |
| `/assets/edit/{id}` | Edit asset |
