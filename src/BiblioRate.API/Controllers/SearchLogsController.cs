using Microsoft.AspNetCore.Mvc;
using BiblioRate.Application.Interfaces;

namespace BiblioRate.API.Controllers;

/// <summary>
/// Arama loglarını sorgulama endpoint'leri.
/// Arama logu ekleme BooksController/Search üzerinden otomatik yapılır.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchLogsController : ControllerBase
{
    private readonly ISearchLogRepository _searchLogRepository;
    private readonly ILogger<SearchLogsController> _logger;

    public SearchLogsController(ISearchLogRepository searchLogRepository, ILogger<SearchLogsController> logger)
    {
        _searchLogRepository = searchLogRepository;
        _logger              = logger;
    }

    // GET api/searchlogs/recent?count=10
    // Son yapılan aramaları döndürür — admin paneli veya analytics için
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentLogs([FromQuery] int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest("count 1 ile 100 arasında olmalıdır.");

        var logs = await _searchLogRepository.GetLastLogsAsync(count);

        var result = logs.Select(l => new
        {
            l.SearchId,
            l.Query,
            l.SearchedAt,
            l.UserId
        });

        return Ok(result);
    }
}
