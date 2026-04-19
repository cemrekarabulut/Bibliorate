using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IGoogleBooksService
{
    Task<IEnumerable<Book>> SearchBooksAsync(string query, string authorName = "", string authorGenre = "General");
}
