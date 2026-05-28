namespace TaskManagement.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string Description,
    DateTime? DueDate
);
