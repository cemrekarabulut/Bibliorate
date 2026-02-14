using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities
{
    public class Review
    {
    [Key]
     public int ReviewId { get; set; } // SQL: review_id 
    public int UserId { get; set; } // Foreign Key 
    public int BookId { get; set; } // Foreign Key 
    public string Comment { get; set; } // SQL: comment (TEXT) 
    public DateTime CreatedAt { get; set; } = DateTime.Now; // SQL: created_at

    // Navigation Properties (İlişkili Nesneler)
    public User User { get; set; }
    public Book Book { get; set; }   
    }
}