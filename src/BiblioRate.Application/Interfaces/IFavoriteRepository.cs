using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IFavoriteRepository
{
    Task AddToFavoritesAsync(Favorite favorite);
    Task RemoveFromFavoritesAsync(int userId, int bookId);
    Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int bookId);
}
