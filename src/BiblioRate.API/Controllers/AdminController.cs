using BiblioRate.Application.DTOs;
using BiblioRate.Application.Interfaces;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    private readonly ApplicationDbContext _context;
    private readonly IBookQualityEvaluator _qualityEvaluator;
    private readonly IServiceScopeFactory _scopeFactory;

    public AdminController(
        DataSeederService dataSeeder,
        ApplicationDbContext context,
        IBookQualityEvaluator qualityEvaluator,
        IServiceScopeFactory scopeFactory)
    {
        _dataSeeder = dataSeeder;
        _context = context;
        _qualityEvaluator = qualityEvaluator;
        _scopeFactory = scopeFactory;
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
    /// <summary>
    /// Google Books API'den kitapları çekip veritabanına ekler.
    /// İşlem arka planda çalışır, hemen 202 Accepted döner.
    /// Render loglarından ilerlemeyi takip edebilirsiniz.
    /// </summary>
    [HttpPost("seed")]
    public IActionResult SeedBooksAsync()
    {
        Console.WriteLine("[AdminController] /api/admin/seed tetiklendi — arka planda başlatılıyor.");

        // Seeder dakikalarca sürer; HTTP timeout almamak için fire-and-forget
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
            try
            {
                await seeder.SeedAsync();
                Console.WriteLine("[AdminController] Seeding tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] Seed arka plan hatası: {ex.Message}");
            }
        });

        return Accepted(new
        {
            message = "Seeding arka planda başlatıldı. Render loglarından ilerlemeyi takip edin.",
            hint = "[Save] satırlarını arayın.",
            timestamp = DateTime.UtcNow
        });
    }

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

    /// <summary>
    /// Veritabanındaki kitapların kalitesini analiz edip raporlar.
    /// Aynı zamanda kalite skorlarını veritabanına kalıcı olarak kaydeder.
    /// </summary>
    [HttpGet("quality-report")]
    public async Task<IActionResult> GetQualityReport()
    {
        // Global query filter aktif olduğundan soft-delete yemiş kitaplar hariç tutulacaktır.
        var books = await _context.Books.ToListAsync();

        if (books.Count == 0)
        {
            return Ok(new QualityReportDto
            {
                TotalBooks = 0,
                PerfectBooks = 0,
                AverageQuality = 0,
                LowQualityBooks = []
            });
        }

        // Skorları yeniden hesapla ve DB'ye kaydet
        foreach (var book in books)
        {
            book.QualityScore = _qualityEvaluator.Evaluate(book);
        }

        await _context.SaveChangesAsync();

        var report = new QualityReportDto
        {
            TotalBooks = books.Count,
            PerfectBooks = books.Count(b => b.QualityScore == 100),
            AverageQuality = books.Average(b => b.QualityScore),
            LowQualityBooks = books
                .Where(b => b.QualityScore < 40)
                .Select(b => new LowQualityBookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    QualityScore = b.QualityScore
                })
                .ToList()
        };

        return Ok(report);
    }

    /// <summary>
    /// Veritabanındaki gürültülü (Noise) kayıtları, örneğin özetleri (SparkNotes vb.) kalıcı olarak siler.
    /// </summary>
    [HttpDelete("noise")]
    public async Task<IActionResult> HardDeleteNoise()
    {
        string[] noisyKeywords = ["sparknotes", "notes", "sampler", "abridged", "summary"];

        var noiseBooks = await _context.Books
            .Where(b => noisyKeywords.Any(k => 
                b.Title.ToLower().Contains(k) || (b.Author != null && b.Author.ToLower().Contains(k))))
            .ToListAsync();

        if (noiseBooks.Count == 0)
        {
            return Ok(new { message = "Gürültülü kayıt bulunamadı." });
        }

        _context.Books.RemoveRange(noiseBooks);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{noiseBooks.Count} gürültülü kayıt (SparkNotes, Sampler, vb.) kalıcı olarak silindi." });
    }
}
