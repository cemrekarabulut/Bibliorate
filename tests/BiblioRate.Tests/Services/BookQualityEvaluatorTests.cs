using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Services;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class BookQualityEvaluatorTests
{
    private readonly BookQualityEvaluator _sut = new();

    // ── HasGoodThumbnail (30 puan) ────────────────────────────────────────────

    [Fact]
    public void Evaluate_GoodThumbnailAndGoodDescAndValidIsbnAndCleanGenre_Returns100()
    {
        // Arrange — tüm kriterleri karşılayan bir kitap
        var book = new Book
        {
            ThumbnailUrl = "https://books.google.com/books/content?id=abc&printsec=frontcover&img=1&zoom=2",
            Description  = new string('A', 250), // > 200 karakter
            Isbn         = "9780132350884",       // valid ISBN-13 (Clean Code)
            Genre        = "Software Engineering"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().Be(100);
    }

    [Fact]
    public void Evaluate_EmptyThumbnailUrl_DoesNotGetThumbnailPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "0000000000",
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — hiç puan yok (thumbnail=0, desc=0, isbn=0, genre=0)
        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_PlaceholderUrl_DoesNotGetThumbnailPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "https://example.com/placeholder.jpg",
            Description  = "Short",
            Isbn         = "bad-isbn",
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — thumbnail puanı 0
        score.Should().BeLessThan(30);
    }

    [Fact]
    public void Evaluate_NoImageUrl_DoesNotGetThumbnailPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "https://example.com/no_image.jpg",
            Description  = "Short",
            Isbn         = "bad",
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeLessThan(30);
    }

    // ── HasGoodDesc (25 puan) ─────────────────────────────────────────────────

    [Fact]
    public void Evaluate_DescriptionLongerThan200_GetsDescriptionPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = new string('X', 201),
            Isbn         = "0000000000",
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — desc(25) + isbn-fail-but-desc-bonus(15) = 40
        score.Should().BeGreaterThanOrEqualTo(25);
    }

    [Fact]
    public void Evaluate_DescriptionExactly200_DoesNotGetDescPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = new string('X', 200), // NOT > 200
            Isbn         = "9780132350884",
            Genre        = "Software"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — desc puan almamalı (200 karakter tam, > 200 değil)
        score.Should().BeLessThan(100);
    }

    // ── IsValidIsbn13 (20 puan) ───────────────────────────────────────────────

    [Fact]
    public void Evaluate_ValidIsbn13_GetsIsbnPoints()
    {
        // Arrange — ISBN-13: 9780132350884 (Clean Code)
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "9780132350884",
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — isbn(20) = 20
        score.Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void Evaluate_InvalidIsbn_DoesNotGetIsbnPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "1234567890123", // yanlış checksum
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeLessThan(20);
    }

    [Fact]
    public void Evaluate_IsbnWithDashes_ParsedCorrectly()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "978-0-13-235088-4", // tire içeren geçerli ISBN
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void Evaluate_NullIsbn_DoesNotGetIsbnPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = null!,
            Genre        = "General"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeLessThan(20);
    }

    // ── IsCleanGenre (25 puan) ────────────────────────────────────────────────

    [Fact]
    public void Evaluate_CleanGenre_GetsGenrePoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "bad",
            Genre        = "Software Engineering"
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(25);
    }

    [Theory]
    [InlineData("General")]
    [InlineData("Juvenile Fiction")]
    [InlineData("Undefined")]
    [InlineData("Adopted children")]
    public void Evaluate_NoisyGenre_DoesNotGetGenrePoints(string noisyGenre)
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = "Short",
            Isbn         = "bad",
            Genre        = noisyGenre
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert
        score.Should().BeLessThan(25);
    }

    // ── ISBN-fail + GoodDesc bonus (15 puan) ──────────────────────────────────

    [Fact]
    public void Evaluate_BadIsbnButGoodDesc_Gets15BonusPoints()
    {
        // Arrange
        var book = new Book
        {
            ThumbnailUrl = "",
            Description  = new string('D', 250), // > 200
            Isbn         = "0000000000",          // invalid
            Genre        = "General"              // noisy — 0 genre
        };

        // Act
        var score = _sut.Evaluate(book);

        // Assert — desc(25) + bonus(15) = 40
        score.Should().Be(40);
    }

    // ── Clamp 0-100 ───────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ScoreAlwaysBetween0And100()
    {
        // Arrange — kötü kitap
        var badBook = new Book { ThumbnailUrl = "", Description = "", Isbn = "bad", Genre = "General" };

        // Act
        var score = _sut.Evaluate(badBook);

        // Assert
        score.Should().BeInRange(0, 100);
    }
}
