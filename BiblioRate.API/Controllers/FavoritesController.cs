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
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoritesController(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    // 1. Kitabı Favorilere Ekle
    [HttpPost]
    public async Task<IActionResult> AddToFavorites([FromBody] Favorite favorite)
    {
        // Validation hatasını önlemek için navigation property'leri temizliyoruz
        favorite.User = null;
        favorite.Book = null;

        // Zaten favoride mi kontrolü
        var exists = await _favoriteRepository.IsFavoriteAsync(favorite.UserId, favorite.BookId);
        if (exists)
            return BadRequest("Bu kitap zaten favorilerinizde.");

        try
        {
            await _favoriteRepository.AddToFavoritesAsync(favorite);
            return Ok(new { message = "Kitap favorilere eklendi!", favId = favorite.FavId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Favori eklenirken hata oluştu: {ex.Message}");
        }
    }

    // 2. Kullanıcının Tüm Favorilerini Getir
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFavorites(int userId)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
        return Ok(favorites);
    }

    // 3. Favorilerden Çıkar
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromFavorites(int userId, int bookId)
    {
        await _favoriteRepository.RemoveFromFavoritesAsync(userId, bookId);
        return Ok(new { message = "Kitap favorilerden çıkarıldı." });
    }
}