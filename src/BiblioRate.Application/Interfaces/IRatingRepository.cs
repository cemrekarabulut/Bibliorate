using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IRatingRepository
{
    Task AddRatingAsync(Rating rating);
    Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId);
    Task<double> GetAverageScoreAsync(int bookId);
}
