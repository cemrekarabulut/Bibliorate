using System.Net;
using System.Text.Json;

namespace BiblioRate.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate             _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment            _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message    = "Sunucu tarafında bir hata oluştu.",
            // Detayı sadece geliştirme ortamında göster — production'da sızıntı olmasın
            Detail     = _env.IsDevelopment() ? exception.Message : null
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
