using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Authentication operations (public endpoints)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user and receive a JWT token
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">User created successfully</response>
    /// <response code="400">Validation error or email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Title = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Login with email and password to obtain a JWT Bearer token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.IsSuccess)
            return Unauthorized(new ProblemDetails { Title = result.Error });

        return Ok(result.Data);
    }
}
