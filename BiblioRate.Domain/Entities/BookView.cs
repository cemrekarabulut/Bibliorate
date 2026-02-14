using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities
{
    public class BookView
    {
    [Key]
     public int ViewId { get; set; } // SQL: view_id 
    public int? UserId { get; set; } // SQL: user_id (Nullable) 
    public int BookId { get; set; } // SQL: book_id 
    public DateTime ViewedAt { get; set; } = DateTime.Now; // SQL: viewed_at

    // Navigation Properties
    public User? User { get; set; }
    public Book Book { get; set; }   
    }
}