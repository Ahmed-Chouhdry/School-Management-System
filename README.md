# School Management System — School Portal

A full-stack School Portal application for managing users and their wallet balances, built with a **.NET 8 Clean Architecture backend** and a **React + Redux Toolkit + TypeScript frontend**.

```
project-root/
├── SchoolManagementSystem.api/          # Backend solution
│   ├── SchoolManagementSystem.Domain/
│   ├── SchoolManagementSystem.Application/
│   ├── SchoolManagementSystem.Infrastructure/
│   └── SchoolManagementSystem.WebApi/
└── school-managment-system-app/         # Frontend (React + Vite)
    ├── src/
    │   ├── api/
    │   ├── components/
    │   ├── data/
    │   ├── store/
    │   └── types.ts
    └── ...
```

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Backend — Setup & Configuration](#backend--setup--configuration)
3. [Database — Connection String, Migrations & Seeding](#database--connection-string-migrations--seeding)
4. [Backend — API Endpoints](#backend--api-endpoints)
5. [Backend — Domain Business Rules](#backend--domain-business-rules)
6. [Post Configuration (CORS, JSON, Swagger)](#post-configuration-cors-json-swagger)
7. [Frontend — Setup & Configuration](#frontend--setup--configuration)
8. [Frontend — Component Breakdown](#frontend--component-breakdown)
9. [Frontend — State Management (Redux)](#frontend--state-management-redux)
10. [Connecting Frontend to Backend](#connecting-frontend-to-backend)
11. [Running the Full Stack](#running-the-full-stack)
12. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

The backend follows **Clean Architecture**, separating concerns into four projects with a strict dependency direction (outer layers depend on inner layers, never the reverse):

```
WebApi  →  Infrastructure  →  Application  →  Domain
```

| Layer | Responsibility |
|---|---|
| **Domain** | Pure business entities and rules. No dependencies on any other project or framework. |
| **Application** | Use cases, DTOs, service interfaces, and orchestration logic. Depends only on Domain. |
| **Infrastructure** | EF Core `DbContext`, repository implementations, database seeding. Depends on Application + Domain. |
| **WebApi** | ASP.NET Core controllers, startup configuration, Swagger, CORS. Depends on all layers (for DI wiring only). |

The frontend is a **React 18 + TypeScript** SPA built with **Vite**, using **Redux Toolkit** for state management and native `fetch` for API calls.

---

## Backend — Setup & Configuration

### Project References

Set these up in Visual Studio (or via `dotnet add reference`) so the dependency direction above is enforced:

```bash
dotnet add SchoolManagementSystem.Application reference SchoolManagementSystem.Domain
dotnet add SchoolManagementSystem.Infrastructure reference SchoolManagementSystem.Application
dotnet add SchoolManagementSystem.Infrastructure reference SchoolManagementSystem.Domain
dotnet add SchoolManagementSystem.WebApi reference SchoolManagementSystem.Application
dotnet add SchoolManagementSystem.WebApi reference SchoolManagementSystem.Infrastructure
```

### Required NuGet Packages

**SchoolManagementSystem.Infrastructure**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**SchoolManagementSystem.WebApi**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Global Tool (one-time, for migrations)

```bash
dotnet tool install --global dotnet-ef
```

---

## Database — Connection String, Migrations & Seeding

### 1. Connection String

Add a connection string to **`SchoolManagementSystem.WebApi/appsettings.json`** (or `appsettings.Development.json` for local-only settings). Pick whichever SQL Server target you have available:

**Option A — SQL Server LocalDB** (ships with Visual Studio, zero extra setup)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SchoolPortalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Option B — SQL Server / SQL Server Express installed locally**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SchoolPortalDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Option C — SQL Server in Docker**
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sql-school -d mcr.microsoft.com/mssql/server:2022-latest
```
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SchoolPortalDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  }
}
```

> Only keep **one** active `DefaultConnection` value at a time. Switching between LocalDB/Express/Docker mid-project without re-running migrations against the new target is the most common source of "data isn't there" bugs.

### 2. DbContext Registration

In **`SchoolManagementSystem.Infrastructure/DependencyInjection.cs`**:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly("SchoolManagementSystem.Infrastructure")));

        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
```

The `MigrationsAssembly(...)` call is required — without it, EF Core tries to place migrations in the startup project (WebApi) instead of Infrastructure, where the `DbContext` actually lives.

### 3. Creating & Applying Migrations

Run these from the solution root (i.e. `SchoolManagementSystem.api/`):

```bash
dotnet ef migrations add InitialCreate -p SchoolManagementSystem.Infrastructure -s SchoolManagementSystem.WebApi -o Persistence/Migrations

dotnet ef database update -p SchoolManagementSystem.Infrastructure -s SchoolManagementSystem.WebApi
```

- `-p` (project) — where the `DbContext` lives (Infrastructure)
- `-s` (startup project) — used to resolve DI and read `appsettings.json` (WebApi)

**Auto-migration on startup** is also wired into `Program.cs` for local dev convenience (applies any pending migrations automatically every time the app starts):

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}
```

> ⚠️ Auto-migrate-on-startup is convenient for development but not recommended for production — multiple instances starting simultaneously can race on schema changes. In production, run migrations as a separate CI/CD step and remove the `MigrateAsync()` call (seeding can also be removed or gated behind an environment check).

### 4. Data Seeding

Since `User` uses a private constructor and encapsulated setters (to protect wallet invariants), EF Core's declarative `HasData` seeding doesn't fit cleanly. Instead, seeding happens **at runtime**, through the same domain constructor and methods the rest of the app uses — so seeded users get real audit trail records (`WalletAdjustment` rows), not just a raw balance.

**`SchoolManagementSystem.Infrastructure/Persistence/DataSeeder.cs`**
```csharp
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // already seeded — avoids duplicating on every restart

        var users = new List<User>
        {
            new("Ahmed", "Javed", "ahmed.javed@school.edu", UserRole.Admin),
            new("Sara", "Khan", "sara.khan@school.edu", UserRole.Teacher),
            new("Bilal", "Ahmed", "bilal.ahmed@school.edu", UserRole.Teacher),
            new("Hina", "Riaz", "hina.riaz@school.edu", UserRole.Student),
            new("Zain", "Malik", "zain.malik@school.edu", UserRole.Student),
            new("Fatima", "Noor", "fatima.noor@school.edu", UserRole.Student, UserStatus.Inactive),
        };

        users[3].ApplyWalletAdjustment(500, "Initial wallet top-up (seed data)");
        users[4].ApplyWalletAdjustment(250, "Initial wallet top-up (seed data)");

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
```

The `AnyAsync()` guard means seeding only ever runs once against a given database — subsequent app restarts are no-ops.

**To force a re-seed during development**, drop and recreate the database:
```bash
dotnet ef database drop -p SchoolManagementSystem.Infrastructure -s SchoolManagementSystem.WebApi
dotnet ef database update -p SchoolManagementSystem.Infrastructure -s SchoolManagementSystem.WebApi
```

---

## Backend — API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/users` | Returns all users |
| `GET` | `/users/{id}` | Returns a single user by ID, or `404` if not found |
| `POST` | `/users/{id}/wallet-adjustments` | Applies a wallet adjustment (positive = credit, negative = debit) |

**Request body — `POST /users/{id}/wallet-adjustments`**
```json
{
  "amount": 100.00,
  "reason": "Monthly allowance"
}
```

**Success response**
```json
{
  "id": "guid",
  "userId": "guid",
  "amount": 100.00,
  "resultingBalance": 350.00,
  "reason": "Monthly allowance",
  "createdAtUtc": "2026-07-14T10:00:00Z"
}
```

**Error responses**
- `404 Not Found` — user doesn't exist: `{ "error": "User 'xxx' not found." }`
- `400 Bad Request` — business rule violation (e.g. would go negative): `{ "error": "Cannot apply adjustment of -500. Current balance is 250, resulting balance would be negative." }`

---

## Backend — Domain Business Rules

All wallet logic is enforced **inside the `User` entity itself**, not in the service layer — this guarantees the balance can never be mutated without going through the rule checks, and that every successful change produces an audit record.

```csharp
public WalletAdjustment ApplyWalletAdjustment(decimal amount, string reason)
{
    if (amount == 0)
        throw new InvalidWalletAdjustmentException("Adjustment amount cannot be zero.");

    var newBalance = WalletBalance + amount;

    if (newBalance < 0)
        throw new InsufficientWalletBalanceException(WalletBalance, amount);

    WalletBalance = newBalance;

    var adjustment = new WalletAdjustment(Id, amount, newBalance, reason);
    _walletAdjustments.Add(adjustment);

    return adjustment;
}
```

Rules enforced:
- ✅ Positive amounts **increase** the balance
- ✅ Negative amounts **decrease** the balance
- ✅ Balance can **never go negative** — the operation is rejected entirely rather than clamped to zero
- ✅ Every adjustment (successful) is recorded as a `WalletAdjustment` row, forming a full audit trail

> **Important:** when saving a new `WalletAdjustment`, it must be explicitly added via `_context.WalletAdjustments.AddAsync(adjustment)` in the repository rather than relying solely on EF Core's collection-navigation fixup. Relying on fixup alone (adding only to the entity's in-memory backing field) can cause EF Core to mistrack the new row as `Modified` instead of `Added`, which produces a `DbUpdateConcurrencyException` on save (an `UPDATE` statement targeting a row that has never existed matches zero rows).

---

## Post Configuration (CORS, JSON, Swagger)

**`SchoolManagementSystem.WebApi/Program.cs`**

```csharp
using System.Text.Json.Serialization;
using SchoolManagementSystem.Application;
using SchoolManagementSystem.Infrastructure;
using SchoolManagementSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializes enums (Role, Status) as strings ("Active") instead of ints (0)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDev"); // must be registered before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Viewing Swagger

1. Set `SchoolManagementSystem.WebApi` as the startup project.
2. Run via **F5** in Visual Studio, or:
   ```bash
   dotnet run --project SchoolManagementSystem.WebApi
   ```
3. Navigate to:
   ```
   https://localhost:{port}/swagger
   ```
   The exact port is shown in the console output (or check `Properties/launchSettings.json`).

If Swagger 404s, confirm `ASPNETCORE_ENVIRONMENT=Development` is set in `launchSettings.json`, since the Swagger middleware is currently gated behind `IsDevelopment()`.

If HTTPS calls fail from the browser due to an untrusted local certificate:
```bash
dotnet dev-certs https --trust
```

---

## Frontend — Setup & Configuration

### Tech Stack

- **React 18** + **TypeScript**
- **Vite** (dev server + build tool)
- **Redux Toolkit** (`@reduxjs/toolkit`, `react-redux`) for state management
- Native `fetch` for API calls (no axios dependency)

### Environment Variables

Create a `.env` file in `school-managment-system-app/`:
```
VITE_API_BASE_URL=https://localhost:7123
```
Match the port to whatever your backend's `dotnet run` output shows.

### Folder Structure

```
src/
├── api/
│   ├── client.ts       # fetch wrapper + endpoint calls
│   ├── mappers.ts       # ApiUser (backend shape) → User (frontend shape)
│   └── types.ts         # ApiUser, WalletAdjustmentResponse, ApiError
├── components/
│   ├── SearchBar.tsx
│   ├── UserTable.tsx
│   ├── UserDetails.tsx
│   └── WalletModal.tsx
├── data/
│   └── mockUsers.ts      # (legacy — superseded by live API data)
├── store/
│   ├── index.ts          # Redux store config + typed hooks
│   └── userSlice.ts      # users state, thunks, reducers
├── types.ts              # frontend User interface
├── App.tsx
└── main.tsx
```

---

## Frontend — Component Breakdown

### `App.tsx`
Top-level container. Fetches all users on mount (`useEffect` → `dispatch(fetchUsers())`), derives `selectedUser` and `activeModalUser` from Redux state, and composes the layout: `SearchBar` + `UserTable` in a card, `UserDetails` alongside it, and a conditionally-rendered `WalletModal`.

### `SearchBar.tsx`
Controlled text input. Purely presentational — takes `value` and `onChange`, filters happen in `App.tsx` via `useMemo` against `name`/`email`.

### `UserTable.tsx`
Renders the filtered user list as a table: avatar, name, email, status badge, formatted wallet balance, and two actions per row — **View Details** (selects the user) and **Adjust** (opens the wallet modal).

### `UserDetails.tsx`
Side panel showing the currently selected user's full details (avatar, ID, email, status badge, formatted balance) with a **Modify User Funds** button that opens `WalletModal`. Shows a placeholder message when no user is selected.

### `WalletModal.tsx`
Form for crediting/debiting a user's wallet:
- Toggle between **Add Funds** / **Deduct Funds**
- Amount input with client-side validation (must be > 0; a deduction warning if it would exceed the current balance — this is a UX hint only, the backend is the actual source of truth for the negative-balance rule)
- On submit, dispatches `adjustWallet({ userId, amount, reason })` — where `amount` is **signed** (positive for add, negative for deduct) to match the backend's `ApplyWalletAdjustment` contract
- Displays either a success message or the backend's rejection reason (e.g. if the balance would go negative)

---

## Frontend — State Management (Redux)

**`store/index.ts`** — standard Redux Toolkit store setup with typed `useAppDispatch` / `useAppSelector` hooks.

**`store/userSlice.ts`** — manages:

| State field | Purpose |
|---|---|
| `list` | Array of `User` objects, populated from the API |
| `searchTerm` | Current search filter text |
| `selectedUserId` | ID of the user shown in the details panel |
| `activeModalUserId` | ID of the user currently being edited in `WalletModal` |
| `status` | `'idle' \| 'loading' \| 'succeeded' \| 'failed'` for the users list fetch |
| `error` | Error message if `fetchUsers` fails |
| `walletError` | Error message scoped to the wallet adjustment modal |

**Async thunks:**
- `fetchUsers()` — calls `GET /users`, maps each `ApiUser` → `User`
- `fetchUserById(id)` — calls `GET /users/{id}` (available for on-demand refresh scenarios; the details panel currently derives from the already-fetched list rather than calling this on every selection, since re-fetching a single row on every click adds latency without benefit at this data scale)
- `adjustWallet({ userId, amount, reason })` — calls `POST /users/{id}/wallet-adjustments`; on success, updates `walletBalance` in the store using the **server-returned `resultingBalance`** rather than a client-computed value, since the backend is authoritative for the negative-balance rule

---

## Connecting Frontend to Backend

### 1. Type Mapping

The backend `UserDto` and frontend `User` interface don't match 1:1 (`firstName`/`lastName` vs `name`, no `avatarUrl`/`joinedDate` on the backend). A mapper bridges the two:

```typescript
// src/api/mappers.ts
export function mapApiUserToUser(apiUser: ApiUser): User {
  const name = `${apiUser.firstName} ${apiUser.lastName}`;
  return {
    id: apiUser.id,
    name,
    email: apiUser.email,
    status: apiUser.status,
    walletBalance: apiUser.walletBalance,
    avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`,
  };
}
```

### 2. API Client

```typescript
// src/api/client.ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7123';

export const api = {
  getUsers: () => fetch(`${API_BASE_URL}/users`).then(handleResponse<ApiUser[]>),
  getUserById: (id: string) => fetch(`${API_BASE_URL}/users/${id}`).then(handleResponse<ApiUser>),
  adjustWallet: (userId: string, amount: number, reason?: string) =>
    fetch(`${API_BASE_URL}/users/${userId}/wallet-adjustments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amount, reason }),
    }).then(handleResponse<WalletAdjustmentResponse>),
};
```

### 3. CORS

The backend's `FrontendDev` CORS policy must list the frontend's actual dev server origin (`http://localhost:5173` by default for Vite). If Vite runs on a different port on your machine, update `WithOrigins(...)` in `Program.cs` to match.

---

## Running the Full Stack

**Terminal 1 — Backend**
```bash
cd SchoolManagementSystem.api
dotnet run --project SchoolManagementSystem.WebApi
```
Confirms the port in the console output (e.g. `https://localhost:7123`), auto-applies migrations, and seeds initial users on first run.

**Terminal 2 — Frontend**
```bash
cd school-managment-system-app
npm install
npm run dev
```
Opens at `http://localhost:5173` by default. Confirm `.env`'s `VITE_API_BASE_URL` matches the backend's port from Terminal 1.

**Verify the connection:**
1. Open the frontend in a browser.
2. The user table should populate with the seeded users (Ahmed Javed, Sara Khan, etc.).
3. Click **Adjust** on a user, submit a wallet adjustment, and confirm the balance updates and persists on page refresh (proving it's actually round-tripping through the database, not just local state).

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| `GET /users` fails from the browser with a CORS/network-looking error | HTTPS dev cert not trusted, or CORS origin mismatch | Run `dotnet dev-certs https --trust`; confirm `WithOrigins` matches Vite's actual port |
| `status: 0` instead of `"Active"` in API responses | `JsonStringEnumConverter` not registered | Add `.AddJsonOptions(...)` in `Program.cs` as shown above |
| Swagger 404s at `/swagger` | Not running in `Development` environment | Check `ASPNETCORE_ENVIRONMENT` in `launchSettings.json` |
| `DbUpdateConcurrencyException` on wallet adjustment | New `WalletAdjustment` entity mistracked as `Modified` instead of `Added` (EF Core collection-fixup issue) | Explicitly call `_context.WalletAdjustments.AddAsync(adjustment)` in the repository before `SaveChangesAsync` |
| Users list is empty even after seeding should have run | Connection string points at a different DB than the one migrations/seeding ran against | Confirm only one `DefaultConnection` is active; re-run `dotnet ef database update`; check with `sqllocaldb info` if using LocalDB |
| `GET /users` fires twice on page load in dev | React 18 `StrictMode` double-invokes effects in development only | Expected behavior — harmless and does not occur in production builds; only worth guarding with a `useRef` flag if it's causing a real problem (e.g. rate limiting) |
| Wallet balance in UI doesn't match backend after a rapid double-submit | Client-side balance math drifting from server-side validation | Already mitigated — `adjustWallet.fulfilled` sets balance from the API's `resultingBalance`, not a locally computed value |

---

## Summary Checklist

- [ ] Backend connection string configured and pointing at a reachable SQL Server instance
- [ ] `dotnet ef database update` run successfully (or auto-migrate confirmed working on startup)
- [ ] Seeded users visible via `GET /users` in Swagger
- [ ] CORS policy origin matches the frontend's actual dev server URL
- [ ] Frontend `.env` `VITE_API_BASE_URL` matches backend's running port
- [ ] Wallet adjustment round-trips correctly and rejects attempts that would go negative
- [ ] Swagger accessible at `/swagger` for manual API testing
