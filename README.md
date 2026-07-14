# School Management System - Wallet Admin Portal

A full-stack school management portal for viewing users and managing wallet balances. The backend is an ASP.NET Core Web API using Clean Architecture and Entity Framework Core, and the frontend is a React + Redux Toolkit + TypeScript app built with Vite.

## Project Structure

```text
.
|-- SchoolManagementSystem.api/
|   |-- SchoolManagementSystem.Domain/          # Entities, enums, and domain rules
|   |-- SchoolManagementSystem.Application/     # DTOs, service interfaces, use cases
|   |-- SchoolManagementSystem.Infrastructure/  # EF Core, repositories, migrations, seed data
|   |-- SchoolManagementSystem.WebApi/          # Controllers, DI, Swagger, API startup
|   `-- tests/                                  # xUnit test projects
`-- school-managment-system-app/
    |-- src/
    |   |-- api/                                # fetch client, API types, mappers
    |   |-- components/                         # Search, user table, detail panel, wallet modal
    |   |-- store/                              # Redux Toolkit store and user slice
    |   `-- types/                              # Frontend domain types
    `-- package.json
```

## Tech Stack

| Area | Tools |
| --- | --- |
| Backend | ASP.NET Core, .NET `net10.0`, Entity Framework Core, SQL Server |
| Architecture | Domain, Application, Infrastructure, WebApi projects |
| API docs | Swagger / OpenAPI |
| Backend tests | xUnit, FluentAssertions, Moq, EF Core InMemory |
| Frontend | React 19, TypeScript, Vite 8 |
| State | Redux Toolkit, React Redux |
| Frontend tests | Vitest, Testing Library, jsdom |
| Lint/build | Oxlint, TypeScript project build |

## Features

- Lists seeded school users from the backend API.
- Searches users by name or email.
- Shows selected user details.
- Adds or deducts wallet funds from a modal.
- Prevents zero-value and over-balance deductions.
- Persists wallet adjustments through SQL Server.
- Keeps wallet balance changes auditable with `WalletAdjustment` records.
- Exposes Swagger for API testing in development.

## Prerequisites

Install these before running the full stack:

- .NET SDK compatible with `net10.0`
- Node.js and npm
- SQL Server, SQL Server Express, or LocalDB
- EF Core CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

If `dotnet-ef` is already installed, update it when package versions change:

```bash
dotnet tool update --global dotnet-ef
```

## Backend Setup

The backend solution is in `SchoolManagementSystem.api`.

1. Confirm the connection string in:

```text
SchoolManagementSystem.api/SchoolManagementSystem.WebApi/appsettings.Development.json
```

Current default:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SchoolPortalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

For LocalDB, use this instead:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SchoolPortalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

2. Restore and build:

```bash
cd SchoolManagementSystem.api
dotnet restore
dotnet build
```

3. Apply migrations manually if needed:

```bash
dotnet ef database update -p SchoolManagementSystem.Infrastructure -s SchoolManagementSystem.WebApi
```

The API also calls `Database.MigrateAsync()` on startup, so pending migrations are applied automatically during local development.

4. Run the API:

```bash
dotnet run --project SchoolManagementSystem.WebApi --launch-profile https
```

Default launch URLs:

```text
https://localhost:7100
http://localhost:5050
```

Swagger is available in development at:

```text
https://localhost:7100/swagger
```

## Frontend Setup

The frontend app is in `school-managment-system-app`.

1. Install dependencies:

```bash
cd school-managment-system-app
npm install
```

2. Optional: create `.env` if your backend URL is different:

```text
VITE_API_BASE_URL=https://localhost:7100
```

The frontend already defaults to `https://localhost:7100` when this variable is not set.

3. Start Vite:

```bash
npm run dev
```

Open the Vite URL shown in the terminal, usually:

```text
http://localhost:5173
```

## Running the Full Stack

Use two terminals.

Terminal 1:

```bash
cd SchoolManagementSystem.api
dotnet run --project SchoolManagementSystem.WebApi --launch-profile https
```

Terminal 2:

```bash
cd school-managment-system-app
npm run dev
```

Then open the frontend and verify:

- Seeded users load in the table.
- Selecting a user refreshes details from `GET /users/{id}`.
- Add funds updates the displayed wallet balance.
- Deduct funds rejects amounts larger than the current balance.
- Refreshing the browser keeps the updated balance because the API/database is the source of truth.

## API Endpoints

Base URL:

```text
https://localhost:7100
```

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/users` | Returns all users |
| `GET` | `/users/{id}` | Returns one user by ID |
| `POST` | `/users/{id}/wallet-adjustments` | Adds or deducts wallet balance |

Wallet adjustment request:

```json
{
  "amount": 100,
  "reason": "Manual top-up"
}
```

Use a positive `amount` to add funds and a negative `amount` to deduct funds.

Successful response:

```json
{
  "id": "wallet-adjustment-guid",
  "userId": "user-guid",
  "amount": 100,
  "resultingBalance": 600,
  "reason": "Manual top-up",
  "createdAtUtc": "2026-07-14T10:00:00Z"
}
```

Common errors:

| Status | Cause |
| --- | --- |
| `400` | Invalid wallet adjustment, such as zero amount or negative resulting balance |
| `404` | User ID does not exist |
| `500` | Unexpected server error |

## Domain Rules

Wallet behavior is enforced by the `User` domain entity:

- Adjustment amount cannot be zero.
- Positive adjustments increase balance.
- Negative adjustments decrease balance.
- Balance cannot go below zero.
- Every successful adjustment creates a `WalletAdjustment` audit record.

Seed data is created by `DataSeeder` only when the database has no users.

## Testing

Backend tests:

```bash
cd SchoolManagementSystem.api
dotnet test
```

Frontend tests:

```bash
cd school-managment-system-app
npx vitest run
```

Frontend build:

```bash
cd school-managment-system-app
npm run build
```

Frontend lint:

```bash
cd school-managment-system-app
npm run lint
```

## Troubleshooting

| Problem | Fix |
| --- | --- |
| Browser cannot call the API over HTTPS | Run `dotnet dev-certs https --trust` and restart the browser |
| Frontend points to the wrong API port | Set `VITE_API_BASE_URL` in `school-managment-system-app/.env` |
| Database login or connection fails | Update `DefaultConnection` in `appsettings.Development.json` |
| Users are not seeded | Confirm the app is using the expected database and that migrations ran |
| Swagger returns 404 | Run with `ASPNETCORE_ENVIRONMENT=Development` or use the included `https` launch profile |
| Wallet deduction fails | Confirm the deduction amount does not exceed the user's current balance |

## Development Notes

- `Program.cs` enables Swagger only in development.
- CORS currently allows any origin for local development.
- The frontend uses the server-returned `resultingBalance` after wallet adjustments instead of calculating the final balance locally.
- The root folder name is `wallet-admin-app`, while the project namespaces and frontend package still use `SchoolManagementSystem`.
