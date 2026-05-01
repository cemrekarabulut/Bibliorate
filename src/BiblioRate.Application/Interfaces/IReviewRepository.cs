using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
    Task UpdateReviewAsync(Review review);
    Task DeleteReviewAsync(int reviewId);
}
