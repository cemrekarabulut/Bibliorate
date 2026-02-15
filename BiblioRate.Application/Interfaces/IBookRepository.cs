using BiblioRate.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetByIdAsync(int id); // Hata veren satırı buraya ekliyoruz
    Task AddBookAsync(Book book);
    Task AddSearchLogAsync(SearchLog log);
}