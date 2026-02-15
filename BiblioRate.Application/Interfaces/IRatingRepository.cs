using BiblioRate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces
{
    public interface IRatingRepository
    {
    // Bir kitaba puan eklemek için
        Task AddRatingAsync(Rating rating);
        
        // Bir kitabın tüm puanlarını getirmek için
        Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId);
        
        // IMDb tarzı ortalama puanı hesaplamak için
        Task<double> GetAverageScoreAsync(int bookId);    
    }
}