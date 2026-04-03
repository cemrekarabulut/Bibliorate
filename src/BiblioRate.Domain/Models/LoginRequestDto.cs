namespace BiblioRate.Domain.Models;

/// <summary>
/// Login isteğinde kullanıcı adı ve şifreyi taşıyan DTO.
/// </summary>
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
