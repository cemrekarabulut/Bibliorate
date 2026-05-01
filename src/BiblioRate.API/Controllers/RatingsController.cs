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

    public RatingsController(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    /// <summary>
    /// Giriş yapmış kullanıcı adına bir kitaba puan verir.
    /// UserId, request body'den değil JWT token'dan (sub claim) okunur.
    /// Aynı kullanıcı aynı kitabı tekrar puanlayamaz (DB UNIQUE kısıtı).
    /// </summary>
    // POST api/ratings
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddRating([FromBody] CreateRatingRequest request)
    {
        // JWT token'ından kullanıcı kimliğini güvenli şekilde çek
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz token: kullanıcı kimliği bulunamadı." });

        // Aynı kullanıcı bu kitabı daha önce puanladı mı?
        var existing = await _ratingRepository.GetRatingsByBookIdAsync(request.BookId);
        if (existing.Any(r => r.UserId == userId))
            return Conflict(new { message = "Bu kitabı zaten puanladınız." });

        var rating = new Rating
        {
            UserId    = userId,
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

    /// <summary>Bir kitabın ortalama puanını döner. Kimlik doğrulama gerektirmez.</summary>
    // GET api/ratings/average/{bookId}
    [HttpGet("average/{bookId:int}")]
    public async Task<IActionResult> GetAverageScore(int bookId)
    {
        var average = await _ratingRepository.GetAverageScoreAsync(bookId);
        return Ok(new { bookId, averageScore = Math.Round(average, 1) });
    }
}
