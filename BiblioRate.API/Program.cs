using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String'i appsettings.json dosyasından oku 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. DbContext'i servis konteynırına ekle ve MySQL (Pomelo) kullanacağını belirt 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// OpenAPI/Swagger desteği
builder.Services.AddOpenApi();

// Controller desteğini ekle (React ile konuşmak için gerekli olacak)
builder.Services.AddControllers();

var app = builder.Build();

// HTTP istek hattını (pipeline) yapılandır
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Controller'ları haritala
app.MapControllers();

// Örnek WeatherForecast API'si (İsteğe bağlı, silebilirsin)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}