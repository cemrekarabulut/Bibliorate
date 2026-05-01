using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.Application.Interfaces;

public interface IRatingRepository
{
    Task AddRatingAsync(Rating rating);
    Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId);
    Task<IEnumerable<Rating>> GetRatingsByUserIdAsync(int userId);
    Task UpdateRatingAsync(Rating rating);
    Task<double> GetAverageScoreAsync(int bookId);
    Task<BooksStatsResponseDto> GetBooksStatsForAnalyticsAsync(CancellationToken cancellationToken = default);
}
