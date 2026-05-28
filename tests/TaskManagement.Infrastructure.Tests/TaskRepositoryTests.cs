using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests;

public class TaskRepositoryTests
{
    private readonly LiteDbContext _context;
    private readonly TaskRepository _sut;

    public TaskRepositoryTests()
    {
        // Fresh in-memory database for each test class instance (xUnit creates new instance per test by default)
        _context = new LiteDbContext(":memory:");
        _sut = new TaskRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTask()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Testing LiteDB repo",
            Status = TaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _sut.CreateAsync(task);
        var retrieved = await _sut.GetByIdAndUserIdAsync(task.Id, task.UserId);

        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Test Task");
        retrieved.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyTasksForGivenUser()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        await _sut.CreateAsync(new TaskItem { Id = Guid.NewGuid(), UserId = user1, Title = "U1-1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _sut.CreateAsync(new TaskItem { Id = Guid.NewGuid(), UserId = user1, Title = "U1-2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _sut.CreateAsync(new TaskItem { Id = Guid.NewGuid(), UserId = user2, Title = "U2-1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

        var user1Tasks = await _sut.GetByUserIdAsync(user1);

        user1Tasks.Should().HaveCount(2);
        user1Tasks.All(t => t.UserId == user1).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenTaskBelongsToDifferentUser()
    {
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = owner, Title = "Secret", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _sut.CreateAsync(task);

        var result = await _sut.GetByIdAndUserIdAsync(task.Id, other);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "Old", Status = TaskStatus.Todo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _sut.CreateAsync(task);

        task.Title = "New Title";
        task.Status = TaskStatus.Done;
        await _sut.UpdateAsync(task);

        var updated = await _sut.GetByIdAndUserIdAsync(task.Id, task.UserId);
        updated!.Title.Should().Be("New Title");
        updated.Status.Should().Be(TaskStatus.Done);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "To be deleted", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _sut.CreateAsync(task);

        await _sut.DeleteAsync(task.Id);

        var afterDelete = await _sut.GetByIdAndUserIdAsync(task.Id, task.UserId);
        afterDelete.Should().BeNull();
    }

}
