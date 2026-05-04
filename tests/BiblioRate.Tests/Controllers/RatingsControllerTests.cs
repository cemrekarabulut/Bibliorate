using System.Security.Claims;
using BiblioRate.API.Controllers;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class RatingsControllerTests
{
    private readonly Mock<IRatingRepository> _ratingRepoMock;
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly RatingsController _sut;

    public RatingsControllerTests()
    {
        _ratingRepoMock = new Mock<IRatingRepository>();
        _reviewRepoMock = new Mock<IReviewRepository>();
        _sut = new RatingsController(_ratingRepoMock.Object, _reviewRepoMock.Object);
    }

    // ── AddRating ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddRating_NoTokenClaim_Returns401()
    {
        // Arrange — controller without any claims
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await _sut.AddRating(new CreateRatingRequest { BookId = 1, Score = 8 });

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AddRating_AlreadyRated_Returns409Conflict()
    {
        // Arrange
        SetUserClaim(_sut, "5");
        var existingRatings = new List<Rating> { new() { UserId = 5, BookId = 1, Score = 7 } };
        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(1)).ReturnsAsync(existingRatings);

        // Act
        var result = await _sut.AddRating(new CreateRatingRequest { BookId = 1, Score = 8 });

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task AddRating_ValidRequest_WithoutComment_Returns200()
    {
        // Arrange
        SetUserClaim(_sut, "10");
        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(2)).ReturnsAsync([]);
        _ratingRepoMock.Setup(r => r.AddRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(2)).ReturnsAsync(8.0);

        // Act
        var result = await _sut.AddRating(new CreateRatingRequest { BookId = 2, Score = 8, Comment = null });

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        _reviewRepoMock.Verify(r => r.AddReviewAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task AddRating_ValidRequest_WithComment_AlsoAddsReview()
    {
        // Arrange
        SetUserClaim(_sut, "10");
        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(3)).ReturnsAsync([]);
        _ratingRepoMock.Setup(r => r.AddRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(3)).ReturnsAsync(9.0);
        _reviewRepoMock.Setup(r => r.AddReviewAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddRating(new CreateRatingRequest { BookId = 3, Score = 9, Comment = "Harika!" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _reviewRepoMock.Verify(r => r.AddReviewAsync(It.Is<Review>(rv =>
            rv.Comment == "Harika!" && rv.BookId == 3)), Times.Once);
    }

    // ── UpdateRating ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRating_NoTokenClaim_Returns401()
    {
        // Arrange
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await _sut.UpdateRating(new CreateRatingRequest { BookId = 1, Score = 5 });

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateRating_RatingNotFound_Returns404()
    {
        // Arrange
        SetUserClaim(_sut, "7");
        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(7)).ReturnsAsync([]);

        // Act
        var result = await _sut.UpdateRating(new CreateRatingRequest { BookId = 1, Score = 5 });

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateRating_ExistingRating_WithoutComment_DeletesExistingReview()
    {
        // Arrange
        SetUserClaim(_sut, "8");
        var existingRating = new Rating { RatingId = 100, UserId = 8, BookId = 4, Score = 6 };
        var existingReview = new Review { ReviewId = 200, UserId = 8, BookId = 4, Comment = "Eski yorum" };

        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(8)).ReturnsAsync([existingRating]);
        _ratingRepoMock.Setup(r => r.UpdateRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(4)).ReturnsAsync(7.0);
        _reviewRepoMock.Setup(r => r.GetReviewsByUserIdAsync(8)).ReturnsAsync([existingReview]);
        _reviewRepoMock.Setup(r => r.DeleteReviewAsync(200)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateRating(new CreateRatingRequest { BookId = 4, Score = 7, Comment = null });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _reviewRepoMock.Verify(r => r.DeleteReviewAsync(200), Times.Once);
    }

    [Fact]
    public async Task UpdateRating_ExistingRating_UpdatesExistingReview()
    {
        // Arrange
        SetUserClaim(_sut, "9");
        var existingRating = new Rating { RatingId = 101, UserId = 9, BookId = 5, Score = 6 };
        var existingReview = new Review { ReviewId = 201, UserId = 9, BookId = 5, Comment = "Eski" };

        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(9)).ReturnsAsync([existingRating]);
        _ratingRepoMock.Setup(r => r.UpdateRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(5)).ReturnsAsync(8.0);
        _reviewRepoMock.Setup(r => r.GetReviewsByUserIdAsync(9)).ReturnsAsync([existingReview]);
        _reviewRepoMock.Setup(r => r.UpdateReviewAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateRating(new CreateRatingRequest { BookId = 5, Score = 8, Comment = "Yeni yorum" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _reviewRepoMock.Verify(r => r.UpdateReviewAsync(It.Is<Review>(rv => rv.Comment == "Yeni yorum")), Times.Once);
    }

    [Fact]
    public async Task UpdateRating_ExistingRating_CreatesNewReviewIfNoneExists()
    {
        // Arrange
        SetUserClaim(_sut, "11");
        var existingRating = new Rating { RatingId = 102, UserId = 11, BookId = 6, Score = 5 };

        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(11)).ReturnsAsync([existingRating]);
        _ratingRepoMock.Setup(r => r.UpdateRatingAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(6)).ReturnsAsync(6.0);
        _reviewRepoMock.Setup(r => r.GetReviewsByUserIdAsync(11)).ReturnsAsync([]);
        _reviewRepoMock.Setup(r => r.AddReviewAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateRating(new CreateRatingRequest { BookId = 6, Score = 6, Comment = "Yeni" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _reviewRepoMock.Verify(r => r.AddReviewAsync(It.Is<Review>(rv => rv.Comment == "Yeni")), Times.Once);
    }

    // ── GetBookRatings ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookRatings_ReturnsOkWithCombinedRatingsAndReviews()
    {
        // Arrange
        var user = new User { UserId = 1, Username = "UserA" };
        var ratings = new List<Rating> { new() { UserId = 1, BookId = 10, Score = 8, User = user } };
        var reviews = new List<Review> { new() { UserId = 1, BookId = 10, Comment = "İyi", User = user } };

        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(10)).ReturnsAsync(ratings);
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(10)).ReturnsAsync(reviews);

        // Act
        var result = await _sut.GetBookRatings(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetBookRatings_NoRatings_NoReviews_ReturnsEmptyList()
    {
        // Arrange
        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(20)).ReturnsAsync([]);
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(20)).ReturnsAsync([]);

        // Act
        var result = await _sut.GetBookRatings(20);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBookRatings_ReviewWithoutRating_IncludedAsUnratedReview()
    {
        // Arrange
        var user = new User { UserId = 2, Username = "ReviewOnly" };
        _ratingRepoMock.Setup(r => r.GetRatingsByBookIdAsync(30)).ReturnsAsync([]);
        _reviewRepoMock.Setup(r => r.GetReviewsByBookIdAsync(30))
                       .ReturnsAsync([new Review { UserId = 2, BookId = 30, Comment = "Salt yorum", User = user }]);

        // Act
        var result = await _sut.GetBookRatings(30);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetUserRatings ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserRatings_ReturnsOkWithCombinedData()
    {
        // Arrange
        var user = new User { UserId = 15, Username = "UserB" };
        var ratings = new List<Rating> { new() { UserId = 15, BookId = 1, Score = 7, User = user } };
        var reviews = new List<Review> { new() { UserId = 15, BookId = 1, Comment = "Güzel", User = user } };

        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(15)).ReturnsAsync(ratings);
        _reviewRepoMock.Setup(r => r.GetReviewsByUserIdAsync(15)).ReturnsAsync(reviews);

        // Act
        var result = await _sut.GetUserRatings(15);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUserRatings_NoData_ReturnsEmptyOk()
    {
        // Arrange
        _ratingRepoMock.Setup(r => r.GetRatingsByUserIdAsync(99)).ReturnsAsync([]);
        _reviewRepoMock.Setup(r => r.GetReviewsByUserIdAsync(99)).ReturnsAsync([]);

        // Act
        var result = await _sut.GetUserRatings(99);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetAverageScore ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAverageScore_ReturnsOkWithRoundedAverage()
    {
        // Arrange
        _ratingRepoMock.Setup(r => r.GetAverageScoreAsync(7)).ReturnsAsync(7.666);

        // Act
        var result = await _sut.GetAverageScore(7);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void SetUserClaim(ControllerBase controller, string userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
