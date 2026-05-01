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
            .Where(b => !b.IsDeleted) // Yedek güvenlik - Context Query filter kullanılıyor olsa da DB sorgusunda garanti eder
            .OrderByDescending(b => b.QualityScore)
            .ThenByDescending(b => b.Ratings.Any() ? b.Ratings.Average(r => (double)r.Score) : 0)
            .ToListAsync();
    }

    /// <summary>FindAsync PK üzerinden çalıştığı için FirstOrDefault'tan daha performanslıdır.</summary>
    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
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
