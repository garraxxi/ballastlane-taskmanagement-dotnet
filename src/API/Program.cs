using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Auth;
using TaskManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICES
// ============================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === DEFINITIVE OPENAPI + SCALAR SETUP (Modern .NET 10 native approach) ===
// This is the clean, future-proof way in .NET 10. No more Swashbuckle/OpenAPI version conflicts.
// Use the modern .NET 10 OpenAPI support with basic customization
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Task Management API";
        document.Info.Version = "1.0";
        document.Info.Description =
            "RESTful API for a personal task management system.\n\n" +
            "Built following Clean Architecture principles using LiteDB (no EF/Dapper/Mediator).\n" +
            "All task operations are strictly scoped to the authenticated user.";

        return Task.CompletedTask;
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ThisIsASuperSecretKeyForDevelopmentOnly123456789!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagement";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagementUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register layers
builder.Services.AddInfrastructure(); // LiteDB + repositories + JWT token service (uses default "TaskManagement.db")
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS for Angular frontend (localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ============================================
// SEED DEMO DATA (for easy demo during interview)
// ============================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LiteDbContext>();
    SeedData(context);
}

// ============================================
// PIPELINE - Modern OpenAPI + Scalar UI (definitive for .NET 10)
// ============================================
app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Task Management API")
        .WithTheme(ScalarTheme.Default)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddPreferredSecuritySchemes("Bearer");
});

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ============================================
// SEEDING HELPER
// ============================================
static void SeedData(LiteDbContext context)
{
    if (context.Users.FindAll().Any())
        return;

    var demoUser = new TaskManagement.Domain.Entities.User
    {
        Id = Guid.NewGuid(),
        Email = "demo@taskmanagement.com",
        FullName = "Demo User",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    context.Users.Insert(demoUser);

    var tasks = new List<TaskManagement.Domain.Entities.TaskItem>
    {
        new()
        {
            Id = Guid.NewGuid(),
            UserId = demoUser.Id,
            Title = "Prepare for Ballast Lane interview",
            Description = "Complete the .NET technical exercise following Clean Architecture principles",
            Status = TaskManagement.Domain.Enums.TaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = Guid.NewGuid(),
            UserId = demoUser.Id,
            Title = "Review Clean Architecture",
            Description = "Make sure layers are properly separated and dependencies point inward",
            Status = TaskManagement.Domain.Enums.TaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = Guid.NewGuid(),
            UserId = demoUser.Id,
            Title = "Write unit tests for services",
            Description = "Achieve good coverage on Application layer",
            Status = TaskManagement.Domain.Enums.TaskStatus.Todo,
            DueDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };

    context.Tasks.Insert(tasks);
}
