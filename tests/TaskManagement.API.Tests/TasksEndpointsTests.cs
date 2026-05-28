using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagement.Application.DTOs.Tasks;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.API.Tests;

/// <summary>
/// Integration tests for Task endpoints using WebApplicationFactory.
/// Each test creates its own client(s) for better isolation.
/// </summary>
public class TasksEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TasksEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<(HttpClient Client, string Token)> CreateAuthenticatedClientAsync(string? email = null)
    {
        var client = CreateClient();
        email ??= $"user_{Guid.NewGuid()}@test.com";

        var (token, _) = await TestAuthHelper.RegisterAndGetTokenAsync(
            client, 
            email: email, 
            password: "TestPass123!");

        return (client, token);
    }

    // ==================== AUTH / SECURITY ====================

    [Fact]
    public async Task GetMyTasks_Returns401_WhenNoToken()
    {
        var client = CreateClient(); // No auth header

        var response = await client.GetAsync("/api/Tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_Returns401_WhenNoToken()
    {
        var client = CreateClient();
        var request = new CreateTaskRequest("Test", "Desc", null);

        var response = await client.PostAsJsonAsync("/api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== CRUD + OWNERSHIP ====================

    [Fact]
    public async Task GetMyTasks_Returns200_WithTasks_ForAuthenticatedUser()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/Tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTask_Returns201_AndTask_WhenValidRequest()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var request = new CreateTaskRequest(
            "Integration Test Task", 
            "Created from API test", 
            DateTime.UtcNow.AddDays(3));

        var response = await client.PostAsJsonAsync("/api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<TaskDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Integration Test Task");
        created.Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public async Task CreateTask_Returns400_WhenTitleIsEmpty()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var request = new CreateTaskRequest("", "Description", null);

        var response = await client.PostAsJsonAsync("/api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTaskById_Returns200_WhenUserOwnsTask()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Create a task
        var createResp = await client.PostAsJsonAsync("/api/Tasks", 
            new CreateTaskRequest("Owned Task", "Test", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        // Get it back
        var response = await client.GetAsync($"/api/Tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetTaskById_Returns404_WhenTaskBelongsToDifferentUser()
    {
        // User A creates a task
        var (clientA, _) = await CreateAuthenticatedClientAsync("userA@test.com");
        var createResp = await clientA.PostAsJsonAsync("/api/Tasks", 
            new CreateTaskRequest("User A Private Task", "", null));
        var taskA = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        // User B tries to access it
        var (clientB, _) = await CreateAuthenticatedClientAsync("userB@test.com");
        var response = await clientB.GetAsync($"/api/Tasks/{taskA!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_Returns200_WhenOwnerUpdates()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var createResp = await client.PostAsJsonAsync("/api/Tasks", 
            new CreateTaskRequest("Original Title", "Original", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var updateRequest = new UpdateTaskRequest("Updated Title", null, null, null);
        var response = await client.PutAsJsonAsync($"/api/Tasks/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();
        updated!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteTask_Returns204_WhenOwnerDeletes()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var createResp = await client.PostAsJsonAsync("/api/Tasks", 
            new CreateTaskRequest("To Be Deleted", "", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var response = await client.DeleteAsync($"/api/Tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_Returns404_WhenDeletingOtherUsersTask()
    {
        // User A creates task
        var (clientA, _) = await CreateAuthenticatedClientAsync("owner@test.com");
        var createResp = await clientA.PostAsJsonAsync("/api/Tasks", 
            new CreateTaskRequest("Someone Else Task", "", null));
        var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        // User B tries to delete it
        var (clientB, _) = await CreateAuthenticatedClientAsync("attacker@test.com");
        var response = await clientB.DeleteAsync($"/api/Tasks/{task!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
