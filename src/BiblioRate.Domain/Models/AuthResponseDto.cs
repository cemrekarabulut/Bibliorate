namespace BiblioRate.Domain.Models;

/// <summary>
/// Register ve Login sonucunda dönen kullanıcı bilgisi + JWT token.
/// </summary>
public class AuthResponseDto
{
    public string UserId   { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;

    /// <summary>Login sonrasında dolar; Register'da boş kalır.</summary>
    public string? Token { get; set; }
}
