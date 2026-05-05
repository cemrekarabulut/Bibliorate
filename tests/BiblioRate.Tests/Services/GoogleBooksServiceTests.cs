using System.Net;
using System.Text.Json;
using BiblioRate.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using FluentAssertions;

namespace BiblioRate.Tests.Services;

public class GoogleBooksServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly GoogleBooksService _service;

    public GoogleBooksServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["GoogleBooksApiKey"]).Returns("test-key");

        _service = new GoogleBooksService(_httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task SearchBooksAsync_ShouldReturnBooks_WhenApiRespondsSuccessfully()
    {
        // Arrange
        var apiResponse = new
        {
            items = new[]
            {
                new
                {
                    id = "1",
                    volumeInfo = new
                    {
                        title = "Test Book",
                        language = "en",
                        authors = new[] { "Test Author" },
                        description = "This is a long enough description to pass the 50 character limit check in the service.",
                        publishedDate = "2023-01-01"
                    }
                }
            }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SearchBooksAsync("test query", "Test Author");

        // Assert
        result.Should().NotBeEmpty();
        result.First().Title.Should().Be("Test Book");
    }

    [Theory]
    [InlineData("en", "This is a very long description that meets the length requirement of fifty characters.", true)]
    [InlineData("tr", "Türkçe kitap açıklaması test ediliyor.", false)] // Language branch
    [InlineData("en", "Short", false)] // Description length branch
    public async Task SearchBooksAsync_ShouldHandleVariousScenarios(string lang, string desc, bool expectedSuccess)
    {
        // Arrange
        var apiResponse = new
        {
            items = new[]
            {
                new
                {
                    id = "2",
                    volumeInfo = new
                    {
                        title = "Scenario Book",
                        language = lang,
                        authors = new[] { "Author" },
                        description = desc
                    }
                }
            }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SearchBooksAsync("query", "Author");

        // Assert
        if (expectedSuccess) result.Should().NotBeEmpty();
        else result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("http://books.google.com/thumbnail", "https://books.google.com/thumbnail")]
    [InlineData("https://covers.openlibrary.org/b/isbn/123-L.jpg", "https://covers.openlibrary.org/b/isbn/123-L.jpg")]
    [InlineData("https://books.google.com/thumbnail?zoom=1&edge=curl", "https://books.google.com/thumbnail?zoom=2")]
    [InlineData("", "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover")]
    public void SanitizeThumbnail_ShouldWorkCorrectly(string input, string expected)
    {
        // Act
        var result = GoogleBooksService.SanitizeThumbnail(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SearchBooksAsync_ShouldHandleRateLimit_AndThrowException()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.TooManyRequests };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _service.SearchBooksAsync("query");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SearchBooksAsync_ShouldSkipBlacklistedCategories()
    {
        // Arrange
        var apiResponse = new
        {
            items = new[]
            {
                new
                {
                    id = "3",
                    volumeInfo = new
                    {
                        title = "Biography of Someone",
                        language = "en",
                        authors = new[] { "Author" },
                        categories = new[] { "Biography" }, // Blacklisted
                        description = "This is a long enough description to pass the character limit check."
                    }
                }
            }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SearchBooksAsync("query", "Author");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooksAsync_ShouldFallbackToIsbn10_WhenIsbn13IsMissing()
    {
        // Arrange
        var apiResponse = new
        {
            items = new[]
            {
                new
                {
                    id = "isbn10",
                    volumeInfo = new
                    {
                        title = "ISBN 10 Book",
                        language = "en",
                        authors = new[] { "Author" },
                        description = "This is a very long description that is definitely more than fifty characters long to pass the validation check.",
                        industryIdentifiers = new[] { new { type = "ISBN_10", identifier = "1234567890" } }
                    }
                }
            }
        };

        var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(apiResponse)) };
        _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

        // Act
        var result = await _service.SearchBooksAsync("query", "Author");

        // Assert
        result.Should().NotBeEmpty();
        result.First().Isbn.Should().Be("1234567890");
    }

    [Theory]
    [InlineData("https://google.com/no_cover.jpg", "1234567890", "https://covers.openlibrary.org/b/isbn/1234567890-L.jpg")] // Bad signal (no_cover)
    [InlineData(null, "ISBN_MISSING_1", "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover")] // Last fallback branch
    public void ResolveThumbnail_ShouldHandleAllBranches(string? url, string isbn, string expected)
    {
        // Act
        var result = GoogleBooksService.ResolveThumbnail(url, isbn);

        // Assert
        result.Should().Be(expected);
    }
}
