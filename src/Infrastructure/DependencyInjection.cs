using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Auth;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString = "TaskManagement.db")
    {
        services.AddSingleton<LiteDbContext>(_ => new LiteDbContext(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
