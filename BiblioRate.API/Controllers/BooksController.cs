using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Entities; 
using BiblioRate.Application.Interfaces; 
using BiblioRate.Domain.Models; 

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IGoogleBooksService _googleBooksService;
    private readonly IBookViewRepository _viewRepository;

    public BooksController(IBookRepository bookRepository, 
                           IGoogleBooksService googleBooksService,
                           IBookViewRepository viewRepository)
    {
        _bookRepository = bookRepository;
        _googleBooksService = googleBooksService;
        _viewRepository = viewRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocalBooks()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        var bookDtos = books.Select(b => MapToDto(b));
        return Ok(bookDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookById(int id, int? userId = null)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return NotFound("Kitap bulunamadı.");

        await _viewRepository.AddViewAsync(new BookView 
        { 
            BookId = id, 
            UserId = userId,
            ViewedAt = DateTime.Now 
        });

        return Ok(MapToDto(book));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string q, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Arama terimi boş olamaz.");

        var log = new SearchLog
        {
            Query = q,
            UserId = userId,
            SearchedAt = DateTime.Now
        };
        await _bookRepository.AddSearchLogAsync(log);

        var localBooks = await _bookRepository.GetAllBooksAsync();
        var filteredLocal = localBooks
            .Where(b => b.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(b => MapToDto(b));

        var googleBooks = await _googleBooksService.SearchBooksAsync(q);

        var result = new
        {
            LocalResults = filteredLocal,
            GlobalResults = googleBooks
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        if (book == null) return BadRequest("Kitap verisi geçersiz.");

        try 
        {
            book.BookId = 0; 
            await _bookRepository.AddBookAsync(book);
            return Ok(new { message = "Kitap başarıyla kaydedildi!", bookId = book.BookId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Kayıt sırasında hata oluştu: {ex.Message}");
        }
    }

    // ARTIK TAMAMEN UYUMLU YARDIMCI METOT
    private static BookDto MapToDto(Book b)
    {
        return new BookDto
        {
            BookId = b.BookId,
            Title = b.Title,
            // 'Author' (tekil string) -> 'Authors' (List<string>) dönüşümü
            Authors = !string.IsNullOrEmpty(b.Author) 
                      ? b.Author.Split(',').Select(a => a.Trim()).ToList() 
                      : new List<string>(),
            Description = b.Description,
            // ARTIK BURASI BOŞ DEĞİL: Entity'deki ThumbnailUrl kullanılıyor
            ThumbnailUrl = b.ThumbnailUrl ?? string.Empty, 
            // Şimdilik varsayılan değerler; ileride Rating tablosundan hesaplanabilir
            RatingAvg = 0.0,
            RatingCount = 0
        };
    }
}