using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IReviewRepository _reviewRepository;

    public RatingsController(IRatingRepository ratingRepository, IReviewRepository reviewRepository)
    {
        _ratingRepository = ratingRepository;
        _reviewRepository = reviewRepository;
    }

    /// <summary>
    /// Giriş yapmış kullanıcı adına bir kitaba puan ve yorum (opsiyonel) verir.
    /// </summary>
    // POST api/ratings
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddRating([FromBody] CreateRatingRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz token: kullanıcı kimliği bulunamadı." });

        var existingRatings = await _ratingRepository.GetRatingsByBookIdAsync(request.BookId);
        if (existingRatings.Any(r => r.UserId == userId))
            return Conflict(new { message = "Bu kitabı zaten puanladınız." });

        var rating = new Rating
        {
            UserId    = userId,
            BookId    = request.BookId,
            Score     = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddRatingAsync(rating);

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            var review = new Review
            {
                UserId = userId,
                BookId = request.BookId,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };
            await _reviewRepository.AddReviewAsync(review);
        }

        var newAverage = await _ratingRepository.GetAverageScoreAsync(request.BookId);

        return Ok(new
        {
            message        = "Puanınız ve yorumunuz başarıyla kaydedildi!",
            ratingId       = rating.RatingId,
            currentAverage = Math.Round(newAverage, 1)
        });
    }

    /// <summary>
    /// Giriş yapmış kullanıcı adına bir kitaba verilmiş puanı ve yorumu günceller.
    /// </summary>
    // PUT api/ratings
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateRating([FromBody] CreateRatingRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz token: kullanıcı kimliği bulunamadı." });

        var existingRatings = await _ratingRepository.GetRatingsByUserIdAsync(userId);
        var rating = existingRatings.FirstOrDefault(r => r.BookId == request.BookId);
        
        if (rating == null)
            return NotFound(new { message = "Güncellenecek puan bulunamadı." });

        rating.Score = request.Score;
        await _ratingRepository.UpdateRatingAsync(rating);

        var existingReviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
        var review = existingReviews.FirstOrDefault(r => r.BookId == request.BookId);

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            if (review != null)
            {
                review.Comment = request.Comment;
                await _reviewRepository.UpdateReviewAsync(review);
            }
            else
            {
                var newReview = new Review
                {
                    UserId = userId,
                    BookId = request.BookId,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow
                };
                await _reviewRepository.AddReviewAsync(newReview);
            }
        }
        else if (review != null)
        {
            await _reviewRepository.DeleteReviewAsync(review.ReviewId);
        }

        var newAverage = await _ratingRepository.GetAverageScoreAsync(request.BookId);

        return Ok(new
        {
            message        = "Puanınız ve yorumunuz başarıyla güncellendi!",
            ratingId       = rating.RatingId,
            currentAverage = Math.Round(newAverage, 1)
        });
    }

    /// <summary>Bir kitaba ait tüm puan ve yorumları birleştirip listeler.</summary>
    // GET api/ratings/book/{bookId}
    [HttpGet("book/{bookId:int}")]
    public async Task<IActionResult> GetBookRatings(int bookId)
    {
        var ratings = await _ratingRepository.GetRatingsByBookIdAsync(bookId);
        var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);

        var result = ratings.Select(r => new
        {
            UserId = r.UserId,
            BookId = r.BookId,
            Score = r.Score,
            Comment = reviews.FirstOrDefault(rev => rev.UserId == r.UserId)?.Comment,
            CreatedAt = r.CreatedAt,
            Username = r.User?.Username ?? "Anonim Kullanıcı"
        });

        return Ok(result);
    }

    /// <summary>Bir kullanıcıya ait tüm puan ve yorumları birleştirip listeler.</summary>
    // GET api/ratings/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserRatings(int userId)
    {
        var ratings = await _ratingRepository.GetRatingsByUserIdAsync(userId);
        var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);

        var result = ratings.Select(r => new
        {
            UserId = r.UserId,
            BookId = r.BookId,
            Score = r.Score,
            Comment = reviews.FirstOrDefault(rev => rev.BookId == r.BookId)?.Comment,
            CreatedAt = r.CreatedAt,
            Username = r.User?.Username ?? "Anonim Kullanıcı"
        });

        return Ok(result);
    }

    /// <summary>Bir kitabın ortalama puanını döner.</summary>
    // GET api/ratings/average/{bookId}
    [HttpGet("average/{bookId:int}")]
    public async Task<IActionResult> GetAverageScore(int bookId)
    {
        var average = await _ratingRepository.GetAverageScoreAsync(bookId);
        return Ok(new { bookId, averageScore = Math.Round(average, 1) });
    }
}
