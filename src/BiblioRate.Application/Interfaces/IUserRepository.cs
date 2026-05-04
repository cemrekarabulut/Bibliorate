using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.Application.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UserExistsAsync(string username, string email);

    /// <summary>JWT token'ındaki sub claim'den kullanıcıyı getirir.</summary>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>Profil alanlarını (Username, Bio, AvatarUrl) günceller ve kaydeder.</summary>
    Task UpdateProfileAsync(User user);
}
