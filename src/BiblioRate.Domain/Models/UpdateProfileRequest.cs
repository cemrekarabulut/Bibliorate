using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>
/// PUT /api/auth/profile için istek modeli.
/// Tüm alanlar opsiyoneldir; null gelen alan güncellenmez.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>Yeni kullanıcı adı. Boş bırakılırsa mevcut değer korunur.</summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
    public string? Username { get; set; }

    /// <summary>Kullanıcı biyografisi. Boş bırakılırsa mevcut değer korunur.</summary>
    [StringLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
    public string? Bio { get; set; }

    /// <summary>Profil resmi URL'si. Boş bırakılırsa mevcut değer korunur.</summary>
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? AvatarUrl { get; set; }
}
