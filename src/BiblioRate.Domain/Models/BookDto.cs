using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

public class BookDto
{
    [JsonPropertyName("id")]
    public int BookId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; } = [];

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("ratingAvg")]
    public double RatingAvg { get; set; }

    [JsonPropertyName("ratingCount")]
    public int RatingCount { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = [];
}
