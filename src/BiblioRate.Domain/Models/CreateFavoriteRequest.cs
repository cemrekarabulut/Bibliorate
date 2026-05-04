using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Models;

/// <summary>POST /api/favorites için istek modeli.</summary>
public class CreateFavoriteRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }
}
