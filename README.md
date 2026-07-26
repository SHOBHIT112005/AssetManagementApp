# Asset Management Application

A full-stack Asset Management system built with **ASP.NET Core Blazor Server**, **Entity Framework Core**, **Dapper**, **Microsoft SQL Server**, and a **Python AI Agent** powered by Google Gemini. The project follows clean layered architecture and includes role-based access control, Excel bulk import, and natural language querying through an integrated AI chat interface.

## Technology Stack

| Area | Technology |
| --- | --- |
| Frontend | Blazor Server (.NET 10), HTML5, CSS3 |
| Backend | ASP.NET Core |
| ORM | Entity Framework Core (writes, CRUD) |
| Micro-ORM | Dapper + SqlBuilder (reads, dynamic queries) |
| Database | Microsoft SQL Server |
| Authentication | ASP.NET Core Identity (Cookie-based) |
| AI Agent | Python, Google ADK, Gemini LLM, FastAPI |
| Architecture | Clean Architecture (4-layer) |

## Solution Structure

```
AssetManagementApp/
├── src/
│   ├── AssetManagement.Domain          # Entities, enums — no dependencies
│   ├── AssetManagement.Application     # Interfaces, services, DTOs, business logic
│   ├── AssetManagement.Infrastructure  # EF Core DbContext, repositories, migrations, Dapper queries
│   └── AssetManagement.Web             # Blazor Server UI, routing, DI composition, static assets
├── admin_agent/                        # Python AI agent (Google ADK + FastAPI)
├── ApplicationDocs/                    # Detailed documentation (architecture, concepts, features)
├── sample-data/                        # Sample Excel files for bulk import
├── SeedEmployeeData.sql                # SQL script to seed 100 employees
└── README.md
```

### Layer Responsibilities

| Layer | Responsibility |
| --- | --- |
| **Domain** | Core entities (`Employee`, `Asset`, `AssetAssignment`, `ApplicationUser`) and enums (`AssetStatus`, `AssetCondition`, `AssetType`, `Department`, `EmployeeDesignation`, `EmployeeStatus`). Has zero dependencies on other layers. |
| **Application** | Service interfaces and implementations, repository contracts, DTOs, and all business logic. Depends only on Domain. |
| **Infrastructure** | EF Core `AssetDbContext`, concrete repositories, Identity configuration, `SqlConnectionFactory`, Dapper-based read repositories (`AdminAgentQueryRepository`, `AssetAssignmentRepository`), and database migrations. |
| **Web** | Blazor Server components (pages, forms, layout), `Program.cs` DI registration, static assets (CSS, JS, images), and API endpoints (login/logout). |

## Architecture

```
Blazor UI  →  Service  →  Repository  →  DbContext / Dapper  →  SQL Server
```

**Core rules:**
- UI never accesses `DbContext` or repositories directly — only services.
- Services contain business logic and coordinate between repositories.
- Repositories handle persistence only.
- All database access goes through the Infrastructure layer.
- The Unit of Work pattern (`IUnitOfWork`) groups multi-repository operations into atomic transactions.

## Features

### Authentication & Authorization

- **ASP.NET Core Identity** with cookie-based authentication.
- **Two roles:** `Admin` and `Employee`.
- Admin users see the full dashboard, asset/employee management, assignments, import, and the AI agent.
- Employee users see only their own assignments and profile.
- Login page with form-based authentication; logout via POST to prevent CSRF.
- Change password functionality for all authenticated users.
- Employee provisioning: when an admin creates an employee, a corresponding Identity user account is auto-created.

### Admin Dashboard

- Overview cards showing: Total Assets, Available Assets, Active Employees, Assigned Assets, Expiring Warranty, and Under Repair counts.
- Personalized greeting banner.
- Feature highlights section.

### Employee Management

- Paginated employee list with search and filters (department, designation, status).
- Create, edit, and soft-deactivate employees (inactive employees remain visible for history).
- My Profile page for employees to view/edit their own details.
- Fields: Full Name, Department, Email, Phone Number, Designation, Date of Birth, Status.

### Asset Management

- Paginated asset list with search and filters (type, status, condition).
- Create and edit assets with full validation (warranty date must be after purchase date, etc.).
- Fields: Asset Name, Type (Laptop, Desktop, Monitor, Keyboard, Mouse, Phone, Printer), Serial Number, Purchase Date, Warranty Expiry Date, Status (Available, Assigned, Under Repair, Retired), Condition (New, Good, NeedsRepair, Damaged).

### Asset Assignments

- Assign available assets to active employees with department/designation filtering.
- Return assigned assets (updates asset status back to Available).
- Full assignment history with search, date range filters, and pagination.
- Employee-facing "My Assignments" page with personalized greeting banner, showing current and past assignments with one-click return.

### Excel Bulk Import

