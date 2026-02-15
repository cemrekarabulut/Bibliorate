using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BCrypt.Net;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // 1. Kayıt Ol (Register)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (await _userRepository.UserExistsAsync(user.Username, user.Email))
            return BadRequest("Bu kullanıcı adı veya e-posta zaten kullanımda.");

        try
        {
            await _userRepository.AddUserAsync(user);
            return Ok(new { message = "Kayıt başarıyla tamamlandı!", userId = user.UserId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Kayıt sırasında hata oluştu: {ex.Message}");
        }
    }

    // 2. Giriş Yap (Login)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }

        return Ok(new 
        { 
            message = "Giriş başarılı!", 
            userId = user.UserId, 
            username = user.Username 
        });
    }
}

// Login isteği için küçük bir yardımcı sınıf (DTO)
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}