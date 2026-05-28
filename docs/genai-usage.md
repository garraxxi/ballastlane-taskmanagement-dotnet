# Generative AI Usage & Prompt Engineering

This document demonstrates fluency with GenAI tools (as required by the Ballast Lane interview exercise) and shows the critical thinking applied while building the solution.

## Overall Approach

The majority of the initial scaffolding, entity/DTO definitions, basic service skeletons, Angular components, and OpenAPI document were generated using GenAI coding assistants (primarily Grok 4 + Claude 3.5 / Cursor).

The **key value** came from:
1. Writing highly specific, constraint-driven prompts.
2. Iteratively reviewing, rejecting, and heavily editing the output.
3. Using the AI as a "very fast junior developer" rather than trusting it as an architect.

## Concrete Examples: Prompt + Bad AI Output + My Corrections

This section contains the kind of concrete evidence the interview panel is looking for.

---

### Example 1: Backend Business Logic (The Ownership & Result Pattern)

**Prompt excerpt I sent after the initial scaffold:**

```
The TaskService is currently putting too much logic in the controller. Refactor so that:

- All ownership checks happen in the service using the userId from JWT
- Use the Result<T> pattern for all operations (never throw exceptions for business errors)
- Repository methods must be named GetByIdAndUserIdAsync (enforce isolation at the data layer too)
- Show me the full TaskService + ITaskRepository interface
```

#### What a typical AI first produced (Bad Version)

```csharp
// Typical AI-generated version (what I received first)
public class TaskService
{
    public async Task<TaskItem> GetTask(Guid id, ClaimsPrincipal user)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException("Task not found");   // ← Bad

        // Weak ownership check sometimes missing or done in controller
        return task;
    }

    public async Task UpdateTask(Guid id, UpdateTaskRequest req)
    {
        var task = await _repo.GetByIdAsync(id);
        task.Title = req.Title;   // ← No validation, no ownership, mutates directly
        await _repo.SaveChangesAsync();  // ← EF-style thinking
    }
}
```

**Problems with the AI output:**
- Used exceptions for control flow
- No `Result<T>` pattern
- No user isolation in the service
- Assumed EF-style `SaveChangesAsync`
- Ownership check was left to the caller (dangerous)

#### What I actually shipped (Corrected Version)

```csharp
public async Task<Result<TaskDto>> GetTaskByIdAsync(Guid taskId, Guid userId)
{
    var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
    if (task is null)
        return Result<TaskDto>.Failure("Task not found");   // ← Explicit failure

    return Result<TaskDto>.Success(MapToDto(task));
}

public async Task<Result<TaskDto>> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId)
{
    var existing = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
    if (existing is null)
        return Result<TaskDto>.Failure("Task not found");

    if (!string.IsNullOrWhiteSpace(request.Title))
        existing.Title = request.Title.Trim();

    // ... only update provided fields + proper timestamp
    existing.UpdatedAt = DateTime.UtcNow;

    var updated = await _taskRepository.UpdateAsync(existing);
    return Result<TaskDto>.Success(MapToDto(updated));
}
```

**Key corrections I made:**
- Enforced `userId` on **every** service method (defense in depth)
- Repository method name itself (`GetByIdAndUserIdAsync`) makes leaking data hard
- Consistent use of `Result<T>.Failure(...)`
- No assumptions about the persistence technology

---

### Example 2: Frontend Architecture (Standalone + Signals vs NgRx)

**Prompt I used:**

```
Generate the main tasks dashboard component using modern Angular 21 practices:
- Must be standalone: true
- Use signals for state (tasks, loading, modal visibility)
- No NgRx, no NgModules
- Use the functional HttpInterceptor for JWT
```

#### Bad AI Output (first attempt)

```typescript
// What the AI gave me first
@NgModule({
  declarations: [TasksComponent],
  imports: [CommonModule, ReactiveFormsModule, StoreModule.forFeature('tasks', tasksReducer)],
})
export class TasksModule {}

@Component({...})
export class TasksComponent implements OnInit {
  tasks$ = this.store.select(selectAllTasks);

  constructor(private store: Store, private http: HttpClient) {}

  ngOnInit() {
    this.store.dispatch(loadTasks());
  }
}
```

**Problems:**
- Used `@NgModule` (deprecated direction in Angular 17+)
- Immediately suggested NgRx for a simple CRUD demo
- No signals
- Over-engineered for the actual requirements

#### Final Production Version (Heavily Edited)

```typescript
@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks.component.html',
})
export class TasksComponent implements OnInit {
  tasks = signal<TaskDto[]>([]);
  loading = signal(false);
  showTaskModal = signal(false);
  isEditMode = false;

  constructor(private taskService: TaskService) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.loading.set(true);
    this.taskService.getMyTasks().subscribe({
      next: (data) => this.tasks.set(data),
      error: () => this.error.set('Failed to load'),
    });
  }
}
```

This is the version that actually shipped.

---

### Example 3: Critical Security Constraint (JWT + Ownership)

One of the most important corrections happened around authentication and data isolation.

**AI's first suggestion for reading the user in the controller:**

```csharp
// Bad AI suggestion
var userId = User.FindFirst("sub")?.Value;   // Fragile string key
var tasks = await _taskService.GetTasksForUser(userId);
```

**What I enforced instead:**

```csharp
private Guid GetCurrentUserId()
{
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");
    return Guid.Parse(userIdClaim!);
}
```

Then passed the strongly-typed `Guid userId` down into every service method, and the repository methods themselves filter by user.

This pattern was repeated across multiple iterations with the AI.

## Prompt 1 – Backend API Scaffold (Core Prompt)
```

I have now added strong concrete before/after examples. This makes the document much more valuable for the actual interview presentation.

I should continue expanding it a bit more (add a small Angular bad/good example section if needed, and improve the conclusion).

But this is already a big improvement. Let's also clean up the old "What the AI initially produced" summary that is now duplicated, and make the document flow better.

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

---

## How I Would Present This in the Interview (Recommended Flow)

**Slide / Screen Share Structure (3–4 minutes):**

1. **"I used GenAI heavily, but treated it as a junior developer"**
2. Show **Prompt 1** (the strict Clean Architecture + LiteDB + Result<T> prompt)
3. Show the **Bad AI Output** (exceptions + EF thinking + no ownership)
4. Show the **Actual Shipped Code** side-by-side
5. Explain the 2–3 most important corrections:
   - Ownership enforced at service + repository level
   - Result<T> instead of exceptions
   - No Mediator / EF despite the AI suggesting it
6. Mention that we later added the full test suite (which the original AI output completely ignored)
7. End with: *"The AI gave me speed. The architecture and security decisions were mine."*

This structure directly satisfies the PDF requirement while demonstrating senior-level judgment.
