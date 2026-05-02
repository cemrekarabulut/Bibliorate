using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

public class ReviewDto
{
    [JsonPropertyName("reviewId")]
    public int ReviewId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
