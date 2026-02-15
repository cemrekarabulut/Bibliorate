using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities
{
    public class Favorite
    {
    [Key]    
    public int FavId { get; set; } // SQL: fav_id 
    public int UserId { get; set; } // Foreign Key 
    public int BookId { get; set; } // Foreign Key 
    public DateTime CreatedAt { get; set; } = DateTime.Now; // SQL: created_at

    // Navigation Properties
    public User? User { get; set; }
    public Book? Book { get; set; }    
    }
}