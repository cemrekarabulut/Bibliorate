using BiblioRate.Application.Interfaces;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Repositories;
using BiblioRate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using BiblioRate.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı ve Repository Ayarları
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repository Kayıtları (Dependency Injection)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IBookViewRepository, BookViewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISearchLogRepository, SearchLogRepository>();

// 2. Servis Kayıtları ve HTTP İstemcileri
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();

builder.Services.AddHttpClient("FlaskApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// 3. CORS Ayarları (Berra'nın Flask uygulaması veya frontend için kapıyı açıyoruz)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. API ve Swagger Ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- ÖNEMLİ: GLOBAL HATA YÖNETİMİ ---
// Hataları JSON formatında yakalamak için en başa ekliyoruz
app.UseMiddleware<ExceptionMiddleware>();

// 5. Geliştirme ortamında Swagger'ı aç
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS Middleware'ini yönlendirmelerden önce aktif ediyoruz
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();