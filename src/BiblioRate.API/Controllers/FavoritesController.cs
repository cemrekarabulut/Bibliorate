using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using BiblioRate.API.Extensions;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoritesController(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    /// <summary>Kitabı favorilere ekler.</summary>
    // POST api/favorites
    [HttpPost]
    public async Task<IActionResult> AddToFavorites([FromBody] CreateFavoriteRequest request)
    {
        if (await _favoriteRepository.IsFavoriteAsync(request.UserId, request.BookId))
            return Conflict("Bu kitap zaten favorilerinizde.");

        var favorite = new Favorite
        {
            UserId    = request.UserId,
            BookId    = request.BookId,
            CreatedAt = DateTime.UtcNow
        };

        await _favoriteRepository.AddToFavoritesAsync(favorite);
        return CreatedAtAction(
            nameof(GetUserFavorites),
            new { userId = favorite.UserId },
            new { message = "Kitap favorilere eklendi!", favId = favorite.FavId });
    }

    /// <summary>Kullanıcının favori kitaplarını listeler.</summary>
    // GET api/favorites/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetUserFavorites(int userId)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
        return Ok(favorites
            .Where(f => f.Book is not null)
            .Select(f =>
            {
                var book    = f.Book!;
                var ratings = book.Ratings ?? [];
                var avg     = ratings.Any() ? ratings.Average(r => (double)r.Score) : 0.0;
                var count   = ratings.Count;
                var reviewCount = book.Reviews?.Count ?? 0;
                return book.ToDto(Math.Round(avg, 1), count);
            }));
    }

    /// <summary>Kitabı favorilerden çıkarır.</summary>
    // DELETE api/favorites/remove?userId=1&bookId=2
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromFavorites([FromQuery] int userId, [FromQuery] int bookId)
    {
        await _favoriteRepository.RemoveFromFavoritesAsync(userId, bookId);
        return Ok(new { message = "Kitap favorilerden çıkarıldı." });
    }
}
