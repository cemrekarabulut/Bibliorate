using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class Rating
{
    [Key] public int RatingId { get; set; }
    public int      UserId    { get; set; }
    public int      BookId    { get; set; }

    [Range(1, 10)]
    public int      Score     { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Book? Book { get; set; }
}
