using BiblioRate.API.Controllers;
using BiblioRate.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace BiblioRate.Tests.Controllers;

public class RecommendationControllerTests
{
    private readonly Mock<ILogger<RecommendationController>> _loggerMock;

    public RecommendationControllerTests()
    {
        _loggerMock = new Mock<ILogger<RecommendationController>>();
    }

    private RecommendationController CreateControllerWithResponse(HttpResponseMessage httpResponse)
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

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("FlaskApi")).Returns(httpClient);

        return new RecommendationController(factoryMock.Object, _loggerMock.Object);
    }

    // ── GetSmartRecommendations ───────────────────────────────────────────────

    [Fact]
    public async Task GetSmartRecommendations_FlaskReturnsData_ReturnsOk()
    {
        // Arrange
        var data = new List<RecommendationDto>
        {
            new() { Title = "Clean Code", Rating = 9.0, Votes = 120 }
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetSmartRecommendations(userId: 1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSmartRecommendations_FlaskReturnsNull_Returns404NotFound()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<List<RecommendationDto>?>(null)
        };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetSmartRecommendations(userId: 2);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSmartRecommendations_FlaskUnavailable_Returns503()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Flask çevrimdışı."));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://flask-api/") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("FlaskApi")).Returns(httpClient);

        var sut = new RecommendationController(factoryMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GetSmartRecommendations(userId: 3);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }

    // ── GetRecommendations ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecommendations_FlaskReturnsData_ReturnsOk()
    {
        // Arrange
        var data = new List<RecommendationDto>
        {
            new() { Title = "The Pragmatic Programmer", Rating = 8.5, Votes = 200 }
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetRecommendations(userId: 5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecommendations_FlaskReturnsEmptyList_Returns404()
    {
        // Arrange
        var emptyData = new List<RecommendationDto>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(emptyData) };
        var sut = CreateControllerWithResponse(httpResponse);

        // Act
        var result = await sut.GetRecommendations(userId: 6);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRecommendations_FlaskUnavailable_Returns503()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://flask-api/") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("FlaskApi")).Returns(httpClient);
        var sut = new RecommendationController(factoryMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GetRecommendations(userId: 7);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
    }
}
