using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>Flask / harici analiz tüketicileri için kitap + puan özet şeması.</summary>
public class BooksStatsResponseDto
{
    [JsonPropertyName("totalBooks")]
    public int TotalBooks { get; set; }

    [JsonPropertyName("totalRatings")]
    public int TotalRatings { get; set; }

    [JsonPropertyName("overallAverageScore")]
    public double OverallAverageScore { get; set; }

    [JsonPropertyName("byBook")]
    public List<BookRatingStatsDto> ByBook { get; set; } = [];
}

public class BookRatingStatsDto
{
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("genre")]
    public string Genre { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("averageRating")]
    public double AverageRating { get; set; }

    [JsonPropertyName("ratingCount")]
    public int RatingCount { get; set; }
}
