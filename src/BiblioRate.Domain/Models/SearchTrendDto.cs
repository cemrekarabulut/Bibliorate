using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/analytics/search-trend → {date, searches}
/// </summary>
public class SearchTrendDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("searches")]
    public int Searches { get; set; }
}
