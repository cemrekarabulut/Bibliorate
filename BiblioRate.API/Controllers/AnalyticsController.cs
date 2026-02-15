using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AnalyticsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // 1. En Çok Görüntülenen Kitaplar
    [HttpGet("most-viewed")]
    public async Task<IActionResult> GetMostViewed()
    {
        var client = _httpClientFactory.CreateClient("FlaskApi");
        try
        {
            var result = await client.GetFromJsonAsync<List<object>>("api/analytics/most-viewed");
            return Ok(result);
        }
        catch
        {
            return StatusCode(503, "Analiz servisine şu an ulaşılamıyor. Berra'nın Flask uygulaması açık mı?");
        }
    }

    // 2. En Yüksek Puanlı Kitaplar
    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRated()
    {
        var client = _httpClientFactory.CreateClient("FlaskApi");
        var result = await client.GetFromJsonAsync<List<object>>("api/analytics/top-rated");
        return Ok(result);
    }

    // 3. Popüler Türler
    [HttpGet("genre-popularity")]
    public async Task<IActionResult> GetGenrePopularity()
    {
        var client = _httpClientFactory.CreateClient("FlaskApi");
        var result = await client.GetFromJsonAsync<List<object>>("api/analytics/genre-popularity");
        return Ok(result);
    }
}