using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioRate.Domain.Models;
using System.Net.Http.Json;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RecommendationController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("smart/{userId}")]
    public async Task<IActionResult> GetSmartRecommendations(int userId)
    {
        var client = _httpClientFactory.CreateClient("FlaskApi");

        try
        {
            // Berra'nın Python'daki smart-recommend endpoint'ini çağırıyoruz
            var recommendations = await client.GetFromJsonAsync<List<RecommendationDto>>($"api/recommend-smart/{userId}");
            
            if (recommendations == null || !recommendations.Any())
                return NotFound("Şu an için size uygun bir öneri bulamadık.");

            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Öneri motoruna bağlanırken hata oluştu: {ex.Message}");
        }
    }
}