using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/analytics/views-over-time → {date, views}
/// </summary>
public class ViewsOverTimeDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("views")]
    public int Views { get; set; }
}
