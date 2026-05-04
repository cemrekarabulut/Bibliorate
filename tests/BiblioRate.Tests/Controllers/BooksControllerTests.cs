using BiblioRate.API.Controllers;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookRepository>     _bookRepoMock;
    private readonly Mock<IGoogleBooksService> _googleMock;
    private readonly Mock<IBookViewRepository> _viewRepoMock;
    private readonly Mock<IRatingRepository>   _ratingRepoMock;
    private readonly BooksController   _sut;

    public BooksControllerTests()
    {
        _bookRepoMock   = new Mock<IBookRepository>();
        _googleMock     = new Mock<IGoogleBooksService>();
        _viewRepoMock   = new Mock<IBookViewRepository>();
        _ratingRepoMock = new Mock<IRatingRepository>();

        _sut = new BooksController(
            _bookRepoMock.Object,
            _googleMock.Object,
            _viewRepoMock.Object,
            _ratingRepoMock.Object);
    }

    // ── GetBooksStats ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBooksStats_ReturnsOkWithStatsDto()
    {
        // Arrange
        var stats = new BooksStatsResponseDto { TotalBooks = 5, TotalRatings = 10, OverallAverageScore = 7.5 };
        _ratingRepoMock.Setup(r => r.GetBooksStatsForAnalyticsAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(stats);

        // Act
        var result = await _sut.GetBooksStats(CancellationToken.None);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(stats);
    }

    // ── GetLocalBooks ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLocalBooks_EmptyDatabase_ReturnsOkWithEmptyList()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetAllBooksAsync())
                     .ReturnsAsync([]);

        // Act
        var result = await _sut.GetLocalBooks();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var books = ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        books.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLocalBooks_WithBooks_ReturnsMappedDtos()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { BookId = 1, Title = "Test Book", Author = "Author1", Genre = "Fiction",
                    Ratings = [new Rating { Score = 8 }] },
            new() { BookId = 2, Title = "Another Book", Author = "Author2", Genre = "Science",
                    Ratings = [] }
        };
        _bookRepoMock.Setup(r => r.GetAllBooksAsync()).ReturnsAsync(books);

        // Act
        var result = await _sut.GetLocalBooks();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].RatingAvg.Should().Be(8.0);
        dtos[1].RatingAvg.Should().Be(0.0);
    }

    // ── GetBookById ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookById_ExistingId_ReturnsOkAndRecordsView()
    {
        // Arrange
        var book = new Book { BookId = 1, Title = "Test", Author = "A", Genre = "G" };
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _viewRepoMock.Setup(r => r.AddViewAsync(It.IsAny<BookView>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetBookById(1, userId: null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _viewRepoMock.Verify(r => r.AddViewAsync(It.Is<BookView>(v => v.BookId == 1)), Times.Once);
    }

    [Fact]
    public async Task GetBookById_NonExistingId_Returns404NotFound()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.GetBookById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBookById_WithUserId_RecordsViewWithUserId()
    {
        // Arrange
        var book = new Book { BookId = 5, Title = "Book", Author = "A", Genre = "G" };
        _bookRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(book);
        _viewRepoMock.Setup(r => r.AddViewAsync(It.IsAny<BookView>())).Returns(Task.CompletedTask);

        // Act
        await _sut.GetBookById(5, userId: 42);

        // Assert
        _viewRepoMock.Verify(r => r.AddViewAsync(It.Is<BookView>(v => v.UserId == 42 && v.BookId == 5)), Times.Once);
    }

    // ── Search ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_EmptyQuery_Returns400BadRequest()
    {
        // Act
        var result = await _sut.Search("  ");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_NullQuery_Returns400BadRequest()
    {
        // Act
        var result = await _sut.Search(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_ValidQuery_ReturnsOkWithLocalAndGlobalResults()
    {
        // Arrange
        var localBooks = new List<Book>
        {
            new() { BookId = 1, Title = "Clean Code", Author = "Robert C. Martin", Genre = "Technology" },
            new() { BookId = 2, Title = "Refactoring",  Author = "Martin Fowler",   Genre = "Technology" }
        };
        var googleBooks = new List<Book>
        {
            new() { BookId = 0, Title = "Clean Architecture", Author = "Robert C. Martin", Genre = "Technology" }
        };

        _bookRepoMock.Setup(r => r.AddSearchLogAsync(It.IsAny<SearchLog>())).Returns(Task.CompletedTask);
        _bookRepoMock.Setup(r => r.GetAllBooksAsync()).ReturnsAsync(localBooks);
        _googleMock.Setup(g => g.SearchBooksAsync("Clean", "", "General")).ReturnsAsync(googleBooks);

        // Act
        var result = await _sut.Search("Clean");

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Search_ValidQuery_LogsSearch()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.AddSearchLogAsync(It.IsAny<SearchLog>())).Returns(Task.CompletedTask);
        _bookRepoMock.Setup(r => r.GetAllBooksAsync()).ReturnsAsync([]);
        _googleMock.Setup(g => g.SearchBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync([]);

        // Act
        await _sut.Search("Code", userId: 7);

        // Assert
        _bookRepoMock.Verify(r => r.AddSearchLogAsync(It.Is<SearchLog>(l =>
            l.Query == "Code" && l.UserId == 7)), Times.Once);
    }

    // ── CreateBook ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBook_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateBookRequest
        {
            Title  = "New Book",
            Author = "Test Author",
            Genre  = "Fiction",
            Year   = 2024,
            Isbn   = "0000000000"
        };
        _bookRepoMock.Setup(r => r.AddBookAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateBook(request);

        // Assert
        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        _bookRepoMock.Verify(r => r.AddBookAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task CreateBook_WithPublishedAt_SetsPublishedAtFromRequest()
    {
        // Arrange
        var publishedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var request = new CreateBookRequest
        {
            Title       = "Old Book",
            Author      = "Author",
            Genre       = "History",
            PublishedAt = publishedDate
        };

        Book? capturedBook = null;
        _bookRepoMock.Setup(r => r.AddBookAsync(It.IsAny<Book>()))
                     .Callback<Book>(b => capturedBook = b)
                     .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateBook(request);

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.PublishedAt.Should().Be(publishedDate);
    }
}
