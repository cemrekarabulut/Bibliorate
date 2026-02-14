using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Domain.Entities
{
    public class User
    {
     public int UserId { get; set; } // SQL: user_id
    public string Username { get; set; } // SQL: username
    public string Email { get; set; } // SQL: email
    public string PasswordHash { get; set; } // SQL: password_hash
    public DateTime CreatedAt { get; set; } = DateTime.Now; // SQL: created_at

     // İlişkiler
    public ICollection<Rating> Ratings { get; set; } 
    public ICollection<Review> Reviews { get; set; } 
    public ICollection<Favorite> Favorites { get; set; } 
    public ICollection<BookView> BookViews { get; set; } 
    public ICollection<SearchLog> SearchLogs { get; set; } 
    }
}