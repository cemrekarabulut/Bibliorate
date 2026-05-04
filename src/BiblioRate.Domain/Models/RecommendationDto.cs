using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/recommend-smart/{userId} ve /api/recommend/{userId}
/// Dönüş: {title, rating, votes}
/// </summary>
public class RecommendationDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("votes")]
    public int Votes { get; set; }
}
