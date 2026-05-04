using BiblioRate.API.Controllers;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly ReviewsController _sut;

    public ReviewsControllerTests()
    {
        _reviewRepoMock = new Mock<IReviewRepository>();
        _sut = new ReviewsController(_reviewRepoMock.Object);
    }

    // ── AddReview ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReview_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateReviewRequest { UserId = 1, BookId = 10, Comment = "Harika kitap!" };
        _reviewRepoMock.Setup(r => r.AddReviewAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddReview(request);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        _reviewRepoMock.Verify(r => r.AddReviewAsync(It.Is<Review>(rv =>
            rv.UserId == 1 && rv.BookId == 10 && rv.Comment == "Harika kitap!")), Times.Once);
    }

    [Fact]
    public async Task AddReview_SetsCreatedAtToNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var request = new CreateReviewRequest { UserId = 2, BookId = 5, Comment = "İyi" };

        Review? captured = null;
        _reviewRepoMock.Setup(r => r.AddReviewAsync(It.IsAny<Review>()))
                       .Callback<Review>(rv => captured = rv)
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.AddReview(request);

        // Assert
        captured.Should().NotBeNull();
        captured!.CreatedAt.Should().BeOnOrAfter(before);
    }

    // ── GetReviewsByBook ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetReviewsByBook_WithReviews_ReturnsOkWithMappedData()
    {
        // Arrange
        var user = new User { Username = "TestUser" };
        var reviews = new List<Review>
        {
            new() { ReviewId = 1, UserId = 10, BookId = 5, Comment = "Güzel", User = user },
            new() { ReviewId = 2, UserId = 11, BookId = 5, Comment = "Fena değil", User = null }
        };
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(5)).ReturnsAsync(reviews);

        // Act
        var result = await _sut.GetReviewsByBook(5);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReviewsByBook_EmptyList_ReturnsOkWithEmptyResult()
    {
        // Arrange
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(99)).ReturnsAsync([]);

        // Act
        var result = await _sut.GetReviewsByBook(99);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReviewsByBook_NullUser_FallsBackToAnonymousUsername()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new() { ReviewId = 3, UserId = 20, BookId = 8, Comment = "Yorumum", User = null }
        };
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(8)).ReturnsAsync(reviews);

        // Act
        var result = await _sut.GetReviewsByBook(8);

        // Assert — anonymous fallback içerdiğini doğrula (reflection ile)
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        // System.Text.Json encodes non-ASCII chars (e.g. 'ı' → \u0131).
        // Check for the Unicode-escaped form to avoid encoding mismatch.
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value,
            new System.Text.Json.JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        json.Should().Contain("Anonim Kullanıcı");
    }

    // ── DeleteReview ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteReview_ValidId_Returns200Ok()
    {
        // Arrange
        _reviewRepoMock.Setup(r => r.DeleteReviewAsync(7)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteReview(7);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        _reviewRepoMock.Verify(r => r.DeleteReviewAsync(7), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_CallsRepositoryWithCorrectId()
    {
        // Arrange
        _reviewRepoMock.Setup(r => r.DeleteReviewAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteReview(42);

        // Assert
        _reviewRepoMock.Verify(r => r.DeleteReviewAsync(42), Times.Once);
    }
}
