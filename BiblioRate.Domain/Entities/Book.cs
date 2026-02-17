using System;
using System.Collections.Generic;

namespace BiblioRate.Domain.Entities
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = "Bilinmeyen Yazar";
        public string Genre { get; set; } = "Genel";
        public int Year { get; set; }
        
        // Berra'nın analizleri için varsayılan değerler
        public string Description { get; set; } = "Açıklama bulunmuyor.";
        public string ThumbnailUrl { get; set; } = string.Empty;

        // Çağlar'ın Import işlemi için benzersiz Google ID'si
        public string? GoogleBookId { get; set; } 

        public string Isbn { get; set; } = "0000000000"; 
        public DateTime PublishedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<BookView> BookViews { get; set; } = new List<BookView>();
    }
}