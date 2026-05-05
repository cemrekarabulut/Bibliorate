using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class DataSeederServiceTests
{
    private readonly DataSeederService _service;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IGoogleBooksService> _mockGoogle;
    private readonly Mock<IBookRepository> _mockRepo;
    private readonly Mock<IBookSimilarityScorer> _mockSimilarity;
    private readonly Mock<IBookQualityEvaluator> _mockQualityEvaluator;

    public DataSeederServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _mockGoogle = new Mock<IGoogleBooksService>();
        _mockRepo = new Mock<IBookRepository>();
        _mockSimilarity = new Mock<IBookSimilarityScorer>();
        _mockQualityEvaluator = new Mock<IBookQualityEvaluator>();

        _service = new DataSeederService(
            _mockGoogle.Object,
            _mockRepo.Object,
            _context,
            _mockQualityEvaluator.Object,
            _mockSimilarity.Object);
    }

    [Fact]
    public async Task SeedAsync_ShouldProcessQueries_WhenDbIsEmpty()
    {
        // Arrange
        _mockGoogle.Setup(g => g.SearchBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Book> { new Book { Title = "Seeded Book", Description = "Long enough description for seeding test" } });
        
        _mockQualityEvaluator.Setup(q => q.Evaluate(It.IsAny<Book>())).Returns(100);

        // Act - CancellationToken ile kısa kesiyoruz ki tüm listeyi dönmesin
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // İlk döngüden sonra iptal et
        
        try { await _service.SeedAsync(cts.Token); } catch(OperationCanceledException) {}

        // Assert
        _mockGoogle.Verify(g => g.SearchBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateExistingCategoriesAsync_ShouldNormalizeAndDedup()
    {
        // Arrange
        var book1 = new Book { BookId = 1, Title = "Book A", Author = "Gillian Flynn", QualityScore = 50 };
        var book2 = new Book { BookId = 2, Title = "Book A", Author = "Gillian Flynn", QualityScore = 80 }; // Duplicate with better score
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync();

        _mockQualityEvaluator.Setup(q => q.Evaluate(It.IsAny<Book>())).Returns(90);

        // Act
        await _service.UpdateExistingCategoriesAsync();

        // Assert
        _context.Books.Count().Should().Be(1);
        _context.Books.First().Author.Should().Be("Gillian Flynn"); // Normalization check
        _context.Books.First().Genre.Should().Be("Mystery & Thriller"); // Genre normalization
    }

    [Fact]
    public async Task UpdateExistingCategoriesAsync_ShouldDedupByLevenshtein()
    {
        // Arrange
        // "The Midnight Library" ve "Midnight Library" (yazar Matt Haig) benzer bulunmalı
        var book1 = new Book { BookId = 1, Title = "The Midnight Library", Author = "Matt Haig", QualityScore = 50 };
        var book2 = new Book { BookId = 2, Title = "Midnight Library", Author = "Matt Haig", QualityScore = 90 };
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync();

        _mockQualityEvaluator.Setup(q => q.Evaluate(It.IsAny<Book>())).Returns(95);

        // Act
        await _service.UpdateExistingCategoriesAsync();

        // Assert
        _context.Books.Count().Should().Be(1);
        _context.Books.First().QualityScore.Should().Be(95);
    }

    [Fact]
    public async Task UpdateExistingCategoriesAsync_ShouldWork_WhenDbIsEmpty()
    {
        // Act
        Func<Task> act = async () => await _service.UpdateExistingCategoriesAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }
}
