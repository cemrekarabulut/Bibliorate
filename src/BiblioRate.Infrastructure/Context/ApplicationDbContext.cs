using BiblioRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Book>      Books      { get; set; }
    public DbSet<User>      Users      { get; set; }
    public DbSet<Rating>    Ratings    { get; set; }
    public DbSet<Review>    Reviews    { get; set; }
    public DbSet<BookView>  BookViews  { get; set; }
    public DbSet<Favorite>  Favorites  { get; set; }
    public DbSet<SearchLog> SearchLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter: soft-delete — IsDeleted=true olan kayıtlar otomatik gizlenir
        modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);

        // Veritabanı düzeyinde UNIQUE kısıtlamaları
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.UserId, r.BookId }).IsUnique();

        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.BookId }).IsUnique();
    }
}
