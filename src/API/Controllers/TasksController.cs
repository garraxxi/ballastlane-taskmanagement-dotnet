using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Task management operations. All endpoints require authentication via JWT Bearer token.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Get all tasks belonging to the currently authenticated user
    /// </summary>
    /// <response code="200">Returns the list of tasks</response>
    /// <response code="401">Unauthorized - valid JWT Bearer token required</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.GetMyTasksAsync(userId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new ProblemDetails { Title = result.Error });
    }

    /// <summary>
    /// Get a single task by ID (only if it belongs to the current user)
    /// </summary>
    /// <param name="id">Task identifier</param>
    /// <response code="200">Returns the requested task</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Task not found or does not belong to the user</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.GetTaskByIdAsync(id, userId);
        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(new ProblemDetails { Title = result.Error });
    }

    /// <summary>
    /// Create a new task for the authenticated user
    /// </summary>
    /// <param name="request">Task creation details</param>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.CreateTaskAsync(request, userId);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Title = result.Error });

        return CreatedAtAction(nameof(GetTask), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing task (partial update supported)
    /// </summary>
    /// <param name="id">Task identifier</param>
    /// <param name="request">Fields to update (any subset is allowed)</param>
    /// <response code="200">Task updated successfully</response>
    /// <response code="400">Validation or business rule error</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Task not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.UpdateTaskAsync(id, request, userId);
        return result.IsSuccess
            ? Ok(result.Data)
            : result.Error == "Task not found"
                ? NotFound(new ProblemDetails { Title = result.Error })
                : BadRequest(new ProblemDetails { Title = result.Error });
    }

    /// <summary>
    /// Delete a task belonging to the current user
    /// </summary>
    /// <param name="id">Task identifier</param>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.DeleteTaskAsync(id, userId);

        if (!result.IsSuccess)
            return result.Error == "Task not found"
                ? NotFound(new ProblemDetails { Title = result.Error })
                : BadRequest(new ProblemDetails { Title = result.Error });

        return NoContent();
    }
}
