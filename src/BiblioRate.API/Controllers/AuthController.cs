using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration  _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration  = configuration;
    }

    /// <summary>Yeni kullanıcı kaydı.</summary>
    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userRepository.UserExistsAsync(request.Username, request.Email))
            return Conflict("Bu kullanıcı adı veya e-posta zaten kullanımda.");

        // Entity'yi DTO'dan oluştur — navigation property'ler temiz
        var user = new User
        {
            Username     = request.Username,
            Email        = request.Email,
            PasswordHash = request.Password   // UserRepository hash'leyecek
        };

        await _userRepository.AddUserAsync(user);

        return Ok(new AuthResponseDto
        {
            UserId   = user.UserId.ToString(),
            Username = user.Username,
            Email    = user.Email
        });
    }

    /// <summary>Kullanıcı girişi. Başarılı olursa JWT token döner.</summary>
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        return Ok(new AuthResponseDto
        {
            UserId   = user.UserId.ToString(),
            Username = user.Username,
            Email    = user.Email,
            Token    = GenerateJwtToken(user)
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey      = _configuration["Jwt:Key"]
                          ?? throw new InvalidOperationException("Jwt:Key yapılandırılmamış.");
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
