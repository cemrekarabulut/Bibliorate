using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class Favorite
{
    [Key] public int FavId     { get; set; }
    public int       UserId    { get; set; }
    public int       BookId    { get; set; }
    public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Book? Book { get; set; }
}
