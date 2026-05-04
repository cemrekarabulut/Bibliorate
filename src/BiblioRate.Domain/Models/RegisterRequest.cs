using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>POST /api/auth/register için istek modeli.</summary>
public class RegisterRequest
{
    [Required, MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}
