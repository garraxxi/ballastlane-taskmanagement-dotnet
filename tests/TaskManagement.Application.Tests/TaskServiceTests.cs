using FluentAssertions;
using Moq;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _sut = new TaskService(_taskRepoMock.Object);
    }

    [Fact]
    public async Task GetMyTasksAsync_ShouldReturnSuccessWithDtos_WhenRepositoryReturnsTasks()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Task 1", Description = "Desc", Status = TaskStatus.Todo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _taskRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(tasks);

        var result = await _sut.GetMyTasksAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnFailure_WhenTaskNotFoundForUser()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAndUserIdAsync(taskId, userId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetTaskByIdAsync(taskId, userId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Task not found");
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldReturnFailure_WhenTitleIsEmptyOrWhitespace()
    {
        var request = new CreateTaskRequest("", "Some description", null);
        var userId = Guid.NewGuid();

        var result = await _sut.CreateTaskAsync(request, userId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Title is required");
        _taskRepoMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldCreateTodoTaskWithTimestamps_WhenValidRequest()
    {
        var request = new CreateTaskRequest("Valid Title", "Description here", DateTime.UtcNow.AddDays(3));
        var userId = Guid.NewGuid();
        TaskItem? captured = null;
        _taskRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => captured = t)
            .ReturnsAsync((TaskItem t) => t);

        var result = await _sut.CreateTaskAsync(request, userId);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Title.Should().Be("Valid Title");
        captured.Status.Should().Be(TaskStatus.Todo);
        captured.UserId.Should().Be(userId);
        captured.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldReturnFailure_WhenTaskDoesNotBelongToUser()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAndUserIdAsync(taskId, userId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.UpdateTaskAsync(taskId, new UpdateTaskRequest(null, null, TaskStatus.InProgress, null), userId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Task not found");
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldOnlyUpdateProvidedFields_AndAlwaysSetUpdatedAt()
    {
        var userId = Guid.NewGuid();
        var existing = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Original",
            Description = "Original desc",
            Status = TaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _taskRepoMock.Setup(r => r.GetByIdAndUserIdAsync(existing.Id, userId)).ReturnsAsync(existing);
        _taskRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        var request = new UpdateTaskRequest("New Title", null, null, null); // only title provided
        var result = await _sut.UpdateTaskAsync(existing.Id, request, userId);

        result.IsSuccess.Should().BeTrue();
        existing.Title.Should().Be("New Title");
        existing.Description.Should().Be("Original desc"); // unchanged
        existing.Status.Should().Be(TaskStatus.Todo);       // unchanged
        existing.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnSuccess_WhenTaskBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var existing = new TaskItem { Id = taskId, UserId = userId, Title = "To delete" };
        _taskRepoMock.Setup(r => r.GetByIdAndUserIdAsync(taskId, userId)).ReturnsAsync(existing);
        _taskRepoMock.Setup(r => r.DeleteAsync(taskId)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteTaskAsync(taskId, userId);

        result.IsSuccess.Should().BeTrue();
        _taskRepoMock.Verify(r => r.DeleteAsync(taskId), Times.Once);
    }
}
