using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.API.Tests;

public static class TestAuthHelper
{
    /// <summary>
    /// Registers a new user via the API and returns the JWT token + response.
    /// Useful for tests that need an authenticated user.
    /// </summary>
    public static async Task<(string Token, AuthResponse Response)> RegisterAndGetTokenAsync(
        HttpClient client,
        string email = "testuser@example.com",
        string password = "TestPass123!",
        string fullName = "Test User")
    {
        var registerRequest = new RegisterRequest(email, password, fullName);

        var response = await client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        if (authResponse is null)
            throw new Exception("Failed to deserialize auth response");

        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse.Token);

        return (authResponse.Token, authResponse);
    }

    /// <summary>
    /// Logs in with the demo user credentials and attaches the token to the client.
    /// </summary>
    public static async Task<string> LoginWithDemoUserAsync(HttpClient client)
    {
        var loginRequest = new LoginRequest("demo@taskmanagement.com", "Demo123!");

        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse is null)
            throw new Exception("Failed to deserialize auth response during demo login");

        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse.Token);

        return authResponse.Token;
    }
}
