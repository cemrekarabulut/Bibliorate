using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _context.Books.ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
    }

    public async Task AddSearchLogAsync(SearchLog log)
    {
        await _context.SearchLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    } 
    public async Task AddBookAsync(Book book)
{
    await _context.Books.AddAsync(book);
    await _context.SaveChangesAsync();
}   
    }
}