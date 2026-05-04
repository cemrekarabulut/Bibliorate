using BiblioRate.API.Extensions;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using FluentAssertions;

namespace BiblioRate.Tests.Extensions;

public class BookMappingExtensionsTests
{
    // ── ToDto – Temel Mapping ─────────────────────────────────────────────────

    [Fact]
    public void ToDto_BasicBook_MapsAllCoreFields()
    {
        // Arrange
        var book = new Book
        {
            BookId       = 42,
            Title        = "Clean Code",
            Author       = "Robert C. Martin",
            Genre        = "Software Engineering",
            Description  = "A handbook of agile software craftsmanship.",
            ThumbnailUrl = "https://books.google.com/books/content?id=abc&zoom=1",
            Isbn         = "9780132350884"
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.BookId.Should().Be(42);
        dto.Title.Should().Be("Clean Code");
        dto.Genre.Should().Be("Software Engineering");
        dto.Description.Should().Be("A handbook of agile software craftsmanship.");
    }

    [Fact]
    public void ToDto_SingleAuthor_ReturnsSingleItemInAuthorsList()
    {
        // Arrange
        var book = new Book { Title = "Test", Author = "John Doe", Genre = "Fiction" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Authors.Should().HaveCount(1).And.Contain("John Doe");
    }

    [Fact]
    public void ToDto_MultipleAuthorsCommaSeparated_SplitsCorrectly()
    {
        // Arrange
        var book = new Book { Title = "Multi", Author = "Author A, Author B, Author C", Genre = "Fiction" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Authors.Should().HaveCount(3);
        dto.Authors.Should().Contain("Author A");
        dto.Authors.Should().Contain("Author B");
        dto.Authors.Should().Contain("Author C");
    }

    [Fact]
    public void ToDto_EmptyAuthor_ReturnsEmptyAuthorsList()
    {
        // Arrange
        var book = new Book { Title = "NoAuthor", Author = "", Genre = "Fiction" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Authors.Should().BeEmpty();
    }

    // ── ToDto – Rating Hesaplama ──────────────────────────────────────────────

    [Fact]
    public void ToDto_NoRatings_ReturnsZeroRatingAvg()
    {
        // Arrange
        var book = new Book { Title = "Test", Author = "A", Genre = "G", Ratings = [] };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.RatingAvg.Should().Be(0.0);
        dto.AverageRating.Should().Be(0.0);
        dto.RatingCount.Should().Be(0);
    }

    [Fact]
    public void ToDto_WithRatings_CalculatesAverageCorrectly()
    {
        // Arrange — (8 + 6 + 10) / 3 = 8.0
        var book = new Book
        {
            Title   = "Test",
            Author  = "A",
            Genre   = "G",
            Ratings =
            [
                new Rating { Score = 8 },
                new Rating { Score = 6 },
                new Rating { Score = 10 }
            ]
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.RatingAvg.Should().Be(8.0);
        dto.AverageRating.Should().Be(8.0);
        dto.RatingCount.Should().Be(3);
    }

    [Fact]
    public void ToDto_ExplicitRatingAvgParam_OverridesCalculation()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "G", Ratings = [new Rating { Score = 5 }] };

        // Act — explicit override
        var dto = book.ToDto(ratingAvg: 9.9, ratingCount: 100);

        // Assert
        dto.RatingAvg.Should().Be(9.9);
        dto.RatingCount.Should().Be(100);
    }

    [Fact]
    public void ToDto_RatingAvgAndAverageRatingAreBothSet()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "G", Ratings = [new Rating { Score = 7 }] };

        // Act
        var dto = book.ToDto();

        // Assert — ikisi de aynı değerde olmalı (React uyumu)
        dto.RatingAvg.Should().Be(dto.AverageRating);
    }

    // ── ToDto – Kategori ─────────────────────────────────────────────────────

    [Fact]
    public void ToDto_Genre_PopulatedInCategories()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "Mystery" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Categories.Should().ContainSingle().Which.Should().Be("Mystery");
        dto.Genre.Should().Be("Mystery");
    }

