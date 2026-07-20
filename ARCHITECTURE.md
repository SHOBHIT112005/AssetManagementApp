# Architecture

AssetManagementApp follows a **Clean Architecture** (sometimes called Onion Architecture).
The cardinal rule: dependencies always point **inward**, toward the domain. Outer layers
know about inner layers; inner layers know nothing about outer ones. This means you can
replace SQL Server with another provider, or swap Blazor for a REST API, without touching
a single line of business logic — and when debugging, each layer has exactly one
responsibility, so you always know *where* to look.

```
        ┌─────────────────────────────────────────────┐
        │  Presentation                                │
        │  AssetManagement.Web  (Blazor Server)        │
        └──────────────────────┬──────────────────────┘
                               │
                               ▼
        ┌─────────────────────────────────────────────┐
        │  Application  (business logic & contracts)   │
        │  Services · DTOs · Interfaces · UnitOfWork   │
        └──────────────────────┬──────────────────────┘
                               │  depends on
                               ▼
        ┌─────────────────────────────────────────────┐
        │  Domain  (enterprise core)                   │
        │  Entities · Enums                            │
        └─────────────────────────────────────────────┘
                               ▲  implements interfaces of
                               │
        ┌──────────────────────┴──────────────────────┐
        │  Infrastructure                              │
        │  EF Core DbContext · Repositories · UoW      │
        │  Dapper (reserved) · Migrations              │
        └─────────────────────────────────────────────┘
```

## Dependency graph (project references)

```
AssetManagement.Web
 ├─► AssetManagement.Application
 ├─► AssetManagement.Infrastructure
 └─► AssetManagement.Domain

AssetManagement.Infrastructure
 ├─► AssetManagement.Application
 └─► AssetManagement.Domain

AssetManagement.Application
 └─► AssetManagement.Domain

AssetManagement.Domain
     (no project references — pure C#)
```

All four projects target **.NET 10** with nullable reference types and implicit usings
enabled.

---

## Project responsibilities

### Domain (`AssetManagement.Domain`)

Pure C#. Zero framework dependencies. This is the innermost ring — if it can't compile
with just the .NET base class library, it doesn't belong here.

| Folder       | Contents |
|--------------|----------|
| `Entities/`  | `Asset`, `Employee`, `AssetAssignment` — core domain objects with data-annotation validation |
| `Enums/`     | `AssetStatus`, `AssetCondition`, `AssetType`, `Department`, `EmployeeStatus`, `EmployeeDesignation`, plus search/sort field enums (`AssetSearchField`, `AssetSortField`, `EmployeeSearchField`, `EmployeeSortField`, `AssignmentSearchField`, `AssignmentSortField`) |

**Key design decisions:**

- Entities use `System.ComponentModel.DataAnnotations` for validation so the same rules
  are enforced in both the Blazor UI (client-side) and the service layer (server-side).
- `AssetAssignment` links `Asset` ↔ `Employee` with navigation properties and tracks
  assignment/return dates plus optional notes.
- Enums are strongly typed (no magic strings) and most use explicit integer backing values
  starting at `1` to avoid accidental default-value bugs.

---

### Application (`AssetManagement.Application`)

The brain. Depends only on **Domain**. Defines the *contracts* that Infrastructure must
implement (Dependency Inversion Principle in action).

| Folder         | Contents |
|----------------|----------|
| `Interfaces/`  | `IAssetRepository`, `IEmployeeRepository`, `IAssetAssignmentRepository` — data-access contracts; `IAssetService`, `IEmployeeService`, `IAssetAssignmentService` — business-logic contracts; `IUnitOfWork` — transaction boundary |
| `Services/`    | `AssetService`, `EmployeeService`, `AssetAssignmentService` — implement the `I*Service` interfaces, orchestrate validation, status transitions, and delegate persistence to repositories + UoW |
| `DTOs/`        | Query, summary, and result types grouped by feature area: |
|                | **`AssetDTOs/`** — `AssetQueryDto`, `AssetSummaryDto`, `AssignmentSummaryDto` |
|                | **`EmployeeDTOs/`** — `EmployeeQueryDto`, `EmployeeSummaryDto` |
|                | **`AssignmentDTOs/`** — `AssignmentQueryDto`, `AssetAssignmentHistoryDto` |
|                | **Root** — `PagedResultDto<T>` (generic paging envelope), `SortDirection` |

**Patterns in use:**

- **Unit of Work** — services call `IUnitOfWork.SaveChangesAsync()` to commit changes
  after repository mutations, keeping the transaction boundary explicit.
- **Query DTOs** — each list endpoint accepts a typed query DTO that bundles search term,
  search field, filters, sort field, sort direction, page number, and page size. This
  keeps method signatures clean and makes adding new filters a one-property change.
- **Summary DTOs** — lightweight aggregation objects (e.g. `AssetSummaryDto` with
  counts by status) designed for dashboard/stats displays.

---

### Infrastructure (`AssetManagement.Infrastructure`)

The hands. Implements Application's interfaces with real data-access technology.

