using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext _context;

    public RatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddRatingAsync(Rating rating)
    {
        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId)
    {
        return await _context.Ratings
            .Where(r => r.BookId == bookId)
            .ToListAsync();
    }

    /// <summary>
    /// Ortalama puanı veritabanında hesaplar — tüm satırları belleğe çekmez.
    /// </summary>
    public async Task<double> GetAverageScoreAsync(int bookId)
    {
        return await _context.Ratings
            .Where(r => r.BookId == bookId)
            .AverageAsync(r => (double?)r.Score) ?? 0.0;
    }
}
