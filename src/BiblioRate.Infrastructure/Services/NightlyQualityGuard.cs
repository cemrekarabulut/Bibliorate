using BiblioRate.Application.Interfaces;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Services
{
// Infrastructure/Services/NightlyQualityGuard.cs
public class NightlyQualityGuard : INightlyQualityGuard
{
    private readonly ApplicationDbContext _context;
    private const int QualityThreshold = 55;

    public NightlyQualityGuard(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine("[NightlyGuard] Tarama başladı...");

        var lowQualityBooks = await _context.Books
            .Where(b => !b.IsDeleted && b.QualityScore < QualityThreshold)
            .ToListAsync(ct);

        foreach (var book in lowQualityBooks)
        {
            book.IsDeleted = true;
            book.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        Console.WriteLine($"[NightlyGuard] {lowQualityBooks.Count} düşük kaliteli kitap soft-delete edildi.");
    }
}   
}