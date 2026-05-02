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
            .Where(b => !b.IsDeleted)
            .Include(b => b.Ratings)
            .Include(b => b.Reviews)
            .OrderByDescending(b => b.QualityScore)
            .ThenByDescending(b => b.Ratings.Any() ? b.Ratings.Average(r => (double)r.Score) : 0)
            .ToListAsync();
    }

    /// <summary>FindAsync PK üzerinden çalıştığı için FirstOrDefault'tan daha performanslıdır.</summary>
    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Ratings)
                .ThenInclude(r => r.User)
            .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.BookId == id && !b.IsDeleted);
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
