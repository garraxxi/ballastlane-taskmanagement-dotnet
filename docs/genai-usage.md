# Generative AI Usage & Prompt Engineering

This document demonstrates fluency with GenAI tools (as required by the Ballast Lane interview exercise) and shows the critical thinking applied while building the solution.

## Overall Approach

The majority of the initial scaffolding, entity/DTO definitions, basic service skeletons, Angular components, and OpenAPI document were generated using GenAI coding assistants (primarily Grok 4 + Claude 3.5 / Cursor).

The **key value** came from:
1. Writing highly specific, constraint-driven prompts.
2. Iteratively reviewing, rejecting, and heavily editing the output.
3. Using the AI as a "very fast junior developer" rather than trusting it as an architect.

## Prompt 1 – Backend API Scaffold (Core Prompt)

**Prompt sent:**

```
You are an expert .NET architect specializing in Clean Architecture.

Generate a complete .NET 10 Web API project for a personal task management system with the following strict constraints:

- MUST follow Clean Architecture (Domain, Application, Infrastructure, API layers)
- Domain must be completely pure with zero NuGet packages
- Use LiteDB (embedded NoSQL) for persistence — DO NOT use Entity Framework, Dapper, or any ORM
- NO Mediator pattern
- Implement full user registration + login with JWT Bearer authentication (HMAC-SHA256)
- Tasks must be strictly scoped to the authenticated user (ownership enforced)
- Each task has: title, description, status (Todo/InProgress/Done), dueDate (nullable), timestamps
- Use the Result<T> success/failure pattern instead of throwing exceptions for business errors
- Include proper repository interfaces in Application and concrete implementations in Infrastructure
- Seed a demo user (demo@taskmanagement.com / Demo123!) + 3 sample tasks on startup if DB is empty
- Use BCrypt.Net-Next for password hashing
- Modern .NET 10 minimal hosting + native OpenAPI + Scalar UI (not Swashbuckle)

Provide the folder structure first, then the key files (entities, enums, Result<T>, ITaskService + TaskService, repositories, controllers, Program.cs, DependencyInjection).

Explain every major design decision you make.
```

**What the AI initially produced (summary of problems):**
- Suggested using EF Core "for simplicity" despite explicit instructions.
- Put too much logic in controllers (anemic services).
- Used `throw new Exception(...)` everywhere instead of Result<T>.
- Proposed Mediator + CQRS "because it's modern".
- Created a single monolithic `TaskManagementContext` instead of proper repositories.
- Forgot user isolation on several endpoints.

**How I validated + corrected:**
- Rejected the EF suggestion immediately and re-prompted with stronger language.
- Manually rewrote large portions of the service layer to enforce ownership checks (`GetByIdAndUserIdAsync` pattern in the repository).
- Replaced all exception-based flows with the `Result<T>` pattern (I provided the Result class skeleton and asked the AI to use it consistently).
- Removed every Mediator reference.
- Added the explicit `LiteDbContext` + index creation myself.

## Prompt 2 – Authentication & Security Details

**Follow-up prompt excerpt:**

```
Refactor the auth implementation:
- Use BCrypt.Net-Next (not ASP.NET Identity)
- JWT must include NameIdentifier claim for user ID
- 24 hour expiry, no refresh tokens for this exercise
- Register must reject duplicate emails (case-insensitive)
- Login must verify the BCrypt hash

Show the full AuthService + JwtTokenService + how claims are read in the TasksController.
```

**Improvements made after review:**
- Added lowercasing of email on registration and lookup.
- Moved token generation behind `IJwtTokenService` interface (good for testing).
- Controller now uses `ClaimTypes.NameIdentifier` with fallback to "sub".

## Prompt 3 – Frontend (Angular 21)

**Prompt:**

```
Generate a modern Angular 21 standalone-component SPA (no NgModules) for the task management system.

Requirements:
- Tailwind CSS 4
- Signals for all state (tasks, loading, modals)
- Functional HttpInterceptor for attaching Bearer token from localStorage
- Functional CanActivateFn auth guard
- Responsive dashboard with Tailwind cards + status dropdowns
- Create task via modal (datetime-local for due date)
- Inline status updates that call PUT
- Delete with confirmation
- Login/Register toggle form

Use environment files for the API base URL. Prefer reactive forms or template-driven with ngModel.
```

**What needed heavy editing:**
- AI initially generated NgModule-based code (old habits).
- Suggested `@angular/material` instead of pure Tailwind.
- Created overly complex NgRx store for a simple CRUD demo.
- Missed proper error handling from the `ProblemDetails` responses.

**Final state:** The current `tasks.component.ts/html` is ~40% AI-generated skeleton + 60% manual refinement for signals, proper error display, and due-date handling.

## Key Edge Cases & Validations I Explicitly Added / Fixed

| Area                    | AI Initial Weakness                  | Final Improvement                                      |
|-------------------------|--------------------------------------|--------------------------------------------------------|
| Task ownership          | Sometimes allowed cross-user access  | All service methods require `userId` + repo methods filter by `(id, userId)` |
| Partial updates         | Naive full replace                   | Only non-null provided fields are applied (with one later bug fix) |
| DueDate clearing        | Not handled                          | Special handling in service (still has one subtle bug we will fix) |
| Duplicate email         | Not validated                        | Explicit check in `AuthService.RegisterAsync`          |
| Title validation        | Only on client                       | Server-side required check in `TaskService`            |
| Error responses         | Inconsistent                         | Consistent use of `ProblemDetails` + documented shape  |
| In-memory testing       | No guidance                          | Repository tests use LiteDB ":memory:" + IDisposable   |

## What I Would Do Differently / Lessons

- I would have prompted the AI earlier to generate the **test projects** alongside the implementation (TDD style). We added excellent coverage later, but it would have been even stronger to have the tests generated first.
- The initial LiteDB connection string handling was sloppy — I had to fix the DI registration manually (see bug fixes in main README).
- For the frontend, asking for "signals-first" + "no NgRx" in the first prompt would have saved time.

## Conclusion

GenAI dramatically accelerated the **mechanical** parts of the project (boilerplate, DTOs, basic CRUD flows, Angular component wiring). The real engineering work — and the parts that demonstrate senior-level thinking — were:

- Enforcing architectural boundaries when the AI wanted to take shortcuts
- Designing the ownership + security model
- Choosing LiteDB + Result<T> under the interview constraints
- Writing the comprehensive unit tests that the original AI output completely ignored

All prompts, iterations, and rejections were done inside the coding session. The final codebase reflects deliberate architectural choices, not blind acceptance of generated code.

This approach matches the interview expectation of "fluency with GenAI tools **and** critical thinking when evaluating AI-generated code."
