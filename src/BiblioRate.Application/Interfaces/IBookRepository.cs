using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetByIdAsync(int id);
    Task AddBookAsync(Book book);
    Task AddSearchLogAsync(SearchLog log);
}
