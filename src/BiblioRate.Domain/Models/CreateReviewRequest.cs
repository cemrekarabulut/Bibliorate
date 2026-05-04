using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>POST /api/reviews için istek modeli.</summary>
public class CreateReviewRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required, MinLength(1, ErrorMessage = "Yorum içeriği boş olamaz.")]
    public string Comment { get; set; } = string.Empty;
}
