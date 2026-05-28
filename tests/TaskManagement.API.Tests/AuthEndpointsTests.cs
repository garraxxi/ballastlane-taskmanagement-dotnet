using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.API.Tests;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturn200AndToken_WhenValidData()
    {
        var request = new RegisterRequest("newuser@test.com", "SecurePass123!", "New User");

        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("newuser@test.com");
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailAlreadyExists()
    {
        // First registration
        var request = new RegisterRequest("duplicate@test.com", "SecurePass123!", "User One");
        await _client.PostAsJsonAsync("/api/Auth/register", request);

        // Second registration with same email
        var duplicateRequest = new RegisterRequest("duplicate@test.com", "AnotherPass123!", "User Two");
        var response = await _client.PostAsJsonAsync("/api/Auth/register", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_WithDemoCredentials()
    {
        var request = new LoginRequest("demo@taskmanagement.com", "Demo123!");

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WithInvalidCredentials()
    {
        var request = new LoginRequest("demo@taskmanagement.com", "WrongPassword!");

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
