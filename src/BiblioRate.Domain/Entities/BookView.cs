using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class BookView
{
    [Key] public int      ViewId   { get; set; }
    public int?           UserId   { get; set; }
    public int            BookId   { get; set; }
    public DateTime       ViewedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Book  Book { get; set; } = null!;
}
