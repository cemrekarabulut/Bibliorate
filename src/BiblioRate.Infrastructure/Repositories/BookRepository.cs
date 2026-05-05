using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Ratings)  // Rating verileri frontend'e düşsün diye eager load
            .Include(b => b.Reviews)  // Anasayfada review count doğru gelsin diye eklendi
            .Where(b => !b.IsDeleted) // Yedek güvenlik - Context Query filter kullanılıyor olsa da DB sorgusunda garanti eder
            .OrderByDescending(b => b.QualityScore)
            .ThenByDescending(b => b.Ratings.Any() ? b.Ratings.Average(r => (double)r.Score) : 0)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        // FindAsync, Include'u desteklemez — Ratings ve Reviews için FirstOrDefaultAsync kullanılır
        return await _context.Books
            .Include(b => b.Ratings)
            .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.BookId == id);
    }

    public async Task AddBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
    }

    public async Task AddSearchLogAsync(SearchLog log)
    {
        await _context.SearchLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
