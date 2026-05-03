using BiblioRate.API.Controllers;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BiblioRate.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<ILogger<AnalyticsController>> _loggerMock;

    public AnalyticsControllerTests()
    {
        _loggerMock = new Mock<ILogger<AnalyticsController>>();
    }

    private AnalyticsController CreateControllerWithResponse(HttpResponseMessage httpResponse)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://flask-api/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient("FlaskApi")).Returns(httpClient);

        return new AnalyticsController(httpClientFactoryMock.Object, _loggerMock.Object);
    }

    // ── GetMostViewed ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMostViewed_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<BookAnalyticsDto> { new() { Title = "Book1" } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(data)
        };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetMostViewed();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMostViewed_FlaskUnavailable_Returns503()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Flask bağlantısı kesildi."));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://flask-api/") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("FlaskApi")).Returns(httpClient);

        var sut = new AnalyticsController(factoryMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GetMostViewed();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    // ── GetTopRated ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTopRated_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<BookAnalyticsDto> { new() { Title = "TopBook", Rating = 9.5 } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetTopRated();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetGenrePopularity ────────────────────────────────────────────────────

    [Fact]
    public async Task GetGenrePopularity_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<GenrePopularityDto> { new() { Genre = "Fiction", Count = 100 } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetGenrePopularity();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetViewsOverTime ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetViewsOverTime_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<ViewsOverTimeDto> { new() { Date = "2024-01-01", Views = 250 } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetViewsOverTime();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetSearchTrend ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSearchTrend_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<SearchTrendDto> { new() { Date = "2024-01-01", Searches = 50 } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetSearchTrend();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetMostActiveUsers ────────────────────────────────────────────────────

    [Fact]
    public async Task GetMostActiveUsers_FlaskSuccess_ReturnsOk()
    {
        // Arrange
        var data = new List<ActiveUserDto> { new() { Username = "UserA", Views = 300 } };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetMostActiveUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