| Folder          | Contents |
|-----------------|----------|
| `Data/`         | `AssetDbContext` — EF Core `DbContext` exposing `DbSet<Asset>`, `DbSet<Employee>`, `DbSet<AssetAssignment>`; `UnitOfWork` — `IUnitOfWork` implementation wrapping `DbContext.SaveChangesAsync()` |
| `Repositories/` | `AssetRepository`, `EmployeeRepository`, `AssetAssignmentRepository` — EF Core implementations of the corresponding `I*Repository` interfaces with full search, filter, sort, and pagination logic |
| `Migrations/`   | EF Core migration history |
| `Dapper/`       | Reserved for future raw-SQL / Dapper-based queries (currently empty) |

**Key packages:**

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore` 10.x | ORM |
| `Microsoft.EntityFrameworkCore.SqlServer` 10.x | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` 10.x | Migrations tooling |
| `Dapper` 2.x | Reserved for future high-performance queries |

**Repository implementation notes:**

- Read queries use `.AsNoTracking()` for performance.
- Filtering, sorting, and pagination are composed as `IQueryable` before execution —
  everything translates to a single SQL query.
- Write operations (`Add`, `Update`, `Delete`) stage changes on the `DbContext`; nothing
  hits the database until the service calls `IUnitOfWork.SaveChangesAsync()`.

---

### Presentation (`AssetManagement.Web`)

Blazor Server app. The outermost ring — its only job is to wire DI, render UI, and
delegate to the Application layer. It references all three inner projects so it can act
as the **composition root**.

| Folder          | Contents |
|-----------------|----------|
| `Components/`   | Razor components organised by feature area: |
|                 | **`Assets/`** — `Assets.razor` (list + search), `CreateAsset.razor`, `EditAsset.razor` |
|                 | **`Employees/`** — `Employees.razor`, `CreateEmployee.razor`, `EditEmployee.razor` |
|                 | **`Assignments/`** — `AssignAsset.razor`, `ReturnAsset.razor`, `AssignmentHistory.razor` |
|                 | **`Layout/`** — `MainLayout.razor`, `NavMenu.razor`, `ReconnectModal.razor` (+ isolated CSS/JS) |
|                 | **`Pages/`** — `Home.razor`, `Error.razor`, `NotFound.razor` |
|                 | **`Shared/`** — reusable components (currently empty — planned) |
| `Extensions/`   | (Currently empty — reserved for DI extension methods) |
| `Services/`     | (Currently empty — reserved for presentation-layer services) |
| `Program.cs`    | Composition root: registers `DbContext`, repositories, services, and UoW |
| `wwwroot/`      | Static assets (CSS, JS, images) |

**Key packages:**

| Package | Purpose |
|---------|---------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.x | Prepared for future authentication/authorisation |
| `Microsoft.EntityFrameworkCore.Design` 10.x | Design-time migrations support |

**Rendering model:** Interactive Server (`AddInteractiveServerComponents`) — all
component logic executes on the server over a SignalR connection. This means Blazor
components call Application services **in-process** (no HTTP round-trip).

---

## Request flow (example: Assets list page)

```
Browser (SignalR)
  → Assets.razor component
    → IAssetService.GetAllAssetsAsync(queryDto)        [Application — validation & defaults]
      → IAssetRepository.GetAllAsync(queryDto)          [Infrastructure — EF Core query]
        → AssetDbContext → SQL Server
      ← PagedResultDto<Asset>
    ← PagedResultDto<Asset>  (pagination, count, items)
  ← Rendered HTML table pushed over SignalR
```

## Request flow (example: Assign an asset)

```
Browser (SignalR)
  → AssignAsset.razor component
    → IAssetAssignmentService.AssignAssetAsync(assignment)    [Application — business rules]
      → IAssetAssignmentRepository.AddAsync(assignment)       [Infrastructure — stage insert]
      → IAssetRepository (update asset status → Assigned)     [Infrastructure — stage update]
      → IUnitOfWork.SaveChangesAsync()                        [Infrastructure — commit transaction]
        → AssetDbContext → SQL Server
    ← void (success) or exception
  ← UI navigates / shows confirmation
```

---

## Composition root (`Program.cs`)

All dependency injection is wired in a single file — `AssetManagement.Web/Program.cs`:

```csharp
// Data access
builder.Services.AddDbContext<AssetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories (Infrastructure → Application interfaces)
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();

// Services (Application)
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

Everything is registered as **Scoped** — one instance per SignalR circuit (request).

---

## Configuration

`appsettings.json` (in `AssetManagement.Web`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AssetManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Point `DefaultConnection` at any SQL Server instance — local, remote, or Azure SQL. No
code change required, just update the connection string.

---

## Where bugs live

| Symptom | Suspect layer |
|---------|---------------|
| Data looks wrong / query returns unexpected results | **Infrastructure** — check repository LINQ queries |
| Business rule not enforced (e.g. status not updated on assignment) | **Application** — check service logic |
| UI not reflecting data / binding issues / rendering glitches | **Presentation** — check Razor components |
| Wrong service injected / service not found at runtime | **Composition root** — check `Program.cs` DI registrations |
| Schema mismatch / column missing | **Infrastructure** — check migrations & `AssetDbContext` |
