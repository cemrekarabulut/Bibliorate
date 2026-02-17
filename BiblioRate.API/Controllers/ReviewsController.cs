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
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewsController(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    // 1. Yeni Yorum Ekle
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] Review review)
    {
        // Berra'nın duygu analizi (sentiment analysis) yapabilmesi için zaman damgası ekliyoruz
        review.CreatedAt = DateTime.Now;

        // Validation hatasını önlemek için navigation property'leri temizliyoruz
        review.User = null;
        review.Book = null;

        if (string.IsNullOrWhiteSpace(review.Comment))
            return BadRequest("Yorum içeriği boş olamaz.");

        try
        {
            await _reviewRepository.AddReviewAsync(review);
            return Ok(new { message = "Yorumunuz başarıyla eklendi!", reviewId = review.ReviewId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Yorum kaydedilirken bir hata oluştu: {ex.Message}");
        }
    }

    // 2. Bir Kitaba Ait Tüm Yorumları Getir
    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetReviewsByBook(int bookId)
    {
        var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);

        // Çağlar'ın frontend'de yorumu yapanın adını ve tarihini şık göstermesi için basit bir projeksiyon yapalım
        var reviewList = reviews.Select(r => new {
            r.ReviewId,
            r.Comment,
            r.CreatedAt,
            r.UserId,
            // Repository içinden User bilgisini de çekiyorsak:
            Username = r.User?.Username ?? "Anonim Kullanıcı"
        });

        return Ok(reviewList);
    }

    // 3. Yorum Sil
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await _reviewRepository.DeleteReviewAsync(id);
        return Ok(new { message = "Yorum başarıyla silindi." });
    }
}