using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<List<TaskDto>>> GetMyTasksAsync(Guid userId)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId);
        var dtos = tasks.Select(MapToDto).ToList();
        return Result<List<TaskDto>>.Success(dtos);
    }

    public async Task<Result<TaskDto>> GetTaskByIdAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
        if (task is null)
            return Result<TaskDto>.Failure("Task not found");

        return Result<TaskDto>.Success(MapToDto(task));
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<TaskDto>.Failure("Title is required");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = TaskStatus.Todo,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _taskRepository.CreateAsync(task);
        return Result<TaskDto>.Success(MapToDto(created));
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId)
    {
        var existing = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
        if (existing is null)
            return Result<TaskDto>.Failure("Task not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            existing.Title = request.Title.Trim();

        if (request.Description is not null)
            existing.Description = request.Description.Trim();

        if (request.Status.HasValue)
            existing.Status = request.Status.Value;

        // Only update DueDate when the client explicitly sends the property.
        // - HasValue == true  → set to the provided date
        // - HasValue == false && request.DueDate == null → we currently cannot reliably distinguish
        //   "client omitted the field" from "client sent null to clear".
        // For this implementation we only apply when HasValue (common case). Clearing an existing
        // due date can be added later with a better DTO shape (e.g. a separate "clearDueDate" flag).
        if (request.DueDate.HasValue)
            existing.DueDate = request.DueDate;

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _taskRepository.UpdateAsync(existing);
        return Result<TaskDto>.Success(MapToDto(updated));
    }

    public async Task<Result<bool>> DeleteTaskAsync(Guid taskId, Guid userId)
    {
        var existing = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
        if (existing is null)
            return Result<bool>.Failure("Task not found");

        await _taskRepository.DeleteAsync(taskId);
        return Result<bool>.Success(true);
    }

    private static TaskDto MapToDto(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.DueDate, task.CreatedAt, task.UpdatedAt);
}
