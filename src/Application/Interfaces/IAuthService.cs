using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
