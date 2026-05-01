using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models;

/// <summary>
/// Flask /api/analytics/most-active-users → {username, views}
/// </summary>
public class ActiveUserDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("views")]
    public int Views { get; set; }
}
