using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
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
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rating>> GetRatingsByUserIdAsync(int userId)
    {
        return await _context.Ratings
            .Where(r => r.UserId == userId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task UpdateRatingAsync(Rating rating)
    {
        _context.Ratings.Update(rating);
        await _context.SaveChangesAsync();
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

    public async Task<BooksStatsResponseDto> GetBooksStatsForAnalyticsAsync(
        CancellationToken cancellationToken = default)
    {
        var byBook = await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.BookId)
            .Select(b => new BookRatingStatsDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre,
                Year = b.Year,
                AverageRating = b.Ratings.Any()
                    ? b.Ratings.Average(r => (double)r.Score)
                    : 0.0,
                RatingCount = b.Ratings.Count
            })
            .ToListAsync(cancellationToken);

        var totalRatings = await _context.Ratings.CountAsync(cancellationToken);
        var overallAverage = await _context.Ratings.AverageAsync(
            r => (double?)r.Score,
            cancellationToken) ?? 0.0;

        return new BooksStatsResponseDto
        {
            TotalBooks = byBook.Count,
            TotalRatings = totalRatings,
            OverallAverageScore = overallAverage,
            ByBook = byBook
        };
    }
}
