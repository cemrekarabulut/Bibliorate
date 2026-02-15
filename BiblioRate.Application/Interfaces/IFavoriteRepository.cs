using BiblioRate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces
{
    public interface IFavoriteRepository
    {
    Task AddToFavoritesAsync(Favorite favorite); // Favorilere ekle
        Task RemoveFromFavoritesAsync(int userId, int bookId); // Favorilerden çıkar
        Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId); // Kullanıcının favori listesi
        Task<bool> IsFavoriteAsync(int userId, int bookId); // Zaten favoride mi kontrolü    
    }
}