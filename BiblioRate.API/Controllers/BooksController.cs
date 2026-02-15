using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Entities; 
using BiblioRate.Application.Interfaces; 

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IGoogleBooksService _googleBooksService;

    public BooksController(IBookRepository bookRepository, IGoogleBooksService googleBooksService)
    {
        _bookRepository = bookRepository;
        _googleBooksService = googleBooksService;
    }

    // 1. Tüm yerel kitapları listeleme (MySQL)
    [HttpGet]
    public async Task<IActionResult> GetLocalBooks()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        return Ok(books);
    }

    // 2. Hibrit Arama: Hem MySQL hem Google Books
    [HttpGet("search")]
    public async Task<IActionResult> Search(string q, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Arama terimi boş olamaz.");

        // Aramayı veritabanına logluyoruz (Anomali tespiti için temel veri)
        var log = new SearchLog
        {
            Query = q,
            UserId = userId,
            SearchedAt = DateTime.Now
        };
        await _bookRepository.AddSearchLogAsync(log);

        var localBooks = await _bookRepository.GetAllBooksAsync();
        var filteredLocal = localBooks.Where(b => b.Title.Contains(q, StringComparison.OrdinalIgnoreCase));

        var googleBooks = await _googleBooksService.SearchBooksAsync(q);

        var result = new
        {
            LocalResults = filteredLocal,
            GlobalResults = googleBooks
        };

        return Ok(result);
    }

    // 3. Kitap Kaydetme: Google'dan gelen veriyi yerel DB'ye alır
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        if (book == null) return BadRequest("Kitap verisi geçersiz.");

        try 
        {
            // BookId'yi 0 set ediyoruz ki veritabanı otomatik (Auto Increment) atasın
            book.BookId = 0; 
            
            await _bookRepository.AddBookAsync(book);
            return Ok(new { message = "Kitap başarıyla MySQL veritabanına kaydedildi!", bookId = book.BookId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Kayıt sırasında hata oluştu: {ex.Message}");
        }
    }
}