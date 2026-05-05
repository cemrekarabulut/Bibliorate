using BiblioRate.Application.DTOs;
using BiblioRate.Domain.Models;
using FluentAssertions;

namespace BiblioRate.Tests.DTOs;

public class ComprehensiveDtoTests
{
    [Fact]
    public void Application_DTOs_ShouldWork()
    {
        var login = new UserLoginDto { Username = "u", Password = "p" };
        login.Username.Should().Be("u");
        login.Password.Should().Be("p");

        var register = new UserRegisterDto { Username = "u", Email = "e", Password = "p" };
        register.Username.Should().Be("u");
        register.Email.Should().Be("e");
        register.Password.Should().Be("p");
    }

    [Fact]
    public void Domain_Models_ShouldWork()
    {
        var activeUser = new ActiveUserDto { Username = "u", Views = 10 };
        activeUser.Username.Should().Be("u");
        activeUser.Views.Should().Be(10);

        var authRes = new AuthResponseDto { Token = "t", UserId = "1", Username = "u", Email = "e" };
        authRes.Token.Should().Be("t");
        authRes.UserId.Should().Be("1");

        var bookAnalytic = new BookAnalyticsDto
        {
            Views = 10,
            Rating = 4.5,
            Votes = 100
        };
        bookAnalytic.Views.Should().Be(10);
        bookAnalytic.Rating.Should().Be(4.5);
        bookAnalytic.Votes.Should().Be(100);

        var bookDto = new BookDto { BookId = 1, Title = "T" };
        bookDto.BookId.Should().Be(1);

        var stats = new BooksStatsResponseDto { TotalBooks = 1, TotalRatings = 1, OverallAverageScore = 5.0 };
        stats.TotalBooks.Should().Be(1);

        var createBook = new CreateBookRequest { Title = "T" };
        createBook.Title.Should().Be("T");

        var createFav = new CreateFavoriteRequest { UserId = 1, BookId = 1 };
        createFav.UserId.Should().Be(1);

        var createRating = new CreateRatingRequest { UserId = 1, BookId = 1, Score = 5 };
        createRating.Score.Should().Be(5);

        var genrePop = new GenrePopularityDto { Genre = "G", Count = 1 };
        genrePop.Genre.Should().Be("G");

        var loginReq = new LoginRequestDto { Username = "u", Password = "p" };
        loginReq.Username.Should().Be("u");

        var recommend = new RecommendationDto { Title = "T", Rating = 5.0, Votes = 1 };
        recommend.Title.Should().Be("T");

        var registerReq = new RegisterRequest { Username = "u", Email = "e", Password = "p" };
        registerReq.Username.Should().Be("u");

        var reviewDto = new ReviewDto { ReviewId = 1, Comment = "C" };
        reviewDto.ReviewId.Should().Be(1);

        var searchTrend = new SearchTrendDto { Date = "2024", Searches = 1 };
        searchTrend.Date.Should().Be("2024");

        var updateProfile = new UpdateProfileRequest { Username = "u" };
        updateProfile.Username.Should().Be("u");

        var viewsOverTime = new ViewsOverTimeDto { Date = "2024", Views = 1 };
        viewsOverTime.Date.Should().Be("2024");
    }
}
