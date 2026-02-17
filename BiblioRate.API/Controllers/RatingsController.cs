using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> AddRating([FromBody] Rating rating)
    {
        // Berra'nın anomali tespiti için zaman damgası ekliyoruz
        rating.CreatedAt = DateTime.Now; 
        
        // Frontend'den gelmesi gerekmeyen objeleri null yaparak güvene alıyoruz
        rating.User = null;
        rating.Book = null;

        if (rating.Score < 1 || rating.Score > 10)
            return BadRequest("Puan 1 ile 10 arasında olmalıdır.");

        try
        {
            await _ratingRepository.AddRatingAsync(rating);
            
            // Çağlar puan verdikten sonra güncel ortalamayı da aynı anda dönebiliriz
            var newAverage = await _ratingRepository.GetAverageScoreAsync(rating.BookId);
            
            return Ok(new { 
                message = "Puanınız başarıyla kaydedildi!", 
                ratingId = rating.RatingId,
                currentAverage = Math.Round(newAverage, 1) // Çağlar'ın UI'ı güncellemesi için kolaylık
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Veritabanına kaydedilirken hata oluştu: {ex.Message}");
        }
    }

    [HttpGet("average/{bookId}")]
    public async Task<IActionResult> GetAverageScore(int bookId)
    {
        var average = await _ratingRepository.GetAverageScoreAsync(bookId);
        var formattedAverage = Math.Round(average, 1);
        return Ok(new { BookId = bookId, AverageScore = formattedAverage });
    }
}