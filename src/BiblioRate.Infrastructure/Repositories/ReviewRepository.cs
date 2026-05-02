using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddReviewAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId)
    {
        return await _context.Reviews
            .Where(r => r.BookId == bookId)
            .Include(r => r.User)                    // Controller r.User?.Username'e erişiyor
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
    {
        return await _context.Reviews
            .Where(r => r.UserId == userId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateReviewAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review is not null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
