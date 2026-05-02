using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Entities;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Models;
using BiblioRate.API.Extensions;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository     _bookRepository;
    private readonly IGoogleBooksService _googleBooksService;
    private readonly IBookViewRepository _viewRepository;
    private readonly IRatingRepository   _ratingRepository;

    public BooksController(
        IBookRepository     bookRepository,
        IGoogleBooksService googleBooksService,
        IBookViewRepository viewRepository,
        IRatingRepository   ratingRepository)
    {
        _bookRepository     = bookRepository;
        _googleBooksService = googleBooksService;
        _viewRepository     = viewRepository;
        _ratingRepository   = ratingRepository;
    }

    /// <summary>Kitap ve puan özetleri (Flask / analitik entegrasyonu).</summary>
    // GET api/books/stats
    [HttpGet("stats")]
    public async Task<ActionResult<BooksStatsResponseDto>> GetBooksStats(CancellationToken cancellationToken)
        => Ok(await _ratingRepository.GetBooksStatsForAnalyticsAsync(cancellationToken));

    /// <summary>Veritabanındaki tüm kitapları listeler.</summary>
    // GET api/books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetLocalBooks()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        return Ok(books.Select(b =>
        {
            var avg     = b.Ratings.Any() ? Math.Round(b.Ratings.Average(r => (double)r.Score), 1) : 0.0;
            var rCount  = b.Ratings.Count;
            var rvCount = b.Reviews.Count;
            return b.ToDto(avg, rCount, rvCount);
        }));
    }

    /// <summary>Belirli bir kitabı getirir ve görüntülenme kaydı oluşturur.</summary>
    // GET api/books/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetBookById(int id, [FromQuery] int? userId = null)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null) return NotFound("Kitap bulunamadı.");

        await _viewRepository.AddViewAsync(new BookView
        {
            BookId   = id,
            UserId   = userId,
            ViewedAt = DateTime.UtcNow
        });

        var avg     = book.Ratings.Any() ? Math.Round(book.Ratings.Average(r => (double)r.Score), 1) : 0.0;
        var rCount  = book.Ratings.Count;
        var rvCount = book.Reviews.Count;

        // Reviews detaylı listeyi de ekle (/api/books/{id} sayfası için)
        var dto = book.ToDto(avg, rCount, rvCount);
        return Ok(dto);
    }

    /// <summary>Kitap arar: yerel DB + Google Books API.</summary>
    // GET api/books/search?q=...&userId=...
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Arama terimi boş olamaz.");

        // Google HTTP çağrısı paralel (DB'ye dokunmaz)
        var googleTask = _googleBooksService.SearchBooksAsync(q);

        // DB işlemleri sıralı (aynı DbContext)
        await _bookRepository.AddSearchLogAsync(new SearchLog
        {
            Query      = q,
            UserId     = userId,
            SearchedAt = DateTime.UtcNow
        });

        var localBooks  = await _bookRepository.GetAllBooksAsync();
        var googleBooks = await googleTask;

        return Ok(new
        {
            LocalResults  = localBooks
                .Where(b => b.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                .Select(b =>
                {
                    var avg     = b.Ratings.Any() ? Math.Round(b.Ratings.Average(r => (double)r.Score), 1) : 0.0;
                    var rCount  = b.Ratings.Count;
                    var rvCount = b.Reviews.Count;
                    return b.ToDto(avg, rCount, rvCount);
                }),
            GlobalResults = googleBooks.Select(b => b.ToDto())
        });
    }

    /// <summary>Yeni kitap ekler.</summary>
    // POST api/books
    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookRequest request)
    {
        var book = new Book
        {
            Title        = request.Title,
            Author       = request.Author,
            Genre        = request.Genre,
            Year         = request.Year,
            Description  = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            GoogleBookId = request.GoogleBookId,
            Isbn         = request.Isbn,
            PublishedAt  = request.PublishedAt ?? DateTime.UtcNow
        };

        await _bookRepository.AddBookAsync(book);
        return CreatedAtAction(
            nameof(GetBookById),
            new { id = book.BookId },
            new { message = "Kitap başarıyla kaydedildi!", bookId = book.BookId });
    }
}
