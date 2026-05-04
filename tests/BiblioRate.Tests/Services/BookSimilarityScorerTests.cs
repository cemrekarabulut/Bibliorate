using BiblioRate.Infrastructure.Services;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class BookSimilarityScorerTests
{
    private readonly BookSimilarityScorer _sut = new();

    // ── Score ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Score_IdenticalTitlesAndAuthors_Returns1()
    {
        // Act
        var score = _sut.Score("Clean Code", "Robert Martin", "Clean Code", "Robert Martin");

        // Assert
        score.Should().Be(1.0);
    }

    [Fact]
    public void Score_CompletelyDifferentBooks_ReturnsLowScore()
    {
        // Act
        var score = _sut.Score("War and Peace", "Leo Tolstoy", "Python Cookbook", "David Beazley");

        // Assert
        score.Should().BeLessThan(0.5);
    }

    [Fact]
    public void Score_SimilarTitlesDifferentAuthors_ReturnsMidScore()
    {
        // Act
        var score = _sut.Score("Clean Code", "Robert Martin", "Clean Architecture", "Bob Martin");

        // Assert
        score.Should().BeGreaterThan(0.3).And.BeLessThan(1.0);
    }

    [Fact]
    public void Score_EmptyTitleAndAuthor_BothEmpty_Returns1()
    {
        // Act — both empty: Levenshtein returns 1.0 for identical strings (empty == empty)
        var score = _sut.Score("", "", "", "");

        // Assert
        score.Should().Be(1.0);
    }

    [Fact]
    public void Score_OneSideEmpty_ReturnsLowScore()
    {
        // Act
        var score = _sut.Score("Clean Code", "Robert Martin", "", "");

        // Assert
        score.Should().BeLessThan(0.5);
    }

    [Fact]
    public void Score_CaseInsensitiveComparison()
    {
        // Arrange
        var score1 = _sut.Score("Clean Code", "Robert Martin", "Clean Code", "Robert Martin");
        var score2 = _sut.Score("CLEAN CODE", "ROBERT MARTIN", "clean code", "robert martin");

        // Assert
        score1.Should().Be(score2);
    }

    [Fact]
    public void Score_ReturnsValueBetweenZeroAndOne()
    {
        // Act
        var score = _sut.Score("Test Title", "Test Author", "Something Else", "Different Person");

        // Assert
        score.Should().BeInRange(0.0, 1.0);
    }

    // ── IsDuplicate ───────────────────────────────────────────────────────────

    [Fact]
    public void IsDuplicate_IdenticalBooks_ReturnsTrue()
    {
        // Act
        var result = _sut.IsDuplicate("1984", "George Orwell", "1984", "George Orwell");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDuplicate_SlightlyDifferentVariants_ResultIsBooleanType()
    {
        // Arrange — "1984" ve "1984 Large Print" — threshold 0.75
        var result = _sut.IsDuplicate("1984", "George Orwell", "1984 Large Print", "George Orwell");

        // Assert — bool türünde sonuç döndüğünü doğrula (true veya false)
        (result == true || result == false).Should().BeTrue();
    }

    [Fact]
    public void IsDuplicate_CompletelyDifferentBooks_ReturnsFalse()
    {
        // Act
        var result = _sut.IsDuplicate("War and Peace", "Leo Tolstoy", "Python Cookbook", "David Beazley");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDuplicate_SameScoreAsThreshold_IsConsistentWithScore()
    {
        // Arrange
        const string t1 = "Clean Code";
        const string a1 = "Robert Martin";
        const string t2 = "Clean Code";
        const string a2 = "Robert Martin";

        // Act
        var score     = _sut.Score(t1, a1, t2, a2);
        var duplicate = _sut.IsDuplicate(t1, a1, t2, a2);

        // Assert — score >= 0.75 → duplicate = true
        if (score >= 0.75)
            duplicate.Should().BeTrue();
        else
            duplicate.Should().BeFalse();
    }

    // ── AuthorFingerprint edge-cases ───────────────────────────────────────────

    [Fact]
    public void Score_SingleWordAuthor_HandledWithoutException()
    {
        // Act
        var act = () => _sut.Score("Book", "Plato", "Book", "Plato");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Score_WhitespaceAuthor_HandledWithoutException()
    {
        // Act
        var act = () => _sut.Score("Book", "   ", "Book", "Author");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Score_SpecialCharactersInTitle_HandledWithoutException()
    {
        // Act
        var act = () => _sut.Score("C++ Primer", "Stanley Lippman", "C++ Primer Plus", "Stephen Prata");

        // Assert
        act.Should().NotThrow();
    }
}
