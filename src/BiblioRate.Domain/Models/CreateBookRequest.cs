using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>POST /api/books için istek modeli.</summary>
public class CreateBookRequest
{
    [Required, MinLength(1)]
    public string  Title        { get; set; } = string.Empty;
    public string  Author       { get; set; } = "Bilinmeyen Yazar";
    public string  Genre        { get; set; } = "Genel";
    public int     Year         { get; set; }
    public string  Description  { get; set; } = string.Empty;
    public string  ThumbnailUrl { get; set; } = string.Empty;
    public string? GoogleBookId { get; set; }
    public string  Isbn         { get; set; } = "0000000000";
    public DateTime? PublishedAt { get; set; }
}
