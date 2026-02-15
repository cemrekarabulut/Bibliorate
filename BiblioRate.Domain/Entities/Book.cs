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
        public string Genre { get; set; } = "Genel"; // SQL: genre
        public int Year { get; set; } // SQL: year
        public string Description { get; set; } // SQL: description
        
        // Google'dan gelen veriler için ekledik:
        public string Isbn { get; set; } = "0000000000"; 
        public DateTime PublishedAt { get; set; } = DateTime.Now;

        // İlişkiler (Navigation Properties)
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<BookView> BookViews { get; set; } = new List<BookView>();
    }
}