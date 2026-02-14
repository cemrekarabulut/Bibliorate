using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities
{
    public class Rating
    {
    [Key]
     public int RatingId { get; set; } // SQL: rating_id
    public int UserId { get; set; } // Foreign Key
    public int BookId { get; set; } // Foreign Key
    public int Score { get; set; } // SQL: score (1-10)
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public User User { get; set; }
    public Book Book { get; set; }   
    }
}