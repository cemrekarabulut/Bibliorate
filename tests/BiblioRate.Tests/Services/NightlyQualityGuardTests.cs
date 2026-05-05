using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class NightlyQualityGuardTests
{
    private readonly NightlyQualityGuard _guard;
    private readonly ApplicationDbContext _context;

    public NightlyQualityGuardTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _guard = new NightlyQualityGuard(_context);
    }

    [Fact]
    public async Task RunAsync_ShouldSoftDeleteLowQualityBooks()
    {
        // Arrange
        var goodBook = new Book { Title = "Good", QualityScore = 80, IsDeleted = false };
        var badBook = new Book { Title = "Bad", QualityScore = 10, IsDeleted = false };
        _context.Books.AddRange(goodBook, badBook);
        await _context.SaveChangesAsync();

        // Act
        await _guard.RunAsync(CancellationToken.None);

        // Assert
        badBook.IsDeleted.Should().BeTrue();
        badBook.DeletedAt.Should().NotBeNull();
        goodBook.IsDeleted.Should().BeFalse();
    }
}
