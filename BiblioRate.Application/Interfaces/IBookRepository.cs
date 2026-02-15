using System.Collections.Generic;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces
{
    
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        
        // Metodun başına 'Task'tan önce bir şey yazmaya gerek yok, 
        // interface içinde oldukları için varsayılan olarak erişilebilirdirler.
        Task AddSearchLogAsync(SearchLog log); 
        Task AddBookAsync(Book book);
    }
}