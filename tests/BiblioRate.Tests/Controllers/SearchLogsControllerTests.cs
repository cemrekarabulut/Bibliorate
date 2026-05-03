using BiblioRate.API.Controllers;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BiblioRate.Tests.Controllers;

public class SearchLogsControllerTests
{
    private readonly Mock<ISearchLogRepository>            _searchLogRepoMock;
    private readonly Mock<ILogger<SearchLogsController>> _loggerMock;
    private readonly SearchLogsController          _sut;

    public SearchLogsControllerTests()
    {
        _searchLogRepoMock = new Mock<ISearchLogRepository>();
        _loggerMock        = new Mock<ILogger<SearchLogsController>>();
        _sut = new SearchLogsController(_searchLogRepoMock.Object, _loggerMock.Object);
    }

    // ── GetRecentLogs ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecentLogs_CountLessThan1_Returns400BadRequest()
    {
        // Act
        var result = await _sut.GetRecentLogs(0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetRecentLogs_CountGreaterThan100_Returns400BadRequest()
    {
        // Act
        var result = await _sut.GetRecentLogs(101);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetRecentLogs_CountExactly1_ReturnsOk()
    {
        // Arrange
        _searchLogRepoMock.Setup(r => r.GetLastLogsAsync(1))
                          .ReturnsAsync([new SearchLog { SearchId = 1, Query = "a", UserId = null }]);

        // Act
        var result = await _sut.GetRecentLogs(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentLogs_CountExactly100_ReturnsOk()
    {
        // Arrange
        var logs = Enumerable.Range(1, 100)
            .Select(i => new SearchLog { SearchId = i, Query = $"q{i}", UserId = null })
            .ToList();
        _searchLogRepoMock.Setup(r => r.GetLastLogsAsync(100)).ReturnsAsync(logs);

        // Act
        var result = await _sut.GetRecentLogs(100);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentLogs_DefaultCount_PassesTenToRepository()
    {
        // Arrange
        _searchLogRepoMock.Setup(r => r.GetLastLogsAsync(10)).ReturnsAsync([]);

        // Act
        await _sut.GetRecentLogs();

        // Assert
        _searchLogRepoMock.Verify(r => r.GetLastLogsAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetRecentLogs_WithLogs_ReturnsMappedResult()
    {
        // Arrange
        var logs = new List<SearchLog>
        {
            new() { SearchId = 1, Query = "Harry Potter", SearchedAt = DateTime.UtcNow, UserId = 5 },
            new() { SearchId = 2, Query = "Clean Code",   SearchedAt = DateTime.UtcNow, UserId = null }
        };
        _searchLogRepoMock.Setup(r => r.GetLastLogsAsync(10)).ReturnsAsync(logs);

        // Act
        var result = await _sut.GetRecentLogs(10);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("Harry Potter");
        json.Should().Contain("Clean Code");
    }
}
