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

    // 1. Bir kitaba puan ver (IMDb tarzı oy kullanma)
    [HttpPost]
    public async Task<IActionResult> AddRating([FromBody] Rating rating)
    {
        // ÖNEMLİ: Navigation property'leri null yapıyoruz. 
        // Böylece API, bizden tam bir User veya Book objesi beklemiyor.
        rating.User = null;
        rating.Book = null;

        if (rating.Score < 1 || rating.Score > 10)
            return BadRequest("Puan 1 ile 10 arasında olmalıdır.");

        try
        {
            await _ratingRepository.AddRatingAsync(rating);
            return Ok(new { message = "Puanınız başarıyla kaydedildi!", ratingId = rating.RatingId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Veritabanına kaydedilirken hata oluştu: {ex.Message}");
        }
    }

    // 2. Bir kitabın ortalama puanını getir (BiblioRate Puanı)
    [HttpGet("average/{bookId}")]
    public async Task<IActionResult> GetAverageScore(int bookId)
    {
        var average = await _ratingRepository.GetAverageScoreAsync(bookId);
        
        // Virgülden sonra iki basamak göstererek daha profesyonel (IMDb gibi) bir görünüm sağlayalım
        var formattedAverage = Math.Round(average, 1);
        
        return Ok(new { BookId = bookId, AverageScore = formattedAverage });
    }
}