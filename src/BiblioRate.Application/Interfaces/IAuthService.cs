using BiblioRate.Application.DTOs;
using BiblioRate.Domain.Models;

namespace BiblioRate.Application.Interfaces;

/// <summary>
/// Kimlik doğrulama işlemlerini tanımlayan servis arayüzü.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur. Şifre BCrypt ile hash'lenerek saklanır.
    /// </summary>
    /// <returns>Kayıt olan kullanıcı bilgileri (token içermez).</returns>
    Task<AuthResponseDto> RegisterAsync(UserRegisterDto request);

    /// <summary>
    /// Kullanıcı adı ve şifreyi doğrular; başarılıysa JWT token döner.
    /// </summary>
    /// <returns>Kullanıcı bilgileri + geçerli JWT token.</returns>
    Task<AuthResponseDto> LoginAsync(UserLoginDto request);
}
