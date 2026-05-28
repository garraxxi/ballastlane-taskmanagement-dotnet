# Ballast Lane - Task Management Architecture

## Overview

Full-stack task management application using **Clean Architecture** on the backend (.NET 10) with a **standalone Angular 21 SPA** frontend, connected via REST over HTTP with JWT Bearer authentication.

```
┌──────────────────────────────────────────────────────┐
│                   Angular 21 SPA                      │
│              (localhost:4200)                         │
│   Auth Page (Login / Register)                        │
│         ↓                                             │
│   Tasks Dashboard (CRUD, status updates)              │
│         ↑                                             │
│   JWT Bearer Token injected via HTTP Interceptor      │
├──────────────────────────────────────────────────────┤
│              ASP.NET Core 10 Web API                   │
│              (https://localhost:7171)                  │
│              (http://localhost:5171)                   │
│                                                        │
│   ┌────────────────────────────────────────────────┐  │
│   │  API Layer (Controllers)                      │  │
│   │  AuthController   [AllowAnonymous]            │  │
│   │    POST /api/Auth/login                       │  │
│   │    POST /api/Auth/register                    │  │
│   │  TasksController  [Authorize]                 │  │
│   │    GET    /api/Tasks                          │  │
│   │    GET    /api/Tasks/{id}                     │  │
│   │    POST   /api/Tasks                          │  │
│   │    PUT    /api/Tasks/{id}                     │  │
│   │    DELETE /api/Tasks/{id}                     │  │
│   ├────────────────────────────────────────────────┤  │
│   │  Application Layer (Services + DTOs)          │  │
│   │  ITaskService → TaskService                   │  │
│   │  IAuthService → AuthService                   │  │
│   │  Interfaces: ITaskRepository, IUserRepository, │  │
│   │             IJwtTokenService                  │  │
│   │  DTOs: TaskDto, CreateTaskRequest,            │  │
│   │        UpdateTaskRequest, AuthResponse,        │  │
│   │        LoginRequest, RegisterRequest           │  │
│   │  Common: Result<T> (Success/Failure pattern)  │  │
│   ├────────────────────────────────────────────────┤  │
│   │  Domain Layer (Entities + Enums)              │  │
│   │  TaskItem (Id, UserId, Title, Description,    │  │
│   │            Status, DueDate, timestamps)        │  │
│   │  User (Id, Email, FullName, PasswordHash,      │  │
│   │        timestamps)                             │  │
│   │  TaskStatus enum: Todo, InProgress, Done      │  │
│   ├────────────────────────────────────────────────┤  │
│   │  Infrastructure Layer (Persistence + Auth)    │  │
│   │  LiteDB (embedded NoSQL, file-based)          │  │
│   │  UserRepository, TaskRepository               │  │
│   │  JwtTokenService (HMAC-SHA256, 24h expiry)    │  │
│   │  LiteDbContext (LiteDB wrapper + indexes)     │  │
│   │  DependencyInjection (DI registration)        │  │
│   └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

---

## Backend (.NET 10 / C#)

### Architecture Layers

| Layer | Project | Dependencies | Responsibility |
|-------|---------|-------------|---------------|
| **Domain** | `TaskManagement.Domain` | None | Pure entities & enums. Zero external package references. |
| **Application** | `TaskManagement.Application` | Domain, BCrypt.Net-Next | Business logic services, DTOs, repository interfaces, `Result<T>` pattern. |
| **Infrastructure** | `TaskManagement.Infrastructure` | Domain, Application, LiteDB, JWT packages, BCrypt | Concrete implementations for data access, JWT generation, DI registration. |
| **API** | `TaskManagement.API` | All above + JwtBearer, OpenAPI, Scalar | Controllers, middleware pipeline, CORS, auth config, startup seeding. |

### NuGet Packages

| Package | Purpose |
|---------|---------|
| `BCrypt.Net-Next` 4.2.0 | Password hashing and verification |
| `LiteDB` 5.0.21 | Embedded NoSQL document database (file-based) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.8 | JWT Bearer token validation |
| `Microsoft.AspNetCore.OpenApi` 10.0.8 | Native OpenAPI document generation (.NET 10) |
| `Scalar.AspNetCore` 2.x | Modern API reference UI (replaces Swagger UI) |
| `System.IdentityModel.Tokens.Jwt` 8.0.0 | JWT token creation |
| `Microsoft.IdentityModel.Tokens` 8.0.0 | Token signing/validation primitives |

### Key Design Patterns

- **Clean Architecture**: Strict dependency inversion — Domain innermost, API outermost. Application defines interfaces; Infrastructure implements them. No layer references outward.
- **Repository Pattern**: `ITaskRepository` / `IUserRepository` abstracts data access behind interfaces.
- **Result Pattern**: `Result<T>` with `.IsSuccess` / `.Data` / `.Error` provides explicit error handling without exceptions.
- **Dependency Injection**: All layers registered via `IServiceCollection` extensions (`AddInfrastructure()` in Infrastructure, manual scoped registration in API).
- **JWT Authentication**: HMAC-SHA256 symmetric key, 24-hour token expiry, `ClaimTypes.NameIdentifier` for user identification.

### API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/Auth/register` | Anonymous | Register new user, returns JWT |
| POST | `/api/Auth/login` | Anonymous | Login, returns JWT |
| GET | `/api/Tasks` | Bearer | List current user's tasks |
| GET | `/api/Tasks/{id}` | Bearer | Get single task (owner only) |
| POST | `/api/Tasks` | Bearer | Create new task |
| PUT | `/api/Tasks/{id}` | Bearer | Update task (partial) |
| DELETE | `/api/Tasks/{id}` | Bearer | Delete task (owner only) |

