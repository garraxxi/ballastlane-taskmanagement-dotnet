using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Tasks;

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    TaskStatus? Status,
    DateTime? DueDate
);

