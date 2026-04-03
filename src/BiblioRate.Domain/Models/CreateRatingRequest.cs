using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>POST /api/ratings için istek modeli.</summary>
public class CreateRatingRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required, Range(1, 10, ErrorMessage = "Puan 1 ile 10 arasında olmalıdır.")]
    public int Score { get; set; }
}
