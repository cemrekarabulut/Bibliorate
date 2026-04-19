using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BiblioRate.Application.DTOs;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BiblioRate.Infrastructure.Services;

/// <summary>
/// IAuthService implementasyonu.
/// - Register: BCrypt ile hash'leyerek kullanıcı kaydeder.
/// - Login:    Hash doğrulaması yapıp JWT token üretir.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration  _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration  = configuration;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Register
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto request)
    {
        // Kullanıcı adı veya e-posta daha önce kullanılmış mı?
        var exists = await _userRepository.UserExistsAsync(request.Username, request.Email);
        if (exists)
            throw new InvalidOperationException("Bu kullanıcı adı veya e-posta adresi zaten kullanımda.");

        var user = new User
        {
            Username     = request.Username.Trim(),
            Email        = request.Email.Trim(),
            // BCrypt hash — UserRepository.AddUserAsync içinde de hash yapılıyor;
            // düz metin gönderilip orada hash'lenmesi için PasswordHash alanına
            // düz metin atıyoruz (repository sorumlu).
            PasswordHash = request.Password
        };

        await _userRepository.AddUserAsync(user);

        return new AuthResponseDto
        {
            UserId   = user.Id.ToString(),
            Username = user.Username,
            Email    = user.Email
            // Token = null → Register sonrasında token dönmüyoruz
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Login
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(UserLoginDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username.Trim());

        // Kullanıcı bulunamazsa veya şifre yanlışsa aynı mesajı dön (güvenlik)
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı.");

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            UserId   = user.Id.ToString(),
            Username = user.Username,
            Email    = user.Email,
            Token    = token
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // JWT Üretimi
    // ──────────────────────────────────────────────────────────────────────────
    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key yapılandırılmamış.");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username",                    user.Username),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
