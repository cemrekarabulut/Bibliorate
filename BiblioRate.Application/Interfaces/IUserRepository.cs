using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces
{
    public interface IUserRepository
    {
    Task AddUserAsync(User user);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UserExistsAsync(string username, string email);    
    }
}