- Import employees and assets from `.xlsx` files.
- Validates each row against the same Data Annotation rules used by forms.
- Reports per-row errors (row number + message) for any invalid data.
- Sample data file provided in `sample-data/`.

### AI-Powered Natural Language Agent

An integrated AI chat interface allows admins to query data using natural language instead of filters.

**Architecture:** The agent is a separate Python microservice using the Google Agent Development Kit (ADK) and Gemini LLM, exposed via FastAPI and communicating over JSON-RPC 2.0.

**Example queries:**
- *"Show me all laptops under repair"*
- *"Get all employees in the IT department"*
- *"Which assets have warranties expiring before 2025?"*
- *"Show employees born after 1995"*

**How it works:**
1. The Blazor chat UI sends the user's natural language prompt to `AdminAgentService` (C#).
2. The service forwards it via HTTP POST to the Python FastAPI agent.
3. The Gemini LLM parses the intent and returns a structured JSON with `target_entity`, `filters`, and `limit`.
4. The C# service deserializes the JSON and passes the filters to `AdminAgentQueryRepository`.
5. The repository uses Dapper `SqlBuilder` to construct parameterized SQL queries — only `SELECT` statements, never writes.
6. Results are formatted into a table and returned to the UI.

**Security:** The agent is read-only by design. The Python agent only produces filter objects; the C# repository only executes pre-defined `SELECT` templates. Even a prompt injection attempt cannot produce write operations.

**Queryable entities:**

| Entity | Filterable Fields |
| --- | --- |
| Asset | Type, Status, Condition, AssetName, SerialNumber, PurchaseDate, WarrantyExpiryDate |
| Employee | Department, Designation, Status, FullName, Email, DateOfBirth |
| Assignment | AssignmentDate, ReturnDate, Employee.FullName, Employee.Department, Asset.Type, Asset.AssetName |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or full instance)
- [Python 3.11+](https://www.python.org/) and [uv](https://docs.astral.sh/uv/) (for the AI agent)
- A [Google Gemini API key](https://aistudio.google.com/apikey) (for the AI agent)

### Database Setup

1. Configure your connection string in `src/AssetManagement.Web/appsettings.json`.

2. Install the EF Core CLI (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. Apply migrations:
   ```bash
   dotnet ef database update --project src/AssetManagement.Infrastructure --startup-project src/AssetManagement.Web
   ```

4. (Optional) Seed employee data:
   ```bash
   sqlcmd -S your_server -d your_database -i SeedEmployeeData.sql
   ```

### Run the .NET Application

```bash
dotnet restore
dotnet build
dotnet run --project src/AssetManagement.Web
```

Or for development with hot reload:

```bash
cd src/AssetManagement.Web
dotnet watch
```

### Run the AI Agent (Optional)

1. Create a `.env` file in the project root with your API key:
   ```
   GOOGLE_API_KEY=your_gemini_api_key_here
   ```

2. Install Python dependencies and start the agent:
   ```bash
   uv sync
   uv run uvicorn admin_agent.fast_api_app:app --host 0.0.0.0 --port 8000
   ```

3. The agent endpoint defaults to `http://localhost:8000/a2a/admin_agent`. This can be overridden in `appsettings.json` under `Agent:Endpoint`.

## Routes

### Admin Routes

| Route | Purpose |
| --- | --- |
| `/` | Admin dashboard with overview stats |
| `/assets` | Asset list with search, filters, pagination |
| `/assets/create` | Create a new asset |
| `/assets/edit/{id}` | Edit an existing asset |
| `/assets/assign/{id}` | Assign an asset to an employee |
| `/employees` | Employee list with search, filters, pagination |
| `/employees/create` | Create a new employee (auto-provisions login) |
| `/employees/edit/{id}` | Edit an existing employee |
| `/assignments` | Full assignment history |
| `/import` | Bulk import employees/assets from Excel |
| `/change-password` | Change own password |

### Employee Routes

| Route | Purpose |
| --- | --- |
| `/my-assignments` | View own assigned assets, return assets |
| `/my-profile` | View and edit own profile |
| `/change-password` | Change own password |

## Documentation

Detailed documentation is available in the `ApplicationDocs/` directory:

| Document | Contents |
| --- | --- |
| `ARCHITECTURE.md` | Layered architecture, dependency flow, project structure |
| `csharp-dotnet-concepts.md` | C# and .NET concepts used in the project (DI, EF Core, Dapper, Blazor, async/await, records, generics, IConfiguration, IHttpClientFactory) |
| `agent-implementation.md` | AI agent architecture, request lifecycle, security model, JSON-RPC protocol |
| `asset-management-features.md` | Detailed feature documentation for assets, employees, and assignments |
| `auth-implementation.md` | Authentication and authorization implementation details |
| `excel-import.md` | Excel import feature: parsing, validation, error handling |
| `orm-strategy.md` | When and why EF Core vs. Dapper is used |
