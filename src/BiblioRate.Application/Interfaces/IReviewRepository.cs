using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review);
    Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId);
    Task DeleteReviewAsync(int reviewId);
}