### Demo Data (Seeded on Startup)

- **User**: `demo@taskmanagement.com` / `Demo123!`
- **3 sample tasks** with varying statuses and due dates

---

## Frontend (Angular 21)

### Tech Stack

| Tool | Version | Purpose |
|------|---------|---------|
| Angular | 21.2 | SPA framework (standalone components) |
| TypeScript | 5.9 | Typed JavaScript |
| Tailwind CSS | 4.3 | Utility-first CSS styling |
| RxJS | 7.8 | Reactive HTTP calls + state |
| Vitest | 4.0 | Frontend unit testing (configured via `@angular/build`) |
| xUnit + Moq + FluentAssertions | 2.9 / 4.20 | Backend unit tests for Application & Infrastructure layers (21 tests covering services, validation, ownership, repositories with in-memory LiteDB) |
| ng-openapi-gen | 1.0 | OpenAPI client generation (configured) |

### Project Structure

```
src/app/
├── app.ts                         # Root component (standalone)
├── app.config.ts                  # App-wide providers (router, HTTP client, interceptor)
├── app.routes.ts                  # Route definitions (lazy-loaded components)
├── app.html / app.css             # Root template (just <router-outlet>)
│
├── core/
│   ├── models/
│   │   ├── auth.models.ts         # AuthResponse, LoginRequest, RegisterRequest
│   │   └── task.models.ts         # TaskDto, CreateTaskRequest, UpdateTaskRequest, TaskStatus
│   ├── services/
│   │   ├── auth.service.ts        # Login/register/logout, localStorage token management
│   │   └── task.service.ts        # CRUD HTTP calls to /api/Tasks
│   ├── guards/
│   │   └── auth.guard.ts          # Route guard — redirects to /login if unauthenticated
│   └── interceptors/
│       └── auth.interceptor.ts    # Functional HTTP interceptor — attaches Bearer token
│
├── auth/
│   ├── auth.component.ts          # Login/register form (toggle between modes)
│   └── auth.component.html        # Tailwind-styled auth card
│
└── tasks/
    ├── tasks.component.ts         # Task list, create modal, inline status updates, delete
    └── tasks.component.html       # Dashboard UI with navbar, task cards, create modal
```

### Key Frontend Patterns

- **Standalone Components**: No NgModules — `@Component({ standalone: true })` with lazy routes via `loadComponent()`.
- **Signals**: `signal<T>()` for reactive state (tasks list, loading, error, modal visibility).
- **Functional Guards & Interceptors**: `CanActivateFn` for auth guard, `HttpInterceptorFn` for JWT injection.
- **Local Storage**: Token and user info persisted in `localStorage` under `tm_token` / `tm_user` keys.
- **Tailwind CSS**: Utility classes for all styling — no custom CSS files needed.

---

## How It All Fits Together

1. **User opens the app** → Redirected to `/login` (or `/tasks` if already authenticated).
2. **Auth component**: User logs in or registers → API returns JWT.
3. **Token stored** in localStorage → `AuthService` exposes `currentUser$` observable.
4. **Navigation** to `/tasks` → `AuthGuard` checks `isLoggedIn()`. On every HTTP request, `AuthInterceptor` attaches `Authorization: Bearer <token>`.
5. **Tasks dashboard**: On init, calls `GET /api/Tasks` → displays list. User can create (modal), update status (dropdown), or delete tasks.
6. **Update status**: Dropdown change triggers `PUT /api/Tasks/{id}` with numeric enum (0=Todo, 1=InProgress, 2=Done).
7. **Backend**: `TasksController` extracts `userId` from JWT claims, calls `TaskService` → validates ownership → `TaskRepository` queries/updates LiteDB.
8. **API documentation**: Available at `/scalar/v1` via Scalar UI with live HTTP client testing.
9. **Tests**: Comprehensive backend unit tests (Application + Infrastructure layers) + frontend component tests. Run with `dotnet test` and `npm test -- --watch=false` in the frontend folder. See root README.md for full setup.
