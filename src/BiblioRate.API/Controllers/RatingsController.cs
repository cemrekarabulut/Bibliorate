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

    public RatingsController(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    /// <summary>Bir kitaba puan verir. Aynı kullanıcı aynı kitabı tekrar puanlayamaz.</summary>
    // POST api/ratings
    [HttpPost]
    public async Task<IActionResult> AddRating([FromBody] CreateRatingRequest request)
    {
        var existing = await _ratingRepository.GetRatingsByBookIdAsync(request.BookId);
        if (existing.Any(r => r.UserId == request.UserId))
            return Conflict("Bu kitabı zaten puanladınız.");

        var rating = new Rating
        {
            UserId    = request.UserId,
            BookId    = request.BookId,
            Score     = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddRatingAsync(rating);

        var newAverage = await _ratingRepository.GetAverageScoreAsync(request.BookId);

        return Ok(new
        {
            message        = "Puanınız başarıyla kaydedildi!",
            ratingId       = rating.RatingId,
            currentAverage = Math.Round(newAverage, 1)
        });
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
