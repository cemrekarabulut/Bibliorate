using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Models;
using System.Net.Http.Json;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IHttpClientFactory           _httpClientFactory;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IHttpClientFactory httpClientFactory, ILogger<AnalyticsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    // GET api/analytics/most-viewed
    // Flask: /api/analytics/most-viewed → [{title, views}]
    [HttpGet("most-viewed")]
    public Task<IActionResult> GetMostViewed()
        => ForwardFlaskRequest<List<BookAnalyticsDto>>("api/analytics/most-viewed");

    // GET api/analytics/top-rated
    // Flask: /api/analytics/top-rated → [{title, rating, votes}]
    [HttpGet("top-rated")]
    public Task<IActionResult> GetTopRated()
        => ForwardFlaskRequest<List<BookAnalyticsDto>>("api/analytics/top-rated");

    // GET api/analytics/genre-popularity
    // Flask: /api/analytics/genre-popularity → [{genre, count}]
    [HttpGet("genre-popularity")]
    public Task<IActionResult> GetGenrePopularity()
        => ForwardFlaskRequest<List<GenrePopularityDto>>("api/analytics/genre-popularity");

    // GET api/analytics/views-over-time
    // Flask: /api/analytics/views-over-time → [{date, views}]
    [HttpGet("views-over-time")]
    public Task<IActionResult> GetViewsOverTime()
        => ForwardFlaskRequest<List<ViewsOverTimeDto>>("api/analytics/views-over-time");

    // GET api/analytics/search-trend
    // Flask: /api/analytics/search-trend → [{date, searches}]
    [HttpGet("search-trend")]
    public Task<IActionResult> GetSearchTrend()
        => ForwardFlaskRequest<List<SearchTrendDto>>("api/analytics/search-trend");

    // GET api/analytics/most-active-users
    // Flask: /api/analytics/most-active-users → [{username, views}]
    [HttpGet("most-active-users")]
    public Task<IActionResult> GetMostActiveUsers()
        => ForwardFlaskRequest<List<ActiveUserDto>>("api/analytics/most-active-users");

    // --- Private Helpers ---

    private async Task<IActionResult> ForwardFlaskRequest<T>(string path)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FlaskApi");
            var result = await client.GetFromJsonAsync<T>(path);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Flask analiz servisine ulaşılamadı: {Path}", path);
            return StatusCode(503, "Analiz servisine şu an ulaşılamıyor.");
        }
    }
}
