using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace BiblioRate.Tests.Repositories;

public class AllRepositoriesTests
{
    private readonly ApplicationDbContext _context;

    public AllRepositoriesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task FavoriteRepository_ShouldAddAndRemoveFavorite()
    {
        var repo = new FavoriteRepository(_context);
        var favorite = new Favorite { UserId = 1, BookId = 1 };

        await repo.AddToFavoritesAsync(favorite);
        _context.Favorites.Should().Contain(favorite);

        await repo.RemoveFromFavoritesAsync(1, 1);
        _context.Favorites.Should().NotContain(favorite);
    }

    [Fact]
    public async Task RatingRepository_ShouldAddRating()
    {
        var repo = new RatingRepository(_context);
        var rating = new Rating { UserId = 1, BookId = 1, Score = 5 };

        await repo.AddRatingAsync(rating);
        _context.Ratings.Should().Contain(rating);
    }

    [Fact]
    public async Task ReviewRepository_ShouldAddReview()
    {
        var repo = new ReviewRepository(_context);
        var review = new Review { UserId = 1, BookId = 1, Comment = "Great book" };

        await repo.AddReviewAsync(review);
        _context.Reviews.Should().Contain(review);
    }

    [Fact]
    public async Task SearchLogRepository_ShouldAddLog()
    {
        var repo = new SearchLogRepository(_context);
        var log = new SearchLog { Query = "test", UserId = 1 };

        await repo.AddLogAsync(log);
        _context.SearchLogs.Should().Contain(log);
    }

    [Fact]
    public async Task UserRepository_ShouldHandleUserOperations()
    {
        var repo = new UserRepository(_context);
        var user = new User { Username = "test", Email = "test@test.com", PasswordHash = "hash" };

        await repo.AddUserAsync(user);
        var exists = await repo.UserExistsAsync("test", "test@test.com");
        exists.Should().BeTrue();

        var fetched = await repo.GetByUsernameAsync("test");
        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task FavoriteRepository_ShouldGetUserFavorites()
    {
        var repo = new FavoriteRepository(_context);
        var book = new Book { BookId = 10, Title = "Favorite Book" };
        _context.Books.Add(book);
        _context.Favorites.Add(new Favorite { UserId = 2, BookId = 10 });
        await _context.SaveChangesAsync();

        var result = await repo.GetUserFavoritesAsync(2);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UserRepository_ShouldGetByUsername()
    {
        var repo = new UserRepository(_context);
        _context.Users.Add(new User { Username = "unique", Email = "u@u.com", PasswordHash = "h" });
        await _context.SaveChangesAsync();

        var result = await repo.GetByUsernameAsync("unique");
        result.Should().NotBeNull();
        result!.Username.Should().Be("unique");
    }

    [Fact]
    public async Task BookViewRepository_ShouldAddView()
    {
        var repo = new BookViewRepository(_context);
        await repo.AddViewAsync(new BookView { BookId = 1 });
        _context.BookViews.Should().Contain(v => v.BookId == 1);
    }

    [Fact]
    public async Task RatingRepository_ShouldCalculateAverage()
    {
        var repo = new RatingRepository(_context);
        _context.Ratings.Add(new Rating { BookId = 5, UserId = 1, Score = 4 });
        _context.Ratings.Add(new Rating { BookId = 5, UserId = 2, Score = 2 });
        await _context.SaveChangesAsync();

        var average = await repo.GetAverageScoreAsync(5);
        average.Should().Be(3);
    }

    [Fact]
    public async Task ReviewRepository_ShouldGetByBookId()
    {
        var repo = new ReviewRepository(_context);
        _context.Users.Add(new User { Id = 5, Username = "Reviewer", Email = "r@r.com", PasswordHash = "h" });
        _context.Reviews.Add(new Review { BookId = 5, UserId = 5, Comment = "Review 1" });
        await _context.SaveChangesAsync();

        var reviews = await repo.GetReviewsByBookIdAsync(5);
        reviews.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RatingRepository_ShouldUpdateRating()
    {
        var repo = new RatingRepository(_context);
        _context.Users.Add(new User { Id = 10, Username = "Rater", Email = "r@r.com", PasswordHash = "h" });
        var rating = new Rating { BookId = 6, UserId = 10, Score = 3 };
        await repo.AddRatingAsync(rating);
        
        rating.Score = 5;
        await repo.UpdateRatingAsync(rating);
        
        var fetched = await repo.GetRatingsByBookIdAsync(6);
        fetched.Should().NotBeEmpty();
        fetched.First().Score.Should().Be(5);
    }

    [Fact]
    public async Task ReviewRepository_ShouldDeleteReview()
    {
        var repo = new ReviewRepository(_context);
        var review = new Review { ReviewId = 100, BookId = 1, UserId = 1, Comment = "To delete" };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await repo.DeleteReviewAsync(100);
        _context.Reviews.Should().NotContain(review);
    }
}
