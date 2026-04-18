using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required] public string Username     { get; set; } = string.Empty;
    [Required] public string Email        { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Rating>    Ratings    { get; set; } = [];
    public ICollection<Review>    Reviews    { get; set; } = [];
    public ICollection<Favorite>  Favorites  { get; set; } = [];
    public ICollection<BookView>  BookViews  { get; set; } = [];
    public ICollection<SearchLog> SearchLogs { get; set; } = [];
}
