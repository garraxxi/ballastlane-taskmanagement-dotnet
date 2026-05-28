# Ballast Lane - Task Management System

Full-stack task management application built as a .NET technical interview exercise for Ballast Lane.

- **Backend**: .NET 10 Web API following strict Clean Architecture (Domain → Application → Infrastructure → API)
- **Data**: LiteDB (embedded NoSQL, file-based) — no Entity Framework, Dapper, or Mediator
- **Auth**: JWT Bearer tokens with BCrypt password hashing
- **Frontend**: Angular 21 SPA (standalone components, Tailwind, Signals, Vitest)
- **API Docs**: Scalar UI (modern OpenAPI client)

## Informal User Story

As a busy professional, I want to securely manage my personal tasks (create, view, update status, delete) so that I can stay organized and meet my deadlines. I need to register and log in so that only I can see and modify my own tasks.

## Prerequisites

- .NET 10 SDK
- Node.js 20+ and npm
- (Optional) Angular CLI (`npm install -g @angular/cli`) if you want to use `ng` commands directly

## Quick Start (Demo)

1. **Start the backend** (from repo root):

   ```bash
   cd src/API
   dotnet run
   ```

   The API runs on:
   - HTTPS: `https://localhost:7171`
   - HTTP: `http://localhost:5171`

2. **Start the frontend** (in a new terminal, from repo root):

   ```bash
   cd task-management-frontend
   npm install          # first time only
   ng serve             # or: npm start
   ```

   Open http://localhost:4200

3. **Login with the seeded demo account**:

   - Email: `demo@taskmanagement.com`
   - Password: `Demo123!`

   Three sample tasks are pre-loaded for the demo user.

## API Documentation & Testing

Modern API reference UI (with built-in HTTP client):

- https://localhost:7171/scalar/v1

All task endpoints require a valid JWT Bearer token (obtained from `/api/Auth/login` or `/api/Auth/register`).

## Running Tests

**Backend (.NET):**

```bash
dotnet test
```

- 12 focused unit tests for Application layer (services + Result pattern + validation + ownership)
- 9 tests for Infrastructure layer (repositories with in-memory LiteDB)

**Frontend (Angular + Vitest):**

```bash
cd task-management-frontend
npm test -- --watch=false
```

## Project Structure

```
.
├── README.md
├── docs/
│   ├── architecture.md          # Detailed Clean Architecture overview
│   └── openapi.yaml             # Complete OpenAPI 3.1 spec
├── src/
│   ├── API/                     # ASP.NET Core 10 Web API (controllers, DI, seeding, Scalar)
│   ├── Application/             # Business logic, DTOs, interfaces, Result<T> pattern
│   ├── Domain/                  # Pure entities + enums (no dependencies)
│   └── Infrastructure/          # LiteDB, repositories, JWT token service
├── tests/
│   ├── TaskManagement.Application.Tests/
│   └── TaskManagement.Infrastructure.Tests/
└── task-management-frontend/    # Angular 21 standalone SPA
```

## Key Technical Decisions

- **Clean Architecture** enforced: Dependencies point inward only. Application layer defines all interfaces.
- **LiteDB** chosen to satisfy the "no EF / Dapper / Mediator" constraint while still demonstrating proper repository abstraction and indexing.
- **Result<T>** pattern for explicit success/failure handling instead of exceptions.
- **JWT + BCrypt** for stateless authentication with 24h expiry.
- **Frontend**: Signals + standalone components (no NgModules), functional interceptors/guards, Tailwind utility classes.

See [docs/architecture.md](docs/architecture.md) for full diagrams, layer responsibilities, and endpoint table.

## Seeded Demo Data

On first startup (when no users exist), the backend automatically seeds:
- User: `demo@taskmanagement.com` / `Demo123!`
- 3 tasks with varying statuses and due dates (including one referencing "Write unit tests")

## Security Notes

- The JWT signing secret lives in `appsettings.json` (acceptable for a local demo / interview project).
- A stray development secret file that was previously committed has been removed and added to `.gitignore`.
- In a real production system you would use Azure Key Vault / AWS Secrets Manager / user secrets + environment variables.

## Next Steps / Known Polish Items

- Full task editing UI in the frontend dashboard (currently supports status changes inline + full create/delete)
- Additional edge-case and controller integration tests

This project was developed with heavy use of generative AI tooling. See [docs/genai-usage.md](docs/genai-usage.md) for the exact prompts used, validation steps, corrections, and how edge cases/authentication/validation were handled.

---

Built for the Ballast Lane .NET Technical Interview Exercise (V5).
