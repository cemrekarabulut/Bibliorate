using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/analytics/most-viewed  → {title, views}
/// Flask /api/analytics/top-rated    → {title, rating, votes}
/// Her iki endpoint'i de kapsayan birleşik DTO.
/// Null alanlar JSON'da atlanır; frontend null-check yapmalı.
/// </summary>
public class BookAnalyticsDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>most-viewed endpoint'inden gelir.</summary>
    [JsonPropertyName("views")]
    public int? Views { get; set; }

    /// <summary>top-rated endpoint'inden gelir.</summary>
    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    /// <summary>top-rated endpoint'inden gelir.</summary>
    [JsonPropertyName("votes")]
    public int? Votes { get; set; }
}
