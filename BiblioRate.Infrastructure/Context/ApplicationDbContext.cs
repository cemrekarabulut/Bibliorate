using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Tabloları temsil eden DbSet'ler
    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<BookView> BookViews { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<SearchLog> SearchLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Arkadaşının SQL'indeki UNIQUE kısıtlamalarını burada tanımlıyoruz
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.UserId, r.BookId }).IsUnique();

        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.BookId }).IsUnique();
    }
}