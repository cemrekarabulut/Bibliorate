using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewsController(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    /// <summary>Bir kitaba yorum ekler.</summary>
    // POST api/reviews
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewRequest request)
    {
        var review = new Review
        {
            UserId    = request.UserId,
            BookId    = request.BookId,
            Comment   = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddReviewAsync(review);

        return CreatedAtAction(
            nameof(GetReviewsByBook),
            new { bookId = review.BookId },
            new { message = "Yorumunuz başarıyla eklendi!", reviewId = review.ReviewId });
    }

    /// <summary>Bir kitaba ait tüm yorumları listeler.</summary>
    // GET api/reviews/book/{bookId}
    [HttpGet("book/{bookId:int}")]
    public async Task<IActionResult> GetReviewsByBook(int bookId)
    {
        var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);

        return Ok(reviews.Select(r => new
        {
            r.ReviewId,
            r.Comment,
            r.CreatedAt,
            r.UserId,
            Username = r.User?.Username ?? "Anonim Kullanıcı"
        }));
    }

    /// <summary>Yorumu siler.</summary>
    // DELETE api/reviews/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await _reviewRepository.DeleteReviewAsync(id);
        return Ok(new { message = "Yorum başarıyla silindi." });
    }
}
