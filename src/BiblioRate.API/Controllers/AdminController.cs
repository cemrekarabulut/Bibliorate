using BiblioRate.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiblioRate.API.Controllers;

/// <summary>
/// Yönetici endpoint'leri — hassas DB operasyonları için kullanılır.
/// Production'da bu controller'a ağ seviyesinde erişim kısıtlanmalıdır.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly DataSeederService _dataSeeder;

    public AdminController(DataSeederService dataSeeder)
    {
        _dataSeeder = dataSeeder;
    }

    /// <summary>
    /// Veritabanını tek seferlik süpürür:
    /// <list type="bullet">
    ///   <item>Fuzzy deduplication — "1984" ve "1984 Large Print" gibi varyantları temizler.</item>
    ///   <item>Survival of the Fittest — görseli olan ve açıklaması en uzun kaydı korur.</item>
    ///   <item>Genre normalizasyonu — yazar koruma tablosu + merge kuralları uygulanır.</item>
    /// </list>
    /// Loglara kaç kitap silindiği yazılır.
    /// </summary>
    /// <remarks>
    /// Bu endpoint yalnızca bir kez çalıştırılmalıdır. İkinci çalışmada
    /// silinecek/değiştirilecek kayıt kalmadığından (idempotent) zarar vermez,
    /// ancak gereksiz DB sorgusu yapar.
    /// </remarks>
    [HttpPost("cleanup")]
    public async Task<IActionResult> RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("[AdminController] /api/admin/cleanup tetiklendi.");
            await _dataSeeder.UpdateExistingCategoriesAsync(cancellationToken);

            return Ok(new
            {
                message = "Veritabanı temizleme ve kategori normalizasyonu başarıyla tamamlandı.",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdminController] Cleanup hatası: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
