using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models; // Yeni eklediğimiz DTO'yu kullanmak için
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

            // Çağlar'ın frontend'i için uyumlu DTO'yu hazırlıyoruz
            var response = new AuthResponseDto
            {
                UserId = user.UserId.ToString(),
                Username = user.Username,
                Email = user.Email
            };

            return Ok(new { message = "Kayıt başarıyla tamamlandı!", user = response });
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

        // Giriş başarılı olduğunda Çağlar'ın beklediği "user_id" formatını dönüyoruz
        var response = new AuthResponseDto
        {
            UserId = user.UserId.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        return Ok(response);
    }
}

// Login isteği için yardımcı sınıf (Giriş yaparken şifre açık metin gelir)
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}