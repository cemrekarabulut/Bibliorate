using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class Book
{
    public int BookId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Author      { get; set; } = "Bilinmeyen Yazar";
    public string Genre       { get; set; } = "Genel";
    public int    Year        { get; set; }
    public string Description { get; set; } = "Açıklama bulunmuyor.";
    public string ThumbnailUrl { get; set; } = string.Empty;

    public string? GoogleBookId { get; set; }

    public string   Isbn        { get; set; } = "0000000000";
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public int QualityScore { get; set; } // 0-100 arası veri kalite puanı

    // Soft-delete alanları — NightlyQualityGuard tarafından yönetilir
    public bool      IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Rating>   Ratings   { get; set; } = [];
    public ICollection<Review>   Reviews   { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<BookView> BookViews { get; set; } = [];
}
