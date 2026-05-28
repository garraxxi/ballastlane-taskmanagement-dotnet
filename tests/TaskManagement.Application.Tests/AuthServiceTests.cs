using FluentAssertions;
using Moq;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenService>();
        _sut = new AuthService(_userRepoMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailOrPasswordMissing()
    {
        var req = new RegisterRequest("", "short", "Full Name");

        var result = await _sut.RegisterAsync(req);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email and password are required");
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var req = new RegisterRequest("existing@example.com", "SecurePass123!", "Jane");
        _userRepoMock.Setup(r => r.GetByEmailAsync("existing@example.com"))
                     .ReturnsAsync(new User { Email = "existing@example.com" });

        var result = await _sut.RegisterAsync(req);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("A user with this email already exists");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_HashPassword_AndReturnToken_WhenValid()
    {
        var req = new RegisterRequest("new@example.com", "SecurePass123!", "New User");
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake.jwt.token");

        var result = await _sut.RegisterAsync(req);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be("new@example.com");
        result.Data.Token.Should().Be("fake.jwt.token");
        _userRepoMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Email == "new@example.com" &&
            !string.IsNullOrWhiteSpace(u.PasswordHash) &&
            u.FullName == "New User")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFoundOrPasswordWrong()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("nobody@example.com")).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest("nobody@example.com", "wrong"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@taskmanagement.com",
            FullName = "Demo User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!")
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("valid.jwt.token");

        var result = await _sut.LoginAsync(new LoginRequest(user.Email, "Demo123!"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().Be("valid.jwt.token");
        result.Data.Email.Should().Be(user.Email);
    }
}
