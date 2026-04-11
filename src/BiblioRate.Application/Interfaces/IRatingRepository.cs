using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.Application.Interfaces;

public interface IRatingRepository
{
    Task AddRatingAsync(Rating rating);
    Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId);
    Task<double> GetAverageScoreAsync(int bookId);
    Task<BooksStatsResponseDto> GetBooksStatsForAnalyticsAsync(CancellationToken cancellationToken = default);
}
