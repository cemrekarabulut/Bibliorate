using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/analytics/genre-popularity → {genre, count}
/// </summary>
public class GenrePopularityDto
{
    [JsonPropertyName("genre")]
    public string Genre { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
