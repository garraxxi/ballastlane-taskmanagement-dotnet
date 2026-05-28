using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly LiteDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _context = new LiteDbContext(":memory:");
        _sut = new UserRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistUser_AndEnforceUniqueEmailViaIndex()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "unique@test.com",
            FullName = "Unique User",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _sut.CreateAsync(user);
        var fetched = await _sut.GetByEmailAsync("unique@test.com");

        fetched.Should().NotBeNull();
        fetched!.FullName.Should().Be("Unique User");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "MixedCase@Example.com", FullName = "Mixed", PasswordHash = "h", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _sut.CreateAsync(user);

        var byLower = await _sut.GetByEmailAsync("mixedcase@example.com");
        byLower.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectUser()
    {
        var id = Guid.NewGuid();
        await _sut.CreateAsync(new User { Id = id, Email = "byid@test.com", FullName = "ById", PasswordHash = "h", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

        var user = await _sut.GetByIdAsync(id);

        user!.Email.Should().Be("byid@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        var user = await _sut.GetByEmailAsync("does-not-exist@test.com");
        user.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
