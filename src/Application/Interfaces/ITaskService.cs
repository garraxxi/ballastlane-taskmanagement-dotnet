using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs.Tasks;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<Result<List<TaskDto>>> GetMyTasksAsync(Guid userId);
    Task<Result<TaskDto>> GetTaskByIdAsync(Guid taskId, Guid userId);
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request, Guid userId);
    Task<Result<TaskDto>> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId);
    Task<Result<bool>> DeleteTaskAsync(Guid taskId, Guid userId);
}
