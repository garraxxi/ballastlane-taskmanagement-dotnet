using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

