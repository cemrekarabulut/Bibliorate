using BiblioRate.API.Controllers;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class FavoritesControllerTests
{
    private readonly Mock<IFavoriteRepository> _favRepoMock;
    private readonly FavoritesController _sut;

    public FavoritesControllerTests()
    {
        _favRepoMock = new Mock<IFavoriteRepository>();
        _sut = new FavoritesController(_favRepoMock.Object);
    }

    // ── AddToFavorites ────────────────────────────────────────────────────────

    [Fact]
    public async Task AddToFavorites_AlreadyFavorited_Returns409Conflict()
    {
        // Arrange
        var request = new CreateFavoriteRequest { UserId = 1, BookId = 10 };
        _favRepoMock.Setup(r => r.IsFavoriteAsync(1, 10)).ReturnsAsync(true);

        // Act
        var result = await _sut.AddToFavorites(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        _favRepoMock.Verify(r => r.AddToFavoritesAsync(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task AddToFavorites_NotYetFavorited_Returns201Created()
    {
        // Arrange
        var request = new CreateFavoriteRequest { UserId = 2, BookId = 20 };
        _favRepoMock.Setup(r => r.IsFavoriteAsync(2, 20)).ReturnsAsync(false);
        _favRepoMock.Setup(r => r.AddToFavoritesAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddToFavorites(request);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        _favRepoMock.Verify(r => r.AddToFavoritesAsync(It.Is<Favorite>(f =>
            f.UserId == 2 && f.BookId == 20)), Times.Once);
    }

    [Fact]
    public async Task AddToFavorites_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var request = new CreateFavoriteRequest { UserId = 3, BookId = 30 };
        _favRepoMock.Setup(r => r.IsFavoriteAsync(3, 30)).ReturnsAsync(false);

        Favorite? captured = null;
        _favRepoMock.Setup(r => r.AddToFavoritesAsync(It.IsAny<Favorite>()))
                    .Callback<Favorite>(f => captured = f)
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.AddToFavorites(request);

        // Assert
        captured.Should().NotBeNull();
        captured!.CreatedAt.Should().BeOnOrAfter(before);
    }

    // ── GetUserFavorites ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserFavorites_WithBooks_ReturnsMappedBookDtos()
    {
        // Arrange
        var book = new Book
        {
            BookId = 1, Title = "Test", Author = "Auth", Genre = "Fiction",
            Ratings = [new Rating { Score = 9 }]
        };
        var favorites = new List<Favorite>
        {
            new() { UserId = 5, BookId = 1, Book = book }
        };
        _favRepoMock.Setup(r => r.GetUserFavoritesAsync(5)).ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetUserFavorites(5);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].RatingAvg.Should().Be(9.0);
    }

    [Fact]
    public async Task GetUserFavorites_FavoritesWithNullBook_FiltersOutNullBooks()
    {
        // Arrange
        var favorites = new List<Favorite>
        {
            new() { UserId = 6, BookId = 1, Book = null! },
            new() { UserId = 6, BookId = 2, Book = new Book { BookId = 2, Title = "Real Book", Author = "A", Genre = "G" } }
        };
        _favRepoMock.Setup(r => r.GetUserFavoritesAsync(6)).ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetUserFavorites(6);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].Title.Should().Be("Real Book");
    }

    [Fact]
    public async Task GetUserFavorites_EmptyList_ReturnsOkWithEmptyResult()
    {
        // Arrange
        _favRepoMock.Setup(r => r.GetUserFavoritesAsync(99)).ReturnsAsync([]);

        // Act
        var result = await _sut.GetUserFavorites(99);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        dtos.Should().BeEmpty();
    }

    // ── RemoveFromFavorites ───────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFromFavorites_ValidRequest_Returns200Ok()
    {
        // Arrange
        _favRepoMock.Setup(r => r.RemoveFromFavoritesAsync(1, 10)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RemoveFromFavorites(1, 10);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        _favRepoMock.Verify(r => r.RemoveFromFavoritesAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task RemoveFromFavorites_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        _favRepoMock.Setup(r => r.RemoveFromFavoritesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.RemoveFromFavorites(userId: 42, bookId: 77);

        // Assert
        _favRepoMock.Verify(r => r.RemoveFromFavoritesAsync(42, 77), Times.Once);
    }
}
