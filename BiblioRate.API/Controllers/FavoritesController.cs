using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models; // BookDto kullanımı için
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
        // Çağlar'ın gönderdiği verideki isimlendirme çakışmalarını önlemek için temizlik
        favorite.User = null;
        favorite.Book = null;

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

    // 2. Kullanıcının Tüm Favorilerini Getir (Çağlar'ın Frontend Uyumu İçin DTO'lu)
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFavorites(int userId)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
        
        // Çağlar favori listesini gösterirken 'BookDto' formatında veri bekliyor
        var favoriteBooks = favorites.Select(f => new BookDto
        {
            BookId = f.Book.BookId,
            Title = f.Book.Title,
            Authors = !string.IsNullOrEmpty(f.Book.Author) 
                      ? f.Book.Author.Split(',').Select(a => a.Trim()).ToList() 
                      : new List<string>(),
            ThumbnailUrl = f.Book.ThumbnailUrl ?? string.Empty,
            Description = f.Book.Description
        });

        return Ok(favoriteBooks);
    }

    // 3. Favorilerden Çıkar
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromFavorites([FromQuery] int userId, [FromQuery] int bookId)
    {
        await _favoriteRepository.RemoveFromFavoritesAsync(userId, bookId);
        return Ok(new { message = "Kitap favorilerden çıkarıldı." });
    }
}