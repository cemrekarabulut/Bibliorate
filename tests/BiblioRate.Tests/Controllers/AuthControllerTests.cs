using System.Security.Claims;
using BiblioRate.API.Controllers;
using BiblioRate.Application.DTOs;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new AuthController(_authServiceMock.Object);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new UserRegisterDto { Username = "testuser", Email = "test@test.com", Password = "Pass123!" };
        var response = new AuthResponseDto { UserId = "1", Username = "testuser", Email = "test@test.com" };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Register(request);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().Be(response);
    }

    [Fact]
    public async Task Register_DuplicateUser_Returns409Conflict()
    {
        // Arrange
        var request = new UserRegisterDto { Username = "exists", Email = "exists@test.com", Password = "Pass123!" };
        _authServiceMock.Setup(s => s.RegisterAsync(request))
                        .ThrowsAsync(new InvalidOperationException("Kullanıcı zaten mevcut."));

        // Act
        var result = await _sut.Register(request);

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(409);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Arrange
        var request = new UserLoginDto { Username = "testuser", Password = "Pass123!" };
        var response = new AuthResponseDto { UserId = "1", Username = "testuser", Token = "jwt.token.here" };
        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Login(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        var request = new UserLoginDto { Username = "bad", Password = "wrong" };
        _authServiceMock.Setup(s => s.LoginAsync(request))
                        .ThrowsAsync(new UnauthorizedAccessException("Şifre hatalı."));

        // Act
        var result = await _sut.Login(request);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_ValidToken_Returns200Ok()
    {
        // Arrange
        var request = new UpdateProfileRequest { Username = "newname" };
        var response = new AuthResponseDto { UserId = "42", Username = "newname" };
        _authServiceMock.Setup(s => s.UpdateProfileAsync(42, request)).ReturnsAsync(response);

        SetUserClaim(_sut, "42");

        // Act
        var result = await _sut.UpdateProfile(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task UpdateProfile_NoUserIdInToken_Returns401()
    {
        // Arrange — no claims at all
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _sut.UpdateProfile(new UpdateProfileRequest());

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_UserNotFound_Returns404()
    {
        // Arrange
        _authServiceMock.Setup(s => s.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<UpdateProfileRequest>()))
                        .ThrowsAsync(new KeyNotFoundException("Kullanıcı bulunamadı."));
        SetUserClaim(_sut, "99");

        // Act
        var result = await _sut.UpdateProfile(new UpdateProfileRequest());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_DuplicateUsername_Returns409()
    {
        // Arrange
        _authServiceMock.Setup(s => s.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<UpdateProfileRequest>()))
                        .ThrowsAsync(new InvalidOperationException("Kullanıcı adı zaten alınmış."));
        SetUserClaim(_sut, "5");

        // Act
        var result = await _sut.UpdateProfile(new UpdateProfileRequest { Username = "taken" });

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void SetUserClaim(ControllerBase controller, string userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
