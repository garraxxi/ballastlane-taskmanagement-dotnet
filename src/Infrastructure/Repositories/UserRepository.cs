using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LiteDbContext _context;

    public UserRepository(LiteDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _context.Users.FindOne(x => x.Email == email.ToLowerInvariant());
        return Task.FromResult<User?>(user);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = _context.Users.FindOne(x => x.Id == id);
        return Task.FromResult<User?>(user);
    }

    public Task<User> CreateAsync(User user)
    {
        _context.Users.Insert(user);
        return Task.FromResult(user);
    }
}
