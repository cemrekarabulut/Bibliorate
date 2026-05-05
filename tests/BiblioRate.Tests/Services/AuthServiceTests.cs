using BiblioRate.Application.DTOs;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using BiblioRate.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenSuccessful()
    {
        // Arrange
        var request = new UserRegisterDto { Username = "newuser", Email = "new@test.com", Password = "password" };
        _mockUserRepository.Setup(u => u.UserExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        _mockUserRepository.Verify(u => u.AddUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Username = "user", Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        _mockUserRepository.Setup(u => u.GetByUsernameAsync("user")).ReturnsAsync(user);
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("verylongsecretkeythatisatleast32characterslong");

        // Act
        var result = await _authService.LoginAsync(new UserLoginDto { Username = "user", Password = "password" });

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateFields()
    {
        // Arrange
        var user = new User { Id = 1, Username = "old" };
        _mockUserRepository.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(user);
        var request = new UpdateProfileRequest { Username = "new", Bio = "New Bio" };

        // Act
        await _authService.UpdateProfileAsync(1, request);

        // Assert
        user.Username.Should().Be("new");
        _mockUserRepository.Verify(u => u.UpdateProfileAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
    {
        // Arrange
        _mockUserRepository.Setup(u => u.GetByUsernameAsync("user")).ReturnsAsync((User)null!);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(new UserLoginDto { Username = "user", Password = "wrong" });

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepository.Setup(u => u.GetByIdAsync(999)).ReturnsAsync((User)null!);

        // Act
        Func<Task> act = async () => await _authService.UpdateProfileAsync(999, new UpdateProfileRequest());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
