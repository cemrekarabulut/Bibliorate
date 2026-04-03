using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class Review
{
    [Key] public int    ReviewId  { get; set; }
    public int          UserId    { get; set; }
    public int          BookId    { get; set; }

    [Required, MinLength(1)]
    public string       Comment   { get; set; } = string.Empty;
    public DateTime     CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Book? Book { get; set; }
}
