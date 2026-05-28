namespace TaskManagement.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string Email,
    string FullName
);
