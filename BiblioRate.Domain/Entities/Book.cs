using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Domain.Entities
{
    public class Book
    {
     public int BookId { get; set; } // SQL: book_id
    public string Title { get; set; } // SQL: title
    public string Author { get; set; } // SQL: author
    public string Genre { get; set; } // SQL: genre
    public int Year { get; set; } // SQL: year
    public string Description { get; set; } // SQL: description

    // İlişkiler (Navigation Properties)
     // Book.cs içindeki mevcut özelliklerin altına ekle:
    public ICollection<Rating> Ratings { get; set; } 
    public ICollection<Review> Reviews { get; set; } 
    public ICollection<Favorite> Favorites { get; set; } 
    public ICollection<BookView> BookViews { get; set; } 
    }
}