    [Fact]
    public void ToDto_EmptyGenre_ReturnsEmptyCategories()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Categories.Should().BeEmpty();
    }

    // ── ToDto – Reviews Mapping ───────────────────────────────────────────────

    [Fact]
    public void ToDto_WithReviews_MapsReviewsAndOrdersByDateDesc()
    {
        // Arrange
        var user = new User { Username = "TestUser" };
        var older = new Review { ReviewId = 1, UserId = 1, Comment = "Eski", CreatedAt = DateTime.UtcNow.AddDays(-2), User = user };
        var newer = new Review { ReviewId = 2, UserId = 2, Comment = "Yeni", CreatedAt = DateTime.UtcNow,              User = user };

        var book = new Book { Title = "T", Author = "A", Genre = "G", Reviews = [older, newer] };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Reviews.Should().HaveCount(2);
        dto.Reviews[0].Comment.Should().Be("Yeni"); // newest first
        dto.Reviews[1].Comment.Should().Be("Eski");
    }

    [Fact]
    public void ToDto_NullReviews_ReturnsEmptyReviewList()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "G", Reviews = null! };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Reviews.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ReviewWithNullUser_FallsBackToUnknownUser()
    {
        // Arrange
        var book = new Book
        {
            Title   = "T",
            Author  = "A",
            Genre   = "G",
            Reviews = [new Review { ReviewId = 1, UserId = 1, Comment = "Yorum", User = null }]
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.Reviews[0].Username.Should().Be("Unknown User");
    }

    // ── ToDto – ThumbnailUrl Sanitization ────────────────────────────────────

    [Fact]
    public void ToDto_HttpThumbnail_UpgradesToHttps()
    {
        // Arrange
        var book = new Book { Title = "T", Author = "A", Genre = "G", ThumbnailUrl = "http://books.google.com/thumb.jpg" };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().StartWith("https://");
    }

    [Fact]
    public void ToDto_Zoom1InUrl_ReplacedWithZoom2()
    {
        // Arrange
        var book = new Book
        {
            Title        = "T",
            Author       = "A",
            Genre        = "G",
            ThumbnailUrl = "https://books.google.com/books/content?id=abc&zoom=1"
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().Contain("zoom=2").And.NotContain("zoom=1");
    }

    [Fact]
    public void ToDto_EmptyThumbnail_WithValidIsbn_FallsBackToOpenLibrary()
    {
        // Arrange
        var book = new Book
        {
            Title        = "T",
            Author       = "A",
            Genre        = "G",
            ThumbnailUrl = "",
            Isbn         = "9780132350884"
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().Contain("openlibrary.org");
    }

    [Fact]
    public void ToDto_EmptyThumbnailAndNoIsbn_FallsBackToPlaceholder()
    {
        // Arrange
        var book = new Book
        {
            Title        = "T",
            Author       = "A",
            Genre        = "G",
            ThumbnailUrl = "",
            Isbn         = ""
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().Contain("placehold.co");
    }

    [Fact]
    public void ToDto_OpenLibraryUrl_NotManipulated()
    {
        // Arrange — OpenLibrary URL'leri zoom/edge manipülasyonundan muaf
        var originalUrl = "https://covers.openlibrary.org/b/isbn/9780132350884-L.jpg";
        var book = new Book { Title = "T", Author = "A", Genre = "G", ThumbnailUrl = originalUrl };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().Be(originalUrl);
    }

    [Fact]
    public void ToDto_BadGoogleUrl_FallsBackToIsbnUrl()
    {
        // Arrange — "books.google.com/books?id=" bad signal
        var book = new Book
        {
            Title        = "T",
            Author       = "A",
            Genre        = "G",
            ThumbnailUrl = "https://books.google.com/books?id=xyz",
            Isbn         = "9780132350884"
        };

        // Act
        var dto = book.ToDto();

        // Assert
        dto.ThumbnailUrl.Should().Contain("openlibrary.org");
    }
}
