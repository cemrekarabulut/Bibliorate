using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Infrastructure.Repositories
{
public class RatingRepository : IRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public RatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Veritabanına yeni bir puan ekler
        public async Task AddRatingAsync(Rating rating)
        {
            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
        }

        // 2. Bir kitabın aldığı tüm puanları listeler
        public async Task<IEnumerable<Rating>> GetRatingsByBookIdAsync(int bookId)
        {
            return await _context.Ratings
                .Where(r => r.BookId == bookId)
                .ToListAsync();
        }

        // 3. IMDb tarzı: Kitabın ortalama puanını hesaplar
        public async Task<double> GetAverageScoreAsync(int bookId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            if (!ratings.Any()) return 0;

            return ratings.Average(r => r.Score);
        }
    }    
}