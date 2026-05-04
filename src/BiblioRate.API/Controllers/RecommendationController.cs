using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Models;
using System.Net.Http.Json;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IHttpClientFactory                _httpClientFactory;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(IHttpClientFactory httpClientFactory, ILogger<RecommendationController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    // GET api/recommendation/smart/{userId}
    // Flask: /api/recommend-smart/{user_id} → [{title, rating, votes}]
    // Views + Favorites'a göre en favori genre'yi bulur, o genre'de yüksek puanlı kitap önerir.
    [HttpGet("smart/{userId:int}")]
    public Task<IActionResult> GetSmartRecommendations(int userId)
        => ForwardFlaskRecommend($"api/recommend-smart/{userId}");

    // GET api/recommendation/{userId}
    // Flask: /api/recommend/{user_id} → [{title, rating, votes}]
    // Sadece Views'a göre en favori genre'yi bulur.
    [HttpGet("{userId:int}")]
    public Task<IActionResult> GetRecommendations(int userId)
        => ForwardFlaskRecommend($"api/recommend/{userId}");

    // --- Private Helpers ---

    private async Task<IActionResult> ForwardFlaskRecommend(string path)
    {
        try
        {
            var client          = _httpClientFactory.CreateClient("FlaskApi");
            var recommendations = await client.GetFromJsonAsync<List<RecommendationDto>>(path);

            if (recommendations is null || recommendations.Count == 0)
                return NotFound("Şu an için size uygun bir öneri bulamadık.");

            return Ok(recommendations);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Öneri motoruna bağlanılamadı. Path: {Path}", path);
            return StatusCode(503, "Öneri motoruna şu an ulaşılamıyor.");
        }
    }
}
