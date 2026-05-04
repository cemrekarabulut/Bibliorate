using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities;

public class SearchLog
{
    [Key]     public int      SearchId   { get; set; }
    public    int?            UserId     { get; set; }

    [Required]
    public    string          Query      { get; set; } = string.Empty;
    public    DateTime        SearchedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
