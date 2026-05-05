using BiblioRate.Infrastructure.Services;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class GenreNormalizerTests
{
    [Theory]
    [InlineData("Fiction", "Fiction")]
    [InlineData("Classic", "Classics & Philosophy")]
    [InlineData("Mystery", "Mystery & Thriller")]
    [InlineData("Historical Fiction", "Drama")]
    [InlineData("Juvenile Fiction", "Fiction")]
    [InlineData("Detective", "Mystery & Thriller")] // Merge rule branch
    [InlineData("Philosophy", "Classics & Philosophy")] // Merge rule branch
    [InlineData(null, "Fiction")] // Null branch
    [InlineData("Nonexistent", "Nonexistent")] // No match branch
    public void Normalize_ShouldMapCorrectly(string? raw, string expected)
    {
        // Act
        var result = GenreNormalizer.Normalize(raw);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Normalize_ShouldUseAuthorProtection()
    {
        // Act
        var result = GenreNormalizer.Normalize("Some Genre", author: "Gillian Flynn");

        // Assert
        result.Should().Be("Mystery & Thriller");
    }

    [Fact]
    public void Normalize_ShouldDetectRomanceFromDescription()
    {
        // Act
        var result = GenreNormalizer.Normalize("Fiction", description: "A beautiful romantic story.");

        // Assert
        result.Should().Be("Romance");
    }

    [Theory]
    [InlineData("Fiction", "This is a lovely romance story about love.", "General", "Author", "Romance")] // Description signal branch
    [InlineData("Fiction", "Ordinary story", "General", "Author", "Fiction")]
    public void ResolveFromCategories_ShouldDetectSignals(string cat, string desc, string authGenre, string auth, string expected)
    {
        // Act
        var result = GenreNormalizer.ResolveFromCategories(new List<string> { cat }, desc, authGenre, auth);

        // Assert
        result.Should().Be(expected);
    }
}
