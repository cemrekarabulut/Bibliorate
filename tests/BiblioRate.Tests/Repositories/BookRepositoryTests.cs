using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace BiblioRate.Tests.Repositories;

public class BookRepositoryTests
{
    private readonly BookRepository _repository;
    private readonly ApplicationDbContext _context;

    public BookRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _repository = new BookRepository(_context);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks()
    {
        // Arrange
        _context.Books.Add(new Book { Title = "Book 1", Author = "Author 1" });
        _context.Books.Add(new Book { Title = "Book 2", Author = "Author 2" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllBooksAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectBook()
    {
        // Arrange
        var book = new Book { Title = "Target Book" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(book.BookId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Target Book");
    }

    [Fact]
    public async Task AddBookAsync_ShouldAddBookToDatabase()
    {
        // Arrange
        var book = new Book { Title = "New Book" };

        // Act
        await _repository.AddBookAsync(book);

        // Assert
        _context.Books.Should().Contain(book);
    }
}
