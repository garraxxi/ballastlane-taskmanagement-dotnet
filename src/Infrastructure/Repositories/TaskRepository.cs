using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly LiteDbContext _context;

    public TaskRepository(LiteDbContext context)
    {
        _context = context;
    }

    public Task<List<TaskItem>> GetByUserIdAsync(Guid userId)
    {
        var tasks = _context.Tasks.Find(x => x.UserId == userId).ToList();
        return Task.FromResult(tasks);
    }

    public Task<TaskItem?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        var task = _context.Tasks.FindOne(x => x.Id == id && x.UserId == userId);
        return Task.FromResult<TaskItem?>(task);
    }

    public Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Insert(task);
        return Task.FromResult(task);
    }

    public Task<TaskItem> UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        return Task.FromResult(task);
    }

    public Task DeleteAsync(Guid id)
    {
        _context.Tasks.Delete(id);
        return Task.CompletedTask;
    }
}
