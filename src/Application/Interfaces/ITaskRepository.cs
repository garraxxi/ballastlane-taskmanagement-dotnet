using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetByUserIdAsync(Guid userId);
    Task<TaskItem?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task DeleteAsync(Guid id);
}
