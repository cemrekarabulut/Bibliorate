using BiblioRate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces
{
    public interface IReviewRepository
    {
    Task AddReviewAsync(Review review); // Yorum ekle
        Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId); // Kitabın tüm yorumlarını getir
        Task DeleteReviewAsync(int reviewId); // Yorum silme (Opsiyonel)    
    }
}