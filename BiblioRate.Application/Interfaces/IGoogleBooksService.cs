using System.Collections.Generic;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces
{
    // 'class' yerine 'interface' yazıyoruz
    public interface IGoogleBooksService
    {
        // Google'dan arama terimine göre kitapları getirir
        Task<IEnumerable<Book>> SearchBooksAsync(string query);
    }
}